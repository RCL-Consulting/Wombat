using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Programme.Filing;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Programme.Trainees;

/// <summary>
/// The programme's current registrars in one acting role's scope, each with this period's figures, the EPAs furthest from
/// target, and the last filing (T358, flow 06, lane A2; Q1, C6, C7, E5; reviews 9 and 16): Programme trainees, and Home's
/// Registrars and Nothing filed cards.
/// </summary>
/// <remarks>
/// <para>
/// <b>Who is listed.</b> The scope's profiles (<see cref="ProgrammeScope.Profiles" />, the acting role's, never the union of
/// the roles held, E4), less any that is not a current trainee's (<see cref="TraineeScopeResolver.KeepCurrentAsync" />,
/// T238): an ended programme, an erased trainee's pseudonym, an account that lost Trainee or that an administrator locked
/// (T268) is nobody to follow up.
/// </para>
/// <para>
/// <b>The figures are My progress's.</b> Each registrar's items are tallied by <see cref="QuotaProgressCalculator" />
/// through <see cref="CurriculumCoverageReader" />'s own tally, from the same rows Targets by EPA counts, so "1 of 3" here,
/// on My progress and in the EPA's count on Home cannot disagree. Exemption is computed (D14, D42), so a row says why in
/// words, never "as stored" (review 9).
/// </para>
/// <para>
/// <b>The order</b> (T298, D11): fewest targets met first, as a share of those that apply, then surname, first name and id;
/// an exempt registrar last. Short on an EPA: furthest from its target first, then fewest met, then surname. Nothing filed
/// alone: longest without a filing first. Nothing here reads an activity row but through <see cref="FilingMoments" />,
/// which returns dates.
/// </para>
/// </remarks>
public static class ProgrammeRosterReader
{
    /// <summary>How many EPAs Furthest short names (D11).</summary>
    public const int FurthestShortCount = 3;

    public static async Task<ProgrammeRosterRead> ReadAsync(
        IApplicationDbContext dbContext,
        IUserAdministrationService users,
        ProgrammeScopeDto scope,
        ProgrammeTraineesFilter filter,
        DateOnly asOf,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(filter);

        var profiles = await ProgrammeScope.Profiles(dbContext, scope)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var current = await TraineeScopeResolver.KeepCurrentAsync(dbContext, users, profiles, cancellationToken);

        var facts = await CurriculumCoverageReader.LoadAsync(dbContext, users, current, cancellationToken);
        var coverage = CurriculumCoverageReader.Compute(asOf, current, facts);
        var lastFiled = await FilingMoments.LastFiledAsync(
            dbContext, current.Select(profile => profile.UserId).ToList(), cancellationToken);

        var epas = coverage.Epas
            .GroupBy(epa => epa.EpaId)
            .Select(group => group.First())
            .OrderBy(epa => epa.EpaCode, StringComparer.Ordinal)
            .Select(epa => new EpaFilterOptionDto(epa.EpaId, epa.EpaCode, epa.EpaTitle, epa.QuotaPeriod, epa.Target))
            .ToList();
        var shortOn = filter.ShortOnEpaId is int shortOnId ? epas.FirstOrDefault(epa => epa.EpaId == shortOnId) : null;

        var everyone = current
            .Select(profile => Row(profile, facts, asOf, lastFiled, filter.ShortOnEpaId))
            .ToList();

        var match = everyone
            .Where(row => filter.ShortOnEpaId is null || row.ShortOn is not null)
            .Where(row => filter.TrainingYear is not int year || row.TrainingYear == year)
            .Where(row => !filter.NothingFiled || row.NothingFiled);

        var semester = AcademicPeriod.Containing(asOf);

        return new ProgrammeRosterRead(
            scope,
            QuotaText.SemesterName(semester),
            semester.NominalEnd,
            everyone.Count,
            everyone.Count(row => row.IsExempt),
            Order(match, filter, facts.Contacts).ToList(),
            epas,
            everyone.Select(row => row.TrainingYear).OfType<int>().Distinct().Order().ToList(),
            shortOn,
            coverage)
        {
            Filter = filter
        };
    }

    private static ProgrammeTraineeRowDto Row(
        TraineeProfile profile,
        CurriculumCoverageReader.CoverageFacts facts,
        DateOnly asOf,
        IReadOnlyDictionary<string, DateTime> lastFiled,
        int? shortOnEpaId)
    {
        var tallies = CurriculumCoverageReader.TalliesFor(profile, facts, asOf);

        var semester = tallies.Where(entry => entry.Item.QuotaPeriod == QuotaPeriod.Semester && entry.Tally.Applies).ToList();
        var year = tallies.Where(entry => entry.Item.QuotaPeriod != QuotaPeriod.Semester && entry.Tally.Applies).ToList();

        // D14 waives a target a registrar has; one with none at all is not exempt (as the coverage reader counts them).
        ProgrammeExemption? exemption = tallies.Count > 0 && semester.Count + year.Count == 0
            ? asOf < profile.ProgrammeStartDate ? ProgrammeExemption.NotStarted : ProgrammeExemption.StartedPartWay
            : null;

        var shortfalls = tallies
            .Where(entry => entry.Tally.Applies && !entry.Tally.IsMet)
            .Select(entry => Shortfall(entry.Item, entry.Tally))
            .ToList();

        // D11: largest shortfall first, then a semester target before a yearly one, then by code; three at most.
        var furthest = shortfalls
            .OrderByDescending(shortfall => shortfall.Short)
            .ThenBy(shortfall => shortfall.IsPerSemester ? 0 : 1)
            .ThenBy(shortfall => shortfall.EpaCode, StringComparer.Ordinal)
            .Take(FurthestShortCount)
            .ToList();

        var filedOn = lastFiled.TryGetValue(profile.UserId, out var filed) ? filed : (DateTime?)null;

        return new ProgrammeTraineeRowDto(
            profile.Id,
            profile.UserId,
            CurriculumCoverageReader.NameOf(facts.Contacts, profile.UserId) ?? profile.UserId,
            profile.GetStage(asOf),
            semester.Count(entry => entry.Tally.IsMet),
            semester.Count,
            year.Count(entry => entry.Tally.IsMet),
            year.Count,
            exemption,
            furthest,
            shortOnEpaId is int epaId ? shortfalls.FirstOrDefault(shortfall => shortfall.EpaId == epaId) : null,
            filedOn is { } on ? ProgrammeCalendar.DateOf(on) : null)
        {
            ProgrammeStartDate = profile.ProgrammeStartDate,
            AdmittedOn = profile.AdmittedOn,
            AcademicYear = AcademicPeriod.Containing(asOf).Year,
            NothingFiled = FilingMoments.NothingFiled(profile.AdmittedOn, filedOn, asOf)
        };
    }

    private static EpaShortfallDto Shortfall(CurriculumCoverageReader.CoverageItem item, QuotaWindowTally tally)
        => new(item.EpaId, item.EpaCode, item.QuotaPeriod, tally.Count, tally.Target)
        {
            WindowEnd = tally.Window.NominalEnd,
            AcademicYear = tally.Window.AcademicYear
        };

    /// <summary>The share of applying targets met; a registrar with none applying sorts as having met them all.</summary>
    private static double Share(ProgrammeTraineeRowDto row)
    {
        var applying = row.SemesterApplying + row.YearApplying;
        return applying == 0 ? 1d : (double)(row.SemesterMet + row.YearMet) / applying;
    }

    private static IEnumerable<ProgrammeTraineeRowDto> Order(
        IEnumerable<ProgrammeTraineeRowDto> rows,
        ProgrammeTraineesFilter filter,
        IReadOnlyDictionary<string, UserContact> contacts)
    {
        if (filter.ShortOnEpaId is not null)
        {
            // "Furthest from its target first, then fewest EPAs met, then by surname."
            return rows
                .OrderByDescending(row => row.ShortOn?.Short ?? 0)
                .ThenBy(Share)
                .ThenBySurname(row => row.TraineeUserId, contacts);
        }

        if (filter.NothingFiled)
        {
            // "Longest without first": never filed before filed long ago.
            return rows
                .OrderBy(row => row.LastFiledOn ?? DateOnly.MinValue)
                .ThenBySurname(row => row.TraineeUserId, contacts);
        }

        // "Fewest met first, then by surname", an exempt registrar last.
        return rows
            .OrderBy(row => row.IsExempt ? 1 : 0)
            .ThenBy(row => row.IsExempt ? 0d : Share(row))
            .ThenBySurname(row => row.TraineeUserId, contacts);
    }
}
