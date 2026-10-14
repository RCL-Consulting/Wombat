using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T366 (flow 09, board R3-3; T323): the committee's resolve form is one column at read width, its Outcome's two radios
/// one a line, so each outcome reads whole at every width. In a file of its own so the lanes do not meet in one test file.
/// </summary>
public sealed partial class StylesheetRuleTests
{
    [Fact]
    public void TheResolveForm_IsOneColumn_AtReadWidth()
    {
        var css = Stylesheet.AppCss();

        css.RulesFor(".resolve-form").Should().ContainSingle()
            .Which.Value("max-width").Should().Be("48rem");
        css.RulesFor(".resolve-form .form-grid").Should().ContainSingle()
            .Which.Value("grid-template-columns").Should().Be("minmax(0, 1fr)");
        css.RulesFor(".resolve-form .check-grid.resolve-outcomes").Should().ContainSingle()
            .Which.Value("grid-template-columns").Should().Be("minmax(0, 1fr)");
    }
}
