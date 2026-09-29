using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Dashboards.Administrator;
using Wombat.Application.Features.Dashboards.Assessor;
using Wombat.Application.Features.Dashboards.Coordinator;
using Wombat.Application.Features.Dashboards.SpecialityAdmin;
using Wombat.Application.Features.Dashboards.SubSpecialityAdmin;
using Wombat.Web.Components.Pages.Dashboards;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Dashboards;

/// <summary>
/// T297: the staff dashboards' waiting cards. The Coordinator's stalled rows each open their activity, where they were
/// plain text; the programme admins' "Pending reviews" says "1 activity" and no longer links to an inbox that lists only
/// what the admin can move, which read "Inbox clear" beside a count of 1 (Steps 3.53, 3.54).
/// </summary>
public sealed class WaitingCardsTests : TestContext
{
    private static readonly DateTime When = new(2026, 9, 18, 8, 0, 0, DateTimeKind.Utc);

    private readonly TestAuthorizationContext _auth;

    public WaitingCardsTests()
    {
        _auth = this.AddTestAuthorization();
        _auth.SetAuthorized("staff@test");
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "staff-1"));
    }

    [Fact]
    public void EachStalledRequest_LinksToItsActivity_NamedByItsInstrumentAndItsTrainee()
    {
        _auth.SetRoles("Coordinator");
        Services.AddSingleton<IScopedSender>(new Sender(new CoordinatorDashboardSummaryDto(
            [
                new StalledRequestItem(10, "Portfolio and Logbook Review (Paediatrics)", "Pieter du Plessis", When),
                new StalledRequestItem(21, "Mini-CEX (Paediatrics)", "Nomsa Mahlangu", When.AddDays(5))
            ],
            [])));

        var cut = RenderComponent<CoordinatorDashboard>();
        cut.WaitForState(() => cut.FindAll("a[href^='/activities/']").Count == 2);

        var links = cut.FindAll("a[href^='/activities/']").ToList();
        links.Select(link => link.GetAttribute("href")).Should().Equal("/activities/10", "/activities/21");
        var mahlangu = links.Last();
        mahlangu.GetAttribute("aria-label").Should().Be("Mini-CEX (Paediatrics) for Nomsa Mahlangu");
        Text(mahlangu.ParentElement!).Should().Be("Mini-CEX (Paediatrics) — Nomsa Mahlangu");
    }

    // The T297 review's test of the Trainee's Activity inbox card, which named another trainee's row, went with the card:
    // since T342 it is Needs you, the caller's own work only (NeedsYouCardTests).

    /// <summary>
    /// T335, flow 01 (R2-Landing-Assessor, S20): what waits on the Assessor is one card, "Waiting for your rating", its
    /// count as its badge and its rows under it. Until T335 "Pending requests" counted them, "Awaiting your review" listed
    /// them, and an Actions card linked the inbox a third time.
    /// </summary>
    [Fact]
    public void TheAssessorsWaitingWork_IsOneCard_ItsCountTheBadge()
    {
        _auth.SetRoles("Assessor");
        Services.AddSingleton<IScopedSender>(new Sender(new AssessorDashboardSummaryDto(
            12,
            [new AwaitingReviewItem(21, "Mini-CEX (Paediatrics)", "Nomsa Mahlangu", "requested", "Requested", When, IsOverdue: true)],
            [])));

        var cut = RenderComponent<AssessorDashboard>();
        cut.WaitForState(() => cut.FindAll(".detail-card").Count > 0 && cut.FindAll(".skeleton").Count == 0);

        Titles(cut).Should().Equal("Waiting for your rating", "Recent decisions");
        var waiting = cut.FindAll(".detail-card").First();
        waiting.QuerySelector("h2 .badge")!.TextContent.Should().Be("12", "the count is the inbox's, whatever the card lists");
        waiting.ClassList.Should().Contain("detail-card--warning", "one row is overdue");
        waiting.QuerySelectorAll("li a").Select(link => link.GetAttribute("href")).Should().Equal("/activities/21");
        waiting.QuerySelector(".dashboard-card-footer a")!.GetAttribute("href").Should().Be("/activities/inbox");
        cut.Markup.Should().NotContainAny(["Pending requests", "Awaiting your review", "Open my inbox"]);
    }

    /// <summary>
    /// T335, flow 01 (R2-Shell-Admin, S20): the Administrator's job status is in System health only, and the Maintenance
    /// card, four nav links over again, is gone. Its fifth, Curriculum progress, is on Curricula (CurriculaListScopeTests).
    /// </summary>
    [Fact]
    public void TheAdministratorsHome_HasSystemHealthAndUsers_AndNoMaintenanceCard()
    {
        _auth.SetRoles("Administrator");
        Services.AddSingleton<IScopedSender>(new Sender(new AdministratorDashboardSummaryDto(DatabaseHealthy: true, TotalUserCount: 12)));

        var cut = RenderComponent<AdministratorDashboard>();
        cut.WaitForState(() => cut.FindAll(".detail-card").Count > 0 && cut.FindAll(".skeleton").Count == 0);

        Titles(cut).Should().Equal("System health", "Users across institutions");
        cut.FindAll(".detail-card").First().ClassList.Should().Contain("dashboard-span-2");
        cut.FindAll("a").Should().BeEmpty("Maintenance's links were the nav's, and Curriculum progress is on Curricula");
    }

    // T328: every row of a dashboard's list is a .list-row, and no dashboard lays a row out with an inline flex style.
    [Fact]
    public void NoDashboard_LaysOutARowWithAnInlineFlexStyle()
    {
        var folder = Path.Combine(Navigation.PageAccess.SolutionRoot(), "src", "Wombat.Web", "Components", "Pages", "Dashboards");

        Directory.GetFiles(folder, "*.razor")
            .Where(file => File.ReadAllText(file).Contains("display:flex", StringComparison.Ordinal))
            .Select(Path.GetFileName)
            .Should().BeEmpty("a row is .list-row, a row of buttons .actions-cell, and a row of figures .dashboard-metric-row");
    }

    [Theory]
    [InlineData(1, "1 activity awaiting review")]
    [InlineData(2, "2 activities awaiting review")]
    [InlineData(0, "0 activities awaiting review")]
    public void TheSpecialityAdminsPendingReviews_CountsInTheSingularForOne_AndLinksToNoInbox(int count, string expected)
    {
        _auth.SetRoles("SpecialityAdmin");
        Services.AddSingleton<IScopedSender>(new Sender(new SpecialityAdminDashboardSummaryDto(count, 5, 0, Coverage())));

        var cut = RenderComponent<SpecialityAdminDashboard>();
        cut.WaitForState(() => cut.FindAll(".dashboard-metric").Count > 0);

        Text(cut.FindAll(".dashboard-metric").First()).Should().Be(expected);
        cut.FindAll("a[href='/activities/inbox']").Should().BeEmpty();
    }

    [Theory]
    [InlineData(1, "1 activity awaiting review")]
    [InlineData(3, "3 activities awaiting review")]
    public void TheSubSpecialityAdminsPendingReviews_CountsInTheSingularForOne_AndLinksToNoInbox(int count, string expected)
    {
        _auth.SetRoles("SubSpecialityAdmin");
        Services.AddSingleton<IScopedSender>(new Sender(new SubSpecialityAdminDashboardSummaryDto(count, 5, 0, Coverage())));

        var cut = RenderComponent<SubSpecialityAdminDashboard>();
        cut.WaitForState(() => cut.FindAll(".dashboard-metric").Count > 0);

        Text(cut.FindAll(".dashboard-metric").First()).Should().Be(expected);
        cut.FindAll("a[href='/activities/inbox']").Should().BeEmpty();
    }

    private static CurriculumCoverage Coverage()
        => new(new DateOnly(2026, 9, 23), "Semester 2, 2026", "July to November", [], [], 0);

    private static List<string> Titles(IRenderedFragment cut)
        => cut.FindAll(".dashboard-card-title > span:not(.badge)").Select(title => title.TextContent.Trim()).ToList();

    private static string Text(IElement element)
        => string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private sealed class Sender(object summary) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => Task.FromResult((TResponse)summary);

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
