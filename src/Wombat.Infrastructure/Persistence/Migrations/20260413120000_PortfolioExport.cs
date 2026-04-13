using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PortfolioExport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InstitutionBrands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InstitutionId = table.Column<int>(type: "integer", nullable: false),
                    LogoBase64 = table.Column<string>(type: "text", nullable: true),
                    PrimaryColorHex = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    SecondaryColorHex = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstitutionBrands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstitutionBrands_Institutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Institutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioExports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TraineeUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ExportedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ExportedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FilterFromDate = table.Column<DateOnly>(type: "date", nullable: true),
                    FilterToDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ContentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FileName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioExports", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionBrands_InstitutionId",
                table: "InstitutionBrands",
                column: "InstitutionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioExports_ContentHash",
                table: "PortfolioExports",
                column: "ContentHash");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioExports_TraineeUserId",
                table: "PortfolioExports",
                column: "TraineeUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "InstitutionBrands");
            migrationBuilder.DropTable(name: "PortfolioExports");
        }
    }
}
