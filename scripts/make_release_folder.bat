@echo off
setlocal ENABLEDELAYEDEXPANSION
REM PixelBeav Patch: PixelPatch-1.1.36 | 2025-10-18 | Changed: yes
REM Ziel: Release-Ordner **eine Ebene über** pixelbeav.dev (im Repo-Root). Dateiname: Release.<versionsnummer>.zip

REM 1) Script-Verzeichnis
set "SCRIPT_DIR=%~dp0"

REM 2) Repo-Root finden (Ordner mit PixelBeav.sln)
set "CUR=%SCRIPT_DIR%"
set "MAX=10"
:find_root
if exist "%CUR%PixelBeav.sln" goto found_root
set "CUR=%CUR%..\"
set /a MAX-=1
if %MAX% LEQ 0 (
  REM Fallback: eine Ebene über pixelbeav.dev
  set "CUR=%SCRIPT_DIR%..\"
  goto found_root
)
goto find_root

:found_root
for %%I in ("%CUR%") do set "REPO_ROOT=%%~fI"
set "DEV_DIR=%REPO_ROOT%\pixelbeav.dev"
set "RELEASE_DIR=%REPO_ROOT%\Release"

if not exist "%RELEASE_DIR%" mkdir "%RELEASE_DIR%"

echo Repo-Root  : "%REPO_ROOT%"
echo Projekt    : "%DEV_DIR%"
echo Release dir: "%RELEASE_DIR%"

REM 3) Version lesen und ZIP im Release-Ordner erzeugen (ohne Wrapper, Inhalte direkt aus pixelbeav.dev)
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$root = Resolve-Path '%REPO_ROOT%'; $dev = Join-Path $root 'pixelbeav.dev';" ^
  "$verFile = Join-Path $dev 'docs\newchat\version.txt';" ^
  "$kv = @{}; if (Test-Path $verFile) { Get-Content $verFile | Where-Object {$_ -match '='} | ForEach-Object { $p=$_.Split('='); if($p.Length -ge 2){ $kv[$p[0].Trim()]=$p[1].Trim() } } }" ^
  "$rel = $kv['release']; $pat = $kv['patch']; if(-not $rel){$rel='1.1'}; if(-not $pat){$pat='36'};" ^
  "$ver = '{0}.{1}' -f $rel,$pat;" ^
  "$zip = Join-Path '%RELEASE_DIR%' ('Release.{0}.zip' -f $ver);" ^
  "if (Test-Path $zip) { Remove-Item $zip -Force };" ^
  "Compress-Archive -Path (Join-Path $dev '*') -DestinationPath $zip -Force;" ^
  "Write-Host ('Erstellt: ' + $zip)"

endlocal
