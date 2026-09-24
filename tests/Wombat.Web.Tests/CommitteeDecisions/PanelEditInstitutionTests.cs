using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionsList;
using Wombat.Application.Features.Institutions.Queries.GetSpecialitiesList;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// The panel form names the panel's institution for every scope when an Administrator creates it, and for anyone else
/// leaves a Speciality-scoped panel at their own institution. (T182)
/// </summary>
/// <remarks>
/// Every panel runs at one institution now, because a panel reviews only that institution's trainees. The form used to
/// offer an institution only for an institution-wide panel, so an Administrator's Speciality-scoped panel was created
/// with none and could review nobody; the handler now refuses it without one.
/// </remarks>
public sealed class PanelEditInstitutionTests : TestContext
{
    private const int InstitutionA = 1;
    private const int InstitutionB = 2;
    private const int Paediatrics = 4;

    private readonly RecordingSender _sender = new();

    public PanelEditInstitutionTests()
    {
        Services.AddSingleton<IScopedSender>(_sender);
        _sender
            .On<GetInstitutionsListQuery>(_ => new[]
            {
                new InstitutionDto(InstitutionA, "Kgosi Kgari", "KGK", null, true, DateTime.UtcNow),
                new InstitutionDto(InstitutionB, "Other Hospital", "OTH", null, true, DateTime.UtcNow)
            })
            .On<GetSpecialitiesListQuery>(_ => new[] { new SpecialityDto(Paediatrics, 1, "Paediatrics", null, true) })
            .On<ListPanelMemberCandidatesQuery>(_ => new[]
            {
                new PanelMemberCandidateDto("zulu", "zulu@test", "Thandi", "Zulu", InstitutionA)
            })
            .On<CreateDecisionPanelCommand>(command => new DecisionPanelDetailDto(
                5, command.Name, command.Scope, command.InstitutionId ?? InstitutionA, command.SpecialityId, []));
    }

    [Fact]
    public void AnAdministrator_NamesTheInstitution_OfASpecialityPanel()
    {
        SignInAs(WombatRoles.Administrator);
        var cut = RenderComponent<PanelEdit>();
        cut.WaitForState(() => cut.FindAll("#panel-scope").Count == 1);

        cut.Find("#panel-scope").Change(DecisionPanelScope.Speciality.ToString());

        cut.FindAll("#panel-institution").Should().ContainSingle("an Administrator belongs to no institution");
        cut.FindAll("#panel-speciality").Should().ContainSingle();
        cut.Markup.Should().NotContain("The panel runs at your institution.");

        FillAndSubmit(cut, institutionId: InstitutionB);

        var command = _sender.Received.OfType<CreateDecisionPanelCommand>().Should().ContainSingle().Which;
        command.Scope.Should().Be(DecisionPanelScope.Speciality);
        command.InstitutionId.Should().Be(InstitutionB);
        command.SpecialityId.Should().Be(Paediatrics);
    }

    [Theory]
    [InlineData(WombatRoles.SpecialityAdmin)]
    [InlineData(WombatRoles.InstitutionalAdmin)]
    public void AnyoneElse_CreatesASpecialityPanel_AtTheirOwnInstitution(string role)
    {
        SignInAs(role);
        var cut = RenderComponent<PanelEdit>();
        cut.WaitForState(() => cut.FindAll("#panel-scope").Count == 1);

        cut.Find("#panel-scope").Change(DecisionPanelScope.Speciality.ToString());

        cut.FindAll("#panel-institution").Should().BeEmpty();
        cut.Markup.Should().Contain("The panel runs at your institution.");

        FillAndSubmit(cut, institutionId: null);

        _sender.Received.OfType<CreateDecisionPanelCommand>().Should().ContainSingle()
            .Which.InstitutionId.Should().BeNull("the handler pins the panel to the caller's institution");
    }

    [Fact]
    public void AnInstitutionWidePanel_StillAsksForTheInstitution()
    {
        SignInAs(WombatRoles.InstitutionalAdmin);
        var cut = RenderComponent<PanelEdit>();
        cut.WaitForState(() => cut.FindAll("#panel-scope").Count == 1);

        cut.Find("#panel-scope").Change(DecisionPanelScope.Institution.ToString());

        cut.FindAll("#panel-institution").Should().ContainSingle();
        cut.FindAll("#panel-speciality").Should().BeEmpty();
    }

    private void SignInAs(string role)
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("someone@test");
        auth.SetRoles(role);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "someone"));
    }

    private static void FillAndSubmit(IRenderedComponent<PanelEdit> cut, int? institutionId)
    {
        cut.Find("#panel-name").Change("Paediatrics annual review");
        if (institutionId is int id)
        {
            cut.Find("#panel-institution").Change(id.ToString());
        }

        cut.Find("#panel-speciality").Change(Paediatrics.ToString());
        cut.Find("#panel-chair").Change("zulu");
        cut.Find("form").Submit();
    }

    /// <summary>Answers what a test registers, records every request, and refuses anything unregistered.</summary>
    private sealed class RecordingSender : IScopedSender
    {
        private readonly Dictionary<Type, Func<object, object?>> _answers = [];

        public List<object> Received { get; } = [];

        public RecordingSender On<TRequest>(Func<TRequest, object?> answer)
        {
            _answers[typeof(TRequest)] = request => answer((TRequest)request);
            return this;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => Task.FromResult((TResponse)Answer(request)!);

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            Answer(request);
            return Task.CompletedTask;
        }

        private object? Answer(object request)
        {
            Received.Add(request);

            return _answers.TryGetValue(request.GetType(), out var answer)
                ? answer(request)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
        }
    }
}
