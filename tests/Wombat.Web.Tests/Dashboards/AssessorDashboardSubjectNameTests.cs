using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Dashboards.Assessor;
using Wombat.Web.Components.Pages.Dashboards;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Dashboards;

/// <summary>
/// The assessor's dashboard says whose each assessment is, by the name its query carried, as the coordinator's cards do
/// (T250). Until T250 the query put the subject's user id in <c>SubjectName</c> and the page left it out, so a card of
/// three Mini-CEX rows did not say whose any of them was.
/// </summary>
public sealed class AssessorDashboardSubjectNameTests : TestContext
{
    public AssessorDashboardSubjectNameTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("assessor@test");
        auth.SetRoles("Assessor");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "assessor-1"));
        Services.AddSingleton<IScopedSender>(new Sender(new AssessorDashboardSummaryDto(
            0,
            [new AcceptedActivityItem(44, "Mini-CEX", "Thandi Nkosi", "Accepted", new DateTime(2026, 3, 21, 8, 0, 0, DateTimeKind.Utc), IsOverdue: false)],
            [new RecentDecisionItem(43, "Mini-CEX", "Sipho Dlamini", "completed", "Completed", IsFinished: true, new DateTime(2026, 3, 20, 8, 0, 0, DateTimeKind.Utc))])));
    }

    [Fact]
    public void EachRow_NamesWhoseAssessmentItIs()
    {
        var cut = RenderComponent<AssessorDashboard>();
        cut.WaitForState(() => cut.FindAll(".badge").Count >= 2);

        Text(RowFor(cut, 44)).Should().Be("Mini-CEX — Thandi Nkosi Accepted");
        Text(RowFor(cut, 43)).Should().Be("Mini-CEX — Sipho Dlamini Completed");
    }

    [Fact]
    public void EachRowsLink_IsNamedByItsInstrumentAndItsTrainee()
    {
        // Several rows of one instrument would otherwise give a screen reader several links all called "Mini-CEX". The name
        // starts with the visible label, as the MSF campaign list's row links do (T206).
        var cut = RenderComponent<AssessorDashboard>();
        cut.WaitForState(() => cut.FindAll(".badge").Count >= 2);

        cut.Find("a[href='/activities/44']").GetAttribute("aria-label").Should().Be("Mini-CEX for Thandi Nkosi");
        cut.Find("a[href='/activities/43']").GetAttribute("aria-label").Should().Be("Mini-CEX for Sipho Dlamini");
    }

    private static IElement RowFor(IRenderedFragment cut, int activityId)
        => cut.FindAll("li").Single(item => item.QuerySelector($"a[href='/activities/{activityId}']") is not null);

    private static string Text(IElement element)
        => string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private sealed class Sender(AssessorDashboardSummaryDto summary) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request is GetAssessorDashboardSummaryQuery
                ? Task.FromResult((TResponse)(object)summary)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
