using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Dashboards.Trainee;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Dashboards;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;
using static Wombat.Web.Tests.Dashboards.TraineeHomeFixtures;
using static Wombat.Web.Tests.Progress.ProgressFixtures;

namespace Wombat.Web.Tests.Dashboards;

/// <summary>
/// T355, flow 05 (R1; Q2, Q3; R3-C-Home and its R3-H-* states): the Trainee's Home is four cards in order, Your targets,
/// Needs you, Recent decisions and My authorisations; each card's rows are its links, and no card is a link around them
/// (T280). Drawn from hand-built read models for the cast on 2026-10-03 (C6): Anele Dlamini (training year 3), Lerato
/// Molefe (training year 4), Sipho Ndlovu (training year 1) and Pieter du Plessis (ended).
/// </summary>
/// <remarks>
/// The loading and load-error frames and the graduate's card are Home's, held for every role by <c>HomeFrameTests</c>;
/// the other-role line by <c>AssessorHomeTests</c>. Needs you's rows are flow 03's (<c>NeedsYouCardTests</c>).
/// </remarks>
public sealed class TraineeHomeTests : WombatTestContext
{
    private const string Paed012 = "PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams";

    private static readonly IReadOnlyDictionary<string, string> EpaTitles = new Dictionary<string, string>
    {
        ["PAED-001"] = "Providing paediatric emergency care to children",
        ["PAED-002"] = "Managing common paediatric presentations",
        ["PAED-003"] = "Providing intensive care to children",
        ["PAED-004"] = "Managing common neonatal conditions",
        ["PAED-005"] = "Providing neonatal care in intensive and high-care settings",
        ["PAED-006"] = "Managing long-term health conditions (LTHCs)",
        ["PAED-007"] = "Maintaining and promoting the health and well-being of children",
        ["PAED-009"] = "Managing children with surgical conditions",
        ["PAED-010"] = "Leading and operating within a clinical team",
        ["PAED-012"] = "Communicating with and counselling patients, caregivers and healthcare teams",
        ["PAED-008"] = "Evaluating and managing neurodevelopmental and behavioural presentations in children",
        ["PAED-011"] = "Managing population health challenges",
        ["PAED-013"] = "Teaching and supervising",
        ["PAED-014"] = "Conducting research",
        ["PAED-015"] = "Managing a portfolio"
    };

    public TraineeHomeTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("dlamini@kgk.test");
        auth.SetRoles(WombatRoles.Trainee);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "trainee-1"));
    }

    // ---- the frame: four cards, in order, none a link around its links ----

    [Fact]
    public void Home_IsFourCardsInOrder_EachASectionNamedByItsTitle_NoneALink()
    {
        var cut = RenderHome(Typical());

        Titles(cut).Should().Equal("Your targets", "Needs you", "Recent decisions", "My authorisations");
        foreach (var id in new[] { "card-targets", "card-decisions", "card-stars" })
        {
            var section = cut.Find($"section[aria-labelledby='{id}']");
            section.QuerySelector($"h2#{id}").Should().NotBeNull();
        }

        cut.Find("section[aria-labelledby='card-targets']").ClassList.Should().Contain("detail-card--emphasis").And.Contain("dashboard-span-2");
        cut.Find("section[aria-labelledby='card-decisions']").ClassList.Should().Contain("dashboard-span-2");
        cut.FindAll("a.detail-card, .detail-card--interactive").Should().BeEmpty("a card's rows are its links (T280)");
        cut.FindAll("a a").Should().BeEmpty();
        cut.Markup.Should().NotContainAny(["Upcoming deadlines", "Recent activities", "Curriculum targets"], "Q3; T298");
    }

    [Fact]
    public void WhileTheReadRuns_TheFourTitlesShow_AndNothingIsOffered()
    {
        Services.AddSingleton<IScopedSender>(new HangingSender());
        var cut = RenderComponent<TraineeDashboard>();

        Titles(cut).Should().Equal("Your targets", "Needs you", "Recent decisions", "My authorisations");
        cut.FindAll(".dashboard-grid a").Should().BeEmpty();
        cut.Find("p.visually-hidden[role='status']").TextContent.Should().Be("Loading your Home.");
    }

    // ---- Your targets ----

    [Fact]
    public void Typical_YourTargets_NamesTheTrainingYear_TheTwoFigures_AndTheFiveFurthestShort()
    {
        // R3-H-typical (Anele Dlamini, training year 3).
        var card = Card(RenderHome(Typical()), "card-targets");

        Text(card.QuerySelector(".targets-line")!).Should().Be("Training year 3 — it sets the minimum level each encounter is judged against.");
        Metrics(card).Should().Equal(("1 of 10", "EPAs met this semester"), ("0 of 5", "EPAs met in 2026"));
        card.QuerySelectorAll(".targets-note").Should().BeEmpty();
        Text(card.QuerySelector("h3.progress-group-label")!).Should().Be("Furthest short");

        var rows = card.QuerySelectorAll("ul > li.progress-row.progress-row--link").ToList();
        rows.Select(row => Text(row.QuerySelector("a.progress-row-link")!)).Should().Equal(
            "PAED-002 — Managing common paediatric presentations",
            "PAED-003 — Providing intensive care to children",
            "PAED-005 — Providing neonatal care in intensive and high-care settings",
            "PAED-010 — Leading and operating within a clinical team",
            Paed012);
        rows.Select(row => row.QuerySelector("a")!.GetAttribute("href")).Should().Equal(
            "/portfolio/progress/2", "/portfolio/progress/3", "/portfolio/progress/5", "/portfolio/progress/10", "/portfolio/progress/12");

        // Its figure and date under the name, never beside it, and a bar that never stands without the words (R1).
        var first = rows[0];
        first.Children.Select(child => child.LocalName + "." + child.ClassName).Should().Equal(
            "a.progress-row-link", "div.progress-bar", "p.progress-row-meta");
        Text(first.QuerySelector(".progress-row-meta")!).Should().Be("0 of 3 this semester · 3 more by 2026-11-30");
        var bar = first.QuerySelector(".progress-bar")!;
        bar.GetAttribute("role").Should().Be("progressbar");
        bar.GetAttribute("aria-label").Should().Be("PAED-002: 0 of 3 this semester");
        bar.GetAttribute("aria-valuemax").Should().Be("3");
        bar.GetAttribute("aria-valuenow").Should().Be("0");

        var footer = card.QuerySelector(".dashboard-card-footer a")!;
        Text(footer).Should().Be("Open My progress");
        footer.GetAttribute("href").Should().Be("/portfolio/progress");
        card.TextContent.Should().NotContain(" / ", "a figure is never \"n / m\" (R3)");
    }

    [Fact]
    public void FirstDay_ListsTheFirstFiveByCode_WhenEveryShortfallIsTheSame()
    {
        // R3-H-first-day (Lerato Molefe, training year 4): nothing yet, so the largest shortfall ties and code decides.
        var card = Card(RenderHome(Home(Summary(D, new(2023, 1, 14), 4, Semester(("001", 0, 3), ("002", 0, 3), ("003", 0, 3), ("004", 0, 3), ("005", 0, 3), ("006", 0, 3), ("010", 0, 3))))), "card-targets");

        Text(card.QuerySelector(".targets-line")!).Should().StartWith("Training year 4 —");
        Metrics(card).Should().Equal(("0 of 7", "EPAs met this semester"));
        card.QuerySelectorAll("a.progress-row-link").Select(link => Text(link).Split(' ')[0])
            .Should().Equal("PAED-001", "PAED-002", "PAED-003", "PAED-004", "PAED-005");
    }

    [Fact]
    public void InDecember_EachRowSaysDecemberStillCounts()
    {
        // R3-H-december (C3).
        var card = Card(RenderHome(Typical(December)), "card-targets");

        Text(card.QuerySelector("li.progress-row .progress-row-meta")!)
            .Should().Be("0 of 3 this semester · 3 more; encounters in December still count towards Semester 2, 2026.");
    }

    [Fact]
    public void AllMet_SaysSo_AndListsNothingShort()
    {
        // R3-H-all-met (note 15): both kinds named, and no "Furthest short" with nothing short.
        var summary = Summary(D, new(2023, 1, 14), 3, [.. Semester(("001", 3, 3), ("002", 3, 3)), .. Yearly(("008", 1, 1))]);
        var card = Card(RenderHome(Home(summary)), "card-targets");

        Metrics(card).Should().Equal(("2 of 2", "EPAs met this semester"), ("1 of 1", "EPAs met in 2026"));
        card.QuerySelectorAll(".targets-note").Select(Text)
            .Should().Equal("Every target is met for Semester 2, 2026 and the 2026 academic year.");
        card.QuerySelectorAll(".progress-group-label, li.progress-row").Should().BeEmpty();
    }

    [Fact]
    public void StartedPartWay_SaysWhenTargetsBegin_WithNoFigures()
    {
        // R3-H-part-way: a curriculum of semester items whose current semester the start waives (D14).
        var waived = Waived(Semester2Of2026, QuotaWindowStatus.ExemptPartialPeriod, 2, 3, "semester 1, 2027");
        var summary = Summary(D, new(2026, 8, 18), 1, [Item("PAED-001", QuotaPeriod.Semester, 3, waived)],
            semesterStart: new QuotaStartDto("semester 1, 2027", new(2027, 1, 1)));
        var card = Card(RenderHome(Home(summary)), "card-targets");

        card.QuerySelectorAll(".targets-note").Select(Text).Should().Equal(
            "No semester target this semester: you started part-way through it. Semester targets begin with Semester 1, 2027.");
        card.QuerySelectorAll(".dashboard-metric, li.progress-row").Should().BeEmpty();
        Text(card.QuerySelector(".targets-line")!).Should().StartWith("Training year 1 —");
    }

    [Fact]
    public void NotStarted_SaysWhenItStarts_WithNoTrainingYearAndNoFigures()
    {
        // R3-H-not-started.
        var notYet = new QuotaWindowDto(
            "Semester 2, 2026", "July to November", new(2026, 7, 1), new(2026, 11, 30), QuotaWindowStatus.NotStarted, 0, 3,
            false, 0, 0, 0, null, false, "semester 1, 2027", new(2027, 1, 1));
        var summary = Summary(D, new(2027, 1, 15), null, [Item("PAED-001", QuotaPeriod.Semester, 3, notYet)],
            semesterStart: new QuotaStartDto("semester 1, 2027", new(2027, 1, 1)));
        var card = Card(RenderHome(Home(summary)), "card-targets");

        card.QuerySelectorAll(".targets-note").Select(Text).Should().Equal("Your programme starts on 2027-01-15.");
        card.QuerySelectorAll(".targets-line, .dashboard-metric, li.progress-row").Should().BeEmpty();
    }

    [Fact]
    public void NoCurriculum_SaysSo()
    {
        // R3-H-no-curriculum.
        var cut = RenderHome(Home(null));
        var card = Card(cut, "card-targets");

        card.QuerySelectorAll(".targets-note").Select(Text).Should().Equal(
            "No curriculum assigned yet. Once you are admitted, your targets for each period appear here.");
        card.QuerySelectorAll(".targets-line, .dashboard-metric, li.progress-row").Should().BeEmpty();
        Text(Card(cut, "card-stars").QuerySelector(".card-empty")!).Should().Be("No STAR yet.");
    }

    [Fact]
    public void NoEpaInUse_SaysNoTargetApplies_UnderTheTrainingYear()
    {
        // R3-H-no-epa-in-use (T158): admitted, so never "Once you are admitted".
        var card = Card(RenderHome(Home(Summary(D, new(2024, 1, 15), 3, []))), "card-targets");

        Text(card.QuerySelector(".targets-line")!).Should().StartWith("Training year 3 —");
        card.QuerySelectorAll(".targets-note").Select(Text).Should().Equal(
            "No EPA on your curriculum is in use at the moment, so no target applies to you.");
        card.QuerySelectorAll(".dashboard-metric, li.progress-row").Should().BeEmpty();
    }

    [Fact]
    public void Ended_SaysTheProgrammeEnded_InIsoDates_AndShowsNoTarget()
    {
        // R3-H-ended (A.4.6; T252; D1): Pieter du Plessis, whose programme ended on 2026-10-01.
        var ended = Summary(new(2026, 10, 1), new(2022, 1, 10), 5, Semester(("001", 1, 3)))
            with { Ended = new ProgrammeEndDto(Completed: false, EndedOn: new(2026, 10, 1), Today: D) };
        var cut = RenderHome(Home(ended, standing: Standing(5, [Epa(2, "PAED-001", EpaTitles["PAED-001"])])));
        var card = Card(cut, "card-targets");

        card.QuerySelectorAll(".targets-note").Select(Text).Should().Equal(
            "Your programme ended on 2026-10-01, so no target applies to you any more. Your progress in each period is kept " +
            "on My progress, read-only.");
        card.QuerySelectorAll(".targets-line, .dashboard-metric, li.progress-row, .progress-bar").Should().BeEmpty();
        Text(Card(cut, "card-stars").QuerySelector(".card-empty")!).Should().Be("No STAR yet.");
    }

    // ---- Recent decisions ----

    [Fact]
    public void Typical_RecentDecisions_AreFlow04sRows_EachWithTheCountItMade()
    {
        // R3-H-typical: newest decision first, the link and "to <assessor>", the badge and its day, then the count line.
        var card = Card(RenderHome(Typical()), "card-decisions");

        var rows = card.QuerySelectorAll("ul.decided-list > li.decided-row").ToList();
        rows.Should().HaveCount(4);
        rows.Select(row => Text(row.QuerySelector("a.activity-link")!)).Should().Equal(
            "Mini-CEX (Paediatrics) · PAED-004 · 2026-10-01, to Fatima Khumalo",
            "Clinical Case Analysis (Paediatrics) · PAED-001 · 2026-09-27, to Sarah Botha",
            "Case-Based Discussion (Paediatrics) · PAED-001 · 2026-09-24, to Thandi Zulu",
            "Mini-CEX (Paediatrics) · PAED-001 · 2026-09-23, to David Naidoo");
        rows[0].QuerySelector("a.activity-link")!.GetAttribute("href").Should().Be("/activities/31");
        rows[0].QuerySelector(".activity-link-to")!.TextContent.Should().Be("to Fatima Khumalo");

        var badge = rows[0].QuerySelector(".decided-meta .badge")!;
        Text(badge).Should().Be("Completed");
        badge.ClassList.Should().Contain("badge-completed");
        Text(rows[0].QuerySelector(".decided-meta .decided-date")!).Should().Be("2026-10-03");

        rows.Select(row => Text(row.QuerySelector("p.decided-note")!)).Should().Equal(
            "PAED-004: 1 of 3 this semester.",
            "PAED-001: 3 of 3 this semester, met.",
            "PAED-001: 3 of 3 this semester, met.",
            "PAED-001: 3 of 3 this semester, met.");
        card.QuerySelectorAll(".dashboard-card-footer").Should().BeEmpty("Recent decisions has no footer");
    }

    [Fact]
    public void ADeclinedRequest_OffersFileItAgain_NamedForItsRow_AndNoCount()
    {
        // R3-H-decline (E5): Sipho Ndlovu's Mini-CEX, declined by Fatima Khumalo.
        var declined = Decision(41, epaCode: "PAED-002", observedOn: new(2026, 9, 13), nominee: "Fatima Khumalo",
            state: "declined", stateLabel: "Declined", credited: null);
        var card = Card(RenderHome(Home(Ndlovu(), decisions: [declined])), "card-decisions");

        var row = card.QuerySelector("li.decided-row")!;
        var badge = row.QuerySelector(".badge")!;
        Text(badge).Should().Be("Declined");
        badge.ClassList.Should().Contain("badge-declined");
        var notes = row.QuerySelectorAll("p.decided-note").ToList();
        notes.Should().ContainSingle();
        var again = notes[0].QuerySelector("a")!;
        Text(again).Should().Be("File it again, to someone else");
        again.GetAttribute("href").Should().Be("/activities/new?from=41");
        again.GetAttribute("aria-label").Should().Be("File it again, to someone else: Mini-CEX (Paediatrics) · PAED-002 · 2026-09-13");
    }

    [Fact]
    public void MixedDecisions_EachSayWhatItMade_OneLineByKind()
    {
        // R3-H-decisions-mixed (E6): a discussion credits nothing; a completion counts; a decline is filed again. The
        // two Mini-CEX on PAED-002 of 2026-09-13 carry their assessor in the name (E7).
        var discussed = Decision(51, typeName: "Reflective Exercise (Paediatrics)", observedOn: new(2026, 9, 13), nominee: "Sarah Botha",
            state: "discussed", stateLabel: "Discussed", credited: 0) with { Shape = ActivityTypeShape.DiscussedOrReviewed };
        var completed = Decision(52, epaCode: "PAED-002", observedOn: new(2026, 9, 13), nominee: "Sarah Botha",
            countLine: CurrentLine("PAED-002", count: 1), displayName: "Mini-CEX (Paediatrics) · PAED-002 · 2026-09-13 · Sarah Botha")
            with { DisplayNameHasNominee = true };
        var declined = Decision(53, epaCode: "PAED-002", observedOn: new(2026, 9, 13), nominee: "Fatima Khumalo",
            state: "declined", stateLabel: "Declined", credited: null,
            displayName: "Mini-CEX (Paediatrics) · PAED-002 · 2026-09-13 · Fatima Khumalo") with { DisplayNameHasNominee = true };
        var card = Card(RenderHome(Home(Ndlovu(), decisions: [discussed, completed, declined])), "card-decisions");

        var rows = card.QuerySelectorAll("li.decided-row").ToList();
        Text(rows[0].QuerySelector("a.activity-link")!).Should().Be("Reflective Exercise (Paediatrics) · PAED-001 · 2026-09-13, with Sarah Botha");
        rows[0].QuerySelector(".badge")!.ClassList.Should().Contain("badge-completed", "Discussed is finished (E6)");
        Text(rows[0].QuerySelector("p.decided-note")!).Should().Be("Credits nothing.");
        Text(rows[1].QuerySelector("a.activity-link")!).Should().Be("Mini-CEX (Paediatrics) · PAED-002 · 2026-09-13 · Sarah Botha");
        Text(rows[1].QuerySelector("p.decided-note")!).Should().Be("PAED-002: 1 of 3 this semester.");
        Text(rows[2].QuerySelector("p.decided-note a")!).Should().Be("File it again, to someone else");
        rows[2].QuerySelector("p.decided-note a")!.GetAttribute("aria-label")
            .Should().Be("File it again, to someone else: Mini-CEX (Paediatrics) · PAED-002 · 2026-09-13 · Fatima Khumalo");
    }

    [Fact]
    public void AnOlderWindowsDecision_NamesItsWindow_AndAPausedOne_SaysItsCreditWaits()
    {
        // E5: the activity's own window; note 2: paused and credited nothing.
        var older = Decision(61, observedOn: new(2026, 5, 14), countLine: OlderLine());
        var paused = Decision(62, epaCode: "PAED-012", epaId: 12, observedOn: new(2026, 10, 3), nominee: "Mohammed Patel",
            credited: 0, epaInForce: false);
        var card = Card(RenderHome(Home(Ndlovu(), decisions: [paused, older])), "card-decisions");

        card.QuerySelectorAll("p.decided-note").Select(Text).Should().Equal(
            "Its credit to PAED-012 waits while the EPA is paused.",
            "PAED-001, Semester 1, 2026: 3 of 3, met.");
    }

    [Fact]
    public void NoDecisions_SaysSo()
    {
        var card = Card(RenderHome(Home(Ndlovu())), "card-decisions");

        Text(card.QuerySelector("p.card-empty")!).Should().Be("No decisions yet.");
        card.QuerySelectorAll("li").Should().BeEmpty();
    }

    [Fact]
    public void Returned_NeedsYouHasTheRow_AndTheOneNameReadsTheSameOnBothCards()
    {
        // R3-H-returned: Needs you is flow 03's, unchanged; its "Open My activities" is Home's one way to My activities.
        var cut = RenderHome(Home(Ndlovu(), needsYou: [ActivityRows.Returned(71)], decisions: [Decision(72, epaCode: "PAED-002", countLine: CurrentLine("PAED-002"))]));

        Card(cut, null).QuerySelectorAll(".needs-you-row").Should().ContainSingle();
        cut.FindAll("a[href='/activities/mine']").Should().ContainSingle();
    }

    // ---- My authorisations ----

    [Fact]
    public void WithStars_TheSummaryInThePanelsWords_ThenEachStarBelowItsLevel_ALinkToItsEpaPage()
    {
        // R3-H-with-stars (Lerato Molefe, training year 4): 2 at or above, PAED-010's STAR of 4 below 5 and expiring.
        var epas = new List<EpaStandingDto>
        {
            Epa(1, "PAED-001", EpaTitles["PAED-001"], star: "5", status: EntrustmentStandingStatus.AtOrAbove),
            Epa(10, "PAED-010", EpaTitles["PAED-010"], star: "4", status: EntrustmentStandingStatus.Below, expires: new(2026, 10, 23)),
            Epa(12, "PAED-012", EpaTitles["PAED-012"], star: "5", status: EntrustmentStandingStatus.AtOrAbove)
        };
        epas.AddRange(Enumerable.Range(20, 12).Select(id => Epa(id, $"PAED-0{id}", "Another EPA")));
        var card = Card(RenderHome(Home(Molefe(), standing: Standing(4, epas))), "card-stars");

        Text(card.QuerySelector("p.stars-summary")!).Should().Be("2 at or above · 1 below · 12 with no decision, of 15 EPAs");
        var row = card.QuerySelectorAll("ul.decided-list > li.decided-row").Should().ContainSingle().Which;
        var link = row.QuerySelector("a.activity-link")!;
        Text(link).Should().Be("PAED-010 — Leading and operating within a clinical team");
        link.GetAttribute("href").Should().Be("/portfolio/progress/10");
        Text(row.QuerySelector("p.decided-note")!).Should().Be("4, below training year 4's level of 5 · expires 2026-10-23");

        var footer = card.QuerySelector(".dashboard-card-footer a")!;
        Text(footer).Should().Be("Open My authorisations");
        footer.GetAttribute("href").Should().Be("/portfolio/authorisations");
    }

    [Fact]
    public void NoStar_SaysWhichTrainingYearsLevelOneWillBeReadAgainst()
    {
        // R3-H-typical's card: Anele Dlamini, training year 3, no STAR yet.
        var card = Card(RenderHome(Typical()), "card-stars");

        Text(card.QuerySelector("p.card-empty")!)
            .Should().Be("No STAR yet. When the committee issues one, it shows here against training year 3's level.");
        card.QuerySelectorAll(".stars-summary, li").Should().BeEmpty();
        card.QuerySelector(".dashboard-card-footer a")!.GetAttribute("href").Should().Be("/portfolio/authorisations");
    }

    // ---- a pending trainee: unchanged ----

    [Fact]
    public void APendingTrainee_SeesOnlyAwaitingAdmission()
    {
        var cut = RenderHome(new TraineeDashboardSummaryDto(null, [], [], null, IsPendingTrainee: true));

        Titles(cut).Should().Equal("Awaiting admission");
    }

    // ---- the cast ----

    /// <summary>Anele Dlamini on 2026-10-03 (R3-H-typical), or on <paramref name="asOf" />.</summary>
    private static TraineeDashboardSummaryDto Typical(DateOnly? asOf = null)
    {
        var day = asOf ?? D;
        var summary = Summary(day, new(2024, 1, 15), 3,
        [
            .. Semester(("001", 3, 3), ("002", 0, 3), ("003", 0, 3), ("004", 1, 3), ("005", 0, 3), ("006", 0, 2), ("007", 0, 1),
                ("009", 1, 3), ("010", 0, 3), ("012", 0, 3)),
            .. Yearly(("008", 0, 1), ("011", 0, 1), ("013", 0, 1), ("014", 0, 1), ("015", 0, 1))
        ]);
        return Home(summary,
            decisions:
            [
                Decision(31, epaCode: "PAED-004", epaId: 4, observedOn: new(2026, 10, 1), nominee: "Fatima Khumalo", countLine: CurrentLine("PAED-004", 4, 1)),
                Decision(32, typeName: "Clinical Case Analysis (Paediatrics)", observedOn: new(2026, 9, 27), nominee: "Sarah Botha", countLine: CurrentLine(count: 3)),
                Decision(33, typeName: "Case-Based Discussion (Paediatrics)", observedOn: new(2026, 9, 24), nominee: "Thandi Zulu", countLine: CurrentLine(count: 3)),
                Decision(34, observedOn: new(2026, 9, 23), countLine: CurrentLine(count: 3))
            ],
            standing: Standing(3, [Epa(1, "PAED-001", EpaTitles["PAED-001"])]));
    }

    private static TraineeCurriculumProgressSummaryDto Ndlovu()
        => Summary(D, new(2026, 1, 12), 1, Semester(("001", 0, 3), ("002", 1, 3)));

    private static TraineeCurriculumProgressSummaryDto Molefe()
        => Summary(D, new(2023, 1, 14), 4, Semester(("001", 3, 3), ("002", 0, 3)));

    private static TraineeDashboardSummaryDto Home(
        TraineeCurriculumProgressSummaryDto? targets,
        IReadOnlyList<ActivitySummaryDto>? needsYou = null,
        IReadOnlyList<ActivitySummaryDto>? decisions = null,
        EntrustmentStandingDto? standing = null)
        => new(targets, needsYou ?? [], decisions ?? [], standing, IsPendingTrainee: false);

    private static TraineeCurriculumProgressDto[] Semester(params (string Code, int Count, int Target)[] items)
        => items.Select(item => Kind(item, QuotaPeriod.Semester, Semester2Of2026)).ToArray();

    private static TraineeCurriculumProgressDto[] Yearly(params (string Code, int Count, int Target)[] items)
        => items.Select(item => Kind(item, QuotaPeriod.AcademicYear, Year2026)).ToArray();

    private static TraineeCurriculumProgressDto Kind((string Code, int Count, int Target) item, QuotaPeriod period, Window window)
    {
        var code = $"PAED-{item.Code}";
        var id = int.Parse(item.Code, System.Globalization.CultureInfo.InvariantCulture);
        return Item(code, period, item.Target, Counting(window, item.Count, item.Target), title: EpaTitles.TryGetValue(code, out var title) ? title : "An EPA")
            with { CurriculumItemId = id, EpaId = id };
    }

    // ---- rendering ----

    private IRenderedComponent<TraineeDashboard> RenderHome(TraineeDashboardSummaryDto summary)
    {
        Services.AddSingleton<IScopedSender>(new Sender(summary));
        var cut = RenderComponent<TraineeDashboard>();
        cut.WaitForState(() => cut.FindAll(".skeleton").Count == 0);
        return cut;
    }

    private static IElement Card(IRenderedFragment cut, string? headingId)
        => headingId is null
            ? cut.FindAll(".detail-card").Single(card => card.QuerySelector("h2")?.TextContent.Contains("Needs you") == true)
            : cut.Find($"section[aria-labelledby='{headingId}']");

    private static List<string> Titles(IRenderedFragment cut)
        => cut.FindAll(".detail-card .dashboard-card-title > span:not(.badge):not(.visually-hidden)").Select(Text).ToList();

    private static IReadOnlyList<(string Value, string Label)> Metrics(IElement card)
        => card.QuerySelectorAll(".dashboard-metric")
            .Select(metric => (Text(metric.QuerySelector(".dashboard-metric-value")!), Text(metric.QuerySelector(".dashboard-metric-label")!)))
            .ToList();

    private static string Text(IElement element)
        => string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private sealed class Sender(object summary) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => Task.FromResult((TResponse)summary);

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class HangingSender : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => new TaskCompletionSource<TResponse>().Task;

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
