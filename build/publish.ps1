#Requires -Version 7.0
<#
.SYNOPSIS
    Publica o SwitchcraftKeys como single-file .exe em dist/.
.PARAMETER Version
    Sobrescreve a versao lida do .csproj.
.EXAMPLE
    .\build\publish.ps1
    .\build\publish.ps1 -Version 0.2.0
#>
[CmdletBinding()]
param(
    [string]$Version = ""
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
$ProjPath = Join-Path $RepoRoot "src\SwitchcraftKeys\SwitchcraftKeys.csproj"
$DistDir  = Join-Path $RepoRoot "dist"

# Read version from .csproj if not provided
if (-not $Version) {
    [xml]$proj = Get-Content $ProjPath
    $Version = $proj.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
    if (-not $Version) {
        Write-Error "Could not read <Version> from $ProjPath"
        exit 1
    }
}

$OutputExe = "switchcraft-keys-v$Version-win-x64.exe"
$OutputPath = Join-Path $DistDir $OutputExe

Write-Host "==> Publishing SwitchcraftKeys v$Version" -ForegroundColor Cyan

New-Item -ItemType Directory -Path $DistDir -Force | Out-Null

$result = dotnet publish $ProjPath `
    -c Release `
    -r win-x64 `
    --no-self-contained `
    -p:PublishSingleFile=true `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    -o "$DistDir\_staging" 2>&1

$exitCode = $LASTEXITCODE
$result | ForEach-Object { Write-Host $_ }

if ($exitCode -ne 0) {
    Write-Host "`n[FAIL] Publish failed (exit $exitCode)" -ForegroundColor Red
    exit $exitCode
}

$staged = Get-ChildItem "$DistDir\_staging" -Filter "*.exe" | Select-Object -First 1
if (-not $staged) {
    Write-Error "No .exe found in staging output."
}

Move-Item -LiteralPath $staged.FullName -Destination $OutputPath -Force
Remove-Item -LiteralPath "$DistDir\_staging" -Recurse -Force

$size = [math]::Round((Get-Item $OutputPath).Length / 1MB, 2)
Write-Host "`n[OK] Published: $OutputPath ($size MB)" -ForegroundColor Green
