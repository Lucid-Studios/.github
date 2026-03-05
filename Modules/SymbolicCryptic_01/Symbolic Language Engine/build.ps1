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
exit $LASTEXITCODE
