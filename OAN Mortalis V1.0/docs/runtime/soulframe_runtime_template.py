#!/usr/bin/env python3
"""
Reference runtime template for SoulFrame.Actualized hosted device.

This script is a template only. It is intended to run outside the repository,
for example under C:\\CradleTek\\runtime\\soulframe_runtime.py.
"""

import argparse
import hashlib
import json
import os
from datetime import datetime, timezone
from typing import Any, Dict

import requests
from flask import Flask, jsonify, request


def utc_now() -> str:
    return datetime.now(timezone.utc).isoformat()


def hash_hex(value: str) -> str:
    return hashlib.sha256(value.encode("utf-8")).hexdigest()


def emit_telemetry(event_type: str, detail: Dict[str, Any]) -> None:
    endpoint = os.getenv("SOULFRAME_TELEMETRY_ENDPOINT", "").strip()
    payload = {
        "event_type": event_type,
        "timestamp": utc_now(),
        "event_hash": hash_hex(f"{event_type}|{json.dumps(detail, sort_keys=True)}"),
        "detail": detail,
    }
    if not endpoint:
        print(json.dumps(payload))
        return
    try:
        requests.post(endpoint, json=payload, timeout=1.5)
    except Exception:
        print(json.dumps(payload))


def infer_stub(task: str, context: str) -> Dict[str, Any]:
    short_hash = hash_hex(context)[:16]
    return {
        "decision": f"{task}-ok",
        "payload": json.dumps({"trace": short_hash, "context": context[:120]}),
        "confidence": 0.74,
    }


def parse_constraints(payload: Dict[str, Any]) -> Dict[str, Any]:
    constraints = payload.get("opal_constraints") or {}
    return {
        "domain": constraints.get("domain", "general"),
        "drift_limit": float(constraints.get("drift_limit", 0.02)),
        "max_tokens": int(constraints.get("max_tokens", 128)),
    }


def validate_constraints(context: str, constraints: Dict[str, Any]) -> str | None:
    if constraints["drift_limit"] < 0 or constraints["drift_limit"] > 1:
        return "drift_limit must be between 0 and 1"
    if constraints["max_tokens"] <= 0:
        return "max_tokens must be > 0"
    token_count = len(context.split())
    if token_count > constraints["max_tokens"]:
        return "context exceeds max token budget"
    return None


app = Flask(__name__)


@app.get("/health")
def health() -> Any:
    model_path = os.getenv("SOULFRAME_MODEL_PATH", "C:\\CradleTek\\models\\seed.gguf")
    return jsonify({
        "status": "ok",
        "model_path": model_path,
        "time": utc_now(),
    })


@app.post("/vm/spawn")
@app.post("/vm/pause")
@app.post("/vm/reset")
@app.post("/vm/destroy")
@app.post("/vm/upgrade")
def vm_control() -> Any:
    op = request.path.split("/")[-1]
    emit_telemetry("InferenceRequested", {"task": f"vm-{op}"})
    emit_telemetry("InferenceCompleted", {"task": f"vm-{op}"})
    return jsonify({"accepted": True, "operation": op})


def handle_task(task_name: str) -> Any:
    payload = request.get_json(silent=True) or {}
    task = payload.get("task", task_name)
    context = payload.get("context") or payload.get("text") or payload.get("prompt") or ""
    constraints = parse_constraints(payload)

    emit_telemetry("InferenceRequested", {"task": task, "domain": constraints["domain"]})
    violation = validate_constraints(context, constraints)
    if violation:
        emit_telemetry("ConstraintViolation", {"task": task, "reason": violation})
        emit_telemetry("InferenceRefused", {"task": task, "reason": "constraint"})
        return jsonify({"decision": f"{task}-refused", "payload": "{}", "confidence": 0.0}), 400

    result = infer_stub(task, context)
    if constraints["drift_limit"] < 0.05 and result["confidence"] < 0.45:
        emit_telemetry("DriftDetected", {"task": task, "confidence": result["confidence"]})

    emit_telemetry("InferenceCompleted", {"task": task, "confidence": result["confidence"]})
    return jsonify(result)


@app.post("/infer")
def infer() -> Any:
    return handle_task("infer")


@app.post("/classify")
def classify() -> Any:
    return handle_task("classify")


@app.post("/semantic_expand")
def semantic_expand() -> Any:
    return handle_task("semantic_expand")


@app.post("/embedding")
def embedding() -> Any:
    return handle_task("embedding")


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--host", default=os.getenv("SOULFRAME_API_HOST", "127.0.0.1"))
    parser.add_argument("--port", type=int, default=int(os.getenv("SOULFRAME_API_PORT", "8888")))
    args = parser.parse_args()
    app.run(host=args.host, port=args.port, debug=False)


if __name__ == "__main__":
    main()
