using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Activities.Services;

/// <summary>
/// What one application of the credit rules did. The counters are per-call, not cumulative: they describe
/// this completion, so the caller can stamp the outcome onto the transition that caused it (T109).
/// </summary>
/// <param name="UpdatedRows">
/// The progress rows this call created or incremented: one per curriculum item credited, each the row for the
/// semester that contains the encounter date (T130). Its count is therefore the number of ITEMS credited, which
/// is what <c>ActivityTransition.CreditedItemCount</c> records.
/// </param>
/// <param name="ScaleMismatchCount">
/// Curriculum items that counted toward volume but were refused the minimum, because the assessment's
/// entrustment ladder and the item's were both known and different.
/// </param>
/// <param name="UnverifiedLevelCount">
/// Curriculum items whose level comparison was made without scale proof on at least one side.
/// </param>
public sealed record CreditApplicationResult(
    IReadOnlyList<CurriculumItemProgress> UpdatedRows,
    int ScaleMismatchCount,
    int UnverifiedLevelCount)
{
    public static CreditApplicationResult Empty { get; } = new([], 0, 0);
}

/// <summary>
/// The facts about an activity that credit depends on. They are passed explicitly rather than read off the entity,
/// so that <see cref="ICreditApplier.PlanAsync" /> can run BEFORE the transition mutates the activity, using the data,
/// the encounter date and the moment the transition is about to write.
/// </summary>
/// <param name="ObservedOnDeclared">
/// Whether <paramref name="ObservedOn" /> was stated (<c>ObservationDateSource.Declared</c>) rather than being the day
/// the activity was created. Credit does not depend on it; the progress row records it beside the date, so a reader
/// can mark an undated last encounter (T219). No default: every caller says which it is.
/// </param>
/// <param name="CreditedAt">
/// The moment of credit: the time of the transition credit is recorded against (UTC). Only curriculum items whose EPA was
/// in force at that moment are credited (<c>CurriculumItemsInForce.InForceAt</c>, T196), so a replay judges an EPA's
/// pause as the live path did. It is the only thing judged as of that moment: the items themselves, their targets and
/// their scale pins are read as they stand.
/// </param>
public sealed record CreditSubject(
    string SubjectUserId,
    DateOnly ObservedOn,
    bool ObservedOnDeclared,
    string DataJson,
    DateTime CreditedAt)
{
    /// <summary>
    /// Set only when an EPA is reactivated (T196, D48): credit this EPA's items and no others, as in force whatever the
    /// catalogue said at <see cref="CreditedAt" />. The completion was paused because the EPA was inactive; its other
    /// items were judged when it completed, and credit never re-litigates them.
    /// </summary>
    public int? ResumedEpaId { get; init; }

    /// <summary>
    /// Set only by the replay a completion or a withdrawal runs when the end it records takes credit back (T281,
    /// <c>ProgrammeEndCredit</c>): the end that request is recording, which it has not saved yet. Credit judges the
    /// encounter against it in place of the stored end when it is recorded on the profile credit accrues against.
    /// </summary>
    /// <remarks>
    /// Passed explicitly, not read off an unsaved profile the request happens to track: every other plan, the live
    /// completion's and the tool gate's included, reads the profile as it is stored, and a request that edits a profile
    /// for any other reason does not change what its credit plans against.
    /// </remarks>
    public PendingProgrammeEnd? PendingEnd { get; init; }

    /// <summary>
    /// The subject, date and data an activity already carries, credited at its newest transition: for a replay, or for a
    /// caller that has already transitioned it.
    /// </summary>
    /// <remarks>
    /// The newest transition is the one credit is recorded against: <c>CreditApplier</c> builds its dedupe key from it and
    /// the rebuild stamps it, by the same selection.
    /// </remarks>
    public static CreditSubject Of(Activity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);
        return new CreditSubject(
            activity.SubjectUserId,
            activity.ObservedOn,
            activity.ObservedOnSource == ObservationDateSource.Declared,
            activity.DataJson,
            CreditedAtOf(activity));
    }

    /// <summary>The time of the transition credit is recorded against, or the activity's last update when it has none.</summary>
    public static DateTime CreditedAtOf(Activity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);
        return CreditReplay.CreditedTransition(activity)?.OccurredOn ?? activity.UpdatedOn;
    }
}

/// <summary>
/// A programme end a request is recording and has not yet saved (T281): the profile it is recorded on, and the last day.
/// See <see cref="CreditSubject.PendingEnd" />.
/// </summary>
public sealed record PendingProgrammeEnd(int TraineeProfileId, DateOnly EndedOn);

/// <summary>One curriculum item a completion will credit, decided with every read already done.</summary>
public sealed record PlannedCredit(int CurriculumItemId, int Amount, LevelComparison Comparison, int? ItemScaleId);

/// <summary>
/// Everything credit needs to know, gathered by <see cref="ICreditApplier.PlanAsync" /> without mutating
/// anything, so that <see cref="ICreditApplier.Apply" /> can finish without a single await.
/// </summary>
/// <remarks>
/// <see cref="ExistingRows" /> are TRACKED entities. They are every progress row the trainee has on every
/// candidate item, in every semester, gathered from <c>Local</c> and from the database. All periods are
/// included so the dedupe can look across buckets: an activity credits an item at most once, whichever
/// semester it lands in.
/// </remarks>
/// <param name="ObservedOnDeclared">The subject's <see cref="CreditSubject.ObservedOnDeclared" />, carried to the rows (T219).</param>
public sealed record CreditPlan(
    string TraineeUserId,
    DateOnly ObservedOn,
    bool ObservedOnDeclared,
    IReadOnlyList<PlannedCredit> Credits,
    IReadOnlyList<CurriculumItemProgress> ExistingRows)
{
    public static CreditPlan Nothing { get; } = new(string.Empty, default, false, [], []);
}

public interface ICreditApplier
{
    /// <summary>
    /// Every read credit needs: the trainee, their curriculum, the matched items, the level comparisons, the
    /// scale bindings and the progress rows. Mutates nothing, so a failure here (a dropped connection, a
    /// cancellation) leaves nothing half-written for the audit pipeline's catch to commit.
    /// </summary>
    /// <param name="activityType">
    /// Carries the <em>pinned</em> version's <c>CreditRulesJson</c> and <c>SchemaJson</c>. The schema is
    /// required as well as the rules because the scale an achieved ordinal sits on is declared by the
    /// <c>scale_key</c> of the field the directive names, and pinning means that answer cannot drift for
    /// the life of the activity (T109).
    /// </param>
    Task<CreditPlan> PlanAsync(
        CreditSubject subject,
        ActivityType activityType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the plan onto the progress rows: synchronous, no I/O, so nothing can fail between the first
    /// increment and the caller's save. The activity must already carry the transition credit is recorded
    /// against, because the dedupe key is <c>{activityId}:{transitionKey}</c>.
    /// </summary>
    CreditApplicationResult Apply(CreditPlan plan, Activity completedActivity);

    /// <summary>
    /// <see cref="PlanAsync" /> then <see cref="Apply" />, for an activity that has already been transitioned
    /// and stamped, credited as <see cref="CreditSubject.Of" /> describes it. The live transition path calls the two
    /// halves separately, with the transition in between, and so does the rebuild's replay, which may add a pending end
    /// to the subject (T281).
    /// </summary>
    Task<CreditApplicationResult> ApplyAsync(
        Activity completedActivity,
        ActivityType activityType,
        CancellationToken cancellationToken = default);
}
