using System.Text.Json;
using FluentAssertions;

namespace Wombat.Application.Tests.Features.Epas;

/// <summary>
/// Guards the seeded Paediatric EPA catalogue against silent drift from the published document
/// (EPA version 11.1, College of Paediatricians of South Africa).
/// </summary>
/// <remarks>
/// These assert figures the document states independently of the per-EPA tables — 15 EPAs, 78
/// descriptors, 55 observed encounters per registrar per year — so an editing mistake in the seed
/// file fails here rather than reaching a trainee's portfolio. The catalogue is a national
/// artefact; being quietly wrong about it is the expensive failure.
/// </remarks>
public sealed class PaediatricCatalogueSeedTests
{
    private static JsonElement LoadCatalogue()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Persistence", "Seeds", "paediatric-epa-v11.1.json");
        File.Exists(path).Should().BeTrue($"the catalogue seed file must ship with the build (looked in {path})");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.Clone();
    }

    private static JsonElement[] Epas() => LoadCatalogue().GetProperty("epas").EnumerateArray().ToArray();

    [Fact]
    public void ContainsFifteenEpas()
    {
        Epas().Should().HaveCount(15);
    }

    [Fact]
    public void ContainsSeventyEightDescriptors()
    {
        // Annexure A states 78 independently of the per-EPA descriptor lists.
        Epas().Sum(epa => epa.GetProperty("descriptors").GetArrayLength()).Should().Be(78);
    }

    [Fact]
    public void TotalsFiftyFiveObservationsPerYear()
    {
        // Annexure B: "55 observed encounters per registrar per year ... and 220 across the
        // four-year programme."
        Epas().Sum(epa => epa.GetProperty("observationsPerYear").GetInt32()).Should().Be(55);
    }

    [Fact]
    public void UsesTheSixRungLadder()
    {
        var levels = LoadCatalogue().GetProperty("scale").GetProperty("levels").EnumerateArray().ToArray();

        levels.Select(level => level.GetProperty("order").GetInt32()).Should().Equal(1, 2, 3, 4, 5, 6);

        // Order is the rank; Label is the rung as the College prints it. They diverge from rung 3
        // because v11.1 splits level 3 — this is the whole reason a six-rung ladder is needed.
        levels.Select(level => level.GetProperty("label").GetString())
            .Should().Equal("1", "2", "3a", "3b", "4", "5");
    }

    [Fact]
    public void EveryEpaHasAFullYearOneToYearFourCurve()
    {
        foreach (var epa in Epas())
        {
            var stages = epa.GetProperty("stageLevels");
            var code = epa.GetProperty("code").GetString();

            stages.EnumerateObject().Select(stage => stage.Name)
                .Should().BeEquivalentTo(["1", "2", "3", "4"], $"{code} must set a target for every training year");
        }
    }

    [Fact]
    public void EveryTargetIsAValidRungAndNeverRungOne()
    {
        // v11.1: level 1 ("the supervisor performs the activity") is never a target — it is where
        // a trainee starts, not something the programme asks them to reach.
        foreach (var epa in Epas())
        {
            var code = epa.GetProperty("code").GetString();
            foreach (var stage in epa.GetProperty("stageLevels").EnumerateObject())
            {
                stage.Value.GetInt32().Should().BeInRange(2, 6, $"{code} year {stage.Name} must be a rung above the first");
            }
        }
    }

    [Fact]
    public void EveryCurveIsMonotonic()
    {
        // Supervision requirements never go backwards across the programme.
        foreach (var epa in Epas())
        {
            var code = epa.GetProperty("code").GetString();
            var curve = Enumerable.Range(1, 4)
                .Select(year => epa.GetProperty("stageLevels").GetProperty(year.ToString()).GetInt32())
                .ToArray();

            curve.Should().BeInAscendingOrder($"{code}'s targets must not regress year on year");
        }
    }

    [Fact]
    public void MinimumLevelMatchesTheFinalYearTarget()
    {
        // The flat fallback used when a trainee has no resolvable stage must be the
        // end-of-programme expectation, not something laxer.
        foreach (var epa in Epas())
        {
            var code = epa.GetProperty("code").GetString();
            epa.GetProperty("minimumLevelOrder").GetInt32()
                .Should().Be(epa.GetProperty("stageLevels").GetProperty("4").GetInt32(), $"{code}");
        }
    }

    [Fact]
    public void CodesAreUniqueAndSortInPublishedOrder()
    {
        var codes = Epas().Select(epa => epa.GetProperty("code").GetString()!).ToArray();

        codes.Should().OnlyHaveUniqueItems();
        // Zero-padded so ordinal sort matches the document's EPA 1..15 sequence.
        codes.Should().BeInAscendingOrder(StringComparer.Ordinal);
        codes.Should().AllSatisfy(code => code.Should().MatchRegex(@"^PAED-\d{3}$"));
    }

    [Fact]
    public void EveryEpaBelongsToOneOfTheFivePublishedDomains()
    {
        string[] domains =
        [
            "Ambulatory, Emergency and Critical Care in Children",
            "Care for the Newborn",
            "Chronic, Developmental and Preventive Care",
            "Leadership, Population and Systems Health",
            "Ethics, Communication, Education and Palliative Care"
        ];

        Epas().Select(epa => epa.GetProperty("domain").GetString())
            .Should().OnlyContain(domain => domains.Contains(domain));
    }

    [Fact]
    public void EveryEpaSpecifiesMultiSourceFeedback()
    {
        // v11.1 names MSF as a tool on all fifteen EPAs — the one tool with no exceptions.
        foreach (var epa in Epas())
        {
            epa.GetProperty("wbaTools").EnumerateArray().Select(tool => tool.GetString())
                .Should().Contain("MSF", epa.GetProperty("code").GetString());
        }
    }
}
