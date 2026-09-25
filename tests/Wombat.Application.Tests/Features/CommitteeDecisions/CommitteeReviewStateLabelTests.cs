using FluentAssertions;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Tests.Features.CommitteeDecisions;

/// <summary>
/// One label per committee review state, in one place (<see cref="CommitteeDecisionWording.StateLabel" />), which every
/// surface prints: the schedule, the review page, the trainee's own reviews and the portfolio PDF (T250). Before T250 each
/// printed the enum's name, so "InProgress" and "UnderAppeal" read as code.
/// </summary>
public sealed class CommitteeReviewStateLabelTests
{
    private static readonly Dictionary<CommitteeReviewState, string> Labels = new()
    {
        [CommitteeReviewState.Scheduled] = "Scheduled",
        [CommitteeReviewState.InProgress] = "In progress",
        [CommitteeReviewState.Decided] = "Decided",
        [CommitteeReviewState.Ratified] = "Ratified",
        [CommitteeReviewState.UnderAppeal] = "Under appeal",
        [CommitteeReviewState.Final] = "Closed"
    };

    [Fact]
    public void EveryState_HasItsLabel()
    {
        // Exhaustive over the enum, so a state added without a label here fails rather than printing its name.
        Enum.GetValues<CommitteeReviewState>()
            .ToDictionary(state => state, CommitteeDecisionWording.StateLabel)
            .Should().Equal(Labels);
    }

    [Fact]
    public void TheListAndDetailDtos_CarryTheLabel()
    {
        foreach (var (state, label) in Labels)
        {
            var item = new CommitteeReviewListItemDto(
                1, "trainee-1", 2, "General CCC", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30),
                new DateOnly(2026, 7, 2), state, null, null)
            {
                AcademicYear = 2026,
                Semester = 1
            };
            var detail = new CommitteeReviewDetailDto(
                1, "trainee-1", 2, "General CCC", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30),
                new DateOnly(2026, 7, 2), state, null, null, null, null, null, [], [], [])
            {
                AcademicYear = 2026,
                Semester = 1
            };

            item.StateLabel.Should().Be(label);
            detail.StateLabel.Should().Be(label);
        }
    }

    [Theory]
    [InlineData(CommitteeReviewState.Scheduled, "scheduled")]
    [InlineData(CommitteeReviewState.InProgress, "in progress")]
    [InlineData(CommitteeReviewState.Decided, "decided, not yet ratified")]
    [InlineData(CommitteeReviewState.Ratified, "ratified")]
    [InlineData(CommitteeReviewState.UnderAppeal, "under appeal")]
    [InlineData(CommitteeReviewState.Final, "closed")]
    public void ARefusalSaysTheStateMidSentence_FromTheSameLabels(CommitteeReviewState state, string words)
    {
        CommitteeDecisionWording.StateInSentence(state).Should().Be(words);
    }
}
