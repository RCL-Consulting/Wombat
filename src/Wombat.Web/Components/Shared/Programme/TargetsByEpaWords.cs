using System.Globalization;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Curricula;

namespace Wombat.Web.Components.Shared.Programme;

/// <summary>
/// The words of Home's "Targets by EPA" card (T358, flow 06, lane A2; Q5, review 13): each EPA's programme figure, the one
/// exemption wording, the rule line, the cadence line and the link's hidden tail. Read from
/// <see cref="CurriculumCoverage" />, whose EPAs come fewest registrars met first, then by code.
/// </summary>
/// <remarks>
/// The programme's figure is "2 of 5" over "registrars met this semester" or "registrars met in 2026", never a percentage
/// (Spec § Dashboard layout grid). One exemption wording, "1 registrar exempt this period, not counted".
/// </remarks>
public static class TargetsByEpaWords
{
    /// <summary>The card's title on every Home (round 3: one title).</summary>
    public const string Title = "Targets by EPA";

    /// <summary>
    /// An EPA's figure and its caption: ("2 of 5", "registrars met this semester") or ("0 of 5", "registrars met in 2026");
    /// when every registrar holding it is exempt, ("All exempt", "this period").
    /// </summary>
    public static (string Value, string Label) Figure(EpaTargetCoverage epa, CurriculumCoverage coverage)
    {
        ArgumentNullException.ThrowIfNull(epa);
        ArgumentNullException.ThrowIfNull(coverage);

        if (epa.TraineesApplying == 0)
        {
            return ("All exempt", "this period");
        }

        return ($"{Count(epa.TraineesMet)} of {Count(epa.TraineesApplying)}", MetCaption(epa, coverage));
    }

    /// <summary>
    /// The rule line (c1, c6): "Fewest registrars met first. Semester 2, 2026 ends on 2026-11-30.", with "1 registrar exempt
    /// this period, not counted. " before it when any is.
    /// </summary>
    public static string Rule(CurriculumCoverage coverage)
    {
        ArgumentNullException.ThrowIfNull(coverage);

        var semester = AcademicPeriod.Containing(coverage.AsOf);
        var exempt = coverage.ExemptTraineeCount > 0
            ? $"{ProgrammeWords.ExemptCount(coverage.ExemptTraineeCount)} exempt this period, not counted. "
            : string.Empty;
        return $"{exempt}Fewest registrars met first. {coverage.CurrentSemesterName} ends on {QuotaText.Iso(semester.NominalEnd)}.";
    }

    /// <summary>
    /// The cadence line under an EPA's name: "3 per semester", "1 per academic year", then " · Kgosi Kgari Teaching
    /// Hospital's own" for a local extra and " · every registrar has met it" when everyone holding it has.
    /// </summary>
    public static string Cadence(EpaTargetCoverage epa)
    {
        ArgumentNullException.ThrowIfNull(epa);

        var own = epa.OwningInstitutionName is { Length: > 0 } owner ? $" · {owner}'s own" : string.Empty;
        var all = IsMetByAll(epa) ? " · every registrar has met it" : string.Empty;
        return $"{QuotaText.TargetPhrase(epa.QuotaPeriod, epa.Target)}{own}{all}";
    }

    /// <summary>
    /// Every registrar holding the EPA has met it: its name is then text, not a link, since a list of the registrars short
    /// on it would be empty (round 3 item 33).
    /// </summary>
    public static bool IsMetByAll(EpaTargetCoverage epa)
    {
        ArgumentNullException.ThrowIfNull(epa);
        return epa.TraineesApplying > 0 && epa.TraineesMet == epa.TraineesApplying;
    }

    /// <summary>
    /// The link's visually hidden tail after the EPA's name: ": 2 of 5 registrars met this semester. Show the registrars
    /// short on it.".
    /// </summary>
    public static string LinkTail(EpaTargetCoverage epa, CurriculumCoverage coverage)
    {
        ArgumentNullException.ThrowIfNull(epa);
        ArgumentNullException.ThrowIfNull(coverage);

        var figure = epa.TraineesApplying == 0
            ? "all exempt this period"
            : $"{Count(epa.TraineesMet)} of {Count(epa.TraineesApplying)} {MetCaption(epa, coverage)}";
        return $": {figure}. Show the registrars short on it.";
    }

    /// <summary>An EPA's name as the row shows it: "PAED-002 — Managing common paediatric presentations".</summary>
    public static string Name(EpaTargetCoverage epa)
    {
        ArgumentNullException.ThrowIfNull(epa);
        return $"{epa.EpaCode} — {epa.EpaTitle}";
    }

    /// <summary>The card with no current registrar (c7). It has no foot.</summary>
    public const string Empty = "No targets this period: there is no current registrar.";

    private static string MetCaption(EpaTargetCoverage epa, CurriculumCoverage coverage)
        => epa.IsPerSemester
            ? "registrars met this semester"
            : $"registrars met in {AcademicPeriod.Containing(coverage.AsOf).Year.ToString(CultureInfo.InvariantCulture)}";

    private static string Count(int value) => value.ToString(CultureInfo.InvariantCulture);
}
