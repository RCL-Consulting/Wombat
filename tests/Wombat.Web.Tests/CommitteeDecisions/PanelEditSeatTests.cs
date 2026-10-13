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
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;
using static Wombat.Web.Tests.CommitteeDecisions.PanelSeatPicker;

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

    // The panel as stored: its erased member's name is the pseudonym the store holds (round 3 M6).
    private IReadOnlyList<DecisionPanelMemberDto> _stored =
    [
        new(1, "zulu", DecisionPanelMemberRole.Chair),
        new(2, "naidoo", DecisionPanelMemberRole.Member),
        new(3, "erased-7f3a", DecisionPanelMemberRole.Member) { Name = "Former user 7f3a" }
    ];

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
            // Only an Administrator signs in here, whose reach is every panel (T194): the specialities once an institution is
            // chosen, and neither institution here has adopted any (T245 review).
            .On<GetDecisionPanelFormOptionsQuery>(query => new DecisionPanelFormOptionsDto(
                MayCreateInstitutionWide: true, Specialities: query.InstitutionId is null ? null : []))
            // An Administrator may say which College committee a panel sits as (T131 slice 3), so the form reads the list.
            .On<GetDecisionBodiesQuery>(_ => new[] { new DecisionBodyDto("neonatal", "Neonatal team Clinical Competency Committee") })
            // An Administrator is offered nobody until an institution is chosen, as the handler does.
            .On<ListPanelMemberCandidatesQuery>(query => query.InstitutionId == InstitutionA
                ? new[]
                {
                    new PanelMemberCandidateDto("zulu", "zulu@test", "Thandi", "Zulu", InstitutionA),
                    new PanelMemberCandidateDto("naidoo", "naidoo@test", "Priya", "Naidoo", InstitutionA)
                }
                : Array.Empty<PanelMemberCandidateDto>())
            .On<GetDecisionPanelByIdQuery>(_ => new DecisionPanelDetailDto(
                PanelId, "Paediatrics CCC", DecisionPanelScope.Institution, InstitutionA, null, _stored))
            .On<UpdateDecisionPanelCommand>(command =>
            {
                _stored = command.Members.Select((member, index) => new DecisionPanelMemberDto(index + 10, member.UserId, member.Role)).ToArray();
                return new DecisionPanelDetailDto(PanelId, "Paediatrics CCC", DecisionPanelScope.Institution, InstitutionA, null, []);
            });
    }

    [Fact]
    public void AStoredMemberWhoCanNoLongerSit_IsNamed_IsInNoControl_AndIsNotSentBack()
    {
        SignInAs(WombatRoles.Administrator);

        var cut = RenderComponent<PanelEdit>(parameters => parameters.Add(page => page.PanelId, PanelId));
        cut.WaitForState(() => cut.FindAll("#panel-chair").Count == 1);

        _sender.Received.OfType<ListPanelMemberCandidatesQuery>().Should().ContainSingle()
            .Which.InstitutionId.Should().Be(InstitutionA, "the panel's own institution, not the Administrator's none");
        // Round 3 M6, C12: named as stored, and a live alert as it first shows.
        Squash(cut.Find(".alert-warning").TextContent)
            .Should().Be("Former user 7f3a can no longer sit on this panel, so is not listed below: only an active " +
                         "committee member at its institution who is not a trainee can. Saving takes Former user 7f3a off " +
                         "the panel.");
        cut.Find(".alert-warning").GetAttribute("role").Should().Be("alert");
        Options(cut).Should().Equal("zulu", "naidoo");
        Listed(cut).Should().Equal("Thandi Zulu", "Priya Naidoo");

        // The members' own form: an Administrator also sees the College committee form below it (T131 slice 3).
        cut.FindAll("form").Single(form => form.QuerySelector("#panel-chair") is not null).Submit();

        _sender.Received.OfType<UpdateDecisionPanelCommand>().Should().ContainSingle().Which.Members
            .Select(member => $"{member.UserId}:{member.Role}")
            .Should().Equal("zulu:Chair", "naidoo:Member");

        // C12: the warning is read again from the saved panel, and the result says who is off it, with the focus.
        cut.WaitForAssertion(() => Squash(cut.Find(".alert-success").TextContent).Should().Be(
            "Panel members updated. Former user 7f3a is off the panel. Thandi Zulu chairs, with 1 member and 0 external " +
            "members."));
        cut.FindAll(".alert-warning").Should().BeEmpty("the saved panel seats nobody who can no longer sit");
        _sender.Received.OfType<GetDecisionPanelByIdQuery>().Should().HaveCount(2, "the panel is read again after the save");
    }

    [Fact]
    public void AnAdministratorCreatingAPanel_IsOfferedTheChosenInstitutionsMembers_AndNobodyBefore()
    {
        SignInAs(WombatRoles.Administrator);

        var cut = RenderComponent<PanelEdit>();
        cut.WaitForState(() => cut.FindAll("#panel-scope").Count == 1);
        Squash(cut.Find("#panel-seats-none").TextContent)
            .Should().Be("Choose the institution first: a panel seats committee members from its own institution.");

        ChooseScope(cut, DecisionPanelScope.Institution);
        cut.Find("#panel-institution").Change(InstitutionB.ToString());

        Options(cut).Should().BeEmpty("nobody at the other hospital may sit");
        Squash(cut.Find("#panel-seats-none").TextContent)
            .Should().Be("No active committee member at Other Hospital who is not a trainee can sit yet.");

        cut.Find("#panel-institution").Change(InstitutionA.ToString());

        _sender.Received.OfType<ListPanelMemberCandidatesQuery>().Select(query => query.InstitutionId)
            .Should().Equal(null, InstitutionB, InstitutionA);
        Options(cut).Should().Equal("zulu", "naidoo");
        cut.Find("#panel-seats caption").TextContent.Trim().Should().Be(
            "Seats on the panel: the active committee members at Kgosi Kgari who are not trainees, by surname");
    }

    [Fact]
    public void AMemberListRefusedByItsValidator_IsSaidOnTheSeats_NotInTheValidatorsLogFormat()
    {
        // T213 item 5: the page used to print ValidationException.Message, "Validation failed: -- Members: ... Severity:
        // Error". Round 3 S6: a refused group is named by its message, and the summary's line focuses its first radio.
        SignInAs(WombatRoles.Administrator);
        _sender.On<UpdateDecisionPanelCommand>(_ => throw new FluentValidation.ValidationException(
            [new FluentValidation.Results.ValidationFailure("Members", DecisionPanelComposition.TooFewMembers)]));

        var cut = RenderComponent<PanelEdit>(parameters => parameters.Add(page => page.PanelId, PanelId));
        cut.WaitForState(() => cut.FindAll("#panel-chair").Count == 1);
        cut.FindAll("form").Single(form => form.QuerySelector("#panel-chair") is not null).Submit();

        cut.WaitForAssertion(() => Squash(cut.Find("#panel-refusal li").TextContent)
            .Should().Be("Seats: " + DecisionPanelComposition.TooFewMembers));
        Squash(cut.Find("#panel-refusal strong").TextContent).Should().Be("The panel's members were not saved.");
        var group = cut.Find("#panel-seats");
        group.ClassList.Should().Contain("is-invalid");
        group.GetAttribute("aria-describedby").Should().Be("panel-seats-msg");
        Squash(cut.Find("#panel-seats-msg").TextContent).Should().Be(DecisionPanelComposition.TooFewMembers);
        cut.Find("#panel-refusal a").GetAttribute("href").Should().Be("#seat-1-none", "the first radio: Zulu's row is the chair's");
        cut.Markup.Should().NotContain("Validation failed").And.NotContain("Severity");
    }

    [Fact]
    public void AFrameworksInvalidOperation_OnSave_IsTheFixedSentence_ButAHandlersRefusalIsItsOwnWords()
    {
        // T364, build review R4 (T338): the framework throws InvalidOperationException too, so only the handlers' own
        // refusal (CommitteeRefusedException) is printed; anything else is the page's fixed sentence, logged.
        SignInAs(WombatRoles.Administrator);
        var refusal = (Exception)new InvalidOperationException("Sequence contains more than one element");
        _sender.On<UpdateDecisionPanelCommand>(_ => throw refusal);

        var cut = RenderComponent<PanelEdit>(parameters => parameters.Add(page => page.PanelId, PanelId));
        cut.WaitForState(() => cut.FindAll("#panel-chair").Count == 1);
        AngleSharp.Dom.IElement MembersForm() => cut.FindAll("form").Single(form => form.QuerySelector("#panel-chair") is not null);
        MembersForm().Submit();

        cut.WaitForAssertion(() => Squash(cut.Find("#panel-refusal li").TextContent)
            .Should().Be(Wombat.Web.Components.Pages.CommitteeDecisions.Panels.PanelWords.SaveFailed));
        cut.Markup.Should().NotContain("Sequence contains");

        refusal = new CommitteeRefusedException(PanelSeat.NotEligible);
        MembersForm().Submit();

        cut.WaitForAssertion(() => Squash(cut.Find("#panel-refusal li").TextContent).Should().Be(PanelSeat.NotEligible));
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
