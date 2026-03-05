[CmdletBinding()]
param(
    [string]$ModulePath,
    [string]$OutDir,
    [string]$CognitionTelemetryPath,
    [double]$CoverageWarningThreshold = 0.6
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
if ([string]::IsNullOrWhiteSpace($CognitionTelemetryPath)) {
    $CognitionTelemetryPath = Join-Path $OutDir "cognition_telemetry.json"
}

function Read-JsonFile {
    param([Parameter(Mandatory = $true)][string]$Path)
    return Get-Content -Raw -Encoding utf8 $Path | ConvertFrom-Json
}

function Write-JsonFile {
    param([Parameter(Mandatory = $true)][string]$Path, [Parameter(Mandatory = $true)][object]$Object)
    $Object | ConvertTo-Json -Depth 20 | Set-Content -Encoding utf8 $Path
}

function Clamp {
    param([double]$Value, [double]$Min, [double]$Max)
    return [Math]::Min($Max, [Math]::Max($Min, $Value))
}

function New-BiasBlock {
    param(
        [string]$FromType,
        [string]$FromRef,
        [string]$ToType,
        [string]$ToRef,
        [double]$Bias,
        [string]$Reason
    )
    return [pscustomobject]@{
        from = [pscustomobject]@{ type = $FromType; ref = $FromRef }
        to = [pscustomobject]@{ type = $ToType; ref = $ToRef }
        bias = [Math]::Round((Clamp -Value $Bias -Min -3.0 -Max 3.0), 4)
        reason = $Reason
    }
}

function Add-Bias {
    param(
        [System.Collections.Generic.List[object]]$List,
        [string]$FromType,
        [string]$FromRef,
        [string]$ToType,
        [string]$ToRef,
        [double]$Bias,
        [string]$Reason
    )
    $List.Add((New-BiasBlock -FromType $FromType -FromRef $FromRef -ToType $ToType -ToRef $ToRef -Bias $Bias -Reason $Reason))
}

function Normalize-Word {
    param([string]$Text)
    if ([string]::IsNullOrWhiteSpace($Text)) { return "" }
    $t = $Text.ToLowerInvariant()
    $t = $t -replace '^[^\p{L}\p{Nd}]+', ''
    $t = $t -replace '[^\p{L}\p{Nd}]+$', ''
    return $t
}

function Get-NodeDegree {
    param([hashtable]$Adj, [string]$NodeId)
    if ($Adj.ContainsKey($NodeId)) { return [int]$Adj[$NodeId].Count }
    return 0
}

function Get-NamePart {
    param([string]$Id)
    if ([string]::IsNullOrWhiteSpace($Id)) { return "" }
    $parts = $Id.Split('.')
    return $parts[$parts.Length - 1]
}

function Get-MappingName {
    param([string]$Qualified)
    if ([string]::IsNullOrWhiteSpace($Qualified)) { return "" }
    $parts = $Qualified.Split('.')
    return $parts[$parts.Length - 1]
}

function Ensure-Adj {
    param([hashtable]$Adj, [string]$Node)
    if (-not $Adj.ContainsKey($Node)) {
        $Adj[$Node] = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    }
}

$required = @{
    Sheaf = (Join-Path $ModulePath "gel.sheaf.v0.2.0.json")
    Reserved = (Join-Path $ModulePath "ReservedIndex.json")
    Relation = (Join-Path $ModulePath "RelationIndex.json")
}
$errors = New-Object System.Collections.Generic.List[string]
foreach ($k in $required.Keys) {
    if (-not (Test-Path -Path $required[$k] -PathType Leaf)) {
        $errors.Add("Missing required file: $($required[$k])")
    }
}
if ($errors.Count -gt 0) {
    foreach ($e in $errors) { Write-Error $e }
    exit 1
}

if (-not (Test-Path -Path $OutDir -PathType Container)) {
    New-Item -ItemType Directory -Path $OutDir | Out-Null
}

$sheafRoot = Read-JsonFile -Path $required.Sheaf
$reservedIndex = Read-JsonFile -Path $required.Reserved
$relationIndex = Read-JsonFile -Path $required.Relation
$encode = $sheafRoot.examples.encode
if ($null -eq $encode -or $null -eq $encode.sheaf) {
    Write-Error "Canonical sheaf example not found for SCAR generation."
    exit 1
}
$sheaf = $encode.sheaf
$surface = [string]$encode.surface

$reservedTokens = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($t in @($reservedIndex.reserved_tokens)) { [void]$reservedTokens.Add([string]$t) }
foreach ($t in @($reservedIndex.reserved_meta)) { [void]$reservedTokens.Add([string]$t) }
foreach ($t in @($reservedIndex.reserved_control)) { [void]$reservedTokens.Add([string]$t) }

$nodes = [ordered]@{}
$entityIds = [System.Collections.Generic.List[string]]::new()
$eventIds = [System.Collections.Generic.List[string]]::new()
$stateIds = [System.Collections.Generic.List[string]]::new()

foreach ($e in @($sheaf.entities)) {
    if ($null -eq $e -or [string]::IsNullOrWhiteSpace([string]$e.id)) { continue }
    $id = [string]$e.id
    $nodes[$id] = [pscustomobject]@{ id = $id; type = "entity"; lexemes = @((Normalize-Word ([string]$e.root))); scope = "s0" }
    $entityIds.Add($id)
}

$evCounter = 0
foreach ($ev in @($sheaf.events)) {
    if ($null -eq $ev) { continue }
    $id = if ($ev.PSObject.Properties.Name -contains "id" -and -not [string]::IsNullOrWhiteSpace([string]$ev.id)) { [string]$ev.id } else { $evCounter++; "ev$evCounter" }
    $nodes[$id] = [pscustomobject]@{ id = $id; type = "event"; lexemes = @((Normalize-Word ([string]$ev.verb))); scope = "s0" }
    $eventIds.Add($id)
}

$stCounter = 0
foreach ($st in @($sheaf.states)) {
    if ($null -eq $st) { continue }
    $stCounter++
    $id = if ($st.PSObject.Properties.Name -contains "id" -and -not [string]::IsNullOrWhiteSpace([string]$st.id)) { [string]$st.id } else { "st$stCounter" }
    $nodes[$id] = [pscustomobject]@{ id = $id; type = "state"; lexemes = @((Normalize-Word ([string]$st.value))); scope = "s0" }
    $stateIds.Add($id)
}

$adj = @{}
foreach ($nodeId in $nodes.Keys) { Ensure-Adj -Adj $adj -Node $nodeId }

$stateById = @{}
for ($i = 0; $i -lt @($sheaf.states).Count; $i++) {
    $st = @($sheaf.states)[$i]
    $id = @($stateIds)[$i]
    $stateById[$id] = $st
}

foreach ($ev in @($sheaf.events)) {
    if ($null -eq $ev) { continue }
    $evId = if ($ev.PSObject.Properties.Name -contains "id" -and -not [string]::IsNullOrWhiteSpace([string]$ev.id)) { [string]$ev.id } else { "" }
    if ([string]::IsNullOrWhiteSpace($evId)) {
        foreach ($cand in $eventIds) {
            if ($nodes[$cand].lexemes[0] -eq (Normalize-Word ([string]$ev.verb))) { $evId = $cand; break }
        }
    }
    if ([string]::IsNullOrWhiteSpace($evId)) { continue }

    $agent = if ($ev.PSObject.Properties.Name -contains "agent") { [string]$ev.agent } else { "" }
    $target = if ($ev.PSObject.Properties.Name -contains "target") { [string]$ev.target } else { "" }

    if (-not [string]::IsNullOrWhiteSpace($agent) -and $nodes.Contains($agent)) {
        Ensure-Adj -Adj $adj -Node $agent
        Ensure-Adj -Adj $adj -Node $evId
        [void]$adj[$agent].Add($evId)
        [void]$adj[$evId].Add($agent)
    }
    if (-not [string]::IsNullOrWhiteSpace($target) -and $nodes.Contains($target)) {
        Ensure-Adj -Adj $adj -Node $target
        Ensure-Adj -Adj $adj -Node $evId
        [void]$adj[$target].Add($evId)
        [void]$adj[$evId].Add($target)
    }
}

foreach ($stId in $stateIds) {
    $st = $stateById[$stId]
    if ($null -eq $st) { continue }
    $target = if ($st.PSObject.Properties.Name -contains "target") { [string]$st.target } else { "" }
    if (-not [string]::IsNullOrWhiteSpace($target) -and $nodes.Contains($target)) {
        Ensure-Adj -Adj $adj -Node $target
        Ensure-Adj -Adj $adj -Node $stId
        [void]$adj[$target].Add($stId)
        [void]$adj[$stId].Add($target)
    }
}

foreach ($rel in @($sheaf.relations)) {
    if ($null -eq $rel) { continue }
    $src = [string]$rel.source
    $dst = [string]$rel.target
    if ($nodes.Contains($src) -and $nodes.Contains($dst)) {
        Ensure-Adj -Adj $adj -Node $src
        Ensure-Adj -Adj $adj -Node $dst
        [void]$adj[$src].Add($dst)
        [void]$adj[$dst].Add($src)
    }
}

$rawTokens = [regex]::Matches($surface, "[A-Za-z0-9']+|[^\sA-Za-z0-9]") | ForEach-Object { $_.Value }
$tokens = New-Object System.Collections.Generic.List[object]
$tokenToNode = @{}
$warnings = New-Object System.Collections.Generic.List[string]

for ($i = 0; $i -lt $rawTokens.Count; $i++) {
    $text = [string]$rawTokens[$i]
    $norm = Normalize-Word $text
    $candidates = New-Object System.Collections.Generic.List[string]

    if (-not [string]::IsNullOrWhiteSpace($norm)) {
        foreach ($nodeId in $nodes.Keys) {
            $lex = @($nodes[$nodeId].lexemes)
            if ($lex -contains $norm) { $candidates.Add($nodeId) }
        }
    }

    $nodeRef = $null
    if ($candidates.Count -eq 1) {
        $nodeRef = $candidates[0]
    }
    elseif ($candidates.Count -gt 1) {
        $best = $candidates[0]
        $bestDegree = Get-NodeDegree -Adj $adj -NodeId $best
        foreach ($c in $candidates) {
            $d = Get-NodeDegree -Adj $adj -NodeId $c
            if ($d -gt $bestDegree) {
                $best = $c
                $bestDegree = $d
            }
        }
        $nodeRef = $best
    }

    $tokenObj = [pscustomobject]@{
        pos = $i
        text = $text
        node_ref = $nodeRef
        scope = "s0"
    }
    $tokens.Add($tokenObj)
    $tokenToNode[$i] = $nodeRef
}

$mappedCount = @($tokens | Where-Object { $null -ne $_.node_ref }).Count
$coverage = if ($tokens.Count -gt 0) { [Math]::Round(($mappedCount / [double]$tokens.Count), 6) } else { 1.0 }
if ($coverage -lt $CoverageWarningThreshold) {
    $warnings.Add("Low token-node mapping coverage: $coverage")
}

$anchors = New-Object System.Collections.Generic.List[object]
foreach ($nodeId in $nodes.Keys) {
    $anchors.Add([pscustomobject]@{
        id = "ANCHOR:$nodeId"
        node_ref = $nodeId
    })
}

if ($anchors.Count -eq 0) {
    Write-Error "SCAR generation failed: no anchors created from sheaf nodes."
    exit 1
}

$biasBlocks = New-Object System.Collections.Generic.List[object]

# Rule 1: local self + neighbor
for ($i = 0; $i -lt $tokens.Count; $i++) {
    Add-Bias -List $biasBlocks -FromType "token" -FromRef "$i" -ToType "token" -ToRef "$i" -Bias 0.2 -Reason "SELF_BIAS"
    for ($j = [Math]::Max(0, $i - 2); $j -le [Math]::Min($tokens.Count - 1, $i + 2); $j++) {
        if ($j -eq $i) { continue }
        Add-Bias -List $biasBlocks -FromType "token" -FromRef "$i" -ToType "token" -ToRef "$j" -Bias 0.1 -Reason "NEIGHBOR_BIAS"
    }
}

# Rule 2: token <-> anchor
for ($i = 0; $i -lt $tokens.Count; $i++) {
    $nodeRef = $tokenToNode[$i]
    if ($null -eq $nodeRef) { continue }
    Add-Bias -List $biasBlocks -FromType "token" -FromRef "$i" -ToType "anchor" -ToRef "ANCHOR:$nodeRef" -Bias 1.0 -Reason "TOKEN_TO_ANCHOR"
    Add-Bias -List $biasBlocks -FromType "anchor" -FromRef "ANCHOR:$nodeRef" -ToType "token" -ToRef "$i" -Bias 0.5 -Reason "ANCHOR_TO_TOKEN"
}

# Rule 3: event roles
$missingCritical = 0
foreach ($ev in @($sheaf.events)) {
    if ($null -eq $ev) { continue }
    $evId = if ($ev.PSObject.Properties.Name -contains "id") { [string]$ev.id } else { "" }
    if ([string]::IsNullOrWhiteSpace($evId)) {
        foreach ($cand in $eventIds) {
            if ($nodes[$cand].lexemes[0] -eq (Normalize-Word ([string]$ev.verb))) { $evId = $cand; break }
        }
    }
    if ([string]::IsNullOrWhiteSpace($evId)) { $missingCritical++; continue }

    $agent = if ($ev.PSObject.Properties.Name -contains "agent") { [string]$ev.agent } else { "" }
    $target = if ($ev.PSObject.Properties.Name -contains "target") { [string]$ev.target } else { "" }

    if (-not [string]::IsNullOrWhiteSpace($agent) -and $nodes.Contains($agent)) {
        Add-Bias -List $biasBlocks -FromType "anchor" -FromRef "ANCHOR:$agent" -ToType "anchor" -ToRef "ANCHOR:$evId" -Bias 1.0 -Reason "AGENT_OF"
    }
    else { $missingCritical++ }

    if (-not [string]::IsNullOrWhiteSpace($target) -and $nodes.Contains($target)) {
        Add-Bias -List $biasBlocks -FromType "anchor" -FromRef "ANCHOR:$evId" -ToType "anchor" -ToRef "ANCHOR:$target" -Bias 0.8 -Reason "TARGET_OF"
        if (-not [string]::IsNullOrWhiteSpace($agent) -and $nodes.Contains($agent)) {
            Add-Bias -List $biasBlocks -FromType "anchor" -FromRef "ANCHOR:$agent" -ToType "anchor" -ToRef "ANCHOR:$target" -Bias 0.2 -Reason "CO_PARTICIPATION"
            Add-Bias -List $biasBlocks -FromType "anchor" -FromRef "ANCHOR:$target" -ToType "anchor" -ToRef "ANCHOR:$agent" -Bias 0.2 -Reason "CO_PARTICIPATION"
        }
    }
}

# Rule 4: state attachment
for ($i = 0; $i -lt $stateIds.Count; $i++) {
    $stId = $stateIds[$i]
    $st = @($sheaf.states)[$i]
    $target = if ($st.PSObject.Properties.Name -contains "target") { [string]$st.target } else { "" }
    if ([string]::IsNullOrWhiteSpace($target) -or -not $nodes.Contains($target)) { continue }

    Add-Bias -List $biasBlocks -FromType "anchor" -FromRef "ANCHOR:$target" -ToType "anchor" -ToRef "ANCHOR:$stId" -Bias 0.8 -Reason "STATE_ATTACHMENT"
    Add-Bias -List $biasBlocks -FromType "anchor" -FromRef "ANCHOR:$stId" -ToType "anchor" -ToRef "ANCHOR:$target" -Bias 0.8 -Reason "STATE_ATTACHMENT"

    $entityTokenPositions = @($tokens | Where-Object { $_.node_ref -eq $target } | ForEach-Object { [int]$_.pos })
    $stateTokenPositions = @($tokens | Where-Object { $_.node_ref -eq $stId } | ForEach-Object { [int]$_.pos })
    foreach ($a in $entityTokenPositions) {
        foreach ($b in $stateTokenPositions) {
            Add-Bias -List $biasBlocks -FromType "token" -FromRef "$a" -ToType "token" -ToRef "$b" -Bias 0.4 -Reason "ENTITY_STATE_TOKEN_COUPLING"
        }
    }
}

$causalTypes = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
$temporalTypes = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
$referenceTypes = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($k in @("CAUSES", "ENABLES", "PREVENTS")) { [void]$causalTypes.Add($k) }
foreach ($k in @("BEFORE", "AFTER", "DURING", "OVERLAPS")) { [void]$temporalTypes.Add($k) }
foreach ($k in @("REFERS_TO", "ALIAS_OF", "DERIVES_FROM", "MAPS_TO")) { [void]$referenceTypes.Add($k) }

$typedRelationCount = 0
$causalRelationCount = 0
$temporalRelationCount = 0
$referenceRelationCount = 0

# Rule 5: typed relations
foreach ($rel in @($sheaf.relations)) {
    if ($null -eq $rel) { continue }
    $src = [string]$rel.source
    $dst = [string]$rel.target
    $rtype = [string]$rel.relation
    if ([string]::IsNullOrWhiteSpace($src) -or [string]::IsNullOrWhiteSpace($dst)) { continue }
    if (-not $nodes.Contains($src) -or -not $nodes.Contains($dst)) {
        $warnings.Add("Relation references unknown endpoint: $src -> $dst ($rtype)")
        continue
    }

    $bias = 0.6
    if ($causalTypes.Contains($rtype)) { $bias = 0.9; $causalRelationCount++ }
    elseif ($temporalTypes.Contains($rtype)) { $bias = 0.7; $temporalRelationCount++ }
    elseif ($referenceTypes.Contains($rtype)) { $bias = 0.7; $referenceRelationCount++ }

    $typedRelationCount++
    Add-Bias -List $biasBlocks -FromType "anchor" -FromRef "ANCHOR:$src" -ToType "anchor" -ToRef "ANCHOR:$dst" -Bias $bias -Reason "REL_$rtype"
}

# Rule 6: graph-distance decay (up to 2 hops)
foreach ($src in $nodes.Keys) {
    $queue = New-Object System.Collections.Generic.Queue[string]
    $distance = @{}
    $distance[$src] = 0
    $queue.Enqueue($src)

    while ($queue.Count -gt 0) {
        $cur = $queue.Dequeue()
        $d = [int]$distance[$cur]
        if ($d -ge 2) { continue }
        foreach ($n in $adj[$cur]) {
            if (-not $distance.ContainsKey($n)) {
                $distance[$n] = $d + 1
                $queue.Enqueue($n)
            }
        }
    }

    foreach ($dst in $distance.Keys) {
        if ($dst -eq $src) { continue }
        $hop = [int]$distance[$dst]
        if ($hop -eq 1) {
            Add-Bias -List $biasBlocks -FromType "anchor" -FromRef "ANCHOR:$src" -ToType "anchor" -ToRef "ANCHOR:$dst" -Bias 0.3 -Reason "GRAPH_HOP1"
        }
        elseif ($hop -eq 2) {
            Add-Bias -List $biasBlocks -FromType "anchor" -FromRef "ANCHOR:$src" -ToType "anchor" -ToRef "ANCHOR:$dst" -Bias 0.15 -Reason "GRAPH_HOP2"
        }
    }
}

# Glue maps and Rule 7 scope behavior
$domainDir = Join-Path $ModulePath "DomainSheaves"
$glueMaps = @()
$domainPackages = @{}
$glueRelationsUsed = 0
$scopeBarrierOverrides = 0
$crossDomainEdgesCreated = 0
$scopeBarrierCount = 0

if (Test-Path -Path $domainDir -PathType Container) {
    foreach ($pkgPath in @(Get-ChildItem -File -Path $domainDir -Filter "package.*.json" | Select-Object -ExpandProperty FullName)) {
        $pkg = Read-JsonFile -Path $pkgPath
        if ([string]$pkg.schema -ne "gel.sheaf_package.v0.1.0") {
            $errors.Add("Invalid sheaf package schema: $pkgPath")
            continue
        }
        $domKey = "{0}.{1}" -f ([string]$pkg.domain), ([string]$pkg.specialization)
        $domainPackages[$domKey] = $pkg
    }

    foreach ($gluePath in @(Get-ChildItem -File -Path $domainDir -Filter "glue.map.*.json" | Select-Object -ExpandProperty FullName)) {
        $glue = Read-JsonFile -Path $gluePath
        if ([string]$glue.schema -ne "gel.glue_map.v0.1.0") {
            $errors.Add("Invalid glue map schema: $gluePath")
            continue
        }

        $doms = @($glue.domains)
        if ($doms.Count -ne 2) {
            $errors.Add("Glue map must declare exactly two domains: $gluePath")
            continue
        }

        foreach ($d in $doms) {
            if (-not $domainPackages.ContainsKey([string]$d)) {
                $errors.Add("Glue map domain not found in package set: $d ($gluePath)")
            }
        }

        # reserved token identifier check
        foreach ($m in @($glue.relation_mappings) + @($glue.operator_mappings)) {
            if ($null -eq $m) { continue }
            $left = [string]$m.left
            $right = [string]$m.right
            foreach ($id in @($left, $right)) {
                $name = Get-MappingName $id
                if ($reservedTokens.Contains($name)) {
                    $errors.Add("Glue mapping identifier collides with reserved token: $id")
                }
            }
        }

        # existence checks in declared domain packages
        $leftPkg = $domainPackages[[string]$doms[0]]
        $rightPkg = $domainPackages[[string]$doms[1]]

        foreach ($m in @($glue.relation_mappings)) {
            if ($null -eq $m) { continue }
            $leftName = Get-MappingName ([string]$m.left)
            $rightName = Get-MappingName ([string]$m.right)
            if (@($leftPkg.relations) -notcontains $leftName) {
                $errors.Add("Glue relation left side missing in package $($doms[0]): $leftName")
            }
            if (@($rightPkg.relations) -notcontains $rightName) {
                $errors.Add("Glue relation right side missing in package $($doms[1]): $rightName")
            }
        }

        foreach ($m in @($glue.operator_mappings)) {
            if ($null -eq $m) { continue }
            $leftName = Get-MappingName ([string]$m.left)
            $rightName = Get-MappingName ([string]$m.right)
            if (@($leftPkg.operators) -notcontains $leftName) {
                $errors.Add("Glue operator left side missing in package $($doms[0]): $leftName")
            }
            if (@($rightPkg.operators) -notcontains $rightName) {
                $errors.Add("Glue operator right side missing in package $($doms[1]): $rightName")
            }
        }

        # specialization cycle detection (directed)
        $edges = @{}
        foreach ($m in @($glue.operator_mappings)) {
            if ($null -eq $m) { continue }
            if ([string]$m.type -eq "specialization") {
                $l = Get-MappingName ([string]$m.left)
                $r = Get-MappingName ([string]$m.right)
                if (-not $edges.ContainsKey($l)) {
                    $edges[$l] = New-Object System.Collections.Generic.List[string]
                }
                $edges[$l].Add($r)
            }
        }

        $temp = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
        $perm = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
        $cycleFound = $false

        function VisitNode {
            param([string]$Node, [hashtable]$Edges, [System.Collections.Generic.HashSet[string]]$Temp, [System.Collections.Generic.HashSet[string]]$Perm, [ref]$CycleFoundRef)
            if ($Perm.Contains($Node)) { return }
            if ($Temp.Contains($Node)) { $CycleFoundRef.Value = $true; return }
            $Temp.Add($Node) | Out-Null
            if ($Edges.ContainsKey($Node)) {
                foreach ($n in $Edges[$Node]) {
                    VisitNode -Node $n -Edges $Edges -Temp $Temp -Perm $Perm -CycleFoundRef $CycleFoundRef
                }
            }
            $Temp.Remove($Node) | Out-Null
            $Perm.Add($Node) | Out-Null
        }

        foreach ($n in $edges.Keys) {
            $r = [ref]$cycleFound
            VisitNode -Node $n -Edges $edges -Temp $temp -Perm $perm -CycleFoundRef $r
        }
        if ($cycleFound) {
            $errors.Add("Specialization cycle detected in glue map: $gluePath")
        }

        $glueMaps += $glue
    }
}

if ($errors.Count -gt 0) {
    foreach ($e in $errors) { Write-Error $e }
    exit 1
}

# Apply barrier rule placeholder (single scope canonical example usually yields zero barriers)
$uniqueScopes = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($t in $tokens) { [void]$uniqueScopes.Add([string]$t.scope) }
$scopeList = @($uniqueScopes)
for ($i = 0; $i -lt $scopeList.Count; $i++) {
    for ($j = $i + 1; $j -lt $scopeList.Count; $j++) {
        $scopeBarrierCount++
        $barrier = -2.0
        $reason = "SCOPE_BARRIER"
        if ($glueMaps.Count -gt 0) {
            $barrier = -0.5
            $reason = "SCOPE_BARRIER_GLUE"
            $scopeBarrierOverrides++
        }
        Add-Bias -List $biasBlocks -FromType "scope" -FromRef $scopeList[$i] -ToType "scope" -ToRef $scopeList[$j] -Bias $barrier -Reason $reason
    }
}

# Apply simple glue-induced cross-domain routing additions
if ($glueMaps.Count -gt 0) {
    foreach ($gm in $glueMaps) {
        foreach ($rm in @($gm.relation_mappings)) {
            if ($null -eq $rm) { continue }
            $type = [string]$rm.type
            if ($type -eq "equivalent") {
                foreach ($evId in $eventIds) {
                    Add-Bias -List $biasBlocks -FromType "anchor" -FromRef "ANCHOR:$evId" -ToType "anchor" -ToRef "ANCHOR:$evId" -Bias 0.2 -Reason "GLUE_EQUIVALENT"
                    $crossDomainEdgesCreated++
                }
                $glueRelationsUsed++
            }
            elseif ($type -eq "causal_overlap") {
                foreach ($evId in $eventIds) {
                    Add-Bias -List $biasBlocks -FromType "anchor" -FromRef "ANCHOR:$evId" -ToType "anchor" -ToRef "ANCHOR:$evId" -Bias 0.1 -Reason "GLUE_CAUSAL_OVERLAP"
                    $crossDomainEdgesCreated++
                }
                $glueRelationsUsed++
            }
        }
    }
}

# Rule 8: fractional metadata modifiers
$roleBoost = 0.0
$globalBoost = 0.0
if ($sheaf.PSObject.Properties.Name -contains "metadata") {
    $md = $sheaf.metadata
    if ($md.PSObject.Properties.Name -contains "confidence" -and [string]$md.confidence -eq "1/1/1") {
        $globalBoost = 0.1
    }
    elseif ($md.PSObject.Properties.Name -contains "confidence" -and [string]$md.confidence -eq "1/0/1") {
        $roleBoost = 0.1
    }
}

if ($globalBoost -ne 0.0 -or $roleBoost -ne 0.0) {
    for ($i = 0; $i -lt $biasBlocks.Count; $i++) {
        $b = $biasBlocks[$i]
        $boost = $globalBoost
        if ([string]$b.reason -match "AGENT_OF|TARGET_OF|CO_PARTICIPATION") {
            $boost += $roleBoost
        }
        $b.bias = [Math]::Round((Clamp -Value ([double]$b.bias + $boost) -Min -3.0 -Max 3.0), 4)
        $biasBlocks[$i] = $b
    }
}

if ($missingCritical -gt 0) {
    $warnings.Add("Missing critical nodes referenced by events/states: $missingCritical")
}

$avgBiasMagnitude = if ($biasBlocks.Count -gt 0) {
    [Math]::Round((($biasBlocks | ForEach-Object { [Math]::Abs([double]$_.bias) } | Measure-Object -Average).Average), 6)
}
else { 0.0 }

$headGate = [ordered]@{
    schema = "sli.scar.head_gate.v0.1.0"
    gates = @(
        [pscustomobject]@{ family = "ROLE"; value = [Math]::Round([Math]::Min(1.0, 0.5 + (0.1 * $eventIds.Count)), 4) },
        [pscustomobject]@{ family = "CAUSAL"; value = [Math]::Round([Math]::Min(1.0, 0.5 + (0.2 * $causalRelationCount)), 4) },
        [pscustomobject]@{ family = "TEMPORAL"; value = [Math]::Round([Math]::Min(1.0, 0.5 + (0.2 * $temporalRelationCount)), 4) },
        [pscustomobject]@{ family = "REF"; value = [Math]::Round([Math]::Min(1.0, 0.5 + (0.2 * $referenceRelationCount)), 4) },
        [pscustomobject]@{ family = "TYPE_MEREOLOGY"; value = [Math]::Round([Math]::Min(1.0, 0.5 + (0.1 * $typedRelationCount)), 4) }
    )
}

$kvAnchor = [ordered]@{
    schema = "sli.scar.kv_anchor.v0.1.0"
    anchors = @($anchors.ToArray() | ForEach-Object {
        [pscustomobject]@{
            id = [string]$_.id
            node_ref = [string]$_.node_ref
            policy = "append_only"
        }
    })
}

$biasSpec = [ordered]@{
    schema = "sli.scar.bias_spec.v0.1.0"
    source = [ordered]@{
        sheaf_id = "canonical_example"
        version = "v0.1"
    }
    tokens = @($tokens.ToArray())
    anchors = @($anchors.ToArray())
    bias_blocks = @($biasBlocks.ToArray())
    defaults = [ordered]@{
        self_bias = 0.2
        unmapped_bias = 0.0
    }
}

$scarTelemetry = [ordered]@{
    schema = "sli.scar.telemetry.v0.1.0"
    bias_block_count = $biasBlocks.Count
    anchor_count = $anchors.Count
    token_node_map_coverage_rate = $coverage
    scope_barrier_count = $scopeBarrierCount
    avg_bias_magnitude = $avgBiasMagnitude
    warnings = @($warnings)
    glue_maps_loaded = $glueMaps.Count
    glue_relations_used = $glueRelationsUsed
    scope_barrier_overrides = $scopeBarrierOverrides
    cross_domain_edges_created = $crossDomainEdgesCreated
}

$biasPath = Join-Path $OutDir "scar_bias_spec.json"
$headPath = Join-Path $OutDir "scar_head_gate.json"
$kvPath = Join-Path $OutDir "scar_kv_anchor.json"
$telemetryPath = Join-Path $OutDir "scar_telemetry.json"

Write-JsonFile -Path $biasPath -Object $biasSpec
Write-JsonFile -Path $headPath -Object $headGate
Write-JsonFile -Path $kvPath -Object $kvAnchor
Write-JsonFile -Path $telemetryPath -Object $scarTelemetry

# Merge into cognition telemetry
if (-not (Test-Path -Path $CognitionTelemetryPath -PathType Leaf)) {
    Write-Error "Cognition telemetry file not found for SCAR merge: $CognitionTelemetryPath"
    exit 1
}
$cognition = Read-JsonFile -Path $CognitionTelemetryPath
if (-not ($cognition.PSObject.Properties.Name -contains "metrics") -or $null -eq $cognition.metrics) {
    $cognition | Add-Member -MemberType NoteProperty -Name metrics -Value ([ordered]@{})
}

$metricsMap = [ordered]@{}
foreach ($prop in $cognition.metrics.PSObject.Properties) {
    $metricsMap[$prop.Name] = $prop.Value
}

if (-not $metricsMap.Contains("layer0")) {
    $metricsMap["layer0"] = [ordered]@{}
}

$layer0 = [ordered]@{}
if ($metricsMap["layer0"] -is [pscustomobject]) {
    foreach ($prop in $metricsMap["layer0"].PSObject.Properties) { $layer0[$prop.Name] = $prop.Value }
}
elseif ($metricsMap["layer0"] -is [System.Collections.IDictionary]) {
    foreach ($k in $metricsMap["layer0"].Keys) { $layer0[[string]$k] = $metricsMap["layer0"][$k] }
}
$layer0["scar"] = $scarTelemetry
$metricsMap["layer0"] = [pscustomobject]$layer0

# Forward-compat placeholders
$metricsMap["layer_0"] = [pscustomobject]@{ scar = $scarTelemetry }
$metricsMap["layer_1"] = [pscustomobject]@{ semantic_flow = $null }
$metricsMap["layer_2"] = [pscustomobject]@{ drift = $null }
$metricsMap["layer_3"] = [pscustomobject]@{ flow = $null }

$cognition.metrics = [pscustomobject]$metricsMap
Write-JsonFile -Path $CognitionTelemetryPath -Object $cognition

Write-Host "Wrote SCAR bias spec: $biasPath"
Write-Host "Wrote SCAR head gate: $headPath"
Write-Host "Wrote SCAR kv anchor: $kvPath"
Write-Host "Wrote SCAR telemetry: $telemetryPath"
Write-Host "Merged SCAR telemetry into cognition telemetry: $CognitionTelemetryPath"

exit 0
