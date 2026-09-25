using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.Activities.Commands.RebuildCurriculumProgress;

/// <summary>
/// Recomputes <see cref="CurriculumItemProgress" /> from the completed activities that produced it, for one trainee or for
/// everybody, and saves it in one <c>SaveChangesAsync</c>. What <see cref="RebuildCurriculumProgressCommand" /> runs once
/// it has authorised its caller, and what a completion or a withdrawal runs for its trainee when the end it records takes
/// credit back (T281, <c>CompleteTraineeProfileCommand</c>, <c>DeactivateTraineeProfileCommand</c>).
/// </summary>
/// <remarks>
/// <para>
/// Moved here from the rebuild's handler, so every caller replays by one implementation. Everything the
/// command's remarks say about zeroing in place, the single save, the mid-rebuild refusal and the roll back holds for
/// both. In particular, the save is the caller's one save: any change the caller has already made to the context (the
/// end a completion records) is committed with the replay, or, when the replay throws, not by it. A caller that has
/// changed anything must put it back itself when this throws, because the audit pipeline's catch saves the context.
/// </para>
/// <para>
/// The activities are the caller's own <c>Set&lt;Activity&gt;()</c>, passed in, not read here: the activity read boundary
/// (<c>ActivityReadBoundaryTests</c>) looks for that read in each handler, and each caller states there why its read
/// needs no read scope. The replay returns counts only.
/// </para>
/// </remarks>
internal static class CurriculumProgressReplay
{
    /// <param name="activities">The caller's <c>Set&lt;Activity&gt;()</c>, unfiltered: the replay confines it to the trainee.</param>
    /// <param name="traineeUserId">
    /// The trainee whose progress rows are zeroed and whose activities are replayed; every other row is untouched and
    /// unread. Null replays everybody.
    /// </param>
    /// <param name="pendingEnd">
    /// The end the caller is recording on the trainee's profile and has not saved (T281): every completion is judged
    /// against it (<see cref="CreditSubject.PendingEnd" />), which applies it to the profile it names and to no other. Null
    /// for the rebuild, which judges the stored ends.
    /// </param>
    public static async Task<RebuildCurriculumProgressResult> RunAsync(
        IApplicationDbContext dbContext,
        ICreditApplier creditApplier,
        IQueryable<Activity> activities,
        string? traineeUserId,
        PendingProgrammeEnd? pendingEnd,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(creditApplier);
        ArgumentNullException.ThrowIfNull(activities);

        var scope = string.IsNullOrWhiteSpace(traineeUserId) ? null : traineeUserId;

        var progressSet = dbContext.Set<CurriculumItemProgress>();

        IQueryable<CurriculumItemProgress> progressQuery = progressSet;
        if (scope is not null)
        {
            progressQuery = progressQuery.Where(row => row.TraineeUserId == scope);
        }

        var existingRows = await progressQuery.ToListAsync(cancellationToken);

        // Captured BEFORE anything is touched, so a replay that throws can be undone in memory as well
        // as in the database. Leaving the zeroed rows sitting in a dirty DbContext would re-create the
        // very defect this handler is fixing, one seam further out: the context is scoped to the
        // request, and anything that called SaveChanges after us would commit the zeroes.
        var snapshots = existingRows.Select(row => (Row: row, Before: ProgressTally.Capture(row))).ToList();
        var preExisting = existingRows.ToHashSet<CurriculumItemProgress>(ReferenceEqualityComparer.Instance);

        // Split, because two collection includes in one query multiply rows: every activity times every version
        // of its type times every transition. T130 made this run at startup (CurriculumProgressBootstrapper),
        // where the cost is paid before the host is ready.
        IQueryable<Activity> activityQuery = activities
            .Include(activity => activity.ActivityType)
                .ThenInclude(activityType => activityType.Versions)
            .Include(activity => activity.Transitions)
            .AsSplitQuery();

        if (scope is not null)
        {
            activityQuery = activityQuery.Where(activity => activity.SubjectUserId == scope);
        }

        var loaded = await activityQuery.ToListAsync(cancellationToken);

        // Replayed in filing order, not encounter order (CreditReplay.InFilingOrder). Replaying in the order the live path
        // wrote LastActivityId is what makes a rebuild reproduce the incremental path exactly rather than approximately.
        var replayOrder = CreditReplay.InFilingOrder(loaded);

        var creditedKeys = new HashSet<CurriculumItemProgressKey>();
        var stamps = new List<(ActivityTransition Transition, int CreditedItemCount, int ScaleMismatchCount)>();
        var activitiesReplayed = 0;
        var creditApplications = 0;

        try
        {
            foreach (var row in existingRows)
            {
                ProgressTally.Zero(row);
            }

            foreach (var activity in replayOrder)
            {
                // A terminal state of the pinned workflow, under pinned rules that declare credit, with the pinned
                // schema, as the live path passes them (CreditReplay.PinnedCreditingType).
                var pinnedType = CreditReplay.PinnedCreditingType(activity);
                if (pinnedType is null)
                {
                    continue;
                }

                // Whether each EPA was in force is judged at the completion's own moment (CreditSubject.Of: its newest
                // transition), so an EPA deactivated since keeps the credit it earned while it was in force, and one
                // deactivated before the completion gives none (T196, D48). Only that: the items the directive matches,
                // their targets and their scale pins are the curriculum as it stands today. So is the programme's end: an
                // encounter after it credits nothing, whenever the end was recorded (T281), and the end the caller is
                // recording counts as recorded (pendingEnd). Planned and applied as ApplyAsync would, with that end added.
                var subject = CreditSubject.Of(activity) with { PendingEnd = pendingEnd };
                var credited = creditApplier.Apply(
                    await creditApplier.PlanAsync(subject, pinnedType, cancellationToken), activity);

                activitiesReplayed++;
                creditApplications += credited.UpdatedRows.Count;

                foreach (var row in credited.UpdatedRows)
                {
                    creditedKeys.Add(CurriculumItemProgressKey.Of(row));
                }

                // T106 item 12. Until now the outcome was stamped on the transition path only, so an
                // administrator who fixed a curriculum and ran a rebuild got the credit but kept the
                // stale zero — and ActivityView's "this credited nothing" banner stayed on for ever
                // after the exact remediation it told the reader to perform.
                //
                // The transition stamped is the newest one, selected with the same expression
                // CreditApplier.GetCreditKey uses to build the dedupe key. That is not a coincidence to
                // be tidied away: the stamp has to land on the transition the credit was recorded
                // against, or the two records describe different moves.
                var creditedTransition = CreditReplay.CreditedTransition(activity);

                if (creditedTransition is not null)
                {
                    // Deferred rather than written here, so the only mutations outstanding when the
                    // replay can still throw are the progress rows, which the catch below restores.
                    stamps.Add((creditedTransition, credited.UpdatedRows.Count, credited.ScaleMismatchCount));
                }
            }
        }
        catch
        {
            RollBack(progressSet, dbContext.Set<ActivityTransition>(), snapshots, preExisting, scope, stampsApplied: []);
            throw;
        }

        // A live completion that opened a new semester row after the rows above were read is invisible to the
        // zeroing and the removal, and the xmin token guards only rows that were read. The replay would find that
        // completion's key already in the new row, skip it, and then re-stamp its transition "credited nothing".
        // Such a row is the one kind that is tracked, not pre-existing and not Added (CreditApplier's tracking
        // query loaded it from the database). Refuse rather than write a false stamp; RollBack detaches it.
        var arrivedMidRebuild = progressSet.Local
            .Where(row => !preExisting.Contains(row) &&
                          (scope is null || string.Equals(row.TraineeUserId, scope, StringComparison.Ordinal)) &&
                          progressSet.Entry(row).State != EntityState.Added)
            .ToList();

        if (arrivedMidRebuild.Count > 0)
        {
            RollBack(progressSet, dbContext.Set<ActivityTransition>(), snapshots, preExisting, scope, stampsApplied: []);

            // Worded for both callers: an administrator's rebuild, and the replay a completion or a withdrawal runs.
            throw new InvalidOperationException(
                "Curriculum progress changed while the rebuild was running: a completion was credited at the same moment. Nothing was changed; try again.");
        }

        // The stamps, the removals and the save are guarded as well as the replay. If the save fails
        // (a deadlock, a dropped connection, a concurrency conflict raised by the xmin token because a
        // live completion moved a row mid-rebuild), the audit pipeline's catch saves this same
        // DbContext again. Left dirty, that second save would either commit the whole rebuild while the
        // operator is told it failed, or fail identically and lose the audit row as well.
        var stampsApplied = new List<(ActivityTransition Transition, int? CreditedItemCount, int? ScaleMismatchCount)>();
        List<CurriculumItemProgress> removedRows;
        try
        {
            foreach (var (transition, creditedItemCount, scaleMismatchCount) in stamps)
            {
                stampsApplied.Add((transition, transition.CreditedItemCount, transition.CreditScaleMismatchCount));
                transition.CreditedItemCount = creditedItemCount;
                transition.CreditScaleMismatchCount = scaleMismatchCount;
            }

            // Only now, with the replacement already built in memory, does anything get deleted: a row the
            // replay did not reproduce is one a rebuild from empty would never have created — the activity
            // behind it was cancelled, re-dated into another semester, or its curriculum item was withdrawn.
            // Leaving it would make the rebuild additive rather than a rebuild.
            removedRows = existingRows
                .Where(row => !creditedKeys.Contains(CurriculumItemProgressKey.Of(row)))
                .ToList();

            progressSet.RemoveRange(removedRows);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            RollBack(progressSet, dbContext.Set<ActivityTransition>(), snapshots, preExisting, scope, stampsApplied);
            throw;
        }

        return new RebuildCurriculumProgressResult(
            ActivitiesReplayed: activitiesReplayed,
            CreditApplications: creditApplications,
            ProgressRowsWritten: creditedKeys.Count,
            ProgressRowsRemoved: removedRows.Count,
            TransitionsStamped: stamps.Count);
    }

    /// <summary>
    /// Puts the DbContext back so that nothing the rebuild did is written by a later save, which the audit
    /// pipeline's catch will make.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every row that was loaded at the start gets its tally back and is marked Unchanged. That also cancels
    /// a pending removal. Every other tracked progress row in scope is DETACHED, whatever its state, and rows
    /// are matched by reference rather than by key. Two kinds of row fall into that set. One is a row the
    /// replay added (state Added). The other is a row <c>CreditApplier</c> loaded from the database because a
    /// live completion inserted it after the rebuild's initial read (state Unchanged or Modified). Calling
    /// <c>Remove</c> on the second kind would mark it Deleted, and the audit save would then delete a real
    /// credit. Detaching writes nothing at all.
    /// </para>
    /// <para>
    /// Stamped transitions get their previous values back and are marked Unchanged. Restoring the value alone is
    /// not enough. If the failure was the save itself, change detection has already run and flagged the stamp
    /// Modified, and the flag outlives the value.
    /// </para>
    /// <para>
    /// Only what the replay itself changed is put back. A change the caller made before it (a completion's end) is the
    /// caller's to put back.
    /// </para>
    /// </remarks>
    private static void RollBack(
        DbSet<CurriculumItemProgress> progressSet,
        DbSet<ActivityTransition> transitionSet,
        IReadOnlyList<(CurriculumItemProgress Row, ProgressTally Before)> snapshots,
        HashSet<CurriculumItemProgress> preExisting,
        string? scope,
        IReadOnlyList<(ActivityTransition Transition, int? CreditedItemCount, int? ScaleMismatchCount)> stampsApplied)
    {
        foreach (var (row, before) in snapshots)
        {
            before.RestoreTo(row);
            progressSet.Entry(row).State = EntityState.Unchanged;
        }

        foreach (var row in progressSet.Local.ToList())
        {
            if (preExisting.Contains(row))
            {
                continue;
            }

            if (scope is not null && !string.Equals(row.TraineeUserId, scope, StringComparison.Ordinal))
            {
                continue;
            }

            progressSet.Entry(row).State = EntityState.Detached;
        }

        foreach (var (transition, creditedItemCount, scaleMismatchCount) in stampsApplied)
        {
            transition.CreditedItemCount = creditedItemCount;
            transition.CreditScaleMismatchCount = scaleMismatchCount;
            transitionSet.Entry(transition).State = EntityState.Unchanged;
        }
    }

    /// <summary>
    /// Everything a replay writes to a progress row, so it can be zeroed before the replay and put back
    /// if the replay never finishes. The identity columns (the item, the trainee and the semester) are
    /// deliberately absent: they are not rebuilt, and a restore must not be able to move a row to another
    /// trainee or another semester.
    /// </summary>
    private readonly record struct ProgressTally(
        int CountsSoFar,
        int MinimumLevelReachedCount,
        int? MinimumLevelScaleId,
        int ScaleMismatchCount,
        int UnverifiedLevelCount,
        int? LastActivityId,
        DateOnly? LastObservedOn,
        bool LastObservedOnDeclared,
        DateTime LastUpdated,
        string CreditedActivityKeysJson)
    {
        public static ProgressTally Capture(CurriculumItemProgress row)
            => new(
                row.CountsSoFar,
                row.MinimumLevelReachedCount,
                row.MinimumLevelScaleId,
                row.ScaleMismatchCount,
                row.UnverifiedLevelCount,
                row.LastActivityId,
                row.LastObservedOn,
                row.LastObservedOnDeclared,
                row.LastUpdated,
                row.CreditedActivityKeysJson);

        /// <summary>
        /// Resets a row to the state a rebuild-from-empty would start it in. Clearing
        /// <c>CreditedActivityKeysJson</c> is the load-bearing part: it is the dedupe set that makes
        /// re-crediting an activity a no-op, so without this the replay would add nothing at all.
        /// </summary>
        public static void Zero(CurriculumItemProgress row)
        {
            row.CountsSoFar = 0;
            row.MinimumLevelReachedCount = 0;
            row.MinimumLevelScaleId = null;
            row.ScaleMismatchCount = 0;
            row.UnverifiedLevelCount = 0;
            row.LastActivityId = null;
            row.LastObservedOn = null;
            row.LastObservedOnDeclared = false;
            row.CreditedActivityKeysJson = "[]";
        }

        public void RestoreTo(CurriculumItemProgress row)
        {
            row.CountsSoFar = CountsSoFar;
            row.MinimumLevelReachedCount = MinimumLevelReachedCount;
            row.MinimumLevelScaleId = MinimumLevelScaleId;
            row.ScaleMismatchCount = ScaleMismatchCount;
            row.UnverifiedLevelCount = UnverifiedLevelCount;
            row.LastActivityId = LastActivityId;
            row.LastObservedOn = LastObservedOn;
            row.LastObservedOnDeclared = LastObservedOnDeclared;
            row.LastUpdated = LastUpdated;
            row.CreditedActivityKeysJson = CreditedActivityKeysJson;
        }
    }
}
