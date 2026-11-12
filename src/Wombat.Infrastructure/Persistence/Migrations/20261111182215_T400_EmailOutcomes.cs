using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class T400_EmailOutcomes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailOutcomes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Refusal = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailOutcomes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailOutcomes_Kind_OccurredAt",
                table: "EmailOutcomes",
                columns: new[] { "Kind", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailOutcomes_OccurredAt",
                table: "EmailOutcomes",
                column: "OccurredAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailOutcomes");
        }
    }
}
