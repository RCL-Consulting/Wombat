using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class ModifyAssessmentTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assessments_AssessmentCategories_AssessmentCategoryId",
                table: "Assessments");

            migrationBuilder.DropIndex(
                name: "IX_Assessments_AssessmentCategoryId",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "AssessmentCategoryId",
                table: "Assessments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssessmentCategoryId",
                table: "Assessments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_AssessmentCategoryId",
                table: "Assessments",
                column: "AssessmentCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_AssessmentCategories_AssessmentCategoryId",
                table: "Assessments",
                column: "AssessmentCategoryId",
                principalTable: "AssessmentCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
