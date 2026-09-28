using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// My account's stylesheet half (T339, flow 02; R3-MA-*, R3-Spec): the stacked details list, the sign-in rows, ConfirmDialog's
/// form mode at 448px and 32px, and E7's 44px controls below 641px. Lane C's rules sit in one delimited block of app.css.
/// </summary>
public sealed class MyAccountStyleTests
{
    private const string Phone = "@media (max-width: 640.98px)";

    [Fact]
    public void LaneCsRules_AreOneDelimitedBlock()
    {
        var css = File.ReadAllText(Stylesheet.WebFile("wwwroot", "app.css"));

        var open = css.IndexOf("/* T339 lane C: My account */", StringComparison.Ordinal);
        var close = css.IndexOf("/* end T339 lane C */", StringComparison.Ordinal);
        open.Should().BePositive();
        close.Should().BeGreaterThan(open);
        css.IndexOf("/* T339 lane C: My account */", open + 1, StringComparison.Ordinal).Should().Be(-1, "one block");
        css[open..close].Should().Contain(".signin-method {").And.Contain(".dialog-form {").And.Contain(".details-list--stacked {");
    }

    // Every selector in the sheet that matches the stacked list's element, the base rule's with it, so the cascade decides
    // (specificity, then source order). Until the step F review the test read the stacked rule alone, and passed while
    // the later base rule, of the same specificity, set each term beside its value in the browser.
    private static readonly string[] StackedList = [".details-list", ".details-list--stacked", ".details-list.details-list--stacked"];

    private static readonly string[] StackedRow =
        [".details-list > div", ".details-list--stacked > div", ".details-list.details-list--stacked > div"];

    [Fact]
    public void TheStackedDetailsList_PutsEachTermAboveItsValue_12pxBetweenRows_OverTheBaseList_AndAValueWrapsAnywhere()
    {
        var app = Stylesheet.AppCss();

        app.Cascaded(StackedList)["gap"].Should().Be("0.75rem", "12px between the rows (DESIGN.md § My account)");
        foreach (var row in new[] { app.Cascaded(StackedRow), app.CascadedIn(Phone, StackedRow) })
        {
            row["display"].Should().Be("flex", "the stacked row wins over .details-list > div's grid");
            row["flex-direction"].Should().Be("column", "each term above its value");
            row["gap"].Should().Be("var(--space-xs)");
        }

        app.Cascaded(".details-list dd", ".details-list--stacked dd", ".details-list.details-list--stacked dd")["overflow-wrap"]
            .Should().Be("anywhere", "a 40-character address stays in its card");
    }

    [Theory]
    [InlineData(".my-account-card .alert")]
    [InlineData(".dialog-form .alert")]
    public void AnAlertInACardOrTheDialog_KeepsTheBlocksRhythm_NotItsOwnMargin(string selector)
    {
        // .alert's own 40px margin-bottom would stand under a result on top of the card's 16px gap (T339 step F review).
        var alert = Stylesheet.AppCss().Cascaded(".alert", ".alert-danger", ".alert-success", selector);

        alert["margin-top"].Should().Be("0");
        alert["margin-bottom"].Should().Be("0");
    }

    [Fact]
    public void TheDialogsActionRow_HasTheTopRuleEveryActionRowHas()
    {
        var app = Stylesheet.AppCss();

        // As .form-actions, the action mode's row (FormActions): a rule above, 16px under it.
        var actions = app.Cascaded(".dialog-actions");
        actions["border-top"].Should().Be("1px solid var(--border-color)").And.Be(app.Computed(".form-actions")["border-top"]);
        actions["padding-top"].Should().Be("var(--space-md)").And.Be(app.Computed(".form-actions")["padding-top"]);
    }

    [Fact]
    public void ASignInRow_IsItsNameThenItsAction_WhichStacksAtPhoneWidth()
    {
        var app = Stylesheet.AppCss();

        app.Computed(".signin-method")["display"].Should().Be("flex");
        app.Computed(".signin-method-reason")["color"].Should().Be("var(--muted-text)", "5.09:1 on the surface (R3-Spec)");
        app.ComputedIn(Phone, ".signin-method")["flex-direction"].Should().Be("column", "each sign-in row stacks at 390 (R3-MA-390-Linked)");
    }

    [Fact]
    public void TheDialogsFormMode_Is448Px_Padded32_And24WithStackedButtons_AtPhoneWidth()
    {
        var app = Stylesheet.AppCss();

        app.Computed(".dialog-card")["max-width"].Should().Be("28rem", "448px (C7)");
        app.Computed(".dialog-form")["padding"].Should().Be("var(--space-xl)");
        app.ComputedIn(Phone, ".dialog-form")["padding"].Should().Be("var(--space-lg)");
        app.ComputedIn(Phone, ".dialog-actions")["flex-direction"].Should().Be("column-reverse", "stacked, the action first");
    }

    [Fact]
    public void MyAccountsControls_Are44Px_BelowTheSmallBreakpoint()
    {
        var app = Stylesheet.AppCss();

        app.ComputedIn(Phone, ".my-account .btn")["min-height"].Should().Be("2.75rem", "E7");
        app.ComputedIn(Phone, ".my-account .form-control")["min-height"].Should().Be("2.75rem", "E7");
    }
}
