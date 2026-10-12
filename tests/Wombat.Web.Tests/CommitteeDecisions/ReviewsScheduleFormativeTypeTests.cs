using System.Security.Claims;
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
/// T361, review R1 (T309 item 2): a formative check-in decides nothing whatever its stored type, so the schedule's Type
/// cell reads "Formative", never the stored "Annual progression"; a binding review beside it keeps its own type.
/// </summary>
public sealed class ReviewsScheduleFormativeTypeTests : TestContext
{
    private const int Formative = 30;
    private const int Binding = 31;

    public ReviewsScheduleFormativeTypeTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("coordinator@test");
        auth.SetRoles(WombatRoles.Coordinator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "coordinator-a"));

        Services.AddSingleton<IScopedSender>(new ListSender(
        [
            Review(Formative, isFormative: true),
            Review(Binding, isFormative: false)
        ]));
    }

    [Fact]
    public void AFormativeCheckInsTypeCell_ReadsFormative_NotItsStoredType()
    {
        var cut = RenderComponent<ReviewsSchedule>();
        cut.WaitForState(() => cut.FindAll("table tbody tr").Count == 2);

        var typeColumn = cut.FindAll("thead th").Select(th => th.TextContent.Trim()).ToList().IndexOf("Type");
        var types = cut.FindAll("tbody tr")
            .ToDictionary(
                row => row.QuerySelector("a[href^='/committee/reviews/']")!.GetAttribute("href")!,
                row => row.QuerySelectorAll("td")[typeColumn].TextContent.Trim());

        types[$"/committee/reviews/{Formative}"].Should().Be("Formative");
        types[$"/committee/reviews/{Binding}"].Should().Be("Annual progression");
    }

    private static CommitteeReviewListItemDto Review(int id, bool isFormative)
        => new(
            id, "paeds-a", 10, "General CCC", new DateOnly(2026, 7, 1), new DateOnly(2026, 11, 30),
            new DateOnly(2026, 10, 4), CommitteeReviewState.Scheduled, null, null, isFormative,
            CommitteeReviewType.AnnualProgression)
        {
            AcademicYear = 2026,
            Semester = 2,
            TraineeName = "Palesa Paeds"
        };

    /// <summary>Lists the given reviews, no panel, and the access a coordinator has.</summary>
    private sealed class ListSender(IReadOnlyList<CommitteeReviewListItemDto> reviews) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => Task.FromResult((TResponse)(request switch
            {
                GetCommitteeReviewsAccessQuery => (object)new CommitteeReviewsAccessDto(MaySchedule: true, TraineeNote: null),
                ListDecisionPanelsQuery => (object)Array.Empty<DecisionPanelSummaryDto>(),
                ListReviewsForPanelQuery => (object)reviews.ToList(),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            }));

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
    }
}
