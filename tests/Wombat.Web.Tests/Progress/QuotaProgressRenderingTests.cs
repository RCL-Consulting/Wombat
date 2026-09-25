using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Commands.RebuildCurriculumProgress;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Dashboards.Trainee;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.CurriculumProgress;
using Wombat.Web.Components.Pages.Dashboards;
using Wombat.Web.Components.Pages.Portfolio;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Progress;

/// <summary>
/// T130: the surfaces the annual quota is read on, rendered from hand-built read models.
/// </summary>
/// <remarks>
/// <para>
/// The figures themselves are computed, and tested, in the Application layer (<c>QuotaProgressCalculator</c>,
/// <c>TraineeQuotaProgressReader</c>, <c>CurriculumCoverageReader</c>). What only a render can show is the copy:
/// that a semester item reads "this semester" and an annual one "in 2026", that an exempt item loses its bar
/// rather than showing a target it cannot meet (D14), that the closed window's result survives the boundary,
/// and that the rebuild runs only once the operator has confirmed it.
/// </para>
/// <para>
/// Every date is fixed. 23 September 2026 is in semester 2 of the 2026 academic year, whose College end is
/// 30 November. The read models are what the reader would build for the stated programme start on that day,
/// so a fixture that could not occur (a semester item exempt beside a counting one, say) is never rendered.
/// </para>
/// </remarks>
public sealed class QuotaProgressRenderingTests : WombatTestContext
{
    private static readonly DateOnly AsOf = new(2026, 9, 23);

    private static readonly WindowShape Semester1Of2026 = new("Semester 1, 2026", "January to June", new(2026, 1, 1), new(2026, 6, 30));
    private static readonly WindowShape Semester2Of2026 = new("Semester 2, 2026", "July to November", new(2026, 7, 1), new(2026, 11, 30));
    private static readonly WindowShape Year2026 = new("2026 academic year", "January to November", new(2026, 1, 1), new(2026, 11, 30));
    private static readonly WindowShape Year2025 = new("2025 academic year", "January to November", new(2025, 1, 1), new(2025, 11, 30));

    private readonly CultureInfo _originalCulture = CultureInfo.CurrentCulture;
    private readonly CultureInfo _originalUiCulture = CultureInfo.CurrentUICulture;
    private readonly TestAuthorizationContext _auth;

    public QuotaProgressRenderingTests()
    {
        _auth = this.AddTestAuthorization();
        _auth.SetAuthorized("trainee@test");
        _auth.SetRoles(WombatRoles.Trainee);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "trainee-1"));
    }

    // ---------------------------------------------------------------------------------------------
    // MyProgress
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void ASemesterItem_ReadsAgainstThisSemestersTarget_WithTheCollegeEndAsItsDueDate()
    {
        // The question the page exists to answer: what is expected of me this period. Before T130 this
        // card read "1 / 24", a lifetime multiple nobody published.
        var cut = RenderMyProgress(BoundaryStarter());
        var card = ItemCard(cut, "PAED-001");

        Text(card.QuerySelector(".progress-row-head .muted")!).Should().Be("1 of 3 this semester");
        Text(card).Should().Contain("2 more by 30 November 2026.",
            "semester 2 closes on the College's 30 November, not on the 31 December the resolver folds into it");
        Text(card).Should().Contain("At the minimum level when observed: 1 of 1.");
        Text(card).Should().Contain("Last encounter date: 2026-08-12.",
            "a stated date reads as the encounter date alone, in EncounterDate.Label's words (T219)");
        Text(card).Should().NotContain("not recorded");
        Text(card).Should().Contain("Target: 3 per semester (6 a year).");
        Text(card).Should().Contain("Minimum now 3a.");

        var bar = card.QuerySelector(".progress-bar")!;
        bar.GetAttribute("role").Should().Be("progressbar");
        bar.GetAttribute("aria-valuemin").Should().Be("0");
        bar.GetAttribute("aria-valuemax").Should().Be("3");
        bar.GetAttribute("aria-valuenow").Should().Be("1");
        bar.GetAttribute("aria-label").Should().Be("PAED-001: 1 of 3 this semester");
        bar.QuerySelector(".progress-bar-fill")!.GetAttribute("style").Should().Be("width:33%");
        bar.QuerySelector(".progress-bar-fill")!.ClassList.Should().NotContain("is-complete");
    }

    [Fact]
    public void TheClosedSemestersResult_SurvivesTheBoundary()
    {
        // Without a previous-window line, a semester 1 shortfall would vanish from the product on 1 July,
        // which is exactly when a committee sitting at the boundary wants it.
        var cut = RenderMyProgress(BoundaryStarter());

        Text(ItemCard(cut, "PAED-001")).Should().Contain("Semester 1, 2026: 2 of 3, 1 short");
        Text(ItemCard(cut, "PAED-006")).Should().Contain("Semester 1, 2026: 2 of 2, met");
        Text(ItemCard(cut, "PAED-011")).Should().Contain("2025 academic year: 1 of 1, met");
    }

    [Fact]
    public void AMetSemesterTarget_SaysSo_AndFillsTheBar()
    {
        var cut = RenderMyProgress(BoundaryStarter());
        var card = ItemCard(cut, "PAED-006");

        Text(card.QuerySelector(".progress-row-head .muted")!).Should().Be("2 of 2 this semester");
        Text(card).Should().Contain("Target met for Semester 2, 2026.");
        Text(card).Should().NotContain("more by");
        card.QuerySelector(".progress-bar")!.GetAttribute("aria-valuemax").Should().Be("2");
        var fill = card.QuerySelector(".progress-bar-fill")!;
        fill.GetAttribute("style").Should().Be("width:100%");
        fill.ClassList.Should().Contain("is-complete");
    }

    [Fact]
    public void AnAnnualItem_ReadsAgainstTheAcademicYear()
    {
        var cut = RenderMyProgress(BoundaryStarter());
        var card = ItemCard(cut, "PAED-011");

        Text(card.QuerySelector(".progress-row-head .muted")!).Should().Be("0 of 1 in 2026");
        Text(card).Should().Contain("1 more by 30 November 2026.");
        Text(card).Should().Contain("Target: 1 per academic year.");
        Text(card).Should().NotContain("a year)", "the '(n a year)' gloss is only for semester targets");
        Text(card).Should().NotContain("At the minimum level when observed",
            "with nothing counted there is no share of encounters to report");
        Text(card).Should().NotContain("Last encounter");

        var bar = card.QuerySelector(".progress-bar")!;
        bar.GetAttribute("aria-valuemax").Should().Be("1");
        bar.GetAttribute("aria-valuenow").Should().Be("0");
        bar.GetAttribute("aria-label").Should().Be("PAED-011: 0 of 1 in 2026");
    }

    [Fact]
    public void AnUndatedLastEncounter_IsMarkedAsTheDayItsFormWasCreated()
    {
        // T219 (split from T197). The latest encounter this semester was filed with no encounter date, so the date the
        // progress row holds is only the day its form was created. Printed bare, it passes the audit clock off as a
        // clinical fact; it reads in EncounterDate.Label's words instead, the sentence naming the date so that "not
        // recorded" cannot read as though the encounter were not. The item beside it, whose date was stated, is not
        // marked, and neither is its own previous-semester line, which prints no date.
        var cut = RenderMyProgress(Summary(
            AsOf,
            programmeStart: new(2025, 1, 1),
            stage: 2,
            [
                Item(1, "PAED-001", "Providing paediatric emergency care to children", QuotaPeriod.Semester, 3,
                    Counting(Semester2Of2026, count: 2, target: 3, minimumReached: 2, lastObservedOn: new(2026, 9, 10), lastObservedOnDeclared: false)),
                Item(6, "PAED-006", "Managing long-term health conditions (LTHCs)", QuotaPeriod.Semester, 2,
                    Counting(Semester2Of2026, count: 1, target: 2, minimumReached: 1, lastObservedOn: new(2026, 9, 1)))
            ]));

        var undated = Text(ItemCard(cut, "PAED-001"));
        undated.Should().Contain("Last encounter date: not recorded (created 2026-09-10).");
        undated.Should().NotContain("10 Sep 2026", "the created day must not also be printed as though it were the encounter");

        Text(ItemCard(cut, "PAED-006")).Should().Contain("Last encounter date: 2026-09-01.");
        Text(ItemCard(cut, "PAED-006")).Should().NotContain("not recorded");
    }

    [Fact]
    public void Items_AreGroupedUnderEachSemesterAndOnceAYear()
    {
        // Annexure B frames the two differently: 25 per semester, and 5 "as opportunities arise". The
        // groups carry that framing so the period word need not be repeated on every card.
        var cut = RenderMyProgress(BoundaryStarter());

        var groups = cut.FindAll("h3.progress-group-title").ToList();
        groups.Select(Text).Should().Equal("Each semester", "Once a year");

        CardCodesUnder(groups[0]).Should().Equal("PAED-001", "PAED-006");
        CardCodesUnder(groups[1]).Should().Equal("PAED-011");
    }

    [Fact]
    public void ThePeriodSummary_CountsTargetsMetByKind_AndNamesTheTrainingYearApart()
    {
        // D17: the training year (365-day blocks from the start) and the academic year (the College's
        // calendar) are different things, and the page must never let one read as the other.
        var cut = RenderMyProgress(BoundaryStarter());
        var details = Details(cut.Find("section.detail-card dl.details-list"));

        details["Semester"].Should().Be("Semester 2, 2026 · July to November");
        details["Semester targets"].Should().Be("1 of 2 EPAs met this semester");
        details["Yearly targets"].Should().Be("0 of 1 EPAs met in 2026");
        details["Training year"].Should().Be("2 — it sets the minimum level each encounter is judged against");
    }

    [Fact]
    public void ABoundaryStarter_SeesNoExemptionNotice()
    {
        var cut = RenderMyProgress(BoundaryStarter());

        cut.FindAll(".alert").Should().BeEmpty();
        PageText(cut).Should().NotContain("part-way");
    }

    [Fact]
    public void AnExemptItem_ShowsItsCountAndWhenTargetsStart_ButNoBarAndNoTarget()
    {
        // D14: a registrar who starts part-way through is exempt for the partial period. The page says so
        // rather than showing a target they cannot meet. The encounters are still evidence, so the count
        // stays visible; there is simply nothing to measure it against.
        var cut = RenderMyProgress(LateStarter());

        var semesterCard = ItemCard(cut, "PAED-001");
        semesterCard.QuerySelectorAll(".progress-bar").Should().BeEmpty();
        semesterCard.QuerySelector(".progress-row-head .muted").Should().BeNull("no fraction is shown for a waived target");
        Text(semesterCard).Should().Contain("No target this semester · 1 recorded · targets start with semester 1, 2027");
        Text(semesterCard).Should().NotContain("more by");

        var annualCard = ItemCard(cut, "PAED-011");
        annualCard.QuerySelectorAll(".progress-bar").Should().BeEmpty();
        Text(annualCard).Should().Contain("No target in 2026 · 0 recorded · targets start with the 2027 academic year");

        var details = Details(cut.Find("section.detail-card dl.details-list"));
        details["Semester targets"].Should().Be("none apply yet");
        details["Yearly targets"].Should().Be("none apply yet");
    }

    [Fact]
    public void AnExemptStart_IsExplainedOnceAtPageLevel_NamingTheStartAndTheFirstCountedWindows()
    {
        // Fifteen cards each saying "targets start ..." with two different dates, and nothing reconciling
        // them, is what the page-level notice prevents. It must also say the exempt encounters do not carry.
        // A 20 September start waives both kinds, and each kind is explained in its own sentence: D42 waives them
        // for different reasons, and usually only one of them.
        var cut = RenderMyProgress(LateStarter());

        var alert = cut.Find(".alert.alert-info");
        Text(alert).Should().Be(
            "You started the programme on 20 September 2026. That was part-way through the semester, so under the " +
            "College's rule no semester target applies until semester 1, 2027, which starts on 1 January 2027. " +
            "Encounters on those EPAs before then stay in your portfolio as evidence, but do not count towards a later " +
            "semester's target. That was in the second half of the academic year, so under the College's rule no " +
            "yearly target applies until the 2027 academic year, which starts on 1 January 2027. Encounters on those " +
            "EPAs before then stay in your portfolio as evidence, but do not count towards a later year's target.");
    }

    [Fact]
    public void AMidJulyStarter_CountsSemesterItems_ButIsExemptFromTheYear_AndOnlyTheYearlyStartIsNamed()
    {
        // D42: a start inside a semester's first calendar month counts as a boundary start, but only a
        // semester-1 start keeps the academic year. So on the same page one kind carries a bar and the other
        // does not, which is the case a page-wide "exempt" switch would get wrong.
        var cut = RenderMyProgress(MidJulyStarter());

        var semesterCard = ItemCard(cut, "PAED-001");
        semesterCard.QuerySelectorAll(".progress-bar").Should().ContainSingle();
        Text(semesterCard.QuerySelector(".progress-row-head .muted")!).Should().Be("1 of 3 this semester");

        var annualCard = ItemCard(cut, "PAED-011");
        annualCard.QuerySelectorAll(".progress-bar").Should().BeEmpty();
        Text(annualCard).Should().Contain("No target in 2026 · 0 recorded · targets start with the 2027 academic year");

        // The review of T130 found the first version of this notice telling a July starter that "no target applies
        // until the next boundary" while ten semester targets applied on the same page.
        var alert = Text(cut.Find(".alert.alert-info"));
        alert.Should().Contain("You started the programme on 15 July 2026.");
        alert.Should().Contain("no yearly target applies until the 2027 academic year, which starts on 1 January 2027.");
        alert.Should().Contain("Your semester targets apply now.");
        alert.Should().NotContain("no semester target", "semester targets already apply to this trainee");
        alert.Should().NotContain("no target applies");
    }

    [Fact]
    public void APreviousSemesterTheTraineeWasExemptFor_ReadsAsNoTarget_NotAsAShortfall()
    {
        // A 1 April starter is exempt for semester 1 but held to semester 2. Their semester-1 line must not
        // read "1 of 3, 2 short": no target applied, so nothing was short.
        var cut = RenderMyProgress(AprilStarter());

        var card = ItemCard(cut, "PAED-001");
        Text(card).Should().Contain("Semester 1, 2026: no target (you started part-way through) · 1 recorded");
        Text(card).Should().NotContain("short");

        // The 2025 academic year lies wholly before the start, so the reader passes no previous window.
        Text(ItemCard(cut, "PAED-011")).Should().NotContain("2025 academic year");
    }

    [Fact]
    public void AWindowTheProgrammeEndedIn_OrAfter_ReadsAsNoTarget_NeverAsALateStartOrAShortfall()
    {
        // T209, D49. The one fixture here no reader builds today: the page reads the active profile only, and an active
        // programme has not ended. It pins the copy for the statuses D49 added, so that no reader handing the page one can
        // make an ended programme read "you started part-way through", "targets start with" or "short". Programme ended
        // 15 May 2026: semester 1 was cut short before June, semester 2 is after the end, and the year was cut short
        // before November.
        var cut = RenderMyProgress(Summary(
            AsOf,
            programmeStart: new(2025, 1, 1),
            stage: 2,
            [
                Item(1, "PAED-001", "Providing paediatric emergency care to children", QuotaPeriod.Semester, 3,
                    NoTargetAfterTheEnd(Semester2Of2026, QuotaWindowStatus.AfterProgrammeEnd, count: 2, target: 3),
                    previous: NoTargetAfterTheEnd(Semester1Of2026, QuotaWindowStatus.ExemptProgrammeEnded, count: 1, target: 3)),
                Item(11, "PAED-011", "Managing population health challenges", QuotaPeriod.AcademicYear, 1,
                    NoTargetAfterTheEnd(Year2026, QuotaWindowStatus.ExemptProgrammeEnded, count: 0, target: 1),
                    previous: Counting(Year2025, count: 1, target: 1, minimumReached: 1))
            ]));

        var semester = ItemCard(cut, "PAED-001");
        Text(semester).Should().Contain("No target: this is after your programme ended · 2 recorded");
        Text(semester).Should().Contain("Semester 1, 2026: no target (your programme ended part-way through) · 1 recorded");
        semester.QuerySelector(".progress-bar").Should().BeNull("no target applies, so there is nothing to fill");

        var year = ItemCard(cut, "PAED-011");
        Text(year).Should().Contain("No target in 2026 · 0 recorded · your programme ended part-way through");
        Text(year).Should().Contain("2025 academic year: 1 of 1, met");
        year.QuerySelector(".progress-bar").Should().BeNull();

        foreach (var card in new[] { semester, year })
        {
            Text(card).Should().NotContain("started part-way");
            Text(card).Should().NotContain("targets start with");
            Text(card).Should().NotContain("short");
            Text(card).Should().NotContain("No target yet");
        }
    }

    [Fact]
    public void AProgrammeNotYetStarted_SaysWhenItStarts_AndWhichWindowsCountFirst()
    {
        var cut = RenderMyProgress(NotYetStarted());

        var alert = Text(cut.Find(".alert.alert-info"));
        alert.Should().Be(
            "Your programme starts on 1 January 2027. Your first semester targets are for semester 1, 2027. " +
            "Your first yearly targets are for the 2027 academic year.");
        alert.Should().NotContain("part-way", "a trainee who has not started has not started part-way through anything");

        var semesterCard = ItemCard(cut, "PAED-001");
        semesterCard.QuerySelectorAll(".progress-bar").Should().BeEmpty();
        Text(semesterCard).Should().Contain("No target yet · targets start with semester 1, 2027");
        Text(semesterCard).Should().NotContain("recorded");
        Text(ItemCard(cut, "PAED-011")).Should().Contain("No target yet · targets start with the 2027 academic year");

        var details = Details(cut.Find("section.detail-card dl.details-list"));
        details.Should().NotContainKey("Training year", "there is no training year before the programme starts");
    }

    [Fact]
    public void InDecember_ThePageSaysTheTeachingYearHasEnded_AndThatDecemberStillCounts()
    {
        // D40 folds December into semester 2 so the resolver is total, but the College's year runs January
        // to November. The page names the College's months and explains the fold rather than implying
        // semester 2 runs to 31 December.
        var cut = RenderMyProgress(DecemberReader());

        var alert = Text(cut.Find(".alert.alert-info"));
        alert.Should().Be(
            "The 2026 academic year ended on 30 November. Encounters observed in December still count towards " +
            "semester 2, 2026 and your 2026 yearly targets. Semester 1, 2027 starts on 1 January.");

        var details = Details(cut.Find("section.detail-card dl.details-list"));
        details["Semester"].Should().Be("Semester 2, 2026 · July to November");
        PageText(cut).Should().NotContain("31 December");
    }

    [Fact]
    public void BeforeDecember_ThereIsNoEndOfYearNotice()
    {
        var cut = RenderMyProgress(BoundaryStarter());

        PageText(cut).Should().NotContain("academic year ended");
    }

    [Fact]
    public void ATraineeWithNoCurriculum_IsToldSo()
    {
        // The reader returns null when there is no active trainee profile.
        var cut = RenderMyProgress(null);

        PageText(cut).Should().Contain("No curriculum items assigned yet.");
        cut.FindAll(".progress-bar").Should().BeEmpty();
    }

    [Fact]
    public void AnAdmittedTraineeWithNoItemInForce_IsNotToldToWaitForAdmission()
    {
        // T158. The reader returns a summary with no items when every EPA on the trainee's curriculum is inactive. They
        // are admitted, so "once you are admitted" would be wrong.
        var cut = RenderMyProgress(Summary(AsOf, programmeStart: new(2025, 1, 1), stage: 2, []));

        PageText(cut).Should().Contain("No EPA on your curriculum is in use at the moment, so no target applies to you.")
            .And.NotContain("Once you are admitted");
        cut.FindAll(".progress-bar").Should().BeEmpty();
    }

    [Fact]
    public void ThePage_AsksForTheSignedInTraineesProgress_WithoutPinningADate()
    {
        // The page takes "today" from the reader's South African calendar. A page that pinned AsOf would
        // freeze its period.
        var sender = new FakeSender()
            .On<GetCurriculumProgressForTraineeQuery>(_ => BoundaryStarter())
            .On<GetEpaTrajectoryForTraineeQuery>(_ => Array.Empty<EpaTrajectoryDto>())
            .On<GetEntrustmentStandingForTraineeQuery>(_ => null)
            .On<GetMsfCoverageForTraineeQuery>(_ => null);
        RenderMyProgressWith(sender);

        var query = sender.Received.OfType<GetCurriculumProgressForTraineeQuery>().Should().ContainSingle().Which;
        query.TraineeUserId.Should().Be("trainee-1");
        query.AsOf.Should().BeNull();

        // T113: the handler authorises against the caller, so the page must pass the signed-in principal itself.
        query.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be("trainee-1");
    }

    // ---------------------------------------------------------------------------------------------
    // TraineeDashboard
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void TheDashboardCard_ShowsTargetsMetByKind_AndTheUnmetItems()
    {
        var cut = RenderTraineeDashboard(BoundaryStarter());
        var card = CurriculumTargetsCard(cut);

        card.GetAttribute("href").Should().Be("/portfolio/progress");
        Text(card.QuerySelector("h3")!).Should().Be("Curriculum targets");
        Text(card).Should().Contain("Semester 2, 2026 · July to November");
        Metrics(card).Should().Equal(
            ("1 / 2", "semester targets met"),
            ("0 / 1", "yearly targets met (2026)"));

        var rows = card.QuerySelectorAll(".progress-row").ToList();
        rows.Select(row => Text(row.QuerySelector(".progress-row-head span")!))
            .Should().Equal(
                "PAED-001 — Providing paediatric emergency care to children",
                "PAED-011 — Managing population health challenges");
        Text(rows[0].QuerySelector(".progress-row-head .muted")!).Should().Be("1 of 3 this semester");
        Text(rows[1].QuerySelector(".progress-row-head .muted")!).Should().Be("0 of 1 in 2026");
        rows[0].QuerySelector(".progress-bar")!.GetAttribute("aria-valuemax").Should().Be("3");
        Text(card).Should().NotContain("PAED-006", "a met target is not a thing to chase");
        Text(card).Should().NotContain("part-way");
    }

    [Fact]
    public void TheDashboardCard_ListsAtMostFiveUnmetItems_FurthestFromTarget_AndNoExemptOnes()
    {
        // A 15-item wall is not a glance. The card keeps the five furthest from target (ties by code) and
        // leaves out anything exempt, which has no target to be short of. A mid-July starter is held to the
        // semester (D42) but not to the year, so every annual item here is exempt.
        var cut = RenderTraineeDashboard(MidJulyStarterWithManyItems());
        var card = CurriculumTargetsCard(cut);

        var codes = card.QuerySelectorAll(".progress-row .progress-row-head span:first-child")
            .Select(span => Text(span).Split(' ')[0])
            .ToList();
        codes.Should().Equal("PAED-001", "PAED-003", "PAED-002", "PAED-005", "PAED-004");

        Text(card).Should().NotContain("PAED-007", "the sixth unmet item is cut");
        Text(card).Should().NotContain("PAED-006").And.NotContain("PAED-010");
        Text(card).Should().NotContain("PAED-008", "an exempt item has no target to be short of");
        Text(card).Should().NotContain("PAED-011");

        // No "0 / 0": a kind with no applying target says so (the review of T130 found the first version printing it).
        Metrics(card).Should().Equal(
            ("2 / 8", "semester targets met"),
            ("—", "no yearly target applies yet"));
        Text(card).Should().Contain(
            "You started part-way through a period. Yearly targets begin with the 2027 academic year.");
        Text(card).Should().NotContain("Semester targets begin");
    }

    [Fact]
    public void TheDashboardCard_WithNoCurriculum_SaysSo()
    {
        var cut = RenderTraineeDashboard(null);
        var card = CurriculumTargetsCard(cut);

        Text(card).Should().Contain("No curriculum assigned yet.");
        card.QuerySelectorAll(".dashboard-metric").Should().BeEmpty();
    }

    [Fact]
    public void TheDashboardCard_ForAnAdmittedTraineeWithNoItemInForce_DoesNotSayNoCurriculum()
    {
        // T158: every EPA on the curriculum is inactive, so the summary has no items, but the trainee is admitted.
        var cut = RenderTraineeDashboard(Summary(AsOf, programmeStart: new(2025, 1, 1), stage: 2, []));
        var card = CurriculumTargetsCard(cut);

        Text(card).Should().Contain("No EPA on your curriculum is in use at the moment, so no target applies to you.")
            .And.NotContain("No curriculum assigned yet.");
        card.QuerySelectorAll(".dashboard-metric").Should().BeEmpty();
    }

    // ---------------------------------------------------------------------------------------------
    // EpaTargetCoverageList
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Coverage_IsACountOfTraineesWhoMetTheTarget_NotAMean()
    {
        // "4 of 9" tells a coordinator five have not; "44%" could mean everyone is halfway.
        var cut = RenderCoverage(Coverage(
            exemptTrainees: 2,
            new EpaTargetCoverage(1, "PAED-001", "Providing paediatric emergency care to children", QuotaPeriod.Semester, 3, 4, 9, 2)));

        var row = cut.Find(".progress-row");
        Text(row.QuerySelector(".progress-row-head > span:first-child")!)
            .Should().Be("PAED-001 — Providing paediatric emergency care to children (3 per semester)");
        Text(row.QuerySelector(".progress-row-head > .muted")!).Should().Be("4 of 9 met");

        var bar = row.QuerySelector(".progress-bar")!;
        bar.GetAttribute("role").Should().Be("progressbar");
        bar.GetAttribute("aria-valuemax").Should().Be("9");
        bar.GetAttribute("aria-valuenow").Should().Be("4");
        bar.GetAttribute("aria-label").Should().Be("PAED-001: 4 of 9 trainees met the target");
        bar.QuerySelector(".progress-bar-fill")!.GetAttribute("style").Should().Be("width:44%");
        bar.QuerySelector(".progress-bar-fill")!.ClassList.Should().NotContain("is-complete");
    }

    [Fact]
    public void AnEpaEveryTraineeIsExemptFrom_ReadsAllExempt_WithNoBar()
    {
        // Nobody applying is not 0%: a bar would read as a coverage failure, and a percentage would divide
        // by zero.
        var cut = RenderCoverage(Coverage(
            exemptTrainees: 0,
            new EpaTargetCoverage(11, "PAED-011", "Managing population health challenges", QuotaPeriod.AcademicYear, 1, 0, 0, 3)));

        var row = cut.Find(".progress-row");
        Text(row.QuerySelector(".progress-row-head > span:first-child")!)
            .Should().Be("PAED-011 — Managing population health challenges (1 per academic year)");
        Text(row.QuerySelector(".progress-row-head > .muted")!).Should().Be("all exempt");
        row.QuerySelectorAll(".progress-bar").Should().BeEmpty();
        Text(row).Should().NotContain("of 0");
    }

    [Fact]
    public void AnEpaEveryApplyingTraineeMet_FillsTheBar()
    {
        var cut = RenderCoverage(Coverage(
            exemptTrainees: 0,
            new EpaTargetCoverage(6, "PAED-006", "Managing long-term health conditions (LTHCs)", QuotaPeriod.Semester, 2, 5, 5, 0)));

        Text(cut.Find(".progress-row-head > .muted")).Should().Be("5 of 5 met");
        var fill = cut.Find(".progress-bar-fill");
        fill.GetAttribute("style").Should().Be("width:100%");
        fill.ClassList.Should().Contain("is-complete");
    }

    [Fact]
    public void TheCoverageHeading_NamesTheSemester_AndCountsExemptTrainees()
    {
        var plural = RenderCoverage(Coverage(
            exemptTrainees: 2,
            new EpaTargetCoverage(1, "PAED-001", "Providing paediatric emergency care to children", QuotaPeriod.Semester, 3, 4, 9, 2)));
        Text(plural.Find("p.progress-row-meta")).Should().Be("Semester 2, 2026 · July to November · 2 trainees exempt this period");

        var singular = RenderCoverage(Coverage(
            exemptTrainees: 1,
            new EpaTargetCoverage(1, "PAED-001", "Providing paediatric emergency care to children", QuotaPeriod.Semester, 3, 4, 9, 1)));
        Text(singular.Find("p.progress-row-meta")).Should().Be("Semester 2, 2026 · July to November · 1 trainee exempt this period");

        var none = RenderCoverage(Coverage(
            exemptTrainees: 0,
            new EpaTargetCoverage(1, "PAED-001", "Providing paediatric emergency care to children", QuotaPeriod.Semester, 3, 4, 9, 0)));
        Text(none.Find("p.progress-row-meta")).Should().Be("Semester 2, 2026 · July to November");
    }

    [Fact]
    public void CoverageWithNoEpas_SaysThereAreNoTargets()
    {
        var cut = RenderCoverage(Coverage(exemptTrainees: 0));

        Text(cut.Find("p.muted")).Should().Be("No curriculum targets for these trainees.");
        cut.FindAll(".progress-row").Should().BeEmpty();
    }

    // ---------------------------------------------------------------------------------------------
    // CurriculumProgressRebuild
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void TheRebuildPage_OffersTheButton_AndSendsNothingUntilConfirmed()
    {
        // The rebuild re-scores every trainee in every institution. Opening the dialog must not start it.
        var sender = new FakeSender().On<RebuildCurriculumProgressCommand>(_ => ARebuildResult());
        var cut = RenderRebuildPage(sender);

        var button = PageRebuildButton(cut);
        button.HasAttribute("disabled").Should().BeFalse();

        button.Click();

        JSInterop.VerifyInvoke("wombatDialog.showModal");
        sender.Received.Should().BeEmpty("only the dialog's confirm button runs the rebuild");
        cut.FindAll("dl.details-list").Should().BeEmpty();
    }

    [Fact]
    public void TheRebuildPage_SaysAnInactiveEpaKeepsItsProgress_AndNoLongerWarnsItIsLost()
    {
        // T196, D48. Until T196 a rebuild while an EPA was inactive removed the progress it had earned, and this page
        // warned of it. The rebuild now judges each completion at its own moment, so it loses nothing; the page says when
        // a paused completion counts instead, and the dialog no longer threatens the loss.
        var cut = RenderRebuildPage(new FakeSender());

        cut.FindAll(".alert.alert-warning").Should().BeEmpty("a rebuild no longer removes an inactive EPA's progress");
        cut.Markup.Should()
            .Contain("An inactive EPA keeps the progress it earned while it was active.")
            .And.Contain("reactivating it credits those activities without a rebuild");
        Text(cut.Find("dialog")).Should().Contain("An inactive EPA keeps the progress it earned while it was active.")
            .And.NotContain("keeps none of its progress");
    }

    [Fact]
    public void TheRebuildPage_SaysItCreditsAgainstTodaysCurriculum_AndJudgesOnlyAnEpasPauseAsOfTheCompletion()
    {
        // T196 review. A rebuild reads today's items, targets and scale pins; that is what makes it the repair after a
        // curriculum edit. Only whether an EPA was active is judged as of the completion. The page said the opposite:
        // "against the curriculum as it stood when the activity was completed".
        var cut = RenderRebuildPage(new FakeSender());

        var text = Text(cut.Find(".form-container"));
        text.Should().Contain("A rebuild credits each activity against the curriculum as it is today: its items, targets and minimum levels.")
            .And.Contain("Only whether an EPA was active is judged as of when the activity was completed.")
            .And.NotContain("as it stood when the activity was completed");
    }

    [Fact]
    public void ConfirmingTheRebuild_SendsExactlyOneGlobalRebuild_AndListsWhatItMoved()
    {
        var sender = new FakeSender().On<RebuildCurriculumProgressCommand>(_ => ARebuildResult());
        var cut = RenderRebuildPage(sender);

        PageRebuildButton(cut).Click();
        ConfirmButton(cut).Click();

        var command = sender.Received.Should().ContainSingle().Which.Should().BeOfType<RebuildCurriculumProgressCommand>().Which;
        command.TraineeUserId.Should().BeNull("the page rebuilds everybody");
        command.Principal.IsInRole(WombatRoles.Administrator).Should().BeTrue("the handler authorizes the signed-in caller");

        Text(cut.Find(".alert.alert-success")).Should().Be("Curriculum progress was rebuilt.");
        Details(cut.Find("dl.details-list")).Should().Equal(new Dictionary<string, string>
        {
            ["Activities re-read"] = "6",
            ["Curriculum items credited"] = "7",
            ["Semester tallies written"] = "4",
            ["Stale tallies removed"] = "2",
            ["Completions re-stamped"] = "5"
        });
        cut.FindAll(".alert-danger").Should().BeEmpty();

        // The page closes the dialog before it starts the rebuild, so the modal's button cannot fire a second run
        // while the first is going; ConfirmDialog closes it again afterwards, which is harmless.
        JSInterop.VerifyInvoke("wombatDialog.close", calledTimes: 2);
    }

    [Fact]
    public void AFailedRebuild_SaysNothingWasChanged_AndLeavesTheButtonUsable()
    {
        var sender = new FakeSender().On<RebuildCurriculumProgressCommand>(
            _ => throw new InvalidOperationException("Only a global Administrator may rebuild curriculum progress."));
        var cut = RenderRebuildPage(sender);

        PageRebuildButton(cut).Click();
        ConfirmButton(cut).Click();

        sender.Received.OfType<RebuildCurriculumProgressCommand>().Should().ContainSingle();
        Text(cut.Find(".alert.alert-danger")).Should().Be(
            "The rebuild failed and nothing was changed: Only a global Administrator may rebuild curriculum progress.");
        cut.FindAll(".alert-success").Should().BeEmpty();
        cut.FindAll("dl.details-list").Should().BeEmpty();

        var button = PageRebuildButton(cut);
        button.HasAttribute("disabled").Should().BeFalse();
        Text(button).Should().Be("Rebuild progress");
    }

    [Fact]
    public void ASuccessfulRetry_ClearsThePreviousFailure()
    {
        var calls = 0;
        var sender = new FakeSender().On<RebuildCurriculumProgressCommand>(_ =>
            ++calls == 1 ? throw new InvalidOperationException("deadlock detected") : ARebuildResult());
        var cut = RenderRebuildPage(sender);

        PageRebuildButton(cut).Click();
        ConfirmButton(cut).Click();
        cut.FindAll(".alert-danger").Should().ContainSingle();

        PageRebuildButton(cut).Click();
        ConfirmButton(cut).Click();

        sender.Received.Should().HaveCount(2);
        cut.FindAll(".alert-danger").Should().BeEmpty("a stale failure beside a success would contradict it");
        Text(cut.Find(".alert.alert-success")).Should().Be("Curriculum progress was rebuilt.");
    }

    [Fact]
    public async Task WhileTheRebuildRuns_TheButtonSaysSo_AndAPressAsksNothing()
    {
        // A second press while the first rebuild is still replaying would queue a second global rebuild. The button is
        // not disabled (T234): the dialog, closed as the rebuild starts, hands the focus back to it, and a browser drops
        // the focus of a button it disables, to the page. A press while it runs opens no dialog.
        var pending = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var sender = new FakeSender().OnAsync<RebuildCurriculumProgressCommand>(_ => pending.Task);
        var cut = RenderRebuildPage(sender);

        PageRebuildButton(cut).Click();
        var confirming = ConfirmButton(cut).ClickAsync(new MouseEventArgs());

        cut.WaitForAssertion(() => Text(PageRebuildButton(cut)).Should().Be("Rebuilding…"));
        PageRebuildButton(cut).HasAttribute("disabled").Should().BeFalse("it has the focus (T234)");

        PageRebuildButton(cut).Click();
        JSInterop.VerifyInvoke("wombatDialog.showModal", calledTimes: 1);

        pending.SetResult(ARebuildResult());
        await confirming;

        cut.WaitForAssertion(() => Text(PageRebuildButton(cut)).Should().Be("Rebuild progress"), AsyncWorkTimeout);
        sender.Received.Should().ContainSingle();
    }

    // ---------------------------------------------------------------------------------------------
    // Fixtures: what the reader builds on 23 September 2026 for each kind of starter
    // ---------------------------------------------------------------------------------------------

    /// <summary>Started 1 January 2025, on a boundary: every window counts, and the previous ones did too.</summary>
    private static TraineeCurriculumProgressSummaryDto BoundaryStarter()
        => Summary(
            AsOf,
            programmeStart: new(2025, 1, 1),
            stage: 2,
            [
                Item(1, "PAED-001", "Providing paediatric emergency care to children", QuotaPeriod.Semester, 3,
                    Counting(Semester2Of2026, count: 1, target: 3, minimumReached: 1, lastObservedOn: new(2026, 8, 12)),
                    previous: Counting(Semester1Of2026, count: 2, target: 3, minimumReached: 2, lastObservedOn: new(2026, 6, 30))),
                Item(6, "PAED-006", "Managing long-term health conditions (LTHCs)", QuotaPeriod.Semester, 2,
                    Counting(Semester2Of2026, count: 2, target: 2, minimumReached: 2, lastObservedOn: new(2026, 9, 1)),
                    previous: Counting(Semester1Of2026, count: 2, target: 2, minimumReached: 1, lastObservedOn: new(2026, 5, 4))),
                Item(11, "PAED-011", "Managing population health challenges", QuotaPeriod.AcademicYear, 1,
                    Counting(Year2026, count: 0, target: 1),
                    previous: Counting(Year2025, count: 1, target: 1, minimumReached: 1, lastObservedOn: new(2025, 10, 2)))
            ]);

    /// <summary>
    /// Started 20 September 2026 (the dev trainee's own admission date): after semester 2's first month and in
    /// semester 2, so exempt from both kinds until 1 January 2027. The windows before the start are not passed.
    /// </summary>
    private static TraineeCurriculumProgressSummaryDto LateStarter()
        => Summary(
            AsOf,
            programmeStart: new(2026, 9, 20),
            stage: 1,
            [
                Item(1, "PAED-001", "Providing paediatric emergency care to children", QuotaPeriod.Semester, 3,
                    Exempt(Semester2Of2026, count: 1, target: 3, "semester 1, 2027", new(2027, 1, 1))),
                Item(11, "PAED-011", "Managing population health challenges", QuotaPeriod.AcademicYear, 1,
                    Exempt(Year2026, count: 0, target: 1, "the 2027 academic year", new(2027, 1, 1)))
            ],
            semesterStart: new QuotaStartDto("semester 1, 2027", new(2027, 1, 1)),
            yearStart: new QuotaStartDto("the 2027 academic year", new(2027, 1, 1)));

    /// <summary>
    /// Started 15 July 2026: inside semester 2's first month, so held to it (D42), but a semester-2 start, so
    /// exempt from the 2026 academic year.
    /// </summary>
    private static TraineeCurriculumProgressSummaryDto MidJulyStarter()
        => Summary(
            AsOf,
            programmeStart: new(2026, 7, 15),
            stage: 1,
            [
                Item(1, "PAED-001", "Providing paediatric emergency care to children", QuotaPeriod.Semester, 3,
                    Counting(Semester2Of2026, count: 1, target: 3, minimumReached: 0, lastObservedOn: new(2026, 9, 2))),
                Item(11, "PAED-011", "Managing population health challenges", QuotaPeriod.AcademicYear, 1,
                    Exempt(Year2026, count: 0, target: 1, "the 2027 academic year", new(2027, 1, 1)))
            ],
            yearStart: new QuotaStartDto("the 2027 academic year", new(2027, 1, 1)));

    /// <summary>The same mid-July starter with a fuller catalogue, for the dashboard's top-five cut.</summary>
    private static TraineeCurriculumProgressSummaryDto MidJulyStarterWithManyItems()
        => Summary(
            AsOf,
            programmeStart: new(2026, 7, 15),
            stage: 1,
            [
                SemesterItem(1, "PAED-001", "Providing paediatric emergency care to children", target: 3, count: 0),
                SemesterItem(2, "PAED-002", "Managing common paediatric presentations", target: 3, count: 1),
                SemesterItem(3, "PAED-003", "Providing intensive care to children", target: 3, count: 0),
                SemesterItem(4, "PAED-004", "Managing common neonatal conditions", target: 3, count: 2),
                SemesterItem(5, "PAED-005", "Providing neonatal care in intensive and high-care settings", target: 3, count: 1),
                SemesterItem(6, "PAED-006", "Managing long-term health conditions (LTHCs)", target: 2, count: 2),
                SemesterItem(7, "PAED-007", "Maintaining and promoting the health and well-being of children", target: 1, count: 0),
                Item(8, "PAED-008", "Evaluating and managing neurodevelopmental and behavioural presentations in children", QuotaPeriod.AcademicYear, 1,
                    Exempt(Year2026, count: 0, target: 1, "the 2027 academic year", new(2027, 1, 1))),
                SemesterItem(10, "PAED-010", "Leading and operating within a clinical team", target: 3, count: 4),
                Item(11, "PAED-011", "Managing population health challenges", QuotaPeriod.AcademicYear, 1,
                    Exempt(Year2026, count: 1, target: 1, "the 2027 academic year", new(2027, 1, 1)))
            ],
            yearStart: new QuotaStartDto("the 2027 academic year", new(2027, 1, 1)));

    /// <summary>
    /// Started 1 April 2026: after semester 1's first month, so exempt from semester 1, but a semester-1 start,
    /// so held to the 2026 academic year and to semester 2.
    /// </summary>
    private static TraineeCurriculumProgressSummaryDto AprilStarter()
        => Summary(
            AsOf,
            programmeStart: new(2026, 4, 1),
            stage: 1,
            [
                Item(1, "PAED-001", "Providing paediatric emergency care to children", QuotaPeriod.Semester, 3,
                    Counting(Semester2Of2026, count: 0, target: 3),
                    previous: Exempt(Semester1Of2026, count: 1, target: 3, "semester 2, 2026", new(2026, 7, 1))),
                Item(11, "PAED-011", "Managing population health challenges", QuotaPeriod.AcademicYear, 1,
                    Counting(Year2026, count: 1, target: 1, minimumReached: 1, lastObservedOn: new(2026, 5, 20)))
            ]);

    /// <summary>Starts 1 January 2027: on a boundary, but not yet.</summary>
    private static TraineeCurriculumProgressSummaryDto NotYetStarted()
        => Summary(
            AsOf,
            programmeStart: new(2027, 1, 1),
            stage: null,
            [
                Item(1, "PAED-001", "Providing paediatric emergency care to children", QuotaPeriod.Semester, 3,
                    NotStarted(Semester2Of2026, target: 3, "semester 1, 2027", new(2027, 1, 1))),
                Item(11, "PAED-011", "Managing population health challenges", QuotaPeriod.AcademicYear, 1,
                    NotStarted(Year2026, target: 1, "the 2027 academic year", new(2027, 1, 1)))
            ],
            semesterStart: new QuotaStartDto("semester 1, 2027", new(2027, 1, 1)),
            yearStart: new QuotaStartDto("the 2027 academic year", new(2027, 1, 1)));

    /// <summary>A boundary starter reading the page on 10 December 2026, after the College's year has ended.</summary>
    private static TraineeCurriculumProgressSummaryDto DecemberReader()
        => Summary(
            new DateOnly(2026, 12, 10),
            programmeStart: new(2025, 1, 1),
            stage: 2,
            [
                Item(1, "PAED-001", "Providing paediatric emergency care to children", QuotaPeriod.Semester, 3,
                    Counting(Semester2Of2026, count: 3, target: 3, minimumReached: 3, lastObservedOn: new(2026, 12, 4)),
                    previous: Counting(Semester1Of2026, count: 3, target: 3, minimumReached: 2, lastObservedOn: new(2026, 6, 12))),
                Item(11, "PAED-011", "Managing population health challenges", QuotaPeriod.AcademicYear, 1,
                    Counting(Year2026, count: 1, target: 1, minimumReached: 1, lastObservedOn: new(2026, 12, 8)),
                    previous: Counting(Year2025, count: 1, target: 1, minimumReached: 1, lastObservedOn: new(2025, 10, 2)))
            ]);

    private static TraineeCurriculumProgressDto SemesterItem(int id, string code, string title, int target, int count)
        => Item(id, code, title, QuotaPeriod.Semester, target,
            Counting(Semester2Of2026, count, target, minimumReached: count, lastObservedOn: count > 0 ? new(2026, 9, 10) : null));

    private static TraineeCurriculumProgressDto Item(
        int id,
        string code,
        string title,
        QuotaPeriod period,
        int target,
        QuotaWindowDto current,
        QuotaWindowDto? previous = null)
        => new(id, EpaId: id, code, title, period, target, current, previous, EffectiveMinimumLevelOrder: 3, EffectiveMinimumLevelLabel: "3a", TrainingYearChangedOn: null);

    private static QuotaWindowDto Counting(
        WindowShape window, int count, int target, int minimumReached = 0, DateOnly? lastObservedOn = null, bool lastObservedOnDeclared = true)
        => new(
            window.Name,
            window.Months,
            window.Start,
            window.NominalEnd,
            QuotaWindowStatus.Counting,
            count,
            target,
            IsMet: count >= target,
            Shortfall: Math.Max(0, target - count),
            PercentOfTarget: Math.Min(100, count * 100 / target),
            minimumReached,
            lastObservedOn,
            LastObservedOnDeclared: lastObservedOn is not null && lastObservedOnDeclared,
            FirstCountedName: null,
            FirstCountedOn: null);

    private static QuotaWindowDto Exempt(WindowShape window, int count, int target, string firstCountedName, DateOnly firstCountedOn)
        => new(
            window.Name,
            window.Months,
            window.Start,
            window.NominalEnd,
            QuotaWindowStatus.ExemptPartialPeriod,
            count,
            target,
            IsMet: false,
            Shortfall: 0,
            PercentOfTarget: 0,
            MinimumLevelReachedCount: count,
            LastObservedOn: count > 0 ? window.Start.AddDays(10) : null,
            LastObservedOnDeclared: count > 0,
            firstCountedName,
            firstCountedOn);

    /// <summary>A window D49 holds to no target: the programme ended in it before its last month, or before it began.</summary>
    private static QuotaWindowDto NoTargetAfterTheEnd(WindowShape window, QuotaWindowStatus status, int count, int target)
        => new(
            window.Name,
            window.Months,
            window.Start,
            window.NominalEnd,
            status,
            count,
            target,
            IsMet: false,
            Shortfall: 0,
            PercentOfTarget: 0,
            MinimumLevelReachedCount: count,
            LastObservedOn: count > 0 ? window.Start.AddDays(10) : null,
            LastObservedOnDeclared: count > 0,
            FirstCountedName: null,
            FirstCountedOn: null);

    private static QuotaWindowDto NotStarted(WindowShape window, int target, string firstCountedName, DateOnly firstCountedOn)
        => new(
            window.Name,
            window.Months,
            window.Start,
            window.NominalEnd,
            QuotaWindowStatus.NotStarted,
            Count: 0,
            target,
            IsMet: false,
            Shortfall: 0,
            PercentOfTarget: 0,
            MinimumLevelReachedCount: 0,
            LastObservedOn: null,
            LastObservedOnDeclared: false,
            firstCountedName,
            firstCountedOn);

    /// <summary>The page-level figures, derived from the items the way the reader derives them.</summary>
    private static TraineeCurriculumProgressSummaryDto Summary(
        DateOnly asOf,
        DateOnly programmeStart,
        int? stage,
        IReadOnlyList<TraineeCurriculumProgressDto> items,
        QuotaStartDto? semesterStart = null,
        QuotaStartDto? yearStart = null)
    {
        var semesterItems = items.Where(item => item.IsPerSemester).ToList();
        var yearItems = items.Where(item => !item.IsPerSemester).ToList();
        var semester2NominalEnd = new DateOnly(asOf.Year, 11, 30);

        return new TraineeCurriculumProgressSummaryDto(
            asOf,
            programmeStart,
            stage,
            CurrentSemesterName: "Semester 2, 2026",
            CurrentSemesterMonths: "July to November",
            CurrentSemesterNominalEnd: semester2NominalEnd,
            IsAfterTeachingYear: asOf > semester2NominalEnd,
            SemesterTargetsMet: semesterItems.Count(item => item.Current.IsMet),
            SemesterTargetsApplying: semesterItems.Count(item => item.Current.Applies),
            YearTargetsMet: yearItems.Count(item => item.Current.IsMet),
            YearTargetsApplying: yearItems.Count(item => item.Current.Applies),
            HasSemesterItems: semesterItems.Count > 0,
            HasYearItems: yearItems.Count > 0,
            ProgrammeNotStarted: asOf < programmeStart,
            SemesterTargetsStart: semesterItems.Count > 0 ? semesterStart : null,
            YearTargetsStart: yearItems.Count > 0 ? yearStart : null,
            Items: items);
    }

    private static CurriculumCoverage Coverage(int exemptTrainees, params EpaTargetCoverage[] epas)
        => new(AsOf, "Semester 2, 2026", "July to November", [], epas, exemptTrainees);

    private static RebuildCurriculumProgressResult ARebuildResult()
        => new(ActivitiesReplayed: 6, CreditApplications: 7, ProgressRowsWritten: 4, ProgressRowsRemoved: 2, TransitionsStamped: 5);

    private sealed record WindowShape(string Name, string Months, DateOnly Start, DateOnly NominalEnd);

    // ---------------------------------------------------------------------------------------------
    // Rendering helpers
    // ---------------------------------------------------------------------------------------------

    private IRenderedComponent<MyProgress> RenderMyProgress(TraineeCurriculumProgressSummaryDto? summary)
        => RenderMyProgressWith(new FakeSender()
            .On<GetCurriculumProgressForTraineeQuery>(_ => summary)
            .On<GetEpaTrajectoryForTraineeQuery>(_ => Array.Empty<EpaTrajectoryDto>())
            .On<GetEntrustmentStandingForTraineeQuery>(_ => null)
            .On<GetMsfCoverageForTraineeQuery>(_ => null));

    private IRenderedComponent<MyProgress> RenderMyProgressWith(FakeSender sender)
    {
        PinCulture();
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<MyProgress>();
        cut.WaitForState(() => cut.Markup.Contains("Curriculum targets"));

        return cut;
    }

    private IRenderedComponent<TraineeDashboard> RenderTraineeDashboard(TraineeCurriculumProgressSummaryDto? targets)
    {
        PinCulture();
        Services.AddSingleton<IScopedSender>(new FakeSender()
            .On<GetTraineeDashboardSummaryQuery>(_ => new TraineeDashboardSummaryDto(targets, [], [], [], IsPendingTrainee: false)));

        var cut = RenderComponent<TraineeDashboard>();
        cut.WaitForState(() => cut.Markup.Contains("Curriculum targets"));

        return cut;
    }

    private IRenderedComponent<EpaTargetCoverageList> RenderCoverage(CurriculumCoverage coverage)
    {
        PinCulture();
        return RenderComponent<EpaTargetCoverageList>(parameters => parameters.Add(list => list.Coverage, coverage));
    }

    private IRenderedComponent<CurriculumProgressRebuild> RenderRebuildPage(FakeSender sender)
    {
        PinCulture();

        // The rebuild page is Administrator-only; the handler checks the caller it is handed.
        _auth.SetAuthorized("admin@test");
        _auth.SetRoles(WombatRoles.Administrator);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));

        // ConfirmDialog opens and closes a native <dialog> through JS. Set both calls up explicitly (bUnit's
        // strict mode would throw on anything else) so the tests can also verify they were made.
        JSInterop.SetupVoid("wombatDialog.showModal", _ => true).SetVoidResult();
        JSInterop.SetupVoid("wombatDialog.close", _ => true).SetVoidResult();

        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<CurriculumProgressRebuild>();
        cut.WaitForState(() => cut.Markup.Contains("Curriculum progress"));

        return cut;
    }

    private static IElement PageRebuildButton(IRenderedFragment cut)
        => cut.FindAll(".form-container button").Single(button => button.Closest("dialog") is null);

    private static IElement ConfirmButton(IRenderedFragment cut)
        => cut.FindAll("dialog button").Single(button => Text(button) == "Rebuild progress");

    private static IElement ItemCard(IRenderedFragment cut, string epaCode)
        => cut.FindAll(".dashboard-grid > section.detail-card")
            .Single(card => Text(card.QuerySelector(".progress-row-head h3")!) == epaCode);

    private static IReadOnlyList<string> CardCodesUnder(IElement groupHeading)
        => groupHeading.NextElementSibling!
            .QuerySelectorAll("section.detail-card .progress-row-head h3")
            .Select(Text)
            .ToList();

    // The other dashboard cards nest a footer <a> inside the card's own <a>, which the HTML parser answers by
    // cloning the outer anchor, so some ".detail-card" elements carry no heading.
    private static IElement CurriculumTargetsCard(IRenderedFragment cut)
        => cut.FindAll(".detail-card").Single(card => card.QuerySelector("h3") is { } heading && Text(heading) == "Curriculum targets");

    private static IReadOnlyList<(string Value, string Label)> Metrics(IElement card)
        => card.QuerySelectorAll(".dashboard-metric")
            .Select(metric => (
                Text(metric.QuerySelector(".dashboard-metric-value")!),
                Text(metric.QuerySelector(".dashboard-metric-label")!)))
            .ToList();

    private static Dictionary<string, string> Details(IElement detailsList)
        => detailsList.Children
            .Where(child => child.LocalName == "div")
            .ToDictionary(
                row => Text(row.QuerySelector("dt")!),
                row => Text(row.QuerySelector("dd")!));

    /// <summary>An element's text with Razor's source indentation collapsed to single spaces.</summary>
    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private static string PageText(IRenderedFragment cut)
        => Regex.Replace(string.Join(" ", cut.Nodes.Select(node => node.TextContent)), @"\s+", " ").Trim();

    /// <summary>
    /// The page formats dates with the current culture. Pin one whose month names are English, so "30 November
    /// 2026" does not depend on the machine the suite runs on.
    /// </summary>
    private static void PinCulture()
    {
        var southAfrican = CultureInfo.GetCultureInfo("en-ZA");
        CultureInfo.CurrentCulture = southAfrican;
        CultureInfo.CurrentUICulture = southAfrican;
    }

    protected override void Dispose(bool disposing)
    {
        CultureInfo.CurrentCulture = _originalCulture;
        CultureInfo.CurrentUICulture = _originalUiCulture;
        base.Dispose(disposing);
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
