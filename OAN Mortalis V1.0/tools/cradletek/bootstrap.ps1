param(
    [string]$CradleTekRuntimeRoot = $env:CRADLETEK_RUNTIME_ROOT,
    [string]$PythonBin = $env:PYTHON_BIN,
    [string]$RuntimeEnvFile = $env:RUNTIME_ENV_FILE
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($CradleTekRuntimeRoot)) {
    $CradleTekRuntimeRoot = "C:\CradleTek"
}

if ([string]::IsNullOrWhiteSpace($PythonBin)) {
    $PythonBin = "python"
}

if ([string]::IsNullOrWhiteSpace($RuntimeEnvFile)) {
    $RuntimeEnvFile = Join-Path $CradleTekRuntimeRoot "runtime\soulframe_runtime.env"
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
Write-Host "[cradletek-bootstrap] Runtime root: $CradleTekRuntimeRoot"

$hypervFeature = Get-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V-All -ErrorAction SilentlyContinue
if ($null -eq $hypervFeature -or $hypervFeature.State -ne "Enabled") {
    Write-Warning "[cradletek-bootstrap] Hyper-V is not enabled. Continue only if your runtime topology does not require VM isolation."
}

$pythonCmd = Get-Command $PythonBin -ErrorAction SilentlyContinue
if ($null -eq $pythonCmd) {
    Write-Error "[cradletek-bootstrap] Python executable '$PythonBin' not found."
}

$folders = @(
    "runtime",
    "models",
    "logs",
    "telemetry",
    "api",
    "services"
)

foreach ($folder in $folders) {
    $path = Join-Path $CradleTekRuntimeRoot $folder
    New-Item -Path $path -ItemType Directory -Force | Out-Null
}

$venvDir = Join-Path $CradleTekRuntimeRoot "runtime\.venv"
if (-not (Test-Path $venvDir)) {
    & $PythonBin -m venv $venvDir
}

$venvPython = Join-Path $venvDir "Scripts\python.exe"
if (-not (Test-Path $venvPython)) {
    Write-Error "[cradletek-bootstrap] Virtual environment python executable not found at '$venvPython'."
}

& $venvPython -m pip install --upgrade pip | Out-Null
& $venvPython -m pip install flask uvicorn requests | Out-Null

if (-not (Test-Path $RuntimeEnvFile)) {
    $envTemplate = Join-Path $repoRoot "docs\runtime\soulframe_runtime.env.example"
    Copy-Item -Path $envTemplate -Destination $RuntimeEnvFile -Force
    Write-Host "[cradletek-bootstrap] Created runtime env from template: $RuntimeEnvFile"
}

$runtimeScript = Join-Path $CradleTekRuntimeRoot "runtime\soulframe_runtime.py"
if (-not (Test-Path $runtimeScript)) {
    $runtimeTemplate = Join-Path $repoRoot "docs\runtime\soulframe_runtime_template.py"
    Copy-Item -Path $runtimeTemplate -Destination $runtimeScript -Force
    Write-Host "[cradletek-bootstrap] Copied runtime API template to runtime root."
}

Write-Host "[cradletek-bootstrap] Bootstrap complete."
Write-Host "[cradletek-bootstrap] NOTE: Model weights are not downloaded by this script."
