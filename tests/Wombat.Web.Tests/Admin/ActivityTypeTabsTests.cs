using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T398 (flow 17, Q1, T280, M2, N20, N21): the builder's tabs are the tabs pattern. A writer has five, a reader four; a new
/// type opens on Details, an existing one on Form; a roving tabindex and aria-controls on the selected tab only;
/// Left, Right, Home and End move the focus; a save keeps the tab; a tab with refusals carries its count in its name.
/// </summary>
public sealed class ActivityTypeTabsTests : TestContext
{
    public ActivityTypeTabsTests() => BuilderKit.SignIn(this, WombatRoles.InstitutionalAdmin);

    [Fact]
    public void AWriter_HasFiveTabs_InATablist_TheSelectedOneControllingItsPanel()
    {
        var cut = BuilderKit.Render(this, new BuilderSender(BuilderKit.Kgk()));

        var bar = cut.Find(".tab-bar");
        bar.GetAttribute("role").Should().Be("tablist");
        bar.GetAttribute("aria-label").Should().Be("Parts of this activity type");

        var tabs = cut.FindAll("[role=tab]");
        tabs.Select(tab => tab.TextContent.Trim()).Should().Equal("Details", "Form", "Workflow", "Credit", "Publish");
        tabs.Select(tab => tab.GetAttribute("aria-selected")).Should().Equal(
            ["false", "true", "false", "false", "false"], "an existing type opens on Form (T2)");
        tabs.Select(tab => tab.GetAttribute("tabindex")).Should().Equal(["-1", "0", "-1", "-1", "-1"], "a roving tabindex");
        tabs.Select(tab => tab.GetAttribute("aria-controls")).Should().Equal(null, "panel-form", null, null, null);
        tabs.Should().OnlyContain(tab => !tab.HasAttribute("aria-label") && !tab.HasAttribute("aria-describedby"), "N20");

        var panel = cut.Find("#panel-form");
        panel.GetAttribute("role").Should().Be("tabpanel");
        panel.GetAttribute("aria-labelledby").Should().Be("tab-form");
        panel.HasAttribute("tabindex").Should().BeFalse("a panel holding a control takes no tabindex (N21)");
        cut.Markup.Should().NotContain("is-active").And.NotContain("Metadata");
    }

    [Fact]
    public void ANewType_OpensOnDetails()
    {
        var cut = BuilderKit.Render(this, new BuilderSender(BuilderKit.New(BuilderKit.InstitutionScopes)), id: null);

        BuilderKit.TabNamed(cut, "Details").GetAttribute("aria-selected").Should().Be("true");
        cut.FindAll("#panel-details").Should().ContainSingle();
    }

    [Fact]
    public void AReader_HasFourTabs_AndNoPublish()
    {
        var cut = BuilderKit.Render(this, new BuilderSender(BuilderKit.Kgk(publishedVersion: 1, hasDraft: false, canWrite: false)));

        cut.FindAll("[role=tab]").Select(tab => tab.TextContent.Trim()).Should().Equal("Details", "Form", "Workflow", "Credit");
        BuilderKit.Tab(cut, "Workflow");
        cut.Find("#panel-workflow").GetAttribute("tabindex").Should().Be("0", "a reader's read-only panel takes the focus itself");
    }

    [Theory]
    [InlineData("tab-form", "ArrowRight", "tab-workflow")]
    [InlineData("tab-form", "ArrowLeft", "tab-details")]
    [InlineData("tab-details", "ArrowLeft", "tab-publish")]
    [InlineData("tab-publish", "ArrowRight", "tab-details")]
    [InlineData("tab-credit", "Home", "tab-details")]
    [InlineData("tab-form", "End", "tab-publish")]
    public void TheArrowHomeAndEndKeys_MoveTheFocusAlongTheTabs(string from, string key, string to)
    {
        var cut = BuilderKit.Render(this, new BuilderSender(BuilderKit.Kgk()));

        cut.Find($"#{from}").KeyDown(new KeyboardEventArgs { Key = key });

        BuilderKit.FocusedIds(this).Should().Equal([to]);
        BuilderKit.TabNamed(cut, "Form").GetAttribute("aria-selected").Should().Be("true", "moving the focus selects nothing");
    }

    [Fact]
    public void AnyOtherKey_MovesNothing()
    {
        var cut = BuilderKit.Render(this, new BuilderSender(BuilderKit.Kgk()));

        cut.Find("#tab-form").KeyDown(new KeyboardEventArgs { Key = "Tab" });

        BuilderKit.FocusedIds(this).Should().BeEmpty("Tab leaves the bar, as it always does");
    }

    [Fact]
    public void ASave_KeepsTheTab_AndATabPress_KeepsTheResult()
    {
        var cut = BuilderKit.Render(this, new BuilderSender(BuilderKit.Kgk()));
        BuilderKit.Tab(cut, "Workflow");

        BuilderKit.Button(cut, "Save draft").Click();

        BuilderKit.TabNamed(cut, "Workflow").GetAttribute("aria-selected").Should().Be("true", "a save keeps the tab (T2)");
        cut.Find(".action-result").TextContent.Trim().Should().Be("Draft saved.");

        BuilderKit.Tab(cut, "Details");

        cut.Find(".action-result").TextContent.Trim().Should().Be("Draft saved.", "a tab press is no save, publish or discard (E02b)");
    }

    [Fact]
    public void ATabWithRefusals_CarriesItsCountInItsName()
    {
        var sender = new BuilderSender(BuilderKit.Kgk())
        {
            SaveRefusal = new ActivityTypeDraftRefusedException(
            [
                new RefusalPlace(RefusalTab.Form, RefusalKind.EmptySection, "Section 'session' must contain at least one field.") { SectionKey = "session" },
                new RefusalPlace(RefusalTab.Form, RefusalKind.Other, "Schema must contain at least one section."),
                new RefusalPlace(RefusalTab.Credit, RefusalKind.Other, "Credit rule 1 has no amount.")
            ])
        };
        var cut = BuilderKit.Render(this, sender);

        BuilderKit.Button(cut, "Save draft").Click();

        cut.Find("#tab-form").TextContent.Trim().Should().Be("Form 2 problems");
        cut.Find("#tab-credit").TextContent.Trim().Should().Be("Credit 1 problem");
        cut.Find("#tab-credit .badge").ClassList.Should().Contain("badge-declined");
        cut.Find("#tab-workflow").TextContent.Trim().Should().Be("Workflow");
    }
}
