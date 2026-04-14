#!/usr/bin/env bash
# wombat-backup.sh — nightly PostgreSQL backup for Wombat.
#
# Install at: /usr/local/bin/wombat-backup.sh
# Cron (root): 0 2 * * * /usr/local/bin/wombat-backup.sh >> /var/log/wombat-backup.log 2>&1
#
# Retention: 14 daily, 4 weekly (Sunday), 6 monthly (1st of month).
# All backups land in BACKUP_DIR. Adjust REMOTE_DEST to rsync off-host.

set -euo pipefail

BACKUP_DIR="/var/backups/wombat"
DB_NAME="wombat"
DB_USER="wombat"
DATE=$(date +%Y-%m-%d)
DOW=$(date +%u)    # 1=Mon … 7=Sun
DOM=$(date +%-d)   # day-of-month, no leading zero

mkdir -p "$BACKUP_DIR/daily" "$BACKUP_DIR/weekly" "$BACKUP_DIR/monthly"

DAILY_FILE="$BACKUP_DIR/daily/wombat-$DATE.dump"

echo "[$(date -Iseconds)] Starting backup..."

# Dump (custom format — supports parallel restore)
sudo -u "$DB_USER" pg_dump -Fc -d "$DB_NAME" -f "$DAILY_FILE"
echo "[$(date -Iseconds)] Dump written: $DAILY_FILE"

# Weekly copy (Sunday)
if [ "$DOW" -eq 7 ]; then
    WEEK=$(date +%Y-W%V)
    cp "$DAILY_FILE" "$BACKUP_DIR/weekly/wombat-$WEEK.dump"
    echo "[$(date -Iseconds)] Weekly copy: wombat-$WEEK.dump"
fi

# Monthly copy (1st of month)
if [ "$DOM" -eq 1 ]; then
    MONTH=$(date +%Y-%m)
    cp "$DAILY_FILE" "$BACKUP_DIR/monthly/wombat-$MONTH.dump"
    echo "[$(date -Iseconds)] Monthly copy: wombat-$MONTH.dump"
fi

# Prune old backups
find "$BACKUP_DIR/daily"   -name "*.dump" -mtime +14  -delete
find "$BACKUP_DIR/weekly"  -name "*.dump" -mtime +28  -delete
find "$BACKUP_DIR/monthly" -name "*.dump" -mtime +180 -delete

echo "[$(date -Iseconds)] Backup complete."
