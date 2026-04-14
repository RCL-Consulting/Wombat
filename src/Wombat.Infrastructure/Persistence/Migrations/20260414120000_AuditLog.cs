using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActorUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ActorDisplay = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ActorIpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ActorUserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SubjectType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    InstitutionId = table.Column<int>(type: "integer", nullable: true),
                    SpecialityId = table.Column<int>(type: "integer", nullable: true),
                    SummaryJson = table.Column<string>(type: "jsonb", nullable: false),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditEntryArchives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActorUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ActorDisplay = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ActorIpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ActorUserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SubjectType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    InstitutionId = table.Column<int>(type: "integer", nullable: true),
                    SpecialityId = table.Column<int>(type: "integer", nullable: true),
                    SummaryJson = table.Column<string>(type: "jsonb", nullable: false),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEntryArchives", x => x.Id);
                });

            // AuditEntries indexes
            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_OccurredAt",
                table: "AuditEntries",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_ActorUserId_OccurredAt",
                table: "AuditEntries",
                columns: new[] { "ActorUserId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_SubjectType_SubjectId_OccurredAt",
                table: "AuditEntries",
                columns: new[] { "SubjectType", "SubjectId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_SummaryJson",
                table: "AuditEntries",
                column: "SummaryJson")
                .Annotation("Npgsql:IndexMethod", "gin");

            // AuditEntryArchives indexes
            migrationBuilder.CreateIndex(
                name: "IX_AuditEntryArchives_OccurredAt",
                table: "AuditEntryArchives",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntryArchives_ArchivedAt",
                table: "AuditEntryArchives",
                column: "ArchivedAt");

            // Append-only enforcement: PostgreSQL trigger prevents UPDATE and DELETE on AuditEntries.
            // Belt-and-braces: the domain model has no mutation methods, but this guards against
            // accidental future mutations at the database level.
            //
            // NOTE: Also apply the following GRANT manually after deployment to restrict the app
            // database user to INSERT + SELECT only on AuditEntries:
            //   REVOKE UPDATE, DELETE ON "AuditEntries" FROM <app_user>;
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION prevent_audit_entry_mutation()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
    RAISE EXCEPTION 'AuditEntries is append-only: UPDATE and DELETE are not permitted.';
END;
$$;

CREATE TRIGGER audit_entries_immutable
BEFORE UPDATE OR DELETE ON ""AuditEntries""
FOR EACH ROW EXECUTE FUNCTION prevent_audit_entry_mutation();
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP TRIGGER IF EXISTS audit_entries_immutable ON ""AuditEntries"";
DROP FUNCTION IF EXISTS prevent_audit_entry_mutation();
");

            migrationBuilder.DropTable(name: "AuditEntries");
            migrationBuilder.DropTable(name: "AuditEntryArchives");
        }
    }
}
