using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Curricula;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Curricula;

/// <summary>
/// T109 — an administrator must be able to pin a curriculum item's minima to an entrustment ladder, and
/// must be stopped from pinning one that cannot express them.
/// </summary>
/// <remarks>
/// The pin is the half of T109 that makes the engine reachable. Without it the only curricula with a
/// ladder are the two the seeders author, and an operator-built curriculum — which is what the defect was
/// raised against — can never be corrected through the product.
/// </remarks>
public sealed class CurriculumItemScalePinTests
{
    private const int FiveRungScaleId = 900;
    private const int SixRungScaleId = 901;

    [Fact]
    public async Task AddCurriculumItem_PinsTheScale()
    {
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext);

        var result = await new AddCurriculumItemCommandHandler(dbContext).Handle(
            new AddCurriculumItemCommand(3000, 5000, 3, QuotaPeriod.AcademicYear, 4, 12, null, null, null, Administrator(), FiveRungScaleId),
            CancellationToken.None);

        var item = result.Items.Single();
        item.ScaleId.Should().Be(FiveRungScaleId);
        item.ScaleName.Should().Be("Paed General Entrustment Scale");
    }

    [Fact]
    public async Task AddCurriculumItem_WithNoScale_LeavesItUnpinned()
    {
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext);

        var result = await new AddCurriculumItemCommandHandler(dbContext).Handle(
            new AddCurriculumItemCommand(3000, 5000, 3, QuotaPeriod.AcademicYear, 4, 12, null, null, null, Administrator()),
            CancellationToken.None);

        result.Items.Single().ScaleId.Should().BeNull("unpinned is the safe default, not an error state");
    }

    [Fact]
    public async Task UpdateCurriculumItem_CanPinAndUnpin()
    {
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext);

        var added = await new AddCurriculumItemCommandHandler(dbContext).Handle(
            new AddCurriculumItemCommand(3000, 5000, 3, QuotaPeriod.AcademicYear, 4, 12, null, null, null, Administrator()),
            CancellationToken.None);
        var itemId = added.Items.Single().Id;

        var pinned = await new UpdateCurriculumItemCommandHandler(dbContext).Handle(
            new UpdateCurriculumItemCommand(3000, itemId, 5000, 3, QuotaPeriod.AcademicYear, 4, 12, null, null, null, Administrator(), SixRungScaleId),
            CancellationToken.None);
        pinned.Items.Single().ScaleId.Should().Be(SixRungScaleId);

        var unpinned = await new UpdateCurriculumItemCommandHandler(dbContext).Handle(
            new UpdateCurriculumItemCommand(3000, itemId, 5000, 3, QuotaPeriod.AcademicYear, 4, 12, null, null, null, Administrator()),
            CancellationToken.None);
        unpinned.Items.Single().ScaleId.Should().BeNull("an administrator must be able to withdraw a pin they are unsure of");
    }

    [Fact]
    public async Task AddCurriculumItem_WhenTheScaleCannotExpressTheMinimum_IsRefused()
    {
        // The five-rung ladder has no rung 6, so it cannot be what a minimum of 6 was written against.
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext);

        var act = () => new AddCurriculumItemCommandHandler(dbContext).Handle(
            new AddCurriculumItemCommand(3000, 5000, 3, QuotaPeriod.AcademicYear, 6, 12, null, null, null, Administrator(), FiveRungScaleId),
            CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*requires level 6*");
    }

    [Fact]
    public async Task AddCurriculumItem_WhenAStageOverrideExceedsTheScale_IsRefused()
    {
        // The flat minimum fits, but a per-stage override does not — and the stage map is where the real
        // progression curve lives, so checking only the flat value would miss most of the risk.
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext);

        var act = () => new AddCurriculumItemCommandHandler(dbContext).Handle(
            new AddCurriculumItemCommand(3000, 5000, 3, QuotaPeriod.AcademicYear, 4, 12, null, """{"1":2,"4":6}""", null, Administrator(), FiveRungScaleId),
            CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*requires level 6*");
    }

    [Fact]
    public async Task AddCurriculumItem_WhenTheScaleDoesNotExist_IsRefused()
    {
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext);

        var act = () => new AddCurriculumItemCommandHandler(dbContext).Handle(
            new AddCurriculumItemCommand(3000, 5000, 3, QuotaPeriod.AcademicYear, 4, 12, null, null, null, Administrator(), 4242),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private static ClaimsPrincipal Administrator()
        => new(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Administrator")], "test"));

    private static ApplicationDbContext CreateDbContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static async Task SeedAsync(ApplicationDbContext dbContext)
    {
        var college = new College { Id = 1, Name = "CPSA", ShortCode = "CPSA" };
        var speciality = new Speciality { Id = 1, Name = "Paediatrics", CollegeId = 1, College = college };
        var subSpeciality = new SubSpeciality { Id = 1, Name = "Paediatrics", SpecialityId = 1, Speciality = speciality };

        dbContext.Colleges.Add(college);
        dbContext.Specialities.Add(speciality);
        dbContext.SubSpecialities.Add(subSpeciality);

        dbContext.EntrustmentScales.AddRange(
            new EntrustmentScale
            {
                Id = FiveRungScaleId,
                Name = "Paed General Entrustment Scale",
                Levels = Enumerable.Range(1, 5)
                    .Select(order => new EntrustmentLevel { Id = 9000 + order, Order = order, Label = order.ToString() })
                    .ToList()
            },
            new EntrustmentScale
            {
                Id = SixRungScaleId,
                Name = "CPSA Paediatric Entrustment Scale v11.1",
                Levels = Enumerable.Range(1, 6)
                    .Select(order => new EntrustmentLevel { Id = 9100 + order, Order = order, Label = order.ToString() })
                    .ToList()
            });

        dbContext.Epas.Add(new Epa { Id = 5000, Code = "EPA-1", Title = "Take a history", SubSpecialityId = 1 });

        dbContext.Curricula.Add(new Curriculum
        {
            Id = 3000,
            SubSpecialityId = 1,
            SubSpeciality = subSpeciality,
            Name = "Paediatric EPA Curriculum",
            Version = "11.1",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            IsActive = true
        });

        await dbContext.SaveChangesAsync();
    }
}
