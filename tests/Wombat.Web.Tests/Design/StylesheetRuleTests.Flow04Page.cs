using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T350 (flow 04, lane C; R3-Spec § 3, § 5): the way on and the move bar's rules. In a file of its own so the page lanes
/// do not meet in one test file.
/// </summary>
public sealed partial class StylesheetRuleTests
{
    private const string Phone = "@media (max-width: 640.98px)";

    [Fact]
    public void TheWayOn_IsAColumnOnTheSpacingScale()
    {
        // Nit T3: the gap is var(--space-md), not the canvas's 0.75rem.
        var wayOn = Stylesheet.AppCss().RulesFor(".way-on").Should().ContainSingle().Which;

        wayOn.Value("display").Should().Be("flex");
        wayOn.Value("flex-direction").Should().Be("column");
        wayOn.Value("gap").Should().Be("var(--space-md)");
        wayOn.Value("margin-bottom").Should().Be("var(--space-lg)");
    }

    [Fact]
    public void OnAPhone_TheWayOnsButtonsStack_At44px()
    {
        // C8: Open the next, Back to … and Go to Home are 2.75rem below 641px, and the way on's two stack, the first on top.
        var css = Stylesheet.AppCss();

        css.CascadedIn(Phone, ".activity-page .way-on .btn")["min-height"].Should().Be("2.75rem");
        css.CascadedIn(Phone, ".activity-page .way-on-none .btn")["min-height"].Should().Be("2.75rem");
        css.CascadedIn(Phone, ".form-actions", ".way-on .form-actions", ".activity-page .way-on .form-actions")["flex-direction"].Should().Be("column");
    }

    [Fact]
    public void TheNotePanelsSendAndKeep_StackAt44px_OnAPhone_ByTheBarsOwnRule()
    {
        // C8: the panel's buttons are a .form-actions--moves, which stacks below 641px with every button 2.75rem.
        var css = Stylesheet.AppCss();

        css.CascadedIn(Phone, ".form-actions--moves")["flex-direction"].Should().Be("column");
        css.CascadedIn(Phone, ".btn", ".form-actions--moves .btn")["min-height"].Should().Be("2.75rem");
    }

    [Fact]
    public void AGreyedMovesReason_SitsBesideItsButton_NotOnARowOfItsOwn()
    {
        // Note 11: .move-reasons takes a row of its own under the bar; the beside modifier does not.
        var beside = Stylesheet.AppCss().RulesFor(".move-reasons--beside").Should().ContainSingle().Which;

        beside.Value("flex-basis").Should().Be("auto");
    }
}
