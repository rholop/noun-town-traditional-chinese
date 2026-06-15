# NounTownZhTW

Adds Traditional Chinese (Taiwan, zh-TW) as a UI/learning language to
*Noun Town Language Learning*, via a [BepInEx](https://github.com/BepInEx/BepInEx)
IL2CPP + Harmony mod:

- A new "Chinese (Traditional)" entry in the language picker
- Traditional-Chinese item names, localisation strings, and dialogue
  (converted from the game's zh-CN data via OpenCC `s2twp`)
- A fallback CJK font (Noto Sans CJK TC) so Traditional-only glyphs render
  correctly

## Building the package

From the mod's source checkout (this directory's parent repo):

```sh
mod/package/build_package.sh
```

This builds the plugin DLL and stages a self-contained, installable copy at
`mod/package/dist/NounTownZhTW/`. Copy that directory anywhere (including to
a different game install) and run its `install.sh`.

## Installing

```sh
cd mod/package/dist/NounTownZhTW
./install.sh "/path/to/Noun Town Language Learning"
```

`install.sh`:

1. Copies the BepInEx 6 IL2CPP + Doorstop runtime into the game directory
   (`BepInEx/core`, `BepInEx/unity-libs`, `dotnet/`, `doorstop_config.ini`,
   `.doorstop_version`, `winhttp.dll`).
2. Copies the zh-TW data build scripts to `mod/scripts/` in the game
   directory and installs their Python dependencies (UnityPy,
   opencc-python-reimplemented) via `pip install --user`.
3. Re-runs the build pipeline **against that game's actual asset bundles**,
   so the generated `zh-TW` TextAssets cover all of that install's content
   (not just what's in the demo).
4. Copies the plugin DLL, fonts, and generated shadow bundles into
   `BepInEx/plugins/NounTownZhTW/`.

Requires `python3`, `pip`, and `bash`.

### Steam / Proton launch option

Doorstop hooks the game via `winhttp.dll`. Under Proton this DLL override
must be enabled. In Steam, set this game's launch options to:

```
WINEDLLOVERRIDES="winhttp=n,b" %command%
```

### After a game update

Re-run `install.sh` against the updated install. Step 3 rebuilds the zh-TW
shadow bundles from the (possibly changed) game data, and step 4 redeploys
the plugin.

## Uninstalling

Delete `BepInEx/`, `dotnet/`, `doorstop_config.ini`, `.doorstop_version`,
`winhttp.dll`, and `mod/` from the game directory, and remove the
`WINEDLLOVERRIDES` launch option.

## Troubleshooting

Logs are written to `BepInEx/LogOutput.log` in the game directory.

## License

The NounTownZhTW plugin, build scripts, and installer in this package are
licensed under the MIT License - see
[`LICENSE_NounTownTraditionalChinese.txt`](LICENSE_NounTownTraditionalChinese.txt).

This package also bundles [BepInEx](https://github.com/BepInEx/BepInEx), the
Unity modding framework used to load the plugin, in `bepinex/`. BepInEx is
© the BepInEx Team and is distributed unmodified under the GNU Lesser General
Public License v2.1 - see
[`LICENSE_BepInEx.txt`](LICENSE_BepInEx.txt) (also included alongside the
BepInEx files in `bepinex/`).
