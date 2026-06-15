# Noun Town — Traditional Chinese (zh-TW)

Adds Traditional Chinese (Taiwan, zh-TW) as a UI/learning language to
*Noun Town Language Learning*, via a [BepInEx](https://github.com/BepInEx/BepInEx)
IL2CPP + Harmony mod:

- A new "Chinese (Traditional)" entry in the language picker
- Traditional-Chinese item names, localisation strings, and dialogue
  (converted from the game's zh-CN data via OpenCC `s2twp`)
- A fallback CJK font (Noto Sans CJK TC) so Traditional-only glyphs render
  correctly

## Installing

```sh
./install.sh "/path/to/Noun Town Language Learning"
```

This:

1. Copies the bundled BepInEx 6 IL2CPP + Doorstop runtime
   (`mod/bepinex/`) into the game directory (`BepInEx/core`,
   `BepInEx/unity-libs`, `dotnet/`, `doorstop_config.ini`,
   `.doorstop_version`, `winhttp.dll`).
2. Copies the zh-TW data build scripts to `mod/scripts/` in the game
   directory and installs their Python dependencies (UnityPy,
   opencc-python-reimplemented) via `pip install --user`.
3. Runs the build pipeline **against that game's actual asset bundles**, so
   the generated `zh-TW` TextAssets cover all of that install's content (not
   just the demo).
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

## Building from source

`mod/plugin/NounTownZhTW/` is the plugin's C# source
(`Plugin.cs` + `NounTownZhTW.csproj`); a prebuilt `NounTownZhTW.dll` is
committed alongside it so `install.sh` works from a fresh clone with no
build step.

To rebuild the plugin and refresh the bundled BepInEx framework
(`mod/bepinex/`) - e.g. after changing the plugin source or updating
BepInEx - run `mod/package/build_package.sh` from a game install that
already has BepInEx 6 IL2CPP set up (so the project can reference its
`BepInEx/core` and `BepInEx/interop` assemblies), then commit the results.

## License

The NounTownZhTW plugin, build scripts, and installer in this repository are
licensed under the MIT License - see
[`mod/package/LICENSE_NounTownTraditionalChinese.txt`](mod/package/LICENSE_NounTownTraditionalChinese.txt).

This repository also bundles [BepInEx](https://github.com/BepInEx/BepInEx)
(`mod/bepinex/`), the Unity modding framework used to load the plugin.
BepInEx is © the BepInEx Team and is distributed unmodified under the GNU
Lesser General Public License v2.1 - see
[`mod/package/LICENSE_BepInEx.txt`](mod/package/LICENSE_BepInEx.txt).
