using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Curricula.Quota;

/// <summary>
/// How many trainees have met one EPA's target in the current window (T130). One row per distinct target: two
/// curriculum versions that hold the same EPA against different targets give two rows, each labelled with the
/// target its trainees are actually measured against.
/// </summary>
/// <param name="TraineesMet">Trainees whose target applies and is met.</param>
/// <param name="TraineesApplying">
/// Trainees whose curriculum holds the item and whose target applies today: not exempt under D14 and already
/// started. Trainees with no credit at all are included. They are exactly the ones a coverage view exists to show.
/// </param>
/// <param name="TraineesExempt">Trainees who hold the item but are exempt this window, or have not started.</param>
public sealed record EpaTargetCoverage(
    int EpaId,
    string EpaCode,
    string EpaTitle,
    QuotaPeriod QuotaPeriod,
    int Target,
    int TraineesMet,
    int TraineesApplying,
    int TraineesExempt)
{
    /// <summary>For a bar: met as a share of those applying, 0 to 100. Zero when nobody applies.</summary>
    public int PercentMet => TraineesApplying == 0 ? 0 : TraineesMet * 100 / TraineesApplying;

    public bool IsPerSemester => QuotaPeriod == QuotaPeriod.Semester;
}

/// <summary>One trainee's targets in the current window, by kind (T130).</summary>
public sealed record TraineeTargetCoverage(
    string TraineeUserId,
    int SemesterTargetsMet,
    int SemesterTargetsApplying,
    int YearTargetsMet,
    int YearTargetsApplying)
{
    public int TargetsMet => SemesterTargetsMet + YearTargetsMet;
    public int TargetsApplying => SemesterTargetsApplying + YearTargetsApplying;
}

/// <summary>A set of trainees' targets in the current window, per trainee and per EPA.</summary>
/// <param name="ExemptTraineeCount">Trainees none of whose targets apply today (D14, or not yet started).</param>
public sealed record CurriculumCoverage(
    DateOnly AsOf,
    string CurrentSemesterName,
    string CurrentSemesterMonths,
    IReadOnlyList<TraineeTargetCoverage> Trainees,
    IReadOnlyList<EpaTargetCoverage> Epas,
    int ExemptTraineeCount);

/// <summary>
/// The committee, speciality-admin and sub-speciality-admin views of curriculum progress. One implementation,
/// reading through the same <see cref="QuotaProgressCalculator" /> as the trainee's own page (T130).
/// </summary>
/// <remarks>
/// <para>
/// Before T130 each of the three dashboards held its own copy of a lifetime percentage: the mean of
/// <c>min(100, CountsSoFar / RequiredCount)</c>, divided by every active trainee in scope. Once progress is stored
/// per semester, that arithmetic adds every semester's row together and passes 100%. It also counted exempt
/// trainees as 0%, and divided by trainees whose curriculum does not hold the EPA at all.
/// </para>
/// <para>
/// What this reports instead is counts, never a mean: "4 of 9 trainees met this semester's target" tells a
/// coordinator that five have not, where "41%" could mean everyone is halfway. A trainee counts for an EPA only
/// when their curriculum holds the item, the item is national core or their own institution's local extra, and
/// the target applies to them today.
/// </para>
/// </remarks>
public static class CurriculumCoverageReader
{
    public static async Task<CurriculumCoverage> ReadAsync(
        IApplicationDbContext dbContext,
        IReadOnlyCollection<TraineeProfile> activeProfiles,
        DateOnly asOf,
        CancellationToken cancellationToken)
    {
        var semester = AcademicPeriod.Containing(asOf);

        if (activeProfiles.Count == 0)
        {
            return new CurriculumCoverage(asOf, QuotaText.SemesterName(semester), QuotaText.Months(semester), [], [], 0);
        }

        var curriculumIds = activeProfiles.Select(profile => profile.CurriculumId).Distinct().ToList();
        var traineeUserIds = activeProfiles.Select(profile => profile.UserId).Distinct().ToList();

        // In force only (T158): a deactivated EPA's item is owed by nobody, so it has no row here and puts nobody in a
        // denominator. A trainee whose every item is retired then has no targets, which is not an exemption.
        var items = await dbContext.Set<CurriculumItem>()
            .AsNoTracking()
            .InForce()
            .Where(item => curriculumIds.Contains(item.CurriculumId))
            .Select(item => new CoverageItem(
                item.Id,
                item.CurriculumId,
                item.OwningInstitutionId,
                item.EpaId,
                item.Epa.Code,
                item.Epa.Title,
                item.QuotaPeriod,
                item.RequiredCount))
            .ToListAsync(cancellationToken);

        var itemIds = items.Select(item => item.Id).ToList();
        var rows = await dbContext.Set<CurriculumItemProgress>()
            .AsNoTracking()
            .Where(row => traineeUserIds.Contains(row.TraineeUserId) && itemIds.Contains(row.CurriculumItemId))
            .Select(row => new
            {
                row.TraineeUserId,
                Row = new QuotaProgressRow(
                    row.CurriculumItemId,
                    row.AcademicYear,
                    row.Semester,
                    row.CountsSoFar,
                    row.MinimumLevelReachedCount,
                    row.LastObservedOn)
            })
            .ToListAsync(cancellationToken);

        var rowsByTrainee = rows
            .GroupBy(row => row.TraineeUserId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Select(entry => entry.Row).ToList(), StringComparer.Ordinal);

        return Compute(asOf, activeProfiles, items, rowsByTrainee);
    }

    internal sealed record CoverageItem(
        int Id,
        int CurriculumId,
        int? OwningInstitutionId,
        int EpaId,
        string EpaCode,
        string EpaTitle,
        QuotaPeriod QuotaPeriod,
        int RequiredCount);

    internal static CurriculumCoverage Compute(
        DateOnly asOf,
        IReadOnlyCollection<TraineeProfile> activeProfiles,
        IReadOnlyList<CoverageItem> items,
        IReadOnlyDictionary<string, List<QuotaProgressRow>> rowsByTrainee)
    {
        var semester = AcademicPeriod.Containing(asOf);
        var trainees = new List<TraineeTargetCoverage>();

        // Keyed by the target, not by the EPA alone. CloneAsNewVersion keeps the EpaId, so v1 and v2 of a
        // curriculum share EPAs, and an administrator may give v2 a different target. Merging them would label a
        // row with whichever item happened to be seen first while counting trainees against both.
        var perTarget = new Dictionary<(int EpaId, QuotaPeriod QuotaPeriod, int RequiredCount), (CoverageItem First, int Met, int Applying, int Exempt)>();
        var exemptTrainees = 0;

        // Deterministic order, so the same data gives the same rows on every load.
        foreach (var profile in activeProfiles.OrderBy(profile => profile.UserId, StringComparer.Ordinal).ThenBy(profile => profile.Id))
        {
            var traineeRows = rowsByTrainee.TryGetValue(profile.UserId, out var found) ? found : [];
            var traineeItems = items
                .Where(item =>
                    item.CurriculumId == profile.CurriculumId &&
                    (item.OwningInstitutionId is null || item.OwningInstitutionId == profile.InstitutionId))
                .OrderBy(item => item.Id)
                .ToList();

            if (traineeItems.Count == 0)
            {
                // No targets at all is not an exemption: D14 waives a target a trainee has. Such a trainee is
                // left out, as the pre-T130 dashboards left them out.
                continue;
            }

            int semesterMet = 0, semesterApplying = 0, yearMet = 0, yearApplying = 0;
            foreach (var item in traineeItems)
            {
                var tally = QuotaProgressCalculator.For(
                    item.Id, item.QuotaPeriod, item.RequiredCount, traineeRows, profile.ProgrammeStartDate, asOf).Current;

                var key = (item.EpaId, item.QuotaPeriod, item.RequiredCount);
                (CoverageItem First, int Met, int Applying, int Exempt) entry =
                    perTarget.TryGetValue(key, out var existing) ? existing : (item, 0, 0, 0);
                if (tally.Applies)
                {
                    entry.Applying++;
                    if (tally.IsMet)
                    {
                        entry.Met++;
                    }

                    if (item.QuotaPeriod == QuotaPeriod.Semester)
                    {
                        semesterApplying++;
                        semesterMet += tally.IsMet ? 1 : 0;
                    }
                    else
                    {
                        yearApplying++;
                        yearMet += tally.IsMet ? 1 : 0;
                    }
                }
                else
                {
                    entry.Exempt++;
                }

                perTarget[key] = entry;
            }

            if (semesterApplying + yearApplying == 0)
            {
                exemptTrainees++;
                continue;
            }

            trainees.Add(new TraineeTargetCoverage(profile.UserId, semesterMet, semesterApplying, yearMet, yearApplying));
        }

        var epas = perTarget.Values
            .Select(entry => new EpaTargetCoverage(
                entry.First.EpaId,
                entry.First.EpaCode,
                entry.First.EpaTitle,
                entry.First.QuotaPeriod,
                entry.First.RequiredCount,
                entry.Met,
                entry.Applying,
                entry.Exempt))
            .OrderBy(epa => epa.EpaCode, StringComparer.Ordinal)
            .ThenBy(epa => epa.QuotaPeriod)
            .ThenBy(epa => epa.Target)
            .ToList();

        // Fewest targets met first, as a share of those that apply: the trainees a committee needs to see.
        var ordered = trainees
            .OrderBy(trainee => (double)trainee.TargetsMet / trainee.TargetsApplying)
            .ThenBy(trainee => trainee.TraineeUserId, StringComparer.Ordinal)
            .ToList();

        return new CurriculumCoverage(asOf, QuotaText.SemesterName(semester), QuotaText.Months(semester), ordered, epas, exemptTrainees);
    }
}
