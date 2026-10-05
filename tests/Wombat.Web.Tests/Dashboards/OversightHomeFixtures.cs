using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Dashboards.CommitteeMember;
using Wombat.Application.Features.Dashboards.Coordinator;
using Wombat.Application.Features.Dashboards.Oversight;
using Wombat.Application.Features.Dashboards.SpecialityAdmin;
using Wombat.Application.Features.Dashboards.SubSpecialityAdmin;
using Wombat.Application.Features.Programme;
using Wombat.Application.Features.Programme.Trainees;
using Wombat.Application.Features.Programme.Waiting;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Dashboards;

/// <summary>
/// The four oversight Homes' summaries as R2-Home draws them (T358, flow 06, lane B), at Step 3.52 and 3.30 (D =
/// 2026-10-04): the cast's registrars, the requests waiting for named assessors, the EPAs' coverage.
/// </summary>
internal static class OversightHomeFixtures
{
    public static readonly DateOnly D = new(2026, 10, 4);

    /// <summary>10:00 on D in South Africa.</summary>
    public static readonly DateTime DAt10 = new(2026, 10, 4, 8, 0, 0, DateTimeKind.Utc);

    public static readonly ProgrammeScopeDto Kgk = new(
        WombatRoles.Coordinator, ProgrammeScopeKind.Institution, "Kgosi Kgari Teaching Hospital", 10, [], []);

    public static readonly ProgrammeScopeDto Paediatrics = new(
        WombatRoles.SpecialityAdmin, ProgrammeScopeKind.Speciality, "Paediatrics", 10, [100], [1000]);

    // ---- registrars ----

    private static int _profileId = 100;

    public static ProgrammeTraineeRowDto Registrar(
        string name,
        int trainingYear,
        int semesterMet = 0,
        int yearMet = 0,
        ProgrammeExemption? exemption = null,
        DateOnly? lastFiled = null,
        int? profileId = null)
        => new(
            profileId ?? Interlocked.Increment(ref _profileId),
            name.ToLowerInvariant().Replace(' ', '-'),
            name,
            trainingYear,
            exemption is null ? semesterMet : 0,
            exemption is null ? 10 : 0,
            exemption is null ? yearMet : 0,
            exemption is null ? 5 : 0,
            exemption,
            [],
            null,
            lastFiled)
        {
            ProgrammeStartDate = new DateOnly(2026, 1, 15),
            AdmittedOn = new DateOnly(2026, 1, 15),
            AcademicYear = 2026
        };

    /// <summary>Step 3.52's five, fewest met first, then by surname (c1).</summary>
    public static IReadOnlyList<ProgrammeTraineeRowDto> Typical() =>
    [
        Registrar("Pieter du Plessis", 2, profileId: 101),
        Registrar("Nomsa Mahlangu", 1, profileId: 104),
        Registrar("Sipho Ndlovu", 1, profileId: 105),
        Registrar("Anele Dlamini", 3, semesterMet: 1, profileId: 102),
        Registrar("Lerato Molefe", 4, semesterMet: 1, profileId: 103)
    ];

    /// <summary>Step 2.33: everyone at nought, by surname (c4).</summary>
    public static IReadOnlyList<ProgrammeTraineeRowDto> AllZeros() =>
    [
        Registrar("Anele Dlamini", 3),
        Registrar("Pieter du Plessis", 2),
        Registrar("Nomsa Mahlangu", 1),
        Registrar("Lerato Molefe", 4),
        Registrar("Sipho Ndlovu", 1)
    ];

    public static RegistrarsCardDto Card(IReadOnlyList<ProgrammeTraineeRowDto> rows, int? total = null)
        => new(rows.Take(5).ToList(), total ?? rows.Count);

    // ---- Targets by EPA ----

    public static EpaTargetCoverage Epa(int id, string code, string title, int met, int of, QuotaPeriod period = QuotaPeriod.Semester, int target = 3, string? owner = null)
        => new(id, code, title, period, target, met, of, 0) { OwningInstitutionName = owner };

    /// <summary>c1: every EPA nought of five but PAED-001, two of five, which sorts last; PAED-008 is yearly.</summary>
    public static CurriculumCoverage Coverage(int exempt = 0, params EpaTargetCoverage[] epas)
        => new(D, "Semester 2, 2026", "July to November", [], epas.Length > 0 ? epas : TypicalEpas(), exempt);

    public static EpaTargetCoverage[] TypicalEpas() =>
    [
        Epa(2, "PAED-002", "Managing common paediatric presentations", 0, 5),
        Epa(8, "PAED-008", "Evaluating and managing neurodevelopmental and behavioural presentations in children", 0, 5, QuotaPeriod.AcademicYear, 1),
        Epa(1, "PAED-001", "Providing paediatric emergency care to children", 2, 5)
    ];

    public static CurriculumCoverage NoCoverage() => new(D, "Semester 2, 2026", "July to November", [], [], 0);

    // ---- Waiting for assessors ----

    public static ActivitySummaryDto Request(
        int id, string name, string epaCode, string observed, string from, string with, string stateLabel, int waitedDays, bool overdue)
        => ActivityRows.Waiting(
                id, typeName: name, subjectName: from, stateLabel: stateLabel, waitedDays: waitedDays, overdue: overdue,
                epaCode: epaCode, observedOn: DateOnly.Parse(observed, System.Globalization.CultureInfo.InvariantCulture))
            with
            {
                Holder = new ActivityHolderDto(ActivityHolderKind.Person, with.ToLowerInvariant().Replace(' ', '-'), with, false, DAt10),
                NomineeName = with
            };

    public static ActivitySummaryDto Portfolio() => Request(
        10, "Portfolio and Logbook Review (Paediatrics)", "PAED-015", "2026-10-03", "Pieter du Plessis", "Mohammed Patel",
        "Awaiting review", 8, overdue: true);

    public static ActivitySummaryDto MiniCex() => Request(
        21, "Mini-CEX (Paediatrics)", "PAED-004", "2026-10-01", "Nomsa Mahlangu", "Thandi Zulu", "Requested", 8, overdue: true);

    public static ActivitySummaryDto Cbd() => Request(
        30, "Case-Based Discussion (Paediatrics)", "PAED-002", "2026-09-29", "Pieter du Plessis", "Fatima Khumalo",
        "Requested", 0, overdue: false);

    public static WaitingForAssessorsDto Waiting(ProgrammeScopeDto scope, IReadOnlyList<ActivitySummaryDto> items, int? total = null, int? overdue = null)
    {
        var count = total ?? items.Count;
        var late = overdue ?? items.Count(item => item.IsOverdue);
        return new WaitingForAssessorsDto(scope, items.Take(5).ToList(), count, late, count, late, [], 7, 5, 1, 5, true);
    }

    // ---- the four summaries ----

    public static CommitteeMemberDashboardSummaryDto Committee(RegistrarsCardDto? card = null, CurriculumCoverage? coverage = null)
        => new(card ?? Card(Typical()), coverage ?? Coverage());

    public static SpecialityAdminDashboardSummaryDto SpecialityAdmin(
        WaitingForAssessorsDto? waiting = null, RegistrarsCardDto? card = null, CurriculumCoverage? coverage = null)
        => new(waiting ?? Waiting(Paediatrics, [Portfolio(), Cbd()]), card ?? Card(Typical()), coverage ?? Coverage());

    public static SubSpecialityAdminDashboardSummaryDto SubSpecialityAdmin(
        WaitingForAssessorsDto? waiting = null, RegistrarsCardDto? card = null, CurriculumCoverage? coverage = null)
        => new(waiting ?? Waiting(Paediatrics, [Portfolio(), Cbd()]), card ?? Card(Typical()), coverage ?? Coverage());

    public static CoordinatorDashboardSummaryDto Coordinator(
        WaitingForAssessorsDto? waiting = null,
        RegistrarsCardDto? nothingFiled = null,
        IReadOnlyList<ExpiringInvitationItem>? invitations = null)
        => new(
            waiting ?? Waiting(Kgk, [Portfolio(), MiniCex(), Cbd()]),
            nothingFiled ?? RegistrarsCardDto.Empty,
            invitations ?? []);

    public static ExpiringInvitationItem Invitation(string email = "expiring@kgk.wombat.local", string role = WombatRoles.Trainee, string label = "Trainee")
        => new(1, email, role, new DateOnly(2026, 10, 7)) { TargetRoleLabel = label };
}
