using System.Security.Claims;
using System.Text.RegularExpressions;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// The trainee's reviews name the period each review sat for, then its evidence window, as the schedule does (T212). The
/// Period column showed only the window, and a semester-2 sitting's window is the whole academic year, so a year's
/// sittings read the same.
/// </summary>
public sealed partial class MyReviewsPeriodTests : TestContext
{
    private const string TraineeId = "trainee-1";

    private readonly Dictionary<int, CommitteeReviewListItemDto> _reviews = new()
    {
        [30] = ListItem(30, semester: 1),
        [31] = ListItem(31, semester: 2)
    };

    public MyReviewsPeriodTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("trainee@test");
        auth.SetRoles(WombatRoles.Trainee);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, TraineeId));
        Services.AddSingleton<IScopedSender>(new Sender(_reviews));
    }

    [Fact]
    public void EachRow_NamesItsPeriod_ThenItsWindow()
    {
        var cut = RenderPage();

        var cells = cut.FindAll("tbody tr").Select(row => Text(row.QuerySelectorAll("td")[1])).ToArray();

        cells.Should().Equal(
            "2026 S1 · 2026-01-01 to 2026-12-31",
            "2026 S2 · 2026-01-01 to 2026-12-31");
        cut.FindAll("thead th").Select(header => header.TextContent.Trim()).Should().Contain("Period");
    }

    [Fact]
    public void TheDetail_SaysWhichPeriodTheReviewSatFor_AndItsWindow()
    {
        var cut = RenderPage();

        cut.FindAll("button").Where(button => button.TextContent.Trim() == "View").ElementAt(1).Click();
        cut.WaitForState(() => cut.Markup.Contains("Review detail"));

        Text(cut.Find("#my-review-period")).Should().Be("Sits for: 2026 S2");
        Text(cut.Find("#my-review-window")).Should().Be("Evidence window: 2026-01-01 to 2026-12-31");
    }

    private IRenderedComponent<MyReviews> RenderPage()
    {
        var cut = RenderComponent<MyReviews>();
        cut.WaitForState(() => cut.FindAll("button").Count(button => button.TextContent.Trim() == "View") == 2);
        return cut;
    }

    // Both sittings of one year, whose evidence windows are the same: the whole year (the S1 window widened by hand).
    private static CommitteeReviewListItemDto ListItem(int id, int semester)
        => new(
            id, TraineeId, 20, "General CCC", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31),
            new DateOnly(2027, 1, 8), CommitteeReviewState.Ratified, null, null)
        {
            AcademicYear = 2026,
            Semester = semester
        };

    private static CommitteeReviewDetailDto Detail(CommitteeReviewListItemDto item)
        => new(
            item.Id, TraineeId, 20, "General CCC", item.ReviewPeriodFrom, item.ReviewPeriodTo, item.ScheduledOn,
            item.State, new DateTime(2027, 1, 8, 9, 0, 0, DateTimeKind.Utc), "chair-1",
            new DateTime(2027, 1, 8, 12, 0, 0, DateTimeKind.Utc), "chair-1", null, [], [], [])
        {
            AcademicYear = item.AcademicYear,
            Semester = item.Semester
        };

    private static string Text(AngleSharp.Dom.IElement element) => Whitespace().Replace(element.TextContent, " ").Trim();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    private sealed class Sender(IReadOnlyDictionary<int, CommitteeReviewListItemDto> reviews) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object response = request switch
            {
                ListReviewsForTraineeQuery => reviews.Values.OrderBy(review => review.Id).ToArray(),
                GetCommitteeReviewByIdQuery query => Detail(reviews[query.ReviewId]),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
