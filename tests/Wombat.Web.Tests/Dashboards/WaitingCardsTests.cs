using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Dashboards.Coordinator;
using Wombat.Application.Features.Dashboards.SpecialityAdmin;
using Wombat.Application.Features.Dashboards.SubSpecialityAdmin;
using Wombat.Application.Features.Dashboards.Trainee;
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

    /// <summary>
    /// The T297 review: the Trainee's Activity inbox card lists the inbox's rows, which for a trainee who also assesses
    /// hold other trainees' requests. Such a row says whose it is, in its text and its link's name, as the Assessor's card
    /// does (T250); the caller's own row stays a plain link.
    /// </summary>
    [Fact]
    public void TheTraineesInboxCard_NamesTheTraineeOnARowThatIsNotTheCallersOwn()
    {
        _auth.SetRoles("Trainee", "Assessor");
        Services.AddSingleton<IScopedSender>(new Sender(new TraineeDashboardSummaryDto(
            null,
            [
                new ActivityInboxItem(21, "Mini-CEX (Paediatrics)", "requested", "Requested", When, SubjectName: "Nomsa Mahlangu"),
                new ActivityInboxItem(22, "Reflective Exercise (Paediatrics)", "draft", "Draft", When)
            ],
            [],
            [],
            IsPendingTrainee: false)));

        var cut = RenderComponent<TraineeDashboard>();
        cut.WaitForState(() => cut.FindAll("a[href='/activities/22']").Count == 1);

        // Read by row: the card is itself a link to the inbox (DashboardCard's Href), so each row's link is nested in it,
        // and the HTML parser that reads the markup back splits the nesting, and the row's inner span with it.
        var theirs = cut.Find("a[href='/activities/21']");
        theirs.GetAttribute("aria-label").Should().Be("Mini-CEX (Paediatrics) for Nomsa Mahlangu");
        Text(theirs.Closest("li")!).Should().Contain("Mini-CEX (Paediatrics) — Nomsa Mahlangu");

        var own = cut.Find("a[href='/activities/22']");
        own.HasAttribute("aria-label").Should().BeFalse("her own row needs no name");
        Text(own.Closest("li")!).Should().NotContain("—", "her own row names nobody");
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
