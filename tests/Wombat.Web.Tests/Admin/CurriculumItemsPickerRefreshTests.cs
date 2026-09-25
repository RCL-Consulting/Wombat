using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Curricula;
using Wombat.Web.Services;
using FakeSender = Wombat.Web.Tests.Admin.CurriculumItemsFakeSender;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T222: the item editor's EPA pickers offer only EPAs the curriculum does not already hold, are asked again after each
/// Add, Save and Remove, and the Add form says so when nothing is left to add.
/// </summary>
/// <remarks>
/// <para>
/// Which EPAs are left out is the Application's rule (<c>CurriculumAdminScope.HoldsItsEpaAgainst</c>, answered by
/// <c>ListCurriculumItemEpaOptionsQuery</c>); these tests pin the page's half. The fake answers each picker from the
/// items it holds now, as the query would, so a page that asked once and kept the answer shows an EPA the command would
/// refuse. Before T222 the Add picker on the v11.1 curriculum offered its fifteen EPAs, all already items, and the Add
/// command refused every one with "already contains".
/// </para>
/// <para>
/// The curriculum holds PAED-001 to PAED-003 (items 11 to 13); its sub-speciality also has PAED-004 and PAED-005.
/// </para>
/// </remarks>
public sealed class CurriculumItemsPickerRefreshTests : TestContext
{
    private static readonly IReadOnlyList<EpaDto> EveryEpa = [.. FakeSender.Epas, .. FakeSender.MoreEpas];

    public CurriculumItemsPickerRefreshTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin@test");
        auth.SetRoles(WombatRoles.Administrator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));

        JSInterop.SetupVoid("wombatDialog.showModal", _ => true).SetVoidResult();
        JSInterop.SetupVoid("wombatDialog.close", _ => true).SetVoidResult();
    }

    // ---- Nothing left to add ----

    [Theory]
    [InlineData(true, "Add item",
        "Every national EPA of General Paediatrics is already on this curriculum, as a national item or as an institution's own item, so there is none left to add. To add an item, first add a national EPA of General Paediatrics, or remove a national item to free its EPA. An EPA an institution has as its own item is freed only when that institution removes it.")]
    [InlineData(false, "Add your institution's own item",
        "Every EPA your institution could add is already on this curriculum: each national EPA of General Paediatrics, and each of your institution's own EPAs of General Paediatrics. To add an item of your own, first create a local EPA of General Paediatrics.")]
    public void TheAddForm_WithNothingLeftToAdd_SaysSo_InsteadOfOfferingAnEmptyPicker(bool canEditCurriculum, string heading, string emptyText)
    {
        // The College's view makes a national item, an institution's one of its own; the empty state speaks of the item
        // the form would have made, as its heading does.
        var cut = RenderPage(new FakeSender { CanEditCurriculum = canEditCurriculum, EpaOptionsFor = itemId => itemId is null ? [] : null });

        var container = cut.Find(".form-container");
        Text(container.QuerySelector("h3")!).Should().Be(heading);
        var empty = container.QuerySelector(".detail-card--empty-compact");
        empty.Should().NotBeNull();
        Text(empty!).Should().Be(emptyText);
        empty!.GetAttribute("tabindex").Should().Be("-1", "the page can move the focus to it, and Tab does not stop on it");

        cut.FindAll("#curriculum-item-epa").Should().BeEmpty("no empty select");
        cut.FindAll("form").Should().BeEmpty("no Add button with nothing to add");
    }

    [Fact]
    public void TheAddForm_WithSomethingToAdd_OffersIt_AndNoEmptyState()
    {
        var cut = RenderPage(Stateful());

        Options(cut, "curriculum-item-epa").Should().Equal("PAED-004", "PAED-005");
        cut.FindAll(".detail-card--empty-compact").Should().BeEmpty();
    }

    // ---- Asked again after each command ----

    [Fact]
    public void AfterAnAdd_TheAddPickerIsAskedAgain_AndNoLongerOffersTheEpaJustAdded()
    {
        var sender = Stateful();
        var cut = RenderPage(sender);

        cut.Find("#curriculum-item-epa").Change("5");
        AddWithMinimum(cut);

        sender.Adds.Should().ContainSingle().Which.EpaId.Should().Be(5);
        cut.WaitForAssertion(() => Options(cut, "curriculum-item-epa").Should().Equal("PAED-004"));
        sender.EpaOptionQueries.Count(query => query.ItemId is null).Should().Be(2, "once on load, once after the Add");
        cut.Find("#curriculum-item-epa").GetAttribute("value").Should().Be("4", "the reset form selects the first EPA still on offer");
    }

    [Fact]
    public void AnAddThatTakesTheLastEpa_LeavesTheEmptyState_WhichTakesTheFocus_AndSaysFirstWhatTheAddDid()
    {
        // The Add button went with the form, and the focus would otherwise fall to the page. The result alert at the top
        // arrives already filled just as the focus moves, and may not be read (T222 review), so the empty state the focus
        // lands on starts with it.
        var sender = Stateful(nameable: [.. FakeSender.Epas, FakeSender.MoreEpas[0]]);
        var cut = RenderPage(sender);
        Options(cut, "curriculum-item-epa").Should().Equal("PAED-004");

        AddWithMinimum(cut);

        cut.WaitForAssertion(() => cut.FindAll(".form-container .detail-card--empty-compact").Should().ContainSingle());
        Text(cut.Find(".alert.alert-success")).Should().Be("Curriculum item added.");
        var empty = cut.Find(".form-container .detail-card--empty-compact");
        Text(empty).Should().Be($"Curriculum item added. {NationalEmptyText}");
        empty.QuerySelectorAll(".text-danger").Should().BeEmpty("it is not a refusal");
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.AddEmptyState.Id));

        // The result goes with every other result as the next action starts.
        BeginEdit(cut, "PAED-001");
        Text(cut.Find(".form-container .detail-card--empty-compact")).Should().Be(NationalEmptyText);
    }

    [Fact]
    public void AfterARemove_ItsEpaIsOfferedAgain_AndTheAddFormKeepsItsChoice()
    {
        var sender = Stateful();
        var cut = RenderPage(sender);
        cut.Find("#curriculum-item-epa").Change("5");

        Remove(cut, "PAED-002");

        cut.WaitForAssertion(() => Options(cut, "curriculum-item-epa").Should().Equal("PAED-002", "PAED-004", "PAED-005"));
        cut.Find("#curriculum-item-epa").GetAttribute("value").Should().Be("5",
            "what the operator had chosen is still on offer, so it is kept");
    }

    [Fact]
    public void AfterASave_TheEpaTheItemLeftIsOffered_AndTheOneItNowNamesIsNot()
    {
        var sender = Stateful();
        var cut = RenderPage(sender);

        BeginEdit(cut, "PAED-003");
        Options(cut, "edit-epa").Should().Equal(new[] { "PAED-003", "PAED-004", "PAED-005" },
            "an edit row offers its own item's EPA and the free ones, not another item's");
        sender.EpaOptionQueries.Last().ItemId.Should().Be(13);

        cut.Find("#edit-epa").Change("4");
        ClickIn(cut.Find("tr.is-editing td[colspan]"), "Save");

        sender.Updates.Should().ContainSingle().Which.EpaId.Should().Be(4);
        cut.WaitForAssertion(() => Options(cut, "curriculum-item-epa").Should().Equal("PAED-003", "PAED-005"));
    }

    [Fact]
    public void AnOpenEditRow_IsAskedAgain_WhenARemoveFreesAnEpa()
    {
        var sender = Stateful();
        var cut = RenderPage(sender);

        BeginEdit(cut, "PAED-001");
        Options(cut, "edit-epa").Should().Equal("PAED-001", "PAED-004", "PAED-005");

        Remove(cut, "PAED-002");

        cut.WaitForAssertion(() => Options(cut, "edit-epa").Should().Equal("PAED-001", "PAED-002", "PAED-004", "PAED-005"));
        sender.EpaOptionQueries.Last(query => query.ItemId is not null).ItemId.Should().Be(11);
        cut.Find("#edit-epa").GetAttribute("value").Should().Be("1", "the row keeps its choice");
    }

    [Fact]
    public void AnOpenEditRowsChoice_ThatTheAddFormTakes_GoesBackToTheItemsStoredEpa()
    {
        // The row had moved PAED-001's item to PAED-004, and the Add form then put PAED-004 on the curriculum. The row's
        // choice is no longer on offer; it goes back to the stored EPA rather than show a select with nothing selected
        // and a note naming the wrong EPA.
        var sender = Stateful();
        var cut = RenderPage(sender);

        BeginEdit(cut, "PAED-001");
        cut.Find("#edit-epa").Change("4");
        cut.Find("#curriculum-item-epa").Change("4");
        AddWithMinimum(cut);

        cut.WaitForAssertion(() => Options(cut, "edit-epa").Should().Equal("PAED-001", "PAED-005"));
        cut.Find("#edit-epa").GetAttribute("value").Should().Be("1");
        cut.FindAll("#edit-epa-refused").Should().BeEmpty("the stored EPA is one the item can name");
    }

    // ---- After a refusal: the page reads the curriculum and its pickers again (T222 review) ----

    [Fact]
    public void ARefusedAdd_OfAnEpaAddedElsewhere_ReadsThePageAgain_SoThePickerNoLongerOffersIt_AndKeepsTheRefusal()
    {
        // Another tab put PAED-004 on the curriculum after this page asked its picker. Without the reload the picker went on
        // offering PAED-004 and every Add of it was refused, until the page was reloaded by hand.
        var sender = Stateful();
        var cut = RenderPage(sender);
        cut.Find("#curriculum-item-epa").Change("4");
        sender.AddElsewhere(21, 4);

        AddWithMinimum(cut);

        sender.Adds.Should().ContainSingle();
        cut.WaitForAssertion(() => Options(cut, "curriculum-item-epa").Should().Equal("PAED-005"));
        var refusal = cut.FindAll("[role=alert]").Should().ContainSingle().Subject;
        refusal.Closest("form").Should().NotBeNull("the refusal stays where the operator is looking");
        Text(refusal).Should().Be("This curriculum already contains the selected EPA.");
        Row(cut, "PAED-004").Should().NotBeNull("the list shows the item added elsewhere");
        sender.CurriculumReads.Should().Be(2, "once on load, once after the refusal");
        cut.Find("#curriculum-item-epa").GetAttribute("value").Should().Be("5", "the refused EPA is no longer on offer");
        cut.Find("#curriculum-item-level").GetAttribute("value").Should().Be("3", "the rest of what was picked is kept");
    }

    [Fact]
    public void ARefusedAdd_ThatLeavesNothingToAdd_PutsTheRefusalAtTheHeadOfTheEmptyState_WhichTakesTheFocus()
    {
        // The form, and the refusal inside it, are gone with the picker's last EPA.
        var sender = Stateful(nameable: [.. FakeSender.Epas, FakeSender.MoreEpas[0]]);
        var cut = RenderPage(sender);
        sender.AddElsewhere(21, 4);

        AddWithMinimum(cut);

        cut.WaitForAssertion(() => cut.FindAll("form").Should().BeEmpty());
        var empty = cut.Find(".form-container .detail-card--empty-compact");
        Text(empty).Should().Be($"This curriculum already contains the selected EPA. {NationalEmptyText}");
        Text(empty.QuerySelector(".text-danger")!).Should().Be("This curriculum already contains the selected EPA.");
        cut.FindAll(".alert-success, .alert-danger").Should().BeEmpty("no success, and the refusal is not also shown elsewhere");
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.AddEmptyState.Id));
    }

    [Fact]
    public void ARefusedSave_OfAnEpaAddedElsewhere_ReadsThePageAgain_AndTheRowGoesBackToItsStoredEpa()
    {
        var sender = Stateful();
        var cut = RenderPage(sender);
        BeginEdit(cut, "PAED-001");
        cut.Find("#edit-epa").Change("4");
        sender.AddElsewhere(21, 4);

        ClickIn(cut.Find("tr.is-editing td[colspan]"), "Save");

        sender.Updates.Should().ContainSingle().Which.EpaId.Should().Be(4);
        cut.WaitForAssertion(() => Options(cut, "edit-epa").Should().Equal("PAED-001", "PAED-005"));
        cut.Find("#edit-epa").GetAttribute("value").Should().Be("1");
        var refusal = cut.FindAll("[role=alert]").Should().ContainSingle().Subject;
        refusal.Closest("td[colspan]").Should().NotBeNull("the row stays open with its refusal beside Save");
        Text(refusal).Should().Be("This curriculum already contains the selected EPA.");
        Options(cut, "curriculum-item-epa").Should().Equal("PAED-005");
    }

    [Fact]
    public void ARefusedSave_OfAnItemRemovedElsewhere_ClosesTheRow_AndTheRefusalMovesToTheTopAndTakesTheFocus()
    {
        var sender = Stateful();
        var cut = RenderPage(sender);
        BeginEdit(cut, "PAED-003");
        var editPickerAsks = sender.EpaOptionQueries.Count(query => query.ItemId is not null);
        sender.RemoveElsewhere(13);

        ClickIn(cut.Find("tr.is-editing td[colspan]"), "Save");

        cut.WaitForAssertion(() => cut.FindAll("tr.is-editing").Should().BeEmpty("the item, and so its row, is gone"));
        var refusal = cut.FindAll("[role=alert]").Should().ContainSingle().Subject;
        refusal.Closest(".action-result").Should().NotBeNull();
        Text(refusal).Should().Be("The requested curriculum item was not found.");
        cut.FindAll("tbody tr").Select(row => Text(row.QuerySelector("td")!).Split(' ')[0]).Should().Equal("PAED-001", "PAED-002");
        sender.EpaOptionQueries.Count(query => query.ItemId is not null).Should().Be(editPickerAsks,
            "a closed row's picker is not asked, which the query would refuse");
        Options(cut, "curriculum-item-epa").Should().Equal("PAED-003", "PAED-004", "PAED-005");
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.ResultRegion.Id));
    }

    [Fact]
    public void ARefusedRemove_OfAnItemRemovedElsewhere_ReadsThePageAgain_SoItsRowGoes_AndItsEpaIsOffered()
    {
        var sender = Stateful();
        var cut = RenderPage(sender);
        sender.RemoveElsewhere(12);

        Remove(cut, "PAED-002");

        cut.WaitForAssertion(() => Text(cut.Find(".action-result [role=alert]")).Should().Be("The requested curriculum item was not found."));
        cut.FindAll("tbody tr").Select(row => Text(row.QuerySelector("td")!).Split(' ')[0]).Should().Equal("PAED-001", "PAED-003");
        Options(cut, "curriculum-item-epa").Should().Equal("PAED-002", "PAED-004", "PAED-005");
    }

    [Fact]
    public void ARefusedRemove_KeepsItsRefusal_WhenThePickersCannotThenBeAskedAgain()
    {
        // The refusal says why the item was not removed; a failure of the picker asked after it must not replace it.
        var asks = 0;
        var sender = new FakeSender
        {
            RemoveFailure = new InvalidOperationException("The requested curriculum item was not found."),
            EpaOptionsFor = itemId => itemId is null && ++asks > 1 ? throw new InvalidOperationException("The requested curriculum was not found.") : null
        };
        var cut = RenderPage(sender);

        Remove(cut, "PAED-002");

        cut.WaitForAssertion(() => Text(cut.Find(".action-result [role=alert]")).Should().Be("The requested curriculum item was not found."));
        asks.Should().Be(2, "the picker was asked again after the refusal");
    }

    // ---- helpers ----

    private const string NationalEmptyText =
        "Every national EPA of General Paediatrics is already on this curriculum, as a national item or as an institution's own item, so there is none left to add. To add an item, first add a national EPA of General Paediatrics, or remove a national item to free its EPA. An EPA an institution has as its own item is freed only when that institution removes it.";

    /// <summary>
    /// A curriculum holding PAED-001 to PAED-003, whose pickers are answered from the items it holds at each ask, as
    /// <c>ListCurriculumItemEpaOptionsQuery</c> answers: every nameable EPA but those another item holds.
    /// </summary>
    private static FakeSender Stateful(IReadOnlyList<EpaDto>? nameable = null)
    {
        var epas = nameable ?? EveryEpa;
        FakeSender sender = null!;
        sender = new FakeSender
        {
            StoresItems = true,
            EpaOptionsFor = itemId => epas
                .Where(epa => !sender.CurrentItems.Any(item => item.Id != itemId && item.EpaId == epa.Id))
                .ToList()
        };
        return sender;
    }

    private IRenderedComponent<CurriculumItemsEdit> RenderPage(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<CurriculumItemsEdit>(parameters => parameters.Add(page => page.Id, FakeSender.CurriculumId));
        cut.WaitForState(() => cut.FindAll("tbody tr").Count > 0);

        return cut;
    }

    /// <summary>Fills the one field the Add form cannot default (an unpinned minimum) and submits it.</summary>
    private static void AddWithMinimum(IRenderedComponent<CurriculumItemsEdit> cut)
    {
        cut.Find("#curriculum-item-level").Change("3");
        cut.Find("form").Submit();
    }

    private static void Remove(IRenderedComponent<CurriculumItemsEdit> cut, string epaCode)
    {
        ClickIn(Row(cut, epaCode), "Remove");
        cut.FindAll("dialog button").Single(button => Text(button) == "Remove item").Click();
    }

    private static void BeginEdit(IRenderedComponent<CurriculumItemsEdit> cut, string epaCode)
        => ClickIn(Row(cut, epaCode), "Edit");

    // The option labels' codes, in the order the select lists them.
    private static IReadOnlyList<string> Options(IRenderedComponent<CurriculumItemsEdit> cut, string selectId)
        => cut.FindAll($"#{selectId} option")
            .Select(option => option.TextContent.Trim().Split(' ')[0])
            .ToList();

    private static IElement Row(IRenderedComponent<CurriculumItemsEdit> cut, string epaCode)
        => cut.FindAll("tbody tr")
            .Where(row => row.QuerySelector("td[colspan]") is null)
            .Single(row => Text(row.QuerySelector("td")!).StartsWith(epaCode, StringComparison.Ordinal));

    private static void ClickIn(IElement container, string label)
        => container.QuerySelectorAll("button").First(button => Text(button) == label).Click();

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();
}
