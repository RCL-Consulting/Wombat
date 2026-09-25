namespace Wombat.Application.Common.Interfaces;

/// <summary>
/// A held credit lock (<see cref="IEpaCreditLock" />, T230; <see cref="ITraineeCreditLock" />, T281): commit it after the
/// caller's save. Disposing it without committing rolls back whatever was saved under it, which is what a request that
/// throws needs: the audit pipeline's catch then writes into a context that has no transaction left open.
/// </summary>
/// <remarks>
/// A hold taken while the context already has a transaction open (another hold's) joins it and opens nothing: its commit
/// and its disposal do nothing, and the lock it took is released when the transaction's owner commits or rolls back. So a
/// caller holding two locks commits both after its save, in either order, and disposes both.
/// </remarks>
public interface ICreditHold : IAsyncDisposable
{
    /// <summary>Commits the save made under the hold, and releases the lock. A no-op for a hold that opened nothing.</summary>
    Task CommitAsync(CancellationToken cancellationToken);
}
