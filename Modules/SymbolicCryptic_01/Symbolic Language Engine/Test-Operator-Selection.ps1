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

function Read-JsonFile {
    param([Parameter(Mandatory = $true)][string]$Path)
    return Get-Content -Raw -Encoding utf8 $Path | ConvertFrom-Json
}

function Write-JsonFile {
    param([Parameter(Mandatory = $true)][string]$Path, [Parameter(Mandatory = $true)][object]$Object)
    $Object | ConvertTo-Json -Depth 40 | Set-Content -Encoding utf8 $Path
}

function Normalize-Node {
    param([object]$Node)
    if ($null -eq $Node) { return $null }
    if ($Node -is [System.Collections.IDictionary]) {
        $ordered = [ordered]@{}
        foreach ($k in @($Node.Keys) | Sort-Object) { $ordered[[string]$k] = Normalize-Node $Node[$k] }
        return [pscustomobject]$ordered
    }
    if ($Node -is [pscustomobject]) {
        $ordered = [ordered]@{}
        foreach ($p in @($Node.PSObject.Properties.Name) | Sort-Object) { $ordered[$p] = Normalize-Node $Node.$p }
        return [pscustomobject]$ordered
    }
    if ($Node -is [System.Collections.IEnumerable] -and -not ($Node -is [string])) {
        $arr = @()
        foreach ($i in $Node) { $arr += ,(Normalize-Node $i) }
        return $arr
    }
    return $Node
}

function Get-CanonicalJson {
    param([object]$Node)
    return ((Normalize-Node $Node) | ConvertTo-Json -Depth 50 -Compress)
}

function Get-Sha256 {
    param([string]$Text)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
    return ([System.BitConverter]::ToString($sha.ComputeHash($bytes)).Replace('-', '').ToLowerInvariant())
}

function Merge-Objects {
    param([object]$Base, [object]$Override)
    if ($null -eq $Base) { return $Override }
    if ($null -eq $Override) { return $Base }

    if ($Base -is [pscustomobject] -or $Base -is [System.Collections.IDictionary]) {
        if (-not ($Override -is [pscustomobject] -or $Override -is [System.Collections.IDictionary])) { return $Override }
        $result = [ordered]@{}
        $baseProps = if ($Base -is [pscustomobject]) { @($Base.PSObject.Properties.Name) } else { @($Base.Keys) }
        foreach ($k in $baseProps) {
            $name = [string]$k
            $result[$name] = if ($Base -is [pscustomobject]) { $Base.$name } else { $Base[$name] }
        }
        $overrideProps = if ($Override -is [pscustomobject]) { @($Override.PSObject.Properties.Name) } else { @($Override.Keys) }
        foreach ($k in $overrideProps) {
            $name = [string]$k
            $ov = if ($Override -is [pscustomobject]) { $Override.$name } else { $Override[$name] }
            if ($result.Contains($name)) {
                $result[$name] = Merge-Objects -Base $result[$name] -Override $ov
            } else {
                $result[$name] = $ov
            }
        }
        return [pscustomobject]$result
    }

    if ($Base -is [System.Collections.IEnumerable] -and -not ($Base -is [string])) { return $Override }
    return $Override
}

function Deep-Clone {
    param([object]$Node)
    return ($Node | ConvertTo-Json -Depth 50 | ConvertFrom-Json)
}

$reasonsCatalog = @(
    "ROLE_NOT_ALLOWED",
    "TRADE_NOT_ALLOWED",
    "UNSIGNED_REPO_DENIED",
    "SEAL_ADMISSION_REQUIRED",
    "EXPOSURE_POLICY_WEAKENING_DENIED",
    "TOOL_PERMISSION_OUT_OF_SCOPE"
)
$reasonSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($r in $reasonsCatalog) { [void]$reasonSet.Add($r) }

$governanceDir = Join-Path $ModulePath "Governance"
$manifestPath = Join-Path $governanceDir "oan.operator_selection_manifest.v0.1.0.json"
$oePath = Join-Path $governanceDir "oan.oe_header.v0.1.0.json"
$roleManifestPath = Join-Path $governanceDir "oan.role_manifest.v0.1.0.json"
$charterPath = Join-Path $governanceDir "oan.career_charter.v0.1.0.json"
$bondPath = Join-Path $governanceDir "bonding_events.sample.v0.1.0.json"

$required = @($manifestPath, $oePath, $roleManifestPath, $charterPath, $bondPath)
foreach ($f in $required) {
    if (-not (Test-Path -Path $f -PathType Leaf)) {
        Write-Error "Missing required governance input: $f"
        exit 1
    }
}

$manifest = Read-JsonFile -Path $manifestPath
$oe = Read-JsonFile -Path $oePath
$roleManifest = Read-JsonFile -Path $roleManifestPath
$charter = Read-JsonFile -Path $charterPath
$bond = Read-JsonFile -Path $bondPath

$errors = New-Object System.Collections.Generic.List[string]

$profileFiles = @(Get-ChildItem -File -Path $governanceDir -Filter "profile.*.json" | Select-Object -ExpandProperty FullName)
$profilesById = @{}
foreach ($pf in $profileFiles) {
    $p = Read-JsonFile -Path $pf
    $profilesById[[string]$p.profile_id] = $p
    if ([string]$p.schema -ne "oan.operator_profile.v0.1.0") { $errors.Add("Invalid operator profile schema: $pf") }
    if (@("personal","enterprise","government") -notcontains [string]$p.tier) { $errors.Add("Invalid profile tier in $pf") }
    if (@("individual","corporate","governmental") -notcontains [string]$p.taxonomy_class) { $errors.Add("Invalid profile taxonomy_class in $pf") }
    if (@("tutorial","collaborative","operational","directive") -notcontains [string]$p.explanation_policy) { $errors.Add("Invalid profile explanation_policy in $pf") }
    if (@("suggestive","confirm_required","policy_bound","locked_procedural") -notcontains [string]$p.autonomy_policy) { $errors.Add("Invalid profile autonomy_policy in $pf") }
}

# Enum checks for manifest
if ([string]$manifest.schema -ne "oan.operator_selection_manifest.v0.1.0") { $errors.Add("Invalid operator manifest schema id") }
if (@("personal","enterprise","government") -notcontains [string]$manifest.tier) { $errors.Add("Invalid manifest tier") }
if (@("individual","corporate","governmental") -notcontains [string]$manifest.taxonomy_class) { $errors.Add("Invalid taxonomy_class") }
if (@("tutorial","collaborative","operational","directive") -notcontains [string]$manifest.explanation_policy) { $errors.Add("Invalid explanation_policy") }
if (@("suggestive","confirm_required","policy_bound","locked_procedural") -notcontains [string]$manifest.autonomy_policy) { $errors.Add("Invalid autonomy_policy") }
foreach ($code in @($manifest.denial_reason_codes)) {
    if (-not $reasonSet.Contains([string]$code)) { $errors.Add("Unknown denial reason code in manifest: $code") }
}

if (-not $profilesById.ContainsKey([string]$manifest.profile_id)) {
    $errors.Add("profile_id not found: $($manifest.profile_id)")
}

$resolvedOrder = New-Object System.Collections.Generic.List[string]
$permMarks = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
$tempMarks = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)

function Resolve-Profile {
    param([string]$ProfileId)
    if ($permMarks.Contains($ProfileId)) { return }
    if ($tempMarks.Contains($ProfileId)) {
        $errors.Add("Profile inheritance cycle detected at $ProfileId")
        return
    }
    if (-not $profilesById.ContainsKey($ProfileId)) {
        $errors.Add("Unknown profile reference: $ProfileId")
        return
    }

    $tempMarks.Add($ProfileId) | Out-Null
    $p = $profilesById[$ProfileId]
    foreach ($parent in @($p.inherits)) { Resolve-Profile -ProfileId ([string]$parent) }
    $tempMarks.Remove($ProfileId) | Out-Null
    $permMarks.Add($ProfileId) | Out-Null
    $resolvedOrder.Add($ProfileId)
}

foreach ($baseId in @($manifest.inherits)) { Resolve-Profile -ProfileId ([string]$baseId) }
Resolve-Profile -ProfileId ([string]$manifest.profile_id)

$effective = $null
foreach ($id in @($resolvedOrder.ToArray())) {
    $effective = Merge-Objects -Base $effective -Override $profilesById[$id]
}

# Manifest-local overrides (hard override)
$manifestOverrides = [ordered]@{
    profile_id = $manifest.profile_id
    tier = $manifest.tier
    taxonomy_class = $manifest.taxonomy_class
    explanation_policy = $manifest.explanation_policy
    autonomy_policy = $manifest.autonomy_policy
    verification_policy = $manifest.verification_policy
    telemetry_policy = $manifest.telemetry_policy
    repo_attachment_policy = $manifest.repo_attachment_policy
    security_posture = $manifest.security_posture
    seal_admission_policy = $manifest.seal_admission_policy
    allowed_careers = $manifest.allowed_careers
    allowed_jobs = $manifest.allowed_jobs
    allowed_trades = $manifest.allowed_trades
    allowed_interface_permissions = $manifest.allowed_interface_permissions
    allowed_tool_permissions = $manifest.allowed_tool_permissions
    required_exposure_policy = $manifest.required_exposure_policy
    denial_reason_codes = $manifest.denial_reason_codes
}
$effective = Merge-Objects -Base $effective -Override ([pscustomobject]$manifestOverrides)

$hash1 = Get-Sha256 (Get-CanonicalJson $effective)
$hash2 = Get-Sha256 (Get-CanonicalJson $effective)
$inheritanceDeterministic = ($hash1 -eq $hash2)
if (-not $inheritanceDeterministic) { $errors.Add("Effective profile hash is not deterministic") }

function Evaluate-OperatorPolicy {
    param(
        [object]$Oe,
        [object]$RoleM,
        [object]$Charter,
        [object]$Bond,
        [object]$Effective,
        [System.Collections.Generic.HashSet[string]]$ReasonSet
    )

    $codes = New-Object System.Collections.Generic.List[string]
    function Add-Code { param([string]$Code) if (-not $codes.Contains($Code)) { $codes.Add($Code) } }

    $allowedCareers = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($x in @($Effective.allowed_careers)) { [void]$allowedCareers.Add([string]$x) }
    $allowedJobs = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($x in @($Effective.allowed_jobs)) { [void]$allowedJobs.Add([string]$x) }
    $allowedTrades = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($x in @($Effective.allowed_trades)) { [void]$allowedTrades.Add([string]$x) }
    $allowedInterfaces = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($x in @($Effective.allowed_interface_permissions)) { [void]$allowedInterfaces.Add([string]$x) }
    $allowedTools = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($x in @($Effective.allowed_tool_permissions)) { [void]$allowedTools.Add([string]$x) }
    $allowedRepoClasses = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($x in @($Effective.repo_attachment_policy.allowed_repo_classes)) { [void]$allowedRepoClasses.Add([string]$x) }

    $activeCareer = [string]$Oe.activation_selectors.active_career_id
    $activeJob = [string]$Oe.activation_selectors.active_job_id
    if (-not $allowedCareers.Contains($activeCareer) -or -not $allowedJobs.Contains($activeJob)) { Add-Code "ROLE_NOT_ALLOWED" }

    foreach ($t in @($Oe.activation_selectors.enabled_trades)) {
        if (-not $allowedTrades.Contains([string]$t)) { Add-Code "TRADE_NOT_ALLOWED" }
    }

    foreach ($rc in @($Oe.activation_selectors.attached_repo_classes)) {
        if (-not $allowedRepoClasses.Contains([string]$rc)) { Add-Code "UNSIGNED_REPO_DENIED" }
    }
    if ([bool]$Effective.repo_attachment_policy.forbidden_unsigned -and [int]$Oe.activation_selectors.unsigned_repo_count -gt 0) {
        Add-Code "UNSIGNED_REPO_DENIED"
    }

    if ([bool]$Effective.seal_admission_policy.required -and -not [bool]$Oe.seal_admission.seal_present) { Add-Code "SEAL_ADMISSION_REQUIRED" }

    if ([string]$Oe.exposure_policy.prime_mode -ne [string]$Effective.required_exposure_policy.prime_mode) {
        Add-Code "EXPOSURE_POLICY_WEAKENING_DENIED"
    }
    if ([bool]$Effective.required_exposure_policy.cryptic_payload_in_prime_forbidden -and -not [bool]$Oe.exposure_policy.cryptic_payload_in_prime_forbidden) {
        Add-Code "EXPOSURE_POLICY_WEAKENING_DENIED"
    }

    foreach ($role in @($RoleM.roles)) {
        if (-not $allowedCareers.Contains([string]$role.career_id) -or -not $allowedJobs.Contains([string]$role.job_id)) {
            Add-Code "ROLE_NOT_ALLOWED"
        }
        foreach ($tr in @($role.allowed_trades)) { if (-not $allowedTrades.Contains([string]$tr)) { Add-Code "TRADE_NOT_ALLOWED" } }
        foreach ($tp in @($role.tool_permissions)) { if (-not $allowedTools.Contains([string]$tp)) { Add-Code "TOOL_PERMISSION_OUT_OF_SCOPE" } }
        foreach ($ip in @($role.interface_permissions)) { if (-not $allowedInterfaces.Contains([string]$ip)) { Add-Code "TOOL_PERMISSION_OUT_OF_SCOPE" } }
    }

    foreach ($career in @($Charter.careers)) {
        if (-not $allowedCareers.Contains([string]$career.career_id)) { Add-Code "ROLE_NOT_ALLOWED" }
        foreach ($tr in @($career.allowed_trades)) { if (-not $allowedTrades.Contains([string]$tr)) { Add-Code "TRADE_NOT_ALLOWED" } }
    }

    foreach ($evt in @($Bond.events)) {
        if ($null -eq $evt.lease) { continue }
        if (-not $allowedCareers.Contains([string]$evt.career_id) -or -not $allowedJobs.Contains([string]$evt.job_id)) {
            Add-Code "ROLE_NOT_ALLOWED"
        }
        foreach ($tp in @($evt.lease.tool_permissions)) { if (-not $allowedTools.Contains([string]$tp)) { Add-Code "TOOL_PERMISSION_OUT_OF_SCOPE" } }
    }

    $unknown = @($codes | Where-Object { -not $ReasonSet.Contains([string]$_) })
    return [pscustomobject]@{
        codes = @($codes)
        unknown_codes = $unknown
        pass = (@($codes).Count -eq 0)
    }
}

$baselineEval = Evaluate-OperatorPolicy -Oe $oe -RoleM $roleManifest -Charter $charter -Bond $bond -Effective $effective -ReasonSet $reasonSet
foreach ($u in @($baselineEval.unknown_codes)) { $errors.Add("Unknown emitted denial code: $u") }

# reason-code correctness checks via deterministic synthetic scenarios
$scenarioResults = New-Object System.Collections.Generic.List[object]
$scenarioChecksPass = $true

function Run-Scenario {
    param([string]$Name, [scriptblock]$Mutator, [string]$ExpectedCode)
    $oeC = Deep-Clone $oe
    $roleC = Deep-Clone $roleManifest
    $charterC = Deep-Clone $charter
    $bondC = Deep-Clone $bond
    $effC = Deep-Clone $effective
    & $Mutator $oeC $roleC $charterC $bondC $effC
    $res = Evaluate-OperatorPolicy -Oe $oeC -RoleM $roleC -Charter $charterC -Bond $bondC -Effective $effC -ReasonSet $reasonSet
    $ok = (@($res.codes) -contains $ExpectedCode)
    if (-not $ok) { $script:scenarioChecksPass = $false }
    $scenarioResults.Add([pscustomobject]@{ name = $Name; expected = $ExpectedCode; codes = @($res.codes); pass = $ok })
}

Run-Scenario -Name "role_not_allowed" -ExpectedCode "ROLE_NOT_ALLOWED" -Mutator { param($oeM,$r,$c,$b,$e) $oeM.activation_selectors.active_career_id = "career.unknown" }
Run-Scenario -Name "trade_not_allowed" -ExpectedCode "TRADE_NOT_ALLOWED" -Mutator { param($oeM,$r,$c,$b,$e) $oeM.activation_selectors.enabled_trades += "trade.unknown" }
Run-Scenario -Name "unsigned_repo_denied" -ExpectedCode "UNSIGNED_REPO_DENIED" -Mutator { param($oeM,$r,$c,$b,$e) $oeM.activation_selectors.unsigned_repo_count = 1; $e.repo_attachment_policy.forbidden_unsigned = $true }
Run-Scenario -Name "seal_required" -ExpectedCode "SEAL_ADMISSION_REQUIRED" -Mutator { param($oeM,$r,$c,$b,$e) $e.seal_admission_policy.required = $true; $oeM.seal_admission.seal_present = $false }
Run-Scenario -Name "exposure_weaken" -ExpectedCode "EXPOSURE_POLICY_WEAKENING_DENIED" -Mutator { param($oeM,$r,$c,$b,$e) $oeM.exposure_policy.cryptic_payload_in_prime_forbidden = $false }
Run-Scenario -Name "tool_out_of_scope" -ExpectedCode "TOOL_PERMISSION_OUT_OF_SCOPE" -Mutator { param($oeM,$r,$c,$b,$e) $b.events[0].lease.tool_permissions += "tool.unauthorized" }

if (-not $scenarioChecksPass) { $errors.Add("One or more denial reason code scenarios failed") }

$checks = [ordered]@{
    operator_profile_policy_applied = ($errors.Count -eq 0)
    operator_hard_override_enforced = ($baselineEval.pass)
    operator_inheritance_deterministic = $inheritanceDeterministic
    operator_denial_reason_codes_valid = ($scenarioChecksPass -and @($baselineEval.unknown_codes).Count -eq 0)
}

$report = [ordered]@{
    schema = "oan.operator_selection_report.v0.1.0"
    pass = ($errors.Count -eq 0 -and $baselineEval.pass)
    effective_profile_id = [string]$effective.profile_id
    effective_policy_hash = $hash1
    profile_resolution_order = @($resolvedOrder.ToArray())
    denial_reason_codes = @($baselineEval.codes)
    checks = $checks
    scenario_checks = @($scenarioResults.ToArray())
    errors = @($errors)
}

$outPath = Join-Path $TelemetryDir "operator_selection_report.json"
Write-JsonFile -Path $outPath -Object $report

if (Test-Path -Path $CognitionTelemetryPath -PathType Leaf) {
    $cognition = Read-JsonFile -Path $CognitionTelemetryPath
    if (-not ($cognition.PSObject.Properties.Name -contains "metrics") -or $null -eq $cognition.metrics) { $cognition | Add-Member -MemberType NoteProperty -Name metrics -Value ([ordered]@{}) }

    $metricsMap = [ordered]@{}
    foreach ($prop in $cognition.metrics.PSObject.Properties) { $metricsMap[$prop.Name] = $prop.Value }

    if (-not $metricsMap.Contains("layer0")) { $metricsMap["layer0"] = [ordered]@{} }
    $layer0 = [ordered]@{}
    if ($metricsMap["layer0"] -is [pscustomobject]) { foreach ($prop in $metricsMap["layer0"].PSObject.Properties) { $layer0[$prop.Name] = $prop.Value } }
    elseif ($metricsMap["layer0"] -is [System.Collections.IDictionary]) { foreach ($k in $metricsMap["layer0"].Keys) { $layer0[[string]$k] = $metricsMap["layer0"][$k] } }

    $layer0["operator"] = [ordered]@{
        effective_profile_id = $report.effective_profile_id
        effective_policy_hash = $report.effective_policy_hash
        denial_reason_codes = $report.denial_reason_codes
        checks = $report.checks
    }

    $metricsMap["layer0"] = [pscustomobject]$layer0
    if (-not $metricsMap.Contains("layer_0")) { $metricsMap["layer_0"] = [ordered]@{} }
    $layer_0 = [ordered]@{}
    if ($metricsMap["layer_0"] -is [pscustomobject]) { foreach ($prop in $metricsMap["layer_0"].PSObject.Properties) { $layer_0[$prop.Name] = $prop.Value } }
    elseif ($metricsMap["layer_0"] -is [System.Collections.IDictionary]) { foreach ($k in $metricsMap["layer_0"].Keys) { $layer_0[[string]$k] = $metricsMap["layer_0"][$k] } }
    $layer_0["operator"] = $layer0["operator"]
    $metricsMap["layer_0"] = [pscustomobject]$layer_0

    $cognition.metrics = [pscustomobject]$metricsMap
    Write-JsonFile -Path $CognitionTelemetryPath -Object $cognition
}

Write-Host "Wrote operator selection report: $outPath"
if ($report.pass -eq $false) {
    foreach ($e in @($errors)) { Write-Error $e }
    foreach ($c in @($baselineEval.codes)) { Write-Error "Operator policy denial: $c" }
    exit 1
}
exit 0
