using Wombat.Domain.Activities.Credit;

namespace Wombat.Domain.Activities;

/// <summary>
/// How late an encounter may be filed before the filing is called late (T160, D15), and which activity types the
/// credit-protecting rules apply to.
/// </summary>
/// <remarks>
/// <para>
/// The College's answer to D15: a filing more than fourteen days after the encounter is warned about and recorded, and
/// never refused. The "never refused" half carries the weight. A registrar blocked by a date rule types today's date
/// instead, and that destroys the encounter date T119 exists to protect. So nothing on the write path reads this
/// constant to refuse; it decides only whether a warning is shown and how a recorded filing is labelled.
/// </para>
/// <para>
/// The two hard bounds on an encounter date (not after today, not before the trainee's programme began) live with the
/// write path in <c>EncounterDateGate</c>, because the second needs the trainee's profile. Only the first applies to
/// every type; the second, like the lateness warning and record, applies only where <see cref="CanCredit" /> holds.
/// </para>
/// <para>
/// Every date here is a South African calendar date (<see cref="Curricula.ProgrammeCalendar" />): the encounter date is
/// one as typed, and the filing date is taken on that calendar so that the two agree about which day it is.
/// </para>
/// </remarks>
public static class EncounterDatePolicy
{
    /// <summary>
    /// How many days after the encounter a filing is still on time. A filing on day 14 is on time; day 15 is late.
    /// </summary>
    public const int LateFilingDays = 14;

    /// <summary>Whole calendar days from the encounter to the filing. Zero when both are the same day.</summary>
    public static int DaysAfterEncounter(DateOnly encounteredOn, DateOnly filedOn)
        => filedOn.DayNumber - encounteredOn.DayNumber;

    /// <summary>Whether a filing this many days after its encounter is late (more than <see cref="LateFilingDays" />).</summary>
    public static bool IsLateFiling(int daysAfterEncounter) => daysAfterEncounter > LateFilingDays;

    /// <summary>
    /// Whether an activity pinned to these credit rules can credit anything: they declare at least one
    /// <c>counts_for</c> directive. Only such an activity's encounter date is held to the rules that exist to protect
    /// credit: the programme-start bound, the lateness warning on the form, the lateness record on the filing, and so the
    /// "Filed N days after the encounter" label in its history.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Those rules protect credit and nothing else. A date before the programme began matters because credit picks the
    /// curriculum item's minimum from the stage the trainee was in on that date, and a date with no stage silently drops
    /// to the flat minimum (T119); a late filing matters because D15 is about late WBA filing. A research output, a
    /// journal club or a reflective exercise credits nothing, so its date may fairly precede the programme (work from
    /// before admission) and its filing is late for nobody. Such a type still gets the future check, which is about the
    /// date being true, not about credit.
    /// </para>
    /// <para>
    /// The one predicate the server gate (<c>EncounterDateGate</c>), the service's filing record and the form's warning
    /// all ask, so the three cannot disagree about which types they cover. Callers pass the rules of the version the
    /// activity is PINNED to (on the create page, the version a create would pin). Blank rules credit nothing, as the
    /// terminal move's credit check treats them (T108). Malformed rules throw <see cref="CreditRulesParseException" />: a
    /// pinned version was parsed when it was published, so they are a corrupt row, not a type that credits nothing.
    /// </para>
    /// </remarks>
    public static bool CanCredit(string? creditRulesJson)
        => !string.IsNullOrWhiteSpace(creditRulesJson) &&
           CreditRulesParser.Parse(creditRulesJson).CountsFor.Count > 0;
}
