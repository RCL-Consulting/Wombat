using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Components.Pages.Portfolio;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// The committee review page and the trainee's progress page show where the trainee stands on each EPA against
/// Annexure A's target for their training year and against the exit rule (T166). Every verdict is words in its cell,
/// the exit rule names the EPAs short of it, and the page says the rule gates nothing.
/// </summary>
public sealed partial class EntrustmentStandingPanelTests : TestContext
{
    private const string Cpsa = "CPSA Paediatric Entrustment Scale v11.1";
    private const string InformationOnly =
        "For information only. Recording a Graduate decision or completing the programme does not check the exit rule.";

    private readonly TestAuthorizationContext _auth;

    public EntrustmentStandingPanelTests()
    {
        _auth = this.AddTestAuthorization();
        var southAfrican = CultureInfo.GetCultureInfo("en-ZA");
        CultureInfo.CurrentCulture = southAfrican;
        CultureInfo.CurrentUICulture = southAfrican;
    }

    // ─── The panel ───────────────────────────────────────────────────────────

    [Fact]
    public void EachEpa_ReadsItsVerdictInWords()
    {
        var cut = RenderPanel(Standing(
            Epa("PAED-001", decision: "3a", year: EntrustmentStandingStatus.Below, exit: EntrustmentStandingStatus.Below),
            Epa("PAED-002", decision: "3b", year: EntrustmentStandingStatus.AtOrAbove, exit: EntrustmentStandingStatus.Below),
            Epa("PAED-003", decision: null, year: EntrustmentStandingStatus.NoDecision, exit: EntrustmentStandingStatus.NoDecision)));

        var rows = cut.FindAll("tbody tr").ToList();
        rows.Select(row => Text(row.QuerySelector(".badge")!.TextContent)).Should().Equal("Below", "At or above", "No decision");
        Text(rows[0].Children[1].TextContent).Should().Be("3b", "the year-2 target, as a rung");
        Text(rows[0].Children[2].TextContent).Should().StartWith("3a");
        Text(rows[2].Children[2].TextContent).Should().Be("None");
        Text(rows[0].Children[4].TextContent).Should().Be("5 Not yet");
        cut.FindAll("th").Select(header => Text(header.TextContent)).Should().Contain("Year 2 target");
    }

    [Fact]
    public void TheExitRule_CountsByExitLevel_AndNamesTheEpasShortOfIt()
    {
        var cut = RenderPanel(Standing(
            [
                Epa("PAED-001", decision: "5", year: EntrustmentStandingStatus.AtOrAbove, exit: EntrustmentStandingStatus.AtOrAbove),
                Epa("PAED-002", decision: "3b", year: EntrustmentStandingStatus.AtOrAbove, exit: EntrustmentStandingStatus.Below),
                Epa("PAED-008", decision: null, year: EntrustmentStandingStatus.NoDecision, exit: EntrustmentStandingStatus.NoDecision, exitLabel: "4", exitOrder: 5)
            ],
            new ExitRuleReadinessDto(
                3,
                1,
                [new ExitLevelGroupDto(6, "5", 2, 1), new ExitLevelGroupDto(5, "4", 1, 0)],
                ["PAED-002", "PAED-008"])));

        var text = PageText(cut);
        text.Should().Contain("1 of 3 EPAs at their exit level by STAR decision (level 5: 1 of 2 · level 4: 0 of 1).");
        text.Should().Contain("Not yet: PAED-002, PAED-008.");
        text.Should().Contain(InformationOnly);
        text.Should().Contain("2 at or above · 0 below · 1 with no decision, of 3 EPAs");
    }

    [Fact]
    public void ATraineeAtEveryExitLevel_IsToldSo()
    {
        var cut = RenderPanel(Standing(
            [Epa("PAED-001", decision: "5", year: EntrustmentStandingStatus.AtOrAbove, exit: EntrustmentStandingStatus.AtOrAbove)],
            new ExitRuleReadinessDto(1, 1, [new ExitLevelGroupDto(6, "5", 1, 1)], [])));

        var text = PageText(cut);
        text.Should().Contain("1 of 1 EPAs at their exit level by STAR decision. Every EPA is at its exit level.");
        text.Should().NotContain("Not yet:");
    }

    [Fact]
    public void ADecisionOnAnotherLadder_SaysWhyItIsNotCompared()
    {
        var epa = Epa("PAED-001", decision: "Supervises others", year: EntrustmentStandingStatus.NotComparable,
            exit: EntrustmentStandingStatus.NotComparable, otherLadder: "O-R Scale");

        var cut = RenderPanel(Standing(epa));

        var row = cut.Find("tbody tr");
        Text(row.Children[2].TextContent).Should().StartWith("Supervises others on O-R Scale");
        Text(row.Children[3].QuerySelector(".badge")!.TextContent).Should().Be("Not comparable");
        Text(row.Children[3].QuerySelector("div")!.TextContent).Should().Be($"Decided on O-R Scale, not {Cpsa}.");
        Text(row.Children[4].TextContent).Should().Be("5 Not comparable");
        PageText(cut).Should().Contain("1 not comparable");
    }

    [Fact]
    public void AnUnpinnedItem_SaysTheCurriculumSetsNoLadder()
    {
        var epa = Epa("PAED-001", decision: "5", year: EntrustmentStandingStatus.NotComparable,
            exit: EntrustmentStandingStatus.NotComparable) with { ScaleName = null };

        var cut = RenderPanel(Standing(epa));

        var cell = cut.Find("tbody tr").Children[3];
        Text(cell.QuerySelector(".badge")!.TextContent).Should().Be("Not comparable");
        Text(cell.QuerySelector("div")!.TextContent).Should().Be("The curriculum sets no ladder for this EPA.");
    }

    [Fact]
    public void TheLatestRating_LinksToItsActivity_AndMarksAnUndatedEncounter()
    {
        var epa = Epa("PAED-001", decision: null, year: EntrustmentStandingStatus.NoDecision, exit: EntrustmentStandingStatus.NoDecision)
            with
            {
                LatestRating = new StandingRatingDto(
                    31, new DateOnly(2026, 3, 20), ObservedOnDeclared: false, 4, "3b", null, "Direct observation",
                    EntrustmentStandingStatus.AtOrAbove)
            };

        var cut = RenderPanel(Standing(epa));

        var cell = cut.Find("tbody tr").Children[5];
        cell.QuerySelector("a")!.GetAttribute("href").Should().Be("/activities/31");
        Text(cell.QuerySelector("a")!.TextContent).Should().Be("3b — open this PAED-001 rating");
        cell.QuerySelectorAll("div").Select(line => Text(line.TextContent)).Should().Equal(
            "2026-03-20 (filed; no encounter date)",
            "Direct observation; at or above the target");
    }

    [Fact]
    public void BeforeTheProgrammeStarts_TheYearOneTargetsAreSaidToBeYearOnes()
    {
        var standing = Standing(Epa("PAED-001", decision: null, year: EntrustmentStandingStatus.NoDecision,
            exit: EntrustmentStandingStatus.NoDecision)) with
        {
            TargetYear = 1,
            ProgrammeNotStarted = true,
            ProgrammeStartDate = new DateOnly(2027, 1, 15)
        };

        var cut = RenderComponent<EntrustmentStandingPanel>(parameters => parameters
            .Add(panel => panel.Standing, standing)
            .Add(panel => panel.Self, true));

        PageText(cut).Should().Contain("Your programme starts on 15 January 2027, so the targets shown are for training year 1.");
    }

    [Fact]
    public void AFailedLoad_SaysSoInThePanel()
    {
        var cut = RenderComponent<EntrustmentStandingPanel>(parameters => parameters
            .Add(panel => panel.LoadError, "The database is unavailable."));

        Text(cut.Find(".alert").TextContent).Should().Be("The entrustment standing could not be loaded: The database is unavailable.");
        cut.FindAll("table").Should().BeEmpty();
    }

    [Theory]
    [InlineData(true, "You have no curriculum on record yet, so there are no targets to compare against.")]
    [InlineData(false, "No standing to show: this trainee has no curriculum on record, or your account does not oversee their programme.")]
    public void NothingToShow_IsNotClaimedToBeNoCurriculum_ForSomeoneElse(bool self, string expected)
    {
        // For someone else, null is also what a panel member who does not oversee the trainee gets (T113).
        var cut = RenderComponent<EntrustmentStandingPanel>(parameters => parameters
            .Add(panel => panel.Standing, null)
            .Add(panel => panel.Self, self));

        Text(cut.Find("p").TextContent).Should().Be(expected);
    }

    [Fact]
    public void AnEpaWithNoLevelForTheYear_SaysTheExitLevelStandsIn()
    {
        var cut = RenderPanel(Standing(
            Epa("PAED-001", decision: "3b", year: EntrustmentStandingStatus.AtOrAbove, exit: EntrustmentStandingStatus.Below),
            Epa("LOCAL-020", decision: null, year: EntrustmentStandingStatus.NoDecision, exit: EntrustmentStandingStatus.NoDecision,
                yearTargetIsExitLevel: true)));

        var rows = cut.FindAll("tbody tr").ToList();
        Text(rows[0].Children[1].TextContent).Should().Be("3b");
        Text(rows[1].Children[1].FirstChild!.TextContent).Should().Be("5");
        Text(rows[1].Children[1].QuerySelector("div")!.TextContent).Should().Be("Exit level; no year 2 level set");
        rows[0].Children[1].QuerySelector("div").Should().BeNull("PAED-001's target is the year's own");
        PageText(cut).Should().Contain(
            "Each target is the level the curriculum sets for training year 2: Annexure A's, for the College's EPAs. " +
            "Where it sets none, the EPA's exit level stands in, and the row says so.");
    }

    [Fact]
    public void AYearTheCurriculumSetsNoLevelFor_IsNotPassedOffAsAnnexureAs()
    {
        var standing = Standing(
            Epa("PAED-001", decision: "5", year: EntrustmentStandingStatus.AtOrAbove, exit: EntrustmentStandingStatus.AtOrAbove,
                yearTargetIsExitLevel: true),
            Epa("PAED-008", decision: "4", year: EntrustmentStandingStatus.AtOrAbove, exit: EntrustmentStandingStatus.AtOrAbove,
                exitLabel: "4", exitOrder: 5, yearTargetIsExitLevel: true)) with { TargetYear = 5 };

        var cut = RenderPanel(standing);

        var text = PageText(cut);
        text.Should().Contain("The curriculum sets no level for training year 5, so each EPA's exit level stands in as its target.");
        text.Should().NotContain("Annexure A's, for");
        cut.FindAll("tbody tr").Select(row => (Text(row.Children[1].FirstChild!.TextContent), Text(row.Children[1].QuerySelector("div")!.TextContent)))
            .Should().Equal(("5", "Exit level; no year 5 level set"), ("4", "Exit level; no year 5 level set"));
    }

    [Fact]
    public void AnInstitutionsOwnEpa_IsMarked_AndTheExitRuleSaysItIsNotCounted()
    {
        var cut = RenderPanel(Standing(
            [
                Epa("PAED-001", decision: "5", year: EntrustmentStandingStatus.AtOrAbove, exit: EntrustmentStandingStatus.AtOrAbove),
                Epa("LOCAL-020", decision: null, year: EntrustmentStandingStatus.NoDecision, exit: EntrustmentStandingStatus.NoDecision,
                    isLocal: true)
            ],
            new ExitRuleReadinessDto(1, 1, [new ExitLevelGroupDto(6, "5", 1, 1)], [])));

        var rows = cut.FindAll("tbody tr").ToList();
        rows[0].Children[0].QuerySelectorAll("div").Select(line => Text(line.TextContent)).Should().Equal("PAED-001 title");
        rows[1].Children[0].QuerySelectorAll("div").Select(line => Text(line.TextContent)).Should().Equal(
            "LOCAL-020 title", "The institution's own EPA; not in the exit rule");
        PageText(cut).Should().Contain(
            "1 of 1 EPAs at their exit level by STAR decision. Every EPA the rule counts is at its exit level. " +
            "The institution's own EPA is not part of the College's rule, so it is not counted.");
    }

    [Fact]
    public void ACurriculumOfOnlyLocalEpas_SaysTheCollegesRuleCountsNoneOfThem()
    {
        var cut = RenderPanel(Standing(
            [
                Epa("LOCAL-020", decision: null, year: EntrustmentStandingStatus.NoDecision, exit: EntrustmentStandingStatus.NoDecision, isLocal: true),
                Epa("LOCAL-021", decision: null, year: EntrustmentStandingStatus.NoDecision, exit: EntrustmentStandingStatus.NoDecision, isLocal: true)
            ],
            new ExitRuleReadinessDto(0, 0, [], [])));

        var text = PageText(cut);
        text.Should().Contain(
            "The College's exit rule counts only its own EPAs, and none of them is in use on this curriculum. " +
            "The institution's own 2 EPAs are not part of the College's rule, so they are not counted.");
        text.Should().NotContain("0 of 0");
    }

    // ─── The committee review page ───────────────────────────────────────────

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TheReviewPage_AsksForTheReviewsTraineeAsTheSignedInChair_AndShowsTheTable(bool formative)
    {
        SignIn("chair-1", WombatRoles.CommitteeMember);
        var sender = ReviewSender(Review(CommitteeReviewState.InProgress, formative))
            .On<GetEntrustmentStandingForTraineeQuery>(_ => Standing(
                Epa("PAED-001", decision: "3a", year: EntrustmentStandingStatus.Below, exit: EntrustmentStandingStatus.Below)));
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderReview();

        var query = sender.Received.OfType<GetEntrustmentStandingForTraineeQuery>().Should().ContainSingle().Subject;
        query.TraineeUserId.Should().Be("trainee-1");
        query.AsOf.Should().Be(new DateOnly(2026, 6, 30), "the review period is over: its targets are the year it ended in");
        query.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be("chair-1");

        var section = cut.FindAll("section.detail-card").Single(card => card.QuerySelector("h3")?.TextContent == "Entrustment against Annexure A");
        section.ClassList.Should().Contain("full-width");
        section.QuerySelectorAll("tbody tr").Should().ContainSingle();
    }

    [Fact]
    public void AReviewHeldAfterItsPeriod_ShowsTheTargetsOfTheYearThePeriodEndedIn_AndSaysSo()
    {
        // Programme started 1 July 2025; the period ends on 30 June 2026, the last day of year 1. Held later, "today"
        // would be year 2, and the panel would judge the year under review against the next year's targets.
        SignIn("chair-1", WombatRoles.CommitteeMember);
        var sender = ReviewSender(Review(CommitteeReviewState.InProgress, formative: false))
            .On<GetEntrustmentStandingForTraineeQuery>(query => Standing(
                Epa("PAED-001", decision: "3a", year: EntrustmentStandingStatus.AtOrAbove, exit: EntrustmentStandingStatus.Below))
                with { AsOf = query.AsOf ?? new DateOnly(2026, 9, 24), ProgrammeStartDate = new DateOnly(2025, 7, 1), TargetYear = 1 });
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderReview();

        PageText(cut).Should().Contain(
            "The review period's last day, 30 June 2026, falls in training year 1, so the targets shown are that year's. " +
            "STAR decisions and ratings are shown as they stand today.");
    }

    [Fact]
    public void AReviewWhosePeriodIsStillRunning_ReadsTodaysTrainingYear()
    {
        SignIn("chair-1", WombatRoles.CommitteeMember);
        var sender = ReviewSender(Review(CommitteeReviewState.InProgress, formative: false, periodTo: new DateOnly(2099, 12, 31)))
            .On<GetEntrustmentStandingForTraineeQuery>(_ => Standing(
                Epa("PAED-001", decision: "3a", year: EntrustmentStandingStatus.Below, exit: EntrustmentStandingStatus.Below)));
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderReview();

        sender.Received.OfType<GetEntrustmentStandingForTraineeQuery>().Single().AsOf.Should().BeNull("today is inside the period");
        var text = PageText(cut);
        text.Should().Contain("In training year 2 on 1 March 2026.");
        text.Should().NotContain("review period's last day");
    }

    [Fact]
    public void TheReviewPage_StandsWhenTheStandingFails()
    {
        SignIn("chair-1", WombatRoles.CommitteeMember);
        var sender = ReviewSender(Review(CommitteeReviewState.InProgress, formative: false))
            .On<GetEntrustmentStandingForTraineeQuery>(_ => throw new InvalidOperationException("The database is unavailable."));
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderReview();

        cut.FindAll(".alert-danger").Should().BeEmpty("the review itself loaded");
        cut.Markup.Should().Contain("Record decision");
        var section = cut.FindAll("section.detail-card").Single(card => card.QuerySelector("h3")?.TextContent == "Entrustment against Annexure A");
        Text(section.TextContent).Should().Contain("The entrustment standing could not be loaded: The database is unavailable.");
    }

    [Fact]
    public void RatifyingTheReview_ReadsTheStandingAgain()
    {
        // Ratifying issues the staged STAR decisions, which is what the standing reads.
        SignIn("chair-1", WombatRoles.CommitteeMember);
        var standings = new Queue<EntrustmentStandingDto>(
        [
            Standing(Epa("PAED-001", decision: null, year: EntrustmentStandingStatus.NoDecision, exit: EntrustmentStandingStatus.NoDecision)),
            Standing(Epa("PAED-001", decision: "3b", year: EntrustmentStandingStatus.AtOrAbove, exit: EntrustmentStandingStatus.Below))
        ]);
        var sender = ReviewSender(Review(CommitteeReviewState.Decided, formative: false))
            .On<RatifyCommitteeDecisionCommand>(_ => Review(CommitteeReviewState.Ratified, formative: false))
            .On<GetEntrustmentStandingForTraineeQuery>(_ => standings.Dequeue());
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderReview();
        Badges(cut).Should().Equal("No decision");

        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Ratify").Click();
        cut.WaitForState(() => cut.Markup.Contains("Decision ratified."));

        sender.Received.OfType<GetEntrustmentStandingForTraineeQuery>().Should().HaveCount(2);
        Badges(cut).Should().Equal("At or above");
    }

    // ─── The trainee's progress page ─────────────────────────────────────────

    [Fact]
    public void MyProgress_AsksForTheSignedInTraineesStanding_AsThatTrainee()
    {
        SignIn("trainee-1", WombatRoles.Trainee);
        var sender = new RecordingSender()
            .On<GetCurriculumProgressForTraineeQuery>(_ => null)
            .On<GetEpaTrajectoryForTraineeQuery>(_ => Array.Empty<EpaTrajectoryDto>())
            .On<GetEntrustmentStandingForTraineeQuery>(_ => Standing(
                Epa("PAED-001", decision: "3a", year: EntrustmentStandingStatus.Below, exit: EntrustmentStandingStatus.Below)))
            .On<GetMsfCoverageForTraineeQuery>(_ => null);
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<MyProgress>();
        cut.WaitForState(() => cut.Markup.Contains("Entrustment against Annexure A"));

        var query = sender.Received.OfType<GetEntrustmentStandingForTraineeQuery>().Should().ContainSingle().Subject;
        query.TraineeUserId.Should().Be("trainee-1");
        query.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be("trainee-1");
        PageText(cut).Should().Contain("You are in training year 2 on 1 March 2026.");
        Badges(cut).Should().Equal("Below");
    }

    [Fact]
    public void MyProgress_WithNothingOnRecord_DoesNotSayItTwice()
    {
        SignIn("trainee-1", WombatRoles.Trainee);
        Services.AddSingleton<IScopedSender>(new RecordingSender()
            .On<GetCurriculumProgressForTraineeQuery>(_ => null)
            .On<GetEpaTrajectoryForTraineeQuery>(_ => Array.Empty<EpaTrajectoryDto>())
            .On<GetEntrustmentStandingForTraineeQuery>(_ => null)
            .On<GetMsfCoverageForTraineeQuery>(_ => null));

        var cut = RenderComponent<MyProgress>();
        cut.WaitForState(() => cut.Markup.Contains("Curriculum targets"));

        cut.Markup.Should().NotContain("Entrustment against Annexure A");
    }

    // ─── Fixtures ────────────────────────────────────────────────────────────

    private IRenderedComponent<EntrustmentStandingPanel> RenderPanel(EntrustmentStandingDto standing)
        => RenderComponent<EntrustmentStandingPanel>(parameters => parameters.Add(panel => panel.Standing, standing));

    private IRenderedComponent<ReviewDetail> RenderReview()
    {
        var cut = RenderComponent<ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, 30));
        cut.WaitForState(() => cut.Markup.Contains("Entrustment against Annexure A"));
        return cut;
    }

    private void SignIn(string userId, string role)
    {
        _auth.SetAuthorized(userId);
        _auth.SetRoles(role);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, userId));
    }

    private static IReadOnlyList<string> Badges(IRenderedFragment cut)
        => cut.FindAll("tbody .badge").Select(badge => Text(badge.TextContent)).ToList();

    private static EntrustmentStandingDto Standing(params EpaStandingDto[] epas)
        => Standing(
            epas,
            new ExitRuleReadinessDto(
                epas.Length,
                epas.Count(epa => epa.ExitStatus == EntrustmentStandingStatus.AtOrAbove),
                [new ExitLevelGroupDto(6, "5", epas.Length, epas.Count(epa => epa.ExitStatus == EntrustmentStandingStatus.AtOrAbove))],
                epas.Where(epa => epa.ExitStatus != EntrustmentStandingStatus.AtOrAbove).Select(epa => epa.EpaCode).ToArray()));

    private static EntrustmentStandingDto Standing(EpaStandingDto[] epas, ExitRuleReadinessDto exit)
        => new(new DateOnly(2026, 3, 1), new DateOnly(2025, 1, 1), 2, false, epas, exit);

    private static EpaStandingDto Epa(
        string code,
        string? decision,
        EntrustmentStandingStatus year,
        EntrustmentStandingStatus exit,
        string? otherLadder = null,
        string exitLabel = "5",
        int exitOrder = 6,
        bool isLocal = false,
        bool yearTargetIsExitLevel = false)
        => new(
            100,
            1,
            code,
            $"{code} title",
            Cpsa,
            isLocal,
            yearTargetIsExitLevel ? exitOrder : 4,
            yearTargetIsExitLevel ? exitLabel : "3b",
            yearTargetIsExitLevel,
            exitOrder,
            exitLabel,
            decision is null
                ? null
                : new StandingDecisionDto(41, 3, decision, otherLadder, new DateOnly(2026, 1, 15), null),
            year,
            exit,
            null);

    private static CommitteeReviewDetailDto Review(CommitteeReviewState state, bool formative, DateOnly? periodTo = null)
        => new(
            30,
            "trainee-1",
            20,
            "Paediatrics CCC",
            new DateOnly(2026, 1, 1),
            periodTo ?? new DateOnly(2026, 6, 30),
            new DateOnly(2026, 7, 2),
            state,
            new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc),
            "chair-1",
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

    private static RecordingSender ReviewSender(CommitteeReviewDetailDto review)
        => new RecordingSender()
            .On<GetCommitteeReviewByIdQuery>(_ => review)
            .On<ListPendingEntrustmentDecisionsForReviewQuery>(_ => Array.Empty<PendingEntrustmentDecisionDto>())
            .On<GetSamplingConcentrationWarningsQuery>(_ => null)
            .On<CountMsfCampaignsOutsideSnapshotQuery>(_ => MsfCampaignsOutsideSnapshotDto.None)
            .On<GetEpaTrajectoryForTraineeQuery>(_ => Array.Empty<EpaTrajectoryDto>())
            .On<ListStarEpaOptionsForReviewQuery>(_ => Array.Empty<StarEpaOptionDto>())
            .On<GetEntrustmentScalesListQuery>(_ => Array.Empty<EntrustmentScaleDto>())
            .On<GetMsfCoverageForTraineeQuery>(_ => null);

    private static string Text(string text) => Whitespace().Replace(text, " ").Trim();

    /// <summary>Text, not markup: Razor's line breaks are layout, and markup encodes the middle dot.</summary>
    private static string PageText(IRenderedFragment cut) => Text(string.Join(" ", cut.Nodes.Select(node => node.TextContent)));

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

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
