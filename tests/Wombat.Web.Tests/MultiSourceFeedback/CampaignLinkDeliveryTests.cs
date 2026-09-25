using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Services;
using Wombat.Web.Tests.Activities;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// An open campaign's page says how many links did not reach their respondents, never whose, and its Resend sends each
/// of them a new link. (T251)
/// </summary>
/// <remarks>
/// Until T251 the page said that each respondent had been emailed a link, as soon as the campaign opened, and nothing
/// ever said otherwise: a mail the worker dropped was only logged. The counts are the query's
/// (<see cref="MsfCampaignSetupDto.LinksNotDelivered" />, <see cref="MsfCampaignSetupDto.LinksBeingSent" />); what they
/// count is <c>MsfLinkDeliveryTests</c>.
/// </remarks>
public sealed class CampaignLinkDeliveryTests : WombatTestContext
{
    private const int CampaignId = 7;
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    public CampaignLinkDeliveryTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("coordinator@test");
        auth.SetRoles(WombatRoles.Coordinator);
        auth.SetClaims(
            new Claim(ClaimTypes.NameIdentifier, "coordinator-1"),
            new Claim(WombatClaimTypes.InstitutionId, "1"));

        Services.AddSingleton<IActivityReferenceDataService>(new StubActivityReferenceDataService());
        JSInterop.SetupVoid("wombatDialog.showModal", _ => true).SetVoidResult();
        JSInterop.SetupVoid("wombatDialog.close", _ => true).SetVoidResult();
    }

    // ─── What the page says ──────────────────────────────────────────────────

    [Theory]
    [InlineData(1, "1 link was not delivered. Resend sends each of these respondents a new link; this page never says who they are.", "Resend 1 link")]
    [InlineData(3, "3 links were not delivered. Resend sends each of these respondents a new link; this page never says who they are.", "Resend 3 links")]
    public void AnOpenCampaignWithLinksNotDelivered_SaysHowMany_NeverWhose_AndOffersResend(
        int notDelivered, string warning, string label)
    {
        var cut = Render(new DeliverySender(Open(notDelivered: notDelivered, beingSent: 0)));

        var alert = cut.Find("#msf-links-not-delivered");
        Text(alert).Should().Be(warning);
        alert.ClassList.Should().Contain("alert-warning");
        alert.HasAttribute("role").Should().BeFalse("it is standing content, there on every visit until the links are resent");

        var resend = cut.Find("#msf-resend-links");
        Text(resend).Should().Be(label);
        resend.GetAttribute("aria-describedby").Should().Be("msf-links-not-delivered");
        resend.HasAttribute("disabled").Should().BeFalse();
        resend.HasAttribute("aria-disabled").Should().BeFalse();

        cut.Markup.Should().NotContain("@example.test", "the page never lists an address once the campaign has opened");
    }

    [Fact]
    public void LinksStillBeingSent_AreCounted_WithHowToFindOutWhetherTheyArrived()
    {
        var cut = Render(new DeliverySender(Open(notDelivered: 0, beingSent: 2)));

        Text(cut.Find("#msf-links-being-sent"))
            .Should().Be("2 links are still being sent. Reload this page to see whether they were delivered.");
        cut.FindAll("#msf-links-not-delivered").Should().BeEmpty();
        cut.FindAll("#msf-resend-links").Should().BeEmpty("nothing is offered for a link that may still arrive");
    }

    [Fact]
    public void AnOpenCampaignWhoseLinksWereAllDelivered_SaysNothingOfThem_AndClaimsNoDelivery()
    {
        var cut = Render(new DeliverySender(Open(notDelivered: 0, beingSent: 0)));

        cut.FindAll("#msf-links-not-delivered").Should().BeEmpty();
        cut.FindAll("#msf-links-being-sent").Should().BeEmpty();
        cut.FindAll("#msf-resend-links").Should().BeEmpty();
        Text(cut.Find("#msf-campaign-state-note")).Should().StartWith("Open. Each respondent is emailed a link,")
            .And.NotContain("has been emailed");
    }

    [Theory]
    [InlineData(MsfCampaignState.Draft)]
    [InlineData(MsfCampaignState.UnderReview)]
    [InlineData(MsfCampaignState.Withdrawn)]
    public void ACampaignThatIsNotOpen_ShowsNoDeliveryAndNoResend(MsfCampaignState state)
    {
        var cut = Render(new DeliverySender(Open(notDelivered: 2, beingSent: 1) with { State = state }));

        cut.FindAll("#msf-links-not-delivered").Should().BeEmpty();
        cut.FindAll("#msf-links-being-sent").Should().BeEmpty();
        cut.FindAll("#msf-resend-links").Should().BeEmpty();
    }

    // ─── Resend ──────────────────────────────────────────────────────────────

    [Fact]
    public void Resend_IsSentOnce_KeepsTheFocusWhileItRuns_ThenSaysWhatItDid_AndMovesTheFocusToTheResult()
    {
        var sender = new DeliverySender(Open(notDelivered: 2, beingSent: 0)) { Hold = true };
        var cut = Render(sender);

        cut.Find("#msf-resend-links").Click();

        // Not disabled by its own resend (T234): it has the focus. It says it is unavailable, and a second press sends
        // nothing.
        var running = cut.Find("#msf-resend-links");
        running.HasAttribute("disabled").Should().BeFalse();
        running.GetAttribute("aria-disabled").Should().Be("true");
        Text(running).Should().Be("Resending links…");
        cut.Find("#msf-withdraw-campaign").HasAttribute("disabled").Should().BeTrue("a withdraw would race the resend");

        cut.Find("#msf-resend-links").Click();
        sender.Resends.Should().Be(1, "one resend is in flight");

        sender.Release();

        cut.WaitForAssertion(() => cut.FindAll(".alert-success").Should().ContainSingle(), AsyncWorkTimeout);
        Text(cut.Find(".alert-success")).Should().Be("2 new links are being sent.");
        cut.Find(".alert-success").GetAttribute("role").Should().Be("status");
        sender.Resends.Should().Be(1);

        // Read again: the links resent are being sent, and Resend, which had the focus, is gone.
        cut.FindAll("#msf-resend-links").Should().BeEmpty();
        cut.FindAll("#msf-links-not-delivered").Should().BeEmpty();
        Text(cut.Find("#msf-links-being-sent")).Should().StartWith("2 links are still being sent.");
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.ResultRegion.Id));
    }

    [Fact]
    public void ARefusedResend_IsAnnounced_AndLeavesTheFocusOnResend_ForTheRetry()
    {
        var sender = new DeliverySender(Open(notDelivered: 2, beingSent: 0))
        {
            Refusal = new InvalidOperationException(ResendMsfLinksCommandHandler.LinksNotSent)
        };
        var cut = Render(sender);

        cut.Find("#msf-resend-links").Click();

        cut.WaitForAssertion(() => cut.FindAll(".alert-danger").Should().ContainSingle());
        var refusal = cut.Find(".alert-danger");
        Text(refusal).Should().Be(ResendMsfLinksCommandHandler.LinksNotSent);
        refusal.GetAttribute("role").Should().Be("alert");
        cut.FindAll(".alert-success").Should().BeEmpty();

        var resend = cut.Find("#msf-resend-links");
        Text(resend).Should().Be("Resend 2 links");
        resend.HasAttribute("aria-disabled").Should().BeFalse();
        JSInterop.Invocations.Should().NotContain(invocation => invocation.Identifier == FocusIdentifier,
            "the button that had the focus is still there, for the retry the refusal asks for");

        cut.Find("#msf-resend-links").Click();
        sender.Resends.Should().Be(2, "the retry is sent");
    }

    /// <summary>
    /// Closed elsewhere while this page was open: the refusal says the campaign changed, the page reads it again as it is
    /// now, and with Resend gone the refusal takes the focus.
    /// </summary>
    [Fact]
    public void AResendRefusedBecauseTheCampaignClosedElsewhere_ShowsItClosed_AndMovesTheFocusToTheRefusal()
    {
        var sender = new DeliverySender(Open(notDelivered: 2, beingSent: 0))
        {
            Refusal = new InvalidOperationException(ResendMsfLinksCommandHandler.CampaignChanged),
            StateAfterRefusal = MsfCampaignState.UnderReview
        };
        var cut = Render(sender);

        cut.Find("#msf-resend-links").Click();

        cut.WaitForAssertion(() => Text(cut.Find("#msf-campaign-state")).Should().Be("Under review"));
        Text(cut.Find(".alert-danger")).Should().Be(ResendMsfLinksCommandHandler.CampaignChanged);
        cut.FindAll("#msf-resend-links").Should().BeEmpty();
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.ResultRegion.Id));
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private IRenderedComponent<CampaignEdit> Render(DeliverySender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);
        return RenderComponent<CampaignEdit>(parameters => parameters.Add(page => page.CampaignId, CampaignId));
    }

    private static MsfCampaignSetupDto Open(int notDelivered, int beingSent)
        => new(CampaignId, "Annual MSF", MsfTemplateKind.Msf, MsfCampaignState.Open,
            [MsfRespondentCategory.PeerDoctor, MsfRespondentCategory.Consultant, MsfRespondentCategory.Nurse, MsfRespondentCategory.Ahp])
        {
            SubjectName = "Sipho Dlamini",
            OpensOn = new DateOnly(2029, 3, 1),
            ClosesOn = new DateOnly(2029, 3, 21),
            Invitees = [new MsfInviteeCountDto(MsfRespondentCategory.PeerDoctor, 3, 0), new MsfInviteeCountDto(MsfRespondentCategory.Nurse, 2, 0)],
            LinksNotDelivered = notDelivered,
            LinksBeingSent = beingSent
        };

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    /// <summary>
    /// Answers the page's reads with the campaign as it is now, and each resend as the handler would: the links not
    /// delivered become links being sent. It can hold a resend until released, or refuse it.
    /// </summary>
    private sealed class DeliverySender(MsfCampaignSetupDto setup) : IScopedSender
    {
        private MsfCampaignSetupDto _setup = setup;
        private TaskCompletionSource? _held;

        public int Resends { get; private set; }

        public bool Hold { get; init; }

        public Exception? Refusal { get; init; }

        public MsfCampaignState? StateAfterRefusal { get; init; }

        public void Release() => (_held ?? throw new InvalidOperationException("Nothing was held.")).SetResult();

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case GetMsfCampaignSetupQuery:
                    return (TResponse)(object)_setup;

                case ResendMsfLinksCommand:
                    Resends++;
                    if (Refusal is not null)
                    {
                        if (StateAfterRefusal is { } state)
                        {
                            _setup = _setup with { State = state, LinksNotDelivered = 0, LinksBeingSent = 0 };
                        }

                        throw Refusal;
                    }

                    if (Hold)
                    {
                        _held = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                        await _held.Task;
                    }

                    var resent = _setup.LinksNotDelivered;
                    _setup = _setup with { LinksNotDelivered = 0, LinksBeingSent = _setup.LinksBeingSent + resent };
                    return (TResponse)(object)resent;

                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
    }
}
