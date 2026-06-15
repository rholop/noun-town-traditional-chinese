@echo off
rem Drag your "Noun Town Language Learning" game folder onto this file, or
rem double-click it and paste the path when prompted.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0install.ps1" "%~1"
echo.
pause
