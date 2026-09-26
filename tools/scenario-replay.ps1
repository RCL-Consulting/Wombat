# scenario-replay.ps1 — run the scenario runbook against its own database, beside the dev app.
#
# Usage:
#   tools\scenario-replay.ps1 create <db>                 Create an empty database (the name must start wombat_scenario).
#   tools\scenario-replay.ps1 publish                     Publish Wombat.Web (Debug) to .scenario-app/bin, away from the
#                                                         dev app's locked bin/x64/Release.
#   tools\scenario-replay.ps1 start <db> [port] [smtp]    Start the published app in Development against <db> on
#                                                         http://localhost:<port> (default 5180), with Wombat:BaseUrl and
#                                                         Wombat:MsfRespondUrl set to that address so emailed links point at
#                                                         it. appsettings.Development
#                                                         sends mail to localhost:25; by default this overrides Email:SmtpHost
#                                                         to empty, so every email body is written to the log,
#                                                         .scenario-app/<db>.log, as a "Stub email" line. Pass smtp=keep to
#                                                         use the Development SMTP settings instead.
#   tools\scenario-replay.ps1 stop [port]                 Stop whatever listens on <port> (default 5180).
#   tools\scenario-replay.ps1 sql <db> "<query>"          Run a query and print the rows (unaligned, | separated).
#   tools\scenario-replay.ps1 sql <db> <file.sql>         The same, reading the query from a file (use this from bash,
#                                                         which strips the double quotes of "TableName" identifiers).
#   tools\scenario-replay.ps1 dump <db> <name>            pg_dump <db> to recovery/<name>.dump.
#   tools\scenario-replay.ps1 restore <dump-name> <db>    Create <db> from recovery/<dump-name>.dump.
#
# The runbook is execution/knowledge/scenario-paediatrics/README.md. The server, user and password come from Wombat.Web's
# user secrets, as tools\db-snapshot.ps1 reads them; nothing here prints the password.

param(
    [Parameter(Mandatory=$true, Position=0)]
    [ValidateSet('create','publish','start','stop','sql','dump','restore')]
    [string]$Command,

    [Parameter(Position=1)]
    [string]$Arg1,

    [Parameter(Position=2)]
    [string]$Arg2,

    [Parameter(Position=3)]
    [string]$Arg3
)

$ErrorActionPreference = 'Stop'

$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$appDir = Join-Path $root ".scenario-app"
$recoveryDir = Join-Path $root "recovery"
$webProject = Join-Path $root "src/Wombat.Web/Wombat.Web.csproj"

$pgBin = "C:\Program Files\PostgreSQL\16\bin"
$psql = Join-Path $pgBin "psql.exe"
$pgDump = Join-Path $pgBin "pg_dump.exe"
$pgRestore = Join-Path $pgBin "pg_restore.exe"

function Get-Connection {
    $secretsLines = & dotnet user-secrets list --project $webProject
    $connLine = $secretsLines | Where-Object { $_ -match "^ConnectionStrings:DefaultConnection\s*=" } | Select-Object -First 1
    if (-not $connLine) { throw "ConnectionStrings:DefaultConnection not found in Wombat.Web user-secrets" }
    $conn = ($connLine -replace "^[^=]+=\s*", "").Trim()
    $parts = [ordered]@{}
    foreach ($pair in $conn -split ';') {
        if ($pair -match '^\s*([^=]+)\s*=\s*(.+)\s*$') { $parts[$matches[1].Trim()] = $matches[2].Trim() }
    }
    return $parts
}

function Assert-ScenarioName($db) {
    if (-not $db -or $db -notmatch '^wombat_scenario[a-z0-9_]*$') {
        throw "A replay database's name must start wombat_scenario (lower case, digits, underscores): '$db'"
    }
}

function Invoke-Psql($parts, $database, $sql) {
    $env:PGPASSWORD = $parts['Password']
    # Through a file, never -c: Windows PowerShell 5.1 strips the double quotes inside a native argument, which turns
    # "AspNetUsers" into aspnetusers.
    $file = [System.IO.Path]::GetTempFileName()
    try {
        [System.IO.File]::WriteAllText($file, $sql, (New-Object System.Text.UTF8Encoding($false)))
        $output = & $psql -h $parts['Host'] -p $parts['Port'] -U $parts['Username'] -d $database -v ON_ERROR_STOP=1 -t -A -F '|' -f $file 2>&1
    } finally {
        Remove-Item $file -ErrorAction SilentlyContinue
    }
    if ($LASTEXITCODE -ne 0) { throw "psql failed against ${database}: $output" }
    return $output
}

function Get-ConnectionString($parts, $database) {
    $copy = [ordered]@{}
    foreach ($key in $parts.Keys) { $copy[$key] = $parts[$key] }
    $copy['Database'] = $database
    return ($copy.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" }) -join ';'
}

switch ($Command) {
    'create' {
        Assert-ScenarioName $Arg1
        $parts = Get-Connection
        $exists = Invoke-Psql $parts 'postgres' "SELECT 1 FROM pg_database WHERE datname = '$Arg1';"
        if ($exists -eq '1') { throw "Database '$Arg1' already exists. Choose another name; this script never drops one." }
        Invoke-Psql $parts 'postgres' "CREATE DATABASE $Arg1 OWNER $($parts['Username']);" | Out-Null
        Write-Host "Created empty database $Arg1. The app's start-up migrates and seeds it."
    }
    'publish' {
        $out = Join-Path $appDir "bin"
        & dotnet publish $webProject -c Debug -o $out
        if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed (exit $LASTEXITCODE)" }
        Write-Host "Published to $out"
    }
    'start' {
        Assert-ScenarioName $Arg1
        $port = if ($Arg2) { $Arg2 } else { '5180' }
        $dll = Join-Path $appDir "bin/Wombat.Web.dll"
        if (-not (Test-Path $dll)) { throw "Not published yet: run tools\scenario-replay.ps1 publish" }
        $parts = Get-Connection
        $log = Join-Path $appDir "$Arg1.log"
        $env:ASPNETCORE_ENVIRONMENT = 'Development'
        $env:ConnectionStrings__DefaultConnection = Get-ConnectionString $parts $Arg1
        $env:ASPNETCORE_URLS = "http://localhost:$port"
        # Command-line configuration outranks appsettings and user secrets. BaseUrl builds every emailed link.
        # MsfRespondUrl must sit on BaseUrl's scheme, host and port, or Open campaign is refused.
        $appArgs = @("`"$dll`"", "--Wombat:BaseUrl=http://localhost:$port", "--Wombat:MsfRespondUrl=http://localhost:$port/msf/respond")
        if ($Arg3 -ne 'smtp=keep') { $appArgs += "--Email:SmtpHost=" }
        $process = Start-Process -FilePath 'dotnet' -ArgumentList $appArgs -WorkingDirectory (Join-Path $appDir "bin") `
            -RedirectStandardOutput $log -RedirectStandardError "$log.err" -PassThru -WindowStyle Hidden
        Remove-Item Env:ConnectionStrings__DefaultConnection
        $mail = if ($Arg3 -eq 'smtp=keep') { 'Development SMTP settings' } else { 'the log' }
        Write-Host "Started pid $($process.Id) on http://localhost:$port against $Arg1; mail to $mail; log $log"
    }
    'stop' {
        $port = if ($Arg1) { [int]$Arg1 } else { 5180 }
        $listener = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
        if (-not $listener) { Write-Host "Nothing listens on $port."; break }
        Stop-Process -Id $listener.OwningProcess -Force
        Write-Host "Stopped pid $($listener.OwningProcess) on $port."
    }
    'sql' {
        Assert-ScenarioName $Arg1
        if (-not $Arg2) { throw "Query required: tools\scenario-replay.ps1 sql <db> `"<query>`" (or a path to a .sql file)" }
        # A .sql file sidesteps the shells: called from bash, the double quotes around "TableName" identifiers are lost.
        $query = if ($Arg2 -like '*.sql' -and (Test-Path $Arg2)) { Get-Content -Raw $Arg2 } else { $Arg2 }
        $parts = Get-Connection
        Invoke-Psql $parts $Arg1 $query
    }
    'dump' {
        Assert-ScenarioName $Arg1
        if (-not $Arg2) { throw "Name required: tools\scenario-replay.ps1 dump <db> <name>" }
        $parts = Get-Connection
        if (-not (Test-Path $recoveryDir)) { New-Item -ItemType Directory -Path $recoveryDir | Out-Null }
        $path = Join-Path $recoveryDir "$Arg2.dump"
        $env:PGPASSWORD = $parts['Password']
        & $pgDump -h $parts['Host'] -p $parts['Port'] -U $parts['Username'] -d $Arg1 -Fc -f $path
        if ($LASTEXITCODE -ne 0) { throw "pg_dump failed (exit $LASTEXITCODE)" }
        Write-Host "Wrote $path ($([math]::Round((Get-Item $path).Length / 1MB, 2)) MB)"
    }
    'restore' {
        if (-not $Arg1) { throw "Dump name required: tools\scenario-replay.ps1 restore <dump-name> <db>" }
        Assert-ScenarioName $Arg2
        $path = Join-Path $recoveryDir "$Arg1.dump"
        if (-not (Test-Path $path)) { throw "No dump at $path" }
        $parts = Get-Connection
        $exists = Invoke-Psql $parts 'postgres' "SELECT 1 FROM pg_database WHERE datname = '$Arg2';"
        if ($exists -eq '1') { throw "Database '$Arg2' already exists. Choose another name; this script never drops one." }
        Invoke-Psql $parts 'postgres' "CREATE DATABASE $Arg2 OWNER $($parts['Username']);" | Out-Null
        $env:PGPASSWORD = $parts['Password']
        & $pgRestore -h $parts['Host'] -p $parts['Port'] -U $parts['Username'] -d $Arg2 --no-owner --no-acl $path
        if ($LASTEXITCODE -ne 0) { throw "pg_restore failed (exit $LASTEXITCODE)" }
        Write-Host "Restored $path into $Arg2."
    }
}
