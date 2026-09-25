using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Domain.Tests.CommitteeDecisions;

/// <summary>
/// T258: a review still open (scheduled, in progress, or decided and not yet ratified) can be withdrawn, recording when and
/// why, and nothing more is decided at it. A review whose decision is ratified is settled and is refused, changing nothing.
/// </summary>
public sealed class CommitteeReviewWithdrawalTests
{
    private static readonly DateTime WithdrawnAt = new(2026, 9, 25, 8, 30, 0, DateTimeKind.Utc);

    private static CommitteeReview Scheduled(bool formative = false) => new()
    {
        AcademicYear = 2026,
        Semester = 1,
        TraineeUserId = "trainee-1",
        ReviewPeriodFrom = new DateOnly(2026, 1, 1),
        ReviewPeriodTo = new DateOnly(2026, 6, 30),
        ScheduledOn = new DateOnly(2026, 7, 2),
        IsFormative = formative
    };

    private static CommitteeReview InProgress(bool formative = false)
    {
        var review = Scheduled(formative);
        review.Start([], "chair-1", DateTime.UtcNow);
        return review;
    }

    private static CommitteeReview Decided()
    {
        var review = InProgress();
        review.RecordDecision(
            CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, "chair-1", DateTime.UtcNow,
            CommitteeQuorumFixture.ChairAndMember, [], []);
        return review;
    }

    private static CommitteeReview Ratified()
    {
        var review = Decided();
        review.Ratify("chair-1", DateTime.UtcNow);
        return review;
    }

    public static TheoryData<string> OpenReviews => new() { "scheduled", "in progress", "decided", "formative scheduled", "formative in progress" };

    private static CommitteeReview Open(string which) => which switch
    {
        "scheduled" => Scheduled(),
        "in progress" => InProgress(),
        "decided" => Decided(),
        "formative scheduled" => Scheduled(formative: true),
        "formative in progress" => InProgress(formative: true),
        _ => throw new ArgumentOutOfRangeException(nameof(which))
    };

    [Theory]
    [MemberData(nameof(OpenReviews))]
    public void Withdraw_AnOpenReview_RecordsWhenAndWhy_AndEndsIt(string which)
    {
        var review = Open(which);
        Assert.Contains(review.State, CommitteeReview.OpenStates);

        review.Withdraw("  " + CommitteeReview.WithdrawnTraineeErased + "  ", WithdrawnAt);

        Assert.Equal(CommitteeReviewState.Withdrawn, review.State);
        Assert.Equal(WithdrawnAt, review.WithdrawnOn);
        Assert.Equal(CommitteeReview.WithdrawnTraineeErased, review.WithdrawalReason);
        Assert.DoesNotContain(review.State, CommitteeReview.OpenStates);
    }

    [Fact]
    public void Withdraw_ADecidedReview_KeepsItsUnratifiedDecision_AndRatifyingItIsRefused()
    {
        var review = Decided();

        review.Withdraw(CommitteeReview.WithdrawnTraineeErased, WithdrawnAt);

        Assert.NotNull(review.GetCurrentDecision());
        Assert.Null(review.RatifiedOn);
        Assert.Throws<InvalidOperationException>(() => review.Ratify("chair-1", DateTime.UtcNow));
        Assert.Equal(CommitteeReviewState.Withdrawn, review.State);
    }

    [Fact]
    public void AWithdrawnReview_CannotBeStarted()
    {
        var review = Scheduled();
        review.Withdraw(CommitteeReview.WithdrawnTraineeErased, WithdrawnAt);

        Assert.Throws<InvalidOperationException>(() => review.Start([], "chair-1", DateTime.UtcNow));
        Assert.Equal(CommitteeReviewState.Withdrawn, review.State);
    }

    public static TheoryData<string> SettledReviews => new() { "ratified", "under appeal", "final", "withdrawn" };

    private static CommitteeReview Settled(string which)
    {
        switch (which)
        {
            case "ratified":
                return Ratified();
            case "under appeal":
            {
                var review = Ratified();
                review.LodgeAppeal("The evidence was misread.", "trainee-1", DateTime.UtcNow);
                return review;
            }
            case "final":
            {
                var review = InProgress(formative: true);
                review.Close("chair-1", DateTime.UtcNow);
                return review;
            }
            case "withdrawn":
            {
                var review = Scheduled();
                review.Withdraw("Withdrawn once.", WithdrawnAt);
                return review;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(which));
        }
    }

    [Theory]
    [MemberData(nameof(SettledReviews))]
    public void Withdraw_ASettledReview_IsRefused_AndChangesNothing(string which)
    {
        var review = Settled(which);
        var state = review.State;
        var withdrawnOn = review.WithdrawnOn;
        var reason = review.WithdrawalReason;

        var exception = Assert.Throws<InvalidOperationException>(
            () => review.Withdraw(CommitteeReview.WithdrawnTraineeErased, WithdrawnAt.AddDays(1)));

        Assert.Contains("can be withdrawn", exception.Message, StringComparison.Ordinal);
        Assert.Equal(state, review.State);
        Assert.Equal(withdrawnOn, review.WithdrawnOn);
        Assert.Equal(reason, review.WithdrawalReason);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Withdraw_WithNoReason_IsRefused_AndChangesNothing(string reason)
    {
        var review = Scheduled();

        Assert.Throws<InvalidOperationException>(() => review.Withdraw(reason, WithdrawnAt));

        Assert.Equal(CommitteeReviewState.Scheduled, review.State);
        Assert.Null(review.WithdrawnOn);
        Assert.Null(review.WithdrawalReason);
    }

    [Fact]
    public void Withdraw_WithAReasonLongerThanIsStored_IsRefused_AndChangesNothing()
    {
        var review = Scheduled();

        Assert.Throws<InvalidOperationException>(
            () => review.Withdraw(new string('x', CommitteeReview.WithdrawalReasonMaxLength + 1), WithdrawnAt));

        Assert.Equal(CommitteeReviewState.Scheduled, review.State);
        Assert.Null(review.WithdrawnOn);

        review.Withdraw(new string('x', CommitteeReview.WithdrawalReasonMaxLength), WithdrawnAt);
        Assert.Equal(CommitteeReviewState.Withdrawn, review.State);
    }
}
