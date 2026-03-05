#!/usr/bin/env python3
"""
Windows-native inference service template for CradleTek runtime.

This file is source code and should be copied to
<CRADLETEK_RUNTIME_ROOT>\\runtime\\inference_service\\app.py
by scripts/windows-bootstrap.ps1.
"""

import hashlib
import json
import os
import subprocess
from datetime import datetime, timezone
from pathlib import Path
from typing import Any, Dict, Optional

from flask import Flask, jsonify, request

APP = Flask(__name__)


def utc_now() -> str:
    return datetime.now(timezone.utc).isoformat()


def str_hash(value: str) -> str:
    return hashlib.sha256(value.encode("utf-8")).hexdigest()


def runtime_root() -> Path:
    return Path(os.getenv("CRADLETEK_RUNTIME_ROOT", str(Path.home() / "CradleTek")))


def load_config() -> Dict[str, Any]:
    cfg_path = runtime_root() / "runtime" / "config.json"
    if cfg_path.exists():
        with cfg_path.open("r", encoding="utf-8") as handle:
            cfg = json.load(handle)
    else:
        cfg = {}

    cfg.setdefault("model_path", str(runtime_root() / "models" / "seed.gguf"))
    cfg.setdefault("inference_port", 8181)
    cfg.setdefault("max_context", 2048)
    cfg.setdefault("telemetry_enabled", True)

    if os.getenv("OAN_MODEL_PATH"):
        cfg["model_path"] = os.getenv("OAN_MODEL_PATH")
    if os.getenv("SOULFRAME_API_PORT"):
        cfg["inference_port"] = int(os.getenv("SOULFRAME_API_PORT", "8181"))

    return cfg


CONFIG = load_config()


def emit_telemetry(event_type: str, detail: Dict[str, Any]) -> None:
    if not CONFIG.get("telemetry_enabled", True):
        return

    payload = {
        "event_type": event_type,
        "timestamp": utc_now(),
        "event_hash": str_hash(f"{event_type}|{json.dumps(detail, sort_keys=True)}"),
        "detail": detail,
    }

    telemetry_file = runtime_root() / "telemetry" / "inference_events.ndjson"
    telemetry_file.parent.mkdir(parents=True, exist_ok=True)
    with telemetry_file.open("a", encoding="utf-8") as handle:
        handle.write(json.dumps(payload) + "\n")


def find_llama_cli() -> Optional[Path]:
    candidates = [
        runtime_root() / "runtime" / "llama.cpp" / "bin" / "llama-cli.exe",
        runtime_root() / "runtime" / "llama.cpp" / "bin" / "main.exe",
    ]
    for candidate in candidates:
        if candidate.exists():
            return candidate
    return None


def run_llama(prompt: str, max_tokens: int) -> str:
    model_path = Path(CONFIG["model_path"])
    cli = find_llama_cli()
    if cli is None or not model_path.exists():
        return json.dumps(
            {
                "mode": "stub",
                "reason": "llama-cli or model missing",
                "trace": str_hash(prompt)[:16],
            }
        )

    args = [
        str(cli),
        "-m",
        str(model_path),
        "-p",
        prompt,
        "-n",
        str(max_tokens),
        "--temp",
        "0.2",
    ]

    result = subprocess.run(args, capture_output=True, text=True, check=False, timeout=120)
    output = (result.stdout or "").strip()
    if not output:
        output = (result.stderr or "").strip()
    if not output:
        output = json.dumps({"mode": "stub", "trace": str_hash(prompt)[:16]})
    return output[:4000]


def parse_payload() -> Dict[str, Any]:
    data = request.get_json(silent=True) or {}
    task = data.get("task") or "infer"
    context = data.get("context") or data.get("text") or data.get("prompt") or ""
    constraints = data.get("opal_constraints") or {}
    max_tokens = int(constraints.get("max_tokens", CONFIG["max_context"]))
    return {"task": task, "context": context, "max_tokens": max_tokens}


def handle_inference(default_task: str) -> Any:
    payload = parse_payload()
    task = payload["task"] or default_task
    context = payload["context"]
    max_tokens = max(1, min(payload["max_tokens"], int(CONFIG["max_context"])))

    emit_telemetry("InferenceRequested", {"task": task, "max_tokens": max_tokens})
    body = run_llama(context, max_tokens)
    response = {
        "decision": f"{task}-ok",
        "payload": body,
        "confidence": 0.70,
    }
    emit_telemetry("InferenceCompleted", {"task": task})
    return jsonify(response)


@APP.get("/health")
def health() -> Any:
    return jsonify(
        {
            "status": "ok",
            "time": utc_now(),
            "model_path": CONFIG["model_path"],
            "inference_port": CONFIG["inference_port"],
        }
    )


@APP.post("/infer")
def infer() -> Any:
    return handle_inference("infer")


@APP.post("/classify")
def classify() -> Any:
    return handle_inference("classify")


@APP.post("/semantic_expand")
def semantic_expand() -> Any:
    return handle_inference("semantic_expand")


@APP.post("/embedding")
def embedding() -> Any:
    return handle_inference("embedding")


@APP.post("/vm/spawn")
@APP.post("/vm/pause")
@APP.post("/vm/reset")
@APP.post("/vm/destroy")
@APP.post("/vm/upgrade")
def vm_control() -> Any:
    operation = request.path.rsplit("/", 1)[-1]
    emit_telemetry("InferenceRequested", {"task": f"vm-{operation}"})
    emit_telemetry("InferenceCompleted", {"task": f"vm-{operation}"})
    return jsonify({"accepted": True, "operation": operation})


if __name__ == "__main__":
    host = os.getenv("SOULFRAME_API_HOST", "127.0.0.1")
    port = int(os.getenv("SOULFRAME_API_PORT", str(CONFIG["inference_port"])))
    APP.run(host=host, port=port, debug=False)
