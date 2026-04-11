using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CurriculumItemProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CurriculumItemProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CurriculumItemId = table.Column<int>(type: "integer", nullable: false),
                    TraineeUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CountsSoFar = table.Column<int>(type: "integer", nullable: false),
                    MinimumLevelReachedCount = table.Column<int>(type: "integer", nullable: false),
                    LastActivityId = table.Column<int>(type: "integer", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreditedActivityKeysJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumItemProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumItemProgresses_CurriculumItems_CurriculumItemId",
                        column: x => x.CurriculumItemId,
                        principalTable: "CurriculumItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumItemProgresses_CurriculumItemId_TraineeUserId",
                table: "CurriculumItemProgresses",
                columns: new[] { "CurriculumItemId", "TraineeUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumItemProgresses_TraineeUserId",
                table: "CurriculumItemProgresses",
                column: "TraineeUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurriculumItemProgresses");
        }
    }
}
