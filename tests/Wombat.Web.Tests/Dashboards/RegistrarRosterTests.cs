using AngleSharp.Dom;
using Bunit;
using FluentAssertions;
using Wombat.Application.Features.Programme.Trainees;
using Wombat.Web.Components.Shared.Programme;
using static Wombat.Web.Tests.Dashboards.OversightHomeFixtures;

namespace Wombat.Web.Tests.Dashboards;

/// <summary>
/// RegistrarRoster (T358, flow 06; R2-Home c1, c6, c11, k4): Home's previews of Programme trainees, each row its own link
/// to the registrar's page, "n more in Programme trainees." past the rows.
/// </summary>
public sealed class RegistrarRosterTests : TestContext
{
    [Fact]
    public void ARosterRow_IsTheNameOverTheTrainingYear_ThenTwoFigures()
    {
        var cut = Render(Typical().Take(1).ToList(), total: 1);

        var row = cut.Find("ul.list-unstyled > li.roster-row");
        var link = row.QuerySelector("div > a.progress-row-link")!;
        link.GetAttribute("href").Should().Be("/programme/trainees/101");
        Text(link).Should().Be("Pieter du Plessis");
        Text(row.QuerySelector("div > p.progress-row-meta")!).Should().Be("Training year 2");
        row.QuerySelectorAll(":scope > .dashboard-metric").Select(figure => Text(figure)).Should().Equal(
            "0 of 10 EPAs met this semester", "0 of 5 EPAs met in 2026");
        cut.FindAll("p.waiting-more").Should().BeEmpty();
    }

    [Fact]
    public void PastTheRows_HowManyMoreTheListHolds()
    {
        Text(Render(Typical(), total: 8).Find("p.waiting-more")).Should().Be("3 more in Programme trainees.");
        Text(Render(Typical(), total: 6).Find("p.waiting-more")).Should().Be("1 more in Programme trainees.");
    }

    [Fact]
    public void AnExemptRow_HasTheBadgeAndWhy_InPlaceOfTheFigures()
    {
        var cut = Render([Registrar("Thabo New", 1, exemption: ProgrammeExemption.NotStarted)], total: 1);

        var row = cut.Find("li.roster-row");
        row.QuerySelectorAll(".dashboard-metric").Should().BeEmpty();
        Text(row.QuerySelector(".roster-exempt")!).Should().Be("Exempt this period Starts on 2026-01-15");
    }

    [Fact]
    public void TheFiledRows_AreTheNameOverWhenLastFiled_WithNoFigures()
    {
        var cut = RenderComponent<RegistrarRoster>(parameters => parameters
            .Add(roster => roster.Rows, [Registrar("Lerato Molefe", 4, profileId: 103)])
            .Add(roster => roster.Total, 1)
            .Add(roster => roster.FiledMeta, true));

        var row = cut.Find("li.progress-row.progress-row--link");
        row.QuerySelector("a.progress-row-link")!.GetAttribute("href").Should().Be("/programme/trainees/103");
        Text(row.QuerySelector(".progress-row-meta")!).Should().Be("Nothing filed yet · Training year 4");
        cut.FindAll(".roster-row, .dashboard-metric").Should().BeEmpty();
    }

    private IRenderedComponent<RegistrarRoster> Render(IReadOnlyList<ProgrammeTraineeRowDto> rows, int total)
        => RenderComponent<RegistrarRoster>(parameters => parameters
            .Add(roster => roster.Rows, rows.Take(5).ToList())
            .Add(roster => roster.Total, total));

    private static string Text(IElement element)
        => string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
