using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// T307 (D51): an appeal is resolved as Dismissed (2) or Remitted (3). The third outcome, Upheld (1), did what Dismissed
    /// does, leaving the appealed decision in force, so every stored Upheld is rewritten as what the engine did with it,
    /// Dismissed, before the check refuses 1 from then on.
    /// </summary>
    /// <remarks>
    /// Down drops the check and leaves the rewritten rows Dismissed: which of them had been Upheld is not kept, and there is
    /// no real data to keep it for (CLAUDE.md § Nothing is live).
    /// </remarks>
    public partial class T307_AppealOutcomesDismissedOrRemitted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""UPDATE "CommitteeAppeals" SET "Outcome" = 2 WHERE "Outcome" = 1;""");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CommitteeAppeals_Outcome",
                table: "CommitteeAppeals",
                sql: "\"Outcome\" IS NULL OR \"Outcome\" IN (2, 3)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_CommitteeAppeals_Outcome",
                table: "CommitteeAppeals");
        }
    }
}
