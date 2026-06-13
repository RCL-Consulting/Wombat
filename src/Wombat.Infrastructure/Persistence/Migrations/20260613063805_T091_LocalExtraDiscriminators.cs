using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class T091_LocalExtraDiscriminators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Epas_SubSpecialityId_Code",
                table: "Epas");

            migrationBuilder.AddColumn<int>(
                name: "OwningInstitutionId",
                table: "Epas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OwningInstitutionId",
                table: "CurriculumItems",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Epas_OwningInstitutionId",
                table: "Epas",
                column: "OwningInstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_Epas_SubSpecialityId_Code",
                table: "Epas",
                columns: new[] { "SubSpecialityId", "Code" },
                unique: true,
                filter: "\"OwningInstitutionId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Epas_SubSpecialityId_OwningInstitutionId_Code",
                table: "Epas",
                columns: new[] { "SubSpecialityId", "OwningInstitutionId", "Code" },
                unique: true,
                filter: "\"OwningInstitutionId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumItems_OwningInstitutionId",
                table: "CurriculumItems",
                column: "OwningInstitutionId");

            migrationBuilder.AddForeignKey(
                name: "FK_CurriculumItems_Institutions_OwningInstitutionId",
                table: "CurriculumItems",
                column: "OwningInstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Epas_Institutions_OwningInstitutionId",
                table: "Epas",
                column: "OwningInstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CurriculumItems_Institutions_OwningInstitutionId",
                table: "CurriculumItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Epas_Institutions_OwningInstitutionId",
                table: "Epas");

            migrationBuilder.DropIndex(
                name: "IX_Epas_OwningInstitutionId",
                table: "Epas");

            migrationBuilder.DropIndex(
                name: "IX_Epas_SubSpecialityId_Code",
                table: "Epas");

            migrationBuilder.DropIndex(
                name: "IX_Epas_SubSpecialityId_OwningInstitutionId_Code",
                table: "Epas");

            migrationBuilder.DropIndex(
                name: "IX_CurriculumItems_OwningInstitutionId",
                table: "CurriculumItems");

            migrationBuilder.DropColumn(
                name: "OwningInstitutionId",
                table: "Epas");

            migrationBuilder.DropColumn(
                name: "OwningInstitutionId",
                table: "CurriculumItems");

            migrationBuilder.CreateIndex(
                name: "IX_Epas_SubSpecialityId_Code",
                table: "Epas",
                columns: new[] { "SubSpecialityId", "Code" },
                unique: true);
        }
    }
}
