#Requires -Version 7.0
<#
.SYNOPSIS
    Compila a solucao SwitchcraftKeys.
.PARAMETER Config
    Configuracao de build: Debug (padrao) ou Release.
.PARAMETER Release
    Gera release portable (zip). Exige -Config Release.
.PARAMETER Installer
    Gera instalador Windows (Inno Setup). Exige -Release e ISCC.exe em PATH.
.EXAMPLE
    .\build\build.ps1
    .\build\build.ps1 -Config Release
    .\build\build.ps1 -Config Release -Release
    .\build\build.ps1 -Config Release -Release -Installer
#>
[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Config = "Debug",
    [switch]$Release,
    [switch]$Installer
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
$Sln = Join-Path $RepoRoot "src\SwitchcraftKeys.slnx"
$ProjPath = Join-Path $RepoRoot "src\SwitchcraftKeys\SwitchcraftKeys.csproj"

# Get current version from .csproj
[xml]$proj = Get-Content $ProjPath
$version = $proj.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1

# Get git commit SHA (short form)
$sha = "unknown"
try {
    $sha = & git -C $RepoRoot rev-parse --short HEAD 2>$null
    if ($LASTEXITCODE -ne 0) { $sha = "unknown" }
}
catch { $sha = "unknown" }

$informational = "$version+$sha"

# Validate Release + Installer params
if ($Installer -and -not $Release) {
    Write-Host "[FAIL] -Installer requires -Release" -ForegroundColor Red
    exit 1
}

if (($Release -or $Installer) -and $Config -ne "Release") {
    Write-Host "[FAIL] -Release and -Installer require -Config Release" -ForegroundColor Red
    exit 1
}

Write-Host "==> Building SwitchcraftKeys [$Config]" -ForegroundColor Cyan
Write-Host "    Version: $informational" -ForegroundColor DarkGray

$result = dotnet build $Sln -c $Config --no-incremental 2>&1
$exitCode = $LASTEXITCODE

$result | ForEach-Object { Write-Host $_ }

if ($exitCode -ne 0) {
    Write-Host "`n[FAIL] Build failed (exit $exitCode)" -ForegroundColor Red
    exit $exitCode
}

Write-Host "`n[OK] Build succeeded [$Config] - $informational" -ForegroundColor Green

# Generate release if requested
if ($Release) {
    Write-Host "`n==> Generating release..." -ForegroundColor Cyan
    $releaseScript = Join-Path $RepoRoot "build\build-release.ps1"
    
    if ($Installer) {
        & $releaseScript -Version $version -Arch x64 -Installer
    }
    else {
        & $releaseScript -Version $version -Arch x64
    }
}

