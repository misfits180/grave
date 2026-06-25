#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 1 || $# -gt 2 ]]; then
  echo "Usage: $0 /path/to/7DaysToDie_Data/Managed [output-directory]" >&2
  exit 2
fi

MANAGED_DIR="$1"
DIST_ROOT="${2:-dist}"
MOD_DIR="$DIST_ROOT/grave"
BUILD_DIR="$DIST_ROOT/build"

rm -rf "$MOD_DIR" "$BUILD_DIR"
mkdir -p "$MOD_DIR"

OUT_DIR="$BUILD_DIR" ./scripts/build-game-mod.sh "$MANAGED_DIR"

cp "$BUILD_DIR/GraveAlive.dll" "$MOD_DIR/GraveAlive.dll"
cp ModInfo.xml README.md "$MOD_DIR/"
cp -R Config "$MOD_DIR/Config"
cp -R docs "$MOD_DIR/docs"

echo "Packaged installable mod folder: $MOD_DIR"
echo "Copy that 'grave' folder into your 7 Days To Die/Mods folder."
