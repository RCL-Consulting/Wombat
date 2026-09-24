using System.Text.Json;
using FluentAssertions;
using Wombat.Domain.Curricula;
using Wombat.Web.Components.Pages.Admin.Curricula;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T125: the year-by-year minimum editor's model. It must read a year exactly as credit does, write the JSON the
/// command accepts, and never drop what it cannot render.
/// </summary>
public sealed class StageMinimaDraftTests
{
    [Fact]
    public void ReadsEachYearAsCreditDoes_AndWritesTheCanonicalShape()
    {
        // ParseStageOverrides accepts a level written as a string; so does the editor, and it writes it back as a number.
        const string stored = """{"2":"4","1":3}""";

        var draft = StageMinimaDraft.Parse(stored);

        draft.Rows.Select(row => (row.Year, row.Level)).Should().Equal((1, 3), (2, 4));
        draft.Kept.Should().BeEmpty();
        CurriculumItem.ParseStageOverrides(draft.ToJson()).Should().Equal(CurriculumItem.ParseStageOverrides(stored));
        CurriculumItem.NormalizeStageOverridesJson(draft.ToJson()).Should().Be("""{"1":3,"2":4}""");
    }

    [Fact]
    public void AnEntryThatIsNotAYearAndALevel_IsKeptVerbatim_AndWrittenBack()
    {
        const string stored = """{"1":3,"0":2,"2":25,"note":{"by":"college"}}""";

        var draft = StageMinimaDraft.Parse(stored);

        draft.Rows.Select(row => row.Year).Should().Equal(1);
        draft.Kept.Select(entry => entry.Name).Should().Equal("0", "2", "note");

        using var written = JsonDocument.Parse(draft.ToJson()!);
        written.RootElement.EnumerateObject()
            .ToDictionary(property => property.Name, property => property.Value.GetRawText())
            .Should().BeEquivalentTo(new Dictionary<string, string>
            {
                ["1"] = "3",
                ["0"] = "2",
                ["2"] = "25",
                ["note"] = """{"by":"college"}"""
            });
    }

    [Fact]
    public void AYearSpelledTwice_ShowsTheSpellingCreditReads_AndKeepsTheOtherAsAnEarlierSpelling()
    {
        // Only direct SQL can store this (the column is text, and every writer normalises). Credit reads the LAST
        // spelling, so the row must show that one; showing the first would put a minimum on screen that is not in force,
        // and removing the "leftover" to get the item to save would quietly change year 1's minimum from 5 to 3.
        const string stored = """{"1":3,"01":5}""";

        var draft = StageMinimaDraft.Parse(stored);

        CurriculumItem.ParseStageOverrides(stored).Should().Equal(new Dictionary<int, int> { [1] = 5 });
        draft.Rows.Select(row => (row.Year, row.Level)).Should().Equal((1, 5));
        draft.Kept.Should().ContainSingle().Which.Should().Be(new KeptStageEntry("1", "3", EarlierSpellingOf: 1));
        draft.HasUnrenderedEntries.Should().BeTrue();

        // Written back, the earlier spelling stays earlier, so credit still reads 5.
        CurriculumItem.ParseStageOverrides(draft.ToJson()).Should().Equal(new Dictionary<int, int> { [1] = 5 });

        draft.RemoveKept(draft.Kept.Single());
        draft.ToJson().Should().Be("""{"1":5}""");
    }

    [Fact]
    public void AStoredValueThatIsNotAnObject_IsWrittenBackAsItWas_AndTakesNoYearsUntilRemoved()
    {
        const string stored = "[3,4]";

        var draft = StageMinimaDraft.Parse(stored);

        draft.Unreadable.Should().Be(stored);
        draft.CanAddYear.Should().BeFalse();
        draft.HasUnrenderedEntries.Should().BeTrue("the page does not save until it is removed");
        draft.ToJson().Should().Be(stored);

        draft.RemoveUnreadable();
        draft.HasUnrenderedEntries.Should().BeFalse();
        draft.AddYear();

        draft.ToJson().Should().Be("""{"1":null}""", "a year with no rung is written as null, never dropped");
    }

    [Fact]
    public void AddYear_FillsTheFirstGap_ThenExtends()
    {
        var draft = StageMinimaDraft.Parse("""{"2":3,"4":5}""");

        draft.NextYear.Should().Be(1);
        draft.AddYear();
        draft.NextYear.Should().Be(3);
        draft.AddYear();
        draft.NextYear.Should().Be(5);

        draft.Rows.Select(row => row.Year).Should().Equal(1, 2, 3, 4);
    }

    [Fact]
    public void ClearLevels_EmptiesEveryYear_KeepsTheYears_AndLeavesKeptEntriesAlone()
    {
        var draft = StageMinimaDraft.Parse("""{"1":3,"2":4,"note":1}""");

        draft.ClearLevels();

        draft.Rows.Should().OnlyContain(row => row.Level == null);
        draft.Rows.Select(row => row.Year).Should().Equal(1, 2);
        draft.Kept.Should().ContainSingle().Which.Should().Be(new KeptStageEntry("note", "1"));
    }

    [Fact]
    public void Nothing_IsNull()
    {
        StageMinimaDraft.Parse(null).ToJson().Should().BeNull();
        StageMinimaDraft.Parse("{}").ToJson().Should().BeNull();
    }
}
