using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.CommitteeDecisions;

/// <summary>
/// Who decides an EPA (T131 slice 3, Decision 3): the one routing predicate, and the routing card's handler that asks it
/// of every panel at an institution. EPAs 4 and 5 go to a panel sitting as the neonatal committee where the institution
/// has one covering the trainee, and to the general panel where it has none; never to another institution's panel, nor
/// to a panel that covers another speciality.
/// </summary>
public sealed class DecisionRoutingTests
{
    private const int InstitutionA = 1;
    private const int InstitutionB = 2;
    private const int Paediatrics = 1;
    private const int Surgery = 2;
    private const int GeneralPaediatrics = 11;
    private const int GeneralSurgery = 21;
    private const int PaediatricsCurriculum = 100;
    private const int SurgeryCurriculum = 200;
    private const string Neonatal = "neonatal";

    private static readonly TraineeScope PaedsTraineeAtA = new(InstitutionA, Paediatrics, GeneralPaediatrics);
    private static readonly TraineeScope SurgeryTraineeAtA = new(InstitutionA, Surgery, GeneralSurgery);

    private readonly string _databaseName = Guid.NewGuid().ToString();

    // ─── The predicate ───────────────────────────────────────────────────────

    [Fact]
    public void Eligibility_IsTheTraineesInstitution_AndInstitutionWideOrTheTraineesSpeciality()
    {
        DecisionRouting.IsEligible(General(1, InstitutionA), PaedsTraineeAtA).Should().BeTrue();
        DecisionRouting.IsEligible(General(2, InstitutionB), PaedsTraineeAtA).Should().BeFalse("another institution's");
        DecisionRouting.IsEligible(General(3, InstitutionA, Paediatrics), PaedsTraineeAtA).Should().BeTrue();
        DecisionRouting.IsEligible(General(4, InstitutionA, Surgery), PaedsTraineeAtA).Should().BeFalse("another speciality's");
        DecisionRouting.IsEligible(General(5, InstitutionB, Paediatrics), PaedsTraineeAtA)
            .Should().BeFalse("a speciality id is national: it counts only at the trainee's institution");
        DecisionRouting.IsEligible(General(6, InstitutionA), null).Should().BeFalse("a trainee with no profile");
        DecisionRouting.IsEligible(General(7, InstitutionA, Paediatrics), new TraineeScope(InstitutionA, null, null))
            .Should().BeFalse("a speciality panel cannot cover a trainee whose speciality is unknown");
    }

    [Fact]
    public void ANeonatalItem_GoesToTheNeonatalPanel_WhereOneIsConfigured_AndNotToTheGeneralPanel()
    {
        var general = General(1, InstitutionA);
        var neonatal = Body(2, InstitutionA);
        var panels = new[] { general, neonatal };

        Routes(Neonatal, PaedsTraineeAtA, panels).Should().Equal(neonatal.Id);
        Routes(null, PaedsTraineeAtA, panels).Should().Equal(general.Id);
    }

    [Fact]
    public void ANeonatalItem_GoesToTheGeneralPanels_WhereNoneIsConfigured()
    {
        var general = General(1, InstitutionA);
        var paediatricsGeneral = General(2, InstitutionA, Paediatrics);
        var panels = new[] { general, paediatricsGeneral };

        Routes(Neonatal, PaedsTraineeAtA, panels).Should().Equal(general.Id, paediatricsGeneral.Id);
        DecisionRouting.BodyPanelFor(Neonatal, PaedsTraineeAtA, panels).Should().BeNull();
    }

    [Fact]
    public void ANeonatalItem_NeverGoesToAnotherInstitutionsOrAnotherSpecialitysNeonatalPanel()
    {
        var general = General(1, InstitutionA);
        var neonatalAtB = Body(2, InstitutionB);
        var neonatalForSurgeryAtA = Body(3, InstitutionA, Surgery);
        var panels = new[] { general, neonatalAtB, neonatalForSurgeryAtA };

        Routes(Neonatal, PaedsTraineeAtA, panels).Should().Equal(general.Id);
        DecisionRouting.RoutesTo(Neonatal, neonatalAtB, PaedsTraineeAtA, panels).Should().BeFalse();
        DecisionRouting.RoutesTo(Neonatal, neonatalForSurgeryAtA, PaedsTraineeAtA, panels).Should().BeFalse();
    }

    [Fact]
    public void APanelCoveringTheTraineesSpeciality_ComesBeforeAnInstitutionWideOne()
    {
        var general = General(1, InstitutionA);
        var institutionWideNeonatal = Body(2, InstitutionA);
        var paediatricNeonatal = Body(3, InstitutionA, Paediatrics);
        var panels = new[] { general, institutionWideNeonatal, paediatricNeonatal };

        Routes(Neonatal, PaedsTraineeAtA, panels).Should().Equal(paediatricNeonatal.Id);

        // A surgical trainee's EPA with the same tag is not the paediatric panel's: the institution-wide one takes it.
        Routes(Neonatal, SurgeryTraineeAtA, panels).Should().Equal(institutionWideNeonatal.Id);
    }

    [Fact]
    public void AnItemWithNoBody_NeverGoesToABodysPanel()
    {
        var general = General(1, InstitutionA);
        var neonatal = Body(2, InstitutionA);

        DecisionRouting.RoutesTo((string?)null, neonatal, PaedsTraineeAtA, [general, neonatal]).Should().BeFalse();
        DecisionRouting.RoutesTo(" ", neonatal, PaedsTraineeAtA, [general, neonatal]).Should().BeFalse("blank is no body");
    }

    [Fact]
    public void ThePanelAskedAbout_IsAlwaysACandidate_AndTheItemOverloadReadsTheItemsBody()
    {
        var neonatal = Body(2, InstitutionA);
        var item = new CurriculumItem { DecisionBodyKey = " Neonatal " };

        DecisionRouting.RoutesTo(item, neonatal, PaedsTraineeAtA, []).Should().BeTrue(
            "a caller that hands over too few body panels cannot make the body's own panel refuse its EPAs");
        DecisionRouting.RoutesTo(new CurriculumItem(), neonatal, PaedsTraineeAtA, []).Should().BeFalse();
    }

    // ─── The routing card's handler ──────────────────────────────────────────

    [Fact]
    public async Task EpasFourAndFive_RouteToTheGeneralPanel_WhereNoNeonatalPanelIsConfigured()
    {
        await using var db = await SeededDbAsync();
        AddPanel(db, 10, "A annual review", InstitutionA);
        AddPanel(db, 20, "B neonatal CCC", InstitutionB, bodyKey: Neonatal);
        AddPanel(db, 30, "A surgical neonatal", InstitutionA, Surgery, Neonatal);
        await db.SaveChangesAsync();

        var routing = await RoutingAsync(db, null, TestPrincipals.InstitutionalAdmin(InstitutionA));

        var paediatrics = Programme(routing, PaediatricsCurriculum);
        foreach (var code in new[] { "PAED-004", "PAED-005" })
        {
            var line = Line(paediatrics, code);
            line.Panels.Select(panel => panel.Name).Should().Equal(["A annual review"], code);
            line.FallsBackToGeneral.Should().BeTrue();
            line.DecisionBodyName.Should().Be("Neonatal team Clinical Competency Committee");
        }

        Line(paediatrics, "PAED-001").Panels.Select(panel => panel.Name).Should().Equal("A annual review");
        Line(paediatrics, "PAED-001").FallsBackToGeneral.Should().BeFalse();
    }

    [Fact]
    public async Task EpasFourAndFive_RouteToTheNeonatalPanel_WhereOneIsConfigured()
    {
        await using var db = await SeededDbAsync();
        AddPanel(db, 10, "A annual review", InstitutionA);
        AddPanel(db, 11, "A neonatal CCC", InstitutionA, bodyKey: Neonatal);
        AddPanel(db, 20, "B neonatal CCC", InstitutionB, bodyKey: Neonatal);
        await db.SaveChangesAsync();

        var routing = await RoutingAsync(db, null, TestPrincipals.InstitutionalAdmin(InstitutionA));

        var paediatrics = Programme(routing, PaediatricsCurriculum);
        foreach (var code in new[] { "PAED-004", "PAED-005" })
        {
            Line(paediatrics, code).Panels.Select(panel => panel.Name).Should().Equal(["A neonatal CCC"], code);
            Line(paediatrics, code).FallsBackToGeneral.Should().BeFalse();
        }

        Line(paediatrics, "PAED-001").Panels.Select(panel => panel.Name)
            .Should().Equal(["A annual review"], "the neonatal CCC decides only the EPAs the College gives it");

        // The surgical programme has no tagged EPA, and a surgery trainee's general panel is the institution-wide one.
        Line(Programme(routing, SurgeryCurriculum), "SURG-001").Panels.Select(panel => panel.Name)
            .Should().Equal("A annual review");
    }

    [Fact]
    public async Task APaediatricNeonatalPanel_ComesBeforeTheInstitutionWideOne_AndAnotherSpecialitysPanelTakesNothing()
    {
        await using var db = await SeededDbAsync();
        AddPanel(db, 10, "A annual review", InstitutionA);
        AddPanel(db, 11, "A neonatal CCC", InstitutionA, bodyKey: Neonatal);
        AddPanel(db, 12, "A paediatric neonatal CCC", InstitutionA, Paediatrics, Neonatal);
        AddPanel(db, 13, "A surgical review", InstitutionA, Surgery);
        await db.SaveChangesAsync();

        var paediatrics = Programme(
            await RoutingAsync(db, null, TestPrincipals.InstitutionalAdmin(InstitutionA)), PaediatricsCurriculum);

        Line(paediatrics, "PAED-004").Panels.Select(panel => panel.Name).Should().Equal("A paediatric neonatal CCC");
        Line(paediatrics, "PAED-001").Panels.Select(panel => panel.Name)
            .Should().Equal(["A annual review"], "a Surgery panel is not a paediatric trainee's committee");
    }

    [Fact]
    public async Task TheCard_ListsOnlyItemsInForce_AndNeverAnotherInstitutionsLocalItem()
    {
        await using var db = await SeededDbAsync();
        AddPanel(db, 10, "A annual review", InstitutionA);
        db.Epas.AddRange(
            new Epa { Id = 90, SubSpecialityId = GeneralPaediatrics, Code = "PAED-090", Title = "B's own", IsActive = true },
            new Epa { Id = 91, SubSpecialityId = GeneralPaediatrics, Code = "PAED-091", Title = "A's own", IsActive = true },
            new Epa { Id = 92, SubSpecialityId = GeneralPaediatrics, Code = "PAED-092", Title = "Retired", IsActive = false });
        db.CurriculumItems.AddRange(
            Item(1090, PaediatricsCurriculum, 90, owningInstitutionId: InstitutionB),
            Item(1091, PaediatricsCurriculum, 91, owningInstitutionId: InstitutionA),
            Item(1092, PaediatricsCurriculum, 92));
        await db.SaveChangesAsync();

        var codes = Programme(await RoutingAsync(db, null, TestPrincipals.Coordinator(InstitutionA)), PaediatricsCurriculum)
            .Lines.Select(line => line.EpaCode);

        codes.Should().Equal("PAED-001", "PAED-004", "PAED-005", "PAED-091");
    }

    [Fact]
    public async Task TheProgrammes_AreTheAdoptedCurricula_AndThoseTheInstitutionsTraineesFollow()
    {
        await using var db = await SeededDbAsync();
        db.Curricula.Add(new Curriculum { Id = 101, SubSpecialityId = GeneralPaediatrics, Name = "General Paediatrics", Version = "11.0" });
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1, UserId = "on-an-older-version", InstitutionId = InstitutionA, CurriculumId = 101, IsActive = true,
            ProgrammeStartDate = new DateOnly(2024, 1, 15), ExpectedCompletionDate = new DateOnly(2028, 1, 15)
        });
        await db.SaveChangesAsync();

        var atA = await RoutingAsync(db, null, TestPrincipals.InstitutionalAdmin(InstitutionA));
        var atB = await RoutingAsync(db, null, TestPrincipals.InstitutionalAdmin(InstitutionB));

        atA!.Programmes.Select(programme => programme.CurriculumId)
            .Should().BeEquivalentTo([PaediatricsCurriculum, 101, SurgeryCurriculum]);
        atB!.Programmes.Select(programme => programme.CurriculumId).Should().Equal(PaediatricsCurriculum);
    }

    // ─── Scope (the T056 family) ─────────────────────────────────────────────

    [Fact]
    public async Task InstitutionBsAdmin_GetsNullForAsRouting_AndTheirOwnWhenTheyNameNone()
    {
        await using var db = await SeededDbAsync();
        AddPanel(db, 10, "A annual review", InstitutionA);
        AddPanel(db, 20, "B annual review", InstitutionB);
        await db.SaveChangesAsync();
        var adminOfB = TestPrincipals.InstitutionalAdmin(InstitutionB);

        (await RoutingAsync(db, InstitutionA, adminOfB)).Should().BeNull();

        var own = await RoutingAsync(db, null, adminOfB);
        own!.InstitutionId.Should().Be(InstitutionB);
        own.Programmes.SelectMany(programme => programme.Lines).SelectMany(line => line.Panels)
            .Select(panel => panel.Name).Distinct().Should().Equal("B annual review");

        (await RoutingAsync(db, InstitutionB, adminOfB))!.InstitutionId.Should().Be(InstitutionB);
    }

    public static TheoryData<string> CommitteeRolesOfA => new()
    {
        WombatRoles.InstitutionalAdmin, WombatRoles.SpecialityAdmin, WombatRoles.SubSpecialityAdmin,
        WombatRoles.Coordinator, WombatRoles.CommitteeMember
    };

    [Theory]
    [MemberData(nameof(CommitteeRolesOfA))]
    public async Task EveryRoleThatWorksWithTheCommittee_ReadsItsOwnInstitution_AndNoOther(string role)
    {
        await using var db = await SeededDbAsync();
        var caller = TestPrincipals.InRole(role, "caller", InstitutionA, Paediatrics, GeneralPaediatrics);

        (await RoutingAsync(db, null, caller))!.InstitutionId.Should().Be(InstitutionA);
        (await RoutingAsync(db, InstitutionB, caller)).Should().BeNull();
    }

    [Fact]
    public async Task ATraineeOrAnAssessor_GetsNull_EvenForTheirOwnInstitution()
    {
        await using var db = await SeededDbAsync();

        (await RoutingAsync(db, null, TestPrincipals.Trainee("trainee", InstitutionA))).Should().BeNull();
        (await RoutingAsync(db, InstitutionA, TestPrincipals.InRole(WombatRoles.Assessor, "assessor", InstitutionA)))
            .Should().BeNull();
    }

    [Fact]
    public async Task AnAdministrator_MustNameTheInstitution_AndAnUnknownOneIsNull()
    {
        await using var db = await SeededDbAsync();
        var administrator = TestPrincipals.Administrator();

        var unnamed = () => RoutingAsync(db, null, administrator);
        await unnamed.Should().ThrowAsync<InvalidOperationException>().WithMessage("Choose the institution*");

        (await RoutingAsync(db, InstitutionB, administrator))!.InstitutionName.Should().Be("B");
        (await RoutingAsync(db, 999, administrator)).Should().BeNull();
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private static int[] Routes(string? bodyKey, TraineeScope trainee, IReadOnlyList<DecisionPanel> panels)
        => panels
            .Where(panel => DecisionRouting.RoutesTo(bodyKey, panel, trainee, panels))
            .Select(panel => panel.Id)
            .ToArray();

    private static DecisionPanel General(int id, int institutionId, int? specialityId = null)
        => new()
        {
            Id = id,
            Name = $"Panel {id}",
            InstitutionId = institutionId,
            Scope = specialityId is null ? DecisionPanelScope.Institution : DecisionPanelScope.Speciality,
            SpecialityId = specialityId
        };

    private static DecisionPanel Body(int id, int institutionId, int? specialityId = null)
    {
        var panel = General(id, institutionId, specialityId);
        panel.DecisionBodyKey = Neonatal;
        return panel;
    }

    private static CommitteeRoutingProgrammeDto Programme(CommitteeRoutingDto? routing, int curriculumId)
        => routing!.Programmes.Should().ContainSingle(programme => programme.CurriculumId == curriculumId).Which;

    private static CommitteeRoutingLineDto Line(CommitteeRoutingProgrammeDto programme, string code)
        => programme.Lines.Should().ContainSingle(line => line.EpaCode == code).Which;

    private static async Task<CommitteeRoutingDto?> RoutingAsync(
        ApplicationDbContext db, int? institutionId, ClaimsPrincipal principal)
        => await new GetCommitteeRoutingQueryHandler(db).Handle(
            new GetCommitteeRoutingQuery(institutionId, principal), CancellationToken.None);

    private static void AddPanel(
        ApplicationDbContext db, int id, string name, int institutionId, int? specialityId = null, string? bodyKey = null)
        => db.DecisionPanels.Add(new DecisionPanel
        {
            Id = id,
            Name = name,
            Scope = specialityId is null ? DecisionPanelScope.Institution : DecisionPanelScope.Speciality,
            InstitutionId = institutionId,
            SpecialityId = specialityId,
            DecisionBodyKey = bodyKey,
            CreatedOn = DateTime.UtcNow
        });

    private static CurriculumItem Item(
        int id, int curriculumId, int epaId, string? bodyKey = null, int? owningInstitutionId = null)
        => new()
        {
            Id = id,
            CurriculumId = curriculumId,
            EpaId = epaId,
            RequiredCount = 1,
            MinimumLevelOrder = 3,
            DecisionBodyKey = bodyKey,
            OwningInstitutionId = owningInstitutionId
        };

    private async Task<ApplicationDbContext> SeededDbAsync()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);

        db.Institutions.AddRange(
            new Institution { Id = InstitutionA, Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = InstitutionB, Name = "B", ShortCode = "B", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Specialities.AddRange(
            new Speciality { Id = Paediatrics, CollegeId = 1, Name = "Paediatrics", IsActive = true },
            new Speciality { Id = Surgery, CollegeId = 1, Name = "Surgery", IsActive = true });
        db.SubSpecialities.AddRange(
            new SubSpeciality { Id = GeneralPaediatrics, SpecialityId = Paediatrics, Name = "General Paediatrics", IsActive = true },
            new SubSpeciality { Id = GeneralSurgery, SpecialityId = Surgery, Name = "General Surgery", IsActive = true });
        db.Curricula.AddRange(
            new Curriculum { Id = PaediatricsCurriculum, SubSpecialityId = GeneralPaediatrics, Name = "General Paediatrics", Version = "11.1" },
            new Curriculum { Id = SurgeryCurriculum, SubSpecialityId = GeneralSurgery, Name = "General Surgery", Version = "1" });
        db.InstitutionCurriculumAdoptions.AddRange(
            new InstitutionCurriculumAdoption { Id = 1, InstitutionId = InstitutionA, CurriculumId = PaediatricsCurriculum, SubSpecialityId = GeneralPaediatrics },
            new InstitutionCurriculumAdoption { Id = 2, InstitutionId = InstitutionA, CurriculumId = SurgeryCurriculum, SubSpecialityId = GeneralSurgery },
            new InstitutionCurriculumAdoption { Id = 3, InstitutionId = InstitutionB, CurriculumId = PaediatricsCurriculum, SubSpecialityId = GeneralPaediatrics },
            new InstitutionCurriculumAdoption { Id = 4, InstitutionId = InstitutionB, CurriculumId = SurgeryCurriculum, SubSpecialityId = GeneralSurgery, IsActive = false });
        db.DecisionBodies.Add(new DecisionBody { Key = Neonatal, Name = "Neonatal team Clinical Competency Committee" });
        db.Epas.AddRange(
            new Epa { Id = 1, SubSpecialityId = GeneralPaediatrics, Code = "PAED-001", Title = "Ward round", IsActive = true },
            new Epa { Id = 4, SubSpecialityId = GeneralPaediatrics, Code = "PAED-004", Title = "Newborn", IsActive = true },
            new Epa { Id = 5, SubSpecialityId = GeneralPaediatrics, Code = "PAED-005", Title = "Sick newborn", IsActive = true },
            new Epa { Id = 50, SubSpecialityId = GeneralSurgery, Code = "SURG-001", Title = "Theatre list", IsActive = true });
        db.CurriculumItems.AddRange(
            Item(1001, PaediatricsCurriculum, 1),
            Item(1004, PaediatricsCurriculum, 4, Neonatal),
            Item(1005, PaediatricsCurriculum, 5, Neonatal),
            Item(2001, SurgeryCurriculum, 50));

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }
}
