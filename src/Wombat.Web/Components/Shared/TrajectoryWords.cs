using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Application.Features.Curricula.Quota;

namespace Wombat.Web.Components.Shared;

/// <summary>
/// The words of a rating trajectory (T355; Q5, R4, C11): the summary under its heading, the region's and the drawing's
/// names, the key, the empty, and the table's Rating and "Against the minimum then" cells. One class for the EPA page and
/// the committee's review page, so the chart is said one way on both.
/// </summary>
/// <remarks>
/// Multi-source feedback is never plotted or counted here (D8, D36). Dates ISO (T325; decision D1). Built as whole
/// strings, because Razor drops a space standing alone before an expression.
/// </remarks>
public static class TrajectoryWords
{
    /// <summary>Said under a chart whose EPA has MSF beside it: MSF is not a rating (D8, D36).</summary>
    public const string NotPlotted = "Multi-source feedback is not plotted.";

    /// <summary>The key's name (the list's <c>aria-label</c>).</summary>
    public const string Key = "How to read the chart";

    /// <summary>The key's line for a point.</summary>
    public const string KeyRating = "A rating";

    /// <summary>The key's line for a hollow point in the other scale's lane.</summary>
    public const string KeyOtherScale = "Rated on another scale: counts towards the number, not the level";

    /// <summary>A trajectory with no rating in its window (the built chart's empty was "No observations to plot.").</summary>
    public const string NoRating = "No rating yet.";

    /// <summary>
    /// The line under the chart's heading (C11): "3 ratings in the 2026 academic year, from Thandi Zulu, David Naidoo and
    /// Mohammed Patel. 2 at the minimum, 1 below."; one rating only, "1 rating so far, from David Naidoo. At the minimum.";
    /// on the committee's page, the review's window and the trainee's name, "Lerato Molefe · 3 ratings in the review
    /// window, 2026-01-01 to 2026-12-31, from …".
    /// </summary>
    /// <remarks>
    /// The assessors are named in the order of their first rating, oldest first, as the table lists them; more than three
    /// are counted ("from 4 assessors"), never "distinct assessors". The verdicts are each rating's
    /// <see cref="TrajectoryPointDto.AgainstMinimum" />: "at the minimum" (at or above the training year's minimum when it
    /// was observed), "on another scale", "below", "with no minimum".
    /// </remarks>
    /// <param name="reviewWindow">Whether the chart is drawn over a committee review's window (D2): its window is named.</param>
    /// <param name="subjectName">The trainee, named on the committee's page; null on their own.</param>
    public static string Summary(EpaTrajectoryDto trajectory, bool reviewWindow, string? subjectName)
    {
        ArgumentNullException.ThrowIfNull(trajectory);

        var points = trajectory.Points;
        var count = points.Count;
        var ratings = count == 1 ? "1 rating" : $"{count} ratings";

        string span;
        if (reviewWindow)
        {
            span = trajectory.WindowFrom is { } from && trajectory.WindowTo is { } to
                ? $"{ratings} in the review window, {QuotaText.Iso(from)} to {QuotaText.Iso(to)}"
                : $"{ratings} in the review window";
        }
        else if (count == 1)
        {
            span = "1 rating so far";
        }
        else
        {
            span = trajectory.WindowFrom is { } from
                ? $"{ratings} in the {from.Year} academic year"
                : ratings;
        }

        var lead = subjectName is null ? span : $"{subjectName} · {span}";
        var assessors = From(points);
        var first = assessors is null ? $"{lead}." : $"{lead}, {assessors}.";

        return count == 0 ? first : $"{first} {Verdicts(points)}";
    }

    /// <summary>The chart's region, a named focusable scroll area (E1): "Rating chart for PAED-001".</summary>
    public static string RegionName(string epaCode) => $"Rating chart for {epaCode}";

    /// <summary>The drawing's name, for what it adds to the table: "Chart of the 3 ratings in the table below."</summary>
    public static string SvgName(int count)
        => $"Chart of the {(count == 1 ? "1 rating" : $"{count} ratings")} in the table below.";

    /// <summary>
    /// The key's line for the shaded area, from the stepped minimum: "Below the minimum: 4 until 2026-01-13, then 5
    /// (training year 4)"; one level across the window, "Below the minimum: 5 (training year 4)". Null when the trainee's
    /// curriculum holds no minimum for the EPA (no step).
    /// </summary>
    public static string? KeyBelow(EpaTrajectoryDto trajectory)
    {
        ArgumentNullException.ThrowIfNull(trajectory);

        // A step that keeps the level (a new training year, the same minimum) does not change the edge.
        var runs = new List<(TrajectoryMinimumStepDto First, TrajectoryMinimumStepDto Last)>();
        foreach (var step in trajectory.MinimumSteps)
        {
            if (runs.Count > 0 && runs[^1].Last.MinimumLabel == step.MinimumLabel)
            {
                runs[^1] = (runs[^1].First, step);
            }
            else
            {
                runs.Add((step, step));
            }
        }

        if (runs.Count == 0)
        {
            return null;
        }

        var parts = new List<string>();
        for (var index = 0; index < runs.Count - 1; index++)
        {
            parts.Add($"{runs[index].First.MinimumLabel} until {QuotaText.Iso(runs[index + 1].First.From.AddDays(-1))}");
        }

        var last = runs[^1].Last;
        parts.Add(last.TrainingYear is { } year ? $"{last.MinimumLabel} (training year {year})" : last.MinimumLabel);

        return $"Below the minimum: {string.Join(", then ", parts)}";
    }

    /// <summary>The key's line for the exit level: "Exit level 5". Null when the curriculum holds none for the EPA.</summary>
    public static string? KeyExit(EpaTrajectoryDto trajectory)
    {
        ArgumentNullException.ThrowIfNull(trajectory);
        return trajectory.ExitLevelLabel is { } exit ? $"Exit level {exit}" : null;
    }

    /// <summary>
    /// The table's Rating cell: the rung on the EPA's ladder, "4"; for a rating on another ladder, its own rung and that
    /// ladder, "Independent on O-R Scale" (C11's "[rating] on [scale]", filled from the activity).
    /// </summary>
    public static string RatingCell(TrajectoryPointDto point)
    {
        ArgumentNullException.ThrowIfNull(point);
        return point.OffLadder && point.OtherScaleName is { } scale
            ? $"{point.OtherScaleRatingLabel ?? point.RatingLabel} on {scale}"
            : point.RatingLabel;
    }

    /// <summary>
    /// The table's "Against the minimum then" cell (E2; note 14): "At or above (5, training year 4)", "Below (5, training
    /// year 4)", "Not on the ladder: counts towards the number, not the level", or "No minimum". "At or above", the
    /// panel's words, since it covers a rating above the minimum as well as one at it.
    /// </summary>
    public static string AgainstMinimum(TrajectoryPointDto point)
    {
        ArgumentNullException.ThrowIfNull(point);

        var then = point.TrainingYear is { } year
            ? $"({point.MinimumLabel}, training year {year})"
            : $"({point.MinimumLabel})";

        return point.AgainstMinimum switch
        {
            TrajectoryAgainstMinimum.AtOrAbove => $"At or above {then}",
            TrajectoryAgainstMinimum.Below => $"Below {then}",
            TrajectoryAgainstMinimum.NotComparable => "Not on the ladder: counts towards the number, not the level",
            _ => "No minimum"
        };
    }

    /// <summary>"from Thandi Zulu, David Naidoo and Mohammed Patel", or "from 4 assessors"; null with no rating.</summary>
    private static string? From(IReadOnlyList<TrajectoryPointDto> points)
    {
        var names = points
            .OrderBy(point => point.ObservedOn)
            .ThenBy(point => point.ActivityId)
            .DistinctBy(point => point.AssessorUserId, StringComparer.Ordinal)
            .Select(point => string.IsNullOrWhiteSpace(point.AssessorName) ? point.AssessorUserId : point.AssessorName)
            .ToList();

        return names.Count switch
        {
            0 => null,
            1 => $"from {names[0]}",
            <= 3 => $"from {string.Join(", ", names.Take(names.Count - 1))} and {names[^1]}",
            _ => $"from {names.Count} assessors"
        };
    }

    /// <summary>"2 at the minimum, 1 below."; one rating, "At the minimum." or "Below the minimum.".</summary>
    private static string Verdicts(IReadOnlyList<TrajectoryPointDto> points)
    {
        if (points.Count == 1)
        {
            return points[0].AgainstMinimum switch
            {
                TrajectoryAgainstMinimum.AtOrAbove => "At the minimum.",
                TrajectoryAgainstMinimum.Below => "Below the minimum.",
                TrajectoryAgainstMinimum.NotComparable => "On another scale.",
                _ => "No minimum applies."
            };
        }

        int Count(TrajectoryAgainstMinimum verdict) => points.Count(point => point.AgainstMinimum == verdict);

        var parts = new List<string>();
        void Add(int count, string words)
        {
            if (count > 0)
            {
                parts.Add($"{count} {words}");
            }
        }

        Add(Count(TrajectoryAgainstMinimum.AtOrAbove), "at the minimum");
        Add(Count(TrajectoryAgainstMinimum.NotComparable), "on another scale");
        Add(Count(TrajectoryAgainstMinimum.Below), "below");
        Add(Count(TrajectoryAgainstMinimum.NotGated), "with no minimum");

        return $"{string.Join(", ", parts)}.";
    }
}
