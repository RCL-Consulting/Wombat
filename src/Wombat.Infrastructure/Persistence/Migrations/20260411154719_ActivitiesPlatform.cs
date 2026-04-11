using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ActivitiesPlatform : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Scope = table.Column<int>(type: "integer", nullable: false),
                    ScopeId = table.Column<int>(type: "integer", nullable: true),
                    SchemaJson = table.Column<string>(type: "jsonb", nullable: false),
                    WorkflowJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreditRulesJson = table.Column<string>(type: "jsonb", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    OwnerUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActivityTypeId = table.Column<int>(type: "integer", nullable: false),
                    SchemaVersion = table.Column<int>(type: "integer", nullable: false),
                    SubjectUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CreatedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CurrentState = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DataJson = table.Column<string>(type: "jsonb", nullable: false),
                    EpaId = table.Column<int>(type: "integer", nullable: true),
                    CurriculumItemId = table.Column<int>(type: "integer", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Activities_ActivityTypes_ActivityTypeId",
                        column: x => x.ActivityTypeId,
                        principalTable: "ActivityTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActivityPermissionRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActivityTypeId = table.Column<int>(type: "integer", nullable: false),
                    TransitionKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ActorRuleJson = table.Column<string>(type: "jsonb", nullable: false),
                    FieldRequirementJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityPermissionRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityPermissionRules_ActivityTypes_ActivityTypeId",
                        column: x => x.ActivityTypeId,
                        principalTable: "ActivityTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivityTransitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActivityId = table.Column<int>(type: "integer", nullable: false),
                    FromState = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ToState = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TransitionKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ActorUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    OccurredOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SnapshotJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityTransitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityTransitions_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_ActivityTypeId_CurrentState_SubjectUserId",
                table: "Activities",
                columns: new[] { "ActivityTypeId", "CurrentState", "SubjectUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_CreatedOn",
                table: "Activities",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_DataJson",
                table: "Activities",
                column: "DataJson")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_SubjectUserId",
                table: "Activities",
                column: "SubjectUserId");

            migrationBuilder.Sql(
                """
                CREATE INDEX "IX_Activities_DataJson_EpaId_Expression"
                ON "Activities" ((("DataJson"->>'epa_id')));
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX "IX_Activities_DataJson_AssessorUserId_Expression"
                ON "Activities" ((("DataJson"->>'assessor_user_id')));
                """);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityPermissionRules_ActivityTypeId_TransitionKey",
                table: "ActivityPermissionRules",
                columns: new[] { "ActivityTypeId", "TransitionKey" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityTransitions_ActivityId_OccurredOn",
                table: "ActivityTransitions",
                columns: new[] { "ActivityId", "OccurredOn" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityTypes_Key_Version",
                table: "ActivityTypes",
                columns: new[] { "Key", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityTypes_Scope_ScopeId",
                table: "ActivityTypes",
                columns: new[] { "Scope", "ScopeId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityPermissionRules");

            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Activities_DataJson_EpaId_Expression";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Activities_DataJson_AssessorUserId_Expression";""");

            migrationBuilder.DropTable(
                name: "ActivityTransitions");

            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropTable(
                name: "ActivityTypes");
        }
    }
}
