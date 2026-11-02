using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Adoptions;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Adoptions;

/// <summary>
/// T392 (flow 15, Q6, Q7, F2, S1, S3): Curricula's "Open for adoption" offers the open versions an institution has not
/// actively adopted, each measured against its active adoption of the discipline, with what the superseding dialog says.
/// </summary>
/// <remarks>
/// The world: Paediatrics holds 11.1 (2026-01-01), 11.2 (2026-10-30), a clone of 11.2 dated the same day (a later id) and
/// Neonatology's 1.0, held back; General Medicine holds IM Core. Kgosi Kgari (KGK) has adopted 11.1 and holds KGK-001 there
/// as its own item. Before T392 the picker listed every open version, the adopted one included, which Adopt then refused.
/// </remarks>
public sealed class GetAdoptableCurriculaTests
{
    private const int Kgk = 10;
    private const int Other = 11;
    private const int Paediatrics = 1000;
    private const int GeneralMedicine = 2000;
    private const int V111 = 3000;
    private const int V112 = 3001;
    private const int V112Clone = 3003;
    private const int HeldBack = 3002;
    private const int ImCore = 4000;
    private const int Paed001 = 500;
    private const int Kgk001 = 600;

    [Fact]
    public async Task TheActiveAdoption_IsLeftOut_AndAHeldBackVersionIsNotOffered()
    {
        await using var db = await SeededAsync();

        var rows = await ReadAsync(db, TestPrincipals.InstitutionalAdmin(Kgk));

        rows.Select(row => row.Id).Should().BeEquivalentTo([V112, V112Clone, ImCore],
            "11.1 is a row with its state, never an offer (Q7), and a held-back version is not open (T301)");
    }

    [Fact]
    public async Task EachRow_IsNewerOrOlder_ThanTheActiveAdoptionOfItsDiscipline_ByEffectiveFromThenId()
    {
        await using var db = await SeededAsync();
        await AdoptAsync(db, V112);

        var rows = await ReadAsync(db, TestPrincipals.InstitutionalAdmin(Kgk));

        var byId = rows.ToDictionary(row => row.Id);
        byId[V111].Relation.Should().Be(AdoptionRelation.Older, "going back stays adoptable (Q21)");
        byId[V112Clone].Relation.Should().Be(AdoptionRelation.Newer, "a clone dated the same day is the later by its id (F2)");
        byId[ImCore].Relation.Should().Be(AdoptionRelation.None, "KGK has adopted no General Medicine version");
        byId[ImCore].AdoptedVersion.Should().BeNull();
        byId[V111].AdoptedVersion.Should().Be("11.2");
        byId[V111].AdoptedName.Should().Be("Paediatric EPA Curriculum");
        byId[V111].AdoptedCurriculumId.Should().Be(V112);

        await AdoptAsync(db, V112Clone);
        (await ReadAsync(db, TestPrincipals.InstitutionalAdmin(Kgk))).Single(row => row.Id == V112).Relation
            .Should().Be(AdoptionRelation.Older, "with the clone adopted, the version of the same date and an earlier id is the older");
    }

    [Fact]
    public async Task TheDialog_CountsTheActiveRegistrarsOnTheAdoptedVersion_AndTheOwnItemsNotCarriedOver()
    {
        await using var db = await SeededAsync();
        AddProfile(db, 1, Kgk, V111, isActive: true);
        AddProfile(db, 2, Kgk, V111, isActive: true);
        AddProfile(db, 3, Kgk, V111, isActive: true);
        AddProfile(db, 4, Kgk, V111, isActive: false);
        AddProfile(db, 5, Other, V111, isActive: true);
        await db.SaveChangesAsync();

        var v112 = (await ReadAsync(db, TestPrincipals.InstitutionalAdmin(Kgk))).Single(row => row.Id == V112);

        v112.Relation.Should().Be(AdoptionRelation.Newer);
        v112.AdoptedRegistrarCount.Should().Be(3, "the active profiles of KGK on 11.1: not an ended one, nor another institution's");
        v112.OwnItemsNotCarried.Should().Equal(["KGK-001"]);
        v112.NationalItemCount.Should().Be(1, "the version's national items only");
        v112.EffectiveFrom.Should().Be(new DateOnly(2026, 10, 30));

        // Once 11.2 holds KGK-001 as KGK's own too, going back to 11.1 carries nothing away (R3-A08).
        db.CurriculumItems.Add(Item(9001, V112, Kgk001, Kgk));
        await db.SaveChangesAsync();
        await AdoptAsync(db, V112);
        var back = (await ReadAsync(db, TestPrincipals.InstitutionalAdmin(Kgk))).Single(row => row.Id == V111);
        back.OwnItemsNotCarried.Should().BeEmpty();
        back.AdoptedRegistrarCount.Should().Be(0, "nobody has been moved to 11.2");
    }

    [Fact]
    public async Task TheAdministrator_ReadsTheInstitutionSheChose_AndNothingWithoutOne()
    {
        await using var db = await SeededAsync();

        (await ReadAsync(db, TestPrincipals.Administrator())).Should().BeEmpty("there is no institution to measure against");
        (await ReadAsync(db, TestPrincipals.Administrator(), Kgk)).Select(row => row.Id)
            .Should().BeEquivalentTo([V112, V112Clone, ImCore]);
        (await ReadAsync(db, TestPrincipals.Administrator(), Other)).Select(row => row.Id)
            .Should().BeEquivalentTo([V111, V112, V112Clone, ImCore], "the other institution has adopted nothing");
    }

    [Fact]
    public async Task AScopedCaller_NamingAnotherInstitution_ReadsNothing_AndNoOtherRoleReadsAnything()
    {
        await using var db = await SeededAsync();

        (await ReadAsync(db, TestPrincipals.InstitutionalAdmin(Other), Kgk)).Should().BeEmpty(
            "another institution's id reads nothing, never that institution's (T056)");
        (await ReadAsync(db, TestPrincipals.InstitutionalAdmin(Kgk), Kgk)).Should().HaveCount(3);
        (await ReadAsync(db, TestPrincipals.CollegeAdmin(5))).Should().BeEmpty();
        (await ReadAsync(db, TestPrincipals.Coordinator(Kgk))).Should().BeEmpty();
    }

    [Fact]
    public async Task TheAdministrator_NamingNoInstitutionThatExists_ReadsNothing()
    {
        // N-6: as a scoped caller's other id does.
        await using var db = await SeededAsync();

        (await ReadAsync(db, TestPrincipals.Administrator(), 999_999)).Should().BeEmpty();
        (await ReadAsync(db, TestPrincipals.Administrator(), Kgk)).Should().NotBeEmpty("guard: the chosen institution reads");
    }

    private static Task<IReadOnlyList<AdoptableCurriculumDto>> ReadAsync(
        ApplicationDbContext db, System.Security.Claims.ClaimsPrincipal principal, int? institutionId = null)
        => new GetAdoptableCurriculaQueryHandler(db).Handle(new GetAdoptableCurriculaQuery(principal, institutionId), CancellationToken.None);

    private static async Task AdoptAsync(ApplicationDbContext db, int curriculumId)
    {
        // What the page would name as superseded: the active adoption of the version's discipline (C4).
        var subSpecialityId = await db.Curricula.Where(curriculum => curriculum.Id == curriculumId).Select(curriculum => curriculum.SubSpecialityId).SingleAsync();
        var expected = await db.InstitutionCurriculumAdoptions
            .Where(adoption => adoption.InstitutionId == Kgk && adoption.SubSpecialityId == subSpecialityId && adoption.IsActive)
            .Select(adoption => (int?)adoption.CurriculumId)
            .SingleOrDefaultAsync();
        await new AdoptCurriculumCommandHandler(db).Handle(
            new AdoptCurriculumCommand(Kgk, curriculumId, expected, TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);
    }

    private static async Task<ApplicationDbContext> SeededAsync()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        db.Institutions.AddRange(
            new Institution { Id = Kgk, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK" },
            new Institution { Id = Other, Name = "Other Hospital", ShortCode = "OH" });
        db.Colleges.AddRange(
            new College { Id = 5, Name = "College of Paediatricians of South Africa", ShortCode = "CPSA" },
            new College { Id = 6, Name = "Demo College", ShortCode = "DEMO" });
        db.Specialities.AddRange(
            new Speciality { Id = 100, CollegeId = 5, Name = "Paediatrics" },
            new Speciality { Id = 200, CollegeId = 6, Name = "General Medicine" });
        db.SubSpecialities.AddRange(
            new SubSpeciality { Id = Paediatrics, SpecialityId = 100, Name = "Paediatrics" },
            new SubSpeciality { Id = 1001, SpecialityId = 100, Name = "Neonatology" },
            new SubSpeciality { Id = GeneralMedicine, SpecialityId = 200, Name = "General Internal Medicine" });
        db.Epas.AddRange(
            new Epa { Id = Paed001, Code = "PAED-001", Title = "National", SubSpecialityId = Paediatrics },
            new Epa { Id = Kgk001, Code = "KGK-001", Title = "Outreach", SubSpecialityId = Paediatrics, OwningInstitutionId = Kgk });
        db.Curricula.AddRange(
            Curriculum(V111, Paediatrics, "Paediatric EPA Curriculum", "11.1", new DateOnly(2026, 1, 1)),
            Curriculum(V112, Paediatrics, "Paediatric EPA Curriculum", "11.2", new DateOnly(2026, 10, 30)),
            Curriculum(V112Clone, Paediatrics, "Paediatric EPA Curriculum", "11.2b", new DateOnly(2026, 10, 30)),
            Curriculum(HeldBack, 1001, "Neonatology EPA Curriculum", "1.0", new DateOnly(2026, 10, 30), isActive: false),
            Curriculum(ImCore, GeneralMedicine, "IM Core Curriculum", "2026.1", new DateOnly(2026, 1, 1)));
        db.CurriculumItems.AddRange(
            Item(9000, V111, Paed001, null),
            Item(9002, V111, Kgk001, Kgk),
            Item(9003, V112, Paed001, null));
        db.InstitutionCurriculumAdoptions.Add(new InstitutionCurriculumAdoption
        {
            Id = 1, InstitutionId = Kgk, CurriculumId = V111, SubSpecialityId = Paediatrics, AdoptedOn = new DateOnly(2026, 10, 30), IsActive = true
        });
        await db.SaveChangesAsync();
        return db;
    }

    private static Curriculum Curriculum(int id, int subSpecialityId, string name, string version, DateOnly effectiveFrom, bool isActive = true)
        => new() { Id = id, SubSpecialityId = subSpecialityId, Name = name, Version = version, EffectiveFrom = effectiveFrom, IsActive = isActive };

    private static CurriculumItem Item(int id, int curriculumId, int epaId, int? owner)
        => new()
        {
            Id = id, CurriculumId = curriculumId, EpaId = epaId, OwningInstitutionId = owner,
            RequiredCount = 1, QuotaPeriod = QuotaPeriod.AcademicYear, MinimumLevelOrder = 3, WindowMonths = 12
        };

    private static void AddProfile(ApplicationDbContext db, int id, int institutionId, int curriculumId, bool isActive)
        => db.TraineeProfiles.Add(new TraineeProfile
        {
            Id = id, UserId = $"trainee-{id}", InstitutionId = institutionId, CurriculumId = curriculumId,
            ProgrammeStartDate = new DateOnly(2026, 1, 15), ExpectedCompletionDate = new DateOnly(2030, 1, 15), IsActive = isActive
        });
}
