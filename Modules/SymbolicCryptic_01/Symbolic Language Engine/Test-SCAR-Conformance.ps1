[CmdletBinding()]
param(
    [string]$OutDir,
    [string]$ModulePath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
if ([string]::IsNullOrWhiteSpace($ModulePath)) { $ModulePath = $PSScriptRoot }
if ([string]::IsNullOrWhiteSpace($OutDir)) { $OutDir = Join-Path $ModulePath "telemetry" }

$contractPath = Join-Path $ModulePath "sli.scar.adapter_contract.v0.1.0.json"
$biasPath = Join-Path $OutDir "scar_bias_spec.json"
$headPath = Join-Path $OutDir "scar_head_gate.json"
$kvPath = Join-Path $OutDir "scar_kv_anchor.json"
$telemetryPath = Join-Path $OutDir "scar_telemetry.json"

foreach ($p in @($contractPath, $biasPath, $headPath, $kvPath, $telemetryPath)) {
    if (-not (Test-Path -Path $p -PathType Leaf)) { Write-Error "Missing required SCAR artifact: $p"; exit 1 }
}

$contract = Get-Content -Raw -Encoding utf8 $contractPath | ConvertFrom-Json
$bias = Get-Content -Raw -Encoding utf8 $biasPath | ConvertFrom-Json
$head = Get-Content -Raw -Encoding utf8 $headPath | ConvertFrom-Json
$kv = Get-Content -Raw -Encoding utf8 $kvPath | ConvertFrom-Json
$tel = Get-Content -Raw -Encoding utf8 $telemetryPath | ConvertFrom-Json

$failures = New-Object System.Collections.Generic.List[string]
if ([string]$bias.schema -ne [string]$contract.outputs.bias_spec) { $failures.Add("Bias schema mismatch") }
if ([string]$head.schema -ne [string]$contract.outputs.head_gate) { $failures.Add("Head gate schema mismatch") }
if ([string]$kv.schema -ne [string]$contract.outputs.kv_anchor) { $failures.Add("KV anchor schema mismatch") }
if ([string]$tel.schema -ne [string]$contract.outputs.telemetry) { $failures.Add("Telemetry schema mismatch") }
if (@($bias.anchors).Count -lt 1) { $failures.Add("Anchors missing") }
if (@($bias.bias_blocks).Count -lt 1) { $failures.Add("Bias blocks missing") }

$result = [ordered]@{
    schema = "sli.scar.conformance.v0.1.0"
    pass = ($failures.Count -eq 0)
    failures = @($failures)
}

$outPath = Join-Path $OutDir "scar_conformance.json"
$result | ConvertTo-Json -Depth 10 | Set-Content -Encoding utf8 $outPath
Write-Host "Wrote SCAR conformance: $outPath"
if ($failures.Count -gt 0) { foreach ($f in $failures) { Write-Error $f }; exit 1 }
exit 0
