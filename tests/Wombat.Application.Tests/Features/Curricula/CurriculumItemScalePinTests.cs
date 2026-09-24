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
            new AddCurriculumItemCommand(3000, 5000, 3, QuotaPeriod.AcademicYear, 4, 12, null, null, null, null, null, false, Administrator(), FiveRungScaleId),
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
            new AddCurriculumItemCommand(3000, 5000, 3, QuotaPeriod.AcademicYear, 4, 12, null, null, null, null, null, false, Administrator()),
            CancellationToken.None);

        result.Items.Single().ScaleId.Should().BeNull("unpinned is the safe default, not an error state");
    }

    [Fact]
    public async Task UpdateCurriculumItem_CanPinAndUnpin()
    {
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext);

        var added = await new AddCurriculumItemCommandHandler(dbContext).Handle(
            new AddCurriculumItemCommand(3000, 5000, 3, QuotaPeriod.AcademicYear, 4, 12, null, null, null, null, null, false, Administrator()),
            CancellationToken.None);
        var itemId = added.Items.Single().Id;

        var pinned = await new UpdateCurriculumItemCommandHandler(dbContext).Handle(
            new UpdateCurriculumItemCommand(3000, itemId, 5000, 3, QuotaPeriod.AcademicYear, 4, 12, null, null, null, null, null, false, Administrator(), SixRungScaleId),
            CancellationToken.None);
        pinned.Items.Single().ScaleId.Should().Be(SixRungScaleId);

        var unpinned = await new UpdateCurriculumItemCommandHandler(dbContext).Handle(
            new UpdateCurriculumItemCommand(3000, itemId, 5000, 3, QuotaPeriod.AcademicYear, 4, 12, null, null, null, null, null, false, Administrator()),
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
            new AddCurriculumItemCommand(3000, 5000, 3, QuotaPeriod.AcademicYear, 6, 12, null, null, null, null, null, false, Administrator(), FiveRungScaleId),
            CancellationToken.None);

        // T136: the refusal names the field, the value and the ladder, so the operator knows what to change. A new
        // item has no previous ladder, so there is no "(rung …)" to add.
        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("Minimum level 6 is not a rung on Paed General Entrustment Scale, which has 5 rungs.");
    }

    [Fact]
    public async Task AddCurriculumItem_WhenAStageOverrideExceedsTheScale_IsRefused()
    {
        // The flat minimum fits, but a per-stage override does not — and the stage map is where the real
        // progression curve lives, so checking only the flat value would miss most of the risk.
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext);

        var act = () => new AddCurriculumItemCommandHandler(dbContext).Handle(
            new AddCurriculumItemCommand(3000, 5000, 3, QuotaPeriod.AcademicYear, 4, 12, null, """{"1":2,"4":6}""", null, null, null, false, Administrator(), FiveRungScaleId),
            CancellationToken.None);

        // Only the year that does not fit is named: the flat 4 and year 1's 2 are rungs on the five-rung ladder.
        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("Year 4 minimum 6 is not a rung on Paed General Entrustment Scale, which has 5 rungs.");
    }

    // ---- T125 / T136: the Update path ----

    [Fact]
    public async Task UpdateCurriculumItem_RePinnedToAShorterLadderWithTheOldOrdinals_IsRefused_NamingEachField_AndChangesNothing()
    {
        // T136's reproduction: PAED-001 on the six-rung v11.1 ladder, minimum 6 and a year curve reaching 6, re-pinned
        // to a five-rung ladder without touching the ordinals. The pin is refused, and the refusal names the scale,
        // each field that does not fit, its value, and the rung the operator knew it as on the ladder it is leaving.
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext);
        var itemId = await SeedPinnedItemAsync(dbContext, SixRungScaleId, 6, """{"1":3,"2":4,"3":5,"4":6}""");

        var act = () => new UpdateCurriculumItemCommandHandler(dbContext).Handle(
            new UpdateCurriculumItemCommand(3000, itemId, 5000, 3, QuotaPeriod.AcademicYear, 6, 12, null, """{"1":3,"2":4,"3":5,"4":6}""", null, null, null, false, Administrator(), FiveRungScaleId),
            CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage(
                "Minimum level 6 (rung 5) and year 4 minimum 6 (rung 5) are not rungs on Paed General Entrustment Scale, which has 5 rungs. " +
                "The rungs in brackets are as CPSA Paediatric Entrustment Scale v11.1 names them.");

        // Refused before the first mutation. AuditPipelineBehavior saves the request's DbContext from its catch, so a
        // handler that assigned the item's fields and then threw would commit them. Save as it does, then read back:
        // re-reading without the save would show the stored row whatever the handler had changed in memory.
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
        var stored = await dbContext.CurriculumItems.AsNoTracking().SingleAsync(item => item.Id == itemId);
        stored.ScaleId.Should().Be(SixRungScaleId);
        stored.MinimumLevelOrder.Should().Be(6);
        CurriculumItem.ParseStageOverrides(stored.MinimumLevelByStageJson).Should().Equal(
            new Dictionary<int, int> { [1] = 3, [2] = 4, [3] = 5, [4] = 6 });
    }

    [Fact]
    public async Task UpdateCurriculumItem_ChangingTheScaleTheFlatMinimumAndTheStageMinimaTogether_Succeeds()
    {
        // T136: the path that works, and that nothing guarded. The check is on the REQUESTED values, so re-picking
        // every minimum on the new ladder in the same save is a valid re-pin, not a refusal about the old values.
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext);
        var itemId = await SeedPinnedItemAsync(dbContext, SixRungScaleId, 6, """{"1":3,"2":4,"3":5,"4":6}""");

        var result = await new UpdateCurriculumItemCommandHandler(dbContext).Handle(
            new UpdateCurriculumItemCommand(3000, itemId, 5000, 3, QuotaPeriod.AcademicYear, 4, 12, null, """{"1":2,"2":3,"3":3,"4":4}""", null, null, null, false, Administrator(), FiveRungScaleId),
            CancellationToken.None);

        var dto = result.Items.Single();
        dto.ScaleId.Should().Be(FiveRungScaleId);
        dto.MinimumLevelOrder.Should().Be(4);

        var stored = await dbContext.CurriculumItems.AsNoTracking().SingleAsync(item => item.Id == itemId);
        stored.ScaleId.Should().Be(FiveRungScaleId);
        stored.MinimumLevelOrder.Should().Be(4);
        CurriculumItem.ParseStageOverrides(stored.MinimumLevelByStageJson).Should().Equal(
            new Dictionary<int, int> { [1] = 2, [2] = 3, [3] = 3, [4] = 4 });
    }

    [Fact]
    public async Task AddCurriculumItem_WhenTheScaleDoesNotExist_IsRefused()
    {
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext);

        var act = () => new AddCurriculumItemCommandHandler(dbContext).Handle(
            new AddCurriculumItemCommand(3000, 5000, 3, QuotaPeriod.AcademicYear, 4, 12, null, null, null, null, null, false, Administrator(), 4242),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private static async Task<int> SeedPinnedItemAsync(ApplicationDbContext dbContext, int scaleId, int minimum, string stageMinimaJson)
    {
        var item = new CurriculumItem
        {
            CurriculumId = 3000,
            EpaId = 5000,
            RequiredCount = 3,
            QuotaPeriod = QuotaPeriod.AcademicYear,
            MinimumLevelOrder = minimum,
            WindowMonths = 12,
            MinimumLevelByStageJson = stageMinimaJson,
            ScaleId = scaleId
        };
        dbContext.CurriculumItems.Add(item);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
        return item.Id;
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
                // The College's labels: v11.1 splits level 3, so Order 6 is rung "5" (T100).
                Levels = new[] { "1", "2", "3a", "3b", "4", "5" }
                    .Select((label, index) => new EntrustmentLevel { Id = 9101 + index, Order = index + 1, Label = label })
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
