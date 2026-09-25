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

    // T199 item 2: an institution-wide panel sent without its institution was refused in the validator's log format,
    // "Validation failed: -- : Institution-scoped panels require an institution. Severity: Error", with no property name
    // because the rule is on the whole command. The refusal here is the real validator's, thrown as the validation
    // pipeline throws it; the page shows its message alone (RefusalText, T213), for either scope.
    [Theory]
    [InlineData(DecisionPanelScope.Institution, "Institution-scoped panels require an institution.")]
    [InlineData(DecisionPanelScope.Speciality, "Speciality-scoped panels require a speciality.")]
    public void APanelSentWithoutWhatItsScopeNeeds_IsRefusedInTheValidatorsWords_NotItsLogFormat(
        DecisionPanelScope scope, string refusal)
    {
        SignInAs(WombatRoles.InstitutionalAdmin);
        _sender
            .On<ListPanelMemberCandidatesQuery>(_ => new[]
            {
                new PanelMemberCandidateDto("zulu", "zulu@test", "Thandi", "Zulu", InstitutionA),
                new PanelMemberCandidateDto("naidoo", "naidoo@test", "Priya", "Naidoo", InstitutionA)
            })
            .On<CreateDecisionPanelCommand>(command =>
            {
                new CreateDecisionPanelCommandValidator().ValidateAndThrow(command);
                return new DecisionPanelDetailDto(5, command.Name, command.Scope, InstitutionA, command.SpecialityId, []);
            });

        var cut = RenderComponent<PanelEdit>();
        cut.WaitForState(() => cut.FindAll("#panel-scope").Count == 1);
        cut.Find("#panel-scope").Change(scope.ToString());
        cut.Find("#panel-name").Change("Paediatrics annual review");
        cut.Find("#panel-chair").Change("zulu");
        cut.Find("#panel-members").Change(new[] { "naidoo" });
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => System.Text.RegularExpressions.Regex.Replace(cut.Find(".alert-danger").TextContent, @"\s+", " ")
            .Trim().Should().Be(refusal));
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
