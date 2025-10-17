
**Lies das Handbuch in der von mir bereitgestellten Datei. Falls nicht vorhanden, erinnere mich daran, sie dir zu geben.**
**Gib mir zu Chat-Beginn die aktuelle Projekt-ZIP (artefaktfrei) und dieses Handbuch; ich bestätige mit „Handbuch vollständig gelesen. Alle Befehle sind umgesetzt.“ und liefere dann ausschließlich ZIP-Patches.**
**Sanity-Check (Repo):** Nach Link prüfe ich, ob `PixelBeav.App/Views/MainWindow.xaml` **direkt aus dem Repo** lesbar ist (HTTP 200, nicht leer, XAML-Header). Probleme →
`[FEHLER] MainWindow.xaml nicht lesbar – Ursache: <kurz> | Aktion: <kurz>`
Repo: `https://github.com/RiseStudio-Backoffice/PixelBeav.App`

**Startsatz (Chat-Titel) liegt separat in**: `Docs/NewChat/Startsatz.txt`.

**Automatisch mit eingelesene Begleitdateien (Reihenfolge, falls vorhanden):**
- `Docs/NewChat/VERSION.txt`  (Fallback: `Docs/VERSION.txt` → `VERSION.txt`)
- `Docs/NewChat/PATCH_NOTES.txt`  (Fallback: `Docs/PATCH_NOTES.txt` → `PATCH_NOTES.txt`)
- `Docs/NewChat/PROJECT_TREE.txt`  (Fallback: `Docs/PROJECT_TREE.txt` → `PROJECT_TREE.txt`)
- `Docs/NewChat/ENV.txt`  (Fallback: `Docs/ENV.txt` → `ENV.txt`)

---

# A) Kommunikation, Meldungen & Projektkontext

## A1. Kommunikationsprinzipien
- Deutsch, **kurz & präzise**; keine Prosa (nur auf Wunsch).
- **Keine Inline-Codes/Zeilenfixes** – **alle Änderungen nur als ZIP**.
- Unklarheiten: fehlende Datei/Info **knapp benennen**; **kein** „bitte warten“.
- **Neuer Chat – erwartet:** `PATCH_NOTES.txt`, `PROJECT_TREE.txt`, `VERSION.txt` (z. B. `1.0`), `ENV.txt`. Fehlt etwas → **kurz nachfragen**.

## A2. Meldungen & Symbolsprache
- Bestätigung nach Lektüre: **„Handbuch vollständig gelesen. Alle Befehle sind umgesetzt.“**
- Lieferübersicht: `Ersetzen: <Pfad/Datei>` / `Hinzufügen: <Pfad/Datei>`
- **Ton bei Programmänderungen:** **„witzige Sekretärin“** – kurz, charmant, 2–3 Sätze, kein Slang.
- Tags: **[OK]**, **[HINWEIS]**, **[WARNUNG]**, **[FEHLER]**
- Standardblock (max. 3 Zeilen):
  ```
  [WARNUNG|FEHLER] Kurzbeschreibung
  Datei: <Pfad/Datei>    Code/ID: <optional>
  Ursache: <1 Zeile> | Aktion: <1 Zeile>
  ```
- **Manifest im ZIP (Pflicht):** `PATCH_NOTES.txt` – Zeile: `Ersetzen|Hinzufügen ; <Pfad/Datei>`
- Zusätzlich: `PROJECT_TREE.txt` (artefaktfrei), optional `CHECKSUMS.txt` (SHA-256)

## A3. Projektkontext & Metadaten
- **Projektname:** *Beschreibungstexte für YouTube (PixelBeav)*.
- **Ziel:** Spiele verwalten/auswählen, **deutsche** Beschreibungen anzeigen (Steam/weitere Quellen), daraus **Prompts** erzeugen, um **YouTube-Beschreibungstexte** zu erstellen; später Prompts für **Titel** & **Thumbnails**.
- **Profile/Personas (mehrbenutzerfähig):** Name, Stil-Tags (z. B. humorvoll/ernst), Tonfall-Leitworte, Du/Sie; **aktives Profil** fließt automatisch in Prompts ein.
- **Persistenz-Grundsätze:** Alle **Beschreibungen, Titel, Prompt-Varianten** sowie **Bildmaterialien** werden gespeichert; klare Ordner/Dateinamen; Konfig/Listen als **JSON (UTF-8 ohne BOM)**.

---

# B) Lieferformat, Namensschema & Disziplin

## B1. ZIP-only
- Änderungen **nur als ZIP**, **identische Projektstruktur ab Root**, **flach** (kein Extra-Oberordner).
- Pro Datei: **Ersetzen** oder **Hinzufügen**.
- **Ausschlüsse:** `bin/`, `obj/`, `.vs/`, `.git*`, `.github/`, `packages/`, `PackageCache/`.

## B2. Namensschema
- Namespaces: `PixelBeav.App.<Bereich>`; C#-Klassen/Dateien: **PascalCase**; Assets: **kebab/snake_case**; Docs: **PascalCase**/**UPPER_SNAKE**.
- **Patch-ZIP:** `pixelbeav_patch_<kurzthema>_<YYYYMMDD-HHMM>.zip`
- **Full-Release:** `PixelBeav App Version {MAJOR.MINOR}.zip` (Minor **0..9**; bei **10** → Minor=0 & Major+1).

## B3. Dokument-/Dateiformate (Teil-Update)
- Patch-ZIP: nur geänderte/neue Dateien; **UTF-8 ohne BOM**.
- Beilagen: `PATCH_NOTES.txt`, `PROJECT_TREE.txt`, optional `CHECKSUMS.txt`.
- **App-Daten-Snapshot (optional bei Patch, Pflicht bei Full-Release – separat):** `PixelBeav_AppData_JSON.zip` – **nur JSON** aus `%AppData%/PixelBeav.App/`
  (`profiles.json`, `games.json`, `blacklist.json`, `config.json`, **`titles/{gameId}.json`**, **`descriptions/{gameId}.json`**, `yt_presets.json`).

## B4. Voll-Release (Komplett-Update)
- **Turnus:** Nach **10** Patches **oder** auf Anforderung.
- **Name:** `PixelBeav App Version {MAJOR.MINOR}.zip` (Regel s. oben).
- **Inhalt (komplett, artefaktfrei):** `Docs/` (inkl. Handbuch), `PixelBeav.App/` (ohne Artefakte), `.editorconfig`, `README.md`/`LICENSE` falls vorhanden.
- **Beilagen:** `RELEASE_NOTES.txt` (kurz), `PROJECT_TREE.txt`, optional `CHECKSUMS.txt`.
- **Zusätzlich (separater Download, Pflicht bei Full-Release):** **`PixelBeav_AppData_JSON.zip`** (neuester App-Daten-Snapshot).

---

# C) Code- & Projektkonventionen

## C1. Encoding & Zeilenenden
- **Alle Dateien:** **UTF-8 ohne BOM**; `.editorconfig`: `charset = utf-8`, `end_of_line = crlf`.
- Umlaut/XAML-Probleme: BOM/ZWSP entfernen; `bin/` & `obj/` löschen; Rebuild.
- VS-Standard für neue Dateien: UTF-8 (ohne Signatur).

## C2. Lokalisierung
- UI & Inhalte **Deutsch**; externe Quellen möglichst **deutschsprachig**.

## C3. Persistenz/Dateiformate
- JSON **UTF-8** (ohne BOM); Lesen tolerant nur für Altbestände.
- **App-Datenbasis (Laufzeit/Schreibort):** `%AppData%/PixelBeav.App/`.
- **Vorlagen im Repository:** `Data/` – nur Templates/Beispieldaten; beim **ersten Start** von dort nach `%AppData%/PixelBeav.App/` kopieren, falls leer.

## C4. Datenlayout (Dateien & Ordner)
- **games.json** – … (aktuelle de-Beschreibungen).
- **titles/** – `titles/{gameId}.json` (append-only).
- **descriptions/** – `descriptions/{gameId}.json` (Historie; Archiv-Trigger beim Entfernen aus Thumbs).
- **profiles.json**, **config.json**, **yt_presets.json**, **blacklist.json**, **imports/** …

## C5. WPF/XAML-Leitplanken
- `DataContext` für `MainWindow` **im Code-Behind**; Typen bei Ambiguität vollqualifizieren; keine nicht existenten Properties.

## C6. Umgang mit bestehenden Dateien
- Immer auf zuletzt gelieferter Originalbasis; keine Duplikate/Umbenennungen; keine „Ersatz-Klone“; API-Brüche nur nach Freigabe.

---

# E) Versionspflege & Auto-Einlesen

## E1. Version anpassen bei Full-Release
- Beim Full-Release wird die Versionsnummer **übernommen und überall aktualisiert**:
  - **Startsatz**: `Docs/NewChat/Startsatz.txt` → `Projekt PixelBeav - Version {MAJOR.MINOR}`
  - **VERSION.txt**: `Docs/NewChat/VERSION.txt` → nur `{MAJOR.MINOR}`
  - **Release-Name**: `PixelBeav App Version {MAJOR.MINOR}.zip`

## E2. Auto-Einlesen von Begleitdateien
- Reihenfolge: **Docs/NewChat/** → **Docs/** → **Repo-Root**
  - `VERSION.txt`, `PATCH_NOTES.txt`, `PROJECT_TREE.txt`, `ENV.txt`
- Fehlt etwas, **kurz nachfragen**.

---

# Anhang

## Projektstruktur (artefaktfrei, Auszug)
```
PixelBeav (Repo Root)/
├─ Docs/
│  └─ NewChat/
│     ├─ Startsatz.txt
│     ├─ VERSION.txt
│     ├─ ENV.txt
│     ├─ PATCH_NOTES.txt
│     ├─ PROJECT_TREE.txt
│     └─ HANDBUCH_PixelBeav.md
├─ Data/
│  └─ (JSON-Vorlagen & Ordner)
├─ PixelBeav.App/
│  └─ ...
└─ README.md
```
