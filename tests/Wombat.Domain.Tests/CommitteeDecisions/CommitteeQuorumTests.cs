using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Domain.Tests.CommitteeDecisions;

/// <summary>
/// A panel present for the chair's decision: the chair and one member, the smallest quorum (T165).
/// </summary>
internal static class CommitteeQuorumFixture
{
    public static IReadOnlyCollection<DecisionPanelMember> ChairAndMember =>
    [
        new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair },
        new DecisionPanelMember { UserId = "member-1", Role = DecisionPanelMemberRole.Member }
    ];

    /// <summary>The chair and the external member: an appeal sitting an external member can resolve.</summary>
    public static IReadOnlyCollection<DecisionPanelMember> ChairAndExternal =>
    [
        new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair },
        new DecisionPanelMember { UserId = "external-1", Role = DecisionPanelMemberRole.External }
    ];
}

/// <summary>
/// A committee decision is taken by at least two panel members, the chair and one other, recorded as present; and a
/// decision whose attendance falls short is not ratified. (T165, D46)
/// </summary>
public sealed class CommitteeQuorumTests
{
    private static readonly DateTime Now = new(2026, 9, 24, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void RecordingADecision_KeepsWhoWasPresent_WithTheRoleEachHeld()
    {
        var review = StartedReview();
        var external = new DecisionPanelMember { UserId = "external-1", Role = DecisionPanelMemberRole.External };

        review.RecordDecision(
            CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, "chair-1", Now,
            [Chair(), Member("member-1"), external]);

        Assert.Equal(CommitteeReviewState.Decided, review.State);
        var decision = Assert.Single(review.Decisions);
        Assert.Equal(
            ["chair-1:Chair", "external-1:External", "member-1:Member"],
            decision.Attendees.Select(attendee => $"{attendee.UserId}:{attendee.Role}").Order().ToArray());
        Assert.Null(review.QuorumShortfall());

        // A snapshot of the sitting: the panel changing afterwards does not rewrite who was there.
        external.Role = DecisionPanelMemberRole.Chair;
        Assert.Equal(DecisionPanelMemberRole.External, decision.Attendees.Single(attendee => attendee.UserId == "external-1").Role);
    }

    public static TheoryData<string, string[]> AttendanceWithoutAQuorum => new()
    {
        { "the chair alone", ["chair-1"] },
        { "nobody", [] },
        { "two members without the chair", ["member-1", "member-2"] },
        { "the chair twice", ["chair-1", "chair-1"] }
    };

    [Theory]
    [MemberData(nameof(AttendanceWithoutAQuorum))]
    public void RecordingADecision_WithoutAQuorumPresent_IsRefused_AndChangesNothing(string because, string[] present)
    {
        var review = StartedReview();
        var members = present.Select(userId => userId == "chair-1" ? Chair() : Member(userId)).ToArray();

        Assert.Throws<InvalidOperationException>(() => review.RecordDecision(
            CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, "chair-1", Now, members));

        Assert.True(review.State == CommitteeReviewState.InProgress, because);
        Assert.Empty(review.Decisions);
    }

    [Fact]
    public void RecordingADecision_ByAChairWhoIsNotRecordedAsPresent_IsRefused()
    {
        // Two members are present, and a chair is among them, but not the one recording: whoever records a decision sat.
        var review = StartedReview();

        var refusal = Assert.Throws<InvalidOperationException>(() => review.RecordDecision(
            CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, "chair-2", Now,
            [Chair(), Member("member-1")]));

        Assert.Contains("recording the decision must be recorded as present", refusal.Message, StringComparison.Ordinal);
        Assert.Empty(review.Decisions);
    }

    [Fact]
    public void RecordingADecision_WithAnEmptyRationale_ChangesNothing()
    {
        // The decision, and the attendance it carries, is built before the review is touched, so a refused decision
        // leaves nothing behind.
        var review = StartedReview();

        Assert.Throws<InvalidOperationException>(() => review.RecordDecision(
            CommitteeDecisionCategory.SatisfactoryProgress, " ", null, "chair-1", Now, CommitteeQuorumFixture.ChairAndMember));

        Assert.Empty(review.Decisions);
        Assert.Equal(CommitteeReviewState.InProgress, review.State);
    }

    [Theory]
    [InlineData(new string[0], "Nobody was recorded as present")]
    [InlineData(new[] { "chair-1" }, "Only the chair was recorded as present")]
    [InlineData(new[] { "member-1", "member-2" }, "The chair was not recorded as present")]
    public void Ratifying_ADecisionWhoseAttendanceFallsShort_IsRefused_AndStampsNothing(string[] attended, string reason)
    {
        // A decision recorded before T165 has no attendance; the others cannot be recorded through RecordDecision at all,
        // so the attendance is rewritten after a quorate recording to stand for them.
        var review = DecidedReview();
        var attendees = review.GetCurrentDecision()!.Attendees;
        attendees.Clear();
        foreach (var userId in attended)
        {
            attendees.Add(new CommitteeDecisionAttendee
            {
                UserId = userId,
                Role = userId == "chair-1" ? DecisionPanelMemberRole.Chair : DecisionPanelMemberRole.Member
            });
        }

        var refusal = Assert.Throws<InvalidOperationException>(() => review.Ratify("chair-1", Now));

        Assert.StartsWith(reason, refusal.Message, StringComparison.Ordinal);
        Assert.EndsWith(CommitteeReview.QuorumRule, refusal.Message, StringComparison.Ordinal);
        Assert.Equal(refusal.Message, review.QuorumShortfall());
        Assert.Equal(CommitteeReviewState.Decided, review.State);
        Assert.Null(review.RatifiedOn);
        Assert.Null(review.RatifiedByUserId);
    }

    [Fact]
    public void Ratifying_ADecisionTakenByAQuorum_Succeeds()
    {
        var review = DecidedReview();

        review.Ratify("chair-1", Now);

        Assert.Equal(CommitteeReviewState.Ratified, review.State);
        Assert.Equal("chair-1", review.RatifiedByUserId);
    }

    // ─── An appeal that remits the decision ──────────────────────────────────

    [Fact]
    public void EachDecision_KeepsItsOwnSitting_AndTheCurrentOneIsWhatIsHeldToTheQuorum()
    {
        // The review's sitting took the first decision; the appeal's sitting, the replacement. Before T165 attendance was
        // the review's, so the replacement was shown as taken by the first sitting.
        var review = AppealedReview();

        review.ResolveAppeal(
            CommitteeAppealOutcome.Remitted, "external-1", Now.AddDays(30), CommitteeDecisionCategory.SatisfactoryProgress,
            "Conditions lifted on appeal.", null, CommitteeQuorumFixture.ChairAndExternal);

        Assert.Equal(CommitteeReviewState.Final, review.State);
        var replacement = review.GetCurrentDecision()!;
        var original = review.Decisions.Single(decision => decision != replacement);
        Assert.Equal(["chair-1:Chair", "external-1:External"], Sitting(replacement));
        Assert.Equal(["chair-1:Chair", "member-1:Member"], Sitting(original));
        Assert.Equal("external-1", replacement.DecidedByChairUserId);
        Assert.Null(review.QuorumShortfall());
    }

    [Fact]
    public void TheQuorumThatCounts_IsTheCurrentDecisionsOwn()
    {
        // Two decisions, two sittings. Whether the review stands on a quorum is the current decision's attendance alone:
        // the first sitting's, whatever it held, says nothing about the replacement, and the reverse.
        var review = AppealedReview();
        review.ResolveAppeal(
            CommitteeAppealOutcome.Remitted, "external-1", Now.AddDays(30), CommitteeDecisionCategory.SatisfactoryProgress,
            "Conditions lifted on appeal.", null, CommitteeQuorumFixture.ChairAndExternal);
        var replacement = review.GetCurrentDecision()!;
        var original = review.Decisions.Single(decision => decision != replacement);

        original.Attendees.Clear();
        Assert.Null(review.QuorumShortfall());

        replacement.Attendees.Clear();
        Assert.StartsWith("Nobody was recorded as present", review.QuorumShortfall(), StringComparison.Ordinal);
    }

    public static TheoryData<string, string[]?, string> RemittalsWithoutAQuorum => new()
    {
        { "no attendance at all", null, CommitteeReview.QuorumRule },
        { "the resolver alone", ["external-1"], CommitteeReview.QuorumRule },
        { "without the chair", ["external-1", "member-1"], "The chair must be recorded as present. " + CommitteeReview.QuorumRule },
        { "without the resolver", ["chair-1", "member-1"], "Whoever records the decision must be recorded as present." },
        { "someone twice", ["chair-1", "external-1", "external-1"], "A panel member is recorded as present more than once." }
    };

    [Theory]
    [MemberData(nameof(RemittalsWithoutAQuorum))]
    public void RemittingAnAppeal_WithoutAQuorumPresent_IsRefused_AndLeavesTheAppealOpen(
        string because, string[]? present, string message)
    {
        var review = AppealedReview();

        var refusal = Assert.Throws<InvalidOperationException>(() => review.ResolveAppeal(
            CommitteeAppealOutcome.Remitted, "external-1", Now, CommitteeDecisionCategory.SatisfactoryProgress,
            "Conditions lifted on appeal.", null, present?.Select(Seat).ToArray()));

        Assert.Equal(message, refusal.Message);
        Assert.True(review.State == CommitteeReviewState.UnderAppeal, because);
        Assert.Single(review.Decisions);
        Assert.Null(review.Appeals.Single().ResolvedOn);
        Assert.Null(review.Appeals.Single().Outcome);
    }

    [Theory]
    [InlineData(CommitteeAppealOutcome.Upheld)]
    [InlineData(CommitteeAppealOutcome.Dismissed)]
    public void AnAppealThatLeavesTheDecisionStanding_TakesNoAttendance(CommitteeAppealOutcome outcome)
    {
        // Only a remittal takes a decision; upholding or dismissing leaves the quorate one standing.
        var review = AppealedReview();

        review.ResolveAppeal(outcome, "chair-1", Now);

        Assert.Equal(CommitteeReviewState.Final, review.State);
        Assert.Equal(["chair-1:Chair", "member-1:Member"], Sitting(Assert.Single(review.Decisions)));
    }

    // ─── Panel composition ───────────────────────────────────────────────────

    [Fact]
    public void APanel_OfTheChairAndOneOther_IsComposedAsTheRuleRequires()
    {
        string[] userIds = ["chair-1", "member-1"];
        DecisionPanelMemberRole[] roles = [DecisionPanelMemberRole.Chair, DecisionPanelMemberRole.Member];

        Assert.True(DecisionPanelComposition.NamesEachMemberOnce(userIds));
        Assert.True(DecisionPanelComposition.HasOneChair(roles));
        Assert.True(DecisionPanelComposition.HoldsAQuorum(userIds));
    }

    [Fact]
    public void APanel_OfOneMember_DoesNotHoldAQuorum()
    {
        Assert.False(DecisionPanelComposition.HoldsAQuorum(["chair-1"]));
        Assert.False(DecisionPanelComposition.HoldsAQuorum(["chair-1", " chair-1 "]), "one person named twice is one person");
        Assert.False(DecisionPanelComposition.HoldsAQuorum(["chair-1", " "]), "a blank entry is nobody");
    }

    [Fact]
    public void APanel_NamingSomeoneTwice_IsRefused_HoweverTheIdIsPadded()
    {
        Assert.False(DecisionPanelComposition.NamesEachMemberOnce(["chair-1", "member-1", "member-1 "]));
    }

    [Fact]
    public void APanel_NeedsExactlyOneChair()
    {
        Assert.False(DecisionPanelComposition.HasOneChair([DecisionPanelMemberRole.Member, DecisionPanelMemberRole.External]));
        Assert.False(DecisionPanelComposition.HasOneChair([DecisionPanelMemberRole.Chair, DecisionPanelMemberRole.Chair]));
    }

    private static CommitteeReview StartedReview()
    {
        var review = new CommitteeReview
        {
            AcademicYear = 2026,
            Semester = 1,
            TraineeUserId = "trainee-1",
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 6, 30),
            ScheduledOn = new DateOnly(2026, 7, 2)
        };

        review.Start([], "chair-1", Now);
        return review;
    }

    private static CommitteeReview DecidedReview()
    {
        var review = StartedReview();
        review.RecordDecision(
            CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, "chair-1", Now, CommitteeQuorumFixture.ChairAndMember);
        return review;
    }

    private static CommitteeReview AppealedReview()
    {
        var review = DecidedReview();
        review.Ratify("chair-1", Now);
        review.LodgeAppeal("The conditions are disproportionate.", "trainee-1", Now);
        return review;
    }

    private static string[] Sitting(CommitteeDecision decision)
        => decision.Attendees.Select(attendee => $"{attendee.UserId}:{attendee.Role}").Order().ToArray();

    private static DecisionPanelMember Seat(string userId) => userId switch
    {
        "chair-1" => Chair(),
        "external-1" => new DecisionPanelMember { UserId = userId, Role = DecisionPanelMemberRole.External },
        _ => Member(userId)
    };

    private static DecisionPanelMember Chair() => new() { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair };

    private static DecisionPanelMember Member(string userId) => new() { UserId = userId, Role = DecisionPanelMemberRole.Member };
}
