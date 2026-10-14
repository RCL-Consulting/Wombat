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
using Wombat.Web.Tests.Accessibility;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// My committee reviews, the list only (T366, flow 09; boards R3-1-*; round 3 F1, M1, S8; T239, T212, T329). Each review
/// opens on her review's page, a link named by the review; State is a badge; the table is hand-written and stacks at 900px;
/// a failed read is fixed words with Try again. The detail that sat under the list is her review's page now
/// (<see cref="MyReviewPageTests" />).
/// </summary>
public sealed partial class MyReviewsListTests : TestContext
{
    private const string TraineeId = "trainee-1";

    public MyReviewsListTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("trainee@test");
        auth.SetRoles(WombatRoles.Trainee);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, TraineeId));
    }

    [Fact]
    public void EachView_LinksToHerReviewsPage_NamedByTheReview_AndNoTwoReadTheSame()
    {
        // Both sittings of a year; a formative review beside the binding one; a second binding review for the same period;
        // another panel's.
        var cut = RenderPage(
            ListItem(33, "General CCC", semester: 2, new DateOnly(2027, 1, 8)),
            ListItem(31, "General CCC", semester: 2, new DateOnly(2026, 11, 2)),
            ListItem(34, "Neonatal CCC", semester: 2, new DateOnly(2026, 11, 3)),
            ListItem(32, "General CCC", semester: 2, new DateOnly(2026, 9, 1), formative: true),
            ListItem(30, "General CCC", semester: 1, new DateOnly(2026, 6, 20)));

        var views = Views(cut);
        views.Select(view => AccessibleNames.NameOf(cut, view)).Should().Equal(
            "View the 2026 S2 review before General CCC, scheduled 2027-01-08",
            "View the 2026 S2 review before General CCC, scheduled 2026-11-02",
            "View the 2026 S2 review before Neonatal CCC",
            "View the 2026 S2 formative review before General CCC",
            "View the 2026 S1 review before General CCC");
        views.Select(view => view.GetAttribute("href")).Should().Equal(
            "/committee/my-reviews/33", "/committee/my-reviews/31", "/committee/my-reviews/34", "/committee/my-reviews/32",
            "/committee/my-reviews/30");
        views.Should().OnlyContain(view => view.LocalName == "a" && view.ClassList.Contains("btn-sm"),
            "View opens a page: a link (DESIGN.md § Button system)");
    }

    [Fact]
    public void TwoReviewsSharingAName_EachAddTheirType_AsDrMolefesPairDoes()
    {
        // M1: Dr Molefe's #7 (pre-graduation) above #1 (annual progression), one period, one panel, one day.
        var cut = RenderPage(
            ListItem(7, "Paed Annual Review Panel", 2, new DateOnly(2026, 10, 13), type: CommitteeReviewType.PreGraduation,
                from: new DateOnly(2023, 1, 15)),
            ListItem(1, "Paed Annual Review Panel", 2, new DateOnly(2026, 10, 13)));

        Views(cut).Select(view => AccessibleNames.NameOf(cut, view)).Should().Equal(
            "View the 2026 S2 review before Paed Annual Review Panel, Pre-graduation",
            "View the 2026 S2 review before Paed Annual Review Panel, Annual progression");
        cut.FindAll("tbody tr").Select(row => Text(row.QuerySelector("th")!)).Should().Equal(
            "2026 S2 2023-01-15 to 2026-12-31", "2026 S2 2026-01-01 to 2026-12-31");
    }

    [Fact]
    public void TheTable_IsHandWritten_Captioned_AndStacksAt900px()
    {
        var cut = RenderPage(ListItem(30, "General CCC", 2, new DateOnly(2026, 10, 13)));

        var table = cut.Find("table");
        table.ClassList.Should().Contain(["clinic-table", "clinic-table--stack", "clinic-table--stack-wide"], "S8");
        Text(table.QuerySelector("caption")!).Should().Be("Your committee reviews, newest first");
        cut.FindAll("thead th").Should().OnlyContain(header => header.GetAttribute("scope") == "col");
        cut.FindAll("thead th").Select(header => Text(header)).Should().Equal("Panel", "Period", "Type", "State", "Decision", "Actions");
        cut.Find("thead th:last-child span.visually-hidden").TextContent.Should().Be("Actions",
            "the column's links say what they are to a sighted user");
        // T366, D11: the period is each row's header, as the trainee is the register's.
        var period = cut.Find("tbody tr th");
        period.GetAttribute("scope").Should().Be("row");
        Text(period).Should().StartWith("2026 S2");
        cut.FindAll("tbody td[data-label]").Select(cell => cell.GetAttribute("data-label")).Should().Equal(
            "Panel", "Type", "State", "Decision");
        Views(cut).Should().OnlyContain(view => view.ParentElement!.ClassList.Contains("actions-cell")
                                                && view.ParentElement!.ParentElement!.LocalName == "td");
        cut.Find("tbody th[scope=row] .period-window").Should().NotBeNull("the window under the period (T212)");
    }

    [Fact]
    public void State_IsABadge_InWords_TintedAsTheReviewPageTintsIt()
    {
        var cut = RenderPage(
            ListItem(52, "General CCC", 2, new DateOnly(2026, 10, 13), state: CommitteeReviewState.Final),
            ListItem(51, "General CCC", 2, new DateOnly(2026, 10, 12), state: CommitteeReviewState.UnderAppeal),
            ListItem(50, "General CCC", 2, new DateOnly(2026, 10, 11)));

        var badges = cut.FindAll("tbody td[data-label='State'] .badge");
        badges.Select(badge => Text(badge)).Should().Equal("Closed", "Under appeal", "Ratified");
        badges.Select(badge => badge.ClassList.Single(name => name.StartsWith("badge-", StringComparison.Ordinal)))
            .Should().Equal("badge-completed", "badge-accepted", "badge-completed");
        cut.Markup.Should().NotContain("UnderAppeal");
    }

    [Fact]
    public void AFormativeCheckIn_ReadsAsOne_WithNoBindingDecision()
    {
        var cut = RenderPage(ListItem(6, "Paed Annual Review Panel", 2, new DateOnly(2026, 10, 13), formative: true));

        var row = cut.Find("tbody tr");
        Text(row.QuerySelector("td[data-label='Type']")!).Should().Be("Formative check-in");
        Text(row.QuerySelector("td[data-label='Decision']")!).Should().Be("No binding decision");
    }

    [Fact]
    public void NoReviews_ReadsTodaysEmptyState()
    {
        var cut = RenderPage();

        cut.Find(".state-panel-title").TextContent.Should().Be("No decisions yet");
        cut.Find(".state-panel-copy").TextContent.Should().Be("Ratified committee outcomes will appear here.");
        cut.FindAll("table").Should().BeEmpty();
    }

    [Fact]
    public void WhileLoading_TheHeaderIsThere_AStatusLineSaysSo_AndNoViewIsOffered()
    {
        var pending = new TaskCompletionSource<IReadOnlyList<CommitteeReviewListItemDto>>();
        Services.AddSingleton<IScopedSender>(new Sender(_ => pending.Task));

        var cut = RenderComponent<MyReviews>();

        cut.Find("h1").TextContent.Trim().Should().Be("My committee reviews");
        cut.Find("p.visually-hidden[role='status']").TextContent.Should().Be("Loading your committee reviews.");
        cut.FindAll(".skeleton").Should().HaveCount(3);
        Views(cut).Should().BeEmpty();
    }

    [Fact]
    public void AFailedRead_IsFixedWords_WithTryAgain_NeverTheExceptionsText()
    {
        var attempts = 0;
        Services.AddSingleton<IScopedSender>(new Sender(_ => ++attempts == 1
            ? Task.FromException<IReadOnlyList<CommitteeReviewListItemDto>>(new InvalidOperationException("Npgsql: connection refused"))
            : Task.FromResult<IReadOnlyList<CommitteeReviewListItemDto>>([ListItem(30, "General CCC", 2, new DateOnly(2026, 10, 13))])));

        var cut = RenderComponent<MyReviews>();
        cut.WaitForState(() => cut.FindAll(".alert-danger").Count == 1);

        Text(cut.Find(".alert-danger")).Should().Contain(
            "Could not load your committee reviews. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain("Npgsql");

        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Try again").Click();
        cut.WaitForState(() => Views(cut).Count == 1);
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private IRenderedComponent<MyReviews> RenderPage(params CommitteeReviewListItemDto[] reviews)
    {
        Services.AddSingleton<IScopedSender>(new Sender(_ => Task.FromResult<IReadOnlyList<CommitteeReviewListItemDto>>(reviews)));
        var cut = RenderComponent<MyReviews>();
        cut.WaitForState(() => Views(cut).Count == reviews.Length && cut.FindAll("[role='status']").All(status => status.TextContent.Length == 0));
        return cut;
    }

    private static List<AngleSharp.Dom.IElement> Views(IRenderedFragment cut)
        => cut.FindAll("tbody a").Where(view => view.TextContent.Trim() == "View").ToList();

    private static CommitteeReviewListItemDto ListItem(
        int id,
        string panel,
        int semester,
        DateOnly scheduledOn,
        bool formative = false,
        CommitteeReviewState? state = null,
        CommitteeReviewType type = CommitteeReviewType.AnnualProgression,
        DateOnly? from = null)
        => new(
            id, TraineeId, 20, panel, from ?? new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31),
            scheduledOn, state ?? (formative ? CommitteeReviewState.Final : CommitteeReviewState.Ratified),
            formative ? null : CommitteeDecisionCategory.SatisfactoryProgress, null, formative, type)
        {
            AcademicYear = 2026,
            Semester = semester
        };

    private static string Text(AngleSharp.Dom.IElement element) => Whitespace().Replace(element.TextContent, " ").Trim();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    private sealed class Sender(Func<ListReviewsForTraineeQuery, Task<IReadOnlyList<CommitteeReviewListItemDto>>> list) : IScopedSender
    {
        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request is ListReviewsForTraineeQuery query
                ? (TResponse)(object)await list(query)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
