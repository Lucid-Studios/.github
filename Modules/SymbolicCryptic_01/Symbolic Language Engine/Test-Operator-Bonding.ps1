[CmdletBinding()]
param(
    [string]$ModulePath,
    [string]$TelemetryDir,
    [string]$CognitionTelemetryPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($ModulePath)) { $ModulePath = $PSScriptRoot }
if ([string]::IsNullOrWhiteSpace($TelemetryDir)) { $TelemetryDir = Join-Path $ModulePath "telemetry" }
if ([string]::IsNullOrWhiteSpace($CognitionTelemetryPath)) { $CognitionTelemetryPath = Join-Path $TelemetryDir "cognition_telemetry.json" }
if (-not (Test-Path -Path $TelemetryDir -PathType Container)) { New-Item -ItemType Directory -Path $TelemetryDir | Out-Null }

. (Join-Path $ModulePath "Governance-Founding-Helpers.ps1")

$gov = Join-Path $ModulePath "Governance"
$identityPath = Join-Path $gov "oan.operator_identity_record.v0.1.0.json"
$bondingPath = Join-Path $gov "oan.operator_bonding_set.v0.1.0.json"
$manifestPath = Join-Path $gov "oan.operator_selection_manifest.v0.1.0.json"
$roleManifestPath = Join-Path $gov "oan.role_manifest.v0.1.0.json"
$charterPath = Join-Path $gov "oan.career_charter.v0.1.0.json"
$firstBootReportPath = Join-Path $TelemetryDir "first_boot_report.json"

$required = @($identityPath,$bondingPath,$manifestPath,$roleManifestPath,$charterPath,$firstBootReportPath)
foreach ($f in $required) {
    if (-not (Test-Path -Path $f -PathType Leaf)) { Write-Error "Missing bonding input: $f"; exit 1 }
}

$identity = Read-FoundingJson $identityPath
$bonding = Read-FoundingJson $bondingPath
$manifest = Read-FoundingJson $manifestPath
$roleManifest = Read-FoundingJson $roleManifestPath
$charter = Read-FoundingJson $charterPath
$firstBootReport = Read-FoundingJson $firstBootReportPath

$errors = New-Object System.Collections.Generic.List[string]
$codes = New-Object System.Collections.Generic.List[string]
function Add-Code { param([string]$Code, [string]$Message)
    if (-not $codes.Contains($Code)) { $codes.Add($Code) }
    $errors.Add($Message)
}

if (-not [bool]$firstBootReport.pass -or -not [bool]$firstBootReport.sanctuary_constituted) {
    Add-Code "BONDING_FOUNDING_REQUIRED" "Founding must pass and be constituted before bonding validation."
}

if ([string]$bonding.operator_manifest_ref.schema -ne "oan.operator_selection_manifest.v0.1.0") {
    Add-Code "BONDING_PROFILE_MISMATCH" "Bonding set operator manifest reference schema mismatch."
}
if ([string]$bonding.operator_identity_ref.schema -ne "oan.operator_identity_record.v0.1.0") {
    Add-Code "BONDING_IDENTITY_MISMATCH" "Bonding set operator identity reference schema mismatch."
}

if (-not [bool]$bonding.anti_downgrade_required) {
    Add-Code "POLICY_ANTIBLEED_WEAKENING_DENIED" "Bonding set must require anti-downgrade enforcement."
}

$allowedCareers = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($c in @($manifest.allowed_careers)) { [void]$allowedCareers.Add([string]$c) }
$allowedJobs = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($j in @($manifest.allowed_jobs)) { [void]$allowedJobs.Add([string]$j) }
$allowedTrades = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($t in @($manifest.allowed_trades)) { [void]$allowedTrades.Add([string]$t) }
$allowedTools = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($p in @($manifest.allowed_tool_permissions)) { [void]$allowedTools.Add([string]$p) }
$allowedRepos = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($r in @($manifest.repo_attachment_policy.allowed_repo_classes)) { [void]$allowedRepos.Add([string]$r) }

foreach ($c in @($bonding.authority_scope.allowed_careers)) {
    if (-not $allowedCareers.Contains([string]$c)) { Add-Code "BONDING_AUTHORITY_SCOPE_VIOLATION" "Bonding career not allowed by profile: $c" }
}
foreach ($j in @($bonding.authority_scope.allowed_jobs)) {
    if (-not $allowedJobs.Contains([string]$j)) { Add-Code "BONDING_AUTHORITY_SCOPE_VIOLATION" "Bonding job not allowed by profile: $j" }
}
foreach ($t in @($bonding.authority_scope.allowed_trades)) {
    if (-not $allowedTrades.Contains([string]$t)) { Add-Code "BONDING_AUTHORITY_SCOPE_VIOLATION" "Bonding trade not allowed by profile: $t" }
}
foreach ($tp in @($bonding.authority_scope.allowed_tool_permissions)) {
    if (-not $allowedTools.Contains([string]$tp)) { Add-Code "BONDING_AUTHORITY_SCOPE_VIOLATION" "Bonding tool permission out of profile scope: $tp" }
}
foreach ($rc in @($bonding.authority_scope.allowed_repo_classes)) {
    if (-not $allowedRepos.Contains([string]$rc)) { Add-Code "BONDING_AUTHORITY_SCOPE_VIOLATION" "Bonding repo class not allowed by profile: $rc" }
}

foreach ($role in @($roleManifest.roles)) {
    if (-not $allowedCareers.Contains([string]$role.career_id) -or -not $allowedJobs.Contains([string]$role.job_id)) {
        Add-Code "BONDING_PROFILE_MISMATCH" "Role manifest career/job outside profile scope."
    }
}
foreach ($career in @($charter.careers)) {
    if (-not $allowedCareers.Contains([string]$career.career_id)) { Add-Code "BONDING_PROFILE_MISMATCH" "Career charter includes disallowed career: $($career.career_id)" }
}

if ([string]$identity.founding_actor.actor_id -eq "" -or [string]$identity.bonded_operator.operator_id -eq "") {
    Add-Code "BONDING_IDENTITY_MISMATCH" "Founding actor and bonded operator must both be present."
}

$report = [ordered]@{
    schema = "oan.operator_bonding_report.v0.1.0"
    pass = ($errors.Count -eq 0)
    founding_actor_id = [string]$identity.founding_actor.actor_id
    bonded_operator_id = [string]$identity.bonded_operator.operator_id
    profile_id = [string]$manifest.profile_id
    denial_reason_codes = @($codes)
    errors = @($errors)
}

$outPath = Join-Path $TelemetryDir "operator_bonding_report.json"
Write-FoundingJson -Path $outPath -Object $report
Merge-FoundingLayer0Telemetry -CognitionTelemetryPath $CognitionTelemetryPath -SectionName "bonding" -SectionValue ([ordered]@{
    founding_actor_id = $report.founding_actor_id
    bonded_operator_id = $report.bonded_operator_id
    denial_reason_codes = $report.denial_reason_codes
})

Write-Host "Wrote operator bonding report: $outPath"
if (-not $report.pass) {
    foreach ($e in @($errors)) { Write-Error $e }
    exit 1
}
exit 0
