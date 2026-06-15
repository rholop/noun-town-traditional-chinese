#!/usr/bin/env python3
"""Convert extracted zh-CN locale data to zh-TW (Taiwan) via OpenCC s2twp.

Inputs:  mod/extracted/*.json  (produced by extract_bundles.py)
Outputs: mod/zh-TW/*.json       (mod payload, ready for the Harmony patches)
         mod/zh-TW/itemlanguage_diff_report.json  (vocab-substitution review list)
"""
import json
import os
import sys

from opencc import OpenCC

EXTRACTED_DIR = os.path.join(os.path.dirname(__file__), "..", "extracted")
OUT_DIR = os.path.join(os.path.dirname(__file__), "..", "zh-TW")

cc_twp = OpenCC("s2twp")  # Taiwan standard, with vocabulary substitution
cc_t = OpenCC("s2t")      # glyph-only conversion, used as a baseline for diffing


def load(name):
    with open(os.path.join(EXTRACTED_DIR, name), encoding="utf-8") as f:
        return json.load(f)


def save(name, data):
    out_path = os.path.join(OUT_DIR, name)
    with open(out_path, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
        f.write("\n")
    print(f"wrote {out_path}")


def convert_localisation(in_name, out_name):
    data = load(in_name)
    converted = {
        "localisationJson": [
            {**entry, "Text": cc_twp.convert(entry["Text"])}
            for entry in data["localisationJson"]
        ]
    }
    save(out_name, converted)
    return converted


def convert_itemlanguage():
    data = load("languagedata_zh-CN.json")
    items = data["itemLanguageJson"]

    converted_items = []
    diff_report = []
    for item in items:
        new_item = dict(item)
        vocab_changed_fields = {}
        for field in ("Text", "AcceptableWords"):
            original = item.get(field) or ""
            if not original:
                continue
            twp = cc_twp.convert(original)
            shape_only = cc_t.convert(original)
            new_item[field] = twp
            if twp != shape_only:
                vocab_changed_fields[field] = {
                    "zh-CN": original,
                    "shape_only_s2t": shape_only,
                    "zh-TW_s2twp": twp,
                }
        converted_items.append(new_item)
        if vocab_changed_fields:
            diff_report.append(
                {
                    "Id": item["Id"],
                    "Phonetics": item.get("Phonetics", ""),
                    "changes": vocab_changed_fields,
                }
            )

    save("itemlanguage_zh-TW.json", {"itemLanguageJson": converted_items})
    save("itemlanguage_diff_report.json", diff_report)
    print(f"itemLanguageJson: {len(items)} items, {len(diff_report)} with vocab substitution")


def convert_dialogue():
    data = load("dialogue_zh-CN.json")
    converted = {
        "dialogueLanguage": [
            {**entry, "Text": cc_twp.convert(entry["Text"])}
            for entry in data["dialogueLanguage"]
        ]
    }
    save("dialogue_zh-TW.json", converted)
    print(f"dialogueLanguage: {len(converted['dialogueLanguage'])} entries converted")


def main():
    os.makedirs(OUT_DIR, exist_ok=True)

    loc = convert_localisation("localisation_zh-CN.json", "localisation_zh-TW.json")
    print(f"localisationJson: {len(loc['localisationJson'])} entries converted")

    locp = convert_localisation(
        "localisation_private0_zh-CN.json", "localisation_private0_zh-TW.json"
    )
    print(f"localisationJson (private.0): {len(locp['localisationJson'])} entries converted")

    convert_itemlanguage()
    convert_dialogue()


if __name__ == "__main__":
    sys.exit(main())
