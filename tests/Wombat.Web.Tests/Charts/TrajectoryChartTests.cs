using AngleSharp.Dom;
using Bunit;
using FluentAssertions;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Tests.Charts;

/// <summary>
/// TrajectoryChart in its new form (T355, flow 05; Q5, E1, E2, R4; notes 10, 11, 13; R3-C-Trajectory's states three, one,
/// other-scale, paused-heading and committee). Both drawings are in the markup at their fixed sizes inside a named,
/// focusable region; the table, always visible, says everything the drawing does; the heading's level is the page's.
/// </summary>
public sealed class TrajectoryChartTests : TestContext
{
    private static readonly DateOnly From = new(2026, 1, 1);
    private static readonly DateOnly To = new(2026, 12, 31);
    private static readonly DateOnly D = new(2026, 10, 3);

    /// <summary>The CPSA v11.1 ladder: ordinal 5 is the College's rung "4", ordinal 6 rung "5" (T100).</summary>
    private static readonly IReadOnlyList<TrajectoryRungDto> CpsaLadder =
    [
        new(1, "1"), new(2, "2"), new(3, "3a"), new(4, "3b"), new(5, "4"), new(6, "5")
    ];

    // ---- the drawings and the region (Q5, E1) ----

    [Fact]
    public void BothDrawings_AreInTheMarkup_AtTheirFixedSizes_InsideANamedFocusableRegion()
    {
        var cut = Render(MolefePaed001());

        var region = cut.Find("section.trajectory-card div.trajectory-figure");
        region.GetAttribute("role").Should().Be("region");
        region.GetAttribute("aria-label").Should().Be("Rating chart for PAED-001");
        region.GetAttribute("tabindex").Should().Be("0");

        var wide = region.QuerySelector("svg.trajectory-chart.trajectory-chart--sized.trajectory-chart--wide")!;
        wide.GetAttribute("width").Should().Be("900");
        wide.GetAttribute("viewBox").Should().StartWith("0 0 900 ");
        var narrow = region.QuerySelector("svg.trajectory-chart.trajectory-chart--sized.trajectory-chart--narrow")!;
        narrow.GetAttribute("width").Should().Be("322");
        narrow.GetAttribute("viewBox").Should().StartWith("0 0 322 ");
    }

    [Fact]
    public void ThePhoneDrawing_FitsTheFigureOfACardAt390_SoItsRegionNeverScrolls()
    {
        // The T355 replay's A.7.3: at 390 a card's figure is 324px (390, less the 16px gutters, the card's 16px padding and
        // its edges). The phone drawing was 326, so the focusable region scrolled 2px sideways. It is 322 now; the region
        // stays the floor only for a card narrower than that.
        TrajectoryFrame.Narrow.Width.Should().BeLessThanOrEqualTo(324);
    }

    [Fact]
    public void EachDrawing_IsNamedForWhatItAddsToTheTable()
    {
        var cut = Render(MolefePaed001());

        cut.FindAll("svg[role='img']").Select(svg => svg.GetAttribute("aria-label"))
            .Should().Equal("Chart of the 3 ratings in the table below.", "Chart of the 3 ratings in the table below.");
    }

    [Fact]
    public void TheCard_IsAnchoredByItsEpasId_ForTheStandingPanelsLinks()
    {
        var cut = Render(MolefePaed001());

        var card = cut.Find("section.trajectory-card");
        card.Id.Should().Be("trajectory-2");
        card.GetAttribute("aria-labelledby").Should().Be("trajectory-2-h");
        cut.Find("#trajectory-2-h").Should().NotBeNull();
    }

    // ---- the ladder, the months and the semesters ----

    [Fact]
    public void TheLadder_IsDrawnWhole_HighestRungFirst_NamedAsTheCollegePrintsIt()
    {
        var cut = Render(DlaminiPaed001());

        Labels(Wide(cut), "trajectory-chart-y-label").Should().Equal("5", "4", "3b", "3a", "2", "1");
        Labels(Narrow(cut), "trajectory-chart-y-label").Should().Equal("5", "4", "3b", "3a", "2", "1");
    }

    [Fact]
    public void TheTimeAxis_NamesEveryMonth_At900_AndEveryOtherMonth_At322()
    {
        var cut = Render(MolefePaed001());

        Labels(Wide(cut), "trajectory-chart-x-label").Should().Equal(
            "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec");
        Labels(Narrow(cut), "trajectory-chart-x-label").Should().Equal("Jan", "Mar", "May", "Jul", "Sep", "Nov");
    }

    [Fact]
    public void SemesterTwo_IsBanded_AndEachSemesterIsNamed()
    {
        var cut = Render(MolefePaed001());

        var band = Wide(cut).QuerySelector("rect.trajectory-chart-band")!;
        band.GetAttribute("x").Should().Be("441", "1 July on the 900 drawing's axis, as the board draws it");
        band.GetAttribute("width").Should().Be("363", "July to December");
        Labels(Wide(cut), "trajectory-chart-band-label").Should().Equal("Semester 1, 2026", "Semester 2, 2026");
    }

    /// <summary>
    /// T355, build review G1: the committee's pre-graduation review reads four years (act 5's review #7, 2023-01-15 to
    /// 2026-12-31). Its axis names quarters at 900 and years at 322, every Semester 2 is banded, no "Semester n, yyyy" is
    /// named, the years stand above the plot where the axis names quarters, and no two names in a row overlap.
    /// </summary>
    [Fact]
    public void AReviewWindowOfYears_IsNamedByQuarterOrYear_EverySemesterTwoBanded_WithNoNameOverAnother()
    {
        var cut = Render(PreGraduationReview());

        var wide = Wide(cut);
        Labels(wide, "trajectory-chart-x-label").Should().Equal(
            "Jan", "Apr", "Jul", "Oct", "Jan", "Apr", "Jul", "Oct", "Jan", "Apr", "Jul", "Oct", "Jan", "Apr", "Jul", "Oct");
        Labels(wide, "trajectory-chart-band-label").Should().Equal("2023", "2024", "2025", "2026");
        wide.QuerySelectorAll("rect.trajectory-chart-band").Should().HaveCount(4, "July to December of each year");
        ShouldNotOverlap(wide, "trajectory-chart-x-label", TrajectoryFrame.Wide, centred: true);
        ShouldNotOverlap(wide, "trajectory-chart-band-label", TrajectoryFrame.Wide, centred: false);

        var narrow = Narrow(cut);
        Labels(narrow, "trajectory-chart-x-label").Should().Equal("2023", "2024", "2025", "2026");
        Labels(narrow, "trajectory-chart-band-label").Should().BeEmpty("the axis already names the years");
        narrow.QuerySelectorAll("rect.trajectory-chart-band").Should().HaveCount(4);
        ShouldNotOverlap(narrow, "trajectory-chart-x-label", TrajectoryFrame.Narrow, centred: true);

        cut.Markup.Should().NotContain("Semester 1,").And.NotContain("Semester 2,");
    }

    [Fact]
    public void AYearsWindow_KeepsItsMonthsAndSemesters()
    {
        TrajectoryDrawing.AxisUnitFor(TrajectoryFrame.Wide, 365).Should().Be(TrajectoryDrawing.AxisUnit.Month);
        TrajectoryDrawing.AxisUnitFor(TrajectoryFrame.Narrow, 365).Should().Be(TrajectoryDrawing.AxisUnit.Month);
        TrajectoryDrawing.AxisUnitFor(TrajectoryFrame.Wide, 1447).Should().Be(TrajectoryDrawing.AxisUnit.Quarter);
        TrajectoryDrawing.AxisUnitFor(TrajectoryFrame.Narrow, 1447).Should().Be(TrajectoryDrawing.AxisUnit.Year);
    }

    // ---- the minimum, the exit level, Today (R4) ----

    [Fact]
    public void TheMinimum_StepsWhereTheTrainingYearChanged_AndIsLabelledInWords()
    {
        var cut = Render(MolefePaed001());

        // 4 until 2026-01-13, then 5: the edge of rung "4" from 1 January, of rung "5" from 14 January (x 110 at 900).
        Wide(cut).QuerySelector("path.trajectory-chart-minimum")!.GetAttribute("d").Should().Be("M84 114H110V74H804");
        Wide(cut).QuerySelector("path.trajectory-chart-below")!.GetAttribute("d").Should().Be("M84 114H110V74H804V274H84Z");
        Labels(Wide(cut), "trajectory-chart-minimum-label").Should().Equal("Minimum 5");
        Labels(Narrow(cut), "trajectory-chart-minimum-label").Should().Equal("Min 5");
        Labels(Wide(cut), "trajectory-chart-line-label").Should().Equal("Exit 5");
        Wide(cut).QuerySelector("line.trajectory-chart-exit")!.GetAttribute("y1").Should().Be("54");
    }

    [Fact]
    public void AMinimumStepOffTheLadder_DrawsNoMinimum_RatherThanRunningThePreviousOn()
    {
        // T355, build review R5: a per-year minimum naming an ordinal the drawn ladder has no rung for (a re-pinned scale).
        var trajectory = MolefePaed001() with
        {
            MinimumSteps = [new(From, 3, 5, "4"), new(new DateOnly(2026, 1, 14), 4, 9, "9")]
        };

        var cut = Render(trajectory);

        cut.FindAll("path.trajectory-chart-minimum").Should().BeEmpty();
        cut.FindAll("path.trajectory-chart-below").Should().BeEmpty();
        Labels(Wide(cut), "trajectory-chart-minimum-label").Should().BeEmpty();
    }

    [Fact]
    public void ARunOfRatingsInTheLastDays_IsSetBackFromThePlotsRightEdge()
    {
        // T355, build review R6: six ratings on 29 to 31 December are set a dot apart; pushed right, the run would pass the
        // plot's edge (804 at 900, 268 at 322) and sit over "Exit 5". It is set back from it, each still a dot apart.
        var points = Enumerable.Range(0, 6)
            .Select(index => Point(40 + index, new DateOnly(2026, 12, 29 + (index / 2)), 6, "5", TrajectoryAgainstMinimum.AtOrAbove))
            .ToArray();
        var cut = Render(MolefePaed001() with { Points = points });

        foreach (var (svg, frame) in new[] { (Wide(cut), TrajectoryFrame.Wide), (Narrow(cut), TrajectoryFrame.Narrow) })
        {
            var xs = svg.QuerySelector("path.trajectory-chart-line")!.GetAttribute("d")!
                .TrimStart('M').Split('L')
                .Select(pair => double.Parse(pair.Split(' ')[0], System.Globalization.CultureInfo.InvariantCulture))
                .ToList();
            xs.Should().HaveCount(6);
            xs.Should().OnlyContain(x => x <= frame.Right && x >= frame.Left);
            xs.Zip(xs.Skip(1), (a, b) => b - a).Should().OnlyContain(gap => gap >= (2 * frame.Radius) + 2, "each still a dot apart");
        }
    }

    [Fact]
    public void Today_IsARule_OnlyWhenThePageGivesOne()
    {
        var withToday = Render(MolefePaed001(), today: D);
        Wide(withToday).QuerySelectorAll("line.trajectory-chart-today").Should().ContainSingle();
        Labels(Wide(withToday), "trajectory-chart-note").Should().Contain("Today");

        var committee = Render(MolefePaed001(), today: null);
        committee.FindAll("line.trajectory-chart-today").Should().BeEmpty("the committee's window is the review's, D2");
        committee.Markup.Should().NotContain(">Today<");
    }

    [Fact]
    public void TheKey_SaysWhatEachMarkIs()
    {
        var cut = Render(MolefePaed001());

        var key = cut.Find("ul.trajectory-key");
        key.GetAttribute("aria-label").Should().Be("How to read the chart");
        key.QuerySelectorAll("li").Select(Text).Should().Equal(
            "Below the minimum: 4 until 2026-01-13, then 5 (training year 4)", "Exit level 5", "A rating");
    }

    // ---- the points ----

    [Fact]
    public void OneRating_IsOneDot_AndNoLine()
    {
        var cut = Render(DlaminiPaed001());

        Wide(cut).QuerySelectorAll("path.trajectory-chart-dots").Should().ContainSingle();
        cut.FindAll("path.trajectory-chart-line").Should().BeEmpty();
        cut.FindAll("path.trajectory-chart-hollow").Should().BeEmpty();
    }

    [Fact]
    public void ThreeRatings_AreJoinedOldestFirst_EachSetADotApartFromTheOneBefore()
    {
        var cut = Render(MolefePaed001());

        // 21, 22 and 24 September: 2px a day at 900, so the second and third are set a dot (2 x 7 + 3) apart.
        Wide(cut).QuerySelector("path.trajectory-chart-line")!.GetAttribute("d").Should().Be("M603 54L620 54L637 94");
    }

    [Fact]
    public void ARatingOnAnotherScale_IsHollowInItsOwnLane_LeftOutOfTheLine_AndKeyed()
    {
        var cut = Render(OtherScalePaed005());

        Wide(cut).QuerySelector("path.trajectory-chart-hollow")!.GetAttribute("d").Should().Contain(" 300a7 7",
            "the hollow ring sits in the lane under the rungs");
        cut.FindAll("path.trajectory-chart-line").Should().BeEmpty("one rating is on the ladder");
        Labels(Wide(cut), "trajectory-chart-note").Should().Contain("Other scale");
        Labels(Narrow(cut), "trajectory-chart-note").Should().Contain("Other");
        cut.Find("ul.trajectory-key").TextContent.Should().Contain("Rated on another scale: counts towards the number, not the level");
    }

    // ---- the table (E2; note 11) ----

    [Fact]
    public void TheTable_ListsEachRatingOldestFirst_ItsEncounterTheRowsHeader()
    {
        var cut = Render(MolefePaed001());

        var table = cut.Find("table.clinic-table.clinic-table--stack.trajectory-table");
        table.QuerySelector("caption")!.TextContent.Should().Be("The ratings, oldest first");
        table.QuerySelectorAll("thead th").Select(Text).Should().Equal(
            "Encounter", "Rating", "Against the minimum then", "Activity", "Assessor");

        var rows = table.QuerySelectorAll("tbody tr");
        rows.Select(row => row.QuerySelector("th[scope='row']")!.TextContent).Should().Equal("2026-09-21", "2026-09-22", "2026-09-24");
        rows.Select(row => row.QuerySelectorAll("td").Select(Text).ToArray()).Should().BeEquivalentTo(new[]
        {
            new[] { "5", "At or above (5, training year 4)", "Mini-CEX (Paediatrics) · PAED-001 · 2026-09-21", "Thandi Zulu" },
            new[] { "5", "At or above (5, training year 4)", "Case-Based Discussion (Paediatrics) · PAED-001 · 2026-09-22", "David Naidoo" },
            new[] { "4", "Below (5, training year 4)", "DOPS (Paediatrics) · PAED-001 · 2026-09-24", "Mohammed Patel" }
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public void EachActivity_IsA44pxBlockLink_ToItsPage_AndEveryOtherCellIsLabelledByItsColumn()
    {
        var cut = Render(MolefePaed001());

        var link = cut.Find("table.trajectory-table tbody tr a");
        link.ClassList.Should().Contain(["activity-link", "activity-block-link"]);
        link.GetAttribute("href").Should().Be("/activities/11");
        cut.Find("table.trajectory-table tbody tr").QuerySelectorAll("td[data-label]")
            .Select(cell => cell.GetAttribute("data-label")).Should().Equal("Rating", "Against the minimum then", "Assessor");
    }

    [Fact]
    public void ARatingOnAnotherScale_IsNamedOnItsOwnLadder_AndNotJudgedOnThisOne()
    {
        var cut = Render(OtherScalePaed005());

        var first = cut.Find("table.trajectory-table tbody tr").QuerySelectorAll("td").Select(Text).ToArray();
        first[0].Should().Be("Independent on O-R Scale");
        first[1].Should().Be("Not on the ladder: counts towards the number, not the level");
    }

    [Fact]
    public void AnUndatedEncounter_IsMarkedNotRecorded_WithTheDayItWasCreated()
    {
        var undated = MolefePaed001() with
        {
            Points = [Point(11, new DateOnly(2026, 3, 20), 6, "5", TrajectoryAgainstMinimum.AtOrAbove) with { ObservedOnDeclared = false }]
        };

        Render(undated).Find("table.trajectory-table tbody th").TextContent.Should().Be("not recorded (created 2026-03-20)");
    }

    // ---- the heading (note 10; C5) ----

    [Fact]
    public void OnTheEpaPage_TheHeadingIsAnH2_RatingTrajectory_MarkedWhenPaused()
    {
        var paused = MolefePaed001() with { EpaInForce = false };

        var cut = Render(paused, title: "Rating trajectory", level: 2);

        var heading = cut.Find("h2#trajectory-2-h");
        heading.ClassList.Should().Contain("epa-section-title");
        heading.TextContent.Should().Be("Rating trajectory (no longer in use)");
        heading.QuerySelector("span.paused-mark")!.TextContent.Should().Be("(no longer in use)");
    }

    [Fact]
    public void OnTheCommitteePage_TheHeadingNamesTheEpa_OneLevelBelowItsSection_AndTheSummaryNamesTheTrainee()
    {
        var cut = RenderComponent<TrajectoryChart>(parameters => parameters
            .Add(chart => chart.Trajectory, MolefePaed001())
            .Add(chart => chart.HeadingLevel, 4)
            .Add(chart => chart.ReviewWindow, true)
            .Add(chart => chart.SubjectName, "Lerato Molefe"));

        cut.Find("h4#trajectory-2-h").TextContent.Should().Be("PAED-001 — Providing paediatric emergency care to children");
        Text(cut.Find(".trajectory-head p")).Should().Be(
            "Lerato Molefe · 3 ratings in the review window, 2026-01-01 to 2026-12-31, from Thandi Zulu, David Naidoo and " +
            "Mohammed Patel. 2 at the minimum, 1 below.");
        cut.FindAll("line.trajectory-chart-today").Should().BeEmpty();
    }

    [Fact]
    public void NoRating_IsSaid_InTheCard_WithNoDrawing()
    {
        var none = MolefePaed001() with { Points = [] };

        var cut = Render(none, title: "Rating trajectory");

        cut.Find("p.card-empty").TextContent.Should().Be("No rating yet.");
        cut.FindAll("svg").Should().BeEmpty();
        cut.FindAll("table").Should().BeEmpty();
    }

    [Fact]
    public void MultiSourceFeedback_IsSaidNotToBePlotted_WhereThePageHasItBeside()
    {
        Render(MolefePaed001(), msf: true).Find("p.trajectory-msf").TextContent.Should().Be("Multi-source feedback is not plotted.");
        Render(MolefePaed001()).FindAll("p.trajectory-msf").Should().BeEmpty();
    }

    // ---- the axis' words (T123) ----

    [Fact]
    public void ARungsName_IsEncodedOnTheAxis()
    {
        var trajectory = DlaminiPaed001() with { Rungs = [new(1, "<x>")], Points = [Point(1, new(2026, 3, 10), 1, "<x>", TrajectoryAgainstMinimum.NotGated)] };

        var cut = Render(trajectory);

        cut.Markup.Should().Contain("&lt;x&gt;").And.NotContain("<x>");
    }

    [Fact]
    public void ALongRungName_ShowsItsOrdinalOnTheAxis_UnlessThatIsAnotherRungsName()
    {
        var ladder = new TrajectoryRungDto[]
        {
            new(1, "1"), new(2, "2"), new(3, "3a"), new(4, "3b"), new(5, "4 (independent)"), new(6, "5")
        };
        var trajectory = DlaminiPaed001() with { Rungs = ladder, MinimumSteps = [] };

        Labels(Wide(Render(trajectory)), "trajectory-chart-y-label").Should().Equal("5", "4 (…", "3b", "3a", "2", "1");
    }

    // ---- the cast ----

    private static EpaTrajectoryDto MolefePaed001() => new(
        2, "PAED-001", "Providing paediatric emergency care to children", true, 1, "CPSA Paediatric Entrustment Scale v11.1",
        CpsaLadder,
        [
            Point(11, new(2026, 9, 21), 6, "5", TrajectoryAgainstMinimum.AtOrAbove, "Thandi Zulu", "Mini-CEX (Paediatrics) · PAED-001 · 2026-09-21", 4, "5"),
            Point(12, new(2026, 9, 22), 6, "5", TrajectoryAgainstMinimum.AtOrAbove, "David Naidoo", "Case-Based Discussion (Paediatrics) · PAED-001 · 2026-09-22", 4, "5"),
            Point(13, new(2026, 9, 24), 5, "4", TrajectoryAgainstMinimum.Below, "Mohammed Patel", "DOPS (Paediatrics) · PAED-001 · 2026-09-24", 4, "5")
        ])
    {
        ExitLevelOrder = 6,
        ExitLevelLabel = "5",
        MinimumSteps = [new(From, 3, 5, "4"), new(new DateOnly(2026, 1, 14), 4, 6, "5")],
        WindowFrom = From,
        WindowTo = To
    };

    /// <summary>Act 5's review #7: Molefe's pre-graduation review, read over 2023-01-15 to 2026-12-31 (D2).</summary>
    private static EpaTrajectoryDto PreGraduationReview() => MolefePaed001() with
    {
        MinimumSteps =
        [
            new(new DateOnly(2023, 1, 15), 1, 3, "3a"), new(new DateOnly(2024, 1, 15), 2, 4, "3b"),
            new(new DateOnly(2025, 1, 14), 3, 5, "4"), new(new DateOnly(2026, 1, 14), 4, 6, "5")
        ],
        WindowFrom = new DateOnly(2023, 1, 15),
        WindowTo = To
    };

    private static EpaTrajectoryDto DlaminiPaed001() => new(
        2, "PAED-001", "Providing paediatric emergency care to children", true, 1, "CPSA Paediatric Entrustment Scale v11.1",
        CpsaLadder,
        [Point(21, new(2026, 9, 23), 5, "4", TrajectoryAgainstMinimum.AtOrAbove, "David Naidoo", "Mini-CEX (Paediatrics) · PAED-001 · 2026-09-23", 3, "4")])
    {
        ExitLevelOrder = 6,
        ExitLevelLabel = "5",
        MinimumSteps = [new(From, 2, 4, "3b"), new(new DateOnly(2026, 1, 14), 3, 5, "4")],
        WindowFrom = From,
        WindowTo = To
    };

    private static EpaTrajectoryDto OtherScalePaed005() => DlaminiPaed001() with
    {
        EpaId = 5,
        EpaCode = "PAED-005",
        EpaTitle = "Providing neonatal care in intensive and high-care settings",
        Points =
        [
            Point(31, new(2026, 8, 10), 4, "3b", TrajectoryAgainstMinimum.NotComparable, "Sarah Botha", "DOPS (Paediatrics) · PAED-005 · 2026-08-10", 3, null)
                with { OffLadder = true, OtherScaleName = "O-R Scale", OtherScaleRatingLabel = "Independent" },
            Point(32, new(2026, 9, 12), 4, "3b", TrajectoryAgainstMinimum.Below, "Mohammed Patel", "Case-Based Discussion (Paediatrics) · PAED-005 · 2026-09-12", 3, "4")
        ]
    };

    private static TrajectoryPointDto Point(
        int activityId, DateOnly observedOn, int rating, string ratingLabel, TrajectoryAgainstMinimum against,
        string assessor = "Thandi Zulu", string activity = "", int? year = null, string? minimum = null)
        => new(activityId, observedOn, ObservedOnDeclared: true, rating, ratingLabel, "Direct observation", assessor.ToLowerInvariant())
        {
            AssessorName = assessor,
            ActivityName = activity,
            TrainingYear = year,
            MinimumLabel = minimum,
            AgainstMinimum = against
        };

    private IRenderedComponent<TrajectoryChart> Render(
        EpaTrajectoryDto trajectory, DateOnly? today = null, string? title = null, int level = 2, bool msf = false)
        => RenderComponent<TrajectoryChart>(parameters => parameters
            .Add(chart => chart.Trajectory, trajectory)
            .Add(chart => chart.Today, today)
            .Add(chart => chart.Title, title)
            .Add(chart => chart.HeadingLevel, level)
            .Add(chart => chart.MsfBeside, msf));

    private static IElement Wide(IRenderedFragment cut) => cut.Find("svg.trajectory-chart--wide");

    private static IElement Narrow(IRenderedFragment cut) => cut.Find("svg.trajectory-chart--narrow");

    private static List<string> Labels(IElement svg, string cssClass)
        => svg.QuerySelectorAll($"text.{cssClass}").Select(text => text.TextContent).ToList();

    /// <summary>
    /// No two names in a row overlap: each label's extent at the frame's size (as the drawing measures it), centred on its
    /// x or starting there, ends before the next begins.
    /// </summary>
    private static void ShouldNotOverlap(IElement svg, string cssClass, TrajectoryFrame frame, bool centred)
    {
        var extents = svg.QuerySelectorAll($"text.{cssClass}")
            .Select(text =>
            {
                var x = double.Parse(text.GetAttribute("x")!, System.Globalization.CultureInfo.InvariantCulture);
                var width = TrajectoryDrawing.TextWidth(frame, text.TextContent);
                return centred ? (Start: x - (width / 2), End: x + (width / 2)) : (Start: x, End: x + width);
            })
            .OrderBy(extent => extent.Start)
            .ToList();

        extents.Zip(extents.Skip(1), (a, b) => b.Start - a.End).Should().OnlyContain(gap => gap > 0, $"no {cssClass} sits over another");
    }

    private static string Text(IElement element) => string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
