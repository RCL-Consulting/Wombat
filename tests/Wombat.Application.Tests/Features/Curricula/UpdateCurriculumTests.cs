using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Curricula;

/// <summary>
/// T394 (flow 16, Q19, round 3's decision 5): a curriculum version's sub-speciality is fixed once it holds an item or has an
/// adoption row, so <see cref="UpdateCurriculumCommand" /> refuses a changed one, naming the cause (its national items, or the
/// institutions that adopted it), before anything is staged; and its field refusals are the page's words (T324).
/// </summary>
public sealed class UpdateCurriculumTests : IAsyncLifetime
{
    private const int CollegeId = 5;
    private const int Paediatrics = 1000;
    private const int Neonatology = 1001;
    private const int Version = 3000;
    private const int Kgk = 10;
    private const int Other = 11;
    private const int Third = 12;

    private ApplicationDbContext _db = null!;

    public async Task InitializeAsync()
    {
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        _db.Institutions.AddRange(
            new Institution { Id = Kgk, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK" },
            new Institution { Id = Other, Name = "Other Hospital", ShortCode = "OH" },
            new Institution { Id = Third, Name = "Aardvark Hospital", ShortCode = "AH" });
        _db.Colleges.Add(new College { Id = CollegeId, Name = "College of Paediatricians of South Africa", ShortCode = "CPSA" });
        _db.Specialities.Add(new Speciality { Id = 100, CollegeId = CollegeId, Name = "Paediatrics" });
        _db.SubSpecialities.AddRange(
            new SubSpeciality { Id = Paediatrics, SpecialityId = 100, Name = "Paediatrics" },
            new SubSpeciality { Id = Neonatology, SpecialityId = 100, Name = "Neonatology" });
        _db.Epas.AddRange(
            new Epa { Id = 500, Code = "NEO-001", Title = "National", SubSpecialityId = Neonatology },
            new Epa { Id = 600, Code = "KGK-001", Title = "Own", SubSpecialityId = Neonatology, OwningInstitutionId = Kgk });
        _db.Curricula.Add(new Curriculum
        {
            Id = Version, SubSpecialityId = Neonatology, Name = "Neonatology EPA Curriculum", Version = "1.0",
            EffectiveFrom = new DateOnly(2026, 11, 2), IsActive = false
        });
        await _db.SaveChangesAsync();
    }

    public Task DisposeAsync()
    {
        _db.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task AnEmptyUnadoptedVersion_MayMoveToAnotherSubSpeciality()
    {
        var saved = await UpdateAsync(Paediatrics);

        saved.SubSpecialityId.Should().Be(Paediatrics);
        (await _db.Curricula.AsNoTracking().SingleAsync(entity => entity.Id == Version)).SubSpecialityId.Should().Be(Paediatrics);
    }

    [Fact]
    public async Task AVersionHoldingANationalItem_RefusesTheMove_ByItsItems_NothingStaged()
    {
        _db.CurriculumItems.Add(Item(9000, 500, owner: null));
        _db.InstitutionCurriculumAdoptions.Add(Adoption(1, Kgk, isActive: true));
        await _db.SaveChangesAsync();

        var refused = await RefusedAsync();

        refused.Cause.Should().Be(CurriculumSubSpecialityFixedCause.HoldsItems, "a national item is named first, before any adoption");
        refused.AdoptedBy.Should().BeEmpty();
        await NothingChangedAsync();
    }

    [Fact]
    public async Task AnAdoptedVersion_RefusesTheMove_NamingItsAdopter_NothingStaged()
    {
        _db.InstitutionCurriculumAdoptions.Add(Adoption(1, Kgk, isActive: true));
        await _db.SaveChangesAsync();

        var refused = await RefusedAsync();

        refused.Cause.Should().Be(CurriculumSubSpecialityFixedCause.Adopted);
        refused.AdoptedBy.Should().Equal("Kgosi Kgari Teaching Hospital");
        refused.Message.Should().NotContain("Kgosi", "the audit row carries the message; the names are the page's to say");
        await NothingChangedAsync();
    }

    // An own item implies its institution's adoption, so the adopter is the cause; a superseded adoption counts as one.
    [Fact]
    public async Task AnOwnItemAndSupersededAdoptions_NameEveryAdopter_OnceEach_Ordinally()
    {
        _db.CurriculumItems.Add(Item(9001, 600, owner: Kgk));
        _db.InstitutionCurriculumAdoptions.AddRange(
            Adoption(1, Kgk, isActive: false),
            Adoption(2, Kgk, isActive: true),
            Adoption(3, Other, isActive: false),
            Adoption(4, Third, isActive: true));
        await _db.SaveChangesAsync();

        var refused = await RefusedAsync();

        refused.Cause.Should().Be(CurriculumSubSpecialityFixedCause.Adopted);
        refused.AdoptedBy.Should().Equal("Aardvark Hospital", "Kgosi Kgari Teaching Hospital", "Other Hospital");
        await NothingChangedAsync();
    }

    [Fact]
    public async Task AFixedVersion_StillSaves_WhenItsSubSpecialityIsUnchanged()
    {
        _db.CurriculumItems.Add(Item(9000, 500, owner: null));
        await _db.SaveChangesAsync();

        var saved = await UpdateAsync(Neonatology, name: "Neonatal EPA Curriculum", isActive: true);

        saved.Name.Should().Be("Neonatal EPA Curriculum");
        saved.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "1.0", "Name", "Enter a name.")]
    [InlineData("Neonatology EPA Curriculum", " ", "Version", "Enter a version.")]
    public void TheValidator_RefusesAnEmptyField_InThePagesWords(string name, string version, string field, string message)
    {
        var result = new UpdateCurriculumCommandValidator().Validate(
            new UpdateCurriculumCommand(Version, Neonatology, name, version, new DateOnly(2026, 11, 2), null, false, TestPrincipals.CollegeAdmin(CollegeId)));

        result.Errors.Should().ContainSingle().Which.Should().Match<FluentValidation.Results.ValidationFailure>(
            failure => failure.PropertyName == field && failure.ErrorMessage == message);
    }

    [Fact]
    public void TheValidator_RefusesAnEndBeforeTheStart_UnderEffectiveTo()
    {
        var result = new UpdateCurriculumCommandValidator().Validate(
            new UpdateCurriculumCommand(Version, Neonatology, "N", "1.0", new DateOnly(2026, 11, 2), new DateOnly(2026, 10, 1), false, TestPrincipals.CollegeAdmin(CollegeId)));

        result.Errors.Should().ContainSingle().Which.ErrorMessage.Should().Be("Effective to must be on or after Effective from.");
        result.Errors[0].PropertyName.Should().Be(nameof(UpdateCurriculumCommand.EffectiveTo));
    }

    private Task<CurriculumDto> UpdateAsync(int subSpecialityId, string name = "Neonatology EPA Curriculum", bool isActive = false)
        => new UpdateCurriculumCommandHandler(_db).Handle(
            new UpdateCurriculumCommand(Version, subSpecialityId, name, "1.0", new DateOnly(2026, 11, 2), null, isActive, TestPrincipals.CollegeAdmin(CollegeId)),
            CancellationToken.None);

    private async Task<CurriculumSubSpecialityFixedException> RefusedAsync()
    {
        var act = () => new UpdateCurriculumCommandHandler(_db).Handle(
            new UpdateCurriculumCommand(Version, Paediatrics, "Renamed", "9.9", new DateOnly(2026, 11, 3), null, true, TestPrincipals.CollegeAdmin(CollegeId)),
            CancellationToken.None);
        return (await act.Should().ThrowAsync<CurriculumSubSpecialityFixedException>()).Which;
    }

    private async Task NothingChangedAsync()
    {
        _db.ChangeTracker.HasChanges().Should().BeFalse("the refusal comes before anything is assigned (T201)");
        var stored = await _db.Curricula.AsNoTracking().SingleAsync(entity => entity.Id == Version);
        stored.SubSpecialityId.Should().Be(Neonatology);
        stored.Name.Should().Be("Neonatology EPA Curriculum");
        stored.Version.Should().Be("1.0");
        stored.IsActive.Should().BeFalse();
    }

    private static CurriculumItem Item(int id, int epaId, int? owner) => new()
    {
        Id = id, CurriculumId = Version, EpaId = epaId, OwningInstitutionId = owner,
        RequiredCount = 1, QuotaPeriod = QuotaPeriod.AcademicYear, MinimumLevelOrder = 3, WindowMonths = 12
    };

    private static InstitutionCurriculumAdoption Adoption(int id, int institutionId, bool isActive) => new()
    {
        Id = id, InstitutionId = institutionId, CurriculumId = Version, SubSpecialityId = Neonatology,
        AdoptedOn = new DateOnly(2026, 11, 3), IsActive = isActive
    };
}
