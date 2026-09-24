using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Components.Pages.Portfolio;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// Per EPA and semester, whether a released MSF campaign covered it (T168): on the trainee's progress page beside each
/// quota card, and on the committee review page as a table. Worded as D9 requires (a campaign COVERING the EPA was
/// released, never one "about" it), and shown as coverage, never as a shortfall against a target.
/// </summary>
public sealed partial class MsfCoverageRenderingTests : TestContext
{
    private static readonly MsfCoveragePeriodDto Semester1 = Period(2026, 1, hasEnded: true);
    private static readonly MsfCoveragePeriodDto Semester2 = Period(2026, 2, hasEnded: false);

    private readonly TestAuthorizationContext _auth;
    private readonly CultureInfo _originalCulture = CultureInfo.CurrentCulture;
    private readonly CultureInfo _originalUiCulture = CultureInfo.CurrentUICulture;

    public MsfCoverageRenderingTests()
    {
        _auth = this.AddTestAuthorization();
        var southAfrican = CultureInfo.GetCultureInfo("en-ZA");
        CultureInfo.CurrentCulture = southAfrican;
        CultureInfo.CurrentUICulture = southAfrican;
    }

    // ─── My progress ─────────────────────────────────────────────────────────

    [Fact]
    public void EachQuotaCard_SaysBesideItsQuotaLines_WhetherAReleasedCampaignCoveringTheEpaClosed_NewestSemesterFirst()
    {
        SignIn("trainee-1", WombatRoles.Trainee);
        var coverage = Coverage(
            [Semester1, Semester2],
            Row(7, "PAED-001", Cell(2026, 1, Campaign(50, new DateOnly(2026, 3, 10))), Cell(2026, 2)),
            Row(8, "PAED-002", Cell(2026, 1), Cell(2026, 2)));
        var cut = RenderMyProgress(coverage);

        MsfLine(cut, "PAED-001").Should().Be(
            "MSF in Semester 2, 2026: no released campaign covering this EPA has closed yet. " +
            "MSF in Semester 1, 2026: covered by a released campaign that closed on 10 March 2026.");
        MsfLine(cut, "PAED-002").Should().Be(
            "MSF in Semester 2, 2026: no released campaign covering this EPA has closed yet. " +
            "MSF in Semester 1, 2026: no campaign covering this EPA that closed in the semester has been released.");
    }

    [Fact]
    public void AnEndedSemesterWithNoReleasedCampaign_IsNotWordedAsFinal()
    {
        // A campaign that closed on 20 June and is released on 10 July covers semester 1 from then: until it is, the
        // ended semester's line must leave room for it, and must not name the unreleased campaign.
        var phrase = Wombat.Web.Components.Shared.MsfCoverageText.PeriodPhrase(Semester1, Cell(2026, 1));

        phrase.Should().Be("no campaign covering this EPA that closed in the semester has been released");
        Wombat.Web.Components.Shared.MsfCoverageText.PeriodPhrase(Semester2, Cell(2026, 2)).Should().Be("no released campaign covering this EPA has closed yet");
    }

    [Fact]
    public void ASemesterWithTwoCampaigns_SaysHowManyAndWhenTheLatestClosed()
    {
        SignIn("trainee-1", WombatRoles.Trainee);
        var coverage = Coverage(
            [Semester2],
            Row(7, "PAED-001", Cell(2026, 2, Campaign(51, new DateOnly(2026, 9, 1)), Campaign(50, new DateOnly(2026, 7, 20)))),
            Row(8, "PAED-002", Cell(2026, 2)));
        var cut = RenderMyProgress(coverage);

        MsfLine(cut, "PAED-001").Should().Be(
            "MSF in Semester 2, 2026: covered by 2 released campaigns, the latest closed on 1 September 2026.");
    }

    [Fact]
    public void ThisPeriod_CountsTheEpasCoveredThisSemester_AndSaysMsfCountsTowardsNoTarget()
    {
        SignIn("trainee-1", WombatRoles.Trainee);
        var coverage = Coverage(
            [Semester1, Semester2 with { EpasCovered = 1 }],
            Row(7, "PAED-001", Cell(2026, 1), Cell(2026, 2, Campaign(50, new DateOnly(2026, 8, 10)))),
            Row(8, "PAED-002", Cell(2026, 1), Cell(2026, 2)));
        var cut = RenderMyProgress(coverage);

        Details(cut.Find("section.detail-card dl.details-list"))["Multi-source feedback"].Should().Be(
            "1 of 2 EPAs covered by a released campaign that closed this semester. " +
            "MSF is tracked on its own and counts towards no target.");
    }

    [Fact]
    public void TheProgressPage_AsksForTheSignedInTraineesCoverage_WithNoSpan()
    {
        SignIn("trainee-1", WombatRoles.Trainee);
        var sender = ProgressSender(_ => null);
        Services.AddSingleton<IScopedSender>(sender);

        RenderProgressPage();

        var query = sender.Received.OfType<GetMsfCoverageForTraineeQuery>().Should().ContainSingle().Subject;
        query.TraineeUserId.Should().Be("trainee-1");
        query.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be("trainee-1");
        query.From.Should().BeNull("the reader picks this semester and the one before, on today's calendar");
        query.To.Should().BeNull();
    }

    [Fact]
    public void AFailedCoverageRead_SaysSo_AndTheQuotaCardsStillShow()
    {
        SignIn("trainee-1", WombatRoles.Trainee);
        Services.AddSingleton<IScopedSender>(ProgressSender(_ => throw new InvalidOperationException("The database is unavailable.")));

        var cut = RenderProgressPage();

        PageText(cut).Should().Contain("Your multi-source feedback coverage could not be loaded: The database is unavailable.");
        cut.FindAll(".progress-bar").Should().HaveCount(2, "the targets the trainee came for are still there");
        cut.Markup.Should().NotContain("MSF in ");
    }

    // ─── The committee review page ───────────────────────────────────────────

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TheReviewPage_AsksForTheReviewPeriod_AndPutsTheCardRightAfterTheStanding(bool formative)
    {
        SignIn("chair-1", WombatRoles.CommitteeMember);
        var sender = ReviewSender(Review(formative), _ => Coverage(
            [Semester1],
            Row(7, "PAED-001", Cell(2026, 1, Campaign(50, new DateOnly(2026, 3, 10))))));
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderReview();

        var query = sender.Received.OfType<GetMsfCoverageForTraineeQuery>().Should().ContainSingle().Subject;
        query.TraineeUserId.Should().Be("trainee-1", "the review's trainee, never the caller");
        query.From.Should().Be(new DateOnly(2026, 1, 1));
        query.To.Should().Be(new DateOnly(2026, 6, 30));
        query.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be("chair-1");

        var headings = cut.FindAll("section.detail-card > h3").Select(Text).ToList();
        headings.IndexOf("Multi-source feedback by EPA").Should().Be(headings.IndexOf("Entrustment against Annexure A") + 1);
        CoverageCard(cut).ClassList.Should().Contain("full-width");
    }

    [Fact]
    public void TheReviewPage_GivesThePanelItsPeriod_SoAPeriodStartingInsideASemesterIsSaidToBeReadWhole()
    {
        SignIn("chair-1", WombatRoles.CommitteeMember);
        Services.AddSingleton<IScopedSender>(ReviewSender(
            Review(formative: false, periodFrom: new DateOnly(2026, 2, 1)),
            _ => Coverage([Semester1], Row(7, "PAED-001", Cell(2026, 1)))));

        var cut = RenderReview();

        Text(CoverageCard(cut)).Should().Contain("A semester is read whole, so a campaign shown may have closed outside the review period.");
    }

    [Fact]
    public void TheReviewPage_GivesThePanelTheReviewsState_SoAScheduledReviewIsNotToldOfASnapshotItHasNot()
    {
        SignIn("chair-1", WombatRoles.CommitteeMember);
        Services.AddSingleton<IScopedSender>(ReviewSender(
            Review(formative: false, state: CommitteeReviewState.Scheduled),
            _ => Coverage([Semester1], Row(7, "PAED-001", Cell(2026, 1)))));

        var cut = RenderReview();

        var card = Text(CoverageCard(cut));
        card.Should().Contain("Read live. Nothing is frozen yet: starting the review freezes the evidence snapshot");
        card.Should().NotContain("released after the review started");
    }

    [Fact]
    public void TheTable_HasAColumnPerSemester_AndSaysCoveredOrNoneReleasedInEachCell()
    {
        var cut = RenderPanel(
            Coverage(
                [Semester1 with { EpasCovered = 1 }, Semester2],
                Row(7, "PAED-001", Cell(2026, 1, Campaign(50, new DateOnly(2026, 3, 10))), Cell(2026, 2)),
                Row(8, "PAED-002", Cell(2026, 1), Cell(2026, 2)),
                Row(9, "HOST-009", Cell(2026, 1, Campaign(52, new DateOnly(2026, 5, 2)), Campaign(50, new DateOnly(2026, 3, 10))), Cell(2026, 2), isLocal: true)),
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31));

        cut.FindAll("thead th").Select(Text).Should().Equal("EPA", "Semester 1, 2026", "Semester 2, 2026");

        // The EPA cell by its parts: each is its own block, so their text runs together in textContent.
        var rows = cut.FindAll("tbody tr").ToList();
        rows.Select(row => row.Children[0].Children.Select(Text).ToList()).Should().BeEquivalentTo(
            new[]
            {
                new[] { "PAED-001", "PAED-001 title" },
                new[] { "PAED-002", "PAED-002 title" },
                new[] { "HOST-009", "HOST-009 title", "The institution's own EPA" }
            },
            options => options.WithStrictOrdering());
        rows.Select(row => row.Children.Skip(1).Select(Text).ToList()).Should().BeEquivalentTo(
            new[]
            {
                new[] { "Covered Annual MSF #50, closed 10 Mar 2026", "None released yet" },
                new[] { "None released", "None released yet" },
                new[] { "Covered 2 campaigns; the latest Annual MSF #52, closed 2 May 2026", "None released yet" }
            },
            options => options.WithStrictOrdering());

        Details(cut.Find("dl.details-list")).Should().Equal(new Dictionary<string, string>
        {
            ["Semester 1, 2026 (January to June)"] = "1 of 3 EPAs covered",
            ["Semester 2, 2026 (July to November)"] = "0 of 3 EPAs covered so far"
        });
        cut.FindAll(".badge").Should().BeEmpty("coverage is not a verdict against a target, so it takes no tint");
    }

    [Fact]
    public void TheOpening_SaysWhatCoveredMeans_ThatItIsLive_AndThatMsfCountsTowardsNoTarget()
    {
        var cut = RenderPanel(Coverage([Semester1], Row(7, "PAED-001", Cell(2026, 1))), new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30));

        Text(cut.Find("p.muted")).Should().Be(
            "For each semester the review period touches, whether a released multi-source feedback campaign covering the " +
            "EPA closed in it. A campaign covers several EPAs and is not about any one of them. Only released campaigns " +
            "count, and MSF counts towards no encounter target. Read live: a campaign released after the review started " +
            "is shown here, though the evidence snapshot does not hold it.");
    }

    [Theory]
    [InlineData(CommitteeReviewState.Scheduled,
        "Read live. Nothing is frozen yet: starting the review freezes the evidence snapshot, and a campaign released " +
        "after that is shown here but not held in it.")]
    [InlineData(CommitteeReviewState.InProgress,
        "Read live: a campaign released after the review started is shown here, though the evidence snapshot does not hold it.")]
    [InlineData(CommitteeReviewState.Decided, Concluded)]
    [InlineData(CommitteeReviewState.Ratified, Concluded)]
    [InlineData(CommitteeReviewState.UnderAppeal, Concluded)]
    [InlineData(CommitteeReviewState.Final, Concluded)]
    public void TheLiveSentence_FitsWhereTheReviewStands(CommitteeReviewState state, string expected)
    {
        var cut = RenderPanel(
            Coverage([Semester1], Row(7, "PAED-001", Cell(2026, 1))), new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), state);

        Text(cut.Find("p.muted")).Should().EndWith(
            "MSF counts towards no encounter target. " + expected,
            "only the live sentence differs, and the review's whole semesters add no other");
    }

    private const string Concluded =
        "Read live, so it may differ from what the panel saw: a campaign released after the review started is shown " +
        "here, though the evidence snapshot does not hold it.";

    [Theory]
    [InlineData(2026, 2, 1, 2026, 6, 30)]
    [InlineData(2026, 1, 1, 2026, 5, 31)]
    public void AReviewPeriodThatStartsOrEndsInsideASemester_SaysTheSemesterIsReadWhole(
        int fromYear, int fromMonth, int fromDay, int toYear, int toMonth, int toDay)
    {
        var cut = RenderPanel(
            Coverage([Semester1], Row(7, "PAED-001", Cell(2026, 1))),
            new DateOnly(fromYear, fromMonth, fromDay),
            new DateOnly(toYear, toMonth, toDay));

        Text(cut.Find("p.muted")).Should().Contain("A semester is read whole, so a campaign shown may have closed outside the review period.");
    }

    [Fact]
    public void AReviewPeriodOfWholeSemesters_DoesNotSayTheSemesterIsReadWhole()
    {
        var cut = RenderPanel(
            Coverage([Semester1, Semester2], Row(7, "PAED-001", Cell(2026, 1), Cell(2026, 2))),
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31));

        Text(cut.Find("p.muted")).Should().NotContain("read whole");
    }

    [Fact]
    public void NothingToShow_IsNotSaidToBeNoCurriculum_AndAFailureSaysSo()
    {
        var empty = RenderPanel(null, null, null);
        Text(empty.Find("p")).Should().Be(
            "No coverage to show: this trainee has no curriculum on record, or your account does not oversee their programme.");

        var failed = RenderComponent<Wombat.Web.Components.Shared.MsfCoveragePanel>(parameters => parameters
            .Add(panel => panel.LoadError, "The database is unavailable."));
        PageText(failed).Should().Be("The multi-source feedback coverage could not be loaded: The database is unavailable.");
    }

    [Fact]
    public void AFailedCoverageRead_OnTheReviewPage_StaysInItsCard()
    {
        SignIn("chair-1", WombatRoles.CommitteeMember);
        Services.AddSingleton<IScopedSender>(ReviewSender(Review(formative: false), _ => throw new InvalidOperationException("The database is unavailable.")));

        var cut = RenderReview();

        Text(CoverageCard(cut)).Should().Contain("The multi-source feedback coverage could not be loaded: The database is unavailable.");
        cut.FindAll(".alert-danger").Should().BeEmpty("the page's own error banner is for the review and its actions");
    }

    // ─── Fixtures ────────────────────────────────────────────────────────────

    private static MsfCoveragePeriodDto Period(int year, int semester, bool hasEnded)
        => semester == 1
            ? new(year, 1, $"Semester 1, {year}", "January to June", new(year, 1, 1), new(year, 6, 30), new(year, 6, 30), hasEnded, 0)
            : new(year, 2, $"Semester 2, {year}", "July to November", new(year, 7, 1), new(year, 12, 31), new(year, 11, 30), hasEnded, 0);

    private static MsfCoverageDto Coverage(MsfCoveragePeriodDto[] periods, params MsfEpaCoverageDto[] epas)
        => new(new DateOnly(2026, 9, 23), periods, epas);

    private static MsfEpaCoverageDto Row(int epaId, string code, params MsfEpaPeriodCoverageDto[] cells)
        => Row(epaId, code, cells, isLocal: false);

    private static MsfEpaCoverageDto Row(int epaId, string code, MsfEpaPeriodCoverageDto first, MsfEpaPeriodCoverageDto second, bool isLocal)
        => Row(epaId, code, [first, second], isLocal);

    private static MsfEpaCoverageDto Row(int epaId, string code, MsfEpaPeriodCoverageDto[] cells, bool isLocal)
        => new(100 + epaId, epaId, code, $"{code} title", isLocal, cells);

    private static MsfEpaPeriodCoverageDto Cell(int year, int semester, params MsfCoveringCampaignDto[] campaigns)
        => new(year, semester, campaigns);

    private static MsfCoveringCampaignDto Campaign(int id, DateOnly closedOn)
        => new(id, "Annual MSF", closedOn, closedOn.AddDays(3));

    private IRenderedComponent<Wombat.Web.Components.Shared.MsfCoveragePanel> RenderPanel(
        MsfCoverageDto? coverage, DateOnly? from, DateOnly? to, CommitteeReviewState? state = null)
        => RenderComponent<Wombat.Web.Components.Shared.MsfCoveragePanel>(parameters => parameters
            .Add(panel => panel.Coverage, coverage)
            .Add(panel => panel.ReviewPeriodFrom, from)
            .Add(panel => panel.ReviewPeriodTo, to)
            .Add(panel => panel.ReviewState, state));

    private IRenderedComponent<MyProgress> RenderMyProgress(MsfCoverageDto coverage)
    {
        Services.AddSingleton<IScopedSender>(ProgressSender(_ => coverage));
        var cut = RenderProgressPage();
        cut.WaitForState(() => cut.Markup.Contains("Multi-source feedback"));
        return cut;
    }

    private IRenderedComponent<MyProgress> RenderProgressPage()
    {
        var cut = RenderComponent<MyProgress>();
        cut.WaitForState(() => cut.Markup.Contains("Curriculum targets"));
        return cut;
    }

    /// <summary>Two per-semester items, PAED-001 (EPA 7) and PAED-002 (EPA 8), in semester 2 of 2026.</summary>
    private static RecordingSender ProgressSender(Func<GetMsfCoverageForTraineeQuery, object?> coverage)
        => new RecordingSender()
            .On<GetCurriculumProgressForTraineeQuery>(_ => ProgressSummary())
            .On<GetEpaTrajectoryForTraineeQuery>(_ => Array.Empty<EpaTrajectoryDto>())
            .On<GetEntrustmentStandingForTraineeQuery>(_ => null)
            .On(coverage);

    private static TraineeCurriculumProgressSummaryDto ProgressSummary()
    {
        TraineeCurriculumProgressDto Item(int epaId, string code) => new(
            100 + epaId, epaId, code, $"{code} title", QuotaPeriod.Semester, 3,
            new QuotaWindowDto("Semester 2, 2026", "July to November", new(2026, 7, 1), new(2026, 11, 30),
                QuotaWindowStatus.Counting, 1, 3, false, 2, 33, 1, new DateOnly(2026, 8, 1), null, null),
            null, 3, "3a", null);

        return new TraineeCurriculumProgressSummaryDto(
            new DateOnly(2026, 9, 23), new DateOnly(2025, 1, 1), 2, "Semester 2, 2026", "July to November",
            new DateOnly(2026, 11, 30), false, 0, 2, 0, 0, true, false, false, null, null,
            [Item(7, "PAED-001"), Item(8, "PAED-002")]);
    }

    private static RecordingSender ReviewSender(CommitteeReviewDetailDto review, Func<GetMsfCoverageForTraineeQuery, object?> coverage)
        => new RecordingSender()
            .On<GetCommitteeReviewByIdQuery>(_ => review)
            .On<ListPendingEntrustmentDecisionsForReviewQuery>(_ => Array.Empty<PendingEntrustmentDecisionDto>())
            .On<GetSamplingConcentrationWarningsQuery>(_ => null)
            .On<CountMsfCampaignsOutsideSnapshotQuery>(_ => MsfCampaignsOutsideSnapshotDto.None)
            .On<GetEpaTrajectoryForTraineeQuery>(_ => Array.Empty<EpaTrajectoryDto>())
            .On<ListStarEpaOptionsForReviewQuery>(_ => Array.Empty<StarEpaOptionDto>())
            .On<GetEntrustmentScalesListQuery>(_ => Array.Empty<EntrustmentScaleDto>())
            .On<GetEntrustmentStandingForTraineeQuery>(_ => null)
            .On(coverage);

    private static CommitteeReviewDetailDto Review(
        bool formative, DateOnly? periodFrom = null, CommitteeReviewState state = CommitteeReviewState.InProgress)
        => new(
            30,
            "trainee-1",
            20,
            "Paediatrics CCC",
            periodFrom ?? new DateOnly(2026, 1, 1),
            new DateOnly(2026, 6, 30),
            new DateOnly(2026, 7, 2),
            state,
            state == CommitteeReviewState.Scheduled ? null : new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc),
            state == CommitteeReviewState.Scheduled ? null : "chair-1",
            null,
            null,
            null,
            [],
            [],
            [],
            IsFormative: formative)
        {
            AcademicYear = 2026,
            Semester = 1
        };

    private IRenderedComponent<ReviewDetail> RenderReview()
    {
        var cut = RenderComponent<ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, 30));
        cut.WaitForState(() => cut.Markup.Contains("Multi-source feedback by EPA"));
        return cut;
    }

    private static IElement CoverageCard(IRenderedFragment cut)
        => cut.FindAll("section.detail-card").Single(card => card.QuerySelector("h3") is { } heading && Text(heading) == "Multi-source feedback by EPA");

    /// <summary>The MSF line of one quota card, found by the card's EPA code.</summary>
    private static string MsfLine(IRenderedFragment cut, string epaCode)
        => cut.FindAll(".dashboard-grid > section.detail-card")
            .Single(card => Text(card.QuerySelector(".progress-row-head h3")!) == epaCode)
            .QuerySelectorAll("p.progress-row-meta")
            .Select(Text)
            .Single(line => line.StartsWith("MSF in ", StringComparison.Ordinal));

    private static Dictionary<string, string> Details(IElement detailsList)
        => detailsList.Children
            .Where(child => child.LocalName == "div")
            .ToDictionary(row => Text(row.QuerySelector("dt")!), row => Text(row.QuerySelector("dd")!));

    private void SignIn(string userId, string role)
    {
        _auth.SetAuthorized(userId);
        _auth.SetRoles(role);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, userId));
    }

    private static string Text(IElement element) => Whitespace().Replace(element.TextContent, " ").Trim();

    private static string PageText(IRenderedFragment cut)
        => Whitespace().Replace(string.Join(" ", cut.Nodes.Select(node => node.TextContent)), " ").Trim();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    protected override void Dispose(bool disposing)
    {
        CultureInfo.CurrentCulture = _originalCulture;
        CultureInfo.CurrentUICulture = _originalUiCulture;
        base.Dispose(disposing);
    }

    /// <summary>Answers what a test registers, records every request, and refuses anything unregistered.</summary>
    private sealed class RecordingSender : IScopedSender
    {
        private readonly Dictionary<Type, Func<object, object?>> _answers = [];

        public List<object> Received { get; } = [];

        public RecordingSender On<TRequest>(Func<TRequest, object?> answer)
        {
            _answers[typeof(TRequest)] = request => answer((TRequest)request);
            return this;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Received.Add(request);

            return _answers.TryGetValue(request.GetType(), out var answer)
                ? Task.FromResult((TResponse)answer(request)!)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
