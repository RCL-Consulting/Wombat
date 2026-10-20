using Microsoft.EntityFrameworkCore;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Identity;

/// <summary>
/// One database transaction around work that writes through Identity's stores and the context they share
/// (<c>AddEntityFrameworkStores&lt;ApplicationDbContext&gt;</c>): every <c>UserManager</c> call saves at once, so without it
/// each step commits on its own. The accept (T373, flow 11, F10) and the admission (T373 fix pass, B2) run in it.
/// </summary>
internal static class IdentityTransaction
{
    /// <summary>
    /// Runs <paramref name="work" /> in one transaction on <paramref name="dbContext" />, committed only when it returns. On
    /// a fault it is rolled back, and what the work left tracked is discarded: the failed command's audit row is saved
    /// through the same context (<c>AuditPipelineBehavior</c>, T201), and would otherwise commit what the rollback undid, or
    /// a change not yet saved. Inside a transaction already open the work joins it; a provider with no transactions (the
    /// in-memory one the unit tests use) runs it as it is.
    /// </summary>
    public static async Task<T> RunAsync<T>(
        ApplicationDbContext dbContext,
        Func<CancellationToken, Task<T>> work,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(work);

        var database = dbContext.Database;
        if (!database.IsRelational() || database.CurrentTransaction is not null)
        {
            return await work(cancellationToken);
        }

        await using var transaction = await database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await work(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            dbContext.ChangeTracker.Clear();
            throw;
        }
    }
}
