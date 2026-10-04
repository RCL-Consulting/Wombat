using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.Epas;

namespace Wombat.Web.Components.Shared.Progress;

/// <summary>
/// The words of one EPA's page under My progress (T355; R2): its h1, tab and subtitle, the level line, the Entrustment
/// lines, the empties, and the page's loading and error words. Counts are <see cref="ProgressWords" />'s, so the page and
/// My progress's row say a count alike.
/// </summary>
/// <remarks>Dates ISO (T325; decision D1). Straight apostrophes (C11).</remarks>
public static class EpaPageWords
{
    /// <summary>The Entrustment section's empty: "No STAR yet."</summary>
    public const string NoStar = "No STAR yet.";

    /// <summary>Activities on this EPA's empty, beside "Log an activity".</summary>
    public const string NoActivity = "No activity on this EPA yet.";

    /// <summary>The h1 while the page loads (C8): the h1 FocusOnNavigate focused is then the loaded page's.</summary>
    public const string Loading = "Loading this EPA";

    /// <summary>The always-present status's words while the page loads (C8; Spec § 5).</summary>
    public const string LoadingStatus = "Loading this EPA.";

    /// <summary>The h1 and the crumb when the page's read failed.</summary>
    public const string ErrorHeading = "EPA";

    /// <summary>The page's load error, in fixed words: never the exception's text (T329, T272).</summary>
    public const string LoadError = "Could not load this EPA. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>
    /// The activities table's caption. Not "every" activity: the list is its first page of the largest size (T355, build
    /// review R1, D4).
    /// </summary>
    public const string ActivitiesCaption = "Your finished activities on this EPA, newest encounter first";

    /// <summary>
    /// "The newest 100 of 130 are listed.", under the table when the list stops short of the total; null when it lists
    /// them all (T355, build review R1, D4).
    /// </summary>
    public static string? Truncated(int shown, int total)
        => total > shown ? $"The newest {shown} of {total} are listed." : null;

    /// <summary>"Not in the College's exit rule.": a local item's Entrustment line and subtitle segment.</summary>
    public const string NotInExitRule = "Not in the College's exit rule.";

    /// <summary>The h1's words: "PAED-012 — Communicating with …", the EPA picker's own text (<see cref="EpaOptionLabel" />).</summary>
    public static string Title(string code, string title) => EpaOptionLabel.For(code, title, inForce: true);

    /// <summary>
    /// The browser tab: the h1's words, with " (no longer in use)" when the EPA is paused, then " · Wombat" (C5). The crumb
    /// stays the code.
    /// </summary>
    public static string Tab(string code, string title, bool inForce) => $"{EpaOptionLabel.For(code, title, inForce)} · Wombat";

    /// <summary>
    /// The line under the h1: the target, the cadence and the exit level, "3 a semester · Decided each semester · Exit
    /// level 5"; a local item "1 a year · Not in the College's exit rule" (its exit level is no part of the College's rule).
    /// </summary>
    public static string Subtitle(TraineeCurriculumProgressDto item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var parts = new List<string> { $"{item.Target} a {(item.IsPerSemester ? "semester" : "year")}" };
        if (ProgressWords.Cadence(item) is { } cadence)
        {
            parts.Add(cadence);
        }

        if (item.IsLocal)
        {
            parts.Add(NotInExitRule.TrimEnd('.'));
        }
        else if (item.ExitLevelLabel is { } exit)
        {
            parts.Add($"Exit level {exit}");
        }

        return string.Join(" · ", parts);
    }

    /// <summary>
    /// The level, once per EPA, naming its training year (R3): "Training year 4: level 5, the minimum each encounter is
    /// judged against and your STAR's target." Where the curriculum names no level for the year (KGK-001 has no per-year
    /// map; <see cref="TraineeCurriculumProgressDto.MinimumByTrainingYear" />), or before the programme starts, the item's
    /// own minimum: "Minimum 3a" (C4).
    /// </summary>
    public static string LevelLine(TraineeCurriculumProgressDto item, int? stage)
    {
        ArgumentNullException.ThrowIfNull(item);
        return stage is { } year && item.MinimumByTrainingYear
            ? $"Training year {year}: level {item.EffectiveMinimumLevelLabel}, the minimum each encounter is judged against and your STAR's target."
            : $"Minimum {item.EffectiveMinimumLevelLabel}";
    }

    /// <summary>"Exit level 5 · reached", "Exit level 5 · not yet", or "· not comparable" for a STAR on another ladder.</summary>
    public static string ExitLine(EpaStandingDto epa)
    {
        ArgumentNullException.ThrowIfNull(epa);
        var status = epa.ExitStatus switch
        {
            EntrustmentStandingStatus.AtOrAbove => "reached",
            EntrustmentStandingStatus.NotComparable => "not comparable",
            _ => "not yet"
        };
        return $"Exit level {epa.ExitLevelLabel} · {status}";
    }

    /// <summary>"Issued 2026-10-03", and ", expires 2026-10-23" when it does (C4: no "No expiry").</summary>
    public static string Issued(StandingDecisionDto decision)
    {
        ArgumentNullException.ThrowIfNull(decision);
        return decision.ExpiresOn is { } expires
            ? $"Issued {QuotaText.Iso(decision.IssuedOn)}, expires {QuotaText.Iso(expires)}"
            : $"Issued {QuotaText.Iso(decision.IssuedOn)}";
    }

    /// <summary>The Entrustment section of a paused EPA: "While PAED-012 is paused it is not in your standing against Annexure A."</summary>
    public static string PausedEntrustment(string code) => $"While {code} is paused it is not in your standing against Annexure A.";
}
