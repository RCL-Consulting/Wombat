using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class T283_AccountInvitationDeliveryOutcome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // What became of each account invitation's mail (T283), reported by the mail worker. Nothing is backfilled,
            // as T251 did not backfill MSF links: whether an invitation mailed before the deploy arrived is not known. So
            // every invitation still open at the deploy, issued more than an hour before it, reads as not delivered, and
            // the invitations list offers to resend it. Only scenario data holds any (W-007).
            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryFailedOn",
                table: "Invitations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryFailures",
                table: "Invitations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SentOn",
                table: "Invitations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Invitations_DeliveryFailures",
                table: "Invitations",
                sql: "\"DeliveryFailures\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Invitations_DeliveryOutcome",
                table: "Invitations",
                sql: "\"SentOn\" IS NULL OR \"DeliveryFailedOn\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Invitations_DeliveryFailures",
                table: "Invitations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Invitations_DeliveryOutcome",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "DeliveryFailedOn",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "DeliveryFailures",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "SentOn",
                table: "Invitations");
        }
    }
}
