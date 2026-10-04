using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Portfolio;
using Wombat.Web.Components.Shared;
using Wombat.Web.Components.Shared.Progress;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;
using static Wombat.Web.Tests.Progress.ProgressFixtures;

namespace Wombat.Web.Tests.Progress;

/// <summary>
/// My progress (T355, flow 05; R3-C-Progress; Q1, R3, R5; C1, C2, C3, C8): This period, the EPA index, the standing panel
/// whole, and every R3-P state: first, counting, standing, exit met, paused, local item, after move, December, part-way,
/// not started, no curriculum, no EPA in use, not comparable, section error, loading and load error; and the ended view,
/// flow 13's as built, with its dates ISO (D1).
/// </summary>
/// <remarks>
/// The figures and their words are computed and tested beneath it (<c>TraineeQuotaProgressReader</c>,
/// <see cref="ProgressWords" />); what only a render shows is which part says what, where, and what it never says. The
/// index's own rows are <see cref="EpaProgressTableTests" />. The cast's day is 2026-10-03.
/// </remarks>
public sealed class MyProgressTests : WombatTestContext
{
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";
    private const string Secret = "Npgsql: the connection string was rejected.";

    public MyProgressTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("trainee@test");
        auth.SetRoles(WombatRoles.Trainee);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "trainee-1"));
        Services.AddSingleton<TimeProvider>(new FixedClock(new DateTimeOffset(2026, 10, 3, 8, 0, 0, TimeSpan.Zero)));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    // ─── The header and This period ──────────────────────────────────────────

    [Fact]
    public void TheHeader_NamesTheTrainingYearAndTheSemester()
    {
        // 2.40: "Training year 4 · Semester 2, 2026".
        var cut = Render(Molefe());

        Text(cut.Find("h1")).Should().Be("My progress");
        Text(cut.Find(".page-subtitle")).Should().Be("Training year 4 · Semester 2, 2026");
    }

    [Fact]
    public void ThisPeriod_IsTheTwoFigures_TheEndsLine_TheTrainingYear_AndMsf()
    {
        var cut = Render(Molefe(), msf: Coverage(covered: 2, epas: 3));

        var card = cut.Find("section.detail-card.period-card");
        card.GetAttribute("aria-labelledby").Should().Be("period-h");
        var heading = card.QuerySelector("h2#period-h")!;
        Text(heading).Should().Be("This period");
        heading.GetAttribute("tabindex").Should().Be("-1");

        Metrics(card).Should().Equal(("1 of 2", "EPAs met this semester"), ("0 of 1", "EPAs met in 2026"));
        Text(card.QuerySelector("p.period-ends")!).Should().Be("Semester 2, 2026 ends on 2026-11-30.");
        card.QuerySelectorAll(".period-lines p").Select(Text).Should().Equal(
            "Training year 4 — it sets the minimum level each encounter is judged against.",
            "Multi-source feedback: 2 of 3 EPAs covered by a released campaign that closed this semester. MSF is tracked on its own and counts towards no target.");
        Text(card.QuerySelector(".period-lines .period-line-term")!).Should().Be("Multi-source feedback:");
    }

    [Fact]
    public void TheFirstVisit_CountsNothing_AndSaysNothingIsMet()
    {
        // R3-P-first: a registrar with nothing credited yet reads "0 of n", never a blank.
        var cut = Render(Summary(D, new DateOnly(2023, 1, 15), 4,
        [
            Item("PAED-001", QuotaPeriod.Semester, 3, Counting(Semester2Of2026, 0, 3)) with { EpaId = 1 },
            Item("PAED-008", QuotaPeriod.AcademicYear, 1, Counting(Year2026, 0, 1)) with { EpaId = 8 }
        ]));

        Metrics(cut.Find("section.period-card")).Should().Equal(("0 of 1", "EPAs met this semester"), ("0 of 1", "EPAs met in 2026"));
        Text(IndexRow(cut, "PAED-001").QuerySelector(".count-figure")!).Should().Be("0 of 3 this semester");
        Text(IndexRow(cut, "PAED-001").QuerySelector(".count-meta")!).Should().Be("3 more by 2026-11-30");
    }

    [Fact]
    public void ACurriculumOfOneKind_ShowsOnlyThatKindsFigure()
    {
        var cut = Render(Summary(D, new DateOnly(2023, 1, 15), 4,
        [
            Item("PAED-001", QuotaPeriod.Semester, 3, Counting(Semester2Of2026, 1, 3)) with { EpaId = 1 }
        ]));

        Metrics(cut.Find("section.period-card")).Should().Equal(("0 of 1", "EPAs met this semester"));
        cut.FindAll("table").Should().ContainSingle();
    }

    [Fact]
    public void InDecember_TheNoticeLeadsWithTheYearsEnd_AndTheEndsLineSaysDecemberStillCounts()
    {
        // R3-P-december; D40, C3.
        var cut = Render(Summary(December, new DateOnly(2023, 1, 15), 3,
        [
            Item("PAED-001", QuotaPeriod.Semester, 3, Counting(Semester2Of2026, 3, 3)) with { EpaId = 1 },
            Item("PAED-008", QuotaPeriod.AcademicYear, 1, Counting(Year2026, 0, 1)) with { EpaId = 8 }
        ]));

        var alert = cut.Find(".alert.alert-info");
        Text(alert.QuerySelector("strong")!).Should().Be("The 2026 academic year ended on 2026-11-30.");
        Text(alert).Should().Be(
            "The 2026 academic year ended on 2026-11-30. Encounters observed in December still count towards semester 2, " +
            "2026 and your 2026 yearly targets. Semester 1, 2027 starts on 2027-01-01.");
        Text(cut.Find("p.period-ends")).Should().Be("Semester 2, 2026 counts encounters observed in December.");
        PageText(cut).Should().NotContain("31 December").And.NotContain("30 November");
    }

    [Fact]
    public void BeforeDecember_ThereIsNoNotice()
    {
        var cut = Render(Molefe());

        cut.FindAll(".alert").Should().BeEmpty();
        PageText(cut).Should().NotContain("academic year ended").And.NotContain("part-way");
    }

    [Fact]
    public void AStartPartWay_IsExplainedOnce_AboveThisPeriod_InTodaysWords_DatesIso()
    {
        // R3-P-part-way; C2: MyProgress.razor's sentences, word for word, dates ISO. This period's figures read "none apply
        // yet" over their kind.
        var cut = Render(Summary(D, new DateOnly(2026, 8, 18), 1,
            [
                Item("PAED-002", QuotaPeriod.Semester, 3,
                    Waived(Semester2Of2026, QuotaWindowStatus.ExemptPartialPeriod, 2, 3, "semester 1, 2027")) with { EpaId = 2 },
                Item("PAED-008", QuotaPeriod.AcademicYear, 1,
                    Waived(Year2026, QuotaWindowStatus.ExemptPartialPeriod, 0, 1, "the 2027 academic year")) with { EpaId = 8 }
            ],
            semesterStart: new QuotaStartDto("semester 1, 2027", new DateOnly(2027, 1, 1)),
            yearStart: new QuotaStartDto("the 2027 academic year", new DateOnly(2027, 1, 1))));

        var alert = cut.Find(".alert.alert-info");
        Text(alert).Should().Be(
            "You started the programme on 2026-08-18. That was part-way through the semester, so under the College's rule no " +
            "semester target applies until semester 1, 2027, which starts on 2027-01-01. Encounters on those EPAs before then " +
            "stay in your portfolio as evidence, but do not count towards a later semester's target. That was in the second " +
            "half of the academic year, so under the College's rule no yearly target applies until the 2027 academic year, " +
            "which starts on 2027-01-01. Encounters on those EPAs before then stay in your portfolio as evidence, but do not " +
            "count towards a later year's target.");
        IsBefore(cut, "You started the programme on", "id=\"period-h\"").Should().BeTrue("the notice stands above This period");
        Metrics(cut.Find("section.period-card")).Should().Equal(("none apply yet", "Semester targets"), ("none apply yet", "Yearly targets"));
    }

    [Fact]
    public void AProgrammeNotYetStarted_SaysWhenItStarts_AndHasNoTrainingYearLine()
    {
        // R3-P-not-started; C2.
        var cut = Render(Summary(D, new DateOnly(2027, 1, 15), null,
            [
                Item("PAED-001", QuotaPeriod.Semester, 3, Waived(Semester2Of2026, QuotaWindowStatus.NotStarted, 0, 3, "semester 1, 2027")) with { EpaId = 1 },
                Item("PAED-008", QuotaPeriod.AcademicYear, 1, Waived(Year2026, QuotaWindowStatus.NotStarted, 0, 1, "the 2027 academic year")) with { EpaId = 8 }
            ],
            semesterStart: new QuotaStartDto("semester 1, 2027", new DateOnly(2027, 1, 15)),
            yearStart: new QuotaStartDto("the 2027 academic year", new DateOnly(2027, 1, 15))));

        Text(cut.Find(".alert.alert-info")).Should().Be(
            "Your programme starts on 2027-01-15. Your first semester targets are for semester 1, 2027. Your first yearly " +
            "targets are for the 2027 academic year.");
        cut.Find("section.period-card").TextContent.Should().NotContain("Training year");
        Text(IndexRow(cut, "PAED-001").QuerySelector("td.count-cell")!).Should().Be("No target yet · targets start with semester 1, 2027");
    }

    // ─── Your EPAs ───────────────────────────────────────────────────────────

    [Fact]
    public void YourEpas_IsTheIndex_UnderItsOwnHeading_WithTheStarColumnFromTheStanding()
    {
        var cut = Render(Molefe(), standing: Standing(Epa(1, "PAED-001", "5", EntrustmentStandingStatus.AtOrAbove)));

        var section = cut.Find("section.index-section");
        section.GetAttribute("aria-labelledby").Should().Be("index-h");
        Text(section.QuerySelector("h2#index-h")!).Should().Be("Your EPAs");
        section.QuerySelectorAll("caption.index-caption").Select(Text).Should().Equal("Each semester · 2 EPAs", "Once a year · 1 EPA");
        Text(IndexRow(cut, "PAED-001").QuerySelector(".verdict-cell .badge")!).Should().Be("At or above");
        IndexRow(cut, "PAED-001").QuerySelector("th a.epa-link")!.GetAttribute("href").Should().Be("/portfolio/progress/1");
    }

    [Fact]
    public void APausedEpa_IsInItsOwnGroup_AndCountedInNoFigure()
    {
        // R3-P-paused; D48: Anele Dlamini's PAED-012 is paused. The figures count nine semester EPAs, not ten.
        var summary = Molefe() with { Paused = [new PausedItemDto(112, 12, "PAED-012", "Communicating", QuotaPeriod.Semester)] };

        var cut = Render(summary);

        var paused = cut.FindAll("section.index-section table").Last();
        Text(paused.QuerySelector("caption")!).Should().Be("No longer in use · 1 EPA");
        paused.QuerySelector("tr.is-paused .paused-mark").Should().NotBeNull();
        Metrics(cut.Find("section.period-card")).Should().Contain(("1 of 2", "EPAs met this semester"));
    }

    [Fact]
    public void TheTrajectories_AreNotOnThisPage()
    {
        // Q5: each EPA's chart is on its page; My progress draws none and reads none.
        var sender = Sender(Molefe());
        var cut = RenderWith(sender);

        sender.Received.OfType<GetEpaTrajectoryForTraineeQuery>().Should().BeEmpty();
        PageText(cut).Should().NotContain("Rating trajectory");
        cut.FindAll("svg.trajectory-chart").Should().BeEmpty();
    }

    // ─── The standing ────────────────────────────────────────────────────────

    [Fact]
    public void TheStanding_IsItsOwnSection_UnderTheIndex()
    {
        var cut = Render(Molefe(), standing: Standing(Epa(1, "PAED-001", "5", EntrustmentStandingStatus.AtOrAbove)));

        var section = cut.Find("section.standing-panel");
        section.GetAttribute("aria-labelledby").Should().Be("standing-h");
        var heading = section.QuerySelector("h2#standing-h")!;
        Text(heading).Should().Be("Entrustment against Annexure A");
        heading.GetAttribute("tabindex").Should().Be("-1");
        IsBefore(cut, "id=\"index-h\"", "id=\"standing-h\"").Should().BeTrue("the panel is under the index");
        section.QuerySelector("tbody th a.epa-link")!.GetAttribute("href").Should().Be("/portfolio/progress/1");
    }

    [Fact]
    public void AnExitMet_IsSaid_AndEveryStarCellReadsAtOrAbove()
    {
        // R3-P-exit-met.
        var standing = Standing(
            Epa(1, "PAED-001", "5", EntrustmentStandingStatus.AtOrAbove),
            Epa(3, "PAED-003", "5", EntrustmentStandingStatus.AtOrAbove),
            Epa(8, "PAED-008", "4", EntrustmentStandingStatus.AtOrAbove)) with
        {
            Exit = new ExitRuleReadinessDto(3, 3, [new ExitLevelGroupDto(6, "5", 2, 2), new ExitLevelGroupDto(5, "4", 1, 1)], [])
        };

        var cut = Render(Molefe(), standing: standing);

        Text(cut.Find("section.standing-panel")).Should().Contain(
            "3 of 3 EPAs at their exit level by STAR decision (level 5: 2 of 2 · level 4: 1 of 1). Every EPA is at its exit level.");
        // Each indexed EPA the standing reads (PAED-001, PAED-008; PAED-003 is no item of Molefe()) is at or above; a row
        // the standing has no row for reads "No decision", never blank (T355, build review A3).
        cut.FindAll("section.index-section tbody tr")
            .Where(row => row.QuerySelector("th")?.TextContent is { } header && new[] { "PAED-001", "PAED-008" }.Any(header.Contains))
            .Select(row => Text(row.QuerySelector(".verdict-cell .badge")!))
            .Should().Equal("At or above", "At or above");
    }

    [Fact]
    public void TheExitLine_NamesTheEpasNotYetThere_InCodeOrder()
    {
        var standing = Standing(
            Epa(1, "PAED-001", "5", EntrustmentStandingStatus.AtOrAbove),
            Epa(3, "PAED-003", null, EntrustmentStandingStatus.NoDecision),
            Epa(8, "PAED-008", null, EntrustmentStandingStatus.NoDecision)) with
        {
            Exit = new ExitRuleReadinessDto(3, 1, [new ExitLevelGroupDto(6, "5", 2, 1), new ExitLevelGroupDto(5, "4", 1, 0)], ["PAED-003", "PAED-008"])
        };

        var cut = Render(Molefe(), standing: standing);

        Text(cut.Find("section.standing-panel")).Should().Contain("Not yet: PAED-003, PAED-008.");
    }

    [Fact]
    public void ALocalItem_IsMarkedInTheIndex_AndTheStandingSaysTheCollegesRuleLeavesItOut()
    {
        // R3-P-local-item: KGK-001, Kgosi Kgari Teaching Hospital's own.
        var summary = Summary(D, new DateOnly(2024, 1, 15), 3,
        [
            Item("PAED-001", QuotaPeriod.Semester, 3, Counting(Semester2Of2026, 3, 3)) with { EpaId = 1 },
            Item("KGK-001", QuotaPeriod.AcademicYear, 1, Counting(Year2026, 0, 1), title: "Running a paediatric outreach clinic at a district hospital", minimum: "3a") with
            {
                EpaId = 21, OwningInstitutionName = "Kgosi Kgari Teaching Hospital", MinimumByTrainingYear = false
            }
        ]);
        var standing = Standing(
            Epa(1, "PAED-001", "4", EntrustmentStandingStatus.AtOrAbove),
            Epa(21, "KGK-001", null, EntrustmentStandingStatus.NoDecision) with { IsLocal = true, YearTargetIsExitLevel = true, YearTargetLabel = "3a", ExitLevelLabel = "3a" }) with
        {
            TargetYear = 3,
            Exit = new ExitRuleReadinessDto(1, 0, [new ExitLevelGroupDto(6, "5", 1, 0)], ["PAED-001"])
        };

        var cut = Render(summary, standing: standing);

        Text(IndexRow(cut, "KGK-001").QuerySelector("th .badge")!).Should().Be("Kgosi Kgari Teaching Hospital's own");
        Text(IndexRow(cut, "KGK-001").QuerySelector("td[data-label='Committee decides'] .visually-hidden")!).Should().Be("No decision cadence");
        var panel = Text(cut.Find("section.standing-panel"));
        panel.Should().Contain("The institution's own EPA is not part of the College's rule, so it is not counted.");
        panel.Should().Contain("The institution's own EPA; not in the exit rule");
        panel.Should().Contain("Exit level; no year 3 level set");
    }

    [Fact]
    public void ANotComparableStar_ReadsItsLadder_InTheIndexAndThePanel()
    {
        // R3-P-not-comparable; C11's slot.
        var standing = Standing(
            Epa(12, "PAED-012", "Independent", EntrustmentStandingStatus.NotComparable, ladder: "O-R Scale"));
        var summary = Summary(D, new DateOnly(2023, 1, 15), 4,
        [
            Item("PAED-012", QuotaPeriod.Semester, 3, Counting(Semester2Of2026, 2, 3)) with { EpaId = 12 }
        ]);

        var cut = Render(summary, standing: standing);

        Text(IndexRow(cut, "PAED-012").QuerySelector(".verdict-cell")!).Should().Be("Not comparable Independent on O-R Scale");
        var panel = Text(cut.Find("section.standing-panel"));
        panel.Should().Contain("Independent on O-R Scale");
        panel.Should().Contain("Decided on O-R Scale, not CPSA Paediatric Entrustment Scale v11.1.");
        panel.Should().Contain("1 not comparable");
    }

    // ─── Empty states ────────────────────────────────────────────────────────

    [Fact]
    public void ATraineeWithNoCurriculum_IsToldSo_UnderTheSemestersName()
    {
        // R3-P-no-curriculum: the reader returns null when there is no trainee profile at all.
        var cut = Render(null);

        var empty = cut.Find(".detail-card.detail-card--empty");
        Text(empty.QuerySelector(".state-panel-title")!).Should().Be("No curriculum items assigned yet.");
        Text(empty.QuerySelector(".state-panel-copy")!).Should().Be("Once you are admitted to a curriculum, your EPAs will appear here.");
        Text(cut.Find(".page-subtitle")).Should().Be("Semester 2, 2026");
        cut.FindAll("section.period-card, section.index-section, section.standing-panel").Should().BeEmpty();
    }

    [Fact]
    public void AnAdmittedTraineeWithNoEpaInUse_IsNotToldToWaitForAdmission()
    {
        // R3-P-no-epa-in-use (T158): admitted, every EPA paused or none yet.
        var cut = Render(Summary(D, new DateOnly(2025, 1, 15), 2, []), standing: Standing());

        Text(cut.Find(".detail-card--empty .state-panel-title")).Should().Be(
            "No EPA on your curriculum is in use at the moment, so no target applies to you.");
        cut.FindAll(".detail-card--empty .state-panel-copy").Should().BeEmpty();
        PageText(cut).Should().NotContain("Once you are admitted");
        cut.FindAll("section.period-card, section.standing-panel").Should().BeEmpty();
        cut.FindAll(".progress-bar").Should().BeEmpty();
    }

    [Fact]
    public void WithEveryEpaPaused_ThePausedGroupStillSaysWhy()
    {
        var summary = Summary(D, new DateOnly(2025, 1, 15), 2, []) with
        {
            Paused = [new PausedItemDto(112, 12, "PAED-012", "Communicating", QuotaPeriod.Semester)]
        };

        var cut = Render(summary);

        cut.Find(".detail-card--empty");
        Text(cut.Find("section.index-section caption")).Should().Be("No longer in use · 1 EPA");
    }

    // ─── Loading and failures ────────────────────────────────────────────────

    [Fact]
    public void WhileItLoads_TheStatusSaysSo_AndOnceLoadedItIsEmpty()
    {
        // C8: the status is on the page from the first render, filled only while the read runs.
        var pending = new TaskCompletionSource<object?>();
        var sender = new FakeSender()
            .OnAsync<GetCurriculumProgressForTraineeQuery>(_ => pending.Task)
            .On<GetEntrustmentStandingForTraineeQuery>(_ => null)
            .On<GetMsfCoverageForTraineeQuery>(_ => null);
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<MyProgress>();

        Text(cut.Find("p.visually-hidden[role=status]")).Should().Be("Loading My progress.");
        cut.FindAll(".skeleton").Should().NotBeEmpty();
        cut.FindAll(".page-subtitle").Should().BeEmpty();

        pending.SetResult(Molefe());
        cut.WaitForAssertion(() => cut.Find("section.period-card"), AsyncWorkTimeout);
        Text(cut.Find("p.visually-hidden[role=status]")).Should().BeEmpty();
    }

    [Fact]
    public void AFailedRead_IsTheFixedWords_WithTryAgain_AndNeverTheExceptionsText()
    {
        // R3-P-load-error; T329, T272.
        var sender = new FakeSender()
            .On<GetCurriculumProgressForTraineeQuery>(_ => throw new InvalidOperationException(Secret))
            .On<GetEntrustmentStandingForTraineeQuery>(_ => null)
            .On<GetMsfCoverageForTraineeQuery>(_ => null);
        var cut = RenderWith(sender, waitFor: "Could not load your progress.");

        var alert = cut.Find(".action-result .alert.alert-danger");
        Text(alert.QuerySelector(".alert-row-text")!).Should().Be(MyProgress.LoadFailed);
        Text(alert.QuerySelector("button")!).Should().Be("Try again");
        cut.Markup.Should().NotContain(Secret);
        cut.FindAll("section").Should().BeEmpty("nothing is drawn under it");
    }

    [Fact]
    public void TryAgain_ReadsAgain_AndThisPeriodsHeadingTakesTheFocus()
    {
        // Spec § 5: the answer takes the focus, This period's heading on My progress.
        var fail = true;
        var sender = new FakeSender()
            .On<GetCurriculumProgressForTraineeQuery>(_ => fail ? throw new InvalidOperationException(Secret) : Molefe())
            .On<GetEntrustmentStandingForTraineeQuery>(_ => null)
            .On<GetMsfCoverageForTraineeQuery>(_ => null);
        var cut = RenderWith(sender, waitFor: "Could not load your progress.");

        fail = false;
        cut.Find(".alert-row button").Click();

        cut.WaitForAssertion(() => cut.Find("h2#period-h"));
        FocusedReference().Should().Be(cut.Find("h2#period-h").GetAttribute("blazor:elementreference"));
    }

    [Fact]
    public void WhenTheStandingFails_ItsSectionSaysSo_WithTryAgain_AndTheIndexSaysNotLoaded()
    {
        // R3-P-section-error; C8: under the panel's heading, never the exception's text; the counts stand.
        var sender = new FakeSender()
            .On<GetCurriculumProgressForTraineeQuery>(_ => Molefe())
            .On<GetEntrustmentStandingForTraineeQuery>(_ => throw new InvalidOperationException(Secret))
            .On<GetMsfCoverageForTraineeQuery>(_ => Coverage(covered: 0, epas: 3));
        var cut = RenderWith(sender);

        var section = cut.Find("section.standing-panel");
        var alert = section.QuerySelector(".alert.alert-danger")!;
        Text(alert.QuerySelector(".alert-row-text")!).Should().Be(
            "Could not load your standing. Nothing has changed. Try again, or come back in a few minutes.");
        Text(alert.QuerySelector("button")!).Should().Be("Try again");
        IsBefore(cut, "id=\"standing-h\"", "Could not load your standing.").Should().BeTrue("Try again sits under the panel's heading");
        cut.FindAll("section.index-section .verdict-cell").Select(Text).Should().AllBe("Not loaded");
        cut.Find("section.period-card");
        cut.Markup.Should().NotContain(Secret);
    }

    [Fact]
    public void TheStandingsTryAgain_ReadsTheStandingAlone_AndItsHeadingTakesTheFocus()
    {
        var fail = true;
        var sender = new FakeSender()
            .On<GetCurriculumProgressForTraineeQuery>(_ => Molefe())
            .On<GetEntrustmentStandingForTraineeQuery>(_ => fail
                ? throw new InvalidOperationException(Secret)
                : Standing(Epa(1, "PAED-001", "5", EntrustmentStandingStatus.AtOrAbove)))
            .On<GetMsfCoverageForTraineeQuery>(_ => null);
        var cut = RenderWith(sender);
        // The heading is kept across the read, and bUnit writes an element's reference only where it was first drawn.
        var heading = cut.Find("h2#standing-h").GetAttribute("blazor:elementreference");

        fail = false;
        cut.Find("section.standing-panel .alert-row button").Click();

        cut.WaitForAssertion(() => cut.Find("section.standing-panel table"));
        sender.Received.OfType<GetCurriculumProgressForTraineeQuery>().Should().ContainSingle("only the standing is read again");
        sender.Received.OfType<GetEntrustmentStandingForTraineeQuery>().Should().HaveCount(2);
        heading.Should().NotBeNullOrEmpty();
        FocusedReference().Should().Be(heading);
        Text(IndexRow(cut, "PAED-001").QuerySelector(".verdict-cell .badge")!).Should().Be("At or above");
    }

    [Fact]
    public void WhenMsfFails_ThisPeriodSaysSoInALine_AndTheCountsStand()
    {
        // R3-Spec § 1: "Multi-source feedback: Could not load MSF coverage. Your counts above are not affected." No button.
        var sender = new FakeSender()
            .On<GetCurriculumProgressForTraineeQuery>(_ => Molefe())
            .On<GetEntrustmentStandingForTraineeQuery>(_ => null)
            .On<GetMsfCoverageForTraineeQuery>(_ => throw new InvalidOperationException(Secret));
        var cut = RenderWith(sender);

        var line = cut.Find("section.period-card p.section-error");
        Text(line).Should().Be("Multi-source feedback: Could not load MSF coverage. Your counts above are not affected.");
        line.QuerySelector("button").Should().BeNull();
        cut.FindAll(".alert").Should().BeEmpty();
        cut.Markup.Should().NotContain(Secret);
        Metrics(cut.Find("section.period-card")).Should().NotBeEmpty();
    }

    [Fact]
    public void ThePage_AsksForTheSignedInTraineesProgress_WithoutPinningADate()
    {
        // The page takes "today" from the reader's South African calendar (T325). A page that pinned AsOf would freeze
        // its period. T113: the handler authorises against the caller, so the page passes the signed-in principal itself.
        var sender = Sender(Molefe());
        RenderWith(sender);

        var query = sender.Received.OfType<GetCurriculumProgressForTraineeQuery>().Should().ContainSingle().Which;
        query.TraineeUserId.Should().Be("trainee-1");
        query.AsOf.Should().BeNull();
        query.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be("trainee-1");
        sender.Received.OfType<GetEntrustmentStandingForTraineeQuery>().Single().AsOf.Should().BeNull();
        sender.Received.OfType<GetMsfCoverageForTraineeQuery>().Single().To.Should().BeNull();
    }

    // ─── Once the programme has ended (T252; flow 13's, R5) ──────────────────

    [Fact]
    public void AGraduate_SeesEveryPeriodOfTheirProgramme_ReadOnly_WithTheEndAtTheTop_DatesIso()
    {
        var cut = Render(Graduate(), standing: Standing(Epa(1, "PAED-001", "5", EntrustmentStandingStatus.AtOrAbove)));

        Text(cut.Find(".alert.alert-info")).Should().Be(
            "You completed your programme on 2026-06-30. This page is your record of it and is read-only: no target " +
            "applies to you any more. A period your programme ended in before that period's last month has no target, " +
            "and no period after it is listed.");
        cut.Find(".alert.alert-info").HasAttribute("role").Should().BeFalse("standing page content is not announced on every visit");
        Text(cut.Find(".page-subtitle")).Should().Be("Your record of the programme you ended: what your curriculum held you to in each period.");

        var programme = cut.FindAll("section.detail-card").Single(card => card.QuerySelector("h3") is { } h3 && Text(h3) == "Your programme");
        Details(programme.QuerySelector("dl.details-list")!).Should().Equal(new Dictionary<string, string>
        {
            ["Started"] = "2025-01-01",
            ["Completed"] = "2026-06-30",
            ["Training year"] = "2 when your programme ended"
        });

        var semester = EndedCard(cut, "PAED-001");
        Details(semester.QuerySelector("dl.details-list")!).Should().Equal(new Dictionary<string, string>
        {
            ["Semester 1, 2026"] = "2 of 3, 1 short; 1 at the minimum level when observed",
            ["Semester 2, 2025"] = "3 of 3, met; 3 at the minimum level when observed"
        }, "each line as the portfolio PDF prints it, newest first");
        Text(semester).Should().Contain("Target: 3 per semester (6 a year). Minimum when your programme ended: 5.");

        cut.FindAll(".progress-bar").Should().BeEmpty("nothing is still to do");
        PageText(cut).Should().NotContain("more by").And.NotContain("No curriculum items assigned yet");
        cut.FindAll("section.period-card").Should().BeEmpty();
        cut.Find("section.standing-panel");
    }

    [Fact]
    public void AnEndedProgrammesEpa_LinksToItsPage()
    {
        // The trajectories are on the EPA pages now (Q5), so the ended record reaches each one (R5).
        var cut = Render(Graduate());

        var link = EndedCard(cut, "PAED-001").QuerySelector("h3 a.epa-link")!;
        link.GetAttribute("href").Should().Be("/portfolio/progress/1");
        link.GetAttribute("aria-label").Should().Be("PAED-001 — Providing paediatric emergency care to children");
    }

    [Fact]
    public void AWithdrawnTrainee_IsToldTheirProgrammeEnded_AndThePeriodItEndedInHoldsNoTarget()
    {
        var cut = Render(Withdrawn());

        Text(cut.Find(".alert.alert-info")).Should().StartWith("Your programme ended on 2026-08-20. This page is your record of it");
        var programme = cut.FindAll("section.detail-card").Single(card => card.QuerySelector("h3") is { } h3 && Text(h3) == "Your programme");
        Details(programme.QuerySelector("dl.details-list")!)["Ended"].Should().Be("2026-08-20");
        Details(EndedCard(cut, "PAED-001").QuerySelector("dl.details-list")!).Should().Equal(new Dictionary<string, string>
        {
            ["Semester 2, 2026"] = "no target (your programme ended part-way through) · 1 recorded",
            ["Semester 1, 2026"] = "3 of 3, met; 2 at the minimum level when observed"
        });
    }

    [Fact]
    public void AnEndThatWasNeverRecorded_SaysSo_RatherThanInventingADay()
    {
        var cut = Render(Graduate() with { Ended = new ProgrammeEndDto(Completed: false, EndedOn: null, Today: new DateOnly(2026, 9, 23)) });

        Text(cut.Find(".alert.alert-info")).Should().Be(
            "Your programme has ended. Wombat did not record the day it ended, so your periods are shown up to 2026-09-23. " +
            "This page is your record of it and is read-only: no target applies to you any more.");
        var programme = cut.FindAll("section.detail-card").Single(card => card.QuerySelector("h3") is { } h3 && Text(h3) == "Your programme");
        Details(programme.QuerySelector("dl.details-list")!)["Ended"].Should().Be("The day was not recorded");
    }

    [Fact]
    public void AnEndedProgrammesCard_StillSaysWhetherAReleasedMsfCampaignCoveredTheEpa()
    {
        var coverage = new MsfCoverageDto(
            new DateOnly(2026, 8, 20),
            [
                new MsfCoveragePeriodDto(2026, 1, "Semester 1, 2026", "January to June", new(2026, 1, 1), new(2026, 6, 30), new(2026, 6, 30), HasEnded: true, EpasCovered: 1),
                new MsfCoveragePeriodDto(2026, 2, "Semester 2, 2026", "July to November", new(2026, 7, 1), new(2026, 12, 31), new(2026, 11, 30), HasEnded: false, EpasCovered: 0)
            ],
            [
                new MsfEpaCoverageDto(101, EpaId: 1, "PAED-001", "Providing paediatric emergency care to children", IsLocal: false,
                [
                    new MsfEpaPeriodCoverageDto(2026, 1, [new MsfCoveringCampaignDto(50, "Annual MSF", new(2026, 3, 10), new(2026, 3, 13))]),
                    new MsfEpaPeriodCoverageDto(2026, 2, [])
                ])
            ]);

        var cut = Render(Withdrawn(), msf: coverage);

        EndedCard(cut, "PAED-001").QuerySelectorAll("p.progress-row-meta").Select(Text).Should().Contain(
            "MSF in Semester 2, 2026: no released campaign covering this EPA has closed yet. " +
            "MSF in Semester 1, 2026: covered by a released campaign that closed on 2026-03-10.");
    }

    [Fact]
    public void AnEndedProgramme_SaysWhenItsMsfCoverageCouldNotBeLoaded_InFixedWords_AndStillShowsItsPeriods()
    {
        var sender = new FakeSender()
            .On<GetCurriculumProgressForTraineeQuery>(_ => Withdrawn())
            .On<GetEntrustmentStandingForTraineeQuery>(_ => null)
            .On<GetMsfCoverageForTraineeQuery>(_ => throw new InvalidOperationException(Secret));
        var cut = RenderWith(sender, waitFor: "Curriculum targets");

        Text(cut.Find("p.section-error")).Should().Be(
            "Multi-source feedback: Could not load MSF coverage. Your counts above are not affected.");
        cut.Markup.Should().NotContain(Secret);
        Details(EndedCard(cut, "PAED-001").QuerySelector("dl.details-list")!).Should().ContainKey("Semester 2, 2026");
        cut.Markup.Should().NotContain("MSF in ");
    }

    [Fact]
    public void AnEndedProgramme_ReadsItsStandingAndMsfCoverage_AsOnItsLastDay()
    {
        var sender = Sender(Withdrawn());
        RenderWith(sender, waitFor: "Curriculum targets");

        sender.Received.OfType<GetEntrustmentStandingForTraineeQuery>().Should().ContainSingle()
            .Which.AsOf.Should().Be(new DateOnly(2026, 8, 20));
        var coverage = sender.Received.OfType<GetMsfCoverageForTraineeQuery>().Should().ContainSingle().Which;
        (coverage.From, coverage.To, coverage.AsOf).Should().Be(((DateOnly?)null, (DateOnly?)new DateOnly(2026, 8, 20), (DateOnly?)null));
    }

    // ─── Fixtures ────────────────────────────────────────────────────────────

    /// <summary>Lerato Molefe on 2026-10-03, training year 4: PAED-001 met, PAED-012 two of three, PAED-008 none.</summary>
    private static TraineeCurriculumProgressSummaryDto Molefe()
        => Summary(D, new DateOnly(2023, 1, 15), 4,
        [
            Item("PAED-001", QuotaPeriod.Semester, 3, Counting(Semester2Of2026, 3, 3)) with { EpaId = 1, DecisionCadence = QuotaPeriod.Semester },
            Item("PAED-012", QuotaPeriod.Semester, 3, Counting(Semester2Of2026, 2, 3), title: "Communicating") with { EpaId = 12, DecisionCadence = QuotaPeriod.Semester },
            Item("PAED-008", QuotaPeriod.AcademicYear, 1, Counting(Year2026, 0, 1), title: "Evaluating") with { EpaId = 8, DecisionCadence = QuotaPeriod.AcademicYear }
        ]);

    /// <summary>Started 2025-01-01, completed 2026-06-30, read on 2026-09-23: every period back to the start (T252).</summary>
    private static TraineeCurriculumProgressSummaryDto Graduate()
        => Summary(new DateOnly(2026, 6, 30), new DateOnly(2025, 1, 1), 2,
        [
            Item("PAED-001", QuotaPeriod.Semester, 3, Counting(Semester1Of2026, 2, 3, minimumReached: 1),
                periods: [Counting(Semester1Of2026, 2, 3, minimumReached: 1), Counting(new Window("Semester 2, 2025", "July to November", new(2025, 7, 1), new(2025, 11, 30)), 3, 3, minimumReached: 3)])
                with { EpaId = 1 }
        ]) with { Ended = new ProgrammeEndDto(Completed: true, EndedOn: new DateOnly(2026, 6, 30), Today: new DateOnly(2026, 9, 23)) };

    /// <summary>Started 2025-01-01, deactivated with a last day of 2026-08-20 (D49: semester 2 holds no target).</summary>
    private static TraineeCurriculumProgressSummaryDto Withdrawn()
        => Summary(new DateOnly(2026, 8, 20), new DateOnly(2025, 1, 1), 2,
        [
            Item("PAED-001", QuotaPeriod.Semester, 3, Waived(Semester2Of2026, QuotaWindowStatus.ExemptProgrammeEnded, 1, 3),
                periods: [Waived(Semester2Of2026, QuotaWindowStatus.ExemptProgrammeEnded, 1, 3), Counting(Semester1Of2026, 3, 3, minimumReached: 2)])
                with { EpaId = 1 }
        ]) with { Ended = new ProgrammeEndDto(Completed: false, EndedOn: new DateOnly(2026, 8, 20), Today: new DateOnly(2026, 9, 23)) };

    private static EntrustmentStandingDto Standing(params EpaStandingDto[] epas)
        => new(D, new DateOnly(2023, 1, 15), 4, false, epas, new ExitRuleReadinessDto(epas.Length, 0, [new ExitLevelGroupDto(6, "5", epas.Length, 0)], epas.Select(epa => epa.EpaCode).ToArray()));

    private static EpaStandingDto Epa(int epaId, string code, string? level, EntrustmentStandingStatus status, string? ladder = null)
        => new(100 + epaId, epaId, code, $"{code} title", "CPSA Paediatric Entrustment Scale v11.1", IsLocal: false, 6, "5", false, 6, "5",
            level is null ? null : new StandingDecisionDto(epaId, 6, level, ladder, D, null), status, status, null);

    private static MsfCoverageDto Coverage(int covered, int epas)
        => new(
            D,
            [
                new MsfCoveragePeriodDto(2026, 1, "Semester 1, 2026", "January to June", new(2026, 1, 1), new(2026, 6, 30), new(2026, 6, 30), HasEnded: true, EpasCovered: 0),
                new MsfCoveragePeriodDto(2026, 2, "Semester 2, 2026", "July to November", new(2026, 7, 1), new(2026, 12, 31), new(2026, 11, 30), HasEnded: false, EpasCovered: covered)
            ],
            Enumerable.Range(1, epas)
                .Select(id => new MsfEpaCoverageDto(200 + id, id, $"PAED-00{id}", "t", IsLocal: false,
                    [new MsfEpaPeriodCoverageDto(2026, 1, []), new MsfEpaPeriodCoverageDto(2026, 2, [])]))
                .ToList());

    private static FakeSender Sender(
        TraineeCurriculumProgressSummaryDto? summary, EntrustmentStandingDto? standing = null, MsfCoverageDto? msf = null)
        => new FakeSender()
            .On<GetCurriculumProgressForTraineeQuery>(_ => summary)
            .On<GetEntrustmentStandingForTraineeQuery>(_ => standing)
            .On<GetMsfCoverageForTraineeQuery>(_ => msf);

    private IRenderedComponent<MyProgress> Render(
        TraineeCurriculumProgressSummaryDto? summary, EntrustmentStandingDto? standing = null, MsfCoverageDto? msf = null)
        => RenderWith(Sender(summary, standing, msf));

    private IRenderedComponent<MyProgress> RenderWith(FakeSender sender, string? waitFor = null)
    {
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<MyProgress>();
        cut.WaitForState(() => waitFor is null
            ? !cut.Markup.Contains(MyProgress.LoadingWords)
            : cut.Markup.Contains(waitFor));
        return cut;
    }

    /// <summary>The element reference the last focus call was given (bUnit renders each @ref as blazor:elementreference).</summary>
    private string? FocusedReference()
        => JSInterop.Invocations.Last(invocation => invocation.Identifier == FocusIdentifier)
            .Arguments[0].Should().BeOfType<ElementReference>().Which.Id;

    /// <summary>Whether <paramref name="first" /> comes before <paramref name="second" /> in the page's markup.</summary>
    private static bool IsBefore(IRenderedFragment cut, string first, string second)
        => cut.Markup.IndexOf(first, StringComparison.Ordinal) is >= 0 and var a
           && cut.Markup.IndexOf(second, StringComparison.Ordinal) is var b && a < b;

    private static IElement IndexRow(IRenderedFragment cut, string code)
        => cut.FindAll("section.index-section tbody tr")
            .Single(row => Text(row.QuerySelector("th")!).StartsWith(code + " ", StringComparison.Ordinal));

    private static IElement EndedCard(IRenderedFragment cut, string code)
        => cut.FindAll(".dashboard-grid > section.detail-card")
            .Single(card => Text(card.QuerySelector(".progress-row-head h3")!) == code);

    private static IReadOnlyList<(string Value, string Label)> Metrics(IElement card)
        => card.QuerySelectorAll(".dashboard-metric")
            .Select(metric => (Text(metric.QuerySelector(".dashboard-metric-value")!), Text(metric.QuerySelector(".dashboard-metric-label")!)))
            .ToList();

    private static Dictionary<string, string> Details(IElement detailsList)
        => detailsList.Children
            .Where(child => child.LocalName == "div")
            .ToDictionary(row => Text(row.QuerySelector("dt")!), row => Text(row.QuerySelector("dd")!));

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private static string PageText(IRenderedFragment cut)
        => Regex.Replace(string.Join(" ", cut.Nodes.Select(node => node.TextContent)), @"\s+", " ").Trim();

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    /// <summary>Answers each request type it is told about and records every request it receives.</summary>
    private sealed class FakeSender : IScopedSender
    {
        private readonly Dictionary<Type, Func<object, Task<object?>>> _answers = [];

        public List<object> Received { get; } = [];

        public FakeSender On<TRequest>(Func<TRequest, object?> answer)
        {
            _answers[typeof(TRequest)] = request => Task.FromResult(answer((TRequest)request));
            return this;
        }

        public FakeSender OnAsync<TRequest>(Func<TRequest, Task<object?>> answer)
        {
            _answers[typeof(TRequest)] = request => answer((TRequest)request);
            return this;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Received.Add(request);

            if (!_answers.TryGetValue(request.GetType(), out var answer))
            {
                throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }

            return (TResponse)(await answer(request))!;
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
