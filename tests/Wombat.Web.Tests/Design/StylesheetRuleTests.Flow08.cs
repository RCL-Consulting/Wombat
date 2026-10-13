using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T364 (flow 08, the build review's fix pass): the schedule page's held-seat reason on a phone, and the seat picker's
/// stacked row header kept to the seat picker. In a file of its own so the flows do not meet in one test file.
/// </summary>
public sealed partial class StylesheetRuleTests
{
    private const string Flow08Phone = "@media (max-width: 640.98px)";

    /// <summary>
    /// D1: below 641px the schedule page's actions stack column-reverse (the primary on top), which put "Create review is
    /// not offered…" under Open review and Cancel. The reason is the last item by order, so the reversed column shows it
    /// first, before the buttons it explains.
    /// </summary>
    [Fact]
    public void TheHeldSeatsReason_IsAboveTheStackedActions_OnAPhone()
    {
        var css = Stylesheet.AppCss();

        css.RulesFor(".committee-calendar .schedule-grid .form-actions", Flow08Phone).Should().ContainSingle()
            .Which.Value("flex-direction").Should().Be("column-reverse");
        css.RulesFor(".committee-calendar .schedule-grid .form-actions > .move-reasons", Flow08Phone).Should().ContainSingle()
            .Which.Value("order").Should().Be("1");
    }

    /// <summary>
    /// D5: the stacked row header loses its tint, edge and padding only in the seat picker, never on the other stacked
    /// tables of the five committee pages.
    /// </summary>
    [Fact]
    public void TheStackedRowHeaderRule_IsTheSeatPickersOnly()
    {
        var css = Stylesheet.AppCss();

        css.RulesFor(".committee-calendar .clinic-table--stack tbody th", Flow08Phone).Should().BeEmpty();
        css.RulesFor(".committee-calendar .panel-seats .clinic-table--stack tbody th", Flow08Phone).Should().ContainSingle()
            .Which.Value("padding").Should().Be("0");
    }
}
