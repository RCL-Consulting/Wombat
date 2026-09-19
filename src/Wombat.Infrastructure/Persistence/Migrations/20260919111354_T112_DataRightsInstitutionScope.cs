using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class T112_DataRightsInstitutionScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InstitutionId",
                table: "DataRightsRequests",
                type: "integer",
                nullable: true);

            // Backfill from the requester's identity record. This is the same answer the runtime now
            // takes off the submitting principal's own claim — WombatUserClaimsPrincipalFactory issues
            // the institution_id claim straight from AspNetUsers."InstitutionId" — so a backfilled row
            // and one submitted a minute later carry the same stamp. A requester whose record has no
            // institution stays NULL, which leaves the request Administrator-only.
            migrationBuilder.Sql("""
                UPDATE "DataRightsRequests" d
                SET "InstitutionId" = u."InstitutionId"
                FROM "AspNetUsers" u
                WHERE u."Id" = d."RequesterUserId";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_DataRightsRequests_InstitutionId",
                table: "DataRightsRequests",
                column: "InstitutionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DataRightsRequests_InstitutionId",
                table: "DataRightsRequests");

            migrationBuilder.DropColumn(
                name: "InstitutionId",
                table: "DataRightsRequests");
        }
    }
}
