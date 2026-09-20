#!/usr/bin/env bash
#
# Restore rehearsal — proves the nightly dump is actually restorable.
#
# A backup nobody has restored is a hypothesis. This takes the newest dump from
# /var/backups/wombat/daily/, restores it into a throwaway database, counts the
# rows that matter, and drops the throwaway. Production is never touched.
#
# Run on the server, as root:
#   Get-Content -Raw deploy/verify/restore-rehearsal.sh | ssh root@<host> "tr -d '\r' | bash -s"
#
# Good looks like: a non-zero TOC entry count, and roles/users/migrations counts
# that match production. Anything zero means the dump is not a backup.
#
# Promoted 2026-09-20 from the 17-19 June first-boot scratch set (was stage5c.sh).
set -euo pipefail
DUMP=$(ls -1t /var/backups/wombat/daily/*.dump | head -1)
echo "dump: $DUMP"

echo '=== TOC integrity (as root, can read 0600) ==='
pg_restore -l "$DUMP" | grep -cE '^[0-9]+;' | xargs -I{} echo "TOC entries: {}"

echo '=== restore into throwaway DB and query ==='
sudo -u postgres dropdb --if-exists wombat_restore_test
sudo -u postgres createdb wombat_restore_test
# root reads the 0600 dump and streams it to postgres pg_restore over stdin
cat "$DUMP" | sudo -u postgres pg_restore -d wombat_restore_test --no-owner --no-privileges 2>&1 | tail -3 || true
echo "--- row counts in restored DB ---"
sudo -u postgres psql -d wombat_restore_test -tAc 'SELECT '\''roles='\'' || count(*) FROM "AspNetRoles";'
sudo -u postgres psql -d wombat_restore_test -tAc 'SELECT '\''users='\'' || count(*) FROM "AspNetUsers";'
sudo -u postgres psql -d wombat_restore_test -tAc 'SELECT '\''migrations='\'' || count(*) FROM "__EFMigrationsHistory";'
echo '--- drop throwaway ---'
sudo -u postgres dropdb wombat_restore_test
echo DONE
