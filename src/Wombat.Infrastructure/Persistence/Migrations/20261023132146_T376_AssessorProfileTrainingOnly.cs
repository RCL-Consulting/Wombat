using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class T376_AssessorProfileTrainingOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssessorProfiles_Specialities_SpecialityId",
                table: "AssessorProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_AssessorProfiles_SubSpecialities_SubSpecialityId",
                table: "AssessorProfiles");

            migrationBuilder.DropIndex(
                name: "IX_AssessorProfiles_SpecialityId",
                table: "AssessorProfiles");

            migrationBuilder.DropIndex(
                name: "IX_AssessorProfiles_SubSpecialityId",
                table: "AssessorProfiles");

            migrationBuilder.DropColumn(
                name: "SpecialityId",
                table: "AssessorProfiles");

            migrationBuilder.DropColumn(
                name: "SubSpecialityId",
                table: "AssessorProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SpecialityId",
                table: "AssessorProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubSpecialityId",
                table: "AssessorProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssessorProfiles_SpecialityId",
                table: "AssessorProfiles",
                column: "SpecialityId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessorProfiles_SubSpecialityId",
                table: "AssessorProfiles",
                column: "SubSpecialityId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssessorProfiles_Specialities_SpecialityId",
                table: "AssessorProfiles",
                column: "SpecialityId",
                principalTable: "Specialities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AssessorProfiles_SubSpecialities_SubSpecialityId",
                table: "AssessorProfiles",
                column: "SubSpecialityId",
                principalTable: "SubSpecialities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
