using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T366 (flow 09, the build review's fix pass): the stacked note cell's padding, the failed download's Try again at 44px,
/// and the badge's alignment in a detail list kept to the two flow 09 pages. In a file of its own so the flows do not meet
/// in one test file.
/// </summary>
public sealed partial class StylesheetRuleTests
{
    private const string Flow09Phone = "@media (max-width: 640.98px)";
    private const string Flow09Stacked = "@media (max-width: 900px)";

    /// <summary>D4: the stacked row is padded already; the note cell adds none, so its line does not sit doubly indented.</summary>
    [Fact]
    public void TheStackedNoteCell_AddsNoPadding()
    {
        var css = Stylesheet.AppCss();

        css.RulesFor(".clinic-table--stack-wide td.history-note", Flow09Stacked).Should().ContainSingle()
            .Which.Value("padding").Should().Be("0");
    }

    /// <summary>D7: a failed download's Try again, inside No longer in force's row, is a 44px target on a phone.</summary>
    [Fact]
    public void AFailedDownloadsTryAgain_Is44px_OnAPhone()
    {
        var css = Stylesheet.AppCss();

        css.RulesFor(".my-authorisations .download-result .btn", Flow09Phone).Should().ContainSingle()
            .Which.Value("min-height").Should().Be("44px");
    }

    /// <summary>D12: the badge's nudge in a detail list is the two flow 09 pages', not every detail list's.</summary>
    [Fact]
    public void TheDetailListBadgeNudge_IsScopedToTheFlow09Pages()
    {
        var css = Stylesheet.AppCss();

        css.RulesFor(".detail-list dd .badge").Should().BeEmpty();
        css.RulesFor(".my-review-page .detail-list dd .badge").Should().ContainSingle();
        css.RulesFor(".my-authorisations .detail-list dd .badge").Should().ContainSingle();
    }
}
