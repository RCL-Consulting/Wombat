using System.Text.Json.Nodes;
using Wombat.Domain.Activities.Schema;
using SchemaField = Wombat.Domain.Activities.Schema.FormField;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// The seeded activity types' files, as the build copies them next to this assembly, for the tests that hold every
/// seeded form to a rendering rule (T177's labels, T193's help text).
/// </summary>
internal static class SeedSchemas
{
    /// <summary>Every seed folder that holds a form schema, in key order.</summary>
    public static IReadOnlyList<string> Keys()
        => Directory.EnumerateDirectories(SeedsFolder())
            .Select(Path.GetFileName)
            .Select(seedKey => seedKey!)
            .Where(seedKey => File.Exists(PathOf(seedKey, "schema.json")))
            .Order(StringComparer.Ordinal)
            .ToList();

    public static TheoryData<string> KeysAsTheoryData()
    {
        var data = new TheoryData<string>();
        foreach (var seedKey in Keys())
        {
            data.Add(seedKey);
        }

        return data;
    }

    public static string Schema(string seedKey) => File.ReadAllText(PathOf(seedKey, "schema.json"));

    /// <summary>The seed's credit rules, or null for a seed that declares none.</summary>
    public static string? CreditRules(string seedKey)
    {
        var path = PathOf(seedKey, "credit.json");
        return File.Exists(path) ? File.ReadAllText(path) : null;
    }

    public static IReadOnlyList<SchemaField> Fields(string seedKey)
        => FormSchemaParser.Parse(Schema(seedKey)).Sections.SelectMany(section => section.Fields).ToList();

    /// <summary>
    /// The schema with every <c>show_if</c> removed. What is checked is how a field renders, not whether it is visible,
    /// and a field a condition hides from an empty form would otherwise escape the check.
    /// </summary>
    public static string EveryFieldVisible(string schemaJson)
    {
        var root = JsonNode.Parse(schemaJson)!.AsObject();
        foreach (var section in root["sections"]!.AsArray().Select(node => node!.AsObject()))
        {
            section.Remove("show_if");
            foreach (var field in section["fields"]!.AsArray().Select(node => node!.AsObject()))
            {
                field.Remove("show_if");
            }
        }

        return root.ToJsonString();
    }

    private static string SeedsFolder() => Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds");

    private static string PathOf(string seedKey, string file) => Path.Combine(SeedsFolder(), seedKey, file);
}
