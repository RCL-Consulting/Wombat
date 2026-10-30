using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Flow 14, data rights (T387, build-contracts § 0): the optional-processing opt-out goes (T318, Q6); a correction
    /// keeps who recorded it and names no target id (Q26, S11); a request carries Postgres's xmin as its concurrency token
    /// (F8); UnderReview, which nothing set, is gone (Q8).
    /// </summary>
    /// <inheritdoc />
    public partial class T387_DataRightsFlow14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // EF generated an AddColumn for "xmin" on DataRightsRequests and it has been REMOVED by hand, deliberately, as
            // T121's was: xmin is a PostgreSQL system column on every table, so adding it fails ("column name xmin
            // conflicts with a system column name"). DataRightsRequestConfiguration maps it as a shadow concurrency token
            // because it already exists; there is no DDL to write.

            // UnderReview (1) left DataRightsRequestStatus (Q8). Nothing ever set it, so no row should hold it; one that
            // does waits for a decision again.
            migrationBuilder.Sql("UPDATE \"DataRightsRequests\" SET \"Status\" = 0 WHERE \"Status\" = 1;");

            migrationBuilder.DropColumn(
                name: "TargetId",
                table: "DataRightsRectifications");

            migrationBuilder.DropColumn(
                name: "OptOutOfOptionalProcessing",
                table: "AspNetUsers");

            // A correction is applied when it is recorded (DataRightsRectification.Create). No page recorded one before
            // T387, so no row should lack its moment; one that does is given the request's decision moment, else now.
            migrationBuilder.Sql(
                "UPDATE \"DataRightsRectifications\" AS c SET \"AppliedOn\" = COALESCE(r.\"DecidedOn\", now()) " +
                "FROM \"DataRightsRequests\" AS r WHERE c.\"RequestId\" = r.\"Id\" AND c.\"AppliedOn\" IS NULL;");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AppliedOn",
                table: "DataRightsRectifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            // Who recorded it (S11). A row from before T387 has no recorder: the empty string, which no account holds.
            migrationBuilder.AddColumn<string>(
                name: "AppliedByUserId",
                table: "DataRightsRectifications",
                type: "character varying(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No DropColumn for "xmin": see Up. It is a system column and was never created here. UnderReview is not
            // restored: no row is moved back to it.

            migrationBuilder.DropColumn(
                name: "AppliedByUserId",
                table: "DataRightsRectifications");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AppliedOn",
                table: "DataRightsRectifications",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<Guid>(
                name: "TargetId",
                table: "DataRightsRectifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "OptOutOfOptionalProcessing",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
