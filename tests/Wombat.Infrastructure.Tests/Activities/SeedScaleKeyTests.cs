using System.Text.Json;
using FluentAssertions;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities.Schema;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.Activities;

/// <summary>
/// Every <c>scale_key</c> in the seed corpus must name a scale the product actually seeds. (T110)
/// </summary>
/// <remarks>
/// <para>
/// A schema binds a rating field to an entrustment ladder by the scale's <b>exact name</b> — there is
/// no key column to bind to. Nothing compared the two, and for months <c>mini_cex</c>, <c>cbd</c>,
/// <c>dops</c> and <c>acat</c> all declared <c>"scale_key": "or_scale"</c> while the only seeded scale
/// was named <c>"O-R Scale"</c>. Twenty-three fields across four tools bound to nothing at all.
/// </para>
/// <para>
/// It was invisible because those same fields declare inline <c>options</c>, and the form renders the
/// picker from those first — so the rung list looked perfectly healthy while the binding behind it had
/// never once resolved. The four <c>_cpsa</c> tools escaped only because
/// <c>CpsaWbaSeedTests</c> pinned their scale name; there was no equivalent guard for the rest, and
/// <b>that absence is the defect this file exists to close</b>, not the typo it found.
/// </para>
/// <para>
/// Names only, never a raw numeric id. The resolver accepts an id, but an id is a fact about one
/// database — a seed that shipped <c>"2"</c> would bind to whatever scale happened to be second
/// wherever it was restored. Four operator-built <c>*_paed</c> types did exactly that until the dev
/// database was rebuilt on 2026-09-20; they were never seeds, and nothing outside a seed folder is
/// covered here.
/// </para>
/// </remarks>
public sealed class SeedScaleKeyTests
{
    /// <summary>
    /// Every scale the product seeds, from the two places that seed one: <see cref="DataSeeder" /> in
    /// code, and the paediatric catalogue in JSON. Adding a third means adding it here — which is the
    /// point, since the failure this guards is a name that binds to nothing.
    /// </summary>
    private static IReadOnlyCollection<string> SeededScaleNames()
    {
        var names = new List<string> { DataSeeder.OrScaleName };

        var cataloguePath = Path.Combine(
            AppContext.BaseDirectory, "Persistence", "Seeds", "paediatric-epa-v11.1.json");

        if (File.Exists(cataloguePath))
        {
            using var document = JsonDocument.Parse(File.ReadAllText(cataloguePath));
            if (document.RootElement.TryGetProperty("scale", out var scale) &&
                scale.TryGetProperty("name", out var name) &&
                name.GetString() is { Length: > 0 } catalogueScale)
            {
                names.Add(catalogueScale);
            }
        }

        return names;
    }

    public static TheoryData<string> SeedDirectories
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (var directory in Directory.EnumerateDirectories(
                         Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds")))
            {
                data.Add(Path.GetFileName(directory));
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void EveryScaleKeyNamesASeededScale(string seedKey)
    {
        var schemaPath = Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", seedKey, "schema.json");
        var schema = FormSchemaParser.Parse(File.ReadAllText(schemaPath));
        var seeded = SeededScaleNames();

        var declared = schema.Sections
            .SelectMany(section => section.Fields)
            .Where(field => !string.IsNullOrWhiteSpace(field.ScaleKey))
            .Select(field => (field.Key, ScaleKey: field.ScaleKey!))
            .ToArray();

        foreach (var (fieldKey, scaleKey) in declared)
        {
            seeded.Should().Contain(scaleKey,
                "'{0}' field '{1}' declares scale_key '{2}', which is not the exact name of any seeded " +
                "scale. A scale_key binds by name; one that matches nothing resolves to nothing, and the " +
                "inline options keep the picker looking healthy while it does. Seeded scales: {3}",
                seedKey, fieldKey, scaleKey, string.Join(", ", seeded));
        }
    }

    /// <summary>
    /// The rated field in particular, which is the one that decides credit and which ladder a
    /// trajectory point sits on (T126). A component scale binding to nothing is a display bug; the
    /// rated field binding to nothing is a silent credit failure.
    /// </summary>
    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void TheRatedFieldBindsToASeededScale(string seedKey)
    {
        var schemaPath = Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", seedKey, "schema.json");
        var schema = FormSchemaParser.Parse(File.ReadAllText(schemaPath));

        if (schema.RatedLevelField is null)
        {
            return;
        }

        var rated = schema.Sections
            .SelectMany(section => section.Fields)
            .Single(field => string.Equals(field.Key, schema.RatedLevelField, StringComparison.Ordinal));

        rated.ScaleKey.Should().NotBeNullOrWhiteSpace(
            "'{0}' declares '{1}' as its entrustment rating, so that field must say which ladder it is " +
            "rated against", seedKey, schema.RatedLevelField);

        SeededScaleNames().Should().Contain(rated.ScaleKey!,
            "'{0}' rates on '{1}', which names no seeded scale", seedKey, rated.ScaleKey);
    }

    /// <summary>
    /// The classifier, run over the REAL seed corpus rather than a fixture. (T134)
    /// </summary>
    /// <remarks>
    /// This is the guard that was missing when the gate was widened. The sampling handler's test
    /// fixture seeded activity types with no schema at all — a shape nothing can publish since T126 —
    /// so every test passed through the family-name arm and none of them proved that the actual
    /// seeded tools are recognised as rated. Asserting against the files on disk is what makes the
    /// declaration-only gate safe: if a seed stops declaring its pointer, this fails by name.
    /// </remarks>
    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void TheClassifierAgreesWithWhatTheSeedDeclares(string seedKey)
    {
        var schemaPath = Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", seedKey, "schema.json");
        var schemaJson = File.ReadAllText(schemaPath);
        var declaresRating = FormSchemaParser.Parse(schemaJson).RatedLevelField is not null;

        var verdict = RatedActivityTypes.Classify(seedKey, DeclaredWbaToolKey(seedKey), schemaJson);

        verdict.IsRated.Should().Be(declaresRating,
            "'{0}' is rated exactly when its schema says so, never because of what it is called", seedKey);
        verdict.SourceBucket.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// What each seed's evidence is called. The nineteen rows present at T144 are what each seed was called before
    /// it, when the category was read from the type's key; a later seed adds its row here deliberately. Typed here
    /// rather than derived, so the classifier under test cannot supply its own expectation.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string> ExpectedSourceBucketBySeed =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["acat"] = "Conversation",
            ["cbd"] = "Conversation",
            ["cbd_cpsa"] = "Conversation",
            ["cca_cpsa"] = "Case analysis",
            ["chart_stimulated_recall_cpsa"] = "Conversation",
            // T154. Unrated evidence has no category, so each counts as its own source under its own key.
            ["clinical_audit_cpsa"] = "clinical_audit_cpsa",
            ["direct_observation_cpsa"] = "Direct observation",
            ["dops"] = "Direct observation",
            ["dops_cpsa"] = "Direct observation",
            ["journal_club"] = "journal_club",
            ["mini_cex"] = "Direct observation",
            ["mini_cex_cpsa"] = "Direct observation",
            ["msf_cpsa"] = "msf_cpsa",
            ["portfolio_review_cpsa"] = "portfolio_review_cpsa",
            ["procedure_log"] = "procedure_log",
            ["qi_project"] = "qi_project",
            ["rca_cpsa"] = "Case analysis",
            ["reflective_exercise_cpsa"] = "reflective_exercise_cpsa",
            ["reflective_note"] = "reflective_note",
            ["research_output"] = "research_output",
            ["teaching_session"] = "teaching_session"
        };

    /// <summary>
    /// T144 moved the category onto the instrument each type declares. On the seed corpus that must change
    /// nothing: every seed classified by its catalogue <c>WbaToolKey</c> reads exactly as it did by its key,
    /// so no committee's source count and no trajectory label moves.
    /// </summary>
    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void EverySeedClassifiesByItsInstrumentExactlyAsItDidByItsKey(string seedKey)
    {
        var schemaJson = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", seedKey, "schema.json"));

        ExpectedSourceBucketBySeed.Should().ContainKey(seedKey,
            "a new seed must state here what its evidence is called");

        RatedActivityTypes.Classify(seedKey, DeclaredWbaToolKey(seedKey), schemaJson).SourceBucket
            .Should().Be(ExpectedSourceBucketBySeed[seedKey],
                "'{0}' declares instrument '{1}'", seedKey, DeclaredWbaToolKey(seedKey));
    }

    /// <summary>
    /// Every instrument in the College's vocabulary has a stated evidence category in the classifier, "none"
    /// included, and the classifier states none the vocabulary lacks. A new instrument otherwise arrives with
    /// no category and nobody having decided that. (T144)
    /// </summary>
    [Fact]
    public void TheClassifierStatesACategoryForEveryInstrumentInTheVocabulary()
    {
        var cataloguePath = Path.Combine(
            AppContext.BaseDirectory, "Persistence", "Seeds", "paediatric-epa-v11.1.json");
        using var document = JsonDocument.Parse(File.ReadAllText(cataloguePath));

        var vocabulary = document.RootElement.GetProperty("wbaToolVocabulary")
            .EnumerateArray()
            .Select(tool => tool.GetProperty("key").GetString()!)
            .ToArray();

        vocabulary.Should().NotBeEmpty();
        RatedActivityTypes.ClassifiedWbaToolKeys.Should().BeEquivalentTo(vocabulary);
    }

    /// <summary>
    /// The instrument a seed declares, from the catalogue both seeders create it from. A folder with no
    /// catalogue entry fails here by name rather than classifying as unkeyed.
    /// </summary>
    private static string? DeclaredWbaToolKey(string seedKey)
    {
        var entry = ActivityTypeSeedCatalogue.Entries
            .SingleOrDefault(candidate => string.Equals(candidate.Key, seedKey, StringComparison.Ordinal));

        entry.Should().NotBeNull("seed folder '{0}' must be registered in ActivityTypeSeedCatalogue", seedKey);
        return entry!.WbaToolKey;
    }

    /// <summary>
    /// Nine rated, six not — the split stated as a number, so a new seed cannot quietly change it.
    /// </summary>
    /// <remarks>
    /// <c>msf_cpsa</c> joined the rated set under [T121], and that is a decision rather than a
    /// consequence of adding a folder. It carries an <c>overall_level</c> scale field bound to the CPSA
    /// ladder, which <c>Schema_DeclaresARatedFieldExactlyWhenItCarriesAScale</c> requires it to name as
    /// its rated field — there is no way to keep the field and stay out of this list. What follows from
    /// being in it is that MSF counts as rated evidence on the committee sampling report, where it then
    /// contributes nothing to the numerator because it names no assessor. That asymmetry is deliberate:
    /// an MSF asserts a level but has no observing assessor, so the assessor-concentration arithmetic
    /// correctly has nothing to weigh. The same is true of the trajectory chart, which drops it at
    /// <c>TryParseObservation</c> for the same reason.
    /// </remarks>
    [Fact]
    public void ExactlyTwelveSeededToolsAreRated()
    {
        var rated = Directory
            .EnumerateDirectories(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds"))
            .Select(directory => Path.GetFileName(directory)!)
            .Where(key => RatedActivityTypes.Classify(
                key,
                DeclaredWbaToolKey(key),
                File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", key, "schema.json"))).IsRated)
            .OrderBy(key => key, StringComparer.Ordinal)
            .ToArray();

        // T120 added three rated instruments. reflective_exercise_cpsa is deliberately absent: D6 makes it unrated
        // evidence, so it declares no rated_level_field and nothing charts it as entrustment. So are T154's
        // clinical_audit_cpsa and portfolio_review_cpsa, for the same reason.
        rated.Should().Equal(
            "acat", "cbd", "cbd_cpsa", "cca_cpsa", "chart_stimulated_recall_cpsa", "direct_observation_cpsa",
            "dops", "dops_cpsa", "mini_cex", "mini_cex_cpsa", "msf_cpsa", "rca_cpsa");
    }

    /// <summary>
    /// The guard is only worth having if it fails on the shape it was written for.
    /// </summary>
    [Fact]
    public void TheGuardRejectsAKeyThatNamesNoSeededScale()
    {
        SeededScaleNames().Should().NotContain("or_scale",
            "that is the exact value four seeds carried for months while binding to nothing");
        SeededScaleNames().Should().Contain(DataSeeder.OrScaleName);
    }
}
