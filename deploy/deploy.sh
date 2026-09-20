#!/usr/bin/env bash
# deploy.sh — publish Wombat.Web and deploy to the production server.
#
# Usage:
#   ./deploy/deploy.sh [user@host]
#
# The default host is wombat-prod. Override for staging:
#   ./deploy/deploy.sh wombat@staging.example.com
#
# Prerequisites:
#   - SSH key auth configured for the target host
#   - rsync + bash available locally (Linux / macOS / WSL / Git-Bash).
#     On native Windows PowerShell use deploy/deploy.ps1 instead (tar + scp).
#   - .NET 10 SDK installed locally (dotnet publish runs here, not on server)
#   - root (or a sudo user) on the target; restart runs via ssh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"
PUBLISH_DIR="$REPO_ROOT/publish"
REMOTE="${1:-wombat-prod}"
REMOTE_APP="/opt/wombat/app"
REMOTE_PREV="/opt/wombat/app.prev"

# `dotnet publish` does not clean its output directory, so a file dropped from the
# project lingers in $PUBLISH_DIR. rsync --delete would remove it from the server, but
# only until someone deploys with deploy.ps1 instead, which tars whatever is here.
# Wipe first so both scripts ship exactly what the current source produces.
# Guarded: never rm -rf a path that is empty or not under the repo root.
[ -n "$REPO_ROOT" ] || { echo "REPO_ROOT did not resolve; refusing to clear anything" >&2; exit 1; }
case "$PUBLISH_DIR" in
    "$REPO_ROOT"/?*) rm -rf "$PUBLISH_DIR" ;;
    *) echo "refusing to clear unexpected publish dir: $PUBLISH_DIR" >&2; exit 1 ;;
esac

echo "==> Publishing Wombat.Web (Release)..."
dotnet publish "$REPO_ROOT/src/Wombat.Web/Wombat.Web.csproj" \
    -c Release \
    -o "$PUBLISH_DIR" \
    --nologo

echo "==> Rotating previous release on $REMOTE..."
ssh "$REMOTE" "rm -rf $REMOTE_PREV && ([ -d $REMOTE_APP ] && mv $REMOTE_APP $REMOTE_PREV || true)"

echo "==> Syncing binaries to $REMOTE:$REMOTE_APP ..."
rsync -az --delete "$PUBLISH_DIR/" "$REMOTE:$REMOTE_APP/"

echo "==> Restarting wombat service (auto-applies EF migrations on startup)..."
# The service applies pending EF migrations on boot using systemd's EnvironmentFile,
# which parses wombat.env correctly (the connection string contains ';'). A one-shot
# `dotnet ... --migrate` would need the env, and a bash `. wombat.env` mis-splits the
# connection string on ';' — so rely on the service restart instead. Type=notify makes
# restart block until the app is ready (or fail).
ssh "$REMOTE" "sudo systemctl restart wombat"

echo "==> Waiting for health check..."
sleep 5
ssh "$REMOTE" "curl -sf http://127.0.0.1:5080/health && echo ' OK'"

# The cron scripts have no other deployment path. The rsync above covers /opt/wombat/app
# only, so before this step /usr/local/bin/wombat-*.sh was whatever first-boot installed —
# which is how T097's backup rewrite sat undeployed for three months while the docs
# described it as live. Sync them every deploy instead.
echo "==> Syncing cron scripts to /usr/local/bin ..."
for s in wombat-backup.sh wombat-health.sh; do
    # tr -d '\r' because a Windows checkout is CRLF and bash will not run that.
    tr -d '\r' < "$SCRIPT_DIR/$s" \
        | ssh "$REMOTE" "sudo tee /usr/local/bin/$s >/dev/null && sudo chmod +x /usr/local/bin/$s"
    echo "    $s"
done

echo "==> Deploy complete."
