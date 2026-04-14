using Wombat.Application.Audit;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Audit;

namespace Wombat.Infrastructure.Audit;

public sealed class AuditWriter : IAuditWriter
{
    private readonly IApplicationDbContext _dbContext;

    public AuditWriter(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<AuditEntry>().Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
