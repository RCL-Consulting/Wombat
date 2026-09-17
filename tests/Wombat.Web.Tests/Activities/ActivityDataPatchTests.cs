using System.Text.Json.Nodes;
using FluentAssertions;
using Wombat.Web.Components.Shared.Activities;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T070 step 7's patch builder. The page sends a diff, not the whole form: anything else is
/// rejected by <c>ActivityService.MergeWritableKeys</c>, which throws on a patched key the actor
/// does not own unless the value matches what is already stored.
/// </summary>
public sealed class ActivityDataPatchTests
{
    private const string StoredJson = """
        { "epa_id": "3", "overall_level": "2", "strengths": "As before" }
        """;

    [Fact]
    public void Build_EmitsOnlyWritableKeysWhoseValueChanged()
    {
        const string workingJson = """
            { "epa_id": "9", "overall_level": "5", "strengths": "As before", "plan": "Repeat in a month" }
            """;

        var patch = ActivityDataPatch.Build(
            StoredJson,
            workingJson,
            new HashSet<string>(StringComparer.Ordinal) { "overall_level", "strengths", "plan" });

        // epa_id changed but is not writable -> excluded (sending it would throw server-side).
        // strengths is writable but unchanged -> excluded (nothing to say).
        // Key order follows the working document, so the patch is deterministic.
        patch.Should().Be("""{"overall_level":"5","plan":"Repeat in a month"}""");
    }

    [Fact]
    public void Build_ReturnsNull_WhenNoWritableValueChanged()
    {
        const string workingJson = """
            { "epa_id": "9", "overall_level": "2", "strengths": "As before" }
            """;

        ActivityDataPatch.Build(
            StoredJson,
            workingJson,
            new HashSet<string>(StringComparer.Ordinal) { "overall_level", "strengths" })
            .Should().BeNull();
    }

    [Fact]
    public void Build_ReturnsNull_WhenTheActorOwnsNothing()
    {
        // The non-bound user on the detail page: an empty writable set means no patch at all,
        // never an empty object.
        ActivityDataPatch.Build(StoredJson, """{ "overall_level": "5" }""", new HashSet<string>())
            .Should().BeNull();

        ActivityDataPatch.Build(StoredJson, """{ "overall_level": "5" }""", null)
            .Should().BeNull();
    }

    [Fact]
    public void Build_EmitsExplicitNull_ForAClearedWritableKey()
    {
        // ActivityForm removes the key when a field is blanked. Omitting it from the patch would
        // leave the stored value in place; a JSON null overwrites it, and SchemaValidator reads
        // null as absent.
        var patch = ActivityDataPatch.Build(
            StoredJson,
            """{ "epa_id": "3" }""",
            new HashSet<string>(StringComparer.Ordinal) { "overall_level", "strengths" });

        patch.Should().Be("""{"overall_level":null,"strengths":null}""");
    }

    [Fact]
    public void Build_TreatsAnAbsentWritableKeyThatWasNeverStoredAsNoChange()
    {
        ActivityDataPatch.Build(
            StoredJson,
            StoredJson,
            new HashSet<string>(StringComparer.Ordinal) { "overall_level", "improvements", "plan" })
            .Should().BeNull();
    }

    [Fact]
    public void Build_ComparesValuesStructurally_NotTextually()
    {
        // Same values, different formatting and property order: no patch.
        const string stored = """{ "tags": ["a", "b"], "detail": { "x": 1, "y": 2 } }""";
        const string working = """{"detail":{"y":2,"x":1},"tags":["a","b"]}""";

        ActivityDataPatch.Build(stored, working, new HashSet<string>(StringComparer.Ordinal) { "tags", "detail" })
            .Should().BeNull();

        // A real change inside the array does produce one, carrying only the key that moved.
        var patch = ActivityDataPatch.Build(
            stored,
            """{ "tags": ["a", "c"], "detail": { "x": 1, "y": 2 } }""",
            new HashSet<string>(StringComparer.Ordinal) { "tags", "detail" });

        JsonNode.Parse(patch!)!.AsObject().Select(property => property.Key).Should().Equal("tags");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Build_TreatsMissingJsonAsAnEmptyObject(string? blank)
    {
        ActivityDataPatch.Build(blank, blank, new HashSet<string>(StringComparer.Ordinal) { "overall_level" })
            .Should().BeNull();

        ActivityDataPatch.Build(blank, """{ "overall_level": "5" }""", new HashSet<string>(StringComparer.Ordinal) { "overall_level" })
            .Should().Be("""{"overall_level":"5"}""");
    }

    [Fact]
    public void Build_Throws_WhenEitherDocumentIsNotAnObject()
    {
        var writable = new HashSet<string>(StringComparer.Ordinal) { "overall_level" };

        FluentActions.Invoking(() => ActivityDataPatch.Build("[1,2]", "{}", writable))
            .Should().Throw<InvalidOperationException>();

        FluentActions.Invoking(() => ActivityDataPatch.Build("{}", "\"nope\"", writable))
            .Should().Throw<InvalidOperationException>();
    }
}
