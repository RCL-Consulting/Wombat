using System.Security.Claims;
using System.Text.RegularExpressions;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// The review page records who was present at each decision and says who was, offers as present only those who may sit,
/// disables Record and Ratify with the reason when a quorum is out of reach, fixes the staged STARs once the decision is
/// recorded, has a remitted appeal record its own sitting, and says when the chair rated every line of the evidence.
/// (T165)
/// </summary>
public sealed partial class ReviewDetailQuorumTests : TestContext
{
    private const string ShortOfAQuorum =
        "Only the chair was recorded as present when this decision was recorded. " +
        "A committee decision needs at least two panel members present: the chair and at least one other.";

    private readonly TestAuthorizationContext _auth;

    public ReviewDetailQuorumTests()
    {
        _auth = this.AddTestAuthorization();
        SignInAs("chair-1");
    }

    // ─── Recording ───────────────────────────────────────────────────────────

    [Fact]
    public void TheRecordForm_OffersThePanel_WithTheChairTickedAndLocked()
    {
        var (cut, _) = Render(Review(CommitteeReviewState.InProgress));

        var fieldset = PresentFieldset(cut);
        var boxes = fieldset.QuerySelectorAll("input[type=checkbox]").ToArray();
        var labels = fieldset.QuerySelectorAll("label").Select(label => label.TextContent.Trim()).ToArray();

        labels.Should().Equal("Thandi Zulu (chair)", "Priya Naidoo", "Anna Botha (external)");
        boxes[0].HasAttribute("checked").Should().BeTrue();
        boxes[0].HasAttribute("disabled").Should().BeTrue("the chair records the decision, so the chair was there");
        boxes.Skip(1).Should().OnlyContain(box => !box.HasAttribute("disabled") && !box.HasAttribute("checked"));
        fieldset.QuerySelectorAll("label").Select(label => label.GetAttribute("for"))
            .Should().Equal(boxes.Select(box => box.Id), "every checkbox is named by its own label");
        fieldset.TextContent.Should().Contain(CommitteeReviewDetailDto.QuorumRule);
    }

    [Fact]
    public void TheRecordForm_DoesNotOfferAPanelMemberWhoMayNotSit()
    {
        // A deactivated, departed or erased member, or the trainee: the form cannot send someone the server refuses.
        var review = Review(CommitteeReviewState.InProgress) with
        {
            PanelMembers =
            [
                Person("chair-1", DecisionPanelMemberRole.Chair, "Thandi Zulu"),
                Person("member-1", DecisionPanelMemberRole.Member, "Priya Naidoo"),
                Person("gone-1", DecisionPanelMemberRole.Member, "Gone Away", maySit: false)
            ]
        };

        var (cut, _) = Render(review);

        PresentFieldset(cut).QuerySelectorAll("label").Select(label => label.TextContent.Trim())
            .Should().Equal("Thandi Zulu (chair)", "Priya Naidoo");
    }

    [Fact]
    public void RecordingTheDecision_SendsTheChairAndEveryoneTicked()
    {
        var sender = new FakeSender(Review(CommitteeReviewState.InProgress));
        var (cut, _) = Render(sender);

        cut.Find("#decision-rationale").Change("On track across the board.");
        PresentFieldset(cut).QuerySelectorAll("input[type=checkbox]")[2].Change(true);
        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Record decision").Click();
        cut.WaitForState(() => sender.Recorded.Count == 1);

        sender.Recorded.Single().PresentUserIds.Should().Equal("chair-1", "external-1");

        // The command's answer names nobody; the page names them from the panel it loaded (WithNamesFrom).
        cut.WaitForState(() => cut.Markup.Contains("Decision recorded."));
        Whitespace().Replace(cut.Find("#decision-41").TextContent, " ")
            .Should().Contain("Present: Thandi Zulu (chair), Anna Botha (external)");
    }

    [Fact]
    public void RecordIsDisabled_WithTheReason_WhenThePanelCannotSeatAQuorum()
    {
        // Finding 4's case: a panel whose only seatable member is the chair. The server would refuse only after the chair
        // had written the rationale; T107's pattern says so first.
        var review = Review(CommitteeReviewState.InProgress) with
        {
            PanelMembers =
            [
                Person("chair-1", DecisionPanelMemberRole.Chair, "Thandi Zulu"),
                Person("gone-1", DecisionPanelMemberRole.Member, "Gone Away", maySit: false)
            ]
        };

        var (cut, _) = Render(review);

        var record = cut.FindAll("button").Single(button => button.TextContent.Trim() == "Record decision");
        record.HasAttribute("disabled").Should().BeTrue();
        record.GetAttribute("aria-describedby").Should().Be("record-reason");
        Whitespace().Replace(cut.Find("#record-reason").TextContent, " ").Trim()
            .Should().StartWith("Record decision: Only the chair can be recorded as present");
    }

    [Fact]
    public void RecordIsOffered_WhenThePanelCanSeatAQuorum_AndSaysItFixesTheStagedStars()
    {
        var (cut, _) = Render(Review(CommitteeReviewState.InProgress));

        var record = cut.FindAll("button").Single(button => button.TextContent.Trim() == "Record decision");
        record.HasAttribute("disabled").Should().BeFalse();
        record.GetAttribute("aria-describedby").Should().Be("record-decision-note");
        cut.Find("#record-decision-note").TextContent.Should().Contain("cannot be changed afterwards");
        cut.FindAll("#record-reason").Should().BeEmpty();
    }

    // ─── Each decision's own sitting ─────────────────────────────────────────

    [Fact]
    public void EachDecision_NamesItsOwnSitting()
    {
        // The review's decision, then an appeal's replacement: each says who took it. Before T165 the page showed the
        // first sitting's attendance beside whichever decision was current.
        var review = Review(CommitteeReviewState.Final) with
        {
            Decisions =
            [
                Decision(42, supersedes: 41, Person("chair-1", DecisionPanelMemberRole.Chair, "Thandi Zulu"),
                    Person("external-1", DecisionPanelMemberRole.External, "Anna Botha")),
                Decision(41, supersedes: null, Person("chair-1", DecisionPanelMemberRole.Chair, "Thandi Zulu"),
                    Person("member-1", DecisionPanelMemberRole.Member, "Priya Naidoo"))
            ]
        };

        var (cut, _) = Render(review);

        Whitespace().Replace(cut.Find("#decision-42").TextContent, " ")
            .Should().Contain("Present: Thandi Zulu (chair), Anna Botha (external)")
            .And.NotContain("Replaced on appeal");
        Whitespace().Replace(cut.Find("#decision-41").TextContent, " ")
            .Should().Contain("Present: Thandi Zulu (chair), Priya Naidoo")
            .And.Contain("Replaced on appeal by the decision above.");
        cut.FindAll("fieldset").Should().NotContain(set => set.TextContent.Contains("Tick every panel member"),
            "the attendance is taken when a decision is recorded, and none is being recorded");
    }

    [Fact]
    public void ADecisionWithNoAttendance_SaysNobodyWasRecorded()
    {
        var (_, text) = Render(Review(CommitteeReviewState.Decided) with { Decisions = [Decision(41, supersedes: null)] });

        text.Should().Contain("Nobody was recorded as present for this decision.");
    }

    // ─── Ratifying ───────────────────────────────────────────────────────────

    [Fact]
    public void RatifyIsOffered_WhenTheAttendanceHoldsAQuorum()
    {
        var (cut, _) = Render(Review(CommitteeReviewState.Decided));

        var ratify = cut.FindAll("button").Single(button => button.TextContent.Trim() == "Ratify");
        ratify.HasAttribute("disabled").Should().BeFalse();
        cut.FindAll("#ratify-reason").Should().BeEmpty();
    }

    [Fact]
    public void RatifyIsDisabled_WithTheReason_WhenTheAttendanceFallsShort()
    {
        // T107's pattern: shown, not hidden, and described by visible text.
        var (cut, _) = Render(Review(CommitteeReviewState.Decided) with { QuorumShortfall = ShortOfAQuorum });

        var ratify = cut.FindAll("button").Single(button => button.TextContent.Trim() == "Ratify");
        ratify.HasAttribute("disabled").Should().BeTrue();
        ratify.GetAttribute("aria-describedby").Should().Be("ratify-reason");
        Whitespace().Replace(cut.Find("#ratify-reason").TextContent, " ").Trim().Should().Be($"Ratify: {ShortOfAQuorum}");
        cut.Find("#ratify-reason").ParentElement!.ClassList.Should().Contain("workflow-action-reasons");
    }

    // ─── The staged STARs ────────────────────────────────────────────────────

    [Fact]
    public void WhileTheReviewIsInProgress_StarsCanBeStagedAndRemoved()
    {
        var (cut, _) = Render(Review(CommitteeReviewState.InProgress), pending: [Pending(1, noLongerFits: null)]);

        cut.FindAll("button").Should().Contain(button => button.TextContent.Trim() == "Stage pending decision");
        cut.FindAll("button").Should().Contain(button => button.TextContent.Trim() == "Remove");
    }

    [Fact]
    public void OnceTheDecisionIsRecorded_TheStagedStarsAreFixed_AndOnlyOneThatNoLongerFitsCanBeRemoved()
    {
        const string Stale = "PAED-008 is not on this trainee's curriculum, or its EPA is deactivated, so a STAR cannot be granted on it.";
        var (cut, text) = Render(
            Review(CommitteeReviewState.Decided),
            pending: [Pending(1, noLongerFits: null), Pending(2, noLongerFits: Stale)]);

        cut.FindAll("button").Should().NotContain(button => button.TextContent.Trim() == "Stage pending decision");
        text.Should().Contain("Fixed when the committee's decision was recorded: ratifying issues exactly these.");
        var items = cut.FindAll("section.detail-card li").Where(item => item.TextContent.Contains("PAED-00")).ToArray();
        items.Single(item => item.TextContent.Contains("PAED-007")).QuerySelectorAll("button").Should().BeEmpty();
        var stale = items.Single(item => item.TextContent.Contains("PAED-008"));
        stale.QuerySelectorAll("button").Select(button => button.TextContent.Trim()).Should().Equal("Remove");
        Whitespace().Replace(stale.TextContent, " ").Should().Contain($"No longer fits: {Stale}");
    }

    // ─── The appeal ──────────────────────────────────────────────────────────

    [Fact]
    public void RemittingAnAppeal_RecordsWhoSatForIt_WithTheChairAndTheResolverLocked()
    {
        // The external member resolves for independence; the chair must sit too. The external sits on the appeal body but
        // does not chair (T213).
        SignInAs("external-1");
        var sender = new FakeSender(Review(CommitteeReviewState.UnderAppeal) with { CallerChairs = false });
        var (cut, _) = Render(sender);

        cut.Find("#appeal-outcome").Change(CommitteeAppealOutcome.Remitted.ToString());
        cut.Find("#appeal-category").Change(CommitteeDecisionCategory.SatisfactoryProgress.ToString());
        cut.Find("#appeal-rationale").Change("Conditions lifted on appeal.");

        var fieldset = cut.FindAll("fieldset").Single(set => set.QuerySelector("#remit-present-help") is not null);
        var boxes = fieldset.QuerySelectorAll("input[type=checkbox]").ToArray();
        boxes.Select(box => box.HasAttribute("disabled")).Should().Equal(true, false, true);
        boxes.Select(box => box.Id).Should().Equal("remit-present-0", "remit-present-1", "remit-present-2");

        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Resolve appeal").Click();
        cut.WaitForState(() => sender.Resolved.Count == 1);

        sender.Resolved.Single().PresentUserIds.Should().Equal("chair-1", "external-1");
    }

    [Fact]
    public void DismissingAnAppeal_SendsNoAttendance()
    {
        var sender = new FakeSender(Review(CommitteeReviewState.UnderAppeal));
        var (cut, _) = Render(sender);

        cut.FindAll("fieldset").Should().NotContain(set => set.QuerySelector("#remit-present-help") != null);
        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Resolve appeal").Click();
        cut.WaitForState(() => sender.Resolved.Count == 1);

        sender.Resolved.Single().PresentUserIds.Should().BeNull();
    }

    [Fact]
    public void RemittingIsDisabled_WithTheReason_WhenThePanelCannotSeatAQuorum()
    {
        var review = Review(CommitteeReviewState.UnderAppeal) with
        {
            PanelMembers = [Person("chair-1", DecisionPanelMemberRole.Chair, "Thandi Zulu")]
        };
        var (cut, _) = Render(review);

        cut.Find("#appeal-outcome").Change(CommitteeAppealOutcome.Remitted.ToString());

        var resolve = cut.FindAll("button").Single(button => button.TextContent.Trim() == "Resolve appeal");
        resolve.HasAttribute("disabled").Should().BeTrue();
        resolve.GetAttribute("aria-describedby").Should().Be("remit-reason");
        cut.Find("#remit-reason").TextContent.Should().Contain("Resolve appeal:");
    }

    // ─── The chair's own ratings ─────────────────────────────────────────────

    [Fact]
    public void WhenTheChairRatedEveryLine_ThePanelIsWarned_AndNotStopped()
    {
        var (cut, text) = Render(Review(CommitteeReviewState.InProgress, evidence: [RatedLine(1, "chair-1"), RatedLine(2, "chair-1")]));

        text.Should().Contain("The chair rated all of this evidence");
        text.Should().Contain("Every rated record in the evidence snapshot was rated by Thandi Zulu, who chairs this panel.");
        cut.FindAll("button").Should().Contain(button => button.TextContent.Trim() == "Record decision");
    }

    [Fact]
    public void WhenAnotherAssessorRatedALine_ThereIsNoWarning()
    {
        var (_, text) = Render(Review(CommitteeReviewState.InProgress, evidence: [RatedLine(1, "chair-1"), RatedLine(2, "assessor-a")]));

        text.Should().NotContain("The chair rated all of this evidence");
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private void SignInAs(string userId)
    {
        _auth.SetAuthorized($"{userId}@test");
        _auth.SetRoles("CommitteeMember");
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, userId));
    }

    private static AngleSharp.Dom.IElement PresentFieldset(IRenderedComponent<ReviewDetail> cut)
        => cut.FindAll("fieldset").Single(set => set.QuerySelector("#decision-present-help") is not null);

    private (IRenderedComponent<ReviewDetail> Cut, string Text) Render(
        CommitteeReviewDetailDto review, IReadOnlyList<PendingEntrustmentDecisionDto>? pending = null)
        => Render(new FakeSender(review, pending));

    private (IRenderedComponent<ReviewDetail> Cut, string Text) Render(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, 30));
        cut.WaitForState(() => cut.Markup.Contains("Evidence snapshot"));

        var text = string.Join(" ", cut.Nodes.Select(node => node.TextContent));
        return (cut, Whitespace().Replace(text, " "));
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    private static CommitteePersonDto Person(string userId, DecisionPanelMemberRole role, string name, bool maySit = true)
        => new(userId, role) { Name = name, MaySit = maySit };

    private static CommitteeDecisionDto Decision(int id, int? supersedes, params CommitteePersonDto[] attendees)
        => new(id, CommitteeDecisionCategory.SatisfactoryProgress, $"Decision {id}.", null,
            new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc).AddDays(id), "chair-1", supersedes)
        {
            Attendees = attendees.Select(person => person with { MaySit = false }).ToArray()
        };

    private static PendingEntrustmentDecisionDto Pending(int id, string? noLongerFits)
        => new(id, 30, 6 + id, $"PAED-00{6 + id}", "An EPA", 3, "3a", new DateOnly(2026, 7, 2), null, "Ready.", [],
            new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc), "chair-1")
        {
            NoLongerFits = noLongerFits
        };

    private static CommitteeEvidenceDto RatedLine(int id, string assessor)
        => new(
            id,
            CommitteeEvidenceSourceType.Activity,
            100 + id,
            null,
            null,
            $"CCA #{100 + id}",
            "State: completed.",
            new DateTime(2026, 2, 2, 8, 0, 0, DateTimeKind.Utc),
            EpaId: 7,
            EpaCode: "PAED-001",
            EpaTitle: "Acute admission",
            InstrumentKey: "cca",
            InstrumentName: "CCA",
            IsRatedInstrument: true,
            RatingOrder: 3,
            RatingLabel: "3a",
            ObservedOn: new DateOnly(2026, 2, 2),
            ObservedOnDeclared: true,
            SourceState: "completed",
            AssessorUserId: assessor);

    private static CommitteeReviewDetailDto Review(
        CommitteeReviewState state,
        IReadOnlyList<CommitteeEvidenceDto>? evidence = null)
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
            state is CommitteeReviewState.Scheduled or CommitteeReviewState.InProgress
                ? []
                : [Decision(41, supersedes: null,
                    Person("chair-1", DecisionPanelMemberRole.Chair, "Thandi Zulu"),
                    Person("member-1", DecisionPanelMemberRole.Member, "Priya Naidoo"))],
            [],
            evidence ?? [])
        {
            AcademicYear = 2026,
            Semester = 1,
            // Signed in as the chair unless a test says otherwise: the page offers the chair's controls by what the query
            // says the caller may do (T213).
            CallerChairs = true,
            CallerResolvesAppeals = true,
            TraineeName = "Lerato Molefe",
            PanelMembers =
            [
                Person("chair-1", DecisionPanelMemberRole.Chair, "Thandi Zulu"),
                Person("member-1", DecisionPanelMemberRole.Member, "Priya Naidoo"),
                Person("external-1", DecisionPanelMemberRole.External, "Anna Botha")
            ]
        };

    private sealed class FakeSender : IScopedSender
    {
        private readonly CommitteeReviewDetailDto _review;
        private readonly IReadOnlyList<PendingEntrustmentDecisionDto> _pending;

        public FakeSender(CommitteeReviewDetailDto review, IReadOnlyList<PendingEntrustmentDecisionDto>? pending = null)
        {
            _review = review;
            _pending = pending ?? [];
        }

        public List<RecordCommitteeDecisionCommand> Recorded { get; } = [];

        public List<ResolveAppealCommand> Resolved { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case RecordCommitteeDecisionCommand record:
                    Recorded.Add(record);
                    break;
                case ResolveAppealCommand resolve:
                    Resolved.Add(resolve);
                    break;
            }

            object? response = request switch
            {
                GetCommitteeReviewByIdQuery => _review,
                // The command's answer names nobody and seats nobody, as the shared mapper's does; the page keeps what it
                // loaded.
                RecordCommitteeDecisionCommand recorded => _review with
                {
                    TraineeName = null,
                    State = CommitteeReviewState.Decided,
                    PanelMembers = _review.PanelMembers.Select(person => person with { Name = null, MaySit = false }).ToArray(),
                    Decisions =
                    [
                        new CommitteeDecisionDto(41, recorded.Category, recorded.Rationale, recorded.Conditions,
                            new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc), "chair-1", null)
                        {
                            Attendees = _review.PanelMembers
                                .Where(person => recorded.PresentUserIds.Contains(person.UserId))
                                .Select(person => person with { Name = null, MaySit = false })
                                .ToArray()
                        }
                    ]
                },
                ResolveAppealCommand => _review with { State = CommitteeReviewState.Final },
                ListPendingEntrustmentDecisionsForReviewQuery => _pending,
                GetSamplingConcentrationWarningsQuery => null,
                CountMsfCampaignsOutsideSnapshotQuery => MsfCampaignsOutsideSnapshotDto.None,
                GetEpaTrajectoryForTraineeQuery => Array.Empty<EpaTrajectoryDto>(),
                ListStarEpaOptionsForReviewQuery => Array.Empty<StarEpaOptionDto>(),
                GetEntrustmentScalesListQuery => Array.Empty<EntrustmentScaleDto>(),
                // Advisory cards the page loads apart (T166, T168); not what these tests are about.
                GetEntrustmentStandingForTraineeQuery => null,
                GetMsfCoverageForTraineeQuery => null,
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response!);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
