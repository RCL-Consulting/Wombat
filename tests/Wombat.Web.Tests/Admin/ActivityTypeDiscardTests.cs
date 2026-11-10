using AngleSharp.Dom;
using Bunit;
using FluentAssertions;
using Wombat.Domain.Identity;

namespace Wombat.Web.Tests.Admin;

/// <summary>T398 (flow 17, Q8, Q14; R3-E07, R2-E09, R2-E10): Discard draft… asks first, its body by whether a version is published.</summary>
public sealed class ActivityTypeDiscardTests : TestContext
{
    public ActivityTypeDiscardTests() => BuilderKit.SignIn(this, WombatRoles.InstitutionalAdmin);

    [Fact]
    public void OnAPublishedType_ItAsksWithThePublishedBody_AndDiscardsOnConfirm()
    {
        var sender = new BuilderSender(BuilderKit.Kgk(publishedVersion: 1));
        var cut = BuilderKit.Render(this, sender);

        var trigger = cut.Find("#builder-discard");
        trigger.TextContent.Trim().Should().Be("Discard draft…");
        trigger.GetAttribute("aria-haspopup").Should().Be("dialog");
        trigger.ClassList.Should().Contain("btn-outline");
        trigger.Click();

        sender.Discards.Should().BeEmpty("it asks first");
        var dialog = Dialog(cut);
        dialog.QuerySelector("h2")!.TextContent.Should().Be("Discard the draft of KGK Teaching Session Log?");
        dialog.TextContent.Should().Contain(
            "Its form, workflow and credit rules go back to version 1, which stays published. Its details (name, description, scope, This tool is, Active) were saved as you changed them, and stay.");
        var buttons = dialog.QuerySelectorAll("button");
        buttons.ElementAt(0).TextContent.Trim().Should().Be("Cancel", "Cancel first in the DOM");
        buttons.ElementAt(1).ClassList.Should().Contain("btn-danger");

        buttons.ElementAt(1).Click();

        sender.Discards.Should().ContainSingle();
        cut.Find(".action-result").TextContent.Trim().Should().Be("Draft discarded.");
        cut.FindAll("#builder-discard").Should().BeEmpty();
    }

    [Fact]
    public void OnANeverPublishedType_TheBodySaysSo()
    {
        var cut = BuilderKit.Render(this, new BuilderSender(BuilderKit.Kgk()));

        Dialog(cut).TextContent.Should().Contain(
            "It has never been published. Its form, workflow and credit rules go back to the starting draft; its details stay.");
    }

    [Fact]
    public void Cancel_DiscardsNothing()
    {
        var sender = new BuilderSender(BuilderKit.Kgk());
        var cut = BuilderKit.Render(this, sender);
        cut.Find("#builder-discard").Click();

        Dialog(cut).QuerySelectorAll("button").Single(button => button.TextContent.Trim() == "Cancel").Click();

        sender.Discards.Should().BeEmpty();
        JSInterop.Invocations.Where(call => call.Identifier == "wombatDialog.showModal")
            .Select(call => call.Arguments[1]).Should().Contain("builder-discard", "Cancel and Escape give the focus back");
    }

    [Fact]
    public void AFailedDiscard_IsTheFixedSentence()
    {
        var sender = new BuilderSender(BuilderKit.Kgk()) { DiscardRefusal = new InvalidOperationException("boom") };
        var cut = BuilderKit.Render(this, sender);
        cut.Find("#builder-discard").Click();

        Dialog(cut).QuerySelectorAll("button").Single(button => button.TextContent.Trim() == "Discard draft").Click();

        cut.Find(".action-result .alert-danger").TextContent.Trim().Should().Be(
            "The draft could not be discarded. Nothing has changed. Try again, or come back in a few minutes.");
    }

    /// <summary>
    /// The operator's F1 (2026-11-10): a system-managed type that holds a draft from before T334 offers Discard draft…
    /// alone, to whoever its scope admits (<c>CanDiscardDraft</c>); Save draft and Publish stay a writer's. A reader the
    /// scope does not admit is offered nothing.
    /// </summary>
    [Fact]
    public void ASystemManagedTypesDraft_OffersDiscardAlone_WhereTheScopeAdmits()
    {
        var release = BuilderKit.Kgk(publishedVersion: 1, canWrite: false) with { SystemManaged = true, WrittenBy = "the MSF release", CanDiscardDraft = true };
        var sender = new BuilderSender(release);
        var cut = BuilderKit.Render(this, sender);

        cut.FindAll("#builder-save").Should().BeEmpty("Save draft stays refused on a release's type");
        cut.FindAll("#builder-publish").Should().BeEmpty("Publish too");
        cut.Find("#builder-discard").Click();
        Dialog(cut).QuerySelectorAll("button").Single(button => button.TextContent.Trim() == "Discard draft").Click();

        sender.Discards.Should().ContainSingle();
        cut.Find(".action-result").TextContent.Trim().Should().Be("Draft discarded.");
        cut.FindAll("#builder-discard").Should().BeEmpty("nothing is left to discard");
    }

    [Fact]
    public void ASystemManagedTypesDraft_OffersNothing_ToAReaderTheScopeDoesNotAdmit()
    {
        var release = BuilderKit.Kgk(publishedVersion: 1, canWrite: false) with { SystemManaged = true, WrittenBy = "the MSF release" };
        var cut = BuilderKit.Render(this, new BuilderSender(release));

        cut.FindAll("#builder-discard").Should().BeEmpty();
        cut.FindAll("dialog").Should().BeEmpty();
    }

    private static IElement Dialog(IRenderedFragment cut)
        => cut.FindAll("dialog").Single(dialog => dialog.QuerySelector("h2")!.TextContent.StartsWith("Discard the draft", StringComparison.Ordinal));
}
