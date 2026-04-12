using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProcedureCatalogueSeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcedureCatalogueEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcedureCatalogueEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcedureCatalogueEntries_Category_Name",
                table: "ProcedureCatalogueEntries",
                columns: new[] { "Category", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcedureCatalogueEntries_Key",
                table: "ProcedureCatalogueEntries",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcedureCatalogueEntries");
        }
    }
}
