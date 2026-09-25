using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Features.Trainees;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Services;
using Wombat.Web.Tests.Activities;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// T224 review: someone who holds Trainee runs no campaign, whatever role brings them to the campaign pages
/// (<see cref="MsfCampaignRules.RunsNoCampaigns" />), and the pages say so rather than leave them to find out.
/// </summary>
/// <remarks>
/// Until the review the list offered them "New campaign" and "Create the first MSF campaign", and the create form a
/// required Trainee picker with nobody in it, so a submit gave only "required" and the reason was never said
/// (DESIGN.md § A row the caller cannot change, T211). Now both pages show the reason as standing content, in the words a
/// create refusal gives; the list offers no "New campaign", and the create form is not shown.
/// </remarks>
public sealed class RunsNoCampaignsPageTests : TestContext
{
    private const string CallerId = "registrar-1";

    private readonly TestAuthorizationContext _auth;
    private readonly Sender _sender = new();

    public RunsNoCampaignsPageTests()
    {
        _auth = this.AddTestAuthorization();
        _auth.SetAuthorized("caller@test");

        Services.AddSingleton<IScopedSender>(_sender);
        Services.AddSingleton<IActivityReferenceDataService>(new StubActivityReferenceDataService());
        JSInterop.SetupVoid("wombatDialog.showModal", _ => true).SetVoidResult();
        JSInterop.SetupVoid("wombatDialog.close", _ => true).SetVoidResult();
    }

    public static TheoryData<string> RolesThatBringThemHere => new() { WombatRoles.Coordinator, WombatRoles.Administrator };

    // ─── The campaign list ───────────────────────────────────────────────────

    [Theory]
    [MemberData(nameof(RolesThatBringThemHere))]
    public void TheList_SaysWhy_OffersNoNewCampaign_AndAsksForNothing(string role)
    {
        SignIn(role, WombatRoles.Trainee);

        var cut = RenderComponent<CampaignsList>();

        StandingReason(cut).Should().Be(MsfCampaignRules.TraineeRunsNoCampaigns);
        cut.FindAll("a[href='/msf/campaigns/new']").Should().BeEmpty("they cannot create a campaign there");
        cut.Markup.Should().NotContain("Create the first MSF campaign").And.NotContain("No MSF campaigns");
        _sender.Asked.Should().BeEmpty("the list would be empty whatever it held");
    }

    [Theory]
    [MemberData(nameof(RolesThatBringThemHere))]
    public void TheList_ForSomeoneWhoHoldsNoTrainee_OffersNewCampaign_AndSaysNothingOfIt(string role)
    {
        SignIn(role);

        var cut = RenderComponent<CampaignsList>();
        cut.WaitForState(() => cut.Markup.Contains("No MSF campaigns"));

        cut.FindAll("#msf-runs-no-campaigns").Should().BeEmpty();
        Text(cut.Find("a[href='/msf/campaigns/new']")).Should().Be("New campaign");
        _sender.Asked.Should().Equal(nameof(ListMsfCampaignsForCoordinatorQuery));
    }

    // ─── The campaign page ───────────────────────────────────────────────────

    [Theory]
    [MemberData(nameof(RolesThatBringThemHere))]
    public void TheCreatePage_SaysWhy_AndShowsNoCreateForm_AndListsNobody(string role)
    {
        SignIn(role, WombatRoles.Trainee);

        var cut = RenderComponent<CampaignEdit>();

        StandingReason(cut).Should().Be(MsfCampaignRules.TraineeRunsNoCampaigns);
        cut.FindAll("#msf-subject, #msf-template-id, #msf-opens").Should().BeEmpty();
        cut.FindAll("button").Select(Text).Should().NotContain("Create campaign");
        _sender.Asked.Should().NotContain(nameof(ListTraineesForSpecialityQuery), "no trainee is listed for them");

        // The Quick template card stays: a template is about no trainee (T224 review, T225).
        cut.FindAll("h3").Select(Text).Should().Contain("Quick template");
        cut.FindAll("#msf-template-name").Should().ContainSingle();
        cut.FindAll("button").Select(Text).Should().Contain("Add template");
    }

    [Fact]
    public void TheCreatePage_ForSomeoneWhoHoldsNoTrainee_ShowsTheCreateForm_AndSaysNothingOfIt()
    {
        SignIn(WombatRoles.Coordinator);

        var cut = RenderComponent<CampaignEdit>();

        cut.FindAll("#msf-runs-no-campaigns").Should().BeEmpty();
        cut.FindAll("#msf-subject").Should().ContainSingle();
        cut.FindAll("button").Select(Text).Should().Contain("Create campaign");
    }

    [Fact]
    public void ACampaignsPage_SaysWhy_BesideItsUnavailableCard()
    {
        // The handler answers them with null, as for an id that names nothing; the card alone would not say why.
        SignIn(WombatRoles.Coordinator, WombatRoles.Trainee);

        var cut = RenderComponent<CampaignEdit>(parameters => parameters.Add(page => page.CampaignId, 7));
        cut.WaitForState(() => cut.Markup.Contains("Campaign unavailable"));

        StandingReason(cut).Should().Be(MsfCampaignRules.TraineeRunsNoCampaigns);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private void SignIn(params string[] roles)
    {
        _auth.SetRoles(roles);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, CallerId));
    }

    /// <summary>The standing reason: a warning with no live role, since it is there on every visit (DESIGN.md, T193).</summary>
    private static string StandingReason(IRenderedFragment cut)
    {
        var alert = cut.Find("#msf-runs-no-campaigns");
        alert.ClassList.Should().Contain("alert-warning");
        alert.HasAttribute("role").Should().BeFalse("standing content is not announced on every load");
        return Text(alert);
    }

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    /// <summary>Answers what the pages ask with nothing, and records what was asked.</summary>
    private sealed class Sender : IScopedSender
    {
        public List<string> Asked { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is not ListMsfTemplatesQuery)
            {
                Asked.Add(request.GetType().Name);
            }

            object? response = request switch
            {
                ListMsfTemplatesQuery => (IReadOnlyList<MsfTemplateDto>)[new MsfTemplateDto(1, "Default MSF", null, false, true, [])],
                ListMsfCampaignsForCoordinatorQuery => (IReadOnlyList<MsfCampaignSummaryDto>)[],
                ListTraineesForSpecialityQuery => (IReadOnlyList<TraineeProfileDto>)[],
                GetMsfCampaignSetupQuery => null,
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response!);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
