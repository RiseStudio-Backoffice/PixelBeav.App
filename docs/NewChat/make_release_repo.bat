@echo off
REM Baut eine Full-Release-ZIP aus dem Repo-Root
powershell -ExecutionPolicy Bypass -File "%~dp0make_release_repo.ps1"
pause
