using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class T164_LearnerFeedbackKind : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Kind",
                table: "MsfTemplates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TeachingContext",
                table: "MsfInvitations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kind",
                table: "MsfTemplates");

            migrationBuilder.DropColumn(
                name: "TeachingContext",
                table: "MsfInvitations");
        }
    }
}
