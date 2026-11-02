using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Wombat.Application.Features.Curricula;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Shared;
using FakeSender = Wombat.Web.Tests.Admin.CurriculumItemsFakeSender;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T222, T392 (flow 15, T3, T11, N6, S10; R3-B13, B14): a row's Edit (a link to the item's page) and Remove… are outline
/// actions named by the item, and Remove… asks first in one ConfirmDialog titled by the item, whose red button is the only
/// red one. The result, or the refusal in the command's words (T312) or fixed words (T329), takes the focus at the list's
/// top once the dialog has closed.
/// </summary>
public sealed class CurriculumItemsRemoveTests : CurriculumItemsTestBase
{
    private const string GrooteSchuur = "Groote Schuur Hospital";
    private const string RedCross = "Red Cross War Memorial Children's Hospital";

    public CurriculumItemsRemoveTests()
    {
        SignIn(WombatRoles.Administrator);
    }

    [Fact]
    public void EachRowsActions_AreOutlineActions_NamedByTheItemsEpa()
    {
        var cut = RenderList(new FakeSender());

        foreach (var code in new[] { "PAED-001", "PAED-002", "PAED-003" })
        {
            var actions = Row(cut, code).QuerySelectorAll(".actions-cell a, .actions-cell button").ToList();
            actions.Select(Text).Should().Equal("Edit", "Remove…");
            actions.Select(action => action.GetAttribute("aria-label")).Should().Equal($"Edit {code}", $"Remove {code}");
            actions.Should().OnlyContain(action => action.ClassList.Contains("btn") && action.ClassList.Contains("btn-sm")
                                                   && action.ClassList.Contains("btn-outline") && !action.ClassList.Contains("btn-danger"));
        }
    }

    [Fact]
    public void TwoInstitutionsOwnItemsOnOneEpa_AreNamedApart_ByWhoseItemEachIs()
    {
        // T223 review: two institutions may each hold an item of their own on one EPA, and an Administrator reads both.
        var cut = RenderList(TwoInstitutionsOnOneEpa());

        var names = cut.FindAll("tbody .actions-cell a, tbody .actions-cell button").Select(action => action.GetAttribute("aria-label")).ToList();
        names.Should().Equal(
            "Edit PAED-001", "Remove PAED-001",
            $"Edit PAED-002 ({GrooteSchuur}'s own item)", $"Remove PAED-002 ({GrooteSchuur}'s own item)",
            $"Edit PAED-002 ({RedCross}'s own item)", $"Remove PAED-002 ({RedCross}'s own item)");
        names.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Remove_OpensADialogTitledByTheItem_AndSendsNothingUntilConfirmed()
    {
        // T3 (R3-B13): the title names the item; the body is the built one, its dash the em dash (S10).
        var sender = new FakeSender();
        var cut = RenderList(sender);

        RemoveButton(cut, "PAED-002").Click();

        JSInterop.VerifyInvoke("wombatDialog.showModal");
        sender.Removes.Should().BeEmpty();
        Text(cut.Find("dialog h2")).Should().Be("Remove PAED-002 from Paediatrics v11.1 (11.1)?");
        Text(cut.Find("dialog p")).Should().Be(
            "Remove PAED-002 — PAED-002 title from Paediatrics v11.1 (11.1)? Its target, minima, decision and tool list are deleted, and so is each trainee's tally toward it. Trainees on this curriculum, in every institution that has adopted it, are no longer measured against this EPA. This cannot be undone.");
        cut.FindAll("dialog button").Select(Text).Should().Equal("Cancel", "Remove item");
        cut.FindAll(".btn-danger").Should().ContainSingle("the red button is only in the dialog's footer")
            .Which.TextContent.Trim().Should().Be("Remove item");
    }

    [Theory]
    [InlineData(40, GrooteSchuur, "Groote Schuur Hospital's trainees on this curriculum are no longer measured against this EPA.")]
    [InlineData(40, null, "Your institution's trainees on this curriculum are no longer measured against this EPA.")]
    public void TheDialog_SaysWhoseTraineesAnOwnItemStopsMeasuring(int owningInstitutionId, string? ownerName, string sentence)
    {
        var cut = RenderList(new FakeSender(
        [
            FakeSender.Item(11, 1, 3, QuotaPeriod.Semester, null),
            FakeSender.Item(12, 2, 3, QuotaPeriod.AcademicYear, null, owningInstitutionId: owningInstitutionId, owningInstitutionName: ownerName)
        ]));

        RemoveButton(cut, "PAED-002").Click();

        Text(cut.Find("dialog p")).Should().EndWith($"{sentence} This cannot be undone.");
    }

    [Fact]
    public void CancellingTheDialog_SendsNothing_AndKeepsTheItem()
    {
        var sender = new FakeSender();
        var cut = RenderList(sender);

        RemoveButton(cut, "PAED-002").Click();
        cut.FindAll("dialog button").Single(button => Text(button) == "Cancel").Click();

        sender.Removes.Should().BeEmpty();
        Row(cut, "PAED-002").Should().NotBeNull();
    }

    [Fact]
    public void ConfirmingTheDialog_RemovesThatItem_AndItsResultTakesTheFocusOnceTheDialogHasClosed()
    {
        // T11 (R3-B14): "KGK-001 removed from this curriculum." at the list's top, focused.
        var sender = new FakeSender();
        var cut = RenderList(sender);

        RemoveButton(cut, "PAED-002").Click();
        ConfirmRemove(cut);

        sender.Removes.Should().ContainSingle().Which.ItemId.Should().Be(12);
        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-success")).Should().Be("PAED-002 removed from this curriculum."));
        cut.Find(".action-result .alert-success").GetAttribute("role").Should().Be("status");
        cut.FindAll("tbody tr").Should().HaveCount(2);
        Text(cut.Find(".form-section-head h2")).Should().Be("Items (2)");
        ResultTookTheFocusAfterTheDialogClosed(cut);
    }

    [Fact]
    public void RemovingOneOfTwoItemsOnAnEpa_SaysWhoseWasRemoved()
    {
        var sender = TwoInstitutionsOnOneEpa();
        var cut = RenderList(sender);

        cut.FindAll("button").Single(button => button.GetAttribute("aria-label") == $"Remove PAED-002 ({GrooteSchuur}'s own item)").Click();
        ConfirmRemove(cut);

        sender.Removes.Should().ContainSingle().Which.ItemId.Should().Be(21);
        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-success"))
            .Should().Be($"PAED-002 ({GrooteSchuur}'s own item) removed from this curriculum."));
    }

    [Fact]
    public void ARemoveAnEndedRecordHolds_IsRefusedInTheCommandsWords_Focused_AndTheItemKept()
    {
        // T312's refusal, typed (T392), so the page says it without printing an exception's text.
        var cut = RenderList(new FakeSender { RemoveFailure = new CurriculumItemOnEndedRecordsException(2) });

        RemoveButton(cut, "PAED-002").Click();
        ConfirmRemove(cut);

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-danger")).Should().Be(
            RemoveCurriculumItemCommandHandler.EndedRecordsHoldTheItem(2)));
        cut.Find(".action-result .alert-danger").GetAttribute("role").Should().Be("alert");
        Row(cut, "PAED-002").Should().NotBeNull();
        ResultTookTheFocusAfterTheDialogClosed(cut);
    }

    [Fact]
    public void AnyOtherRefusal_IsFixedWords_NeverTheExceptionsText_AndThePageIsReadAgain()
    {
        // T329; the T222 review's stale page: the item was removed elsewhere, so the list reads it again.
        var sender = new FakeSender { RemoveFailure = new InvalidOperationException("The requested curriculum item was not found.") };
        var cut = RenderList(sender);
        var reads = sender.CurriculumReads;

        RemoveButton(cut, "PAED-002").Click();
        ConfirmRemove(cut);

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-danger")).Should().Be(
            "PAED-002 could not be removed. Nothing has changed. Try again, or come back in a few minutes."));
        cut.Markup.Should().NotContain("was not found");
        sender.CurriculumReads.Should().Be(reads + 1);
    }

    [Fact]
    public void ASecondConfirmWhileTheFirstRemoveRuns_SendsNothing()
    {
        // As T206's Withdraw: the second confirm can reach the circuit before the render that closes the dialog does.
        var sender = new FakeSender { HoldRemoves = true };
        var cut = RenderList(sender);

        RemoveButton(cut, "PAED-002").Click();
        ConfirmRemove(cut);
        ConfirmRemove(cut);

        sender.Release();
        cut.WaitForAssertion(() => cut.FindAll(".action-result .alert-success").Should().ContainSingle());
        sender.Removes.Should().ContainSingle();
    }

    [Fact]
    public void AfterARemove_TheAddLinkComesBack_WhenItsEpaIsFreeAgain()
    {
        // R3-B14: the picker is asked again after the Remove, and offers the EPA it freed.
        FakeSender sender = null!;
        sender = new FakeSender
        {
            StoresItems = true,
            EpaOptionsFor = _ => sender.CurrentItems.Any(item => item.EpaId == 2) ? [] : [FakeSender.Epas[1]]
        };
        var cut = RenderList(sender);
        cut.FindAll(".form-section-head a").Should().BeEmpty("no EPA is left to add");

        RemoveButton(cut, "PAED-002").Click();
        ConfirmRemove(cut);

        cut.WaitForAssertion(() => Text(cut.Find(".form-section-head a")).Should().Be("Add item"));
        cut.FindAll(".curriculum-items-page > p.muted").Should().BeEmpty("the empty line goes with the link's return");
    }

    // ---- helpers ----

    private static FakeSender TwoInstitutionsOnOneEpa()
        => new(
        [
            FakeSender.Item(11, 1, 3, QuotaPeriod.Semester, null),
            FakeSender.Item(21, 2, 3, QuotaPeriod.AcademicYear, null, owningInstitutionId: 40, owningInstitutionName: GrooteSchuur),
            FakeSender.Item(22, 2, 2, QuotaPeriod.AcademicYear, null, owningInstitutionId: 41, owningInstitutionName: RedCross)
        ]);

    private void ResultTookTheFocusAfterTheDialogClosed(IRenderedComponent<Wombat.Web.Components.Pages.Admin.Curricula.CurriculumItemsEdit> cut)
    {
        // bUnit on .NET 10 leaves an element's blazor:elementReference empty in the markup, so the focused reference is
        // matched to the page's own result region.
        var region = cut.FindComponent<ActionResult>();
        cut.WaitForAssertion(() => FocusCalls().Should().ContainSingle()
            .Which.Arguments[0].Should().BeOfType<ElementReference>().Which.Id.Should().Be(region.Instance.Element.Id));

        // While the modal is open, nothing outside it can take the focus.
        var calls = JSInterop.Invocations.Select(invocation => invocation.Identifier).ToList();
        calls.IndexOf("wombatDialog.close").Should().BeGreaterThan(-1)
            .And.BeLessThan(calls.IndexOf(FocusIdentifier), "the dialog closes before the result takes the focus");
    }

    private static AngleSharp.Dom.IElement RemoveButton(IRenderedFragment cut, string epaCode)
        => Row(cut, epaCode).QuerySelectorAll("button").Single(button => Text(button) == "Remove…");

    private static void ConfirmRemove(IRenderedFragment cut)
        => cut.FindAll("dialog button").Single(button => Text(button) == "Remove item").Click();
}
