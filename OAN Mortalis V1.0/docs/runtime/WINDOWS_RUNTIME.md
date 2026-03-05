# Windows Runtime Setup

## Objective

Run the CME runtime stack natively on Windows with runtime state rooted at:

```text
C:\CradleTek\
```

Repository content remains source-only. Runtime artifacts are external.

## Runtime Layout

Bootstrap creates:

```text
C:\CradleTek\
  runtime\
  models\
  logs\
  telemetry\
  vm\
  cme\
```

## Prerequisites

1. Windows 10/11 with PowerShell.
2. Python 3 on `PATH`.
3. `git` on `PATH`.
4. `cmake` and Visual Studio C++ build tools (or equivalent MSVC toolchain).
5. Hyper-V support preferred for microVM workflows.

## Bootstrap Command

From repository root:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\windows-bootstrap.ps1
```

Optional flags:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\windows-bootstrap.ps1 -SkipLlamaBuild
powershell -ExecutionPolicy Bypass -File scripts\windows-bootstrap.ps1 -SkipStartService
```

The script performs:

1. Runtime directory creation under `C:\CradleTek`.
2. Hyper-V and CPU virtualization checks.
3. Environment variable configuration:
`CRADLETEK_RUNTIME_ROOT`, `OAN_RUNTIME_ROOT`, `OAN_MODEL_PATH`,
`OAN_SELF_GEL`, `OAN_CSELF_GEL`, `OAN_GOA`, `OAN_CGOA`, `OAN_SOULFRAME_HOST_URL`.
4. Python venv setup at `C:\CradleTek\runtime\venv`.
5. Python package install (`flask`, `requests`).
6. `llama.cpp` clone/build/install to `C:\CradleTek\runtime\llama.cpp`.
7. Inference service deployment and service start.

## llama.cpp Installation Details

Source repository:
[ggerganov/llama.cpp](https://github.com/ggerganov/llama.cpp)

Installed layout:

```text
C:\CradleTek\runtime\llama.cpp\
  src\
  build\
  bin\
```

Expected binaries in `bin\` include `llama-cli.exe` when build succeeds.

## Flask Inference Service

Runtime service path:

```text
C:\CradleTek\runtime\inference_service\app.py
```

Default endpoint base:

```text
http://127.0.0.1:8181
```

Endpoints:

1. `POST /infer`
2. `POST /classify`
3. `POST /semantic_expand`
4. `POST /embedding`

Additional control endpoints for existing client compatibility:

1. `GET /health`
2. `POST /vm/spawn`
3. `POST /vm/pause`
4. `POST /vm/reset`
5. `POST /vm/destroy`
6. `POST /vm/upgrade`

## Runtime Config

Config file:

```text
C:\CradleTek\runtime\config.json
```

Schema fields:

1. `model_path`
2. `inference_port`
3. `max_context`
4. `telemetry_enabled`

## Model Installation

1. Copy model file manually to `C:\CradleTek\models\` (example: `seed.gguf`).
2. Update `C:\CradleTek\runtime\config.json` field `model_path`.
3. Restart runtime with `scripts\windows-bootstrap.ps1 -SkipLlamaBuild`.

## SoulFrame.Host Connectivity

`SoulFrame.Host` resolves endpoint from `OAN_SOULFRAME_HOST_URL` or defaults to:

```text
http://127.0.0.1:8181
```

Validation command:

```powershell
Invoke-RestMethod -Method Get -Uri http://127.0.0.1:8181/health
```
