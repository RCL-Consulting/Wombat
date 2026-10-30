using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Audit;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.DataRights;

namespace Wombat.Application.Features.DataRights.Commands;

/// <remarks>
/// <c>DecisionNote</c> is redacted from the audit summary, for the same reason as on
/// ApproveDataRightsRequestCommand: the AuditPipelineBehavior would otherwise write the reviewer's
/// narrative about a named data subject's request into SummaryJson. A rejection note says more than
/// an approval does — it has to argue why the data is being kept. The RequestId stays in the
/// clear. (T101)
/// </remarks>
public sealed record RejectDataRightsRequestCommand(
    Guid RequestId,
    [property: Redact] string DecisionNote,
    ClaimsPrincipal Principal) : IRequest<DataRightsRequestDto>;

public sealed class RejectDataRightsRequestCommandValidator : AbstractValidator<RejectDataRightsRequestCommand>
{
    public RejectDataRightsRequestCommandValidator()
    {
        RuleFor(command => command.RequestId).NotEmpty();
        RuleFor(command => command.DecisionNote).NotEmpty().MaximumLength(4000);
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class RejectDataRightsRequestCommandHandler : IRequestHandler<RejectDataRightsRequestCommand, DataRightsRequestDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly AccountNotices _notices;

    public RejectDataRightsRequestCommandHandler(IApplicationDbContext dbContext, AccountNotices notices)
    {
        _dbContext = dbContext;
        _notices = notices;
    }

    /// <remarks>
    /// As <see cref="ApproveDataRightsRequestCommandHandler" /> (T387, F8, S13): the caller's scope, then Withdrawn, then
    /// decided meanwhile, all before <see cref="DataRightsRequest.Reject" />, the one change; a save that loses a race on
    /// the request's xmin is cleared and refused the same way once the row is read again.
    /// </remarks>
    public async Task<DataRightsRequestDto> Handle(RejectDataRightsRequestCommand request, CancellationToken cancellationToken)
    {
        var actorUserId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User identity is required.");

        var entity = await _dbContext.Set<DataRightsRequest>()
            .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken)
            ?? throw new InvalidOperationException("Data rights request not found.");
        // T112: the gate runs AFTER the load, because it is the REQUEST's institution being checked,
        // not merely the caller's role. Refusal and not-found are both thrown before anything is
        // mutated, so a caller out of scope changes nothing.
        DataRightsAuthorization.DemandReviewAccess(request.Principal, entity);
        DataRightsDecisionRefusals.DemandSubmitted(entity.Status);

        var utcNow = DateTime.UtcNow;
        entity.Reject(actorUserId, request.DecisionNote, utcNow);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            // T387, F8: withdrawn or decided after it was read. The rejection goes, so the audit pipeline commits nothing.
            _dbContext.Set<DataRightsRequest>().Entry(entity).State = EntityState.Detached;
            await DataRightsDecisionRefusals.ThrowForLostRaceAsync(_dbContext, entity.Id, exception, cancellationToken);
            throw;
        }

        // T319, T387 (Q5, M3): the requester is told, after the commit; a failed send is logged and the rejection stands.
        // The notice carries no note and names no decider: the answer is read on My data rights.
        var onDay = ProgrammeCalendar.DateOf(utcNow);
        await _notices.SendAsync(entity.RequesterUserId, "data-rights-rejected", account =>
            Task.FromResult(DataRightsDecisionEmail.Build(
                account.Email,
                entity.Type,
                DataRightsNoticeOutcome.Rejected,
                onDay,
                _notices.LinkTo(DataRightsDecisionRefusals.MyDataRightsPath))));

        return new DataRightsRequestDto(
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
            entity.CompletedOn);
    }

}
