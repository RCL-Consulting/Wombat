using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionsList;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Components.Pages.CommitteeDecisions.Panels;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;
using static Wombat.Web.Tests.CommitteeDecisions.PanelSeatPicker;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// Decision panels and a panel's page as the boards draw them (T364, flow 08: R2-1, R2-2): the states each board names.
/// While the list loads it never says there is no routing (T329); a failed read is fixed words with Try again (T338); a
/// manager who may create no panel is told why (T260); the list names what each panel covers and who sits on it (round 3
/// M8); a panel's page is named by the panel, says it was created, and is flow 01's Page not found for an id out of reach
/// (Q6, M7); a new panel's missing fields are refused beside them in Wombat's words (T324, S4).
/// </summary>
public sealed class PanelPagesTests : TestContext
{
    private const int InstitutionA = 1;
    private const int PanelId = 20;
    private const string Kgk = "Kgosi Kgari Teaching Hospital";

    private readonly Sender _sender = new();
    private readonly TestAuthorizationContext _auth;

    public PanelPagesTests()
    {
        Services.AddSingleton<IScopedSender>(_sender);
        _auth = this.AddTestAuthorization();
        _sender
            .On<GetInstitutionsListQuery>(_ => new[] { new InstitutionDto(InstitutionA, Kgk, "KGK", null, true, DateTime.UtcNow) })
            .On<GetDecisionPanelFormOptionsQuery>(_ => new DecisionPanelFormOptionsDto(
                MayCreateInstitutionWide: true, Specialities: [new SpecialityDto(4, 1, "Paediatrics", null, true)])
            {
                MayManagePanels = true,
                InstitutionName = Kgk
            })
            .On<GetDecisionBodiesQuery>(_ => new[] { new DecisionBodyDto("neonatal", "Neonatal team Clinical Competency Committee") })
            .On<ListPanelMemberCandidatesQuery>(_ => new[]
            {
                new PanelMemberCandidateDto("botha", "botha@kgk.wombat.local", "Sarah", "Botha", InstitutionA),
                new PanelMemberCandidateDto("naidoo", "naidoo@kgk.wombat.local", "David", "Naidoo", InstitutionA),
                new PanelMemberCandidateDto("vanrensburg", "vanrensburg@sun.wombat.local", "John", "van Rensburg", InstitutionA),
                new PanelMemberCandidateDto("zulu", "zulu@kgk.wombat.local", "Thandi", "Zulu", InstitutionA)
            })
            .On<GetDecisionPanelByIdQuery>(_ => Panel())
            .On<ListDecisionPanelsQuery>(_ => new[] { Summary() })
            .On<GetCommitteeRoutingQuery>(_ => Routing());
        SignInAs(WombatRoles.InstitutionalAdmin);
    }

    // ─── Decision panels ─────────────────────────────────────────────────────

    [Fact]
    public void WhileTheListLoads_TheHeaderAndSkeletonsShow_AndNothingSaysThereIsNoRouting()
    {
        // C11, T329: "There is no committee routing to show" used to show while the list was read.
        var list = _sender.Hold<ListDecisionPanelsQuery>();
        var routing = _sender.Hold<GetCommitteeRoutingQuery>();

        var cut = RenderComponent<PanelsList>();

        cut.Find("h1").TextContent.Trim().Should().Be("Decision panels");
        cut.Find("p[role=status]").TextContent.Trim().Should().Be("Loading Decision panels.");
        cut.Find("#routing-title").TextContent.Trim().Should().Be("Who decides each EPA");
        cut.FindAll(".skeleton").Should().NotBeEmpty();
        cut.Markup.Should().NotContain("There is no committee routing").And.NotContain("No panel at this institution");
        cut.FindAll("a.btn").Select(link => link.TextContent.Trim()).Should().NotContain("New panel",
            "New panel waits for the read (C11)");

        list.SetResult(new[] { Summary() });
        routing.SetResult(Routing());

        cut.WaitForState(() => cut.FindAll("#routing-table-100").Count == 1);
        cut.Find("p[role=status]").TextContent.Trim().Should().BeEmpty();
        cut.FindAll("a.btn").Select(link => link.TextContent.Trim()).Should().Contain("New panel");
    }

    [Fact]
    public void TheList_NamesWhatEachPanelCovers_AndWhoSitsOnIt_Captioned_AndStackingAt900()
    {
        var cut = RenderComponent<PanelsList>();
        cut.WaitForState(() => cut.FindAll("#routing-table-100").Count == 1);

        cut.Find(".page-subtitle").TextContent.Trim()
            .Should().Be($"The panels that take entrustment and progression decisions at {Kgk}.");
        var table = cut.Find("table");
        table.ClassList.Should().Contain(["clinic-table--stack", "clinic-table--stack-wide"]);
        table.QuerySelector("caption")!.TextContent.Trim().Should().Be($"Decision panels at {Kgk}");
        table.QuerySelector("tbody tr")!.QuerySelectorAll("th, td").Take(4).Select(cell => Squash(cell.TextContent)).Should().Equal(
            "Paed Annual Review Panel", "Speciality · Paediatrics", "General panel",
            "4: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external)");
        cut.FindAll("#routing-table-100").Single().ClassList.Should().Contain("clinic-table--stack-wide");
        cut.FindAll("h2").Select(heading => heading.TextContent.Trim()).Should().Equal("Who decides each EPA");
        cut.Find("h3").TextContent.Trim().Should().Be("Paediatric EPA Curriculum 11.1");
    }

    [Fact]
    public void AnAdministratorWhoSwitchesInstitutionQuickly_SeesOnlyTheLastInstitutionsPanels()
    {
        // T364, build review R1: A's read answering after B's must not put A's panels under B.
        SignInAs(WombatRoles.Administrator);
        var slowA = new TaskCompletionSource<object?>();
        _sender.OnTask<ListDecisionPanelsQuery>(query => query.InstitutionId == InstitutionA
            ? slowA.Task
            : Task.FromResult<object?>(new[] { Summary() with { Id = 30, Name = "Other CCC", InstitutionId = 2 } }));
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo($"/committee/panels?institution={InstitutionA}");
        var cut = RenderComponent<PanelsList>();

        navigation.NavigateTo("/committee/panels?institution=2");
        cut.WaitForState(() => cut.Markup.Contains("Other CCC"));
        slowA.SetResult(new[] { Summary() });

        // The panels table (Who decides each EPA names its own panels): B's one panel, never A's beside or under it.
        cut.WaitForAssertion(() => cut.Find("table").QuerySelectorAll("tbody tr")
            .Select(row => Squash(row.QuerySelector("th, td")!.TextContent))
            .Should().Equal(["Other CCC"], "A's answer came after B was asked for"));
    }

    [Fact]
    public void AFailedRead_IsFixedWords_WithTryAgain_AndTheFailureIsNotPrinted()
    {
        var fails = true;
        _sender.On<ListDecisionPanelsQuery>(_ => fails
            ? throw new InvalidOperationException("Npgsql: connection refused")
            : new[] { Summary() });

        var cut = RenderComponent<PanelsList>();
        cut.WaitForState(() => cut.FindAll(".alert-danger").Count == 1);

        Squash(cut.Find(".alert-danger").TextContent).Should().Be(
            "Could not load Decision panels. Nothing has changed. Try again, or come back in a few minutes. Try again");
        cut.Markup.Should().NotContain("Npgsql");
        cut.FindAll("#routing-title").Should().BeEmpty("the page's alert says it; Who decides each EPA is not drawn");

        fails = false;
        cut.Find(".alert-danger button").Click();

        cut.WaitForState(() => cut.Markup.Contains("Paed Annual Review Panel"));
        cut.FindAll(".alert-danger").Should().BeEmpty();
    }

    [Fact]
    public void AManagerWhoMayCreateNoPanel_IsToldWhy_AndOfferedNoNewPanel()
    {
        // T260: a speciality admin whose institution has adopted no curriculum in the speciality.
        SignInAs(WombatRoles.SpecialityAdmin);
        _sender.On<GetDecisionPanelFormOptionsQuery>(_ => new DecisionPanelFormOptionsDto(false, []) { MayManagePanels = true });

        var cut = RenderComponent<PanelsList>();
        cut.WaitForState(() => cut.FindAll("#panels-none-creatable").Count == 1);

        Squash(cut.Find("#panels-none-creatable").TextContent).Should().Be(PanelWords.NoneCreatable);
        cut.FindAll("a.btn").Select(link => link.TextContent.Trim()).Should().NotContain("New panel");

        // D6 (T364 build review): the reason names roles, never a pronoun for them.
        PanelWords.NoneCreatable.Should().Be(
            "You cannot create a decision panel. A speciality or sub-speciality administrator creates the panels of the " +
            "speciality administered, at the administrator's own institution, once that institution has adopted a " +
            "curriculum in the speciality. An institutional administrator creates the institution's panels.");
        PanelWords.NoneCreatable.Split(' ', '.', ',').Should().NotContain(["their", "they", "them", "his", "her"]);
    }

    [Theory]
    [InlineData(WombatRoles.Coordinator)]
    [InlineData(WombatRoles.CommitteeMember)]
    public void SomeoneWhoManagesNoPanel_IsNotToldWhyNewPanelIsMissing(string role)
    {
        SignInAs(role);
        _sender.On<GetDecisionPanelFormOptionsQuery>(_ => new DecisionPanelFormOptionsDto(false, []));

        var cut = RenderComponent<PanelsList>();
        cut.WaitForState(() => cut.Markup.Contains("Paed Annual Review Panel"));

        cut.FindAll("#panels-none-creatable").Should().BeEmpty();
    }

    // ─── A panel's page ──────────────────────────────────────────────────────

    [Fact]
    public void APanelsPage_IsNamedByThePanel_SaysWhatItCovers_AndHasNoBackToPanels()
    {
        var cut = RenderPanel();

        cut.Find("h1").TextContent.Trim().Should().Be("Paed Annual Review Panel");
        cut.Find(".page-subtitle").TextContent.Trim().Should().Be($"Speciality panel · Paediatrics · {Kgk}");
        cut.FindAll("h2").Select(heading => heading.TextContent.Trim())
            .Should().Equal("About this panel", "Who sits on it", "Decides for");
        cut.FindAll(".details-list > div").Select(row => Squash(row.TextContent)).Should().Equal(
            "Scope Speciality: Paediatrics", $"Institution {Kgk}", "Open reviews None");
        cut.Markup.Should().Contain(PanelWords.PanelRules).And.NotContain("Back to panels");
        Seats(cut).Should().Equal(
            "Sarah Botha:member", "David Naidoo:member", "John van Rensburg:external", "Thandi Zulu:chair");
        Squash(cut.Find("#panel-seat-count").TextContent).Should().Be("4 seats: the chair, 2 members and 1 external member.");
        cut.Find("#panel-seats caption").TextContent.Trim().Should().Be(
            $"Seats on the panel: the active committee members at {Kgk} who are not trainees, by surname");
        cut.FindAll("#panel-seats .person-cell span").Select(span => span.TextContent)
            .Should().Contain("vanrensburg@sun.wombat.local", "the whole address (S3)");
    }

    [Fact]
    public void ANewlyCreatedPanelsPage_SaysItWasCreated_WithTheFocus()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo($"/committee/panels/{PanelId}?created=1");

        var cut = RenderPanel();

        cut.WaitForAssertion(() => Squash(cut.Find(".alert-success").TextContent).Should().Be(
            "Paed Annual Review Panel was created. Its reviews can now be scheduled on Committee reviews."));
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.FindComponent<ActionResult>().Instance.Element.Id));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("yes")]
    [InlineData("true")]
    public void AnyOtherCreatedValue_SaysNothingWasCreated(string value)
    {
        // T364, build review R7: only Create panel's own landing (?created=1) announces it; an old panel's address with
        // another value is just the panel's page.
        Services.GetRequiredService<NavigationManager>().NavigateTo($"/committee/panels/{PanelId}?created={value}");

        var cut = RenderPanel();

        cut.WaitForState(() => cut.FindAll("h1").Count == 1 && cut.Markup.Contains("Paed Annual Review Panel"));
        cut.FindAll(".alert-success").Should().BeEmpty();
        cut.Markup.Should().NotContain("was created");
    }

    [Fact]
    public void APanelOutOfReach_IsFlow01sPageNotFound_WordForWord()
    {
        _sender.On<GetDecisionPanelByIdQuery>(_ => null);

        var cut = RenderComponent<PanelEdit>(parameters => parameters.Add(page => page.PanelId, 999));
        cut.WaitForState(() => cut.FindAll(".system-panel").Count == 1);

        cut.Find("h1").TextContent.Trim().Should().Be("Page not found");
        cut.FindAll(".system-panel p").Select(paragraph => paragraph.TextContent.Trim())
            .Should().Equal("There is no page at this address.", "Check the address, or start again from Home.");
        cut.FindAll("form").Should().BeEmpty();
        cut.Markup.Should().NotContain("institution", "nothing is said about records elsewhere (M7)");
    }

    [Fact]
    public void APanelsFailedRead_IsFixedWords_UnderTheLoadingHeading()
    {
        _sender.On<GetDecisionPanelByIdQuery>(_ => throw new TimeoutException("Npgsql timeout"));

        var cut = RenderComponent<PanelEdit>(parameters => parameters.Add(page => page.PanelId, PanelId));
        cut.WaitForState(() => cut.FindAll(".alert-danger").Count == 1);

        cut.Find("h1").TextContent.Trim().Should().Be("Decision panel");
        Squash(cut.Find(".alert-danger").TextContent).Should().Be(
            "Could not load this panel. Nothing has changed. Try again, or come back in a few minutes. Try again");
        cut.Markup.Should().NotContain("Npgsql");
    }

    [Fact]
    public void ASubSpecialityAdminsNewPanel_SaysItsOneScopeAsText_AndOffersNoCommittee()
    {
        SignInAs(WombatRoles.SubSpecialityAdmin);
        _sender.On<GetDecisionPanelFormOptionsQuery>(_ => new DecisionPanelFormOptionsDto(
            MayCreateInstitutionWide: false, Specialities: [new SpecialityDto(4, 1, "Paediatrics", null, true)])
        {
            MayManagePanels = true,
            InstitutionName = Kgk
        });

        var cut = RenderComponent<PanelEdit>();
        cut.WaitForState(() => cut.FindAll("#panel-scope").Count == 1);

        cut.Find("h1").TextContent.Trim().Should().Be("New panel");
        Squash(cut.Find("dl.form-group").TextContent).Should().Be(
            "Scope Speciality A sub-speciality administrator creates Speciality-scoped panels for the speciality of the " +
            "sub-speciality.");
        cut.FindAll("#panel-body").Should().BeEmpty();
        cut.FindAll(".form-actions a, .form-actions button").Select(control => control.TextContent.Trim())
            .Should().Equal("Cancel", "Create panel");
        cut.Find(".form-actions a").GetAttribute("href").Should().Be("/committee/panels");
    }

    [Fact]
    public void ANewPanelMissingItsNameAndChair_IsRefusedBesideEach_InWombatsWords_WithTheFocusOnTheSummary()
    {
        // T324: "Give the panel a name." and "Choose a chair.", never DataAnnotations' "The Name field is required.".
        var cut = RenderComponent<PanelEdit>();
        cut.WaitForState(() => cut.FindAll("#panel-scope").Count == 1);
        Choose(cut, "David Naidoo", "member");

        cut.Find("form").Submit();

        _sender.Received.OfType<CreateDecisionPanelCommand>().Should().BeEmpty();
        cut.FindAll("#panel-refusal li").Select(line => Squash(line.TextContent))
            .Should().Equal("Panel name: Give the panel a name.", "Chair: Choose a chair.");
        cut.Find("#panel-name-msg").TextContent.Trim().Should().Be("Give the panel a name.");
        cut.Find("#panel-chair-msg").TextContent.Trim().Should().Be("Choose a chair.");
        cut.Find("#panel-name").GetAttribute("aria-invalid").Should().Be("true");
        cut.Find("#panel-chair").GetAttribute("aria-describedby").Should().Contain("panel-chair-msg");
        Squash(cut.Find("#panel-seat-count").TextContent).Should().Be("1 seat: no chair, 1 member and 0 external members.");
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().NotBeNull());
        cut.Markup.Should().NotContain("field is required");
    }

    [Fact]
    public void TheNewPanelForm_OffersEveryCommitteeMember_ByNameWithTheirWholeAddress()
    {
        var cut = RenderComponent<PanelEdit>();
        cut.WaitForState(() => cut.FindAll("#panel-scope").Count == 1);

        cut.FindAll("#panel-chair option").Select(option => option.TextContent.Trim()).Should().Equal(
            "Choose a chair", "Sarah Botha (botha@kgk.wombat.local)", "David Naidoo (naidoo@kgk.wombat.local)",
            "John van Rensburg (vanrensburg@sun.wombat.local)", "Thandi Zulu (zulu@kgk.wombat.local)");
        Seats(cut).Should().OnlyContain(seat => seat.EndsWith(":none", StringComparison.Ordinal));
        cut.FindAll("#panel-seats legend").Select(legend => legend.TextContent.Trim()).Should().Equal(
            "Seat for Sarah Botha", "Seat for David Naidoo", "Seat for John van Rensburg", "Seat for Thandi Zulu");
        Squash(cut.Find("#panel-seat-count").TextContent).Should().Be("0 seats: no chair, 0 members and 0 external members.");
    }

    // ─── The words ───────────────────────────────────────────────────────────

    [Fact]
    public void TheWords_CountInTheSingular_JoinNames_AndRangeEpaCodes()
    {
        PanelWords.SeatCount(true, 0, 0).Should().Be("1 seat: the chair, 0 members and 0 external members.");
        PanelWords.SeatCount(true, 1, 1).Should().Be("3 seats: the chair, 1 member and 1 external member.");
        PanelWords.MembersUpdated(["Sarah Botha"], "Thandi Zulu", 1, 1).Should().Be(
            "Panel members updated. Sarah Botha is off the panel. Thandi Zulu chairs, with 1 member and 1 external member.");
        PanelWords.MembersUpdated([], "Thandi Zulu", 2, 1)
            .Should().Be("Panel members updated. Thandi Zulu chairs, with 2 members and 1 external member.");
        PanelWords.CannotSit(["Sarah Botha", "David Naidoo"]).Should().Be(
            "Sarah Botha and David Naidoo can no longer sit on this panel, so are not listed below: only an active " +
            "committee member at its institution who is not a trainee can. Saving takes Sarah Botha and David Naidoo off " +
            "the panel.");
        PanelWords.Ranges(Codes(1, 2, 3, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15)).Should().Be("PAED-001 to PAED-003, PAED-006 to PAED-015");
        PanelWords.Ranges(Codes(5, 4)).Should().Be("PAED-004, PAED-005");
        PanelWords.Ranges(Codes(1, 3)).Should().Be("PAED-001, PAED-003");
        PanelWords.Ranges(["LOCAL-A", "PAED-002"]).Should().Be("LOCAL-A, PAED-002");
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private static string[] Codes(params int[] numbers) => numbers.Select(number => $"PAED-{number:000}").ToArray();

    private IRenderedComponent<PanelEdit> RenderPanel()
    {
        var cut = RenderComponent<PanelEdit>(parameters => parameters.Add(page => page.PanelId, PanelId));
        cut.WaitForState(() => cut.FindAll("#panel-chair").Count == 1);
        return cut;
    }

    private static DecisionPanelDetailDto Panel()
        => new(
            PanelId,
            "Paed Annual Review Panel",
            DecisionPanelScope.Speciality,
            InstitutionA,
            4,
            [
                new DecisionPanelMemberDto(1, "zulu", DecisionPanelMemberRole.Chair) { Name = "Thandi Zulu" },
                new DecisionPanelMemberDto(2, "naidoo", DecisionPanelMemberRole.Member) { Name = "David Naidoo" },
                new DecisionPanelMemberDto(3, "botha", DecisionPanelMemberRole.Member) { Name = "Sarah Botha" },
                new DecisionPanelMemberDto(4, "vanrensburg", DecisionPanelMemberRole.External) { Name = "John van Rensburg" }
            ])
        {
            InstitutionName = Kgk,
            SpecialityName = "Paediatrics"
        };

    private static DecisionPanelSummaryDto Summary()
        => new(PanelId, "Paed Annual Review Panel", DecisionPanelScope.Speciality, InstitutionA, 4, 4)
        {
            CallerMayManage = true,
            InstitutionName = Kgk,
            SpecialityName = "Paediatrics",
            MemberNames =
            [
                new("Thandi Zulu", DecisionPanelMemberRole.Chair),
                new("David Naidoo", DecisionPanelMemberRole.Member),
                new("Sarah Botha", DecisionPanelMemberRole.Member),
                new("John van Rensburg", DecisionPanelMemberRole.External)
            ]
        };

    private static CommitteeRoutingDto Routing()
    {
        IReadOnlyList<CommitteeRoutedPanelDto> general = [new CommitteeRoutedPanelDto(PanelId, "Paed Annual Review Panel")];
        return new CommitteeRoutingDto(InstitutionA, Kgk,
        [
            new CommitteeRoutingProgrammeDto(100, "Paediatric EPA Curriculum", "11.1", "Paediatrics",
                Enumerable.Range(1, 15).Select(number => number is 4 or 5
                        ? new CommitteeRoutingLineDto(number, $"PAED-{number:000}", "EPA", "neonatal",
                            "Neonatal team Clinical Competency Committee", general, true)
                        : new CommitteeRoutingLineDto(number, $"PAED-{number:000}", "EPA", null, null, general, false))
                    .ToArray())
        ]);
    }

    private void SignInAs(string role)
    {
        _auth.SetAuthorized("someone@test");
        _auth.SetRoles(role);
        _auth.SetClaims(
            new Claim(ClaimTypes.NameIdentifier, "someone"),
            new Claim(WombatClaimTypes.InstitutionId, InstitutionA.ToString()));
    }

    /// <summary>Answers what a test registers, or holds a request until the test answers it; records every request.</summary>
    private sealed class Sender : IScopedSender
    {
        private readonly Dictionary<Type, Func<object, Task<object?>>> _answers = [];

        public List<object> Received { get; } = [];

        public Sender On<TRequest>(Func<TRequest, object?> answer)
        {
            _answers[typeof(TRequest)] = request => Task.FromResult(answer((TRequest)request));
            return this;
        }

        /// <summary>Answers each request of this type with the task the test gives for it.</summary>
        public Sender OnTask<TRequest>(Func<TRequest, Task<object?>> answer)
        {
            _answers[typeof(TRequest)] = request => answer((TRequest)request);
            return this;
        }

        /// <summary>Holds every request of this type until the returned source is answered.</summary>
        public TaskCompletionSource<object?> Hold<TRequest>()
        {
            var held = new TaskCompletionSource<object?>();
            _answers[typeof(TRequest)] = _ => held.Task;
            return held;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Received.Add(request);
            if (!_answers.TryGetValue(request.GetType(), out var answer))
            {
                throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }

            return (TResponse)(await answer(request))!;
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
    }
}
