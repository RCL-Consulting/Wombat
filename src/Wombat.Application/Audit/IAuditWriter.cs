using Wombat.Domain.Audit;

namespace Wombat.Application.Audit;

/// <summary>
/// Writes audit entries to persistent storage.
/// Use this interface for features that do not go through MediatR (rare):
/// authentication events, direct data-rights actions, etc.
/// </summary>
public interface IAuditWriter
{
    /// <summary>
    /// Adds the entry to the request's DbContext and saves it. The save flushes every change the context is tracking,
    /// not only the entry: see <see cref="AuditPipelineBehavior{TRequest,TResponse}" /> for what that means for a
    /// command that throws.
    /// </summary>
    Task WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Discards every change the request's DbContext is tracking, then adds the entry and saves it alone. For a failed
    /// command whose changes the database refused, whether in the handler's own save or, where a library swallowed that
    /// refusal, in an ordinary write carrying them: the refused changes are still tracked, and saving them again would
    /// fail again, losing the entry. (T201)
    /// </summary>
    Task WriteDiscardingPendingChangesAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}
