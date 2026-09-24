using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionsList;
using Wombat.Application.Features.Institutions.Queries.GetSpecialitiesList;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// The panel form offers the people the panel may seat, for the panel's institution, by the rule its save enforces; and a
/// stored member who can no longer sit is said to be taken off, not silently sent back to be refused. (T165)
/// </summary>
public sealed class PanelEditSeatTests : TestContext
{
    private const int InstitutionA = 1;
    private const int InstitutionB = 2;
    private const int PanelId = 20;

    private readonly TestAuthorizationContext _auth;
    private readonly RecordingSender _sender = new();

    public PanelEditSeatTests()
    {
        _auth = this.AddTestAuthorization();
        Services.AddSingleton<IScopedSender>(_sender);
        _sender
            .On<GetInstitutionsListQuery>(_ => new[]
            {
                new InstitutionDto(InstitutionA, "Kgosi Kgari", "KGK", null, true, DateTime.UtcNow),
                new InstitutionDto(InstitutionB, "Other Hospital", "OTH", null, true, DateTime.UtcNow)
            })
            .On<GetSpecialitiesListQuery>(_ => Array.Empty<SpecialityDto>())
            // An Administrator may say which College committee a panel sits as (T131 slice 3), so the form reads the list.
            .On<GetDecisionBodiesQuery>(_ => new[] { new DecisionBodyDto("neonatal", "Neonatal team Clinical Competency Committee") })
            .On<ListPanelMemberCandidatesQuery>(query => query.InstitutionId == InstitutionA || query.InstitutionId is null
                ? new[]
                {
                    new PanelMemberCandidateDto("zulu", "zulu@test", "Thandi", "Zulu", InstitutionA),
                    new PanelMemberCandidateDto("naidoo", "naidoo@test", "Priya", "Naidoo", InstitutionA)
                }
                : Array.Empty<PanelMemberCandidateDto>())
            .On<GetDecisionPanelByIdQuery>(_ => new DecisionPanelDetailDto(
                PanelId, "Paediatrics CCC", DecisionPanelScope.Institution, InstitutionA, null,
                [
                    new DecisionPanelMemberDto(1, "zulu", DecisionPanelMemberRole.Chair),
                    new DecisionPanelMemberDto(2, "naidoo", DecisionPanelMemberRole.Member),
                    new DecisionPanelMemberDto(3, "erased-7f3a", DecisionPanelMemberRole.Member)
                ]))
            .On<UpdateDecisionPanelCommand>(command => new DecisionPanelDetailDto(
                PanelId, "Paediatrics CCC", DecisionPanelScope.Institution, InstitutionA, null, []));
    }

    [Fact]
    public void AStoredMemberWhoCanNoLongerSit_IsSaidToBeTakenOff_AndIsNotSentBack()
    {
        SignInAs(WombatRoles.Administrator);

        var cut = RenderComponent<PanelEdit>(parameters => parameters.Add(page => page.PanelId, PanelId));
        cut.WaitForState(() => cut.FindAll("#panel-chair").Count == 1);

        _sender.Received.OfType<ListPanelMemberCandidatesQuery>().Should().ContainSingle()
            .Which.InstitutionId.Should().Be(InstitutionA, "the panel's own institution, not the Administrator's none");
        System.Text.RegularExpressions.Regex.Replace(cut.Find(".alert-warning").TextContent, @"\s+", " ").Trim()
            .Should().Be("1 member of this panel is no longer an active committee member at its institution, so is not " +
                         "listed below. Saving takes that member off the panel.");

        // The members' own form: an Administrator also sees the College committee form above it (T131 slice 3).
        cut.FindAll("form").Single(form => form.QuerySelector("#panel-chair") is not null).Submit();

        _sender.Received.OfType<UpdateDecisionPanelCommand>().Should().ContainSingle().Which.Members
            .Select(member => $"{member.UserId}:{member.Role}")
            .Should().Equal("zulu:Chair", "naidoo:Member");
    }

    [Fact]
    public void AnAdministratorCreatingAPanel_IsOfferedTheChosenInstitutionsMembers_AndNobodyBefore()
    {
        SignInAs(WombatRoles.Administrator);

        var cut = RenderComponent<PanelEdit>();
        cut.WaitForState(() => cut.FindAll("#panel-scope").Count == 1);
        cut.Find("#panel-scope").Change(DecisionPanelScope.Institution.ToString());
        cut.Find("#panel-institution").Change(InstitutionB.ToString());

        Options(cut).Should().BeEmpty("nobody at the other hospital may sit");
        cut.Markup.Should().NotContain("Choose the institution first");

        cut.Find("#panel-institution").Change(InstitutionA.ToString());

        _sender.Received.OfType<ListPanelMemberCandidatesQuery>().Select(query => query.InstitutionId)
            .Should().Equal(null, InstitutionB, InstitutionA);
        Options(cut).Should().Equal("zulu", "naidoo");
    }

    private static IReadOnlyList<string?> Options(IRenderedComponent<PanelEdit> cut)
        => cut.Find("#panel-chair").QuerySelectorAll("option")
            .Select(option => option.GetAttribute("value"))
            .Where(value => !string.IsNullOrEmpty(value))
            .ToArray();

    private void SignInAs(string role)
    {
        _auth.SetAuthorized("someone@test");
        _auth.SetRoles(role);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "someone"));
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
