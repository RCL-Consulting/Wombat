namespace Wombat.Domain.Activities;

public sealed class ActivityTransition
{
    public int Id { get; set; }
    public int ActivityId { get; set; }
    public string FromState { get; set; } = string.Empty;
    public string ToState { get; set; } = string.Empty;
    public string TransitionKey { get; set; } = string.Empty;
    public string ActorUserId { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; }
    public string? Note { get; set; }
    public string SnapshotJson { get; set; } = "{}";

    /// <summary>
    /// How many curriculum items this transition credited, or <c>null</c> when credit was never
    /// evaluated for it (T108).
    /// </summary>
    /// <remarks>
    /// Three-valued on purpose:
    /// <list type="bullet">
    ///   <item><c>null</c> — credit was not evaluated. A non-terminal transition, or a pinned
    ///   activity-type version whose <c>counts_for</c> is empty (a reflective note, journal club,
    ///   procedure log, QI project, research output, teaching session). These can never be flagged
    ///   as uncredited because they were never meant to credit anything.</item>
    ///   <item><c>0</c> — credit WAS evaluated and matched nothing. This is the T108 signal: the
    ///   encounter is recorded, the assessor is done, and not one curriculum requirement moved.</item>
    ///   <item><c>&gt; 0</c> — that many <c>CurriculumItemProgress</c> rows were credited.</item>
    /// </list>
    /// Rows written before T108 stay <c>null</c>: the outcome genuinely was not recorded for them,
    /// and claiming otherwise retrospectively would be a guess.
    /// </remarks>
    public int? CreditedItemCount { get; set; }

    /// <summary>
    /// How many curriculum items this transition counted for volume but refused the minimum on, because the
    /// assessment's entrustment ladder and the curriculum item's were both known and different (T109).
    /// </summary>
    /// <remarks>
    /// Three-valued on the same contract as <see cref="CreditedItemCount" />: <c>null</c> means credit was
    /// never evaluated for this move, <c>0</c> means it was evaluated and nothing was refused, and
    /// <c>&gt; 0</c> is the count refused. Rows written before T109 stay <c>null</c>.
    /// </remarks>
    public int? CreditScaleMismatchCount { get; set; }

    public Activity Activity { get; set; } = null!;
}
