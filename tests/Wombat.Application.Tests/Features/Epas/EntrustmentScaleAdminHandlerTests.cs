using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.Epas.Commands.CreateEntrustmentScale;
using Wombat.Application.Features.Epas.Commands.DeleteEntrustmentScale;
using Wombat.Application.Features.Epas.Commands.UpdateEntrustmentScale;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;
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
                ],
                TestPrincipals.Administrator()),
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
                ],
                TestPrincipals.Administrator()),
            CancellationToken.None);

        var second = async () => await handler.Handle(
            new CreateEntrustmentScaleCommand(
                "Paed Scale",
                null,
                [
                    new EntrustmentLevelInput(1, "A", null),
                    new EntrustmentLevelInput(2, "B", null)
                ],
                TestPrincipals.Administrator()),
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
                ],
                TestPrincipals.Administrator()),
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
                ],
                TestPrincipals.Administrator()),
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
                ],
                TestPrincipals.Administrator()),
            CancellationToken.None);

        var delete = new DeleteEntrustmentScaleCommandHandler(db);
        await delete.Handle(new DeleteEntrustmentScaleCommand(seeded.Id, TestPrincipals.Administrator()), CancellationToken.None);

        (await db.Set<EntrustmentScale>().AnyAsync()).Should().BeFalse();
        (await db.Set<EntrustmentLevel>().AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task Delete_RejectsScalePinnedToACurriculumItem()
    {
        // T109 added two Restrict foreign keys to EntrustmentScales. Without a pre-flight check the delete
        // reaches PostgreSQL and comes back as a raw DbUpdateException — a stack trace where the
        // administrator needed a sentence.
        await using var db = CreateDb();
        var seeded = await SeedTwoLevelScaleAsync(db, "Pinned");

        db.Set<CurriculumItem>().Add(new CurriculumItem
        {
            Id = 1,
            CurriculumId = 3000,
            EpaId = 5000,
            RequiredCount = 1,
            MinimumLevelOrder = 2,
            WindowMonths = 12,
            ScaleId = seeded.Id
        });
        await db.SaveChangesAsync();

        var delete = new DeleteEntrustmentScaleCommandHandler(db);
        var action = async () => await delete.Handle(new DeleteEntrustmentScaleCommand(seeded.Id, TestPrincipals.Administrator()), CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*curriculum items*");
    }

    [Fact]
    public async Task Delete_RejectsScaleTraineeProgressWasScoredOn()
    {
        await using var db = CreateDb();
        var seeded = await SeedTwoLevelScaleAsync(db, "Scored");

        db.Set<CurriculumItemProgress>().Add(new CurriculumItemProgress
        {
            Id = 1,
            CurriculumItemId = 1,
            TraineeUserId = "trainee-1",
            AcademicYear = 2026,
            Semester = 1,
            MinimumLevelScaleId = seeded.Id
        });
        await db.SaveChangesAsync();

        var delete = new DeleteEntrustmentScaleCommandHandler(db);
        var action = async () => await delete.Handle(new DeleteEntrustmentScaleCommand(seeded.Id, TestPrincipals.Administrator()), CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*progress*");
    }

    [Fact]
    public async Task Delete_RejectsScaleThatIsASubSpecialitysDefault_NamingIt_AndCommitsNothing()
    {
        // SubSpecialities.DefaultEntrustmentScaleId is ON DELETE RESTRICT, and until T232 the handler never asked about
        // it: the delete reached PostgreSQL and the administrator read a raw DbUpdateException.
        await using var db = CreateDb();
        var seeded = await SeedTwoLevelScaleAsync(db, "Neonatal ladder");
        SeedSubSpeciality(db, specialityId: 40, "Paediatrics", subSpecialityId: 41, "Neonatology", seeded.Id);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var delete = new DeleteEntrustmentScaleCommandHandler(db);
        var action = async () => await delete.Handle(
            new DeleteEntrustmentScaleCommand(seeded.Id, TestPrincipals.Administrator()), CancellationToken.None);

        var refusal = (await action.Should().ThrowAsync<InvalidOperationException>()).Which;
        refusal.Message.Should().Be(
            "This entrustment scale is the default scale of the sub-speciality \"Neonatology\" (Paediatrics), so it " +
            "cannot be deleted. Change that sub-speciality's default entrustment scale to another scale, or to no " +
            "default, first.");
        refusal.InnerException.Should().BeNull("the refusal is the handler's own, made before any save");

        // The audit trap: the failure row is saved through this same context, so whatever the handler tracked before
        // it threw would be committed with it.
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var stored = await db.Set<EntrustmentScale>().Include(scale => scale.Levels).SingleAsync(scale => scale.Id == seeded.Id);
        stored.Levels.Should().HaveCount(2, "no level was removed ahead of the refusal");
        (await db.Set<SubSpeciality>().SingleAsync(entity => entity.Id == 41)).DefaultEntrustmentScaleId.Should().Be(seeded.Id);
    }

    [Fact]
    public async Task Delete_RejectsScaleThatIsTheDefaultOfSeveralSubSpecialities_NamingEachWithItsSpeciality()
    {
        // A sub-speciality's name is unique only within its speciality, so each is named with its own.
        await using var db = CreateDb();
        var seeded = await SeedTwoLevelScaleAsync(db, "Shared ladder");
        SeedSubSpeciality(db, specialityId: 50, "Surgery", subSpecialityId: 51, "General", seeded.Id);
        SeedSubSpeciality(db, specialityId: 52, "Paediatrics", subSpecialityId: 53, "General", seeded.Id);
        SeedSubSpeciality(db, specialityId: 54, "Internal Medicine", subSpecialityId: 55, "Cardiology", seeded.Id);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var delete = new DeleteEntrustmentScaleCommandHandler(db);
        var action = async () => await delete.Handle(
            new DeleteEntrustmentScaleCommand(seeded.Id, TestPrincipals.Administrator()), CancellationToken.None);

        (await action.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(
            "This entrustment scale is the default scale of the sub-specialities \"Cardiology\" (Internal Medicine), " +
            "\"General\" (Paediatrics) and \"General\" (Surgery), so it cannot be deleted. Change each one's default " +
            "entrustment scale to another scale, or to no default, first.");
    }

    [Fact]
    public async Task Delete_RemovesScale_WhenTheOnlySubSpecialityDefaultIsAnotherScale()
    {
        await using var db = CreateDb();
        var kept = await SeedTwoLevelScaleAsync(db, "Kept");
        var removed = await SeedTwoLevelScaleAsync(db, "Removed");
        SeedSubSpeciality(db, specialityId: 60, "Paediatrics", subSpecialityId: 61, "Paediatrics", kept.Id);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await new DeleteEntrustmentScaleCommandHandler(db).Handle(
            new DeleteEntrustmentScaleCommand(removed.Id, TestPrincipals.Administrator()), CancellationToken.None);

        (await db.Set<EntrustmentScale>().Select(scale => scale.Id).ToListAsync()).Should().Equal(kept.Id);
    }

    private static void SeedSubSpeciality(
        ApplicationDbContext db, int specialityId, string specialityName, int subSpecialityId, string name, int defaultScaleId)
    {
        db.Set<Speciality>().Add(new Speciality { Id = specialityId, CollegeId = 1, Name = specialityName });
        db.Set<SubSpeciality>().Add(new SubSpeciality
        {
            Id = subSpecialityId,
            SpecialityId = specialityId,
            Name = name,
            DefaultEntrustmentScaleId = defaultScaleId
        });
    }

    [Fact]
    public async Task Update_RejectsRenameWhenAPublishedSchemaBindsToTheName()
    {
        // The schema DSL binds a field to a ladder by exact NAME. A rename breaks that binding silently:
        // the key stops resolving, the credit engine falls back to comparing bare ordinals, and T109's
        // protection disappears with no error anywhere (the rung picker empties at the same moment).
        await using var db = CreateDb();
        var seeded = await SeedTwoLevelScaleAsync(db, "CPSA Paediatric Entrustment Scale v11.1");

        db.Set<ActivityTypeVersion>().Add(new ActivityTypeVersion
        {
            Id = 1,
            ActivityTypeId = 7,
            Version = 2,
            SchemaJson = """
                {
                  "version": 1,
                  "sections": [
                    {
                      "key": "assessment",
                      "title": "Entrustment",
                      "fields": [
                        { "key": "overall_level", "type": "scale", "label": "Level", "scale_key": "CPSA Paediatric Entrustment Scale v11.1" }
                      ]
                    }
                  ]
                }
                """,
            WorkflowJson = "{}",
            CreditRulesJson = "{}"
        });
        await db.SaveChangesAsync();

        var update = new UpdateEntrustmentScaleCommandHandler(db);
        var action = async () => await update.Handle(
            new UpdateEntrustmentScaleCommand(
                seeded.Id,
                "CPSA Paediatric Entrustment Scale v11.2",
                null,
                [new EntrustmentLevelUpdate(null, 1, "Low", null), new EntrustmentLevelUpdate(null, 2, "High", null)],
                TestPrincipals.Administrator()),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*binds to this entrustment scale by name*");
    }

    [Fact]
    public async Task Delete_RejectsScaleAPublishedSchemaBindsToByName()
    {
        // Deleting does exactly what renaming does to a scale_key binding, so the same guard has to stand
        // on both doors — a guard on one is the same hole with an extra step.
        await using var db = CreateDb();
        var seeded = await SeedTwoLevelScaleAsync(db, "Local Ladder");

        db.Set<ActivityTypeVersion>().Add(new ActivityTypeVersion
        {
            Id = 2,
            ActivityTypeId = 9,
            Version = 1,
            SchemaJson = """
                {
                  "version": 1,
                  "sections": [
                    {
                      "key": "assessment",
                      "title": "Entrustment",
                      "fields": [
                        { "key": "overall_level", "type": "scale", "label": "Level", "scale_key": "Local Ladder" }
                      ]
                    }
                  ]
                }
                """,
            WorkflowJson = "{}",
            CreditRulesJson = "{}"
        });
        await db.SaveChangesAsync();

        var delete = new DeleteEntrustmentScaleCommandHandler(db);
        var action = async () => await delete.Handle(new DeleteEntrustmentScaleCommand(seeded.Id, TestPrincipals.Administrator()), CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*binds to this entrustment scale by name*");
    }

    [Fact]
    public async Task Update_RejectsRemovingARungAPinnedCurriculumItemRequires()
    {
        // The level validator forces the incoming set to be contiguous from 1, so a removal always removes
        // the TOP rung — exactly the one an "Independent" minimum names. Losing it would make that minimum
        // permanently unmeetable, with nothing anywhere saying so.
        await using var db = CreateDb();
        var seeded = await SeedTwoLevelScaleAsync(db, "Pinned ladder");

        db.Set<CurriculumItem>().Add(new CurriculumItem
        {
            Id = 2,
            CurriculumId = 3000,
            EpaId = 5000,
            RequiredCount = 1,
            MinimumLevelOrder = 2,
            WindowMonths = 12,
            ScaleId = seeded.Id
        });
        await db.SaveChangesAsync();

        var update = new UpdateEntrustmentScaleCommandHandler(db);
        var action = async () => await update.Handle(
            new UpdateEntrustmentScaleCommand(
                seeded.Id,
                "Pinned ladder",
                null,
                [new EntrustmentLevelUpdate(seeded.Levels[0].Id, 1, "Low", null)],
                TestPrincipals.Administrator()),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*requires level 2*");
    }

    [Fact]
    public async Task Update_AllowsRenameWhenNoSchemaBindsToTheName()
    {
        await using var db = CreateDb();
        var seeded = await SeedTwoLevelScaleAsync(db, "Unreferenced");

        var update = new UpdateEntrustmentScaleCommandHandler(db);
        var result = await update.Handle(
            new UpdateEntrustmentScaleCommand(
                seeded.Id,
                "Renamed freely",
                null,
                [new EntrustmentLevelUpdate(null, 1, "Low", null), new EntrustmentLevelUpdate(null, 2, "High", null)],
                TestPrincipals.Administrator()),
            CancellationToken.None);

        result.Name.Should().Be("Renamed freely");
    }

    private static async Task<EntrustmentScaleDto> SeedTwoLevelScaleAsync(ApplicationDbContext db, string name)
        => await new CreateEntrustmentScaleCommandHandler(db).Handle(
            new CreateEntrustmentScaleCommand(
                name,
                null,
                [
                    new EntrustmentLevelInput(1, "Low", null),
                    new EntrustmentLevelInput(2, "High", null)
                ],
                TestPrincipals.Administrator()),
            CancellationToken.None);

    [Fact]
    public async Task Create_InstitutionalAdmin_Rejected()
    {
        await using var db = CreateDb();
        var handler = new CreateEntrustmentScaleCommandHandler(db);

        var act = () => handler.Handle(
            new CreateEntrustmentScaleCommand(
                "Should fail",
                null,
                [
                    new EntrustmentLevelInput(1, "A", null),
                    new EntrustmentLevelInput(2, "B", null)
                ],
                TestPrincipals.InstitutionalAdmin(1)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
}
