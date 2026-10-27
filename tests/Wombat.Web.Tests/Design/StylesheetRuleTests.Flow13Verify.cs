using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T379 (flow 13, lane V): Verify portfolio export's rules. Its column spaces the answer and the form (A2); the details
/// list's values wrap inside their column (S21) and stack at 390 (A.7.12, V07); a hash or an id breaks anywhere; and on a
/// phone the field and Verify are 44px, Verify the row's width, scoped to the page, never a bare .btn. In a file of its own
/// so the flows do not meet in one test file.
/// </summary>
public sealed partial class StylesheetRuleTests
{
    // Flow13Phone, the phone media query, is declared once for flow 13, in StylesheetRuleTests.Flow13End.cs.

    [Fact]
    public void Verify_SpacesItsAnswerAndItsForm_AndWrapsItsValues()
    {
        var css = Stylesheet.AppCss();

        var page = css.Computed(".verify-page");
        page["display"].Should().Be("flex");
        page["flex-direction"].Should().Be("column");
        page["gap"].Should().Be("var(--space-lg)");
        page["max-width"].Should().Be("44rem");
        css.Computed(".verify-page > *")["margin"].Should().Be("0", "the page spaces its parts, not their margins");

        css.Computed(".verify-page .detail-list")["grid-template-columns"]
            .Should().Be("minmax(8rem, max-content) minmax(0, 1fr)");
        css.Computed(".verify-page .detail-list dd")["min-width"].Should().Be("0");
        css.Computed(".hash-value")["word-break"].Should().Be("break-all");
        css.Computed(".cmd-list code")["font-family"].Should().Be("var(--font-mono)");
    }

    [Fact]
    public void OnAPhone_VerifyIsOneColumn_AndItsControlsAre44px()
    {
        var css = Stylesheet.AppCss();

        css.ComputedIn(Flow13Phone, ".verify-page .detail-list")["grid-template-columns"].Should().Be("minmax(0, 1fr)");
        css.ComputedIn(Flow13Phone, ".verify-page .form-control")["min-height"].Should().Be("2.75rem");
        css.ComputedIn(Flow13Phone, ".verify-page .form-actions .btn")["min-height"].Should().Be("2.75rem");
        css.ComputedIn(Flow13Phone, ".verify-page .form-actions .btn")["width"].Should().Be("100%");
        css.RulesFor(".verify-page .btn", Flow13Phone).Should().BeEmpty("never a bare .btn (M6)");
    }
}
