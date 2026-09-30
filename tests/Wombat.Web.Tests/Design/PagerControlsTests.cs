using AngleSharp.Dom;
using Bunit;
using FluentAssertions;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T350, flow 04 (C10 f; DESIGN.md § List page, R2): the pager's Previous and Next are aria-disabled at its ends, never
/// disabled. A natively disabled button drops the focus, so Next pressed onto the last page left the reader on the page's
/// body; an aria-disabled one keeps its place and focus, and a press there sends nothing. Every page with a pager (My
/// activities, Decisions due, the Activity inbox's decisions) has it.
/// </summary>
public sealed class PagerControlsTests : TestContext
{
    [Fact]
    public void OnTheFirstPage_PreviousIsAriaDisabled_NotDisabled_AndSendsNothing()
    {
        var pages = new List<int>();
        var cut = Render(page: 1, total: 45, pages);

        var previous = Button(cut, "Previous");
        previous.GetAttribute("aria-disabled").Should().Be("true");
        previous.HasAttribute("disabled").Should().BeFalse();
        Button(cut, "Next").HasAttribute("aria-disabled").Should().BeFalse();

        previous.Click();
        pages.Should().BeEmpty("Previous on page 1 sends nothing");

        Button(cut, "Next").Click();
        pages.Should().Equal(2);
    }

    [Fact]
    public void OnTheLastPage_NextIsAriaDisabled_NotDisabled_AndSendsNothing()
    {
        var pages = new List<int>();
        var cut = Render(page: 3, total: 45, pages);

        var next = Button(cut, "Next");
        next.GetAttribute("aria-disabled").Should().Be("true");
        next.HasAttribute("disabled").Should().BeFalse();
        Button(cut, "Previous").HasAttribute("aria-disabled").Should().BeFalse();

        next.Click();
        pages.Should().BeEmpty("Next on the last page sends nothing");

        Button(cut, "Previous").Click();
        pages.Should().Equal(2);
    }

    [Fact]
    public void OnOnlyOnePage_BothAreAriaDisabled()
    {
        var cut = Render(page: 1, total: 5, []);

        cut.FindAll(".pager-actions button").Select(button => button.GetAttribute("aria-disabled")).Should().Equal("true", "true");
        cut.FindAll("button[disabled]").Should().BeEmpty();
    }

    // C5; the build review's D1: the range takes an en dash, as DESIGN.md § Pager, the board and the other lists' counts
    // write it ("Showing 1–20 of 45"); "Per page:" and its sizes as DESIGN.md gives them.
    [Fact]
    public void ItsWords_AreDesignMds()
    {
        var cut = Render(page: 1, total: 45, []);

        cut.Find(".pager-info").TextContent.Should().Be("Showing 1–20 of 45");
        cut.Find(".pager-page-size-label").TextContent.Should().Be("Per page:");
        cut.FindAll(".pager-page-size-select option").Select(option => option.TextContent).Should().Equal("10", "20", "50", "100");
    }

    // The build review's D2 (R3-C-Inbox): given a name, the pager is a navigation landmark by that name; unnamed, a plain
    // block, as My activities and Decisions due have it.
    [Fact]
    public void Named_ItIsANavLandmark_ElseAPlainBlock()
    {
        var named = RenderComponent<PagerControls>(parameters => parameters
            .Add(pager => pager.TotalCount, 45)
            .Add(pager => pager.Label, "Decided by you, pages"));
        var nav = named.Find("nav.pager");
        nav.GetAttribute("aria-label").Should().Be("Decided by you, pages");
        nav.QuerySelectorAll(".pager-actions button").Should().HaveCount(2);

        Render(page: 1, total: 45, []).Find(".pager").TagName.Should().Be("DIV");
    }

    private IRenderedComponent<PagerControls> Render(int page, int total, List<int> pages)
        => RenderComponent<PagerControls>(parameters => parameters
            .Add(pager => pager.Page, page)
            .Add(pager => pager.PageSize, 20)
            .Add(pager => pager.TotalCount, total)
            .Add(pager => pager.OnPageChanged, (int next) => pages.Add(next)));

    private static IElement Button(IRenderedFragment cut, string label)
        => cut.FindAll(".pager-actions button").Single(button => button.TextContent.Trim() == label);
}
