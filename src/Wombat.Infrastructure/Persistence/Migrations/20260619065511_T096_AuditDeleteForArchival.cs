using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class T096_AuditDeleteForArchival : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // AuditEntries stays append-only, but the sanctioned archival mover
            // (AuditLogRetentionJob) must be able to move 2-year-old rows to
            // AuditEntryArchives. The original trigger blocked ALL deletes, which would
            // make that job fail. Allow DELETE only inside a transaction that explicitly
            // opts in via the session-local GUC `wombat.allow_audit_delete = 'on'`
            // (a custom GUC any role can SET LOCAL; no superuser needed). Every other
            // DELETE and all UPDATEs are still rejected.
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION prevent_audit_entry_mutation()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
    IF TG_OP = 'DELETE' AND current_setting('wombat.allow_audit_delete', true) = 'on' THEN
        RETURN OLD;
    END IF;
    RAISE EXCEPTION 'AuditEntries is append-only: UPDATE and DELETE are not permitted.';
END;
$$;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore the original unconditional block.
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION prevent_audit_entry_mutation()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
    RAISE EXCEPTION 'AuditEntries is append-only: UPDATE and DELETE are not permitted.';
END;
$$;
");
        }
    }
}
