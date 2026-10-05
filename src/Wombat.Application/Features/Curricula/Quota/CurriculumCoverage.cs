using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

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

    /// <summary>
    /// The institution whose local extra this EPA is, "Kgosi Kgari Teaching Hospital", or null for the College's own
    /// (T358, flow 06: Targets by EPA's cadence line says "Kgosi Kgari Teaching Hospital's own").
    /// </summary>
    public string? OwningInstitutionName { get; init; }
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

    /// <summary>
    /// The trainee's name as stored, "Nomsa Mahlangu", or null when no account names them (T358: the reader orders ties by
    /// surname, so it reads the names, and its callers need not read them again).
    /// </summary>
    public string? Name { get; init; }
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
    /// <param name="users">
    /// Who the trainees are: their names order equal shares by surname, then first name (T298), for every caller alike
    /// (T358, moved here from the committee handler so the committee's, the speciality admins' and Programme trainees'
    /// rows agree).
    /// </param>
    public static async Task<CurriculumCoverage> ReadAsync(
        IApplicationDbContext dbContext,
        IUserAdministrationService users,
        IReadOnlyCollection<TraineeProfile> activeProfiles,
        DateOnly asOf,
        CancellationToken cancellationToken)
    {
        var facts = await LoadAsync(dbContext, users, activeProfiles, cancellationToken);
        return Compute(asOf, activeProfiles, facts);
    }

    /// <summary>
    /// What a coverage read reads, once: the profiles' in-force items, their credit rows, and their names. Programme
    /// trainees reads it too (<c>ProgrammeRosterReader</c>, T358), so its per-registrar figures and Targets by EPA come
    /// from the same rows.
    /// </summary>
    internal static async Task<CoverageFacts> LoadAsync(
        IApplicationDbContext dbContext,
        IUserAdministrationService users,
        IReadOnlyCollection<TraineeProfile> activeProfiles,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(activeProfiles);

        if (activeProfiles.Count == 0)
        {
            return new CoverageFacts([], new Dictionary<string, List<QuotaProgressRow>>(StringComparer.Ordinal), NoContacts);
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

        // The owners' names, for a local extra's cadence line (T358).
        var ownerIds = items.Select(item => item.OwningInstitutionId).OfType<int>().Distinct().ToArray();
        if (ownerIds.Length > 0)
        {
            var owners = await dbContext.Set<Institution>()
                .AsNoTracking()
                .Where(institution => ownerIds.Contains(institution.Id))
                .ToDictionaryAsync(institution => institution.Id, institution => institution.Name, cancellationToken);
            items = items
                .Select(item => item.OwningInstitutionId is int owner
                    ? item with { OwningInstitutionName = owners.GetValueOrDefault(owner) }
                    : item)
                .ToList();
        }

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
                    row.LastObservedOn,
                    row.LastObservedOnDeclared)
            })
            .ToListAsync(cancellationToken);

        var rowsByTrainee = rows
            .GroupBy(row => row.TraineeUserId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Select(entry => entry.Row).ToList(), StringComparer.Ordinal);

        // Names for exactly the trainees read; the contact, not the display name, because ties are broken by surname, and
        // "Pieter du Plessis" cannot be split (T298).
        var contacts = await users.GetContactsAsync(traineeUserIds, cancellationToken) ?? NoContacts;

        return new CoverageFacts(items, rowsByTrainee, contacts);
    }

    private static readonly IReadOnlyDictionary<string, UserContact> NoContacts =
        new Dictionary<string, UserContact>(StringComparer.Ordinal);

    internal sealed record CoverageItem(
        int Id,
        int CurriculumId,
        int? OwningInstitutionId,
        int EpaId,
        string EpaCode,
        string EpaTitle,
        QuotaPeriod QuotaPeriod,
        int RequiredCount)
    {
        public string? OwningInstitutionName { get; init; }
    }

    internal sealed record CoverageFacts(
        IReadOnlyList<CoverageItem> Items,
        IReadOnlyDictionary<string, List<QuotaProgressRow>> RowsByTrainee,
        IReadOnlyDictionary<string, UserContact> Contacts);

    /// <summary>
    /// One profile's items (the national core and its own institution's local extras), each with its current window's
    /// tally, in item id order: the one tally every coverage figure and every Programme trainees figure is counted from.
    /// </summary>
    internal static IReadOnlyList<(CoverageItem Item, QuotaWindowTally Tally)> TalliesFor(
        TraineeProfile profile,
        CoverageFacts facts,
        DateOnly asOf)
    {
        var traineeRows = facts.RowsByTrainee.TryGetValue(profile.UserId, out var found) ? found : [];

        // The end is passed as every reader passes it (D49), so an ended programme is never held to a target.
        return facts.Items
            .Where(item =>
                item.CurriculumId == profile.CurriculumId &&
                (item.OwningInstitutionId is null || item.OwningInstitutionId == profile.InstitutionId))
            .OrderBy(item => item.Id)
            .Select(item => (item, QuotaProgressCalculator.For(
                item.Id, item.QuotaPeriod, item.RequiredCount, traineeRows, profile.ProgrammeStartDate, profile.EndedOn, asOf).Current))
            .ToList();
    }

    /// <summary>
    /// The name a trainee is listed by, "Nomsa Mahlangu"; null when no account names them (an erased trainee's pseudonym).
    /// </summary>
    internal static string? NameOf(IReadOnlyDictionary<string, UserContact> contacts, string userId)
        => contacts.TryGetValue(userId, out var contact) && $"{contact.FirstName} {contact.LastName}".Trim() is { Length: > 0 } name
            ? name
            : null;

    /// <summary>
    /// The tie-break every list of the programme's registrars uses (T298): surname, then first name, then the id, which
    /// only separates two people of one name. A trainee with no name on record sorts by the id, as UserDisplayNames reads.
    /// </summary>
    internal static IOrderedEnumerable<T> ThenBySurname<T>(
        this IOrderedEnumerable<T> ordered,
        Func<T, string> userIdOf,
        IReadOnlyDictionary<string, UserContact> contacts)
        => ordered
            .ThenBy(entry => contacts.TryGetValue(userIdOf(entry), out var contact) ? contact.LastName : userIdOf(entry),
                StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(entry => contacts.TryGetValue(userIdOf(entry), out var contact) ? contact.FirstName : string.Empty,
                StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(userIdOf, StringComparer.Ordinal);

    internal static CurriculumCoverage Compute(
        DateOnly asOf,
        IReadOnlyCollection<TraineeProfile> activeProfiles,
        CoverageFacts facts)
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
            var tallies = TalliesFor(profile, facts, asOf);
            if (tallies.Count == 0)
            {
                // No targets at all is not an exemption: D14 waives a target a trainee has. Such a trainee is
                // left out, as the pre-T130 dashboards left them out.
                continue;
            }

            int semesterMet = 0, semesterApplying = 0, yearMet = 0, yearApplying = 0;
            foreach (var (item, tally) in tallies)
            {
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

            trainees.Add(new TraineeTargetCoverage(profile.UserId, semesterMet, semesterApplying, yearMet, yearApplying)
            {
                Name = NameOf(facts.Contacts, profile.UserId)
            });
        }

        // Fewest registrars met first, then by code (T358, review 13: Targets by EPA leads with the EPA the fewest have
        // met, so KGK-001 heads the card at the story's end), then the window and the target, which only separate two
        // curriculum versions' targets for one EPA.
        var epas = perTarget.Values
            .Select(entry => new EpaTargetCoverage(
                entry.First.EpaId,
                entry.First.EpaCode,
                entry.First.EpaTitle,
                entry.First.QuotaPeriod,
                entry.First.RequiredCount,
                entry.Met,
                entry.Applying,
                entry.Exempt)
            {
                OwningInstitutionName = entry.First.OwningInstitutionName
            })
            .OrderBy(epa => epa.TraineesMet)
            .ThenBy(epa => epa.EpaCode, StringComparer.Ordinal)
            .ThenBy(epa => epa.QuotaPeriod)
            .ThenBy(epa => epa.Target)
            .ToList();

        // Fewest targets met first, as a share of those that apply: the trainees a committee needs to see. Equal shares
        // by surname, then first name (T298), for every caller (T358).
        var ordered = trainees
            .OrderBy(trainee => (double)trainee.TargetsMet / trainee.TargetsApplying)
            .ThenBySurname(trainee => trainee.TraineeUserId, facts.Contacts)
            .ToList();

        return new CurriculumCoverage(asOf, QuotaText.SemesterName(semester), QuotaText.Months(semester), ordered, epas, exemptTrainees);
    }
}
