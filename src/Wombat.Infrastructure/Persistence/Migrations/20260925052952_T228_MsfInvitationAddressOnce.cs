using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class T228_MsfInvitationAddressOnce : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // An address a campaign invited more than once keeps one invitation (T228), or the unique index below could
            // not be built: the one that has responded, else the first. The others go, with any response one of them
            // holds, which is the second response from one person that T228 is about. Only scenario data holds any
            // (dev campaign 11, the F2 browser check); compatibility is not a constraint (W-007). An erased address is
            // no one's, so it is left alone.
            migrationBuilder.Sql(
                """
                DELETE FROM "MsfInvitations" AS invitation
                USING (
                    SELECT "Id",
                           row_number() OVER (
                               PARTITION BY "CampaignId", lower("RespondentEmail")
                               ORDER BY "RespondedOn" IS NULL, "Id") AS position
                    FROM "MsfInvitations"
                    WHERE "RespondentEmail" IS NOT NULL
                ) AS ranked
                WHERE invitation."Id" = ranked."Id" AND ranked.position > 1;
                """);

            migrationBuilder.AddColumn<string>(
                name: "RespondentEmailKey",
                table: "MsfInvitations",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true,
                computedColumnSql: "lower(\"RespondentEmail\")",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_MsfInvitations_CampaignId_RespondentEmailKey",
                table: "MsfInvitations",
                columns: new[] { "CampaignId", "RespondentEmailKey" },
                unique: true,
                filter: "\"RespondentEmailKey\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MsfInvitations_CampaignId_RespondentEmailKey",
                table: "MsfInvitations");

            migrationBuilder.DropColumn(
                name: "RespondentEmailKey",
                table: "MsfInvitations");
        }
    }
}
