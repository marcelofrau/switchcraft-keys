#Requires -Version 7.0
<#
.SYNOPSIS
    Gerencia a versao semantica do SwitchcraftKeys.
.PARAMETER Bump
    Incrementa patch, minor ou major.
.PARAMETER Set
    Define versao explicitamente (ex: 1.0.0).
.PARAMETER Tag
    Cria git tag local v{version} apos atualizar.
.PARAMETER Show
    Exibe versao atual e commit SHA.
.EXAMPLE
    .\build\version.ps1 -Bump patch
    .\build\version.ps1 -Bump minor
    .\build\version.ps1 -Set 1.0.0
    .\build\version.ps1 -Bump patch -Tag
    .\build\version.ps1 -Show
#>
[CmdletBinding(DefaultParameterSetName = "Bump")]
param(
    [Parameter(ParameterSetName = "Bump")]
    [ValidateSet("patch", "minor", "major")]
    [string]$Bump,

    [Parameter(ParameterSetName = "Set")]
    [string]$Set,

    [Parameter(ParameterSetName = "Show")]
    [switch]$Show,

    [switch]$Tag
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
$ProjPath = Join-Path $RepoRoot "src\SwitchcraftKeys\SwitchcraftKeys.csproj"
$Changelog = Join-Path $RepoRoot "CHANGELOG.md"

# Helper: Get git commit SHA (short form)
function Get-GitCommitShort {
    try {
        $sha = & git -C $RepoRoot rev-parse --short HEAD 2>$null
        if ($LASTEXITCODE -eq 0 -and $sha) {
            return $sha
        }
    }
    catch { }
    return "unknown"
}

# Read current version
[xml]$proj = Get-Content $ProjPath
$current = $proj.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
if (-not $current) { Write-Error "Could not read <Version> from $ProjPath" }

# Handle -Show
if ($PSCmdlet.ParameterSetName -eq "Show") {
    $sha = Get-GitCommitShort
    Write-Host "Version: $current" -ForegroundColor Cyan
    Write-Host "Build (commit): $sha" -ForegroundColor DarkGray
    Write-Host "Informational: $current+$sha" -ForegroundColor Green
    return
}

Write-Host "Current version: $current" -ForegroundColor DarkGray

# Calculate new version
if ($PSCmdlet.ParameterSetName -eq "Set") {
    if ($Set -notmatch '^\d+\.\d+\.\d+$') {
        Write-Error "Invalid version format: '$Set'. Expected X.Y.Z"
    }
    $newVersion = $Set
} else {
    $parts = $current -split '\.'
    $major = [int]$parts[0]
    $minor = [int]$parts[1]
    $patch = [int]$parts[2]

    switch ($Bump) {
        "major" { $major++; $minor = 0; $patch = 0 }
        "minor" { $minor++; $patch = 0 }
        "patch" { $patch++ }
    }

    $newVersion = "$major.$minor.$patch"
}

Write-Host "==> Bumping version: $current -> $newVersion" -ForegroundColor Cyan

# Update .csproj: <Version>
$content = Get-Content $ProjPath -Raw
$content = $content -replace "<Version>$([regex]::Escape($current))</Version>", "<Version>$newVersion</Version>"
Set-Content -LiteralPath $ProjPath -Value $content -NoNewline

Write-Host "  Updated: <Version> in $ProjPath" -ForegroundColor DarkGray

# Update CHANGELOG.md — prepend new [Unreleased] entry
if (Test-Path $Changelog) {
    $today = (Get-Date).ToString("yyyy-MM-dd")
    $cl = Get-Content $Changelog -Raw
    $entry = "## [v$newVersion] - $today`n`n### Added`n- `n`n### Changed`n- `n`n### Fixed`n- `n`n"
    $cl = $cl -replace "(## \[)", "$entry`$1"
    Set-Content -LiteralPath $Changelog -Value $cl -NoNewline
    Write-Host "  Updated: $Changelog" -ForegroundColor DarkGray
}

# Git tag
if ($Tag) {
    $tagName = "v$newVersion"
    git -C $RepoRoot tag $tagName
    Write-Host "  Git tag created: $tagName (local only)" -ForegroundColor DarkGray
}

Write-Host "`n[OK] Version set to $newVersion" -ForegroundColor Green
$sha = Get-GitCommitShort
Write-Host "     Informational: $newVersion+$sha" -ForegroundColor DarkGray

