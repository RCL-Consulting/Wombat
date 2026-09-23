using System.Globalization;
using Wombat.Domain.Curricula;

namespace Wombat.Domain.Tests.Curricula;

/// <summary>
/// T122. A curriculum item's <see cref="CurriculumItem.PermittedToolsJson" /> is read through
/// <see cref="CurriculumItem.ParsePermittedTools" /> and written through
/// <see cref="CurriculumItem.NormalizePermittedToolsJson" />.
/// </summary>
/// <remarks>
/// <para>
/// Two properties matter, and both are about never refusing the wrong thing. First, the parser fails OPEN (D21):
/// anything that is not a usable list of keys reads as "no restriction", because a restriction nobody can explain
/// empties the EPA picker and refuses every submit. Second, the writer never stores <c>[]</c>: an empty list would
/// read as "no instrument may credit this EPA" if any reader ever stopped treating empty as unrestricted.
/// </para>
/// <para>
/// The column is <c>jsonb</c>, so Postgres hands back its own rendering of whatever was written. The parser is what
/// makes the stored value comparable at all, which is why it must canonicalise (normalise, de-duplicate, sort), not
/// merely deserialise.
/// </para>
/// </remarks>
public sealed class CurriculumItemPermittedToolsTests
{
    // ----- ParsePermittedTools: fail open -----

    /// <summary>
    /// Every shape that is not an array of usable keys gives the empty list, which the predicate reads as
    /// unrestricted. A parser that threw on these would turn one bad row (only direct SQL can write one) into an
    /// exception on every picker load and every submit for that curriculum.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not json")]
    [InlineData("[\"cbd\"")]
    [InlineData("{\"tools\":[\"cbd\"]}")]
    [InlineData("{}")]
    [InlineData("42")]
    [InlineData("true")]
    [InlineData("null")]
    [InlineData("\"cbd\"")]
    [InlineData("[]")]
    [InlineData("[\"\", \" \"]")]
    [InlineData("[\"\\t\", \"\\n\"]")]
    [InlineData("[1, 2, 3]")]
    [InlineData("[null, true, {}, []]")]
    public void AnythingThatIsNotAUsableListOfKeysReadsAsEmpty(string? json)
    {
        Assert.Empty(CurriculumItem.ParsePermittedTools(json));
    }

    /// <summary>
    /// A non-string entry is dropped, not fatal, and does not take the string entries beside it down with it. Nested
    /// arrays and objects are not searched: a key is only ever a top-level string.
    /// </summary>
    [Fact]
    public void NonStringEntriesAreDroppedAndTheStringsBesideThemSurvive()
    {
        var tools = CurriculumItem.ParsePermittedTools("[\"cbd\", 1, null, true, {\"key\":\"dops\"}, [\"msf\"], \"mini_cex\"]");

        Assert.Equal(new[] { "cbd", "mini_cex" }, tools);
    }

    /// <summary>
    /// Case and padding variants of one key are one key. The predicate compares the normalised activity-type key
    /// against this list, so a stored <c>" CBD "</c> that survived un-normalised would never match <c>cbd</c> and would
    /// refuse the instrument the College named.
    /// </summary>
    [Fact]
    public void DuplicatesAndCaseOrWhitespaceVariantsCollapseToOneNormalisedKey()
    {
        var tools = CurriculumItem.ParsePermittedTools("[\"CBD\", \" cbd \", \"cbd\", \"Dops\", \"\\tDOPS\\n\", \"msf\"]");

        Assert.Equal(new[] { "cbd", "dops", "msf" }, tools);
    }

    /// <summary>
    /// The list is sorted ordinally, so two stored renderings of the same set compare equal by
    /// <see cref="Enumerable.SequenceEqual{TSource}(IEnumerable{TSource}, IEnumerable{TSource})" />, which is how the
    /// seeder's startup warning decides whether a seeded item still matches the catalogue.
    /// </summary>
    [Fact]
    public void TheListIsSortedOrdinally()
    {
        Assert.Equal(new[] { "cbd", "cca" }, CurriculumItem.ParsePermittedTools("[\"cca\", \"cbd\"]"));

        // The whole T122 vocabulary, shuffled, comes back in ordinal order.
        var vocabulary = new[]
        {
            "rca", "msf", "cbd", "reflective_exercise", "mini_cex", "chart_stimulated_recall", "dops",
            "portfolio_review", "cca", "learner_feedback", "direct_observation", "clinical_audit"
        };
        var expected = vocabulary.Order(StringComparer.Ordinal).ToArray();

        var tools = CurriculumItem.ParsePermittedTools(
            "[" + string.Join(",", vocabulary.Select(key => $"\"{key}\"")) + "]");

        Assert.Equal(expected, tools);
    }

    /// <summary>
    /// Postgres re-renders a <c>jsonb</c> value with its own separators. The parser must see the re-rendered form and
    /// the compact form the writer produced as the same list, or every comparison against a stored value would report
    /// a difference that is not there.
    /// </summary>
    [Fact]
    public void ThePostgresRenderingAndTheCompactRenderingParseToTheSameList()
    {
        var compact = CurriculumItem.ParsePermittedTools("[\"cbd\",\"dops\",\"msf\"]");
        var rerendered = CurriculumItem.ParsePermittedTools("  [\"cbd\", \"dops\", \"msf\"]  ");

        Assert.Equal(compact, rerendered);
    }

    [Fact]
    public void ParsingIsCultureInvariant()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

            Assert.Equal(new[] { "clinical_audit", "mini_cex" }, CurriculumItem.ParsePermittedTools("[\"MINI_CEX\", \"CLINICAL_AUDIT\"]"));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    // ----- NormalizePermittedToolsJson: never store [] -----

    /// <summary>
    /// Nothing usable to store means null, never <c>"[]"</c>. Null is the permanent, meaningful "any instrument"; an
    /// empty array is a second spelling of it that one careless reader away from meaning "no instrument at all".
    /// </summary>
    [Fact]
    public void NothingUsableToStoreIsNullNeverAnEmptyArray()
    {
        Assert.Null(CurriculumItem.NormalizePermittedToolsJson(null));
        Assert.Null(CurriculumItem.NormalizePermittedToolsJson(Array.Empty<string?>()));
        Assert.Null(CurriculumItem.NormalizePermittedToolsJson(new string?[] { "" }));
        Assert.Null(CurriculumItem.NormalizePermittedToolsJson(new string?[] { "", " ", "\t", null }));
    }

    /// <summary>
    /// The stored form is exactly one string per set: compact, normalised, distinct, sorted. The migration's frozen
    /// literals are asserted equal to this output, so its shape is a contract, not a detail.
    /// </summary>
    [Fact]
    public void TheStoredFormIsCompactSortedJsonOfNormalisedDistinctKeys()
    {
        var json = CurriculumItem.NormalizePermittedToolsJson(new string?[] { "msf", "DOPS", " cbd ", "cbd", "", null });

        Assert.Equal("[\"cbd\",\"dops\",\"msf\"]", json);
    }

    [Fact]
    public void ASingleKeyIsStillStoredAsAnArray()
    {
        Assert.Equal("[\"mini_cex\"]", CurriculumItem.NormalizePermittedToolsJson(new[] { " Mini_CEX " }));
    }

    public static TheoryData<string?[]?> RoundTripInputs() => new()
    {
        null,
        Array.Empty<string?>(),
        new string?[] { "", " " },
        new string?[] { "cbd" },
        new string?[] { "msf", "DOPS", " cbd ", "cbd" },
        new string?[] { "cca", "cbd", "mini_cex", "Mini_Cex", null },
        new string?[] { "rca", "chart_stimulated_recall", "reflective_exercise", "clinical_audit", "portfolio_review" }
    };

    /// <summary>
    /// Write, read, write again: the second write produces the same string as the first, and reading either gives the
    /// same list. The admin editor loads the parsed list into checkboxes and saves it back, so an unchanged save must
    /// store exactly what was there.
    /// </summary>
    [Theory]
    [MemberData(nameof(RoundTripInputs))]
    public void WritingThenReadingThenWritingAgainIsStable(string?[]? input)
    {
        var stored = CurriculumItem.NormalizePermittedToolsJson(input);
        var parsed = CurriculumItem.ParsePermittedTools(stored);
        var restored = CurriculumItem.NormalizePermittedToolsJson(parsed);

        Assert.Equal(stored, restored);
        Assert.Equal(parsed, CurriculumItem.ParsePermittedTools(restored));

        var expected = (input ?? Array.Empty<string?>())
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Select(key => key!.Trim().ToLowerInvariant())
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(expected, parsed);
        Assert.Equal(expected.Length == 0, stored is null);
    }
}
