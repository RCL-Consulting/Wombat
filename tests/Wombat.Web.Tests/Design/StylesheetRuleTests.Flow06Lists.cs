using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T358 (flow 06, the replay's A.7.8a at 768px): Programme trainees' and Waiting for assessors' tables stack from 900px,
/// where the sidebar leaves the main column too narrow for their columns, as the dashboard grid goes to one column.
/// </summary>
public sealed partial class StylesheetRuleTests
{
    private const string ListsStack = "@media (max-width: 900px)";

    [Fact]
    public void TheTwoListsTables_StackFrom900px_TheirHeaderRowKeptForAScreenReader()
    {
        var css = Stylesheet.AppCss();

        css.CascadedIn(ListsStack, ".clinic-table--stack-wide thead")["position"].Should().Be("absolute");
        css.CascadedIn(ListsStack, ".clinic-table--stack-wide tbody tr")["display"].Should().Be("grid");
        css.CascadedIn(ListsStack, ".clinic-table--stack-wide th[scope=\"row\"]")["display"].Should().Be("block");
        css.CascadedIn(ListsStack, ".clinic-table--stack-wide td[data-label]::before")["font-weight"].Should().Be("600");
    }
}
