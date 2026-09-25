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
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// The panel pages' half of T131 slice 3: an InstitutionalAdmin or an Administrator says which College committee a panel
/// sits as, anyone else sees it read-only, and the panel list says who decides each EPA. A Speciality or SubSpecialityAdmin
/// is offered only the scope they may create (T194 item 2).
/// </summary>
public sealed class PanelDecisionBodyTests : TestContext
{
    private const int InstitutionA = 1;
    private const int Paediatrics = 4;
    private const int PanelId = 7;
    private const string NeonatalName = "Neonatal team Clinical Competency Committee";

    private readonly RecordingSender _sender = new();

    public PanelDecisionBodyTests()
    {
        Services.AddSingleton<IScopedSender>(_sender);
        _sender
            .On<GetInstitutionsListQuery>(_ => new[]
            {
                new InstitutionDto(InstitutionA, "Kgosi Kgari", "KGK", null, true, DateTime.UtcNow)
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
            .On<GetDecisionBodiesQuery>(_ => new[] { new DecisionBodyDto("neonatal", NeonatalName) })
            .On<GetDecisionPanelByIdQuery>(_ => Panel(bodyKey: "neonatal"))
            .On<CreateDecisionPanelCommand>(command => new DecisionPanelDetailDto(
                5, command.Name, command.Scope, InstitutionA, command.SpecialityId, [], command.DecisionBodyKey,
                command.DecisionBodyKey is null ? null : NeonatalName))
            .On<SetDecisionPanelBodyCommand>(command => Panel(
                string.IsNullOrWhiteSpace(command.DecisionBodyKey) ? null : command.DecisionBodyKey));
    }

    // ─── Creating a panel ────────────────────────────────────────────────────

    [Theory]
    [InlineData(WombatRoles.SpecialityAdmin)]
    [InlineData(WombatRoles.SubSpecialityAdmin)]
    public void ASpecialityAdmin_IsOfferedOnlyTheSpecialityScope_AndNoCommittee(string role)
    {
        SignInAs(role);
        var cut = RenderComponent<PanelEdit>();
        cut.WaitForState(() => cut.FindAll("#panel-scope").Count == 1);

        cut.FindAll("#panel-scope option").Select(option => option.GetAttribute("value"))
            .Should().Equal(DecisionPanelScope.Speciality.ToString());
        cut.FindAll("#panel-body").Should().BeEmpty();
        _sender.Received.OfType<GetDecisionBodiesQuery>().Should().BeEmpty();
    }

    [Theory]
    [InlineData(WombatRoles.InstitutionalAdmin)]
    [InlineData(WombatRoles.Administrator)]
    public void AnInstitutionalAdminOrAdministrator_IsOfferedBothScopes_AndTheCommittee(string role)
    {
        SignInAs(role);
        var cut = RenderComponent<PanelEdit>();
        cut.WaitForState(() => cut.FindAll("#panel-body").Count == 1);

        cut.FindAll("#panel-scope option").Select(option => option.GetAttribute("value"))
            .Should().Equal(DecisionPanelScope.Institution.ToString(), DecisionPanelScope.Speciality.ToString());
        cut.Find("label[for=panel-body]").TextContent.Should().Contain("Decides for");
        cut.FindAll("#panel-body option").Select(option => option.TextContent.Trim())
            .Should().Equal("General panel", NeonatalName);
    }

    [Fact]
    public void AnInstitutionalAdmin_CreatesAPanelThatSitsAsTheNeonatalCommittee()
    {
        SignInAs(WombatRoles.InstitutionalAdmin);
        var cut = RenderComponent<PanelEdit>();
        cut.WaitForState(() => cut.FindAll("#panel-body").Count == 1);

        cut.Find("#panel-name").Change("Neonatal CCC");
        cut.Find("#panel-scope").Change(DecisionPanelScope.Institution.ToString());
        cut.Find("#panel-institution").Change(InstitutionA.ToString());
        cut.Find("#panel-body").Change("neonatal");
        cut.Find("#panel-chair").Change("zulu");
        cut.Find("form").Submit();

        _sender.Received.OfType<CreateDecisionPanelCommand>().Should().ContainSingle()
            .Which.DecisionBodyKey.Should().Be("neonatal");
    }

    // ─── Editing a panel ─────────────────────────────────────────────────────

    [Fact]
    public void AnInstitutionalAdmin_ChangesTheCommittee_InItsOwnCard()
    {
        SignInAs(WombatRoles.InstitutionalAdmin);
        var cut = RenderComponent<PanelEdit>(parameters => parameters.Add(page => page.PanelId, PanelId));
        cut.WaitForState(() => cut.FindAll("#panel-body").Count == 1);

        cut.Find("#panel-body-title").TextContent.Should().Be("Decides for");
        cut.Find("#panel-body").GetAttribute("value").Should().Be("neonatal");

        cut.Find("#panel-body").Change("");
        cut.FindAll("form").Single(form => form.QuerySelector("#panel-body") is not null).Submit();

        var command = _sender.Received.OfType<SetDecisionPanelBodyCommand>().Should().ContainSingle().Which;
        command.PanelId.Should().Be(PanelId);
        command.DecisionBodyKey.Should().BeNullOrEmpty();
        cut.WaitForState(() => cut.Markup.Contains("The panel is a general panel."));
        _sender.Received.OfType<UpdateDecisionPanelCommand>().Should().BeEmpty("the members are their own form");
    }

    // ─── T234: a save keeps the keyboard focus ───────────────────────────────

    /// <summary>
    /// T234. Save committee was disabled while it ran, and its form is rebuilt with what was stored, so the focus fell to
    /// the page body. A save that is done moves the focus to its result.
    /// </summary>
    [Fact]
    public void SavingTheCommittee_MovesTheFocusToItsResult()
    {
        SignInAs(WombatRoles.InstitutionalAdmin);
        var cut = RenderPanel();

        cut.Find("#panel-body").Change("");
        BodyForm(cut).Submit();

        cut.WaitForState(() => cut.Markup.Contains("The panel is a general panel."));
        cut.WaitForAssertion(() => FocusedReference().Id.Should().Be(cut.FindComponent<ActionResult>().Instance.Element.Id));
    }

    [Fact]
    public void SavingTheMembers_MovesTheFocusToItsResult()
    {
        SignInAs(WombatRoles.InstitutionalAdmin);
        _sender.On<UpdateDecisionPanelCommand>(_ => Panel("neonatal"));
        var cut = RenderPanel();

        MembersForm(cut).Submit();

        cut.WaitForState(() => cut.Markup.Contains("Panel members updated."));
        cut.WaitForAssertion(() => FocusedReference().Id.Should().Be(cut.FindComponent<ActionResult>().Instance.Element.Id));
    }

    [Fact]
    public void WhileTheCommitteeSaves_SaveCommitteeStaysEnabled_SavePanelIsDisabled_AndASecondPressSendsNothing()
    {
        SignInAs(WombatRoles.InstitutionalAdmin);
        _sender.Hold<SetDecisionPanelBodyCommand>();
        var cut = RenderPanel();

        BodyForm(cut).Submit();

        BodyForm(cut).QuerySelector("button[type=submit]")!.HasAttribute("disabled").Should().BeFalse(
            "it has the focus, and a browser drops the focus of a button it disables, to the page (T234)");
        BodyForm(cut).QuerySelector("button[type=submit]")!.GetAttribute("aria-disabled").Should().Be("true",
            "a second press does nothing (T234 review)");
        MembersForm(cut).QuerySelector("button[type=submit]")!.HasAttribute("disabled").Should().BeTrue();

        BodyForm(cut).Submit();
        MembersForm(cut).Submit();
        _sender.HeldSends.Should().Be(1, "a second press while the save runs sends nothing");
        _sender.Received.OfType<UpdateDecisionPanelCommand>().Should().BeEmpty();

        _sender.Release();

        cut.WaitForState(() => cut.Markup.Contains("The panel sits as the"));
        _sender.HeldSends.Should().Be(1);
    }

    [Fact]
    public void ARefusedCommittee_IsShownBesideItsButton_WhichKeepsTheFocus()
    {
        SignInAs(WombatRoles.InstitutionalAdmin);
        _sender.On<SetDecisionPanelBodyCommand>(_ => throw new InvalidOperationException("The committee cannot be changed now."));
        var cut = RenderPanel();

        BodyForm(cut).Submit();

        cut.WaitForAssertion(() => BodyForm(cut).QuerySelector(".alert-danger")!.TextContent.Trim()
            .Should().Be("The committee cannot be changed now."));
        JSInterop.Invocations.Should().NotContain(invocation => invocation.Identifier == "Blazor._internal.domWrapper.focus");
        BodyForm(cut).QuerySelector("button[type=submit]")!.HasAttribute("disabled").Should().BeFalse();
    }

    private IRenderedComponent<PanelEdit> RenderPanel()
    {
        var cut = RenderComponent<PanelEdit>(parameters => parameters.Add(page => page.PanelId, PanelId));
        cut.WaitForState(() => cut.FindAll("#panel-body").Count == 1 && cut.FindAll("#panel-chair").Count == 1);
        return cut;
    }

    private static AngleSharp.Dom.IElement BodyForm(IRenderedComponent<PanelEdit> cut)
        => cut.FindAll("form").Single(form => form.QuerySelector("#panel-body") is not null);

    private static AngleSharp.Dom.IElement MembersForm(IRenderedComponent<PanelEdit> cut)
        => cut.FindAll("form").Single(form => form.QuerySelector("#panel-chair") is not null);

    private Microsoft.AspNetCore.Components.ElementReference FocusedReference()
        => JSInterop.VerifyFocusAsyncInvoke().Arguments[0].Should().BeOfType<Microsoft.AspNetCore.Components.ElementReference>().Subject;

    [Fact]
    public void ASpecialityAdmin_SeesTheCommitteeReadOnly()
    {
        SignInAs(WombatRoles.SpecialityAdmin);
        var cut = RenderComponent<PanelEdit>(parameters => parameters.Add(page => page.PanelId, PanelId));
        cut.WaitForState(() => cut.FindAll("#panel-body-title").Count == 1);

        cut.FindAll("#panel-body").Should().BeEmpty();
        cut.Find("aside dl dd").TextContent.Should().Be(NeonatalName);
        cut.Markup.Should().Contain("Only an institutional administrator can change which College committee a panel sits as.");
    }

    // ─── Who decides each EPA ────────────────────────────────────────────────

    [Fact]
    public void ThePanelList_SaysWhoDecidesEachEpa_GroupingTheEpasThatGoTheSameWay()
    {
        SignInAs(WombatRoles.InstitutionalAdmin);
        _sender
            .On<ListDecisionPanelsQuery>(_ => new[]
            {
                new DecisionPanelSummaryDto(10, "Annual review", DecisionPanelScope.Institution, InstitutionA, null, 3),
                new DecisionPanelSummaryDto(11, "Neonatal CCC", DecisionPanelScope.Institution, InstitutionA, null, 3, "neonatal", NeonatalName)
            })
            .On<GetCommitteeRoutingQuery>(query => Routing(neonatalPanel: new CommitteeRoutedPanelDto(11, "Neonatal CCC")));

        var cut = RenderComponent<PanelsList>();
        cut.WaitForState(() => cut.FindAll("[aria-labelledby=routing-programme-100] tbody tr").Count == 2);

        _sender.Received.OfType<GetCommitteeRoutingQuery>().Should().ContainSingle()
            .Which.InstitutionId.Should().BeNull("anyone but an Administrator reads their own institution");

        var rows = cut.FindAll("[aria-labelledby=routing-programme-100] tbody tr").ToArray();
        rows[0].QuerySelector("th")!.TextContent.Should().Be("PAED-004, PAED-005");
        rows[0].QuerySelector("td")!.TextContent.Should().Contain("Neonatal CCC").And.Contain($"Sits as the {NeonatalName}.");
        rows[1].QuerySelector("th")!.TextContent.Should().Be("PAED-001, PAED-002");
        rows[1].QuerySelector("td")!.TextContent.Trim().Should().Be("Annual review");

        cut.FindAll("tbody tr").Select(row => row.TextContent)
            .Should().Contain(text => text.Contains("Neonatal CCC") && text.Contains(NeonatalName), "the list says what each panel decides for");
    }

    [Fact]
    public void ThePanelList_SaysWhenNoPanelSitsAsTheCommittee_AndTheGeneralPanelDecides()
    {
        SignInAs(WombatRoles.Coordinator);
        _sender
            .On<ListDecisionPanelsQuery>(_ => Array.Empty<DecisionPanelSummaryDto>())
            .On<GetCommitteeRoutingQuery>(_ => Routing(neonatalPanel: null));

        var cut = RenderComponent<PanelsList>();
        cut.WaitForState(() => cut.FindAll("[aria-labelledby=routing-programme-100] tbody tr").Count == 2);

        var neonatalRow = cut.FindAll("[aria-labelledby=routing-programme-100] tbody tr").First();
        neonatalRow.QuerySelector("th")!.TextContent.Should().Be("PAED-004, PAED-005");
        neonatalRow.QuerySelector("td")!.TextContent.Should().Contain("Annual review")
            .And.Contain($"No panel covering this programme sits as the {NeonatalName}, so the general panel decides.");
    }

    [Fact]
    public void TheFallback_NamesSeveralGeneralPanelsInThePlural()
    {
        SignInAs(WombatRoles.Coordinator);
        _sender
            .On<ListDecisionPanelsQuery>(_ => Array.Empty<DecisionPanelSummaryDto>())
            .On<GetCommitteeRoutingQuery>(_ => Routing(
                neonatalPanel: null,
                generals: [new CommitteeRoutedPanelDto(10, "Annual review"), new CommitteeRoutedPanelDto(12, "Paediatric CCC")]));

        var cut = RenderComponent<PanelsList>();
        cut.WaitForState(() => cut.FindAll("[aria-labelledby=routing-programme-100] tbody tr").Count == 2);

        cut.FindAll("[aria-labelledby=routing-programme-100] tbody tr").First().QuerySelector("td")!.TextContent
            .Should().Contain("Annual review, Paediatric CCC")
            .And.Contain($"No panel covering this programme sits as the {NeonatalName}, so the general panels decide.");
    }

    [Fact]
    public void WhenNoPanelCoversTheProgramme_TheRowSaysSo_AndClaimsNoGeneralPanelDecides()
    {
        // The review's copy finding: the row used to say both "No panel at this institution covers this programme." and
        // "routed to the general panel", which contradict each other.
        SignInAs(WombatRoles.Coordinator);
        _sender
            .On<ListDecisionPanelsQuery>(_ => Array.Empty<DecisionPanelSummaryDto>())
            .On<GetCommitteeRoutingQuery>(_ => Routing(neonatalPanel: null, generals: []));

        var cut = RenderComponent<PanelsList>();
        cut.WaitForState(() => cut.FindAll("[aria-labelledby=routing-programme-100] tbody tr").Count == 2);

        var neonatalCell = cut.FindAll("[aria-labelledby=routing-programme-100] tbody tr").First().QuerySelector("td")!.TextContent;
        neonatalCell.Should().Contain("No panel at this institution covers this programme.");
        neonatalCell.Should().NotContain("general panel");
    }

    [Fact]
    public void AnAdministrator_ChoosesTheInstitution_BeforeTheCardAsks()
    {
        SignInAs(WombatRoles.Administrator);
        _sender
            .On<ListDecisionPanelsQuery>(_ => Array.Empty<DecisionPanelSummaryDto>())
            .On<GetCommitteeRoutingQuery>(_ => Routing(neonatalPanel: null));

        var cut = RenderComponent<PanelsList>();
        cut.WaitForState(() => cut.FindAll("#routing-institution option").Count == 2);

        _sender.Received.OfType<GetCommitteeRoutingQuery>().Should().BeEmpty();
        cut.Markup.Should().Contain("Choose an institution to see which panel decides each EPA there.");

        cut.Find("#routing-institution").Change(InstitutionA.ToString());

        _sender.Received.OfType<GetCommitteeRoutingQuery>().Should().ContainSingle()
            .Which.InstitutionId.Should().Be(InstitutionA);
        cut.WaitForState(() => cut.FindAll("[aria-labelledby=routing-programme-100]").Count == 1);
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private static DecisionPanelDetailDto Panel(string? bodyKey)
        => new(
            PanelId,
            "Paediatric CCC",
            DecisionPanelScope.Speciality,
            InstitutionA,
            Paediatrics,
            [new DecisionPanelMemberDto(1, "zulu", DecisionPanelMemberRole.Chair)],
            bodyKey,
            bodyKey is null ? null : NeonatalName);

    /// <param name="neonatalPanel">The panel sitting as the neonatal CCC, or null when the general panels take EPAs 4–5.</param>
    /// <param name="generals">The general panels covering the programme; one, "Annual review", unless given.</param>
    private static CommitteeRoutingDto Routing(
        CommitteeRoutedPanelDto? neonatalPanel,
        IReadOnlyList<CommitteeRoutedPanelDto>? generals = null)
    {
        generals ??= [new CommitteeRoutedPanelDto(10, "Annual review")];
        IReadOnlyList<CommitteeRoutedPanelDto> neonatal = neonatalPanel is null ? generals : [neonatalPanel];
        var fallsBack = neonatalPanel is null;

        return new CommitteeRoutingDto(InstitutionA, "Kgosi Kgari",
        [
            new CommitteeRoutingProgrammeDto(100, "General Paediatrics", "11.1", "Paediatrics",
            [
                new CommitteeRoutingLineDto(1, "PAED-001", "Ward round", null, null, generals, false),
                new CommitteeRoutingLineDto(2, "PAED-002", "Clinic", null, null, generals, false),
                new CommitteeRoutingLineDto(4, "PAED-004", "Newborn", "neonatal", NeonatalName, neonatal, fallsBack),
                new CommitteeRoutingLineDto(5, "PAED-005", "Sick newborn", "neonatal", NeonatalName, neonatal, fallsBack)
            ])
        ]);
    }

    private void SignInAs(string role)
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("someone@test");
        auth.SetRoles(role);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "someone"));
    }

    /// <summary>Answers what a test registers, records every request, and refuses anything unregistered.</summary>
    private sealed class RecordingSender : IScopedSender
    {
        private readonly Dictionary<Type, Func<object, object?>> _answers = [];
        private Type? _heldType;
        private TaskCompletionSource? _held;

        public List<object> Received { get; } = [];

        /// <summary>How many requests of the held type the page sent; each is answered only once released.</summary>
        public int HeldSends { get; private set; }

        public RecordingSender On<TRequest>(Func<TRequest, object?> answer)
        {
            _answers[typeof(TRequest)] = request => answer((TRequest)request);
            return this;
        }

        /// <summary>Holds each request of this type until <see cref="Release" />: the window a second press falls into.</summary>
        public RecordingSender Hold<TRequest>()
        {
            _heldType = typeof(TRequest);
            return this;
        }

        public void Release() => (_held ?? throw new InvalidOperationException("Nothing was held.")).SetResult();

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request.GetType() == _heldType
                ? HeldAsync<TResponse>(request)
                : Task.FromResult((TResponse)Answer(request)!);

        private async Task<TResponse> HeldAsync<TResponse>(object request)
        {
            HeldSends++;
            _held = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            await _held.Task;
            return (TResponse)Answer(request)!;
        }

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
