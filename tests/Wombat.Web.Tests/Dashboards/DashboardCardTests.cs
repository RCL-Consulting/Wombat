using Bunit;
using FluentAssertions;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Tests.Dashboards;

public sealed class DashboardCardTests : TestContext
{
    [Fact]
    public void DefaultCard_RendersDetailCardClass()
    {
        var cut = RenderComponent<DashboardCard>(parameters => parameters
            .Add(p => p.Title, "Test Card")
            .AddChildContent("<p>Body</p>"));

        cut.Markup.Should().Contain("class=\"detail-card\"");
        cut.Find("h2.dashboard-card-title").TextContent.Trim().Should().Be("Test Card", "a card's title is an h2, under Home's h1 (T335)");
        cut.Markup.Should().Contain("Body");
    }

    // T335, flow 01 (R2-Landing-Loading): while its dashboard reads, a card is its title and a skeleton, and offers nothing.
    [Fact]
    public void ALoadingCard_ShowsItsTitleAndASkeleton_NotItsContent()
    {
        var cut = RenderComponent<DashboardCard>(parameters => parameters
            .Add(p => p.Title, "Activity inbox")
            .Add(p => p.IsLoading, true)
            .AddChildContent("<a href=\"/activities/inbox\">Open inbox</a>"));

        cut.Find("h2").TextContent.Trim().Should().Be("Activity inbox");
        cut.FindAll(".skeleton").Should().NotBeEmpty();
        cut.Find(".dashboard-card-skeleton").GetAttribute("aria-hidden").Should().Be("true");
        cut.Markup.Should().NotContain("Open inbox");
    }

    [Fact]
    public void ALoadingCardThatLinks_IsNoLinkUntilItHasLoaded()
    {
        var cut = RenderComponent<DashboardCard>(parameters => parameters
            .Add(p => p.Title, "Curriculum targets")
            .Add(p => p.Href, "/portfolio/progress")
            .Add(p => p.IsLoading, true)
            .AddChildContent("<p>Targets</p>"));

        cut.FindAll("a").Should().BeEmpty();
        cut.Markup.Should().NotContain("detail-card--interactive");

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.IsLoading, false));

        cut.Find("a.detail-card--interactive").GetAttribute("href").Should().Be("/portfolio/progress");
    }

    // R2-Landing-Assessor: a card's count as its badge, once the count is known (the Trainee's Needs you since T350).
    [Fact]
    public void ACount_IsTheTitlesBadge_OnceLoaded()
    {
        var cut = RenderComponent<DashboardCard>(parameters => parameters
            .Add(p => p.Title, "Needs you")
            .Add(p => p.Count, 2)
            .AddChildContent("<p>Rows</p>"));

        var badge = cut.Find("h2 .badge");
        badge.TextContent.Should().Be("2");
        badge.ClassList.Should().Contain("badge-submitted", "the info tint: waiting on someone (BadgeFor)");

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.IsLoading, true));

        cut.FindAll(".badge").Should().BeEmpty("a count is not known until the read returns");
    }

    // T358, flow 06 (D10; R2-Home c1, k4): a count of people waits on nobody, so its badge is the draft tone; a count of
    // work waiting keeps the submitted tone, the default.
    [Fact]
    public void TheBadgesTone_IsSubmittedByDefault_AndDraftForACountOfPeople()
    {
        var waiting = RenderComponent<DashboardCard>(parameters => parameters
            .Add(p => p.Title, "Waiting for assessors")
            .Add(p => p.BadgeWords, "3 waiting, 2 overdue")
            .AddChildContent("<p>Rows</p>"));
        waiting.Find("h2 .badge").ClassList.Should().Contain("badge-submitted");

        var registrars = RenderComponent<DashboardCard>(parameters => parameters
            .Add(p => p.Title, "Registrars")
            .Add(p => p.BadgeWords, "5 registrars")
            .Add(p => p.BadgeTone, BadgeState.Draft)
            .AddChildContent("<p>Rows</p>"));
        var badge = registrars.Find("h2 .badge");
        badge.TextContent.Should().Be("5 registrars");
        badge.ClassList.Should().Contain("badge-draft").And.NotContain("badge-submitted");
    }

    [Fact]
    public void EmphasisCard_HasEmphasisClass()
    {
        var cut = RenderComponent<DashboardCard>(parameters => parameters
            .Add(p => p.Title, "Emphasis")
            .Add(p => p.Emphasis, true)
            .AddChildContent("<p>Content</p>"));

        cut.Markup.Should().Contain("detail-card--emphasis");
    }

    [Fact]
    public void WarningCard_HasWarningClass()
    {
        var cut = RenderComponent<DashboardCard>(parameters => parameters
            .Add(p => p.Title, "Warning")
            .Add(p => p.Warning, true)
            .AddChildContent("<p>Content</p>"));

        cut.Markup.Should().Contain("detail-card--warning");
    }

    // T350, flow 04 (note 13; R1): the words badge. The words are the badge, to the eye and to a screen reader alike, and a
    // card with nothing in it has none.
    [Fact]
    public void BadgeWords_AreTheTitlesBadge_AsTheyRead_OnceLoaded()
    {
        var cut = RenderComponent<DashboardCard>(parameters => parameters
            .Add(p => p.Title, "Waiting for you")
            .Add(p => p.BadgeWords, "2 waiting, 1 overdue")
            .AddChildContent("<p>Rows</p>"));

        var badge = cut.Find("h2 .badge");
        badge.TextContent.Should().Be("2 waiting, 1 overdue");
        badge.ClassList.Should().Contain("badge-submitted");
        badge.HasAttribute("aria-hidden").Should().BeFalse();
        cut.FindAll("h2 .visually-hidden").Should().BeEmpty("the words need no second reading");

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.IsLoading, true));
        cut.FindAll(".badge").Should().BeEmpty("the count is not known until the read returns");

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.IsLoading, false).Add(p => p.BadgeWords, (string?)null));
        cut.FindAll(".badge").Should().BeEmpty("nothing to count is no badge, never \"0\"");
    }

    // Nit T15: one stripe per card. A card that wants attention draws the warning one in place of the emphasis one.
    [Fact]
    public void AWarningCard_DrawsTheWarningStripe_InPlaceOfTheEmphasisOne()
    {
        var cut = RenderComponent<DashboardCard>(parameters => parameters
            .Add(p => p.Title, "Waiting for you")
            .Add(p => p.Emphasis, true)
            .Add(p => p.Warning, true)
            .AddChildContent("<p>Rows</p>"));

        cut.Find(".detail-card").ClassList.Should().Contain("detail-card--warning").And.NotContain("detail-card--emphasis");
    }

    // T350: a card with a heading id is a section named by its title, so the Assessor's cards are regions.
    [Fact]
    public void ACardWithAHeadingId_IsASectionNamedByItsTitle()
    {
        var cut = RenderComponent<DashboardCard>(parameters => parameters
            .Add(p => p.Title, "Recent decisions")
            .Add(p => p.HeadingId, "card-decisions")
            .AddChildContent("<p>Rows</p>"));

        var section = cut.Find("section.detail-card");
        section.GetAttribute("aria-labelledby").Should().Be("card-decisions");
        section.QuerySelector("h2#card-decisions")!.TextContent.Trim().Should().Be("Recent decisions");
        section.QuerySelector(".dashboard-card-body p")!.TextContent.Should().Be("Rows");
    }

    [Fact]
    public void CardWithHref_RendersAsAnchor()
    {
        var cut = RenderComponent<DashboardCard>(parameters => parameters
            .Add(p => p.Title, "Linked")
            .Add(p => p.Href, "/test")
            .AddChildContent("<p>Click</p>"));

        cut.Markup.Should().Contain("<a ");
        cut.Markup.Should().Contain("href=\"/test\"");
        cut.Markup.Should().Contain("detail-card--interactive");
    }

    [Fact]
    public void CardWithoutHref_RendersAsDiv()
    {
        var cut = RenderComponent<DashboardCard>(parameters => parameters
            .Add(p => p.Title, "Static")
            .AddChildContent("<p>No link</p>"));

        cut.Markup.Should().NotContain("<a ");
        cut.Markup.Should().Contain("<div ");
    }

    [Fact]
    public void Span2_HasSpanClass()
    {
        var cut = RenderComponent<DashboardCard>(parameters => parameters
            .Add(p => p.Title, "Wide")
            .Add(p => p.Span, 2)
            .AddChildContent("<p>Wide card</p>"));

        cut.Markup.Should().Contain("dashboard-span-2");
    }

    [Fact]
    public void Span3_HasSpanClass()
    {
        var cut = RenderComponent<DashboardCard>(parameters => parameters
            .Add(p => p.Title, "Full")
            .Add(p => p.Span, 3)
            .AddChildContent("<p>Full width</p>"));

        cut.Markup.Should().Contain("dashboard-span-3");
    }

    [Fact]
    public void CardWithIcon_RendersIconComponent()
    {
        var cut = RenderComponent<DashboardCard>(parameters => parameters
            .Add(p => p.Title, "With Icon")
            .Add(p => p.Icon, "book")
            .AddChildContent("<p>Content</p>"));

        cut.Markup.Should().Contain("book.svg");
    }
}
