using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.Activities.Commands.RebuildCurriculumProgress;

/// <summary>
/// Recomputes <see cref="CurriculumItemProgress" /> from the completed activities that produced it —
/// for one trainee, or for everybody. No validator: a caller identity the handler authorizes itself,
/// and an optional user id that either matches rows or does not; there is no shape to check that the
/// handler does not already enforce.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why it suddenly matters (T119).</b> A stored progress row is a tally, not a per-activity outcome,
/// and nothing re-reads it. It cannot even be topped up: <c>CreditApplier</c>'s dedupe key is
/// <c>{activityId}:{transitionKey}</c>, so re-running credit over an already-credited activity is a
/// no-op by design. A rebuild is therefore the only thing in the product that can move a stored number —
/// and wiring <c>Activity.ObservedOn</c> re-dates every completion, which is exactly the change that
/// needs one. Before T119 this command was a convenience; after it, it is the repair tool.
/// </para>
/// <para>
/// <b>It used to be unsafe to run.</b> It deleted every progress row and <em>saved</em>, then replayed
/// and saved again. A failure, a cancellation or a deadlock between the two left every trainee in the
/// system on zero progress, with no way back except running it again. That is repaired by never
/// deleting anything before the replay has finished: the pre-existing rows are zeroed <em>in place</em>,
/// the replay fills them back up through the same <see cref="ICreditApplier" /> the live path uses, and
/// a single <c>SaveChangesAsync</c> commits the lot. EF wraps one SaveChanges in one transaction, so
/// atomicity needs no new seam on <see cref="IApplicationDbContext" /> — which exposes no
/// <c>Database</c>, and therefore no <c>BeginTransaction</c> — and the rebuild stays exercisable on the
/// in-memory provider, which cannot do transactions at all.
/// </para>
/// <para>
/// Zeroing in place rather than deleting is also what makes the replay see a clean slate:
/// <c>CreditApplier</c> reads the trainee's rows with a TRACKING query merged with <c>Local</c>, so
/// identity resolution hands it back the zeroed instances. Rows marked <c>Deleted</c> but not yet
/// saved would have come back still marked for deletion, and every increment written onto them would
/// have been thrown away at save.
/// </para>
/// <para>
/// <b>Semester buckets (T130).</b> A row is the tally for one (item, trainee, semester), and the
/// semester is recomputed from each activity's <c>ObservedOn</c> on every replay. So a rebuild is also
/// the tool that re-buckets: when the semester boundary moves (D40), or an encounter date changed after
/// the activity was credited, rows the replay no longer reproduces are removed and the right ones are
/// written. The key is <see cref="CurriculumItemProgressKey" />, the same definition
/// <c>CreditApplier</c> looks rows up by.
/// </para>
/// <para>
/// <b>Callers (T130).</b> The Administrator page <c>/admin/curriculum-progress</c>, behind a
/// confirmation; and <c>CurriculumProgressBootstrapper</c> at startup, which runs it once when the
/// progress table is empty but completions have credited. The T130 migration empties the table on every
/// existing database, and so does T219's, so without that second caller every trainee would read zero
/// until somebody found the button. The command is atomic, refuses anyone who is not a global
/// <c>Administrator</c>, can be aimed at a single trainee, and hands back a count of everything it moved.
/// </para>
/// </remarks>
/// <param name="Principal">
/// The caller. A rebuild re-scores every trainee in every institution, so it is global-Administrator
/// only — an InstitutionalAdmin is not offered a per-institution variant, because credit is scoped by
/// the trainee's adopted curriculum rather than by the activity's institution stamp and the two are not
/// the same cut.
/// </param>
/// <param name="TraineeUserId">
/// Optional. When set, only this trainee's progress rows are zeroed and only this trainee's activities
/// are replayed; every other row in the table is untouched and unread. Useful after one trainee's
/// encounter dates, curriculum or programme start have been corrected.
/// </param>
[NoValidator]
public sealed record RebuildCurriculumProgressCommand(
    ClaimsPrincipal Principal,
    string? TraineeUserId = null) : IRequest<RebuildCurriculumProgressResult>;

/// <summary>
/// What one rebuild moved. Counts only — no activity row and no trainee data leaves the handler, which
/// is the claim <c>ActivityReadBoundaryTests.ScopeExemptHandlers</c> makes on its behalf.
/// </summary>
/// <param name="ActivitiesReplayed">Terminal activities whose pinned version declares credit, i.e. the ones credit was actually evaluated for.</param>
/// <param name="CreditApplications">Curriculum items credited across all of them, counting an item once per activity that credited it.</param>
/// <param name="ProgressRowsWritten">Distinct progress rows (item, trainee, semester) the replay created or incremented.</param>
/// <param name="ProgressRowsRemoved">Pre-existing progress rows the replay did not reproduce, and which were therefore deleted. A re-bucketing shows up here.</param>
/// <param name="TransitionsStamped">Transitions whose <c>CreditedItemCount</c> was refreshed (T106 item 12).</param>
public sealed record RebuildCurriculumProgressResult(
    int ActivitiesReplayed,
    int CreditApplications,
    int ProgressRowsWritten,
    int ProgressRowsRemoved,
    int TransitionsStamped);

public sealed class RebuildCurriculumProgressCommandHandler : IRequestHandler<RebuildCurriculumProgressCommand, RebuildCurriculumProgressResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICreditApplier _creditApplier;

    public RebuildCurriculumProgressCommandHandler(IApplicationDbContext dbContext, ICreditApplier creditApplier)
    {
        _dbContext = dbContext;
        _creditApplier = creditApplier;
    }

    public async Task<RebuildCurriculumProgressResult> Handle(RebuildCurriculumProgressCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Principal);

        if (!request.Principal.IsAdministrator())
        {
            // Throw rather than return an empty result: a rebuild that silently did nothing would look
            // to an operator exactly like a rebuild that found nothing to change.
            throw new UnauthorizedAccessException("Rebuilding curriculum progress requires the Administrator role.");
        }

        string? scope = string.IsNullOrWhiteSpace(request.TraineeUserId) ? null : request.TraineeUserId;

        var progressSet = _dbContext.Set<CurriculumItemProgress>();

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
        IQueryable<Activity> activityQuery = _dbContext.Set<Activity>()
            .Include(activity => activity.ActivityType)
                .ThenInclude(activityType => activityType.Versions)
            .Include(activity => activity.Transitions)
            .AsSplitQuery();

        if (scope is not null)
        {
            activityQuery = activityQuery.Where(activity => activity.SubjectUserId == scope);
        }

        var activities = await activityQuery.ToListAsync(cancellationToken);

        // Replayed in filing order, not encounter order. The tallies themselves are order-independent,
        // but LastActivityId is not: it means "the completion that most recently moved this row", and
        // the live path writes it in the order transitions happened. Replaying in that same order is
        // what makes a rebuild reproduce the incremental path exactly rather than approximately.
        var replayOrder = activities
            .OrderBy(activity => activity.Transitions.Count == 0
                ? activity.CreatedOn
                : activity.Transitions.Max(transition => transition.OccurredOn))
            .ThenBy(activity => activity.Id)
            .ToList();

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
                var pinnedVersion = activity.ActivityType.Versions.SingleOrDefault(version => version.Version == activity.SchemaVersion);
                if (pinnedVersion is null)
                {
                    continue;
                }

                var workflow = WorkflowParser.Parse(pinnedVersion.WorkflowJson);
                var state = workflow.States.SingleOrDefault(candidate =>
                    string.Equals(candidate.Key, activity.CurrentState, StringComparison.Ordinal));

                if (state is null || !state.Terminal)
                {
                    continue;
                }

                // The same `counts_for` gate the live path checks BEFORE calling the applier
                // (ActivityService.TransitionAsync). Checking it here rather than reading a zero out of
                // the result is what keeps the stamp below three-valued: a reflective note, journal
                // club, procedure log, QI project, research output or teaching session declares an
                // empty counts_for, was never evaluated, and must stay null rather than be flagged as
                // having credited nothing.
                if (!DeclaresCredit(pinnedVersion.CreditRulesJson))
                {
                    continue;
                }

                var credited = await _creditApplier.ApplyAsync(
                    activity,
                    new ActivityType
                    {
                        CreditRulesJson = pinnedVersion.CreditRulesJson,
                        // Same pinned schema the live path passes, so a replay reaches the same scale bindings
                        // and therefore the same credit as the original completion did (T109).
                        SchemaJson = pinnedVersion.SchemaJson
                    },
                    cancellationToken);

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
                var creditedTransition = activity.Transitions
                    .OrderByDescending(transition => transition.OccurredOn)
                    .FirstOrDefault();

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
            RollBack(progressSet, _dbContext.Set<ActivityTransition>(), snapshots, preExisting, scope, stampsApplied: []);
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
            RollBack(progressSet, _dbContext.Set<ActivityTransition>(), snapshots, preExisting, scope, stampsApplied: []);
            throw new InvalidOperationException(
                "Curriculum progress changed while the rebuild was running: a completion was credited mid-rebuild. Nothing was changed; run the rebuild again.");
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

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            RollBack(progressSet, _dbContext.Set<ActivityTransition>(), snapshots, preExisting, scope, stampsApplied);
            throw;
        }

        return new RebuildCurriculumProgressResult(
            ActivitiesReplayed: activitiesReplayed,
            CreditApplications: creditApplications,
            ProgressRowsWritten: creditedKeys.Count,
            ProgressRowsRemoved: removedRows.Count,
            TransitionsStamped: stamps.Count);
    }

    private static bool DeclaresCredit(string creditRulesJson)
        => !string.IsNullOrWhiteSpace(creditRulesJson) &&
           CreditRulesParser.Parse(creditRulesJson).CountsFor.Count > 0;

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
