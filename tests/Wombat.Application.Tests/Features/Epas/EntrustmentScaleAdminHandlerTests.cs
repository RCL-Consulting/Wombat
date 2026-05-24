using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.Epas.Commands.CreateEntrustmentScale;
using Wombat.Application.Features.Epas.Commands.DeleteEntrustmentScale;
using Wombat.Application.Features.Epas.Commands.UpdateEntrustmentScale;
using Wombat.Domain.Epas;
using Wombat.Domain.Forms;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Epas;

public sealed class EntrustmentScaleAdminHandlerTests
{
    [Fact]
    public async Task Create_PersistsScaleWithOrderedLevels()
    {
        await using var db = CreateDb();
        var handler = new CreateEntrustmentScaleCommandHandler(db);

        var result = await handler.Handle(
            new CreateEntrustmentScaleCommand(
                "Paed Scale",
                "Default 5-level scale.",
                [
                    new EntrustmentLevelInput(1, "Observe only", "Trainee observes."),
                    new EntrustmentLevelInput(2, "Direct supervision", null),
                    new EntrustmentLevelInput(3, "Indirect supervision", null),
                    new EntrustmentLevelInput(4, "Independent", null),
                    new EntrustmentLevelInput(5, "Supervises others", null)
                ]),
            CancellationToken.None);

        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("Paed Scale");
        result.Levels.Should().HaveCount(5);
        result.Levels.Select(level => level.Order).Should().BeInAscendingOrder();

        var stored = await db.Set<EntrustmentScale>().Include(scale => scale.Levels)
            .SingleAsync(scale => scale.Id == result.Id);
        stored.Levels.Should().HaveCount(5);
    }

    [Fact]
    public async Task Create_TrimsNameAndRejectsDuplicate()
    {
        await using var db = CreateDb();
        var handler = new CreateEntrustmentScaleCommandHandler(db);

        await handler.Handle(
            new CreateEntrustmentScaleCommand(
                "  Paed Scale  ",
                null,
                [
                    new EntrustmentLevelInput(1, "A", null),
                    new EntrustmentLevelInput(2, "B", null)
                ]),
            CancellationToken.None);

        var second = async () => await handler.Handle(
            new CreateEntrustmentScaleCommand(
                "Paed Scale",
                null,
                [
                    new EntrustmentLevelInput(1, "A", null),
                    new EntrustmentLevelInput(2, "B", null)
                ]),
            CancellationToken.None);

        await second.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task Update_AddsRenamesAndRemovesLevels()
    {
        await using var db = CreateDb();
        var seedHandler = new CreateEntrustmentScaleCommandHandler(db);
        var seeded = await seedHandler.Handle(
            new CreateEntrustmentScaleCommand(
                "Scale",
                null,
                [
                    new EntrustmentLevelInput(1, "First", null),
                    new EntrustmentLevelInput(2, "Second", null),
                    new EntrustmentLevelInput(3, "Third", null)
                ]),
            CancellationToken.None);

        var firstId = seeded.Levels.Single(level => level.Order == 1).Id;
        var secondId = seeded.Levels.Single(level => level.Order == 2).Id;

        var update = new UpdateEntrustmentScaleCommandHandler(db);
        var result = await update.Handle(
            new UpdateEntrustmentScaleCommand(
                seeded.Id,
                "Renamed Scale",
                "Now with edits.",
                [
                    new EntrustmentLevelUpdate(firstId, 1, "First (renamed)", null),
                    new EntrustmentLevelUpdate(secondId, 2, "Second", null),
                    new EntrustmentLevelUpdate(null, 3, "Brand new", null)
                ]),
            CancellationToken.None);

        result.Name.Should().Be("Renamed Scale");
        result.Description.Should().Be("Now with edits.");
        result.Levels.Should().HaveCount(3);
        result.Levels.Single(level => level.Order == 1).Label.Should().Be("First (renamed)");
        result.Levels.Single(level => level.Order == 3).Label.Should().Be("Brand new");
        result.Levels.Should().NotContain(level => level.Label == "Third");
    }

    [Fact]
    public async Task Delete_RemovesScaleWhenUnused()
    {
        await using var db = CreateDb();
        var create = new CreateEntrustmentScaleCommandHandler(db);
        var seeded = await create.Handle(
            new CreateEntrustmentScaleCommand(
                "Throwaway",
                null,
                [
                    new EntrustmentLevelInput(1, "Low", null),
                    new EntrustmentLevelInput(2, "High", null)
                ]),
            CancellationToken.None);

        var delete = new DeleteEntrustmentScaleCommandHandler(db);
        await delete.Handle(new DeleteEntrustmentScaleCommand(seeded.Id), CancellationToken.None);

        (await db.Set<EntrustmentScale>().AnyAsync()).Should().BeFalse();
        (await db.Set<EntrustmentLevel>().AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task Delete_RejectsScaleReferencedByAssessmentForm()
    {
        await using var db = CreateDb();
        var create = new CreateEntrustmentScaleCommandHandler(db);
        var seeded = await create.Handle(
            new CreateEntrustmentScaleCommand(
                "Bound",
                null,
                [
                    new EntrustmentLevelInput(1, "Low", null),
                    new EntrustmentLevelInput(2, "High", null)
                ]),
            CancellationToken.None);

        db.Set<AssessmentForm>().Add(new AssessmentForm
        {
            Id = 1,
            Name = "Form 1",
            ScaleId = seeded.Id,
            IsActive = true
        });
        await db.SaveChangesAsync();

        var delete = new DeleteEntrustmentScaleCommandHandler(db);
        var action = async () => await delete.Handle(new DeleteEntrustmentScaleCommand(seeded.Id), CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*assessment forms*");
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
}
