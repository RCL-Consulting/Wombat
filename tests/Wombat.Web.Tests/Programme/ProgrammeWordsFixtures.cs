using System.Reflection;
using System.Text.RegularExpressions;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Programme;
using Wombat.Application.Features.Programme.Trainees;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;

namespace Wombat.Web.Tests.Programme;

/// <summary>The cast's rows and scopes at Step 3.52 (D = 2026-10-04), for flow 06's words (T358, lane A2).</summary>
internal static partial class ProgrammeWordsFixtures
{
    public static readonly DateOnly D = new(2026, 10, 4);
    public static readonly DateOnly SemesterEnd = new(2026, 11, 30);

    public static readonly ProgrammeScopeDto Kgk = new(
        WombatRoles.CommitteeMember, ProgrammeScopeKind.Institution, "Kgosi Kgari Teaching Hospital", 10, [], []);

    public static readonly ProgrammeScopeDto Paediatrics = new(
        WombatRoles.SubSpecialityAdmin, ProgrammeScopeKind.SubSpeciality, "Paediatrics", 10, [], [1000]);

    public static EpaShortfallDto Short(string code, int count, int target, QuotaPeriod period = QuotaPeriod.Semester)
        => new(int.Parse(code[^3..]), code, period, count, target) { WindowEnd = SemesterEnd, AcademicYear = 2026 };

    public static ProgrammeTraineeRowDto Row(
        string name,
        int? trainingYear = 1,
        int semesterMet = 0,
        int semesterApplying = 10,
        int yearMet = 0,
        int yearApplying = 5,
        ProgrammeExemption? exemption = null,
        IReadOnlyList<EpaShortfallDto>? furthest = null,
        EpaShortfallDto? shortOn = null,
        DateOnly? lastFiled = null,
        DateOnly? programmeStart = null)
        => new(7, name.ToLowerInvariant(), name, trainingYear, semesterMet, semesterApplying, yearMet, yearApplying, exemption,
            furthest ?? [], shortOn, lastFiled)
        {
            ProgrammeStartDate = programmeStart ?? new DateOnly(2026, 1, 15),
            AcademicYear = 2026
        };

    public static ProgrammeRosterRead Read(int current = 5, int exempt = 0, EpaFilterOptionDto? shortOn = null, ProgrammeTraineesFilter? filter = null)
        => new(Kgk, "Semester 2, 2026", SemesterEnd, current, exempt, [], [], [], shortOn, Coverage())
        {
            Filter = filter ?? new ProgrammeTraineesFilter()
        };

    public static readonly EpaFilterOptionDto Paed002 =
        new(2, "PAED-002", "Managing common paediatric presentations", QuotaPeriod.Semester, 3);

    public static readonly EpaFilterOptionDto Paed001 =
        new(1, "PAED-001", "Providing paediatric emergency care to children", QuotaPeriod.Semester, 3);

    public static CurriculumCoverage Coverage(int exempt = 0, params EpaTargetCoverage[] epas)
        => new(D, "Semester 2, 2026", "July to November", [], epas, exempt);

    /// <summary>Every public constant string a words class declares.</summary>
    public static IEnumerable<string> Constants(Type words)
        => words.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && field.FieldType == typeof(string))
            .Select(field => (string)field.GetRawConstantValue()!);

    /// <summary>
    /// A pronoun that names a person (round-3-check 1: the person's name, "the registrar" or "the assessor"), or the second
    /// person a registrar's own pages use and a member of staff's must not (review 8).
    /// </summary>
    public static bool NamesAPersonByPronoun(string text) => PersonalPronoun().IsMatch(text);

    [GeneratedRegex(@"\b(she|her|hers|herself|he|him|his|himself|you|your|yours)\b", RegexOptions.IgnoreCase)]
    private static partial Regex PersonalPronoun();
}
