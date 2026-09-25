using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class T221_CatalogueSeedKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SeedKey",
                table: "SubSpecialities",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeedKey",
                table: "Specialities",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeedKey",
                table: "Epas",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeedKey",
                table: "EntrustmentScales",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeedKey",
                table: "Curricula",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeedKey",
                table: "Colleges",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            // HAND-EDIT. Stamp the catalogue's rows on a database that already holds them, found by the lookups
            // PaediatricCatalogueSeeder used until T221, so the next boot finds each by its key and creates nothing. On a
            // fresh database it touches nothing, because the seeders run after migrations and write the keys themselves.
            migrationBuilder.Sql(BuildSeedKeyStamp());

            migrationBuilder.CreateIndex(
                name: "IX_SubSpecialities_SeedKey",
                table: "SubSpecialities",
                column: "SeedKey",
                unique: true,
                filter: "\"SeedKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Specialities_SeedKey",
                table: "Specialities",
                column: "SeedKey",
                unique: true,
                filter: "\"SeedKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Epas_SeedKey",
                table: "Epas",
                column: "SeedKey",
                unique: true,
                filter: "\"SeedKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EntrustmentScales_SeedKey",
                table: "EntrustmentScales",
                column: "SeedKey",
                unique: true,
                filter: "\"SeedKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Curricula_SeedKey",
                table: "Curricula",
                column: "SeedKey",
                unique: true,
                filter: "\"SeedKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Colleges_SeedKey",
                table: "Colleges",
                column: "SeedKey",
                unique: true,
                filter: "\"SeedKey\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubSpecialities_SeedKey",
                table: "SubSpecialities");

            migrationBuilder.DropIndex(
                name: "IX_Specialities_SeedKey",
                table: "Specialities");

            migrationBuilder.DropIndex(
                name: "IX_Epas_SeedKey",
                table: "Epas");

            migrationBuilder.DropIndex(
                name: "IX_EntrustmentScales_SeedKey",
                table: "EntrustmentScales");

            migrationBuilder.DropIndex(
                name: "IX_Curricula_SeedKey",
                table: "Curricula");

            migrationBuilder.DropIndex(
                name: "IX_Colleges_SeedKey",
                table: "Colleges");

            migrationBuilder.DropColumn(
                name: "SeedKey",
                table: "SubSpecialities");

            migrationBuilder.DropColumn(
                name: "SeedKey",
                table: "Specialities");

            migrationBuilder.DropColumn(
                name: "SeedKey",
                table: "Epas");

            migrationBuilder.DropColumn(
                name: "SeedKey",
                table: "EntrustmentScales");

            migrationBuilder.DropColumn(
                name: "SeedKey",
                table: "Curricula");

            migrationBuilder.DropColumn(
                name: "SeedKey",
                table: "Colleges");
        }

        /// <summary>The fifteen EPA codes of CPSA EPA v11.1, frozen here: the migration cannot read the seed file.</summary>
        internal static readonly string[] CatalogueEpaCodes =
        [
            "PAED-001", "PAED-002", "PAED-003", "PAED-004", "PAED-005",
            "PAED-006", "PAED-007", "PAED-008", "PAED-009", "PAED-010",
            "PAED-011", "PAED-012", "PAED-013", "PAED-014", "PAED-015",
        ];

        /// <summary>
        /// Gives each catalogue row its seed key, finding it exactly as the seeder did before T221 and guessing nothing.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>The College by its short code <c>CPSA</c>; the scale by its name.</item>
        /// <item>The speciality and sub-speciality by the name <c>Paediatrics</c>, each under the row stamped before it, so
        /// another College's Paediatrics or a Neonatology sub-speciality is not matched.</item>
        /// <item>Only the fifteen v11.1 codes, only national EPAs, only in the stamped sub-speciality. A CollegeAdmin's own
        /// PAED-016 and an institution's local PAED-001 keep no key.</item>
        /// <item>Only <c>Paediatric EPA Curriculum</c> at version 11.1 in the stamped sub-speciality: a clone at another
        /// version, another curriculum of the discipline and another discipline's curriculum of the same name keep none.</item>
        /// <item>Only where the key is still null, so a replay (on a restored dump, say) changes nothing.</item>
        /// </list>
        /// A row an administrator had already renamed is not found, and stays without a key. The seeder then announces it
        /// at every startup and creates nothing in its place.
        /// </remarks>
        internal static string BuildSeedKeyStamp()
        {
            var codes = string.Join(", ", CatalogueEpaCodes.Select(code => $"'{code}'"));

            return $"""
                UPDATE "Colleges"
                SET "SeedKey" = 'cpsa'
                WHERE "ShortCode" = 'CPSA'
                  AND "SeedKey" IS NULL;

                UPDATE "Specialities" AS s
                SET "SeedKey" = 'cpsa:paediatrics'
                FROM "Colleges" AS c
                WHERE s."CollegeId" = c."Id"
                  AND c."SeedKey" = 'cpsa'
                  AND s."Name" = 'Paediatrics'
                  AND s."SeedKey" IS NULL;

                UPDATE "SubSpecialities" AS ss
                SET "SeedKey" = 'cpsa:paediatrics:paediatrics'
                FROM "Specialities" AS s
                WHERE ss."SpecialityId" = s."Id"
                  AND s."SeedKey" = 'cpsa:paediatrics'
                  AND ss."Name" = 'Paediatrics'
                  AND ss."SeedKey" IS NULL;

                UPDATE "EntrustmentScales"
                SET "SeedKey" = 'cpsa:scale:v11.1'
                WHERE "Name" = 'CPSA Paediatric Entrustment Scale v11.1'
                  AND "SeedKey" IS NULL;

                UPDATE "Epas" AS e
                SET "SeedKey" = 'cpsa:paediatrics:epa:' || e."Code"
                FROM "SubSpecialities" AS ss
                WHERE e."SubSpecialityId" = ss."Id"
                  AND ss."SeedKey" = 'cpsa:paediatrics:paediatrics'
                  AND e."OwningInstitutionId" IS NULL
                  AND e."Code" IN ({codes})
                  AND e."SeedKey" IS NULL;

                UPDATE "Curricula" AS cu
                SET "SeedKey" = 'cpsa:paediatrics:curriculum:v11.1'
                FROM "SubSpecialities" AS ss
                WHERE cu."SubSpecialityId" = ss."Id"
                  AND ss."SeedKey" = 'cpsa:paediatrics:paediatrics'
                  AND cu."Name" = 'Paediatric EPA Curriculum'
                  AND cu."Version" = '11.1'
                  AND cu."SeedKey" IS NULL;
                """;
        }
    }
}
