using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DataRights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsEnabled",
                table: "ScheduledJobDefinitions",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "OptOutOfDigestEmails",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OptOutOfOptionalProcessing",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DataRightsRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    RequesterDisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RequestedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    DecisionNote = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DecidedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    DecidedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataRightsRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DecisionPanels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Scope = table.Column<int>(type: "integer", nullable: false),
                    InstitutionId = table.Column<int>(type: "integer", nullable: true),
                    SpecialityId = table.Column<int>(type: "integer", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecisionPanels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataRightsErasureRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Pseudonym = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ErasedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RetentionReasonsJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataRightsErasureRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataRightsErasureRecords_DataRightsRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "DataRightsRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataRightsRectifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromValueJson = table.Column<string>(type: "jsonb", nullable: false),
                    ToValueJson = table.Column<string>(type: "jsonb", nullable: false),
                    AppliedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataRightsRectifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataRightsRectifications_DataRightsRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "DataRightsRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommitteeReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TraineeUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    PanelId = table.Column<int>(type: "integer", nullable: false),
                    ReviewPeriodFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ReviewPeriodTo = table.Column<DateOnly>(type: "date", nullable: false),
                    ScheduledOn = table.Column<DateOnly>(type: "date", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    StartedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    RatifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RatifiedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    FinalizedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitteeReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommitteeReviews_DecisionPanels_PanelId",
                        column: x => x.PanelId,
                        principalTable: "DecisionPanels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DecisionPanelMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PanelId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecisionPanelMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DecisionPanelMembers_DecisionPanels_PanelId",
                        column: x => x.PanelId,
                        principalTable: "DecisionPanels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommitteeAppeals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReviewId = table.Column<int>(type: "integer", nullable: false),
                    LodgedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LodgedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Reason = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ResolvedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Outcome = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitteeAppeals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommitteeAppeals_CommitteeReviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "CommitteeReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommitteeDecisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReviewId = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Rationale = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Conditions = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DecidedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DecidedByChairUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    SupersedesDecisionId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitteeDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommitteeDecisions_CommitteeReviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "CommitteeReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommitteeEvidenceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReviewId = table.Column<int>(type: "integer", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    ActivityId = table.Column<int>(type: "integer", nullable: true),
                    MsfCampaignId = table.Column<int>(type: "integer", nullable: true),
                    SupervisorReportId = table.Column<int>(type: "integer", nullable: true),
                    SourceLabel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    SourceRecordedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitteeEvidenceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommitteeEvidenceItems_CommitteeReviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "CommitteeReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeAppeals_ReviewId_LodgedOn",
                table: "CommitteeAppeals",
                columns: new[] { "ReviewId", "LodgedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeDecisions_ReviewId_DecidedOn",
                table: "CommitteeDecisions",
                columns: new[] { "ReviewId", "DecidedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeEvidenceItems_ReviewId_SourceType",
                table: "CommitteeEvidenceItems",
                columns: new[] { "ReviewId", "SourceType" });

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeReviews_PanelId_ScheduledOn",
                table: "CommitteeReviews",
                columns: new[] { "PanelId", "ScheduledOn" });

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeReviews_TraineeUserId_State",
                table: "CommitteeReviews",
                columns: new[] { "TraineeUserId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_DataRightsErasureRecords_Pseudonym",
                table: "DataRightsErasureRecords",
                column: "Pseudonym",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataRightsErasureRecords_RequestId",
                table: "DataRightsErasureRecords",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_DataRightsErasureRecords_UserId",
                table: "DataRightsErasureRecords",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataRightsRectifications_RequestId",
                table: "DataRightsRectifications",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_DataRightsRequests_RequesterUserId",
                table: "DataRightsRequests",
                column: "RequesterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DataRightsRequests_Status_RequestedOn",
                table: "DataRightsRequests",
                columns: new[] { "Status", "RequestedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_DecisionPanelMembers_PanelId_UserId",
                table: "DecisionPanelMembers",
                columns: new[] { "PanelId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DecisionPanels_Name",
                table: "DecisionPanels",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommitteeAppeals");

            migrationBuilder.DropTable(
                name: "CommitteeDecisions");

            migrationBuilder.DropTable(
                name: "CommitteeEvidenceItems");

            migrationBuilder.DropTable(
                name: "DataRightsErasureRecords");

            migrationBuilder.DropTable(
                name: "DataRightsRectifications");

            migrationBuilder.DropTable(
                name: "DecisionPanelMembers");

            migrationBuilder.DropTable(
                name: "CommitteeReviews");

            migrationBuilder.DropTable(
                name: "DataRightsRequests");

            migrationBuilder.DropTable(
                name: "DecisionPanels");

            migrationBuilder.DropColumn(
                name: "OptOutOfDigestEmails",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OptOutOfOptionalProcessing",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<bool>(
                name: "IsEnabled",
                table: "ScheduledJobDefinitions",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");
        }
    }
}
