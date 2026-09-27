<#
.SYNOPSIS
    Stage what one Claude Design thread needs into design/upload/, a gitignored folder (design/BRIEF.md § 3).

.DESCRIPTION
    Copies two kinds of file, and nothing else:
      - Tracked files only, as `git ls-files` lists them: app.css; the fonts, brand and icons under
        src/Wombat.Web/wwwroot; favicon.svg; execution/architecture/DESIGN.md; design/BRIEF.md; and the flow's brief,
        design/flows/NN-*.md. With -WithLayout it adds src/Wombat.Web/Components/Layout, the shell's Razor and CSS.
      - Screenshots from design/baseline/, which is gitignored, so each is copied by name. These are the files the
        flow's Attach section lists, except those in a "Do not attach" paragraph. With -DesignSystem they are the
        examples BRIEF § 2.3 step 1 names.

    Refusals and warnings:
      - It refuses any path under recovery/ or .scenario-app/, any *.dump, pwd_DO_NOT_COMMIT.txt, appsettings*.json,
        user secrets, bin/ or obj/. It also refuses if design/upload/ is not gitignored.
      - Captures and emails that may show a one-time link go into design/upload/crop-first/, never beside the rest,
        with a warning. These are invitation, issued, resent, resend and reset files, and the MSF expiry reminder
        (BRIEF § 3.3). Crop the link out before you upload one, or leave it out.

    design/upload/ is emptied first, so it always holds exactly one staging.

.PARAMETER Flow
    The flow: 01 to 20, or a file-name prefix such as 01-shell.

.PARAMETER DesignSystem
    Stage the design-system set and BRIEF § 2.3's example screenshots, for setting up a design system (pilot step A).

.PARAMETER WithLayout
    Also stage the tracked files under src/Wombat.Web/Components/Layout.

.EXAMPLE
    pwsh design/tools/stage_upload.ps1 -Flow 01
.EXAMPLE
    pwsh design/tools/stage_upload.ps1 -DesignSystem
#>
param(
    [string]$Flow,
    [switch]$DesignSystem,
    [switch]$WithLayout
)

$ErrorActionPreference = 'Stop'

if (-not $Flow -and -not $DesignSystem) {
    throw "Name a flow (-Flow 01) or pass -DesignSystem. See the help: Get-Help design/tools/stage_upload.ps1 -Detailed"
}

$root = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$baseline = Join-Path $root "design/baseline"
$upload = Join-Path $root "design/upload"
$utf8 = New-Object System.Text.UTF8Encoding($false)

# Paths are compared in the repo's own form: relative, with forward slashes.
function ConvertTo-RepoPath([string]$full) {
    $rel = $full.Substring($root.Length).TrimStart('\', '/')
    return ($rel -replace '\\', '/')
}

$denied = @(
    '(^|/)recovery/',
    '(^|/)\.scenario-app/',
    '\.dump$',
    '(^|/)pwd_DO_NOT_COMMIT\.txt$',
    '(^|/)appsettings[^/]*\.json$',
    'UserSecrets',
    '(^|/)secrets\.json$',
    '(^|/)(bin|obj)/',
    '(^|/)[^/]*\.env$',
    '(^|/)\.claude/'
)

function Assert-Allowed([string]$rel) {
    if ($rel -match '(^|/)\.\.(/|$)') { throw "Refused: '$rel' climbs out of the repo." }
    foreach ($pattern in $denied) {
        if ($rel -match $pattern) { throw "Refused: '$rel' matches '$pattern', which never goes to Claude Design (BRIEF § 3.2)." }
    }
}

# One-time links: invitation captures (BRIEF § 3.3), and the emails that carry a registration, MSF respondent or reset
# link: InvitationEmail, MsfInvitationEmail, MsfExpiryReminderEmail and PasswordResetEmail. Case does not matter.
$linkBearing = '(invit|issued|resent|resend|reset|MsfExpiryReminder)'

# --- design/upload/ must be gitignored, and is emptied first
& git -C $root check-ignore -q "design/upload/probe"
if ($LASTEXITCODE -ne 0) { throw "Refused: design/upload/ is not gitignored. Add /design/upload/ to .gitignore first." }
if (Test-Path -LiteralPath $upload) {
    $resolved = (Resolve-Path -LiteralPath $upload).Path
    if ((ConvertTo-RepoPath $resolved) -ne 'design/upload') { throw "Refused to empty '$resolved'." }
    Remove-Item -LiteralPath $resolved -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $upload | Out-Null

$staged = New-Object System.Collections.Generic.List[object]
$missing = New-Object System.Collections.Generic.List[string]

function Copy-Into([string]$sourceFull, [string]$destRel, [string]$group) {
    $dest = Join-Path $upload $destRel
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $dest) | Out-Null
    Copy-Item -LiteralPath $sourceFull -Destination $dest
    $staged.Add([pscustomobject]@{ Group = $group; Path = ($destRel -replace '\\', '/'); Bytes = (Get-Item -LiteralPath $dest).Length })
}

# --- tracked files
$pathspecs = New-Object System.Collections.Generic.List[string]
foreach ($p in @(
        'src/Wombat.Web/wwwroot/app.css',
        'src/Wombat.Web/wwwroot/fonts',
        'src/Wombat.Web/wwwroot/brand',
        'src/Wombat.Web/wwwroot/icons',
        'src/Wombat.Web/wwwroot/favicon.svg',
        'execution/architecture/DESIGN.md',
        'design/BRIEF.md')) { $pathspecs.Add($p) }
if ($WithLayout) { $pathspecs.Add('src/Wombat.Web/Components/Layout') }

$flowRel = $null
if ($Flow) {
    $prefix = $Flow
    if ($prefix -match '^\d$') { $prefix = "0$prefix" }
    $hits = @(Get-ChildItem -LiteralPath (Join-Path $root "design/flows") -Filter "$prefix*.md" -File)
    if ($hits.Count -eq 0) { throw "No flow brief design/flows/$prefix*.md exists." }
    if ($hits.Count -gt 1) {
        throw "Flow '$Flow' matches $($hits.Count) files in design/flows/; name it more exactly (01, or 01-shell)."
    }
    $flowRel = ConvertTo-RepoPath $hits[0].FullName
    $pathspecs.Add($flowRel)
}

foreach ($spec in $pathspecs) {
    $files = @(& git -C $root ls-files -- $spec)
    if ($LASTEXITCODE -ne 0) { throw "git ls-files failed for '$spec'." }
    if ($files.Count -eq 0) {
        if ($spec -eq $flowRel) { throw "Refused: '$spec' is not tracked. Commit the flow's brief first; only tracked files are staged." }
        $missing.Add("$spec (no tracked file)")
        continue
    }
    foreach ($rel in $files) {
        Assert-Allowed $rel
        $group = 'design system'
        if ($rel -like 'design/*' -or $rel -like 'execution/*') { $group = 'brief and contract' }
        if ($rel -like 'src/Wombat.Web/Components/*') { $group = 'shell source (-WithLayout)' }
        Copy-Into (Join-Path $root $rel) $rel $group
    }
}

# --- screenshots, by name, from the gitignored baseline
# The lines under the heading, up to the next heading that matches $stopPattern.
function Get-Section([string]$text, [string]$headingPattern, [string]$stopPattern) {
    $lines = $text -split "`r?`n"
    $out = New-Object System.Collections.Generic.List[string]
    $inside = $false
    foreach ($line in $lines) {
        if ($line -match $headingPattern) { $inside = $true; continue }
        if ($inside -and $line -match $stopPattern) { break }
        if ($inside) { $out.Add($line) }
    }
    return ($out -join "`n")
}

$shotPattern = '(?<![A-Za-z0-9_/\-])((?:act-[1-6A]|states|mail|pdf)/[A-Za-z0-9][A-Za-z0-9._\-]*\.(?:png|html|txt|pdf|eml))'
$shots = New-Object System.Collections.Generic.List[string]

if ($DesignSystem) {
    $brief = [IO.File]::ReadAllText((Join-Path $root "design/BRIEF.md"), $utf8)
    $section = Get-Section $brief '^### 2\.3 ' '^#{2,3} '
    foreach ($m in [regex]::Matches($section, $shotPattern)) {
        if (-not $shots.Contains($m.Groups[1].Value)) { $shots.Add($m.Groups[1].Value) }
    }
}
if ($flowRel) {
    $flowText = [IO.File]::ReadAllText((Join-Path $root $flowRel), $utf8)
    $attach = Get-Section $flowText '^##\s+(\d+\.\s+)?Attach\b' '^## '
    if (-not $attach.Trim()) { throw "No Attach section in $flowRel (a '## Attach' or '## N. Attach' heading)." }
    # A "Do not attach" paragraph names files to leave out (flows 09, 10, 11, 14, 19): skip it, up to the next blank line.
    $kept = New-Object System.Collections.Generic.List[string]
    $skipping = $false
    foreach ($line in ($attach -split "`n")) {
        if ($line -match '(?i)\bdo not attach\b') { $skipping = $true; continue }
        if ($skipping -and -not $line.Trim()) { $skipping = $false }
        if (-not $skipping) { $kept.Add($line) }
    }
    foreach ($m in [regex]::Matches(($kept -join "`n"), $shotPattern)) {
        if (-not $shots.Contains($m.Groups[1].Value)) { $shots.Add($m.Groups[1].Value) }
    }
}

$flagged = New-Object System.Collections.Generic.List[string]
foreach ($shot in $shots) {
    $rel = "design/baseline/$shot"
    Assert-Allowed $rel
    $source = Join-Path $root $rel
    if (-not (Test-Path -LiteralPath $source -PathType Leaf)) { $missing.Add($rel); continue }
    if ((Split-Path -Leaf $shot) -match $linkBearing) {
        Copy-Into $source ("crop-first/baseline/$shot") 'CROP FIRST (may show a one-time link)'
        $flagged.Add($shot)
    }
    else {
        Copy-Into $source ("baseline/$shot") 'screenshots'
    }
}

# --- report
$what = @()
if ($flowRel) { $what += $flowRel }
if ($DesignSystem) { $what += 'the design-system set' }
Write-Output ("Staged into design/upload/ for " + ($what -join ' and ') + ":")
foreach ($g in ($staged | Group-Object Group)) {
    $kb = [math]::Round((($g.Group | Measure-Object Bytes -Sum).Sum) / 1KB, 1)
    Write-Output ""
    Write-Output ("  {0}: {1} files, {2} KB" -f $g.Name, $g.Count, $kb)
    foreach ($f in $g.Group) { Write-Output ("    {0}  ({1} KB)" -f $f.Path, [math]::Round($f.Bytes / 1KB, 1)) }
}
$total = ($staged | Measure-Object Bytes -Sum).Sum
if (-not $total) { $total = 0 }
Write-Output ""
Write-Output ("Total: {0} files, {1} KB ({2} MB)." -f $staged.Count, [math]::Round($total / 1KB, 1), [math]::Round($total / 1MB, 2))

if ($flagged.Count -gt 0) {
    Write-Warning ("{0} file(s) may show a one-time registration, MSF or reset link, so they are in design/upload/crop-first/, not with the rest. Open each and crop the link out before you upload it, or leave it out (BRIEF § 3.3):" -f $flagged.Count)
    foreach ($f in $flagged) { Write-Warning "  $f" }
}
if ($missing.Count -gt 0) {
    Write-Warning ("{0} named file(s) were not found, so they were not staged:" -f $missing.Count)
    foreach ($m in $missing) { Write-Warning "  $m" }
    exit 1
}
Write-Output "Open every screenshot before you upload it (BRIEF § 3.3). Upload design/upload/ minus crop-first/, never the repo."
