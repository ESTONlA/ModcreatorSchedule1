@echo off
REM Batch wrapper for UpdateVersion.ps1
REM Usage: scripts\UpdateVersion.bat [version]
REM Example: scripts\UpdateVersion.bat 1.0.0-beta.3

setlocal
cd /d "%~dp0\.."
powershell.exe -ExecutionPolicy Bypass -File "%~dp0UpdateVersion.ps1" %*
endlocal

