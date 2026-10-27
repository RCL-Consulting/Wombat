using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Curricula;

/// <summary>One item of a trainee's programme, as a record reads it: the EPA live, the target as the programme holds it.</summary>
/// <param name="EpaCode">The EPA's code now: a correction shows on a closed record too (T312).</param>
/// <param name="EpaTitle">The EPA's title now.</param>
/// <param name="EpaInForce">
/// Whether the EPA is in force now (<see cref="CurriculumItemsInForce" />). On a running programme's list it always is. On
/// the list a programme ended with it may not be: an EPA paused after the end stays on the record, headed "(no longer in
/// use)" (T255, T312).
/// </param>
public sealed record ProgrammeItem(
    int CurriculumItemId,
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
/// Which curriculum items a trainee's record reads, and with which targets (T312): the one rule the progress reader, the
/// entrustment standing, MSF coverage and the portfolio export share, so none can read a different list from the others.
/// </summary>
/// <remarks>
/// <para>
/// <b>A running programme</b> reads the catalogue as it stands: the items of the profile's curriculum, the College's and
/// the trainee's own institution's (a curriculum row is shared by every adopting institution, and another institution's
/// local item is a target this trainee could never meet), whose EPA is in force (<see cref="CurriculumItemsInForce" />,
/// T158).
/// </para>
/// <para>
/// <b>An ended programme</b> reads the list it ended with (<see cref="TraineeProfile.EndedCurriculumJson" />), recorded
/// with the end (<see cref="ReadEndedWithAsync" />): every item in force for the trainee on the last day, with its target
/// then. Nothing the catalogue does after the end changes it: an EPA paused since stays, marked as no longer in use; an
/// item added since, or a target changed since, is not read. The EPA's code and title are read live, so a correction still
/// shows. Before T312 a closed record read the live catalogue, and a graduate's record lost an EPA she was entrusted with
/// when it was paused, and gained a shortfall in every closed year when her institution added an item (F-6.21a, F-6.40a).
/// </para>
/// <para>
/// An ended profile with no list recorded (one a test fixture ended without the command) is read as a running one.
/// </para>
/// </remarks>
public static class ProgrammeItems
{
    /// <summary>
    /// The items of <paramref name="profile" />'s curriculum that are the trainee's, in force or not: the College's, and
    /// their own institution's.
    /// </summary>
    public static IQueryable<CurriculumItem> OfCurriculum(IApplicationDbContext dbContext, TraineeProfile profile)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(profile);

        return OfCurriculum(dbContext.Set<CurriculumItem>().AsNoTracking(), profile.CurriculumId, profile.InstitutionId);
    }

    /// <inheritdoc cref="OfCurriculum(IApplicationDbContext, TraineeProfile)" />
    public static IQueryable<CurriculumItem> OfCurriculum(this IQueryable<CurriculumItem> items, int curriculumId, int institutionId)
    {
        ArgumentNullException.ThrowIfNull(items);

        return items.Where(item => item.CurriculumId == curriculumId &&
                                   (item.OwningInstitutionId == null || item.OwningInstitutionId == institutionId));
    }

    /// <summary>Whether the profile's record reads the list its programme ended with rather than the catalogue.</summary>
    public static bool ReadsEndedList(TraineeProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        return !profile.IsActive && profile.EndedCurriculumJson is not null;
    }

    /// <summary>
    /// The list <paramref name="profile" />'s record reads (see the remarks), in EPA-code order: a running programme's items
    /// in force, or the list an ended one ended with.
    /// </summary>
    /// <param name="epaId">Given, only the item for that EPA (one EPA's page).</param>
    public static async Task<IReadOnlyList<ProgrammeItem>> ReadAsync(
        IApplicationDbContext dbContext,
        TraineeProfile profile,
        CancellationToken cancellationToken,
        int? epaId = null)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(profile);

        if (!ReadsEndedList(profile))
        {
            var live = OfCurriculum(dbContext, profile).InForce();
            if (epaId is { } only)
            {
                live = live.Where(item => item.EpaId == only);
            }

            return await Project(live).ToListAsync(cancellationToken);
        }

        var ended = profile.ReadEndedCurriculum()!
            .Where(item => epaId is null || item.EpaId == epaId)
            .ToArray();
        if (ended.Length == 0)
        {
            return [];
        }

        var epaIds = ended.Select(item => item.EpaId).Distinct().ToArray();
        var epas = await dbContext.Set<Epa>()
            .AsNoTracking()
            .Where(epa => epaIds.Contains(epa.Id))
            .Select(epa => new { epa.Id, epa.Code, epa.Title, epa.IsActive })
            .ToDictionaryAsync(epa => epa.Id, cancellationToken);

        // An EPA is never deleted while a curriculum item names it (Restrict), so every one is found; one that were not
        // would have nothing to head its card with, and is left out rather than shown as an id.
        return ended
            .Where(item => epas.ContainsKey(item.EpaId))
            .Select(item =>
            {
                var epa = epas[item.EpaId];
                return new ProgrammeItem(
                    item.CurriculumItemId,
                    item.EpaId,
                    epa.Code,
                    epa.Title,
                    epa.IsActive,
                    item.RequiredCount,
                    item.QuotaPeriod,
                    item.MinimumLevelOrder,
                    item.MinimumLevelByStageJson,
                    item.ScaleId,
                    item.DecisionCadence,
                    item.DecisionIsOpportunistic,
                    item.IsLocal);
            })
            .OrderBy(item => item.EpaCode, StringComparer.Ordinal)
            .ThenBy(item => item.CurriculumItemId)
            .ToArray();
    }

    /// <summary>
    /// The list a programme ending on <paramref name="lastDay" /> ends with: every item of the profile's curriculum that is
    /// the trainee's and in force when the programme ended, with its target as the catalogue holds it now. For the end's
    /// commands (<c>CompleteTraineeProfileCommand</c>, <c>DeactivateTraineeProfileCommand</c>) and an erasure, which read
    /// it before they record the end and keep it with the end in one save (<see cref="TraineeProfile.RecordEndedCurriculum" />).
    /// </summary>
    /// <remarks>
    /// "When the programme ended" is the close of the last day on the South African calendar, or now when the last day is
    /// today, so a pause that began on the last day takes the EPA off the list and one that has not happened yet cannot.
    /// An EPA is in force at a moment as credit judges it (<see cref="CurriculumItemsInForce.InForceAt" />, D48): active, or
    /// its current pause began after the moment. An end recorded after the fact therefore keeps an EPA paused between the
    /// last day and the day the end was recorded, which is the point. What the catalogue cannot say is not guessed: an item
    /// added between the last day and the recording is kept (an item has no creation date), and an EPA paused and
    /// reactivated in that time reads as in force (an EPA keeps only its current pause).
    /// </remarks>
    /// <param name="utcNow">Now, from the caller's clock.</param>
    public static async Task<IReadOnlyList<EndedCurriculumItem>> ReadEndedWithAsync(
        IApplicationDbContext dbContext,
        TraineeProfile profile,
        DateOnly lastDay,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(profile);

        var endOfLastDay = ProgrammeCalendar.EndOf(lastDay);
        var moment = endOfLastDay < utcNow ? endOfLastDay : utcNow;

        var items = await OfCurriculum(dbContext, profile)
            .InForceAt(moment)
            .ToListAsync(cancellationToken);

        return items.Select(EndedCurriculumItem.Of).ToArray();
    }

    /// <summary>
    /// How many ended programmes on <paramref name="curriculumId" /> hold <paramref name="curriculumItemId" /> on the list
    /// they ended with (T312, review B1): what a removal of the item would rewrite.
    /// </summary>
    /// <remarks>
    /// The lists are read and parsed here, never matched as <c>jsonb</c> text (Postgres re-renders what it stores). One
    /// curriculum's ended programmes are few, so they are all read.
    /// </remarks>
    public static async Task<int> EndedRecordsNamingAsync(
        IApplicationDbContext dbContext,
        int curriculumId,
        int curriculumItemId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        var lists = await EndedListsAsync(dbContext, profile => profile.CurriculumId == curriculumId, cancellationToken);
        return lists.Count(list => list.Any(item => item.CurriculumItemId == curriculumItemId));
    }

    /// <summary>
    /// Every item on a list an ended programme ended with (T312), across all curricula: the scale paths ask which ladders
    /// they pin (review B2).
    /// </summary>
    public static async Task<IReadOnlyList<EndedCurriculumItem>> AllEndedItemsAsync(
        IApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        return (await EndedListsAsync(dbContext, _ => true, cancellationToken)).SelectMany(list => list).ToArray();
    }

    private static async Task<IReadOnlyList<IReadOnlyList<EndedCurriculumItem>>> EndedListsAsync(
        IApplicationDbContext dbContext,
        System.Linq.Expressions.Expression<Func<TraineeProfile, bool>> which,
        CancellationToken cancellationToken)
    {
        var stored = await dbContext.Set<TraineeProfile>()
            .AsNoTracking()
            .Where(profile => !profile.IsActive && profile.EndedCurriculumJson != null)
            .Where(which)
            .Select(profile => profile.EndedCurriculumJson)
            .ToListAsync(cancellationToken);

        return stored.Select(json => EndedCurriculumItem.Parse(json) ?? []).ToArray();
    }

    /// <summary>The items as a record reads them, in EPA-code order, whatever their EPA's state.</summary>
    internal static IQueryable<ProgrammeItem> Project(IQueryable<CurriculumItem> items)
        => items
            .OrderBy(item => item.Epa.Code)
            .Select(item => new ProgrammeItem(
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
                item.OwningInstitutionId != null));
}
