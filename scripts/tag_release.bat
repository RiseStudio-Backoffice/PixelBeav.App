@echo off
setlocal
if "%~1"=="" (
  echo Usage: tag_release.bat ^<version^>
  echo Example: tag_release.bat 1.16
  exit /b 1
)
set VER=%~1
git tag v%VER%
git push origin v%VER%
echo Created and pushed tag v%VER%.
