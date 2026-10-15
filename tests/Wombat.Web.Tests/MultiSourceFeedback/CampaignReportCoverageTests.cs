using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Services;
using Wombat.Web.Tests.Activities;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// A released campaign's report says which declared EPAs were not recorded, and gives no reason. (T186)
/// </summary>
/// <remarks>
/// It said "not recorded: this EPA is no longer on the trainee's curriculum". The release does drop an EPA that has left
/// the curriculum, but "not recorded" is read from a missing evidence row, and a missing row does not show why it is
/// missing: on dev it printed that reason for EPAs still on the curriculum whose rows exist, read from a stamp the
/// campaign was released before. The committee snapshot's line says "declared but not recorded", no more.
/// </remarks>
public sealed class CampaignReportCoverageTests : TestContext
{
    private const int CampaignId = 5;

    public CampaignReportCoverageTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("coordinator@test");
        auth.SetRoles(WombatRoles.Coordinator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "coordinator-1"));

        Services.AddSingleton<IActivityReferenceDataService>(new StubActivityReferenceDataService());
    }

    [Fact]
    public void AReleasedCampaign_MarksAnUnrecordedEpa_NotRecorded_WithoutGuessingWhy()
    {
        var cut = Render(MsfCampaignState.Released);

        var items = cut.FindAll(".epa-lines li").Select(item => item.TextContent.Trim()).ToList();
        items.Should().HaveCount(2);
        items[0].Should().StartWith("PAED-001").And.NotContain("not recorded");
        items[1].Should().StartWith("PAED-002").And.EndWith(": not recorded"); // R7b's words (T369)
        cut.Markup.Should().NotContain("curriculum", "a missing evidence row does not say why it is missing");
    }

    [Fact]
    public void ACampaignNotYetReleased_MarksNothing_NotRecorded()
    {
        var cut = Render(MsfCampaignState.UnderReview);

        cut.FindAll(".epa-lines li").Should().HaveCount(2)
            .And.OnlyContain(item => !item.TextContent.Contains("not recorded"));
    }

    private IRenderedComponent<CampaignReport> Render(MsfCampaignState state)
    {
        Services.AddSingleton<IScopedSender>(new FakeSender(Report(state)));

        var cut = RenderComponent<CampaignReport>(parameters => parameters.Add(page => page.CampaignId, CampaignId));
        cut.WaitForState(() => cut.FindAll(".epa-lines li").Count > 0);
        return cut;
    }

    private static MsfCampaignAggregateReportDto Report(MsfCampaignState state)
        => new(CampaignId, "trainee-1", "Default MSF", state, 8, 3, 9, null, true, [], 2, 2,
            [
                new MsfCoveredEpaDto(101, "PAED-001", "Resuscitate a critically ill child", Recorded: true) { WillBeRecorded = true },
                new MsfCoveredEpaDto(102, "PAED-002", "Manage a child with a chronic condition", Recorded: false) { WillBeRecorded = true }
            ],
            null,
            state == MsfCampaignState.Released ? new DateTime(2026, 7, 1, 8, 0, 0, DateTimeKind.Utc) : null);

    private sealed class FakeSender(MsfCampaignAggregateReportDto report) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request is GetCampaignAggregateReportQuery
                ? Task.FromResult((TResponse)(object)report)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
