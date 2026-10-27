using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T379 (flow 13, build-contracts § 0): the rules written before the lanes. The withdrawal's badge, grey under its own name
/// (F10), and the details list's value column that may shrink below its longest word (round-3-check build note 2), so the
/// EPA page's Entrustment card no longer pushes the page sideways between 901 and about 940px. In a file of its own so the
/// flows do not meet in one test file.
/// </summary>
public sealed partial class StylesheetRuleTests
{
    [Fact]
    public void AWithdrawal_IsTheNeutralTint_UnderItsOwnName()
    {
        var css = Stylesheet.AppCss();

        var withdrawn = css.Computed(".badge-withdrawn");
        withdrawn["background"].Should().Be("var(--header-bg)");
        withdrawn["border-color"].Should().Be("var(--input-border)");
    }

    [Fact]
    public void ADetailsListsValueColumn_MayShrinkBelowItsLongestWord()
        => Stylesheet.AppCss().Computed(".details-list > div")["grid-template-columns"]
            .Should().Be("minmax(8rem, max-content) minmax(0, 1fr)");
}
