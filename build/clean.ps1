#Requires -Version 7.0
<#
.SYNOPSIS
    Remove todos os artefatos de build locais.
.EXAMPLE
    .\build\clean.ps1
#>
[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot

Write-Host "==> Cleaning build artifacts" -ForegroundColor Cyan

$targets = @(
    (Join-Path $RepoRoot "dist"),
    (Join-Path $RepoRoot "TestResults")
)

# bin/ and obj/ under src/
Get-ChildItem -Path (Join-Path $RepoRoot "src") -Recurse -Directory |
    Where-Object { $_.Name -eq "bin" -or $_.Name -eq "obj" } |
    ForEach-Object { $targets += $_.FullName }

foreach ($target in $targets) {
    if (Test-Path $target) {
        Remove-Item -LiteralPath $target -Recurse -Force
        Write-Host "  Removed: $target" -ForegroundColor DarkGray
    }
}

Write-Host "`n[OK] Clean complete" -ForegroundColor Green
