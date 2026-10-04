using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T355 (flow 05, lane C; R3-Spec § 3; C7, C13): My progress's index and the standing panel. In a file of its own so the
/// lanes do not meet in one test file.
/// </summary>
public sealed partial class StylesheetRuleTests
{
    private const string PhoneWidth = "@media (max-width: 640.98px)";

    [Fact]
    public void TheIndexsCaption_OutranksTheBuiltCaptionRule_AndReadsAsTheGroupsName()
    {
        // C7: .index-caption alone (0,1,0) lost colour, size, padding and alignment to .clinic-table caption (0,1,1).
        Stylesheet.Specificity(".clinic-table .index-caption").Should().BeGreaterThan(Stylesheet.Specificity(".clinic-table caption"));

        var caption = Stylesheet.AppCss().Cascaded(".clinic-table caption", ".clinic-table .index-caption");
        caption["color"].Should().Be("var(--text-color)", "the group's name, not the built caption's muted note");
        caption["font-size"].Should().Be("1.1rem");
        caption["font-weight"].Should().Be("600");
        caption["text-align"].Should().Be("left");
    }

    [Fact]
    public void OnAPhone_TheIndexsRowHeader_IsItsBlocksFirstLine()
    {
        // R3-P-*-390: stacked, the EPA is a block of its own, with no cell padding or width of its own.
        var css = Stylesheet.AppCss();

        css.CascadedIn(PhoneWidth, ".clinic-table--stack th[scope=\"row\"]")["display"].Should().Be("block");

        var header = css.CascadedIn(PhoneWidth, ".clinic-table th", ".clinic-table--index tbody th");
        header["width"].Should().Be("auto");
        header["padding-top"].Should().Be("0");
        header["padding-left"].Should().Be("0");

        var wide = css.Cascaded(".clinic-table th", ".clinic-table--index tbody th");
        wide["background"].Should().Be("transparent", "a row header on the row's own ground, not the header row's");
        wide["font-weight"].Should().Be("400");
        wide["width"].Should().Be("34%");
    }

    [Fact]
    public void APausedRow_IsOnTheHeaderGround()
    {
        // C13: tr.is-paused, never .detail-card--paused; the paused mark is 4.57:1 on it (Spec § 4).
        var css = Stylesheet.AppCss();

        css.Cascaded(".clinic-table th", ".clinic-table--index tbody th", ".clinic-table--index tr.is-paused th")["background"]
            .Should().Be("var(--header-bg)");
        css.Cascaded(".clinic-table--index tr.is-paused td")["background"].Should().Be("var(--header-bg)");
    }

    [Fact]
    public void AStandingCellsSecondLine_IsMutedAndOnItsOwnLine()
    {
        var meta = Stylesheet.AppCss().Cascaded(".standing-rating-meta");

        meta["display"].Should().Be("block");
        meta["color"].Should().Be("var(--muted-text)");
    }
}
