# Noun Town — Traditional Chinese (zh-TW)

Adds Traditional Chinese (Taiwan, zh-TW) as a UI/learning language to
*Noun Town Language Learning*, via a [BepInEx](https://github.com/BepInEx/BepInEx)
IL2CPP + Harmony mod:

- A new "Chinese (TW)" entry in the language picker
- Traditional-Chinese item names, localisation strings, and dialogue
  (converted from the game's zh-CN data via OpenCC `s2twp`)
- A fallback CJK font (Noto Sans CJK TC) so Traditional-only glyphs render
  correctly

## Installing

Download the latest release for your OS from the
[Releases page](https://github.com/rholop/noun-town-traditional-chinese/releases)
and extract it anywhere (not into the game folder itself).

The release zips bundle the BepInEx mod loader, the plugin, and the zh-TW
build scripts, but - to avoid redistributing any of the game's own data -
**not** the Traditional-Chinese text itself. The installer builds that
locally, from your own copy of the game, the first time it runs.

### Windows

1. Find your game's install folder (in Steam: right-click *Noun Town
   Language Learning* → Manage → Browse local files).
2. Make sure [Python 3](https://www.python.org/downloads/) is installed,
   with "Add python.exe to PATH" checked during setup.
3. Drag the game folder onto `install.bat` (or run
   `powershell -ExecutionPolicy Bypass -File install.ps1 "C:\path\to\game"`).
4. Launch the game normally - no extra launch options are needed.

See `INSTALL.md` inside the zip for details and troubleshooting.

### Linux (Steam / Proton)

```sh
./install.sh "/path/to/Noun Town Language Learning"
```

Requires `python3`, `pip`, and `bash` (already present on most distros).

Doorstop hooks the game via `winhttp.dll`. Under Proton this DLL override
must be enabled - set this game's Steam launch options to (right click the game and then select properties):

```
WINEDLLOVERRIDES="winhttp=n,b; winepulse.drv=d" %command%
```

See `INSTALL.md` inside the zip for details.

### What the installer does

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

### After a game update

Re-run the installer (`install.sh` on Linux, `install.bat`/`install.ps1` on
Windows) against the updated install. Step 3 rebuilds the zh-TW shadow
bundles from the (possibly changed) game data, and step 4 redeploys the
plugin.

## Uninstalling

Delete `BepInEx/`, `dotnet/`, `doorstop_config.ini`, `.doorstop_version`,
`winhttp.dll`, and `mod/` from the game directory, and (Linux/Proton only)
remove the `WINEDLLOVERRIDES` launch option.

## Troubleshooting

Logs are written to `BepInEx/LogOutput.log` in the game directory.

## Building from source

`mod/plugin/NounTownZhTW/` is the plugin's C# source
(`Plugin.cs` + `NounTownZhTW.csproj`); a prebuilt `NounTownZhTW.dll` is
committed alongside it so the installer works from a fresh clone with no
build step.

To rebuild the plugin and refresh the bundled BepInEx framework
(`mod/bepinex/`) - e.g. after changing the plugin source or updating
BepInEx - run `mod/package/build_package.sh` from a game install that
already has BepInEx 6 IL2CPP set up (so the project can reference its
`BepInEx/core` and `BepInEx/interop` assemblies), then commit the results.

### Releasing

`mod/package/build_release.sh vX.Y.Z` assembles
`NounTownZhTW-vX.Y.Z-windows.zip` and `-linux.zip` under `mod/package/dist/`
from the committed `mod/bepinex/`, plugin DLL, fonts, and scripts - run it
after `build_package.sh`, then publish the zips with
`gh release create vX.Y.Z mod/package/dist/*.zip`.

## License

The NounTownZhTW plugin, build scripts, and installer in this repository are
licensed under the MIT License - see
[`mod/package/LICENSE_NounTownTraditionalChinese.txt`](mod/package/LICENSE_NounTownTraditionalChinese.txt).

This repository also bundles [BepInEx](https://github.com/BepInEx/BepInEx)
(`mod/bepinex/`), the Unity modding framework used to load the plugin.
BepInEx is © the BepInEx Team and is distributed unmodified under the GNU
Lesser General Public License v2.1 - see
[`mod/package/LICENSE_BepInEx.txt`](mod/package/LICENSE_BepInEx.txt).
