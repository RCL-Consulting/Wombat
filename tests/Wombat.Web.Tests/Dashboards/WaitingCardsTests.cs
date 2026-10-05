using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Dashboards.Administrator;
using Wombat.Application.Features.Dashboards.Assessor;
using Wombat.Web.Components.Pages.Dashboards;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Dashboards;

/// <summary>
/// T297: the staff dashboards' waiting cards. The Assessor's waiting work is one card, the Administrator's Home has no
/// Maintenance card, and no dashboard lays a row out with an inline style. The programme Homes' cards are flow 06's
/// (T358: CommitteeHomeTests, ProgrammeAdminHomeTests, CoordinatorHomeTests).
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

    // T297's Coordinator "Stalled requests" and the programme admins' "Pending reviews" went with T358 (flow 06, Q3): both
    // Homes now draw Waiting for assessors' first five, each row its own link (CoordinatorHomeTests, ProgrammeAdminHomeTests).

    // The T297 review's test of the Trainee's Activity inbox card, which named another trainee's row, went with the card:
    // since T342 it is Needs you, the caller's own work only (NeedsYouCardTests).

    /// <summary>
    /// T335, flow 01 (R2-Landing-Assessor, S20): what waits on the Assessor is one card, its count its badge and its rows
    /// under it. Until T335 "Pending requests" counted them, "Awaiting your review" listed them, and an Actions card linked
    /// the inbox a third time. Since T350 (flow 04, R1) the card is "Waiting for you", its count in words, its foot "Open
    /// Activity inbox" (AssessorHomeTests holds the rest).
    /// </summary>
    [Fact]
    public void TheAssessorsWaitingWork_IsOneCard_ItsCountTheBadge()
    {
        _auth.SetRoles("Assessor");
        Services.AddSingleton<IScopedSender>(new Sender(TestSupport.ActivityRows.AssessorHome(
            [TestSupport.ActivityRows.Waiting(21, subjectName: "Nomsa Mahlangu", waitedDays: 12, overdue: true, since: When)],
            [])));

        var cut = RenderComponent<AssessorDashboard>();
        cut.WaitForState(() => cut.FindAll(".detail-card").Count > 0 && cut.FindAll(".skeleton").Count == 0);

        Titles(cut).Should().Equal("Waiting for you", "Recent decisions");
        var waiting = cut.FindAll(".detail-card").First();
        waiting.QuerySelector("h2 .badge")!.TextContent.Should().Be("1 waiting, 1 overdue", "the count is the inbox's, in words");
        waiting.ClassList.Should().Contain("detail-card--warning", "one row is overdue");
        waiting.QuerySelectorAll("li a").Select(link => link.GetAttribute("href")).Should().Equal("/activities/21");
        waiting.QuerySelector(".dashboard-card-footer a")!.GetAttribute("href").Should().Be("/activities/inbox");
        cut.Markup.Should().NotContainAny(["Pending requests", "Awaiting your review", "Open my inbox", "Waiting for your rating"]);
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
