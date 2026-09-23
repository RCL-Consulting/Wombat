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

    // Annexure B's per-semester column, hard-coded from the published table (T098-data/annexure-b.json, which
    // lives under execution/ and does not ship with the build). Reading the expectation out of the seed file it
    // is checking would be a tautology. Null means the EPA is one per annum, "as opportunities arise".
    private static readonly IReadOnlyDictionary<string, int?> AnnexureBPerSemester = new Dictionary<string, int?>
    {
        ["PAED-001"] = 3,
        ["PAED-002"] = 3,
        ["PAED-003"] = 3,
        ["PAED-004"] = 3,
        ["PAED-005"] = 3,
        ["PAED-006"] = 2,
        ["PAED-007"] = 1,
        ["PAED-008"] = null,
        ["PAED-009"] = null,
        ["PAED-010"] = 3,
        ["PAED-011"] = null,
        ["PAED-012"] = 3,
        ["PAED-013"] = null,
        ["PAED-014"] = null,
        ["PAED-015"] = 1
    };

    private static int? PerSemester(JsonElement epa)
    {
        var value = epa.GetProperty("observationsPerSemester");
        return value.ValueKind == JsonValueKind.Null ? null : value.GetInt32();
    }

    [Fact]
    public void EveryEpaDeclaresItsPerSemesterFigureExplicitly()
    {
        // T130: the seeder reads this key case-insensitively, so a missing or misspelled key deserialises to
        // null and silently makes that EPA a once-a-year item. Requiring the key on every EPA — null or an
        // integer, never absent — is what separates "one per annum" from "someone forgot".
        foreach (var epa in Epas())
        {
            var code = epa.GetProperty("code").GetString();

            epa.TryGetProperty("observationsPerSemester", out var value)
                .Should().BeTrue($"{code} must state its per-semester figure, even when that figure is null");
            value.ValueKind.Should().BeOneOf(
                [JsonValueKind.Null, JsonValueKind.Number], $"{code}'s per-semester figure must be null or a number");
            if (value.ValueKind == JsonValueKind.Number)
            {
                value.TryGetInt32(out _).Should().BeTrue($"{code}'s per-semester figure must be a whole number of encounters");
            }
        }
    }

    [Fact]
    public void ExactlyTheTenAnnexureBSemesterEpasCarryAPerSemesterFigure()
    {
        // PAED-003, 006, 007 and 015 are on this list although their currency strings say "annually": Annexure B
        // gives them a per-semester figure, and D39 takes the quota period from that column, not from currency.
        // Deriving it from currency would drop them and break Annexure B's own 25-per-semester total.
        Epas()
            .Where(epa => PerSemester(epa) is not null)
            .Select(epa => epa.GetProperty("code").GetString())
            .Should().BeEquivalentTo(
                "PAED-001", "PAED-002", "PAED-003", "PAED-004", "PAED-005",
                "PAED-006", "PAED-007", "PAED-010", "PAED-012", "PAED-015");
    }

    [Fact]
    public void EveryPerSemesterFigureMatchesAnnexureB()
    {
        var seeded = Epas().ToDictionary(epa => epa.GetProperty("code").GetString()!, PerSemester);

        seeded.Should().Equal(AnnexureBPerSemester);
    }

    [Fact]
    public void EveryPerSemesterFigureIsHalfThePerYearFigure()
    {
        // Annexure B splits each semester-cadence EPA's annual frequency evenly across the two semesters.
        foreach (var epa in Epas().Where(epa => PerSemester(epa) is not null))
        {
            var code = epa.GetProperty("code").GetString();
            var perYear = epa.GetProperty("observationsPerYear").GetInt32();

            (PerSemester(epa)!.Value * 2).Should().Be(perYear, $"{code}'s two semesters must add up to its per-annum frequency");
        }
    }

    [Fact]
    public void PerSemesterFiguresTotalTwentyFive()
    {
        // Annexure B: "25 of the 55 fall in each semester, for the EPAs assessed on a semester cadence."
        Epas().Sum(epa => PerSemester(epa) ?? 0).Should().Be(25);
    }

    [Fact]
    public void TwoSemestersPlusTheFiveOncePerAnnumEpasMakeFiftyFive()
    {
        // The per-semester column must reconcile with the per-annum total the document states separately: two
        // semesters of 25, plus the five EPAs that are one per annum and belong to neither semester.
        var oncePerAnnum = Epas().Where(epa => PerSemester(epa) is null).ToArray();

        oncePerAnnum.Should().HaveCount(5);
        oncePerAnnum.Should().AllSatisfy(epa => epa.GetProperty("observationsPerYear").GetInt32().Should().Be(1));

        var perSemesterTotal = Epas().Sum(epa => PerSemester(epa) ?? 0);
        var oncePerAnnumTotal = oncePerAnnum.Sum(epa => epa.GetProperty("observationsPerYear").GetInt32());

        (perSemesterTotal + perSemesterTotal + oncePerAnnumTotal).Should().Be(55);
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
