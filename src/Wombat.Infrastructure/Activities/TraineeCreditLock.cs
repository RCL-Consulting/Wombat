using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Infrastructure.Activities;

/// <summary>
/// Row locks on <c>TraineeProfiles</c>, held in a transaction on the request's own context until its save commits (T281).
/// See <see cref="ITraineeCreditLock" /> for the races they close.
/// </summary>
/// <remarks>
/// <para>
/// <c>FOR SHARE</c> on every profile of the trainee for credit, and <c>FOR NO KEY UPDATE</c> on the one profile whose end
/// is recorded. Shared locks do not conflict with one another, so two completions for one trainee never wait for each
/// other; each conflicts with the end's lock, and two ends of one profile conflict with each other, so the second reads
/// the first's end and is refused as a profile no longer active. Credit locks every profile of its trainee because which
/// one it credits against is the preferred profile (<c>CreditTargetResolver.PickProfileAsync</c>), read after the hold;
/// the profile an end is recorded on is always among them.
/// </para>
/// <para>
/// Not <c>FOR UPDATE</c>: nothing references a profile by foreign key today, but the end's own <c>UPDATE</c> takes
/// <c>FOR NO KEY UPDATE</c> (it changes no key a foreign key could use; the unique index on the user id is partial), so
/// the stronger lock would buy nothing. Every other write to a profile (an edit of its curriculum or start date, an
/// erasure) also takes <c>FOR NO KEY UPDATE</c> when it saves, so it waits for a completion of that trainee in flight, and
/// a completion waits for it: a few milliseconds either way.
/// </para>
/// <para>
/// A profile id that names no row locks nothing, and the caller then finds no profile. The busy messages are what a
/// request is refused with when its hold gives up waiting (<see cref="CreditHolds" />).
/// </para>
/// </remarks>
public sealed class TraineeCreditLock : ITraineeCreditLock
{
    private readonly DbContext? _dbContext;

    /// <param name="dbContext">
    /// The request's context: the lock must be held on the connection its save goes through. Anything that is not an EF
    /// context, or a context on another provider, gets holds that do nothing.
    /// </param>
    public TraineeCreditLock(IApplicationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext as DbContext;
    }

    /// <summary>What recording a programme's end is refused with when its hold gives up waiting.</summary>
    public const string EndBusy =
        "Activities are still being completed for this trainee, so nothing was saved. Try again in a moment.";

    /// <summary>What a completion is refused with when its hold gives up waiting.</summary>
    public const string CreditBusy =
        "This trainee's programme is being ended right now, so nothing was saved. Try again in a moment.";

    public Task<ICreditHold> HoldForEndAsync(int traineeProfileId, CancellationToken cancellationToken)
        => CreditHolds.HoldAsync(
            _dbContext,
            database => database.ExecuteSqlInterpolatedAsync(
                $"SELECT 1 FROM \"TraineeProfiles\" WHERE \"Id\" = {traineeProfileId} FOR NO KEY UPDATE",
                cancellationToken),
            EndBusy,
            cancellationToken);

    public Task<ICreditHold> HoldForCreditAsync(IReadOnlyCollection<string> traineeUserIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(traineeUserIds);
        if (traineeUserIds.Count == 0)
        {
            return Task.FromResult(CreditHolds.Nothing);
        }

        // In id order, so two holders of several trainees (a reactivation crediting a pause's completions) take them in the
        // same order. The lock node sits above the sort, so the rows are locked in the order they are returned.
        var userIds = traineeUserIds.Distinct(StringComparer.Ordinal).ToArray();
        return CreditHolds.HoldAsync(
            _dbContext,
            database => database.ExecuteSqlInterpolatedAsync(
                $"SELECT 1 FROM \"TraineeProfiles\" WHERE \"UserId\" = ANY({userIds}) ORDER BY \"Id\" FOR SHARE",
                cancellationToken),
            CreditBusy,
            cancellationToken);
    }
}
