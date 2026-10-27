using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// T312: an ended programme keeps the list it ended with (<c>TraineeProfile.EndedCurriculumJson</c>), so its record
    /// and its portfolio no longer change when the catalogue does after the end.
    /// </summary>
    /// <remarks>
    /// Adds the column, then gives every profile that has already ended on a recorded day its list, judged as the end's
    /// commands judge it (<c>ProgrammeItems.ReadEndedWithAsync</c>): each item of the profile's curriculum that is the
    /// trainee's (the College's, or their own institution's) whose EPA was in force at the close of the last day (or now,
    /// were that earlier), with the target the catalogue holds today. It is the catalogue as it stands, since it cannot say
    /// what it held on an earlier day: an item added after the end is kept, and an EPA paused after the end is kept, which
    /// is the point. The JSON is the shape <c>EndedCurriculumItem.Serialize</c> writes (camelCase, enums as integers).
    /// A profile ended on a day Wombat did not record (before T209) is left without a list, and is read from the catalogue
    /// as before, since there is no last day to judge it on. A running profile gets none. Nothing is live (CLAUDE.md):
    /// these are scenario rows, and a replay recreates them through the commands.
    /// </remarks>
    public partial class T312_EndedCurriculumList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EndedCurriculumJson",
                table: "TraineeProfiles",
                type: "jsonb",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "TraineeProfiles" AS p
                SET "EndedCurriculumJson" = COALESCE((
                    SELECT jsonb_agg(jsonb_build_object(
                               'curriculumItemId', ci."Id",
                               'epaId', ci."EpaId",
                               'isLocal', ci."OwningInstitutionId" IS NOT NULL,
                               'requiredCount', ci."RequiredCount",
                               'quotaPeriod', ci."QuotaPeriod",
                               'minimumLevelOrder', ci."MinimumLevelOrder",
                               'minimumLevelByStageJson', ci."MinimumLevelByStageJson",
                               'scaleId', ci."ScaleId",
                               'decisionCadence', ci."DecisionCadence",
                               'decisionIsOpportunistic', ci."DecisionIsOpportunistic")
                           ORDER BY ci."Id")
                    FROM "CurriculumItems" AS ci
                    JOIN "Epas" AS e ON e."Id" = ci."EpaId"
                    WHERE ci."CurriculumId" = p."CurriculumId"
                      AND (ci."OwningInstitutionId" IS NULL OR ci."OwningInstitutionId" = p."InstitutionId")
                      AND (e."IsActive"
                           OR (e."DeactivatedOn" IS NOT NULL
                               AND e."DeactivatedOn" > LEAST(
                                   now(),
                                   ((COALESCE(p."CompletedOn", p."DeactivatedOn") + 1)::timestamp - interval '2 hours')
                                       AT TIME ZONE 'UTC')))
                ), '[]'::jsonb)
                WHERE NOT p."IsActive"
                  AND COALESCE(p."CompletedOn", p."DeactivatedOn") IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndedCurriculumJson",
                table: "TraineeProfiles");
        }
    }
}
