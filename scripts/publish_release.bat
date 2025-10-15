@echo off
setlocal enabledelayedexpansion
if "%~1"=="" (
  echo Usage: publish_release.bat ^<version^>
  echo Example: publish_release.bat 1.16
  exit /b 1
)
set VER=%~1
set OUT=publish\win-x64
set ZIP=PixelBeav_%VER%.zip

echo [Publish] .NET 9 win-x64 (framework-dependent)...
dotnet publish PixelBeav.App\PixelBeav.App.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=false -o "%OUT%" || goto :err

echo [Zip] %ZIP%
powershell -NoLogo -NoProfile -Command "if (Test-Path '%ZIP%') { Remove-Item '%ZIP%' }; Add-Type -A System.IO.Compression.FileSystem; [IO.Compression.ZipFile]::CreateFromDirectory('%OUT%', '%ZIP%')"

echo [OK] Publish completed: %ZIP%
exit /b 0

:err
echo [ERROR] Publish failed.
exit /b 1
