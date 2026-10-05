using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T358 (flow 06, lane A1; round 3 items 22, 25): Send a reminder and the filter bar. In a file of its own so the lanes do
/// not meet in one test file.
/// </summary>
public sealed partial class StylesheetRuleTests
{
    /// <summary>Item 22: 44px and the row's width below 641px, wherever Send a reminder sits; btn-sm's 28px above.</summary>
    [Fact]
    public void SendAReminder_Is44pxAndFullWidthOnAPhone_AndASmallButtonAbove()
    {
        var css = Stylesheet.AppCss();

        var phone = css.CascadedIn(Phone, ".btn", ".btn-sm", ".reminder-action");
        phone["min-height"].Should().Be("2.75rem");
        phone["width"].Should().Be("100%");

        var desk = css.Cascaded(".btn", ".btn-sm", ".reminder-action");
        desk["min-height"].Should().Be("1.75rem");
        desk.Should().NotContainKey("width");
    }

    /// <summary>Item 25: Show and Clear filters side by side on the fields' bottom line, inside a column .search-field.</summary>
    [Fact]
    public void TheFilterBarsButtons_SitInARowOnTheFieldsBottomLine_And44pxOnAPhone()
    {
        var css = Stylesheet.AppCss();

        var actions = css.Cascaded(".search-field", ".filter-actions");
        actions["display"].Should().Be("flex");
        actions["flex-direction"].Should().Be("row");
        actions["align-self"].Should().Be("end");

        var button = css.CascadedIn(Phone, ".btn", ".filter-actions .btn");
        button["min-height"].Should().Be("2.75rem");
        button["width"].Should().Be("100%");
    }
}
