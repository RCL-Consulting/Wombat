using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// T358, flow 06: the one migration of the build.
    /// <list type="bullet">
    /// <item><c>TraineeProfiles.AdmittedOn</c> (D1), the South African day a registrar was admitted, which "Nothing filed in
    /// 30 days" counts from when it is later than today − 30 (E5). Admission writes it from now on; every profile already
    /// stored is set to its <c>ProgrammeStartDate</c>, the nearest day it holds (no row recorded its admission).</item>
    /// <item><c>ActivityReminders</c> (Q4, C4): one row per reminder a member of staff sends the assessor a waiting request
    /// names. Its unique index on <c>(ActivityId, SentOnDay)</c> is the same-day block, one reminder per request per South
    /// African day whoever sends it; the second sender in the same instant meets it (D6). The key cascades with the
    /// activity, and nothing points back: a reminder never touches the activity row (C4c).</item>
    /// </list>
    /// </summary>
    public partial class T358_ActivityRemindersAndAdmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "AdmittedOn",
                table: "TraineeProfiles",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            // No stored profile recorded its admission. Its programme start is the nearest day it holds, and a registrar
            // admitted that long ago is read as having been in Wombat the whole window (D1).
            migrationBuilder.Sql("""UPDATE "TraineeProfiles" SET "AdmittedOn" = "ProgrammeStartDate";""");

            migrationBuilder.CreateTable(
                name: "ActivityReminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActivityId = table.Column<int>(type: "integer", nullable: false),
                    SentByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    AssessorUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    SentOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SentOnDay = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityReminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityReminders_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityReminders_ActivityId_SentOn",
                table: "ActivityReminders",
                columns: new[] { "ActivityId", "SentOn" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityReminders_ActivityId_SentOnDay",
                table: "ActivityReminders",
                columns: new[] { "ActivityId", "SentOnDay" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityReminders");

            migrationBuilder.DropColumn(
                name: "AdmittedOn",
                table: "TraineeProfiles");
        }
    }
}
