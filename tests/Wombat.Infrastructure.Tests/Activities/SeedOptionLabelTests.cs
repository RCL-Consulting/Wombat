using System.Text.RegularExpressions;
using FluentAssertions;
using Wombat.Domain.Activities.Schema;

namespace Wombat.Infrastructure.Tests.Activities;

/// <summary>
/// T191: no seeded option shows its key. The CCA's "Documentation reviewed" offered <c>admission_notes</c> and
/// <c>progress_notes</c>, and fourteen other seeds did the same, because an option was a bare string and the form printed
/// it. Every option of every seed is held here, as the refresher would publish it, so the next seed cannot bring it back.
/// </summary>
/// <remarks>
/// The seed files are the source: they are what <c>ActivityTypeSeedRefresher</c> canonicalises and publishes. The form
/// itself is held to the same rule over every seed in <c>Wombat.Web.Tests</c> (<c>ActivityFormOptionLabelTests</c>).
/// </remarks>
public sealed partial class SeedOptionLabelTests
{
    /// <summary>A machine key: lower case, digits and underscores, starting with a letter. <c>picu</c> is one; <c>PICU</c> is not.</summary>
    [GeneratedRegex("^[a-z][a-z0-9]*(?:_[a-z0-9]+)*$")]
    private static partial Regex SnakeCaseKey();

    public static TheoryData<string> SeedKeys
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (var seedKey in SeedKeysWithASchema())
            {
                data.Add(seedKey);
            }

            return data;
        }
    }

    [Fact]
    public void TheCorpusOffersLabelledChoices_SoTheTheoriesAreNotVacuous()
    {
        // If the seed folders stopped reaching this project, or every option lost its label, the theories would pass
        // with nothing to check.
        var choiceFields = SeedKeysWithASchema()
            .SelectMany(seedKey => CanonicalFields(seedKey).Select(field => (seedKey, field)))
            .Where(entry => entry.field.Type is FieldType.Choice or FieldType.MultiChoice && entry.field.Options.Count > 0)
            .ToList();

        choiceFields.Should().HaveCountGreaterThanOrEqualTo(25);
        choiceFields.Single(entry => entry.seedKey == "cca_cpsa" && entry.field.Key == "documents_reviewed")
            .field.LabelFor("admission_notes").Should().Be("Admission notes");
    }

    [Theory]
    [MemberData(nameof(SeedKeys))]
    public void NoSeededOption_IsLabelledWithASnakeCaseKey(string seedKey)
    {
        // Every field that offers options, scales included: whatever the form lists, it lists by label. "1" to "5" on
        // an O-R scale are numbers a reader reads as numbers, not keys, and pass.
        var keysShown = CanonicalFields(seedKey)
            .SelectMany(field => field.Options.Select(option => (field.Key, option.Label)))
            .Where(shown => SnakeCaseKey().IsMatch(shown.Label))
            .Select(shown => $"{shown.Key}: {shown.Label}")
            .ToList();

        keysShown.Should().BeEmpty("'{0}' must show every option in words, as a label, never as the key it stores", seedKey);
    }

    [Theory]
    [MemberData(nameof(SeedKeys))]
    public void EverySeededChoice_StillStoresAKey(string seedKey)
    {
        // The fix is a label beside the key, never words in place of it: the value is what every activity already
        // stored, what show_if and the scenario runbooks compare against, and what an export groups by.
        var notKeys = CanonicalFields(seedKey)
            .Where(field => field.Type is FieldType.Choice or FieldType.MultiChoice)
            .SelectMany(field => field.Options.Select(option => (field.Key, option.Value)))
            .Where(stored => !SnakeCaseKey().IsMatch(stored.Value))
            .Select(stored => $"{stored.Key}: {stored.Value}")
            .ToList();

        notKeys.Should().BeEmpty("'{0}' must keep storing keys; a label changes only what the form shows", seedKey);
    }

    /// <summary>The seed's fields as the refresher publishes them: parsed, serialised and parsed again.</summary>
    private static IReadOnlyList<FormField> CanonicalFields(string seedKey)
    {
        var raw = File.ReadAllText(SchemaPath(seedKey));
        var canonical = FormSchemaParser.Parse(FormSchemaParser.Serialize(FormSchemaParser.Parse(raw)));
        return canonical.Sections.SelectMany(section => section.Fields).ToList();
    }

    private static IEnumerable<string> SeedKeysWithASchema()
        => Directory.EnumerateDirectories(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds"))
            .Select(directory => Path.GetFileName(directory)!)
            .Where(seedKey => File.Exists(SchemaPath(seedKey)))
            .Order(StringComparer.Ordinal)
            .ToList();

    private static string SchemaPath(string seedKey)
        => Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", seedKey, "schema.json");
}
