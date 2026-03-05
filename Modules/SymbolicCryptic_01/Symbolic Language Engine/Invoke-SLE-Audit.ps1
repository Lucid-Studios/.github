[CmdletBinding()]
param(
    [string]$ModulePath,
    [string]$RepoRoot,
    [switch]$StrictReservedKeyCheck
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($ModulePath)) { $ModulePath = $PSScriptRoot }
if ([string]::IsNullOrWhiteSpace($RepoRoot)) { $RepoRoot = "D:\OAN Tech Stack" }

$telemetryDir = Join-Path $ModulePath "telemetry"
if (-not (Test-Path -Path $telemetryDir -PathType Container)) { New-Item -ItemType Directory -Path $telemetryDir | Out-Null }

function Normalize-JsonNode {
    param([object]$Node)
    if ($null -eq $Node) { return $null }
    if ($Node -is [System.Collections.IDictionary]) {
        $ordered = [ordered]@{}
        foreach ($k in @($Node.Keys) | Sort-Object) {
            $ordered[[string]$k] = Normalize-JsonNode $Node[$k]
        }
        return [pscustomobject]$ordered
    }
    if ($Node -is [pscustomobject]) {
        $ordered = [ordered]@{}
        foreach ($p in @($Node.PSObject.Properties.Name) | Sort-Object) {
            $ordered[$p] = Normalize-JsonNode $Node.$p
        }
        return [pscustomobject]$ordered
    }
    if ($Node -is [System.Collections.IEnumerable] -and -not ($Node -is [string])) {
        $arr = @()
        foreach ($item in $Node) { $arr += ,(Normalize-JsonNode $item) }
        return $arr
    }
    return $Node
}

function Get-NormalizedJsonHash {
    param([string]$Path)
    $obj = Get-Content -Raw -Encoding utf8 $Path | ConvertFrom-Json
    $norm = Normalize-JsonNode $obj
    $json = $norm | ConvertTo-Json -Depth 40 -Compress
    $sha = [System.Security.Cryptography.SHA256]::Create()
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
    return [System.BitConverter]::ToString($sha.ComputeHash($bytes)).Replace("-", "").ToLowerInvariant()
}

$artifacts = @(
    "scar_bias_spec.json",
    "scar_head_gate.json",
    "scar_kv_anchor.json",
    "scar_telemetry.json",
    "flow_metrics.json",
    "cognition_telemetry.json"
)

Get-ChildItem -Path $telemetryDir -Filter *.json -File | Remove-Item -Force

$buildScript = Join-Path $ModulePath "build.ps1"
& $buildScript -StrictReservedKeyCheck:$StrictReservedKeyCheck
if ($LASTEXITCODE -ne 0) { Write-Error "Build failed during audit run #1"; exit $LASTEXITCODE }

$run1 = @{}
foreach ($a in $artifacts) {
    $p = Join-Path $telemetryDir $a
    if (-not (Test-Path -Path $p -PathType Leaf)) { Write-Error "Missing artifact after run #1: $a"; exit 1 }
    $run1[$a] = Get-NormalizedJsonHash $p
}

& $buildScript -StrictReservedKeyCheck:$StrictReservedKeyCheck
if ($LASTEXITCODE -ne 0) { Write-Error "Build failed during audit run #2"; exit $LASTEXITCODE }

$run2 = @{}
$determinismFailures = New-Object System.Collections.Generic.List[string]
foreach ($a in $artifacts) {
    $p = Join-Path $telemetryDir $a
    if (-not (Test-Path -Path $p -PathType Leaf)) { Write-Error "Missing artifact after run #2: $a"; exit 1 }
    $run2[$a] = Get-NormalizedJsonHash $p
    if ($run1[$a] -ne $run2[$a]) { $determinismFailures.Add($a) }
}

$flow = Get-Content -Raw -Encoding utf8 (Join-Path $telemetryDir "flow_metrics.json") | ConvertFrom-Json
$cognition = Get-Content -Raw -Encoding utf8 (Join-Path $telemetryDir "cognition_telemetry.json") | ConvertFrom-Json
$scar = Get-Content -Raw -Encoding utf8 (Join-Path $telemetryDir "scar_telemetry.json") | ConvertFrom-Json
$coverage = Get-Content -Raw -Encoding utf8 (Join-Path $telemetryDir "token_node_coverage.json") | ConvertFrom-Json
$sidecarVerify = Get-Content -Raw -Encoding utf8 (Join-Path $telemetryDir "governance_sidecars\\governance_sidecar_verify.json") | ConvertFrom-Json
$bondingReport = Get-Content -Raw -Encoding utf8 (Join-Path $telemetryDir "bonding_contract_report.json") | ConvertFrom-Json
$operatorReport = Get-Content -Raw -Encoding utf8 (Join-Path $telemetryDir "operator_selection_report.json") | ConvertFrom-Json
$constructor = Get-Content -Raw -Encoding utf8 (Join-Path $ModulePath "SymbolicKeyConstructor_ReservedExpanded.json") | ConvertFrom-Json
$rootBaselinePath = Join-Path $ModulePath "RootIndex.sha256"
$rootHashCurrent = (Get-FileHash -Algorithm SHA256 (Join-Path $ModulePath "RootIndex.json")).Hash.ToLowerInvariant()
$rootBaseline = (Get-Content -Raw -Encoding utf8 $rootBaselinePath).Trim().Split('=')[-1]

$checks = [ordered]@{}
$checks["determinism_pass"] = ($determinismFailures.Count -eq 0)
$checks["rootindex_unchanged"] = ($rootHashCurrent -eq $rootBaseline)
$checks["reserved_hard_ban"] = ([string]$constructor.assignment_rules.reserved_enforcement.mode -eq "hard_ban")
$checks["reserved_literals_only"] = ([string]$constructor.assignment_rules.reserved_enforcement.parse_mode -eq "allow_literals_only")
$checks["uniqueness_scope_expanded"] = ([string]$constructor.assignment_rules.symbol_uniqueness -match "OperatorIndex" -and [string]$constructor.assignment_rules.symbol_uniqueness -match "GrammarSheafIndex")
$checks["fractional_metadata_only"] = (@($flow.metrics.fractional_path_violations).Count -eq 0)
$checks["telemetry_layer0_scar"] = ($null -ne $cognition.metrics.layer0.scar)
$checks["telemetry_layer_placeholders"] = ($cognition.metrics.layer_1.semantic_flow -eq $null -and $cognition.metrics.layer_2.drift -eq $null -and $cognition.metrics.layer_3.flow -eq $null)
$checks["glue_isolation"] = (-not (($scar.glue_mappings_active -eq 0) -and ($scar.scope_barrier_overrides -gt 0)))
$checks["coverage_report_present"] = ($null -ne $coverage -and $null -ne $coverage.tokens)
$checks["coverage_target"] = ([double]$coverage.coverage_rate -ge 0.70)
$checks["governance_sidecar_verification"] = [bool]$sidecarVerify.pass
$checks["bonding_chain_monotonic"] = [bool]$bondingReport.checks.bonding_chain_monotonic
$checks["bonding_prime_cryptic_separation"] = [bool]$bondingReport.checks.prime_cryptic_separation
$checks["bonding_role_charter_alignment"] = [bool]$bondingReport.checks.role_charter_alignment
$checks["bonding_anti_bleed"] = [bool]$bondingReport.checks.anti_bleed
$checks["operator_manifest_signature_verified"] = [bool]$sidecarVerify.pass
$checks["operator_profile_policy_applied"] = [bool]$operatorReport.checks.operator_profile_policy_applied
$checks["operator_hard_override_enforced"] = [bool]$operatorReport.checks.operator_hard_override_enforced
$checks["operator_inheritance_deterministic"] = [bool]$operatorReport.checks.operator_inheritance_deterministic
$checks["operator_denial_reason_codes_valid"] = [bool]$operatorReport.checks.operator_denial_reason_codes_valid
$checks["operator_repo_policy_enforced"] = (-not (@($operatorReport.denial_reason_codes) -contains "UNSIGNED_REPO_DENIED"))
$checks["operator_seal_admission_enforced"] = (-not (@($operatorReport.denial_reason_codes) -contains "SEAL_ADMISSION_REQUIRED"))

$allPass = $true
foreach ($v in $checks.Values) { if (-not [bool]$v) { $allPass = $false } }
if ($determinismFailures.Count -gt 0) { $allPass = $false }

$audit = [ordered]@{
    schema = "sli.audit_report.v0.1.0"
    pass = $allPass
    determinism = [ordered]@{
        pass = ($determinismFailures.Count -eq 0)
        failures = @($determinismFailures)
        run1_hashes = $run1
        run2_hashes = $run2
    }
    checks = $checks
    flow_score = $flow.flow_score
    scar_coverage = $coverage.coverage_rate
    glue = [ordered]@{
        maps_loaded = $scar.glue_maps_loaded
        mappings_active = $scar.glue_mappings_active
        scope_barrier_overrides = $scar.scope_barrier_overrides
    }
    governance = [ordered]@{
        sidecar_pass = [bool]$sidecarVerify.pass
        bonding_pass = [bool]$bondingReport.pass
        operator_pass = [bool]$operatorReport.pass
    }
}

$auditPath = Join-Path $telemetryDir "audit_report.json"
$audit | ConvertTo-Json -Depth 30 | Set-Content -Encoding utf8 $auditPath

$reportDir = Join-Path $RepoRoot "docs\audits"
if (-not (Test-Path -Path $reportDir -PathType Container)) { New-Item -ItemType Directory -Path $reportDir | Out-Null }
$mdPath = Join-Path $reportDir "PHASE2_AUDIT.md"
$lines = New-Object System.Collections.Generic.List[string]
$lines.Add("# Phase 2 Audit Report") | Out-Null
$lines.Add("") | Out-Null
$lines.Add("- Pass: $($audit.pass)") | Out-Null
$lines.Add("- Flow Score: $($audit.flow_score)") | Out-Null
$lines.Add("- SCAR Coverage: $($audit.scar_coverage)") | Out-Null
$lines.Add("") | Out-Null
$lines.Add("## Determinism") | Out-Null
$lines.Add("- Pass: $($audit.determinism.pass)") | Out-Null
if ($determinismFailures.Count -gt 0) {
    foreach ($f in $determinismFailures) { $lines.Add("- Failed artifact: $f") | Out-Null }
}
$lines.Add("") | Out-Null
$lines.Add("## Contract Checks") | Out-Null
foreach ($k in $checks.Keys) { $lines.Add(("- {0}: {1}" -f $k, $checks[$k])) | Out-Null }
$lines.Add("") | Out-Null
$lines.Add("## Coverage Breakdown") | Out-Null
foreach ($t in @($coverage.tokens)) {
    $lines.Add("- pos=$($t.pos) text='$($t.text)' node=$($t.node_ref) reason=$($t.reason) eligible=$($t.eligible)") | Out-Null
}
$lines | Set-Content -Encoding utf8 $mdPath

Write-Host "Wrote audit JSON: $auditPath"
Write-Host "Wrote audit markdown: $mdPath"

if (-not $allPass) {
    Write-Error "Audit failed. See $auditPath and $mdPath"
    exit 1
}
exit 0
