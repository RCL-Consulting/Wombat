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
/// The trainee's review list says who took the current decision, by name: that decision's own sitting, and still by name
/// after the trainee lodges an appeal. (T165, T142)
/// </summary>
public sealed partial class MyReviewsAttendanceTests : TestContext
{
    private const int ReviewId = 30;

    public MyReviewsAttendanceTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("trainee@test");
        auth.SetRoles(WombatRoles.Trainee);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "trainee-1"));
    }

    [Fact]
    public void TheCurrentDecision_NamesItsOwnSitting()
    {
        // A remitted review: the replacement is current, and the appeal's sitting took it.
        var review = Review(CommitteeReviewState.Final, named: true) with
        {
            Decisions =
            [
                Decision(42, "chair-1:Thandi Zulu:Chair", "external-1:Anna Botha:External"),
                Decision(41, "chair-1:Thandi Zulu:Chair", "member-1:Priya Naidoo:Member")
            ]
        };

        var cut = Open(new FakeSender(review));

        Present(cut).Should().Be("Present when this decision was taken: Thandi Zulu (chair), Anna Botha (external)");
    }

    [Fact]
    public void AfterLodgingAnAppeal_ThePageStillNamesWhoWasPresent()
    {
        // The command's answer comes from the lookup-free mapper and names nobody. Before the fix the page showed it as it
        // came, so the Present line read as raw user ids (T142's rule, broken).
        var sender = new FakeSender(Review(CommitteeReviewState.Ratified, named: true));
        var cut = Open(sender);

        cut.Find("#trainee-appeal-reason").Change("The window missed my rotation.");
        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Lodge appeal").Click();
        cut.WaitForState(() => cut.Markup.Contains("Appeal lodged."));

        Present(cut).Should().Be("Present when this decision was taken: Thandi Zulu (chair), Priya Naidoo");
    }

    private IRenderedComponent<MyReviews> Open(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<MyReviews>();
        cut.WaitForState(() => cut.FindAll("button").Any(button => button.TextContent.Trim() == "View"));
        cut.FindAll("button").Single(button => button.TextContent.Trim() == "View").Click();
        cut.WaitForState(() => cut.FindAll("#current-decision-present").Count == 1);
        return cut;
    }

    private static string Present(IRenderedComponent<MyReviews> cut)
        => Whitespace().Replace(cut.Find("#current-decision-present").TextContent, " ").Trim();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    /// <param name="attendees">"id:Name:Role" each.</param>
    private static CommitteeDecisionDto Decision(int id, params string[] attendees)
        => new(id, CommitteeDecisionCategory.SatisfactoryProgress, $"Decision {id}.", null,
            new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc).AddDays(id), "chair-1", null)
        {
            Attendees = attendees
                .Select(attendee => attendee.Split(':'))
                .Select(parts => new CommitteePersonDto(parts[0], Enum.Parse<DecisionPanelMemberRole>(parts[2])) { Name = parts[1] })
                .ToArray()
        };

    private static CommitteeReviewDetailDto Review(CommitteeReviewState state, bool named)
    {
        var review = new CommitteeReviewDetailDto(
            ReviewId, "trainee-1", 20, "Paediatrics CCC", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30),
            new DateOnly(2026, 7, 2), state, null, null, null, null, null,
            [Decision(41, "chair-1:Thandi Zulu:Chair", "member-1:Priya Naidoo:Member")], [], []);

        return named
            ? review with { TraineeName = "Lerato Molefe" }
            : review with
            {
                Decisions = review.Decisions
                    .Select(decision => decision with
                    {
                        Attendees = decision.Attendees.Select(person => person with { Name = null }).ToArray()
                    })
                    .ToArray()
            };
    }

    private sealed class FakeSender(CommitteeReviewDetailDto review) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object? response = request switch
            {
                ListReviewsForTraineeQuery => new[]
                {
                    new CommitteeReviewListItemDto(
                        ReviewId, "trainee-1", 20, "Paediatrics CCC", review.ReviewPeriodFrom, review.ReviewPeriodTo,
                        review.ScheduledOn, review.State, CommitteeDecisionCategory.SatisfactoryProgress, null)
                },
                GetCommitteeReviewByIdQuery => review,
                // As the shared mapper answers: nobody named.
                LodgeAppealCommand => Review(CommitteeReviewState.UnderAppeal, named: false),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response!);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
