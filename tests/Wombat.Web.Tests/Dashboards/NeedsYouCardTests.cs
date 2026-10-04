using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Dashboards.Trainee;
using Wombat.Web.Components.Pages.Dashboards;
using Wombat.Web.Components.Shared.Activities;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Dashboards;

/// <summary>
/// T342, flow 03 (E8; R3-C-Mine "home-card"): the Trainee's Home card is Needs you, drawn with My activities' own
/// component from the summary's Needs you rows, which are ListNeedsYouQuery's read (the Application's
/// DashboardNeedsYouParityTests holds that half). Until T342 it was the Activity inbox card.
/// </summary>
public sealed class NeedsYouCardTests : TestContext
{
    public NeedsYouCardTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("trainee@test");
        auth.SetRoles("Trainee");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "trainee-1"));
    }

    [Fact]
    public void TheCard_IsNeedsYou_WithMyActivitiesRowsAndWords_AndItsCountInWords()
    {
        var cut = RenderHome([ActivityRows.Returned(2), ActivityRows.Row(1)]);

        var card = Card(cut);
        card.QuerySelector("h2")!.TextContent.Should().Contain("Needs you").And.NotContain("Activity inbox");
        card.QuerySelector("h2 .badge")!.TextContent.Should().Be("2");
        card.QuerySelector("h2 .badge")!.GetAttribute("aria-hidden").Should().Be("true");
        card.QuerySelector("h2 .visually-hidden")!.TextContent.Should().Be(", 2 items");

        cut.FindComponents<NeedsYouList>().Should().ContainSingle("the same component My activities draws");
        card.QuerySelectorAll(".needs-you-row a.activity-link").Select(link => link.TextContent).Should().Equal(
            "Reflective Exercise (Paediatrics) · PAED-001 · 2026-09-09, with Sarah Botha",
            "Mini-CEX (Paediatrics) · PAED-003 · 2026-09-25, to David Naidoo");
        card.QuerySelectorAll(".needs-you-why").Select(why => why.TextContent).Should().Equal(
            "Returned to you by Sarah Botha on 2026-09-29. Change it and submit again.",
            "Not submitted yet. It is in nobody's inbox until you submit it.");

        var footer = card.QuerySelector(".dashboard-card-footer a")!;
        footer.GetAttribute("href").Should().Be("/activities/mine");
        footer.TextContent.Trim().Should().Be("Open My activities");
        footer.ClassList.Should().Contain(["btn", "btn-sm", "btn-outline"],
            "a button as the sibling footers are, which the phone rule lifts to 44px (T355, build review A2)");
        cut.Markup.Should().NotContain("/activities/inbox", "the registrar's inbox holds nothing of hers");
    }

    [Fact]
    public void TheCard_ListsTheFirstFew_AndCountsThemAll()
    {
        var rows = Enumerable.Range(1, 7).Select(id => ActivityRows.Row(id, epaCode: $"PAED-00{id}")).ToList();
        var cut = RenderHome(rows);

        var card = Card(cut);
        card.QuerySelectorAll(".needs-you-row").Should().HaveCount(GetTraineeDashboardSummaryQueryHandler.NeedsYouListed);
        card.QuerySelector("h2 .badge")!.TextContent.Should().Be("7");
    }

    [Fact]
    public void AnEmptyCard_SaysNothingNeedsYou_AndWhereRequestsAre()
    {
        var cut = RenderHome([]);

        var card = Card(cut);
        card.QuerySelector("p.muted")!.TextContent.Should().Be("Nothing needs you. Requests you have filed are in My activities.");
        card.QuerySelector("h2 .badge").Should().BeNull("no count to show");
        card.QuerySelector(".dashboard-card-footer a")!.GetAttribute("href").Should().Be("/activities/mine");
    }

    private IRenderedComponent<TraineeDashboard> RenderHome(IReadOnlyList<ActivitySummaryDto> needsYou)
    {
        Services.AddSingleton<IScopedSender>(new Sender(new TraineeDashboardSummaryDto(null, needsYou, [], null, IsPendingTrainee: false)));
        var cut = RenderComponent<TraineeDashboard>();
        cut.WaitForState(() => cut.FindAll(".skeleton").Count == 0);
        return cut;
    }

    private static AngleSharp.Dom.IElement Card(IRenderedFragment cut)
        => cut.FindAll(".detail-card").Single(card => card.QuerySelector("h2")?.TextContent.Contains("Needs you") == true);

    private sealed class Sender(object summary) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => Task.FromResult((TResponse)summary);

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
