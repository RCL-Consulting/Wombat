using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T387 (flow 14, build-contracts § 0): the rules written before the lanes. An id breaks anywhere in its cell, the
/// data-rights tables' row headers read as cells, and every list's filter selects and page-size select are 44px on a phone
/// (M4, a system rule). In a file of its own so the flows do not meet in one test file.
/// </summary>
public sealed partial class StylesheetRuleTests
{
    [Fact]
    public void AnId_IsMonospaced_AndBreaksAnywhere()
    {
        var id = Stylesheet.AppCss().Computed(".request-id");

        id["font-family"].Should().Be("var(--font-mono)");

        // T387 review P10: broken anywhere it must be, kept whole where it fits; word-break: break-all split it mid-column.
        id["overflow-wrap"].Should().Be("anywhere");
        id.ContainsKey("word-break").Should().BeFalse();
    }

    [Fact]
    public void AStackedDataRightsRowHeader_DrawsNoRuleUnderIt()
        => Stylesheet.AppCss().ComputedIn("@media (max-width: 900px)", ".data-rights-table.clinic-table--stack-wide tbody th[scope=\"row\"]")
            ["border-bottom"].Should().Be("0", "T387 review P7: the card's first line, not a cell");

    [Fact]
    public void ADataRightsTablesRowHeader_ReadsAsACell()
        => Stylesheet.AppCss().Computed(".data-rights-table tbody th[scope=\"row\"]")["background"]
            .Should().Be("transparent");

    [Fact]
    public void EveryListsSelects_Are44pxOnAPhone()
    {
        var css = Stylesheet.AppCss();

        css.ComputedIn("@media (max-width: 640.98px)", ".search-container .form-select")["min-height"].Should().Be("2.75rem");
        css.ComputedIn("@media (max-width: 640.98px)", ".pager .form-select")["min-height"].Should().Be("2.75rem");
    }
}
