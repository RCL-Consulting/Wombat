using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class T258_CommitteeReviewWithdrawn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WithdrawalReason",
                table: "CommitteeReviews",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WithdrawnOn",
                table: "CommitteeReviews",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_CommitteeReviews_Withdrawn",
                table: "CommitteeReviews",
                sql: "(\"State\" = 7 AND \"WithdrawnOn\" IS NOT NULL AND \"WithdrawalReason\" IS NOT NULL) OR (\"State\" <> 7 AND \"WithdrawnOn\" IS NULL AND \"WithdrawalReason\" IS NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_CommitteeReviews_Withdrawn",
                table: "CommitteeReviews");

            migrationBuilder.DropColumn(
                name: "WithdrawalReason",
                table: "CommitteeReviews");

            migrationBuilder.DropColumn(
                name: "WithdrawnOn",
                table: "CommitteeReviews");
        }
    }
}
