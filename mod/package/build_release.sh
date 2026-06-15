#!/usr/bin/env bash
# Maintainer tool: assemble Windows and Linux release zips for NounTownZhTW.
#
# Packages only redistributable, game-independent files: the bundled
# BepInEx/Doorstop framework (mod/bepinex/), the prebuilt plugin DLL, the CJK
# fallback font, the zh-TW build pipeline scripts, an OS-native installer,
# and licenses.
#
# Deliberately does NOT include mod/shadow/, mod/extracted/, or mod/zh-TW/
# (or anything else derived from the game's own asset bundles) - that data is
# generated locally by the installer against the user's own legally-owned
# copy of the game. Redistributing it would mean redistributing derived
# copyrighted game content. Do not add those directories here.
#
# Usage: mod/package/build_release.sh vX.Y.Z
set -euo pipefail

if [ $# -ne 1 ]; then
    echo "Usage: $0 <version, e.g. v0.2.0>" >&2
    exit 1
fi
VERSION="$1"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_DIR="$(cd "$SCRIPT_DIR/../.." && pwd)"
DIST_DIR="$REPO_DIR/mod/package/dist"
STAGE_DIR="$DIST_DIR/.staging"

rm -rf "$STAGE_DIR"
mkdir -p "$STAGE_DIR/mod/plugin/NounTownZhTW" "$STAGE_DIR/mod/fonts" "$STAGE_DIR/mod/scripts"

echo "==> Staging common payload"
cp -r "$REPO_DIR/mod/bepinex" "$STAGE_DIR/mod/"
cp "$REPO_DIR/mod/plugin/NounTownZhTW/NounTownZhTW.dll" "$STAGE_DIR/mod/plugin/NounTownZhTW/"
cp "$REPO_DIR/mod/fonts/"* "$STAGE_DIR/mod/fonts/"
cp "$REPO_DIR/mod/scripts/"*.py "$REPO_DIR/mod/scripts/"*.sh "$REPO_DIR/mod/scripts/"*.ps1 "$REPO_DIR/mod/scripts/"*.txt "$STAGE_DIR/mod/scripts/"
cp "$REPO_DIR/mod/package/LICENSE_BepInEx.txt" "$REPO_DIR/mod/package/LICENSE_NounTownTraditionalChinese.txt" "$STAGE_DIR/"

build_zip() {
    local platform="$1"
    local zip_name="NounTownZhTW-${VERSION}-${platform}.zip"
    local pkg_dir="$DIST_DIR/.pkg-$platform"

    echo "==> Building $zip_name"
    rm -rf "$pkg_dir"
    cp -r "$STAGE_DIR" "$pkg_dir"

    case "$platform" in
        windows)
            cp "$REPO_DIR/install.ps1" "$REPO_DIR/install.bat" "$pkg_dir/"
            cp "$SCRIPT_DIR/INSTALL_windows.md" "$pkg_dir/INSTALL.md"
            ;;
        linux)
            cp "$REPO_DIR/install.sh" "$pkg_dir/"
            cp "$SCRIPT_DIR/INSTALL_linux.md" "$pkg_dir/INSTALL.md"
            ;;
        *)
            echo "error: unknown platform '$platform'" >&2
            exit 1
            ;;
    esac

    rm -f "$DIST_DIR/$zip_name"
    ( cd "$pkg_dir" && zip -r -q "$DIST_DIR/$zip_name" . )
    rm -rf "$pkg_dir"
}

mkdir -p "$DIST_DIR"
build_zip windows
build_zip linux
rm -rf "$STAGE_DIR"

echo "==> Done. Release zips in $DIST_DIR:"
ls -1 "$DIST_DIR"
