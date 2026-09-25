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
        var lines = review.AgendaLines
            .OrderBy(line => line.EpaCode, StringComparer.Ordinal)
            .ThenBy(line => line.EpaId)
            .Select(line => ToDto(line, sitting, staged.Contains(line.EpaId), evidenceByEpa.GetValueOrDefault(line.EpaId)))
            .ToArray();

        var plan = review.IsFormative
            ? AgendaPlan.Empty
            : await AgendaPlanner.PlanAsync(dbContext, review.TraineeUserId, review.Panel, sitting, today, cancellationToken);

        // What a STAR already decided in its window, so the planner left it off (T215); an EPA this review holds a line
        // for is not named: its line says what this sitting did with it.
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
            DecidedInWindow = plan.DecidedInWindow.Where(epa => !onAgenda.Contains(epa.EpaId)).Select(epa => epa.Code).ToArray()
        };
    }

    /// <summary>A line as the page shows it. <paramref name="evidenceCount" /> is the snapshot's lines about its EPA.</summary>
    public static CommitteeAgendaLineDto ToDto(CommitteeAgendaLine line, AcademicPeriod sitting, bool staged, int evidenceCount)
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
            StatusOf(line, sitting, staged),
            staged && line.State == CommitteeAgendaLineState.Due,
            line.BlocksRatify(staged),
            line.DeferralReason,
            line.EntrustmentDecisionId,
            evidenceCount);
    }

    /// <summary>
    /// How a line reads: its state, and for a line still due, whether it is staged, closing, or optional and why.
    /// </summary>
    public static CommitteeAgendaLineStatus StatusOf(CommitteeAgendaLine line, AcademicPeriod sitting, bool staged)
    {
        ArgumentNullException.ThrowIfNull(line);

        return line.State switch
        {
            CommitteeAgendaLineState.Decided => CommitteeAgendaLineStatus.Decided,
            CommitteeAgendaLineState.Deferred => CommitteeAgendaLineStatus.Deferred,
            CommitteeAgendaLineState.NotDecided => CommitteeAgendaLineStatus.NotDecided,
            _ when staged => CommitteeAgendaLineStatus.Staged,
            _ when line.IsClosing => CommitteeAgendaLineStatus.Due,
            _ when line.IsPartialPeriod => CommitteeAgendaLineStatus.PartialPeriod,
            _ when line.Origin == CommitteeAgendaLineOrigin.Cadence && line.WindowSemesters[^1] != sitting
                => CommitteeAgendaLineStatus.DueByYearEnd,
            _ => CommitteeAgendaLineStatus.AsOpportunityAllows
        };
    }
}
