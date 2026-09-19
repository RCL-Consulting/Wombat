using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class T101_ActivityScopeStamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InstitutionId",
                table: "Activities",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SpecialityId",
                table: "Activities",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubSpecialityId",
                table: "Activities",
                type: "integer",
                nullable: true);

            // Backfill from the subject's trainee profile. This is NOT an inference: it is the same
            // derivation ActivityService.ResolveSubjectScopeAsync performs at creation, down to the
            // tie-break (prefer the active profile, then the most recent), so a backfilled row and a
            // row created a minute later carry identical stamps. Subjects with no profile keep NULL,
            // which withholds scoped oversight rather than granting it.
            migrationBuilder.Sql("""
                UPDATE "Activities" a
                SET "InstitutionId"   = s."InstitutionId",
                    "SubSpecialityId" = s."SubSpecialityId",
                    "SpecialityId"    = s."SpecialityId"
                FROM (
                    SELECT DISTINCT ON (tp."UserId")
                           tp."UserId",
                           tp."InstitutionId",
                           c."SubSpecialityId",
                           ss."SpecialityId"
                    FROM "TraineeProfiles" tp
                    JOIN "Curricula" c        ON c."Id"  = tp."CurriculumId"
                    JOIN "SubSpecialities" ss ON ss."Id" = c."SubSpecialityId"
                    ORDER BY tp."UserId", tp."IsActive" DESC, tp."Id" DESC
                ) s
                WHERE a."SubjectUserId" = s."UserId";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Activities_InstitutionId",
                table: "Activities",
                column: "InstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_SpecialityId",
                table: "Activities",
                column: "SpecialityId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_SubSpecialityId",
                table: "Activities",
                column: "SubSpecialityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Activities_InstitutionId",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_SpecialityId",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_SubSpecialityId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "InstitutionId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "SpecialityId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "SubSpecialityId",
                table: "Activities");
        }
    }
}
