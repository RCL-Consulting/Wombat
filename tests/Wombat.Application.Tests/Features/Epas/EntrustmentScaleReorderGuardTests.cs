using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.Epas.Commands.CreateEntrustmentScale;
using Wombat.Application.Features.Epas.Commands.UpdateEntrustmentScale;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Epas;

/// <summary>
/// T395 (T394, flow 16, lane B; Q16, Q18, P2): on a scale anything uses, a rung may not move or go, because every use reads
/// a rung by its rank and a reorder would silently change what each stored minimum means (the CPSA ladder's nine items).
/// "In use" is the delete's seven uses (<see cref="EntrustmentScaleUses" />). Adding a rung at the top and editing a label
/// stay allowed. A save that removes a rung a pinned item needs is answered by Q6's words first, by EPA, curriculum and
/// rung label, three named then "and ‹N› more." (Q18). Every refusal comes before anything is assigned (T201).
/// </summary>
public sealed class EntrustmentScaleReorderGuardTests
{
    private const int CurriculumId = 3000;
    private const int SubSpecialityId = 1000;

    [Fact]
    public async Task AReorder_OnAScaleInUse_IsRefused_AndChangesNothing()
    {
        await using var db = await SeededAsync();
        var scale = await CreateAsync(db, "Observe only", "Direct supervision", "Indirect supervision");
        Pin(db, scale, epaId: 1, minimum: 1);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var swapped = Levels(scale, (1, 1), (3, 2), (2, 3)); // rungs 2 and 3 swap ranks
        var act = () => Update(db, scale, "CNSA Neurology Entrustment Scale (renamed)", swapped);

        var refusal = (await act.Should().ThrowAsync<EntrustmentScaleUpdateRefusedException>()).Which;
        refusal.Reason.Should().Be(EntrustmentScaleUpdateRefusal.InUse);
        refusal.Message.Should().Be(
            "Not saved. Rungs can be reordered or removed only while nothing uses this scale, and this scale is now in use. " +
            "The page has been read again.");
        refusal.RungsToPutBack.Should().BeEmpty("after Q16's refusal the page is read again; nothing is put back (S14)");

        // The audit pipeline saves a refused handler's context (T201): nothing may have been assigned.
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var stored = await db.EntrustmentScales.Include(entity => entity.Levels).SingleAsync(entity => entity.Id == scale.Id);
        stored.Name.Should().Be("CNSA Neurology Entrustment Scale");
        stored.Levels.OrderBy(level => level.Order).Select(level => level.Label)
            .Should().Equal("Observe only", "Direct supervision", "Indirect supervision");
    }

    [Fact]
    public async Task AReorder_OnAScaleNothingUses_IsSaved()
    {
        await using var db = await SeededAsync();
        var scale = await CreateAsync(db, "Observe only", "Direct supervision", "Indirect supervision");
        db.ChangeTracker.Clear();

        var result = await Update(db, scale, scale.Name, Levels(scale, (1, 1), (3, 2), (2, 3)));

        result.Scale.Levels.Select(level => level.Label).Should().Equal("Observe only", "Indirect supervision", "Direct supervision");
    }

    // P2 and 6.7: a sub-speciality's default is a use, so the CNSA scale is in use from the moment Adult Neurology takes it.
    [Fact]
    public async Task AReorder_OnAScaleASubSpecialityDefaultsTo_IsRefused()
    {
        await using var db = await SeededAsync();
        var scale = await CreateAsync(db, "Observe only", "Direct supervision");
        (await db.SubSpecialities.SingleAsync()).DefaultEntrustmentScaleId = scale.Id;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var act = () => Update(db, scale, scale.Name, Levels(scale, (2, 1), (1, 2)));

        (await act.Should().ThrowAsync<EntrustmentScaleUpdateRefusedException>()).Which.Reason
            .Should().Be(EntrustmentScaleUpdateRefusal.InUse);
    }

    // Q16 covers a removal no item needs: the item needs rung 1, the save removes rung 3.
    [Fact]
    public async Task ARemovalNoItemNeeds_OnAScaleInUse_IsRefusedAsInUse()
    {
        await using var db = await SeededAsync();
        var scale = await CreateAsync(db, "Observe only", "Direct supervision", "Indirect supervision");
        Pin(db, scale, epaId: 1, minimum: 1);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var act = () => Update(db, scale, scale.Name, Levels(scale, (1, 1), (2, 2)));

        (await act.Should().ThrowAsync<EntrustmentScaleUpdateRefusedException>()).Which.Reason
            .Should().Be(EntrustmentScaleUpdateRefusal.InUse);
    }

    // P2, 6.6: a sixth rung at the top and a relabelled rung change no rank, so they are saved on a scale in use.
    [Fact]
    public async Task AddingARungAtTheTop_AndRelabellingOne_OnAScaleInUse_IsSaved()
    {
        await using var db = await SeededAsync();
        var scale = await CreateAsync(db, "Observe only", "Direct supervision");
        Pin(db, scale, epaId: 1, minimum: 2);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var levels = new List<EntrustmentLevelUpdate>
        {
            new(scale.Levels[0].Id, 1, "Observe only (relabelled)", null),
            new(scale.Levels[1].Id, 2, "Direct supervision", "Edited."),
            new(null, 3, "Supervises others", "Supervises juniors performing the activity.")
        };
        var result = await Update(db, scale, scale.Name, levels);

        result.Scale.Levels.Select(level => level.Label)
            .Should().Equal("Observe only (relabelled)", "Direct supervision", "Supervises others");
    }

    // Q18: a save that reorders AND removes a rung an item needs gets Q6's words, not Q16's, and offers to put it back.
    [Fact]
    public async Task ASaveThatReordersAndRemovesANeededRung_GetsQ6sWords_ByLabel_WithTheRungToPutBack()
    {
        await using var db = await SeededAsync();
        var scale = await CreateAsync(db, "Observe only", "Direct supervision", "Indirect supervision", "Supervises others");
        Pin(db, scale, epaId: 1, minimum: 4);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var act = () => Update(db, scale, scale.Name, Levels(scale, (2, 1), (1, 2), (3, 3)));

        var refusal = (await act.Should().ThrowAsync<EntrustmentScaleUpdateRefusedException>()).Which;
        refusal.Reason.Should().Be(EntrustmentScaleUpdateRefusal.RungNeeded);
        refusal.Needs.Should().ContainSingle().Which.Should().Be(new EntrustmentScaleRungNeed(
            "NEU-001", "Neurology EPA Curriculum", "1.0", scale.Levels[3].Id, "Supervises others"));
        refusal.MoreCount.Should().Be(0);
        refusal.RungsToPutBack.Should().Equal(scale.Levels[3].Id);
        refusal.Message.Should().Be(
            "Not saved. NEU-001 on Neurology EPA Curriculum (1.0) needs the rung “Supervises others” of this scale, which the " +
            "new levels leave out. Change that item's minimum first.");
    }

    // Q6, N5: three items named, the tail joined to the third.
    [Fact]
    public async Task FiveItemsNeedingTheRemovedRung_AreThreeNamed_ThenAndTwoMore()
    {
        await using var db = await SeededAsync();
        var scale = await CreateAsync(db, "Observe only", "Direct supervision");
        for (var epaId = 1; epaId <= 5; epaId++)
        {
            Pin(db, scale, epaId, minimum: 2);
        }

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var act = () => Update(db, scale, scale.Name, [new EntrustmentLevelUpdate(scale.Levels[0].Id, 1, "Observe only", null),
            new EntrustmentLevelUpdate(null, 2, "Direct", null)]);

        var refusal = (await act.Should().ThrowAsync<EntrustmentScaleUpdateRefusedException>()).Which;
        refusal.Needs.Select(need => need.EpaCode).Should().Equal("NEU-001", "NEU-002", "NEU-003");
        refusal.MoreCount.Should().Be(2);
        EntrustmentScaleRefusalWords.NeedLines(refusal.Needs, refusal.MoreCount)[2].Should().EndWith(
            "Change that item's minimum first. and 2 more.");
        refusal.Message.Should().EndWith(" and 2 more.");
    }

    // A stage override names a rung too.
    [Fact]
    public async Task AStageOverrideNeedingTheRemovedRung_IsNamed()
    {
        await using var db = await SeededAsync();
        var scale = await CreateAsync(db, "Observe only", "Direct supervision", "Indirect supervision");
        Pin(db, scale, epaId: 1, minimum: 1, stages: """{"3":3}""");
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var act = () => Update(db, scale, scale.Name, Levels(scale, (1, 1), (2, 2)));

        (await act.Should().ThrowAsync<EntrustmentScaleUpdateRefusedException>()).Which.Needs.Single().RungLabel
            .Should().Be("Indirect supervision");
    }

    // R3-C03b: a repeated label is refused under the later rung's own Label, in the field's words.
    [Fact]
    public void ARepeatedLabel_IsRefusedUnderTheLaterRung()
    {
        var result = new UpdateEntrustmentScaleCommandValidator().Validate(new UpdateEntrustmentScaleCommand(
            1, "CNSA Neurology Entrustment Scale", null,
            [
                new EntrustmentLevelUpdate(1, 1, "Observe only", null),
                new EntrustmentLevelUpdate(2, 2, "Indirect supervision", null),
                new EntrustmentLevelUpdate(3, 3, "Indirect supervision ", null),
                new EntrustmentLevelUpdate(4, 4, "", null)
            ],
            TestPrincipals.Administrator()));

        result.Errors.Select(error => (error.PropertyName, error.ErrorMessage)).Should().BeEquivalentTo(new[]
        {
            ("Levels[2].Label", "“Indirect supervision” is already the label of another rung. Give each rung its own label."),
            ("Levels[3].Label", "Enter this rung's label.")
        });
    }

    // ---- helpers ----

    private static async Task<UpdateEntrustmentScaleResult> Update(
        ApplicationDbContext db, EntrustmentScaleDto scale, string name, IReadOnlyList<EntrustmentLevelUpdate> levels)
        => await new UpdateEntrustmentScaleCommandHandler(db).Handle(
            new UpdateEntrustmentScaleCommand(scale.Id, name, scale.Description, levels, TestPrincipals.Administrator()),
            CancellationToken.None);

    /// <summary>The scale's stored rungs, each (stored order, new order), with their labels.</summary>
    private static List<EntrustmentLevelUpdate> Levels(EntrustmentScaleDto scale, params (int Stored, int New)[] moves)
        => moves.Select(move => scale.Levels.Single(level => level.Order == move.Stored))
            .Zip(moves, (level, move) => new EntrustmentLevelUpdate(level.Id, move.New, level.Label, level.Description))
            .ToList();

    private static async Task<EntrustmentScaleDto> CreateAsync(ApplicationDbContext db, params string[] labels)
        => await new CreateEntrustmentScaleCommandHandler(db).Handle(
            new CreateEntrustmentScaleCommand(
                "CNSA Neurology Entrustment Scale",
                null,
                labels.Select((label, index) => new EntrustmentLevelInput(index + 1, label, null)).ToList(),
                TestPrincipals.Administrator()),
            CancellationToken.None);

    private static void Pin(ApplicationDbContext db, EntrustmentScaleDto scale, int epaId, int minimum, string? stages = null)
        => db.CurriculumItems.Add(new CurriculumItem
        {
            CurriculumId = CurriculumId,
            EpaId = epaId,
            RequiredCount = 1,
            QuotaPeriod = QuotaPeriod.AcademicYear,
            MinimumLevelOrder = minimum,
            MinimumLevelByStageJson = stages,
            WindowMonths = 12,
            ScaleId = scale.Id
        });

    private static async Task<ApplicationDbContext> SeededAsync()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        db.Colleges.Add(new College { Id = 5, Name = "College of Neurologists of South Africa", ShortCode = "CNSA" });
        db.Specialities.Add(new Speciality { Id = 100, CollegeId = 5, Name = "Neurology" });
        db.SubSpecialities.Add(new SubSpeciality { Id = SubSpecialityId, SpecialityId = 100, Name = "Adult Neurology" });
        for (var epaId = 1; epaId <= 5; epaId++)
        {
            db.Epas.Add(new Epa { Id = epaId, Code = $"NEU-00{epaId}", Title = $"EPA {epaId}", SubSpecialityId = SubSpecialityId });
        }

        db.Curricula.Add(new Curriculum
        {
            Id = CurriculumId, SubSpecialityId = SubSpecialityId, Name = "Neurology EPA Curriculum", Version = "1.0",
            EffectiveFrom = new DateOnly(2026, 1, 1)
        });
        await db.SaveChangesAsync();
        return db;
    }
}
