using System.Security.Claims;
using MediatR;
using Wombat.Application.Common;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
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
/// <b>Deactivated EPAs (T196, D48).</b> Whether a completion's EPA was in force is judged at the completion's
/// own moment, the transition credit is recorded against (<see cref="CreditSubject.Of" />), not on the day the
/// rebuild runs. So a rebuild while an EPA is inactive keeps the credit it earned while in force, and credits
/// nothing completed during its pause, exactly as the live path did. Before T196 it dropped the lot. That is the
/// one thing judged as of the completion: which items the curriculum holds, their targets and their scale pins
/// are read as they stand today, which is what makes a rebuild the repair tool after a curriculum edit.
/// Reactivating the EPA credits the paused completions itself (<c>ResumedEpaCredit</c>), so this page is no
/// longer the only way to restore them.
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
/// encounter dates have been corrected. A change of curriculum or programme start replays the trainee itself, in the
/// profile's own save (T304, <c>UpdateTraineeProfileCommand</c>).
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

        // The replay a completion or a withdrawal also runs for its trainee (T281), so the two cannot replay differently.
        // The rebuild judges the ends as stored, so it passes none pending.
        return await CurriculumProgressReplay.RunAsync(
            _dbContext, _creditApplier, _dbContext.Set<Activity>(), request.TraineeUserId, pendingEnd: null, pendingMove: null, cancellationToken);
    }
}
