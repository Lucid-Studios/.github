[CmdletBinding()]
param(
    [string]$OutDir,
    [double]$MinCoverage = 0.70
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
if ([string]::IsNullOrWhiteSpace($OutDir)) { $OutDir = Join-Path $PSScriptRoot "telemetry" }
$path = Join-Path $OutDir "token_node_coverage.json"
if (-not (Test-Path -Path $path -PathType Leaf)) {
    Write-Error "Coverage report missing: $path"
    exit 1
}

$data = Get-Content -Raw -Encoding utf8 $path | ConvertFrom-Json
$coverage = [double]$data.coverage_rate
$eligible = [int]$data.eligible_token_count
$mapped = [int]$data.mapped_eligible_token_count
$unmapped = @($data.tokens | Where-Object { $_.eligible -and [string]::IsNullOrWhiteSpace([string]$_.node_ref) })
$mappedTokens = @($data.tokens | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_.node_ref) })

Write-Host "Token-node coverage summary: coverage=$coverage mapped=$mapped eligible=$eligible"
Write-Host "Mapped tokens:"
foreach ($t in $mappedTokens) { Write-Host "  pos=$($t.pos) text='$($t.text)' node=$($t.node_ref) reason=$($t.reason)" }
Write-Host "Unmapped eligible tokens:"
foreach ($t in $unmapped) { Write-Host "  pos=$($t.pos) text='$($t.text)' reason=$($t.reason)" }

if ($coverage -lt $MinCoverage) {
    Write-Error "Coverage below threshold. coverage=$coverage threshold=$MinCoverage"
    exit 1
}

Write-Host "Coverage check passed."
exit 0
