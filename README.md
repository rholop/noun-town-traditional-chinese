# Noun Town — Traditional Chinese (zh-TW)

A [BepInEx](https://github.com/BepInEx/BepInEx) IL2CPP + Harmony mod that adds
Traditional Chinese (Taiwan, zh-TW) as a UI/learning language to
*Noun Town Language Learning*.

- Adds a "Chinese (Traditional)" entry to the language picker
- Item names, UI strings, and dialogue converted from the game's zh-CN data
  via OpenCC `s2twp`
- Bundles a fallback CJK font (Noto Sans CJK TC) so Traditional-only glyphs
  render correctly

## Getting started

See [`mod/package/README.md`](mod/package/README.md) for building the plugin,
packaging it, installing it into a copy of the game, and troubleshooting.

## License

- The mod itself (plugin, build scripts, installer) is MIT licensed - see
  [`mod/package/LICENSE_NounTownTraditionalChinese.txt`](mod/package/LICENSE_NounTownTraditionalChinese.txt).
- The installable package bundles [BepInEx](https://github.com/BepInEx/BepInEx),
  licensed under LGPL-2.1 - see
  [`mod/package/LICENSE_BepInEx.txt`](mod/package/LICENSE_BepInEx.txt).
