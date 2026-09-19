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
/// <c>CreditApplier</c> looks a row up in <c>Local</c> first and only then in the database, so rows
/// marked <c>Deleted</c> but not yet saved would have come back through identity resolution still
/// marked for deletion, and every increment written onto them would have been thrown away at save.
/// </para>
/// <para>
/// <b>Still no entry point, deliberately.</b> T119 asks for an Administrator-only button behind a
/// confirmation; where that button lives, what the confirmation says and how the result is reported are
/// UI questions with a DESIGN.md pass attached, and they are not this change. What is done here instead
/// is to make the danger impossible rather than merely unlikely: the command is atomic, it refuses
/// anyone who is not a global <c>Administrator</c>, it can be aimed at a single trainee instead of the
/// whole corpus, and it hands back a count of everything it moved so whatever calls it can show the
/// operator what happened. Wiring a page to it is now a presentation job with no remaining hazard.
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
/// are replayed; every other row in the table is untouched and unread. T119 wants a rebuild confined to
/// the rows whose dates actually moved, and the restamper knows exactly whose those are.
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
/// <param name="ProgressRowsWritten">Distinct progress rows the replay created or incremented.</param>
/// <param name="ProgressRowsRemoved">Pre-existing progress rows the replay did not reproduce, and which were therefore deleted.</param>
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
        var preExistingKeys = existingRows.Select(KeyOf).ToHashSet();

        IQueryable<Activity> activityQuery = _dbContext.Set<Activity>()
            .Include(activity => activity.ActivityType)
                .ThenInclude(activityType => activityType.Versions)
            .Include(activity => activity.Transitions);

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

        var creditedKeys = new HashSet<(int CurriculumItemId, string TraineeUserId)>();
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
                    creditedKeys.Add(KeyOf(row));
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
            foreach (var (row, before) in snapshots)
            {
                before.RestoreTo(row);
            }

            DetachRowsAddedByTheReplay(progressSet, preExistingKeys, scope);
            throw;
        }

        foreach (var (transition, creditedItemCount, scaleMismatchCount) in stamps)
        {
            transition.CreditedItemCount = creditedItemCount;
            transition.CreditScaleMismatchCount = scaleMismatchCount;
        }

        // Only now, with the replacement already built in memory, does anything get deleted: a row the
        // replay did not reproduce is one a rebuild from empty would never have created — the activity
        // behind it was cancelled, re-dated out of the trainee's programme, or its curriculum item was
        // withdrawn. Leaving it would make the rebuild additive rather than a rebuild.
        var removedRows = existingRows
            .Where(row => !creditedKeys.Contains(KeyOf(row)))
            .ToList();

        progressSet.RemoveRange(removedRows);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RebuildCurriculumProgressResult(
            ActivitiesReplayed: activitiesReplayed,
            CreditApplications: creditApplications,
            ProgressRowsWritten: creditedKeys.Count,
            ProgressRowsRemoved: removedRows.Count,
            TransitionsStamped: stamps.Count);
    }

    /// <summary>
    /// The natural key of a progress row, and the one <c>CreditApplier</c> looks rows up by. Unique in
    /// the database (<c>CurriculumItemProgressConfiguration</c>), so it identifies a row the replay has
    /// just created as readily as one that came out of a table.
    /// </summary>
    private static (int CurriculumItemId, string TraineeUserId) KeyOf(CurriculumItemProgress row)
        => (row.CurriculumItemId, row.TraineeUserId);

    private static bool DeclaresCredit(string creditRulesJson)
        => !string.IsNullOrWhiteSpace(creditRulesJson) &&
           CreditRulesParser.Parse(creditRulesJson).CountsFor.Count > 0;

    /// <summary>
    /// Undoes the rows the failed replay added, by detaching rather than deleting them: calling
    /// <c>Remove</c> on an entity still in the <c>Added</c> state stops the context tracking it
    /// altogether, so nothing is left behind to be written by a later save.
    /// </summary>
    /// <remarks>
    /// A row whose natural key is not among the pre-existing ones can only be one the replay added:
    /// <c>CreditApplier</c> looks a row up by that key in <c>Local</c> before it creates one, so it
    /// never adds a second row for a key already tracked — and everything in scope was tracked by the
    /// query above. Identifying them that way rather than by <c>Id == 0</c> keeps this independent of
    /// whether EF has assigned a temporary key value to the instance yet.
    /// </remarks>
    private static void DetachRowsAddedByTheReplay(
        DbSet<CurriculumItemProgress> progressSet,
        HashSet<(int CurriculumItemId, string TraineeUserId)> preExistingKeys,
        string? scope)
    {
        foreach (var row in progressSet.Local.ToList())
        {
            if (preExistingKeys.Contains(KeyOf(row)))
            {
                continue;
            }

            if (scope is not null && !string.Equals(row.TraineeUserId, scope, StringComparison.Ordinal))
            {
                continue;
            }

            progressSet.Remove(row);
        }
    }

    /// <summary>
    /// Everything a replay writes to a progress row, so it can be zeroed before the replay and put back
    /// if the replay never finishes. The identity columns are deliberately absent: they are not
    /// rebuilt, and a restore must not be able to move a row to another trainee.
    /// </summary>
    private readonly record struct ProgressTally(
        int CountsSoFar,
        int MinimumLevelReachedCount,
        int? MinimumLevelScaleId,
        int ScaleMismatchCount,
        int UnverifiedLevelCount,
        int? LastActivityId,
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
            row.LastUpdated = LastUpdated;
            row.CreditedActivityKeysJson = CreditedActivityKeysJson;
        }
    }
}
