#Requires -Version 7.0
<#
.SYNOPSIS
    Compila e executa o SwitchcraftKeys em modo Debug.
.PARAMETER Config
    Configuracao: Debug (padrao) ou Release.
.PARAMETER NoBuild
    Se presente, pula a compilacao e executa direto.
.EXAMPLE
    .\build\run.ps1
    .\build\run.ps1 -Config Release
    .\build\run.ps1 -NoBuild
#>
[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Config = "Debug",
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
$AppProj = Join-Path $RepoRoot "src\SwitchcraftKeys\SwitchcraftKeys.csproj"

# Compilar se necessario
if (-not $NoBuild) {
    Write-Host "==> Building SwitchcraftKeys [$Config]" -ForegroundColor Cyan
    & dotnet build $AppProj -c $Config --no-incremental -p:OutputType=Exe
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed!" -ForegroundColor Red
        exit 1
    }
}

Write-Host "==> Running SwitchcraftKeys [$Config]" -ForegroundColor Cyan
$env:SWITCHCRAFTKEYS_FORCE_CONSOLE_LOG = "1"
try {
    & dotnet run --project $AppProj -c $Config --no-build -p:OutputType=Exe
    exit $LASTEXITCODE
}
finally {
    Remove-Item Env:\SWITCHCRAFTKEYS_FORCE_CONSOLE_LOG -ErrorAction SilentlyContinue
}
