#!/usr/bin/env python3
"""Build modified copies ("shadow bundles") of the AssetBundles that
need a zh-TW TextAsset added for Noun Town Language Learning.

For each of languagelistbundle, localisationbundle, languagedatabundle, and
dialoguebundle, this script:
  - loads the original bundle from the game's StreamingAssets directory
  - adds a new "zh-TW" TextAsset (content from mod/zh-TW/*.json)
  - appends the corresponding m_PreloadTable/m_Container entries on the
    bundle's AssetBundle object
  - for languagelistbundle, also replaces the content of the 14 existing
    locale TextAssets with their updated (zh-TW-aware) 15-entry arrays
  - writes the result to mod/shadow/<bundlename>

The originals under StreamingAssets are never modified.
"""
import copy
import json
import os

import UnityPy

from gamedir import find_streaming_assets

ROOT = os.path.join(os.path.dirname(__file__), "..", "..")
STREAMING = find_streaming_assets(ROOT)
ZHTW_DIR = os.path.join(os.path.dirname(__file__), "..", "zh-TW")
OUT_DIR = os.path.join(os.path.dirname(__file__), "..", "shadow")

# Arbitrary path_ids for the new TextAsset objects, chosen to be far outside
# the range of hash-derived path_ids already present in each SerializedFile.
NEW_PATH_IDS = {
    "languagelistbundle": 7000000000000000001,
    "localisationbundle": 7000000000000000002,
    "languagedatabundle": 7000000000000000003,
    "dialoguebundle": 7000000000000000004,
}

# Locale codes whose languagelistbundle TextAsset content must be replaced
# with the updated (zh-TW-aware, 15-entry) array.
LANGUAGELIST_LOCALES = [
    "ar-EG", "br-BZ", "de-DE", "el-GR", "en-US", "es-ES", "es-MX",
    "fr-FR", "it-IT", "jp-JP", "ko-KR", "ru-RU", "uk-UA", "zh-CN",
]


def load_json_text(name):
    with open(os.path.join(ZHTW_DIR, name), encoding="utf-8") as f:
        return f.read()


def find_asset_bundle(sf):
    for pid, obj in sf.objects.items():
        if obj.type.name == "AssetBundle":
            return pid, obj
    raise RuntimeError("no AssetBundle object found")


def add_text_asset(sf, ab_obj, name, content, container_path, new_pid):
    # Clone an existing TextAsset's ObjectReader as a template.
    template = None
    for obj in sf.objects.values():
        if obj.type.name == "TextAsset":
            template = obj
            break
    if template is None:
        raise RuntimeError("no TextAsset template found")

    new_reader = copy.copy(template)
    new_reader.path_id = new_pid
    data = new_reader.read()
    data.m_Name = name
    data.m_Script = content
    data.save()
    sf.objects[new_pid] = new_reader

    tree = ab_obj.read_typetree()
    tree["m_PreloadTable"].append({"m_FileID": 0, "m_PathID": new_pid})
    new_index = len(tree["m_PreloadTable"]) - 1
    tree["m_Container"].append([
        container_path,
        {
            "preloadIndex": new_index,
            "preloadSize": 1,
            "asset": {"m_FileID": 0, "m_PathID": new_pid},
        },
    ])
    ab_obj.save_typetree(tree)


def build_languagelistbundle():
    env = UnityPy.load(os.path.join(STREAMING, "languagelistbundle"))
    for sf in env.file.files.values():
        # Replace content of the 14 existing locale TextAssets.
        for obj in sf.objects.values():
            if obj.type.name == "TextAsset":
                d = obj.read()
                if d.m_Name in LANGUAGELIST_LOCALES:
                    d.m_Script = load_json_text(f"languagelist_{d.m_Name}.json")
                    d.save()

        _, ab_obj = find_asset_bundle(sf)
        add_text_asset(
            sf, ab_obj, "zh-TW",
            load_json_text("languagelist_zh-TW.json"),
            "assets/bundledassets/json/languagelistbundle/zh-tw.json",
            NEW_PATH_IDS["languagelistbundle"],
        )

    out_path = os.path.join(OUT_DIR, "languagelistbundle")
    with open(out_path, "wb") as f:
        f.write(env.file.save())
    print(f"wrote {out_path}")


def build_localisationbundle():
    env = UnityPy.load(os.path.join(STREAMING, "localisationbundle"))
    for sf in env.file.files.values():
        _, ab_obj = find_asset_bundle(sf)
        add_text_asset(
            sf, ab_obj, "zh-TW",
            load_json_text("localisation_zh-TW.json"),
            "assets/bundledassets/json/localisationbundle/zh-tw.json",
            NEW_PATH_IDS["localisationbundle"],
        )

    out_path = os.path.join(OUT_DIR, "localisationbundle")
    with open(out_path, "wb") as f:
        f.write(env.file.save())
    print(f"wrote {out_path}")


def build_languagedatabundle():
    env = UnityPy.load(os.path.join(STREAMING, "languagedatabundle"))
    for sf in env.file.files.values():
        _, ab_obj = find_asset_bundle(sf)
        add_text_asset(
            sf, ab_obj, "zh-TW",
            load_json_text("itemlanguage_zh-TW.json"),
            "assets/bundledassets/json/languagedatabundle/zh-tw.json",
            NEW_PATH_IDS["languagedatabundle"],
        )

    out_path = os.path.join(OUT_DIR, "languagedatabundle")
    with open(out_path, "wb") as f:
        f.write(env.file.save())
    print(f"wrote {out_path}")


def build_dialoguebundle():
    env = UnityPy.load(os.path.join(STREAMING, "dialoguebundle"))
    for sf in env.file.files.values():
        _, ab_obj = find_asset_bundle(sf)
        add_text_asset(
            sf, ab_obj, "zh-TW",
            load_json_text("dialogue_zh-TW.json"),
            "assets/bundledassets/json/dialoguebundle/zh-tw.json",
            NEW_PATH_IDS["dialoguebundle"],
        )

    out_path = os.path.join(OUT_DIR, "dialoguebundle")
    with open(out_path, "wb") as f:
        f.write(env.file.save())
    print(f"wrote {out_path}")


def main():
    os.makedirs(OUT_DIR, exist_ok=True)
    build_languagelistbundle()
    build_localisationbundle()
    build_languagedatabundle()
    build_dialoguebundle()


if __name__ == "__main__":
    main()
