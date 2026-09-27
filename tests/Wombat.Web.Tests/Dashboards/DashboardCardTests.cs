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

    // R2-Landing-Assessor: "Waiting for your rating", its count as its badge, once the count is known.
    [Fact]
    public void ACount_IsTheTitlesBadge_OnceLoaded()
    {
        var cut = RenderComponent<DashboardCard>(parameters => parameters
            .Add(p => p.Title, "Waiting for your rating")
            .Add(p => p.Count, 2)
            .AddChildContent("<p>Rows</p>"));

        var badge = cut.Find("h2 .badge");
        badge.TextContent.Should().Be("2");
        badge.ClassList.Should().Contain("badge-submitted", "the info tint: waiting on someone (BadgeFor)");

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.IsLoading, true));

        cut.FindAll(".badge").Should().BeEmpty("a count is not known until the read returns");
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
