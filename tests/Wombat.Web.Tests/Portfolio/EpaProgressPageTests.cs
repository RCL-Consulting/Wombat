using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Application.Features.Activities.Queries.ListActivitiesBySubject;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Curricula.GetEpaProgressForTrainee;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Portfolio;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;
using static Wombat.Web.Tests.Progress.ProgressFixtures;

namespace Wombat.Web.Tests.Portfolio;

/// <summary>
/// One EPA's page under My progress (T355, flow 05; R2, R5; round 1 correction 2; C4, C5, C8, C9, E4; R3-C-Epa's twelve
/// states for the cast on D, 2026-10-03). Its reads are the caller's own; an id that is not theirs is "Page not found".
/// </summary>
public sealed class EpaProgressPageTests : TestContext
{
    private const string UserId = "trainee-1";
    private const int EpaId = 2;

    private static readonly DateOnly From = new(2026, 1, 1);
    private static readonly DateOnly To = new(2026, 12, 31);

    private readonly ScriptedSender _sender = new();
    private readonly List<object> _sent = [];

    public EpaProgressPageTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<IScopedSender>(_sender);
        _sender.Sent = _sent;
    }

    // ---- typical: Lerato Molefe's PAED-001, met, a STAR at or above, three ratings (R3-E-typical) ----

    [Fact]
    public void Typical_HeadsThePageWithTheEpa_TheTabTheSameWords_AndTheCrumbItsCode()
    {
        var cut = RenderTypical();

        Text(cut.Find("h1")).Should().Be("PAED-001 — Providing paediatric emergency care to children");
        cut.Find("p.page-subtitle").TextContent.Should().Be("3 a semester · Decided each semester · Exit level 5");
        TabTitle.Of(this, cut).Should().Be("PAED-001 — Providing paediatric emergency care to children · Wombat");
        cut.Find("p[role='status']").TextContent.Should().BeEmpty("the status is filled only while the EPA loads");
    }

    [Fact]
    public void Typical_Observations_GiveTheCount_ItsBar_AndTheLinesUnderIt()
    {
        var cut = RenderTypical();

        var observations = Section(cut, "obs-h");
        Text(observations.QuerySelector(".epa-figure-row")!).Should().StartWith("3 of 3 this semester");
        var bar = observations.QuerySelector(".progress-bar[role='progressbar']")!;
        bar.GetAttribute("aria-label").Should().Be("3 of 3 this semester");
        bar.QuerySelector(".progress-bar-fill")!.ClassList.Should().Contain("is-complete");
        observations.QuerySelector("p.progress-row-meta")!.TextContent.Should().Be("Target met for Semester 2, 2026.");
        observations.QuerySelectorAll(".epa-lines p").Select(Text).Should().Equal(
            "At the minimum level when observed: 2 of 3",
            "Last encounter 2026-09-24",
            "Semester 1, 2026: 0 of 3, 3 short",
            "Training year 4: level 5, the minimum each encounter is judged against and your STAR's target.",
            "MSF in Semester 2, 2026: no released campaign covering this EPA has closed yet. " +
            "MSF in Semester 1, 2026: no campaign covering this EPA that closed in the semester has been released.");
    }

    [Fact]
    public void Typical_Entrustment_GivesTheStar_ItsVerdict_ItsIssueAndTheExitLevel_AndTheWayToMyAuthorisations()
    {
        var cut = RenderTypical();

        var entrustment = Section(cut, "ent-h");
        entrustment.QuerySelector(".star-level .dashboard-metric-value")!.TextContent.Should().Be("5");
        var badge = entrustment.QuerySelector(".star-level .badge")!;
        badge.TextContent.Should().Be("At or above");
        badge.ClassList.Should().Contain("badge-standing-met");
        Details(entrustment).Should().Equal(("Issued", "2026-10-03"), ("Exit level", "5 · reached"));
        var link = entrustment.QuerySelector("a.epa-auth-link")!;
        link.TextContent.Should().Be("Open My authorisations");
        link.GetAttribute("href").Should().Be("/portfolio/authorisations");
    }

    [Fact]
    public void Typical_DrawsTheChartOverTheAcademicYear_WithToday_UnderAnH2()
    {
        var cut = RenderTypical();

        var query = _sent.OfType<GetEpaTrajectoryForTraineeQuery>().Single();
        (query.TraineeUserId, query.From, query.To, query.EpaId).Should().Be((UserId, From, To, EpaId));

        var heading = cut.Find("section.trajectory-card h2");
        heading.TextContent.Should().Be("Rating trajectory");
        cut.Find("section.trajectory-card").Id.Should().Be("trajectory-2");
        cut.FindAll("svg.trajectory-chart--wide line.trajectory-chart-today").Should().ContainSingle();
        cut.Find("div.trajectory-figure").GetAttribute("aria-label").Should().Be("Rating chart for PAED-001");
    }

    [Fact]
    public void Typical_ListsTheActivitiesOnTheEpa_EachA44pxBlockLink_WithItsStateAndCredit()
    {
        var cut = RenderTypical();

        var query = _sent.OfType<ListActivitiesBySubjectQuery>().Single();
        (query.SubjectUserId, query.Page, query.PageSize, query.EpaId).Should().Be((UserId, 1, ListActivitiesBySubjectQuery.MaxPageSize, EpaId));

        var section = Section(cut, "acts-h");
        section.QuerySelector("h2")!.TextContent.Should().Be("Activities on this EPA");
        var table = section.QuerySelector("table.clinic-table.clinic-table--stack")!;
        table.QuerySelector("caption.visually-hidden")!.TextContent.Should().Be(
            "Your finished activities on this EPA, newest encounter first");
        section.QuerySelectorAll("p.progress-row-meta").Should().BeEmpty("the list holds every one");
        var rows = table.QuerySelectorAll("tbody tr");
        rows.Select(row => row.QuerySelector("a.activity-link.activity-block-link")!.GetAttribute("href"))
            .Should().Equal("/activities/13", "/activities/12", "/activities/11");
        Text(rows[0].QuerySelector("a")!).Should().Be("DOPS (Paediatrics) · PAED-001 · 2026-09-24, to Mohammed Patel");
        rows[0].QuerySelectorAll("td").Skip(1).Select(Text).Should().Equal("Completed", "1 item");
        rows[0].QuerySelector("td[data-label='Credit']").Should().NotBeNull();
        section.QuerySelectorAll(".detail-card--empty").Should().BeEmpty();
    }

    [Fact]
    public void MoreActivitiesThanTheListReads_SaysHowManyOfHowMany()
    {
        // T355, build review R1, D4: the list is the first page of ListActivitiesBySubjectQuery.MaxPageSize, newest first;
        // a registrar with more than that on one EPA is told how many are listed of how many, not that the list is every one.
        var rows = Enumerable.Range(0, 3)
            .Select(index => Completed(20 + index, "Mini-CEX (Paediatrics)", new DateOnly(2026, 9, 21 - index), "Thandi Zulu"))
            .ToList();
        Answer(Typical(), standing: Standing(Star("5")), trajectory: MolefeThree());
        _sender.On<ListActivitiesBySubjectQuery>(_ => new ActivityListPageDto(rows, 1, rows.Count, 130));
        SignIn(WombatRoles.Trainee);

        var cut = RenderComponent<EpaProgress>(parameters => parameters.Add(page => page.EpaId, EpaId));

        var section = Section(cut, "acts-h");
        section.QuerySelectorAll("tbody tr").Should().HaveCount(3);
        Text(section.QuerySelector("p.progress-row-meta")!).Should().Be("The newest 3 of 130 are listed.");
    }

    // ---- one rating, below STAR, no rating, December, once a year ----

    [Fact]
    public void OneRating_SaysSoFar_AndNoStarYet()
    {
        var cut = RenderPage(Dlamini(Item("PAED-001", QuotaPeriod.Semester, 3,
                Counting(Semester2Of2026, 1, 3, minimumReached: 1, lastObservedOn: new(2026, 9, 23)),
                Counting(Semester1Of2026, 0, 3), minimum: "4")),
            trajectory: Trajectory(Rating(21, new(2026, 9, 23), 5, "4", "David Naidoo", TrajectoryAgainstMinimum.AtOrAbove, 3, "4")),
            activities: [Completed(21, "Mini-CEX (Paediatrics)", new(2026, 9, 23), "David Naidoo")]);

        Section(cut, "obs-h").QuerySelector("p.progress-row-meta")!.TextContent.Should().Be("2 more by 2026-11-30.");
        Section(cut, "ent-h").QuerySelector("p")!.TextContent.Should().Be("No STAR yet.");
        cut.FindAll("a.epa-auth-link").Should().BeEmpty("there is no STAR to open");
        Text(cut.Find(".trajectory-head p")).Should().Be("1 rating so far, from David Naidoo. At the minimum.");
    }

    [Fact]
    public void BelowStar_GivesItsExpiry_AndSaysMsfIsNotPlotted_BesideAnMsfRecord()
    {
        var cut = RenderPage(Molefe(Item("PAED-010", QuotaPeriod.Semester, 3,
                Counting(Semester2Of2026, 1, 3, minimumReached: 1, lastObservedOn: new(2026, 9, 29)), Counting(Semester1Of2026, 0, 3),
                title: "Leading and operating within a clinical team")),
            standing: Standing(Star("4", EntrustmentStandingStatus.Below, EntrustmentStandingStatus.Below, expires: new DateOnly(2026, 10, 23))),
            trajectory: Trajectory(Rating(31, new(2026, 9, 29), 6, "5", "Thandi Zulu", TrajectoryAgainstMinimum.AtOrAbove, 4, "5")),
            activities:
            [
                Completed(32, "Multi-Source Feedback (Paediatrics)", new(2026, 10, 3), null, "PAED-010", MsfEvidenceKinds.MsfActivityTypeKey, credit: null, state: "recorded", stateLabel: "Recorded"),
                Completed(31, "Direct Observation (Paediatrics)", new(2026, 9, 29), "Thandi Zulu", "PAED-010")
            ]);

        var entrustment = Section(cut, "ent-h");
        entrustment.QuerySelector(".star-level .badge")!.TextContent.Should().Be("Below");
        Details(entrustment).Should().Equal(("Issued", "2026-10-03"), ("Expires", "2026-10-23"), ("Exit level", "5 · not yet"));
        cut.Find("p.trajectory-msf").TextContent.Should().Be("Multi-source feedback is not plotted.");
        cut.FindAll("table.clinic-table--stack").Last().QuerySelectorAll("tbody tr").First().QuerySelectorAll("td").Skip(1).Select(Text)
            .Should().Equal("Recorded", "—");
    }

    [Fact]
    public void NoRating_SaysSo_AndNoActivity_OffersLogAnActivity()
    {
        var cut = RenderPage(Dlamini(Item("PAED-002", QuotaPeriod.Semester, 3, Counting(Semester2Of2026, 0, 3), Counting(Semester1Of2026, 0, 3),
            title: "Managing common paediatric presentations", minimum: "4")));

        Section(cut, "obs-h").QuerySelector("p.progress-row-meta")!.TextContent.Should().Be("3 more by 2026-11-30.");
        Section(cut, "obs-h").QuerySelectorAll(".epa-lines p").Select(Text).First().Should().Be("Semester 1, 2026: 0 of 3, 3 short");
        cut.Find("section.trajectory-card h2").TextContent.Should().Be("Rating trajectory");
        cut.Find("section.trajectory-card p.card-empty").TextContent.Should().Be("No rating yet.");

        var empty = Section(cut, "acts-h").QuerySelector(".detail-card.detail-card--empty")!;
        empty.QuerySelector(".state-panel-title")!.TextContent.Should().Be("No activity on this EPA yet.");
        var log = empty.QuerySelector("a.btn")!;
        log.TextContent.Should().Be("Log an activity");
        log.GetAttribute("href").Should().Be("/activities/new");
    }

    [Fact]
    public void InDecember_TheCountSaysDecemberStillCounts()
    {
        var item = Item("PAED-002", QuotaPeriod.Semester, 3, Counting(Semester2Of2026, 0, 3), Counting(Semester1Of2026, 0, 3),
            title: "Managing common paediatric presentations", minimum: "4");

        var cut = RenderPage(Dlamini(item) with { AsOf = December, IsAfterTeachingYear = true });

        Section(cut, "obs-h").QuerySelector("p.progress-row-meta")!.TextContent.Should().Be(
            "3 more; encounters in December still count towards Semester 2, 2026.");
    }

    [Fact]
    public void OnceAYear_CountsInItsYear()
    {
        var item = Item("PAED-008", QuotaPeriod.AcademicYear, 1, Counting(Year2026, 0, 1), Counting(Year2025, 0, 1),
            title: "Evaluating and managing neurodevelopmental and behavioural presentations in children", minimum: "4") with
        {
            DecisionCadence = QuotaPeriod.AcademicYear,
            DecisionIsOpportunistic = true,
            ExitLevelLabel = "4"
        };

        var cut = RenderPage(Molefe(item));

        cut.Find("p.page-subtitle").TextContent.Should().Be("1 a year · Decided as opportunity allows · Exit level 4");
        Text(Section(cut, "obs-h").QuerySelector(".epa-figure-row")!).Should().StartWith("0 of 1 in 2026");
        Section(cut, "obs-h").QuerySelector("p.progress-row-meta")!.TextContent.Should().Be("1 more by 2026-11-30.");
        Section(cut, "obs-h").QuerySelectorAll(".epa-lines p").Select(Text).First().Should().Be("2025 academic year: 0 of 1, 1 short");
    }

    // ---- local item, paused, other scale ----

    [Fact]
    public void ALocalItem_CarriesItsInstitutionsBadge_ItsOwnMinimum_AndIsNotInTheExitRule()
    {
        var item = Item("KGK-001", QuotaPeriod.AcademicYear, 1, Counting(Year2026, 0, 1), Counting(Year2025, 0, 1),
            title: "Running a paediatric outreach clinic at a district hospital", minimum: "3a") with
        {
            DecisionCadence = null,
            OwningInstitutionName = "Kgosi Kgari Teaching Hospital",
            MinimumByTrainingYear = false
        };

        var cut = RenderPage(Dlamini(item));

        var badge = cut.Find("h1 span.badge");
        badge.TextContent.Should().Be("Kgosi Kgari Teaching Hospital's own");
        badge.ClassList.Should().Contain("badge-draft");
        cut.Find("p.page-subtitle").TextContent.Should().Be("1 a year · Not in the College's exit rule");
        cut.Find(".epa-level-line").TextContent.Should().Be("Minimum 3a");
        Section(cut, "ent-h").QuerySelectorAll("p").Select(Text).Should().Equal("No STAR yet.", "Not in the College's exit rule.");
        TabTitle.Of(this, cut).Should().Be("KGK-001 — Running a paediatric outreach clinic at a district hospital · Wombat");
    }

    [Fact]
    public void APausedEpa_IsMarkedInItsH1AndTab_SaysWhyItHasNoCount_AndIsNotInTheStanding()
    {
        var item = Item("PAED-012", QuotaPeriod.Semester, 3, Counting(Semester2Of2026, 0, 3), Counting(Semester1Of2026, 0, 3),
            title: "Communicating with and counselling patients, caregivers and healthcare teams", minimum: "4") with { EpaInForce = false };

        var cut = RenderPage(Dlamini(item),
            trajectory: Trajectory(Rating(41, new(2026, 10, 3), 5, "4", "Mohammed Patel", TrajectoryAgainstMinimum.AtOrAbove, 3, "4")) with { EpaInForce = false },
            activities: [Completed(41, "Mini-CEX (Paediatrics)", new(2026, 10, 3), "Mohammed Patel", "PAED-012", credit: 0, inForce: false)]);

        var h1 = cut.Find("h1");
        Text(h1).Should().Be("PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams (no longer in use)");
        h1.QuerySelector("span.paused-mark")!.TextContent.Should().Be("(no longer in use)");
        TabTitle.Of(this, cut).Should().Be(
            "PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams (no longer in use) · Wombat");
        Text(cut.Find(".epa-page-alert .alert.alert-info")).Should().Be(
            "Paused by the College. It is not a target while it is paused. Its ratings and the credit it had earned are kept, " +
            "and what is completed on it meanwhile is credited if it is restored.");

        var observations = Section(cut, "obs-h");
        observations.QuerySelectorAll(".epa-figure-row, p.progress-row-meta").Should().BeEmpty("a paused EPA has no count (N6)");
        observations.QuerySelectorAll(".epa-lines p").Select(Text).Should().Equal(
            "Training year 3: level 4, the minimum each encounter is judged against and your STAR's target.");
        Section(cut, "ent-h").QuerySelector("p")!.TextContent.Should().Be(
            "While PAED-012 is paused it is not in your standing against Annexure A.");
        Text(cut.Find("section.trajectory-card h2")).Should().Be("Rating trajectory (no longer in use)");
        cut.Find("span.activity-cell-paused").TextContent.Should().Be("PAED-012 (no longer in use)");
    }

    [Fact]
    public void ARatingOnAnotherScale_CountsTowardsTheNumber_NotTheLevel()
    {
        var item = Item("PAED-005", QuotaPeriod.Semester, 3,
            Counting(Semester2Of2026, 2, 3, minimumReached: 0, lastObservedOn: new(2026, 9, 12)), Counting(Semester1Of2026, 0, 3),
            title: "Providing neonatal care in intensive and high-care settings", minimum: "4");

        var cut = RenderPage(Dlamini(item), trajectory: Trajectory(
            Rating(51, new(2026, 8, 10), 4, "3b", "Sarah Botha", TrajectoryAgainstMinimum.NotComparable, 3, null)
                with { OffLadder = true, OtherScaleName = "O-R Scale", OtherScaleRatingLabel = "Independent" },
            Rating(52, new(2026, 9, 12), 4, "3b", "Mohammed Patel", TrajectoryAgainstMinimum.Below, 3, "4")));

        Section(cut, "obs-h").QuerySelectorAll(".epa-lines p").Select(Text).First().Should().Be("At the minimum level when observed: 0 of 2");
        cut.Find("table.trajectory-table tbody tr td").TextContent.Should().Be("Independent on O-R Scale");
    }

    // ---- who reads it ----

    [Fact]
    public void AGraduate_ReadsTheStar_ButIsNotOfferedMyAuthorisations()
    {
        var cut = RenderTypical(role: null);

        Section(cut, "ent-h").QuerySelector(".star-level")!.Should().NotBeNull();
        cut.FindAll("a.epa-auth-link").Should().BeEmpty("My authorisations admits the Trainee role alone (E4)");
    }

    [Fact]
    public void AnEndedProgramme_ListsEveryPeriodReadOnly_WithNoCountOrMoreBy()
    {
        var item = Item("PAED-001", QuotaPeriod.Semester, 3, Counting(Semester2Of2026, 1, 3), Counting(Semester1Of2026, 3, 3, minimumReached: 2),
            periods: [Counting(Semester2Of2026, 1, 3), Counting(Semester1Of2026, 3, 3, minimumReached: 2)]);
        var ended = new ProgrammeEndDto(Completed: false, EndedOn: new DateOnly(2026, 10, 1), Today: D);

        var cut = RenderPage(Molefe(item) with { AsOf = new DateOnly(2026, 10, 1), Ended = ended });

        var observations = Section(cut, "obs-h");
        observations.QuerySelectorAll(".epa-figure-row, .progress-bar, p.progress-row-meta").Should().BeEmpty("R5: read-only");
        observations.QuerySelectorAll(".epa-lines p").Select(Text).Take(2).Should().Equal(
            "Semester 2, 2026: 1 of 3 so far; 0 at the minimum level when observed",
            "Semester 1, 2026: 3 of 3, met; 2 at the minimum level when observed");
        cut.Markup.Should().NotContain("more by");
    }

    [Fact]
    public void AnIdThatIsNotTheCallers_IsFlow01sPageNotFound_WordForWord()
    {
        _sender.On<GetEpaProgressForTraineeQuery>(_ => null);
        SignIn(WombatRoles.Trainee);

        var cut = RenderComponent<EpaProgress>(parameters => parameters.Add(page => page.EpaId, 999));

        Text(cut.Find("h1")).Should().Be("Page not found");
        cut.Find(".system-panel").QuerySelectorAll("p").Select(Text).Should().Equal(
            "There is no page at this address.", "Check the address, or start again from Home.");
        Text(cut.Find(".system-panel a.btn.btn-primary")).Should().Be("Go to Home");
        TabTitle.Of(this, cut).Should().Be("Page not found · Wombat");
        _sent.OfType<GetEpaTrajectoryForTraineeQuery>().Should().BeEmpty("nothing else is read for an id that is not the caller's");
    }

    // ---- loading, load error, retry (C8; T329, T272) ----

    [Fact]
    public void WhileItLoads_TheH1AndTheStatusSaySo()
    {
        var pending = new TaskCompletionSource<EpaProgressDto?>();
        _sender.OnAsync<GetEpaProgressForTraineeQuery>(_ => pending.Task);
        SignIn(WombatRoles.Trainee);

        var cut = RenderComponent<EpaProgress>(parameters => parameters.Add(page => page.EpaId, EpaId));

        Text(cut.Find("h1")).Should().Be("Loading this EPA");
        cut.Find("p[role='status']").TextContent.Should().Be("Loading this EPA.");
        TabTitle.Of(this, cut).Should().Be("Loading this EPA · Wombat");
        cut.FindAll(".skeleton").Should().NotBeEmpty();
    }

    [Fact]
    public void AFailedRead_IsThePagesLoadError_InFixedWords_NeverTheExceptionsText()
    {
        _sender.On<GetEpaProgressForTraineeQuery>(_ => Typical());
        _sender.On<GetEntrustmentStandingForTraineeQuery>(_ => throw new InvalidOperationException("relation \"entrustment\" does not exist"));
        SignIn(WombatRoles.Trainee);

        var cut = RenderComponent<EpaProgress>(parameters => parameters.Add(page => page.EpaId, EpaId));

        Text(cut.Find("h1")).Should().Be("EPA");
        Text(cut.Find(".alert.alert-danger .alert-row-text")).Should().Be(
            "Could not load this EPA. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain("relation");
        TabTitle.Of(this, cut).Should().Be("EPA · Wombat");
    }

    [Fact]
    public void TryAgain_ReadsAgain_AndTheLoadedH1TakesTheFocus()
    {
        var fail = true;
        Answer(Typical());
        _sender.On<GetEntrustmentStandingForTraineeQuery>(_ => fail ? throw new InvalidOperationException("down") : Standing(Star("5")));
        SignIn(WombatRoles.Trainee);
        var cut = RenderComponent<EpaProgress>(parameters => parameters.Add(page => page.EpaId, EpaId));

        fail = false;
        cut.Find(".alert-row button").Click();

        cut.WaitForAssertion(() => Text(cut.Find("h1")).Should().Be("PAED-001 — Providing paediatric emergency care to children"));
        JSInterop.VerifyInvoke(PageFocus.FocusHeadingIdentifier);
    }

    // ---- the cast ----

    private IRenderedComponent<EpaProgress> RenderTypical(string? role = WombatRoles.Trainee)
    {
        Answer(Typical(), standing: Standing(Star("5")), trajectory: MolefeThree(), activities:
        [
            Completed(13, "DOPS (Paediatrics)", new(2026, 9, 24), "Mohammed Patel"),
            Completed(12, "Case-Based Discussion (Paediatrics)", new(2026, 9, 22), "David Naidoo"),
            Completed(11, "Mini-CEX (Paediatrics)", new(2026, 9, 21), "Thandi Zulu")
        ]);
        SignIn(role);
        return RenderComponent<EpaProgress>(parameters => parameters.Add(page => page.EpaId, EpaId));
    }

    private IRenderedComponent<EpaProgress> RenderPage(
        EpaProgressDto model,
        EntrustmentStandingDto? standing = null,
        EpaTrajectoryDto? trajectory = null,
        IReadOnlyList<ActivitySummaryDto>? activities = null)
    {
        Answer(model, standing, trajectory, activities ?? []);
        SignIn(WombatRoles.Trainee);
        return RenderComponent<EpaProgress>(parameters => parameters.Add(page => page.EpaId, EpaId));
    }

    private void Answer(
        EpaProgressDto model,
        EntrustmentStandingDto? standing = null,
        EpaTrajectoryDto? trajectory = null,
        IReadOnlyList<ActivitySummaryDto>? activities = null)
    {
        _sender.On<GetEpaProgressForTraineeQuery>(_ => model);
        _sender.On<GetEntrustmentStandingForTraineeQuery>(_ => standing);
        _sender.On<GetEpaTrajectoryForTraineeQuery>(_ => trajectory is null ? Array.Empty<EpaTrajectoryDto>() : new[] { trajectory });
        _sender.On<GetMsfCoverageForTraineeQuery>(_ => Coverage());
        var rows = activities ?? [];
        _sender.On<ListActivitiesBySubjectQuery>(_ => new ActivityListPageDto(rows, 1, ListActivitiesBySubjectQuery.MaxPageSize, rows.Count));
    }

    private void SignIn(string? role)
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized($"{UserId}@test");
        if (role is not null)
        {
            auth.SetRoles(role);
        }

        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, UserId));
    }

    private static EpaProgressDto Typical() => Molefe(Item("PAED-001", QuotaPeriod.Semester, 3,
        Counting(Semester2Of2026, 3, 3, minimumReached: 2, lastObservedOn: new(2026, 9, 24)), Counting(Semester1Of2026, 0, 3)));

    private static EpaProgressDto Molefe(TraineeCurriculumProgressDto item)
        => new(Cadenced(item), D, new DateOnly(2023, 1, 14), 4, false, null);

    private static EpaProgressDto Dlamini(TraineeCurriculumProgressDto item)
        => new(Cadenced(item), D, new DateOnly(2024, 1, 14), 3, false, null);

    // The fixture's items carry no cadence; the cast's semester items are decided each semester.
    private static TraineeCurriculumProgressDto Cadenced(TraineeCurriculumProgressDto item)
        => item.DecisionCadence is null && !item.DecisionIsOpportunistic && !item.IsLocal && item.IsPerSemester
            ? item with { DecisionCadence = QuotaPeriod.Semester }
            : item;

    private static EntrustmentStandingDto Standing(EpaStandingDto epa)
        => new(D, new DateOnly(2023, 1, 14), 4, false, [epa], new ExitRuleReadinessDto(1, 0, [], []));

    private static EpaStandingDto Star(
        string level,
        EntrustmentStandingStatus year = EntrustmentStandingStatus.AtOrAbove,
        EntrustmentStandingStatus exit = EntrustmentStandingStatus.AtOrAbove,
        DateOnly? expires = null)
        => new(1, EpaId, "PAED-001", "Providing paediatric emergency care to children", "CPSA Paediatric Entrustment Scale v11.1",
            false, 6, "5", false, 6, "5", new StandingDecisionDto(7, 6, level, null, D, expires), year, exit, null);

    private static MsfCoverageDto Coverage() => new(
        D,
        [
            new MsfCoveragePeriodDto(2026, 1, "Semester 1, 2026", "January to June", new(2026, 1, 1), new(2026, 6, 30), new(2026, 6, 30), HasEnded: true, EpasCovered: 0),
            new MsfCoveragePeriodDto(2026, 2, "Semester 2, 2026", "July to November", new(2026, 7, 1), new(2026, 12, 31), new(2026, 11, 30), HasEnded: false, EpasCovered: 0)
        ],
        [
            new MsfEpaCoverageDto(1, EpaId, "PAED-001", "Providing paediatric emergency care to children", IsLocal: false,
                [new MsfEpaPeriodCoverageDto(2026, 1, []), new MsfEpaPeriodCoverageDto(2026, 2, [])])
        ]);

    private static EpaTrajectoryDto MolefeThree() => Trajectory(
        Rating(11, new(2026, 9, 21), 6, "5", "Thandi Zulu", TrajectoryAgainstMinimum.AtOrAbove, 4, "5"),
        Rating(12, new(2026, 9, 22), 6, "5", "David Naidoo", TrajectoryAgainstMinimum.AtOrAbove, 4, "5"),
        Rating(13, new(2026, 9, 24), 5, "4", "Mohammed Patel", TrajectoryAgainstMinimum.Below, 4, "5"));

    private static EpaTrajectoryDto Trajectory(params TrajectoryPointDto[] points) => new(
        EpaId, "PAED-001", "Providing paediatric emergency care to children", true, 1, "CPSA Paediatric Entrustment Scale v11.1",
        [new(1, "1"), new(2, "2"), new(3, "3a"), new(4, "3b"), new(5, "4"), new(6, "5")],
        points)
    {
        ExitLevelOrder = 6,
        ExitLevelLabel = "5",
        MinimumSteps = [new(From, 3, 5, "4"), new(new DateOnly(2026, 1, 14), 4, 6, "5")],
        WindowFrom = From,
        WindowTo = To
    };

    private static TrajectoryPointDto Rating(
        int activityId, DateOnly observedOn, int rating, string label, string assessor, TrajectoryAgainstMinimum against, int year, string? minimum)
        => new(activityId, observedOn, ObservedOnDeclared: true, rating, label, "Direct observation", assessor)
        {
            AssessorName = assessor,
            ActivityName = $"Mini-CEX (Paediatrics) · PAED-001 · {observedOn:yyyy-MM-dd}",
            TrainingYear = year,
            MinimumLabel = minimum,
            AgainstMinimum = against
        };

    private static ActivitySummaryDto Completed(
        int id, string typeName, DateOnly observedOn, string? nominee, string epaCode = "PAED-001", string typeKey = "mini_cex_cpsa",
        int? credit = 1, string state = "completed", string stateLabel = "Completed", bool inForce = true)
        => new(id, 2, typeKey, typeName, UserId, state, stateLabel, ActivityRows.When, ActivityRows.When, EpaId, epaCode,
            "Providing paediatric emergency care to children", inForce, observedOn, true, credit)
        {
            Holder = new ActivityHolderDto(ActivityHolderKind.Done, null, null, false, ActivityRows.When),
            NomineeName = nominee,
            DisplayName = $"{typeName} · {epaCode} · {observedOn:yyyy-MM-dd}",
            IsFinished = true
        };

    private static IElement Section(IRenderedFragment cut, string headingId)
        => cut.Find($"section[aria-labelledby='{headingId}']");

    private static List<(string Term, string Value)> Details(IElement section)
        => section.QuerySelectorAll("dl.details-list > div")
            .Select(row => (Text(row.QuerySelector("dt")!), Text(row.QuerySelector("dd")!)))
            .ToList();

    private static string Text(IElement element)
        => string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    /// <summary>Answers each read by its type; the last answer given for a type wins, and every request is kept.</summary>
    private sealed class ScriptedSender : IScopedSender
    {
        private readonly Dictionary<Type, Func<object, Task<object?>>> _answers = [];

        public List<object> Sent { get; set; } = [];

        public void On<TRequest>(Func<TRequest, object?> answer)
            => _answers[typeof(TRequest)] = request => Task.FromResult(answer((TRequest)request));

        public void OnAsync<TRequest>(Func<TRequest, Task<EpaProgressDto?>> answer)
            => _answers[typeof(TRequest)] = async request => await answer((TRequest)request);

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Sent.Add(request);
            return _answers.TryGetValue(request.GetType(), out var answer)
                ? (TResponse)(await answer(request))!
                : throw new InvalidOperationException($"The page sent {request.GetType().Name}, which this test does not answer.");
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException($"The page sent {request.GetType().Name}.");
    }
}
