using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EntrustmentDecisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EntrustmentDecisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TraineeUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    EpaId = table.Column<int>(type: "integer", nullable: false),
                    AuthorisedLevelId = table.Column<int>(type: "integer", nullable: false),
                    IssuedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpiresOn = table.Column<DateOnly>(type: "date", nullable: true),
                    IssuedByCommitteeReviewId = table.Column<int>(type: "integer", nullable: false),
                    IssuedByChairUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Rationale = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RevokedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    RevocationReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SupersededByDecisionId = table.Column<int>(type: "integer", nullable: true),
                    LastExpiryReminderSentOn = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntrustmentDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntrustmentDecisions_CommitteeReviews_IssuedByCommitteeRevi~",
                        column: x => x.IssuedByCommitteeReviewId,
                        principalTable: "CommitteeReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntrustmentDecisions_EntrustmentLevels_AuthorisedLevelId",
                        column: x => x.AuthorisedLevelId,
                        principalTable: "EntrustmentLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntrustmentDecisions_Epas_EpaId",
                        column: x => x.EpaId,
                        principalTable: "Epas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PendingEntrustmentDecisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReviewId = table.Column<int>(type: "integer", nullable: false),
                    EpaId = table.Column<int>(type: "integer", nullable: false),
                    AuthorisedLevelId = table.Column<int>(type: "integer", nullable: false),
                    IssuedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpiresOn = table.Column<DateOnly>(type: "date", nullable: true),
                    Rationale = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    EvidenceLinksJson = table.Column<string>(type: "jsonb", nullable: false),
                    StagedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StagedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingEntrustmentDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PendingEntrustmentDecisions_CommitteeReviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "CommitteeReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PendingEntrustmentDecisions_EntrustmentLevels_AuthorisedLev~",
                        column: x => x.AuthorisedLevelId,
                        principalTable: "EntrustmentLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PendingEntrustmentDecisions_Epas_EpaId",
                        column: x => x.EpaId,
                        principalTable: "Epas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntrustmentEvidenceLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DecisionId = table.Column<int>(type: "integer", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    ActivityId = table.Column<int>(type: "integer", nullable: true),
                    MsfCampaignId = table.Column<int>(type: "integer", nullable: true),
                    CommitteeReviewId = table.Column<int>(type: "integer", nullable: true),
                    SourceLabel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SourceRecordedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntrustmentEvidenceLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntrustmentEvidenceLinks_EntrustmentDecisions_DecisionId",
                        column: x => x.DecisionId,
                        principalTable: "EntrustmentDecisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntrustmentDecisions_AuthorisedLevelId",
                table: "EntrustmentDecisions",
                column: "AuthorisedLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_EntrustmentDecisions_EpaId",
                table: "EntrustmentDecisions",
                column: "EpaId");

            migrationBuilder.CreateIndex(
                name: "IX_EntrustmentDecisions_IssuedByCommitteeReviewId",
                table: "EntrustmentDecisions",
                column: "IssuedByCommitteeReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_EntrustmentDecisions_Status_ExpiresOn",
                table: "EntrustmentDecisions",
                columns: new[] { "Status", "ExpiresOn" });

            migrationBuilder.CreateIndex(
                name: "IX_EntrustmentDecisions_TraineeUserId_EpaId_Status",
                table: "EntrustmentDecisions",
                columns: new[] { "TraineeUserId", "EpaId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_EntrustmentEvidenceLinks_DecisionId_SourceType",
                table: "EntrustmentEvidenceLinks",
                columns: new[] { "DecisionId", "SourceType" });

            migrationBuilder.CreateIndex(
                name: "IX_PendingEntrustmentDecisions_AuthorisedLevelId",
                table: "PendingEntrustmentDecisions",
                column: "AuthorisedLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingEntrustmentDecisions_EpaId",
                table: "PendingEntrustmentDecisions",
                column: "EpaId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingEntrustmentDecisions_ReviewId_EpaId",
                table: "PendingEntrustmentDecisions",
                columns: new[] { "ReviewId", "EpaId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntrustmentEvidenceLinks");

            migrationBuilder.DropTable(
                name: "PendingEntrustmentDecisions");

            migrationBuilder.DropTable(
                name: "EntrustmentDecisions");
        }
    }
}
