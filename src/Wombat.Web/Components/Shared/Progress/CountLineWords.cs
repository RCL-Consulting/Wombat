using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Curricula;

namespace Wombat.Web.Components.Shared.Progress;

/// <summary>
/// The count a decision made, in one rule for Home's Recent decisions row and the completed card's sentence (T355, E5, E6;
/// note 2): the activity's own window, named when it is not the current one, so a Semester 1 encounter completed in
/// Semester 2 reads the same on both ("PAED-001, Semester 1, 2026: 3 of 3, met.").
/// </summary>
/// <remarks>
/// A figure is a count against a target for a named window (R3): never "n / m", a percentage or a lifetime total. A window
/// that holds no target (the programme started or ended part-way through it, D14, D49) shows its count and no fraction.
/// </remarks>
public static class CountLineWords
{
    /// <summary>
    /// "PAED-001: 1 of 3 this semester." / "PAED-001: 3 of 3 this semester, met."; a yearly item "PAED-008: 1 of 1 in 2026,
    /// met."; an older window "PAED-001, Semester 1, 2026: 3 of 3, met." / "…: 1 of 3, 2 short."; a window with no target
    /// "PAED-002: no target this semester · 2 recorded." / "PAED-002, Semester 1, 2026: no target · 2 recorded.".
    /// </summary>
    public static string Line(EpaCountLineDto line)
    {
        ArgumentNullException.ThrowIfNull(line);

        var window = line.Window;
        if (line.IsCurrentWindow)
        {
            var when = line.QuotaPeriod == QuotaPeriod.Semester ? "this semester" : $"in {window.Start.Year}";
            if (!window.Applies)
            {
                return $"{line.EpaCode}: no target {when} · {window.Count} recorded.";
            }

            return $"{line.EpaCode}: {window.Count} of {window.Target} {when}{(window.IsMet ? ", met" : string.Empty)}.";
        }

        if (!window.Applies)
        {
            return $"{line.EpaCode}, {window.Name}: no target · {window.Count} recorded.";
        }

        // A window that is over: met, or how short it fell, as My progress's previous line words it (PreviousLine).
        return window.IsMet
            ? $"{line.EpaCode}, {window.Name}: {window.Count} of {window.Target}, met."
            : $"{line.EpaCode}, {window.Name}: {window.Count} of {window.Target}, {window.Shortfall} short.";
    }

    /// <summary>
    /// The line under a Recent decisions row (E6): null for a declined request (its row has "File it again, to someone
    /// else"; <see cref="ActivitySummaryDto.Declined" />, by the pinned workflow, not a state key: build review R4) and for
    /// an activity about no EPA; "Its credit to PAED-012 waits while the EPA is paused." when the EPA is paused AND it
    /// credited nothing AND its type can credit (note 2: a completion credited before the pause does not wait; R4: a
    /// reflection stamped with an EPA credits nothing by design, and waits for nothing); "Credits nothing." wherever it
    /// credited nothing; else the count line, or null when the count read gave none.
    /// </summary>
    public static string? ForDecision(ActivitySummaryDto item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.Declined || item.EpaId is null)
        {
            return null;
        }

        var creditedNothing = item.CreditedItemCount is null or 0;
        if (creditedNothing && item.CanCredit && item.EpaInForce == false && item.EpaCode is { } code)
        {
            return $"Its credit to {code} waits while the EPA is paused.";
        }

        if (creditedNothing)
        {
            return "Credits nothing.";
        }

        return item.CountLine is { } line ? Line(line) : null;
    }

    /// <summary>
    /// The completed card's count (C5; E5), appended after the built credit sentence ("Rated 4. Credited 1 item to
    /// PAED-001. PAED-001: 1 of 3 this semester."): <see cref="Line" />, or null when the count read gave none.
    /// </summary>
    public static string? ForCard(EpaCountLineDto? line) => line is null ? null : Line(line);
}
