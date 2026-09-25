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
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// Every page that shows a committee review's state prints it in words, "In progress" and "Under appeal", never the enum's
/// name (T250). The schedule, the review page and the trainee's own reviews each printed <c>State</c> as it was, so a panel
/// read "InProgress" and a trainee "UnderAppeal".
/// </summary>
public sealed partial class CommitteeReviewStateLabelTests : TestContext
{
    private const string TraineeId = "trainee-1";

    private readonly TestAuthorizationContext _auth;

    public CommitteeReviewStateLabelTests()
    {
        _auth = this.AddTestAuthorization();
    }

    [Fact]
    public void TheSchedule_PrintsEachReviewsStateInWords()
    {
        SignIn(WombatRoles.Coordinator, "coordinator-a");
        CommitteeReviewListItemDto[] reviews =
        [
            ListItem(40, CommitteeReviewState.Scheduled),
            ListItem(41, CommitteeReviewState.InProgress),
            ListItem(42, CommitteeReviewState.Decided),
            ListItem(43, CommitteeReviewState.Ratified),
            ListItem(44, CommitteeReviewState.UnderAppeal),
            ListItem(45, CommitteeReviewState.Final)
        ];
        Services.AddSingleton<IScopedSender>(new PageSender(reviews));

        var cut = RenderComponent<ReviewsSchedule>();
        cut.WaitForState(() => cut.FindAll("tbody tr").Count == reviews.Length);

        var stateColumn = Column(cut, "State");
        cut.FindAll("tbody tr").Select(row => Text(row.QuerySelectorAll("td")[stateColumn])).Should().Equal(
            "Scheduled", "In progress", "Decided", "Ratified", "Under appeal", "Closed");
        cut.Markup.Should().NotContain("InProgress").And.NotContain("UnderAppeal");
    }

    [Fact]
    public void TheTraineesOwnReviews_PrintEachStateInWords_InTheListAndTheDetail()
    {
        SignIn(WombatRoles.Trainee, TraineeId);
        CommitteeReviewListItemDto[] reviews =
        [
            ListItem(50, CommitteeReviewState.Ratified),
            ListItem(51, CommitteeReviewState.UnderAppeal),
            ListItem(52, CommitteeReviewState.Final)
        ];
        Services.AddSingleton<IScopedSender>(new PageSender(reviews));

        var cut = RenderComponent<MyReviews>();
        cut.WaitForState(() => cut.FindAll("tbody tr").Count == reviews.Length);

        var stateColumn = Column(cut, "State");
        cut.FindAll("tbody tr").Select(row => Text(row.QuerySelectorAll("td")[stateColumn])).Should().Equal(
            "Ratified", "Under appeal", "Closed");

        cut.FindAll("button").Where(button => button.TextContent.Trim() == "View").ElementAt(1).Click();
        cut.WaitForState(() => cut.FindAll("#my-review-state").Count == 1);
        Text(cut.Find("#my-review-state")).Should().Be("State: Under appeal");
        cut.Markup.Should().NotContain("UnderAppeal");
    }

    [Theory]
    [InlineData(CommitteeReviewState.Scheduled, "Scheduled")]
    [InlineData(CommitteeReviewState.InProgress, "In progress")]
    [InlineData(CommitteeReviewState.Decided, "Decided")]
    [InlineData(CommitteeReviewState.Ratified, "Ratified")]
    [InlineData(CommitteeReviewState.UnderAppeal, "Under appeal")]
    [InlineData(CommitteeReviewState.Final, "Closed")]
    public void TheReviewPage_PrintsItsStateInWords(CommitteeReviewState state, string label)
    {
        SignIn(WombatRoles.CommitteeMember, "chair-1");
        Services.AddSingleton<IScopedSender>(new PageSender([ListItem(60, state)]));

        var cut = RenderComponent<ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, 60));
        cut.WaitForState(() => cut.FindAll("#review-state").Count == 1);

        Text(cut.Find("#review-state")).Should().Be(label);
    }

    private void SignIn(string role, string userId)
    {
        _auth.SetAuthorized($"{userId}@test");
        _auth.SetRoles(role);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, userId));
    }

    private static int Column(IRenderedFragment cut, string header)
        => cut.FindAll("thead th").Select(cell => cell.TextContent.Trim()).ToList().IndexOf(header);

    private static CommitteeReviewListItemDto ListItem(int id, CommitteeReviewState state)
        => new(
            id, TraineeId, 20, "General CCC", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30),
            new DateOnly(2026, 7, 2), state, null, null)
        {
            AcademicYear = 2026,
            Semester = 1,
            TraineeName = "Palesa Paeds"
        };

    private static CommitteeReviewDetailDto Detail(CommitteeReviewListItemDto item)
    {
        var started = item.State != CommitteeReviewState.Scheduled;

        return new CommitteeReviewDetailDto(
            item.Id, TraineeId, item.PanelId, item.PanelName, item.ReviewPeriodFrom, item.ReviewPeriodTo, item.ScheduledOn,
            item.State,
            started ? new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc) : null,
            started ? "chair-1" : null,
            null, null, null, [], [], [])
        {
            AcademicYear = item.AcademicYear,
            Semester = item.Semester,
            TraineeName = item.TraineeName
        };
    }

    private static string Text(AngleSharp.Dom.IElement element) => Whitespace().Replace(element.TextContent, " ").Trim();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    /// <summary>Answers what the schedule, the trainee's reviews and the review page ask for on load.</summary>
    private sealed class PageSender(IReadOnlyList<CommitteeReviewListItemDto> reviews) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object? answer = request switch
            {
                ListDecisionPanelsQuery => Array.Empty<DecisionPanelSummaryDto>(),
                GetCommitteeReviewsAccessQuery => new CommitteeReviewsAccessDto(MaySchedule: true, TraineeNote: null),
                ListReviewsForPanelQuery => reviews.ToArray(),
                ListReviewsForTraineeQuery => reviews.ToArray(),
                GetCommitteeReviewByIdQuery query => Detail(reviews.Single(review => review.Id == query.ReviewId)),
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

            return Task.FromResult((TResponse)answer!);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
