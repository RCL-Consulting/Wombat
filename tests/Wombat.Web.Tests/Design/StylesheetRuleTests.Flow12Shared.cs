using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T376 (flow 12, build-contracts § 0): the people pages' shared rules, written before the lanes so each uses them as
/// given: the list pages' column, the user page's column of sections and parts (which the roles, training and sign-in
/// sections share), the note panel holding a form (F11), and their phone rules (M6, S17). In a file of its own so the flows
/// do not meet in one test file.
/// </summary>
public sealed partial class StylesheetRuleTests
{
    private const string Flow12Phone = "@media (max-width: 640.98px)";

    [Fact]
    public void ThePeoplePages_SpaceTheirPartsOnTheScale()
    {
        var css = Stylesheet.AppCss();

        var list = css.Computed(".people-page");
        list["display"].Should().Be("flex");
        list["flex-direction"].Should().Be("column");
        list["gap"].Should().Be("var(--space-lg)");

        var user = css.Computed(".user-page");
        user["gap"].Should().Be("var(--space-lg)");
        user["max-width"].Should().Be("60rem");
        css.Computed(".user-section")["gap"].Should().Be("var(--space-md)");
        css.Computed(".section-part")["border-top"].Should().Be("1px solid var(--border-color)");
        css.Computed(".user-section .actions-cell")["justify-content"].Should().Be("flex-start");
    }

    /// <summary>F11: a form opened in place is the system's note panel, its parts 24px apart.</summary>
    [Fact]
    public void ANotePanelHoldingAForm_SpacesItsParts24px()
        => Stylesheet.AppCss().Computed(".note-panel--form")["gap"].Should().Be("var(--space-lg)");

    /// <summary>
    /// M6: on a phone only what stacks takes the row's width, so the password field's Show toggle keeps its own; S17: a
    /// radio's label is its 44px target; a form's actions stack with the primary on top.
    /// </summary>
    [Fact]
    public void OnAPhone_WhatStacksTakesTheRow_AndEveryControlIs44px()
    {
        var css = Stylesheet.AppCss();

        css.ComputedIn(Flow12Phone, ".user-page .btn")["min-height"].Should().Be("2.75rem");
        css.ComputedIn(Flow12Phone, ".people-page .form-select")["min-height"].Should().Be("2.75rem");
        css.ComputedIn(Flow12Phone, ".user-page .form-actions .btn")["width"].Should().Be("100%");
        css.RulesFor(".user-page .btn", Flow12Phone).Should().NotContain(
            rule => rule.Value("width") == "100%", "a bare .btn rule would stretch the Show toggle too (M6)");
        css.ComputedIn(Flow12Phone, ".user-page .form-actions")["flex-direction"].Should().Be("column-reverse");
        css.ComputedIn(Flow12Phone, ".user-page .choice-group .form-check label")["min-height"].Should().Be("2.75rem");
    }
}
