using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Epas;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Epas;

public sealed class EpaCategoryHandlerTests
{
    [Fact]
    public async Task CreateEpa_DefaultsToCore()
    {
        await using var db = CreateDb();
        SeedSubSpeciality(db);

        var handler = new CreateEpaCommandHandler(db);
        var result = await handler.Handle(
            new CreateEpaCommand(1, "EPA-01", "Test EPA", null, null, TestPrincipals.Administrator()),
            CancellationToken.None);

        result.Category.Should().Be(EpaCategory.Core);
    }

    [Fact]
    public async Task CreateEpa_PersistsElectiveCategory()
    {
        await using var db = CreateDb();
        SeedSubSpeciality(db);

        var handler = new CreateEpaCommandHandler(db);
        var result = await handler.Handle(
            new CreateEpaCommand(1, "EPA-01", "Test EPA", null, null, EpaCategory.Elective, TestPrincipals.Administrator()),
            CancellationToken.None);

        result.Category.Should().Be(EpaCategory.Elective);
        var stored = await db.Set<Epa>().SingleAsync(e => e.Id == result.Id);
        stored.Category.Should().Be(EpaCategory.Elective);
    }

    [Fact]
    public async Task UpdateEpa_PersistsCategoryChange()
    {
        await using var db = CreateDb();
        SeedSubSpeciality(db);
        db.Epas.Add(new Epa { Id = 9, SubSpecialityId = 1, Code = "EPA-09", Title = "Nine", IsActive = true });
        await db.SaveChangesAsync();

        var handler = new UpdateEpaCommandHandler(db);
        var result = await handler.Handle(
            new UpdateEpaCommand(9, 1, "EPA-09", "Nine", null, null, EpaCategory.Elective, true, TestPrincipals.Administrator()),
            CancellationToken.None);

        result.Category.Should().Be(EpaCategory.Elective);
    }

    [Fact]
    public async Task AddCurriculumItem_PersistsStageOverrides()
    {
        await using var db = CreateDb();
        SeedCurriculum(db);

        var handler = new AddCurriculumItemCommandHandler(db);
        var result = await handler.Handle(
            new AddCurriculumItemCommand(
                CurriculumId: 1,
                EpaId: 7,
                RequiredCount: 6,
                MinimumLevelOrder: 5,
                WindowMonths: 36,
                Weight: null,
                MinimumLevelByStageJson: "{\"1\":2,\"2\":3,\"3\":4}",
                Principal: TestPrincipals.Administrator()),
            CancellationToken.None);

        var item = result.Items.Should().ContainSingle().Subject;
        item.MinimumLevelByStageJson.Should().Be("{\"1\":2,\"2\":3,\"3\":4}");
    }

    [Fact]
    public async Task AddCurriculumItem_RejectsInvalidStageOverridesJson()
    {
        var validator = new AddCurriculumItemCommandValidator();
        var command = new AddCurriculumItemCommand(1, 7, 6, 5, 36, null, "not json", TestPrincipals.Administrator());

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(failure => failure.PropertyName == "MinimumLevelByStageJson");
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static void SeedSubSpeciality(ApplicationDbContext db)
    {
        db.Institutions.Add(new Institution { Id = 1, Name = "Hospital" });
        db.Specialities.Add(new Speciality { Id = 1, InstitutionId = 1, Name = "Spec" });
        db.SubSpecialities.Add(new SubSpeciality { Id = 1, SpecialityId = 1, Name = "SubSpec" });
        db.SaveChanges();
    }

    private static void SeedCurriculum(ApplicationDbContext db)
    {
        SeedSubSpeciality(db);
        db.Epas.Add(new Epa { Id = 7, SubSpecialityId = 1, Code = "EPA-07", Title = "EPA 7", IsActive = true });
        db.Curricula.Add(new Wombat.Domain.Curricula.Curriculum
        {
            Id = 1,
            SubSpecialityId = 1,
            Name = "Programme",
            Version = "1.0",
            EffectiveFrom = new DateOnly(2020, 1, 1),
            IsActive = true
        });
        db.SaveChanges();
    }
}
