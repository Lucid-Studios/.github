param(
    [string]$CradleTekRoot = "",
    [string]$PythonBin = "python",
    [string]$LlamaRepoUrl = "https://github.com/ggerganov/llama.cpp",
    [switch]$SkipLlamaBuild,
    [switch]$SkipStartService
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($CradleTekRoot)) {
    $CradleTekRoot = Join-Path $env:SystemDrive "CradleTek"
}

$isWindowsHost = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
    [System.Runtime.InteropServices.OSPlatform]::Windows
)
if (-not $isWindowsHost) {
    throw "windows-bootstrap.ps1 must run on Windows."
}

function Write-Step {
    param([string]$Message)
    Write-Host "[windows-bootstrap] $Message"
}

function Test-Command {
    param([string]$Name)
    return $null -ne (Get-Command $Name -ErrorAction SilentlyContinue)
}

function Invoke-OrThrow {
    param(
        [string]$FilePath,
        [string[]]$Arguments
    )
    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Command failed: $FilePath $($Arguments -join ' ')"
    }
}

function New-CradleTekDirectories {
    param([string]$Root)

    $folders = @("runtime", "models", "logs", "telemetry", "vm", "cme")
    foreach ($folder in $folders) {
        New-Item -Path (Join-Path $Root $folder) -ItemType Directory -Force | Out-Null
    }

    $cmeFolders = @("SelfGEL", "cSelfGEL", "GoA", "cGoA")
    foreach ($folder in $cmeFolders) {
        New-Item -Path (Join-Path $Root "cme\$folder") -ItemType Directory -Force | Out-Null
    }

    Write-Step "Runtime directory layout prepared under $Root."
}

function Test-HyperVAvailability {
    $result = [ordered]@{
        FeatureName = "Microsoft-Hyper-V-All"
        Enabled = $false
        Detail = "Feature state unavailable."
    }

    if (Test-Command "Get-WindowsOptionalFeature") {
        $feature = Get-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V-All -ErrorAction SilentlyContinue
        if ($null -ne $feature) {
            $result.Enabled = $feature.State -eq "Enabled"
            $result.Detail = "State: $($feature.State)"
        } else {
            $result.Detail = "Hyper-V feature not found by Get-WindowsOptionalFeature."
        }
    } else {
        $result.Detail = "Get-WindowsOptionalFeature unavailable in this shell."
    }

    if ($result.Enabled) {
        Write-Step "Hyper-V check passed. $($result.Detail)"
    } else {
        Write-Warning "[windows-bootstrap] Hyper-V not enabled. $($result.Detail)"
    }
}

function Test-CpuVirtualizationSupport {
    $cpu = Get-CimInstance Win32_Processor | Select-Object -First 1
    if ($null -eq $cpu) {
        Write-Warning "[windows-bootstrap] CPU virtualization check unavailable."
        return
    }

    $virtEnabled = [bool]$cpu.VirtualizationFirmwareEnabled
    $slat = [bool]$cpu.SecondLevelAddressTranslationExtensions
    $vmExt = [bool]$cpu.VMMonitorModeExtensions

    Write-Step "CPU virtualization firmware enabled: $virtEnabled"
    Write-Step "CPU SLAT support: $slat"
    Write-Step "CPU VM monitor mode support: $vmExt"

    if (-not $virtEnabled -or -not $slat -or -not $vmExt) {
        Write-Warning "[windows-bootstrap] Virtualization prerequisites are incomplete for Hyper-V microVM workflows."
    }
}

function Set-UserEnvironmentVariables {
    param([string]$Root)

    $values = @{
        "CRADLETEK_RUNTIME_ROOT" = $Root
        "OAN_RUNTIME_ROOT" = $Root
        "OAN_MODEL_PATH" = (Join-Path $Root "models\seed.gguf")
        "OAN_SELF_GEL" = (Join-Path $Root "cme\SelfGEL")
        "OAN_CSELF_GEL" = (Join-Path $Root "cme\cSelfGEL")
        "OAN_GOA" = (Join-Path $Root "cme\GoA")
        "OAN_CGOA" = (Join-Path $Root "cme\cGoA")
        "OAN_SOULFRAME_HOST_URL" = "http://127.0.0.1:8181"
    }

    foreach ($entry in $values.GetEnumerator()) {
        [Environment]::SetEnvironmentVariable($entry.Key, $entry.Value, "User")
        [Environment]::SetEnvironmentVariable($entry.Key, $entry.Value, "Process")
    }

    Write-Step "Environment variables configured for current user and process."
}

function Ensure-PythonRuntime {
    param(
        [string]$Root,
        [string]$PythonExecutable
    )

    if (-not (Test-Command $PythonExecutable)) {
        throw "Python executable '$PythonExecutable' is not available on PATH."
    }

    $venvDir = Join-Path $Root "runtime\venv"
    if (-not (Test-Path $venvDir)) {
        Invoke-OrThrow -FilePath $PythonExecutable -Arguments @("-m", "venv", $venvDir)
    }

    $venvPython = Join-Path $venvDir "Scripts\python.exe"
    if (-not (Test-Path $venvPython)) {
        throw "Virtual environment python executable not found under $venvDir."
    }

    Invoke-OrThrow -FilePath $venvPython -Arguments @("-m", "pip", "install", "--upgrade", "pip")
    Invoke-OrThrow -FilePath $venvPython -Arguments @("-m", "pip", "install", "flask", "requests")
    Write-Step "Python runtime ready at $venvDir."
    return $venvPython
}

function Install-LlamaCpp {
    param(
        [string]$Root,
        [string]$RepoUrl
    )

    if (-not (Test-Command "git")) {
        throw "git is required to clone llama.cpp."
    }
    if (-not (Test-Command "cmake")) {
        throw "cmake is required to build llama.cpp."
    }

    $llamaRoot = Join-Path $Root "runtime\llama.cpp"
    $llamaSrc = Join-Path $llamaRoot "src"
    $llamaBuild = Join-Path $llamaRoot "build"
    $llamaBin = Join-Path $llamaRoot "bin"
    New-Item -Path $llamaRoot -ItemType Directory -Force | Out-Null

    if (Test-Path (Join-Path $llamaSrc ".git")) {
        Invoke-OrThrow -FilePath "git" -Arguments @("-C", $llamaSrc, "pull", "--ff-only")
    } else {
        Invoke-OrThrow -FilePath "git" -Arguments @("clone", $RepoUrl, $llamaSrc)
    }

    Invoke-OrThrow -FilePath "cmake" -Arguments @("-S", $llamaSrc, "-B", $llamaBuild, "-DGGML_OPENMP=ON", "-DCMAKE_BUILD_TYPE=Release")
    Invoke-OrThrow -FilePath "cmake" -Arguments @("--build", $llamaBuild, "--config", "Release")

    New-Item -Path $llamaBin -ItemType Directory -Force | Out-Null
    $patterns = @(
        (Join-Path $llamaBuild "bin\Release\llama-*.exe"),
        (Join-Path $llamaBuild "bin\llama-*.exe"),
        (Join-Path $llamaBuild "bin\Release\main.exe"),
        (Join-Path $llamaBuild "bin\main.exe")
    )

    $copied = $false
    foreach ($pattern in $patterns) {
        $matches = Get-ChildItem -Path $pattern -ErrorAction SilentlyContinue
        foreach ($item in $matches) {
            Copy-Item -Path $item.FullName -Destination (Join-Path $llamaBin $item.Name) -Force
            $copied = $true
        }
    }

    if (-not $copied) {
        Write-Warning "[windows-bootstrap] llama.cpp built but no known binaries were found for copy. Check build output."
    } else {
        Write-Step "llama.cpp binaries installed to $llamaBin."
    }
}

function Install-InferenceServiceTemplate {
    param(
        [string]$Root,
        [string]$RepoRoot
    )

    $serviceTemplate = Join-Path $RepoRoot "runtime\inference_service\app.py"
    $configTemplate = Join-Path $RepoRoot "runtime\config.json"

    if (-not (Test-Path $serviceTemplate)) {
        throw "Missing service template: $serviceTemplate"
    }
    if (-not (Test-Path $configTemplate)) {
        throw "Missing config template: $configTemplate"
    }

    $serviceDir = Join-Path $Root "runtime\inference_service"
    $servicePath = Join-Path $serviceDir "app.py"
    $configPath = Join-Path $Root "runtime\config.json"

    New-Item -Path $serviceDir -ItemType Directory -Force | Out-Null
    Copy-Item -Path $serviceTemplate -Destination $servicePath -Force
    if (-not (Test-Path $configPath)) {
        Copy-Item -Path $configTemplate -Destination $configPath -Force
    }

    Write-Step "Inference service template deployed to $servicePath."
}

function Start-InferenceService {
    param(
        [string]$Root,
        [string]$VenvPython
    )

    $runtimeDir = Join-Path $Root "runtime"
    $servicePath = Join-Path $runtimeDir "inference_service\app.py"
    $configPath = Join-Path $runtimeDir "config.json"
    $logPath = Join-Path $Root "logs\inference_service.log"
    $pidPath = Join-Path $runtimeDir "inference_service.pid"

    if (-not (Test-Path $servicePath)) {
        throw "Service script not found: $servicePath"
    }
    if (-not (Test-Path $configPath)) {
        throw "Runtime config not found: $configPath"
    }

    if (Test-Path $pidPath) {
        $pidValue = Get-Content $pidPath -ErrorAction SilentlyContinue
        if ($pidValue -match "^\d+$") {
            $running = Get-Process -Id ([int]$pidValue) -ErrorAction SilentlyContinue
            if ($null -ne $running) {
                Write-Step "Inference service already running (pid=$pidValue)."
                return
            }
        }
    }

    $config = Get-Content $configPath -Raw | ConvertFrom-Json
    $port = if ($null -ne $config.inference_port) { [int]$config.inference_port } else { 8181 }

    [Environment]::SetEnvironmentVariable("CRADLETEK_RUNTIME_ROOT", $Root, "Process")
    [Environment]::SetEnvironmentVariable("SOULFRAME_API_PORT", "$port", "Process")
    [Environment]::SetEnvironmentVariable("OAN_MODEL_PATH", (Join-Path $Root "models\seed.gguf"), "Process")

    New-Item -Path (Split-Path $logPath -Parent) -ItemType Directory -Force | Out-Null

    $proc = Start-Process `
        -FilePath $VenvPython `
        -ArgumentList @($servicePath) `
        -RedirectStandardOutput $logPath `
        -RedirectStandardError $logPath `
        -PassThru `
        -WindowStyle Hidden

    $proc.Id | Set-Content $pidPath
    Write-Step "Inference service started on http://127.0.0.1:$port (pid=$($proc.Id))."
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path

Write-Step "Starting Windows runtime bootstrap."
New-CradleTekDirectories -Root $CradleTekRoot
Test-HyperVAvailability
Test-CpuVirtualizationSupport
Set-UserEnvironmentVariables -Root $CradleTekRoot

$venvPython = Ensure-PythonRuntime -Root $CradleTekRoot -PythonExecutable $PythonBin
Install-InferenceServiceTemplate -Root $CradleTekRoot -RepoRoot $repoRoot

if (-not $SkipLlamaBuild) {
    Install-LlamaCpp -Root $CradleTekRoot -RepoUrl $LlamaRepoUrl
} else {
    Write-Step "Skipping llama.cpp build by request."
}

if (-not $SkipStartService) {
    Start-InferenceService -Root $CradleTekRoot -VenvPython $venvPython
} else {
    Write-Step "Skipping inference service start by request."
}

Write-Step "Bootstrap complete."
