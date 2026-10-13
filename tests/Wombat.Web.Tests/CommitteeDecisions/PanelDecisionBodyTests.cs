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
using static Wombat.Web.Tests.CommitteeDecisions.PanelSeatPicker;

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

        ScopesOffered(cut)
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

        ScopesOffered(cut)
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
        ChooseScope(cut, DecisionPanelScope.Institution);
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
    public void WhileTheCommitteeSaves_BothSavesAreAriaDisabled_NeverNativelyDisabled_AndASecondPressSendsNothing()
    {
        SignInAs(WombatRoles.InstitutionalAdmin);
        _sender.Hold<SetDecisionPanelBodyCommand>();
        var cut = RenderPanel();

        BodyForm(cut).Submit();

        BodyForm(cut).QuerySelector("button[type=submit]")!.HasAttribute("disabled").Should().BeFalse(
            "it has the focus, and a browser drops the focus of a button it disables, to the page (T234)");
        BodyForm(cut).QuerySelector("button[type=submit]")!.GetAttribute("aria-disabled").Should().Be("true",
            "a second press does nothing (T234 review)");
        // D3 (T364 build review): Save panel is unavailable while the other form saves, said by aria-disabled (DESIGN.md),
        // never the native attribute.
        MembersForm(cut).QuerySelector("button[type=submit]")!.HasAttribute("disabled").Should().BeFalse();
        MembersForm(cut).QuerySelector("button[type=submit]")!.GetAttribute("aria-disabled").Should().Be("true");

        BodyForm(cut).Submit();
        MembersForm(cut).Submit();
        _sender.HeldSends.Should().Be(1, "a second press while the save runs sends nothing");
        _sender.Received.OfType<UpdateDecisionPanelCommand>().Should().BeEmpty();

        _sender.Release();

        cut.WaitForState(() => cut.Markup.Contains("The panel sits as the"));
        _sender.HeldSends.Should().Be(1);
    }

    [Fact]
    public void AFrameworksInvalidOperation_OnTheCommittee_IsTheFixedSentence_NeverItsMessage()
    {
        // T364, build review R4 (T338): only the handler's own refusal is said in its words.
        SignInAs(WombatRoles.InstitutionalAdmin);
        _sender.On<SetDecisionPanelBodyCommand>(_ => throw new InvalidOperationException("Sequence contains no elements"));
        var cut = RenderPanel();

        cut.Find("#panel-body").Change("");
        BodyForm(cut).Submit();

        cut.WaitForAssertion(() => Squash(cut.Find("#panel-body-msg").TextContent)
            .Should().Be($"Not saved: {Wombat.Web.Components.Pages.CommitteeDecisions.Panels.PanelWords.SaveFailed}"));
        cut.Markup.Should().NotContain("Sequence contains");
    }

    [Fact]
    public void WhileTheMembersSave_SaveCommitteeIsAriaDisabled_NeverNativelyDisabled()
    {
        // D3 (T364 build review).
        SignInAs(WombatRoles.InstitutionalAdmin);
        _sender.On<UpdateDecisionPanelCommand>(_ => Panel("neonatal")).Hold<UpdateDecisionPanelCommand>();
        var cut = RenderPanel();

        MembersForm(cut).Submit();

        var saveCommittee = BodyForm(cut).QuerySelector("button[type=submit]")!;
        saveCommittee.HasAttribute("disabled").Should().BeFalse();
        saveCommittee.GetAttribute("aria-disabled").Should().Be("true");
        BodyForm(cut).Submit();
        _sender.HeldSends.Should().Be(1, "Save committee sends nothing while the members save");
        _sender.Received.OfType<SetDecisionPanelBodyCommand>().Should().BeEmpty();

        _sender.Release();
    }

    [Fact]
    public void ARefusedCommittee_IsSaidInTheSectionsSummary_AndBesideTheSelect_WhichGoesBackToWhatIsSaved()
    {
        // C7, C13: the refusal in a summary that takes the focus, and beside its field, in the handler's own words.
        SignInAs(WombatRoles.InstitutionalAdmin);
        _sender.On<SetDecisionPanelBodyCommand>(_ => throw new CommitteeRefusedException("The committee cannot be changed now."));
        var cut = RenderPanel();

        cut.Find("#panel-body").Change("");
        BodyForm(cut).Submit();

        cut.WaitForAssertion(() => Squash(cut.Find("#panel-body-refusal strong").TextContent)
            .Should().Be($"Not saved. The panel still sits as the {NeonatalName}."));
        Squash(cut.Find("#panel-body-refusal li").TextContent).Should().Be("Decides for: The committee cannot be changed now.");
        Squash(cut.Find("#panel-body-msg").TextContent).Should().Be("Not saved: The committee cannot be changed now.");
        cut.Find("#panel-body").GetAttribute("value").Should().Be("neonatal", "the select shows what is saved");
        cut.Find("#panel-body").GetAttribute("aria-describedby").Should().Contain("panel-body-msg");
        cut.WaitForAssertion(() => FocusedReference().Id.Should().NotBe(cut.FindComponent<ActionResult>().Instance.Element.Id));
        BodyForm(cut).QuerySelector("button[type=submit]")!.HasAttribute("disabled").Should().BeFalse();
    }

    /// <summary>
    /// C13, F5: a scratch state, a second tab. A review was scheduled before the panel since the page was read, so Save
    /// committee is refused: the section's summary names the review in a short clause, the words beside the select say
    /// which review and that the select shows what is saved, and the select is back on General panel.
    /// </summary>
    [Fact]
    public void ACommitteeChangeRefusedByAReviewScheduledMeanwhile_NamesTheReview_AndTheSelectShowsWhatIsSaved()
    {
        SignInAs(WombatRoles.InstitutionalAdmin);
        _sender
            .On<GetDecisionPanelByIdQuery>(_ => Panel(bodyKey: null))
            .On<SetDecisionPanelBodyCommand>(_ => throw new DecisionBodyLockedException(
                1, new Wombat.Domain.Curricula.AcademicPeriod(2026, 2), CommitteeReviewState.Scheduled));
        var cut = RenderPanel();

        cut.Find("#panel-body").Change("neonatal");
        BodyForm(cut).Submit();

        cut.WaitForAssertion(() => Squash(cut.Find("#panel-body-refusal strong").TextContent)
            .Should().Be("Not saved. The panel is still a general panel."));
        Squash(cut.Find("#panel-body-refusal a").TextContent).Should().Be("Decides for: review #1 now sits before this panel");
        Squash(cut.Find("#panel-body-refusal li").TextContent).Should().Be(
            "Decides for: review #1 now sits before this panel. What a review decides, and its agenda, follow the College " +
            "committee the panel sat as when it was scheduled, so the panel keeps what it sits as until its open reviews " +
            "are ratified, or closed if formative.");
        Squash(cut.Find("#panel-body-msg").TextContent).Should().Be(
            "Not saved: review #1 (2026 S2, scheduled) now sits before this panel. The select shows what is saved.");
        cut.Find("#panel-body").GetAttribute("value").Should().BeEmpty("General panel, as saved");
        cut.WaitForAssertion(() => FocusedReference().Id.Should().NotBe(cut.FindComponent<ActionResult>().Instance.Element.Id,
            "the section's summary takes the focus"));
    }

    /// <summary>F5, step 4.13: while a review is open before the panel, the lock is said first and no select is offered.</summary>
    [Fact]
    public void WhileAReviewIsOpenBeforeThePanel_TheCommitteeIsLocked_SaidFirst_AndTheReviewIsLinked()
    {
        SignInAs(WombatRoles.InstitutionalAdmin);
        const string locked =
            "Review #1 (2026 S2, scheduled) still sits before this panel. What a review decides, and its agenda, follow the " +
            "College committee the panel sat as when it was scheduled, so the panel keeps what it sits as until its open " +
            "reviews are ratified, or closed if formative.";
        _sender.On<GetDecisionPanelByIdQuery>(_ => Panel(bodyKey: null) with
        {
            OpenReviews = [new PanelOpenReviewDto(1, "2026 S2", CommitteeReviewState.Scheduled, "scheduled", MayOpen: true)],
            DecisionBodyLock = locked
        });

        var cut = RenderComponent<PanelEdit>(parameters => parameters.Add(page => page.PanelId, PanelId));
        cut.WaitForState(() => cut.FindAll("#panel-body-lock").Count == 1);

        cut.FindAll("#panel-body").Should().BeEmpty("no select is offered while a review is open");
        cut.FindAll("form").Where(form => form.TextContent.Contains("Save committee")).Should().BeEmpty();
        cut.Find("#panel-body-value").TextContent.Trim().Should().Be("General panel");
        Squash(cut.Find("#panel-body-lock p").TextContent).Should().Be("It cannot change now. " + locked);
        cut.Find("#panel-body-lock a").GetAttribute("href").Should().Be("/committee/reviews/1");
        cut.Find("#panel-body-lock a").TextContent.Trim().Should().Be("Open review #1");
        Squash(cut.Find("#panel-open-reviews").TextContent).Should().Be("Review #1 (2026 S2, scheduled)");
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
        cut.Find("#panel-body-value").TextContent.Should().Be(NeonatalName);
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
        cut.WaitForState(() => cut.FindAll("#routing-table-100 tbody tr").Count == 2);

        _sender.Received.OfType<GetCommitteeRoutingQuery>().Should().ContainSingle()
            .Which.InstitutionId.Should().BeNull("anyone but an Administrator reads their own institution");

        // Rows read in the order of their first EPA (board R2-1): the general row, then the committee's.
        var rows = cut.FindAll("#routing-table-100 tbody tr").ToArray();
        rows[0].QuerySelector("th")!.TextContent.Should().Be("PAED-001, PAED-002");
        rows[0].QuerySelector("td")!.TextContent.Trim().Should().Be("Annual review");
        rows[1].QuerySelector("th")!.TextContent.Should().Be("PAED-004, PAED-005");
        rows[1].QuerySelector("td")!.TextContent.Trim().Should().Be($"Neonatal CCC, sitting as the {NeonatalName}.");
        cut.Find("#routing-table-100 caption").TextContent.Trim().Should().Be("Who decides each EPA of General Paediatrics 11.1");

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
        cut.WaitForState(() => cut.FindAll("#routing-table-100 tbody tr").Count == 2);

        var neonatalRow = cut.FindAll("#routing-table-100 tbody tr").Last();
        neonatalRow.QuerySelector("th")!.TextContent.Should().Be("PAED-004, PAED-005");
        neonatalRow.QuerySelector("td")!.TextContent.Trim().Should().Be(
            $"Annual review. No panel covering this programme sits as the {NeonatalName}, so the general panel decides.");
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
        cut.WaitForState(() => cut.FindAll("#routing-table-100 tbody tr").Count == 2);

        cut.FindAll("#routing-table-100 tbody tr").Last().QuerySelector("td")!.TextContent
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
        cut.WaitForState(() => cut.FindAll("#routing-table-100 tbody tr").Count == 1);

        // Where no panel covers the programme, every EPA shares the one row (step 2.20).
        var row = cut.Find("#routing-table-100 tbody tr");
        row.QuerySelector("th")!.TextContent.Should().Be("PAED-001, PAED-002, PAED-004, PAED-005");
        var cell = row.QuerySelector("td")!.TextContent;
        cell.Trim().Should().Be("No panel at this institution covers this programme.");
        cell.Should().NotContain("general panel");
    }

    [Fact]
    public void AnAdministrator_ChoosesTheInstitution_BeforeTheCardAsks()
    {
        SignInAs(WombatRoles.Administrator);
        _sender
            .On<ListDecisionPanelsQuery>(_ => Array.Empty<DecisionPanelSummaryDto>())
            .On<GetCommitteeRoutingQuery>(_ => Routing(neonatalPanel: null));

        var cut = RenderComponent<PanelsList>();
        cut.WaitForState(() => cut.FindAll("#panels-institution option").Count == 2);

        // F2: nothing is read for an institution until one is chosen and Show is pressed.
        _sender.Received.OfType<GetCommitteeRoutingQuery>().Should().BeEmpty();
        _sender.Received.OfType<ListDecisionPanelsQuery>().Should().BeEmpty();
        cut.Find(".state-panel-title").TextContent.Trim().Should().Be("Choose an institution");
        cut.Find(".state-panel-copy").TextContent.Trim()
            .Should().Be("An administrator belongs to no institution, so the panels are shown for the one chosen above.");
        cut.Markup.Should().Contain("Choose an institution to see which panel decides each EPA there.");

        cut.Find("#panels-institution").Change(InstitutionA.ToString());
        _sender.Received.OfType<GetCommitteeRoutingQuery>().Should().BeEmpty("nothing changes until Show");
        cut.Find("form").Submit();

        var navigation = Services.GetRequiredService<FakeNavigationManager>();
        navigation.Uri.Should().EndWith($"/committee/panels?institution={InstitutionA}");

        // The address asks for the institution: the page reads it again, the list and Who decides each EPA for it.
        cut.WaitForState(() => cut.FindAll("#routing-table-100").Count == 1);
        _sender.Received.OfType<GetCommitteeRoutingQuery>().Should().ContainSingle()
            .Which.InstitutionId.Should().Be(InstitutionA);
        _sender.Received.OfType<ListDecisionPanelsQuery>().Should().ContainSingle()
            .Which.InstitutionId.Should().Be(InstitutionA);
        cut.Find(".page-subtitle").TextContent.Trim()
            .Should().Be("The panels that take entrustment and progression decisions at Kgosi Kgari.");
        cut.Find("#panels-institution").GetAttribute("value").Should().Be(InstitutionA.ToString());
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
