using FluentAssertions;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Curricula;
using Wombat.Web.Components.Shared.Progress;
using static Wombat.Web.Tests.Progress.ProgressFixtures;

namespace Wombat.Web.Tests.Progress;

/// <summary>
/// T355: the words of a registrar's progress, every phrase Spec § 1 gives Home and My progress for a count. The built
/// sentences the boards keep are asserted byte-equal to the words <c>MyProgress.razor</c> renders today (its
/// <c>PreviousLine</c>, <c>EndedPeriodLine</c>, <c>TargetsLine</c> and its cards' lines, as
/// <c>QuotaProgressRenderingTests</c> pins them), with every date ISO (decision D1).
/// </summary>
public sealed class ProgressWordsTests
{
    private static readonly TraineeCurriculumProgressDto Short = Item(
        "PAED-001", QuotaPeriod.Semester, 3, Counting(Semester2Of2026, 1, 3, minimumReached: 1, lastObservedOn: new(2026, 9, 23)));

    private static readonly TraineeCurriculumProgressDto MetSemester = Item(
        "PAED-001", QuotaPeriod.Semester, 3, Counting(Semester2Of2026, 3, 3, minimumReached: 2));

    private static readonly TraineeCurriculumProgressDto ShortYear = Item(
        "PAED-008", QuotaPeriod.AcademicYear, 1, Counting(Year2026, 0, 1));

    private static readonly TraineeCurriculumProgressDto MetYear = Item(
        "PAED-008", QuotaPeriod.AcademicYear, 1, Counting(Year2026, 1, 1));

    [Fact]
    public void AFigure_IsACountAgainstATargetForANamedWindow()
    {
        ProgressWords.Figure(Short).Should().Be("1 of 3 this semester");
        ProgressWords.Figure(ShortYear).Should().Be("0 of 1 in 2026");
        ProgressWords.WindowWords(Short).Should().Be("this semester");
        ProgressWords.WindowWords(ShortYear).Should().Be("in 2026");
    }

    [Fact]
    public void ACountCell_SaysWhatIsLeft_ByTheCollegesEnd_InIso()
    {
        ProgressWords.CountMeta(Short, D).Should().Be("2 more by 2026-11-30");
        ProgressWords.CountMeta(ShortYear, D).Should().Be("1 more by 2026-11-30");
        ProgressWords.HasBar(Short).Should().BeTrue();
    }

    [Fact]
    public void AMetTarget_NamesItsWindow_WithTheBeforeAYearlyOne()
    {
        // The built "Target met for Semester 2, 2026." kept; the yearly one gains "the" (C3, C11).
        ProgressWords.CountMeta(MetSemester, D).Should().Be("Target met for Semester 2, 2026.");
        ProgressWords.CountMeta(MetYear, D).Should().Be("Target met for the 2026 academic year.");
    }

    [Fact]
    public void InDecember_EachRowNamesItsOwnWindow_AndKeepsItsFigure()
    {
        var threeShort = Item("PAED-002", QuotaPeriod.Semester, 3, Counting(Semester2Of2026, 0, 3));

        ProgressWords.CountMeta(threeShort, December).Should().Be(
            "3 more; encounters in December still count towards Semester 2, 2026.");
        ProgressWords.CountMeta(ShortYear, December).Should().Be(
            "1 more; encounters in December still count towards the 2026 academic year.");
        ProgressWords.ShortRow(threeShort, December).Should().Be(
            "0 of 3 this semester · 3 more; encounters in December still count towards Semester 2, 2026.");
    }

    [Fact]
    public void HomesShortRow_PutsTheFigureBeforeWhatIsLeft()
        => ProgressWords.ShortRow(Item("PAED-002", QuotaPeriod.Semester, 3, Counting(Semester2Of2026, 0, 3)), D)
            .Should().Be("0 of 3 this semester · 3 more by 2026-11-30");

    [Fact]
    public void AWaivedCell_IsItsCountAndWhenTargetsStart_WithNoFigureAndNoBar()
    {
        var semester = Item("PAED-002", QuotaPeriod.Semester, 3,
            Waived(Semester2Of2026, QuotaWindowStatus.ExemptPartialPeriod, 2, 3, "semester 1, 2027"));
        var year = Item("PAED-008", QuotaPeriod.AcademicYear, 1,
            Waived(Year2026, QuotaWindowStatus.ExemptPartialPeriod, 0, 1, "the 2027 academic year"));

        ProgressWords.CountMeta(semester, D).Should().Be("No target this semester · 2 recorded · targets start with semester 1, 2027");
        ProgressWords.CountMeta(year, D).Should().Be("No target in 2026 · 0 recorded · targets start with the 2027 academic year");
        ProgressWords.HasBar(semester).Should().BeFalse();
        ProgressWords.ShortRow(semester, D).Should().Be(ProgressWords.CountMeta(semester, D), "a waived window has no fraction");
    }

    [Fact]
    public void ANotStartedCell_SaysWhenTargetsStart()
    {
        var semester = Item("PAED-002", QuotaPeriod.Semester, 3,
            Waived(Semester2Of2026, QuotaWindowStatus.NotStarted, 0, 3, "semester 1, 2027"));
        var year = Item("PAED-008", QuotaPeriod.AcademicYear, 1,
            Waived(Year2026, QuotaWindowStatus.NotStarted, 0, 1, "the 2027 academic year"));

        ProgressWords.CountMeta(semester, D).Should().Be("No target yet · targets start with semester 1, 2027");
        ProgressWords.CountMeta(year, D).Should().Be("No target yet · targets start with the 2027 academic year");
        ProgressWords.HasBar(semester).Should().BeFalse();
    }

    [Fact]
    public void TheEndedForms_AreAsBuilt()
    {
        var endedIn = Item("PAED-001", QuotaPeriod.Semester, 3,
            Waived(Semester2Of2026, QuotaWindowStatus.ExemptProgrammeEnded, 1, 3));
        var after = Item("PAED-001", QuotaPeriod.Semester, 3,
            Waived(Semester2Of2026, QuotaWindowStatus.AfterProgrammeEnd, 0, 3));

        ProgressWords.CountMeta(endedIn, D).Should().Be("No target this semester · 1 recorded · your programme ended part-way through");
        ProgressWords.CountMeta(after, D).Should().Be("No target: this is after your programme ended · 0 recorded");

        var record = Item("PAED-001", QuotaPeriod.Semester, 3, Counting(Semester1Of2026, 3, 3),
            periods: [Counting(Semester1Of2026, 3, 3)]);
        ProgressWords.HasBar(record).Should().BeFalse("an ended programme's record has nothing still to do (R5)");
    }

    [Fact]
    public void TheEpaPagesLines_AtTheMinimum_AndTheLastEncounter()
    {
        ProgressWords.AtMinimum(Counting(Semester2Of2026, 3, 3, minimumReached: 2)).Should().Be("At the minimum level when observed: 2 of 3");
        ProgressWords.AtMinimum(Counting(Semester2Of2026, 0, 3)).Should().BeNull("there is no line where nothing is credited (C4)");

        ProgressWords.LastEncounter(Counting(Semester2Of2026, 1, 3, lastObservedOn: new(2026, 9, 23))).Should().Be("Last encounter 2026-09-23");
        ProgressWords.LastEncounter(Counting(Semester2Of2026, 1, 3, lastObservedOn: new(2026, 9, 23), declared: false))
            .Should().Be("Last encounter not recorded (created 2026-09-23)", "an undated encounter says so (T219)");
        ProgressWords.LastEncounter(Counting(Semester2Of2026, 0, 3)).Should().BeNull();
    }

    [Fact]
    public void ThePreviousWindow_IsAsBuilt()
    {
        ProgressWords.PreviousLine(Counting(Semester1Of2026, 0, 3)).Should().Be("Semester 1, 2026: 0 of 3, 3 short");
        ProgressWords.PreviousLine(Counting(Year2025, 0, 1)).Should().Be("2025 academic year: 0 of 1, 1 short");
        ProgressWords.PreviousLine(Counting(Semester1Of2026, 2, 2)).Should().Be("Semester 1, 2026: 2 of 2, met");
        ProgressWords.PreviousLine(Waived(Semester1Of2026, QuotaWindowStatus.ExemptPartialPeriod, 2, 3, "semester 2, 2026"))
            .Should().Be("Semester 1, 2026: no target (you started part-way through) · 2 recorded");
        ProgressWords.PreviousLine(Waived(Semester1Of2026, QuotaWindowStatus.ExemptProgrammeEnded, 1, 3))
            .Should().Be("Semester 1, 2026: no target (your programme ended part-way through) · 1 recorded");
        ProgressWords.PreviousLine(Waived(Semester1Of2026, QuotaWindowStatus.AfterProgrammeEnd, 0, 3))
            .Should().Be("Semester 1, 2026: no target (after your programme ended) · 0 recorded");
    }

    [Fact]
    public void AnEndedProgrammesPeriod_IsAsBuilt()
    {
        ProgressWords.EndedPeriodLine(Counting(Semester1Of2026, 3, 3, minimumReached: 2), D)
            .Should().Be("3 of 3, met; 2 at the minimum level when observed");
        ProgressWords.EndedPeriodLine(Counting(Semester1Of2026, 1, 3, minimumReached: 1), D)
            .Should().Be("1 of 3, 2 short; 1 at the minimum level when observed");
        ProgressWords.EndedPeriodLine(Counting(Semester2Of2026, 1, 3, minimumReached: 0), D)
            .Should().Be("1 of 3 so far; 0 at the minimum level when observed", "a period not closed by today is a count so far");
        ProgressWords.EndedPeriodLine(Counting(Semester2Of2026, 0, 3), D).Should().Be("0 of 3 so far");
        ProgressWords.EndedPeriodLine(Waived(Semester2Of2026, QuotaWindowStatus.ExemptProgrammeEnded, 1, 3), D)
            .Should().Be("no target (your programme ended part-way through) · 1 recorded");
        ProgressWords.EndedPeriodLine(Waived(Semester2Of2026, QuotaWindowStatus.NotStarted, 0, 3), D)
            .Should().Be("no target (before your programme started)");
        ProgressWords.EndedPeriodLine(Counting(Semester1Of2026, 1, 3), D).Should().NotContain("more by", "R5");
    }

    [Fact]
    public void ThisPeriod_ItsFigures_ItsEnds_AndItsTrainingYear()
    {
        var summary = Summary(D, new(2023, 1, 15), stage: 4, [MetSemester, Short with { CurriculumItemId = 2 }, ShortYear]);

        ProgressWords.SemesterFigure(summary).Should().Be(("1 of 2", "EPAs met this semester"));
        ProgressWords.YearFigure(summary).Should().Be(("0 of 1", "EPAs met in 2026"));
        ProgressWords.EndsLine(summary).Should().Be("Semester 2, 2026 ends on 2026-11-30.");
        ProgressWords.TrainingYearLine(4).Should().Be("Training year 4 — it sets the minimum level each encounter is judged against.");
        ProgressWords.Subtitle(summary).Should().Be("Training year 4 · Semester 2, 2026");
        ProgressWords.TargetsLine(3, 10, "this semester").Should().Be("3 of 10 EPAs met this semester");
    }

    [Fact]
    public void ThisPeriod_WithNoTargetOfAKind_SaysNoneApplyYet()
    {
        var waived = Item("PAED-002", QuotaPeriod.Semester, 3,
            Waived(Semester2Of2026, QuotaWindowStatus.ExemptPartialPeriod, 2, 3, "semester 1, 2027"));
        var summary = Summary(D, new(2026, 8, 18), stage: 1, [waived]);

        ProgressWords.SemesterFigure(summary).Should().Be(("none apply yet", "Semester targets"));
        ProgressWords.YearFigure(summary).Should().Be(("none apply yet", "Yearly targets"));
        ProgressWords.TargetsLine(0, 0, "this semester").Should().Be("none apply yet");
    }

    [Fact]
    public void InDecember_ThisPeriodCountsDecember_AndTheAlertIsTheBuiltOne_InIso()
    {
        var summary = Summary(December, new(2024, 1, 1), stage: 3, [Short]);

        ProgressWords.EndsLine(summary).Should().Be("Semester 2, 2026 counts encounters observed in December.");
        ProgressWords.DecemberNotice(summary).Should().Be(
            "The 2026 academic year ended on 2026-11-30. Encounters observed in December still count towards semester 2, 2026 " +
            "and your 2026 yearly targets. Semester 1, 2027 starts on 2027-01-01.");
        ProgressWords.DecemberNotice(Summary(D, new(2024, 1, 1), stage: 3, [Short])).Should().BeNull();
    }

    [Fact]
    public void ThePartWayAlert_IsTheBuiltOne_InIso()
    {
        var summary = Summary(
            D,
            new(2026, 8, 18),
            stage: 1,
            [
                Item("PAED-002", QuotaPeriod.Semester, 3, Waived(Semester2Of2026, QuotaWindowStatus.ExemptPartialPeriod, 2, 3, "semester 1, 2027")),
                Item("PAED-008", QuotaPeriod.AcademicYear, 1, Waived(Year2026, QuotaWindowStatus.ExemptPartialPeriod, 0, 1, "the 2027 academic year"))
            ],
            semesterStart: new QuotaStartDto("semester 1, 2027", new(2027, 1, 1)),
            yearStart: new QuotaStartDto("the 2027 academic year", new(2027, 1, 1)));

        ProgressWords.StartNotice(summary).Should().Be(
            "You started the programme on 2026-08-18. That was part-way through the semester, so under the College's rule no " +
            "semester target applies until semester 1, 2027, which starts on 2027-01-01. Encounters on those EPAs before then " +
            "stay in your portfolio as evidence, but do not count towards a later semester's target. That was in the second " +
            "half of the academic year, so under the College's rule no yearly target applies until the 2027 academic year, " +
            "which starts on 2027-01-01. Encounters on those EPAs before then stay in your portfolio as evidence, but do not " +
            "count towards a later year's target.");
    }

    [Fact]
    public void APartWayStart_ThatWaivesOneKindOnly_SaysTheOtherAppliesNow()
    {
        var summary = Summary(
            D,
            new(2026, 7, 15),
            stage: 1,
            [
                Item("PAED-001", QuotaPeriod.Semester, 3, Counting(Semester2Of2026, 0, 3)),
                Item("PAED-008", QuotaPeriod.AcademicYear, 1, Waived(Year2026, QuotaWindowStatus.ExemptPartialPeriod, 0, 1, "the 2027 academic year"))
            ],
            yearStart: new QuotaStartDto("the 2027 academic year", new(2027, 1, 1)));

        ProgressWords.StartNotice(summary).Should().StartWith("You started the programme on 2026-07-15. That was in the second half")
            .And.EndWith("count towards a later year's target. Your semester targets apply now.");
    }

    [Fact]
    public void TheNotStartedAlert_IsTheBuiltOne_InIso()
    {
        var summary = Summary(
            D,
            new(2027, 1, 15),
            stage: null,
            [
                Item("PAED-002", QuotaPeriod.Semester, 3, Waived(Semester2Of2026, QuotaWindowStatus.NotStarted, 0, 3, "semester 1, 2027")),
                Item("PAED-008", QuotaPeriod.AcademicYear, 1, Waived(Year2026, QuotaWindowStatus.NotStarted, 0, 1, "the 2027 academic year"))
            ],
            semesterStart: new QuotaStartDto("semester 1, 2027", new(2027, 1, 15)),
            yearStart: new QuotaStartDto("the 2027 academic year", new(2027, 1, 15)));

        ProgressWords.StartNotice(summary).Should().Be(
            "Your programme starts on 2027-01-15. Your first semester targets are for semester 1, 2027. Your first yearly " +
            "targets are for the 2027 academic year.");
        ProgressWords.Subtitle(summary).Should().Be("Semester 2, 2026");
    }

    [Fact]
    public void NeitherAlert_OnAnEndedProgrammesRecord_OrWhenEveryTargetApplies()
    {
        var running = Summary(D, new(2024, 1, 1), stage: 3, [Short]);
        ProgressWords.StartNotice(running).Should().BeNull();

        var ended = Summary(December, new(2024, 1, 1), stage: 3, [Short]) with
        {
            Ended = new ProgrammeEndDto(Completed: true, EndedOn: new(2026, 12, 1), Today: December)
        };
        ProgressWords.DecemberNotice(ended).Should().BeNull();
    }

    [Fact]
    public void TheIndexsCaptions_AndItsWindowHeading()
    {
        ProgressWords.GroupCaption(QuotaPeriod.Semester, 10).Should().Be("Each semester · 10 EPAs");
        ProgressWords.GroupCaption(QuotaPeriod.AcademicYear, 7).Should().Be("Once a year · 7 EPAs");
        ProgressWords.GroupCaption(QuotaPeriod.AcademicYear, 1).Should().Be("Once a year · 1 EPA");
        ProgressWords.PausedCaption(1).Should().Be("No longer in use · 1 EPA");
        ProgressWords.WindowHeading(Short).Should().Be("Semester 2, 2026");
        ProgressWords.WindowHeading(ShortYear).Should().Be("2026 academic year");
    }

    [Fact]
    public void TheCadence_NamesItsOwnFrame()
    {
        // Notes 8 and 9: PAED-003 is observed each semester and decided once a year, and says so in the semester group.
        ProgressWords.Cadence(Short with { DecisionCadence = QuotaPeriod.Semester }).Should().Be("Decided each semester");
        ProgressWords.Cadence(Short with { DecisionCadence = QuotaPeriod.AcademicYear }).Should().Be("Decided once a year");
        ProgressWords.Cadence(ShortYear with { DecisionCadence = QuotaPeriod.AcademicYear, DecisionIsOpportunistic = true })
            .Should().Be("Decided as opportunity allows");
        ProgressWords.Cadence(ShortYear).Should().BeNull("KGK-001 has no cadence: a dash, with a hidden name beside it");
        ProgressWords.NoCadence.Should().Be("No decision cadence");
    }

    [Fact]
    public void ThePausedRow_AndALocalBadge()
    {
        ProgressWords.PausedRow.Should().Be(
            "Paused by the College. It is not a target while it is paused. Its ratings and the credit it had earned are kept, " +
            "and what is completed on it meanwhile is credited if it is restored.");
        ProgressWords.LocalBadge(ShortYear with { OwningInstitutionName = "Kgosi Kgari Teaching Hospital" })
            .Should().Be("Kgosi Kgari Teaching Hospital's own");
        ProgressWords.LocalBadge(ShortYear).Should().BeNull();
    }

    [Fact]
    public void NoWordIsAFractionOrAPercentage()
    {
        // R3: never "n / m", a percentage or a lifetime total.
        string[] words =
        [
            ProgressWords.Figure(Short), ProgressWords.CountMeta(Short, D), ProgressWords.ShortRow(Short, D),
            ProgressWords.CountMeta(MetYear, December), ProgressWords.AtMinimum(Short.Current)!
        ];

        words.Should().OnlyContain(word => !word.Contains(" / ") && !word.Contains('%'));
    }
}
