using System.Security.Claims;
using System.Text.RegularExpressions;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// T213. The review page offers the chair's controls to the chair alone, and the resolve-appeal form to the appeal body
/// alone, by what the query says the caller may do (the predicates the handlers demand); tells anyone else who can act; shows
/// a validator's refusal in its own words; and never tells a chair to remove a staged decision the handlers will not let
/// them remove.
/// </summary>
public sealed partial class ReviewDetailChairControlsTests : TestContext
{
    /// <summary>Every control only the chair may use, by its visible label.</summary>
    private static readonly string[] ChairsControls =
        ["Record decision", "Ratify", "Close review", "Stage pending decision", "Remove", "Stage", "Defer", "Reinstate"];

    private readonly TestAuthorizationContext _auth;
    private readonly RecordingSender _sender = new();

    public ReviewDetailChairControlsTests()
    {
        _auth = this.AddTestAuthorization();
        Services.AddSingleton<IScopedSender>(_sender);
    }

    // ─── The chair's controls ────────────────────────────────────────────────

    [Fact]
    public void WhileTheReviewIsInProgress_ThePanelsChair_IsOfferedEveryControl()
    {
        SignInAs("chair-1");
        var cut = Render(Review(CommitteeReviewState.InProgress, callerChairs: true), [Pending(7, evidence: [501])]);

        Buttons(cut).Should().Contain(["Record decision", "Stage pending decision", "Remove", "Stage", "Defer", "Reinstate"]);
        cut.FindAll("#decision-rationale").Should().ContainSingle();
        cut.FindAll("#pending-epa").Should().ContainSingle();
        AgendaHeaders(cut).Should().Contain("Action");
        cut.FindAll("#chair-actions-note").Should().BeEmpty();
    }

    [Fact]
    public void WhileTheReviewIsInProgress_AMemberWhoDoesNotChair_IsOfferedNoneOfTheChairsControls_AndIsToldWhoCan()
    {
        // Before T213 every one of these was offered, and each refused on the click ("Only the panel's chair can do this.").
        SignInAs("member-1");
        var cut = Render(Review(CommitteeReviewState.InProgress, callerChairs: false), [Pending(7, evidence: [501])]);

        Buttons(cut).Should().NotContain(ChairsControls);
        cut.FindAll("#decision-rationale").Should().BeEmpty("the decision form is the chair's");
        cut.FindAll("#pending-epa").Should().BeEmpty("the staging form is the chair's");
        AgendaHeaders(cut).Should().Equal(["EPA", "Window", "State", "Evidence"], "nobody else gets an empty Action column");
        Text(cut.Find("#chair-actions-note")).Should().Be(
            "Only the panel's chair, Thandi Zulu, can stage entrustment decisions, defer agenda lines and record the " +
            "committee's decision.");

        // What the review holds is still there to read.
        Text(cut.Find("#agenda-line-1")).Should().Contain("PAED-001");
        Page(cut).Should().Contain("PAED-007 — An EPA");
    }

    [Fact]
    public void OnADecidedReview_AMemberWhoDoesNotChair_IsNotOfferedRatify_NorItsReason()
    {
        SignInAs("member-1");
        var cut = Render(Review(CommitteeReviewState.Decided, callerChairs: false) with { QuorumShortfall = "Short." });

        Buttons(cut).Should().NotContain("Ratify");
        cut.FindAll("#ratify-reason").Should().BeEmpty();
        Text(cut.Find("#chair-actions-note")).Should().Be("Only the panel's chair, Thandi Zulu, can ratify the committee's decision.");
    }

    [Fact]
    public void OnADecidedReview_TheChair_IsOfferedRatify()
    {
        SignInAs("chair-1");
        var cut = Render(Review(CommitteeReviewState.Decided, callerChairs: true));

        Buttons(cut).Should().Contain("Ratify");
        cut.FindAll("#chair-actions-note").Should().BeEmpty();
    }

    [Fact]
    public void OnAFormativeReview_AMemberWhoDoesNotChair_IsNotOfferedClose()
    {
        SignInAs("member-1");
        var cut = Render(Review(CommitteeReviewState.InProgress, callerChairs: false) with { IsFormative = true });

        Buttons(cut).Should().NotContain("Close review");
        Text(cut.Find("#chair-actions-note")).Should().Be("Only the panel's chair, Thandi Zulu, can close this review.");
    }

    // ─── The appeal body ─────────────────────────────────────────────────────

    [Fact]
    public void UnderAppeal_AMemberOutsideTheAppealBody_IsNotOfferedTheResolveForm_AndIsToldWhoResolves()
    {
        SignInAs("member-1");
        var cut = Render(Review(CommitteeReviewState.UnderAppeal, callerChairs: false));

        cut.FindAll("#appeal-outcome").Should().BeEmpty();
        Buttons(cut).Should().NotContain("Resolve appeal");
        Text(cut.Find("#appeal-body-note")).Should().Be(
            "Only the appeal body can resolve the appeal: the panel's chair, Thandi Zulu, and its external member, Anna Botha.");
    }

    [Fact]
    public void UnderAppeal_TheAppealBodyNote_NamesTheChairAlone_WhenThePanelHasNoExternalMember()
    {
        SignInAs("member-1");
        var review = Review(CommitteeReviewState.UnderAppeal, callerChairs: false);
        var cut = Render(review with
        {
            PanelMembers = review.PanelMembers.Where(person => person.UserId != "external-1").ToArray(),
            AppealBody = review.AppealBody.Where(person => person.UserId != "external-1").ToArray()
        });

        Text(cut.Find("#appeal-body-note")).Should().Be("Only the appeal body can resolve the appeal: the panel's chair, Thandi Zulu.");
    }

    [Fact]
    public void UnderAppeal_TheAppealBodyNote_NamesNoExternalMemberWhoCannotAct()
    {
        // T237. The committee chain's browser check: a trainee seated as External (before T237 refused the seat) was
        // named here, to the trainee on their own appeal too, though since T194 they cannot resolve it. The note names
        // the appeal body the query says can act, not every External seat on the panel.
        SignInAs("member-1");
        var review = Review(CommitteeReviewState.UnderAppeal, callerChairs: false);
        var cut = Render(review with
        {
            PanelMembers = [.. review.PanelMembers, Person("rep-1", DecisionPanelMemberRole.External, "Demo Trainee")]
        });

        Text(cut.Find("#appeal-body-note")).Should().Be(
            "Only the appeal body can resolve the appeal: the panel's chair, Thandi Zulu, and its external member, Anna Botha.");
    }

    [Fact]
    public void UnderAppeal_WhenTheChairCannotAct_TheNoteNamesTheExternalMembersAlone()
    {
        SignInAs("member-1");
        var review = Review(CommitteeReviewState.UnderAppeal, callerChairs: false);
        var cut = Render(review with { AppealBody = review.AppealBody.Where(person => person.UserId != "chair-1").ToArray() });

        Text(cut.Find("#appeal-body-note")).Should().Be(
            "Only the appeal body can resolve the appeal: the panel's external member, Anna Botha.");
    }

    [Fact]
    public void UnderAppeal_WhenNoOneOnTheAppealBodyCanAct_TheNoteSaysSo_AndNamesNobody()
    {
        SignInAs("member-1");
        var cut = Render(Review(CommitteeReviewState.UnderAppeal, callerChairs: false) with { AppealBody = [] });

        Text(cut.Find("#appeal-body-note")).Should().Be(
            "Only the appeal body, the panel's chair or an external member, can resolve the appeal, and none of them can " +
            "act now. A panel administrator must seat a chair who can.");
    }

    // ─── Each form only in the state its handler takes it (T213 review) ──────

    [Fact]
    public void WhileTheReviewIsInProgress_TheChair_IsNotOfferedTheResolveForm()
    {
        // Before the T213 review the appeal body was shown a required Outcome with no button on every summative review,
        // above "No appeal has been lodged against this review.".
        SignInAs("chair-1");
        var cut = Render(Review(CommitteeReviewState.InProgress, callerChairs: true));

        cut.FindAll("#appeal-outcome").Should().BeEmpty();
        Buttons(cut).Should().NotContain("Resolve appeal");
        Text(AppealsCard(cut)).Should().Contain("No appeal has been lodged against this review.");
    }

    [Theory]
    [InlineData(CommitteeReviewState.Decided)]
    [InlineData(CommitteeReviewState.Ratified)]
    [InlineData(CommitteeReviewState.Final)]
    public void OnceTheDecisionIsRecorded_TheChair_IsNotShownAnEmptyDecisionForm(CommitteeReviewState state)
    {
        SignInAs("chair-1");
        var cut = Render(Review(state, callerChairs: true));

        cut.FindAll("#decision-rationale").Should().BeEmpty();
        cut.FindAll("#decision-category").Should().BeEmpty();
        cut.FindAll("#decision-conditions").Should().BeEmpty();
        cut.FindAll("#appeal-outcome").Should().BeEmpty("no appeal is open");
        Text(cut.Find("#decision-41")).Should().Contain("On track.");
    }

    [Fact]
    public void UnderAppeal_TheChair_IsOfferedTheResolveForm_AndNoDecisionForm()
    {
        SignInAs("chair-1");
        var cut = Render(Review(CommitteeReviewState.UnderAppeal, callerChairs: true));

        cut.FindAll("#appeal-outcome").Should().ContainSingle();
        Buttons(cut).Should().Contain("Resolve appeal");
        cut.FindAll("#decision-rationale").Should().BeEmpty("the remit form carries its own replacement decision");
    }

    // ─── A decision card with no decision (T213 review) ──────────────────────

    [Fact]
    public void WhileTheReviewIsInProgress_AMemberWhoDoesNotChair_IsToldNoDecisionIsRecordedYet()
    {
        SignInAs("member-1");
        var cut = Render(Review(CommitteeReviewState.InProgress, callerChairs: false));

        Text(cut.Find("#no-decision-note")).Should().Be("No decision has been recorded yet.");
    }

    [Fact]
    public void WhileTheReviewIsInProgress_TheChair_HasTheFormInstead_OfTheNoDecisionNote()
    {
        SignInAs("chair-1");
        var cut = Render(Review(CommitteeReviewState.InProgress, callerChairs: true));

        cut.FindAll("#no-decision-note").Should().BeEmpty();
        cut.FindAll("#decision-rationale").Should().ContainSingle();
    }

    [Fact]
    public void OnADecidedReview_AMemberWhoDoesNotChair_ReadsTheDecision_NotTheNoDecisionNote()
    {
        SignInAs("member-1");
        var cut = Render(Review(CommitteeReviewState.Decided, callerChairs: false));

        cut.FindAll("#no-decision-note").Should().BeEmpty();
        Text(cut.Find("#decision-41")).Should().Contain("On track.");
    }

    // ─── What only the chair can still do at a decided review (T213 review) ──

    [Fact]
    public void OnADecidedReview_AMemberWhoDoesNotChair_IsNotOfferedRemove_ForADecisionThatNoLongerFits_AndIsToldTheChairCan()
    {
        SignInAs("member-1");
        var cut = Render(
            Review(CommitteeReviewState.Decided, callerChairs: false),
            [Pending(7, evidence: [501]) with { NoLongerFits = "PAED-007 is no longer on the trainee's curriculum." }]);

        PendingLine(cut, "PAED-007").QuerySelectorAll("button").Should().BeEmpty();
        Buttons(cut).Should().NotContain("Remove");
        Text(cut.Find("#chair-actions-note")).Should().Be(
            "Only the panel's chair, Thandi Zulu, can remove a staged decision that no longer fits the trainee's " +
            "curriculum and ratify the committee's decision.");
    }

    [Fact]
    public void OnADecidedReview_TheChair_IsOfferedRemove_ForADecisionThatNoLongerFits()
    {
        SignInAs("chair-1");
        var cut = Render(
            Review(CommitteeReviewState.Decided, callerChairs: true),
            [Pending(7, evidence: [501]) with { NoLongerFits = "PAED-007 is no longer on the trainee's curriculum." }]);

        PendingLine(cut, "PAED-007").QuerySelectorAll("button").Select(button => button.TextContent.Trim()).Should().Equal("Remove");
    }

    [Fact]
    public void OnADecidedReview_WithALineThatKeepsItFromBeingRatified_AMemberIsToldTheChairCanDeferIt()
    {
        SignInAs("member-1");
        var review = Review(CommitteeReviewState.Decided, callerChairs: false);
        var cut = Render(review with
        {
            Agenda = review.Agenda! with
            {
                Lines = [Line(1, "PAED-001", CommitteeAgendaLineStatus.Due, CommitteeAgendaLineState.Due, blocksRatify: true)]
            }
        });

        Buttons(cut).Should().NotContain("Defer");
        Text(cut.Find("#chair-actions-note")).Should().Be(
            "Only the panel's chair, Thandi Zulu, can defer an agenda line that keeps the review from being ratified and " +
            "ratify the committee's decision.");
    }

    // ─── A trainee who has moved institution (T213 review) ───────────────────

    [Fact]
    public void WhenTheTraineeHasMoved_TheChair_IsOfferedNoneOfTheChairsControls_AndIsToldWhy()
    {
        // Every chair's action demands that the trainee still train at the panel's institution, and before the T213
        // review each refused on the click with this sentence.
        SignInAs("chair-1");
        var cut = Render(
            Review(CommitteeReviewState.InProgress, callerChairs: true) with { TraineeElsewhere = TraineeMoved },
            [Pending(7, evidence: [501])]);

        Buttons(cut).Should().NotContain(ChairsControls);
        cut.FindAll("#decision-rationale").Should().BeEmpty();
        cut.FindAll("#pending-epa").Should().BeEmpty();
        AgendaHeaders(cut).Should().Equal(["EPA", "Window", "State", "Evidence"]);
        Text(cut.Find("#trainee-elsewhere-note")).Should().Be(TraineeMoved);
        cut.FindAll("#chair-actions-note").Should().BeEmpty("the chair is not the one to wait for");
    }

    [Fact]
    public void WhenTheTraineeHasMoved_AMemberWhoDoesNotChair_IsToldWhy_NotToWaitForTheChair()
    {
        SignInAs("member-1");
        var cut = Render(Review(CommitteeReviewState.Decided, callerChairs: false) with { TraineeElsewhere = TraineeMoved });

        Text(cut.Find("#trainee-elsewhere-note")).Should().Be(TraineeMoved);
        cut.FindAll("#chair-actions-note").Should().BeEmpty();
    }

    [Fact]
    public void WhenTheTraineeHasMoved_AScheduledReview_OffersNoStart()
    {
        SignInAs("member-1");
        var cut = Render(Review(CommitteeReviewState.Scheduled, callerChairs: false) with { TraineeElsewhere = TraineeMoved });

        Buttons(cut).Should().NotContain("Start review");
        Text(cut.Find("#trainee-elsewhere-note")).Should().Be(TraineeMoved);
    }

    [Fact]
    public void AScheduledReview_WhoseTraineeHasNotMoved_OffersStart()
    {
        SignInAs("member-1");
        var cut = Render(Review(CommitteeReviewState.Scheduled, callerChairs: false));

        Buttons(cut).Should().Contain("Start review");
        cut.FindAll("#trainee-elsewhere-note").Should().BeEmpty();
    }

    [Fact]
    public void AScheduledReview_IsNotOfferedToStart_ToAReaderTheHandlerRefuses_AndSaysWhoCan()
    {
        // T194. An institutional administrator reads every review at their institution, but starting one is for the
        // panel's members and the institution's coordinators (CommitteeDecisionAuthorization.WorksOnPanel). Before T194
        // they were offered Start, and the click was refused.
        SignInAs("instadmin-1");
        var cut = Render(Review(CommitteeReviewState.Scheduled, callerChairs: false) with { CallerMayStart = false });

        Buttons(cut).Should().NotContain("Start review");
        Text(cut.Find("#chair-actions-note"))
            .Should().Be("Only the panel's members, and the coordinators of its institution, can start this review.");
    }

    [Fact]
    public void AScheduledReview_OfferedStart_SaysNothingAboutWhoCan()
    {
        SignInAs("member-1");
        var cut = Render(Review(CommitteeReviewState.Scheduled, callerChairs: false));

        Buttons(cut).Should().Contain("Start review");
        cut.FindAll("#chair-actions-note").Should().BeEmpty();
    }

    [Fact]
    public void UnderAppeal_AnExternalMember_IsOfferedTheResolveForm_ThoughNotTheChairsControls()
    {
        SignInAs("external-1");
        var cut = Render(Review(CommitteeReviewState.UnderAppeal, callerChairs: false) with { CallerResolvesAppeals = true });

        cut.FindAll("#appeal-outcome").Should().ContainSingle();
        Buttons(cut).Should().Contain("Resolve appeal");
        cut.FindAll("#appeal-body-note").Should().BeEmpty();
    }

    // ─── Refusals in their own words ─────────────────────────────────────────

    [Fact]
    public void ARecordRefusedByItsValidator_ShowsTheRefusalOnly_NotTheValidatorsLogFormat()
    {
        // Item 5: the page used to print ValidationException.Message: "Validation failed: -- PresentUserIds: … Severity:
        // Error".
        SignInAs("chair-1");
        _sender.On<RecordCommitteeDecisionCommand>(_ => throw new ValidationException(
        [
            new ValidationFailure("PresentUserIds", CommitteeReview.QuorumRule),
            new ValidationFailure("PresentUserIds", CommitteeReview.QuorumRule)
        ]));
        var cut = Render(Review(CommitteeReviewState.InProgress, callerChairs: true));

        cut.Find("#decision-rationale").Change("On track.");
        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Record decision").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".alert-danger")).Should().Be(CommitteeReview.QuorumRule));
        Page(cut).Should().NotContain("Validation failed").And.NotContain("Severity").And.NotContain("PresentUserIds");
    }

    // ─── A staged decision that names no evidence ────────────────────────────

    [Fact]
    public void OnADecidedReview_AStagedDecisionThatNamesNoEvidence_IsNotSaidToBeRemovable()
    {
        // Item 3: once the decision is recorded the staged decisions are fixed (D46), so neither Remove nor Stage will take
        // it; the page says what the ratify refusal says.
        SignInAs("chair-1");
        var cut = Render(Review(CommitteeReviewState.Decided, callerChairs: true), [Pending(7, evidence: [])]);

        var line = PendingLine(cut, "PAED-007");
        Text(line).Should().Contain(
                $"Names no evidence, so the review cannot be ratified with it. {StagedEvidence.UngroundedOnceRecorded}")
            .And.NotContain("Remove it");
        line.QuerySelectorAll("button").Should().BeEmpty();
    }

    [Fact]
    public void WhileTheReviewIsInProgress_AStagedDecisionThatNamesNoEvidence_TellsTheChairToRemoveIt()
    {
        SignInAs("chair-1");
        var cut = Render(Review(CommitteeReviewState.InProgress, callerChairs: true), [Pending(7, evidence: [])]);

        var line = PendingLine(cut, "PAED-007");
        Text(line).Should().Contain(
            "Names no evidence, so the review cannot be ratified with it. Remove it and stage it again, naming the evidence.");
        line.QuerySelectorAll("button").Select(button => button.TextContent.Trim()).Should().Equal("Remove");
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private void SignInAs(string userId)
    {
        _auth.SetAuthorized($"{userId}@test");
        _auth.SetRoles(WombatRoles.CommitteeMember);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, userId));
    }

    private IRenderedComponent<ReviewDetail> Render(
        CommitteeReviewDetailDto review, IReadOnlyList<PendingEntrustmentDecisionDto>? pending = null)
    {
        _sender
            .On<GetCommitteeReviewByIdQuery>(_ => review)
            .On<ListPendingEntrustmentDecisionsForReviewQuery>(_ => pending ?? [])
            .On<GetSamplingConcentrationWarningsQuery>(_ => null)
            .On<CountMsfCampaignsOutsideSnapshotQuery>(_ => MsfCampaignsOutsideSnapshotDto.None)
            .On<GetEpaTrajectoryForTraineeQuery>(_ => Array.Empty<EpaTrajectoryDto>())
            .On<ListStarEpaOptionsForReviewQuery>(_ => new[] { new StarEpaOptionDto(1, "PAED-001", "EPA 1", null, null) })
            .On<GetEntrustmentScalesListQuery>(_ => Array.Empty<EntrustmentScaleDto>())
            .On<GetEntrustmentStandingForTraineeQuery>(_ => null)
            .On<GetMsfCoverageForTraineeQuery>(_ => null)
            .On<GetCommitteeAgendaQuery>(_ => review.Agenda);

        var cut = RenderComponent<ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, 30));
        cut.WaitForState(() => cut.Markup.Contains("Evidence snapshot"));
        return cut;
    }

    private static CommitteeReviewDetailDto Review(CommitteeReviewState state, bool callerChairs)
        => new CommitteeReviewDetailDto(
            30,
            "trainee-1",
            20,
            "Paediatrics CCC",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 6, 30),
            new DateOnly(2026, 7, 2),
            state,
            new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc),
            "chair-1",
            null,
            null,
            null,
            state is CommitteeReviewState.Scheduled or CommitteeReviewState.InProgress ? [] : [Decision()],
            state == CommitteeReviewState.UnderAppeal
                ? [new CommitteeAppealDto(1, new DateTime(2026, 7, 9, 9, 0, 0, DateTimeKind.Utc), "trainee-1", "Unfair.", null, null, null)]
                : [],
            [Evidence(501, 1)])
        {
            AcademicYear = 2026,
            Semester = 1,
            CallerChairs = callerChairs,
            // Every reader here but T194's institutional administrator sits on the panel, so may start it.
            CallerMayStart = true,
            CallerResolvesAppeals = callerChairs,
            TraineeName = "Lerato Molefe",
            Agenda = new CommitteeAgendaDto(
                30, 2026, 1, "2026 S1", false,
                [
                    Line(1, "PAED-001", CommitteeAgendaLineStatus.Due, CommitteeAgendaLineState.Due),
                    Line(4, "PAED-004", CommitteeAgendaLineStatus.Deferred, CommitteeAgendaLineState.Deferred, reason: "Later.")
                ],
                []),
            PanelMembers =
            [
                Person("chair-1", DecisionPanelMemberRole.Chair, "Thandi Zulu"),
                Person("member-1", DecisionPanelMemberRole.Member, "Priya Naidoo"),
                Person("external-1", DecisionPanelMemberRole.External, "Anna Botha")
            ],
            // Who can resolve the appeal, as the query fills it: only under appeal (T237).
            AppealBody = state == CommitteeReviewState.UnderAppeal
                ?
                [
                    Person("chair-1", DecisionPanelMemberRole.Chair, "Thandi Zulu"),
                    Person("external-1", DecisionPanelMemberRole.External, "Anna Botha")
                ]
                : []
        };

    private static CommitteePersonDto Person(string userId, DecisionPanelMemberRole role, string name)
        => new(userId, role) { Name = name, MaySit = true };

    private static CommitteeDecisionDto Decision()
        => new(41, CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null,
            new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc), "chair-1", null)
        {
            Attendees =
            [
                new CommitteePersonDto("chair-1", DecisionPanelMemberRole.Chair) { Name = "Thandi Zulu" },
                new CommitteePersonDto("member-1", DecisionPanelMemberRole.Member) { Name = "Priya Naidoo" }
            ]
        };

    private static PendingEntrustmentDecisionDto Pending(int epaId, IReadOnlyList<int> evidence)
        => new(9, 30, epaId, $"PAED-00{epaId}", "An EPA", 3, "3a", new DateOnly(2026, 7, 2), null, "Ready.", evidence,
            new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc), "chair-1");

    private static CommitteeAgendaLineDto Line(
        int epaId,
        string code,
        CommitteeAgendaLineStatus status,
        CommitteeAgendaLineState state,
        string? reason = null,
        bool blocksRatify = false)
        => new(
            100 + epaId, epaId, code, $"EPA {epaId}", CommitteeAgendaLineOrigin.Cadence, 2026, 1, "2026 S1",
            status == CommitteeAgendaLineStatus.Due, false, state, status, false, blocksRatify, reason, null, 1);

    private const string TraineeMoved =
        "This review's trainee does not train at the panel's institution, so the panel cannot act on it.";

    private static AngleSharp.Dom.IElement AppealsCard(IRenderedComponent<ReviewDetail> cut)
        => cut.FindAll("section.detail-card").Single(card => card.QuerySelector("h3")?.TextContent.Trim() == "Appeals");

    private static CommitteeEvidenceDto Evidence(int id, int epaId)
        => new(
            id, CommitteeEvidenceSourceType.Activity, id, null, null, $"Mini-CEX #{id}", "State: completed.",
            new DateTime(2026, 2, 10, 9, 0, 0, DateTimeKind.Utc),
            EpaId: epaId, EpaCode: $"PAED-00{epaId}", EpaTitle: $"EPA {epaId}", InstrumentName: "Mini-CEX",
            ObservedOn: new DateOnly(2026, 2, 10), ObservedOnDeclared: true, SourceState: "completed", SourceFinished: true);

    private static IReadOnlyList<string> Buttons(IRenderedComponent<ReviewDetail> cut)
        => cut.FindAll("button").Select(button => button.TextContent.Trim()).ToArray();

    private static IReadOnlyList<string> AgendaHeaders(IRenderedComponent<ReviewDetail> cut)
        => cut.FindAll("#agenda-heading ~ .table-container thead th").Select(header => header.TextContent.Trim()).ToArray();

    private static AngleSharp.Dom.IElement PendingLine(IRenderedComponent<ReviewDetail> cut, string code)
        => cut.FindAll("section.detail-card li").Single(item => item.QuerySelector("strong")?.TextContent.StartsWith(code) == true);

    private static string Page(IRenderedComponent<ReviewDetail> cut)
        => Whitespace().Replace(string.Join(" ", cut.Nodes.Select(node => node.TextContent)), " ");

    private static string Text(AngleSharp.Dom.IElement element) => Whitespace().Replace(element.TextContent, " ").Trim();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    /// <summary>Answers what a test registers, records every request, and refuses anything unregistered.</summary>
    private sealed class RecordingSender : IScopedSender
    {
        private readonly Dictionary<Type, Func<object, object?>> _answers = [];

        public RecordingSender On<TRequest>(Func<TRequest, object?> answer)
        {
            _answers.TryAdd(typeof(TRequest), request => answer((TRequest)request));
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
            => _answers.TryGetValue(request.GetType(), out var answer)
                ? answer(request)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
    }
}
