using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class T121_MsfCampaignEpaCoverage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EvidenceRecordedOn",
                table: "MsfCampaigns",
                type: "timestamp with time zone",
                nullable: true);

            // Hand-edited from EF's generated `defaultValue: 0`. Zero would mean "no category minimum"
            // and would silently exempt every campaign that already exists from College decision D11,
            // which is the one thing this column is for. Two is the College's answer and the entity's
            // own default (MsfCampaign.MinimumRespondentCategories). The DB default is not carried into
            // the model, so it applies to the backfill only and never to an insert. (T121)
            migrationBuilder.AddColumn<int>(
                name: "MinimumRespondentCategories",
                table: "MsfCampaigns",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "ReviewerEntrustmentLevel",
                table: "MsfCampaigns",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MsfCampaignEpas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignId = table.Column<int>(type: "integer", nullable: false),
                    EpaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MsfCampaignEpas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MsfCampaignEpas_Epas_EpaId",
                        column: x => x.EpaId,
                        principalTable: "Epas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MsfCampaignEpas_MsfCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "MsfCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MsfCampaignEpas_CampaignId_EpaId",
                table: "MsfCampaignEpas",
                columns: new[] { "CampaignId", "EpaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MsfCampaignEpas_EpaId",
                table: "MsfCampaignEpas",
                column: "EpaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MsfCampaignEpas");

            migrationBuilder.DropColumn(
                name: "EvidenceRecordedOn",
                table: "MsfCampaigns");

            migrationBuilder.DropColumn(
                name: "MinimumRespondentCategories",
                table: "MsfCampaigns");

            migrationBuilder.DropColumn(
                name: "ReviewerEntrustmentLevel",
                table: "MsfCampaigns");
        }
    }
}
