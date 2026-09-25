using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// T196, D48: when an EPA's current pause began, so credit is judged at each completion's moment rather than against
    /// the catalogue as it stands (<c>CurriculumItemsInForce.InForceAt</c>).
    /// </summary>
    /// <remarks>
    /// HAND-EDIT: the backfill between the column and the check. An EPA already inactive has no recorded moment, and the
    /// check refuses an inactive EPA without one. It gets its <c>CreatedOn</c>, which places every completion against it
    /// inside the pause: a rebuild credits it nothing, exactly as the rule before T196 did for an EPA inactive at the
    /// time, and reactivating it credits every completion, as it will for any other pause. Guessing a later moment would
    /// credit, on the next rebuild, completions the live path paused. No inactive EPA is expected on dev (inferred: the
    /// T158 browser check reactivated the one it deactivated, PAED-014) or on production (no catalogue), so this is the
    /// rule for the case, not a repair.
    /// </remarks>
    public partial class T196_EpaDeactivatedOn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivatedOn",
                table: "Epas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql("""UPDATE "Epas" SET "DeactivatedOn" = "CreatedOn" WHERE NOT "IsActive";""");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Epas_DeactivatedOn",
                table: "Epas",
                sql: "(\"IsActive\" AND \"DeactivatedOn\" IS NULL) OR (NOT \"IsActive\" AND \"DeactivatedOn\" IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Epas_DeactivatedOn",
                table: "Epas");

            migrationBuilder.DropColumn(
                name: "DeactivatedOn",
                table: "Epas");
        }
    }
}
