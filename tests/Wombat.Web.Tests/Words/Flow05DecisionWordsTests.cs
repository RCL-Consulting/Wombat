using FluentAssertions;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.Curricula;
using Wombat.Web.Components.Pages.Dashboards;
using Wombat.Web.Components.Shared;
using Wombat.Web.Components.Shared.Progress;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Words;

/// <summary>
/// A decision's line and the completed card's count (T355, E5, E6; note 2; Spec § 1, "A decision's line"): one rule for
/// both, the activity's own window, named when it is not the current one.
/// </summary>
public sealed class CountLineWordsTests
{
    [Fact]
    public void TheCurrentWindow_ReadsThisSemester_AndSaysMetWhenItIs()
    {
        CountLineWords.Line(Line("PAED-001", 1, 3, current: true)).Should().Be("PAED-001: 1 of 3 this semester.");
        CountLineWords.Line(Line("PAED-001", 3, 3, current: true)).Should().Be("PAED-001: 3 of 3 this semester, met.");
    }

    [Fact]
    public void AnOlderWindow_IsNamed_MetOrShort()
    {
        CountLineWords.Line(Line("PAED-001", 3, 3, current: false, name: "Semester 1, 2026"))
            .Should().Be("PAED-001, Semester 1, 2026: 3 of 3, met.");
        CountLineWords.Line(Line("PAED-001", 1, 3, current: false, name: "Semester 1, 2026"))
            .Should().Be("PAED-001, Semester 1, 2026: 1 of 3, 2 short.");
    }

    [Fact]
    public void AYearlyItem_ReadsInItsYear()
    {
        CountLineWords.Line(Line("PAED-008", 1, 1, current: true, period: QuotaPeriod.AcademicYear, name: "2026 academic year"))
            .Should().Be("PAED-008: 1 of 1 in 2026, met.");
        CountLineWords.Line(Line("PAED-008", 0, 1, current: false, period: QuotaPeriod.AcademicYear, name: "2025 academic year", year: 2025))
            .Should().Be("PAED-008, 2025 academic year: 0 of 1, 1 short.");
    }

    [Fact]
    public void AWindowWithNoTarget_ShowsItsCount_AndNoFraction()
    {
        // R3: a waived window shows its count, never a fraction.
        CountLineWords.Line(Line("PAED-002", 2, 3, current: true, status: QuotaWindowStatus.ExemptPartialPeriod))
            .Should().Be("PAED-002: no target this semester · 2 recorded.");
        CountLineWords.Line(Line("PAED-008", 0, 1, current: true, period: QuotaPeriod.AcademicYear, name: "2026 academic year",
                status: QuotaWindowStatus.ExemptPartialPeriod))
            .Should().Be("PAED-008: no target in 2026 · 0 recorded.");
        CountLineWords.Line(Line("PAED-002", 2, 3, current: false, name: "Semester 1, 2026", status: QuotaWindowStatus.ExemptPartialPeriod))
            .Should().Be("PAED-002, Semester 1, 2026: no target · 2 recorded.");
    }

    [Fact]
    public void ADecisionsLine_ByKind()
    {
        var completed = ActivityRows.Decided(1, creditedItemCount: 1) with { CountLine = Line("PAED-001", 3, 3, current: true) };
        CountLineWords.ForDecision(completed).Should().Be("PAED-001: 3 of 3 this semester, met.");

        // E6: "Credits nothing." wherever it credited nothing: a discussed reflection, or a credit of 0.
        CountLineWords.ForDecision(ActivityRows.Decided(2, state: "discussed", stateLabel: "Discussed", creditedItemCount: null))
            .Should().Be("Credits nothing.");
        CountLineWords.ForDecision(ActivityRows.Decided(3, creditedItemCount: 0)).Should().Be("Credits nothing.");

        // Declined alone has File it again in its place.
        CountLineWords.ForDecision(ActivityRows.Decided(4, state: "declined", stateLabel: "Declined", isFinished: false, creditedItemCount: null))
            .Should().BeNull();

        // No EPA, no line.
        CountLineWords.ForDecision(ActivityRows.Decided(5) with { EpaId = null, EpaCode = null, EpaInForce = null }).Should().BeNull();
    }

    [Fact]
    public void ThePausedSentence_IsKeyedOnThePauseAndTheCreditTogether()
    {
        // Note 2: a completion credited nothing because its EPA is paused waits; one credited before the pause does not.
        var waits = ActivityRows.Decided(1, creditedItemCount: 0) with { EpaCode = "PAED-012", EpaInForce = false };
        CountLineWords.ForDecision(waits).Should().Be("Its credit to PAED-012 waits while the EPA is paused.");

        var creditedBefore = ActivityRows.Decided(2, creditedItemCount: 1) with { EpaCode = "PAED-012", EpaInForce = false };
        CountLineWords.ForDecision(creditedBefore).Should().BeNull("its count is not read while the EPA is paused, and it does not wait");
    }

    [Fact]
    public void ThePausedSentence_IsOnlyForATypeThatCanCredit()
    {
        // T355, build review R4: a reflection stamped with a paused EPA credits nothing by design; it waits for nothing.
        var reflection = ActivityRows.Decided(1, state: "discussed", stateLabel: "Discussed", creditedItemCount: null)
            with { EpaCode = "PAED-012", EpaInForce = false, CanCredit = false };
        CountLineWords.ForDecision(reflection).Should().Be("Credits nothing.");
    }

    [Fact]
    public void ADecline_IsTheWorkflowsFact_NotAStateKey()
    {
        // T355, build review R4 (E6: the rule, not a list): a builder's type whose decline state is named otherwise is still
        // a decline, with File it again in place of a line; a state merely called "declined" that is no decline gets its line.
        CountLineWords.ForDecision(ActivityRows.Decided(1, state: "refused", stateLabel: "Refused", isFinished: false, creditedItemCount: null)
                with { Declined = true })
            .Should().BeNull();
        CountLineWords.ForDecision(ActivityRows.Decided(2, state: "declined", stateLabel: "Declined", isFinished: false, creditedItemCount: null)
                with { Declined = false })
            .Should().Be("Credits nothing.");
    }

    [Fact]
    public void TheCardsCount_IsTheLine_OrNothing()
    {
        CountLineWords.ForCard(Line("PAED-001", 1, 3, current: true)).Should().Be("PAED-001: 1 of 3 this semester.");
        CountLineWords.ForCard(Line("PAED-001", 3, 3, current: false, name: "Semester 1, 2026"))
            .Should().Be("PAED-001, Semester 1, 2026: 3 of 3, met.");
        CountLineWords.ForCard(null).Should().BeNull();
    }

    internal static EpaCountLineDto Line(
        string code,
        int count,
        int target,
        bool current,
        QuotaPeriod period = QuotaPeriod.Semester,
        string name = "Semester 2, 2026",
        QuotaWindowStatus status = QuotaWindowStatus.Counting,
        int year = 2026)
    {
        var applies = status == QuotaWindowStatus.Counting;
        var start = period == QuotaPeriod.Semester && name.StartsWith("Semester 2", StringComparison.Ordinal)
            ? new DateOnly(year, 7, 1)
            : new DateOnly(year, 1, 1);
        var window = new QuotaWindowDto(
            name, "July to November", start, new DateOnly(year, 11, 30), status, count, target,
            IsMet: applies && count >= target, Shortfall: applies ? Math.Max(0, target - count) : 0,
            PercentOfTarget: 0, MinimumLevelReachedCount: count, LastObservedOn: null, LastObservedOnDeclared: false,
            FirstCountedName: null, FirstCountedOn: null);
        return new EpaCountLineDto(5000, code, period, window, current);
    }
}

/// <summary>The Trainee's Home in words (T355, R1; Spec § 1, Home's rows).</summary>
public sealed class TraineeHomeWordsTests
{
    [Fact]
    public void TheBuiltEmpties_AreKeptWordForWord()
    {
        TraineeHomeWords.NoCurriculum.Should().Be(
            "No curriculum assigned yet. Once you are admitted, your targets for each period appear here.");
        TraineeHomeWords.NoEpaInUse.Should().Be(
            "No EPA on your curriculum is in use at the moment, so no target applies to you.");
        TraineeHomeWords.NoDecisions.Should().Be("No decisions yet.");
    }

    [Fact]
    public void AllMet_NamesTheWindowsThatApply_AndOneKindOnItsOwn()
    {
        TraineeHomeWords.AllMet(Summary(semester: (10, 10), year: (5, 5)))
            .Should().Be("Every target is met for Semester 2, 2026 and the 2026 academic year.");
        // Note 15: a curriculum with one kind of item.
        TraineeHomeWords.AllMet(Summary(semester: (10, 10), year: (0, 0), yearly: false))
            .Should().Be("Every target is met for Semester 2, 2026.");
        TraineeHomeWords.AllMet(Summary(semester: (0, 0), year: (5, 5), semesterly: false))
            .Should().Be("Every target is met for the 2026 academic year.");
    }

    [Fact]
    public void AllMet_IsSilent_UntilEveryTargetThatAppliesIsMet()
    {
        TraineeHomeWords.AllMet(Summary(semester: (9, 10), year: (5, 5))).Should().BeNull();
        TraineeHomeWords.AllMet(Summary(semester: (0, 0), year: (0, 0))).Should().BeNull("no target applies");
    }

    [Fact]
    public void PartWay_AndNotStarted()
    {
        TraineeHomeWords.PartWay(Summary(semester: (0, 0), year: (0, 5)) with
            {
                SemesterTargetsStart = new QuotaStartDto("semester 1, 2027", new DateOnly(2027, 1, 1))
            })
            .Should().Be("No semester target this semester: you started part-way through it. Semester targets begin with Semester 1, 2027.");
        TraineeHomeWords.PartWay(Summary(semester: (1, 10), year: (0, 5))).Should().BeNull();

        var notStarted = Summary(semester: (0, 0), year: (0, 0)) with
        {
            ProgrammeNotStarted = true,
            ProgrammeStartDate = new DateOnly(2027, 1, 15),
            SemesterTargetsStart = new QuotaStartDto("semester 1, 2027", new DateOnly(2027, 1, 15))
        };
        TraineeHomeWords.NotStarted(notStarted).Should().Be("Your programme starts on 2027-01-15.");
        TraineeHomeWords.PartWay(notStarted).Should().BeNull("not started says it");
        TraineeHomeWords.NotStarted(Summary(semester: (1, 10), year: (0, 5))).Should().BeNull();
    }

    [Fact]
    public void Ended_TakesTheProgrammesEndInItsSharedWords()
    {
        // A.4.6: "Your programme ended on 2026-10-01, so no target applies …". The date is QuotaText.ProgrammeEnded's,
        // ISO (D1); the sentence around it is Home's.
        var ended = new ProgrammeEndDto(Completed: false, EndedOn: new DateOnly(2026, 10, 1), Today: new DateOnly(2026, 10, 3));

        TraineeHomeWords.Ended(ended).Should().Be(
            $"{QuotaText.ProgrammeEnded(ended)}, so no target applies to you any more. " +
            "Your progress in each period is kept on My progress, read-only.");
        TraineeHomeWords.Ended(ended).Should().StartWith("Your programme ended on 2026-10-01");
    }

    [Fact]
    public void NoStar_NamesTheTrainingYear_UntilTheProgrammeHasEnded()
    {
        TraineeHomeWords.NoStar(3, ended: false)
            .Should().Be("No STAR yet. When the committee issues one, it shows here against training year 3's level.");
        TraineeHomeWords.NoStar(3, ended: true).Should().Be("No STAR yet.");
        TraineeHomeWords.NoStar(null, ended: false).Should().Be("No STAR yet.");
    }

    [Fact]
    public void AStarRow_SaysTheLevelAgainstTheYear_AndItsExpiry()
    {
        TraineeHomeWords.StarRow(Star("PAED-010", "4", EntrustmentStandingStatus.Below, expires: new DateOnly(2026, 10, 23)), 4)
            .Should().Be("4, below training year 4's level of 5 · expires 2026-10-23");
        TraineeHomeWords.StarRow(Star("PAED-001", "5", EntrustmentStandingStatus.AtOrAbove, expires: null), 4)
            .Should().Be("5, at or above training year 4's level of 5");
        TraineeHomeWords.StarRow(Star("KGK-001", "3a", EntrustmentStandingStatus.Below, expires: null, yearIsExit: true, target: "3b"), 4)
            .Should().Be("3a, below its exit level of 3b");
        TraineeHomeWords.StarRow(Star("PAED-002", "Independent", EntrustmentStandingStatus.NotComparable, expires: null, ladder: "O-R Scale"), 4)
            .Should().Be("Independent on O-R Scale");
    }

    [Fact]
    public void TheStarRows_AreThoseBelowTheirLevel_OrExpiringWithin30Days_ByCode()
    {
        var today = new DateOnly(2026, 10, 3);
        var standing = Standing(
            Star("PAED-010", "4", EntrustmentStandingStatus.Below, expires: null),
            Star("PAED-001", "5", EntrustmentStandingStatus.AtOrAbove, expires: new DateOnly(2026, 11, 2)),
            Star("PAED-003", "5", EntrustmentStandingStatus.AtOrAbove, expires: new DateOnly(2026, 11, 3)),
            Star("PAED-004", "5", EntrustmentStandingStatus.AtOrAbove, expires: null),
            NoStar("PAED-005"));

        TraineeHomeWords.StarRows(standing, today).Select(epa => epa.EpaCode).Should().Equal("PAED-001", "PAED-010");
    }

    [Fact]
    public void FileItAgain_NamesItsRow()
    {
        var declined = ActivityRows.Decided(1, state: "declined", stateLabel: "Declined", isFinished: false) with
        {
            DisplayName = "Mini-CEX (Paediatrics) · PAED-002 · 2026-09-13"
        };

        TraineeHomeWords.FileAgainName(declined)
            .Should().Be("File it again, to someone else: Mini-CEX (Paediatrics) · PAED-002 · 2026-09-13");
    }

    internal static EpaStandingDto Star(
        string code, string level, EntrustmentStandingStatus status, DateOnly? expires,
        bool yearIsExit = false, string target = "5", string? ladder = null)
        => new(
            1, 1, code, $"{code} title", "CPSA Paediatric Entrustment Scale v11.1", IsLocal: yearIsExit,
            YearTargetOrder: 6, YearTargetLabel: target, YearTargetIsExitLevel: yearIsExit, ExitLevelOrder: 6, ExitLevelLabel: "5",
            new StandingDecisionDto(1, 5, level, ladder, new DateOnly(2026, 1, 15), expires),
            status, status, LatestRating: null);

    internal static EpaStandingDto NoStar(string code)
        => new(
            1, 1, code, $"{code} title", "CPSA Paediatric Entrustment Scale v11.1", IsLocal: false,
            YearTargetOrder: 6, YearTargetLabel: "5", YearTargetIsExitLevel: false, ExitLevelOrder: 6, ExitLevelLabel: "5",
            Decision: null, EntrustmentStandingStatus.NoDecision, EntrustmentStandingStatus.NoDecision, LatestRating: null);

    internal static EntrustmentStandingDto Standing(params EpaStandingDto[] epas)
        => new(new DateOnly(2026, 10, 3), new DateOnly(2023, 1, 15), 4, false, epas, new ExitRuleReadinessDto(0, 0, [], []));

    private static TraineeCurriculumProgressSummaryDto Summary(
        (int Met, int Applying) semester, (int Met, int Applying) year, bool semesterly = true, bool yearly = true)
    {
        var window = new QuotaWindowDto(
            "2026 academic year", "January to November", new DateOnly(2026, 1, 1), new DateOnly(2026, 11, 30),
            QuotaWindowStatus.Counting, 1, 1, true, 0, 100, 1, null, false, null, null);
        var items = new List<TraineeCurriculumProgressDto>();
        if (semesterly)
        {
            items.Add(new TraineeCurriculumProgressDto(1, 1, "PAED-001", "t", QuotaPeriod.Semester, 3,
                window with { Name = "Semester 2, 2026" }, null, 6, "5", null));
        }

        if (yearly)
        {
            items.Add(new TraineeCurriculumProgressDto(2, 2, "PAED-008", "t", QuotaPeriod.AcademicYear, 1, window, null, 5, "4", null));
        }

        return new TraineeCurriculumProgressSummaryDto(
            AsOf: new DateOnly(2026, 10, 3), ProgrammeStartDate: new DateOnly(2023, 1, 15), TraineeStage: 4,
            CurrentSemesterName: "Semester 2, 2026", CurrentSemesterMonths: "July to November",
            CurrentSemesterNominalEnd: new DateOnly(2026, 11, 30), IsAfterTeachingYear: false,
            SemesterTargetsMet: semester.Met, SemesterTargetsApplying: semester.Applying,
            YearTargetsMet: year.Met, YearTargetsApplying: year.Applying,
            HasSemesterItems: semesterly, HasYearItems: yearly, ProgrammeNotStarted: false,
            SemesterTargetsStart: null, YearTargetsStart: null, Items: items);
    }
}

/// <summary>The standing's summary line, said once for Home and the panel (T355, R1).</summary>
public sealed class StandingWordsTests
{
    [Fact]
    public void TheYearLine_IsThePanelsWordForWord()
    {
        var standing = TraineeHomeWordsTests.Standing(
            TraineeHomeWordsTests.Star("PAED-001", "5", EntrustmentStandingStatus.AtOrAbove, null),
            TraineeHomeWordsTests.Star("PAED-002", "5", EntrustmentStandingStatus.AtOrAbove, null),
            TraineeHomeWordsTests.Star("PAED-010", "4", EntrustmentStandingStatus.Below, null),
            TraineeHomeWordsTests.NoStar("PAED-003"));

        StandingWords.YearLine(standing).Should().Be("2 at or above · 1 below · 1 with no decision, of 4 EPAs");
    }

    [Fact]
    public void ANotComparableStar_IsCounted_AsThePanelCountsIt()
    {
        var standing = TraineeHomeWordsTests.Standing(
            TraineeHomeWordsTests.Star("PAED-001", "Independent", EntrustmentStandingStatus.NotComparable, null, ladder: "O-R Scale"),
            TraineeHomeWordsTests.NoStar("PAED-003"));

        StandingWords.YearLine(standing).Should().Be("0 at or above · 0 below · 1 with no decision · 1 not comparable, of 2 EPAs");
    }
}
