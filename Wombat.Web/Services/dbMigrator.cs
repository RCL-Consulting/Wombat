using Microsoft.EntityFrameworkCore;
using Wombat.Data;

namespace Wombat.Web.Services
{
    public class dbMigrator : IHostedService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<dbMigrator> logger;

        public dbMigrator(IServiceProvider serviceProvider, ILogger<dbMigrator> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        public async Task Migrate()
        {
            var scope = serviceProvider.CreateScope();
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                using (var scope = serviceProvider.CreateAsyncScope())
                {
                    using (var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                    {
                        var hasUnAppliedMigrations = await db.Database.GetPendingMigrationsAsync(cancellationToken);
                        if (hasUnAppliedMigrations.Any())
                        {
                            await db.Database.MigrateAsync(cancellationToken);
                        }
                        else
                        {
                            logger.LogInformation("No pending migrations.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred migrating the database.");
            }           
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("dbMigrator is stopping.");
            await Task.CompletedTask;
        }
    }
}
