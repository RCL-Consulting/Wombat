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

        var verdict = RatedActivityTypes.Classify(seedKey, schemaJson);

        verdict.IsRated.Should().Be(declaresRating,
            "'{0}' is rated exactly when its schema says so, never because of what it is called", seedKey);
        verdict.SourceBucket.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Eight rated, six not — the split stated as a number, so a new seed cannot quietly change it.
    /// </summary>
    [Fact]
    public void ExactlyEightSeededToolsAreRated()
    {
        var rated = Directory
            .EnumerateDirectories(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds"))
            .Select(directory => Path.GetFileName(directory)!)
            .Where(key => RatedActivityTypes.Classify(
                key,
                File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", key, "schema.json"))).IsRated)
            .OrderBy(key => key, StringComparer.Ordinal)
            .ToArray();

        rated.Should().Equal(
            "acat", "cbd", "cbd_cpsa", "direct_observation_cpsa",
            "dops", "dops_cpsa", "mini_cex", "mini_cex_cpsa");
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
