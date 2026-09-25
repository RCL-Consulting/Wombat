using System.Globalization;
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

        result.Scale.Name.Should().Be("Renamed Scale");
        result.Scale.Description.Should().Be("Now with edits.");
        result.Scale.Levels.Should().HaveCount(3);
        result.Scale.Levels.Single(level => level.Order == 1).Label.Should().Be("First (renamed)");
        result.Scale.Levels.Single(level => level.Order == 3).Label.Should().Be("Brand new");
        result.Scale.Levels.Should().NotContain(level => level.Label == "Third");
        result.RenameWarning.Should().BeNull("no published schema binds the scale");
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

    private const string DeleteRefusalTail =
        "A published version never changes, and activities stay on the version they were filed on, so no later version " +
        "can take that back. Leave the scale in place instead.";

    [Fact]
    public async Task Delete_RejectsScaleAUiBuiltTypeBindsById_NamingTheType_AndCommitsNothing()
    {
        // T253. The activity-type builder writes scale_key as the scale's id. Until T253 the delete compared keys to
        // the scale's name only, so this delete went through and the type's rating field lost its ladder for good.
        await using var db = CreateDb();
        var seeded = await SeedTwoLevelScaleAsync(db, "Local ladder");
        AddPublishedType(db, 30, "local_mini_cex", "Local Mini-CEX", (1, IdKey(seeded.Id)));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var delete = new DeleteEntrustmentScaleCommandHandler(db);
        var action = async () => await delete.Handle(
            new DeleteEntrustmentScaleCommand(seeded.Id, TestPrincipals.Administrator()), CancellationToken.None);

        var refusal = (await action.Should().ThrowAsync<InvalidOperationException>()).Which;
        refusal.Message.Should().Be(
            "The form of the activity type \"Local Mini-CEX\" (local_mini_cex, version 1) uses this entrustment scale, " +
            "so it cannot be deleted. " + DeleteRefusalTail);

        // The audit trap: the failure row is saved through this same context, so whatever the handler removed before it
        // threw would be committed with it.
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var stored = await db.Set<EntrustmentScale>().Include(scale => scale.Levels).SingleAsync(scale => scale.Id == seeded.Id);
        stored.Levels.Should().HaveCount(2, "no level was removed ahead of the refusal");
    }

    [Fact]
    public async Task Delete_RejectsScaleAPublishedSchemaBindsByName()
    {
        // A schema written before T253, or by hand, still binds by name.
        await using var db = CreateDb();
        var seeded = await SeedTwoLevelScaleAsync(db, "Local Ladder");
        AddPublishedType(db, 9, "local_cbd", "Local CbD", (1, "Local Ladder"));
        await db.SaveChangesAsync();

        var delete = new DeleteEntrustmentScaleCommandHandler(db);
        var action = async () => await delete.Handle(new DeleteEntrustmentScaleCommand(seeded.Id, TestPrincipals.Administrator()), CancellationToken.None);

        (await action.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(
            "The form of the activity type \"Local CbD\" (local_cbd, version 1) uses this entrustment scale, so it " +
            "cannot be deleted. " + DeleteRefusalTail);
    }

    [Fact]
    public async Task Delete_RejectsARenamedSeededScaleTheSeedsBindBySeedKey_NamingEveryTypeAndVersionThatBindsIt()
    {
        // The v11.1 ladder, renamed by an administrator: the seeds bind it by its seed key, which the rename did not
        // touch, so they still bind it. Every version is named, because an old version binds as lastingly as the current.
        await using var db = CreateDb();
        var ladder = await AddSeededScaleAsync(db, "cpsa:scale:v11.1", "Paediatric ladder");
        var other = await SeedTwoLevelScaleAsync(db, "Another ladder");
        AddPublishedType(db, 17, "mini_cex_cpsa", "Mini-CEX (CPSA)", (1, "seed:cpsa:scale:v11.1"), (2, "seed:cpsa:scale:v11.1"), (3, IdKey(other.Id)));
        AddPublishedType(db, 18, "dops_cpsa", "DOPS (CPSA)", (1, IdKey(ladder.Id)));
        AddPublishedType(db, 19, "local_acat", "Local ACAT", (1, IdKey(other.Id)));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var action = async () => await new DeleteEntrustmentScaleCommandHandler(db).Handle(
            new DeleteEntrustmentScaleCommand(ladder.Id, TestPrincipals.Administrator()), CancellationToken.None);

        (await action.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(
            "The forms of the activity types \"DOPS (CPSA)\" (dops_cpsa, version 1) and \"Mini-CEX (CPSA)\" " +
            "(mini_cex_cpsa, versions 1 and 2) use this entrustment scale, so it cannot be deleted. " + DeleteRefusalTail);
    }

    [Fact]
    public async Task Delete_RemovesScale_WhenEveryPublishedSchemaBindsAnotherScale()
    {
        // Bound by each of the three forms, but to the scale that stays: the check asks which scale a key binds, not
        // whether it looks like one.
        await using var db = CreateDb();
        var kept = await AddSeededScaleAsync(db, "demo:scale:o-r", "O-R Scale");
        var removed = await SeedTwoLevelScaleAsync(db, "Removed");
        AddPublishedType(db, 1, "mini_cex", "Mini-CEX", (1, "O-R Scale"), (2, "seed:demo:scale:o-r"), (3, IdKey(kept.Id)));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await new DeleteEntrustmentScaleCommandHandler(db).Handle(
            new DeleteEntrustmentScaleCommand(removed.Id, TestPrincipals.Administrator()), CancellationToken.None);

        (await db.Set<EntrustmentScale>().Select(scale => scale.Id).ToListAsync()).Should().Equal(kept.Id);
    }

    [Fact]
    public async Task Update_RenamesAndWarns_WhenAPublishedSchemaBindsTheOldName_NamingTheType()
    {
        // T253: a rename refuses nothing. A schema still bound by the old name loses its ladder, and the save says so.
        await using var db = CreateDb();
        var seeded = await SeedTwoLevelScaleAsync(db, "Local Ladder");
        AddPublishedType(db, 9, "local_cbd", "Local CbD", (1, "Local Ladder"), (2, "Local Ladder"));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await new UpdateEntrustmentScaleCommandHandler(db).Handle(
            new UpdateEntrustmentScaleCommand(
                seeded.Id,
                "Local Ladder v2",
                null,
                seeded.Levels.Select(level => new EntrustmentLevelUpdate(level.Id, level.Order, level.Label, level.Description)).ToList(),
                TestPrincipals.Administrator()),
            CancellationToken.None);

        result.Scale.Name.Should().Be("Local Ladder v2");
        result.RenameWarning.Should().Be(
            "The form of the activity type \"Local CbD\" (local_cbd, versions 1 and 2) names this scale by its old name, " +
            "\"Local Ladder\", so since it became \"Local Ladder v2\" their scale fields have no ladder: the form " +
            "offers a field's own options, or asks for a bare number, instead of the ladder's rungs, and credit compares " +
            "bare numbers. Renaming the scale back restores them. Picking the scale again in the activity-type builder " +
            "and publishing restores it for new activities only, because activities already filed stay on their version.");

        db.ChangeTracker.Clear();
        (await db.Set<EntrustmentScale>().SingleAsync(scale => scale.Id == seeded.Id)).Name.Should().Be("Local Ladder v2");
    }

    [Fact]
    public async Task Update_RenamesWithoutAWarning_AndEveryBindingBySeedKeyOrIdStillResolves()
    {
        // T253's point: the seeds bind by seed key and the builder by id, so a rename unbinds neither.
        await using var db = CreateDb();
        var ladder = await AddSeededScaleAsync(db, "cpsa:scale:v11.1", "CPSA Paediatric Entrustment Scale v11.1");
        AddPublishedType(db, 17, "mini_cex_cpsa", "Mini-CEX (CPSA)", (1, "seed:cpsa:scale:v11.1"));
        AddPublishedType(db, 30, "local_mini_cex", "Local Mini-CEX", (1, IdKey(ladder.Id)));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await new UpdateEntrustmentScaleCommandHandler(db).Handle(
            new UpdateEntrustmentScaleCommand(
                ladder.Id,
                "Paediatric ladder",
                null,
                ladder.Levels.Select(level => new EntrustmentLevelUpdate(level.Id, level.Order, level.Label, level.Description)).ToList(),
                TestPrincipals.Administrator()),
            CancellationToken.None);

        result.RenameWarning.Should().BeNull();
        (await EntrustmentScaleBindings.ResolveAsync(db, ["seed:cpsa:scale:v11.1", IdKey(ladder.Id), "CPSA Paediatric Entrustment Scale v11.1"]))
            .Should().BeEquivalentTo(new Dictionary<string, int>
            {
                ["seed:cpsa:scale:v11.1"] = ladder.Id,
                [IdKey(ladder.Id)] = ladder.Id
            }, "the old name binds nothing now, and the seed key and id bind as before");
    }

    [Theory]
    [InlineData("12")]
    [InlineData(" 007 ")]
    [InlineData("seed:cpsa:scale:v11.1")]
    [InlineData("seed:")]
    public void BothValidators_RefuseANameAScaleKeyWouldReadAsAnIdOrASeedKey(string name)
    {
        // T253: the three forms of a scale_key are told apart by the string alone, which holds only while no scale is
        // named like an id or a seed key.
        var levels = new[] { new EntrustmentLevelInput(1, "Low", null), new EntrustmentLevelInput(2, "High", null) };
        new CreateEntrustmentScaleCommandValidator()
            .Validate(new CreateEntrustmentScaleCommand(name, null, levels, TestPrincipals.Administrator()))
            .Errors.Select(error => error.ErrorMessage).Should().Equal(EntrustmentScaleBindings.ReservedNameRefusal);

        var updates = new[] { new EntrustmentLevelUpdate(1, 1, "Low", null), new EntrustmentLevelUpdate(2, 2, "High", null) };
        new UpdateEntrustmentScaleCommandValidator()
            .Validate(new UpdateEntrustmentScaleCommand(1, name, null, updates, TestPrincipals.Administrator()))
            .Errors.Select(error => error.ErrorMessage).Should().Equal(EntrustmentScaleBindings.ReservedNameRefusal);
    }

    [Theory]
    [InlineData("CPSA Paediatric Entrustment Scale v11.1")]
    [InlineData("Seed: a local ladder")]
    [InlineData("-1")]
    [InlineData("Level 5")]
    public void BothValidators_AcceptANameAScaleKeyReadsAsAName(string name)
    {
        var levels = new[] { new EntrustmentLevelInput(1, "Low", null), new EntrustmentLevelInput(2, "High", null) };
        new CreateEntrustmentScaleCommandValidator()
            .Validate(new CreateEntrustmentScaleCommand(name, null, levels, TestPrincipals.Administrator()))
            .IsValid.Should().BeTrue();

        var updates = new[] { new EntrustmentLevelUpdate(1, 1, "Low", null), new EntrustmentLevelUpdate(2, 2, "High", null) };
        new UpdateEntrustmentScaleCommandValidator()
            .Validate(new UpdateEntrustmentScaleCommand(1, name, null, updates, TestPrincipals.Administrator()))
            .IsValid.Should().BeTrue();
    }

    private static string IdKey(int scaleId) => scaleId.ToString(CultureInfo.InvariantCulture);

    /// <summary>A scale as a seeder makes it: with a seed key, which no command writes.</summary>
    private static async Task<EntrustmentScaleDto> AddSeededScaleAsync(ApplicationDbContext db, string seedKey, string name)
    {
        var scale = new EntrustmentScale
        {
            SeedKey = seedKey,
            Name = name,
            Levels = [new EntrustmentLevel { Order = 1, Label = "Low" }, new EntrustmentLevel { Order = 2, Label = "High" }]
        };
        db.Set<EntrustmentScale>().Add(scale);
        await db.SaveChangesAsync();

        return new EntrustmentScaleDto(
            scale.Id,
            scale.Name,
            scale.Description,
            scale.Levels.OrderBy(level => level.Order)
                .Select(level => new EntrustmentLevelDto(level.Id, level.Order, level.Label, level.Description))
                .ToList());
    }

    /// <summary>
    /// A published activity type, one version per entry, each version's single rating field bound by that entry's
    /// <c>scale_key</c>.
    /// </summary>
    private static void AddPublishedType(
        ApplicationDbContext db, int typeId, string key, string name, params (int Version, string ScaleKey)[] versions)
    {
        db.Set<ActivityType>().Add(new ActivityType
        {
            Id = typeId,
            Key = key,
            Name = name,
            Version = versions.Max(version => version.Version),
            IsActive = true,
            OwnerUserId = "admin",
            CreatedOn = DateTime.UtcNow,
            Scope = ActivityScope.Global
        });

        foreach (var (version, scaleKey) in versions)
        {
            db.Set<ActivityTypeVersion>().Add(new ActivityTypeVersion
            {
                ActivityTypeId = typeId,
                Version = version,
                SchemaJson = $$"""
                    {
                      "version": 1,
                      "sections": [
                        {
                          "key": "assessment",
                          "title": "Entrustment",
                          "fields": [
                            { "key": "overall_level", "type": "scale", "label": "Level", "scale_key": "{{scaleKey}}" }
                          ]
                        }
                      ]
                    }
                    """,
                WorkflowJson = "{}",
                CreditRulesJson = "{}"
            });
        }
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
    public async Task Update_RefusedForARungAPinnedItemRequires_CommitsNeitherTheRenameNorAnyLevelEdit()
    {
        // T253 review, the audit trap. The audit pipeline's catch saves the request's own context, so whatever the handler
        // assigned before a check threw is committed with the failure row. The name used to be assigned before the level
        // checks: this save, refused for removing rung 3, still renamed the scale, and since a rename refuses nothing and
        // its warning rides only on a successful result, the form bound by the old name lost its ladder unannounced.
        await using var db = CreateDb();
        var seeded = await new CreateEntrustmentScaleCommandHandler(db).Handle(
            new CreateEntrustmentScaleCommand(
                "Local Ladder",
                "Three rungs.",
                [
                    new EntrustmentLevelInput(1, "Low", null),
                    new EntrustmentLevelInput(2, "Middle", null),
                    new EntrustmentLevelInput(3, "High", null)
                ],
                TestPrincipals.Administrator()),
            CancellationToken.None);
        db.Set<CurriculumItem>().Add(new CurriculumItem
        {
            Id = 4,
            CurriculumId = 3000,
            EpaId = 5000,
            RequiredCount = 1,
            MinimumLevelOrder = 3,
            WindowMonths = 12,
            ScaleId = seeded.Id
        });
        AddPublishedType(db, 9, "local_cbd", "Local CbD", (1, "Local Ladder"));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var levelsById = seeded.Levels.ToDictionary(level => level.Order, level => level.Id);
        var action = async () => await new UpdateEntrustmentScaleCommandHandler(db).Handle(
            new UpdateEntrustmentScaleCommand(
                seeded.Id,
                "Local Ladder v2",
                "Two rungs now.",
                [
                    new EntrustmentLevelUpdate(levelsById[1], 1, "Low (relabelled)", null),
                    new EntrustmentLevelUpdate(levelsById[2], 2, "Middle", null)
                ],
                TestPrincipals.Administrator()),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*requires level 3*");

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var stored = await db.Set<EntrustmentScale>().Include(scale => scale.Levels).SingleAsync(scale => scale.Id == seeded.Id);
        stored.Name.Should().Be("Local Ladder", "the refused save's rename was not committed by the failure row");
        stored.Description.Should().Be("Three rungs.");
        stored.Levels.OrderBy(level => level.Order).Select(level => level.Label)
            .Should().Equal(["Low", "Middle", "High"], "no level was removed or relabelled ahead of the refusal");
        (await EntrustmentScaleBindings.ResolveAsync(db, ["Local Ladder"]))
            .Should().ContainKey("Local Ladder", "the form bound by the old name still has its ladder");
    }

    [Fact]
    public async Task Update_NamingALevelThatIsNotOnTheScale_IsRefusedBeforeAnythingChanges()
    {
        // The same trap, by the third check: a level id the scale does not hold (another administrator removed it since
        // the page loaded, say). The name, the description and the removal of the level the save left out used to be
        // done before it was asked.
        await using var db = CreateDb();
        var seeded = await SeedTwoLevelScaleAsync(db, "Local Ladder");
        var other = await SeedTwoLevelScaleAsync(db, "Another ladder");
        db.ChangeTracker.Clear();

        var foreignLevelId = other.Levels[1].Id;
        var action = async () => await new UpdateEntrustmentScaleCommandHandler(db).Handle(
            new UpdateEntrustmentScaleCommand(
                seeded.Id,
                "Local Ladder v2",
                "Edited.",
                [
                    new EntrustmentLevelUpdate(seeded.Levels[0].Id, 1, "Low", null),
                    new EntrustmentLevelUpdate(foreignLevelId, 2, "Borrowed", null)
                ],
                TestPrincipals.Administrator()),
            CancellationToken.None);

        (await action.Should().ThrowAsync<InvalidOperationException>()).Which.Message
            .Should().Be($"Level {foreignLevelId} was not found on scale {seeded.Id}.");

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var stored = await db.Set<EntrustmentScale>().Include(scale => scale.Levels).SingleAsync(scale => scale.Id == seeded.Id);
        stored.Name.Should().Be("Local Ladder");
        stored.Description.Should().BeNull();
        stored.Levels.OrderBy(level => level.Order).Select(level => level.Label).Should().Equal("Low", "High");
        (await db.Set<EntrustmentLevel>().SingleAsync(level => level.Id == foreignLevelId)).Label.Should().Be("High");
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

        result.Scale.Name.Should().Be("Renamed freely");
        result.RenameWarning.Should().BeNull();
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
