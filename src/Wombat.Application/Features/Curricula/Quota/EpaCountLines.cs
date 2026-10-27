using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.Curricula.Quota;

/// <summary>
/// The count a decision made (T355, C5; E5): the trainee's in-force item for the activity's EPA, its window containing the
/// encounter date, tallied as on today, and whether that window is the current one.
/// </summary>
/// <param name="Window">
/// The window the encounter counts towards (<see cref="QuotaWindow.For" /> on its date, so a December encounter counts into
/// semester 2, D40), tallied from the stored progress rows as they stand today, by <see cref="QuotaProgressCalculator" />,
/// the code every progress reader goes through: the line and My progress cannot disagree.
/// </param>
/// <param name="IsCurrentWindow">
/// The window contains today: the line then reads "this semester" / "in 2026"; otherwise it names the window (E5, "PAED-001,
/// Semester 1, 2026: 3 of 3, met.").
/// </param>
public sealed record EpaCountLineDto(
    int EpaId,
    string EpaCode,
    QuotaPeriod QuotaPeriod,
    QuotaWindowDto Window,
    bool IsCurrentWindow);

/// <summary>
/// One reader for the count an activity's EPA and encounter date made (T355, C5; E5), shared by Home's Recent decisions
/// (<c>DecidedOnYours</c>) and the completed card's <c>GetActivityCountLineQuery</c>, so the two say the same count of the
/// same activity (round 1, correction 5: the activity's own window, read live).
/// </summary>
/// <remarks>
/// <para>
/// The item is the one My progress lists for the EPA (<see cref="ProgrammeItems" />): on the curriculum of the trainee's
/// preferred profile (<see cref="TraineeScopeResolver.PreferredProfiles" />), national or the trainee's own institution's,
/// and in force (<see cref="CurriculumItemsInForce" />, T158). A paused EPA has no entry while the programme runs: it is no
/// target while paused (D48), so a count of it would be a figure My progress does not show (note 2: the paused sentence is
/// the words' to say). An ended programme reads the list it ended with, with its targets then (T312).
/// </para>
/// <para>
/// Each window is judged with both ends of the programme (D14, D42, D49), as every reader judges it, so a window the start
/// or the end waives reads as no target with its count. Nothing here reads <c>Activity</c>.
/// </para>
/// </remarks>
public static class EpaCountLines
{
    /// <summary>
    /// The count line of each key, by key: no entry for an EPA that is no in-force item of the trainee's curriculum, nor for
    /// a trainee with no profile.
    /// </summary>
    /// <param name="today">Today on the South African calendar (<see cref="QuotaCalendar.Today(TimeProvider)" />, T325).</param>
    public static async Task<IReadOnlyDictionary<(int EpaId, DateOnly ObservedOn), EpaCountLineDto>> ReadAsync(
        IApplicationDbContext dbContext,
        string traineeUserId,
        IReadOnlyCollection<(int EpaId, DateOnly ObservedOn)> keys,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(keys);

        var lines = new Dictionary<(int EpaId, DateOnly ObservedOn), EpaCountLineDto>();
        if (keys.Count == 0 || string.IsNullOrEmpty(traineeUserId))
        {
            return lines;
        }

        var profile = await TraineeScopeResolver.PreferredProfiles(dbContext)
            .AsNoTracking()
            .Where(entity => entity.UserId == traineeUserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return lines;
        }

        // The list My progress reads (ProgrammeItems, T312): a running programme's items in force, or the list an ended one
        // ended with, so a graduate's count line keeps an EPA paused since her end and its target as it was then.
        var epaIds = keys.Select(key => key.EpaId).ToHashSet();
        var items = (await ProgrammeItems.ReadAsync(dbContext, profile, cancellationToken))
            .Where(item => epaIds.Contains(item.EpaId))
            .Select(item => new
            {
                Id = item.CurriculumItemId,
                item.EpaId,
                item.EpaCode,
                item.RequiredCount,
                item.QuotaPeriod
            })
            .ToList();

        if (items.Count == 0)
        {
            return lines;
        }

        // One item per EPA per curriculum; should two ever exist, the older row is the one read, every time.
        var itemByEpa = items
            .GroupBy(item => item.EpaId)
            .ToDictionary(group => group.Key, group => group.OrderBy(item => item.Id).First());
        var itemIds = itemByEpa.Values.Select(item => item.Id).ToArray();

        var rows = await dbContext.Set<CurriculumItemProgress>()
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

        foreach (var key in keys.Distinct())
        {
            if (!itemByEpa.TryGetValue(key.EpaId, out var item))
            {
                continue;
            }

            // The window containing the encounter, tallied from today's rows: For's Current is that window, whatever day
            // it is read for, because the rows hold every credit already given (E5).
            var encounter = QuotaProgressCalculator.For(
                item.Id, item.QuotaPeriod, item.RequiredCount, rows, profile.ProgrammeStartDate, profile.EndedOn, key.ObservedOn)
                .Current;
            var current = QuotaWindow.For(item.QuotaPeriod, today, profile.ProgrammeStartDate, profile.EndedOn);

            lines[key] = new EpaCountLineDto(
                item.EpaId,
                item.EpaCode,
                item.QuotaPeriod,
                QuotaWindowDto.From(encounter),
                encounter.Window.Start == current.Start);
        }

        return lines;
    }
}
