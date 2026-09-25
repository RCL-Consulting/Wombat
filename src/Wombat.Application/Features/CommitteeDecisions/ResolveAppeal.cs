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
            .SingleOrDefaultAsync(entity => entity.Id == request.ReviewId, cancellationToken)
            ?? throw new InvalidOperationException("The committee review could not be found.");

        // The appeal body answers an appeal against its own ratified review wherever the trainee now trains: resolving
        // it reads no evidence and supersedes no entrustment decision, and a trainee who moved keeps their recourse.
        // The review's trainee was held to the panel's institution when it was ratified. (T182; CommitteeTraineeScope)
        // The appeal body is the panel's chair or an external member, with no Administrator bypass (T165, D46).
        CommitteeDecisionAuthorization.DemandAppealResolverAccess(request.Principal, review.Panel);

        // T165: a remitted appeal replaces the committee's decision, so the replacement is held to what any committee
        // decision is: a quorum of the panel present, each of whom may sit now and none the trainee. Checked here and in
        // the domain before ResolveAppeal changes anything: the audit pipeline saves the request's context from its catch.
        var present = request.Outcome == CommitteeAppealOutcome.Remitted
            ? await PanelSeat.DemandPresentAsync(_users, review, request.PresentUserIds, cancellationToken)
            : null;

        review.ResolveAppeal(
            request.Outcome,
            CommitteeDecisionAuthorization.GetRequiredUserId(request.Principal),
            DateTime.UtcNow,
            request.RemittedCategory,
            request.RemittedRationale,
            request.RemittedConditions,
            present);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return review.ToDetailDto();
    }
}
