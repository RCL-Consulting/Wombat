using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T358 (flow 06, lane C; round 3 item 30): the registrar page's stack. In a file of its own so the lanes do not meet in
/// one test file.
/// </summary>
public sealed partial class StylesheetRuleTests
{
    /// <summary>
    /// Item 30: the sections one under another at one gap, the stack owning the spacing, so the built sections' own top
    /// margins (.index-section's and .standing-panel's from My progress, .list-section's after another) are reset in it.
    /// </summary>
    [Fact]
    public void TheRegistrarStack_SpacesItsSections_AndResetsTheirOwnMargins()
    {
        var css = Stylesheet.AppCss();

        var stack = css.Cascaded(".registrar-stack");
        stack["display"].Should().Be("flex");
        stack["flex-direction"].Should().Be("column");
        stack["gap"].Should().Be("var(--space-lg)");

        css.Cascaded(".index-section", ".registrar-stack > .index-section")["margin-top"].Should().Be("0");
        css.Cascaded(".standing-panel", ".registrar-stack > .standing-panel")["margin-top"].Should().Be("0");
        css.Cascaded(".list-section", ".list-section + .list-section", ".registrar-stack > .list-section")["margin-top"].Should().Be("0");

        css.Cascaded(".index-section")["margin-top"].Should().Be("var(--space-lg)", "My progress keeps its own spacing");
    }
}
