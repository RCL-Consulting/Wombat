#!/usr/bin/env bash
# wombat-health.sh — health check for Wombat. Restart and email on repeated failure.
#
# Install at: /usr/local/bin/wombat-health.sh
# Cron (root): * * * * * /usr/local/bin/wombat-health.sh >> /var/log/wombat-health.log 2>&1
#
# Three consecutive failures trigger a restart and an email alert.
# The failure counter is tracked via a temp file. It resets on success.

set -euo pipefail

HEALTH_URL="http://127.0.0.1:5080/health"
FAIL_FILE="/tmp/wombat-health-fails"
FAIL_THRESHOLD=3
ALERT_EMAIL="renier@rcl.co.za"
SERVICE_NAME="wombat"

current_fails=0
if [ -f "$FAIL_FILE" ]; then
    current_fails=$(cat "$FAIL_FILE")
fi

if curl -sf --max-time 5 "$HEALTH_URL" > /dev/null 2>&1; then
    if [ "$current_fails" -gt 0 ]; then
        echo "[$(date -Iseconds)] Health check recovered after $current_fails failure(s)."
    fi
    rm -f "$FAIL_FILE"
    exit 0
fi

# Health check failed
current_fails=$((current_fails + 1))
echo "$current_fails" > "$FAIL_FILE"
echo "[$(date -Iseconds)] Health check failed (consecutive failures: $current_fails)."

if [ "$current_fails" -ge "$FAIL_THRESHOLD" ]; then
    echo "[$(date -Iseconds)] Threshold reached. Restarting $SERVICE_NAME..."
    systemctl restart "$SERVICE_NAME"
    rm -f "$FAIL_FILE"

    # Email alert is best-effort: only sent if a `mail` MTA is installed and configured.
    # (SMTP is deferred in Phase 1, so this is typically a no-op until mail is set up.)
    if command -v mail >/dev/null 2>&1; then
        echo "Wombat health check failed $current_fails times in a row. Service restarted at $(date)." \
            | mail -s "Wombat auto-restart on $(hostname)" "$ALERT_EMAIL" || true
        echo "[$(date -Iseconds)] Service restarted. Alert sent to $ALERT_EMAIL."
    else
        echo "[$(date -Iseconds)] Service restarted. (mail not installed — no email alert sent.)"
    fi
fi
