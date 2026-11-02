using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Curricula;

/// <summary>
/// T392 (flow 15, Q9, Q10, Q18, F6, S1, S3): each curriculum row carries its state for one institution, what Curricula and
/// the items page say of it ("Adopted on ‹date›", or "Superseded on ‹date›" over "‹N› registrars still on it"), and whether
/// the items page offers an add (<see cref="CurriculumDto.MayAddItem" />).
/// </summary>
/// <remarks>
/// The world: Kgosi Kgari (KGK) adopted 11.1 on 2026-01-10 and 11.2 on 2026-10-30, so 11.1 is superseded. KGK-001 is KGK's
/// own item on 11.1. Two of KGK's registrars are active on 11.1 and one has ended there; another institution's registrar is
/// on 11.1 too.
/// </remarks>
public sealed class CurriculumInstitutionStateTests
{
    private const int Kgk = 10;
    private const int Other = 11;
    private const int CollegeId = 5;
    private const int V111 = 3000;
    private const int V112 = 3001;
    private const int Paed001 = 500;
    private const int Kgk001 = 600;

    private static readonly DateOnly January = new(2026, 1, 10);
    private static readonly DateOnly October = new(2026, 10, 30);

    [Fact]
    public async Task TheActiveVersion_ReadsAdoptedOn_AndTheSupersededOne_SupersededOnByTheNextAdoption()
    {
        await using var db = await SeededAsync();

        var rows = await ListAsync(db, TestPrincipals.InstitutionalAdmin(Kgk));

        var active = rows.Single(row => row.Id == V112).Institution!;
        active.IsActiveAdoption.Should().BeTrue();
        active.AdoptedOn.Should().Be(October);
        active.SupersededOn.Should().BeNull();
        active.InstitutionId.Should().Be(Kgk);
        active.InstitutionName.Should().Be("Kgosi Kgari Teaching Hospital");

        var superseded = rows.Single(row => row.Id == V111).Institution!;
        superseded.IsActiveAdoption.Should().BeFalse();
        superseded.AdoptedOn.Should().Be(January);
        superseded.SupersededOn.Should().Be(October);
        superseded.SupersededByName.Should().Be("Paediatric EPA Curriculum");
        superseded.SupersededByVersion.Should().Be("11.2");
    }

    // S1: going back adds a second adoption of 11.1; all three may be dated the same day, so the order is (AdoptedOn, Id).
    [Fact]
    public async Task AdoptionsOnOneDay_AreOrderedByTheirId()
    {
        await using var db = await SeededAsync(sameDay: true);
        db.InstitutionCurriculumAdoptions.Single(adoption => adoption.Id == 2).IsActive = false;
        db.InstitutionCurriculumAdoptions.Add(Adoption(3, V111, October, isActive: true));
        // A registrar admitted to 11.2 before the institution went back keeps 11.2 on its list (Q9).
        db.TraineeProfiles.Add(Profile(5, Kgk, V112, isActive: true));
        await db.SaveChangesAsync();

        var rows = await ListAsync(db, TestPrincipals.InstitutionalAdmin(Kgk));

        rows.Single(row => row.Id == V111).Institution!.IsActiveAdoption.Should().BeTrue();
        var v112 = rows.Single(row => row.Id == V112).Institution!;
        v112.IsActiveAdoption.Should().BeFalse();
        v112.SupersededOn.Should().Be(October);
        v112.SupersededByVersion.Should().Be("11.1", "the adoption after 11.2's, by its id, went back to 11.1");
    }

    [Fact]
    public async Task TheRegistrarCount_IsTheInstitutionsActiveProfilesOnTheVersion()
    {
        await using var db = await SeededAsync();

        var rows = await ListAsync(db, TestPrincipals.InstitutionalAdmin(Kgk));

        rows.Single(row => row.Id == V111).Institution!.RegistrarCount.Should().Be(2,
            "an ended profile and another institution's registrar are not still on it for KGK");
        rows.Single(row => row.Id == V112).Institution!.RegistrarCount.Should().Be(0);
    }

    [Fact]
    public async Task TheItemCounts_AndTheOwnItemsOnTheDisciplinesOtherVersions()
    {
        await using var db = await SeededAsync();

        var rows = await ListAsync(db, TestPrincipals.InstitutionalAdmin(Kgk));

        var v111 = rows.Single(row => row.Id == V111).Institution!;
        v111.NationalItemCount.Should().Be(1);
        v111.OwnItemCount.Should().Be(1);
        v111.OwnItemsElsewhere.Should().BeEmpty();

        var v112 = rows.Single(row => row.Id == V112).Institution!;
        v112.NationalItemCount.Should().Be(1);
        v112.OwnItemCount.Should().Be(0);
        v112.OwnItemsElsewhere.Should().Equal([new OwnItemsOnVersionDto(V111, "11.1", 1)],
            "R3-B07: \"A new version carries no institution's own items. Your item on 11.1 stays there.\"");
    }

    [Fact]
    public async Task MayAddItem_IsTheCollegesOwn_OrTheInstitutionsOnItsActiveAdoption()
    {
        await using var db = await SeededAsync();

        (await ReadAsync(db, V112, TestPrincipals.InstitutionalAdmin(Kgk)))!.MayAddItem.Should().BeTrue();
        (await ReadAsync(db, V111, TestPrincipals.InstitutionalAdmin(Kgk)))!.MayAddItem.Should().BeFalse(
            "a superseded version keeps its own items for the registrars on it, and takes no new one (F6)");
        (await ReadAsync(db, V111, TestPrincipals.CollegeAdmin(CollegeId)))!.MayAddItem.Should().BeTrue();
        (await ReadAsync(db, V111, TestPrincipals.Administrator()))!.MayAddItem.Should().BeTrue();

        (await ListAsync(db, TestPrincipals.InstitutionalAdmin(Kgk))).Single(row => row.Id == V112).MayAddItem.Should().BeTrue();
    }

    [Fact]
    public async Task TheRead_CarriesTheState_ForTheInstitutionalAdminOnly()
    {
        await using var db = await SeededAsync();

        (await ReadAsync(db, V111, TestPrincipals.InstitutionalAdmin(Kgk)))!.Institution!.SupersededOn.Should().Be(October);
        (await ReadAsync(db, V111, TestPrincipals.CollegeAdmin(CollegeId)))!.Institution.Should().BeNull();
        (await ReadAsync(db, V111, TestPrincipals.Administrator()))!.Institution.Should().BeNull(
            "her items page is every institution's (P1)");
    }

    [Fact]
    public async Task TheAdministrator_ReadsAnInstitutionsCurricula_WithItsState_OnlyWhenSheNamesIt()
    {
        await using var db = await SeededAsync();

        var all = await ListAsync(db, TestPrincipals.Administrator());
        all.Select(row => row.Id).Should().BeEquivalentTo([V111, V112]);
        all.Should().OnlyContain(row => row.Institution == null, "All curricula is no institution's");

        var kgk = await ListAsync(db, TestPrincipals.Administrator(), Kgk);
        kgk.Select(row => row.Id).Should().BeEquivalentTo([V111, V112]);
        kgk.Single(row => row.Id == V111).Institution!.RegistrarCount.Should().Be(2);
        kgk.Single(row => row.Id == V111).Institution!.InstitutionName.Should().Be("Kgosi Kgari Teaching Hospital");

        (await ListAsync(db, TestPrincipals.Administrator(), Other)).Should().BeEmpty("the other institution has adopted nothing");
    }

    [Fact]
    public async Task AScopedCaller_NamingAnotherInstitution_ReadsNothing()
    {
        await using var db = await SeededAsync();

        (await ListAsync(db, TestPrincipals.InstitutionalAdmin(Other), Kgk)).Should().BeEmpty(
            "another institution's id reads nothing, never that institution's (T056)");
        (await ListAsync(db, TestPrincipals.InstitutionalAdmin(Kgk), Kgk)).Should().HaveCount(2);
        (await ListAsync(db, TestPrincipals.CollegeAdmin(CollegeId), Kgk)).Should().BeEmpty();
        (await ListAsync(db, TestPrincipals.CollegeAdmin(CollegeId))).Should().OnlyContain(row => row.Institution == null);
    }

    private static Task<IReadOnlyList<CurriculumDto>> ListAsync(ApplicationDbContext db, ClaimsPrincipal principal, int? institutionId = null)
        => new GetCurriculaListQueryHandler(db).Handle(new GetCurriculaListQuery(principal, institutionId), CancellationToken.None);

    private static Task<CurriculumDto?> ReadAsync(ApplicationDbContext db, int curriculumId, ClaimsPrincipal principal)
        => new GetCurriculumByIdQueryHandler(db).Handle(new GetCurriculumByIdQuery(curriculumId, principal), CancellationToken.None);

    private static async Task<ApplicationDbContext> SeededAsync(bool sameDay = false)
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        db.Institutions.AddRange(
            new Institution { Id = Kgk, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK" },
            new Institution { Id = Other, Name = "Other Hospital", ShortCode = "OH" });
        db.Colleges.Add(new College { Id = CollegeId, Name = "College of Paediatricians of South Africa", ShortCode = "CPSA" });
        db.Specialities.Add(new Speciality { Id = 100, CollegeId = CollegeId, Name = "Paediatrics" });
        db.SubSpecialities.Add(new SubSpeciality { Id = 1000, SpecialityId = 100, Name = "Paediatrics" });
        db.Epas.AddRange(
            new Epa { Id = Paed001, Code = "PAED-001", Title = "National", SubSpecialityId = 1000 },
            new Epa { Id = Kgk001, Code = "KGK-001", Title = "Outreach", SubSpecialityId = 1000, OwningInstitutionId = Kgk });
        db.Curricula.AddRange(
            new Curriculum { Id = V111, SubSpecialityId = 1000, Name = "Paediatric EPA Curriculum", Version = "11.1", EffectiveFrom = new DateOnly(2026, 1, 1) },
            new Curriculum { Id = V112, SubSpecialityId = 1000, Name = "Paediatric EPA Curriculum", Version = "11.2", EffectiveFrom = October });
        db.CurriculumItems.AddRange(
            Item(9000, V111, Paed001, null),
            Item(9001, V111, Kgk001, Kgk),
            Item(9002, V112, Paed001, null));
        db.InstitutionCurriculumAdoptions.AddRange(
            Adoption(1, V111, sameDay ? October : January, isActive: false),
            Adoption(2, V112, October, isActive: true));
        db.TraineeProfiles.AddRange(
            Profile(1, Kgk, V111, isActive: true),
            Profile(2, Kgk, V111, isActive: true),
            Profile(3, Kgk, V111, isActive: false),
            Profile(4, Other, V111, isActive: true));
        await db.SaveChangesAsync();
        return db;
    }

    private static InstitutionCurriculumAdoption Adoption(int id, int curriculumId, DateOnly adoptedOn, bool isActive)
        => new() { Id = id, InstitutionId = Kgk, CurriculumId = curriculumId, SubSpecialityId = 1000, AdoptedOn = adoptedOn, IsActive = isActive };

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
