[CmdletBinding()]
param(
    [switch]$StrictReservedKeyCheck
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptPath = Join-Path $PSScriptRoot "Validate-SLE.ps1"
if (-not (Test-Path -Path $scriptPath -PathType Leaf)) {
    throw "Missing validator script: $scriptPath"
}

& $scriptPath -ModulePath $PSScriptRoot -OutDir (Join-Path $PSScriptRoot "telemetry") -StrictReservedKeyCheck:$StrictReservedKeyCheck
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$scarScriptPath = Join-Path $PSScriptRoot "Generate-SCAR.ps1"
if (-not (Test-Path -Path $scarScriptPath -PathType Leaf)) {
    throw "Missing SCAR generator script: $scarScriptPath"
}

& $scarScriptPath -ModulePath $PSScriptRoot -OutDir (Join-Path $PSScriptRoot "telemetry") -CognitionTelemetryPath (Join-Path $PSScriptRoot "telemetry\\cognition_telemetry.json")
exit $LASTEXITCODE
