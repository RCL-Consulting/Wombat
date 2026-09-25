using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Web.Components.Pages.CommitteeDecisions;

/// <summary>
/// The words and badges of a review's agenda (T131 slice 4), shared by the review page and the scheduling form's preview
/// so the two say the same thing. Sentences are built here in C#, because Razor drops a space standing alone before an
/// expression and runs sentences together.
/// </summary>
public static class CommitteeAgendaText
{
    /// <summary>A line's status as the agenda table's badge says it.</summary>
    public static string StatusLabel(CommitteeAgendaLineStatus status) => status switch
    {
        CommitteeAgendaLineStatus.Due => "Due",
        CommitteeAgendaLineStatus.DueByYearEnd => "Due by year end",
        CommitteeAgendaLineStatus.PartialPeriod => "Partial period",
        CommitteeAgendaLineStatus.AsOpportunityAllows => "As opportunity allows",
        CommitteeAgendaLineStatus.Staged => "Staged",
        CommitteeAgendaLineStatus.Decided => "Decided",
        CommitteeAgendaLineStatus.Deferred => "Deferred",
        CommitteeAgendaLineStatus.NotDecided => "Not decided",
        CommitteeAgendaLineStatus.DecidedElsewhere => "Decided elsewhere",
        _ => status.ToString()
    };

    /// <summary>
    /// The badge a status wears (DESIGN.md § Badges, "Committee agenda"): the three shades of "still due" share the draft
    /// badge and are told apart by their label, and so do the two of "decided".
    /// </summary>
    public static string BadgeClass(CommitteeAgendaLineStatus status) => status switch
    {
        CommitteeAgendaLineStatus.Staged => "badge-submitted",
        CommitteeAgendaLineStatus.Decided => "badge-completed",
        CommitteeAgendaLineStatus.DecidedElsewhere => "badge-completed",
        CommitteeAgendaLineStatus.Deferred => "badge-accepted",
        CommitteeAgendaLineStatus.NotDecided => "badge-declined",
        _ => "badge-draft"
    };

    /// <summary>What a line's status means for this sitting, said under its badge; null where the badge says it all.</summary>
    /// <param name="reviewState">
    /// The review's state. A line decided elsewhere and still due is said to be open to staging only while the review is in
    /// progress, the one state in which a decision can be staged (T235).
    /// </param>
    public static string? StatusDetail(CommitteeAgendaLineDto line, CommitteeReviewState reviewState) => line.Status switch
    {
        CommitteeAgendaLineStatus.Due => "Must be decided at this sitting, or deferred with a reason.",
        CommitteeAgendaLineStatus.DueByYearEnd => "Optional here; decided by the year's last sitting.",
        CommitteeAgendaLineStatus.PartialPeriod => "The trainee joined part-way through the window: optional, never missed.",
        CommitteeAgendaLineStatus.AsOpportunityAllows => line.Origin == CommitteeAgendaLineOrigin.Chair
            ? "Added by the chair."
            : "Decided as opportunity allows: optional.",
        CommitteeAgendaLineStatus.Staged => "A decision is staged below.",
        CommitteeAgendaLineStatus.Deferred => line.DeferralReason is null ? null : $"Reason: {line.DeferralReason}",
        CommitteeAgendaLineStatus.Decided => line.EntrustmentDecisionId is int starId ? $"STAR #{starId}." : null,
        CommitteeAgendaLineStatus.NotDecided => "The review was ratified without deciding it.",
        CommitteeAgendaLineStatus.DecidedElsewhere => DecidedElsewhereDetail(line, reviewState),
        _ => null
    };

    /// <summary>
    /// A line another sitting has decided (T235). Once the review settled its agenda, the stored state says when. Still due,
    /// it is optional; only a review in progress takes a decision staged on it, and a decided one settles it at ratify.
    /// </summary>
    private static string DecidedElsewhereDetail(CommitteeAgendaLineDto line, CommitteeReviewState reviewState)
    {
        const string Optional = "Another sitting has decided it in this window, so it need not be decided here.";

        if (line.State != CommitteeAgendaLineState.Due)
        {
            return "Another sitting had decided it in this window when this review settled its agenda.";
        }

        return reviewState switch
        {
            CommitteeReviewState.InProgress => $"{Optional} A decision staged on it decides it again.",
            CommitteeReviewState.Decided => $"{Optional} Ratifying the review records that.",
            _ => Optional
        };
    }

    /// <summary>The opening of a ratified review's agenda on the trainee's own page (O6).</summary>
    public static string TraineeAgendaIntro(CommitteeAgendaDto agenda)
    {
        ArgumentNullException.ThrowIfNull(agenda);
        return $"The EPAs this review was there to decide for {agenda.PeriodLabel}, and what the committee did with each.";
    }

    /// <summary>
    /// What a line's outcome means, said to the trainee under its badge: the committee's reason for a deferral (O6), the
    /// STAR a decided line names. Null where the badge says it all.
    /// </summary>
    public static string? TraineeStatusDetail(CommitteeAgendaLineDto line)
    {
        ArgumentNullException.ThrowIfNull(line);
        return line.Status switch
        {
            CommitteeAgendaLineStatus.Deferred => line.DeferralReason is null ? null : $"The committee's reason: {line.DeferralReason}",
            CommitteeAgendaLineStatus.Decided => line.EntrustmentDecisionId is int starId ? $"STAR #{starId}." : null,
            CommitteeAgendaLineStatus.NotDecided => "Not decided at this review.",
            CommitteeAgendaLineStatus.DecidedElsewhere => "Decided at another sitting in this window.",
            _ => null
        };
    }

    /// <summary>Where another panel's decision stands, as the line after its panel's name says it.</summary>
    public static string ElsewhereLabel(CommitteeAgendaElsewhereStatus status) => status switch
    {
        CommitteeAgendaElsewhereStatus.Decided => "Decided",
        CommitteeAgendaElsewhereStatus.OnAgenda => "On the agenda of an open review",
        CommitteeAgendaElsewhereStatus.Deferred => "Deferred",
        CommitteeAgendaElsewhereStatus.Missed => "Missed",
        _ => "Not yet decided"
    };

    /// <summary>The badge another panel's decision wears; "Missed" is computed, never stored, and wears the declined badge.</summary>
    public static string ElsewhereBadgeClass(CommitteeAgendaElsewhereStatus status) => status switch
    {
        CommitteeAgendaElsewhereStatus.Decided => "badge-completed",
        CommitteeAgendaElsewhereStatus.OnAgenda => "badge-submitted",
        CommitteeAgendaElsewhereStatus.Deferred => "badge-accepted",
        CommitteeAgendaElsewhereStatus.Missed => "badge-declined",
        _ => "badge-draft"
    };

    /// <summary>"PAED-004", "PAED-004 and PAED-005", "PAED-001, PAED-002 and PAED-004".</summary>
    public static string Codes(IEnumerable<string> codes)
    {
        ArgumentNullException.ThrowIfNull(codes);

        var list = codes.ToArray();
        return list.Length switch
        {
            0 => string.Empty,
            1 => list[0],
            _ => string.Join(", ", list.Take(list.Length - 1)) + " and " + list[^1]
        };
    }

    /// <summary>
    /// The warning on the progression decision while another panel still owes a decision in the period (O8: warn, never
    /// refuse, when the general sitting comes first). Null when nothing is outstanding.
    /// </summary>
    public static string? SittingOrderWarning(CommitteeAgendaDto? agenda)
    {
        if (agenda is null)
        {
            return null;
        }

        var outstanding = agenda.UndecidedElsewhere;
        if (outstanding.Count == 0)
        {
            return null;
        }

        var panels = Codes(outstanding.Select(line => line.PanelName).Distinct(StringComparer.Ordinal));
        return $"{Codes(outstanding.Select(line => line.EpaCode))} {(outstanding.Count == 1 ? "is" : "are")} decided by " +
               $"{panels} and not yet decided for {agenda.PeriodLabel}. A progression decision recorded now is taken " +
               $"without {(outstanding.Count == 1 ? "it" : "them")}.";
    }

    /// <summary>The scheduling preview's opening: how many EPAs, and which must be decided or deferred.</summary>
    public static IReadOnlyList<string> PreviewSentences(CommitteeAgendaPreviewDto preview)
    {
        ArgumentNullException.ThrowIfNull(preview);

        if (!preview.TraineeHasCurriculum)
        {
            return ["This trainee follows no curriculum, so the review will have no agenda."];
        }

        // T131 slice 5: the scheduling handler refuses an entrustment-only review here, with the same reason.
        if (!preview.PanelDecidesAnything)
        {
            return ["This panel decides no EPA on this trainee's curriculum, so an entrustment-only review before it would have nothing to decide."];
        }

        // T215: where a STAR already decided every EPA due, the live sentence says so itself. The note naming them sits
        // outside the live region, and "none is due" would read as if the period asked nothing of this panel.
        if (preview.Lines.Count == 0)
        {
            return preview.DecidedInWindow.Count > 0
                ? [$"Every EPA this panel decides that is due for {preview.PeriodLabel} is already decided in its window, so the review will have nothing on its agenda."]
                : [$"No EPA this panel decides is due for {preview.PeriodLabel}."];
        }

        var sentences = new List<string>
        {
            $"{preview.Lines.Count} EPA{(preview.Lines.Count == 1 ? string.Empty : "s")} will be on the agenda for {preview.PeriodLabel}."
        };

        var closing = preview.Lines.Where(line => line.IsClosing).Select(line => line.EpaCode).ToArray();
        if (closing.Length > 0)
        {
            sentences.Add($"{Codes(closing)} must be decided at this sitting, or deferred with a reason, before it is ratified.");
        }

        return sentences;
    }

    /// <summary>The scheduling preview's notes: what another panel decides, and what is already decided in the window.</summary>
    public static IReadOnlyList<string> PreviewNotes(CommitteeAgendaPreviewDto preview)
    {
        ArgumentNullException.ThrowIfNull(preview);

        var notes = preview.RoutedElsewhere
            .GroupBy(line => line.PanelName, StringComparer.Ordinal)
            .Select(group => $"{Codes(group.Select(line => line.EpaCode))} {(group.Count() == 1 ? "is" : "are")} decided by " +
                             $"{group.Key}: schedule {(group.Count() == 1 ? "it" : "them")} separately.")
            .ToList();

        if (DecidedInWindowNote(preview.DecidedInWindow) is { } decided)
        {
            notes.Add(decided);
        }

        return notes;
    }

    /// <summary>
    /// The EPAs routed to the panel that a STAR already decided in their window, so the planner left them off the agenda
    /// (T215): said in the same words on the scheduling preview and the review's agenda card. Null when there are none.
    /// </summary>
    public static string? DecidedInWindowNote(IReadOnlyList<string> codes)
    {
        ArgumentNullException.ThrowIfNull(codes);
        return codes.Count == 0 ? null : $"Already decided in this window, so not on the agenda: {Codes(codes)}.";
    }

    /// <summary>
    /// The EPAs routed to the panel whose window lost its decision while the review sat, and which its agenda does not hold
    /// (T235): an EPA Start left off because a STAR decided it, whose STAR has since been revoked. One sentence, in the
    /// agenda card, so the chair can stage it. Null when there are none.
    /// </summary>
    public static string? NoLongerDecidedNote(IReadOnlyList<string> codes)
    {
        ArgumentNullException.ThrowIfNull(codes);

        return codes.Count switch
        {
            0 => null,
            1 => $"{codes[0]} is no longer decided in its window: the STAR that decided it has been revoked. It is not on " +
                 "this agenda; stage a decision on it to decide it at this sitting.",
            _ => $"{Codes(codes)} are no longer decided in their windows: the STARs that decided them have been revoked. " +
                 "They are not on this agenda; stage a decision on each to decide it at this sitting."
        };
    }
}
