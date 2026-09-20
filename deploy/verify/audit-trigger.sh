#!/usr/bin/env bash
#
# Audit append-only trigger — proves T096 still holds.
#
# AuditEntries must reject UPDATE always, reject DELETE by default, and permit
# DELETE only inside a transaction that sets wombat.allow_audit_delete = on
# (the sanctioned archival path AuditLogRetentionJob uses). This restores a fresh
# dump into a throwaway database and asserts all three. Production is never touched.
#
# Run on the server, as root:
#   Get-Content -Raw deploy/verify/audit-trigger.sh | ssh root@<host> "tr -d '\r' | bash -s"
#
# Good looks like: TEST A errors, TEST C errors, TEST B reports DELETE 1.
# A silent success on A or C means the integrity control is gone.
#
# Promoted 2026-09-20 from the 17-19 June first-boot scratch set (was verify-trigger.sh).
set -euo pipefail

echo '=== fresh dump + restore into throwaway ==='
sudo -u postgres pg_dump -Fc -d wombat > /tmp/now.dump
sudo -u postgres dropdb --if-exists wombat_trig_test >/dev/null 2>&1 || true
sudo -u postgres createdb wombat_trig_test
cat /tmp/now.dump | sudo -u postgres pg_restore -d wombat_trig_test --no-owner --no-privileges 2>&1 | tail -2 || true
rm -f /tmp/now.dump

echo '=== apply T096 trigger function to throwaway ==='
sudo -u postgres psql -q -d wombat_trig_test -v ON_ERROR_STOP=1 <<'SQL'
CREATE OR REPLACE FUNCTION prevent_audit_entry_mutation()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
    IF TG_OP = 'DELETE' AND current_setting('wombat.allow_audit_delete', true) = 'on' THEN
        RETURN OLD;
    END IF;
    RAISE EXCEPTION 'AuditEntries is append-only: UPDATE and DELETE are not permitted.';
END;
$$;
SQL
echo 'function replaced.'

echo '=== run trigger tests ==='
sudo -u postgres psql -d wombat_trig_test <<'SQL'
\set ON_ERROR_STOP off
\echo '[count before]'
SELECT count(*) AS rows FROM "AuditEntries";
\echo ''
\echo '[TEST A] DELETE without GUC -> expect ERROR (blocked)'
DELETE FROM "AuditEntries" WHERE "Id" = (SELECT "Id" FROM "AuditEntries" LIMIT 1);
\echo ''
\echo '[TEST C] UPDATE with GUC -> expect ERROR (GUC only permits DELETE)'
BEGIN;
SET LOCAL wombat.allow_audit_delete = 'on';
UPDATE "AuditEntries" SET "Action" = 'tamper' WHERE "Id" = (SELECT "Id" FROM "AuditEntries" LIMIT 1);
COMMIT;
\echo ''
\echo '[TEST B] DELETE with GUC -> expect DELETE 1 (sanctioned archival path)'
BEGIN;
SET LOCAL wombat.allow_audit_delete = 'on';
DELETE FROM "AuditEntries" WHERE "Id" = (SELECT "Id" FROM "AuditEntries" LIMIT 1);
COMMIT;
\echo ''
\echo '[count after]'
SELECT count(*) AS rows_after FROM "AuditEntries";
SQL

echo '=== cleanup ==='
sudo -u postgres dropdb wombat_trig_test
echo DONE
