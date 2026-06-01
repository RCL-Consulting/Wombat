using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProgrammeDefaultEntrustmentScale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultEntrustmentScaleId",
                table: "SubSpecialities",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubSpecialities_DefaultEntrustmentScaleId",
                table: "SubSpecialities",
                column: "DefaultEntrustmentScaleId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubSpecialities_EntrustmentScales_DefaultEntrustmentScaleId",
                table: "SubSpecialities",
                column: "DefaultEntrustmentScaleId",
                principalTable: "EntrustmentScales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubSpecialities_EntrustmentScales_DefaultEntrustmentScaleId",
                table: "SubSpecialities");

            migrationBuilder.DropIndex(
                name: "IX_SubSpecialities_DefaultEntrustmentScaleId",
                table: "SubSpecialities");

            migrationBuilder.DropColumn(
                name: "DefaultEntrustmentScaleId",
                table: "SubSpecialities");
        }
    }
}
