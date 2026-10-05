namespace Wombat.Application.Common.Options;

public sealed class DashboardThresholds
{
    public const string SectionName = "DashboardThresholds";

    /// <summary>
    /// How long a request may wait on its assessor before it is overdue (T350, round 2 E1): the Overdue badge, the rule
    /// line's "Overdue once it has waited 7 days.", and since T358 the Coordinator's stalled read too (review 11).
    /// </summary>
    public int AssessorDueDays { get; set; } = 7;

    /// <summary>
    /// How long a request waits on its assessor before the nightly nudge emails them (<c>AssessorPendingNudgeJob</c>): the
    /// job's cutoff and its description, the account page's help text and the rule line's "Its assessor is emailed after
    /// 5." all read this one number (T358, review 11; round 3 item 11), where three of them used to write 5 by hand.
    /// </summary>
    public int AssessorNudgeDays { get; set; } = 5;
}
