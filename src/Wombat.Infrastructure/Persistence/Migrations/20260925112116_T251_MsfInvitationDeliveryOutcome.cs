using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class T251_MsfInvitationDeliveryOutcome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // What became of each MSF link's mail (T251), reported by the mail worker. Nothing is backfilled: whether a
            // link mailed before the deploy arrived is not known, and guessing "sent" would hide exactly the links T251
            // is about. So an open campaign's links issued more than an hour before the deploy, and not answered, read as
            // not delivered, and the campaign page offers to resend them. Only scenario data holds any (W-007); a resend
            // keeps each old link working as the previous one.
            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryFailedOn",
                table: "MsfInvitations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryLinkSelector",
                table: "MsfInvitations",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SentOn",
                table: "MsfInvitations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_MsfInvitations_DeliveryOutcome",
                table: "MsfInvitations",
                sql: "\"SentOn\" IS NULL OR \"DeliveryFailedOn\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_MsfInvitations_DeliveryOutcome",
                table: "MsfInvitations");

            migrationBuilder.DropColumn(
                name: "DeliveryFailedOn",
                table: "MsfInvitations");

            migrationBuilder.DropColumn(
                name: "DeliveryLinkSelector",
                table: "MsfInvitations");

            migrationBuilder.DropColumn(
                name: "SentOn",
                table: "MsfInvitations");
        }
    }
}
