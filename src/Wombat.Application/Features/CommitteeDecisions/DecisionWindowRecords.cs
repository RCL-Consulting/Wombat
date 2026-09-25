using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>An agenda line about a trainee, with what its review is and where its panel sits. (T131 slice 6 review)</summary>
/// <param name="Sitting">When its review sat (<see cref="CommitteeSitting" />).</param>
/// <param name="PanelInstitutionId">The institution of the panel its review sat before.</param>
/// <param name="EntrustmentDecisionId">The STAR a Decided line names.</param>
internal sealed record RecordedAgendaLine(
    string TraineeUserId,
    int EpaId,
    int WindowYear,
    int? WindowSemester,
    CommitteeAgendaLineState State,
    CommitteeReviewState ReviewState,
    CommitteeSitting Sitting,
    int PanelInstitutionId,
    int? EntrustmentDecisionId);

/// <summary>
/// What the committee has on record about some trainees' decisions in one academic year: the STARs issued at sittings for
/// the year, and the agenda lines in the year's windows. The one read the agenda planner and the decisions-due page share,
/// so the two say the same thing about where a decision stands (T131 slice 6 review).
/// </summary>
/// <remarks>
/// Every decision window lies inside one academic year, a semester's or the whole year's, so the year bounds both reads.
/// A STAR superseded by one from a sitting for another year was superseded outside every window of this one, which is
/// what its successor's absence here says to <see cref="CommitteeAgendaStatus.StarDecides" />. Two queries, whatever the
/// number of trainees, matched in memory by trainee and EPA.
/// </remarks>
internal sealed class DecisionWindowRecords
{
    private readonly IReadOnlyDictionary<int, DecisionWindowStar> _starsById;
    private readonly ILookup<(string TraineeUserId, int EpaId), DecisionWindowStar> _stars;
    private readonly ILookup<(string TraineeUserId, int EpaId), RecordedAgendaLine> _lines;

    private DecisionWindowRecords(IReadOnlyList<DecisionWindowStar> stars, IReadOnlyList<RecordedAgendaLine> lines)
    {
        _starsById = stars.ToDictionary(star => star.Id);
        _stars = stars.ToLookup(star => (star.TraineeUserId, star.EpaId));
        _lines = lines.ToLookup(line => (line.TraineeUserId, line.EpaId));
    }

    public static async Task<DecisionWindowRecords> LoadAsync(
        IApplicationDbContext dbContext,
        IReadOnlyCollection<string> traineeUserIds,
        int academicYear,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(traineeUserIds);

        var ids = traineeUserIds.ToArray();

        var stars = await dbContext.Set<EntrustmentDecision>()
            .AsNoTracking()
            .Where(star => ids.Contains(star.TraineeUserId) && star.IssuedByCommitteeReview.AcademicYear == academicYear)
            .Select(star => new
            {
                star.Id,
                star.TraineeUserId,
                star.EpaId,
                star.Status,
                star.SupersededByDecisionId,
                star.IssuedByCommitteeReview.AcademicYear,
                star.IssuedByCommitteeReview.Semester,
                star.IssuedByCommitteeReviewId
            })
            .ToListAsync(cancellationToken);

        var lines = await dbContext.Set<CommitteeAgendaLine>()
            .AsNoTracking()
            .Where(line => ids.Contains(line.Review.TraineeUserId) && line.WindowYear == academicYear)
            .Select(line => new
            {
                line.Review.TraineeUserId,
                line.EpaId,
                line.WindowYear,
                line.WindowSemester,
                line.State,
                ReviewState = line.Review.State,
                line.Review.AcademicYear,
                line.Review.Semester,
                line.ReviewId,
                PanelInstitutionId = line.Review.Panel.InstitutionId,
                line.EntrustmentDecisionId
            })
            .ToListAsync(cancellationToken);

        return new DecisionWindowRecords(
            stars.Select(star => new DecisionWindowStar(
                    star.Id,
                    star.TraineeUserId,
                    star.EpaId,
                    star.Status,
                    star.SupersededByDecisionId,
                    new CommitteeSitting(star.AcademicYear, star.Semester, star.IssuedByCommitteeReviewId)))
                .ToArray(),
            lines.Select(line => new RecordedAgendaLine(
                    line.TraineeUserId,
                    line.EpaId,
                    line.WindowYear,
                    line.WindowSemester,
                    line.State,
                    line.ReviewState,
                    new CommitteeSitting(line.AcademicYear, line.Semester, line.ReviewId),
                    line.PanelInstitutionId,
                    line.EntrustmentDecisionId))
                .ToArray());
    }

    /// <summary>The trainee's STARs on the EPA from a sitting for a period the window holds.</summary>
    public IReadOnlyList<DecisionWindowStar> StarsIn(string traineeUserId, int epaId, QuotaWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);
        return _stars[(traineeUserId, epaId)]
            .Where(star => window.Covers(star.Sitting.AcademicYear, star.Sitting.Semester))
            .ToArray();
    }

    /// <summary>The agenda lines about the trainee's EPA in the window, on any review.</summary>
    public IReadOnlyList<RecordedAgendaLine> LinesIn(string traineeUserId, int epaId, QuotaWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);
        var windowSemester = WindowSemesterOf(window);
        return _lines[(traineeUserId, epaId)]
            .Where(line => line.WindowYear == window.AcademicYear && line.WindowSemester == windowSemester)
            .ToArray();
    }

    /// <summary>Whether the STAR decides the window (<see cref="CommitteeAgendaStatus.StarDecides" />).</summary>
    public bool Decides(DecisionWindowStar star, QuotaWindow window)
        => CommitteeAgendaStatus.StarDecides(star, window, _starsById);

    /// <summary>The STAR as where the decision stands reads it.</summary>
    public StarStanding Standing(DecisionWindowStar star, QuotaWindow window)
    {
        ArgumentNullException.ThrowIfNull(star);
        return new StarStanding(Decides(star, window), star.Sitting);
    }

    /// <summary>
    /// The line as where the decision stands reads it: a Decided line decides only while its STAR does. A STAR is issued by
    /// the review that holds the line, a sitting in the window's year, so it is always among the records read.
    /// </summary>
    public AgendaLineStanding Standing(RecordedAgendaLine line, QuotaWindow window)
    {
        ArgumentNullException.ThrowIfNull(line);
        var starDecides = line.State == CommitteeAgendaLineState.Decided &&
                          line.EntrustmentDecisionId is int starId &&
                          _starsById.TryGetValue(starId, out var star) &&
                          Decides(star, window);
        return new AgendaLineStanding(line.State, line.ReviewState, line.Sitting, starDecides);
    }

    /// <summary>The semester of a semester window, or null for a whole year: what an agenda line records.</summary>
    public static int? WindowSemesterOf(QuotaWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);
        return window.Kind == QuotaPeriod.Semester ? window.Semesters[0].Semester : null;
    }
}
