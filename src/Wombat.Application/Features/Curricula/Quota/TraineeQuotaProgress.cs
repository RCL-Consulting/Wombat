using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Curricula.Quota;

/// <summary>One window of one item, as a reader sees it.</summary>
/// <param name="Name">"Semester 2, 2026" or "2026 academic year".</param>
/// <param name="Months">"July to November", on the College's calendar.</param>
/// <param name="Count">Credited encounters in the window, shown even when the target is waived.</param>
/// <param name="MinimumLevelReachedCount">
/// Of <see cref="Count" />, how many met the minimum level that applied WHEN each was observed (the stage at the
/// encounter date, T073). It is a share of the encounters, not of the target.
/// </param>
/// <param name="LastObservedOn">The latest encounter credited in this window.</param>
/// <param name="FirstCountedName">When the target is waived, the first window the trainee is held to.</param>
/// <param name="FirstCountedOn">The first day of that window.</param>
public sealed record QuotaWindowDto(
    string Name,
    string Months,
    DateOnly Start,
    DateOnly NominalEnd,
    QuotaWindowStatus Status,
    int Count,
    int Target,
    bool IsMet,
    int Shortfall,
    int PercentOfTarget,
    int MinimumLevelReachedCount,
    DateOnly? LastObservedOn,
    string? FirstCountedName,
    DateOnly? FirstCountedOn)
{
    public bool Applies => Status == QuotaWindowStatus.Counting;

    public bool IsExempt => Status == QuotaWindowStatus.ExemptPartialPeriod;

    public bool NotStarted => Status == QuotaWindowStatus.NotStarted;

    public static QuotaWindowDto From(QuotaWindowTally tally)
        => new(
            QuotaText.WindowName(tally.Window),
            QuotaText.Months(tally.Window),
            tally.Window.Start,
            tally.Window.NominalEnd,
            tally.Window.Status,
            tally.Count,
            tally.Target,
            tally.IsMet,
            tally.Shortfall,
            tally.PercentOfTarget,
            tally.MinimumLevelReachedCount,
            tally.LastObservedOn,
            tally.Applies ? null : QuotaText.FirstCountedName(tally.Window),
            tally.Applies ? null : tally.Window.FirstCountedPeriod?.Start);
}

/// <summary>One curriculum item of a trainee's progress (T130).</summary>
/// <param name="Target">The item's target per window: <see cref="CurriculumItem.RequiredCount" />.</param>
/// <param name="Current">The window containing today.</param>
/// <param name="Previous">The window before it, or null when the trainee had not started then.</param>
/// <param name="EffectiveMinimumLevelLabel">
/// The minimum for the trainee's training year TODAY, as the rung a clinician reads ("3a", T100). The encounters
/// in <see cref="Current" /> were each judged against the minimum of the training year they were observed in,
/// which can differ when the training year changed inside the window. <see cref="TrainingYearChangedOn" /> says
/// when that happened.
/// </param>
public sealed record TraineeCurriculumProgressDto(
    int CurriculumItemId,
    string EpaCode,
    string EpaTitle,
    QuotaPeriod QuotaPeriod,
    int Target,
    QuotaWindowDto Current,
    QuotaWindowDto? Previous,
    int EffectiveMinimumLevelOrder,
    string EffectiveMinimumLevelLabel,
    DateOnly? TrainingYearChangedOn)
{
    public bool IsPerSemester => QuotaPeriod == QuotaPeriod.Semester;
}

/// <summary>
/// What a registrar opens the progress page to learn: what is expected of them in this period, and whether they
/// have done it (T130).
/// </summary>
/// <param name="AsOf">The day the figures are for.</param>
/// <param name="TraineeStage">The training year today: <c>TraineeProfile.GetStage</c> (D17), which is not the academic year.</param>
/// <param name="IsAfterTeachingYear">Today is in December, after the College's January–November year.</param>
/// <param name="HasSemesterItems">The curriculum holds at least one per-semester item.</param>
/// <param name="HasYearItems">The curriculum holds at least one per-academic-year item.</param>
/// <param name="SemesterTargetsStart">
/// When semester targets start applying, if they do not apply today (D14). Null when they apply, and null when
/// the curriculum has no semester items, so no surface can announce a waiver of targets the trainee does not have.
/// </param>
/// <param name="YearTargetsStart">The same for academic-year targets. D42 usually waives only ONE of the two kinds.</param>
public sealed record TraineeCurriculumProgressSummaryDto(
    DateOnly AsOf,
    DateOnly ProgrammeStartDate,
    int? TraineeStage,
    string CurrentSemesterName,
    string CurrentSemesterMonths,
    DateOnly CurrentSemesterNominalEnd,
    bool IsAfterTeachingYear,
    int SemesterTargetsMet,
    int SemesterTargetsApplying,
    int YearTargetsMet,
    int YearTargetsApplying,
    bool HasSemesterItems,
    bool HasYearItems,
    bool ProgrammeNotStarted,
    QuotaStartDto? SemesterTargetsStart,
    QuotaStartDto? YearTargetsStart,
    IReadOnlyList<TraineeCurriculumProgressDto> Items);

/// <summary>The first window of a kind a trainee is held to: "semester 1, 2027", from 1 January 2027.</summary>
public sealed record QuotaStartDto(string Name, DateOnly StartsOn);

/// <summary>
/// Builds one trainee's quota progress. Shared by the progress page and the trainee dashboard, so the two
/// cannot disagree (T130).
/// </summary>
public static class TraineeQuotaProgressReader
{
    /// <summary>
    /// The trainee's progress on <paramref name="asOf" />, or null when they have no active trainee profile.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Items are listed from the curriculum, not from the progress rows: a new period has no rows yet, and it must
    /// read "0 of 3", not vanish. The items are the national core plus the trainee's own institution's local
    /// extras. A curriculum row is shared by every adopting institution, and another institution's local item is
    /// a target this trainee could never meet. An item whose EPA is deactivated is no target either
    /// (<see cref="CurriculumItemsInForce" />, T158): it cannot be filed against or credited, so listing it would
    /// show a shortfall nobody can close.
    /// </para>
    /// <para>
    /// Rows are loaded for every semester and read through <see cref="QuotaProgressCalculator" /> in memory. No
    /// period predicate is pushed into SQL. Nothing here reads <c>Activity</c>, so the activity read boundary does
    /// not apply.
    /// </para>
    /// </remarks>
    public static async Task<TraineeCurriculumProgressSummaryDto?> ReadAsync(
        IApplicationDbContext dbContext,
        string traineeUserId,
        DateOnly asOf,
        CancellationToken cancellationToken)
    {
        // Active profiles only, as before: this is the trainee's own current programme. Among several active
        // profiles, the most recent start, which is the tie-break the credit engine uses.
        var profile = await dbContext.Set<TraineeProfile>()
            .AsNoTracking()
            .Where(p => p.UserId == traineeUserId && p.IsActive)
            .OrderByDescending(p => p.ProgrammeStartDate)
            .ThenByDescending(p => p.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return null;
        }

        var stage = profile.GetStage(asOf);

        var items = await dbContext.Set<CurriculumItem>()
            .AsNoTracking()
            .InForce()
            .Where(item => item.CurriculumId == profile.CurriculumId &&
                           (item.OwningInstitutionId == null || item.OwningInstitutionId == profile.InstitutionId))
            .OrderBy(item => item.Epa.Code)
            .Select(item => new
            {
                item.Id,
                EpaCode = item.Epa.Code,
                EpaTitle = item.Epa.Title,
                item.RequiredCount,
                item.QuotaPeriod,
                item.MinimumLevelOrder,
                item.MinimumLevelByStageJson,
                item.ScaleId
            })
            .ToListAsync(cancellationToken);

        var itemIds = items.Select(item => item.Id).ToList();
        var rows = itemIds.Count == 0
            ? []
            : await dbContext.Set<CurriculumItemProgress>()
                .AsNoTracking()
                .Where(row => row.TraineeUserId == traineeUserId && itemIds.Contains(row.CurriculumItemId))
                .Select(row => new QuotaProgressRow(
                    row.CurriculumItemId,
                    row.AcademicYear,
                    row.Semester,
                    row.CountsSoFar,
                    row.MinimumLevelReachedCount,
                    row.LastObservedOn))
                .ToListAsync(cancellationToken);

        var rungs = await EntrustmentRungLabels.LoadAsync(dbContext, items.Select(item => item.ScaleId), cancellationToken);

        var result = new List<TraineeCurriculumProgressDto>(items.Count);
        foreach (var item in items)
        {
            var progress = QuotaProgressCalculator.For(
                item.Id, item.QuotaPeriod, item.RequiredCount, rows, profile.ProgrammeStartDate, asOf);

            var effectiveMinimum = new CurriculumItem
            {
                MinimumLevelOrder = item.MinimumLevelOrder,
                MinimumLevelByStageJson = item.MinimumLevelByStageJson
            }.GetMinimumLevelForStage(stage);

            result.Add(new TraineeCurriculumProgressDto(
                item.Id,
                item.EpaCode,
                item.EpaTitle,
                item.QuotaPeriod,
                item.RequiredCount,
                QuotaWindowDto.From(progress.Current),
                progress.Previous is { } previous && previous.Window.Status != QuotaWindowStatus.NotStarted
                    ? QuotaWindowDto.From(previous)
                    : null,
                effectiveMinimum,
                rungs.Format(item.ScaleId, effectiveMinimum),
                TrainingYearChangedWithin(profile, progress.Current.Window, asOf)));
        }

        var semester = AcademicPeriod.Containing(asOf);
        var semesterWindow = QuotaWindow.For(QuotaPeriod.Semester, asOf, profile.ProgrammeStartDate);
        var yearWindow = QuotaWindow.For(QuotaPeriod.AcademicYear, asOf, profile.ProgrammeStartDate);
        var hasSemesterItems = result.Any(item => item.IsPerSemester);
        var hasYearItems = result.Any(item => !item.IsPerSemester);

        return new TraineeCurriculumProgressSummaryDto(
            asOf,
            profile.ProgrammeStartDate,
            stage,
            QuotaText.SemesterName(semester),
            QuotaText.Months(semester),
            semester.NominalEnd,
            asOf > semester.NominalEnd,
            result.Count(item => item.QuotaPeriod == QuotaPeriod.Semester && item.Current.IsMet),
            result.Count(item => item.QuotaPeriod == QuotaPeriod.Semester && item.Current.Applies),
            result.Count(item => item.QuotaPeriod != QuotaPeriod.Semester && item.Current.IsMet),
            result.Count(item => item.QuotaPeriod != QuotaPeriod.Semester && item.Current.Applies),
            hasSemesterItems,
            hasYearItems,
            asOf < profile.ProgrammeStartDate,
            hasSemesterItems ? StartOf(semesterWindow, profile.ProgrammeStartDate) : null,
            hasYearItems ? StartOf(yearWindow, profile.ProgrammeStartDate) : null,
            result);
    }

    /// <remarks>
    /// A trainee who has not started yet but will start on time (15 January, say) is held to the window their start
    /// falls in, whose first day is before their start. The date shown is their own start date then, never a day
    /// that is already behind them.
    /// </remarks>
    private static QuotaStartDto? StartOf(QuotaWindow window, DateOnly programmeStart)
        => window.Status == QuotaWindowStatus.Counting || window.FirstCountedPeriod is not { } first
            ? null
            : new QuotaStartDto(
                QuotaText.FirstCountedName(window)!,
                first.Start > programmeStart ? first.Start : programmeStart);

    /// <summary>
    /// The day the trainee's training year changed inside the window, up to today, or null. When it did, the
    /// window's encounters were judged against two different minimum levels.
    /// </summary>
    private static DateOnly? TrainingYearChangedWithin(TraineeProfile profile, QuotaWindow window, DateOnly asOf)
    {
        var from = window.Start > profile.ProgrammeStartDate ? window.Start : profile.ProgrammeStartDate;
        if (from > asOf)
        {
            return null;
        }

        var stageThen = profile.GetStage(from);
        var stageNow = profile.GetStage(asOf);
        if (stageThen is null || stageNow is null || stageThen == stageNow)
        {
            return null;
        }

        // GetStage counts whole 365-day blocks from the programme start.
        return profile.ProgrammeStartDate.AddDays(365 * (stageNow.Value - 1));
    }
}
