using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Application.Audit;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <remarks>
/// <para>
/// <c>RemittedRationale</c> and <c>RemittedConditions</c> are redacted for the same reason as
/// <see cref="RecordCommitteeDecisionCommand" />'s: they are the committee's judgement of a named
/// trainee. <c>Outcome</c> and <c>RemittedCategory</c> stay in the clear. (T101)
/// </para>
/// <para>
/// <c>PresentUserIds</c> is read only for a remitted appeal: who sat for the replacement decision, held to the quorum
/// the review's own decision was (T165). It stays in the clear, as it does when a decision is recorded.
/// </para>
/// <para>
/// <c>RemittedCategory</c> is read only for a remitted appeal, and follows the review's type as a recorded decision's
/// category does (T131 slice 5): required on a progression review, refused on an entrustment-only one.
/// </para>
/// </remarks>
public sealed record ResolveAppealCommand(
    int ReviewId,
    CommitteeAppealOutcome Outcome,
    CommitteeDecisionCategory? RemittedCategory,
    [property: Redact] string? RemittedRationale,
    [property: Redact] string? RemittedConditions,
    IReadOnlyList<string>? PresentUserIds,
    ClaimsPrincipal Principal) : IRequest<CommitteeReviewDetailDto>;

public sealed class ResolveAppealCommandValidator : AbstractValidator<ResolveAppealCommand>
{
    public ResolveAppealCommandValidator()
    {
        RuleFor(command => command.ReviewId).GreaterThan(0);
        RuleFor(command => command.Principal).NotNull();
        // Whether a remitted replacement takes a category is its review's type (T131 slice 5), which the validator cannot
        // see: CommitteeReview.ResolveAppeal refuses a progression review's replacement without one, and an
        // entrustment-only review's with one, before anything changes.
        RuleFor(command => command.RemittedCategory).IsInEnum();
        When(command => command.Outcome == CommitteeAppealOutcome.Remitted, () =>
        {
            RuleFor(command => command.RemittedRationale).NotEmpty().MaximumLength(4000);
            // T165: the replacement is a committee decision, so it records a quorum present, as recording one does.
            RuleFor(command => command.PresentUserIds)
                .NotNull()
                .WithMessage(CommitteeReview.QuorumRule)
                .Must(present => present is null || present.All(userId => !string.IsNullOrWhiteSpace(userId)))
                .WithMessage("Every member recorded as present must be named.")
                .Must(present => present is null || DecisionPanelComposition.NamesEachMemberOnce(present))
                .WithMessage("A panel member is recorded as present more than once.")
                .Must(present => present is null || DecisionPanelComposition.HoldsAQuorum(present))
                .WithMessage(CommitteeReview.QuorumRule);
        });
        RuleFor(command => command.RemittedConditions).MaximumLength(4000);
    }
}

public sealed class ResolveAppealCommandHandler : IRequestHandler<ResolveAppealCommand, CommitteeReviewDetailDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public ResolveAppealCommandHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<CommitteeReviewDetailDto> Handle(ResolveAppealCommand request, CancellationToken cancellationToken)
    {
        var review = await _dbContext.Set<CommitteeReview>()
            .Include(entity => entity.Panel)
                .ThenInclude(panel => panel.Members)
            .Include(entity => entity.Decisions)
                .ThenInclude(decision => decision.Attendees)
            .Include(entity => entity.Appeals)
            .Include(entity => entity.EvidenceItems)
            .SingleOrDefaultAsync(entity => entity.Id == request.ReviewId, cancellationToken);

        // Authorise first: an unknown review and one whose appeal body the caller does not sit on get the one refusal,
        // before whether it is under appeal is said (T194 item 1). The appeal body is the panel's chair or an external
        // member, with no Administrator bypass (T165, D46). It answers an appeal against its own ratified review wherever
        // the trainee now trains: resolving it reads no evidence and supersedes no entrustment decision, and a trainee who
        // moved keeps their recourse. The review's trainee was held to the panel's institution when it was ratified.
        // (T182; CommitteeTraineeScope)
        review = CommitteeDecisionAuthorization.DemandAppealBodyReview(request.Principal, review);
        var resolverUserId = CommitteeDecisionAuthorization.GetRequiredUserId(request.Principal);

        // T237: and acts from that seat only while they may sit at the review now (PanelSeat), the rule the page's
        // appeal-body note names people by, so the note and this gate agree on who can resolve the appeal. The seat check
        // above reads claims, which cannot say whether the caller is still an active committee member at the panel's
        // institution, nor rule out the trainee under review once they no longer hold Trainee. The seat's own refusal only
        // to one who may still read the review; anyone else is given the one refusal (T279).
        var eligible = await PanelSeat.EligibleAsync(_users, review.Panel.InstitutionId, cancellationToken);
        await CommitteeDecisionAuthorization.DemandResolvesFromSeatAsync(
            _dbContext, request.Principal, review, eligible, cancellationToken);

        // T165: a remitted appeal replaces the committee's decision, so the replacement is held to what any committee
        // decision is: a quorum of the panel present, each of whom may sit now and none the trainee. Checked here and in
        // the domain before ResolveAppeal changes anything: the audit pipeline saves the request's context from its catch.
        var present = request.Outcome == CommitteeAppealOutcome.Remitted
            ? PanelSeat.DemandPresent(review, request.PresentUserIds, eligible)
            : null;

        review.ResolveAppeal(
            request.Outcome,
            resolverUserId,
            DateTime.UtcNow,
            request.RemittedCategory,
            request.RemittedRationale,
            request.RemittedConditions,
            present);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return review.ToDetailDto();
    }
}
