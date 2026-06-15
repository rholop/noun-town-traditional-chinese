#!/usr/bin/env python3
"""Build the 15-locale languagelist_*.json payload for zh-TW.

For each of the 14 existing UI-locale arrays, append a new Iid 14 / "zh-TW"
entry (Chinese (Traditional)) with a translated "Name".

Also build the 15th locale array itself (zh-TW UI), which is the s2twp
conversion of zh-CN's array plus the zh-TW self-entry.
"""
import json
import os

from opencc import OpenCC

EXTRACTED_DIR = os.path.join(os.path.dirname(__file__), "..", "extracted")
OUT_DIR = os.path.join(os.path.dirname(__file__), "..", "zh-TW")

cc_twp = OpenCC("s2twp")

# Translated "Name" for the new zh-TW / "Chinese (Traditional)" entry, keyed by
# UI locale. Style follows each locale's existing Name for zh-CN (Iid 0) and the
# "(MX)"/"(BR)" parenthetical-qualifier pattern used elsewhere in the same file.
ZH_TW_NAME = {
    "zh-CN": "中文（繁體）",
    "en-US": "Chinese (TW)",
    "fr-FR": "Chinois (traditionnel)",
    "de-DE": "Chinesisch (Traditionell)",
    "it-IT": "Cinese (tradizionale)",
    "jp-JP": "中国語（繁体字）",
    "es-ES": "Chino (tradicional)",
    "ko-KR": "중국어 (번체)",
    "uk-UA": "Китайська (традиційна)",
    "ru-RU": "Китайский (традиционный)",
    "es-MX": "Chino (Tradicional)",
    "el-GR": "Κινεζικά (Παραδοσιακά)",
    "ar-EG": "الصينية (التقليدية)",
    "br-BZ": "Chinês (Tradicional)",
}

LOCALES = list(ZH_TW_NAME.keys())

# Disambiguate the existing zh-CN entry's "Name" now that there are two
# Chinese variants in the picker (only en-US requested so far; other
# locales keep their existing translated "Name" for zh-CN).
ZH_CN_NAME_OVERRIDE = {
    "en-US": "Chinese (CN)",
}


def load(name):
    with open(os.path.join(EXTRACTED_DIR, name), encoding="utf-8") as f:
        return json.load(f)


def save(name, data):
    out_path = os.path.join(OUT_DIR, name)
    with open(out_path, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
        f.write("\n")
    print(f"wrote {out_path}")


def main():
    os.makedirs(OUT_DIR, exist_ok=True)

    # 1. Append the new zh-TW / Iid 14 entry to each of the 14 existing locale arrays.
    for locale in LOCALES:
        data = load(f"languagelist_{locale}.json")

        if locale in ZH_CN_NAME_OVERRIDE:
            for entry in data["languageJson"]:
                if entry["Id"] == "zh-CN":
                    entry["Name"] = ZH_CN_NAME_OVERRIDE[locale]

        data["languageJson"].append(
            {
                "Id": "zh-TW",
                "Iid": 14,
                "EditorName": "Chinese (Traditional)",
                "Name": ZH_TW_NAME[locale],
            }
        )
        save(f"languagelist_{locale}.json", data)

    # 2. Build the 15th locale array: zh-TW UI = s2twp(zh-CN's array) + self-entry.
    zh_cn = load("languagelist_zh-CN.json")
    zh_tw_entries = [
        {**entry, "Name": cc_twp.convert(entry["Name"])}
        for entry in zh_cn["languageJson"]
    ]
    zh_tw_entries.append(
        {
            "Id": "zh-TW",
            "Iid": 14,
            "EditorName": "Chinese (Traditional)",
            "Name": cc_twp.convert(ZH_TW_NAME["zh-CN"]),
        }
    )
    save("languagelist_zh-TW.json", {"languageJson": zh_tw_entries})


if __name__ == "__main__":
    main()
