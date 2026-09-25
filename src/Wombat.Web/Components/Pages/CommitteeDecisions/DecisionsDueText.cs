using Wombat.Application.Features.CommitteeDecisions;

namespace Wombat.Web.Components.Pages.CommitteeDecisions;

/// <summary>
/// The words, badges and links of the decisions-due page (T131 slice 6). Sentences are built here in C#, because Razor
/// drops a space standing alone before an expression and runs sentences together.
/// </summary>
public static class DecisionsDueText
{
    /// <summary>The status filter's options, what still needs someone to act first and what is settled last.</summary>
    public static IReadOnlyList<EntrustmentDecisionDueStatus> FilterOrder { get; } =
    [
        EntrustmentDecisionDueStatus.Missed,
        EntrustmentDecisionDueStatus.NotScheduled,
        EntrustmentDecisionDueStatus.Revoked,
        EntrustmentDecisionDueStatus.DueByYearEnd,
        EntrustmentDecisionDueStatus.Deferred,
        EntrustmentDecisionDueStatus.Scheduled,
        EntrustmentDecisionDueStatus.PartialPeriod,
        EntrustmentDecisionDueStatus.AsOpportunityAllows,
        EntrustmentDecisionDueStatus.Decided
    ];

    /// <summary>A status as its badge says it.</summary>
    public static string StatusLabel(EntrustmentDecisionDueStatus status) => status switch
    {
        EntrustmentDecisionDueStatus.Decided => "Decided",
        EntrustmentDecisionDueStatus.Scheduled => "Scheduled",
        EntrustmentDecisionDueStatus.Deferred => "Deferred",
        EntrustmentDecisionDueStatus.Revoked => "Revoked: re-decide",
        EntrustmentDecisionDueStatus.DueByYearEnd => "Due by year end",
        EntrustmentDecisionDueStatus.PartialPeriod => "Partial period",
        EntrustmentDecisionDueStatus.AsOpportunityAllows => "As opportunity allows",
        EntrustmentDecisionDueStatus.NotScheduled => "Not scheduled",
        EntrustmentDecisionDueStatus.Missed => "Missed",
        _ => status.ToString()
    };

    /// <summary>What a status means for the trainee, said under its badge.</summary>
    public static string Detail(EntrustmentDecisionDueDto item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var star = item.EntrustmentDecisionId is int starId ? $"STAR #{starId}" : "The decision";
        return item.Status switch
        {
            EntrustmentDecisionDueStatus.Decided => item.ReviewId is int decidedAt
                ? $"{star}, issued at review #{decidedAt}."
                : $"{star} stands for {item.WindowLabel}.",
            EntrustmentDecisionDueStatus.Scheduled => $"On the agenda of review #{item.ReviewId}, which is still open.",
            EntrustmentDecisionDueStatus.Deferred => item.HoldingReviewId is int holding && holding == item.ReviewId
                ? $"Deferred at review #{item.ReviewId}, which is still open: its chair can reinstate it there."
                : $"Deferred at review #{item.ReviewId}. A later sitting for {item.WindowLabel} plans it again." +
                  HoldingSentence(item),
            EntrustmentDecisionDueStatus.Revoked => item.HoldingReviewId is null
                ? $"{star}, issued for {item.WindowLabel}, was revoked. Schedule a review to decide it again."
                : $"{star}, issued for {item.WindowLabel}, was revoked." + HoldingSentence(item),
            EntrustmentDecisionDueStatus.DueByYearEnd =>
                $"Decided once a year: due by the last sitting of {item.WindowLabel}, and not missed before the year ends." +
                HoldingSentence(item),
            EntrustmentDecisionDueStatus.PartialPeriod =>
                "The trainee joined the window part-way through: optional, never missed." + HoldingSentence(item),
            EntrustmentDecisionDueStatus.AsOpportunityAllows =>
                "Decided as opportunity allows: optional, never missed." + HoldingSentence(item),
            EntrustmentDecisionDueStatus.NotScheduled => item.HoldingReviewId is null
                ? $"No open review holds it, and {item.WindowLabel} has not ended."
                : $"{item.WindowLabel} has not ended, and no open review's agenda holds it." + HoldingSentence(item),
            EntrustmentDecisionDueStatus.Missed =>
                $"{item.WindowLabel} has ended with nothing decided, deferred or on an open review's agenda." +
                HoldingSentence(item),
            _ => string.Empty
        };
    }

    /// <summary>
    /// Where a row whose seat an open review already holds is to be decided, in the scheduling refusal's terms: a second
    /// binding review for the period would be refused, so the one open is where it goes. Empty where none holds it.
    /// </summary>
    private static string HoldingSentence(EntrustmentDecisionDueDto item)
        => item.HoldingReviewId is int holding
            ? $" Review #{holding} is open for {item.SchedulePeriodLabel} before {item.SchedulePanelName}: decide it there, " +
              "or ratify that review before scheduling another."
            : string.Empty;

    /// <summary>The opening sentence: whose decisions, where, for which period, and what "missed" means.</summary>
    public static string Intro(EntrustmentDecisionsDueDto result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var trainees = result.TraineeCount == 1 ? "1 trainee" : $"{result.TraineeCount} trainees";
        return $"The EPAs due for an entrustment decision in {result.PeriodLabel} for the {trainees} you oversee at " +
               $"{result.InstitutionName}, by each EPA's decision cadence: every semester, or once a year. " +
               "Missed is read today: the EPA's window has ended with nothing decided, deferred or on an open review's agenda.";
    }

    /// <summary>
    /// The link that opens the scheduling form filled with the trainee, the panel the EPA routes to and the period a
    /// sitting that decides the window sits for (for an annual EPA, the year's semester 2). Null when the caller may
    /// schedule the trainee before no panel that decides the EPA.
    /// </summary>
    public static string? ScheduleHref(EntrustmentDecisionDueDto item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return item.SchedulePanelId is int panelId
            ? $"/committee/reviews?panel={panelId}&trainee={Uri.EscapeDataString(item.TraineeUserId)}" +
              $"&period={Uri.EscapeDataString(item.SchedulePeriodKey)}"
            : null;
    }

    /// <summary>
    /// Whether the row still wants a sitting: nothing decides it and no open review's agenda holds it. Such a row offers
    /// Schedule, or names the open review that already holds its seat (<see cref="OffersSchedule" />).
    /// </summary>
    public static bool WantsASitting(EntrustmentDecisionDueDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return item.Status is not (EntrustmentDecisionDueStatus.Decided or EntrustmentDecisionDueStatus.Scheduled);
    }

    /// <summary>
    /// Whether the page offers Schedule on this row: it wants a sitting, and no open binding review already holds the
    /// trainee's seat for the period, where the scheduling handler would refuse a second.
    /// </summary>
    public static bool OffersSchedule(EntrustmentDecisionDueDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return WantsASitting(item) && item.HoldingReviewId is null;
    }
}
