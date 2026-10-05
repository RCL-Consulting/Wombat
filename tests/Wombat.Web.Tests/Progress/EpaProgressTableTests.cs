using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using FluentAssertions;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.Curricula;
using Wombat.Web.Components.Shared;
using Wombat.Web.Components.Shared.Progress;
using static Wombat.Web.Tests.Progress.ProgressFixtures;

namespace Wombat.Web.Tests.Progress;

/// <summary>
/// My progress's EPA index, "Your EPAs" (T355, flow 05; R3-C-Progress; Q1, R3, C2, C3, C7, C13; notes 8, 9): one stacked
/// table per group, each EPA a row header linking to its page, its count for the window containing today, when a
/// committee decides it, and its STAR against the training year; then the paused EPAs, read and never counted.
/// </summary>
/// <remarks>The cast's day is 2026-10-03 (<see cref="ProgressFixtures.D" />); Lerato Molefe is in training year 4.</remarks>
public sealed class EpaProgressTableTests : TestContext
{
    private const string Cpsa = "CPSA Paediatric Entrustment Scale v11.1";

    [Fact]
    public void EachGroup_IsATableOfItsOwn_CaptionedWithItsKindAndCount()
    {
        // C7: the caption is the group, "Each semester · 10 EPAs", and the window column is named for the window.
        var cut = Render(Molefe());

        var tables = cut.FindAll("table").ToList();
        tables.Should().HaveCount(2);
        tables.Select(table => Text(table.QuerySelector("caption.index-caption")!)).Should().Equal(
            "Each semester · 3 EPAs", "Once a year · 2 EPAs");
        tables[0].QuerySelectorAll("thead th").Select(Text).Should().Equal(
            "EPA", "Semester 2, 2026", "Committee decides", "STAR against training year 4");
        tables[1].QuerySelectorAll("thead th").Select(Text).Should().Equal(
            "EPA", "2026 academic year", "Committee decides", "STAR against training year 4");

        foreach (var table in tables)
        {
            table.ClassList.Should().Contain(["clinic-table", "clinic-table--stack", "clinic-table--index"]);
            table.GetAttribute("role").Should().Be("table");
            table.ParentElement!.ClassList.Should().Contain("table-container");
        }
    }

    [Fact]
    public void EachEpa_IsARowHeader_WhoseNameLinksToItsPage()
    {
        // Round 1, correction 2: addressed by the EPA's id, named "Code — Title".
        var cut = Render(Molefe());

        var header = Row(cut, "PAED-001").QuerySelector("th")!;
        header.GetAttribute("scope").Should().Be("row");
        header.GetAttribute("role").Should().Be("rowheader");
        var link = header.QuerySelector("a.epa-link")!;
        link.GetAttribute("href").Should().Be("/portfolio/progress/1");
        Text(link).Should().Be("PAED-001 — Providing paediatric emergency care to children");
        link.QuerySelector(".paused-mark").Should().BeNull("an EPA in force is never marked");
    }

    [Fact]
    public void ACountCell_IsAFigure_ABar_AndALine()
    {
        // R3: a count against a target for a named window, never "n / m" or a percentage.
        var cut = Render(Molefe());

        var cell = Row(cut, "PAED-012").QuerySelector("td.count-cell")!;
        Text(cell.QuerySelector(".count-figure")!).Should().Be("2 of 3 this semester");
        Text(cell.QuerySelector("p.count-meta")!).Should().Be("1 more by 2026-11-30");
        var bar = cell.QuerySelector(".progress-bar")!;
        bar.GetAttribute("role").Should().Be("progressbar");
        bar.GetAttribute("aria-valuemax").Should().Be("3");
        bar.GetAttribute("aria-valuenow").Should().Be("2");
        bar.GetAttribute("aria-label").Should().Be("PAED-012: 2 of 3 this semester");
        bar.QuerySelector(".progress-bar-fill")!.ClassList.Should().NotContain("is-complete");
        cell.TextContent.Should().NotContain("%").And.NotContain(" / ");
    }

    [Fact]
    public void AMetTarget_SaysSoForItsWindow_AndFillsTheBar()
    {
        // C3, C11: "the" before a yearly window's name inside a sentence.
        var cut = Render(Molefe());

        var semester = Row(cut, "PAED-001").QuerySelector("td.count-cell")!;
        Text(semester.QuerySelector("p.count-meta")!).Should().Be("Target met for Semester 2, 2026.");
        semester.QuerySelector(".progress-bar-fill")!.ClassList.Should().Contain("is-complete");

        Text(Row(cut, "KGK-001").QuerySelector("p.count-meta")!).Should().Be("Target met for the 2026 academic year.");
    }

    [Fact]
    public void InDecember_ACellKeepsItsFigure_AndSaysDecemberStillCounts()
    {
        // C3: only "n more by" is replaced; a yearly row names its own window.
        var cut = Render(Summary(December, new DateOnly(2023, 1, 15), 3,
        [
            Item("PAED-004", QuotaPeriod.Semester, 3, Counting(Semester2Of2026, 1, 3)) with { EpaId = 4 },
            Item("PAED-008", QuotaPeriod.AcademicYear, 1, Counting(Year2026, 0, 1)) with { EpaId = 8 }
        ]));

        Text(Row(cut, "PAED-004").QuerySelector(".count-figure")!).Should().Be("1 of 3 this semester");
        Text(Row(cut, "PAED-004").QuerySelector("p.count-meta")!)
            .Should().Be("2 more; encounters in December still count towards Semester 2, 2026.");
        Text(Row(cut, "PAED-008").QuerySelector("p.count-meta")!)
            .Should().Be("1 more; encounters in December still count towards the 2026 academic year.");
    }

    [Fact]
    public void AWaivedWindow_IsItsCountAndWhenTargetsStart_OneLine_NoBar()
    {
        // C2, R3: a waived window shows its count and when targets start, never a fraction or a bar.
        var cut = Render(Summary(D, new DateOnly(2026, 8, 18), 1,
        [
            Item("PAED-002", QuotaPeriod.Semester, 3,
                Waived(Semester2Of2026, QuotaWindowStatus.ExemptPartialPeriod, 2, 3, "semester 1, 2027")) with { EpaId = 2 },
            Item("PAED-008", QuotaPeriod.AcademicYear, 1,
                Waived(Year2026, QuotaWindowStatus.ExemptPartialPeriod, 0, 1, "the 2027 academic year")) with { EpaId = 8 }
        ]));

        var semester = Row(cut, "PAED-002").QuerySelector("td.count-cell")!;
        Text(semester).Should().Be("No target this semester · 2 recorded · targets start with semester 1, 2027");
        semester.QuerySelector(".progress-bar").Should().BeNull();
        semester.QuerySelector("p.count-meta").Should().BeNull("the one line is the cell");
        Text(Row(cut, "PAED-008").QuerySelector("td.count-cell")!)
            .Should().Be("No target in 2026 · 0 recorded · targets start with the 2027 academic year");
    }

    [Fact]
    public void BeforeTheProgrammeStarts_ACellSaysWhenTargetsStart_UnderTodaysWindow()
    {
        // C2: the window column and captions read the window containing today.
        var cut = Render(Summary(D, new DateOnly(2027, 1, 15), null,
        [
            Item("PAED-001", QuotaPeriod.Semester, 3, Waived(Semester2Of2026, QuotaWindowStatus.NotStarted, 0, 3, "semester 1, 2027")) with { EpaId = 1 }
        ]));

        cut.FindAll("thead th").Select(Text).Should().Contain("Semester 2, 2026");
        Text(Row(cut, "PAED-001").QuerySelector("td.count-cell")!).Should().Be("No target yet · targets start with semester 1, 2027");
        Row(cut, "PAED-001").QuerySelector(".progress-bar").Should().BeNull();
    }

    [Fact]
    public void CommitteeDecides_InItsOwnFrame_AndAnItemWithNoneIsADashWithItsWords()
    {
        // Notes 8, 9: PAED-003 is observed each semester and decided once a year; KGK-001 has no cadence.
        var cut = Render(Molefe());

        Text(Cell(cut, "PAED-001", "Committee decides")).Should().Be("Decided each semester");
        Text(Cell(cut, "PAED-003", "Committee decides")).Should().Be("Decided once a year");
        Text(Cell(cut, "PAED-008", "Committee decides")).Should().Be("Decided as opportunity allows");

        var none = Cell(cut, "KGK-001", "Committee decides");
        Text(none.QuerySelector(".cadence-none")!).Should().Be("—");
        none.QuerySelector(".cadence-none")!.GetAttribute("aria-hidden").Should().Be("true");
        Text(none.QuerySelector(".visually-hidden")!).Should().Be("No decision cadence");
    }

    [Fact]
    public void AnInstitutionsOwnEpa_CarriesItsOwnersBadge()
    {
        var cut = Render(Molefe());

        var badge = Row(cut, "KGK-001").QuerySelector("th .badge")!;
        Text(badge).Should().Be("Kgosi Kgari Teaching Hospital's own");
        badge.ClassList.Should().Contain("badge-draft", "a neutral badge (Spec § 4)");
        Row(cut, "PAED-001").QuerySelector("th .badge").Should().BeNull();
    }

    [Fact]
    public void TheStarColumn_ReadsTheStandingByEpaId()
    {
        var cut = Render(Molefe(), Standing());

        Verdict(cut, "PAED-001").Should().Be(("At or above", "5"));
        Verdict(cut, "PAED-012").Should().Be(("Below", "4 · expires 2026-10-23"));
        Verdict(cut, "PAED-003").Should().Be(("Not comparable", "Independent on O-R Scale"));
        Verdict(cut, "PAED-008").Should().Be(("No decision", null));
        Text(Cell(cut, "PAED-003", "STAR against training year 4")).Should().Be("Not comparable Independent on O-R Scale",
            "the badge and the level are two words apart, not run together for a screen reader");
        Text(Cell(cut, "KGK-001", "STAR against training year 4")).Should().Be(
            "No decision", "the standing has no row for it, and a stacked cell is never blank under its label (A3)");
        Cell(cut, "PAED-001", "STAR against training year 4").QuerySelector(".badge")!.ClassList.Should().Contain("badge-standing-met");
    }

    [Fact]
    public void WhenTheStandingFailed_EachStarCellSaysNotLoaded_NeverNoDecision()
    {
        // C8: the standing's own section says it failed; the index claims nothing.
        var cut = Render(Molefe(), standing: null, standingFailed: true);

        foreach (var code in new[] { "PAED-001", "PAED-012", "KGK-001" })
        {
            var cell = Cell(cut, code, "STAR against training year 4");
            Text(cell).Should().Be("Not loaded");
            cell.QuerySelector(".badge").Should().BeNull();
        }
    }

    [Fact]
    public void EveryCellButTheEpa_IsLabelledByItsColumn_ForTheStackedRow()
    {
        var cut = Render(Molefe());

        Row(cut, "PAED-001").QuerySelectorAll("td").Select(cell => cell.GetAttribute("data-label")).Should().Equal(
            "Semester 2, 2026", "Committee decides", "STAR against training year 4");
        Row(cut, "PAED-001").QuerySelector("th")!.HasAttribute("data-label").Should().BeFalse();
    }

    [Fact]
    public void APausedEpa_IsListedApart_WithWhyItIsNotCounted()
    {
        // Spec § 6, C13: read, never counted; tr.is-paused with the paused mark, its page still linked.
        var summary = Molefe() with { Paused = [new PausedItemDto(112, 12, "PAED-012", Title12, QuotaPeriod.Semester)] };

        var cut = Render(summary);

        var paused = cut.FindAll("table").Last();
        Text(paused.QuerySelector("caption.index-caption")!).Should().Be("No longer in use · 1 EPA");
        paused.QuerySelectorAll("thead th").Select(Text).Should().Equal("EPA", "Why it is not counted");
        var row = paused.QuerySelector("tbody tr")!;
        row.ClassList.Should().Contain("is-paused");
        var link = row.QuerySelector("th a.epa-link")!;
        link.GetAttribute("href").Should().Be("/portfolio/progress/12");
        Text(link).Should().Be($"PAED-012 — {Title12} (no longer in use)");
        Text(link.QuerySelector(".paused-mark")!).Should().Be("(no longer in use)");
        Text(row.QuerySelector("td")!).Should().Be(ProgressWords.PausedRow);
        row.QuerySelector("td")!.GetAttribute("data-label").Should().Be("Why it is not counted");
        cut.FindAll(".detail-card--paused").Should().BeEmpty();
    }

    [Fact]
    public void WithNothingPaused_ThereIsNoPausedTable()
    {
        Render(Molefe()).Markup.Should().NotContain("No longer in use");
    }

    // ─── Names as text: the registrar page (T358, flow 06; review 7; R2-Registrar r1) ─

    /// <summary>
    /// On the registrar page a member of staff reads someone else's index: the EPA pages are the registrar's own, so no
    /// name links anywhere, in force or paused; the row header is the name, with its paused mark and its local badge.
    /// </summary>
    [Fact]
    public void WithNamesAsText_NoEpaIsALink_AndEachRowHeaderIsTheName()
    {
        var summary = Molefe() with { Paused = [new PausedItemDto(112, 12, "PAED-012", Title12, QuotaPeriod.Semester)] };

        var cut = RenderComponent<EpaProgressTable>(parameters => parameters
            .Add(table => table.Summary, summary)
            .Add(table => table.TrainingYear, summary.TraineeStage)
            .Add(table => table.EpaNamesAsText, true));

        cut.FindAll("a").Should().BeEmpty();
        Text(Row(cut, "PAED-001").QuerySelector("th")!).Should().Be("PAED-001 — Providing paediatric emergency care to children");
        Text(Row(cut, "KGK-001").QuerySelector("th .badge")!).Should().NotBeEmpty("the institution's own EPA keeps its badge");
        var paused = cut.FindAll("table").Last().QuerySelector("tbody tr th")!;
        Text(paused).Should().Be($"PAED-012 — {Title12} (no longer in use)");
        paused.QuerySelector(".paused-mark").Should().NotBeNull();
    }

    [Fact]
    public void ByDefault_TheNamesStayLinks_AsMyProgressDrawsThem()
    {
        Render(Molefe()).FindAll("th a.epa-link").Should().HaveCount(5);
    }

    // ─── Fixtures ────────────────────────────────────────────────────────────

    private const string Title12 = "Communicating with and counselling patients, caregivers and healthcare teams";

    private IRenderedComponent<EpaProgressTable> Render(
        TraineeCurriculumProgressSummaryDto summary, EntrustmentStandingDto? standing = null, bool standingFailed = false)
        => RenderComponent<EpaProgressTable>(parameters => parameters
            .Add(table => table.Summary, summary)
            .Add(table => table.Standing, standing)
            .Add(table => table.StandingFailed, standingFailed)
            .Add(table => table.TrainingYear, summary.TraineeStage));

    /// <summary>
    /// Lerato Molefe on 2026-10-03, in training year 4, cut to five of her EPAs: PAED-001 met, PAED-003 (decided once a
    /// year), PAED-012 two of three; PAED-008 (as opportunity allows), and Kgosi Kgari's KGK-001, met.
    /// </summary>
    private static TraineeCurriculumProgressSummaryDto Molefe()
        => Summary(D, new DateOnly(2023, 1, 15), 4,
        [
            Item("PAED-001", QuotaPeriod.Semester, 3, Counting(Semester2Of2026, 3, 3)) with
            {
                EpaId = 1, DecisionCadence = QuotaPeriod.Semester
            },
            Item("PAED-003", QuotaPeriod.Semester, 3, Counting(Semester2Of2026, 0, 3), title: "Providing intensive care to children") with
            {
                EpaId = 3, DecisionCadence = QuotaPeriod.AcademicYear
            },
            Item("PAED-012", QuotaPeriod.Semester, 3, Counting(Semester2Of2026, 2, 3), title: Title12) with
            {
                EpaId = 12, DecisionCadence = QuotaPeriod.Semester
            },
            Item("PAED-008", QuotaPeriod.AcademicYear, 1, Counting(Year2026, 0, 1)) with
            {
                EpaId = 8, DecisionCadence = QuotaPeriod.AcademicYear, DecisionIsOpportunistic = true
            },
            Item("KGK-001", QuotaPeriod.AcademicYear, 1, Counting(Year2026, 1, 1), title: "Running a paediatric outreach clinic at a district hospital") with
            {
                EpaId = 21, OwningInstitutionName = "Kgosi Kgari Teaching Hospital", MinimumByTrainingYear = false
            }
        ]);

    private static EntrustmentStandingDto Standing()
        => new(D, new DateOnly(2023, 1, 15), 4, false,
        [
            Epa(1, "PAED-001", new StandingDecisionDto(1, 6, "5", null, D, null), EntrustmentStandingStatus.AtOrAbove),
            Epa(3, "PAED-003", new StandingDecisionDto(3, 5, "Independent", "O-R Scale", D, null), EntrustmentStandingStatus.NotComparable),
            Epa(8, "PAED-008", null, EntrustmentStandingStatus.NoDecision),
            Epa(12, "PAED-012", new StandingDecisionDto(2, 5, "4", null, D, new DateOnly(2026, 10, 23)), EntrustmentStandingStatus.Below)
        ],
        new ExitRuleReadinessDto(0, 0, [], []));

    private static EpaStandingDto Epa(int epaId, string code, StandingDecisionDto? decision, EntrustmentStandingStatus status)
        => new(100 + epaId, epaId, code, $"{code} title", Cpsa, IsLocal: false, 6, "5", false, 6, "5", decision, status, status, null);

    private static IElement Row(IRenderedFragment cut, string code)
        => cut.FindAll("tbody tr").Single(row => Text(row.QuerySelector("th")!).StartsWith(code + " ", StringComparison.Ordinal));

    private static IElement Cell(IRenderedFragment cut, string code, string column)
        => Row(cut, code).QuerySelectorAll("td").Single(cell => cell.GetAttribute("data-label") == column);

    private static (string Badge, string? Level) Verdict(IRenderedFragment cut, string code)
    {
        var cell = Cell(cut, code, "STAR against training year 4");
        return (Text(cell.QuerySelector(".badge")!), cell.QuerySelector(".verdict-level") is { } level ? Text(level) : null);
    }

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();
}
