:: PixelBeav Release: 1.1  |  2025-10-18  |  Changed: no
@echo off
REM Packt eine Full-Release-ZIP aus einem beliebigen Ordner (Default ist in der PS1 gesetzt)
powershell -ExecutionPolicy Bypass -File "%~dp0make_release_folder.ps1"
pause
