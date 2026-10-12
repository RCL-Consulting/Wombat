using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Application.Audit;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <remarks>
/// <para>
/// <c>Rationale</c>, <c>Conditions</c> and <c>UndecidedNote</c> are redacted from the audit summary. They are the
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
/// <para>
/// <c>UndecidedNote</c> is the committee's optional sentence about the EPAs it does not decide at the sitting (T361, D53):
/// every line left unstaged is written Not decided when the review is ratified, and the registrar reads the sentence under
/// each. Redacted, as the rationale is: it is the committee's judgement of a named trainee.
/// </para>
/// </remarks>
public sealed record RecordCommitteeDecisionCommand(
    int ReviewId,
    CommitteeDecisionCategory? Category,
    [property: Redact] string Rationale,
    [property: Redact] string? Conditions,
    [property: Redact] string? UndecidedNote,
    IReadOnlyList<string> PresentUserIds,
    ClaimsPrincipal Principal) : IRequest<CommitteeReviewDetailDto>;

public sealed class RecordCommitteeDecisionCommandValidator : AbstractValidator<RecordCommitteeDecisionCommand>
{
    public RecordCommitteeDecisionCommandValidator()
    {
        RuleFor(command => command.ReviewId).GreaterThan(0);
        RuleFor(command => command.Category).IsInEnum();
        RuleFor(command => command.Rationale).NotEmpty().WithMessage(CommitteeDecisionWording.GiveCommitteeRationale).MaximumLength(4000);
        RuleFor(command => command.Conditions).MaximumLength(CommitteeDecision.ConditionsMaxLength);
        RuleFor(command => command.UndecidedNote).MaximumLength(CommitteeDecision.ConditionsMaxLength);
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
            .SingleOrDefaultAsync(entity => entity.Id == request.ReviewId, cancellationToken);

        // Authorise first: an unknown review and one the caller does not chair get the one refusal, before the review's
        // state is said (T194 item 1). The panel's chair, and no one else: T165 removed the Administrator's bypass from the
        // actions that take a decision (D46). And a chair who may sit at the review now (T256), read from the user store
        // once, for the attendance below too.
        (review, var eligible) = await CommitteeDecisionAuthorization.DemandChairedReviewAsync(
            _dbContext, request.Principal, review, _users, cancellationToken);
        await CommitteeTraineeScope.DemandTraineeAtPanelInstitutionAsync(_dbContext, request.Principal, review, cancellationToken);

        // Every check, the domain's own included, runs before RecordDecision changes anything: the audit pipeline saves
        // the request's context from its catch. Who may be counted is PanelSeat's rule, read from the user store.
        var present = PanelSeat.DemandPresent(review, request.PresentUserIds, eligible);

        // T131 slice 4: the decision fixes the agenda with the staged decisions. A closing line nothing is staged on holds
        // nothing back (T361, D53): ratify writes it Not decided. What is staged is read for E4 (an entrustment-only review
        // with nothing staged is refused, in RecordDecision). T235: a line still due whose window another sitting has
        // decided since it was planned is read by the window rule the planner and the decisions-due page share, and
        // recording settles it so. Asked of a review that can take a decision; RecordDecision refuses any other with its
        // own reason. Reads only.
        IReadOnlyCollection<int> stagedEpaIds = [];
        IReadOnlySet<int> decidedElsewhere = new HashSet<int>();
        if (review is { IsFormative: false, State: CommitteeReviewState.InProgress })
        {
            stagedEpaIds = await _dbContext.Set<PendingEntrustmentDecision>()
                .Where(pending => pending.ReviewId == review.Id)
                .Select(pending => pending.EpaId)
                .ToListAsync(cancellationToken);
            decidedElsewhere = (await AgendaPlanner.PlanForReviewAsync(
                    _dbContext, review, ProgrammeCalendar.DateOf(DateTime.UtcNow), cancellationToken))
                .DecidedElsewhere;
        }

        // The first mutation. Every check above, and the domain's own, runs before it changes anything.
        review.RecordDecision(
            request.Category,
            request.Rationale,
            request.Conditions,
            CommitteeDecisionAuthorization.GetRequiredUserId(request.Principal),
            DateTime.UtcNow,
            present,
            stagedEpaIds,
            decidedElsewhere,
            request.UndecidedNote);

        // Refused whole when the review changed after it was read above: its xmin token (CommitteeReviewConfiguration) is
        // checked with the state this save changes. Staging and removing each mark the review modified, so a decision
        // recorded here fixes exactly the staged decisions that were there when it was read (D46), and a second record of
        // the same review is refused too.
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            // Carried as the inner exception, so the audit pipeline still sees a refused save and writes its row alone
            // (T201). Before T213 the chair was shown EF's own message.
            throw new InvalidOperationException(ReviewChanged, exception);
        }

        return review.ToDetailDto();
    }

    /// <summary>The refusal when the review changed between being read and the save. Nothing is recorded. (T213)</summary>
    /// <remarks>
    /// What changed can be this decision itself, recorded from another tab, after which the review is decided and takes
    /// no second one; so the chair is told to record it again only if the review is still in progress (T213 review).
    /// </remarks>
    public const string ReviewChanged =
        "This review changed while the decision was being recorded: " + CommitteeReviewChanged.WhatChanges + ". " +
        "Nothing was recorded. Reload the review and check what is staged; if it is still in progress, " +
        "record the decision again.";
}
