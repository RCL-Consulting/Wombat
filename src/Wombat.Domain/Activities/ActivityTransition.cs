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

    /// <summary>
    /// On the move that filed an activity of a type that can credit, how many days after its stated encounter it was
    /// filed; <c>null</c> on every other move, and on every move of a type that credits nothing (T160, D15).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The filing is the author's first move out of the workflow's initial state that leads on towards credit (a CPSA
    /// <c>submit</c>), or the create itself when no such move is the author's (a type born in <c>requested</c> or born
    /// terminal, T127). Only the first: a re-submission after a supervisor's <c>return</c> to the draft is not a second
    /// filing, and its row records nothing, so the delay a reader is shown is the author's alone. Counted in South
    /// African calendar days from <see cref="Activity.ObservedOn" /> to the day of <see cref="OccurredOn" />, so it is
    /// never negative: an encounter date after the filing day is refused.
    /// </para>
    /// <para>
    /// Recorded on every filing whose encounter date was stated, late or not, so the lateness a reader is shown is the
    /// policy applied to a fact (<see cref="EncounterDatePolicy.IsLateFiling" />), not a verdict frozen at the time.
    /// <c>null</c> as well when nobody stated the date (<see cref="ObservationDateSource.CreatedOn" />): the encounter
    /// date is then the filing date, and "zero days late" would be a claim nobody made. The system-written path (an MSF
    /// release) files nothing and records nothing.
    /// </para>
    /// <para>
    /// Only a type whose pinned credit rules can credit records it (<see cref="EncounterDatePolicy.CanCredit" />): D15 is
    /// about late WBA filing, and a research output or a reflective exercise filed late is late for nobody. So the
    /// history's "Filed N days after the encounter" never appears on such a type either.
    /// </para>
    /// <para>
    /// Never a refusal. D15: a late filing is warned about and recorded, and a registrar who is refused types today's
    /// date instead.
    /// </para>
    /// </remarks>
    public int? DaysAfterEncounter { get; set; }

    public Activity Activity { get; set; } = null!;
}
