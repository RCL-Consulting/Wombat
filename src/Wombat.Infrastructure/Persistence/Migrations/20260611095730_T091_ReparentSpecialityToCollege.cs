using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class T091_ReparentSpecialityToCollege : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Specialities_Institutions_InstitutionId",
                table: "Specialities");

            migrationBuilder.RenameColumn(
                name: "InstitutionId",
                table: "Specialities",
                newName: "CollegeId");

            migrationBuilder.RenameIndex(
                name: "IX_Specialities_InstitutionId_Name",
                table: "Specialities",
                newName: "IX_Specialities_CollegeId_Name");

            migrationBuilder.AddColumn<int>(
                name: "InstitutionId",
                table: "TraineeProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TraineeProfiles_InstitutionId",
                table: "TraineeProfiles",
                column: "InstitutionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Specialities_Colleges_CollegeId",
                table: "Specialities",
                column: "CollegeId",
                principalTable: "Colleges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TraineeProfiles_Institutions_InstitutionId",
                table: "TraineeProfiles",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Specialities_Colleges_CollegeId",
                table: "Specialities");

            migrationBuilder.DropForeignKey(
                name: "FK_TraineeProfiles_Institutions_InstitutionId",
                table: "TraineeProfiles");

            migrationBuilder.DropIndex(
                name: "IX_TraineeProfiles_InstitutionId",
                table: "TraineeProfiles");

            migrationBuilder.DropColumn(
                name: "InstitutionId",
                table: "TraineeProfiles");

            migrationBuilder.RenameColumn(
                name: "CollegeId",
                table: "Specialities",
                newName: "InstitutionId");

            migrationBuilder.RenameIndex(
                name: "IX_Specialities_CollegeId_Name",
                table: "Specialities",
                newName: "IX_Specialities_InstitutionId_Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Specialities_Institutions_InstitutionId",
                table: "Specialities",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
