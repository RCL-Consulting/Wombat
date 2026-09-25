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
            .On<GetSpecialitiesListQuery>(_ => new[] { new SpecialityDto(Paediatrics, 1, "Paediatrics", null, true) })
            // What a new panel may be (T194): every scope and speciality for an InstitutionalAdmin or an Administrator, the
            // caller's own speciality for anyone else. PanelEditFormOptionsTests runs the real rule.
            .On<GetDecisionPanelFormOptionsQuery>(query =>
                query.Principal.IsInRole(WombatRoles.Administrator) || query.Principal.IsInRole(WombatRoles.InstitutionalAdmin)
                    ? new DecisionPanelFormOptionsDto(MayCreateInstitutionWide: true, Specialities: null)
                    : new DecisionPanelFormOptionsDto(
                        MayCreateInstitutionWide: false, Specialities: [new SpecialityDto(Paediatrics, 1, "Paediatrics", null, true)]))
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
