#requires -Version 7
<#
.SYNOPSIS
    Publish Wombat.Web and deploy to the production Linode from Windows.

.DESCRIPTION
    Windows-native equivalent of deploy/deploy.sh (which needs rsync + bash).
    Uses tar + scp over the OpenSSH client that ships with Windows. It publishes
    framework-dependent Release binaries, ships them as a tarball, rotates the
    previous release on the server (-> app.prev for rollback), extracts, restarts
    the service, and confirms /health.

    There is deliberately no explicit migration step: the service applies EF
    migrations on startup using systemd's EnvironmentFile, which parses wombat.env
    correctly. Hand-running `dotnet Wombat.Web.dll --migrate` does NOT inherit the
    service environment, and sourcing wombat.env from bash mis-splits the connection
    string on its ';'. See deploy/README.md section 6.

    Since T097 the /health gate is meaningful: it probes PostgreSQL, so a deploy that
    comes up unable to reach the database now fails the gate instead of reporting success.

.PARAMETER Remote
    SSH target. Default: root@172.236.8.144 (wombat.rcl.co.za). Pass a different
    user@host or a configured SSH alias for staging.

.EXAMPLE
    ./deploy/deploy.ps1
    ./deploy/deploy.ps1 -Remote root@staging.example.com
#>
param([string]$Remote = 'root@172.236.8.144')

$ErrorActionPreference = 'Stop'
$RepoRoot = Split-Path -Parent $PSScriptRoot
$Publish  = Join-Path $RepoRoot 'publish'
# Staged outside the repo. The tarball is transient build output and has no business
# under deploy/, which otherwise holds tracked scripts. Deleted after a successful
# upload; a failed run leaves it in the OS temp directory, which is what that is for.
$Tgz      = Join-Path ([System.IO.Path]::GetTempPath()) "wombat-publish-$PID.tgz"

# `dotnet publish` does not clean its output directory. A file dropped from the project
# lingers in $Publish, gets tarred, and lands on the server — which extracts into a fresh
# app.new, so the stale file survives the rotation too. Wipe first.
if (Test-Path $Publish) {
    Write-Host '==> Clearing previous publish output...' -ForegroundColor Cyan
    Remove-Item -Recurse -Force $Publish
}

Write-Host '==> Publishing Wombat.Web (Release)...' -ForegroundColor Cyan
dotnet publish (Join-Path $RepoRoot 'src/Wombat.Web/Wombat.Web.csproj') -c Release -o $Publish --nologo
if ($LASTEXITCODE) { throw 'dotnet publish failed' }

Write-Host '==> Packing tarball...' -ForegroundColor Cyan
tar -czf $Tgz -C $Publish .
if ($LASTEXITCODE) { throw 'tar failed' }

Write-Host "==> Uploading to $Remote ..." -ForegroundColor Cyan
scp -q $Tgz "${Remote}:/tmp/publish.tgz"
if ($LASTEXITCODE) { throw 'scp failed' }

Write-Host '==> Deploying on server (rotate, extract, migrate, restart)...' -ForegroundColor Cyan
$remoteScript = @'
set -euo pipefail
rm -rf /opt/wombat/app.new && mkdir -p /opt/wombat/app.new
tar -xzf /tmp/publish.tgz -C /opt/wombat/app.new
rm -rf /opt/wombat/app.prev
[ -d /opt/wombat/app ] && mv /opt/wombat/app /opt/wombat/app.prev || true
mv /opt/wombat/app.new /opt/wombat/app
chown -R wombat:wombat /opt/wombat/app
rm -f /tmp/publish.tgz
# The service auto-applies EF migrations on startup using systemd's EnvironmentFile,
# which parses wombat.env correctly (the connection string contains ';', which a bash
# `. file` source would mis-split). Type=notify means restart blocks until ready/fails.
systemctl restart wombat
ok=0
for i in 1 2 3 4 5 6 7 8; do
  if curl -sf http://127.0.0.1:5080/health >/dev/null 2>&1; then ok=1; echo 'health OK'; break; fi
  sleep 2
done
[ "$ok" = 1 ] || { echo 'HEALTH CHECK FAILED after restart'; journalctl -u wombat -n 30 --no-pager; exit 1; }
'@
# tr -d strips CR so the Windows here-string runs as a clean bash script.
$remoteScript | ssh $Remote "tr -d '\r' | bash -s"
if ($LASTEXITCODE) { throw 'remote deploy failed' }

Remove-Item -Force $Tgz

# The cron scripts have no other deployment path. /opt/wombat/app is all the block above
# touches, so before this step /usr/local/bin/wombat-*.sh was whatever first-boot installed
# — which is how T097's backup rewrite sat undeployed for three months while the docs
# described it as live. Sync them every deploy instead.
Write-Host '==> Syncing cron scripts to /usr/local/bin ...' -ForegroundColor Cyan
$CronScripts = @('wombat-backup.sh', 'wombat-health.sh')
foreach ($s in $CronScripts) {
    scp -q (Join-Path $PSScriptRoot $s) "${Remote}:/tmp/$s.new"
    if ($LASTEXITCODE) { throw "scp of $s failed" }
}
# tr -d '\r' because the repo checkout is CRLF on Windows and bash will not run that.
ssh $Remote 'for s in wombat-backup.sh wombat-health.sh; do tr -d "\r" < /tmp/$s.new > /usr/local/bin/$s && chmod +x /usr/local/bin/$s && rm -f /tmp/$s.new && echo "    $s $(sha256sum /usr/local/bin/$s | cut -c1-12)"; done'
if ($LASTEXITCODE) { throw 'cron script sync failed' }

Write-Host '==> Deploy complete.' -ForegroundColor Green
