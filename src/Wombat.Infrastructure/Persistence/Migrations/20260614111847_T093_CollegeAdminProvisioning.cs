using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class T093_CollegeAdminProvisioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "InstitutionId",
                table: "Invitations",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "CollegeId",
                table: "Invitations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CollegeId",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CollegeId",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "CollegeId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "InstitutionId",
                table: "Invitations",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
