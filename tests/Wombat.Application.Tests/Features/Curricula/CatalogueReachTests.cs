using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Epas;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Curricula;

/// <summary>
/// T394 (flow 16, Q2, S16, T8, build-contracts § 0): how far a College's change reaches, one rule for the curriculum's page,
/// the items, an item's page and an EPA's page; and the curriculum page's facts (S8, S9, Q19).
/// </summary>
/// <remarks>
/// The world, as the replay has it at 6.32: Kgosi Kgari (KGK) adopted 11.1, then 11.2, so 11.1 is superseded; three KGK
/// registrars are in training on 11.1, one has ended there, and one is on 11.2. Other Hospital adopted 11.1 and keeps it,
/// with one registrar. KGK-001 is KGK's own item on 11.1. Neonatology 1.0 is held back, empty, with no national EPA.
/// </remarks>
public sealed class CatalogueReachTests
{
    private const int Kgk = 10;
    private const int Other = 11;
    private const int CollegeId = 5;
    private const int V111 = 3000;
    private const int V112 = 3001;
    private const int Neo10 = 3002;
    private const int Paed001 = 500;
    private const int Paed016 = 516;
    private const int Kgk001 = 600;

    [Fact]
    public async Task AVersion_ReachesItsActiveAdopters_AndTheSupersedersWithRegistrarsStillOnIt()
    {
        await using var db = await SeededAsync();

        var reach = await CatalogueReach.ForCurriculaAsync(db, [V111, V112, Neo10], CancellationToken.None);

        reach[V111].AdoptedBy.Should().Equal("Other Hospital");
        reach[V111].RegistrarCount.Should().Be(1);
        reach[V111].SupersededBy.Should().Equal("Kgosi Kgari Teaching Hospital");
        reach[V111].SupersededRegistrarCount.Should().Be(3, "an ended programme's profile is inactive and is not counted");
        reach[V112].AdoptedBy.Should().Equal("Kgosi Kgari Teaching Hospital");
        reach[V112].RegistrarCount.Should().Be(1);
        reach[V112].SupersededBy.Should().BeEmpty();
        reach[Neo10].AdoptedBy.Should().BeEmpty();
        reach[Neo10].Reaches.Should().BeFalse();
    }

    // T8: an institution that superseded a version and has nobody left on it is not reached by a change to it.
    [Fact]
    public async Task ASuperseder_WithNoRegistrarLeftOnTheVersion_IsNotReached()
    {
        await using var db = await SeededAsync();
        foreach (var profile in db.TraineeProfiles.Where(profile => profile.InstitutionId == Kgk && profile.CurriculumId == V111))
        {
            profile.IsActive = false;
        }

        await db.SaveChangesAsync();

        var reach = await CatalogueReach.ForCurriculumAsync(db, V111, CancellationToken.None);

        reach.SupersededBy.Should().BeEmpty();
        reach.SupersededRegistrarCount.Should().Be(0);
        reach.AdoptedBy.Should().Equal("Other Hospital");
    }

    // S16: a national EPA's count sums its adopted versions' registrars, superseded adoptions included, each institution once.
    [Fact]
    public async Task ANationalEpa_SumsTheVersionsThatHoldIt_EachInstitutionOnce()
    {
        await using var db = await SeededAsync();

        var reach = await CatalogueReach.ForEpaAsync(db, Paed001, owningInstitutionId: null, CancellationToken.None);

        reach.CurriculumCount.Should().Be(2);
        reach.Institutions.Should().Equal("Kgosi Kgari Teaching Hospital", "Other Hospital");
        reach.RegistrarCount.Should().Be(5, "3 KGK and 1 Other on 11.1, 1 KGK on 11.2");
    }

    [Fact]
    public async Task ANationalEpa_OnNoAdoptedVersion_ReachesNobody()
    {
        await using var db = await SeededAsync();

        (await CatalogueReach.ForEpaAsync(db, Paed016, owningInstitutionId: null, CancellationToken.None))
            .CurriculumCount.Should().Be(0);
    }

    // An own item reaches its owner alone: Other Hospital adopts 11.1 too, and reads no item of KGK's.
    [Fact]
    public async Task AnOwnEpa_ReachesItsOwnerAlone_ThroughItsOwnItems()
    {
        await using var db = await SeededAsync();

        var reach = await CatalogueReach.ForEpaAsync(db, Kgk001, owningInstitutionId: Kgk, CancellationToken.None);

        reach.CurriculumCount.Should().Be(1);
        reach.Institutions.Should().Equal("Kgosi Kgari Teaching Hospital");
        reach.RegistrarCount.Should().Be(3);
    }

    [Fact]
    public async Task TheCurriculumsPage_IsToldItsReach_OnlyWhereTheCallerMayChangeIt()
    {
        await using var db = await SeededAsync();

        var college = await new GetCurriculumByIdQueryHandler(db)
            .Handle(new GetCurriculumByIdQuery(V111, TestPrincipals.CollegeAdmin(CollegeId)), CancellationToken.None);
        college!.Reach!.AdoptedBy.Should().Equal("Other Hospital");
        college.NationalItemCount.Should().Be(1, "the College's items alone, never KGK-001 (S9)");
        college.HasNationalEpas.Should().BeTrue();
        college.SubSpecialityFixed.Should().BeTrue();

        var institution = await new GetCurriculumByIdQueryHandler(db)
            .Handle(new GetCurriculumByIdQuery(V111, TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);
        institution!.Reach.Should().BeNull("the reach names other institutions");
        institution.SubSpecialityFixed.Should().BeFalse(
            "whether other institutions hold items on it or have adopted it is the College's to read (code N-6)");
    }

    // Q14 (S8): Neonatology has no national EPA, so its items page says so. Q19: an empty, unadopted version can still move.
    [Fact]
    public async Task AnEmptyVersion_InADisciplineWithNoNationalEpa_IsNotFixed_AndHasNoNationalEpas()
    {
        await using var db = await SeededAsync();

        var neonatology = await new GetCurriculumByIdQueryHandler(db)
            .Handle(new GetCurriculumByIdQuery(Neo10, TestPrincipals.CollegeAdmin(CollegeId)), CancellationToken.None);

        neonatology!.HasNationalEpas.Should().BeFalse();
        neonatology.SubSpecialityFixed.Should().BeFalse();
        neonatology.NationalItemCount.Should().Be(0);
        neonatology.Reach!.Reaches.Should().BeFalse();
    }

    // Q19: an adoption alone fixes the sub-speciality, as an item does.
    [Fact]
    public async Task AnAdoptionAlone_FixesTheSubSpeciality()
    {
        await using var db = await SeededAsync();
        db.InstitutionCurriculumAdoptions.Add(new InstitutionCurriculumAdoption
        {
            Id = 9, InstitutionId = Other, CurriculumId = Neo10, SubSpecialityId = 1001, AdoptedOn = new DateOnly(2026, 11, 2), IsActive = true
        });
        await db.SaveChangesAsync();

        var neonatology = await new GetCurriculumByIdQueryHandler(db)
            .Handle(new GetCurriculumByIdQuery(Neo10, TestPrincipals.CollegeAdmin(CollegeId)), CancellationToken.None);

        neonatology!.SubSpecialityFixed.Should().BeTrue();
    }

    [Fact]
    public async Task AnEpasPage_IsToldItsReach_WhereTheCallerMayChangeIt()
    {
        await using var db = await SeededAsync();

        var page = await new GetEpaPageQueryHandler(db)
            .Handle(new GetEpaPageQuery(Paed001, TestPrincipals.CollegeAdmin(CollegeId)), CancellationToken.None);

        page.Epa!.MayChange.Should().BeTrue();
        page.Epa.Reach!.RegistrarCount.Should().Be(5);
    }

    private static async Task<ApplicationDbContext> SeededAsync()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        db.Institutions.AddRange(
            new Institution { Id = Kgk, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK" },
            new Institution { Id = Other, Name = "Other Hospital", ShortCode = "OH" });
        db.Colleges.Add(new College { Id = CollegeId, Name = "College of Paediatricians of South Africa", ShortCode = "CPSA" });
        db.Specialities.Add(new Speciality { Id = 100, CollegeId = CollegeId, Name = "Paediatrics" });
        db.SubSpecialities.AddRange(
            new SubSpeciality { Id = 1000, SpecialityId = 100, Name = "Paediatrics" },
            new SubSpeciality { Id = 1001, SpecialityId = 100, Name = "Neonatology" });
        db.Epas.AddRange(
            new Epa { Id = Paed001, Code = "PAED-001", Title = "National", SubSpecialityId = 1000 },
            new Epa { Id = Paed016, Code = "PAED-016", Title = "Transitioning", SubSpecialityId = 1000 },
            new Epa { Id = Kgk001, Code = "KGK-001", Title = "Outreach", SubSpecialityId = 1000, OwningInstitutionId = Kgk });
        db.Curricula.AddRange(
            new Curriculum { Id = V111, SubSpecialityId = 1000, Name = "Paediatric EPA Curriculum", Version = "11.1", EffectiveFrom = new DateOnly(2026, 1, 1) },
            new Curriculum { Id = V112, SubSpecialityId = 1000, Name = "Paediatric EPA Curriculum", Version = "11.2", EffectiveFrom = new DateOnly(2026, 11, 2) },
            new Curriculum { Id = Neo10, SubSpecialityId = 1001, Name = "Neonatology EPA Curriculum", Version = "1.0", EffectiveFrom = new DateOnly(2026, 11, 2), IsActive = false });
        db.CurriculumItems.AddRange(
            Item(9000, V111, Paed001, null),
            Item(9001, V111, Kgk001, Kgk),
            Item(9002, V112, Paed001, null));
        db.InstitutionCurriculumAdoptions.AddRange(
            Adoption(1, Kgk, V111, isActive: false),
            Adoption(2, Kgk, V112, isActive: true),
            Adoption(3, Other, V111, isActive: true));
        db.TraineeProfiles.AddRange(
            Profile(1, Kgk, V111, isActive: true),
            Profile(2, Kgk, V111, isActive: true),
            Profile(3, Kgk, V111, isActive: true),
            Profile(4, Kgk, V111, isActive: false),
            Profile(5, Kgk, V112, isActive: true),
            Profile(6, Other, V111, isActive: true));
        await db.SaveChangesAsync();
        return db;
    }

    private static InstitutionCurriculumAdoption Adoption(int id, int institutionId, int curriculumId, bool isActive)
        => new() { Id = id, InstitutionId = institutionId, CurriculumId = curriculumId, SubSpecialityId = 1000, AdoptedOn = new DateOnly(2026, 1, 10), IsActive = isActive };

    private static CurriculumItem Item(int id, int curriculumId, int epaId, int? owner)
        => new()
        {
            Id = id, CurriculumId = curriculumId, EpaId = epaId, OwningInstitutionId = owner,
            RequiredCount = 1, QuotaPeriod = QuotaPeriod.AcademicYear, MinimumLevelOrder = 3, WindowMonths = 12
        };

    private static TraineeProfile Profile(int id, int institutionId, int curriculumId, bool isActive)
        => new()
        {
            Id = id, UserId = $"trainee-{id}", InstitutionId = institutionId, CurriculumId = curriculumId,
            ProgrammeStartDate = new DateOnly(2026, 1, 15), ExpectedCompletionDate = new DateOnly(2030, 1, 15), IsActive = isActive
        };
}
