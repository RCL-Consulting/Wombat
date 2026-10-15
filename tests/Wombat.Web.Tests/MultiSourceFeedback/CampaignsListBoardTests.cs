using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// The campaign list as round 3 drew it (T369, L1–L9): Trainee, the template over its kind, a state badge, Responses "—"
/// until a campaign opened (T270), the plain count in its heading, the caption, the rows stacked below 641px (T323) and
/// named apart (T280), New campaign then MSF coverage, and its loading and load error (T329).
/// </summary>
public sealed class CampaignsListBoardTests : WombatTestContext
{
    private static readonly DateOnly ClosesOn = new(2026, 10, 28);

    public CampaignsListBoardTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("coordinator@test");
        auth.SetRoles(WombatRoles.Coordinator);
        auth.SetClaims(
            new Claim(ClaimTypes.NameIdentifier, "coordinator-1"),
            new Claim(WombatClaimTypes.InstitutionId, "1"));

        JSInterop.SetupVoid("wombatDialog.showModal", _ => true).SetVoidResult();
        JSInterop.SetupVoid("wombatDialog.close", _ => true).SetVoidResult();
    }

    [Fact]
    public void None_SaysSo_UnderNewCampaignThenMsfCoverage()
    {
        var cut = Render(new ListSender());

        cut.WaitForAssertion(() => cut.Find(".detail-card--empty").TextContent.Should().Contain("No MSF campaigns"));
        cut.Find(".detail-card--empty").TextContent.Should().Contain("Create the first MSF campaign to start respondent outreach.");
        cut.FindAll(".msf-header .actions-cell a").Select(Text).Should().Equal("New campaign", "MSF coverage");
        cut.Find(".msf-header .actions-cell a").QuerySelector("svg.icon").Should().NotBeNull();
    }

    [Fact]
    public void ADraftAndAnOpenCampaign_ReadAsDrawn_NewestFirst()
    {
        var cut = Render(new ListSender(
            Campaign(2, "Pieter du Plessis", MsfCampaignState.Draft, invited: 0, responded: 0, hasOpened: false),
            Campaign(1, "Lerato Molefe", MsfCampaignState.Open, invited: 9, responded: 0, hasOpened: true)));

        Text(cut.Find("#msf-campaigns-heading")).Should().Be("Campaigns (2)", "a plain count, as My activities'");
        var table = cut.Find("table");
        table.ClassList.Should().Contain(["clinic-table", "clinic-table--stack"]);
        Text(table.QuerySelector("caption")!).Should().Be(
            "MSF campaigns, newest first. Counted by respondent group: this list never shows who was invited or who responded.");
        table.QuerySelectorAll("thead th").Select(Text)
            .Should().Equal("Trainee", "Template", "State", "Invitations", "Responses", "Closes", "Actions");

        Rows(cut).Should().Equal(
            "Pieter du Plessis | Default MSFMulti-source feedback | Draft | 0 | — | 2026-10-28 | Manage Withdraw…",
            "Lerato Molefe | Default MSFMulti-source feedback | Open | 9 | 0 | 2026-10-28 | Manage View report Withdraw…");

        var first = cut.FindAll("tbody tr").First();
        first.QuerySelector("th")!.ClassList.Should().Contain("trainee-cell");
        first.QuerySelector("th")!.GetAttribute("scope").Should().Be("row");
        first.QuerySelector(".kind-line")!.TextContent.Should().Be("Multi-source feedback");
        first.QuerySelector(".badge")!.ClassList.Should().Contain(BadgeFor.MsfCampaign(MsfCampaignState.Draft));
        first.QuerySelectorAll("td").Select(cell => cell.GetAttribute("data-label"))
            .Should().Equal("Template", "State", "Invitations", "Responses", "Closes", null);
        first.QuerySelector("td.nowrap")!.TextContent.Should().Be("2026-10-28");
        first.QuerySelectorAll(".actions-cell a, .actions-cell button").Select(control => control.GetAttribute("aria-label"))
            .Should().Equal(
                "Manage: the campaign for Pieter du Plessis (Default MSF, closing 2026-10-28)",
                "Withdraw: the campaign for Pieter du Plessis (Default MSF, closing 2026-10-28)");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void AReleasedAndAWithdrawnCampaign_OfferWhatTheirPagesDo()
    {
        var cut = Render(new ListSender(
            Campaign(2, "Pieter du Plessis", MsfCampaignState.Withdrawn, invited: 0, responded: 0, hasOpened: false),
            Campaign(1, "Lerato Molefe", MsfCampaignState.Released, invited: 9, responded: 8, hasOpened: true)));

        Rows(cut).Should().Equal(
            "Pieter du Plessis | Default MSFMulti-source feedback | Withdrawn | 0 | — | 2026-10-28 | View campaign",
            "Lerato Molefe | Default MSFMulti-source feedback | Released | 9 | 8 | 2026-10-28 | View campaign View report");
    }

    [Fact]
    public void ACampaignUnderReview_OffersReviewAndRelease_AndWithdraw()
    {
        var cut = Render(new ListSender(Campaign(1, "Lerato Molefe", MsfCampaignState.UnderReview, invited: 9, responded: 8, hasOpened: true)));

        Rows(cut).Should().Equal(
            "Lerato Molefe | Default MSFMulti-source feedback | Under review | 9 | 8 | 2026-10-28 | View campaign Review and release Withdraw…");
    }

    [Fact]
    public void TwoRowsThatReadTheSame_AreNamedApart()
    {
        // T280: two campaigns for one trainee on one template closing on one day differ only by their state.
        var cut = Render(new ListSender(
            Campaign(2, "Sipho Ndlovu", MsfCampaignState.Draft, invited: 0, responded: 0, hasOpened: false),
            Campaign(1, "Sipho Ndlovu", MsfCampaignState.Released, invited: 5, responded: 5, hasOpened: true)));

        cut.FindAll("tbody tr a").Where(link => Text(link) == "View campaign" || Text(link) == "Manage")
            .Select(link => link.GetAttribute("aria-label"))
            .Should().Equal(
                "Manage: the campaign for Sipho Ndlovu (Default MSF, closing 2026-10-28), Draft",
                "View campaign: the campaign for Sipho Ndlovu (Default MSF, closing 2026-10-28), Released");
    }

    [Fact]
    public void WhileTheListIsRead_ItsHeadingShows_WithSkeletonRows_SaidOnce()
    {
        var sender = new ListSender(Campaign(1, "Lerato Molefe", MsfCampaignState.Open, 9, 0, true)) { HoldRead = true };
        var cut = Render(sender);

        Text(cut.Find(".list-section-title")).Should().Be("Campaigns");
        Text(cut.Find("p[role=status]")).Should().Be("Loading MSF campaigns.");
        cut.FindAll(".skeleton").Should().HaveCount(4);
        cut.FindAll("table").Should().BeEmpty();

        sender.ReleaseRead();
        cut.WaitForAssertion(() => Text(cut.Find("#msf-campaigns-heading")).Should().Be("Campaigns (1)"), AsyncWorkTimeout);
    }

    [Fact]
    public void AFailedRead_IsAFixedSentenceWithTryAgain_NeverTheExceptionsText()
    {
        var sender = new ListSender(Campaign(1, "Lerato Molefe", MsfCampaignState.Open, 9, 0, true)) { ReadFailures = 1 };
        var cut = Render(sender);

        cut.WaitForAssertion(() => Text(cut.Find(".alert-danger .alert-row-text")).Should().Be(
            "Could not load MSF campaigns. Nothing has changed. Try again, or come back in a few minutes."));
        cut.Markup.Should().NotContain("pool exhausted");
        cut.FindAll(".detail-card--empty").Should().BeEmpty("a failed read is not an empty list");

        cut.FindAll("button").Single(button => Text(button) == "Try again").Click();
        cut.WaitForAssertion(() => Text(cut.Find("#msf-campaigns-heading")).Should().Be("Campaigns (1)"));
    }

    private IRenderedComponent<CampaignsList> Render(ListSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);
        return RenderComponent<CampaignsList>();
    }

    private static IReadOnlyList<string> Rows(IRenderedFragment cut)
        => cut.FindAll("tbody tr").Select(row => string.Join(" | ", row.Children.Select(Cell))).ToList();

    /// <summary>A cell's words; an actions cell's, each control's words apart.</summary>
    private static string Cell(IElement cell)
        => cell.QuerySelector(".actions-cell") is { } actions
            ? string.Join(" ", actions.Children.Select(Text))
            : Text(cell);

    private static MsfCampaignSummaryDto Campaign(
        int id, string subjectName, MsfCampaignState state, int invited, int responded, bool hasOpened)
        => new(id, $"trainee-{id}", "Default MSF", ClosesOn.AddDays(-14), ClosesOn, 5, 3, state, invited, responded, null)
        {
            SubjectName = subjectName,
            HasOpened = hasOpened
        };

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class ListSender(params MsfCampaignSummaryDto[] campaigns) : IScopedSender
    {
        private TaskCompletionSource? _held;
        private int _readFailures;

        public bool HoldRead { get; init; }

        public int ReadFailures { init => _readFailures = value; }

        public void ReleaseRead() => (_held ?? throw new InvalidOperationException("No read was held.")).SetResult();

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is not ListMsfCampaignsForCoordinatorQuery)
            {
                throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }

            if (HoldRead && _held is null)
            {
                _held = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                await _held.Task;
            }

            if (_readFailures > 0)
            {
                _readFailures--;
                throw new TimeoutException("pool exhausted");
            }

            return (TResponse)(object)(IReadOnlyList<MsfCampaignSummaryDto>)campaigns.ToList();
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
