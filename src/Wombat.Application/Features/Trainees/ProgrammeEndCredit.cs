using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Commands.RebuildCurriculumProgress;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Trainees;

/// <summary>
/// What recording a programme's end does to the trainee's credit (T281): an encounter observed after the last day credits
/// nothing on the profile (<see cref="TraineeProfile.IsAfterEnd(DateOnly)" />), so an end recorded after such an encounter
/// was credited takes that credit back. Shared by both ways out of the programme, a completion
/// (<see cref="CompleteTraineeProfileCommand" />) and a withdrawal (<see cref="DeactivateTraineeProfileCommand" />).
/// </summary>
/// <remarks>
/// <para>
/// The end may be recorded after the fact (T209), and until it is, the profile is running and credit accrues as usual.
/// So by the time an administrator records a last day of 20 August on 25 September, a WBA observed on 3 September may
/// already sit in the trainee's semester 2 tally. The credit rule alone would not move it: a stored tally is only ever
/// recomputed by a replay. So the end is saved with a replay of the trainee's credit
/// (<see cref="CurriculumProgressReplay" />), the rebuild's own, against the end being recorded, in one save. The replay
/// is handed that end (<see cref="PendingProgrammeEnd" />), because it is not saved yet: saving it first would commit it
/// alone if the replay then failed.
/// </para>
/// <para>
/// The replay rewrites the trainee's whole history against the curriculum as it stands today, as the Administrator's
/// rebuild does: an item added, or a minimum changed, since the tally was last rebuilt is scored now too. It also
/// re-stamps the completions it replays wherever they were filed, at another institution included. That is the
/// rebuild's own behaviour for one trainee, reached here by an InstitutionalAdmin of the profile's institution, and only
/// when the end takes credit back.
/// </para>
/// <para>
/// <b>Concurrency.</b> Both callers hold the profile (<see cref="ITraineeCreditLock.HoldForEndAsync" />) from before they
/// read it until their save commits, and every completion holds its trainee's profiles shared while it plans and saves
/// its credit, as an EPA's reactivation does for the trainees it credits. So <see cref="TakesCreditBackAsync" /> reads
/// every credit that committed before the end, and nothing credits against the profile as it stood before the end once
/// that read has run. The Administrator's rebuild takes no hold: one that overlaps an end's replay is refused by the
/// progress rows' xmin tokens, and otherwise judges the ends as stored.
/// </para>
/// <para>
/// Only when the end takes credit back: when a completion of the trainee's observed after the last day has credited. Any
/// other end changes nothing a replay would change because of it, so it is saved alone, as before T281, and does not
/// re-score the trainee's history against the curriculum as it stands today, which a replay also does.
/// </para>
/// <para>
/// The audit trap: every read that decides whether to replay runs before the caller records the end. The replay itself
/// runs after it (it judges against the end), so when it throws, the end is put back before the exception leaves, and
/// the audit pipeline's save commits nothing. The replay puts back its own changes.
/// </para>
/// </remarks>
internal static class ProgrammeEndCredit
{
    /// <summary>
    /// Whether recording <paramref name="endedOn" /> as the last day of <paramref name="traineeUserId" />'s programme
    /// takes credit back: some activity about them, observed after that day, records that it credited.
    /// </summary>
    /// <param name="activities">The caller's <c>Set&lt;Activity&gt;()</c>.</param>
    public static async Task<bool> TakesCreditBackAsync(
        IQueryable<Activity> activities,
        string traineeUserId,
        DateOnly endedOn,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(activities);

        // The dates, not a date comparison in SQL, so the rule judged is the one credit applies (IsAfterEnd). One trainee's
        // credited activities: a programme's worth of dates at most.
        var creditedOn = await activities
            .AsNoTracking()
            .Where(activity => activity.SubjectUserId == traineeUserId &&
                               activity.Transitions.Any(transition => transition.CreditedItemCount > 0))
            .Select(activity => activity.ObservedOn)
            .ToListAsync(cancellationToken);

        return creditedOn.Any(observedOn => TraineeProfile.IsAfterEnd(endedOn, observedOn));
    }

    /// <summary>
    /// Saves the end the caller has just recorded on <paramref name="profile" />: with a replay of the trainee's credit when
    /// <paramref name="takesCreditBack" />, in the replay's one save, and alone otherwise.
    /// </summary>
    /// <param name="activities">The caller's <c>Set&lt;Activity&gt;()</c>.</param>
    /// <param name="takesCreditBack"><see cref="TakesCreditBackAsync" />'s answer, read before the end was recorded.</param>
    public static async Task SaveAsync(
        IApplicationDbContext dbContext,
        ICreditApplier creditApplier,
        IQueryable<Activity> activities,
        TraineeProfile profile,
        bool takesCreditBack,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(profile);

        if (!takesCreditBack)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        try
        {
            // The replay judges every completion against the end just recorded, which is not saved yet, and saves the end
            // with the tallies it rewrites and the transitions it re-stamps.
            var pendingEnd = new PendingProgrammeEnd(
                profile.Id, profile.EndedOn ?? throw new InvalidOperationException("The profile has no end to replay against."));
            await CurriculumProgressReplay.RunAsync(
                dbContext, creditApplier, activities, profile.UserId, pendingEnd, pendingMove: null, cancellationToken);
        }
        catch
        {
            // The replay has put back what it changed. The end is the caller's change: put it back too, so the audit
            // pipeline's save of this context commits neither.
            var entry = dbContext.Set<TraineeProfile>().Entry(profile);
            entry.CurrentValues.SetValues(entry.OriginalValues);
            entry.State = EntityState.Unchanged;
            throw;
        }
    }
}
