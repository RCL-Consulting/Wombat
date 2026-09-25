using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
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
/// <param name="LastObservedOnDeclared">
/// Whether somebody stated <paramref name="LastObservedOn" />. False means it is only the day a form was created, which
/// a reader marks with <c>EncounterDate.Label</c> ("not recorded (created …)", T219).
/// </param>
/// <param name="FirstCountedName">
/// When the target is waived because the window is before the programme start or cut short by it (D14), the first window
/// the trainee is held to. Null otherwise, and never said of a window the programme's end waives (D49): that trainee's
/// targets did not start later, they stopped.
/// </param>
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
    bool LastObservedOnDeclared,
    string? FirstCountedName,
    DateOnly? FirstCountedOn)
{
    public bool Applies => Status == QuotaWindowStatus.Counting;

    /// <summary>Exempt because the programme started part-way through the window (D14, D42).</summary>
    public bool IsExempt => Status == QuotaWindowStatus.ExemptPartialPeriod;

    public bool NotStarted => Status == QuotaWindowStatus.NotStarted;

    /// <summary>Exempt because the programme ended part-way through the window, before its last month (D49).</summary>
    public bool EndedPartWay => Status == QuotaWindowStatus.ExemptProgrammeEnded;

    /// <summary>The whole window is after the programme ended: outside it, and never short (D49).</summary>
    public bool IsAfterProgrammeEnd => Status == QuotaWindowStatus.AfterProgrammeEnd;

    public static QuotaWindowDto From(QuotaWindowTally tally)
    {
        var startsLater = tally.Window.Status is QuotaWindowStatus.NotStarted or QuotaWindowStatus.ExemptPartialPeriod;

        return new(
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
            tally.LastObservedOnDeclared,
            startsLater ? QuotaText.FirstCountedName(tally.Window) : null,
            startsLater ? tally.Window.FirstCountedPeriod?.Start : null);
    }
}

/// <summary>One curriculum item of a trainee's progress (T130).</summary>
/// <param name="EpaId">The item's EPA. An EPA code is unique only within its namespace (national, or one institution's
/// local extras), so a reader joining other per-EPA figures to this item joins on the id.</param>
/// <param name="Target">The item's target per window: <see cref="CurriculumItem.RequiredCount" />.</param>
/// <param name="Current">The window containing today.</param>
/// <param name="Previous">The window before it, or null when the trainee had not started then.</param>
/// <param name="EffectiveMinimumLevelLabel">
/// The minimum for the trainee's training year TODAY, as the rung a clinician reads ("3a", T100). The encounters
/// in <see cref="Current" /> were each judged against the minimum of the training year they were observed in,
/// which can differ when the training year changed inside the window. <see cref="TrainingYearChangedOn" /> says
/// when that happened.
/// </param>
/// <param name="Periods">
/// When the caller asked for a span, every window of the item's kind from <see cref="Current" /> back to the one
/// containing the span's first day, newest first (<see cref="QuotaProgressCalculator.Since" />): <see cref="Current" />,
/// then <see cref="Previous" /> when the span reaches it, then earlier ones, never one before the programme start.
/// Null when the caller asked for no span: the progress page and the dashboard show the current and previous windows
/// only. (T169, the portfolio export)
/// </param>
public sealed record TraineeCurriculumProgressDto(
    int CurriculumItemId,
    int EpaId,
    string EpaCode,
    string EpaTitle,
    QuotaPeriod QuotaPeriod,
    int Target,
    QuotaWindowDto Current,
    QuotaWindowDto? Previous,
    int EffectiveMinimumLevelOrder,
    string EffectiveMinimumLevelLabel,
    DateOnly? TrainingYearChangedOn,
    IReadOnlyList<QuotaWindowDto>? Periods = null)
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
        // The trainee's preferred profile, the one pick credit, the activity scope stamp and the export all make
        // (TraineeScopeResolver.PreferredProfiles, T185), and only while it is current: this page is the trainee's
        // own current programme, read against today's windows, and a completed programme has no window today. The
        // preferred profile is the active one whenever there is one, so "active only" chooses no differently; until
        // T185 this broke ties among active profiles by the latest programme start, which only a store without the
        // one-active-profile index could hold, and there it read a different curriculum from the one credit landed on.
        var profile = await TraineeScopeResolver.PreferredProfiles(dbContext)
            .AsNoTracking()
            .Where(p => p.UserId == traineeUserId && p.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        return profile is null
            ? null
            : await ReadForProfileAsync(dbContext, profile, asOf, periodsFrom: null, cancellationToken);
    }

    /// <summary>
    /// The same read for a profile the caller has already resolved, active or not, and when
    /// <paramref name="periodsFrom" /> is given, each item also carries every window from <paramref name="asOf" />'s
    /// back to <paramref name="periodsFrom" />'s (<see cref="TraineeCurriculumProgressDto.Periods" />). Everything else
    /// is the same read, so the two cannot disagree.
    /// </summary>
    /// <remarks>
    /// For the portfolio export (T169), which reads the profile its cover names (<c>TraineeScopeResolver</c>'s
    /// preferred profile, the one the export was authorised against), so the programme on the cover and the targets
    /// in the per-EPA section are the same programme. That profile may be a completed one: a graduation export is
    /// read as on the completion day, which is the caller's to choose as <paramref name="asOf" />. Whatever day is
    /// chosen, every window is judged with the profile's actual end (<c>TraineeProfile.EndedOn</c>, D49, T209): the one
    /// the programme ended in before its last month holds no target, and any after it is outside the programme.
    /// </remarks>
    public static async Task<TraineeCurriculumProgressSummaryDto> ReadForProfileAsync(
        IApplicationDbContext dbContext,
        TraineeProfile profile,
        DateOnly asOf,
        DateOnly? periodsFrom,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(profile);
        var traineeUserId = profile.UserId;
        var programmeEnd = profile.EndedOn;

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
                item.EpaId,
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
                    row.LastObservedOn,
                    row.LastObservedOnDeclared))
                .ToListAsync(cancellationToken);

        var rungs = await EntrustmentRungLabels.LoadAsync(dbContext, items.Select(item => item.ScaleId), cancellationToken);

        var result = new List<TraineeCurriculumProgressDto>(items.Count);
        foreach (var item in items)
        {
            var progress = QuotaProgressCalculator.For(
                item.Id, item.QuotaPeriod, item.RequiredCount, rows, profile.ProgrammeStartDate, programmeEnd, asOf);

            var effectiveMinimum = new CurriculumItem
            {
                MinimumLevelOrder = item.MinimumLevelOrder,
                MinimumLevelByStageJson = item.MinimumLevelByStageJson
            }.GetMinimumLevelForStage(stage);

            var periods = periodsFrom is { } since
                ? QuotaProgressCalculator
                    .Since(item.Id, item.QuotaPeriod, item.RequiredCount, rows, profile.ProgrammeStartDate, programmeEnd, since, asOf)
                    .Select(QuotaWindowDto.From)
                    .ToArray()
                : null;

            result.Add(new TraineeCurriculumProgressDto(
                item.Id,
                item.EpaId,
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
                TrainingYearChangedWithin(profile, progress.Current.Window, asOf),
                periods));
        }

        var semester = AcademicPeriod.Containing(asOf);
        var semesterWindow = QuotaWindow.For(QuotaPeriod.Semester, asOf, profile.ProgrammeStartDate, programmeEnd);
        var yearWindow = QuotaWindow.For(QuotaPeriod.AcademicYear, asOf, profile.ProgrammeStartDate, programmeEnd);
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
    /// that is already behind them. Only the start waives a target until a later window (D14): a window the end waives
    /// (D49) says nothing here, because no later window holds the trainee to anything.
    /// </remarks>
    private static QuotaStartDto? StartOf(QuotaWindow window, DateOnly programmeStart)
        => window.Status is not (QuotaWindowStatus.NotStarted or QuotaWindowStatus.ExemptPartialPeriod)
           || window.FirstCountedPeriod is not { } first
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
