# Run the full zh-TW data pipeline against the game install this script lives
# under (mod/scripts/../.. is the game directory). PowerShell port of
# build_all.sh for Windows.
#
# Stages:
#   1. extract_bundles.py    - dump zh-CN/reference TextAssets from the game's bundles
#   2. build_languagelist.py - build the 15-locale languagelist_*.json payload
#   3. convert_opencc.py     - OpenCC s2twp conversion of localisation/itemlanguage/dialogue
#   4. build_shadow_bundles.py - assemble mod/shadow/* with the new zh-TW TextAssets
#
# Usage: build_all.ps1 -Python <python launcher command, e.g. @("python") or @("py","-3")>
param(
    [Parameter(Mandatory = $true)]
    [string[]]$Python
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$PythonExtra = if ($Python.Length -gt 1) { $Python[1..($Python.Length - 1)] } else { @() }

foreach ($script in @(
        "extract_bundles.py",
        "build_languagelist.py",
        "convert_opencc.py",
        "build_shadow_bundles.py"
    )) {
    & $Python[0] @PythonExtra (Join-Path $ScriptDir $script)
    if ($LASTEXITCODE -ne 0) {
        throw "$script failed with exit code $LASTEXITCODE"
    }
}
