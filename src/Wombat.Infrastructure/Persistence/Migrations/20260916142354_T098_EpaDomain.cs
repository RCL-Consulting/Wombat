using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class T098_EpaDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Domain",
                table: "Epas",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Domain",
                table: "Epas");
        }
    }
}
