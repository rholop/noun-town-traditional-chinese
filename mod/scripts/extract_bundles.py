#!/usr/bin/env python3
"""Extract zh-CN and reference TextAssets from the three language data bundles.

Outputs raw JSON files under mod/extracted/ for downstream OpenCC conversion.
Read-only: does not modify any game files.
"""
import json
import os
import sys

import UnityPy

from gamedir import find_streaming_assets

GAME_DIR = os.path.join(os.path.dirname(__file__), "..", "..")
BUNDLE_DIR = find_streaming_assets(GAME_DIR)
OUT_DIR = os.path.join(os.path.dirname(__file__), "..", "extracted")


def load_text_assets(bundle_name):
    path = os.path.join(BUNDLE_DIR, bundle_name)
    env = UnityPy.load(path)
    assets = {}
    for obj in env.objects:
        if obj.type.name == "TextAsset":
            data = obj.read()
            script = data.m_Script
            if isinstance(script, bytes):
                script = script.decode("utf-8")
            assets[data.m_Name] = script
    return assets


def dump(name, text):
    out_path = os.path.join(OUT_DIR, name)
    with open(out_path, "w", encoding="utf-8") as f:
        f.write(text)
    print(f"wrote {out_path} ({len(text)} bytes)")


def main():
    os.makedirs(OUT_DIR, exist_ok=True)

    # languagelistbundle: dump ALL 14 locale arrays (needed to build the 15-locale
    # languagelist_zh-TW.json with translated "Name" fields for zh-TW entry).
    lst = load_text_assets("languagelistbundle")
    for locale, text in lst.items():
        dump(f"languagelist_{locale}.json", text)
    print(f"languagelistbundle locales: {sorted(lst.keys())}")

    # languagedatabundle: master (language-agnostic item defs) + zh-CN itemLanguageJson
    data = load_text_assets("languagedatabundle")
    dump("languagedata_master.json", data["master"])
    dump("languagedata_zh-CN.json", data["zh-CN"])
    print(f"languagedatabundle locales: {sorted(data.keys())}")

    # localisationbundle: zh-CN (578 entries expected, current/live table)
    loc = load_text_assets("localisationbundle")
    dump("localisation_zh-CN.json", loc["zh-CN"])
    print(f"localisationbundle locales: {sorted(loc.keys())}")

    # localisationbundle.private.0: zh-CN (555 entries, likely stale previous-build
    # snapshot -- extracted for completeness pending Phase 2 confirmation)
    locp = load_text_assets("localisationbundle.private.0")
    dump("localisation_private0_zh-CN.json", locp["zh-CN"])
    print(f"localisationbundle.private.0 locales: {sorted(locp.keys())}")

    # dialoguebundle: zh-CN dialogueLanguage (1289 entries)
    dlg = load_text_assets("dialoguebundle")
    dump("dialogue_zh-CN.json", dlg["zh-CN"])
    print(f"dialoguebundle locales: {sorted(dlg.keys())}")


if __name__ == "__main__":
    sys.exit(main())
