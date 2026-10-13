using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionsList;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;
using static Wombat.Web.Tests.CommitteeDecisions.PanelSeatPicker;

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
            // What a new panel may be (T194, T245): both scopes for an InstitutionalAdmin or an Administrator, the Speciality
            // scope for anyone else, and Paediatrics, the one speciality adopted here; an Administrator's only once they have
            // chosen an institution. PanelEditFormOptionsTests runs the real rule.
            .On<GetDecisionPanelFormOptionsQuery>(query =>
                query.Principal.IsInRole(WombatRoles.Administrator) && query.InstitutionId is null
                    ? new DecisionPanelFormOptionsDto(MayCreateInstitutionWide: true, Specialities: null)
                    : new DecisionPanelFormOptionsDto(
                        MayCreateInstitutionWide: query.Principal.IsInRole(WombatRoles.Administrator) ||
                                                  query.Principal.IsInRole(WombatRoles.InstitutionalAdmin),
                        Specialities: [new SpecialityDto(Paediatrics, 1, "Paediatrics", null, true)]))
            .On<ListPanelMemberCandidatesQuery>(_ => new[]
            {
                new PanelMemberCandidateDto("zulu", "zulu@test", "Thandi", "Zulu", InstitutionA)
            })
            // An InstitutionalAdmin and an Administrator are offered the College committee a panel sits as (T131).
            .On<GetDecisionBodiesQuery>(_ => new[] { new DecisionBodyDto("neonatal", "Neonatal team Clinical Competency Committee") })
            .On<CreateDecisionPanelCommand>(command => new DecisionPanelDetailDto(
                5, command.Name, command.Scope, command.InstitutionId ?? InstitutionA, command.SpecialityId, []));
    }

    [Fact]
    public void AnAdministrator_NamesTheInstitution_OfASpecialityPanel()
    {
        SignInAs(WombatRoles.Administrator);
        var cut = RenderComponent<PanelEdit>();
        cut.WaitForState(() => cut.FindAll("#panel-scope").Count == 1);

        ChooseScope(cut, DecisionPanelScope.Speciality);

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

        ChooseScope(cut, DecisionPanelScope.Speciality);

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

        ChooseScope(cut, DecisionPanelScope.Institution);

        cut.FindAll("#panel-institution").Should().ContainSingle();
        cut.FindAll("#panel-speciality").Should().BeEmpty();
    }

    // T199 item 2, T324, round 3 S4: a panel without what its scope needs is refused in Wombat's words, beside the field
    // and in the summary, which takes the focus; never the validator's log format. The form says it before anything is
    // sent.
    [Fact]
    public void AnInstitutionWidePanelWithoutItsInstitution_IsRefusedBesideTheField_BeforeAnythingIsSent()
    {
        SignInAs(WombatRoles.InstitutionalAdmin);
        var cut = RenderComponent<PanelEdit>();
        cut.WaitForState(() => cut.FindAll("#panel-scope").Count == 1);
        ChooseScope(cut, DecisionPanelScope.Institution);
        cut.Find("#panel-name").Change("Paediatrics annual review");
        cut.Find("#panel-chair").Change("zulu");
        cut.Find("form").Submit();

        _sender.Received.OfType<CreateDecisionPanelCommand>().Should().BeEmpty();
        Squash(cut.Find("#panel-refusal").TextContent)
            .Should().StartWith("The panel was not created. Everything you chose is kept below.");
        cut.FindAll("#panel-refusal li").Select(line => Squash(line.TextContent))
            .Should().Equal("Institution: Choose an institution.");
        cut.Find("#panel-institution-msg").TextContent.Trim().Should().Be("Choose an institution.");
        cut.Find("#panel-institution").GetAttribute("aria-describedby").Should().Contain("panel-institution-msg");
        cut.Find("#panel-institution").GetAttribute("aria-invalid").Should().Be("true");
        cut.Find("#panel-refusal a").GetAttribute("href").Should().Be("#panel-institution");
    }

    [Fact]
    public void AFieldTheValidatorRefuses_IsSaidBesideIt_InItsWords_NotItsLogFormat()
    {
        SignInAs(WombatRoles.InstitutionalAdmin);
        _sender.On<CreateDecisionPanelCommand>(_ => throw new ValidationException(
            [new FluentValidation.Results.ValidationFailure("SpecialityId", CreateDecisionPanelCommandValidator.SpecialityMissing)]));
        var cut = RenderComponent<PanelEdit>();
        cut.WaitForState(() => cut.FindAll("#panel-scope").Count == 1);
        FillAndSubmit(cut, institutionId: null);

        cut.WaitForAssertion(() => cut.Find("#panel-speciality-msg").TextContent.Trim().Should().Be("Choose a speciality."));
        Squash(cut.Find("#panel-refusal li").TextContent).Should().Be("Speciality: Choose a speciality.");
        cut.Markup.Should().NotContain("Validation failed").And.NotContain("Severity");
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
