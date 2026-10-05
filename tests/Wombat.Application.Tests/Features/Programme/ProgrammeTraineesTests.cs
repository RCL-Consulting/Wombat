using FluentAssertions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Programme.Trainees;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;
using static Wombat.Application.Tests.Features.Programme.ProgrammeCast;

namespace Wombat.Application.Tests.Features.Programme;

/// <summary>
/// T358 (flow 06, lane A2; Q1, C6, C7, E5; reviews 9, 13 and 16): Programme trainees, the current registrars in the scope
/// of the role the page reads as, each with My progress's figures, the EPAs furthest from target and the last filing.
/// The cast is Step 3.52's (<see cref="ProgrammeCast" />).
/// </summary>
public sealed class ProgrammeTraineesTests
{
    private static readonly string[] Paediatrics = [DuPlessis, Mahlangu, Ndlovu, Dlamini, Molefe];

    [Fact]
    public async Task TheCastAt352_FewestMetFirst_ThenBySurname_WithMyProgressFigures()
    {
        var (db, users) = Build();
        await using var _ = db;

        var result = await ListAsync(db, users, SpecialityAdmin(), WombatRoles.SpecialityAdmin);

        result!.Read.Rows.Select(row => row.TraineeUserId).Should().Equal(Paediatrics);
        result.Read.CurrentCount.Should().Be(5);
        result.Read.ExemptCount.Should().Be(0);
        result.MatchCount.Should().Be(5);
        result.Read.CurrentSemesterName.Should().Be("Semester 2, 2026");
        result.Read.CurrentSemesterEnd.Should().Be(new DateOnly(2026, 11, 30));

        var dlamini = Row(result, Dlamini);
        dlamini.Name.Should().Be("Anele Dlamini");
        dlamini.ProfileId.Should().Be(DlaminiProfile);
        dlamini.TrainingYear.Should().Be(3);
        (dlamini.SemesterMet, dlamini.SemesterApplying, dlamini.YearMet, dlamini.YearApplying).Should().Be((1, 5, 0, 1));
        dlamini.Exemption.Should().BeNull();
        dlamini.LastFiledOn.Should().BeNull();

        // Review 16: the furthest three by shortfall, then semester before yearly, then code (D11).
        dlamini.FurthestShort.Select(epa => epa.EpaCode).Should().Equal("PAED-002", "PAED-003", "PAED-005");
        Row(result, Mahlangu).FurthestShort.Select(epa => epa.EpaCode).Should().Equal("PAED-001", "PAED-003", "PAED-005");
        Row(result, Mahlangu).FurthestShort.Should().OnlyContain(epa =>
            epa.Count == 0 && epa.Target == 3 && epa.Short == 3 && epa.IsPerSemester &&
            epa.WindowEnd == new DateOnly(2026, 11, 30) && epa.AcademicYear == 2026);
        Row(result, DuPlessis).FurthestShort.Select(epa => epa.EpaCode).Should().Equal("PAED-001", "PAED-002", "PAED-003");
    }

    [Fact]
    public async Task ShortOnPaed002_FurthestFromItsTargetFirst_ThenFewestMet_ThenBySurname()
    {
        var (db, users) = Build();
        await using var _ = db;

        var result = await ListAsync(db, users, SpecialityAdmin(), WombatRoles.SpecialityAdmin, shortOn: Paed002);

        // Round-2 review 16: du Plessis, Dlamini, Molefe at 0 first (du Plessis has met nothing), then Mahlangu and Ndlovu.
        result!.Read.Rows.Select(row => row.TraineeUserId).Should().Equal(DuPlessis, Dlamini, Molefe, Mahlangu, Ndlovu);
        Row(result, Mahlangu).ShortOn.Should().BeEquivalentTo(new { EpaCode = "PAED-002", Count = 1, Target = 3, Short = 2 });
        Row(result, Ndlovu).ShortOn!.Count.Should().Be(1, "Sipho Ndlovu's Mini-CEX credits PAED-002 (R3)");
        Row(result, DuPlessis).ShortOn!.Short.Should().Be(3);
        result.Read.ShortOn.Should().BeEquivalentTo(new
        {
            EpaId = Paed002, EpaCode = "PAED-002", EpaTitle = "Managing common paediatric presentations",
            QuotaPeriod = QuotaPeriod.Semester, Target = 3
        });
        result.Read.CurrentCount.Should().Be(5, "the heading's \"5 of 5\" counts every current registrar");
    }

    [Fact]
    public async Task ARegistrarWhoHasMetTheEpa_IsNotShortOnIt()
    {
        var (db, users) = Build();
        await using var _ = db;

        var result = await ListAsync(db, users, SpecialityAdmin(), WombatRoles.SpecialityAdmin, shortOn: Paed001);

        result!.Read.Rows.Select(row => row.TraineeUserId).Should().Equal(DuPlessis, Mahlangu, Ndlovu);
    }

    [Fact]
    public async Task EveryoneAtNought_IsOrderedBySurname_ThenFirstName()
    {
        // Step 2.33 (T298, review 13): every figure 0, so the order is the surnames': Dlamini, du Plessis, Mahlangu,
        // Molefe, Ndlovu. Never the user id, a GUID nobody can follow.
        var db = NewDbWithCastButNoCredit(out var users);
        await using var _ = db;

        var result = await ListAsync(db, users, SpecialityAdmin(), WombatRoles.SpecialityAdmin);

        result!.Read.Rows.Select(row => row.Name).Should().Equal(
            "Anele Dlamini", "Pieter du Plessis", "Nomsa Mahlangu", "Lerato Molefe", "Sipho Ndlovu");
        result.Read.Coverage.Trainees.Select(trainee => trainee.Name).Should().Equal(
            "Anele Dlamini", "Pieter du Plessis", "Nomsa Mahlangu", "Lerato Molefe", "Sipho Ndlovu");
    }

    [Fact]
    public async Task AnExemptRegistrar_IsListedLast_WithWhy()
    {
        var (db, users) = Build();
        await using var _ = db;
        // D14: a start after the semester's first month waives this period's targets.
        AddRegistrar(db, users, 108, "late", "Zola", "Abrahams", 1, programmeStart: new DateOnly(2026, 8, 15));
        AddRegistrar(db, users, 109, "future", "Yusuf", "Adams", 1, programmeStart: new DateOnly(2027, 1, 15));
        db.SaveChanges();

        var result = await ListAsync(db, users, SpecialityAdmin(), WombatRoles.SpecialityAdmin);

        result!.Read.Rows.Select(row => row.TraineeUserId).Should().Equal([.. Paediatrics, "late", "future"]);
        Row(result, "late").Exemption.Should().Be(ProgrammeExemption.StartedPartWay);
        Row(result, "future").Exemption.Should().Be(ProgrammeExemption.NotStarted);
        Row(result, "future").ProgrammeStartDate.Should().Be(new DateOnly(2027, 1, 15));
        Row(result, "future").TrainingYear.Should().BeNull();
        Row(result, "late").FurthestShort.Should().BeEmpty("an exempt registrar owes nothing this period");
        result.Read.ExemptCount.Should().Be(2);
        result.Read.CurrentCount.Should().Be(7);
        result.Read.Coverage.ExemptTraineeCount.Should().Be(2);
    }

    [Fact]
    public async Task AnEnded_AnErased_AndALockedRegistrar_AreNotCurrent()
    {
        var (db, users) = Build();
        await using var _ = db;

        // Withdrawn (T252), erased (a pseudonym no account holds, T258), locked by an administrator (T268).
        db.TraineeProfiles.Find(DuPlessisProfile)!.Deactivate(new DateOnly(2026, 10, 2), D);
        db.TraineeProfiles.Find(NdlovuProfile)!.UserId = "erased-pseudonym";
        db.TraineeProfiles.Add(new TraineeProfile
        {
            Id = 110, UserId = "locked", InstitutionId = Kgk, CurriculumId = PaediatricsCurriculum,
            ProgrammeStartDate = StartFor(2), AdmittedOn = StartFor(2), ExpectedCompletionDate = StartFor(2).AddYears(4)
        });
        users.With(new UserIdentityDetails("locked", "locked@test", "Lindiwe", "Locked", Kgk, [], [], [WombatRoles.Trainee])
        {
            IsLockedOut = true, IsDeactivated = true
        });
        db.SaveChanges();

        var result = await ListAsync(db, users, SpecialityAdmin(), WombatRoles.SpecialityAdmin);

        result!.Read.Rows.Select(row => row.TraineeUserId).Should().Equal(Mahlangu, Dlamini, Molefe);
        result.Read.CurrentCount.Should().Be(3);
    }

    [Fact]
    public async Task EachActingRole_ReadsItsOwnScope_AndNeverAnotherInstitution()
    {
        var (db, users) = Build();
        await using var _ = db;

        // The institution for a Committee member and a Coordinator: the surgical registrar too, never Other Hospital's.
        var asMember = await ListAsync(db, users, CommitteeMember(), WombatRoles.CommitteeMember);
        var asCoordinator = await ListAsync(db, users, Coordinator(), WombatRoles.Coordinator);
        asMember!.Read.Rows.Select(row => row.TraineeUserId).Should().BeEquivalentTo([.. Paediatrics, Surgeon]);
        asCoordinator!.Read.Rows.Select(row => row.TraineeUserId).Should().BeEquivalentTo([.. Paediatrics, Surgeon]);
        asMember.Read.Scope.Name.Should().Be("Kgosi Kgari Teaching Hospital");

        // The speciality's, and the sub-speciality's: Paediatrics only.
        var asSubSpecialityAdmin = await ListAsync(db, users, SubSpecialityAdmin(), WombatRoles.SubSpecialityAdmin);
        asSubSpecialityAdmin!.Read.Rows.Select(row => row.TraineeUserId).Should().Equal(Paediatrics);
        asSubSpecialityAdmin.Read.Scope.Name.Should().Be("Paediatrics");

        // E4: a Speciality admin who also sits on the committee reads Paediatrics as Speciality admin.
        var twoHats = TestPrincipals.InRoles(
            [WombatRoles.SpecialityAdmin, WombatRoles.CommitteeMember], "mokoena", Kgk, specialityId: PaediatricsSpeciality);
        (await ListAsync(db, users, twoHats, WombatRoles.SpecialityAdmin))!.Read.Rows.Should().HaveCount(5);
        (await ListAsync(db, users, twoHats, WombatRoles.CommitteeMember))!.Read.Rows.Should().HaveCount(6);

        // The other hospital's committee member reads only its own.
        (await ListAsync(db, users, CommitteeMember("other", OtherHospital), WombatRoles.CommitteeMember))!
            .Read.Rows.Select(row => row.TraineeUserId).Should().Equal(Elsewhere);
    }

    [Fact]
    public async Task NoScope_IsNull()
    {
        var (db, users) = Build();
        await using var _ = db;

        (await ListAsync(db, users, CommitteeMember(), WombatRoles.Coordinator)).Should().BeNull("the role is not held");
        (await ListAsync(db, users, TestPrincipals.Trainee(Mahlangu, Kgk), WombatRoles.Trainee)).Should().BeNull();
        (await ListAsync(
                db,
                users,
                TestPrincipals.InRoles([WombatRoles.CommitteeMember, WombatRoles.Trainee], Ndlovu, Kgk),
                WombatRoles.CommitteeMember))
            .Should().BeNull("a registrar on the committee reads no peer's record (T185)");
    }

    [Fact]
    public async Task TheTrainingYearFilter_AndItsOptions()
    {
        var (db, users) = Build();
        await using var _ = db;

        var result = await ListAsync(db, users, SpecialityAdmin(), WombatRoles.SpecialityAdmin, year: 1);

        result!.Read.Rows.Select(row => row.TraineeUserId).Should().Equal(Mahlangu, Ndlovu);
        result.Read.TrainingYears.Should().Equal(1, 2, 3, 4);
        result.Read.CurrentCount.Should().Be(5);
        result.Read.Epas.Select(epa => epa.EpaCode).Should().Equal(
            "PAED-001", "PAED-002", "PAED-003", "PAED-004", "PAED-005", "PAED-008");
    }

    [Fact]
    public async Task NothingFiled_StartsAtAdmission_AndCountsWhatHasLeftDraft_LongestWithoutFirst()
    {
        var (db, users) = Build();
        await using var _ = db;
        // Admitted 10 days ago: not yet 30 days to file anything (E5, D1).
        db.TraineeProfiles.Find(NdlovuProfile)!.AdmittedOn = D.AddDays(-10);
        // Filed 40 days ago, and a draft this week, which is not a filing.
        AddActivity(db, LoggedType, Molefe, DAt10.AddDays(-40), "logged");
        AddActivity(db, DraftType, Molefe, DAt10.AddDays(-2), "draft");
        // Filed this week.
        AddActivity(db, LoggedType, Dlamini, DAt10.AddDays(-3), "logged");
        db.SaveChanges();

        var result = await ListAsync(db, users, Coordinator(), WombatRoles.Coordinator, filed: true);

        // Never filed first (by surname), then the longest since.
        result!.Read.Rows.Select(row => row.TraineeUserId).Should().Equal(DuPlessis, Mahlangu, Surgeon, Molefe);
        Row(result, Molefe).LastFiledOn.Should().Be(D.AddDays(-40));
        Row(result, Molefe).NothingFiled.Should().BeTrue();
        result.Read.CurrentCount.Should().Be(6);

        var everyone = await ListAsync(db, users, Coordinator(), WombatRoles.Coordinator);
        Row(everyone!, Dlamini).LastFiledOn.Should().Be(D.AddDays(-3));
        Row(everyone!, Dlamini).NothingFiled.Should().BeFalse();
        Row(everyone!, Ndlovu).NothingFiled.Should().BeFalse("admitted 10 days ago");
    }

    [Fact]
    public async Task SeveralFilters_AllApply()
    {
        var (db, users) = Build();
        await using var _ = db;

        var result = await ListAsync(db, users, SpecialityAdmin(), WombatRoles.SpecialityAdmin, shortOn: Paed001, year: 4, filed: true);

        result!.Read.Rows.Should().BeEmpty();
        result.MatchCount.Should().Be(0);
        result.Read.Filter.Should().Be(new ProgrammeTraineesFilter(Paed001, 4, true));
        result.Read.CurrentCount.Should().Be(5);
    }

    [Fact]
    public async Task APage_Of20()
    {
        var (db, users) = Build();
        await using var _ = db;
        for (var i = 0; i < 20; i++)
        {
            AddRegistrar(db, users, 200 + i, $"extra-{i:00}", "Extra", $"Registrar {i:00}", 2);
        }

        db.SaveChanges();

        var first = await ListAsync(db, users, SpecialityAdmin(), WombatRoles.SpecialityAdmin);
        var second = await ListAsync(db, users, SpecialityAdmin(), WombatRoles.SpecialityAdmin, page: 2);
        var beyond = await ListAsync(db, users, SpecialityAdmin(), WombatRoles.SpecialityAdmin, page: 9);

        first!.Page.Should().HaveCount(20);
        first.MatchCount.Should().Be(25);
        second!.Page.Should().HaveCount(5);
        second.PageNumber.Should().Be(2);
        beyond!.PageNumber.Should().Be(2, "a page past the last reads the last");
        first.Page.Concat(second.Page).Select(row => row.ProfileId).Should().Equal(first.Read.Rows.Select(row => row.ProfileId));
    }

    private static ProgrammeTraineeRowDto Row(ProgrammeTraineesDto result, string userId)
        => result.Read.Rows.Single(row => row.TraineeUserId == userId);

    private static Task<ProgrammeTraineesDto?> ListAsync(
        ApplicationDbContext db,
        IUserAdministrationService users,
        System.Security.Claims.ClaimsPrincipal principal,
        string actingRole,
        int? shortOn = null,
        int? year = null,
        bool filed = false,
        int page = 1)
        => new ListProgrammeTraineesQueryHandler(db, users, TimeProvider.System).Handle(
            new ListProgrammeTraineesQuery(principal, actingRole, shortOn, year, filed, page, AsOf: D),
            CancellationToken.None);

    private static ApplicationDbContext NewDbWithCastButNoCredit(out FakeUserDirectory users)
    {
        var (db, cast) = Build();
        db.CurriculumItemProgresses.RemoveRange(db.CurriculumItemProgresses);
        db.SaveChanges();
        users = cast;
        return db;
    }
}
