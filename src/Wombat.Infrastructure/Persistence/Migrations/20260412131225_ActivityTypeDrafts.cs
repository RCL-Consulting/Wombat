using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ActivityTypeDrafts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ActivityTypes_Key_Version",
                table: "ActivityTypes");

            migrationBuilder.AlterColumn<string>(
                name: "WorkflowJson",
                table: "ActivityTypes",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "SchemaJson",
                table: "ActivityTypes",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "CreditRulesJson",
                table: "ActivityTypes",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AddColumn<string>(
                name: "DisplayFieldsJson",
                table: "ActivityTypes",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StagingCreditRulesJson",
                table: "ActivityTypes",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StagingDisplayFieldsJson",
                table: "ActivityTypes",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StagingSchemaJson",
                table: "ActivityTypes",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StagingUpdatedByUserId",
                table: "ActivityTypes",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StagingUpdatedOn",
                table: "ActivityTypes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StagingWorkflowJson",
                table: "ActivityTypes",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ActivityTypeVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActivityTypeId = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    SchemaJson = table.Column<string>(type: "jsonb", nullable: false),
                    WorkflowJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreditRulesJson = table.Column<string>(type: "jsonb", nullable: false),
                    DisplayFieldsJson = table.Column<string>(type: "jsonb", nullable: false),
                    PublishedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    PublishedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityTypeVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityTypeVersions_ActivityTypes_ActivityTypeId",
                        column: x => x.ActivityTypeId,
                        principalTable: "ActivityTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityTypes_Key",
                table: "ActivityTypes",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityTypeVersions_ActivityTypeId_Version",
                table: "ActivityTypeVersions",
                columns: new[] { "ActivityTypeId", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityTypeVersions");

            migrationBuilder.DropIndex(
                name: "IX_ActivityTypes_Key",
                table: "ActivityTypes");

            migrationBuilder.DropColumn(
                name: "DisplayFieldsJson",
                table: "ActivityTypes");

            migrationBuilder.DropColumn(
                name: "StagingCreditRulesJson",
                table: "ActivityTypes");

            migrationBuilder.DropColumn(
                name: "StagingDisplayFieldsJson",
                table: "ActivityTypes");

            migrationBuilder.DropColumn(
                name: "StagingSchemaJson",
                table: "ActivityTypes");

            migrationBuilder.DropColumn(
                name: "StagingUpdatedByUserId",
                table: "ActivityTypes");

            migrationBuilder.DropColumn(
                name: "StagingUpdatedOn",
                table: "ActivityTypes");

            migrationBuilder.DropColumn(
                name: "StagingWorkflowJson",
                table: "ActivityTypes");

            migrationBuilder.AlterColumn<string>(
                name: "WorkflowJson",
                table: "ActivityTypes",
                type: "jsonb",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SchemaJson",
                table: "ActivityTypes",
                type: "jsonb",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreditRulesJson",
                table: "ActivityTypes",
                type: "jsonb",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityTypes_Key_Version",
                table: "ActivityTypes",
                columns: new[] { "Key", "Version" },
                unique: true);
        }
    }
}
