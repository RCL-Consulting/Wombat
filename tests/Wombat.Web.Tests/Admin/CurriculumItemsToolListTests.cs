using Bunit;
using FluentAssertions;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Identity;
using FakeSender = Wombat.Web.Tests.Admin.CurriculumItemsFakeSender;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T122, on the items list and an item's page of its own since T392 (flow 15, Q14): the tool list, the only surface that
/// writes an EPA's allow-list.
/// </summary>
/// <remarks>
/// Every failure worth testing here is permissive and therefore silent. An item saved with an empty list lets every
/// instrument credit its EPA (D21), so a page that dropped the stored list on an unchanged save, or could not show a stored
/// key to untick it, would widen the College's mapping and nothing anywhere would say so.
/// </remarks>
public sealed class CurriculumItemsToolListTests : CurriculumItemsTestBase
{
    private static readonly IReadOnlyList<WbaToolDto> Vocabulary = FakeSender.Vocabulary;

    public CurriculumItemsToolListTests()
    {
        SignIn(WombatRoles.Administrator);
    }

    // ---- The Tools column ----

    [Fact]
    public void TheToolsColumn_NamesEachItemsInstruments_OrSaysAnyInstrument()
    {
        var cut = RenderList(new FakeSender());

        Text(Cell(cut, "PAED-001", "Tools")).Should().Be("Case-based discussion, Mini-CEX", "the College's names, not the keys");
        // Null is "any instrument" (D21) and must SAY so: a blank cell reads as "not filled in".
        Text(Cell(cut, "PAED-002", "Tools")).Should().Be("Any instrument");
    }

    [Fact]
    public void TheToolsColumn_ShowsAKeyTheVocabularyNoLongerHolds_ByItsRawKey()
        => Text(Cell(RenderList(new FakeSender()), "PAED-003", "Tools")).Should().Be("legacy_tool, Mini-CEX");

    // ---- The item's page ----

    [Fact]
    public void AnItemsPage_HasOneToolsFieldset_OneCheckboxPerTool_TickedForTheItemsKeys()
    {
        var cut = RenderItem(new FakeSender(), 11);

        cut.FindAll("fieldset legend").Select(Text).Should().Contain("Tools");
        cut.Find("fieldset[aria-describedby=item-tools-help]").ClassList.Should().Contain("form-group", "a field-level group (T188)");
        CheckboxIds(cut).Should().Equal(Vocabulary.Select(tool => $"item-tool-{tool.Key}"));
        IsChecked(cut, "item-tool-cbd").Should().BeTrue();
        IsChecked(cut, "item-tool-mini_cex").Should().BeTrue();
        IsChecked(cut, "item-tool-dops").Should().BeFalse();
        Text(cut.Find("label[for=item-tool-cbd]")).Should().Be("Case-based discussion");
    }

    [Fact]
    public void AnItemWithNoList_OpensWithNothingTicked()
    {
        var cut = RenderItem(new FakeSender(), 12);

        CheckboxIds(cut).Should().OnlyContain(id => !IsChecked(cut, id));
    }

    [Fact]
    public void AnOrphanStoredKey_IsItsOwnTickedCheckbox_LabelledWithTheRawKey()
    {
        // The server refuses a key the vocabulary does not hold; unseen, the admin could not untick it.
        var cut = RenderItem(new FakeSender(), 13);

        CheckboxIds(cut).Should().Equal(Vocabulary.Select(tool => $"item-tool-{tool.Key}").Append("item-tool-legacy_tool"));
        IsChecked(cut, "item-tool-legacy_tool").Should().BeTrue();
        Text(cut.Find("label[for=item-tool-legacy_tool]")).Should().Be("legacy_tool");
    }

    [Fact]
    public void TheAddPage_StartsWithNothingTicked()
    {
        var cut = RenderItem(new FakeSender { EpaList = FakeSender.MoreEpas });

        CheckboxIds(cut).Should().Equal(Vocabulary.Select(tool => $"item-tool-{tool.Key}"));
        CheckboxIds(cut).Should().OnlyContain(id => !IsChecked(cut, id));
    }

    [Theory]
    [InlineData(11, new[] { "cbd", "mini_cex" })]
    [InlineData(12, new string[0])]
    [InlineData(13, new[] { "legacy_tool", "mini_cex" })]
    public void SavingAnUnchangedItem_SendsTheSameToolKeys(int itemId, string[] keys)
    {
        // An orphan is sent too, so the server's refusal is what the admin sees, not a silent drop.
        var sender = new FakeSender();
        var cut = RenderItem(sender, itemId);

        cut.Find("form").Submit();

        sender.Updates.Should().ContainSingle().Which.PermittedToolKeys.Should().BeEquivalentTo(keys);
    }

    [Fact]
    public void TickingAndUnticking_ChangesExactlyWhatIsSent()
    {
        var sender = new FakeSender();
        var cut = RenderItem(sender, 11);

        cut.Find("#item-tool-dops").Change(true);
        cut.Find("#item-tool-cbd").Change(false);
        cut.Find("form").Submit();

        sender.Updates.Should().ContainSingle().Which.PermittedToolKeys.Should().BeEquivalentTo(["dops", "mini_cex"]);
    }

    [Fact]
    public void ARefusedSave_KeepsTheOperatorsTicks()
    {
        var sender = new FakeSender { UpdateFailure = new InvalidOperationException("Unknown tool key.") };
        var cut = RenderItem(sender, 11);

        cut.Find("#item-tool-msf").Change(true);
        cut.Find("form").Submit();

        IsChecked(cut, "item-tool-msf").Should().BeTrue();
        cut.FindAll("form").Should().ContainSingle("the form stays as the operator left it");
    }

    [Fact]
    public void AddingAnItem_SendsTheTickedTools()
    {
        var sender = new FakeSender { EpaList = FakeSender.MoreEpas };
        var cut = RenderItem(sender);

        cut.Find("#item-minimum").Change("3");
        cut.Find("#item-tool-msf").Change(true);
        cut.Find("form").Submit();

        sender.Adds.Should().ContainSingle().Which.PermittedToolKeys.Should().BeEquivalentTo(["msf"]);
    }

    [Fact]
    public void AddingAnItemWithNothingTicked_SendsAnEmptyList()
    {
        var sender = new FakeSender { EpaList = FakeSender.MoreEpas };
        var cut = RenderItem(sender);

        cut.Find("#item-minimum").Change("3");
        cut.Find("form").Submit();

        sender.Adds.Should().ContainSingle().Which.PermittedToolKeys.Should().BeEmpty();
    }

    [Fact]
    public void EveryIdOnTheItemsPage_IsUnique()
    {
        var cut = RenderItem(new FakeSender(), 13);

        cut.FindAll("[id]").Select(element => element.Id).Should().OnlyHaveUniqueItems();
    }

    private static IReadOnlyList<string> CheckboxIds(Bunit.IRenderedFragment cut)
        => cut.FindAll(".check-grid input[type=checkbox]").Select(input => input.Id!).ToList();

    private static bool IsChecked(Bunit.IRenderedFragment cut, string id) => cut.Find($"#{id}").HasAttribute("checked");
}
