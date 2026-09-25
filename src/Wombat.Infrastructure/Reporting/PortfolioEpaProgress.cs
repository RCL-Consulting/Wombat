using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Identity;

namespace Wombat.Infrastructure.Reporting;

/// <summary>
/// The portfolio's "Progress per EPA" section (T169): each curriculum item's target and count for the periods the
/// export covers, and the rated observations the entrustment trajectory draws for it.
/// </summary>
/// <remarks>
/// <para>
/// Nothing here is computed from scratch. The targets are <see cref="TraineeQuotaProgressReader" />'s, the read model
/// behind the trainee's progress page and dashboard (T130), read for the programme the cover names on
/// <see cref="AsOf" />, with every window back to the export's first day. The ratings are
/// <see cref="GetEpaTrajectoryForTraineeQueryHandler" />'s, the chart on the same page, over the export's dates and the
/// caller's read scope: only a finished row with an assessor's rating (D44).
/// </para>
/// <para>
/// Built so the next two figures slot in without reworking it: the active entrustment decision against the year
/// target ([T166]) and MSF coverage per period ([T168]) are each one more member on
/// <see cref="PortfolioEpaProgressRow" /> and one more line in <c>EpaProgressSectionComponent</c>.
/// </para>
/// </remarks>
/// <param name="AsOf">
/// The day the targets are read for: the export's last day, or today when the export is open-ended or runs past today,
/// and never after the day a completed or deactivated programme ended (<see cref="ReadOn" />). Printed on the section, so
/// the bytes change with the day only where the printed figures do.
/// </param>
/// <param name="Today">
/// Today on the South African calendar. A period that has not ended by today is still running, and is printed as a
/// count so far rather than as met or short: the counts are whole-period counts as they stand now, whatever
/// <see cref="AsOf" /> is.
/// </param>
/// <param name="FromDate">The export's first day, or null for the whole portfolio.</param>
/// <param name="Programme">The programme the cover names, which is the one the targets are read from.</param>
/// <param name="Targets">The progress read, or null when the trainee has no programme and so no targets.</param>
/// <param name="Rows">One per curriculum item, in EPA-code order, then one per other EPA with rated evidence.</param>
internal sealed record PortfolioEpaProgress(
    DateOnly AsOf,
    DateOnly Today,
    DateOnly? FromDate,
    PortfolioProgramme Programme,
    TraineeCurriculumProgressSummaryDto? Targets,
    IReadOnlyList<PortfolioEpaProgressRow> Rows)
{
    /// <summary>
    /// The day the section reads the targets on: today, or the export's last day when that is earlier, or the day the
    /// programme ended (completed or deactivated) when that is earlier still. A graduate's or a withdrawn trainee's export
    /// is read as on their last day, so it never lists a period that began after they left (D49: outside the programme).
    /// </summary>
    public static DateOnly ReadOn(DateOnly today, DateOnly? exportTo, PortfolioProgramme programme)
    {
        var day = exportTo is { } to && to < today ? to : today;
        return programme.EndedOn is { } ended && ended < day ? ended : day;
    }

    /// <param name="deactivatedEpaIds">
    /// EPAs the programme holds a curriculum item for whose EPA is deactivated (T158): no target any more, and a row
    /// with rated evidence for one says so rather than claiming the EPA is not in the curriculum.
    /// </param>
    public static PortfolioEpaProgress Build(
        DateOnly asOf,
        DateOnly today,
        DateOnly? fromDate,
        PortfolioProgramme programme,
        TraineeCurriculumProgressSummaryDto? targets,
        IReadOnlyList<EpaTrajectoryDto> trajectories,
        IReadOnlySet<int> deactivatedEpaIds)
    {
        var trajectoryByEpa = trajectories.ToDictionary(trajectory => trajectory.EpaId);
        var items = targets?.Items ?? [];

        var rows = items
            .Select(item => new PortfolioEpaProgressRow(
                item.EpaId,
                item.EpaCode,
                item.EpaTitle,
                item,
                NoTarget: null,
                PortfolioEpaRatings.From(trajectoryByEpa.GetValueOrDefault(item.EpaId))))
            .ToList();

        // Rated evidence against an EPA with no target (one outside the trainee's curriculum, one whose EPA was
        // deactivated, or any EPA when there is no programme) is still evidence the committee reads: listed after the
        // curriculum, never silently dropped, and each such row says why it has no target.
        var curriculumEpaIds = items.Select(item => item.EpaId).ToHashSet();
        rows.AddRange(trajectories
            .Where(trajectory => !curriculumEpaIds.Contains(trajectory.EpaId))
            .OrderBy(trajectory => trajectory.EpaCode, StringComparer.Ordinal)
            .Select(trajectory => new PortfolioEpaProgressRow(
                trajectory.EpaId,
                trajectory.EpaCode,
                trajectory.EpaTitle,
                Item: null,
                targets is null
                    ? PortfolioNoTargetReason.NoProgramme
                    : deactivatedEpaIds.Contains(trajectory.EpaId)
                        ? PortfolioNoTargetReason.EpaDeactivated
                        : PortfolioNoTargetReason.NotInCurriculum,
                PortfolioEpaRatings.From(trajectory))));

        return new PortfolioEpaProgress(asOf, today, fromDate, programme, targets, rows);
    }
}

/// <summary>Where the programme the section reads stands.</summary>
internal enum PortfolioProgrammeState
{
    /// <summary>The trainee holds no trainee profile, so there is no programme and no target.</summary>
    None,

    /// <summary>In training: the targets are the progress page's.</summary>
    Active,

    /// <summary>Completed at a final review (<c>TraineeProfile.Complete</c>, T080).</summary>
    Completed,

    /// <summary>
    /// Deactivated without being completed (a withdrawal). The day is <c>TraineeProfile.DeactivatedOn</c> (T209), which a
    /// profile deactivated before T209 does not have.
    /// </summary>
    Inactive
}

/// <summary>The programme the cover names, as the section words it.</summary>
/// <param name="EndedOn">
/// The day the programme ended: the completion day, or the day a deactivated one was ended (<c>TraineeProfile.EndedOn</c>).
/// Null while it runs, and for a programme deactivated before Wombat recorded that day.
/// </param>
internal sealed record PortfolioProgramme(PortfolioProgrammeState State, DateOnly? EndedOn)
{
    public static PortfolioProgramme Of(TraineeProfile? profile) => profile switch
    {
        null => new(PortfolioProgrammeState.None, null),
        { IsActive: true } => new(PortfolioProgrammeState.Active, null),
        { CompletedOn: { } completedOn } => new(PortfolioProgrammeState.Completed, completedOn),
        _ => new(PortfolioProgrammeState.Inactive, profile.DeactivatedOn)
    };

    /// <summary>Only a programme still in progress owes anything more in a period that is still running.</summary>
    public bool IsActive => State == PortfolioProgrammeState.Active;
}

/// <summary>Why an EPA with rated evidence has no target in the section.</summary>
internal enum PortfolioNoTargetReason
{
    /// <summary>The trainee has no programme at all.</summary>
    NoProgramme,

    /// <summary>The programme's curriculum holds no item for this EPA that is the trainee's.</summary>
    NotInCurriculum,

    /// <summary>The curriculum holds the trainee's item for it, but the EPA is deactivated, so it is no target (T158).</summary>
    EpaDeactivated
}

/// <summary>One EPA of the section.</summary>
/// <param name="Item">The trainee's curriculum item for this EPA, with its periods, or null when the EPA has no target
/// and the row exists only for its rated evidence.</param>
/// <param name="NoTarget">Why <paramref name="Item" /> is null; null when it is not.</param>
internal sealed record PortfolioEpaProgressRow(
    int EpaId,
    string EpaCode,
    string EpaTitle,
    TraineeCurriculumProgressDto? Item,
    PortfolioNoTargetReason? NoTarget,
    PortfolioEpaRatings Ratings)
{
    /// <summary>The item's periods, newest first; empty when the EPA has no target.</summary>
    public IReadOnlyList<QuotaWindowDto> Periods => Item?.Periods ?? [];
}

/// <summary>
/// An EPA's rated observations within the export, as the trajectory counts them: the progress page's "n observations
/// from m distinct assessors", and the latest point.
/// </summary>
/// <param name="LatestObservedOn">The latest point's encounter date as printed: an undated one says so
/// (<see cref="EncounterDate.Label" />, T161, D28).</param>
internal sealed record PortfolioEpaRatings(
    int Observations,
    int DistinctAssessors,
    string? LatestLabel,
    bool LatestOffLadder,
    string? LatestObservedOn)
{
    public static PortfolioEpaRatings None { get; } = new(0, 0, null, false, null);

    public static PortfolioEpaRatings From(EpaTrajectoryDto? trajectory)
    {
        if (trajectory is null || trajectory.Points.Count == 0)
        {
            return None;
        }

        // The trajectory orders its points by encounter date, then activity id, so the last is the latest.
        var latest = trajectory.Points[^1];

        return new PortfolioEpaRatings(
            trajectory.Points.Count,
            trajectory.Points.Select(point => point.AssessorUserId).Distinct(StringComparer.Ordinal).Count(),
            latest.RatingLabel,
            latest.OffLadder,
            EncounterDate.Label(latest.ObservedOn, latest.ObservedOnDeclared));
    }
}

/// <summary>One activity type's line in the summary: how many, and how many are finished.</summary>
/// <param name="Complete">Activities in a terminal state of their pinned workflow (D44), which is not always the
/// state called <c>completed</c>: a reflective exercise finishes <c>discussed</c>, an MSF row <c>recorded</c>.</param>
internal sealed record PortfolioTypeSummary(string TypeName, int Total, int Complete);
