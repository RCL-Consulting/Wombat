using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// A review's agenda: the EPAs it is there to decide and where each stands, what another panel sitting as a College
/// committee still owes in the period, and what a STAR already decided in its window (T215). (T131 slice 4)
/// </summary>
/// <remarks>
/// Read through <see cref="CommitteeDecisionAuthorization.DemandReviewAccess" />, the one read ladder of a review and of
/// everything computed from it, so a trainee sees their review's line states only once it is ratified.
/// <see cref="GetCommitteeReviewByIdQuery" /> answers with the same agenda; the review page reads this one again after
/// each action, because the commands answer with the review as the mapper they share builds it, which reads nothing.
/// </remarks>
/// <param name="Today">The day "missed" is judged on; the programme's today (<see cref="ProgrammeCalendar.DateOf" />) when null.</param>
public sealed record GetCommitteeAgendaQuery(int ReviewId, ClaimsPrincipal Principal, DateOnly? Today = null)
    : IRequest<CommitteeAgendaDto>;

public sealed class GetCommitteeAgendaQueryValidator : AbstractValidator<GetCommitteeAgendaQuery>
{
    public GetCommitteeAgendaQueryValidator()
    {
        RuleFor(query => query.ReviewId).GreaterThan(0);
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class GetCommitteeAgendaQueryHandler : IRequestHandler<GetCommitteeAgendaQuery, CommitteeAgendaDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCommitteeAgendaQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CommitteeAgendaDto> Handle(GetCommitteeAgendaQuery request, CancellationToken cancellationToken)
    {
        var review = await _dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .Include(entity => entity.Panel)
                .ThenInclude(panel => panel.Members)
            .Include(entity => entity.AgendaLines)
            .Include(entity => entity.EvidenceItems)
            .SingleOrDefaultAsync(entity => entity.Id == request.ReviewId, cancellationToken);

        // One refusal for an unknown review and one out of reach, before anything about it is said (T194 item 1).
        review = CommitteeDecisionAuthorization.DemandReviewAccess(request.Principal, review);

        return await CommitteeAgendaReader.ReadAsync(
            _dbContext, review, request.Today ?? ProgrammeCalendar.DateOf(DateTime.UtcNow), cancellationToken);
    }
}

/// <summary>Builds a review's <see cref="CommitteeAgendaDto" />. The caller has passed the review's read ladder.</summary>
internal static class CommitteeAgendaReader
{
    /// <param name="review">
    /// The review, with its <see cref="CommitteeReview.Panel" />, <see cref="CommitteeReview.AgendaLines" /> and
    /// <see cref="CommitteeReview.EvidenceItems" /> loaded.
    /// </param>
    public static async Task<CommitteeAgendaDto> ReadAsync(
        IApplicationDbContext dbContext,
        CommitteeReview review,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(review);

        var staged = (await dbContext.Set<PendingEntrustmentDecision>()
                .AsNoTracking()
                .Where(pending => pending.ReviewId == review.Id)
                .Select(pending => pending.EpaId)
                .ToListAsync(cancellationToken))
            .ToHashSet();

        var evidenceByEpa = review.EvidenceItems
            .Where(item => item.EpaId is not null)
            .GroupBy(item => item.EpaId!.Value)
            .ToDictionary(group => group.Key, group => group.Count());

        var sitting = review.Period;

        // T235: the plan for the review's own sitting reads each line it still holds due against the window rule, as the
        // record and ratify handlers do, so a line another sitting has decided reads as optional here and there.
        var plan = await AgendaPlanner.PlanForReviewAsync(dbContext, review, today, cancellationToken);

        var lines = review.AgendaLines
            .OrderBy(line => line.EpaCode, StringComparer.Ordinal)
            .ThenBy(line => line.EpaId)
            .Select(line => ToDto(
                line,
                sitting,
                staged.Contains(line.EpaId),
                plan.DecidedElsewhere.Contains(line.EpaId),
                evidenceByEpa.GetValueOrDefault(line.EpaId),
                reviewWithdrawn: review.State == CommitteeReviewState.Withdrawn))
            .ToArray();

        // What a STAR already decided in its window, so the planner left it off (T215); an EPA this review holds a line
        // for is not named: its line says what this sitting did with it. The same holds the other way round (T235): an EPA
        // whose decision was revoked is named only while the review sits and holds no line for it, for the chair to stage.
        var onAgenda = review.AgendaLines.Select(line => line.EpaId).ToHashSet();

        return new CommitteeAgendaDto(
            review.Id,
            review.AcademicYear,
            review.Semester,
            sitting.ToString(),
            review.IsFormative,
            lines,
            plan.RoutedElsewhere)
        {
            DecidesProgression = review.DecidesProgression,
            DecidedInWindow = plan.DecidedInWindow.Where(epa => !onAgenda.Contains(epa.EpaId)).Select(epa => epa.Code).ToArray(),
            NoLongerDecided = review.State == CommitteeReviewState.InProgress
                ? plan.NoLongerDecided.Where(epa => !onAgenda.Contains(epa.EpaId)).ToArray()
                : []
        };
    }

    /// <summary>A line as the page shows it. <paramref name="evidenceCount" /> is the snapshot's lines about its EPA.</summary>
    /// <param name="decidedElsewhere">
    /// Whether the line is still due and another sitting has decided its window (<see cref="AgendaPlan.DecidedElsewhere" />,
    /// T235): false for a line the planner has just planned, which it plans only when the window is undecided.
    /// </param>
    /// <param name="reviewWithdrawn">Whether the line's review was withdrawn (T258): see <see cref="StatusOf" />.</param>
    public static CommitteeAgendaLineDto ToDto(
        CommitteeAgendaLine line,
        AcademicPeriod sitting,
        bool staged,
        bool decidedElsewhere,
        int evidenceCount,
        bool reviewWithdrawn = false)
    {
        ArgumentNullException.ThrowIfNull(line);

        return new CommitteeAgendaLineDto(
            line.Id,
            line.EpaId,
            line.EpaCode,
            line.EpaTitle,
            line.Origin,
            line.WindowYear,
            line.WindowSemester,
            line.WindowLabel,
            line.IsClosing,
            line.IsPartialPeriod,
            line.State,
            StatusOf(line, sitting, staged, decidedElsewhere, reviewWithdrawn),
            staged && line.State == CommitteeAgendaLineState.Due,
            // A withdrawn review is never ratified, so nothing on it is outstanding (T258).
            !reviewWithdrawn && line.BlocksRatify(staged, decidedElsewhere),
            line.DeferralReason,
            line.EntrustmentDecisionId,
            evidenceCount);
    }

    /// <summary>
    /// How a line reads: its state, and for a line still due, whether it is staged, decided at another sitting (T235),
    /// closing, or optional and why. A line still due on a withdrawn review was not decided, and nothing more is decided at
    /// it (T258).
    /// </summary>
    public static CommitteeAgendaLineStatus StatusOf(
        CommitteeAgendaLine line,
        AcademicPeriod sitting,
        bool staged,
        bool decidedElsewhere,
        bool reviewWithdrawn = false)
    {
        ArgumentNullException.ThrowIfNull(line);

        return line.State switch
        {
            CommitteeAgendaLineState.Decided => CommitteeAgendaLineStatus.Decided,
            CommitteeAgendaLineState.Deferred => CommitteeAgendaLineStatus.Deferred,
            CommitteeAgendaLineState.NotDecided => CommitteeAgendaLineStatus.NotDecided,
            CommitteeAgendaLineState.DecidedElsewhere => CommitteeAgendaLineStatus.DecidedElsewhere,
            _ when reviewWithdrawn => CommitteeAgendaLineStatus.NotDecided,
            _ when staged => CommitteeAgendaLineStatus.Staged,
            _ when decidedElsewhere => CommitteeAgendaLineStatus.DecidedElsewhere,
            _ when line.IsClosing => CommitteeAgendaLineStatus.Due,
            _ when line.IsPartialPeriod => CommitteeAgendaLineStatus.PartialPeriod,
            _ when line.Origin == CommitteeAgendaLineOrigin.Cadence && line.WindowSemesters[^1] != sitting
                => CommitteeAgendaLineStatus.DueByYearEnd,
            _ => CommitteeAgendaLineStatus.AsOpportunityAllows
        };
    }
}
