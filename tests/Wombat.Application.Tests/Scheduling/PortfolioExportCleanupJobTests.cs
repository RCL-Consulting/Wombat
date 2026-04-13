using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.Reporting;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling.Jobs;

namespace Wombat.Application.Tests.Scheduling;

public sealed class PortfolioExportCleanupJobTests
{
    [Fact]
    public async Task ExecuteAsync_DeletesOldExports()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(dbName));
        services.AddScoped<IApplicationDbContext>(p => p.GetRequiredService<ApplicationDbContext>());
        var provider = services.BuildServiceProvider();

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Set<PortfolioExport>().Add(new PortfolioExport
            {
                TraineeUserId = "t1",
                ExportedByUserId = "a1",
                ExportedOn = DateTime.UtcNow.AddDays(-100),
                ContentHash = "old-hash",
                FileName = "old.pdf"
            });
            db.Set<PortfolioExport>().Add(new PortfolioExport
            {
                TraineeUserId = "t2",
                ExportedByUserId = "a1",
                ExportedOn = DateTime.UtcNow.AddDays(-10),
                ContentHash = "recent-hash",
                FileName = "recent.pdf"
            });
            await db.SaveChangesAsync();
        }

        var job = new PortfolioExportCleanupJob(provider.GetRequiredService<IServiceScopeFactory>());
        var context = new ScheduledJobContext(DateTime.UtcNow, NullLogger.Instance);

        await job.ExecuteAsync(context, CancellationToken.None);

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var remaining = await db.Set<PortfolioExport>().ToListAsync();
            remaining.Should().ContainSingle();
            remaining[0].ContentHash.Should().Be("recent-hash");
        }
    }
}
