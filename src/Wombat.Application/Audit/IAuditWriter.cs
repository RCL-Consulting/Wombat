using Wombat.Domain.Audit;

namespace Wombat.Application.Audit;

/// <summary>
/// Writes audit entries to persistent storage.
/// Use this interface for features that do not go through MediatR (rare):
/// authentication events, direct data-rights actions, etc.
/// </summary>
public interface IAuditWriter
{
    Task WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}
