using System.Security.Claims;
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
/// An empty evidence snapshot says which kind of empty it is. (T138)
/// </summary>
/// <remarks>
/// Before a review starts nothing has been frozen, and the page says starting will capture the bundle. After
/// it starts, an empty snapshot means the window held nothing admissible — since T138 a trainee whose only
/// in-window item is an MSF campaign still under review is exactly that — and the old wording told the panel
/// to press a Start button the page no longer shows.
/// </remarks>
public sealed class ReviewDetailEvidenceEmptyStateTests : TestContext
{
    private const string NotFrozenYet = "No evidence has been frozen yet. Starting the review captures the current bundle.";
    private const string StartedEmpty = "The review window held no evidence when this review started.";

    public ReviewDetailEvidenceEmptyStateTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("chair@test");
        auth.SetRoles("CommitteeMember");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "chair-1"));
    }

    [Fact]
    public void AScheduledReview_SaysStartingWillCaptureTheBundle()
    {
        var cut = RenderPage(Review(CommitteeReviewState.Scheduled, evidence: []));

        cut.Markup.Should().Contain(NotFrozenYet);
        cut.Markup.Should().NotContain(StartedEmpty);
    }

    [Theory]
    [InlineData(CommitteeReviewState.InProgress)]
    [InlineData(CommitteeReviewState.Decided)]
    [InlineData(CommitteeReviewState.Final)]
    public void AStartedReviewWithAnEmptySnapshot_SaysTheWindowHeldNothing(CommitteeReviewState state)
    {
        var cut = RenderPage(Review(state, evidence: []));

        cut.Markup.Should().Contain(StartedEmpty);
        cut.Markup.Should().NotContain(NotFrozenYet, "the review has started; there is no Start button to press");
    }

    [Fact]
    public void AStartedReviewWithEvidence_ListsItAndSaysNeither()
    {
        var evidence = new CommitteeEvidenceDto(
            1,
            CommitteeEvidenceSourceType.Activity,
            100,
            null,
            null,
            "Mini-CEX #100",
            "State: completed.",
            new DateTime(2026, 2, 2, 8, 0, 0, DateTimeKind.Utc));

        var cut = RenderPage(Review(CommitteeReviewState.InProgress, evidence: [evidence]));

        cut.Markup.Should().Contain("Mini-CEX #100");
        cut.Markup.Should().NotContain(NotFrozenYet);
        cut.Markup.Should().NotContain(StartedEmpty);
    }

    private IRenderedComponent<ReviewDetail> RenderPage(CommitteeReviewDetailDto review)
    {
        Services.AddSingleton<IScopedSender>(new FakeSender(review));

        var cut = RenderComponent<ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, review.Id));
        cut.WaitForState(() => cut.Markup.Contains("Evidence snapshot"));

        return cut;
    }

    private static CommitteeReviewDetailDto Review(CommitteeReviewState state, IReadOnlyList<CommitteeEvidenceDto> evidence)
    {
        var started = state != CommitteeReviewState.Scheduled;

        return new CommitteeReviewDetailDto(
            30,
            "trainee-1",
            20,
            "Paediatrics CCC",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 6, 30),
            new DateOnly(2026, 7, 2),
            state,
            started ? new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc) : null,
            started ? "chair-1" : null,
            null,
            null,
            null,
            [],
            [],
            evidence)
        {
            AcademicYear = 2026,
            Semester = 1
        };
    }

    private sealed class FakeSender : IScopedSender
    {
        private readonly CommitteeReviewDetailDto _review;

        public FakeSender(CommitteeReviewDetailDto review) => _review = review;

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object? response = request switch
            {
                GetCommitteeReviewByIdQuery => _review,
                ListPendingEntrustmentDecisionsForReviewQuery => Array.Empty<PendingEntrustmentDecisionDto>(),
                GetSamplingConcentrationWarningsQuery => null,
                CountMsfCampaignsOutsideSnapshotQuery => MsfCampaignsOutsideSnapshotDto.None,
                GetEpaTrajectoryForTraineeQuery => Array.Empty<EpaTrajectoryDto>(),
                GetEntrustmentStandingForTraineeQuery => null,
                GetMsfCoverageForTraineeQuery => null,
                ListStarEpaOptionsForReviewQuery => Array.Empty<StarEpaOptionDto>(),
                GetEntrustmentScalesListQuery => Array.Empty<EntrustmentScaleDto>(),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response!);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
