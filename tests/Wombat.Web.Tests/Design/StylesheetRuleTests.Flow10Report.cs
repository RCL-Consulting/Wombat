using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>T369 (flow 10, lane R): the MSF report's rules. In a file of its own so the flows do not meet in one test file.</summary>
public sealed partial class StylesheetRuleTests
{
    /// <summary>The checklist's marks: met green, short amber (R1–R11); the words carry the meaning.</summary>
    [Fact]
    public void TheReportsChecklist_MarksMetGreen_AndShortAmber()
    {
        var css = Stylesheet.AppCss();

        css.Computed(".threshold-list .is-met .icon")["color"].Should().Be("var(--success-color)");
        css.Computed(".threshold-list .is-short .icon")["color"].Should().Be("var(--warning-color)");
        css.Computed(".threshold-list")["list-style"].Should().Be("none");
    }

    /// <summary>R11: a 4,000-character comment with no space stays in the column, its line breaks kept.</summary>
    [Fact]
    public void AGroupsComment_WrapsAnywhere_AndKeepsItsLineBreaks()
    {
        var comment = Stylesheet.AppCss().Computed(".group-comments li");

        comment["overflow-wrap"].Should().Be("anywhere");
        comment["white-space"].Should().Be("pre-line");
    }

    /// <summary>The release section's actions over a rule; on a phone, Release full width and 44px (M10's page rule).</summary>
    [Fact]
    public void TheReleaseActions_StackOnAPhone()
    {
        var css = Stylesheet.AppCss();

        css.Computed(".release-actions")["border-top"].Should().Be("1px solid var(--border-color)");
        css.ComputedIn(Flow10Phone, ".msf-page .release-actions")["flex-direction"].Should().Be("column");
        css.ComputedIn(Flow10Phone, ".msf-page .release-actions .btn")["width"].Should().Be("100%");
        css.ComputedIn(Flow10Phone, ".msf-page .stored")["grid-template-columns"].Should().Be("minmax(0, 1fr)");
    }
}
