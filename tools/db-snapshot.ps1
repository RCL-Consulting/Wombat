# db-snapshot.ps1 — local dev DB recovery points.
#
# Usage:
#   tools\db-snapshot.ps1 take <name>      Snapshot the dev DB. Writes recovery/<name>.dump + clones to template DB wombat_snapshot_<name>.
#   tools\db-snapshot.ps1 restore <name>   Restore the dev DB from snapshot. Prefers the template DB; falls back to the dump file.
#   tools\db-snapshot.ps1 list             List available snapshots (templates + dumps).
#   tools\db-snapshot.ps1 drop <name>      Delete a snapshot (drops template DB + removes dump file).
#
# Stop the Wombat.Web dev app before running take/restore. The script terminates any other
# connections to the source DB so the CREATE/DROP DATABASE calls succeed, but a running app
# will immediately reconnect and could interfere mid-clone.

param(
    [Parameter(Mandatory=$true, Position=0)]
    [ValidateSet('take','restore','list','drop')]
    [string]$Command,

    [Parameter(Position=1)]
    [string]$Name
)

$ErrorActionPreference = 'Stop'

$pgBin = "C:\Program Files\PostgreSQL\16\bin"
$pgDump = Join-Path $pgBin "pg_dump.exe"
$pgRestore = Join-Path $pgBin "pg_restore.exe"
$psql = Join-Path $pgBin "psql.exe"

foreach ($exe in @($pgDump, $pgRestore, $psql)) {
    if (-not (Test-Path $exe)) { throw "PostgreSQL client not found: $exe" }
}

$webProject = Join-Path $PSScriptRoot ".." "src/Wombat.Web/Wombat.Web.csproj" | Resolve-Path
$secretsLines = & dotnet user-secrets list --project $webProject
$connLine = $secretsLines | Where-Object { $_ -match "^ConnectionStrings:DefaultConnection\s*=" } | Select-Object -First 1
if (-not $connLine) { throw "ConnectionStrings:DefaultConnection not found in Wombat.Web user-secrets" }
$conn = ($connLine -replace "^[^=]+=\s*", "").Trim()

$parts = @{}
foreach ($pair in $conn -split ';') {
    if ($pair -match '^\s*([^=]+)\s*=\s*(.+)\s*$') { $parts[$matches[1].Trim()] = $matches[2].Trim() }
}

$pgHost = $parts['Host']
$pgPort = $parts['Port']
$pgDb   = $parts['Database']
$pgUser = $parts['Username']
$pgPass = $parts['Password']

if (-not $pgHost -or -not $pgPort -or -not $pgDb -or -not $pgUser) { throw "Could not parse Host/Port/Database/Username from connection string." }

$env:PGPASSWORD = $pgPass

$recoveryDir = Join-Path $PSScriptRoot ".." "recovery"
if (-not (Test-Path $recoveryDir)) { New-Item -ItemType Directory -Path $recoveryDir -Force | Out-Null }
$recoveryDir = (Resolve-Path $recoveryDir).Path

function Get-DumpPath($n) { Join-Path $recoveryDir "$n.dump" }
function Get-TemplateDb($n) { "wombat_snapshot_$($n -replace '[^a-zA-Z0-9_]','_')" }

function Invoke-PsqlScalar {
    param([string]$Database, [string]$Sql)
    $output = & $psql -h $pgHost -p $pgPort -U $pgUser -d $Database -v ON_ERROR_STOP=1 -t -A -c $Sql 2>&1
    if ($LASTEXITCODE -ne 0) { throw "psql failed against ${Database}: $output" }
    return $output
}

function Disconnect-Database($targetDb) {
    $sql = "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '$targetDb' AND pid <> pg_backend_pid();"
    Invoke-PsqlScalar -Database 'postgres' -Sql $sql | Out-Null
}

function Test-Database($dbName) {
    $result = Invoke-PsqlScalar -Database 'postgres' -Sql "SELECT 1 FROM pg_database WHERE datname = '$dbName';"
    return ($result -eq '1')
}

switch ($Command) {
    'take' {
        if (-not $Name) { throw "Name required: tools\db-snapshot.ps1 take <name>" }
        $dumpPath = Get-DumpPath $Name
        $templateDb = Get-TemplateDb $Name

        Write-Host "Snapshotting '$pgDb' as '$Name'..."

        & $pgDump -h $pgHost -p $pgPort -U $pgUser -d $pgDb -Fc -f $dumpPath
        if ($LASTEXITCODE -ne 0) { throw "pg_dump failed (exit $LASTEXITCODE)" }
        $size = [math]::Round((Get-Item $dumpPath).Length / 1MB, 2)
        Write-Host "  pg_dump wrote $dumpPath ($size MB)"

        if (Test-Database $templateDb) {
            Disconnect-Database $templateDb
            Invoke-PsqlScalar -Database 'postgres' -Sql "DROP DATABASE $templateDb;" | Out-Null
            Write-Host "  Dropped existing template DB $templateDb"
        }
        Disconnect-Database $pgDb
        Invoke-PsqlScalar -Database 'postgres' -Sql "CREATE DATABASE $templateDb WITH TEMPLATE $pgDb OWNER $pgUser;" | Out-Null
        Write-Host "  Cloned to template DB $templateDb"
        Write-Host "Done."
    }
    'restore' {
        if (-not $Name) { throw "Name required: tools\db-snapshot.ps1 restore <name>" }
        $dumpPath = Get-DumpPath $Name
        $templateDb = Get-TemplateDb $Name

        $hasTemplate = Test-Database $templateDb
        $hasDump = Test-Path $dumpPath

        if (-not $hasTemplate -and -not $hasDump) { throw "Snapshot '$Name' not found (no template DB '$templateDb', no dump file '$dumpPath')." }

        Disconnect-Database $pgDb
        Invoke-PsqlScalar -Database 'postgres' -Sql "DROP DATABASE IF EXISTS $pgDb;" | Out-Null
        Write-Host "Dropped current '$pgDb'."

        if ($hasTemplate) {
            Invoke-PsqlScalar -Database 'postgres' -Sql "CREATE DATABASE $pgDb WITH TEMPLATE $templateDb OWNER $pgUser;" | Out-Null
            Write-Host "Restored from template DB $templateDb."
        } else {
            Invoke-PsqlScalar -Database 'postgres' -Sql "CREATE DATABASE $pgDb OWNER $pgUser;" | Out-Null
            & $pgRestore -h $pgHost -p $pgPort -U $pgUser -d $pgDb --no-owner --no-acl $dumpPath
            if ($LASTEXITCODE -ne 0) { throw "pg_restore failed (exit $LASTEXITCODE)" }
            Write-Host "Restored from dump file $dumpPath."
        }
        Write-Host "Done. Restart the Wombat.Web dev app to reconnect."
    }
    'list' {
        Write-Host "Template DB snapshots:"
        $templates = Invoke-PsqlScalar -Database 'postgres' -Sql "SELECT datname FROM pg_database WHERE datname LIKE 'wombat_snapshot_%' ORDER BY datname;"
        if ($templates) {
            ($templates -split "`n") | Where-Object { $_ } | ForEach-Object { Write-Host "  $_" }
        } else { Write-Host "  (none)" }
        Write-Host ""
        Write-Host "Dump files in $recoveryDir`:"
        $dumps = Get-ChildItem $recoveryDir -Filter "*.dump" -ErrorAction SilentlyContinue | Sort-Object Name
        if ($dumps) {
            $dumps | ForEach-Object {
                $size = [math]::Round($_.Length / 1MB, 2)
                Write-Host "  $($_.Name) ($size MB, $($_.LastWriteTime.ToString('yyyy-MM-dd HH:mm')))"
            }
        } else { Write-Host "  (none)" }
    }
    'drop' {
        if (-not $Name) { throw "Name required: tools\db-snapshot.ps1 drop <name>" }
        $dumpPath = Get-DumpPath $Name
        $templateDb = Get-TemplateDb $Name

        if (Test-Database $templateDb) {
            Disconnect-Database $templateDb
            Invoke-PsqlScalar -Database 'postgres' -Sql "DROP DATABASE $templateDb;" | Out-Null
            Write-Host "Dropped template DB $templateDb."
        } else {
            Write-Host "Template DB $templateDb not present."
        }

        if (Test-Path $dumpPath) {
            Remove-Item $dumpPath
            Write-Host "Removed dump file $dumpPath."
        } else {
            Write-Host "Dump file $dumpPath not present."
        }
    }
}
