using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// T379 (flow 13, T313 with T287's verify item; build-contracts § 0): the scheduled job <c>portfolio-export-cleanup</c>
    /// is retired, so its definition and its runs go. It deleted every portfolio export record older than 90 days, and a
    /// genuine PDF then stopped verifying: the record is what Verify checks a file against. Lane V removes the job's class
    /// and its registration; this removes what the scheduler wrote for it (<c>ScheduledJobHost.SeedDefinitionsAsync</c>
    /// writes a definition for every registered job and removes none), so /admin/jobs lists eight jobs, not a ninth that
    /// never runs again.
    /// </summary>
    /// <remarks>
    /// Data only: the model does not change, so the snapshot does not either. The one migration flow 13 needs; no lane adds
    /// another. Down writes nothing back: a definition is the scheduler's to write at startup for a job that is registered,
    /// and the runs were a history of a job that no longer exists. Nothing is live (CLAUDE.md).
    /// </remarks>
    public partial class T379_PortfolioExportCleanupRetired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DELETE FROM "ScheduledJobRuns" WHERE "Key" = 'portfolio-export-cleanup';""");
            migrationBuilder.Sql("""DELETE FROM "ScheduledJobDefinitions" WHERE "Key" = 'portfolio-export-cleanup';""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
