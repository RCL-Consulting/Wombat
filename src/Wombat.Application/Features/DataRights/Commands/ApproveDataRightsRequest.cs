using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wombat.Application.Audit;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Domain.Curricula;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.DataRights.Commands;

/// <remarks>
/// <c>DecisionNote</c> is redacted from the audit summary. The AuditPipelineBehavior audits every
/// request whose type name ends in "Command" and AuditPayloadSerializer writes its properties into
/// SummaryJson; the note discusses a named data subject's request and normally restates what they
/// asked to have removed. The RequestId stays in the clear — it is the join back to the request
/// record, which is where the note is read from under that feature's own access control. (T101)
/// </remarks>
public sealed record ApproveDataRightsRequestCommand(
    Guid RequestId,
    [property: Redact] string DecisionNote,
    ClaimsPrincipal Principal) : IRequest<ApproveDataRightsResult>;

public sealed class ApproveDataRightsRequestCommandValidator : AbstractValidator<ApproveDataRightsRequestCommand>
{
    public ApproveDataRightsRequestCommandValidator()
    {
        RuleFor(command => command.RequestId).NotEmpty();
        RuleFor(command => command.DecisionNote).NotEmpty().MaximumLength(4000);
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class ApproveDataRightsRequestCommandHandler : IRequestHandler<ApproveDataRightsRequestCommand, ApproveDataRightsResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IErasureExecutor _erasureExecutor;
    private readonly IUserAdministrationService _users;
    private readonly AccountNotices _notices;
    private readonly WombatOptions _options;
    private readonly ILogger<ApproveDataRightsRequestCommandHandler> _logger;

    public ApproveDataRightsRequestCommandHandler(
        IApplicationDbContext dbContext,
        IErasureExecutor erasureExecutor,
        IUserAdministrationService users,
        AccountNotices notices,
        IOptions<WombatOptions> options,
        ILogger<ApproveDataRightsRequestCommandHandler> logger)
    {
        _dbContext = dbContext;
        _erasureExecutor = erasureExecutor;
        _users = users;
        _notices = notices;
        _options = options.Value;
        _logger = logger;
    }

    /// <remarks>
    /// Every refusal is found before <see cref="DataRightsRequest.Approve" />, the first change: the audit pipeline saves
    /// the context from its catch (T201), so a refusal after it would commit the approval it refused. The order (T387):
    /// the caller's scope, so a caller out of scope learns nothing of the request's state; Withdrawn before any other state
    /// (F8, S13), in Wombat's words rather than the domain's; decided meanwhile; then, for an Erasure, the salt (N1) and
    /// the last Administrator (F9). A save that loses a race to a withdrawal or another decision (the request's xmin, F8)
    /// is cleared and refused the same way once the row is read again.
    /// </remarks>
    public async Task<ApproveDataRightsResult> Handle(ApproveDataRightsRequestCommand request, CancellationToken cancellationToken)
    {
        var actorUserId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User identity is required.");

        var entity = await _dbContext.Set<DataRightsRequest>()
            .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken)
            ?? throw new InvalidOperationException("Data rights request not found.");

        // T112: the gate runs AFTER the load, because it is the REQUEST's institution being checked, not merely the
        // caller's role; it refuses the requester too, the Administrator included (T387, Q21). First, so a caller out of
        // scope is told nothing about the request, its state included (T387: the scope rule before Withdrawn).
        DataRightsAuthorization.DemandReviewAccess(request.Principal, entity);
        DataRightsDecisionRefusals.DemandSubmitted(entity.Status);

        // Validate type-specific prerequisites BEFORE mutating the request, so a precondition failure leaves it Submitted
        // and actionable rather than stranded in Approved-but-not-executed with no UI recovery. (T084 / F-A1-1)
        string? erasureSalt = null;
        string? erasureAddress = null;
        if (entity.Type == DataRightsRequestType.Erasure)
        {
            erasureSalt = _options.PseudonymSalt;
            if (string.IsNullOrWhiteSpace(erasureSalt))
            {
                // T387, N1: the operator is told what to set, in the log; the page says erasure is not set up.
                _logger.LogError(
                    "Erasure request {RequestId} was not approved: Wombat__PseudonymSalt is not configured on this server.",
                    entity.Id);
                throw new DataRightsRefusedException(
                    DataRightsRefusal.ErasureNotConfigured,
                    "Erasure is not set up on this server. Nothing has been erased, and the request is still Submitted.");
            }

            await DemandNotTheLastAdministratorAsync(entity.RequesterUserId, cancellationToken);

            // T387, F4: the confirmation goes to the address the account holds now. Once erased it holds none.
            erasureAddress = (await _users.GetByIdAsync(entity.RequesterUserId, cancellationToken))?.Email;
        }

        var utcNow = DateTime.UtcNow;
        entity.Approve(actorUserId, request.DecisionNote, utcNow);

        // What an erasure ended, for the result's sentence (T387, S6); null for every other type.
        ErasureOutcome? erasure = null;

        try
        {
            switch (entity.Type)
            {
                case DataRightsRequestType.Erasure:
                    // Completed before the erasure runs, not after, so the executor's saves write the approval and the
                    // completion inside its transaction, with the erasure (T258 review). Completed after, it would be the
                    // save below, outside that transaction: were it refused, the erasure would stand under a request left
                    // Approved, which cannot be approved again. A refused erasure clears what is tracked, the completion
                    // with it.
                    entity.Complete(utcNow);
                    erasure = await _erasureExecutor.ExecuteAsync(entity, erasureSalt!, cancellationToken);
                    break;

                case DataRightsRequestType.Access:
                case DataRightsRequestType.Export:
                    // Access/Export reports are generated on-demand when the user downloads.
                    // Mark completed immediately — the download endpoint serves the report.
                    entity.Complete(utcNow);
                    break;

                case DataRightsRequestType.Objection:
                    // No longer offered (T387, Q8); a stored one is acknowledged and completed, as before.
                    entity.Complete(utcNow);
                    break;

                case DataRightsRequestType.Rectification:
                    // Rectification is applied manually by admin via ApplyRectificationCommand.
                    // Stays in Approved status until rectifications are applied.
                    break;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            // T387, F8: the request changed after it was read. What was staged goes (the request is all an approval of
            // another type stages), so the audit pipeline commits nothing of it.
            _dbContext.Set<DataRightsRequest>().Entry(entity).State = EntityState.Detached;
            await DataRightsDecisionRefusals.ThrowForLostRaceAsync(_dbContext, entity.Id, exception, cancellationToken);
            throw;
        }
        catch (DataRightsRefusedException refusal) when (refusal.Reason == DataRightsRefusal.PersonChanged)
        {
            // The erasure saves several times, and its first save carries the request, so a withdrawal or another decision
            // saved meanwhile arrives as the executor's PersonChanged (it has cleared the tracker). The row says which:
            // the request itself changed, refused in its own words; else something else about the person did.
            await DataRightsDecisionRefusals.ThrowForLostRaceAsync(_dbContext, entity.Id, refusal, cancellationToken);
            throw;
        }

        // T319, T387 (Q5, F4, M3): the requester is told, after the commit, so a refused approval mails nobody; a failed
        // send is logged and the approval stands. The notice names no decider and carries no note.
        var onDay = ProgrammeCalendar.DateOf(utcNow);
        if (entity.Type == DataRightsRequestType.Erasure)
        {
            await _notices.SendToAddressAsync(erasureAddress, entity.RequesterUserId, "data-rights-erased", address =>
                DataRightsDecisionEmail.Build(address, entity.Type, DataRightsNoticeOutcome.Erased, onDay, string.Empty));
        }
        else
        {
            await _notices.SendAsync(entity.RequesterUserId, "data-rights-approved", account =>
                Task.FromResult(DataRightsDecisionEmail.Build(
                    account.Email,
                    entity.Type,
                    DataRightsNoticeOutcome.Approved,
                    onDay,
                    _notices.LinkTo(DataRightsDecisionRefusals.MyDataRightsPath))));
        }

        return new ApproveDataRightsResult(
            new DataRightsRequestDto(
                entity.Id,
                entity.RequesterUserId,
                entity.RequesterDisplayName,
                entity.RequestedOn,
                entity.Type,
                entity.Status,
                entity.Reason,
                entity.DecisionNote,
                entity.DecidedByUserId,
                entity.DecidedOn,
                entity.CompletedOn),
            erasure);
    }

    /// <summary>
    /// Refuses the erasure of Wombat's only active Administrator (T387, F9): nobody would be left to run it, or to add
    /// another. A deactivated Administrator runs nothing, so is not counted.
    /// </summary>
    private async Task DemandNotTheLastAdministratorAsync(string requesterUserId, CancellationToken cancellationToken)
    {
        var activeAdministrators = (await _users.ListUsersInRoleAsync(WombatRoles.Administrator, cancellationToken))
            .Where(user => !user.IsDeactivated)
            .Select(user => user.UserId)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (activeAdministrators.Length == 1 &&
            string.Equals(activeAdministrators[0], requesterUserId, StringComparison.Ordinal))
        {
            throw new DataRightsRefusedException(
                DataRightsRefusal.LastAdministrator,
                "The requester is Wombat's only active Administrator, and erasing the account would leave nobody to run it. " +
                "Nothing has been erased.");
        }
    }
}

/// <summary>
/// The refusals Approve and Reject share (T387, F8, S13): a request no longer Submitted, said in Wombat's words, never the
/// domain's, and the same refusal for a save that lost a race, once the row is read again. Each message names nobody: it
/// goes into the caller's audit row (T201), and the pages map the reason, never the message (T329).
/// </summary>
public static class DataRightsDecisionRefusals
{
    /// <summary>My data rights, which a decision notice links to.</summary>
    public const string MyDataRightsPath = "/account/data-rights";

    public const string WithdrawnMessage =
        "The requester withdrew this request before it was decided. Nothing was decided or erased.";

    public const string DecidedMeanwhileMessage = "This request was decided while this page was open. Nothing more was done.";

    /// <summary>Refuses a request that is not Submitted: Withdrawn first (F8), then any other decided state.</summary>
    public static void DemandSubmitted(DataRightsRequestStatus status)
    {
        if (status == DataRightsRequestStatus.Withdrawn)
        {
            throw new DataRightsRefusedException(DataRightsRefusal.Withdrawn, WithdrawnMessage);
        }

        if (status != DataRightsRequestStatus.Submitted)
        {
            throw new DataRightsRefusedException(DataRightsRefusal.DecidedMeanwhile, DecidedMeanwhileMessage);
        }
    }

    /// <summary>
    /// After a save refused on the request's token, or an erasure refused because something changed: reads the row again
    /// (untracked; the caller has cleared the tracker) and throws Withdrawn or DecidedMeanwhile when the request itself
    /// moved on, with <paramref name="cause" /> inside for the audit pipeline. Returns when it is still Submitted, and the
    /// caller rethrows what it caught.
    /// </summary>
    public static async Task ThrowForLostRaceAsync(
        IApplicationDbContext dbContext,
        Guid requestId,
        Exception cause,
        CancellationToken cancellationToken)
    {
        var status = await dbContext.Set<DataRightsRequest>()
            .AsNoTracking()
            .Where(r => r.Id == requestId)
            .Select(r => (DataRightsRequestStatus?)r.Status)
            .FirstOrDefaultAsync(cancellationToken);

        if (status == DataRightsRequestStatus.Withdrawn)
        {
            throw new DataRightsRefusedException(DataRightsRefusal.Withdrawn, WithdrawnMessage, cause);
        }

        if (status is not null && status != DataRightsRequestStatus.Submitted)
        {
            throw new DataRightsRefusedException(DataRightsRefusal.DecidedMeanwhile, DecidedMeanwhileMessage, cause);
        }
    }
}
