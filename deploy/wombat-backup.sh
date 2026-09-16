#!/usr/bin/env bash
# wombat-backup.sh — nightly backup for Wombat: database + the two irreplaceable secrets.
#
# Install at: /usr/local/bin/wombat-backup.sh
# Cron, installed to /etc/cron.d/wombat-backup (note the mandatory user field):
#   0 2 * * * root /usr/local/bin/wombat-backup.sh >> /var/log/wombat-backup.log 2>&1
#
# Retention (local): 14 daily, 4 weekly (Sunday), 6 monthly (1st of month).
#
# WHAT IS BACKED UP, AND WHY IT IS MORE THAN THE DATABASE
#   1. The PostgreSQL database.
#   2. /opt/wombat/config/wombat.env — holds Wombat__PseudonymSalt, which is documented as
#      NEVER rotatable: losing it permanently breaks the linkability of every POPIA erasure
#      pseudonym already issued. It cannot be reconstructed from anything.
#   3. /opt/wombat/data/keys — the DataProtection key ring. Losing it invalidates every
#      session cookie and antiforgery token (all users logged out) on restore.
#   A database-only backup restores to a box that boots and then cannot honour its own
#   erasure records. Items 2 and 3 are why this script is not just pg_dump.
#
# OFF-HOST IS NOT OPTIONAL
#   A backup on the same filesystem as the database is not a backup — one disk failure,
#   theft, or fire is simultaneously total data loss AND total disclosure of named doctors'
#   competence records. Because the bundle contains secrets, it is encrypted with `age`
#   BEFORE it leaves the box, to a recipient whose private key lives on NEITHER machine.
#
#   Configure in /etc/default/wombat-backup (mode 600):
#     WOMBAT_BACKUP_AGE_RECIPIENT="age1..."         # public key; private half kept offline
#     WOMBAT_BACKUP_REMOTE="user@host:/path"        # rsync target, or an rclone remote
#     WOMBAT_BACKUP_RCLONE_REMOTE="remote:bucket"   # optional, used instead of rsync
#
#   If these are unset the local dump still runs, but the script exits NON-ZERO so cron
#   mails you. A silently-skipped off-host step is how this stayed broken for months.

set -euo pipefail

BACKUP_DIR="/var/backups/wombat"
DB_NAME="wombat"
CONFIG_FILE="/opt/wombat/config/wombat.env"
KEYS_DIR="/opt/wombat/data/keys"
DATE=$(date +%Y-%m-%d)
DOW=$(date +%u)    # 1=Mon … 7=Sun
DOM=$(date +%-d)   # day-of-month, no leading zero

# shellcheck source=/dev/null
[ -r /etc/default/wombat-backup ] && . /etc/default/wombat-backup

mkdir -p "$BACKUP_DIR/daily" "$BACKUP_DIR/weekly" "$BACKUP_DIR/monthly"
chmod 700 "$BACKUP_DIR" "$BACKUP_DIR/daily" "$BACKUP_DIR/weekly" "$BACKUP_DIR/monthly"

DAILY_FILE="$BACKUP_DIR/daily/wombat-$DATE.dump"
BUNDLE_FILE="$BACKUP_DIR/daily/wombat-$DATE.tar.gz"

echo "[$(date -Iseconds)] Starting backup..."

# Dump (custom format — supports parallel restore). The dump is produced by the
# postgres superuser over peer auth (no password needed) and streamed to stdout;
# this script's user (root, under cron) writes the file — so BACKUP_DIR can stay
# root-owned 0700 while pg_dump runs as a non-root DB user.
sudo -u postgres pg_dump -Fc -d "$DB_NAME" > "$DAILY_FILE"
chmod 600 "$DAILY_FILE"
echo "[$(date -Iseconds)] Dump written: $DAILY_FILE"

# Bundle the dump together with the config and the DataProtection key ring, so a restore
# has everything it needs in one artefact.
STAGE=$(mktemp -d)
trap 'rm -rf "$STAGE"' EXIT
mkdir -p "$STAGE/wombat-$DATE"
cp "$DAILY_FILE" "$STAGE/wombat-$DATE/database.dump"

if [ -r "$CONFIG_FILE" ]; then
    cp "$CONFIG_FILE" "$STAGE/wombat-$DATE/wombat.env"
else
    echo "[$(date -Iseconds)] WARNING: $CONFIG_FILE not readable — PseudonymSalt NOT backed up."
fi

if [ -d "$KEYS_DIR" ]; then
    cp -r "$KEYS_DIR" "$STAGE/wombat-$DATE/keys"
else
    echo "[$(date -Iseconds)] WARNING: $KEYS_DIR missing — DataProtection keys NOT backed up."
fi

tar -czf "$BUNDLE_FILE" -C "$STAGE" "wombat-$DATE"
chmod 600 "$BUNDLE_FILE"
echo "[$(date -Iseconds)] Bundle written: $BUNDLE_FILE"

# Weekly / monthly copies of the full bundle (not just the dump).
if [ "$DOW" -eq 7 ]; then
    WEEK=$(date +%Y-W%V)
    cp "$BUNDLE_FILE" "$BACKUP_DIR/weekly/wombat-$WEEK.tar.gz"
    echo "[$(date -Iseconds)] Weekly copy: wombat-$WEEK.tar.gz"
fi

if [ "$DOM" -eq 1 ]; then
    MONTH=$(date +%Y-%m)
    cp "$BUNDLE_FILE" "$BACKUP_DIR/monthly/wombat-$MONTH.tar.gz"
    echo "[$(date -Iseconds)] Monthly copy: wombat-$MONTH.tar.gz"
fi

# Prune old backups
find "$BACKUP_DIR/daily"   -name "*.dump"    -mtime +14  -delete
find "$BACKUP_DIR/daily"   -name "*.tar.gz"  -mtime +14  -delete
find "$BACKUP_DIR/weekly"  -name "*.tar.gz"  -mtime +28  -delete
find "$BACKUP_DIR/monthly" -name "*.tar.gz"  -mtime +180 -delete

# ---------------------------------------------------------------------------
# Off-host leg: encrypt, then ship.
# ---------------------------------------------------------------------------
OFFHOST_OK=0

if [ -z "${WOMBAT_BACKUP_AGE_RECIPIENT:-}" ]; then
    echo "[$(date -Iseconds)] ERROR: WOMBAT_BACKUP_AGE_RECIPIENT unset — refusing to ship an UNENCRYPTED bundle containing wombat.env off-host."
elif ! command -v age >/dev/null 2>&1; then
    echo "[$(date -Iseconds)] ERROR: 'age' not installed (apt install age) — cannot encrypt; not shipping off-host."
else
    ENCRYPTED_FILE="$BUNDLE_FILE.age"
    age -r "$WOMBAT_BACKUP_AGE_RECIPIENT" -o "$ENCRYPTED_FILE" "$BUNDLE_FILE"
    chmod 600 "$ENCRYPTED_FILE"
    echo "[$(date -Iseconds)] Encrypted: $ENCRYPTED_FILE"

    if [ -n "${WOMBAT_BACKUP_RCLONE_REMOTE:-}" ] && command -v rclone >/dev/null 2>&1; then
        rclone copy "$ENCRYPTED_FILE" "$WOMBAT_BACKUP_RCLONE_REMOTE"
        echo "[$(date -Iseconds)] Shipped off-host via rclone to $WOMBAT_BACKUP_RCLONE_REMOTE"
        OFFHOST_OK=1
    elif [ -n "${WOMBAT_BACKUP_REMOTE:-}" ]; then
        rsync -a "$ENCRYPTED_FILE" "$WOMBAT_BACKUP_REMOTE/"
        echo "[$(date -Iseconds)] Shipped off-host via rsync to $WOMBAT_BACKUP_REMOTE"
        OFFHOST_OK=1
    else
        echo "[$(date -Iseconds)] ERROR: neither WOMBAT_BACKUP_RCLONE_REMOTE nor WOMBAT_BACKUP_REMOTE is set — bundle encrypted but NOT shipped off-host."
    fi

    find "$BACKUP_DIR/daily" -name "*.tar.gz.age" -mtime +14 -delete
fi

if [ "$OFFHOST_OK" -eq 0 ]; then
    echo "[$(date -Iseconds)] Backup complete LOCALLY ONLY — off-host copy did not happen. This is not a backup."
    exit 1
fi

echo "[$(date -Iseconds)] Backup complete (local + off-host)."
