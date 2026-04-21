using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Domain.Tests.CommitteeDecisions;

public sealed class FormativeReviewTests
{
    [Fact]
    public void FormativeReview_Start_ThenClose_TransitionsToFinal()
    {
        var review = new CommitteeReview
        {
            TraineeUserId = "trainee-1",
            IsFormative = true,
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 3, 31),
            ScheduledOn = new DateOnly(2026, 4, 1)
        };

        review.Start(Array.Empty<CommitteeEvidence>(), "chair-1", DateTime.UtcNow);
        Assert.Equal(CommitteeReviewState.InProgress, review.State);

        review.Close("chair-1", DateTime.UtcNow);
        Assert.Equal(CommitteeReviewState.Final, review.State);
        Assert.Equal("chair-1", review.RatifiedByUserId);
    }

    [Fact]
    public void FormativeReview_RecordDecision_Throws()
    {
        var review = new CommitteeReview
        {
            TraineeUserId = "trainee-1",
            IsFormative = true,
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 3, 31),
            ScheduledOn = new DateOnly(2026, 4, 1)
        };

        review.Start(Array.Empty<CommitteeEvidence>(), "chair-1", DateTime.UtcNow);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            review.RecordDecision(CommitteeDecisionCategory.SatisfactoryProgress, "Any.", null, "chair-1", DateTime.UtcNow));

        Assert.Contains("formative", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormativeReview_Ratify_Throws()
    {
        var review = new CommitteeReview
        {
            TraineeUserId = "trainee-1",
            IsFormative = true,
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 3, 31),
            ScheduledOn = new DateOnly(2026, 4, 1)
        };

        review.Start(Array.Empty<CommitteeEvidence>(), "chair-1", DateTime.UtcNow);

        Assert.Throws<InvalidOperationException>(() => review.Ratify("chair-1", DateTime.UtcNow));
    }

    [Fact]
    public void SummativeReview_Close_Throws()
    {
        var review = new CommitteeReview
        {
            TraineeUserId = "trainee-1",
            IsFormative = false,
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 3, 31),
            ScheduledOn = new DateOnly(2026, 4, 1)
        };

        review.Start(Array.Empty<CommitteeEvidence>(), "chair-1", DateTime.UtcNow);

        var exception = Assert.Throws<InvalidOperationException>(() => review.Close("chair-1", DateTime.UtcNow));
        Assert.Contains("formative", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormativeReview_CloseFromScheduled_Throws()
    {
        var review = new CommitteeReview
        {
            TraineeUserId = "trainee-1",
            IsFormative = true,
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 3, 31),
            ScheduledOn = new DateOnly(2026, 4, 1)
        };

        Assert.Throws<InvalidOperationException>(() => review.Close("chair-1", DateTime.UtcNow));
    }
}
