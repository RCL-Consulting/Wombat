using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// The auth card's and the password field's stylesheet half (T339, flow 02; R3-SI-*, R3-SI-Toggle, R3-CP-*, R3-Spec): the
/// 480px top-aligned card with its 16px rhythm and 2rem heading (E8), the toggle beside its field and its pressed fill (E9),
/// the [hidden] rule the static toggle depends on, the institutions' 44px buttons, and E7's 44px controls below 641px on the
/// auth card and Change password. Lane B's rules sit in delimited blocks of app.css.
/// </summary>
public sealed class AuthCardStyleTests
{
    private const string Phone = "@media (max-width: 640.98px)";

    private const string Card = ".account-form-container";

    private const string NarrowCard = ".account-form-container:not(.account-form-container--wide)";

    [Theory]
    [InlineData("the auth card")]
    [InlineData("the institutions' buttons")]
    [InlineData("the password field")]
    public void LaneBsRules_AreDelimitedBlocks(string block)
    {
        var css = File.ReadAllText(Stylesheet.WebFile("wwwroot", "app.css"));

        var open = css.IndexOf($"/* T339 lane B: {block} */", StringComparison.Ordinal);
        var close = css.IndexOf($"/* end T339 lane B: {block} */", StringComparison.Ordinal);
        open.Should().BePositive();
        close.Should().BeGreaterThan(open);
    }

    [Fact]
    public void TheHiddenAttribute_Hides_WhateverTheClassDisplaysItAs()
    {
        // A3: .btn's inline-flex beat the browser's own [hidden], so a hidden toggle showed as a dead button.
        var rule = Stylesheet.AppCss().RulesFor("[hidden]").Should().ContainSingle().Which;
        rule.Declarations.Should().ContainSingle(declaration => declaration.Property == "display")
            .Which.Should().Match<CssDeclaration>(declaration => declaration.Value == "none" && declaration.Important);
    }

    [Fact]
    public void TheCard_Is480Px_TopAligned_Padded40_With16PxBetweenItsBlocks()
    {
        var app = Stylesheet.AppCss();

        var card = app.Computed(Card);
        card["max-width"].Should().Be("30rem", "480px as built (E8)");
        card["padding"].Should().Be("2.5rem");
        card["display"].Should().Be("flex");
        card["flex-direction"].Should().Be("column");
        card["gap"].Should().Be("var(--space-md)", "16px between blocks (E8)");
        card["margin"].Should().Be("0 auto", "centred across, never down: auto margins in a flex row centre both ways");
        card["box-shadow"].Should().Be("var(--shadow-raised)");
        app.Computed(".auth-page-main")["align-items"].Should().Be("flex-start", "the card is top-aligned (E8)");
        app.Computed($"{NarrowCard} form")["gap"].Should().Be("var(--space-md)");
        app.ComputedIn(Phone, NarrowCard)["padding"].Should().Be("var(--space-lg)", "24px below 641px");
    }

    [Fact]
    public void TheCardsHeading_Is2Rem_And15RemAtPhoneWidth()
    {
        var app = Stylesheet.AppCss();

        app.Computed($"{Card} h1")["font-size"].Should().Be("2rem");
        app.ComputedIn(Phone, $"{Card} h1")["font-size"].Should().Be("1.5rem", "E8");
    }

    [Fact]
    public void TheWideCard_KeepsItsOwnFlow()
    {
        // The MSF respondent's questionnaire (T205) keeps its margins and its <h2>s: the new rhythm is the auth card's.
        var app = Stylesheet.AppCss();

        app.Computed(".account-form-container--wide")["display"].Should().Be("block");
        app.Computed(".account-form-container--wide h2")["font-size"].Should().Be("2rem");
    }

    [Fact]
    public void TheToggle_StandsBesideItsField_AsTallAsIt_AndPressedTakesTheFill()
    {
        var app = Stylesheet.AppCss();

        var field = app.Computed(".password-field");
        field["display"].Should().Be("flex");
        field["gap"].Should().Be("var(--space-sm)", "8px, where the input's focus ring sits");
        app.Computed(".password-field .form-control")["min-width"].Should().Be("0");

        app.Computed(".btn", ".btn-outline", ".password-toggle")["min-height"].Should().Be("2.375rem", "38px, the field's height");
        app.ComputedIn(Phone, ".btn", ".btn-outline", ".password-toggle")["min-height"].Should().Be("2.75rem", "E7");

        var pressed = app.Computed(".password-toggle[aria-pressed=\"true\"]");
        pressed["background-color"].Should().Be("var(--secondary-color)");
        pressed["color"].Should().Be("var(--on-fill)", "4.86:1 (R3-Spec § Contrast)");
        app.Computed(".password-toggle[aria-pressed=\"true\"] .password-toggle-eye")["display"].Should().Be("none");
        app.Computed(".password-toggle:not([aria-pressed=\"true\"]) .password-toggle-eye-off")["display"].Should().Be("none");
    }

    [Fact]
    public void TheOldInFieldToggle_IsGone()
    {
        var app = Stylesheet.AppCss();

        app.RulesFor(".password-toggle-btn").Should().BeEmpty();
        app.RulesFor(".password-wrapper").Should().BeEmpty();
        app.RulesFor(".password-wrapper .form-control").Should().BeEmpty();
    }

    [Fact]
    public void TheRules_AndTheCardsQuietWords_AreMutedOnTheSurface()
    {
        var app = Stylesheet.AppCss();

        app.Computed(".password-rules")["color"].Should().Be("var(--muted-text)", "5.09:1 on the surface");
        app.Computed(".account-purpose")["color"].Should().Be("var(--muted-text)");
        app.Computed(".sso-divider span")["color"].Should().Be("var(--muted-text)");
        app.Computed(".account-note")["color"].Should().Be("var(--muted-text)");
    }

    [Fact]
    public void AnInstitutionsButton_IsAFullWidth44PxRow_ItsWordsAtTheLeft()
    {
        var button = Stylesheet.AppCss().Computed(".sso-button");

        button["min-height"].Should().Be("2.75rem");
        button["width"].Should().Be("100%");
        button["justify-content"].Should().Be("flex-start");
        button["text-align"].Should().Be("left", "a long name wraps inside the button");
    }

    [Theory]
    [InlineData(NarrowCard + " .form-control")]
    [InlineData(NarrowCard + " .btn")]
    [InlineData(".change-password .form-control")]
    [InlineData(".change-password .btn")]
    [InlineData(".account-form-container .form-check label")] // Remember me: the label is the target (T339 step G, A.7.12)
    public void BelowTheSmallBreakpoint_TheControlsAre44Px(string selector)
        => Stylesheet.AppCss().ComputedIn(Phone, selector)["min-height"].Should().Be("2.75rem", "E7");

    [Theory]
    [InlineData(NarrowCard + " .form-actions")]
    [InlineData(".change-password .form-actions")]
    public void BelowTheSmallBreakpoint_AnActionRowStacks_ThePrimaryOnTop_EachButtonFullWidth(string selector)
    {
        var row = Stylesheet.AppCss().ComputedIn(Phone, selector);

        row["flex-direction"].Should().Be("column-reverse", "the primary is last in the row, so it stands on top");
        row["align-items"].Should().Be("stretch");
    }

    [Fact]
    public void ChangePassword_IsOneColumnOfAbout640Px()
        => Stylesheet.AppCss().Computed(".change-password")["max-width"].Should().Be("40rem");
}
