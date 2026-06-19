#requires -Version 7
<#
.SYNOPSIS
    Publish Wombat.Web and deploy to the production Linode from Windows.

.DESCRIPTION
    Windows-native equivalent of deploy/deploy.sh (which needs rsync + bash).
    Uses tar + scp over the OpenSSH client that ships with Windows. It publishes
    framework-dependent Release binaries, ships them as a tarball, rotates the
    previous release on the server (-> app.prev for rollback), extracts, runs EF
    migrations (with the env file sourced), restarts the service, and confirms
    /health.

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
$Tgz      = Join-Path $RepoRoot 'deploy/.remote/publish.tgz'

Write-Host '==> Publishing Wombat.Web (Release)...' -ForegroundColor Cyan
dotnet publish (Join-Path $RepoRoot 'src/Wombat.Web/Wombat.Web.csproj') -c Release -o $Publish --nologo
if ($LASTEXITCODE) { throw 'dotnet publish failed' }

Write-Host '==> Packing tarball...' -ForegroundColor Cyan
New-Item -ItemType Directory -Force (Split-Path $Tgz) | Out-Null
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
sudo -u wombat bash -c 'set -a; . /opt/wombat/config/wombat.env; set +a; dotnet /opt/wombat/app/Wombat.Web.dll --migrate'
systemctl restart wombat
sleep 3
curl -sf http://127.0.0.1:5080/health && echo ' health OK'
'@
# tr -d strips CR so the Windows here-string runs as a clean bash script.
$remoteScript | ssh $Remote "tr -d '\r' | bash -s"
if ($LASTEXITCODE) { throw 'remote deploy failed' }

Write-Host '==> Deploy complete.' -ForegroundColor Green
