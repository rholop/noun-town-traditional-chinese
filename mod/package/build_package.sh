#!/usr/bin/env bash
# Maintainer tool: refresh the committed, bundled BepInEx framework
# (mod/bepinex/) and the prebuilt plugin DLL
# (mod/plugin/NounTownZhTW/NounTownZhTW.dll) from this game install's BepInEx
# setup, so that ./install.sh works from a fresh clone with no build step.
#
# Run this after updating the bundled BepInEx version or the plugin source,
# then commit the results.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GAME_DIR="$(cd "$SCRIPT_DIR/../.." && pwd)"
BEPINEX_OUT="$GAME_DIR/mod/bepinex"

rm -rf "$BEPINEX_OUT"
mkdir -p "$BEPINEX_OUT/BepInEx"

# --- BepInEx framework (Doorstop + BepInEx 6 IL2CPP runtime) ---
echo "==> Staging BepInEx framework"
cp -r "$GAME_DIR/BepInEx/core" "$BEPINEX_OUT/BepInEx/"
mkdir -p "$BEPINEX_OUT/BepInEx/patchers"
cp -r "$GAME_DIR/BepInEx/unity-libs" "$BEPINEX_OUT/BepInEx/"
mkdir -p "$BEPINEX_OUT/BepInEx/config"
cp "$GAME_DIR/BepInEx/config/BepInEx.cfg" "$BEPINEX_OUT/BepInEx/config/"
cp -r "$GAME_DIR/dotnet" "$BEPINEX_OUT/"
cp "$GAME_DIR/doorstop_config.ini" "$BEPINEX_OUT/"
cp "$GAME_DIR/.doorstop_version" "$BEPINEX_OUT/"
cp "$GAME_DIR/winhttp.dll" "$BEPINEX_OUT/"

# --- Plugin ---
echo "==> Building NounTownZhTW plugin"
export PATH="$HOME/.dotnet:$PATH"
( cd "$GAME_DIR/mod/plugin/NounTownZhTW" && dotnet build -c Release )
cp "$GAME_DIR/mod/plugin/NounTownZhTW/bin/Release/net6.0/NounTownZhTW.dll" "$GAME_DIR/mod/plugin/NounTownZhTW/NounTownZhTW.dll"

echo "==> Done. mod/bepinex/ and mod/plugin/NounTownZhTW/NounTownZhTW.dll refreshed."
