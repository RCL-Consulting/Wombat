using System.Globalization;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Components;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Application.Features.Curricula.Quota;

namespace Wombat.Web.Components.Shared;

/// <summary>
/// The two fixed sizes a rating trajectory is drawn at (T355, Q5, E1; R3-C-Trajectory): 900px wide where its card holds
/// it, 326px below that. Never scaled: each has its own geometry, so a rung and a month stay legible at both.
/// </summary>
/// <param name="Width">The drawing's width, and its viewBox's.</param>
/// <param name="Left">The plot's left edge; the rungs' names stand to its left.</param>
/// <param name="Right">The plot's right edge; "Exit 5" and "Minimum 5" stand to its right.</param>
/// <param name="Top">The top rung's upper edge; the semester names stand above it.</param>
/// <param name="RungHeight">One rung's band.</param>
/// <param name="Radius">A point's radius.</param>
/// <param name="LaneDrop">From the lowest rung's edge to the other scale's lane.</param>
/// <param name="AxisDrop">From the lowest rung's edge to the time axis.</param>
/// <param name="MonthDrop">From the axis to the months' baseline.</param>
/// <param name="FontSize">The labels' size, for placing them (the stylesheet sets it: 13px, 12px at 326).</param>
/// <param name="EveryOtherMonth">Whether only every other month is named (the 326 drawing).</param>
/// <param name="MinimumWord">"Minimum" beside the edge at 900; "Min" at 326.</param>
/// <param name="LaneWord">The lane's name: "Other scale" at 900; "Other" at 326.</param>
public sealed record TrajectoryFrame(
    int Width,
    int Left,
    int Right,
    int Top,
    int RungHeight,
    int Radius,
    int LaneDrop,
    int AxisDrop,
    int MonthDrop,
    int FontSize,
    bool EveryOtherMonth,
    string MinimumWord,
    string LaneWord)
{
    /// <summary>
    /// The 900px drawing (R3-C-Trajectory's wide board), its plot starting 20px further right than the board's so the lane's
    /// name, "Other scale", is not cut at the drawing's edge.
    /// </summary>
    public static readonly TrajectoryFrame Wide = new(900, 84, 804, 34, 40, 7, 26, 50, 20, 13, false, "Minimum", "Other scale");

    /// <summary>The 326px drawing (R3-C-Trajectory's 390 board).</summary>
    public static readonly TrajectoryFrame Narrow = new(326, 44, 268, 30, 32, 6, 22, 42, 18, 12, true, "Min", "Other");
}

/// <summary>
/// One trajectory drawn in one <see cref="TrajectoryFrame" />: every coordinate the chart's SVG needs, worked out once in
/// C# so the markup only places it (T355; R4; notes 10, 11, 13).
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><b>A real time axis</b> over the window read (the EPA page's academic year, the committee's review window, D2), a
/// tick at each month's start, its name in the middle of its month (every other month at 326). Semester 2 (July to
/// December: December counts into it, D40) is banded; each semester is named at its start. A window longer than about 14
/// months (a pre-graduation review's, years long) has no room for a month or a semester's name: its axis is named by
/// quarter, or by year where a quarter's name will not fit, every Semester 2 is still banded, and the years are named
/// above the plot where the axis names quarters (T355, build review G1). No two names in a row overlap, at either size.</item>
/// <item><b>The ladder</b> top to bottom, highest rung first, a gridline through each rung. A rating on another ladder sits
/// hollow in its own lane under the rungs (T123 D30): it counts towards the number, not the level.</item>
/// <item><b>The minimum</b>, stepped where the training year changed (<see cref="EpaTrajectoryDto.MinimumSteps" />), is the
/// lower edge of its rung, and everything below it is shaded; the exit level is a line through its rung.</item>
/// <item><b>Points closer than a dot</b> are set a dot apart, in date order (R4); the table gives each one's date. A run
/// that would pass the plot's right edge is set back from it, so no point sits over "Exit 5" or "Minimum 5" (R6).</item>
/// </list>
/// </remarks>
public sealed class TrajectoryDrawing
{
    private static readonly string[] MonthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    /// <summary>A rung name longer than this is shortened on the axis; the table names every rating in full (T123 D29).</summary>
    private const int MaxAxisLabelChars = 4;

    /// <summary>
    /// About 14 months: a window longer than this names no month or semester, which would overlap at 13px (T355, build
    /// review G1). The EPA page's academic year, and a review of one semester or one year, stay within it.
    /// </summary>
    public const int LongWindowDays = 427;

    /// <summary>The space a label needs either side of its words before the next.</summary>
    private const double LabelGap = 6;

    private readonly TrajectoryFrame _frame;
    private readonly DateOnly _from;
    private readonly int _days;
    private readonly IReadOnlyList<TrajectoryRungDto> _rungs;
    private readonly AxisUnit _axisUnit;

    /// <summary>What the time axis names: each month (a window of about a year), each quarter, or each year (G1).</summary>
    public enum AxisUnit
    {
        Month,
        Quarter,
        Year
    }

    public TrajectoryDrawing(EpaTrajectoryDto trajectory, TrajectoryFrame frame, DateOnly? today)
    {
        ArgumentNullException.ThrowIfNull(trajectory);
        ArgumentNullException.ThrowIfNull(frame);

        _frame = frame;
        (_from, var to) = WindowOf(trajectory);
        _days = Math.Max(1, to.DayNumber - _from.DayNumber + 1);

        // The pinned ladder, highest rung first. With none (an unpinned item), the ratings' own ordinals stand in, so the
        // points still have somewhere to sit.
        _rungs = (trajectory.Rungs.Count > 0
                ? trajectory.Rungs
                : trajectory.Points.Where(point => !point.OffLadder)
                    .Select(point => new TrajectoryRungDto(point.Rating, point.RatingLabel))
                    .DistinctBy(rung => rung.Order))
            .OrderByDescending(rung => rung.Order)
            .ToList();

        RungsBottom = frame.Top + frame.RungHeight * _rungs.Count;
        LaneLineY = RungsBottom + 3;
        LaneY = RungsBottom + frame.LaneDrop;
        AxisY = RungsBottom + frame.AxisDrop;
        MonthY = AxisY + frame.MonthDrop;
        Height = MonthY + (frame.FontSize / 2);
        BandTop = frame.Top - 22;
        BandLabelY = frame.Top - 8;

        RungLines = Path(_rungs.Select(rung => $"M{frame.Left} {Fmt(Centre(rung.Order))}H{frame.Right}"));
        RungLabels = _rungs.Select(rung => new Label(frame.Left - 10, Centre(rung.Order) + (frame.FontSize * 0.35), AxisText(rung))).ToList();

        _axisUnit = AxisUnitFor(frame, _days);
        (Bands, SemesterLabels) = Semesters(to);
        (MonthTicks, MonthLabels) = Months(to);

        (MinimumLine, BelowShading, MinimumLabel) = Minimum(trajectory);
        if (trajectory.ExitLevelOrder is { } exit && IndexOf(exit) >= 0)
        {
            ExitY = Centre(exit);
            ExitLabel = new Label(frame.Right + 6, ExitY.Value + (frame.FontSize * 0.35), $"Exit {trajectory.ExitLevelLabel}");
        }

        if (today is { } day && day >= _from && day <= to)
        {
            TodayX = X(day);
        }

        (Dots, Hollow, Line) = Points(trajectory.Points);
    }

    public int Width => _frame.Width;

    public int Height { get; }

    public int Left => _frame.Left;

    public int Right => _frame.Right;

    public int Top => _frame.Top;

    public int RungsBottom { get; }

    public int LaneLineY { get; }

    public int LaneY { get; }

    public int AxisY { get; }

    public int MonthY { get; }

    public int BandTop { get; }

    public int BandLabelY { get; }

    /// <summary>The lane's name, under the rungs' names.</summary>
    public Label LaneLabel => new(_frame.Left - 10, LaneY + (_frame.FontSize * 0.35), _frame.LaneWord);

    /// <summary>One gridline through each rung's centre.</summary>
    public string RungLines { get; }

    public IReadOnlyList<Label> RungLabels { get; }

    /// <summary>Each Semester 2's band (July to December) the window holds, in order; none where it holds none (G1).</summary>
    public IReadOnlyList<Stripe> Bands { get; }

    /// <summary>
    /// The names above the plot, each at its start: every semester's in a window of about a year, each year's in a longer
    /// one whose axis names quarters, none where the axis itself names the years (G1).
    /// </summary>
    public IReadOnlyList<Label> SemesterLabels { get; }

    /// <summary>What the time axis names (G1).</summary>
    public AxisUnit Unit => _axisUnit;

    public string MonthTicks { get; }

    public IReadOnlyList<Label> MonthLabels { get; }

    /// <summary>The minimum's stepped edge; null when the curriculum holds no minimum for the EPA.</summary>
    public string? MinimumLine { get; }

    /// <summary>The area under <see cref="MinimumLine" />, closed along the lowest rung's edge.</summary>
    public string? BelowShading { get; }

    /// <summary>"Minimum 5" at the edge's last step, right of the plot.</summary>
    public Label? MinimumLabel { get; }

    public double? ExitY { get; }

    public Label? ExitLabel { get; }

    /// <summary>The "Today" rule's x, when today is inside the window and the page asked for it.</summary>
    public double? TodayX { get; }

    /// <summary>The ratings on the ladder, as one path of circles.</summary>
    public string? Dots { get; }

    /// <summary>The ratings on another ladder, as one path of rings in the lane.</summary>
    public string? Hollow { get; }

    /// <summary>The line through the ratings on the ladder, oldest first; null for fewer than two.</summary>
    public string? Line { get; }

    /// <summary>A text placed at (X, Y).</summary>
    public sealed record Label(double X, double Y, string Text)
    {
        public string Xs => Fmt(X);

        public string Ys => Fmt(Y);

        /// <summary>
        /// The label as an SVG <c>text</c> element, its words encoded: Razor reads a <c>&lt;text&gt;</c> tag in code as its
        /// own, and a rung's name is an administrator's free text (T123).
        /// </summary>
        public MarkupString Svg(string cssClass, string? anchor = null)
            => new($"<text class=\"{cssClass}\" x=\"{Xs}\" y=\"{Ys}\"{(anchor is null ? string.Empty : $" text-anchor=\"{anchor}\"")}>{WebUtility.HtmlEncode(Text)}</text>");
    }

    /// <summary>A stripe from X to X + Width.</summary>
    public sealed record Stripe(double X, double Width)
    {
        public string Xs => Fmt(X);

        public string Widths => Fmt(Width);
    }

    /// <summary>
    /// The window a trajectory is drawn over: the window it was read for (<see cref="EpaTrajectoryDto.WindowFrom" />, To),
    /// else the academic year of its first rating (<see cref="TrajectoryWindow" />), else this year's.
    /// </summary>
    public static (DateOnly From, DateOnly To) WindowOf(EpaTrajectoryDto trajectory)
    {
        ArgumentNullException.ThrowIfNull(trajectory);

        var anchor = trajectory.Points.Count > 0
            ? trajectory.Points.Min(point => point.ObservedOn)
            : trajectory.WindowFrom ?? trajectory.WindowTo ?? QuotaCalendar.Today();
        var year = TrajectoryWindow.AcademicYearOf(anchor);
        var from = trajectory.WindowFrom ?? year.From;
        var to = trajectory.WindowTo ?? year.To;
        return to < from ? (from, from) : (from, to);
    }

    /// <summary>Whether a point sits in the other scale's lane: recorded on another ladder, or on no rung of this one.</summary>
    public bool IsOffLadder(TrajectoryPointDto point) => point.OffLadder || IndexOf(point.Rating) < 0;

    public static string Fmt(double value) => Math.Round(value).ToString(CultureInfo.InvariantCulture);

    public double X(DateOnly day) => _frame.Left + ((_frame.Right - _frame.Left) * (double)(day.DayNumber - _from.DayNumber) / _days);

    private int IndexOf(int order)
    {
        for (var index = 0; index < _rungs.Count; index++)
        {
            if (_rungs[index].Order == order)
            {
                return index;
            }
        }

        return -1;
    }

    private double Centre(int order) => _frame.Top + (_frame.RungHeight * IndexOf(order)) + (_frame.RungHeight / 2.0);

    /// <summary>The lower edge of a rung's band: a rating at or above the rung sits above it.</summary>
    private double LowerEdge(int order) => _frame.Top + (_frame.RungHeight * (IndexOf(order) + 1));

    /// <summary>
    /// A rung's name inside the axis' budget. The ordinal stands in for a long name only where it is no other rung's name
    /// (on <c>1, 2, 3a, 3b, "4 (independent)", 5</c> the ordinal 5 is the next rung's name), else the name is cut (T123).
    /// </summary>
    private string AxisText(TrajectoryRungDto rung)
    {
        if (rung.Label.Length <= MaxAxisLabelChars)
        {
            return rung.Label;
        }

        var ordinal = rung.Order.ToString(CultureInfo.InvariantCulture);
        var collides = _rungs.Any(other => other.Order != rung.Order && string.Equals(other.Label, ordinal, StringComparison.Ordinal));
        return collides ? rung.Label[..Math.Max(1, MaxAxisLabelChars - 1)] + "…" : ordinal;
    }

    /// <summary>
    /// What the axis names in this frame (G1): months for a window of about a year; else quarters where a quarter's span
    /// holds its name ("Jan"), as at 900 over four years; else years, as at 326.
    /// </summary>
    public static AxisUnit AxisUnitFor(TrajectoryFrame frame, int days)
    {
        ArgumentNullException.ThrowIfNull(frame);
        if (days <= LongWindowDays)
        {
            return AxisUnit.Month;
        }

        var quarterWidth = (frame.Right - frame.Left) * 91.0 / days;
        return quarterWidth >= TextWidth(frame, "Jan") + (2 * LabelGap) ? AxisUnit.Quarter : AxisUnit.Year;
    }

    /// <summary>A label's width at the frame's size, near enough to keep two apart (the stylesheet sets the font).</summary>
    public static double TextWidth(TrajectoryFrame frame, string text)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(text);
        return text.Length * frame.FontSize * 0.6;
    }

    private (IReadOnlyList<Stripe> Bands, IReadOnlyList<Label> Labels) Semesters(DateOnly to)
    {
        var bands = new List<Stripe>();
        var labels = new List<Label>();
        var start = new DateOnly(_from.Year, _from.Month <= 6 ? 1 : 7, 1);
        while (start <= to)
        {
            var semester = start.Month == 1 ? 1 : 2;
            var next = start.AddMonths(6);
            var shownFrom = start < _from ? _from : start;
            var shownTo = next.AddDays(-1) > to ? to : next.AddDays(-1);
            if (_axisUnit == AxisUnit.Month)
            {
                labels.Add(new Label(X(shownFrom) + 4, BandLabelY, $"Semester {semester}, {start.Year}"));
            }

            // Every Semester 2 the window holds is banded, not only the first (G1).
            if (semester == 2)
            {
                var left = X(shownFrom);
                bands.Add(new Stripe(left, X(shownTo.AddDays(1)) - left));
            }

            start = next;
        }

        // A longer window whose axis names quarters names each year above the plot, where its span holds the name.
        if (_axisUnit == AxisUnit.Quarter)
        {
            for (var year = new DateOnly(_from.Year, 1, 1); year <= to; year = year.AddYears(1))
            {
                var shownFrom = year < _from ? _from : year;
                var shownTo = year.AddYears(1) > to ? to.AddDays(1) : year.AddYears(1);
                var text = year.Year.ToString(CultureInfo.InvariantCulture);
                if (X(shownTo) - X(shownFrom) >= TextWidth(_frame, text) + 4 + LabelGap)
                {
                    labels.Add(new Label(X(shownFrom) + 4, BandLabelY, text));
                }
            }
        }

        return (bands, labels);
    }

    private (string Ticks, IReadOnlyList<Label> Labels) Months(DateOnly to)
    {
        var ticks = new List<string>();
        var labels = new List<Label>();
        var (first, step) = _axisUnit switch
        {
            AxisUnit.Quarter => (new DateOnly(_from.Year, ((_from.Month - 1) / 3 * 3) + 1, 1), 3),
            AxisUnit.Year => (new DateOnly(_from.Year, 1, 1), 12),
            _ => (new DateOnly(_from.Year, _from.Month, 1), 1)
        };
        var month = first;
        var index = 0;
        while (month <= to)
        {
            var next = month.AddMonths(step);
            var left = month < _from ? _from : month;
            var right = next.AddDays(-1) > to ? to.AddDays(1) : next;
            ticks.Add($"M{Fmt(X(left))} {AxisY}V{AxisY + 5}");
            var text = _axisUnit == AxisUnit.Year
                ? month.Year.ToString(CultureInfo.InvariantCulture)
                : MonthNames[month.Month - 1];
            var named = _axisUnit == AxisUnit.Month
                ? !_frame.EveryOtherMonth || index % 2 == 0
                // A quarter or a year cut short by the window is named only where its span holds the name (G1).
                : X(right) - X(left) >= TextWidth(_frame, text) + LabelGap;
            if (named)
            {
                labels.Add(new Label((X(left) + X(right)) / 2, MonthY, text));
            }

            month = next;
            index++;
        }

        ticks.Add($"M{Fmt(X(to.AddDays(1)))} {AxisY}V{AxisY + 5}");
        return (string.Concat(ticks), labels);
    }

    private (string? Line, string? Shading, Label? Label) Minimum(EpaTrajectoryDto trajectory)
    {
        // A step whose level is no rung of the ladder drawn (a re-pinned scale) leaves the minimum undrawn, rather than the
        // previous step running on as if it still applied; the table and each rating's verdict name the real one (R5).
        var steps = trajectory.MinimumSteps.OrderBy(step => step.From).ToList();
        if (steps.Count == 0 || steps.Any(step => IndexOf(step.MinimumOrder) < 0))
        {
            return (null, null, null);
        }

        var path = new StringBuilder();
        for (var index = 0; index < steps.Count; index++)
        {
            var xa = steps[index].From <= _from ? _frame.Left : X(steps[index].From);
            var xb = index + 1 < steps.Count ? X(steps[index + 1].From) : _frame.Right;
            var y = Fmt(LowerEdge(steps[index].MinimumOrder));
            path.Append(index == 0 ? $"M{Fmt(xa)} {y}" : $"V{y}");
            path.Append($"H{Fmt(xb)}");
        }

        var line = path.ToString();
        var last = steps[^1];
        var label = new Label(_frame.Right + 6, LowerEdge(last.MinimumOrder) + (_frame.FontSize * 1.25), $"{_frame.MinimumWord} {last.MinimumLabel}");
        return (line, $"{line}V{RungsBottom}H{_frame.Left}Z", label);
    }

    private (string? Dots, string? Hollow, string? Line) Points(IReadOnlyList<TrajectoryPointDto> points)
    {
        var placed = new List<(double X, double Y, bool Off)>();
        var apart = (2 * _frame.Radius) + 3;
        var previous = double.MinValue;
        foreach (var point in points.OrderBy(point => point.ObservedOn).ThenBy(point => point.ActivityId))
        {
            // Closer than a dot to the one before, it is set a dot apart (R4): two encounters a day apart stay two points.
            var x = Math.Max(X(point.ObservedOn), previous + apart);
            previous = x;
            var off = IsOffLadder(point);
            placed.Add((x, off ? LaneY : Centre(point.Rating), off));
        }

        // A run pushed past the plot's right edge (many ratings in the window's last days) is set back from it, each still
        // a dot from the next, so none sits over the labels right of the plot (R6). Not past the left edge either.
        var limit = (double)_frame.Right;
        for (var index = placed.Count - 1; index >= 0; index--)
        {
            var x = Math.Max(_frame.Left, Math.Min(placed[index].X, limit));
            placed[index] = placed[index] with { X = x };
            limit = x - apart;
        }

        string? Circles(IEnumerable<(double X, double Y, bool Off)> at)
        {
            var r = _frame.Radius;
            var text = string.Concat(at.Select(p =>
                $"M{Fmt(p.X - r)} {Fmt(p.Y)}a{r} {r} 0 1 0 {2 * r} 0a{r} {r} 0 1 0 {-2 * r} 0"));
            return text.Length == 0 ? null : text;
        }

        var on = placed.Where(p => !p.Off).ToList();
        var line = on.Count > 1 ? "M" + string.Join("L", on.Select(p => $"{Fmt(p.X)} {Fmt(p.Y)}")) : null;
        return (Circles(on), Circles(placed.Where(p => p.Off)), line);
    }

    private static string Path(IEnumerable<string> parts) => string.Concat(parts);
}
