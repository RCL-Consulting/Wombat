using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class T165_CommitteeDecisionQuorum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssessorUserId",
                table: "CommitteeEvidenceItems",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CommitteeDecisionAttendees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DecisionId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitteeDecisionAttendees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommitteeDecisionAttendees_CommitteeDecisions_DecisionId",
                        column: x => x.DecisionId,
                        principalTable: "CommitteeDecisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeDecisionAttendees_DecisionId_UserId",
                table: "CommitteeDecisionAttendees",
                columns: new[] { "DecisionId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommitteeDecisionAttendees");

            migrationBuilder.DropColumn(
                name: "AssessorUserId",
                table: "CommitteeEvidenceItems");
        }
    }
}
