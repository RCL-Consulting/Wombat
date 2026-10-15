using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T369 (flow 10, lane K): the campaign pages' own rules, "Flow 10: the campaign". In a file of its own so the lanes do
/// not meet in one test file.
/// </summary>
public sealed partial class StylesheetRuleTests
{
    /// <summary>
    /// S13: the count tables are never stacked (two or three narrow columns fit 358px), so no phone rule names them, and
    /// their numbers are right-aligned in tabular figures.
    /// </summary>
    [Fact]
    public void TheCountTables_StayUnstacked_TheirNumbersAligned()
    {
        var css = Stylesheet.AppCss();

        css.RulesFor(".count-table tbody th", Flow10Phone).Should().BeEmpty();
        css.Computed(".num")["text-align"].Should().Be("right");
        css.Computed(".num")["font-variant-numeric"].Should().Be("tabular-nums");
        css.Computed(".count-box")["max-width"].Should().Be("34rem");
    }

    /// <summary>S14: the add form's three parts on one row, stacked at 900px and below, Add invitee at the row's foot.</summary>
    [Fact]
    public void TheAddForm_StacksAt900px()
    {
        var css = Stylesheet.AppCss();

        css.Computed(".add-invitee")["grid-template-columns"].Should().Be("minmax(0, 2fr) minmax(0, 1.3fr) auto");
        css.Computed(".add-invitee > .btn")["align-self"].Should().Be("end");
        css.ComputedIn("@media (max-width: 900px)", ".add-invitee")["grid-template-columns"].Should().Be("minmax(0, 1fr)");
    }

    /// <summary>
    /// A.7.5 and T323: below 641px a list row's actions are a column of full-width 44px buttons, and every control on the
    /// campaign pages is 44px; scoped to the page column (BRIEF § 11 "Flow 09").
    /// </summary>
    [Fact]
    public void OnAPhone_TheCampaignPagesControlsAre44px_AndARowsActionsStack()
    {
        var css = Stylesheet.AppCss();

        css.ComputedIn(Flow10Phone, ".msf-page td .actions-cell")["flex-direction"].Should().Be("column");
        css.ComputedIn(Flow10Phone, ".msf-page td .actions-cell .btn")["width"].Should().Be("100%");
        css.ComputedIn(Flow10Phone, ".msf-page .form-control")["min-height"].Should().Be("2.75rem");
        css.ComputedIn(Flow10Phone, ".msf-page .form-select")["min-height"].Should().Be("2.75rem");
        css.ComputedIn(Flow10Phone, ".msf-page .epa-checks .form-check")["min-height"].Should().Be("2.75rem");
    }

    /// <summary>A draft's addresses are a list, the address over its group, Remove beside it, so nothing scrolls sideways.</summary>
    [Fact]
    public void ADraftsAddresses_AreRowsThatWrap()
    {
        var css = Stylesheet.AppCss();

        css.Computed(".invitee-row")["grid-template-columns"].Should().Be("minmax(0, 1fr) auto");
        css.Computed(".invitee-address")["overflow-wrap"].Should().Be("anywhere");
        css.Computed(".invitee-rows")["list-style"].Should().Be("none");
    }
}
