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
using Wombat.Web.Tests.Accessibility;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// T239: on the trainee's reviews, each row's View is named by the review it opens, and the column of Views has a header.
/// Every View read "View" to a screen reader, and the column's header was an empty cell.
/// </summary>
public sealed class MyReviewsRowActionTests : TestContext
{
    private const string TraineeId = "trainee-1";

    // Both sittings of a year; a formative review beside the binding one for the same period and panel; and a second
    // binding review for that period, scheduled once the first was ratified.
    private readonly CommitteeReviewListItemDto[] _reviews =
    [
        ListItem(30, "General CCC", semester: 1, new DateOnly(2026, 6, 20)),
        ListItem(31, "General CCC", semester: 2, new DateOnly(2026, 11, 2)),
        ListItem(32, "General CCC", semester: 2, new DateOnly(2026, 9, 1), formative: true),
        ListItem(33, "General CCC", semester: 2, new DateOnly(2027, 1, 8)),
        ListItem(34, "Neonatal CCC", semester: 2, new DateOnly(2026, 11, 3))
    ];

    public MyReviewsRowActionTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("trainee@test");
        auth.SetRoles(WombatRoles.Trainee);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, TraineeId));
        Services.AddSingleton<IScopedSender>(new Sender(_reviews));
    }

    [Fact]
    public void EachView_IsNamedByTheReviewItOpens_AndNoTwoReadTheSame()
    {
        var cut = RenderPage();

        var names = Views(cut).Select(view => AccessibleNames.NameOf(cut, view)).ToList();

        names.Should().Equal(
            "View the 2026 S1 review before General CCC",
            "View the 2026 S2 review before General CCC, scheduled 2026-11-02",
            "View the 2026 S2 formative review before General CCC",
            "View the 2026 S2 review before General CCC, scheduled 2027-01-08",
            "View the 2026 S2 review before Neonatal CCC");
        names.Should().OnlyHaveUniqueItems();
        names.Should().OnlyContain(name => name.StartsWith("View ", StringComparison.Ordinal),
            "the name starts with the visible label, so a speech-input user can say what they see");
    }

    [Fact]
    public void TheActionsColumn_HasAHeader_ThatOnlyAScreenReaderReads()
    {
        var cut = RenderPage();

        var headers = cut.FindAll("thead th");
        headers.Should().HaveCount(cut.Find("tbody tr").QuerySelectorAll("td").Length, "every column has a header");
        headers.Should().OnlyContain(header => header.TextContent.Trim().Length > 0, "a header names the cells under it");

        var actions = headers.Last();
        actions.TextContent.Trim().Should().Be("Actions");
        actions.QuerySelector("span.visually-hidden").Should().NotBeNull("the column's buttons say what they are to a sighted user");
    }

    [Fact]
    public void EachView_SitsInAnActionsCluster_InsideItsCell()
    {
        // DESIGN.md § Table system: a cell made a flex box is no longer a table cell, and its border no longer meets its
        // row's.
        var cut = RenderPage();

        Views(cut).Should().OnlyContain(view => view.ParentElement!.ClassList.Contains("actions-cell")
                                                && view.ParentElement!.ParentElement!.LocalName == "td");
        cut.FindAll("td.actions-cell").Should().BeEmpty();
    }

    [Fact]
    public void AView_StillOpensItsReview()
    {
        var cut = RenderPage();

        Views(cut).Single(view => view.GetAttribute("aria-label") == "View the 2026 S2 review before Neonatal CCC").Click();
        cut.WaitForState(() => cut.Markup.Contains("Review detail"));

        cut.Find("section.detail-card p").TextContent.Should().Contain("Neonatal CCC");
    }

    private IRenderedComponent<MyReviews> RenderPage()
    {
        var cut = RenderComponent<MyReviews>();
        cut.WaitForState(() => Views(cut).Count == _reviews.Length);
        return cut;
    }

    private static List<AngleSharp.Dom.IElement> Views(IRenderedFragment cut)
        => cut.FindAll("tbody button").Where(button => button.TextContent.Trim() == "View").ToList();

    private static CommitteeReviewListItemDto ListItem(
        int id, string panel, int semester, DateOnly scheduledOn, bool formative = false)
        => new(
            id, TraineeId, 20, panel, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31),
            scheduledOn, formative ? CommitteeReviewState.Final : CommitteeReviewState.Ratified, null, null, formative)
        {
            AcademicYear = 2026,
            Semester = semester
        };

    private static CommitteeReviewDetailDto Detail(CommitteeReviewListItemDto item)
        => new(
            item.Id, TraineeId, 20, item.PanelName, item.ReviewPeriodFrom, item.ReviewPeriodTo, item.ScheduledOn,
            item.State, new DateTime(2027, 1, 8, 9, 0, 0, DateTimeKind.Utc), "chair-1",
            new DateTime(2027, 1, 8, 12, 0, 0, DateTimeKind.Utc), "chair-1", null, [], [], [])
        {
            AcademicYear = item.AcademicYear,
            Semester = item.Semester
        };

    private sealed class Sender(IReadOnlyList<CommitteeReviewListItemDto> reviews) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object response = request switch
            {
                ListReviewsForTraineeQuery => reviews.ToArray(),
                GetCommitteeReviewByIdQuery query => Detail(reviews.Single(review => review.Id == query.ReviewId)),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
