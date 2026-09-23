using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Curricula;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T122: the curriculum item editor's tool list — the only surface that writes an EPA's allow-list.
/// </summary>
/// <remarks>
/// <para>
/// Every failure worth testing here is permissive and therefore silent. An item saved with an empty list lets
/// every instrument credit its EPA (D21), so an editor that dropped the stored list on an unchanged save, or
/// could not show a stored key to untick it, would widen the College's mapping and nothing anywhere would say
/// so. The commands take the list positionally for that reason; these tests pin the page that fills it.
/// </para>
/// <para>
/// The inline edit row and the Add form used to bind ONE model instance, so opening a row filled the Add form
/// with that row's values. With a checkbox group in each, the two groups would also have toggled each other.
/// </para>
/// </remarks>
public sealed class CurriculumItemsToolListTests : TestContext
{
    private const int CurriculumId = 5;
    private const int SubSpecialityId = 7;

    // Ordered by Name, as GetWbaToolsQuery returns it.
    private static readonly IReadOnlyList<WbaToolDto> Vocabulary =
    [
        new("cbd", "Case-based discussion", null),
        new("dops", "DOPS", null),
        new("mini_cex", "Mini-CEX", null),
        new("msf", "Multi-source feedback", null)
    ];

    private static readonly IReadOnlyList<EpaDto> Epas =
    [
        Epa(1, "PAED-001"),
        Epa(2, "PAED-002"),
        Epa(3, "PAED-003")
    ];

    public CurriculumItemsToolListTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin@test");
        auth.SetRoles(WombatRoles.Administrator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));
    }

    // ---- The Tools column ----

    [Fact]
    public void TheToolsColumn_NamesEachItemsInstruments_OrSaysAnyInstrument()
    {
        var cut = RenderPage(new FakeSender());

        HeaderTexts(cut).Should().Contain("Tools");

        ToolsCell(cut, "PAED-001").Should().Be("Case-based discussion, Mini-CEX",
            "the list shows the College's names, not the stored keys");

        // Null is "any instrument" (D21) and must SAY so. A blank cell reads as "not filled in", which is the
        // one reading the admin must not come away with.
        ToolsCell(cut, "PAED-002").Should().Be("Any instrument");
    }

    [Fact]
    public void TheToolsColumn_ShowsAKeyTheVocabularyNoLongerHolds_ByItsRawKey()
    {
        // Showing nothing for an orphan would make an item that restricts to an unknown instrument — and so
        // refuses every tool the platform knows — look like one that restricts to fewer tools than it does.
        var cut = RenderPage(new FakeSender());

        ToolsCell(cut, "PAED-003").Should().Be("legacy_tool, Mini-CEX");
    }

    // ---- The edit fieldset ----

    [Fact]
    public void OpeningAnItem_RendersAToolsFieldset_WithOneCheckboxPerTool_TickedForTheItemsKeys()
    {
        var cut = RenderPage(new FakeSender());

        BeginEdit(cut, "PAED-001");

        var fieldset = cut.FindAll("tbody fieldset").Should().ContainSingle().Subject;
        fieldset.QuerySelector("legend")!.TextContent.Trim().Should().Be("Tools");

        EditCheckboxIds(cut).Should().Equal(Vocabulary.Select(tool => $"edit-tool-{tool.Key}"),
            "one checkbox per instrument, in the vocabulary's order, and nothing else");

        IsChecked(cut, "edit-tool-cbd").Should().BeTrue();
        IsChecked(cut, "edit-tool-mini_cex").Should().BeTrue();
        IsChecked(cut, "edit-tool-dops").Should().BeFalse();
        IsChecked(cut, "edit-tool-msf").Should().BeFalse();

        LabelFor(cut, "edit-tool-cbd").Should().Be("Case-based discussion");
        LabelFor(cut, "edit-tool-mini_cex").Should().Be("Mini-CEX");
    }

    [Fact]
    public void TheEditSubRow_SpansEveryColumn()
    {
        // The fieldset sits in its own row under the edited item. A colspan short of the header count leaves
        // the tool list squeezed into the first columns and the table visibly broken.
        var cut = RenderPage(new FakeSender());

        BeginEdit(cut, "PAED-001");

        var cell = cut.FindAll("tbody td[colspan]").Should().ContainSingle().Subject;
        cell.GetAttribute("colspan").Should().Be(HeaderTexts(cut).Count.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public void AnItemWithNoList_OpensWithNothingTicked()
    {
        var cut = RenderPage(new FakeSender());

        BeginEdit(cut, "PAED-002");

        EditCheckboxIds(cut).Should().OnlyContain(id => !IsChecked(cut, id));
    }

    [Fact]
    public void AnOrphanStoredKey_RendersAsItsOwnTickedCheckbox_LabelledWithTheRawKey()
    {
        // The server refuses a key the vocabulary does not hold. If the editor could not show the orphan, the
        // admin could neither see why the save is refused nor untick it — the item would be unsaveable.
        var cut = RenderPage(new FakeSender());

        BeginEdit(cut, "PAED-003");

        EditCheckboxIds(cut).Should().Equal(
            Vocabulary.Select(tool => $"edit-tool-{tool.Key}").Append("edit-tool-legacy_tool"));

        IsChecked(cut, "edit-tool-legacy_tool").Should().BeTrue();
        LabelFor(cut, "edit-tool-legacy_tool").Should().Be("legacy_tool");
        IsChecked(cut, "edit-tool-mini_cex").Should().BeTrue();
    }

    // ---- The Add form ----

    [Fact]
    public void TheAddForm_HasItsOwnToolsFieldset_WithAddPrefixedIds_NothingTicked()
    {
        var cut = RenderPage(new FakeSender());

        var fieldset = cut.FindAll("form fieldset").Should().ContainSingle().Subject;
        fieldset.QuerySelector("legend")!.TextContent.Trim().Should().Be("Tools");

        AddCheckboxIds(cut).Should().Equal(Vocabulary.Select(tool => $"add-tool-{tool.Key}"));
        AddCheckboxIds(cut).Should().OnlyContain(id => !IsChecked(cut, id));
        LabelFor(cut, "add-tool-dops").Should().Be("DOPS");
    }

    [Fact]
    public void EveryIdOnThePage_IsUnique_WithAnItemOpenForEdit()
    {
        // Two checkbox groups for the same vocabulary on one page. Shared ids would make every label in the
        // second group toggle the first group's box, and a screen reader would announce the wrong control.
        var cut = RenderPage(new FakeSender());

        BeginEdit(cut, "PAED-003");

        var ids = cut.FindAll("[id]").Select(element => element.Id!).ToList();
        ids.Should().OnlyHaveUniqueItems();

        // And every label in either group points at a control that exists.
        foreach (var label in cut.FindAll("fieldset label[for]").ToList())
        {
            ids.Should().Contain(label.GetAttribute("for")!);
        }
    }

    // ---- The two models are independent ----

    [Fact]
    public void OpeningAnItem_DoesNotTouchTheAddForm()
    {
        var cut = RenderPage(new FakeSender());

        BeginEdit(cut, "PAED-001");

        IsChecked(cut, "add-tool-cbd").Should().BeFalse("the Add form is not the row being edited");
        IsChecked(cut, "add-tool-mini_cex").Should().BeFalse();

        // The other fields too: the defect was the whole model, not just the new list.
        cut.Find("#curriculum-item-count").GetAttribute("value").Should().Be("1");
    }

    [Fact]
    public void TickingInOneGroup_DoesNotTickTheOther()
    {
        var cut = RenderPage(new FakeSender());

        cut.Find("#add-tool-msf").Change(true);
        BeginEdit(cut, "PAED-001");
        cut.Find("#edit-tool-dops").Change(true);

        IsChecked(cut, "add-tool-msf").Should().BeTrue("opening an item must not reset the Add form");
        IsChecked(cut, "edit-tool-msf").Should().BeFalse("the Add form's tick must not leak into the item");
        IsChecked(cut, "edit-tool-dops").Should().BeTrue();
        IsChecked(cut, "add-tool-dops").Should().BeFalse("the item's tick must not leak into the Add form");
    }

    // ---- What reaches the server ----

    [Fact]
    public void SavingAnUnchangedItem_SendsTheSameToolKeys()
    {
        // The regression that matters most: an operator fixes a target and saves, and the tool list goes with it.
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        BeginEdit(cut, "PAED-001");
        ClickButton(cut, "Save");

        var command = sender.Updates.Should().ContainSingle().Subject;
        command.ItemId.Should().Be(11);
        command.PermittedToolKeys.Should().BeEquivalentTo(["cbd", "mini_cex"]);

        // And the rest of the item rode along unchanged, so the unchanged save really is unchanged.
        command.EpaId.Should().Be(1);
        command.RequiredCount.Should().Be(3);
        command.QuotaPeriod.Should().Be(QuotaPeriod.Semester);
    }

    [Fact]
    public void SavingAnUnchangedItemWithNoList_SendsAnEmptyList()
    {
        // Empty is "any instrument", and it must stay empty — not become null-because-forgotten on one save
        // and something else on the next.
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        BeginEdit(cut, "PAED-002");
        ClickButton(cut, "Save");

        sender.Updates.Should().ContainSingle()
            .Which.PermittedToolKeys.Should().BeEmpty();
    }

    [Fact]
    public void SavingAnUnchangedItemWithAnOrphan_SendsTheOrphanToo()
    {
        // The page does not quietly drop a key it cannot name. Whether the server accepts it is the server's
        // decision, made visibly; an editor that dropped it would change the item without being asked.
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        BeginEdit(cut, "PAED-003");
        ClickButton(cut, "Save");

        sender.Updates.Should().ContainSingle()
            .Which.PermittedToolKeys.Should().BeEquivalentTo(["legacy_tool", "mini_cex"]);
    }

    [Fact]
    public void TickingAndUnticking_ChangesExactlyWhatIsSent()
    {
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        BeginEdit(cut, "PAED-003");
        cut.Find("#edit-tool-legacy_tool").Change(false);
        cut.Find("#edit-tool-dops").Change(true);
        ClickButton(cut, "Save");

        sender.Updates.Should().ContainSingle()
            .Which.PermittedToolKeys.Should().BeEquivalentTo(["dops", "mini_cex"]);

        // The saved list is what the row now shows.
        ToolsCell(cut, "PAED-003").Should().Be("DOPS, Mini-CEX");
    }

    [Fact]
    public void ARejectedSave_KeepsTheRowOpen_WithTheOperatorsTicks()
    {
        // RefreshCurriculumAsync returns false on a refusal so the row stays open. A tool list is the new common
        // refusal (an unknown key), and closing the row would throw away the ticks the error is about.
        var sender = new FakeSender { UpdateFailure = new InvalidOperationException("Unknown tool key 'legacy_tool'.") };
        var cut = RenderPage(sender);

        BeginEdit(cut, "PAED-003");
        cut.Find("#edit-tool-dops").Change(true);
        ClickButton(cut, "Save");

        cut.Markup.Should().Contain("Unknown tool key");
        IsChecked(cut, "edit-tool-dops").Should().BeTrue();
        IsChecked(cut, "edit-tool-legacy_tool").Should().BeTrue();
    }

    [Fact]
    public void CancellingAnEdit_DiscardsTheTicks()
    {
        // BeginEdit copies the item's list. Reopening after Cancel must show what is stored, not what was
        // ticked and abandoned.
        var cut = RenderPage(new FakeSender());

        BeginEdit(cut, "PAED-001");
        cut.Find("#edit-tool-cbd").Change(false);
        ClickButton(cut, "Cancel");
        BeginEdit(cut, "PAED-001");

        IsChecked(cut, "edit-tool-cbd").Should().BeTrue();
    }

    [Fact]
    public void AddingAnItem_SendsTheTickedTools_AndThenClearsThem()
    {
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        cut.Find("#add-tool-dops").Change(true);
        cut.Find("#add-tool-mini_cex").Change(true);
        cut.Find("form").Submit();

        sender.Adds.Should().ContainSingle()
            .Which.PermittedToolKeys.Should().BeEquivalentTo(["dops", "mini_cex"]);

        // A successful add resets the form, so the next item does not inherit this one's instruments.
        IsChecked(cut, "add-tool-dops").Should().BeFalse();
        IsChecked(cut, "add-tool-mini_cex").Should().BeFalse();
    }

    [Fact]
    public void AddingAnItemWithNothingTicked_SendsAnEmptyList()
    {
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        cut.Find("form").Submit();

        sender.Adds.Should().ContainSingle()
            .Which.PermittedToolKeys.Should().NotBeNull().And.BeEmpty();
    }

    // ---- helpers ----

    private IRenderedComponent<CurriculumItemsEdit> RenderPage(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<CurriculumItemsEdit>(parameters => parameters.Add(page => page.Id, CurriculumId));
        cut.WaitForState(() => cut.FindAll("tbody tr").Count > 0);

        return cut;
    }

    private static IReadOnlyList<string> HeaderTexts(IRenderedComponent<CurriculumItemsEdit> cut)
        => cut.FindAll("thead th").Select(header => header.TextContent.Trim()).ToList();

    // A row in display mode: the edit row's first cell is a select listing every EPA, so it is excluded by shape.
    private static IElement DisplayRow(IRenderedComponent<CurriculumItemsEdit> cut, string epaCode)
        => cut.FindAll("tbody tr")
            .Where(row => row.QuerySelector("select") is null)
            .First(row => row.QuerySelectorAll("td").FirstOrDefault()?.TextContent.Trim().StartsWith(epaCode, StringComparison.Ordinal) == true);

    private static string ToolsCell(IRenderedComponent<CurriculumItemsEdit> cut, string epaCode)
    {
        var toolsIndex = HeaderTexts(cut).ToList().IndexOf("Tools");
        return DisplayRow(cut, epaCode).QuerySelectorAll("td")
            .Select(cell => cell.TextContent.Trim())
            .ElementAt(toolsIndex);
    }

    private static void BeginEdit(IRenderedComponent<CurriculumItemsEdit> cut, string epaCode)
        => DisplayRow(cut, epaCode).QuerySelectorAll("button")
            .First(button => button.TextContent.Trim() == "Edit")
            .Click();

    private static void ClickButton(IRenderedComponent<CurriculumItemsEdit> cut, string label)
        => cut.FindAll("button")
            .First(button => string.Equals(button.TextContent.Trim(), label, StringComparison.Ordinal))
            .Click();

    private static IReadOnlyList<string> EditCheckboxIds(IRenderedComponent<CurriculumItemsEdit> cut)
        => cut.FindAll("tbody fieldset input[type=checkbox]").Select(input => input.Id!).ToList();

    private static IReadOnlyList<string> AddCheckboxIds(IRenderedComponent<CurriculumItemsEdit> cut)
        => cut.FindAll("form fieldset input[type=checkbox]").Select(input => input.Id!).ToList();

    private static bool IsChecked(IRenderedComponent<CurriculumItemsEdit> cut, string id)
        => cut.Find($"#{id}").HasAttribute("checked");

    private static string LabelFor(IRenderedComponent<CurriculumItemsEdit> cut, string id)
        => cut.Find($"label[for='{id}']").TextContent.Trim();

    private static EpaDto Epa(int id, string code)
        => new(id, SubSpecialityId, "General Paediatrics", "CMSA", code, $"{code} title", null, null, EpaCategory.Core, true,
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    private static CurriculumItemDto Item(int id, int epaId, int requiredCount, QuotaPeriod period, string? permittedToolsJson)
    {
        var epa = Epas.Single(candidate => candidate.Id == epaId);
        return new CurriculumItemDto(id, epaId, epa.Code, epa.Title, requiredCount, period, 3, 12, null, null, permittedToolsJson);
    }

    private static CurriculumDto Curriculum(IReadOnlyList<CurriculumItemDto> items)
        => new(CurriculumId, 2, SubSpecialityId, "Paediatrics", "General Paediatrics", "CMSA", "Paediatrics v11.1", "11.1",
            new DateOnly(2026, 1, 1), null, true, true, items);

    private sealed class FakeSender : IScopedSender
    {
        private List<CurriculumItemDto> _items =
        [
            Item(11, 1, 3, QuotaPeriod.Semester, """["cbd","mini_cex"]"""),
            Item(12, 2, 3, QuotaPeriod.AcademicYear, null),
            Item(13, 3, 2, QuotaPeriod.Semester, """["legacy_tool","mini_cex"]""")
        ];

        public List<UpdateCurriculumItemCommand> Updates { get; } = [];

        public List<AddCurriculumItemCommand> Adds { get; } = [];

        public Exception? UpdateFailure { get; init; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object response = request switch
            {
                GetCurriculumByIdQuery => Curriculum(_items),
                ListEpasForSubSpecialityQuery => Epas,
                GetEntrustmentScalesListQuery => (IReadOnlyList<EntrustmentScaleDto>)[],
                GetWbaToolsQuery => Vocabulary,
                UpdateCurriculumItemCommand update => Update(update),
                AddCurriculumItemCommand add => Add(add),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        private CurriculumDto Update(UpdateCurriculumItemCommand command)
        {
            Updates.Add(command);
            if (UpdateFailure is not null)
            {
                throw UpdateFailure;
            }

            _items = _items
                .Select(item => item.Id == command.ItemId
                    ? item with { PermittedToolsJson = CurriculumItem.NormalizePermittedToolsJson(command.PermittedToolKeys) }
                    : item)
                .ToList();
            return Curriculum(_items);
        }

        private CurriculumDto Add(AddCurriculumItemCommand command)
        {
            Adds.Add(command);
            return Curriculum(_items);
        }
    }
}
