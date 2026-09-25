using System.Globalization;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Commands.PublishActivityTypeDraft;
using Wombat.Application.Features.Epas.Commands.CreateEntrustmentScale;
using Wombat.Application.Features.Epas.Commands.DeleteEntrustmentScale;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Epas;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Activities;

/// <summary>
/// T253 review, finding 2. A draft's <c>scale_key</c> must bind a scale for the draft to be published.
/// </summary>
/// <remarks>
/// The scale's delete refuses while a published version binds the scale, but a draft is not published: an administrator
/// picks a scale in the builder, saves the draft, the scale is deleted, and the draft is published. Before this the publish
/// went through, and the new version's rating field bound nothing from its first activity on.
/// </remarks>
public sealed class PublishScaleBindingTests
{
    private const string Workflow =
        """{"version":1,"initial_state":"logged","states":[{"key":"logged","label":"Logged"}],"transitions":[]}""";

    [Fact]
    public async Task ADraftBindingAScaleDeletedSinceItWasSaved_IsRefused_NamingTheField_AndNothingIsPublished()
    {
        await using var db = CreateDb();
        var scale = await new CreateEntrustmentScaleCommandHandler(db).Handle(
            new CreateEntrustmentScaleCommand(
                "Local ladder",
                null,
                [new EntrustmentLevelInput(1, "Low", null), new EntrustmentLevelInput(2, "High", null)],
                TestPrincipals.Administrator()),
            CancellationToken.None);
        var idKey = scale.Id.ToString(CultureInfo.InvariantCulture);

        // As the builder saves it: the field's scale_key is the scale's id, in a draft that was never published.
        AddDraftType(db, 40, "local_mini_cex", ("overall_level", "Overall level", idKey));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        // Nothing published binds the scale, so the delete is allowed.
        await new DeleteEntrustmentScaleCommandHandler(db).Handle(
            new DeleteEntrustmentScaleCommand(scale.Id, TestPrincipals.Administrator()), CancellationToken.None);
        db.ChangeTracker.Clear();

        var publish = async () => await new PublishActivityTypeDraftCommandHandler(db).Handle(
            new PublishActivityTypeDraftCommand(40, "admin-user", TestPrincipals.Administrator()), CancellationToken.None);

        (await publish.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(
            $"The field \"Overall level\" (overall_level, scale_key \"{idKey}\") is bound to an entrustment scale that " +
            "does not exist, so this draft cannot be published: the field would have no ladder to rate on. Pick its " +
            "entrustment scale again, save the draft, then publish.");

        // The audit trap: the failure row is saved through this same context.
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var stored = await db.Set<ActivityType>().Include(type => type.Versions).SingleAsync(type => type.Id == 40);
        stored.Version.Should().Be(0, "nothing was published");
        stored.Versions.Should().BeEmpty();
        stored.SchemaJson.Should().BeNull();
        stored.HasDraft.Should().BeTrue("the draft is still there to repair");
    }

    [Fact]
    public async Task EveryFieldWhoseKeyBindsNothing_IsNamed_InEachOfTheThreeForms()
    {
        await using var db = CreateDb();
        AddSeededScale(db, "cpsa:scale:v11.1", "CPSA Paediatric Entrustment Scale v11.1");
        AddDraftType(
            db,
            41,
            "local_dops",
            ("bound_by_seed_key", "Bound", "seed:cpsa:scale:v11.1"),
            ("gone_by_id", "Technique", "9999"),
            ("gone_by_seed_key", "Consent", "seed:nobody:scale"),
            ("gone_by_name", "Aseptic technique", "Ghost ladder"));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var publish = async () => await new PublishActivityTypeDraftCommandHandler(db).Handle(
            new PublishActivityTypeDraftCommand(41, "admin-user", TestPrincipals.Administrator()), CancellationToken.None);

        (await publish.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(
            "The fields \"Technique\" (gone_by_id, scale_key \"9999\"), \"Consent\" (gone_by_seed_key, scale_key " +
            "\"seed:nobody:scale\") and \"Aseptic technique\" (gone_by_name, scale_key \"Ghost ladder\") are bound to " +
            "entrustment scales that do not exist, so this draft cannot be published: those fields would have no ladder " +
            "to rate on. Pick each one's entrustment scale again, save the draft, then publish.");
    }

    [Fact]
    public async Task ADraftWhoseEveryKeyBindsAScale_IsPublished_AndAScaleFieldBoundToNoScaleIsNotAskedAbout()
    {
        await using var db = CreateDb();
        var ladder = AddSeededScale(db, "cpsa:scale:v11.1", "CPSA Paediatric Entrustment Scale v11.1");
        var local = AddSeededScale(db, seedKey: null, "Local ladder");
        await db.SaveChangesAsync();

        AddDraftType(
            db,
            42,
            "local_cbd",
            ("by_seed_key", "By seed key", "seed:cpsa:scale:v11.1"),
            ("by_id", "By id", ladder.Id.ToString(CultureInfo.InvariantCulture)),
            ("by_name", "By name", "Local ladder"),
            ("unbound", "Unbound", null));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var editor = await new PublishActivityTypeDraftCommandHandler(db).Handle(
            new PublishActivityTypeDraftCommand(42, "admin-user", TestPrincipals.Administrator()), CancellationToken.None);

        editor.PublishedVersion.Should().Be(1);
        local.Id.Should().BePositive("guard: the name binds a real scale");
    }

    /// <summary>A type with only a saved draft, one scale field per entry, each bound by that entry's key (none if null).</summary>
    private static void AddDraftType(
        ApplicationDbContext db, int typeId, string key, params (string Key, string Label, string? ScaleKey)[] fields)
    {
        var fieldJson = string.Join(
            ",",
            fields.Select(field => field.ScaleKey is null
                ? $$"""{"key":"{{field.Key}}","type":"scale","label":"{{field.Label}}"}"""
                : $$"""{"key":"{{field.Key}}","type":"scale","label":"{{field.Label}}","scale_key":"{{field.ScaleKey}}"}"""));

        var type = new ActivityType
        {
            Id = typeId,
            Key = key,
            Name = key,
            IsActive = true,
            OwnerUserId = "admin-user",
            CreatedOn = DateTime.UtcNow,
            Scope = ActivityScope.Global
        };
        type.SaveDraft(
            $$"""{"version":1,"sections":[{"key":"assessment","title":"Assessment","fields":[{{fieldJson}}]}]}""",
            Workflow,
            """{"counts_for":[]}""",
            "[]",
            "admin-user");

        db.Set<ActivityType>().Add(type);
    }

    /// <summary>A scale as a seeder makes it (with a seed key, which no command writes), or with none.</summary>
    private static EntrustmentScale AddSeededScale(ApplicationDbContext db, string? seedKey, string name)
    {
        var scale = new EntrustmentScale
        {
            SeedKey = seedKey,
            Name = name,
            Levels = [new EntrustmentLevel { Order = 1, Label = "Low" }, new EntrustmentLevel { Order = 2, Label = "High" }]
        };
        db.Set<EntrustmentScale>().Add(scale);
        return scale;
    }

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
