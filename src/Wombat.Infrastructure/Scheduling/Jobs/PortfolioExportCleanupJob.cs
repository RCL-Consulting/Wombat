using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.Reporting;

namespace Wombat.Infrastructure.Scheduling.Jobs;

public sealed class PortfolioExportCleanupJob : IScheduledJob
{
    private readonly IServiceScopeFactory _scopeFactory;

    public PortfolioExportCleanupJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public string Key => "portfolio-export-cleanup";
    public string CronExpression => "0 2 * * *";
    public string Description => "Deletes portfolio export records older than 90 days (daily at 02:00 UTC).";

    public async Task ExecuteAsync(ScheduledJobContext context, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var cutoff = context.UtcNow.AddDays(-90);

        var expiredExports = dbContext.Set<PortfolioExport>()
            .Where(e => e.ExportedOn < cutoff);

        var count = expiredExports.Count();
        if (count == 0)
        {
            context.Logger.LogInformation("PortfolioExportCleanupJob: no expired exports found.");
            return;
        }

        dbContext.Set<PortfolioExport>().RemoveRange(expiredExports);
        await dbContext.SaveChangesAsync(cancellationToken);

        context.Logger.LogInformation("PortfolioExportCleanupJob: deleted {Count} expired export records.", count);
    }
}
