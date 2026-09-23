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
/// T130 — an administrator must be able to say which window a curriculum item's target is for, and every
/// path that writes or copies an item must carry that choice rather than fall back to the zero value.
/// </summary>
/// <remarks>
/// <see cref="QuotaPeriod.AcademicYear" /> is the zero value, so an omission anywhere on the admin path does
/// not fail — it silently turns "3 per semester" into "3 per academic year", halving what the College asks of
/// every trainee on that EPA. The critique found that shape waiting in the <c>ScaleId</c> precedent (a
/// defaulted trailing parameter the Update handler assigns unconditionally). These tests pin each half:
/// write, edit, re-save, validate, read, and clone.
/// </remarks>
public sealed class CurriculumItemQuotaPeriodTests
{
    private const int CurriculumId = 3000;
    private const int FirstEpaId = 5000;
    private const int SecondEpaId = 5001;

    [Fact]
    public async Task AddCurriculumItem_PersistsASemesterTarget()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);

            var result = await new AddCurriculumItemCommandHandler(dbContext).Handle(
                new AddCurriculumItemCommand(CurriculumId, FirstEpaId, 3, QuotaPeriod.Semester, 4, 12, null, null, null, TestPrincipals.Administrator()),
                CancellationToken.None);

            result.Items.Single().QuotaPeriod.Should().Be(QuotaPeriod.Semester);
        }

        // Read back through a fresh context: the DTO the handler returns is built from the tracked entity, so
        // it would echo the value even if nothing reached the store.
        await using var readContext = CreateDbContext(databaseName);
        var stored = await readContext.Set<CurriculumItem>().SingleAsync();
        stored.QuotaPeriod.Should().Be(QuotaPeriod.Semester);
        stored.RequiredCount.Should().Be(3, "the target is stored as the per-window figure, not multiplied into a total");
    }

    [Fact]
    public async Task UpdateCurriculumItem_ChangesASemesterTargetToAnAcademicYearTarget()
    {
        var databaseName = Guid.NewGuid().ToString();
        int itemId;
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
            var added = await new AddCurriculumItemCommandHandler(dbContext).Handle(
                new AddCurriculumItemCommand(CurriculumId, FirstEpaId, 3, QuotaPeriod.Semester, 4, 12, null, null, null, TestPrincipals.Administrator()),
                CancellationToken.None);
            itemId = added.Items.Single().Id;
        }

        await using (var dbContext = CreateDbContext(databaseName))
        {
            var updated = await new UpdateCurriculumItemCommandHandler(dbContext).Handle(
                new UpdateCurriculumItemCommand(CurriculumId, itemId, FirstEpaId, 6, QuotaPeriod.AcademicYear, 4, 12, null, null, null, TestPrincipals.Administrator()),
                CancellationToken.None);

            updated.Items.Single().QuotaPeriod.Should().Be(QuotaPeriod.AcademicYear);
        }

        await using var readContext = CreateDbContext(databaseName);
        var stored = await readContext.Set<CurriculumItem>().SingleAsync(item => item.Id == itemId);
        stored.QuotaPeriod.Should().Be(QuotaPeriod.AcademicYear, "an edit handler that never assigned the field would leave it at Semester");
        stored.RequiredCount.Should().Be(6);
    }

    [Fact]
    public async Task UpdateCurriculumItem_ThatReSendsTheValuesItsDtoReturned_KeepsASemesterTarget()
    {
        // This is what CurriculumItemsEdit does: load the curriculum, copy the item's DTO into the form, save
        // it unchanged. If the DTO dropped QuotaPeriod, or the command defaulted it, the form would load the
        // zero value and an administrator who only opened and saved PAED-001 would make it a yearly item.
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
            await new AddCurriculumItemCommandHandler(dbContext).Handle(
                new AddCurriculumItemCommand(CurriculumId, FirstEpaId, 3, QuotaPeriod.Semester, 4, 12, 1.5, """{"1":2,"2":3}""", null, TestPrincipals.Administrator()),
                CancellationToken.None);
        }

        CurriculumItemDto loaded;
        await using (var dbContext = CreateDbContext(databaseName))
        {
            var curriculum = await new GetCurriculumByIdQueryHandler(dbContext).Handle(
                new GetCurriculumByIdQuery(CurriculumId, TestPrincipals.Administrator()),
                CancellationToken.None);
            loaded = curriculum!.Items.Single();
        }

        await using (var dbContext = CreateDbContext(databaseName))
        {
            await new UpdateCurriculumItemCommandHandler(dbContext).Handle(
                new UpdateCurriculumItemCommand(
                    CurriculumId,
                    loaded.Id,
                    loaded.EpaId,
                    loaded.RequiredCount,
                    loaded.QuotaPeriod,
                    loaded.MinimumLevelOrder,
                    loaded.WindowMonths,
                    loaded.Weight,
                    loaded.MinimumLevelByStageJson,
                    loaded.PermittedToolKeys,
                    TestPrincipals.Administrator(),
                    loaded.ScaleId),
                CancellationToken.None);
        }

        await using var readContext = CreateDbContext(databaseName);
        var stored = await readContext.Set<CurriculumItem>().SingleAsync();
        stored.QuotaPeriod.Should().Be(QuotaPeriod.Semester);
        stored.RequiredCount.Should().Be(3);
        stored.MinimumLevelOrder.Should().Be(4);
        stored.WindowMonths.Should().Be(12);
        stored.Weight.Should().Be(1.5);
        stored.MinimumLevelByStageJson.Should().Be("""{"1":2,"2":3}""");
    }

    [Fact]
    public void AddCurriculumItemValidator_RefusesAQuotaPeriodTheEnumDoesNotDeclare()
    {
        // The read side tolerates an undeclared value (QuotaWindow reads anything but Semester as a year), so
        // the write side is the only place a stray integer from a form or an API call can be stopped.
        var command = new AddCurriculumItemCommand(CurriculumId, FirstEpaId, 3, (QuotaPeriod)99, 4, 12, null, null, null, TestPrincipals.Administrator());

        var result = new AddCurriculumItemCommandValidator().Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(nameof(AddCurriculumItemCommand.QuotaPeriod));
    }

    [Fact]
    public void UpdateCurriculumItemValidator_RefusesAQuotaPeriodTheEnumDoesNotDeclare()
    {
        var command = new UpdateCurriculumItemCommand(CurriculumId, 7000, FirstEpaId, 3, (QuotaPeriod)99, 4, 12, null, null, null, TestPrincipals.Administrator());

        var result = new UpdateCurriculumItemCommandValidator().Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(nameof(UpdateCurriculumItemCommand.QuotaPeriod));
    }

    [Theory]
    [InlineData(QuotaPeriod.Semester)]
    [InlineData(QuotaPeriod.AcademicYear)]
    public void CurriculumItemValidators_AcceptBothDeclaredQuotaPeriods(QuotaPeriod quotaPeriod)
    {
        // The control for the two refusals above: the same commands with a declared value are valid, so the
        // refusal is the enum rule and not some other rule these fixtures happen to trip.
        var add = new AddCurriculumItemCommand(CurriculumId, FirstEpaId, 3, quotaPeriod, 4, 12, null, null, null, TestPrincipals.Administrator());
        var update = new UpdateCurriculumItemCommand(CurriculumId, 7000, FirstEpaId, 3, quotaPeriod, 4, 12, null, null, null, TestPrincipals.Administrator());

        new AddCurriculumItemCommandValidator().Validate(add).IsValid.Should().BeTrue();
        new UpdateCurriculumItemCommandValidator().Validate(update).IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task GetCurriculaList_CarriesEachItemsQuotaPeriod()
    {
        // Two items with different periods, so a projection that hard-coded either value would fail on one.
        await using var dbContext = CreateDbContext(Guid.NewGuid().ToString());
        await SeedAsync(dbContext, withItems: true);

        var curricula = await new GetCurriculaListQueryHandler(dbContext).Handle(
            new GetCurriculaListQuery(TestPrincipals.Administrator()),
            CancellationToken.None);

        var items = curricula.Single().Items;
        items.Should().HaveCount(2);
        items.Single(item => item.EpaCode == "PAED-001").QuotaPeriod.Should().Be(QuotaPeriod.Semester);
        items.Single(item => item.EpaCode == "PAED-001").RequiredCount.Should().Be(3);
        items.Single(item => item.EpaCode == "PAED-008").QuotaPeriod.Should().Be(QuotaPeriod.AcademicYear);
        items.Single(item => item.EpaCode == "PAED-008").RequiredCount.Should().Be(1);
    }

    [Fact]
    public async Task GetCurriculumById_CarriesEachItemsQuotaPeriod()
    {
        await using var dbContext = CreateDbContext(Guid.NewGuid().ToString());
        await SeedAsync(dbContext, withItems: true);

        var curriculum = await new GetCurriculumByIdQueryHandler(dbContext).Handle(
            new GetCurriculumByIdQuery(CurriculumId, TestPrincipals.Administrator()),
            CancellationToken.None);

        curriculum.Should().NotBeNull();
        var items = curriculum!.Items;
        items.Should().HaveCount(2);
        items.Single(item => item.EpaCode == "PAED-001").QuotaPeriod.Should().Be(QuotaPeriod.Semester);
        items.Single(item => item.EpaCode == "PAED-001").RequiredCount.Should().Be(3);
        items.Single(item => item.EpaCode == "PAED-008").QuotaPeriod.Should().Be(QuotaPeriod.AcademicYear);
        items.Single(item => item.EpaCode == "PAED-008").RequiredCount.Should().Be(1);
    }

    [Fact]
    public async Task CloneCurriculumAsNewVersion_KeepsEachItemsQuotaPeriodInTheNewVersion()
    {
        // The domain clone is tested in CurriculumCloneTests; this is the path an administrator actually takes,
        // through the command handler and the store, so a mapping lost between the two would show here.
        var databaseName = Guid.NewGuid().ToString();
        int cloneId;
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext, withItems: true);

            var clone = await new CloneCurriculumAsNewVersionCommandHandler(dbContext).Handle(
                new CloneCurriculumAsNewVersionCommand(CurriculumId, "11.2", new DateOnly(2027, 1, 1), null, TestPrincipals.Administrator()),
                CancellationToken.None);

            cloneId = clone.Id;
            cloneId.Should().NotBe(CurriculumId);
            clone.Items.Single(item => item.EpaCode == "PAED-001").QuotaPeriod.Should().Be(QuotaPeriod.Semester);
        }

        await using var readContext = CreateDbContext(databaseName);
        var clonedItems = await readContext.Set<CurriculumItem>()
            .Where(item => item.CurriculumId == cloneId)
            .ToListAsync();

        clonedItems.Should().HaveCount(2);
        clonedItems.Single(item => item.EpaId == FirstEpaId).QuotaPeriod.Should().Be(QuotaPeriod.Semester);
        clonedItems.Single(item => item.EpaId == FirstEpaId).RequiredCount.Should().Be(3);
        clonedItems.Single(item => item.EpaId == SecondEpaId).QuotaPeriod.Should().Be(QuotaPeriod.AcademicYear);
        clonedItems.Single(item => item.EpaId == SecondEpaId).RequiredCount.Should().Be(1);
    }

    private static ApplicationDbContext CreateDbContext(string databaseName)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options);

    private static async Task SeedAsync(ApplicationDbContext dbContext, bool withItems = false)
    {
        var college = new College { Id = 1, Name = "CPSA", ShortCode = "CPSA" };
        var speciality = new Speciality { Id = 1, Name = "Paediatrics", CollegeId = 1, College = college };
        var subSpeciality = new SubSpeciality { Id = 1, Name = "Paediatrics", SpecialityId = 1, Speciality = speciality };

        dbContext.Colleges.Add(college);
        dbContext.Specialities.Add(speciality);
        dbContext.SubSpecialities.Add(subSpeciality);

        dbContext.Epas.AddRange(
            new Epa { Id = FirstEpaId, Code = "PAED-001", Title = "Providing paediatric emergency care to children", SubSpecialityId = 1 },
            new Epa { Id = SecondEpaId, Code = "PAED-008", Title = "Evaluating and managing neurodevelopmental presentations", SubSpecialityId = 1 });

        var curriculum = new Curriculum
        {
            Id = CurriculumId,
            SubSpecialityId = 1,
            SubSpeciality = subSpeciality,
            Name = "Paediatric EPA Curriculum",
            Version = "11.1",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            IsActive = true
        };

        if (withItems)
        {
            // Annexure B's figures for the two EPAs: PAED-001 is three per semester, PAED-008 one per annum.
            curriculum.Items.Add(new CurriculumItem
            {
                Id = 7000,
                EpaId = FirstEpaId,
                RequiredCount = 3,
                QuotaPeriod = QuotaPeriod.Semester,
                MinimumLevelOrder = 5,
                WindowMonths = 12
            });
            curriculum.Items.Add(new CurriculumItem
            {
                Id = 7001,
                EpaId = SecondEpaId,
                RequiredCount = 1,
                QuotaPeriod = QuotaPeriod.AcademicYear,
                MinimumLevelOrder = 5,
                WindowMonths = 12
            });
        }

        dbContext.Curricula.Add(curriculum);

        await dbContext.SaveChangesAsync();
    }
}
