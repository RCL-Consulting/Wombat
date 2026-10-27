using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T379 (flow 13, lane P): the programme's end. The ended record's link to the user page is a 44px target at phone width
/// (F5), the closed list's Programme column wraps between its words down to the onboarding column's floor (S24), and the
/// Outcome's day never breaks inside itself. The profile's controls, stacked role rows and actions are flow 12's rules.
/// </summary>
public sealed partial class StylesheetRuleTests
{
    private const string Flow13Phone = "@media (max-width: 640.98px)";

    [Fact]
    public void TheAccountsLinkToTheUserPage_IsALineOfItsOwn_And44PxAtPhoneWidth()
    {
        var css = Stylesheet.AppCss();

        css.Computed(".account-open")["margin"].Should().Be("0");
        css.Computed(".account-open a")["display"].Should().Be("inline-flex");
        css.ComputedIn(Flow13Phone, ".user-page .account-open a")["min-height"].Should().Be("2.75rem");
    }

    [Fact]
    public void TheClosedLists_ProgrammeColumn_KeepsTheOnboardingFloor_AndItsDayStaysWhole()
    {
        var css = Stylesheet.AppCss();

        css.Computed(".inv-page .clinic-table th.col-wrap")["min-width"].Should().Be("7.5rem",
            "the floor holds \"2023-01-15\" whole between 901 and 1100px");
        css.Computed(".outcome-cell > span:not(.badge)")["white-space"].Should().Be("nowrap");
        css.Computed(".closed-list th[scope=\"row\"]")["overflow-wrap"].Should().Be("break-word");
    }

    [Fact]
    public void TheEndsTriggers_AreFlow12sRoleRows_StackedAtPhoneWidth()
        => Stylesheet.AppCss().ComputedIn(Flow13Phone, ".user-page .role-row")["flex-direction"].Should().Be("column");
}
