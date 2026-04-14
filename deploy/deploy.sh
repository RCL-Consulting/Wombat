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
#   - The wombat user on the target can sudo systemctl restart wombat
#     (add a /etc/sudoers.d/wombat-restart rule on the server)
#   - .NET 10 SDK installed locally (dotnet publish runs here, not on server)

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

echo "==> Running migrations..."
ssh "$REMOTE" "sudo -u wombat dotnet $REMOTE_APP/Wombat.Web.dll --migrate"

echo "==> Restarting wombat service..."
ssh "$REMOTE" "sudo systemctl restart wombat"

echo "==> Waiting for health check..."
sleep 5
ssh "$REMOTE" "curl -sf http://127.0.0.1:5080/health && echo ' OK'"

echo "==> Deploy complete."
