using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class T119_ActivityObservedOn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "ObservedOn",
                table: "Activities",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<int>(
                name: "ObservedOnSource",
                table: "Activities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Backfill. EF's AddColumn default would leave every existing row at 0001-01-01 claiming
            // ObservedOnSource = Declared (0), which is two lies: the date is wrong AND it asserts a
            // clinician stated it. Every existing row is dated by the audit clock, so say so.
            //
            // This deliberately does NOT read DataJson, even though four seeds carry a real observed_on.
            // Program.cs migrates BEFORE ActivityTypeSeedRefresher runs, so at this moment no published
            // schema version declares the pointer yet and there is nothing to resolve against. Restamping
            // from the pinned schema is the app's job, through the one resolver in ActivityService, not a
            // second implementation in SQL.
            migrationBuilder.Sql("""
                UPDATE "Activities"
                SET "ObservedOn"       = ("CreatedOn" AT TIME ZONE 'UTC')::date,
                    "ObservedOnSource" = 1;
                """);

            // Turns the silent failure into a loud one. An unstamped row dates to 0001-01-01, which
            // precedes every ProgrammeStartDate, so GetStage returns null and the credit gate quietly
            // falls back to the flat MinimumLevelOrder instead of the stage minimum — a wrong answer that
            // looks like a right one. This makes such a row impossible to persist.
            migrationBuilder.Sql("""
                ALTER TABLE "Activities"
                ADD CONSTRAINT "CK_Activities_ObservedOn_IsStamped"
                CHECK ("ObservedOn" > DATE '0001-01-01');
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Activities_ObservedOn",
                table: "Activities",
                column: "ObservedOn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Activities" DROP CONSTRAINT IF EXISTS "CK_Activities_ObservedOn_IsStamped";
                """);

            migrationBuilder.DropIndex(
                name: "IX_Activities_ObservedOn",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ObservedOn",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ObservedOnSource",
                table: "Activities");
        }
    }
}
