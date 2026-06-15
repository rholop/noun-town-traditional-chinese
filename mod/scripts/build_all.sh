#!/usr/bin/env bash
# Run the full zh-TW data pipeline against the game install this script lives
# under (mod/scripts/../.. is the game directory).
#
# Stages:
#   1. extract_bundles.py   - dump zh-CN/reference TextAssets from the game's bundles
#   2. build_languagelist.py - build the 15-locale languagelist_*.json payload
#   3. convert_opencc.py     - OpenCC s2twp conversion of localisation/itemlanguage/dialogue
#   4. build_shadow_bundles.py - assemble mod/shadow/* with the new zh-TW TextAssets
set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")"

python3 extract_bundles.py
python3 build_languagelist.py
python3 convert_opencc.py
python3 build_shadow_bundles.py
