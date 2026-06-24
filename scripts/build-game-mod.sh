#!/usr/bin/env bash
set -euo pipefail

if [[ $# -ne 1 ]]; then
  echo "Usage: $0 /path/to/7DaysToDie_Data/Managed" >&2
  exit 2
fi

MANAGED_DIR="$1"
OUT_DIR="${OUT_DIR:-build}"

for required in Assembly-CSharp.dll UnityEngine.CoreModule.dll 0Harmony.dll; do
  if [[ ! -f "$MANAGED_DIR/$required" ]]; then
    echo "Missing required file: $MANAGED_DIR/$required" >&2
    exit 1
  fi
done

NETSTANDARD="/usr/lib/mono/4.8-api/Facades/netstandard.dll"
if [[ ! -f "$NETSTANDARD" ]]; then
  NETSTANDARD="/usr/lib/mono/4.5/Facades/netstandard.dll"
fi

if [[ ! -f "$NETSTANDARD" ]]; then
  echo "Could not find Mono netstandard.dll facade." >&2
  exit 1
fi

mkdir -p "$OUT_DIR"

mcs \
  -target:library \
  -define:GRAVE_7DTD \
  -out:"$OUT_DIR/GraveAlive.dll" \
  -r:System.Xml.Linq \
  -r:"$NETSTANDARD" \
  -r:"$MANAGED_DIR/Assembly-CSharp.dll" \
  -r:"$MANAGED_DIR/UnityEngine.CoreModule.dll" \
  -r:"$MANAGED_DIR/0Harmony.dll" \
  Source/GraveAlive/GraveAliveRuntime.cs \
  Source/GraveAlive/Simulation/*.cs \
  Source/GraveAlive/GameIntegration/*.cs

echo "Built $OUT_DIR/GraveAlive.dll"
