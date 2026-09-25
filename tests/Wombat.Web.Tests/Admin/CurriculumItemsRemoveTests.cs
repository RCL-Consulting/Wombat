using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Curricula;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;
using FakeSender = Wombat.Web.Tests.Admin.CurriculumItemsFakeSender;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T222: a curriculum item's row actions follow DESIGN.md § Button system. Edit and Remove are outline buttons named by
/// the item's EPA, under a named actions header, and Remove asks first, in one ConfirmDialog whose red button is the only
/// red one.
/// </summary>
/// <remarks>
/// Before T222 Remove was a red in-row button that deleted the item at the first click, with its settings and every
/// trainee's tally toward it; a column of "Edit" and "Remove" buttons was indistinguishable to a screen reader; and the
/// actions column's header was empty. The result of a Remove takes the focus once the dialog has closed, as T206's
/// Withdraw does: the row's button that had it is gone with its row.
/// </remarks>
public sealed class CurriculumItemsRemoveTests : WombatTestContext
{
    /// <summary>What <see cref="ElementReference" />.FocusAsync calls.</summary>
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    public CurriculumItemsRemoveTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin@test");
        auth.SetRoles(WombatRoles.Administrator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));

        JSInterop.SetupVoid("wombatDialog.showModal", _ => true).SetVoidResult();
        JSInterop.SetupVoid("wombatDialog.close", _ => true).SetVoidResult();
    }

    [Fact]
    public void EachRowsActions_AreOutlineButtons_NamedByTheItemsEpa()
    {
        var cut = RenderPage(new FakeSender());

        foreach (var code in new[] { "PAED-001", "PAED-002", "PAED-003" })
        {
            var buttons = Row(cut, code).QuerySelectorAll("button").ToList();
            buttons.Select(Text).Should().Equal("Edit", "Remove");
            buttons.Select(button => button.GetAttribute("aria-label")).Should().Equal(
                new[] { $"Edit {code}", $"Remove {code}" }, "each name starts with its label and says which item it acts on");
            buttons.Should().OnlyContain(button =>
                button.ClassList.Contains("btn") && button.ClassList.Contains("btn-sm") && button.ClassList.Contains("btn-outline")
                && !button.ClassList.Contains("btn-danger") && button.GetAttribute("type") == "button");
        }
    }

    private const string GrooteSchuur = "Groote Schuur Hospital";
    private const string RedCross = "Red Cross War Memorial Children's Hospital";

    /// <summary>
    /// A national item and two institutions' own items on one EPA, as an Administrator reads them, each local row naming its
    /// owner (<c>CurriculumAdminScope.NamesItemOwners</c>). T223 lets two institutions each hold an item of their own on one EPA.
    /// </summary>
    private static FakeSender TwoInstitutionsOnOneEpa()
        => new(
        [
            FakeSender.Item(11, 1, 3, QuotaPeriod.Semester, null),
            FakeSender.Item(21, 2, 3, QuotaPeriod.AcademicYear, null, owningInstitutionId: 40, owningInstitutionName: GrooteSchuur),
            FakeSender.Item(22, 2, 2, QuotaPeriod.AcademicYear, null, owningInstitutionId: 41, owningInstitutionName: RedCross)
        ]);

    [Fact]
    public void TwoInstitutionsOwnItemsOnOneEpa_AreNamedApart_ByWhoseItemEachIs()
    {
        // T223 review: named by the EPA alone, the page had two "Edit PAED-002" and two "Remove PAED-002" buttons, which a
        // screen reader could not tell apart. A national item cannot share an EPA with either, so its EPA still names it.
        var cut = RenderPage(TwoInstitutionsOnOneEpa());

        var names = cut.FindAll("tbody .actions-cell button").Select(button => button.GetAttribute("aria-label")).ToList();
        names.Should().Equal(
            "Edit PAED-001", "Remove PAED-001",
            $"Edit PAED-002 ({GrooteSchuur}'s own item)", $"Remove PAED-002 ({GrooteSchuur}'s own item)",
            $"Edit PAED-002 ({RedCross}'s own item)", $"Remove PAED-002 ({RedCross}'s own item)");
        names.Should().OnlyHaveUniqueItems("a screen reader must be able to tell every button apart");

        // The edit row's legend, the name of the group of fields under it, says which of the two is open.
        ActionButton(cut, $"Edit PAED-002 ({RedCross}'s own item)").Click();
        cut.WaitForAssertion(() => Text(cut.Find("tr.is-editing td[colspan] > fieldset > legend"))
            .Should().Be($"Edit PAED-002 ({RedCross}'s own item)"));
    }

    [Fact]
    public void RemovingOneOfTwoItemsOnAnEpa_SaysWhoseWasRemoved()
    {
        var sender = TwoInstitutionsOnOneEpa();
        var cut = RenderPage(sender);

        ActionButton(cut, $"Remove PAED-002 ({GrooteSchuur}'s own item)").Click();
        ConfirmRemove(cut);

        sender.Removes.Should().ContainSingle().Which.ItemId.Should().Be(21);
        cut.WaitForAssertion(() => Text(cut.Find(".alert.alert-success"))
            .Should().Be($"PAED-002 ({GrooteSchuur}'s own item) removed from this curriculum.", "another PAED-002 item is still listed"));
    }

    [Fact]
    public void AnItemOfTheCallersOwnInstitution_IsNamedByItsEpaAlone()
    {
        // An InstitutionalAdmin's rows name no owner: every local item they read is their own institution's, and a national
        // item cannot share its EPA, so no two of their rows share an EPA.
        var cut = RenderPage(new FakeSender(
        [
            FakeSender.Item(11, 1, 3, QuotaPeriod.Semester, null),
            FakeSender.Item(21, 2, 3, QuotaPeriod.AcademicYear, null, owningInstitutionId: 40)
        ]));

        cut.FindAll("tbody .actions-cell button").Select(button => button.GetAttribute("aria-label"))
            .Should().Equal("Edit PAED-001", "Remove PAED-001", "Edit PAED-002", "Remove PAED-002");
    }

    [Fact]
    public void TheActionsColumnsHeader_IsNamed_ForAScreenReaderOnly()
    {
        var cut = RenderPage(new FakeSender());

        var header = cut.FindAll("thead th").Last();
        var hidden = header.QuerySelector(".visually-hidden");
        hidden.Should().NotBeNull("an empty header names nothing");
        Text(hidden!).Should().Be("Actions");
        Text(header).Should().Be("Actions", "nothing else is in it");
    }

    [Fact]
    public void Remove_OpensADialogNamingTheItem_AndSendsNothingUntilConfirmed()
    {
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        RemoveButton(cut, "PAED-002").Click();

        JSInterop.VerifyInvoke("wombatDialog.showModal");
        sender.Removes.Should().BeEmpty("opening the dialog must not remove the item");

        var dialog = Text(cut.Find("dialog"));
        dialog.Should().Contain("Remove this item?")
            .And.Contain("Remove PAED-002 - PAED-002 title from Paediatrics v11.1 (11.1)?")
            .And.Contain("Its target, minima, decision and tool list are deleted, and so is each trainee's tally toward it.")
            .And.Contain("This cannot be undone.");
        cut.FindAll("dialog button").Single(button => Text(button) == "Remove item").ClassList.Should().Contain("btn-danger");
        cut.FindAll(".btn-danger").Should().ContainSingle("the red button is only in the dialog's footer");
    }

    [Theory]
    [InlineData(null, null,
        "Trainees on this curriculum, in every institution that has adopted it, are no longer measured against this EPA.")]
    [InlineData(40, "Groote Schuur Hospital",
        "Groote Schuur Hospital's trainees on this curriculum are no longer measured against this EPA.")]
    [InlineData(40, null,
        "Your institution's trainees on this curriculum are no longer measured against this EPA.")]
    public void TheDialog_SaysWhoseTraineesTheItemStopsMeasuring(int? owningInstitutionId, string? ownerName, string sentence)
    {
        // T222 review: a national item measures the trainees of every adopting institution, an institution's own item only
        // that institution's. The owner is named where the row names it (an Administrator), else it is the caller's own.
        var sender = new FakeSender(
        [
            FakeSender.Item(11, 1, 3, QuotaPeriod.Semester, null),
            FakeSender.Item(12, 2, 3, QuotaPeriod.AcademicYear, null, owningInstitutionId: owningInstitutionId, owningInstitutionName: ownerName)
        ]);
        var cut = RenderPage(sender);

        RemoveButton(cut, "PAED-002").Click();

        var body = Text(cut.Find("dialog p"));
        body.Should().Contain(sentence);
        body.Should().EndWith($"{sentence} This cannot be undone.");
    }

    [Fact]
    public void CancellingTheDialog_SendsNothing_AndKeepsTheItem()
    {
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        RemoveButton(cut, "PAED-002").Click();
        cut.FindAll("dialog button").Single(button => Text(button) == "Cancel").Click();

        sender.Removes.Should().BeEmpty();
        Row(cut, "PAED-002").Should().NotBeNull();
        Text(cut.Find("dialog")).Should().NotContain("PAED-002", "the dialog no longer asks about it");
    }

    [Fact]
    public void ConfirmingTheDialog_RemovesThatItem_AndSaysWhichWasRemoved()
    {
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        RemoveButton(cut, "PAED-002").Click();
        ConfirmRemove(cut);

        sender.Removes.Should().ContainSingle().Which.ItemId.Should().Be(12);
        cut.WaitForAssertion(() => Text(cut.Find(".alert.alert-success")).Should().Be("PAED-002 removed from this curriculum."));
        cut.FindAll("tbody tr").Select(row => Text(row.QuerySelector("td")!).Split(' ')[0])
            .Should().Equal("PAED-001", "PAED-003");
    }

    [Fact]
    public void ARemovesResult_IsAnnounced_AndTakesTheFocusOnceTheDialogHasClosed()
    {
        var cut = RenderPage(new FakeSender());

        RemoveButton(cut, "PAED-002").Click();
        ConfirmRemove(cut);

        cut.WaitForAssertion(() => cut.Find(".alert.alert-success").GetAttribute("role").Should().Be("status"));
        ResultTookTheFocusAfterTheDialogClosed(cut);
    }

    [Fact]
    public void ARefusedRemove_IsAnnouncedAsAnAlert_TakesTheFocus_AndKeepsTheItem()
    {
        var cut = RenderPage(new FakeSender
        {
            RemoveFailure = new UnauthorizedAccessException("You do not have permission to modify this curriculum.")
        });

        RemoveButton(cut, "PAED-002").Click();
        ConfirmRemove(cut);

        cut.WaitForAssertion(() => Text(cut.Find(".alert.alert-danger")).Should().Be("You do not have permission to modify this curriculum."));
        cut.Find(".alert.alert-danger").GetAttribute("role").Should().Be("alert");
        cut.FindAll(".alert.alert-success").Should().BeEmpty();
        Row(cut, "PAED-002").Should().NotBeNull();
        ResultTookTheFocusAfterTheDialogClosed(cut);
    }

    public enum RemoveStage
    {
        // The command has been sent and has not answered.
        Command,

        // The command has answered, and the page is asking its pickers again (T222): still part of the Remove, though
        // the command's own in-flight flag is already down.
        PickerRefresh
    }

    [Theory]
    [InlineData(RemoveStage.Command)]
    [InlineData(RemoveStage.PickerRefresh)]
    public void ASecondConfirmWhileTheFirstRemoveRuns_SendsNothing_AndRemoveIsOffMeanwhile(RemoveStage heldAt)
    {
        // As T206's Withdraw: the second confirm can reach the circuit before the render that closes the dialog does.
        var sender = new FakeSender
        {
            HoldRemoves = heldAt == RemoveStage.Command,
            HoldPickersAfterARemove = heldAt == RemoveStage.PickerRefresh
        };
        var cut = RenderPage(sender);

        RemoveButton(cut, "PAED-002").Click();
        ConfirmRemove(cut);
        RemoveButton(cut, "PAED-003").HasAttribute("disabled").Should().BeTrue("a Remove is in flight");
        ConfirmRemove(cut);

        sender.Release();
        cut.WaitForAssertion(() => cut.FindAll(".alert.alert-success").Should().ContainSingle(), AsyncWorkTimeout);
        sender.Removes.Should().ContainSingle();
        cut.WaitForAssertion(() => RemoveButton(cut, "PAED-003").HasAttribute("disabled").Should().BeFalse(), AsyncWorkTimeout);
    }

    // ---- helpers ----

    private void ResultTookTheFocusAfterTheDialogClosed(IRenderedComponent<CurriculumItemsEdit> cut)
    {
        // bUnit on .NET 10 leaves an element's blazor:elementReference empty in the markup, so the focused reference is
        // matched to the page's own result region, the one element that captures it.
        cut.Find(".action-result").GetAttribute("tabindex").Should().Be("-1");
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.ResultRegion.Id));

        // While the modal is open, nothing outside it can take the focus.
        var calls = JSInterop.Invocations.Select(invocation => invocation.Identifier).ToList();
        calls.IndexOf("wombatDialog.close").Should().BeGreaterThan(-1)
            .And.BeLessThan(calls.IndexOf(FocusIdentifier), "the dialog closes before the result takes the focus");
    }

    private IRenderedComponent<CurriculumItemsEdit> RenderPage(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<CurriculumItemsEdit>(parameters => parameters.Add(page => page.Id, FakeSender.CurriculumId));
        cut.WaitForState(() => cut.FindAll("tbody tr").Count > 0);

        return cut;
    }

    private static void ConfirmRemove(IRenderedComponent<CurriculumItemsEdit> cut)
        => cut.FindAll("dialog button").Single(button => Text(button) == "Remove item").Click();

    private static IElement Row(IRenderedComponent<CurriculumItemsEdit> cut, string epaCode)
        => cut.FindAll("tbody tr")
            .Where(row => row.QuerySelector("td[colspan]") is null)
            .Single(row => Text(row.QuerySelector("td")!).StartsWith(epaCode, StringComparison.Ordinal));

    private static IElement RemoveButton(IRenderedComponent<CurriculumItemsEdit> cut, string epaCode)
        => Row(cut, epaCode).QuerySelectorAll("button").Single(button => Text(button) == "Remove");

    /// <summary>The one row action with this accessible name.</summary>
    private static IElement ActionButton(IRenderedComponent<CurriculumItemsEdit> cut, string name)
        => cut.FindAll("tbody .actions-cell button").Single(button => button.GetAttribute("aria-label") == name);

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();
}
