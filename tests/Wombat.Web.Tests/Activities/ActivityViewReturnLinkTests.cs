using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityById;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Web.Components.Pages.Activities;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T361 (flow 07; round 2 review 9, round 3 item 9): flow 03's activity page opened from a committee review's snapshot
/// offers the review's "Back to Lerato Molefe · 2026 S2" in place of its own back link, and only to a reader the review
/// reads for. A forged, unknown or unreadable id changes nothing and names nobody.
/// </summary>
public sealed class ActivityViewReturnLinkTests : WombatTestContext
{
    private const int ActivityId = 12;
    private const int ReviewId = 30;
    private const int SnapshotItemId = 412;

    private const string SchemaJson = """
        { "version": 1, "sections": [ { "key": "about", "title": "About", "fields": [ { "key": "note", "type": "text", "label": "Note" } ] } ] }
        """;

    private const string WorkflowJson = """
        {
          "version": 1,
          "initial_state": "completed",
          "states": [ { "key": "completed", "label": "Completed", "terminal": true } ],
          "transitions": []
        }
        """;

    private readonly ReturnSender _sender = new();

    public ActivityViewReturnLinkTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("chair@test");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "chair-1"));

        Services.AddSingleton<IActivityReferenceDataService, StubActivityReferenceDataService>();
        Services.AddSingleton<IScopedSender>(_sender);
        Services.AddScoped<ActivityNotices>();
    }

    [Fact]
    public void FromTheReviewsSnapshot_TheReviewsBackLink_ReplacesThePagesOwn_AndLandsOnTheLine()
    {
        var cut = RenderAt(ReviewLinks.ActivityFromReview(ActivityId, ReviewId, SnapshotItemId));

        var back = cut.Find("a.return-link");
        back.TextContent.Trim().Should().Be("Back to Lerato Molefe · 2026 S2");
        back.GetAttribute("href").Should().Be("/committee/reviews/30#snap-412");
        cut.Find(".activity-page").ClassList.Should().Contain("activity-page--from-review", "the phone's back crumb gives way to it");
        cut.FindAll("a").Count(link => link.TextContent.Trim().StartsWith("Back to", StringComparison.Ordinal))
            .Should().Be(1, "one back link, never two");
        _sender.ReviewReads.Should().Equal(ReviewId);
    }

    [Fact]
    public void ALineThatIsNotTheReviews_GoesToTheReviewsTop()
    {
        var cut = RenderAt(ReviewLinks.ActivityFromReview(ActivityId, ReviewId, 999));

        cut.Find("a.return-link").GetAttribute("href").Should().Be("/committee/reviews/30");
    }

    [Fact]
    public void AReaderTheReviewDoesNotReadFor_KeepsThePagesOwnLink_AndNoNameIsShown()
    {
        _sender.ReviewRefused = true;

        var cut = RenderAt(ReviewLinks.ActivityFromReview(ActivityId, ReviewId, SnapshotItemId));

        cut.FindAll("a.return-link").Should().BeEmpty();
        cut.Find(".activity-page").ClassList.Should().NotContain("activity-page--from-review");
        cut.Markup.Should().NotContain("Lerato Molefe").And.NotContain("2026 S2");
        _sender.ReviewReads.Should().Equal(ReviewId);
    }

    [Fact]
    public void AReviewThatCannotBeRead_ForAnyOtherReason_LeavesThePageAsItIs()
    {
        _sender.ReviewFailure = new InvalidOperationException("The database is down.");

        var cut = RenderAt(ReviewLinks.ActivityFromReview(ActivityId, ReviewId, SnapshotItemId));

        cut.FindAll("a.return-link").Should().BeEmpty();
        cut.Markup.Should().NotContain("The database is down.");
    }

    [Theory]
    [InlineData("from=review-3x&at=snap-412")]
    [InlineData("from=review-%2B30&at=snap-412")]
    [InlineData("from=review-%2030&at=snap-412")]
    [InlineData("from=review--30&at=snap-412")]
    [InlineData("from=review-0&at=snap-412")]
    [InlineData("from=review-99999999999&at=snap-412")]
    [InlineData("from=30&at=snap-412")]
    [InlineData("from=REVIEW-30&at=snap-412")]
    [InlineData("at=snap-412")]
    public void AnAddressThatIsNotOurs_AsksForNoReview_AndShowsNoLink(string query)
    {
        var cut = RenderAt($"/activities/{ActivityId}?{query}");

        cut.FindAll("a.return-link").Should().BeEmpty();
        _sender.ReviewReads.Should().BeEmpty("only an id of our own form is ever looked up");
    }

    [Fact]
    public void WithoutTheQuery_ThePageIsUnchanged()
    {
        var cut = RenderAt($"/activities/{ActivityId}");

        cut.FindAll("a.return-link").Should().BeEmpty();
        cut.Find(".activity-page").ClassList.Should().NotContain("activity-page--from-review");
        _sender.ReviewReads.Should().BeEmpty();
    }

    private IRenderedComponent<ActivityView> RenderAt(string address)
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo(address);
        var cut = RenderComponent<ActivityView>(parameters => parameters.Add(page => page.ActivityId, ActivityId));
        cut.WaitForState(() => cut.Markup.Contains("Who has it now"));
        return cut;
    }

    private sealed class ReturnSender : IScopedSender
    {
        public List<int> ReviewReads { get; } = [];

        public bool ReviewRefused { get; set; }

        public Exception? ReviewFailure { get; set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case GetActivityByIdQuery:
                    return Task.FromResult((TResponse)(object)Detail());

                case GetCommitteeReviewByIdQuery query:
                    ReviewReads.Add(query.ReviewId);
                    if (ReviewRefused)
                    {
                        // The review ladder's one refusal, for an unknown id and one out of reach alike (T194 item 1).
                        throw new UnauthorizedAccessException("The committee review could not be found among the reviews you can view.");
                    }

                    if (ReviewFailure is not null)
                    {
                        throw ReviewFailure;
                    }

                    return Task.FromResult((TResponse)(object)Review());

                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        private static CommitteeReviewDetailDto Review()
            => new(
                ReviewId,
                "trainee-1",
                20,
                "Paediatrics CCC",
                new DateOnly(2026, 7, 1),
                new DateOnly(2026, 12, 31),
                new DateOnly(2026, 10, 5),
                CommitteeReviewState.InProgress,
                new DateTime(2026, 10, 5, 12, 0, 0, DateTimeKind.Utc),
                "chair-1",
                null,
                null,
                null,
                [],
                [],
                [new CommitteeEvidenceDto(SnapshotItemId, CommitteeEvidenceSourceType.Activity, ActivityId, null, null,
                    "Mini-CEX (Paediatrics) #12", "State: Completed.", null)])
            {
                AcademicYear = 2026,
                Semester = 2,
                TraineeName = "Lerato Molefe"
            };

        private static ActivityDetailDto Detail()
        {
            var activity = new ActivityDto(
                ActivityId,
                2,
                "mini_cex_cpsa",
                "Mini-CEX (Paediatrics)",
                "mini_cex",
                1,
                SchemaJson,
                WorkflowJson,
                "[]",
                """{ "counts_for": [] }""",
                "trainee-1",
                7,
                "assessor-1",
                "completed",
                "Completed",
                "{}",
                null,
                null,
                new DateOnly(2026, 9, 23),
                true,
                new DateTime(2026, 9, 23, 8, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 9, 23, 8, 0, 0, DateTimeKind.Utc),
                []);

            return new ActivityDetailDto(activity, [], []);
        }
    }
}
