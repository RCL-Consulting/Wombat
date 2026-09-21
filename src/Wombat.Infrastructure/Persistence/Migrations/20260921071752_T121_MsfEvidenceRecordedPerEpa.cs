using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class T121_MsfEvidenceRecordedPerEpa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // EF generated an AddColumn for "xmin" here and it has been REMOVED by hand, deliberately.
            // xmin is a PostgreSQL SYSTEM column present on every table, so adding it fails with
            // "column name xmin conflicts with a system column name". The model maps it as a shadow
            // concurrency token (MsfCampaignConfiguration) precisely because it already exists; there is
            // no DDL to write. Nothing else in Up depends on it. (T121)

            migrationBuilder.AddColumn<DateTime>(
                name: "RecordedOn",
                table: "MsfCampaignEpas",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No DropColumn for "xmin": see Up. It is a system column and was never created here.

            migrationBuilder.DropColumn(
                name: "RecordedOn",
                table: "MsfCampaignEpas");
        }
    }
}
