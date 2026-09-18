using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class T109_EntrustmentScalePinning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ScaleId",
                table: "CurriculumItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinimumLevelScaleId",
                table: "CurriculumItemProgresses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScaleMismatchCount",
                table: "CurriculumItemProgresses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UnverifiedLevelCount",
                table: "CurriculumItemProgresses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreditScaleMismatchCount",
                table: "ActivityTransitions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumItems_ScaleId",
                table: "CurriculumItems",
                column: "ScaleId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumItemProgresses_MinimumLevelScaleId",
                table: "CurriculumItemProgresses",
                column: "MinimumLevelScaleId");

            migrationBuilder.AddForeignKey(
                name: "FK_CurriculumItemProgresses_EntrustmentScales_MinimumLevelScal~",
                table: "CurriculumItemProgresses",
                column: "MinimumLevelScaleId",
                principalTable: "EntrustmentScales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CurriculumItems_EntrustmentScales_ScaleId",
                table: "CurriculumItems",
                column: "ScaleId",
                principalTable: "EntrustmentScales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CurriculumItemProgresses_EntrustmentScales_MinimumLevelScal~",
                table: "CurriculumItemProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_CurriculumItems_EntrustmentScales_ScaleId",
                table: "CurriculumItems");

            migrationBuilder.DropIndex(
                name: "IX_CurriculumItems_ScaleId",
                table: "CurriculumItems");

            migrationBuilder.DropIndex(
                name: "IX_CurriculumItemProgresses_MinimumLevelScaleId",
                table: "CurriculumItemProgresses");

            migrationBuilder.DropColumn(
                name: "ScaleId",
                table: "CurriculumItems");

            migrationBuilder.DropColumn(
                name: "MinimumLevelScaleId",
                table: "CurriculumItemProgresses");

            migrationBuilder.DropColumn(
                name: "ScaleMismatchCount",
                table: "CurriculumItemProgresses");

            migrationBuilder.DropColumn(
                name: "UnverifiedLevelCount",
                table: "CurriculumItemProgresses");

            migrationBuilder.DropColumn(
                name: "CreditScaleMismatchCount",
                table: "ActivityTransitions");
        }
    }
}
