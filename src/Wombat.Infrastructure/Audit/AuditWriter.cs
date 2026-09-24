using Wombat.Application.Audit;
using Wombat.Domain.Audit;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Audit;

/// <summary>
/// Writes through the request's scoped <see cref="ApplicationDbContext" />, the same instance the handler used.
/// </summary>
public sealed class AuditWriter : IAuditWriter
{
    private readonly ApplicationDbContext _dbContext;

    public AuditWriter(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<AuditEntry>().Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task WriteDiscardingPendingChangesAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        // Clear() detaches every tracked entity, so nothing the handler staged is sent with the entry: not the changes
        // the database refused, and not anything a later save in this scope would otherwise pick up. (T201)
        _dbContext.ChangeTracker.Clear();

        _dbContext.Set<AuditEntry>().Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
