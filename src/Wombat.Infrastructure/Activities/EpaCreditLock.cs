using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Infrastructure.Activities;

/// <summary>
/// Row locks on <c>Epas</c>, held in a transaction on the request's own context until its save commits (T230). See
/// <see cref="IEpaCreditLock" /> for the two races they close.
/// </summary>
/// <remarks>
/// <para>
/// <c>FOR SHARE</c> for a completion and <c>FOR NO KEY UPDATE</c> for a change to the EPA. Shared locks do not conflict
/// with one another, so two completions on one national EPA never wait for each other; each conflicts with the change's
/// lock, and two changes conflict with each other. An optimistic token on the EPA row was the alternative, and was
/// rejected: the completion would have had to write the row to move it, so every two completions on one EPA, anywhere in
/// the country, would have conflicted and one been refused.
/// </para>
/// <para>
/// Not <c>FOR UPDATE</c>, which also conflicts with <c>FOR KEY SHARE</c>, the lock a foreign-key check takes on the row
/// it references. Under it every insert naming the EPA (a curriculum item, an entrustment decision, a pending one, an MSF
/// campaign's EPA) would wait for the change to commit, and a reactivation plans every completion of its pause inside its
/// hold. <c>FOR NO KEY UPDATE</c> is also what the change's own <c>UPDATE</c> takes, because it changes no key the
/// foreign keys use (the id; the code's unique indexes are partial, so they do not count).
/// </para>
/// <para>
/// A wait is bounded by the command timeout: Npgsql's 30 seconds, which nothing in Wombat changes. A hold that gives up
/// is refused with a message saying the EPA is busy, and the request has changed nothing, because every caller takes its
/// hold before its first mutation. The server's own <c>lock_timeout</c> and <c>statement_timeout</c>, if an operator
/// sets them, are refused the same way.
/// </para>
/// <para>
/// The transaction is <c>READ COMMITTED</c>, named rather than inherited from the server's default: each statement
/// after the lock reads what was committed before it, which is what makes a request that waited see the other's work.
/// Under <c>REPEATABLE READ</c> the waiter would instead be refused with a serialization failure.
/// </para>
/// <para>
/// A context that already has a transaction open is joined: the lock is taken in it, and the hold returned does
/// nothing, because the transaction's owner commits it. The plumbing is <see cref="CreditHolds" />, shared with
/// <see cref="TraineeCreditLock" /> (T281), whose hold joins this one's on the completion's path and the reactivation's.
/// </para>
/// </remarks>
public sealed class EpaCreditLock : IEpaCreditLock
{
    private readonly DbContext? _dbContext;

    /// <param name="dbContext">
    /// The request's context: the lock must be held on the connection its save goes through. Anything that is not an EF
    /// context, or a context on another provider, gets holds that do nothing.
    /// </param>
    public EpaCreditLock(IApplicationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext as DbContext;
    }

    /// <summary>What a change to an EPA is refused with when its hold gives up waiting.</summary>
    public const string ChangeBusy =
        "Activities are still being completed against this EPA, or it is being changed elsewhere, so nothing was saved. " +
        "Save again in a moment.";

    /// <summary>What a completion is refused with when its hold gives up waiting.</summary>
    public const string CreditBusy =
        "An EPA this activity counts towards is being changed right now, so nothing was saved. Try again in a moment.";

    public Task<ICreditHold> HoldForChangeAsync(int epaId, CancellationToken cancellationToken)
        => HoldAsync(
            database => database.ExecuteSqlInterpolatedAsync(
                $"SELECT 1 FROM \"Epas\" WHERE \"Id\" = {epaId} FOR NO KEY UPDATE",
                cancellationToken),
            ChangeBusy,
            cancellationToken);

    public Task<ICreditHold> HoldForCreditAsync(IReadOnlyCollection<int> epaIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(epaIds);
        if (epaIds.Count == 0)
        {
            return Task.FromResult(CreditHolds.Nothing);
        }

        // In id order, so two holders of several EPAs take them in the same order. The lock node sits above the sort,
        // so the rows are locked in the order they are returned.
        var ids = epaIds.Distinct().Order().ToArray();
        return HoldAsync(
            database => database.ExecuteSqlInterpolatedAsync(
                $"SELECT 1 FROM \"Epas\" WHERE \"Id\" = ANY({ids}) ORDER BY \"Id\" FOR SHARE",
                cancellationToken),
            CreditBusy,
            cancellationToken);
    }

    private Task<ICreditHold> HoldAsync(
        Func<DatabaseFacade, Task<int>> takeLock,
        string busy,
        CancellationToken cancellationToken)
        => CreditHolds.HoldAsync(_dbContext, takeLock, busy, cancellationToken);
}
