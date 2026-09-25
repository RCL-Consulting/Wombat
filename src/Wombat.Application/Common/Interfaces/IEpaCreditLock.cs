namespace Wombat.Application.Common.Interfaces;

/// <summary>
/// Serialises a completion's credit against its EPA's deactivation or reactivation (T230), so that each ends with the
/// credit a rebuild would write.
/// </summary>
/// <remarks>
/// <para>
/// A completion credits an EPA's items only while the EPA is in force at the completion's moment (D48,
/// <c>CurriculumItemsInForce.InForceAt</c>), and reactivating an EPA credits the completions filed during its pause
/// (<c>ResumedEpaCredit</c>). Neither save writes anything the other reads, so before T230 two one-request races were
/// open. A completion that read the EPA as inactive, and saved after a reactivation had read its candidates, stayed
/// uncredited. A completion that read the EPA as active, and saved after a deactivation whose moment fell before its
/// own, kept credit a rebuild while the EPA is inactive takes away.
/// </para>
/// <para>
/// A hold is a transaction on the request's own context with a row lock on the EPA, taken before the EPA is read and
/// released when the caller's save commits. A completion holds the EPAs its credit judges <em>shared</em>, so
/// completions never wait for one another; a deactivation or a reactivation holds its EPA against every completion and
/// every other change (though not against a row that merely names the EPA), and reads the clock only once it has it.
/// Whichever comes second waits for the first to commit and then runs against what it wrote: nothing needs re-running,
/// and nobody is refused unless the wait outlasts the command timeout, when the request is refused as busy having
/// changed nothing. That closes both races:
/// </para>
/// <list type="bullet">
/// <item>A reactivation that waited for a completion finds it among the paused completions and credits it. A completion
/// that waited for a reactivation reads the EPA as active and credits it itself.</item>
/// <item>A deactivation that waited for a completion takes its moment after the completion committed, so a completion
/// it let credit is one whose moment falls before the pause, which is what a rebuild judges. A completion that waited
/// for a deactivation reads the pause and judges its own moment against it, as a rebuild does.</item>
/// </list>
/// <para>
/// Only on PostgreSQL. Every other provider (the unit suites' in-memory one) gets a hold that does nothing, because
/// it has no transactions to hold a lock in and no concurrent requests to hold it against.
/// </para>
/// </remarks>
public interface IEpaCreditLock
{
    /// <summary>
    /// For a change to the EPA itself: holds it against completions and other changes until the returned hold is committed
    /// or disposed. Take it before reading the EPA, and read the clock after it: an EPA read, or a moment taken, before the
    /// hold may be one a completion has since overtaken. Anything that calls <c>Epa.Deactivate</c> or
    /// <c>Epa.Reactivate</c> takes it (<c>EpaPauseWritePathTests</c>).
    /// </summary>
    /// <exception cref="InvalidOperationException">The wait outlasted the command timeout; nothing was held.</exception>
    Task<IEpaCreditHold> HoldForChangeAsync(int epaId, CancellationToken cancellationToken);

    /// <summary>
    /// For a completion's credit: holds the EPAs it judges shared until the returned hold is committed or disposed. Take
    /// it before reading whether they are in force. No EPAs, no hold.
    /// </summary>
    /// <exception cref="InvalidOperationException">The wait outlasted the command timeout; nothing was held.</exception>
    Task<IEpaCreditHold> HoldForCreditAsync(IReadOnlyCollection<int> epaIds, CancellationToken cancellationToken);
}

/// <summary>
/// A held EPA lock: commit it after the caller's save. Disposing it without committing rolls back whatever was saved
/// under it, which is what a request that throws needs: the audit pipeline's catch then writes into a context that has
/// no transaction left open.
/// </summary>
public interface IEpaCreditHold : IAsyncDisposable
{
    /// <summary>Commits the save made under the hold, and releases the lock. A no-op for a hold that opened nothing.</summary>
    Task CommitAsync(CancellationToken cancellationToken);
}
