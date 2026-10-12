using FluentAssertions;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// T361 (flow 07; round 2 review 9): the addresses between a review and flow 03's activity page, built and read in one
/// place, so the review page (lane P) and the activity page link the same way.
/// </summary>
public sealed class ReviewLinksTests
{
    [Fact]
    public void TheAddresses()
    {
        ReviewLinks.Review(30).Should().Be("/committee/reviews/30");
        ReviewLinks.SnapshotLine(30, 412).Should().Be("/committee/reviews/30#snap-412");
        ReviewLinks.SnapshotLineId(412).Should().Be("snap-412");
        ReviewLinks.ActivityFromReview(12, 30, 412).Should().Be("/activities/12?from=review-30&at=snap-412");
    }

    [Fact]
    public void WhatItBuilds_ItReadsBack()
    {
        ReviewLinks.TryReadReturn("review-30", "snap-412", out var reviewId, out var itemId).Should().BeTrue();
        reviewId.Should().Be(30);
        itemId.Should().Be(412);
    }

    [Theory]
    [InlineData("snap-")]
    [InlineData("snap-x")]
    [InlineData("snap--4")]
    [InlineData("snap-0")]
    [InlineData("412")]
    [InlineData(null)]
    public void ALineNotOfItsForm_IsNoLine_ButTheReviewStillReads(string? at)
    {
        ReviewLinks.TryReadReturn("review-30", at, out var reviewId, out var itemId).Should().BeTrue();
        reviewId.Should().Be(30);
        itemId.Should().BeNull();
    }

    [Theory]
    [InlineData("review-")]
    [InlineData("review-3x")]
    [InlineData("review-+30")]
    [InlineData("review- 30")]
    [InlineData("review-30 ")]
    [InlineData("review-٣٠")]
    [InlineData("review-0")]
    [InlineData("review-1234567890")]
    [InlineData("Review-30")]
    [InlineData("30")]
    [InlineData(null)]
    public void AReviewNotOfItsForm_ReadsAsNone(string? from)
    {
        ReviewLinks.TryReadReturn(from, "snap-412", out _, out var itemId).Should().BeFalse();
        itemId.Should().BeNull();
    }
}
