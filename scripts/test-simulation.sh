#!/usr/bin/env bash
set -euo pipefail

OUT_DIR="${OUT_DIR:-build/test}"
mkdir -p "$OUT_DIR"

mcs \
  -target:library \
  -out:"$OUT_DIR/GraveAlive.dll" \
  -r:System.Xml.Linq \
  Source/GraveAlive/GraveAliveRuntime.cs \
  Source/GraveAlive/Simulation/*.cs

mcs \
  -out:"$OUT_DIR/GraveAlive.Tests.exe" \
  -r:System.Xml.Linq \
  -r:"$OUT_DIR/GraveAlive.dll" \
  Source/GraveAlive.Tests/Program.cs

MONO_PATH="$OUT_DIR" mono "$OUT_DIR/GraveAlive.Tests.exe"
