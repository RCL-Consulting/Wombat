using FluentAssertions;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Curricula;

/// <summary>
/// T394 (flow 16, T324, N1; R3-D02, D09a, D19): the create's and the clone's validators refuse in the page's words, by the
/// field each is under, and a clone starts held back with the national items only. The duplicate, which only PostgreSQL's
/// unique index can refuse, is <c>CurriculumDuplicatePostgresTests</c>'s.
/// </summary>
public sealed class CreateAndCloneCurriculumTests
{
    [Theory]
    [InlineData(0, "N", "1.0", "SubSpecialityId", "Choose a sub-speciality.")]
    [InlineData(2, "", "1.0", "Name", "Enter a name.")]
    [InlineData(2, "N", "", "Version", "Enter a version.")]
    public void TheCreate_RefusesAnEmptyField_InThePagesWords(int subSpecialityId, string name, string version, string field, string message)
    {
        var result = new CreateCurriculumCommandValidator().Validate(
            new CreateCurriculumCommand(subSpecialityId, name, version, new DateOnly(2026, 11, 2), null, TestPrincipals.Administrator()));

        result.Errors.Should().ContainSingle().Which.Should().Match<ValidationFailure>(
            failure => failure.PropertyName == field && failure.ErrorMessage == message);
    }

    [Fact]
    public void TheCreate_RefusesAnOverlongName_InN1sWords()
    {
        var result = new CreateCurriculumCommandValidator().Validate(
            new CreateCurriculumCommand(2, new string('a', 201), "1.0", new DateOnly(2026, 11, 2), null, TestPrincipals.Administrator()));

        result.Errors.Should().ContainSingle().Which.ErrorMessage.Should().Be("A name is at most 200 characters.");
    }

    [Fact]
    public void TheCreate_RefusesAnEndBeforeTheStart_UnderEffectiveTo()
    {
        var result = new CreateCurriculumCommandValidator().Validate(
            new CreateCurriculumCommand(2, "N", "1.0", new DateOnly(2026, 11, 2), new DateOnly(2026, 10, 1), TestPrincipals.Administrator()));

        result.Errors.Should().ContainSingle().Which.Should().Match<ValidationFailure>(failure =>
            failure.PropertyName == nameof(CreateCurriculumCommand.EffectiveTo)
            && failure.ErrorMessage == "Effective to must be on or after Effective from.");
    }

    [Fact]
    public void TheClone_RefusesAnEmptyNewVersion_InR3D09asWords()
    {
        var result = new CloneCurriculumAsNewVersionCommandValidator().Validate(
            new CloneCurriculumAsNewVersionCommand(3, " ", new DateOnly(2026, 11, 2), null, TestPrincipals.Administrator()));

        result.Errors.Should().ContainSingle().Which.Should().Match<ValidationFailure>(failure =>
            failure.PropertyName == nameof(CloneCurriculumAsNewVersionCommand.Version) && failure.ErrorMessage == "Enter the new version.");
    }

    [Fact]
    public async Task AClone_StartsHeldBack_WithTheNationalItemsOnly()
    {
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        db.Institutions.Add(new Institution { Id = 10, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK" });
        db.Colleges.Add(new College { Id = 5, Name = "CPSA", ShortCode = "CPSA" });
        db.Specialities.Add(new Speciality { Id = 100, CollegeId = 5, Name = "Paediatrics" });
        db.SubSpecialities.Add(new SubSpeciality { Id = 1000, SpecialityId = 100, Name = "Paediatrics" });
        db.Epas.AddRange(
            new Domain.Epas.Epa { Id = 500, Code = "PAED-001", Title = "National", SubSpecialityId = 1000 },
            new Domain.Epas.Epa { Id = 600, Code = "KGK-001", Title = "Own", SubSpecialityId = 1000, OwningInstitutionId = 10 });
        db.Curricula.Add(new Curriculum { Id = 3, SubSpecialityId = 1000, Name = "Paediatric EPA Curriculum", Version = "11.1", EffectiveFrom = new DateOnly(2026, 1, 1), IsActive = true });
        db.CurriculumItems.AddRange(
            new CurriculumItem { Id = 1, CurriculumId = 3, EpaId = 500, RequiredCount = 1, QuotaPeriod = QuotaPeriod.AcademicYear, MinimumLevelOrder = 3, WindowMonths = 12 },
            new CurriculumItem { Id = 2, CurriculumId = 3, EpaId = 600, OwningInstitutionId = 10, RequiredCount = 1, QuotaPeriod = QuotaPeriod.AcademicYear, MinimumLevelOrder = 3, WindowMonths = 12 });
        await db.SaveChangesAsync();

        var clone = await new CloneCurriculumAsNewVersionCommandHandler(db).Handle(
            new CloneCurriculumAsNewVersionCommand(3, "11.2", new DateOnly(2026, 11, 2), null, TestPrincipals.CollegeAdmin(5)),
            CancellationToken.None);

        clone.IsActive.Should().BeFalse();
        clone.Version.Should().Be("11.2");
        clone.Items.Select(item => item.EpaCode).Should().Equal("PAED-001");
    }
}
