# Installing NounTownZhTW (Traditional Chinese) - Windows

1. Find your game's install folder:
   - In Steam, right-click "Noun Town Language Learning" (or "...Demo") ->
     Manage -> Browse local files.
   - The folder should contain "Noun Town Language Learning....exe" and
     "GameAssembly.dll".

2. Make sure Python 3 is installed:
   - Download it from https://www.python.org/downloads/ and run the installer.
   - On the first page of the installer, check "Add python.exe to PATH"
     before clicking Install.
   - If you already have Python 3, you can skip this step.

3. Run the installer:
   - Drag the game folder from step 1 onto `install.bat`, **or**
   - Open PowerShell in this folder and run:
     `powershell -ExecutionPolicy Bypass -File install.ps1 "C:\path\to\game folder"`

   The installer will:
   - Copy the BepInEx mod loader into the game folder
   - Install the Python packages needed to build the Traditional Chinese text
     (UnityPy, OpenCC)
   - Build the Traditional Chinese text from your own copy of the game's data
   - Install the NounTownZhTW plugin

4. Launch the game normally from Steam. In the game's Settings, choose
   "Chinese (TW)" as the language.

No extra Steam launch options are needed on Windows.

## Troubleshooting

- If Windows shows a security warning ("Windows protected your PC") when
  running `install.bat` or `install.ps1`, click "More info" -> "Run anyway"
  (the scripts are unsigned but contain no compiled code).
- Logs are written to `BepInEx/LogOutput.log` in the game folder.

## Uninstalling

Delete `BepInEx/`, `dotnet/`, `doorstop_config.ini`, `.doorstop_version`,
`winhttp.dll`, and `mod/` from the game folder.
