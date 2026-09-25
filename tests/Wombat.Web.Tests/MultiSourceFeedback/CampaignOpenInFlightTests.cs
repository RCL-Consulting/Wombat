using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Features.Trainees;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Services;
using Wombat.Web.Tests.Activities;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// The campaign editor sends an open at most once at a time. (T202)
/// </summary>
/// <remarks>
/// Before T202 the Open button stayed live while its open ran, so a double-click raced two opens and every respondent
/// was mailed two links, one of them dead (T184 made the second one fail at its save, but not before it had mailed).
/// The fake sender holds each open until the test releases it, which is the window a second click falls into.
/// </remarks>
public sealed class CampaignOpenInFlightTests : TestContext
{
    private const int CampaignId = 5;

    public CampaignOpenInFlightTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("coordinator@test");
        auth.SetRoles(WombatRoles.Coordinator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "coordinator-1"));

        Services.AddSingleton<IActivityReferenceDataService>(new StubActivityReferenceDataService());
    }

    [Fact]
    public void WhileAnOpenRuns_TheOpenAndAddButtonsAreDisabled_AndSayWhy()
    {
        var sender = new HeldOpenSender();
        var cut = Render(sender);

        OpenButton(cut).Click();

        OpenButton(cut).HasAttribute("disabled").Should().BeTrue();
        OpenButton(cut).TextContent.Trim().Should().Be("Opening campaign…");
        AddButton(cut).HasAttribute("disabled").Should().BeTrue("an invitee added mid-open would be left out of the mailing");

        sender.Release();

        // Once it has opened, the campaign is read again and offers neither (T217): until then the page went on offering
        // Open, and the draft's invitee form, on a campaign that was open.
        cut.WaitForAssertion(() => cut.FindAll(".alert-success").Should().ContainSingle());
        cut.FindAll("#msf-open-campaign").Should().BeEmpty();
        cut.FindAll("button").Should().NotContain(button => button.TextContent.Trim() == "Add invitee");
    }

    [Fact]
    public void ASecondClickWhileTheFirstOpenRuns_SendsNothing()
    {
        var sender = new HeldOpenSender();
        var cut = Render(sender);

        OpenButton(cut).Click();
        OpenButton(cut).Click();
        OpenButton(cut).Click();

        sender.Opens.Should().Be(1, "one open is in flight, and the clicks after it must not send another");

        sender.Release();
        cut.WaitForAssertion(() => cut.FindAll(".alert-success").Should().ContainSingle());
        sender.Opens.Should().Be(1);
    }

    [Fact]
    public void AnInviteeSubmittedWhileAnOpenRuns_IsNotSent()
    {
        // The open has already loaded the invitations it will mail, so an invitee added now would be stored on a campaign
        // that opens without ever mailing them.
        var sender = new HeldOpenSender();
        var cut = Render(sender);

        OpenButton(cut).Click();
        cut.Find("#msf-respondent-email").Change("peer-1@example.test");
        cut.Find("#msf-respondent-email").Closest("form")!.Submit();

        sender.Invitations.Should().Be(0);

        sender.Release();
        cut.WaitForAssertion(() => cut.FindAll(".alert-success").Should().ContainSingle());
        sender.Invitations.Should().Be(0);
    }

    [Fact]
    public void AnOpenThatIsRefused_LeavesTheButtonReadyToOpenAgain()
    {
        // A failed send tells the coordinator to open the campaign again (T184), so the button must come back.
        var sender = new HeldOpenSender();
        var cut = Render(sender);

        OpenButton(cut).Click();
        sender.Refuse(new InvalidOperationException(OpenMsfCampaignCommandHandler.InvitationsNotSent));

        cut.WaitForAssertion(() => cut.FindAll(".alert-danger").Should().ContainSingle());
        OpenButton(cut).HasAttribute("disabled").Should().BeFalse();

        OpenButton(cut).Click();
        sender.Opens.Should().Be(2, "the retry the refusal asks for is sent");
    }

    private IRenderedComponent<CampaignEdit> Render(HeldOpenSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);
        return RenderComponent<CampaignEdit>(parameters => parameters.Add(page => page.CampaignId, CampaignId));
    }

    private static AngleSharp.Dom.IElement OpenButton(IRenderedComponent<CampaignEdit> cut)
        => cut.Find("#msf-open-campaign");

    private static AngleSharp.Dom.IElement AddButton(IRenderedComponent<CampaignEdit> cut)
        => cut.FindAll("button").Single(button => button.TextContent.Trim() == "Add invitee");

    /// <summary>
    /// Holds every open until the test releases or refuses it, and counts the opens it is sent. A released open leaves the
    /// campaign open, as the handler does, and the page reads it back so (T217).
    /// </summary>
    private sealed class HeldOpenSender : IScopedSender
    {
        private TaskCompletionSource? _inFlight;
        private MsfCampaignState _state = MsfCampaignState.Draft;

        public int Opens { get; private set; }

        public int Invitations { get; private set; }

        public void Release()
        {
            var inFlight = _inFlight ?? throw new InvalidOperationException("No open was sent.");
            _state = MsfCampaignState.Open;
            inFlight.SetResult();
        }

        public void Refuse(Exception refusal)
            => (_inFlight ?? throw new InvalidOperationException("No open was sent.")).SetException(refusal);

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is AddMsfInvitationCommand)
            {
                Invitations++;
                return Task.FromResult((TResponse)(object)Invitations);
            }

            object response = request switch
            {
                ListMsfTemplatesQuery => (IReadOnlyList<MsfTemplateDto>)[new MsfTemplateDto(1, "Default MSF", null, false, true, [])],
                ListTraineesForSpecialityQuery => (IReadOnlyList<TraineeProfileDto>)[],
                GetMsfCampaignSetupQuery setup => new MsfCampaignSetupDto(
                    setup.CampaignId, "Default MSF", MsfTemplateKind.Msf, _state,
                    [MsfRespondentCategory.PeerDoctor, MsfRespondentCategory.Nurse]),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            if (request is not OpenMsfCampaignCommand)
            {
                throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }

            Opens++;
            _inFlight = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            return _inFlight.Task;
        }
    }
}
