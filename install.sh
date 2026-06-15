#!/usr/bin/env bash
# Install the NounTownZhTW (Traditional Chinese) mod into a Noun Town
# Language Learning install.
#
# Usage: ./install.sh "/path/to/Noun Town Language Learning"
set -euo pipefail

REPO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if [ $# -ne 1 ]; then
    echo "Usage: $0 <path to Noun Town Language Learning install directory>" >&2
    exit 1
fi

GAME_DIR="$(cd "$1" && pwd)"

# --- Sanity checks ---
if ! ls "$GAME_DIR"/*.exe >/dev/null 2>&1 || [ ! -f "$GAME_DIR/GameAssembly.dll" ]; then
    echo "error: $GAME_DIR does not look like a Unity IL2CPP game install (missing *.exe / GameAssembly.dll)" >&2
    exit 1
fi

shopt -s nullglob
DATA_DIRS=("$GAME_DIR"/*_Data)
shopt -u nullglob
if [ ${#DATA_DIRS[@]} -ne 1 ]; then
    echo "error: expected exactly one *_Data directory in $GAME_DIR, found: ${DATA_DIRS[*]}" >&2
    exit 1
fi

if [ ! -f "${DATA_DIRS[0]}/StreamingAssets/windows/dialoguebundle" ]; then
    echo "error: ${DATA_DIRS[0]}/StreamingAssets/windows/dialoguebundle not found - is this Noun Town Language Learning?" >&2
    exit 1
fi

echo "==> Target: $GAME_DIR"

# --- 1. BepInEx framework (Doorstop + BepInEx 6 IL2CPP runtime) ---
echo "==> Installing BepInEx framework"
cp -r "$REPO_DIR/mod/bepinex/." "$GAME_DIR/"

# --- 2. Build scripts + dependencies ---
echo "==> Installing zh-TW build scripts"
mkdir -p "$GAME_DIR/mod/scripts"
cp -r "$REPO_DIR/mod/scripts/." "$GAME_DIR/mod/scripts/"

echo "==> Checking Python dependencies (UnityPy, opencc-python-reimplemented)"
python3 -m pip install --user -q -r "$GAME_DIR/mod/scripts/requirements.txt"

# --- 3. Build zh-TW shadow bundles from THIS game's actual bundles ---
echo "==> Building zh-TW shadow bundles"
bash "$GAME_DIR/mod/scripts/build_all.sh"

# --- 4. Deploy the plugin ---
echo "==> Installing NounTownZhTW plugin"
PLUGIN_DIR="$GAME_DIR/BepInEx/plugins/NounTownZhTW"
mkdir -p "$PLUGIN_DIR/fonts" "$PLUGIN_DIR/shadow"
cp "$REPO_DIR/mod/plugin/NounTownZhTW/NounTownZhTW.dll" "$PLUGIN_DIR/"
cp "$REPO_DIR/mod/fonts/"* "$PLUGIN_DIR/fonts/"
cp "$GAME_DIR/mod/shadow/"* "$PLUGIN_DIR/shadow/"

echo "==> Done."
echo
echo "If the game is launched via Steam under Proton, add this to its launch options:"
echo "  WINEDLLOVERRIDES=\"winhttp=n,b\" %command%"
