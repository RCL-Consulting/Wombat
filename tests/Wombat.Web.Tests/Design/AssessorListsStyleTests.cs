using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T350, flow 04 (lane B, "the Assessor's Home and the inbox"): what <c>app.css</c> gives the Assessor's Home, the other-role
/// line and the Activity inbox, held to the round-2 review's corrections and nits.
/// </summary>
public sealed class AssessorListsStyleTests
{
    private const string Phone = "@media (max-width: 640.98px)";

    // C8: below 641px, Home's card footers, every Try again (an alert's action) and the pager's buttons are 44px tall.
    [Theory]
    [InlineData(".dashboard-card-footer .btn")]
    [InlineData(".alert-row .btn")]
    [InlineData(".pager .btn")]
    public void OnAPhone_TheButtonIs44PxTall(string selector)
    {
        var rule = Stylesheet.AppCss().RulesFor(selector, Phone).Should().ContainSingle().Which;

        Stylesheet.Rem(rule.Value("min-height")!).Should().Be(2.75, "44px at 16px to the rem");
    }

    // Nit T2: the decided row's meta has one align-items, not the canvas's dead first one.
    [Fact]
    public void TheDecidedMeta_SaysItsAlignmentOnce()
    {
        var rule = Stylesheet.AppCss().RulesFor(".decided-meta").Should().ContainSingle().Which;

        rule.Declarations.Count(declaration => declaration.Property == "align-items").Should().Be(1);
    }

    // C10 f: a pager's end is aria-disabled, and says it is not to be pressed.
    [Fact]
    public void APagersAriaDisabledEnd_LooksUnavailable()
        => Stylesheet.AppCss().RulesFor(".pager .btn[aria-disabled=\"true\"]").Should().ContainSingle()
            .Which.Value("cursor").Should().Be("not-allowed");

    // The other-role line's action stacks under its words on a phone, as wide as the line.
    [Fact]
    public void OnAPhone_TheOtherRoleLinesActionStandsUnderItsWords()
        => Stylesheet.AppCss().RulesFor(".other-role-line .alert-row", Phone).Should().ContainSingle()
            .Which.Value("flex-direction").Should().Be("column");
}
