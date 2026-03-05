[CmdletBinding()]
param(
    [string]$ModulePath,
    [string]$OutDir,
    [int]$LegacyDuplicateSymbolBaseline = 60,
    [int]$LegacyReservedViolationBaseline = 14,
    [switch]$StrictReservedKeyCheck
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($ModulePath)) {
    if (-not [string]::IsNullOrWhiteSpace($PSScriptRoot)) {
        $ModulePath = $PSScriptRoot
    }
    else {
        $ModulePath = Split-Path -Parent $MyInvocation.MyCommand.Path
    }
}

if ([string]::IsNullOrWhiteSpace($OutDir)) {
    $OutDir = Join-Path $ModulePath "telemetry"
}

function Read-JsonFile {
    param([Parameter(Mandatory = $true)][string]$Path)
    try {
        return Get-Content -Raw -Encoding utf8 $Path | ConvertFrom-Json
    }
    catch {
        throw "Failed to parse JSON file '$Path': $($_.Exception.Message)"
    }
}

function Is-FractionalTriplet {
    param([AllowNull()][object]$Value)
    if ($null -eq $Value) { return $false }
    return ($Value -is [string]) -and ($Value -match '^\d+\/\d+\/\d+$')
}

function Collect-SymbolEntriesFromNestedIndex {
    param(
        [Parameter(Mandatory = $true)][object]$Node,
        [Parameter(Mandatory = $true)][string]$IndexName,
        [Parameter(Mandatory = $true)][string]$PathPrefix,
        [string]$CurrentKey = ""
    )

    $results = @()
    if ($null -eq $Node) { return $results }

    if ($Node -is [System.Collections.IDictionary]) {
        foreach ($k in $Node.Keys) {
            $child = $Node[$k]
            $childPath = if ([string]::IsNullOrWhiteSpace($PathPrefix)) { [string]$k } else { "$PathPrefix/$k" }
            $results += Collect-SymbolEntriesFromNestedIndex -Node $child -IndexName $IndexName -PathPrefix $childPath -CurrentKey ([string]$k)
        }
        return $results
    }

    if ($Node -is [pscustomobject]) {
        $propNames = @($Node.PSObject.Properties.Name)
        if ($propNames -contains "symbol") {
            $symbol = $Node.symbol
            $results += [pscustomobject]@{
                index  = $IndexName
                key    = $CurrentKey
                symbol = [string]$symbol
                path   = $PathPrefix
            }
        }

        foreach ($prop in $Node.PSObject.Properties) {
            if ($prop.Name -eq "symbol") { continue }
            if ($prop.Value -is [pscustomobject] -or $prop.Value -is [System.Collections.IDictionary]) {
                $childPath = if ([string]::IsNullOrWhiteSpace($PathPrefix)) { $prop.Name } else { "$PathPrefix/$($prop.Name)" }
                $results += Collect-SymbolEntriesFromNestedIndex -Node $prop.Value -IndexName $IndexName -PathPrefix $childPath -CurrentKey $prop.Name
            }
        }
    }

    return $results
}

function Collect-Entries {
    param(
        [Parameter(Mandatory = $true)][string]$IndexName,
        [Parameter(Mandatory = $true)][object]$Container,
        [Parameter(Mandatory = $true)][string]$CollectionProperty
    )
    $results = @()
    if (-not ($Container.PSObject.Properties.Name -contains $CollectionProperty)) {
        return $results
    }

    $map = $Container.$CollectionProperty
    if ($null -eq $map) { return $results }

    foreach ($prop in $map.PSObject.Properties) {
        $key = $prop.Name
        $value = $prop.Value
        if ($value -is [pscustomobject] -and ($value.PSObject.Properties.Name -contains "symbol")) {
            $results += [pscustomobject]@{
                index  = $IndexName
                key    = $key
                symbol = [string]$value.symbol
                path   = "$CollectionProperty/$key"
            }
        }
    }
    return $results
}

function Normalize-Count {
    param([int]$Value, [int]$Cap)
    if ($Cap -le 0) { return 1.0 }
    return [Math]::Min(1.0, ($Value / [double]$Cap))
}

function Clamp01 {
    param([double]$Value)
    return [Math]::Min(1.0, [Math]::Max(0.0, $Value))
}

function Get-OptionalString {
    param(
        [Parameter(Mandatory = $true)][object]$Node,
        [Parameter(Mandatory = $true)][string]$Name
    )
    if ($null -eq $Node) { return "" }
    if ($Node -is [pscustomobject] -and ($Node.PSObject.Properties.Name -contains $Name)) {
        return [string]$Node.$Name
    }
    return ""
}

function Get-SheafSignature {
    param([Parameter(Mandatory = $true)][object]$Sheaf)

    $entityCount = @($Sheaf.entities).Count
    $stateCount = @($Sheaf.states).Count
    $eventCount = @($Sheaf.events).Count
    $relationCount = @($Sheaf.relations).Count

    $edgeByType = @{}
    foreach ($rel in @($Sheaf.relations)) {
        if ($null -eq $rel) { continue }
        $type = [string]$rel.relation
        if (-not $edgeByType.ContainsKey($type)) { $edgeByType[$type] = 0 }
        $edgeByType[$type]++
    }

    return [pscustomobject]@{
        node_counts = [pscustomobject]@{
            entities  = $entityCount
            states    = $stateCount
            events    = $eventCount
            relations = $relationCount
        }
        edge_counts_by_relation = $edgeByType
    }
}

function Get-SignatureDelta {
    param([Parameter(Mandatory = $true)][object]$Left, [Parameter(Mandatory = $true)][object]$Right)

    $total = 0.0
    $denom = 0.0

    foreach ($prop in @("entities", "states", "events", "relations")) {
        $l = [double]$Left.node_counts.$prop
        $r = [double]$Right.node_counts.$prop
        $total += [Math]::Abs($l - $r)
        $denom += [Math]::Max($l, $r)
    }

    $allKeys = @{}
    foreach ($k in $Left.edge_counts_by_relation.Keys) { $allKeys[$k] = $true }
    foreach ($k in $Right.edge_counts_by_relation.Keys) { $allKeys[$k] = $true }

    foreach ($k in $allKeys.Keys) {
        $l = if ($Left.edge_counts_by_relation.ContainsKey($k)) { [double]$Left.edge_counts_by_relation[$k] } else { 0.0 }
        $r = if ($Right.edge_counts_by_relation.ContainsKey($k)) { [double]$Right.edge_counts_by_relation[$k] } else { 0.0 }
        $total += [Math]::Abs($l - $r)
        $denom += [Math]::Max($l, $r)
    }

    if ($denom -le 0.0) { return 0.0 }
    return [Math]::Round(($total / $denom), 6)
}

$requiredFiles = @{
    SymbolicIndex = (Join-Path $ModulePath "SymbolicIndex.json")
    RootIndex = (Join-Path $ModulePath "RootIndex.json")
    Constructor = (Join-Path $ModulePath "SymbolicKeyConstructor_ReservedExpanded.json")
    OperatorIndex = (Join-Path $ModulePath "OperatorIndex.json")
    RelationIndex = (Join-Path $ModulePath "RelationIndex.json")
    GrammarSheafIndex = (Join-Path $ModulePath "GrammarSheafIndex.json")
    ReservedIndex = (Join-Path $ModulePath "ReservedIndex.json")
    SheafSchema = (Join-Path $ModulePath "gel.sheaf.v0.2.0.json")
}

$failures = New-Object System.Collections.Generic.List[string]
foreach ($entry in $requiredFiles.GetEnumerator()) {
    if (-not (Test-Path -Path $entry.Value -PathType Leaf)) {
        $failures.Add("Missing required file: $($entry.Value)")
    }
}
if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Error $_ }
    exit 1
}

$symbolicIndex = Read-JsonFile -Path $requiredFiles.SymbolicIndex
$rootIndex = Read-JsonFile -Path $requiredFiles.RootIndex
$constructor = Read-JsonFile -Path $requiredFiles.Constructor
$operatorIndex = Read-JsonFile -Path $requiredFiles.OperatorIndex
$relationIndex = Read-JsonFile -Path $requiredFiles.RelationIndex
$grammarSheafIndex = Read-JsonFile -Path $requiredFiles.GrammarSheafIndex
$reservedIndex = Read-JsonFile -Path $requiredFiles.ReservedIndex
$sheafSchema = Read-JsonFile -Path $requiredFiles.SheafSchema

$prefixEntries = Collect-SymbolEntriesFromNestedIndex -Node $symbolicIndex.prefixes -IndexName "Prefix" -PathPrefix "prefixes"
$suffixEntries = Collect-SymbolEntriesFromNestedIndex -Node $symbolicIndex.suffixes -IndexName "Suffix" -PathPrefix "suffixes"
$rootEntries = Collect-SymbolEntriesFromNestedIndex -Node $rootIndex -IndexName "Root" -PathPrefix "roots"
$operatorEntries = Collect-Entries -IndexName "Operator" -Container $operatorIndex -CollectionProperty "operators"
$relationEntries = Collect-Entries -IndexName "Relation" -Container $relationIndex -CollectionProperty "relations"
$grammarEntries = Collect-Entries -IndexName "GrammarSheaf" -Container $grammarSheafIndex -CollectionProperty "markers"

$allEntries = @()
$allEntries += $prefixEntries
$allEntries += $rootEntries
$allEntries += $suffixEntries
$allEntries += $operatorEntries
$allEntries += $relationEntries
$allEntries += $grammarEntries

$symbolCountByIndex = [ordered]@{
    Prefix = @($prefixEntries).Count
    Root = @($rootEntries).Count
    Suffix = @($suffixEntries).Count
    Operator = @($operatorEntries).Count
    Relation = @($relationEntries).Count
    GrammarSheaf = @($grammarEntries).Count
}

$duplicateGroups = @($allEntries | Group-Object -Property symbol | Where-Object { -not [string]::IsNullOrWhiteSpace($_.Name) -and $_.Count -gt 1 })
$duplicateSymbolCount = $duplicateGroups.Count
$effectiveDuplicateViolationCount = [Math]::Max(0, ($duplicateSymbolCount - $LegacyDuplicateSymbolBaseline))

$reservedTokenSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($token in @($reservedIndex.reserved_tokens)) { [void]$reservedTokenSet.Add([string]$token) }
foreach ($token in @($reservedIndex.reserved_meta)) { [void]$reservedTokenSet.Add([string]$token) }
foreach ($token in @($reservedIndex.reserved_control)) { [void]$reservedTokenSet.Add([string]$token) }

$reservedSymbolViolations = @($allEntries | Where-Object { $reservedTokenSet.Contains([string]$_.symbol) })
$reservedViolationCount = $reservedSymbolViolations.Count
$effectiveReservedViolationCount = [Math]::Max(0, ($reservedViolationCount - $LegacyReservedViolationBaseline))

$emptyOrNullSymbolCount = @($allEntries | Where-Object { [string]::IsNullOrWhiteSpace([string]$_.symbol) }).Count

$semanticEntries = @($operatorEntries + $relationEntries + $grammarEntries)
$reservedKeyViolations = @($semanticEntries | Where-Object {
    $key = [string]$_.key
    $reservedTokenSet.Contains($key)
})
$reservedKeyViolationCount = $reservedKeyViolations.Count

$sheafValidityErrors = New-Object System.Collections.Generic.List[string]
if ([string]$sheafSchema.schema -ne "gel.sheaf.v0.2.0") {
    $sheafValidityErrors.Add("Invalid sheaf schema identifier.")
}
if (-not ($sheafSchema.PSObject.Properties.Name -contains "examples")) {
    $sheafValidityErrors.Add("Sheaf schema missing examples section.")
}
if (-not ($sheafSchema.examples.PSObject.Properties.Name -contains "encode")) {
    $sheafValidityErrors.Add("Sheaf schema examples missing encode payload.")
}

$encodeExampleSheaf = $sheafSchema.examples.encode.sheaf
if ($null -eq $encodeExampleSheaf) {
    $sheafValidityErrors.Add("Sheaf schema encode example missing sheaf payload.")
}

# Fractional constraint: only allow x/y/z values under sheaf.metadata.*
$fractionalViolations = New-Object System.Collections.Generic.List[string]
function Scan-FractionalPaths {
    param([object]$Node, [string]$Path)
    $results = New-Object System.Collections.Generic.List[string]
    if ($null -eq $Node) { return $results }

    if ($Node -is [pscustomobject]) {
        foreach ($prop in $Node.PSObject.Properties) {
            $childPath = if ([string]::IsNullOrWhiteSpace($Path)) { $prop.Name } else { "$Path.$($prop.Name)" }
            if (Is-FractionalTriplet -Value $prop.Value) {
                $results.Add($childPath)
            }
            elseif ($prop.Value -is [pscustomobject] -or $prop.Value -is [System.Collections.IDictionary] -or $prop.Value -is [System.Collections.IEnumerable] -and -not ($prop.Value -is [string])) {
                $sub = Scan-FractionalPaths -Node $prop.Value -Path $childPath
                foreach ($s in $sub) { $results.Add($s) }
            }
        }
    }
    elseif ($Node -is [System.Collections.IEnumerable] -and -not ($Node -is [string])) {
        $i = 0
        foreach ($item in $Node) {
            $childPath = "$Path[$i]"
            if (Is-FractionalTriplet -Value $item) {
                $results.Add($childPath)
            }
            elseif ($item -is [pscustomobject] -or $item -is [System.Collections.IDictionary] -or $item -is [System.Collections.IEnumerable] -and -not ($item -is [string])) {
                $sub = Scan-FractionalPaths -Node $item -Path $childPath
                foreach ($s in $sub) { $results.Add($s) }
            }
            $i++
        }
    }

    return $results
}

$fractionalPaths = @()
if ($null -ne $encodeExampleSheaf) {
    $fractionalPaths = @(Scan-FractionalPaths -Node $encodeExampleSheaf -Path "sheaf")
    foreach ($path in $fractionalPaths) {
        if (-not $path.StartsWith("sheaf.metadata.")) {
            $fractionalViolations.Add("Fractional value not in sheaf.metadata: $path")
        }
    }
}

# Sheaf reference checks on canonical encode example
$danglingReferenceCount = 0
$roleBindingMissingCount = 0
$relationEndpointMissingCount = 0

if ($null -ne $encodeExampleSheaf) {
    $entityIds = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    $eventIds = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    $stateIds = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)

    foreach ($entity in @($encodeExampleSheaf.entities)) {
        if ($null -ne $entity -and -not [string]::IsNullOrWhiteSpace([string]$entity.id)) {
            [void]$entityIds.Add([string]$entity.id)
        }
    }
    foreach ($event in @($encodeExampleSheaf.events)) {
        if ($null -ne $event -and -not [string]::IsNullOrWhiteSpace([string]$event.id)) {
            [void]$eventIds.Add([string]$event.id)
        }
    }
    foreach ($state in @($encodeExampleSheaf.states)) {
        if ($null -ne $state -and ($state.PSObject.Properties.Name -contains "id") -and -not [string]::IsNullOrWhiteSpace([string]$state.id)) {
            [void]$stateIds.Add([string]$state.id)
        }
    }

    foreach ($state in @($encodeExampleSheaf.states)) {
        if ($null -eq $state) { continue }
        $target = [string]$state.target
        if ([string]::IsNullOrWhiteSpace($target) -or -not $entityIds.Contains($target)) {
            $danglingReferenceCount++
        }
    }

    foreach ($event in @($encodeExampleSheaf.events)) {
        if ($null -eq $event) { continue }
        $op = Get-OptionalString -Node $event -Name "op"
        $agent = Get-OptionalString -Node $event -Name "agent"
        if ($op -eq "ACT" -and ([string]::IsNullOrWhiteSpace($agent) -or -not $entityIds.Contains($agent))) {
            $roleBindingMissingCount++
        }

        $target = Get-OptionalString -Node $event -Name "target"
        if (-not [string]::IsNullOrWhiteSpace($target) -and -not $entityIds.Contains($target)) {
            $danglingReferenceCount++
        }

        $instrument = Get-OptionalString -Node $event -Name "instrument"
        if (-not [string]::IsNullOrWhiteSpace($instrument) -and -not $entityIds.Contains($instrument)) {
            $danglingReferenceCount++
        }
    }

    $nodeIds = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($id in $entityIds) { [void]$nodeIds.Add($id) }
    foreach ($id in $eventIds) { [void]$nodeIds.Add($id) }
    foreach ($id in $stateIds) { [void]$nodeIds.Add($id) }

    foreach ($relation in @($encodeExampleSheaf.relations)) {
        if ($null -eq $relation) { continue }
        $source = [string]$relation.source
        $target = [string]$relation.target
        $okSource = -not [string]::IsNullOrWhiteSpace($source) -and $nodeIds.Contains($source)
        $okTarget = -not [string]::IsNullOrWhiteSpace($target) -and $nodeIds.Contains($target)
        if (-not $okSource -or -not $okTarget) {
            $relationEndpointMissingCount++
        }
    }
}

if ($fractionalViolations.Count -gt 0) {
    foreach ($v in $fractionalViolations) { $sheafValidityErrors.Add($v) }
}

$sheafSchemaValidityRate = if ($sheafValidityErrors.Count -eq 0) { 1.0 } else { 0.0 }

$roundTripAvailable = $false
$roundTripShapeDelta = 0.0
if ($null -ne $encodeExampleSheaf -and ($sheafSchema.examples.decode.PSObject.Properties.Name -contains "sheaf")) {
    $roundTripAvailable = $true
    $sigA = Get-SheafSignature -Sheaf $encodeExampleSheaf
    $sigB = Get-SheafSignature -Sheaf $sheafSchema.examples.decode.sheaf
    $roundTripShapeDelta = Get-SignatureDelta -Left $sigA -Right $sigB
}

$decodeFallbackCount = 0

$pRes = Normalize-Count -Value $reservedViolationCount -Cap 1
$pDup = Normalize-Count -Value $duplicateSymbolCount -Cap 1
$pNull = Normalize-Count -Value $emptyOrNullSymbolCount -Cap 5
$pSchema = Clamp01 -Value (1.0 - $sheafSchemaValidityRate)
$pDang = Normalize-Count -Value $danglingReferenceCount -Cap 5
$pRole = Normalize-Count -Value $roleBindingMissingCount -Cap 5
$pRel = Normalize-Count -Value $relationEndpointMissingCount -Cap 5
$pRt = Clamp01 -Value $roundTripShapeDelta

$penalty =
    (0.30 * $pRes) +
    (0.25 * $pDup) +
    (0.20 * $pSchema) +
    (0.15 * $pRt) +
    (0.10 * ((0.34 * $pDang) + (0.33 * $pRole) + (0.33 * $pRel))) +
    (0.05 * $pNull)

$penalty = Clamp01 -Value $penalty
$flowScore = [int][Math]::Round(100.0 * (1.0 - $penalty), 0)

$flowMetrics = [ordered]@{
    schema = "gel.flow_metrics.v0.1.0"
    flow_score = $flowScore
    penalty = [Math]::Round($penalty, 6)
    metrics = [ordered]@{
        symbol_count_by_index = $symbolCountByIndex
        duplicate_symbol_count = $duplicateSymbolCount
        effective_duplicate_symbol_violation_count = $effectiveDuplicateViolationCount
        legacy_duplicate_symbol_baseline = $LegacyDuplicateSymbolBaseline
        reserved_violation_count = $reservedViolationCount
        effective_reserved_violation_count = $effectiveReservedViolationCount
        legacy_reserved_violation_baseline = $LegacyReservedViolationBaseline
        reserved_key_violation_count = $reservedKeyViolationCount
        empty_or_null_symbol_count = $emptyOrNullSymbolCount
        sheaf_schema_validity_rate = $sheafSchemaValidityRate
        dangling_reference_count = $danglingReferenceCount
        role_binding_missing_count = $roleBindingMissingCount
        relation_endpoint_missing_count = $relationEndpointMissingCount
        round_trip_shape_delta = [Math]::Round($roundTripShapeDelta, 6)
        round_trip_available = $roundTripAvailable
        decode_fallback_count = $decodeFallbackCount
        collision_count = $duplicateSymbolCount
        fractional_path_violations = @($fractionalViolations)
    }
}

$cognitionTelemetry = [ordered]@{
    schema = "oan.cognition_telemetry.v0.1.0"
    flow_score = $flowScore
    symbolic_integrity = [Math]::Round(($flowScore / 100.0), 4)
    semantic_stability = $null
    drift_resistance = $null
    reasoning_efficiency = $null
    metrics = [ordered]@{
        layer0 = $flowMetrics.metrics
        layer1 = $null
        layer2 = $null
        layer3 = $null
    }
}

if (-not (Test-Path -Path $OutDir -PathType Container)) {
    New-Item -ItemType Directory -Path $OutDir | Out-Null
}

$flowPath = Join-Path $OutDir "flow_metrics.json"
$cognitionPath = Join-Path $OutDir "cognition_telemetry.json"

$flowMetrics | ConvertTo-Json -Depth 12 | Set-Content -Encoding utf8 $flowPath
$cognitionTelemetry | ConvertTo-Json -Depth 12 | Set-Content -Encoding utf8 $cognitionPath

Write-Host "Wrote flow metrics: $flowPath"
Write-Host "Wrote cognition telemetry: $cognitionPath"

if ($effectiveDuplicateViolationCount -gt 0) {
    $failures.Add("Duplicate symbols exceed baseline. total=$duplicateSymbolCount baseline=$LegacyDuplicateSymbolBaseline effective=$effectiveDuplicateViolationCount")
}
if ($effectiveReservedViolationCount -gt 0) {
    $failures.Add("Reserved symbol assignment violations exceed baseline. total=$reservedViolationCount baseline=$LegacyReservedViolationBaseline effective=$effectiveReservedViolationCount")
}
if ($sheafSchemaValidityRate -lt 1.0) {
    $failures.Add("Canonical sheaf schema example failed validation.")
}
if ($emptyOrNullSymbolCount -gt 0) {
    $failures.Add("Empty/null symbols found: $emptyOrNullSymbolCount")
}
if ($StrictReservedKeyCheck -and $reservedKeyViolationCount -gt 0) {
    $failures.Add("Reserved key violations found in semantic indices: $reservedKeyViolationCount")
}

if ($failures.Count -gt 0) {
    foreach ($f in $failures) { Write-Error $f }
    exit 1
}

Write-Host "SLE v0.2 validation succeeded. FlowScore=$flowScore"
exit 0
