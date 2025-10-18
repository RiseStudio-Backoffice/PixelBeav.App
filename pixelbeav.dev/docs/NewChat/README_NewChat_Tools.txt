[PixelBeav Release: 1.1  |  2025-10-18  |  Changed: no]
NewChat Release-Tools (Docs/NewChat)
====================================

Was ist drin?
- make_release_repo.ps1 / .bat
  Baut eine Full-Release-ZIP **aus dem Repo-Root**. Version aus Docs/NewChat/VERSION.txt.
- make_release_folder.ps1 / .bat
  Packt eine Full-Release-ZIP **aus einem beliebigen Ordner**. Default-Quelle ist:
    E:\LET`S PLAYS\005-BESCHREIBUNG&TITEL\PROGRAMM\PixelBeav.App
  (Kann beim Start geändert werden: PS-Fenster → -Source "C:\Pfad\zum\Ordner")

Artefaktfrei (automatisch ausgeschlossen):
  bin/ obj/ .vs/ .git/ .github/ packages/ PackageCache/ *.user *.suo

Ergebnis:
  <Quelle>\Releases\PixelBeav App Version <VERSION>.zip

Schnellstart:
1) **Ordner-Variante (Windows-Folder):** Doppelklick auf `make_release_folder.bat`
   - oder in PowerShell: `pwsh .\make_release_folder.ps1 -Source "E:\LET`S PLAYS\005-BESCHREIBUNG&TITEL\PROGRAMM\PixelBeav.App" -Version 1.0`
2) **Repo-Variante:** In deinem **Repo-Root** doppelklicken auf `make_release_repo.bat`
   - oder in PowerShell: `pwsh .\make_release_repo.ps1`

Hinweise:
- Wenn Skripte blockiert sind: `Get-ExecutionPolicy` prüfen; temporär: 
  `powershell -ExecutionPolicy Bypass -File .\make_release_repo.ps1`
- `VERSION.txt` muss in Docs/NewChat liegen (oder -Version angeben).
- Die Ordnerquelle **muss** `Docs\NewChat\VERSION.txt` enthalten, falls du -Version nicht setzt.
(Stand: 2025-10-17 13:48)


HINWEIS zu Sonderzeichen im Pfad
--------------------------------
Wenn dein Pfad Zeichen wie **Backtick `** oder **Leerzeichen** enthält, setze -Source in **einzelne Anführungszeichen**:
  pwsh .\make_release_folder.ps1 -Source 'E:\LET`S PLAYS\005-BESCHREIBUNG&TITEL\PROGRAMM\PixelBeav.App'
