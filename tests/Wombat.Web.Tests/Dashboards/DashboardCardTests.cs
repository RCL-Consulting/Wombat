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
        cut.Markup.Should().Contain("<h3>");
        cut.Markup.Should().Contain("Test Card");
        cut.Markup.Should().Contain("Body");
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
