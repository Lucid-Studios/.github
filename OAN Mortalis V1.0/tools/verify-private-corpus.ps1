param()

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-TrackedFiles {
    $files = & git ls-files
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to list tracked files. Run this script from inside a git repository."
    }

    return $files | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
}

function Test-CorpusPathLeak {
    param(
        [Parameter(Mandatory = $true)]
        [string] $CorpusPath,

        [Parameter(Mandatory = $true)]
        [string[]] $Files
    )

    $leaks = New-Object System.Collections.Generic.List[string]

    foreach ($file in $Files) {
        if (-not (Test-Path -LiteralPath $file -PathType Leaf)) {
            continue
        }

        $match = Select-String -LiteralPath $file -SimpleMatch $CorpusPath -Quiet -ErrorAction SilentlyContinue
        if ($match) {
            $leaks.Add($file)
        }
    }

    return $leaks
}

$rawCorpusPath = $env:OAN_REFERENCE_CORPUS
if ([string]::IsNullOrWhiteSpace($rawCorpusPath)) {
    Write-Host "PASS: OAN_REFERENCE_CORPUS is not set for this shell. No leak scan performed."
    exit 0
}

$resolvedCorpusPath = [System.IO.Path]::GetFullPath($rawCorpusPath)
$trackedFiles = Get-TrackedFiles
$leakedFiles = @(Test-CorpusPathLeak -CorpusPath $resolvedCorpusPath -Files $trackedFiles)

if ($leakedFiles.Count -gt 0) {
    Write-Host "FAIL: Detected private corpus path leakage in tracked files."
    Write-Host "Reference identifier: Lucid Research Corpus"
    Write-Host "Files containing leaked path:"
    $leakedFiles | Sort-Object -Unique | ForEach-Object { Write-Host " - $_" }
    exit 1
}

Write-Host "PASS: No tracked files contain the private corpus path."
Write-Host "Reference identifier: Lucid Research Corpus"
exit 0
