using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AssessorTrainingStatusEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TrainingStatus",
                table: "AssessorProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Backfill: any assessor with a recorded training-completion date is fully Trained (3).
            // Assessors with no recorded date keep the NotStarted default (0).
            migrationBuilder.Sql(
                "UPDATE \"AssessorProfiles\" SET \"TrainingStatus\" = 3 WHERE \"TrainingCompletedOn\" IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrainingStatus",
                table: "AssessorProfiles");
        }
    }
}
