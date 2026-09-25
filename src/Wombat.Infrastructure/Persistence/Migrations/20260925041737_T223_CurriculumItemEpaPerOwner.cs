using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// T223: an EPA is on a curriculum once for each institution's trainees, not once for the whole curriculum. Two national
    /// items never share an EPA, nor two items of one institution's own (the two unique indexes, which the model carries);
    /// a national item and an institution's own item never share one either (the exclusion constraint, which it cannot);
    /// but two institutions' own items may, since no trainee is measured against both.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The exclusion constraint compares each column as a one-value range, with <c>&amp;&amp;</c> (overlaps), because a
    /// range has a GiST operator class built in and a plain integer needs the <c>btree_gist</c> extension. An owner of
    /// NULL gives <c>int4range(NULL, NULL)</c>, which is unbounded, so a national item overlaps every owner: it conflicts
    /// with another national item and with every institution's own item on its EPA, while two institutions' single-value
    /// ranges overlap only when they are the same institution.
    /// </para>
    /// <para>
    /// No stored row can violate it: until now the curriculum and EPA alone were unique.
    /// </para>
    /// </remarks>
    public partial class T223_CurriculumItemEpaPerOwner : Migration
    {
        private const string ExclusionConstraint = "EX_CurriculumItems_EpaOncePerInstitution";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CurriculumItems_CurriculumId_EpaId",
                table: "CurriculumItems");

            migrationBuilder.CreateIndex(
                name: "UX_CurriculumItems_Local_Curriculum_Epa_Owner",
                table: "CurriculumItems",
                columns: new[] { "CurriculumId", "EpaId", "OwningInstitutionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_CurriculumItems_National_Curriculum_Epa",
                table: "CurriculumItems",
                columns: new[] { "CurriculumId", "EpaId" },
                unique: true,
                filter: "\"OwningInstitutionId\" IS NULL");

            migrationBuilder.Sql($"""
                ALTER TABLE "CurriculumItems"
                    ADD CONSTRAINT "{ExclusionConstraint}"
                    EXCLUDE USING gist (
                        (int4range("CurriculumId", "CurriculumId", '[]')) WITH &&,
                        (int4range("EpaId", "EpaId", '[]')) WITH &&,
                        (int4range("OwningInstitutionId", "OwningInstitutionId", '[]')) WITH &&);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""ALTER TABLE "CurriculumItems" DROP CONSTRAINT "{ExclusionConstraint}";""");

            migrationBuilder.DropIndex(
                name: "UX_CurriculumItems_Local_Curriculum_Epa_Owner",
                table: "CurriculumItems");

            migrationBuilder.DropIndex(
                name: "UX_CurriculumItems_National_Curriculum_Epa",
                table: "CurriculumItems");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumItems_CurriculumId_EpaId",
                table: "CurriculumItems",
                columns: new[] { "CurriculumId", "EpaId" },
                unique: true);
        }
    }
}
