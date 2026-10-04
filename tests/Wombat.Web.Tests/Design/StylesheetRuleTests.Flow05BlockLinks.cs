using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T355 (flow 05, lane A2; R3-Spec § 3; C7, C9): the block links every flow-05 page lists by. In a file of its own so the
/// lanes do not meet in one test file.
/// </summary>
public sealed partial class StylesheetRuleTests
{
    /// <summary>Each block link, with every selector in the sheet that matches it, as the cascade reads them.</summary>
    public static TheoryData<string[]> BlockLinks() => new()
    {
        new[] { ".progress-row-link" },
        new[] { ".epa-link" },
        new[] { ".standing-rating-link" },
        new[] { ".activity-link", ".activity-block-link" },
        new[] { ".credit-link" },
        new[] { ".epa-auth-link" },
        new[] { ".activity-link", ".decided-row .activity-link" },
        new[] { ".decided-note a" }
    };

    [Theory]
    [MemberData(nameof(BlockLinks))]
    public void OnAPhone_EachBlockLinkIs44px_ItsNameAndSecondLineStacked(string[] selectors)
    {
        // C7: a 44px block whose name and .activity-link-to stack, never side by side ("…2026-09-13from Thandi Zulu").
        var link = Stylesheet.AppCss().CascadedIn(Phone, selectors);

        link["display"].Should().Be("flex");
        link["flex-direction"].Should().Be("column");
        link["justify-content"].Should().Be("center");
        link["align-items"].Should().Be("flex-start");
        link["min-height"].Should().Be("2.75rem");
    }

    [Theory]
    [MemberData(nameof(BlockLinks))]
    public void AboveAPhone_NoBlockLinkIsHeldTo44px(string[] selectors)
        => Stylesheet.AppCss().Cascaded(selectors).Should().NotContainKey("min-height");

    [Fact]
    public void OnAPhone_AnEmptyStatesButtonIs44px()
    {
        // C9: the EPA page's "Log an activity" in its empty state is .btn at 2.25rem; below 641px it is 2.75rem.
        Stylesheet.AppCss().CascadedIn(Phone, ".btn", ".detail-card--empty .btn")["min-height"].Should().Be("2.75rem");
    }

    [Fact]
    public void ADecisionsLine_TakesARowOfItsOwn_InTextColour()
    {
        // R1: the line by kind sits under the link and the badge, a row of its own, and is the row's news, not muted.
        var note = Stylesheet.AppCss().Cascaded(".decided-note");

        note["flex"].Should().Be("1 0 100%");
        note["color"].Should().Be("var(--text-color)");
    }
}
