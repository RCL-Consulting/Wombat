using AngleSharp.Dom;
using Bunit;
using FluentAssertions;
using Wombat.Domain.Identity;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T398 (flow 17, Q15, N2; R3-E11, E12): unticking Active on a published type asks at Save draft; ticking it back asks
/// nothing; each says what it did.
/// </summary>
public sealed class ActivityTypeDeactivateTests : TestContext
{
    public ActivityTypeDeactivateTests() => BuilderKit.SignIn(this, WombatRoles.InstitutionalAdmin);

    [Fact]
    public void ActivesHelp_IsN2s()
    {
        var cut = BuilderKit.Render(this, new BuilderSender(BuilderKit.Kgk()));
        BuilderKit.Tab(cut, "Details");

        cut.Find("#type-is-active").GetAttribute("aria-describedby").Should().Be("type-is-active-help");
        cut.Find("#type-is-active-help").TextContent.Should().Be(
            "An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.");
    }

    [Fact]
    public void UntickingActive_OnAPublishedType_SaveDraftAsks_ThenSaysItIsInactive()
    {
        var sender = new BuilderSender(BuilderKit.Kgk(hasDraft: false, publishedVersion: 1));
        var cut = BuilderKit.Render(this, sender);
        BuilderKit.Tab(cut, "Details");
        cut.Find("#type-is-active").Change(false);

        cut.Find("#builder-save").GetAttribute("aria-haspopup").Should().Be("dialog");
        cut.Find("#builder-save").Click();

        sender.Saves.Should().BeEmpty("it asks first");
        var dialog = Dialog(cut);
        dialog.TextContent.Should().Contain("Registrars are no longer offered it when filing. Activities already filed are not changed.")
            .And.Contain("You can reactivate it on this page.");
        dialog.QuerySelectorAll("button").Single(button => button.TextContent.Trim() == "Deactivate").Click();

        sender.Saves.Should().ContainSingle().Which.IsActive.Should().BeFalse();
        var result = cut.Find(".action-result .alert-success");
        result.QuerySelector("strong")!.TextContent.Should().Be("Draft saved.", "only the first sentence is bold");
        result.TextContent.Trim().Should().Be("Draft saved. KGK Teaching Session Log is inactive: registrars are no longer offered it.");
    }

    [Fact]
    public void TickingActiveBack_AsksNothing_AndSaysItIsActiveAgain()
    {
        var sender = new BuilderSender(BuilderKit.Kgk(publishedVersion: 1, isActive: false));
        var cut = BuilderKit.Render(this, sender);
        BuilderKit.Tab(cut, "Details");
        cut.Find("#type-is-active").Change(true);

        cut.Find("#builder-save").HasAttribute("aria-haspopup").Should().BeFalse();
        cut.Find("#builder-save").Click();

        sender.Saves.Should().ContainSingle();
        cut.Find(".action-result").TextContent.Trim().Should().Be(
            "Draft saved. KGK Teaching Session Log is active again: registrars are offered it.");
    }

    [Fact]
    public void OnANeverPublishedType_UntickingAsksNothing()
    {
        var sender = new BuilderSender(BuilderKit.Kgk());
        var cut = BuilderKit.Render(this, sender);
        BuilderKit.Tab(cut, "Details");
        cut.Find("#type-is-active").Change(false);

        cut.Find("#builder-save").Click();

        sender.Saves.Should().ContainSingle();
        cut.Find(".action-result").TextContent.Trim().Should().Be("Draft saved.");
    }

    private static IElement Dialog(IRenderedFragment cut)
        => cut.FindAll("dialog").Single(dialog => dialog.QuerySelector("h2")!.TextContent == "Deactivate KGK Teaching Session Log?");
}
