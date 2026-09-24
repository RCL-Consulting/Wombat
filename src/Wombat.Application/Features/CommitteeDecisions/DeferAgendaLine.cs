using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Audit;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// Puts an EPA on a review's agenda off, with the committee's reason: the way a closing line is let through ratify
/// without a decision (Decision 6, O2). (T131 slice 4)
/// </summary>
/// <remarks>
/// <para>
/// <c>Reason</c> is redacted from the audit summary, as a staged decision's rationale is (T101): it is the committee's
/// reasoning about a named trainee. The line stores it; the audit row records who deferred, so the line needs no user id.
/// </para>
/// <para>
/// A deferral is part of the decision the panel records, as a staged decision is (T165, D46), so it is made while the
/// review is in progress and fixed when the decision is recorded; recording demands a settled agenda
/// (<see cref="CommitteeReview.EnsureAgendaSettled" />). The one exception mirrors T165's own for a staged decision: a
/// closing line whose staged decision was removed after the recording, because it no longer fits the trainee's curriculum
/// and so could never be issued, may still be deferred, since otherwise nothing could ratify the review.
/// </para>
/// </remarks>
public sealed record DeferAgendaLineCommand(
    int ReviewId,
    int LineId,
    [property: Redact] string Reason,
    ClaimsPrincipal Principal) : IRequest<Unit>;

public sealed class DeferAgendaLineCommandValidator : AbstractValidator<DeferAgendaLineCommand>
{
    public DeferAgendaLineCommandValidator()
    {
        RuleFor(command => command.ReviewId).GreaterThan(0);
        RuleFor(command => command.LineId).GreaterThan(0);
        RuleFor(command => command.Reason)
            .NotEmpty().WithMessage(CommitteeAgendaLine.DeferralReasonRequired)
            .MaximumLength(CommitteeAgendaLine.DeferralReasonMaxLength);
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class DeferAgendaLineCommandHandler : IRequestHandler<DeferAgendaLineCommand, Unit>
{
    /// <summary>The refusal when the review changed between being read and the save, deferring or reinstating. Nothing is written.</summary>
    public const string ReviewChanged = AgendaLineCommands.ReviewChanged;

    /// <summary>The refusal to defer or reinstate once the committee's decision is recorded (T165).</summary>
    public const string FixedWhenDecided = AgendaLineCommands.FixedWhenDecided;

    private readonly IApplicationDbContext _dbContext;

    public DeferAgendaLineCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(DeferAgendaLineCommand request, CancellationToken cancellationToken)
    {
        var (review, line) = await AgendaLineCommands.DemandOpenLineAsync(
            _dbContext, request.ReviewId, request.LineId, request.Principal, cancellationToken);

        var staged = await _dbContext.Set<PendingEntrustmentDecision>()
            .AnyAsync(pending => pending.ReviewId == review.Id && pending.EpaId == line.EpaId, cancellationToken);

        // T165: fixed once the committee's decision is recorded, except a line that keeps the review from being ratified,
        // which recording's own check leaves only when a staged decision was removed afterwards (see the remarks).
        if (review.State != CommitteeReviewState.InProgress && !line.BlocksRatify(staged))
        {
            throw new InvalidOperationException(FixedWhenDecided);
        }

        if (line.State != CommitteeAgendaLineState.Due)
        {
            throw new InvalidOperationException($"{line.EpaCode} is not due at this review, so it cannot be deferred.");
        }

        // A staged decision and a deferral say opposite things about one EPA at one sitting.
        if (staged)
        {
            throw new InvalidOperationException(
                $"A decision on {line.EpaCode} is staged at this review. Remove it to defer the EPA instead.");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new InvalidOperationException(CommitteeAgendaLine.DeferralReasonRequired);
        }

        // The first mutation.
        line.Defer(request.Reason);
        await AgendaLineCommands.SaveAsync(_dbContext, review, cancellationToken);
        return Unit.Value;
    }
}

/// <summary>Takes a deferral back: the EPA is due again at the review. (T131 slice 4)</summary>
public sealed record ReinstateAgendaLineCommand(int ReviewId, int LineId, ClaimsPrincipal Principal) : IRequest<Unit>;

public sealed class ReinstateAgendaLineCommandValidator : AbstractValidator<ReinstateAgendaLineCommand>
{
    public ReinstateAgendaLineCommandValidator()
    {
        RuleFor(command => command.ReviewId).GreaterThan(0);
        RuleFor(command => command.LineId).GreaterThan(0);
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class ReinstateAgendaLineCommandHandler : IRequestHandler<ReinstateAgendaLineCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public ReinstateAgendaLineCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(ReinstateAgendaLineCommand request, CancellationToken cancellationToken)
    {
        var (review, line) = await AgendaLineCommands.DemandOpenLineAsync(
            _dbContext, request.ReviewId, request.LineId, request.Principal, cancellationToken);

        // T165: a deferral is fixed with the committee's decision once it is recorded, with no exception: taking one back
        // then would reopen an EPA the committee settled, and nothing could be staged on it any more.
        if (review.State != CommitteeReviewState.InProgress)
        {
            throw new InvalidOperationException(DeferAgendaLineCommandHandler.FixedWhenDecided);
        }

        if (line.State != CommitteeAgendaLineState.Deferred)
        {
            throw new InvalidOperationException($"{line.EpaCode} is not deferred at this review, so there is nothing to reinstate.");
        }

        // The first mutation.
        line.Reinstate();
        await AgendaLineCommands.SaveAsync(_dbContext, review, cancellationToken);
        return Unit.Value;
    }
}

/// <summary>The checks and the save deferring and reinstating share.</summary>
internal static class AgendaLineCommands
{
    /// <summary>The refusal when the review changed between being read and the save. Nothing is written.</summary>
    public const string ReviewChanged =
        "This review changed while its agenda was being changed: a decision was staged, removed or ratified, or a line " +
        "was deferred. Nothing was changed. Reload the review and try again.";

    /// <summary>
    /// The refusal to defer or reinstate once the committee's decision is recorded: the deferrals are part of it, as the
    /// staged decisions are (<see cref="Wombat.Application.Features.EntrustmentDecisions.StagedStars.FixedWhenDecided" />).
    /// </summary>
    public const string FixedWhenDecided =
        "Agenda lines are deferred with the committee's decision and fixed when it is recorded: they can be deferred or " +
        "reinstated only while the review is in progress.";

    /// <summary>
    /// The review and its agenda line, once every check has passed: the caller chairs the review's panel (one refusal for
    /// an unknown review and another panel's, T194 item 1), the trainee still trains at the panel's institution (T182),
    /// the review is binding and in progress or decided, and the line is its own. Reads only; tracked, for the caller's one
    /// mutation. Whether a decided review's line may still change is each command's (T165).
    /// </summary>
    public static async Task<(CommitteeReview Review, CommitteeAgendaLine Line)> DemandOpenLineAsync(
        IApplicationDbContext dbContext,
        int reviewId,
        int lineId,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        var review = await dbContext.Set<CommitteeReview>()
            .Include(entity => entity.Panel)
                .ThenInclude(panel => panel.Members)
            .Include(entity => entity.AgendaLines)
            .SingleOrDefaultAsync(entity => entity.Id == reviewId, cancellationToken);

        review = CommitteeDecisionAuthorization.DemandChairedReview(principal, review);
        await CommitteeTraineeScope.DemandTraineeAtPanelInstitutionAsync(dbContext, principal, review, cancellationToken);

        if (review.IsFormative)
        {
            throw new InvalidOperationException(CommitteeReview.FormativeHasNoAgenda);
        }

        if (review.State is not (CommitteeReviewState.InProgress or CommitteeReviewState.Decided))
        {
            throw new InvalidOperationException("An agenda line can be changed only while the review is in progress.");
        }

        var line = review.AgendaLines.SingleOrDefault(entity => entity.Id == lineId)
            ?? throw new InvalidOperationException("The agenda line could not be found at this review.");

        return (review, line);
    }

    /// <summary>
    /// Saves the one change, refused whole when the review changed after it was read: the review is marked modified, so
    /// its xmin token is checked in the same save, as staging does. A ratify that committed in between would otherwise
    /// close the agenda around a line reinstated, or deferred, after the ratify had judged it.
    /// </summary>
    public static async Task SaveAsync(IApplicationDbContext dbContext, CommitteeReview review, CancellationToken cancellationToken)
    {
        dbContext.Set<CommitteeReview>().Entry(review).Property(entity => entity.State).IsModified = true;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            // Carried as the inner exception, so the audit pipeline still sees a refused save and writes its row alone
            // (T201).
            throw new InvalidOperationException(ReviewChanged, exception);
        }
    }
}
