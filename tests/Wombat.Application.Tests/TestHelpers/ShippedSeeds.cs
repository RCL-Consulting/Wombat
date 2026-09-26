using Wombat.Domain.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.TestHelpers;

/// <summary>
/// The shipped seed folders (<c>Activities/Seeds/&lt;key&gt;</c>, copied into the test output), for tests that read
/// "waiting" off the workflows the programme really runs (T297), rather than off a hand-copied one.
/// </summary>
internal static class ShippedSeeds
{
    private static readonly DateTime PublishedOn = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static string Folder => Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds");

    /// <summary>Every seed key that ships a workflow, in ordinal order.</summary>
    public static IReadOnlyList<string> Keys()
        => Directory.GetDirectories(Folder)
            .Where(directory => File.Exists(Path.Combine(directory, "workflow.json")))
            .Select(directory => Path.GetFileName(directory)!)
            .OrderBy(key => key, StringComparer.Ordinal)
            .ToList();

    public static string Workflow(string key) => File.ReadAllText(Path.Combine(Folder, key, "workflow.json"));

    /// <summary>
    /// Adds the type at version 1, with the version row a publish writes carrying the seed's workflow: the version every
    /// move on an activity pinned to it is judged against. Adds only; the caller saves.
    /// </summary>
    public static ActivityType AddType(
        ApplicationDbContext db,
        int id,
        string key,
        string? name = null,
        ActivityScope scope = ActivityScope.Global)
    {
        var workflowJson = Workflow(key);
        var type = new ActivityType
        {
            Id = id,
            Key = key,
            Name = name ?? key,
            Scope = scope,
            Version = 1,
            IsActive = true,
            SchemaJson = "{}",
            WorkflowJson = workflowJson,
            CreditRulesJson = """{ "counts_for": [] }""",
            OwnerUserId = "system",
            CreatedOn = PublishedOn
        };
        type.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = id,
            Version = 1,
            SchemaJson = "{}",
            WorkflowJson = workflowJson,
            CreditRulesJson = """{ "counts_for": [] }""",
            PublishedByUserId = "system",
            PublishedOn = PublishedOn
        });
        db.ActivityTypes.Add(type);
        return type;
    }
}
