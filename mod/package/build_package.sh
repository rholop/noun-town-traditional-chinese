#!/usr/bin/env bash
# Stage a self-contained, installable copy of the NounTownZhTW mod under
# mod/package/dist/NounTownZhTW/. Run install.sh from inside that directory
# against a Noun Town Language Learning install to set it up.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GAME_DIR="$(cd "$SCRIPT_DIR/../.." && pwd)"
DIST="$SCRIPT_DIR/dist/NounTownZhTW"

rm -rf "$DIST"
mkdir -p "$DIST/bepinex/BepInEx" "$DIST/plugin/fonts" "$DIST/mod-build/scripts"

# --- BepInEx framework (Doorstop + BepInEx 6 IL2CPP runtime) ---
echo "==> Staging BepInEx framework"
cp -r "$GAME_DIR/BepInEx/core" "$DIST/bepinex/BepInEx/"
mkdir -p "$DIST/bepinex/BepInEx/patchers"
cp -r "$GAME_DIR/BepInEx/unity-libs" "$DIST/bepinex/BepInEx/"
mkdir -p "$DIST/bepinex/BepInEx/config"
cp "$GAME_DIR/BepInEx/config/BepInEx.cfg" "$DIST/bepinex/BepInEx/config/"
cp -r "$GAME_DIR/dotnet" "$DIST/bepinex/"
cp "$GAME_DIR/doorstop_config.ini" "$DIST/bepinex/"
cp "$GAME_DIR/.doorstop_version" "$DIST/bepinex/"
cp "$GAME_DIR/winhttp.dll" "$DIST/bepinex/"
cp "$SCRIPT_DIR/LICENSE_BepInEx.txt" "$DIST/bepinex/"

# --- Plugin ---
echo "==> Building NounTownZhTW plugin"
export PATH="$HOME/.dotnet:$PATH"
( cd "$GAME_DIR/mod/plugin/NounTownZhTW" && dotnet build -c Release )
cp "$GAME_DIR/mod/plugin/NounTownZhTW/bin/Release/net6.0/NounTownZhTW.dll" "$DIST/plugin/"
cp "$GAME_DIR/mod/fonts/"* "$DIST/plugin/fonts/"

# --- zh-TW data build pipeline (re-run at install time against the target game) ---
echo "==> Staging build scripts"
cp "$GAME_DIR"/mod/scripts/{gamedir.py,extract_bundles.py,build_languagelist.py,convert_opencc.py,build_shadow_bundles.py,build_all.sh,requirements.txt} "$DIST/mod-build/scripts/"

# --- Installer + docs ---
cp "$SCRIPT_DIR/install.sh" "$DIST/"
cp "$SCRIPT_DIR/README.md" "$DIST/"
cp "$SCRIPT_DIR/LICENSE_NounTownTraditionalChinese.txt" "$DIST/"
cp "$SCRIPT_DIR/LICENSE_BepInEx.txt" "$DIST/"
chmod +x "$DIST/install.sh" "$DIST/mod-build/scripts/build_all.sh"

echo "==> Package staged at $DIST"
