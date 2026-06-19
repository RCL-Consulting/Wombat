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

echo "==> Deploy complete."
