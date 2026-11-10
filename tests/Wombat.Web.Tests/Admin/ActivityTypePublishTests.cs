using AngleSharp.Dom;
using Bunit;
using FluentAssertions;
using Wombat.Domain.Identity;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T398 (flow 17, Q16, Q17, Q20, S5, S6, S7, R3-E01–E05, R2-E03): Publish is inert without a saved draft; it saves the
/// edits since first, asks only when the saved draft has warnings, lands on the tab it was pressed from, and the Publish
/// tab says what publishing changes from the saved draft.
/// </summary>
public sealed class ActivityTypePublishTests : TestContext
{
    /// <summary>KGK's published version 1 with Audience, and a draft without it.</summary>
    private static readonly string WithoutAudience = BuilderKit.KgkSchema.Replace(
        """{ "key": "audience", "type": "choice", "label": "Audience", "required": true, "options": ["Interns", "Registrars"] },""",
        string.Empty);

    public ActivityTypePublishTests() => BuilderKit.SignIn(this, WombatRoles.InstitutionalAdmin);

    [Fact]
    public void WithNoDraft_PublishIsUnavailable_WithItsReason_AndInert()
    {
        var sender = new BuilderSender(BuilderKit.Kgk(hasDraft: false, publishedVersion: 1));
        var cut = BuilderKit.Render(this, sender);

        var publish = cut.Find("#builder-publish");
        publish.HasAttribute("disabled").Should().BeFalse("S5: unavailable, not disabled");
        publish.GetAttribute("aria-disabled").Should().Be("true");
        publish.GetAttribute("aria-describedby").Should().Be("publish-reason");
        cut.Find("#publish-reason").TextContent.Should().Be("Save a draft to publish.");
        cut.FindAll("#builder-discard").Should().BeEmpty("no Discard draft… without a draft");

        publish.Click();

        sender.Publishes.Should().BeEmpty("inert");
        sender.Saves.Should().BeEmpty();

        BuilderKit.Tab(cut, "Publish");
        cut.Find("#panel-publish").TextContent.Should().Contain("There is no draft to publish: version 1 is what registrars file. Save a draft to publish.");
    }

    [Fact]
    public void Publish_FromCredit_LandsOnCredit_TheResultAboveTheTabs()
    {
        var sender = new BuilderSender(BuilderKit.Kgk());
        var cut = BuilderKit.Render(this, sender);
        BuilderKit.Tab(cut, "Credit");

        cut.Find("#builder-publish").Click();

        sender.Publishes.Should().ContainSingle();
        sender.Saves.Should().BeEmpty("nothing was edited since the draft was saved");
        cut.Find(".action-result").TextContent.Trim().Should().Be("Published version 1.");
        BuilderKit.TabNamed(cut, "Credit").GetAttribute("aria-selected").Should().Be("true", "Q20");
        cut.Find(".builder-strip").TextContent.Should().Contain("Version 1, 2026-11-05, Nolwazi Mbatha").And.Contain("None");
        cut.Find("#builder-publish").GetAttribute("aria-disabled").Should().Be("true");
    }

    [Fact]
    public void Publish_SavesTheEditsSince_First()
    {
        var sender = new BuilderSender(BuilderKit.Kgk());
        var cut = BuilderKit.Render(this, sender);
        BuilderKit.Tab(cut, "Details");
        cut.Find("#type-name").Change("KGK Teaching Log");

        cut.Find("#builder-publish").Click();

        sender.Saves.Should().ContainSingle().Which.Name.Should().Be("KGK Teaching Log");
        sender.Publishes.Should().ContainSingle();
    }

    [Fact]
    public void ThePublishTab_SaysWhatPublishingChanges_FromTheSavedDraft()
    {
        var cut = BuilderKit.Render(this, new BuilderSender(BuilderKit.Kgk(publishedVersion: 1, draftSchema: WithoutAudience)));

        BuilderKit.Tab(cut, "Publish");

        var panel = cut.Find("#panel-publish");
        panel.QuerySelector("h3")!.TextContent.Should().Be("What publishing this draft changes");
        panel.QuerySelectorAll("ul > li").Select(li => li.TextContent).Should().Equal("Field 'Audience' will be removed.");
        panel.TextContent.Should().Contain(
            "Activities already filed stay on version 1, with Audience as they filed it. Only activities filed after the publish use version 2.");
        panel.QuerySelector(".versions-readout").Should().NotBeNull();
        BuilderKit.Button(cut, "Publish version 2").ClassList.Should().Contain("btn-success");
        panel.TextContent.Should().NotContain("Save draft to see what publishing changes.");

        BuilderKit.Tab(cut, "Details");
        cut.Find("#type-name").Change("Renamed");
        BuilderKit.Tab(cut, "Publish");
        cut.Find("#panel-publish").TextContent.Should().Contain("Save draft to see what publishing changes.", "S6");
    }

    [Fact]
    public void AFirstPublish_SaysWhatYouArePublishing()
    {
        var cut = BuilderKit.Render(this, new BuilderSender(BuilderKit.Kgk()));

        BuilderKit.Tab(cut, "Publish");

        var panel = cut.Find("#panel-publish");
        panel.TextContent.Should().Contain("What you are publishing")
            .And.Contain("1 section, 6 fields (5 required)")
            .And.Contain("From Draft to Logged; 1 move")
            .And.Contain("Credits nothing")
            .And.Contain("Nothing to compare: this is the first version.");
        panel.TextContent.Should().NotContain("→");
    }

    [Fact]
    public void WithWarnings_PublishAsks_ItsConfirmAPrimary_ThenPublishes()
    {
        var sender = new BuilderSender(BuilderKit.Kgk(publishedVersion: 1, draftSchema: WithoutAudience));
        var cut = BuilderKit.Render(this, sender);

        cut.Find("#builder-publish").Click();

        sender.Publishes.Should().BeEmpty("it asks first");
        var dialog = Dialog(cut, "Publish version 2 of KGK Teaching Session Log?");
        dialog.TextContent.Should().Contain("Publishing makes this draft version 2. It changes:").And.Contain("Field 'Audience' will be removed.");
        var confirm = dialog.QuerySelectorAll("button").Single(button => button.TextContent.Trim() == "Publish version 2");
        confirm.ClassList.Should().Contain("btn-primary", "S7").And.NotContain("btn-danger");
        ShowedWith().Should().Contain("builder-publish", "Cancel returns the focus to Publish");

        confirm.Click();

        sender.Publishes.Should().ContainSingle();
        cut.Find(".action-result").TextContent.Trim().Should().Be("Published version 2.");
    }

    [Fact]
    public void CancelOnTheWarnings_AfterSavingFirst_SaysDraftSavedNothingPublished()
    {
        var sender = new BuilderSender(BuilderKit.Kgk(publishedVersion: 1, draftSchema: WithoutAudience));
        var cut = BuilderKit.Render(this, sender);
        BuilderKit.Tab(cut, "Details");
        cut.Find("#type-name").Change("KGK Teaching Log");

        cut.Find("#builder-publish").Click();
        sender.Saves.Should().ContainSingle();
        Dialog(cut, "Publish version 2 of KGK Teaching Log?").QuerySelectorAll("button").Single(button => button.TextContent.Trim() == "Cancel").Click();

        sender.Publishes.Should().BeEmpty();
        cut.Find(".action-result").TextContent.Trim().Should().Be("Draft saved. Nothing was published.");
    }

    [Fact]
    public void WithActiveNewlyUnticked_PublishAsksQ15First_CancelSavesNothing()
    {
        var sender = new BuilderSender(BuilderKit.Kgk(publishedVersion: 1));
        var cut = BuilderKit.Render(this, sender);
        BuilderKit.Tab(cut, "Details");
        cut.Find("#type-is-active").Change(false);

        cut.Find("#builder-publish").GetAttribute("aria-haspopup").Should().Be("dialog");
        cut.Find("#builder-publish").Click();

        var question = Dialog(cut, "Deactivate KGK Teaching Session Log?");
        ShowedWith().Should().Contain("builder-publish");
        question.QuerySelectorAll("button").Single(button => button.TextContent.Trim() == "Cancel").Click();

        sender.Saves.Should().BeEmpty("Cancel leaves everything unsaved (R3-E17)");
        sender.Publishes.Should().BeEmpty();
        cut.Find("#type-is-active").HasAttribute("checked").Should().BeFalse("what was typed is kept");
    }

    [Fact]
    public void WithActiveNewlyUnticked_Deactivate_SavesThenPublishes_AndSaysSo()
    {
        var sender = new BuilderSender(BuilderKit.Kgk(publishedVersion: 1));
        var cut = BuilderKit.Render(this, sender);
        BuilderKit.Tab(cut, "Details");
        cut.Find("#type-is-active").Change(false);
        cut.Find("#builder-publish").Click();

        Dialog(cut, "Deactivate KGK Teaching Session Log?").QuerySelectorAll("button").Single(button => button.TextContent.Trim() == "Deactivate").Click();

        sender.Saves.Should().ContainSingle().Which.IsActive.Should().BeFalse();
        sender.Publishes.Should().ContainSingle();
        cut.Find(".action-result").TextContent.Trim().Should().Be(
            "Published version 2. KGK Teaching Session Log is inactive: registrars are no longer offered it.");
        BuilderKit.TabNamed(cut, "Details").GetAttribute("aria-selected").Should().Be("true");
    }

    [Fact]
    public void AFailedPublish_IsTheFixedSentence()
    {
        var sender = new BuilderSender(BuilderKit.Kgk(publishedVersion: 1)) { PublishRefusal = new InvalidOperationException("boom") };
        var cut = BuilderKit.Render(this, sender);

        cut.Find("#builder-publish").Click();

        cut.Find(".action-result .alert-danger").TextContent.Trim().Should().Be(
            "Version 2 could not be published. Nothing has changed. Try again, or come back in a few minutes.");
    }

    private static IElement Dialog(IRenderedFragment cut, string title)
        => cut.FindAll("dialog").Single(dialog => dialog.QuerySelector("h2")!.TextContent == title);

    private IReadOnlyList<string?> ShowedWith()
        => JSInterop.Invocations.Where(call => call.Identifier == "wombatDialog.showModal")
            .Select(call => call.Arguments[1]?.ToString()).ToList();
}
