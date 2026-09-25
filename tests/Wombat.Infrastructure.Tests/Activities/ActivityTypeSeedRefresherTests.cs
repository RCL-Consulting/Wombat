using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Options;
using Wombat.Domain.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.Activities;

/// <summary>
/// T103: the refresher carries seed-folder edits into activity types that already exist.
/// </summary>
/// <remarks>
/// Idempotency is the whole ballgame here, and the trap is that the four JSON columns are
/// <c>jsonb</c>: PostgreSQL parses what it is given, throws the bytes away, and renders its own text
/// on read — keys reordered by length then bytewise, <c>", "</c> and <c>": "</c> separators inserted.
/// The in-memory provider these tests run on stores the string verbatim and does NOT reorder, so a
/// naive raw-string comparison would pass here while bumping the version on every production boot,
/// forever. <see cref="Refresh_IsIdempotent_WhenStoredJsonIsRenderedTheWayPostgresRendersJsonb"/>
/// closes that hole by writing the PostgreSQL rendering into the column deliberately.
/// </remarks>
public sealed class ActivityTypeSeedRefresherTests
{
    private const string SeedActor = ActivityTypeSeedCatalogue.SeedActorUserId;
    private const string OperatorActor = "b5b89012-46f6-4cfd-9057-105dfa8755ac";

    private const string StaleSchemaJson =
        """{"version":1,"sections":[{"key":"detail","title":"Detail","fields":[{"key":"note","type":"text","label":"An older label"}]}]}""";

    private const string StaleWorkflowJson =
        """{"version":1,"initial_state":"logged","states":[{"key":"logged","label":"Logged"}],"transitions":[]}""";

    private const string StaleCreditJson = """{"counts_for":[]}""";

    [Fact]
    public void Catalogue_CoversEverySeedFolderOnDisk()
    {
        var onDisk = Directory
            .EnumerateDirectories(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds"))
            .Select(Path.GetFileName)
            .Order(StringComparer.Ordinal);

        ActivityTypeSeedCatalogue.Entries
            .Select(entry => entry.Key)
            .Order(StringComparer.Ordinal)
            .Should()
            .Equal(onDisk, "the refresher is driven by the catalogue, so a seed folder missing from it would never be refreshed");
    }

    [Fact]
    public async Task Refresh_RepublishesASeededTypeWhoseSeedFolderHasChanged()
    {
        await using var dbContext = CreateDbContext();
        dbContext.ActivityTypes.Add(CreateStoredType("mini_cex"));
        await dbContext.SaveChangesAsync();

        var results = await CreateRefresher(dbContext).RefreshAsync();

        var stored = await LoadAsync(dbContext, "mini_cex");
        var desired = await ActivityTypeSeedCatalogue.ReadCanonicalAsync(Entry("mini_cex"), CancellationToken.None);

        stored.Version.Should().Be(2);
        stored.SchemaJson.Should().Be(desired.SchemaJson);
        stored.WorkflowJson.Should().Be(desired.WorkflowJson);
        stored.CreditRulesJson.Should().Be(desired.CreditRulesJson);
        stored.DisplayFieldsJson.Should().Be(desired.DisplayFieldsJson);
        stored.HasDraft.Should().BeFalse("PublishDraft clears the staging columns");

        Outcome(results, "mini_cex").Should().Be(ActivityTypeSeedRefreshOutcome.Republished);
    }

    [Fact]
    public async Task Refresh_LeavesTheOldVersionInPlaceSoInFlightActivitiesStayPinned()
    {
        await using var dbContext = CreateDbContext();
        dbContext.ActivityTypes.Add(CreateStoredType("mini_cex"));
        await dbContext.SaveChangesAsync();

        await CreateRefresher(dbContext).RefreshAsync();

        var stored = await LoadAsync(dbContext, "mini_cex");

        stored.Versions.Should().HaveCount(2);

        var pinned = stored.Versions.Single(version => version.Version == 1);
        pinned.SchemaJson.Should().Be(
            ActivityTypeSeedCatalogue.Canonicalise(StaleSchemaJson, StaleWorkflowJson, StaleCreditJson, "[]").SchemaJson,
            "an activity created against v1 must keep validating against exactly the v1 it was created against");

        stored.Versions.Should().Contain(
            version => version.Version == stored.Version,
            "a republish must never leave ActivityService.GetPinnedVersion without a row for the current version");
    }

    [Fact]
    public async Task Refresh_IsIdempotent_ASecondRunPublishesNothing()
    {
        await using var dbContext = CreateDbContext();
        dbContext.ActivityTypes.Add(CreateStoredType("mini_cex"));
        await dbContext.SaveChangesAsync();

        var refresher = CreateRefresher(dbContext);

        await refresher.RefreshAsync();
        var versionAfterFirstRun = (await LoadAsync(dbContext, "mini_cex")).Version;

        var secondResults = await refresher.RefreshAsync();
        var stored = await LoadAsync(dbContext, "mini_cex");

        versionAfterFirstRun.Should().Be(2);
        stored.Version.Should().Be(versionAfterFirstRun, "an unchanged seed folder must not bump the version");
        stored.Versions.Should().HaveCount(2);
        Outcome(secondResults, "mini_cex").Should().Be(ActivityTypeSeedRefreshOutcome.Unchanged);
    }

    /// <summary>
    /// The production failure mode, reproduced without a database: the stored bytes are never the
    /// bytes that were written, because <c>jsonb</c> re-renders them.
    /// </summary>
    [Fact]
    public async Task Refresh_IsIdempotent_WhenStoredJsonIsRenderedTheWayPostgresRendersJsonb()
    {
        var entry = Entry("mini_cex");
        var desired = await ActivityTypeSeedCatalogue.ReadCanonicalAsync(entry, CancellationToken.None);

        await using var dbContext = CreateDbContext();
        var activityType = CreateStoredType("mini_cex");

        // Exactly what PostgreSQL would hand back for a jsonb column holding the canonical value.
        activityType.SchemaJson = RenderAsPostgresJsonb(desired.SchemaJson);
        activityType.WorkflowJson = RenderAsPostgresJsonb(desired.WorkflowJson);
        activityType.CreditRulesJson = RenderAsPostgresJsonb(desired.CreditRulesJson);
        activityType.DisplayFieldsJson = RenderAsPostgresJsonb(desired.DisplayFieldsJson);
        dbContext.ActivityTypes.Add(activityType);
        await dbContext.SaveChangesAsync();

        activityType.SchemaJson.Should().NotBe(desired.SchemaJson, "the jsonb rendering must differ byte-for-byte, or this test proves nothing");

        var results = await CreateRefresher(dbContext).RefreshAsync();

        var stored = await LoadAsync(dbContext, "mini_cex");
        stored.Version.Should().Be(1, "a raw-string comparison would bump the version on every boot, forever");
        Outcome(results, "mini_cex").Should().Be(ActivityTypeSeedRefreshOutcome.Unchanged);
    }

    [Fact]
    public async Task Refresh_AfterTheSeedersRun_ReportsEveryGenericTypeUnchanged()
    {
        await using var dbContext = CreateDbContext();
        await new DataSeeder(dbContext).SeedAsync();

        var results = await CreateRefresher(dbContext).RefreshAsync();

        dbContext.ActivityTypes.Should().OnlyContain(activityType => activityType.Version == 1);
        dbContext.Set<ActivityTypeVersion>().Should().HaveCount(10);

        results
            .Where(result => ActivityTypeSeedCatalogue.For(ActivityTypeSeedSource.Generic).Any(entry => entry.Key == result.Key))
            .Should()
            .OnlyContain(result => result.Outcome == ActivityTypeSeedRefreshOutcome.Unchanged);

        results
            .Where(result => ActivityTypeSeedCatalogue.For(ActivityTypeSeedSource.PaediatricCollege).Any(entry => entry.Key == result.Key))
            .Should()
            .OnlyContain(result => result.Outcome == ActivityTypeSeedRefreshOutcome.NotSeededYet);
    }

    [Fact]
    public async Task Refresh_SkipsATypeOwnedByAnOperator()
    {
        await using var dbContext = CreateDbContext();
        dbContext.ActivityTypes.Add(CreateStoredType("mini_cex", ownerUserId: OperatorActor));
        await dbContext.SaveChangesAsync();

        var results = await CreateRefresher(dbContext).RefreshAsync();

        var stored = await LoadAsync(dbContext, "mini_cex");
        stored.Version.Should().Be(1);
        stored.SchemaJson.Should().Contain("An older label");
        Outcome(results, "mini_cex").Should().Be(ActivityTypeSeedRefreshOutcome.SkippedOperatorOwned);
        Detail(results, "mini_cex").Should().Contain(OperatorActor);
    }

    /// <summary>
    /// The guard that actually catches operator customisation. <c>SaveActivityTypeDraftCommand</c>
    /// sets <c>OwnerUserId</c> only when creating a type, so an admin who customises and publishes a
    /// seeded one leaves it reading as seed-owned forever. <c>PublishedByUserId</c> on the newest
    /// version is the honest signal.
    /// </summary>
    [Fact]
    public async Task Refresh_SkipsATypeWhoseNewestVersionWasPublishedByAnOperator()
    {
        await using var dbContext = CreateDbContext();
        dbContext.ActivityTypes.Add(CreateStoredType("mini_cex", publishedByUserId: OperatorActor));
        await dbContext.SaveChangesAsync();

        var results = await CreateRefresher(dbContext).RefreshAsync();

        var stored = await LoadAsync(dbContext, "mini_cex");
        stored.Version.Should().Be(1);
        stored.OwnerUserId.Should().Be(SeedActor, "the builder's edit path never reassigns OwnerUserId — that is the point of this test");
        Outcome(results, "mini_cex").Should().Be(ActivityTypeSeedRefreshOutcome.SkippedOperatorPublished);
    }

    [Fact]
    public async Task Refresh_SkipsATypeWithADraftInFlightAndLeavesTheDraftIntact()
    {
        await using var dbContext = CreateDbContext();
        var activityType = CreateStoredType("mini_cex");
        activityType.SaveDraft(StaleSchemaJson, StaleWorkflowJson, StaleCreditJson, """["note"]""", OperatorActor);
        dbContext.ActivityTypes.Add(activityType);
        await dbContext.SaveChangesAsync();

        var results = await CreateRefresher(dbContext).RefreshAsync();

        var stored = await LoadAsync(dbContext, "mini_cex");
        stored.Version.Should().Be(1);
        stored.HasDraft.Should().BeTrue();
        stored.StagingUpdatedByUserId.Should().Be(OperatorActor);
        stored.StagingSchemaJson.Should().Contain("An older label", "SaveDraft overwrites all six staging columns, so the guard has to run before it");
        Outcome(results, "mini_cex").Should().Be(ActivityTypeSeedRefreshOutcome.SkippedDraftInFlight);
    }

    [Fact]
    public async Task Refresh_SkipsATypeWhoseCurrentVersionHasNoVersionRow()
    {
        await using var dbContext = CreateDbContext();
        dbContext.ActivityTypes.Add(new ActivityType
        {
            Key = "mini_cex",
            Name = "Mini-CEX",
            Scope = ActivityScope.Speciality,
            ScopeId = 1,
            OwnerUserId = SeedActor,
            CreatedOn = DateTime.UtcNow,
            IsActive = true,
            SchemaJson = StaleSchemaJson,
            WorkflowJson = StaleWorkflowJson,
            CreditRulesJson = StaleCreditJson,
            DisplayFieldsJson = """["note"]""",
            Version = 1
        });
        await dbContext.SaveChangesAsync();

        var results = await CreateRefresher(dbContext).RefreshAsync();

        var stored = await LoadAsync(dbContext, "mini_cex");
        stored.Version.Should().Be(1, "bumping would append v2 and leave activities pinned to v1 exactly as broken");
        stored.Versions.Should().BeEmpty();
        Outcome(results, "mini_cex").Should().Be(ActivityTypeSeedRefreshOutcome.SkippedMissingVersionRow);
    }

    [Fact]
    public async Task Refresh_WhenDisabled_ReportsTheDifferenceWithoutWriting()
    {
        await using var dbContext = CreateDbContext();
        dbContext.ActivityTypes.Add(CreateStoredType("mini_cex"));
        await dbContext.SaveChangesAsync();

        var results = await CreateRefresher(dbContext, refreshEnabled: false).RefreshAsync();

        var stored = await LoadAsync(dbContext, "mini_cex");
        stored.Version.Should().Be(1);
        stored.SchemaJson.Should().Contain("An older label");
        Outcome(results, "mini_cex").Should().Be(ActivityTypeSeedRefreshOutcome.ReportedOnly);
        Detail(results, "mini_cex").Should().Contain("schema");
    }

    [Fact]
    public async Task Refresh_WritesAnAuditEntryForEachRepublish()
    {
        await using var dbContext = CreateDbContext();
        dbContext.ActivityTypes.Add(CreateStoredType("mini_cex"));
        await dbContext.SaveChangesAsync();

        await CreateRefresher(dbContext).RefreshAsync();

        var entry = dbContext.AuditEntries.Single();
        entry.Action.Should().Be("ActivityTypeSeedRefresh");
        entry.ActorUserId.Should().Be(SeedActor);
        entry.SummaryJson.Should().Contain("mini_cex").And.Contain("\"toVersion\":2");
    }

    [Fact]
    public async Task Refresh_AppliesEachSeedersOwnDisplayFieldsRule()
    {
        await using var dbContext = CreateDbContext();
        dbContext.ActivityTypes.Add(CreateStoredType("mini_cex"));
        dbContext.ActivityTypes.Add(CreateStoredType("mini_cex_cpsa"));
        await dbContext.SaveChangesAsync();

        await CreateRefresher(dbContext).RefreshAsync();

        var generic = await LoadAsync(dbContext, "mini_cex");
        var cpsa = await LoadAsync(dbContext, "mini_cex_cpsa");

        JsonSerializer.Deserialize<string[]>(generic.DisplayFieldsJson)
            .Should().NotBeNullOrEmpty("DataSeeder derives display fields from the first three schema fields");

        cpsa.DisplayFieldsJson.Should().Be("[]", "PaediatricCatalogueSeeder has always passed an empty array");
    }

    /// <summary>
    /// T253: what the first boot after it republishes, on a database the seeders filled before it. The seeds bound their
    /// scales by name then and by seed key now, so exactly the twelve types with a scale field get a new version, and
    /// the new version binds by seed key. Every other seeded type is unchanged.
    /// </summary>
    [Fact]
    public async Task Refresh_AfterT253_RepublishesExactlyTheTypesThatBoundAScaleByName_BindingItBySeedKey()
    {
        await using var dbContext = CreateDbContext();
        foreach (var entry in ActivityTypeSeedCatalogue.Entries)
        {
            var now = await ActivityTypeSeedCatalogue.ReadCanonicalAsync(entry, CancellationToken.None);
            var beforeT253 = now.SchemaJson
                .Replace("\"seed:cpsa:scale:v11.1\"", "\"CPSA Paediatric Entrustment Scale v11.1\"", StringComparison.Ordinal)
                .Replace("\"seed:demo:scale:o-r\"", "\"O-R Scale\"", StringComparison.Ordinal);

            var activityType = new ActivityType
            {
                Key = entry.Key,
                Name = entry.Key,
                Scope = ActivityScope.Speciality,
                ScopeId = 1,
                OwnerUserId = SeedActor,
                CreatedOn = DateTime.UtcNow,
                IsActive = true
            };
            activityType.SaveDraft(beforeT253, now.WorkflowJson, now.CreditRulesJson, now.DisplayFieldsJson, SeedActor);
            activityType.PublishDraft(SeedActor);
            dbContext.ActivityTypes.Add(activityType);
        }

        await dbContext.SaveChangesAsync();

        var results = await CreateRefresher(dbContext).RefreshAsync();

        results.Where(result => result.Outcome == ActivityTypeSeedRefreshOutcome.Republished)
            .Select(result => result.Key)
            .Order(StringComparer.Ordinal)
            .Should().Equal(
                "acat", "cbd", "cbd_cpsa", "cca_cpsa", "chart_stimulated_recall_cpsa", "direct_observation_cpsa",
                "dops", "dops_cpsa", "mini_cex", "mini_cex_cpsa", "msf_cpsa", "rca_cpsa");
        results.Where(result => result.Outcome != ActivityTypeSeedRefreshOutcome.Republished)
            .Should().OnlyContain(result => result.Outcome == ActivityTypeSeedRefreshOutcome.Unchanged);

        var miniCexCpsa = await LoadAsync(dbContext, "mini_cex_cpsa");
        miniCexCpsa.Version.Should().Be(2);
        miniCexCpsa.Versions.Single(version => version.Version == 2).SchemaJson
            .Should().Contain("\"seed:cpsa:scale:v11.1\"").And.NotContain("CPSA Paediatric Entrustment Scale v11.1");
        miniCexCpsa.Versions.Single(version => version.Version == 1).SchemaJson
            .Should().Contain("\"CPSA Paediatric Entrustment Scale v11.1\"", "v1 is pinned and never changes");
    }

    [Fact]
    public async Task Refresh_DoesNotThrowWhenTheStoredJsonCannotBeParsed()
    {
        await using var dbContext = CreateDbContext();
        var activityType = CreateStoredType("mini_cex");
        activityType.SchemaJson = "{ not json";
        dbContext.ActivityTypes.Add(activityType);
        await dbContext.SaveChangesAsync();

        var results = await CreateRefresher(dbContext).RefreshAsync();

        (await LoadAsync(dbContext, "mini_cex")).Version.Should().Be(1);
        Outcome(results, "mini_cex").Should().Be(ActivityTypeSeedRefreshOutcome.SkippedStoredJsonUnreadable);
        results.Should().HaveCount(ActivityTypeSeedCatalogue.Entries.Count, "one bad type must not stop the other thirteen");
    }

    private static ActivityTypeSeedEntry Entry(string key)
        => ActivityTypeSeedCatalogue.Entries.Single(entry => entry.Key == key);

    private static ActivityTypeSeedRefreshOutcome Outcome(IReadOnlyList<ActivityTypeSeedRefreshResult> results, string key)
        => results.Single(result => result.Key == key).Outcome;

    private static string Detail(IReadOnlyList<ActivityTypeSeedRefreshResult> results, string key)
        => results.Single(result => result.Key == key).Detail;

    private static ActivityType CreateStoredType(
        string key,
        string ownerUserId = SeedActor,
        string publishedByUserId = SeedActor)
    {
        var activityType = new ActivityType
        {
            Key = key,
            Name = key,
            Description = "Stored before the seed folder changed.",
            Scope = ActivityScope.Speciality,
            ScopeId = 1,
            OwnerUserId = ownerUserId,
            CreatedOn = DateTime.UtcNow,
            IsActive = true
        };

        activityType.SaveDraft(StaleSchemaJson, StaleWorkflowJson, StaleCreditJson, "[]", publishedByUserId);
        activityType.PublishDraft(publishedByUserId);
        return activityType;
    }

    private static Task<ActivityType> LoadAsync(ApplicationDbContext dbContext, string key)
        => dbContext.ActivityTypes
            .Include(activityType => activityType.Versions)
            .SingleAsync(activityType => activityType.Key == key);

    private static ActivityTypeSeedRefresher CreateRefresher(ApplicationDbContext dbContext, bool refreshEnabled = true)
        => new(
            dbContext,
            NullLogger<ActivityTypeSeedRefresher>.Instance,
            Options.Create(new WombatOptions { RefreshSeededActivityTypes = refreshEnabled }));

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Mimics how PostgreSQL re-emits a <c>jsonb</c> value: object keys sorted by byte length then
    /// bytewise, and <c>", "</c> / <c>": "</c> separators. Same value, different bytes.
    /// </summary>
    private static string RenderAsPostgresJsonb(string json)
    {
        using var document = JsonDocument.Parse(json);
        var builder = new StringBuilder();
        WriteAsPostgresJsonb(document.RootElement, builder);
        return builder.ToString();
    }

    private static void WriteAsPostgresJsonb(JsonElement element, StringBuilder builder)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                builder.Append('{');
                var firstProperty = true;
                var ordered = element.EnumerateObject()
                    .OrderBy(property => Encoding.UTF8.GetByteCount(property.Name))
                    .ThenBy(property => property.Name, StringComparer.Ordinal);

                foreach (var property in ordered)
                {
                    if (!firstProperty)
                    {
                        builder.Append(", ");
                    }

                    firstProperty = false;
                    builder.Append(JsonSerializer.Serialize(property.Name)).Append(": ");
                    WriteAsPostgresJsonb(property.Value, builder);
                }

                builder.Append('}');
                break;

            case JsonValueKind.Array:
                builder.Append('[');
                var firstItem = true;
                foreach (var item in element.EnumerateArray())
                {
                    if (!firstItem)
                    {
                        builder.Append(", ");
                    }

                    firstItem = false;
                    WriteAsPostgresJsonb(item, builder);
                }

                builder.Append(']');
                break;

            case JsonValueKind.Undefined:
            case JsonValueKind.String:
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
            default:
                builder.Append(element.GetRawText());
                break;
        }
    }
}
