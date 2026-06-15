# Installing NounTownZhTW (Traditional Chinese) - Linux

Requires `bash`, `python3`, and `pip`.

1. Find your game's install folder, e.g.
   `~/.local/share/Steam/steamapps/common/Noun Town Language Learning Demo`.

2. Run the installer:

   ```sh
   ./install.sh "/path/to/Noun Town Language Learning"
   ```

   This will:
   - Copy the BepInEx mod loader into the game folder
   - Install the Python packages needed to build the Traditional Chinese text
     (UnityPy, OpenCC) via `pip install --user`
   - Build the Traditional Chinese text from your own copy of the game's data
   - Install the NounTownZhTW plugin

3. Steam / Proton launch option:

   Doorstop hooks the game via `winhttp.dll`. Under Proton this DLL override
   must be enabled. In Steam, set this game's launch options to:

   ```
   WINEDLLOVERRIDES="winhttp=n,b" %command%
   ```

4. Launch the game. In the game's Settings, choose "Chinese (TW)" as the
   language.

## Troubleshooting

Logs are written to `BepInEx/LogOutput.log` in the game folder.

## Uninstalling

Delete `BepInEx/`, `dotnet/`, `doorstop_config.ini`, `.doorstop_version`,
`winhttp.dll`, and `mod/` from the game folder, and remove the
`WINEDLLOVERRIDES` launch option.
