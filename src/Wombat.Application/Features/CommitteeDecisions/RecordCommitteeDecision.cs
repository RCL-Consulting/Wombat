using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Application.Audit;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <remarks>
/// <para>
/// <c>Rationale</c> and <c>Conditions</c> are redacted from the audit summary. They are the
/// committee's written judgement of a named trainee's progression — the most sensitive prose the
/// product holds — and the audit log is read through a different, coarser gate than the review
/// itself. <c>ReviewId</c>, <c>Category</c> and the actor stay in the clear, which is what makes
/// the entry useful: who decided what, and when. (T101)
/// </para>
/// <para>
/// <c>PresentUserIds</c> is who sat when the decision was taken (T165): at least
/// <see cref="CommitteeReview.Quorum" /> members of the review's panel, the recording chair among them, each of whom may
/// sit on it now (<see cref="PanelSeat" />) and none of whom is the trainee under review. It stays in the clear too; who
/// took a decision is part of what the audit entry is for.
/// </para>
/// <para>
/// <c>Category</c> is the progression outcome, and follows the review's type (T131 slice 5): a progression review's
/// decision needs one, and an entrustment-only review's takes none, because its decision is the STARs staged at it.
/// <see cref="CommitteeReview.CategoryRefusal" /> refuses either mismatch before anything changes.
/// </para>
/// </remarks>
public sealed record RecordCommitteeDecisionCommand(
    int ReviewId,
    CommitteeDecisionCategory? Category,
    [property: Redact] string Rationale,
    [property: Redact] string? Conditions,
    IReadOnlyList<string> PresentUserIds,
    ClaimsPrincipal Principal) : IRequest<CommitteeReviewDetailDto>;

public sealed class RecordCommitteeDecisionCommandValidator : AbstractValidator<RecordCommitteeDecisionCommand>
{
    public RecordCommitteeDecisionCommandValidator()
    {
        RuleFor(command => command.ReviewId).GreaterThan(0);
        RuleFor(command => command.Category).IsInEnum();
        RuleFor(command => command.Rationale).NotEmpty().MaximumLength(4000);
        RuleFor(command => command.Conditions).MaximumLength(4000);
        RuleFor(command => command.PresentUserIds)
            .NotNull()
            .Must(present => present is null || present.All(userId => !string.IsNullOrWhiteSpace(userId)))
            .WithMessage("Every member recorded as present must be named.")
            .Must(present => present is null || DecisionPanelComposition.NamesEachMemberOnce(present))
            .WithMessage("A panel member is recorded as present more than once.")
            .Must(present => present is null || DecisionPanelComposition.HoldsAQuorum(present))
            .WithMessage(CommitteeReview.QuorumRule);
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class RecordCommitteeDecisionCommandHandler : IRequestHandler<RecordCommitteeDecisionCommand, CommitteeReviewDetailDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public RecordCommitteeDecisionCommandHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<CommitteeReviewDetailDto> Handle(RecordCommitteeDecisionCommand request, CancellationToken cancellationToken)
    {
        var review = await _dbContext.Set<CommitteeReview>()
            .Include(entity => entity.Panel)
                .ThenInclude(panel => panel.Members)
            .Include(entity => entity.Decisions)
                .ThenInclude(decision => decision.Attendees)
            .Include(entity => entity.Appeals)
            .Include(entity => entity.EvidenceItems)
            .Include(entity => entity.AgendaLines)
            .SingleOrDefaultAsync(entity => entity.Id == request.ReviewId, cancellationToken)
            ?? throw new InvalidOperationException("The committee review could not be found.");

        // The panel's chair, and no one else: T165 removed the Administrator's bypass from the actions that take a
        // decision (D46).
        CommitteeDecisionAuthorization.DemandChairAccess(request.Principal, review.Panel);
        await CommitteeTraineeScope.DemandTraineeAtPanelInstitutionAsync(_dbContext, request.Principal, review, cancellationToken);

        // Every check, the domain's own included, runs before RecordDecision changes anything: the audit pipeline saves
        // the request's context from its catch. Who may be counted is PanelSeat's rule, read from the user store.
        var present = await PanelSeat.DemandPresentAsync(_users, review, request.PresentUserIds, cancellationToken);

        // T131 slice 4: the decision fixes the agenda with the staged decisions (the deferrals are the committee's too), so
        // every closing line must be staged or deferred before it is recorded, by the predicate ratify enforces again.
        // Asked of a review that can take a decision; RecordDecision refuses any other with its own reason.
        if (review is { IsFormative: false, State: CommitteeReviewState.InProgress })
        {
            var stagedEpaIds = await _dbContext.Set<PendingEntrustmentDecision>()
                .Where(pending => pending.ReviewId == review.Id)
                .Select(pending => pending.EpaId)
                .ToListAsync(cancellationToken);
            review.EnsureAgendaSettled(stagedEpaIds);
        }

        review.RecordDecision(
            request.Category,
            request.Rationale,
            request.Conditions,
            CommitteeDecisionAuthorization.GetRequiredUserId(request.Principal),
            DateTime.UtcNow,
            present);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return review.ToDetailDto();
    }
}
