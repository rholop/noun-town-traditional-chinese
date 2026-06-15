# Install the NounTownZhTW (Traditional Chinese) mod into a Noun Town
# Language Learning install. PowerShell port of install.sh for Windows.
#
# Usage: powershell -ExecutionPolicy Bypass -File install.ps1 "C:\path\to\Noun Town Language Learning"
param(
    [Parameter(Position = 0)]
    [string]$GameDir
)

$ErrorActionPreference = "Stop"

$RepoDir = Split-Path -Parent $MyInvocation.MyCommand.Path

if (-not $GameDir) {
    $GameDir = Read-Host "Path to your 'Noun Town Language Learning' install directory"
}
if (-not $GameDir) {
    throw "Usage: install.ps1 <path to Noun Town Language Learning install directory>"
}

$GameDir = (Resolve-Path -LiteralPath $GameDir).ProviderPath

# --- Sanity checks ---
$exeFound = Get-ChildItem -LiteralPath $GameDir -Filter "*.exe" -File -ErrorAction SilentlyContinue
if (-not $exeFound -or -not (Test-Path (Join-Path $GameDir "GameAssembly.dll"))) {
    throw "error: $GameDir does not look like a Unity IL2CPP game install (missing *.exe / GameAssembly.dll)"
}

$dataDirs = @(Get-ChildItem -LiteralPath $GameDir -Directory -Filter "*_Data")
if ($dataDirs.Count -ne 1) {
    throw "error: expected exactly one *_Data directory in $GameDir, found: $($dataDirs.Name -join ', ')"
}
$dataDir = $dataDirs[0].FullName

$dialogueBundle = Join-Path $dataDir "StreamingAssets/windows/dialoguebundle"
if (-not (Test-Path $dialogueBundle)) {
    throw "error: $dialogueBundle not found - is this Noun Town Language Learning?"
}

Write-Host "==> Target: $GameDir"

# --- 1. BepInEx framework (Doorstop + BepInEx 6 IL2CPP runtime) ---
Write-Host "==> Installing BepInEx framework"
Copy-Item -Path (Join-Path $RepoDir "mod/bepinex/*") -Destination $GameDir -Recurse -Force

# --- 2. Build scripts + dependencies ---
Write-Host "==> Installing zh-TW build scripts"
$scriptsDir = Join-Path $GameDir "mod/scripts"
New-Item -ItemType Directory -Force -Path $scriptsDir | Out-Null
Copy-Item -Path (Join-Path $RepoDir "mod/scripts/*") -Destination $scriptsDir -Recurse -Force

Write-Host "==> Locating Python"
$python = $null
foreach ($candidate in @(@("py", "-3"), @("python"), @("python3"))) {
    if (-not (Get-Command $candidate[0] -ErrorAction SilentlyContinue)) {
        continue
    }
    $extra = if ($candidate.Length -gt 1) { $candidate[1..($candidate.Length - 1)] } else { @() }
    & $candidate[0] @extra --version *>$null
    if ($LASTEXITCODE -eq 0) {
        $python = $candidate
        break
    }
}
if (-not $python) {
    throw "error: Python 3 not found. Install it from https://www.python.org/downloads/ (check 'Add python.exe to PATH' during setup), then re-run this script."
}
$pythonExtra = if ($python.Length -gt 1) { $python[1..($python.Length - 1)] } else { @() }

Write-Host "==> Checking Python dependencies (UnityPy, opencc-python-reimplemented)"
& $python[0] @pythonExtra -m pip install --user -q -r (Join-Path $scriptsDir "requirements.txt")
if ($LASTEXITCODE -ne 0) { throw "pip install failed with exit code $LASTEXITCODE" }

# --- 3. Build zh-TW shadow bundles from THIS game's actual bundles ---
Write-Host "==> Building zh-TW shadow bundles"
& (Join-Path $scriptsDir "build_all.ps1") -Python $python
if ($LASTEXITCODE -ne 0) { throw "build_all.ps1 failed with exit code $LASTEXITCODE" }

# --- 4. Deploy the plugin ---
Write-Host "==> Installing NounTownZhTW plugin"
$pluginDir = Join-Path $GameDir "BepInEx/plugins/NounTownZhTW"
New-Item -ItemType Directory -Force -Path (Join-Path $pluginDir "fonts") | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $pluginDir "shadow") | Out-Null
Copy-Item -Path (Join-Path $RepoDir "mod/plugin/NounTownZhTW/NounTownZhTW.dll") -Destination $pluginDir -Force
Copy-Item -Path (Join-Path $RepoDir "mod/fonts/*") -Destination (Join-Path $pluginDir "fonts") -Recurse -Force
Copy-Item -Path (Join-Path $GameDir "mod/shadow/*") -Destination (Join-Path $pluginDir "shadow") -Recurse -Force

Write-Host "==> Done."
Write-Host ""
Write-Host "Launch the game normally - no extra launch options are needed on Windows."
