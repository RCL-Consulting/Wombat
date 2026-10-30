using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.DataRights;

namespace Wombat.Application.Features.DataRights.Commands;

/// <summary>
/// Completes an approved Rectification once at least one correction is recorded on it (T316; T387, R3-C03, R3-C06), and
/// tells the requester it is complete (T387, C06: the boards draw "Nomsa Mahlangu is emailed").
/// </summary>
public sealed record CompleteRectificationRequestCommand(
    Guid RequestId,
    ClaimsPrincipal Principal) : IRequest<DataRightsRequestDto>;

public sealed class CompleteRectificationRequestCommandValidator : AbstractValidator<CompleteRectificationRequestCommand>
{
    public CompleteRectificationRequestCommandValidator()
    {
        RuleFor(command => command.RequestId).NotEmpty();
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class CompleteRectificationRequestCommandHandler : IRequestHandler<CompleteRectificationRequestCommand, DataRightsRequestDto>
{
    /// <summary>The refusal of Complete request with no correction recorded (T316; R3-C03 draws it, word for word).</summary>
    public const string NoCorrectionRecordedMessage = "Record at least one correction before completing the request.";

    /// <summary>Wombat's own message for a request that is not, or is no longer, an approved Rectification; the page words it.</summary>
    public const string NotAnApprovedRectificationMessage = "Only an approved Rectification request can be completed.";

    private readonly IApplicationDbContext _dbContext;
    private readonly AccountNotices _notices;

    public CompleteRectificationRequestCommandHandler(IApplicationDbContext dbContext, AccountNotices notices)
    {
        _dbContext = dbContext;
        _notices = notices;
    }

    public async Task<DataRightsRequestDto> Handle(CompleteRectificationRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Set<DataRightsRequest>()
            .Include(r => r.Rectifications)
            .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken)
            ?? throw new InvalidOperationException("Data rights request not found.");
        // T112: the gate runs AFTER the load, because it is the REQUEST's institution being checked,
        // not merely the caller's role. Refusal and not-found are both thrown before anything is
        // mutated, so a caller out of scope changes nothing.
        DataRightsAuthorization.DemandRectificationAccess(request.Principal, entity);

        // Every refusal typed and asked before anything is staged (T387, T201, T329): the page maps the reason, never the
        // message. Until T387 a submitted Rectification reached the domain's "Only approved requests can be completed."
        if (entity.Type != DataRightsRequestType.Rectification || entity.Status != DataRightsRequestStatus.Approved)
        {
            throw new DataRightsRefusedException(DataRightsRefusal.NotAnApprovedRectification, NotAnApprovedRectificationMessage);
        }

        if (entity.Rectifications.Count == 0)
        {
            throw new DataRightsRefusedException(DataRightsRefusal.NoCorrectionRecorded, NoCorrectionRecordedMessage);
        }

        var utcNow = DateTime.UtcNow;
        entity.Complete(utcNow);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            // The request's xmin token (T387, F8) refused the save: completed in another tab, or withdrawn by the erasure of
            // its requester, between the read and the save. It is no longer an approved Rectification to complete. Carried
            // as the inner exception, so the audit pipeline discards the refused change and writes its row alone (T201).
            throw new DataRightsRefusedException(
                DataRightsRefusal.NotAnApprovedRectification, NotAnApprovedRectificationMessage, exception);
        }

        // After the commit (AccountNotices): a refused completion mails nobody, and a failed send never fails it. The type,
        // the day and a link to My data rights, never who completed it or what the values were (Q5, M3).
        var completedOn = ProgrammeCalendar.DateOf(utcNow);
        await _notices.SendAsync(entity.RequesterUserId, "data-rights-completed", account => Task.FromResult(
            DataRightsDecisionEmail.Build(
                account.Email,
                entity.Type,
                DataRightsNoticeOutcome.Completed,
                completedOn,
                _notices.LinkTo("/account/data-rights"))));

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
