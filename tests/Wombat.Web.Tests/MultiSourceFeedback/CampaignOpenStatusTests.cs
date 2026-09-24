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
/// What the campaign editor tells the coordinator after opening a campaign. (T184)
/// </summary>
/// <remarks>
/// The success message used to say the links had been "issued to the logging email sender", the development sender
/// that only logs a mail, on every host, including one with a mail server. A refusal is shown as the handler words it:
/// since T184 a failed send says the campaign has not been opened and that it can be opened again.
/// </remarks>
public sealed class CampaignOpenStatusTests : TestContext
{
    private const int CampaignId = 5;

    public CampaignOpenStatusTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("coordinator@test");
        auth.SetRoles(WombatRoles.Coordinator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "coordinator-1"));

        Services.AddSingleton<IActivityReferenceDataService>(new StubActivityReferenceDataService());
    }

    [Fact]
    public void OpeningACampaign_SaysEachRespondentWasEmailed_WithoutNamingTheDevelopmentSender()
    {
        var sender = new FakeSender(openFailure: null);
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<CampaignEdit>(parameters => parameters.Add(page => page.CampaignId, CampaignId));

        OpenButton(cut).Click();

        cut.WaitForAssertion(() => cut.FindAll(".alert-success").Should().ContainSingle());
        sender.Opened.Should().Equal(CampaignId);

        var status = cut.Find(".alert-success").TextContent.Trim();
        status.Should().Be("Campaign opened, and each respondent has been emailed a link to respond.");
        status.Should().NotContainEquivalentOf("logging");
        cut.FindAll(".alert-danger").Should().BeEmpty();
    }

    [Fact]
    public void AFailedOpen_ShowsTheRefusal_AndNoSuccess()
    {
        var sender = new FakeSender(new InvalidOperationException(OpenMsfCampaignCommandHandler.InvitationsNotSent));
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<CampaignEdit>(parameters => parameters.Add(page => page.CampaignId, CampaignId));

        OpenButton(cut).Click();

        cut.WaitForAssertion(() => cut.FindAll(".alert-danger").Should().ContainSingle());
        cut.Find(".alert-danger").TextContent.Trim().Should().Be(OpenMsfCampaignCommandHandler.InvitationsNotSent);
        cut.FindAll(".alert-success").Should().BeEmpty();
    }

    private static AngleSharp.Dom.IElement OpenButton(IRenderedComponent<CampaignEdit> cut)
        => cut.FindAll("button").Single(button => button.TextContent.Trim() == "Open campaign");

    private sealed class FakeSender(Exception? openFailure) : IScopedSender
    {
        public List<int> Opened { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object response = request switch
            {
                ListMsfTemplatesQuery => (IReadOnlyList<MsfTemplateDto>)[new MsfTemplateDto(1, "Default MSF", null, false, true, [])],
                ListTraineesForSpecialityQuery => (IReadOnlyList<TraineeProfileDto>)[],
                GetMsfCampaignSetupQuery setup => new MsfCampaignSetupDto(
                    setup.CampaignId, "Default MSF", MsfTemplateKind.Msf, MsfCampaignState.Draft,
                    [MsfRespondentCategory.PeerDoctor, MsfRespondentCategory.Nurse]),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            if (request is not OpenMsfCampaignCommand open)
            {
                throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }

            if (openFailure is not null)
            {
                return Task.FromException(openFailure);
            }

            Opened.Add(open.CampaignId);
            return Task.CompletedTask;
        }
    }
}
