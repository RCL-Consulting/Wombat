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
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Dashboards;

/// <summary>
/// The assessor's dashboard says whose each assessment is, by the name its query carried (T250). Since T350 (flow 04, R1,
/// R2) each row is its activity's link, its full name "Type · EPA · date" and, on a second line, "from &lt;registrar&gt;",
/// as the Activity inbox's are. Until T250 the query put the subject's user id in <c>SubjectName</c>; until T350 a row was
/// its type, linked and labelled "Mini-CEX for Thandi Nkosi", then " — Thandi Nkosi".
/// </summary>
public sealed class AssessorDashboardSubjectNameTests : TestContext
{
    public AssessorDashboardSubjectNameTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("assessor@test");
        auth.SetRoles("Assessor");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "assessor-1"));
    }

    [Fact]
    public void EachRow_IsItsLink_FromWhoseAssessmentItIs()
    {
        Services.AddSingleton<IScopedSender>(new Sender(ActivityRows.AssessorHome(
            [ActivityRows.Waiting(44, subjectName: "Thandi Nkosi")],
            [ActivityRows.Decided(43, subjectName: "Sipho Dlamini")])));
        var cut = Render();

        var waiting = cut.Find("a[href='/activities/44']");
        waiting.TextContent.Should().Be("Mini-CEX (Paediatrics) · PAED-001 · 2026-09-20, from Thandi Nkosi");
        waiting.QuerySelector(".activity-link-to")!.TextContent.Should().Be("from Thandi Nkosi");
        waiting.HasAttribute("aria-label").Should().BeFalse("its own words name it");

        var decided = cut.Find("a[href='/activities/43']");
        decided.TextContent.Should().Be("Mini-CEX (Paediatrics) · PAED-001 · 2026-09-20, from Sipho Dlamini");
        decided.HasAttribute("aria-label").Should().BeFalse();
        cut.Markup.Should().NotContain("trainee-43").And.NotContain("trainee-44", "no user id is on the page");
    }

    // Note 9, C11: links that read the same across both cards are told apart by their names, the state first.
    [Fact]
    public void LinksThatReadTheSame_AreToldApartByTheirNames()
    {
        Services.AddSingleton<IScopedSender>(new Sender(ActivityRows.AssessorHome(
            [ActivityRows.Waiting(50), ActivityRows.Waiting(51, since: ActivityRows.When.AddMinutes(3))],
            [ActivityRows.Decided(52)])));
        var cut = Render();

        cut.FindAll("a.activity-link").Select(link => link.GetAttribute("aria-label")).Should().Equal(
            "Mini-CEX (Paediatrics) · PAED-001 · 2026-09-20, from Anele Dlamini, Requested, waiting since 2026-09-29 09:30 SAST",
            "Mini-CEX (Paediatrics) · PAED-001 · 2026-09-20, from Anele Dlamini, Requested, waiting since 2026-09-29 09:33 SAST",
            "Mini-CEX (Paediatrics) · PAED-001 · 2026-09-20, from Anele Dlamini, Completed");
    }

    private IRenderedComponent<AssessorDashboard> Render()
    {
        var cut = RenderComponent<AssessorDashboard>();
        cut.WaitForState(() => cut.FindAll(".skeleton").Count == 0);
        return cut;
    }

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
