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
/// cell reads "Formative check-in" (T364, C8), never the stored "Annual progression"; a binding review beside it keeps its
/// own type. There is no Mode column (C8), and a check-in's Decision is "—", read as "None" (S7).
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
    public void AFormativeCheckInsTypeCell_ReadsFormativeCheckIn_NotItsStoredType()
    {
        var cut = RenderComponent<ReviewsSchedule>();
        cut.WaitForState(() => cut.FindAll("table tbody tr").Count == 2);

        var headers = cut.FindAll("thead th").Select(th => th.TextContent.Trim()).ToList();
        headers.Should().NotContain("Mode", "a check-in says so in Type (C8)");
        var rows = cut.FindAll("tbody tr")
            .ToDictionary(
                row => row.QuerySelector("a[href^='/committee/reviews/']")!.GetAttribute("href")!,
                row => row.Children.ToArray());
        var type = headers.IndexOf("Type");
        var decision = headers.IndexOf("Decision");

        rows[$"/committee/reviews/{Formative}"][type].TextContent.Trim().Should().Be("Formative check-in");
        rows[$"/committee/reviews/{Binding}"][type].TextContent.Trim().Should().Be("Annual progression");
        var none = rows[$"/committee/reviews/{Formative}"][decision];
        none.QuerySelector("[aria-hidden='true']")!.TextContent.Should().Be("—");
        none.QuerySelector(".visually-hidden")!.TextContent.Should().Be("None");
        rows[$"/committee/reviews/{Binding}"][decision].TextContent.Trim().Should().Be("Pending");
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
