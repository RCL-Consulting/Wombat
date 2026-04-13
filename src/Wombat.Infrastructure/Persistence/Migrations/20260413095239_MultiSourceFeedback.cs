using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MultiSourceFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MsfTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SpecialityId = table.Column<int>(type: "integer", nullable: true),
                    AllowPatientResponses = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MsfTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MsfCampaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubjectUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    TemplateId = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OpensOn = table.Column<DateOnly>(type: "date", nullable: false),
                    ClosesOn = table.Column<DateOnly>(type: "date", nullable: false),
                    MinimumResponses = table.Column<int>(type: "integer", nullable: false),
                    MinimumCategoryResponses = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    CoordinatorNarrative = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ReviewedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    OpenedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReleasedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WithdrawnOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MsfCampaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MsfCampaigns_MsfTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "MsfTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MsfQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TemplateId = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Prompt = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    ScaleId = table.Column<int>(type: "integer", nullable: true),
                    Required = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MsfQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MsfQuestions_MsfTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "MsfTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MsfInvitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignId = table.Column<int>(type: "integer", nullable: false),
                    RespondentEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    RespondentEmailHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    RespondentCategory = table.Column<int>(type: "integer", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IssuedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresOn = table.Column<DateOnly>(type: "date", nullable: false),
                    RespondedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AnonymizedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MsfInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MsfInvitations_MsfCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "MsfCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MsfResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignId = table.Column<int>(type: "integer", nullable: false),
                    InvitationId = table.Column<int>(type: "integer", nullable: false),
                    SubmittedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MsfResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MsfResponses_MsfCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "MsfCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MsfResponses_MsfInvitations_InvitationId",
                        column: x => x.InvitationId,
                        principalTable: "MsfInvitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MsfResponseAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ResponseId = table.Column<int>(type: "integer", nullable: false),
                    QuestionId = table.Column<int>(type: "integer", nullable: false),
                    ScaleValue = table.Column<int>(type: "integer", nullable: true),
                    LongText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MsfResponseAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MsfResponseAnswers_MsfQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "MsfQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MsfResponseAnswers_MsfResponses_ResponseId",
                        column: x => x.ResponseId,
                        principalTable: "MsfResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MsfCampaigns_SubjectUserId_State",
                table: "MsfCampaigns",
                columns: new[] { "SubjectUserId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_MsfCampaigns_TemplateId_ClosesOn",
                table: "MsfCampaigns",
                columns: new[] { "TemplateId", "ClosesOn" });

            migrationBuilder.CreateIndex(
                name: "IX_MsfInvitations_CampaignId_RespondentCategory",
                table: "MsfInvitations",
                columns: new[] { "CampaignId", "RespondentCategory" });

            migrationBuilder.CreateIndex(
                name: "IX_MsfInvitations_TokenHash",
                table: "MsfInvitations",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MsfQuestions_TemplateId_Order",
                table: "MsfQuestions",
                columns: new[] { "TemplateId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MsfResponseAnswers_QuestionId",
                table: "MsfResponseAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_MsfResponseAnswers_ResponseId_QuestionId",
                table: "MsfResponseAnswers",
                columns: new[] { "ResponseId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MsfResponses_CampaignId",
                table: "MsfResponses",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_MsfResponses_InvitationId",
                table: "MsfResponses",
                column: "InvitationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MsfTemplates_Name",
                table: "MsfTemplates",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MsfResponseAnswers");

            migrationBuilder.DropTable(
                name: "MsfQuestions");

            migrationBuilder.DropTable(
                name: "MsfResponses");

            migrationBuilder.DropTable(
                name: "MsfInvitations");

            migrationBuilder.DropTable(
                name: "MsfCampaigns");

            migrationBuilder.DropTable(
                name: "MsfTemplates");
        }
    }
}
