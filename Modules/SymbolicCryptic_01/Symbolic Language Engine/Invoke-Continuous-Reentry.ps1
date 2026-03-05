[CmdletBinding()]
param(
    [string]$ModulePath,
    [string]$TelemetryDir,
    [string]$CognitionTelemetryPath,
    [string]$SidecarOutDir
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($ModulePath)) { $ModulePath = $PSScriptRoot }
if ([string]::IsNullOrWhiteSpace($TelemetryDir)) { $TelemetryDir = Join-Path $ModulePath "telemetry" }
if ([string]::IsNullOrWhiteSpace($CognitionTelemetryPath)) { $CognitionTelemetryPath = Join-Path $TelemetryDir "cognition_telemetry.json" }
if ([string]::IsNullOrWhiteSpace($SidecarOutDir)) { $SidecarOutDir = Join-Path $TelemetryDir "governance_sidecars" }
if (-not (Test-Path -Path $TelemetryDir -PathType Container)) { New-Item -ItemType Directory -Path $TelemetryDir | Out-Null }

. (Join-Path $ModulePath "Governance-Founding-Helpers.ps1")

$gov = Join-Path $ModulePath "Governance"
$oePath = Join-Path $gov "oan.oe_header.v0.1.0.json"
$reentryPath = Join-Path $gov "oan.continuous_use_reentry.v0.1.0.json"
$foundingEventPath = Join-Path $gov "oan.sanctuary_founding_event.v0.1.0.json"
$identityPath = Join-Path $gov "oan.operator_identity_record.v0.1.0.json"
$manifestPath = Join-Path $gov "oan.operator_selection_manifest.v0.1.0.json"
$firstBootReportPath = Join-Path $TelemetryDir "first_boot_report.json"
$bondingReportPath = Join-Path $TelemetryDir "operator_bonding_report.json"
$sidecarVerifyPath = Join-Path $SidecarOutDir "governance_sidecar_verify.json"

$required = @($oePath,$reentryPath,$foundingEventPath,$identityPath,$manifestPath,$firstBootReportPath,$bondingReportPath,$sidecarVerifyPath)
foreach ($f in $required) {
    if (-not (Test-Path -Path $f -PathType Leaf)) { Write-Error "Missing reentry input: $f"; exit 1 }
}

$oe = Read-FoundingJson $oePath
$reentry = Read-FoundingJson $reentryPath
$foundingEvent = Read-FoundingJson $foundingEventPath
$identity = Read-FoundingJson $identityPath
$manifest = Read-FoundingJson $manifestPath
$firstBootReport = Read-FoundingJson $firstBootReportPath
$bondingReport = Read-FoundingJson $bondingReportPath
$sidecarVerify = Read-FoundingJson $sidecarVerifyPath

$errors = New-Object System.Collections.Generic.List[string]
$codes = New-Object System.Collections.Generic.List[string]
function Add-Code { param([string]$Code, [string]$Message)
    if (-not $codes.Contains($Code)) { $codes.Add($Code) }
    $errors.Add($Message)
}

if (-not [bool]$reentry.requires_prior_founding) { Add-Code "REENTRY_POLICY_INVALID" "Reentry policy must require prior founding." }
if (-not [bool]$reentry.full_replay_required) { Add-Code "REENTRY_POLICY_INVALID" "Reentry policy must require full replay validation." }

if (-not [bool]$firstBootReport.pass -or -not [bool]$firstBootReport.sanctuary_constituted) {
    Add-Code "REENTRY_FOUNDING_REQUIRED" "Reentry requires successful prior founding constitution."
}
if (-not [bool]$bondingReport.pass) { Add-Code "REENTRY_BONDING_REQUIRED" "Reentry requires successful bonding report." }
if (-not [bool]$foundingEvent.sanctuary_constituted) { Add-Code "REENTRY_CONSTITUTION_REQUIRED" "Founding event must indicate constituted sanctuary." }

if ([string]$oe.chain_head_hash -ne "sha256:bonding_chain_head_placeholder") {
    Add-Code "REENTRY_CHAIN_HEAD_MISMATCH" "OE chain head does not match expected reentry chain head." }

if ([string]$identity.bonded_operator.operator_id -ne [string]$foundingEvent.bonded_operator_id) {
    Add-Code "REENTRY_BONDED_OPERATOR_MISMATCH" "Bonded operator mismatch between identity and founding event." }

if (-not [bool]$sidecarVerify.pass) { Add-Code "REENTRY_PROFILE_SIGNATURE_MISSING" "Governance sidecar verification must pass for reentry." }

$allowedCareers = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($c in @($manifest.allowed_careers)) { [void]$allowedCareers.Add([string]$c) }
$allowedJobs = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($j in @($manifest.allowed_jobs)) { [void]$allowedJobs.Add([string]$j) }

if (-not $allowedCareers.Contains([string]$oe.activation_selectors.active_career_id) -or -not $allowedJobs.Contains([string]$oe.activation_selectors.active_job_id)) {
    Add-Code "REENTRY_ROLE_PRECONDITION_FAILED" "Reentry role preconditions failed for active career/job."
}

if ([bool]$manifest.required_exposure_policy.cryptic_payload_in_prime_forbidden -and -not [bool]$oe.exposure_policy.cryptic_payload_in_prime_forbidden) {
    Add-Code "POLICY_EXPOSURE_WEAKENING_DENIED" "Exposure policy weakening detected during reentry."
}
if ([bool]$manifest.seal_admission_policy.required -and -not [bool]$oe.seal_admission.seal_present) {
    Add-Code "POLICY_SEAL_STRICTNESS_WEAKENING_DENIED" "Seal admission strictness violation detected during reentry."
}

$report = [ordered]@{
    schema = "oan.continuous_use_reentry_report.v0.1.0"
    pass = ($errors.Count -eq 0)
    chain_head_verified = ($codes -notcontains "REENTRY_CHAIN_HEAD_MISMATCH")
    bonded_operator_verified = ($codes -notcontains "REENTRY_BONDED_OPERATOR_MISMATCH")
    profile_signature_verified = [bool]$sidecarVerify.pass
    denial_reason_codes = @($codes)
    errors = @($errors)
}

$outPath = Join-Path $TelemetryDir "continuous_use_reentry_report.json"
Write-FoundingJson -Path $outPath -Object $report
Merge-FoundingLayer0Telemetry -CognitionTelemetryPath $CognitionTelemetryPath -SectionName "reentry" -SectionValue ([ordered]@{
    chain_head_verified = $report.chain_head_verified
    bonded_operator_verified = $report.bonded_operator_verified
    profile_signature_verified = $report.profile_signature_verified
    denial_reason_codes = $report.denial_reason_codes
})

Write-Host "Wrote continuous use reentry report: $outPath"
if (-not $report.pass) {
    foreach ($e in @($errors)) { Write-Error $e }
    exit 1
}
exit 0
