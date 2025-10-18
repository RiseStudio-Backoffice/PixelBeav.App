[PixelBeav Release: 1.1  |  2025-10-18  |  Changed: no]

PixelBeav – Patch 1.0.3 (Steam-Scan Recovery Guide)
===================================================

Problem
-------
Nach "Alle Thumbnails löschen" lädt der Steam-Scan keine neuen Thumbnails mehr. Ursache ist sehr wahrscheinlich,
dass der Scan/Download derzeit synchron im UI-Thread blockiert (GetAwaiter().GetResult), wodurch Netzwerkzugriffe
oder Fehler (Timeouts) keine neuen Einträge liefern. Zusätzlich fehlt für leere Bilder ein belastbarer Fallback.

Ziel
----
1) Scan + Anreicherung **asynchron** durchführen, UI nicht blockieren.
2) **Nur installierte** Spiele berücksichtigen.
3) **Keine Duplikate** (SteamAppId > ExecutablePath > Title).
4) **Deutsch** (Kurzbeschreibung) und **Header-Bild** aus dem Store.
5) **Fallback**-Header-URL, wenn Store nichts liefert.
6) Danach Thumbnail-Erzeugung starten.

To-Do (kurz)
------------
1) `MainViewModel`:
   - `ScanSteamCommand = new RelayCommand(async _ => await ScanSteamAndPersistAsync());`
   - Neue Methode `private async Task ScanSteamAndPersistAsync()` implementieren:
     - `await Task.Run(async () => { ... FetchStoreMetaAsync(..., "de") ... EnsureAndGenerateAsync(...) ... });`
     - Danach auf dem UI-Thread `StorageService.SaveGames(...)`, `AllGames.Clear(); foreach ...`, `FilteredGames.Refresh()`.

2) Fallback-URL:
   - Wenn `g.HeaderImageUri` leer ist und `g.SteamAppId` gesetzt: 
     `g.HeaderImageUri = $"https://cdn.akamai.steamstatic.com/steam/apps/{g.SteamAppId}/header.jpg";`

3) Filter "nur installiert":
   - Behalte nur Spiele mit existierender `ExecutablePath` **oder** existierendem `FolderPath`.

4) Deduplizieren:
   - Gruppiere per Schlüssel: `id:{SteamAppId}` > `exe:{ExecutablePath}` > `title:{Title}` und nimm jeweils das erste Element.

5) Thumbnails erzeugen:
   - Für **jedes** verbleibende Spiel `await ThumbnailService.EnsureAndGenerateAsync(g)` aufrufen.

6) Test:
   - App starten → **Steam Scan** ausführen.
   - Erwartung: Thumbnails werden wieder erzeugt, Texte deutsch, keine Duplikate, nur installierte Titel.

Hinweis
------
Falls Netzwerk gesperrt ist oder Steam-API kein Ergebnis liefert, greift die Fallback-Header-URL.
Die Thumbnail-Erzeugung setzt dann zumindest ein gültiges Bild an, ansonsten Platzhalter.

