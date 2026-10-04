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
using Wombat.Application.Features.Curricula.Quota;
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
        Text(row.Children[3].QuerySelector(".standing-rating-meta")!.TextContent).Should().Be($"Decided on O-R Scale, not {Cpsa}.");
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
        Text(cell.QuerySelector(".standing-rating-meta")!.TextContent).Should().Be("The curriculum sets no ladder for this EPA.");
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
        var link = cell.QuerySelector("a.standing-rating-link")!;
        link.GetAttribute("href").Should().Be("/activities/31");
        // T355 (C1): "3b · Encounter …", one 44px block at phone width. Under "Latest rating" the date is named as the
        // encounter's, or it reads as though the rating were not recorded.
        Text(link.TextContent).Should().Be("3b · Encounter not recorded (created 2026-03-20) — open this PAED-001 rating");
        Text(link.QuerySelector(".visually-hidden")!.TextContent).Should().Be("— open this PAED-001 rating");
        cell.QuerySelectorAll(".standing-rating-meta").Select(line => Text(line.TextContent)).Should().Equal(
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

        PageText(cut).Should().Contain("Your programme starts on 2027-01-15, so the targets shown are for training year 1.");
    }

    [Fact]
    public void AnEndedProgramme_OnTheTraineesOwnPage_SaysTheTargetsAreTheYearItEndedIn()
    {
        // T252: the page reads the standing as on the programme's last day, so the year is the one it ended in. "You are
        // in training year 3 on 20 August 2026" would read as though the programme were still running.
        var standing = Standing(Epa("PAED-001", decision: "3b", year: EntrustmentStandingStatus.AtOrAbove,
            exit: EntrustmentStandingStatus.Below)) with
        {
            AsOf = new DateOnly(2026, 8, 20),
            TargetYear = 3
        };

        var cut = RenderComponent<EntrustmentStandingPanel>(parameters => parameters
            .Add(panel => panel.Standing, standing)
            .Add(panel => panel.Self, true)
            .Add(panel => panel.ProgrammeEnd, new ProgrammeEndDto(Completed: false, EndedOn: new DateOnly(2026, 8, 20), Today: new DateOnly(2026, 9, 23))));

        PageText(cut).Should().Contain(
            "Your programme ended on 2026-08-20, in training year 3, so the targets shown are that year's. " +
            "STAR decisions and ratings are shown as they stand today.");
        PageText(cut).Should().NotContain("You are in training year");
    }

    [Fact]
    public void ACompletedProgramme_OnTheTraineesOwnPage_SaysItWasCompleted_AsThePagesNoticeDoes()
    {
        var standing = Standing(Epa("PAED-001", decision: "5", year: EntrustmentStandingStatus.AtOrAbove,
            exit: EntrustmentStandingStatus.AtOrAbove)) with
        {
            AsOf = new DateOnly(2026, 6, 30),
            TargetYear = 4
        };

        var cut = RenderComponent<EntrustmentStandingPanel>(parameters => parameters
            .Add(panel => panel.Standing, standing)
            .Add(panel => panel.Self, true)
            .Add(panel => panel.ProgrammeEnd, new ProgrammeEndDto(Completed: true, EndedOn: new DateOnly(2026, 6, 30), Today: new DateOnly(2026, 9, 23))));

        PageText(cut).Should().Contain(
            "You completed your programme on 2026-06-30, in training year 4, so the targets shown are that year's.");
    }

    [Fact]
    public void AnEndedProgrammeWhoseDayWasNeverRecorded_IsNotSaidToBeRunning()
    {
        // T252 review. A profile ended before T209 recorded the day is read on today. The page's notice says the programme
        // has ended, so "You are in training year 3 on 23 September 2026" beneath it would contradict it.
        var standing = Standing(Epa("PAED-001", decision: "3b", year: EntrustmentStandingStatus.AtOrAbove,
            exit: EntrustmentStandingStatus.Below)) with
        {
            AsOf = new DateOnly(2026, 9, 23),
            TargetYear = 3
        };

        var cut = RenderComponent<EntrustmentStandingPanel>(parameters => parameters
            .Add(panel => panel.Standing, standing)
            .Add(panel => panel.Self, true)
            .Add(panel => panel.ProgrammeEnd, new ProgrammeEndDto(Completed: false, EndedOn: null, Today: new DateOnly(2026, 9, 23))));

        PageText(cut).Should().Contain(
            "Your programme has ended, but Wombat did not record the day it ended, so the targets shown are for training " +
            "year 3, counted from your start date to 2026-09-23. STAR decisions and ratings are shown as they stand today.");
        PageText(cut).Should().NotContain("You are in training year");
    }

    [Fact]
    public void ARunningProgramme_OnTheTraineesOwnPage_SaysWhichYearTheyAreIn()
    {
        var cut = RenderComponent<EntrustmentStandingPanel>(parameters => parameters
            .Add(panel => panel.Standing, Standing(Epa("PAED-001", decision: null, year: EntrustmentStandingStatus.NoDecision,
                exit: EntrustmentStandingStatus.NoDecision)))
            .Add(panel => panel.Self, true)
            .Add(panel => panel.ProgrammeEnd, null));

        PageText(cut).Should().Contain("You are in training year 2 on 2026-03-01.");
        PageText(cut).Should().NotContain("programme ended");
    }

    [Fact]
    public void AFailedLoad_SaysSoInThePanel_InFixedWords_NeverTheExceptions()
    {
        // T355 (T329, T272): the failure's own text is the log's. The committee page offers no Try again of its own.
        var cut = RenderComponent<EntrustmentStandingPanel>(parameters => parameters
            .Add(panel => panel.LoadError, "The database is unavailable."));

        Text(cut.Find(".alert.alert-danger").TextContent).Should().Be(EntrustmentStandingPanel.OthersLoadFailed);
        cut.Markup.Should().NotContain("The database is unavailable.");
        cut.FindAll("button").Should().BeEmpty();
        cut.FindAll("table").Should().BeEmpty();
    }

    [Fact]
    public void AFailedLoad_OnTheTraineesOwnPage_OffersTryAgain()
    {
        var retried = 0;
        var cut = RenderComponent<EntrustmentStandingPanel>(parameters => parameters
            .Add(panel => panel.LoadError, EntrustmentStandingPanel.SelfLoadFailed)
            .Add(panel => panel.Self, true)
            .Add(panel => panel.OnRetry, () => retried++));

        Text(cut.Find(".alert.alert-danger .alert-row-text").TextContent).Should().Be(
            "Could not load your standing. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Find(".alert-row button").Click();
        retried.Should().Be(1);
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
        Text(rows[1].Children[1].QuerySelector(".standing-rating-meta")!.TextContent).Should().Be("Exit level; no year 2 level set");
        rows[0].Children[1].QuerySelector(".standing-rating-meta").Should().BeNull("PAED-001's target is the year's own");
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
        cut.FindAll("tbody tr").Select(row => (Text(row.Children[1].FirstChild!.TextContent), Text(row.Children[1].QuerySelector(".standing-rating-meta")!.TextContent)))
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
        Text(rows[0].Children[0].TextContent).Should().Be("PAED-001 — PAED-001 title");
        rows[0].Children[0].QuerySelector(".standing-rating-meta").Should().BeNull();
        Text(rows[1].Children[0].QuerySelector(".standing-rating-meta")!.TextContent)
            .Should().Be("The institution's own EPA; not in the exit rule");
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
            "The review period's last day, 2026-06-30, falls in training year 1, so the targets shown are that year's. " +
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
        text.Should().Contain("In training year 2 on 2026-03-01.");
        text.Should().NotContain("review period's last day");
    }

    [Fact]
    public void OnTheReviewPage_AnEpaNameLinksToItsChart_OnlyWhereThePageDrawsOne()
    {
        // T355, R4 (the integrator's wiring after wave 2): the panel links a name to the chart further down the page,
        // and only for an EPA the page charts; an EPA with no rating in the review's window keeps its name as text.
        SignIn("chair-1", WombatRoles.CommitteeMember);
        var charted = new EpaTrajectoryDto(1, "PAED-001", "PAED-001 title", true, null, null, [], []);
        var sender = ReviewSender(Review(CommitteeReviewState.InProgress, formative: false))
            .On<GetEpaTrajectoryForTraineeQuery>(_ => new[] { charted })
            .On<GetEntrustmentStandingForTraineeQuery>(_ => Standing(
                Epa("PAED-001", decision: "5", year: EntrustmentStandingStatus.AtOrAbove, exit: EntrustmentStandingStatus.AtOrAbove),
                Epa("PAED-010", decision: null, year: EntrustmentStandingStatus.NoDecision, exit: EntrustmentStandingStatus.NoDecision) with { EpaId = 10 }));
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderReview();

        var headers = cut.FindAll("tbody tr th[scope='row']").Where(th => th.Closest("section.trajectory-card") is null).ToList();
        headers.Should().HaveCount(2);
        headers[0].QuerySelector("a")!.GetAttribute("href").Should().Be("#trajectory-1-h");
        cut.FindAll("section#trajectory-1 #trajectory-1-h").Should().ContainSingle(
            "the link lands on the heading of the chart this page draws, so its summary is in view (4.15)");
        headers[1].QuerySelector("a").Should().BeNull("PAED-010 has no chart on this page");
    }

    [Fact]
    public void OnTheReviewPage_PressingAnEpaName_FocusesItsChart_AndNavigatesNowhere()
    {
        // T355, build review A1: <base href="/"> would resolve "#trajectory-1" against the site's root, and Blazor would
        // navigate to Home, losing the chair's unsaved decision. The link keeps its href for a page with no circuit and,
        // pressed, moves the focus to the chart by script (PageFocus, as RefusalSummary's links do, T342 A1).
        SignIn("chair-1", WombatRoles.CommitteeMember);
        JSInterop.SetupVoid(PageFocus.FocusByIdIdentifier, _ => true);
        var sender = ReviewSender(Review(CommitteeReviewState.InProgress, formative: false))
            .On<GetEpaTrajectoryForTraineeQuery>(_ => new[] { ChartedTrajectory() })
            .On<GetEntrustmentStandingForTraineeQuery>(_ => Standing(
                Epa("PAED-001", decision: "5", year: EntrustmentStandingStatus.AtOrAbove, exit: EntrustmentStandingStatus.AtOrAbove)));
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderReview();
        var navigation = Services.GetRequiredService<FakeNavigationManager>();
        var before = navigation.Uri;

        var link = cut.Find("tbody th[scope='row'] a.epa-link");
        link.GetAttribute("href").Should().Be("#trajectory-1-h");
        link.Attributes.Select(attribute => attribute.Name).Should().Contain(
            "blazor:onclick:preventdefault", "the browser must not follow the fragment against <base href=\"/\">");

        link.Click();

        JSInterop.Invocations.Where(call => call.Identifier == PageFocus.FocusByIdIdentifier)
            .Should().ContainSingle().Which.Arguments.Should().Equal("trajectory-1-h");
        navigation.Uri.Should().Be(before, "the link moves the focus, never the page");
        navigation.History.Should().BeEmpty();
    }

    [Fact]
    public void TheReviewPage_ChartsTheReviewsWindow_UnderAnH4_NamingTheTrainee_WithNoToday()
    {
        // T355, build review G2 (decision D2): the committee page reads the trajectory over the review's own period, heads
        // each chart one level below its section's h3, names the trainee and the window in the summary, and draws no
        // "Today", because the window is the review's, not today's.
        SignIn("chair-1", WombatRoles.CommitteeMember);
        var review = Review(CommitteeReviewState.InProgress, formative: false, periodTo: new DateOnly(2026, 12, 31)) with
        {
            TraineeName = "Lerato Molefe"
        };
        var sender = ReviewSender(review)
            .On<GetEpaTrajectoryForTraineeQuery>(query => new[]
            {
                ChartedTrajectory() with { WindowFrom = query.From, WindowTo = query.To }
            })
            .On<GetEntrustmentStandingForTraineeQuery>(_ => Standing(
                Epa("PAED-001", decision: "5", year: EntrustmentStandingStatus.AtOrAbove, exit: EntrustmentStandingStatus.AtOrAbove)));
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderReview();

        var query = sender.Received.OfType<GetEpaTrajectoryForTraineeQuery>().Single();
        query.From.Should().Be(review.ReviewPeriodFrom);
        query.To.Should().Be(review.ReviewPeriodTo);
        query.EpaId.Should().BeNull("the committee page charts every EPA");

        var card = cut.Find("section#trajectory-1");
        var section = card.ParentElement!.Closest("section.list-section")!;
        section.QuerySelector("h3")!.TextContent.Should().Be("Rating trajectory by EPA");
        section.ClassList.Should().NotContain("detail-card", "the charts are cards; their section is not a card around them");
        section.ClassList.Should().Contain("full-width",
            "the charts span the details grid, never its narrow column (the T355 replay's 4.15: a 267 px card at 1280)");
        Text(card.QuerySelector("h4")!.TextContent).Should().Be("PAED-001 — PAED-001 title");
        Text(card.TextContent).Should().Contain(
            "Lerato Molefe · 3 ratings in the review window, 2026-01-01 to 2026-12-31");
        card.QuerySelectorAll(".trajectory-chart-today").Should().BeEmpty("the committee page reads a review's window, not today");
        card.QuerySelectorAll("tbody tr").Should().HaveCount(3, "each rating is a row of the chart's table");
    }

    /// <summary>
    /// PAED-001 on the CPSA ladder with three ratings by named assessors, as the handler returns a charted EPA: never with
    /// no point (it returns only EPAs that have one).
    /// </summary>
    private static EpaTrajectoryDto ChartedTrajectory()
    {
        var rungs = new[] { "1", "2", "3a", "3b", "4", "5" }
            .Select((label, index) => new TrajectoryRungDto(index + 1, label))
            .ToArray();
        TrajectoryPointDto Point(int id, DateOnly on, int rating, string label) => new(id, on, true, rating, label, "Direct observation", "assessor-a")
        {
            AssessorName = "Dr Sipho Mahlangu",
            ActivityName = $"Mini-CEX (Paediatrics) · PAED-001 · {on:yyyy-MM-dd}",
            TrainingYear = 4,
            MinimumLabel = "5",
            AgainstMinimum = rating >= 6 ? TrajectoryAgainstMinimum.AtOrAbove : TrajectoryAgainstMinimum.Below
        };

        return new EpaTrajectoryDto(
            1, "PAED-001", "PAED-001 title", true, 42, Cpsa, rungs,
            [
                Point(101, new DateOnly(2026, 2, 10), 4, "3b"),
                Point(102, new DateOnly(2026, 5, 12), 5, "4"),
                Point(103, new DateOnly(2026, 9, 21), 6, "5")
            ])
        {
            ExitLevelOrder = 6,
            ExitLevelLabel = "5",
            MinimumSteps = [new TrajectoryMinimumStepDto(new DateOnly(2026, 1, 1), 4, 6, "5")]
        };
    }

    [Fact]
    public void TheReviewPage_StandsWhenTheStandingFails()
    {
        SignIn("chair-1", WombatRoles.CommitteeMember);
        var sender = ReviewSender(Review(CommitteeReviewState.InProgress, formative: false))
            .On<GetEntrustmentStandingForTraineeQuery>(_ => throw new InvalidOperationException("The database is unavailable."));
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderReview();

        cut.Markup.Should().Contain("Record decision");
        var section = cut.FindAll("section.detail-card").Single(card => card.QuerySelector("h3")?.TextContent == "Entrustment against Annexure A");
        cut.FindAll(".alert-danger").Where(alert => !section.Contains(alert)).Should().BeEmpty("the review itself loaded");
        Text(section.QuerySelector(".alert-danger")!.TextContent).Should().Be(EntrustmentStandingPanel.OthersLoadFailed);
        cut.Markup.Should().NotContain("The database is unavailable.", "the failure's text is the log's (T329)");
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

    // ─── T355: the panel changed once, for both pages (C1; R4) ─────────────

    [Fact]
    public void ThePanel_WithNoEpaHref_LeavesTheNameAsText()
    {
        // With no EpaHref, or none for a row (an EPA the committee page draws no chart for), the name is text (R4).
        var cut = RenderPanel(Standing(
            Epa("PAED-001", decision: "5", year: EntrustmentStandingStatus.AtOrAbove, exit: EntrustmentStandingStatus.AtOrAbove)));

        var header = cut.Find("tbody tr th[scope='row']");
        header.GetAttribute("role").Should().Be("rowheader");
        header.QuerySelector("a").Should().BeNull();
        Text(header.TextContent).Should().Be("PAED-001 — PAED-001 title");
    }

    [Fact]
    public void WithEpaHref_EachEpaNameIsALinkToWhereThePageSays()
    {
        var cut = RenderComponent<EntrustmentStandingPanel>(parameters => parameters
            .Add(panel => panel.Standing, Standing(
                Epa("PAED-001", decision: "5", year: EntrustmentStandingStatus.AtOrAbove, exit: EntrustmentStandingStatus.AtOrAbove),
                Epa("PAED-010", decision: null, year: EntrustmentStandingStatus.NoDecision, exit: EntrustmentStandingStatus.NoDecision) with { EpaId = 10 }))
            .Add(panel => panel.Self, true)
            .Add(panel => panel.EpaHref, epa => $"/portfolio/progress/{epa.EpaId}"));

        var links = cut.FindAll("tbody th[scope='row'] a.epa-link");
        links.Select(link => link.GetAttribute("href")).Should().Equal("/portfolio/progress/1", "/portfolio/progress/10");
        links.Select(link => Text(link.TextContent)).Should().Equal("PAED-001 — PAED-001 title", "PAED-010 — PAED-010 title");
        links.SelectMany(link => link.Attributes).Select(attribute => attribute.Name).Should().NotContain(
            "blazor:onclick", "a route is a real navigation: only an in-page link is pressed by script (A1)");
    }

    [Fact]
    public void ThePanel_Stacks_EachCellButTheEpaLabelledByItsColumn()
    {
        // R4: DataTable's Stack. The header row stays in the table for a screen reader; below 641px each cell shows its
        // column's name before its value, and the EPA, the row's header, needs none.
        var cut = RenderPanel(Standing(
            Epa("PAED-001", decision: "5", year: EntrustmentStandingStatus.AtOrAbove, exit: EntrustmentStandingStatus.AtOrAbove)));

        var table = cut.Find("table");
        table.ClassList.Should().Contain(["clinic-table", "clinic-table--stack"]);
        table.GetAttribute("role").Should().Be("table");
        cut.Find("tbody tr").GetAttribute("role").Should().Be("row");
        cut.FindAll("tbody tr td").Select(cell => cell.GetAttribute("data-label")).Should().Equal(
            "Year 2 target", "STAR decision", "Against target", "Exit level", "Latest rating");
        cut.Find("tbody tr th").HasAttribute("data-label").Should().BeFalse();
        Text(cut.Find("caption").TextContent).Should().Be(
            "Each EPA's STAR decision against the training year 2 target and the exit level, with the latest rating.");
    }

    [Fact]
    public void ADecisionsDates_AreIso_AndAnExpiryIsSaid()
    {
        // D1: "Issued 2026-10-03", never "3 Oct 2026".
        var cut = RenderPanel(Standing(
            Epa("PAED-001", decision: "5", year: EntrustmentStandingStatus.AtOrAbove, exit: EntrustmentStandingStatus.AtOrAbove),
            Epa("PAED-010", decision: "4", year: EntrustmentStandingStatus.Below, exit: EntrustmentStandingStatus.Below) with
            {
                Decision = new StandingDecisionDto(42, 5, "4", null, new DateOnly(2026, 10, 3), new DateOnly(2026, 10, 23))
            }));

        var rows = cut.FindAll("tbody tr").ToList();
        Text(rows[0].Children[2].QuerySelector(".standing-rating-meta")!.TextContent).Should().Be("Issued 2026-01-15");
        Text(rows[1].Children[2].QuerySelector(".standing-rating-meta")!.TextContent).Should().Be("Issued 2026-10-03, expires 2026-10-23");
        PageText(cut).Should().NotContain("Jan 2026").And.NotContain("Oct 2026");
    }

    [Fact]
    public void NoStarAndNoRating_ReadNone()
    {
        var cut = RenderPanel(Standing(
            Epa("PAED-003", decision: null, year: EntrustmentStandingStatus.NoDecision, exit: EntrustmentStandingStatus.NoDecision)));

        var row = cut.Find("tbody tr");
        Text(row.Children[2].TextContent).Should().Be("None");
        Text(row.Children[5].TextContent).Should().Be("None");
    }

    [Fact]
    public void TheSummary_IsStandingWordsYearLine()
    {
        // R1: Home's card and the panel say the summary once, in StandingWords' words.
        var standing = Standing(
            Epa("PAED-001", decision: "5", year: EntrustmentStandingStatus.AtOrAbove, exit: EntrustmentStandingStatus.AtOrAbove),
            Epa("PAED-010", decision: "4", year: EntrustmentStandingStatus.Below, exit: EntrustmentStandingStatus.Below),
            Epa("PAED-003", decision: null, year: EntrustmentStandingStatus.NoDecision, exit: EntrustmentStandingStatus.NoDecision));

        var cut = RenderPanel(standing);

        var summary = cut.FindAll("dl.details-list > div")
            .Single(row => Text(row.QuerySelector("dt")!.TextContent) == "STAR decisions against training year 2 targets");
        Text(summary.QuerySelector("dd")!.TextContent).Should().Be(StandingWords.YearLine(standing));
        Text(summary.QuerySelector("dd")!.TextContent).Should().Be("1 at or above · 1 below · 1 with no decision, of 3 EPAs");
    }

    // ─── The trainee's progress page ─────────────────────────────────────────

    [Fact]
    public void MyProgress_AsksForTheSignedInTraineesStanding_AsThatTrainee_AndLinksEachEpaToItsPage()
    {
        SignIn("trainee-1", WombatRoles.Trainee);
        var sender = new RecordingSender()
            .On<GetCurriculumProgressForTraineeQuery>(_ => ProgressSummary(ended: null))
            .On<GetEntrustmentStandingForTraineeQuery>(_ => Standing(
                Epa("PAED-001", decision: "3a", year: EntrustmentStandingStatus.Below, exit: EntrustmentStandingStatus.Below)))
            .On<GetMsfCoverageForTraineeQuery>(_ => null);
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<MyProgress>();
        cut.WaitForState(() => cut.Markup.Contains("Entrustment against Annexure A"));

        var query = sender.Received.OfType<GetEntrustmentStandingForTraineeQuery>().Should().ContainSingle().Subject;
        query.TraineeUserId.Should().Be("trainee-1");
        query.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be("trainee-1");
        var panel = cut.Find("section.standing-panel");
        Text(panel.TextContent).Should().Contain("You are in training year 2 on 2026-03-01.");
        panel.QuerySelectorAll("tbody .badge").Select(badge => Text(badge.TextContent)).Should().Equal("Below");
        panel.QuerySelector("tbody th a.epa-link")!.GetAttribute("href").Should().Be("/portfolio/progress/1");
    }

    [Fact]
    public void MyProgress_ForAnEndedProgramme_HandsThePanelTheEnd_SoItSaysTheYearTheProgrammeEndedIn()
    {
        // T252: the page reads the standing as on the last day and tells the panel how the programme ended. Without the
        // end the panel would say "You are in training year 2 on 2026-08-20" beneath a notice that it has ended.
        SignIn("trainee-1", WombatRoles.Trainee);
        var endedOn = new DateOnly(2026, 8, 20);
        Services.AddSingleton<IScopedSender>(new RecordingSender()
            .On<GetCurriculumProgressForTraineeQuery>(_ => ProgressSummary(new ProgrammeEndDto(Completed: false, EndedOn: endedOn, Today: new DateOnly(2026, 9, 23))))
            .On<GetEntrustmentStandingForTraineeQuery>(query => Standing(
                Epa("PAED-001", decision: "3a", year: EntrustmentStandingStatus.Below, exit: EntrustmentStandingStatus.Below)) with
            {
                AsOf = query.AsOf ?? new DateOnly(2026, 9, 23)
            })
            .On<GetMsfCoverageForTraineeQuery>(_ => null));

        var cut = RenderComponent<MyProgress>();
        cut.WaitForState(() => cut.Markup.Contains("Entrustment against Annexure A"));

        PageText(cut).Should().Contain("Your programme ended on 2026-08-20, in training year 2, so the targets shown are that year's.");
        PageText(cut).Should().NotContain("You are in training year");
    }

    [Fact]
    public void MyProgress_WithNothingOnRecord_DoesNotSayItTwice()
    {
        SignIn("trainee-1", WombatRoles.Trainee);
        Services.AddSingleton<IScopedSender>(new RecordingSender()
            .On<GetCurriculumProgressForTraineeQuery>(_ => null)
            .On<GetEntrustmentStandingForTraineeQuery>(_ => null)
            .On<GetMsfCoverageForTraineeQuery>(_ => null));

        var cut = RenderComponent<MyProgress>();
        cut.WaitForState(() => cut.Markup.Contains("No curriculum items assigned yet."));

        cut.Markup.Should().NotContain("Entrustment against Annexure A");
    }

    /// <summary>One in-force semester item (PAED-001, EPA id 1), running or ended.</summary>
    private static TraineeCurriculumProgressSummaryDto ProgressSummary(ProgrammeEndDto? ended)
    {
        var asOf = ended?.EndedOn ?? new DateOnly(2026, 3, 1);
        var window = new QuotaWindowDto(
            "Semester 1, 2026", "January to June", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), Wombat.Domain.Curricula.QuotaWindowStatus.Counting,
            1, 3, IsMet: false, Shortfall: 2, PercentOfTarget: 33, MinimumLevelReachedCount: 1, LastObservedOn: null,
            LastObservedOnDeclared: false, FirstCountedName: null, FirstCountedOn: null);
        var item = new TraineeCurriculumProgressDto(
            100, 1, "PAED-001", "PAED-001 title", Wombat.Domain.Curricula.QuotaPeriod.Semester, 3, window, null, 4, "3b", null,
            ended is null ? null : [window]);
        return new TraineeCurriculumProgressSummaryDto(
            asOf, new DateOnly(2025, 1, 1), 2, "Semester 1, 2026", "January to June", new DateOnly(2026, 6, 30),
            false, 0, 1, 0, 0, true, false, false, null, null, [item], ended);
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
            Semester = 1,
            // The page offers the chair's controls by what the query says the caller may do (T213); these tests act as the
            // chair.
            CallerChairs = true,
            CallerResolvesAppeals = true
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
