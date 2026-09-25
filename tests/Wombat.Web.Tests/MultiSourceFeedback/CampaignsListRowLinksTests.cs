using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// Each row of the campaign list names its links by what their pages let the coordinator do in the row's state, in the
/// campaign page's words. (T225)
/// </summary>
/// <remarks>
/// Until T225 every row read "Edit" and "Report", whatever the state: "Edit" on a released or a withdrawn campaign, which
/// nothing can change, and "Report" on a draft, which has no responses to report.
/// </remarks>
public sealed class CampaignsListRowLinksTests : TestContext
{
    private const int CampaignId = 7;

    public CampaignsListRowLinksTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("coordinator@test");
        auth.SetRoles(WombatRoles.Coordinator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "coordinator-1"));
    }

    [Theory]
    [InlineData(MsfCampaignState.Draft, "Manage", null)]
    [InlineData(MsfCampaignState.Open, "Manage", "View report")]
    [InlineData(MsfCampaignState.Closed, "View campaign", "Review and release")]
    [InlineData(MsfCampaignState.UnderReview, "View campaign", "Review and release")]
    [InlineData(MsfCampaignState.Released, "View campaign", "View report")]
    [InlineData(MsfCampaignState.Withdrawn, "View campaign", null)]
    public void EachRow_NamesItsLinksByWhatTheStateAllows(MsfCampaignState state, string campaignLink, string? reportLink)
    {
        var cut = RenderPage(Campaign(state));

        var links = Row(cut).QuerySelectorAll("a").ToList();
        links.Select(Text).Should().Equal(reportLink is null ? [campaignLink] : [campaignLink, reportLink]);
        links.Select(Text).Should().NotContain(["Edit", "Report"]);

        links[0].GetAttribute("href").Should().Be($"/msf/campaigns/{CampaignId}");
        links[0].GetAttribute("aria-label").Should().Be(
            $"{campaignLink}: the campaign for Sipho Dlamini (Annual MSF, closing 2029-03-21)");

        if (reportLink is not null)
        {
            links[1].GetAttribute("href").Should().Be($"/msf/reports/{CampaignId}");
            links[1].GetAttribute("aria-label").Should().Be(
                $"{reportLink}: the campaign for Sipho Dlamini (Annual MSF, closing 2029-03-21)");
        }

        // Each is a row action (DESIGN.md § Button system), and its accessible name starts with what it shows.
        links.Should().OnlyContain(link => link.ClassList.Contains("btn") && link.ClassList.Contains("btn-sm") && link.ClassList.Contains("btn-outline"));
        links.Should().OnlyContain(link => link.GetAttribute("aria-label")!.StartsWith(Text(link) + ":", StringComparison.Ordinal));
    }

    private IRenderedComponent<CampaignsList> RenderPage(MsfCampaignSummaryDto campaign)
    {
        Services.AddSingleton<IScopedSender>(new FakeSender(campaign));

        var cut = RenderComponent<CampaignsList>();
        cut.WaitForState(() => cut.FindAll("tbody tr").Count > 0);
        return cut;
    }

    private static IElement Row(IRenderedFragment cut) => cut.FindAll("tbody tr").Single();

    private static MsfCampaignSummaryDto Campaign(MsfCampaignState state)
        => new(CampaignId, "trainee-7", "Annual MSF", new DateOnly(2029, 3, 1), new DateOnly(2029, 3, 21), 8, 3, state, 3, 0, null)
        {
            SubjectName = "Sipho Dlamini"
        };

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class FakeSender(MsfCampaignSummaryDto campaign) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request is ListMsfCampaignsForCoordinatorQuery
                ? Task.FromResult((TResponse)(object)(IReadOnlyList<MsfCampaignSummaryDto>)[campaign])
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
