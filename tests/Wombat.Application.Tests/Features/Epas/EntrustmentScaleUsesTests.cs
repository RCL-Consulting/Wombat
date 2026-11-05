using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Epas;

/// <summary>
/// T394 (flow 16, Q16, T395, build-contracts § 0): one read of what uses a scale, shared by the delete's refusal, the
/// update's reorder guard and the scale's page (<see cref="EntrustmentScaleDto.InUse" />). The delete's own sentences are
/// guarded by <c>EntrustmentScaleAdminHandlerTests</c>; this is the page's half.
/// </summary>
public sealed class EntrustmentScaleUsesTests
{
    [Fact]
    public async Task AScaleNothingUses_ReadsNotInUse()
    {
        await using var db = await SeededAsync();

        var scale = await new GetEntrustmentScaleByIdQueryHandler(db).Handle(new GetEntrustmentScaleByIdQuery(1), CancellationToken.None);

        scale!.InUse.Should().BeFalse();
        scale.Levels.Select(level => level.Label).Should().Equal("Observe only", "Direct supervision");
    }

    [Fact]
    public async Task AScaleAPinnedItemUses_ReadsInUse()
    {
        await using var db = await SeededAsync();
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 9000, CurriculumId = 3000, EpaId = 500, RequiredCount = 1, QuotaPeriod = QuotaPeriod.AcademicYear,
            MinimumLevelOrder = 2, WindowMonths = 12, ScaleId = 1
        });
        await db.SaveChangesAsync();

        (await new GetEntrustmentScaleByIdQueryHandler(db).Handle(new GetEntrustmentScaleByIdQuery(1), CancellationToken.None))!
            .InUse.Should().BeTrue();
    }

    // 6.7 then 6.8: Adult Neurology's default is a use, as the delete refuses it.
    [Fact]
    public async Task AScaleASubSpecialityDefaultsTo_ReadsInUse()
    {
        await using var db = await SeededAsync();
        db.SubSpecialities.Single().DefaultEntrustmentScaleId = 1;
        await db.SaveChangesAsync();

        (await new GetEntrustmentScaleByIdQueryHandler(db).Handle(new GetEntrustmentScaleByIdQuery(1), CancellationToken.None))!
            .InUse.Should().BeTrue();
    }

    [Fact]
    public async Task AnUnknownScale_ReadsNull()
    {
        await using var db = await SeededAsync();

        (await new GetEntrustmentScaleByIdQueryHandler(db).Handle(new GetEntrustmentScaleByIdQuery(99), CancellationToken.None))
            .Should().BeNull();
    }

    private static async Task<ApplicationDbContext> SeededAsync()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        db.Colleges.Add(new College { Id = 5, Name = "College of Neurologists of South Africa", ShortCode = "CNSA" });
        db.Specialities.Add(new Speciality { Id = 100, CollegeId = 5, Name = "Neurology" });
        db.SubSpecialities.Add(new SubSpeciality { Id = 1000, SpecialityId = 100, Name = "Adult Neurology" });
        db.Epas.Add(new Epa { Id = 500, Code = "NEU-001", Title = "National", SubSpecialityId = 1000 });
        db.Curricula.Add(new Curriculum { Id = 3000, SubSpecialityId = 1000, Name = "Neurology", Version = "1.0", EffectiveFrom = new DateOnly(2026, 1, 1) });
        db.EntrustmentScales.Add(new EntrustmentScale
        {
            Id = 1,
            Name = "CNSA Neurology Entrustment Scale",
            Levels =
            [
                new EntrustmentLevel { Id = 11, Order = 2, Label = "Direct supervision" },
                new EntrustmentLevel { Id = 10, Order = 1, Label = "Observe only" }
            ]
        });
        await db.SaveChangesAsync();
        return db;
    }
}
