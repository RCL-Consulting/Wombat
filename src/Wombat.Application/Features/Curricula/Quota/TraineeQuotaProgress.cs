using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

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

    /// <summary>
    /// Whether the window is over by <paramref name="today" />: past its last counted day, which for a window ending in
    /// semester 2 is 31 December, a month after the College's own last day, because December encounters count towards it
    /// (D40). Until then its count is a count so far, not a result, whether or not the trainee is still in the programme:
    /// an encounter observed before the end can still be credited after it (T252).
    /// </summary>
    public bool HasClosedBy(DateOnly today) => today > AcademicPeriod.Containing(NominalEnd).End;

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

    /// <summary>
    /// How often a committee decides this EPA (<see cref="CurriculumItem.DecisionCadence" />, Annexure B), or null where
    /// the item has none (KGK-001). Independent of <see cref="QuotaPeriod" />: PAED-003 is observed each semester and
    /// decided once a year, so the index names the cadence in its own words (T355, C10; notes 8, 9).
    /// </summary>
    public QuotaPeriod? DecisionCadence { get; init; }

    /// <summary>"Decided as opportunity allows" (<see cref="CurriculumItem.DecisionIsOpportunistic" />; T355, C10).</summary>
    public bool DecisionIsOpportunistic { get; init; }

    /// <summary>
    /// The institution whose own item this is ("Kgosi Kgari Teaching Hospital"), or null for the College's (T355, C10).
    /// Only ever the trainee's own institution: another institution's local item is never read.
    /// </summary>
    public string? OwningInstitutionName { get; init; }

    /// <summary>An institution's own item rather than the College's.</summary>
    public bool IsLocal => OwningInstitutionName is not null;

    /// <summary>
    /// The EPA's exit level, as the entrustment standing reads it (<see cref="CurriculumItem.MinimumLevelOrder" />), and as
    /// the rung a clinician reads (T355, C10). Null only from a producer that does not read it.
    /// </summary>
    public int? ExitLevelOrder { get; init; }

    /// <inheritdoc cref="ExitLevelOrder" />
    public string? ExitLevelLabel { get; init; }

    /// <summary>
    /// Whether <see cref="EffectiveMinimumLevelLabel" /> is the curriculum's level for the training year (Annexure A's per-year
    /// map names it) rather than the item's flat minimum, which stands in where the map names none (an item with no map,
    /// such as KGK-001, or a trainee past the years it names). The EPA page says "Training year 4: level 5, …" for the first
    /// and "Minimum 3a" for the second (T355, C4), as the standing says it (<c>EpaStandingDto.YearTargetIsExitLevel</c>).
    /// </summary>
    public bool MinimumByTrainingYear { get; init; }

    /// <summary>
    /// Whether the EPA is in force now (<see cref="CurriculumItemsInForce" />, D48). Every item of
    /// <see cref="TraineeCurriculumProgressSummaryDto.Items" /> is; one EPA's page reads its item whether or not it is
    /// (T355, round 1 correction 2), and a paused one is read but counted nowhere.
    /// </summary>
    public bool EpaInForce { get; init; } = true;
}

/// <summary>
/// An item of the trainee's curriculum whose EPA is not in force now (D48): My progress's "No longer in use" group (T355,
/// Spec § 6). Read, never counted: no figure of the summary includes it.
/// </summary>
public sealed record PausedItemDto(int CurriculumItemId, int EpaId, string EpaCode, string EpaTitle, QuotaPeriod QuotaPeriod);

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
/// <param name="Ended">
/// How and when the programme ended, when the trainee holds no current programme and this is the one they ended on
/// (T252). Null while the programme runs, and on a read of a profile the caller resolved
/// (<see cref="TraineeQuotaProgressReader.ReadForProfileAsync" />), which says nothing about how it chose it. When set,
/// every figure above is read as on the last day (<see cref="AsOf" />), and each item carries every period of the
/// programme (<see cref="TraineeCurriculumProgressDto.Periods" />).
/// </param>
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
    IReadOnlyList<TraineeCurriculumProgressDto> Items,
    ProgrammeEndDto? Ended = null)
{
    /// <summary>
    /// The profile's items whose EPA is not in force now (national, or the trainee's own institution's), by code: the
    /// complement of <see cref="Items" /> (<see cref="CurriculumItemsInForce.NotInForce" />). Listed so a page can say why
    /// they are not targets, and counted in no figure above (D48; T355, Spec § 6).
    /// </summary>
    public IReadOnlyList<PausedItemDto> Paused { get; init; } = [];
}

/// <summary>The first window of a kind a trainee is held to: "semester 1, 2027", from 1 January 2027.</summary>
public sealed record QuotaStartDto(string Name, DateOnly StartsOn);

/// <summary>How a trainee's programme ended, for the reader of a programme that has (T252).</summary>
/// <param name="Completed">
/// True for a completion (graduation, <c>TraineeProfile.CompletedOn</c>); false for a programme ended without being
/// completed (a withdrawal, <c>TraineeProfile.DeactivatedOn</c>).
/// </param>
/// <param name="EndedOn">
/// The last day (<c>TraineeProfile.EndedOn</c>), which D49 reads. Null only for a profile deactivated before Wombat
/// recorded the day (T209), whose periods are then read up to <paramref name="Today" />.
/// </param>
/// <param name="Today">
/// The day the record was read, on the South African calendar. The summary is read as on <paramref name="EndedOn" />, so
/// this is the day that says whether a period has closed (<see cref="QuotaWindowDto.HasClosedBy" />): one that has not
/// is a count so far, since an encounter observed before the end can still be credited after it.
/// </param>
public sealed record ProgrammeEndDto(bool Completed, DateOnly? EndedOn, DateOnly Today);

/// <summary>
/// Builds one trainee's quota progress. Shared by the progress page and the trainee dashboard, so the two
/// cannot disagree (T130).
/// </summary>
public static class TraineeQuotaProgressReader
{
    /// <summary>
    /// The trainee's own progress: their current programme read on <paramref name="today" />, or, when they hold no
    /// current programme, the one they ended on, read as on its last day and marked as ended
    /// (<see cref="TraineeCurriculumProgressSummaryDto.Ended" />, T252). Null when they hold no trainee profile at all.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The profile is the trainee's preferred one (<see cref="TraineeScopeResolver.PreferredProfiles" />, T185): the active
    /// one whenever there is one, else the most recent. It is the pick credit, the activity scope stamp, the standing, the
    /// MSF coverage and the portfolio export all make, so a graduate's progress page reads the programme their export's
    /// cover names. Until T252 this read the active profile only, so a completed or withdrawn trainee was told "No
    /// curriculum items assigned yet" while the PDF printed their periods.
    /// </para>
    /// <para>
    /// An ended programme is read as the portfolio export reads one (<c>PortfolioEpaProgress.ReadOn</c>): as on the last
    /// day, never later, so no period that began after it is listed (D49: outside the programme), with every window back
    /// to the programme start. The window the programme ended in holds no target unless it ended in that window's last
    /// month (D49), which <see cref="QuotaProgressCalculator" /> applies. A profile deactivated before Wombat recorded its
    /// day is read on <paramref name="today" />.
    /// </para>
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
        DateOnly today,
        CancellationToken cancellationToken)
    {
        var profile = await TraineeScopeResolver.PreferredProfiles(dbContext)
            .AsNoTracking()
            .Where(p => p.UserId == traineeUserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return null;
        }

        if (profile.IsActive)
        {
            return await ReadForProfileAsync(dbContext, profile, today, periodsFrom: null, cancellationToken);
        }

        var lastDay = profile.EndedOn is { } ended && ended < today ? ended : today;
        var summary = await ReadForProfileAsync(dbContext, profile, lastDay, periodsFrom: DateOnly.MinValue, cancellationToken);

        return summary with { Ended = new ProgrammeEndDto(profile.CompletedOn is not null, profile.EndedOn, today) };
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

        var items = await FactsAsync(ItemsOf(dbContext, profile).InForce(), cancellationToken);
        var rows = await RowsAsync(dbContext, traineeUserId, items, cancellationToken);
        var rungs = await EntrustmentRungLabels.LoadAsync(dbContext, items.Select(item => item.ScaleId), cancellationToken);
        var owner = await OwnerNameAsync(dbContext, profile, items, cancellationToken);

        var result = items
            .Select(item => Build(item, rows, rungs, owner, profile, stage, asOf, periodsFrom))
            .ToList();

        // T355, Spec § 6: what is not in force is listed apart, so My progress can say why it is no target. Never counted.
        var paused = await ItemsOf(dbContext, profile)
            .NotInForce()
            .OrderBy(item => item.Epa.Code)
            .Select(item => new PausedItemDto(item.Id, item.EpaId, item.Epa.Code, item.Epa.Title, item.QuotaPeriod))
            .ToListAsync(cancellationToken);

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
            result)
        {
            Paused = paused
        };
    }

    /// <summary>
    /// One item of the profile's curriculum, by its EPA, read by the same code as each row of
    /// <see cref="ReadForProfileAsync" />, so one EPA's page and My progress's row cannot disagree (T355, round 1
    /// correction 2). Read whether or not its EPA is in force (<see cref="TraineeCurriculumProgressDto.EpaInForce" />): a
    /// paused EPA's page still says what it is. Null when the EPA is no item of the profile's curriculum, national or the
    /// profile's own institution's (another institution's local item is never this trainee's).
    /// </summary>
    /// <param name="periodsFrom">As <see cref="ReadForProfileAsync" />'s: given, the item carries every window back to it.</param>
    public static async Task<TraineeCurriculumProgressDto?> ReadItemAsync(
        IApplicationDbContext dbContext,
        TraineeProfile profile,
        int epaId,
        DateOnly asOf,
        DateOnly? periodsFrom,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var items = await FactsAsync(ItemsOf(dbContext, profile).Where(item => item.EpaId == epaId), cancellationToken);
        if (items.Count == 0)
        {
            return null;
        }

        // One item per EPA per curriculum, and a national item never shares an EPA with a local one (T223); the lowest
        // id if that ever failed, so the answer is total.
        items = [items.OrderBy(item => item.Id).First()];
        var rows = await RowsAsync(dbContext, profile.UserId, items, cancellationToken);
        var rungs = await EntrustmentRungLabels.LoadAsync(dbContext, items.Select(item => item.ScaleId), cancellationToken);
        var owner = await OwnerNameAsync(dbContext, profile, items, cancellationToken);

        return Build(items[0], rows, rungs, owner, profile, profile.GetStage(asOf), asOf, periodsFrom);
    }

    /// <summary>What a row reads of one item.</summary>
    private sealed record ItemFacts(
        int Id,
        int EpaId,
        string EpaCode,
        string EpaTitle,
        bool EpaInForce,
        int RequiredCount,
        QuotaPeriod QuotaPeriod,
        int MinimumLevelOrder,
        string? MinimumLevelByStageJson,
        int? ScaleId,
        QuotaPeriod? DecisionCadence,
        bool DecisionIsOpportunistic,
        bool IsLocal);

    /// <summary>
    /// The EPAs whose page under My progress opens for <paramref name="traineeUserId" />: every item of their preferred
    /// profile's curriculum, national or their own institution's, in force or paused, exactly what
    /// <c>GetEpaProgressForTraineeQuery</c> finds. Empty when they hold no profile. My activities links a credit only to
    /// such an EPA's page, so a curriculum move or a removed item never makes "Credited 1 item" a link to "Page not
    /// found" (T355, build review G4).
    /// </summary>
    public static async Task<IReadOnlySet<int>> EpasWithAPageAsync(
        IApplicationDbContext dbContext, string traineeUserId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        var profile = await TraineeScopeResolver.PreferredProfiles(dbContext)
            .AsNoTracking()
            .Where(entity => entity.UserId == traineeUserId)
            .FirstOrDefaultAsync(cancellationToken);
        if (profile is null)
        {
            return new HashSet<int>();
        }

        return (await ItemsOf(dbContext, profile).Select(item => item.EpaId).Distinct().ToListAsync(cancellationToken))
            .ToHashSet();
    }

    /// <summary>
    /// The profile's items: the national core plus the trainee's own institution's local extras. A curriculum row is
    /// shared by every adopting institution, and another institution's local item is a target this trainee could never
    /// meet.
    /// </summary>
    private static IQueryable<CurriculumItem> ItemsOf(IApplicationDbContext dbContext, TraineeProfile profile)
        => dbContext.Set<CurriculumItem>()
            .AsNoTracking()
            .Where(item => item.CurriculumId == profile.CurriculumId &&
                           (item.OwningInstitutionId == null || item.OwningInstitutionId == profile.InstitutionId));

    private static Task<List<ItemFacts>> FactsAsync(IQueryable<CurriculumItem> items, CancellationToken cancellationToken)
        => items
            .OrderBy(item => item.Epa.Code)
            .Select(item => new ItemFacts(
                item.Id,
                item.EpaId,
                item.Epa.Code,
                item.Epa.Title,
                item.Epa.IsActive,
                item.RequiredCount,
                item.QuotaPeriod,
                item.MinimumLevelOrder,
                item.MinimumLevelByStageJson,
                item.ScaleId,
                item.DecisionCadence,
                item.DecisionIsOpportunistic,
                item.OwningInstitutionId != null))
            .ToListAsync(cancellationToken);

    private static async Task<List<QuotaProgressRow>> RowsAsync(
        IApplicationDbContext dbContext,
        string traineeUserId,
        IReadOnlyCollection<ItemFacts> items,
        CancellationToken cancellationToken)
    {
        var itemIds = items.Select(item => item.Id).ToList();
        return itemIds.Count == 0
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
    }

    /// <summary>
    /// The name a local item's badge carries ("Kgosi Kgari Teaching Hospital's own", T355 C10): always the profile's own
    /// institution, the only one whose local items <see cref="ItemsOf" /> reads. Null when no item is local.
    /// </summary>
    private static async Task<string?> OwnerNameAsync(
        IApplicationDbContext dbContext,
        TraineeProfile profile,
        IReadOnlyCollection<ItemFacts> items,
        CancellationToken cancellationToken)
        => items.Any(item => item.IsLocal)
            ? await dbContext.Set<Institution>()
                .AsNoTracking()
                .Where(institution => institution.Id == profile.InstitutionId)
                .Select(institution => institution.Name)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

    private static TraineeCurriculumProgressDto Build(
        ItemFacts item,
        IReadOnlyList<QuotaProgressRow> rows,
        EntrustmentRungLookup rungs,
        string? ownerName,
        TraineeProfile profile,
        int? stage,
        DateOnly asOf,
        DateOnly? periodsFrom)
    {
        var programmeEnd = profile.EndedOn;
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

        return new TraineeCurriculumProgressDto(
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
            periods)
        {
            DecisionCadence = item.DecisionCadence,
            DecisionIsOpportunistic = item.DecisionIsOpportunistic,
            OwningInstitutionName = item.IsLocal ? ownerName ?? $"Institution {profile.InstitutionId}" : null,
            // The exit level is the item's flat minimum: the level the standing's exit rule reads (T166).
            ExitLevelOrder = item.MinimumLevelOrder,
            ExitLevelLabel = rungs.Format(item.ScaleId, item.MinimumLevelOrder),
            MinimumByTrainingYear = stage is { } year &&
                                    CurriculumItem.ParseStageOverrides(item.MinimumLevelByStageJson).ContainsKey(year),
            EpaInForce = item.EpaInForce
        };
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
