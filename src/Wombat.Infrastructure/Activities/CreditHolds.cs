using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Infrastructure.Activities;

/// <summary>
/// How a credit lock holds its rows: in a <c>READ COMMITTED</c> transaction on the request's own context, until the
/// caller commits the hold after its save. Shared by <see cref="EpaCreditLock" /> (T230) and <see cref="TraineeCreditLock" />
/// (T281), so the two cannot differ in how they open, join, give up or release.
/// </summary>
/// <remarks>
/// <para>
/// A context that already has a transaction open is joined: the lock is taken in it, and the hold returned does nothing,
/// because the transaction's owner commits it. That is how a completion holds its EPAs and then its trainee in one
/// transaction, and how a reactivation holds its trainees inside the EPA change's.
/// </para>
/// <para>
/// A wait is bounded by the command timeout: Npgsql's 30 seconds, which nothing in Wombat changes. A hold that gives up is
/// refused with the lock's busy message, and the request has changed nothing, because every caller takes its hold before
/// its first mutation. The server's own <c>lock_timeout</c> and <c>statement_timeout</c>, if an operator sets them, are
/// refused the same way. Any other provider gets <see cref="Nothing" />.
/// </para>
/// </remarks>
internal static class CreditHolds
{
    /// <summary>A hold that opened nothing: its commit and its disposal do nothing.</summary>
    public static ICreditHold Nothing { get; } = new Held(null);

    public static async Task<ICreditHold> HoldAsync(
        DbContext? dbContext,
        Func<DatabaseFacade, Task<int>> takeLock,
        string busy,
        CancellationToken cancellationToken)
    {
        if (dbContext is null || !dbContext.Database.IsNpgsql())
        {
            return Nothing;
        }

        var database = dbContext.Database;
        if (database.CurrentTransaction is not null)
        {
            await TakeAsync(takeLock, database, busy);
            return Nothing;
        }

        var transaction = await database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        try
        {
            await TakeAsync(takeLock, database, busy);
        }
        catch
        {
            await transaction.DisposeAsync();
            throw;
        }

        return new Held(transaction);
    }

    /// <summary>
    /// Takes the lock, and turns giving up on it into a refusal the pages can show: they print an exception's message, and
    /// Npgsql's for a command timeout is "Exception while reading from stream".
    /// </summary>
    private static async Task TakeAsync(Func<DatabaseFacade, Task<int>> takeLock, DatabaseFacade database, string busy)
    {
        try
        {
            await takeLock(database);
        }
        catch (NpgsqlException exception) when (GaveUpWaiting(exception))
        {
            throw new InvalidOperationException(busy, exception);
        }
    }

    /// <summary>
    /// The command timeout ran out (Npgsql cancels the statement and reports a <see cref="TimeoutException" />), or the
    /// server's <c>lock_timeout</c> or <c>statement_timeout</c> did. A cancellation the caller asked for is not this:
    /// Npgsql reports it as an <see cref="OperationCanceledException" />.
    /// </summary>
    private static bool GaveUpWaiting(NpgsqlException exception)
        => exception.InnerException is TimeoutException
           || exception is PostgresException { SqlState: PostgresErrorCodes.LockNotAvailable or PostgresErrorCodes.QueryCanceled };

    private sealed class Held(IDbContextTransaction? transaction) : ICreditHold
    {
        public Task CommitAsync(CancellationToken cancellationToken)
            => transaction is null ? Task.CompletedTask : transaction.CommitAsync(cancellationToken);

        // Rolls back a transaction that was not committed, and does nothing the second time.
        public ValueTask DisposeAsync() => transaction?.DisposeAsync() ?? ValueTask.CompletedTask;
    }
}
