@echo off
setlocal enabledelayedexpansion
echo [Build] Restore...
dotnet restore PixelBeavLibrary.sln || goto :err

echo [Build] Compile (Release)...
dotnet build PixelBeavLibrary.sln -c Release --no-restore || goto :err

echo [OK] Build completed.
exit /b 0

:err
echo [ERROR] Build failed.
exit /b 1
