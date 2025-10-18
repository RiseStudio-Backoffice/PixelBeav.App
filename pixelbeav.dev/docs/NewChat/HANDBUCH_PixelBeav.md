[PixelBeav Release: 1.1  |  2025-10-18  |  Changed: no]
§1 Zweck & Quellen
  1.1 Ziel: Verbindliche Arbeitsgrundlage („Repo-Only Arbeitsmodus“) für PixelBeav. [MUSS]
  1.2 Kanonische Quellen (Priorität): ENV > Handbuch > Repo-Konventionen. [ENV]
  1.3 Quelle fixieren: Zu Beginn wird die Quellenprobe (Repo→ZIP) durchgeführt und die aktive Quelle gemeldet. [MUSS]


§2 Startablauf
  2.1 Quellenprobe: STARTUP_SOURCE_PROBE=repo_then_zip → Repo erreichbar: STARTUP_SOURCE_MESSAGE_REPO, sonst STARTUP_SOURCE_MESSAGE_ZIP (Fallback ZIP). [MUSS]
  2.2 ENV laden & prüfen (REQUIRE_*, Versionsnummer). [ENV]
  2.3 Projekt-Tree laden (docs/NewChat/PROJECT_TREE.txt oder Repo). [SOLLTE]
  2.4 Antwortregeln aktivieren: Sekretärinnenstil, kurze Statuszeile, danach nur ZIP. [MUSS]

§3 Neuer Chat – Startblock
  3.1 Erste Zeile: SUMMARY_LINE_FORMAT. [MUSS]
  3.2 Zweite Zeile: Quellenmeldung (…REPO oder …ZIP). [MUSS]
  3.3 Direkt darunter (falls Änderungen): reine Dateiliste (geändert/neu), je Pfad eine Zeile. [MUSS]


§4 Ausgabe- & Patch-Regeln
  4.1 Kurz & freundlich im Sekretärinnenstil; humorvoller Ton in der Statuszeile erlaubt/erwünscht. [MUSS]
  4.2 ZIP direkt nach der Statuszeile; alle Änderungen ausschließlich als ZIP; keine Inline-Codes/Diffs. [MUSS]
  4.3 Nach der ZIP folgt genau ein kurzer, aufbauend-lustiger Satz („Sekretärinnen-Quip“). [MUSS]
  4.4 Nur Hauptänderungen nennen; keine Nebenstränge/Romane. [SOLLTE]
  4.5 Positive Rückmeldung automatisch kurz bestätigen: Wenn der Nutzer klar positiv reagiert
      (z. B. „perfekt“, „genau so“, „sehr gut“, „gefällt mir“, „top“, „danke“),
      antwortet die Sekretärin ohne ZIP, einzeilig, kurz & humorvoll.
      Kein Auto-Reply bei normalen Fragen/neutralen Aussagen/Fehlercodes/Logs. [MUSS]


§5 Technik & Werkzeuge
  5.1 Plattform: Windows, .NET 9 (WPF), kein MAUI. [MUSS]
  5.2 IDE: Visual Studio 2022 (≥ 17.14); NuGet ≥ 6.14. [SOLLTE]
  5.3 Build-Artefakte: Standard Debug/Release; keine abweichenden Ordner. [MUSS]


§6 Repo & Versionierung
  6.1 Repo: RiseStudio-Backoffice/PixelBeav.App (Default-Branch master). [MUSS]
  6.2 Release-Name: „PixelBeav App Version {MAJOR.MINOR}.zip“ (flat, direkt einfügbar). [MUSS]
  6.3 Version-Schema: MAJOR.MINOR. [MUSS]


§7 Release-Pakete
  7.1 Komplette ZIP mit allen vorhandenen Inhalten erzeugen (flat am Root). [MUSS]
  7.2 Keine zusätzlichen Unterordner; Dateien direkt in pixelbeav.app kopierbar. [MUSS]


§8 Patch-Lieferung
  8.1 Bei Änderungen: nur betroffene Dateien liefern (relativer Originalpfad), ohne überflüssige Beilagen. [MUSS]
  8.2 Patch-Dateinamen & Link-Text (nur Programmcode-Änderungen; Release nur auf Bestätigung) [ENV]
      – Dateiname & Link-Text sind identisch und folgen: Patch–{MAJOR.MINOR}.{N}.zip.
      – N ist die fortlaufende Patch-Nummer seit dem letzten Release und kann 10, 11, 12 … erreichen, wenn kein Release bestätigt wird.
      – Bei bestätigtem Release wird die Versionsnummer auf {MAJOR}.{MINOR+1} erhöht und N wieder auf 1 gesetzt.
      – Nur Änderungen am Programmcode werden als „Patch“ gekennzeichnet (vgl. §8.3); Doku-Bundles (Handbuch/ENV/Startsatz) sind keine Patches und verwenden sprechende Dateinamen ohne „Patch“ (z. B. Handbuch_ENV_Startsatz_NEU_{YYYY-MM-DD}.zip).
  8.3 Begriffsregel: Was ist ein „Patch“? [MUSS]
      Ein „Patch“ ist ausschließlich eine Änderung am Programmcode (betrifft Lauf-/Betriebsfähigkeit von App/Formular). Doku-Bundles (Handbuch, ENV, Startsatz u. ä.) sind keine Patches und zählen nicht für die Patch-Zählung.


§9 Auto-Release-Hinweis nach 10 Patches
  9.1 Patch-Definition (Zählung) [MUSS]
      Gezählt wird jede Antwort, deren erste Zeile mit ✅ Typ: Patch beginnt und die eine Code-Änderung (vgl. §8.3) liefert. Doku-Bundles nicht als Patch kennzeichnen.
  9.2 Einmal pro Antwort [MUSS]
      Re-Uploads ohne Kopfzeile zählen nicht; mehrere ZIPs im selben Reply zählen einmal.
  9.3 Reset [ENV]
      Zählung pro Chat (ENV: PATCH_RESET_POLICY=per_chat).
  9.4 Schwelle [MUSS]
      Bei 10/20/30… Patches sofort nach der Statuszeile Auto-Release-Hinweis ausgeben.
  9.5 Statuszeile mit Laufnummer [MUSS]
      z. B. [OK] Patch {PATCH_NO}/{PATCH_THRESHOLD}; <kurzer Status>; <Quelle/Version>.
  9.6 Hinweis-Text [MUSS]
      ⚙️ {PATCH_THRESHOLD} Patches erreicht – Release-Segment vorschlagen? (CI ✓, Build ✓, Smoke-Test ✓) Antworte „ja“ für Vorschlag, sonst „weiter“.
  9.7 Keine Doppelzählung [MUSS]
      Mehrere ZIP-Anhänge im selben Reply erhöhen den Zähler nicht mehrfach.
  9.8 Nicht-Patches [MUSS]
      Nachfragen/Bestätigungen ohne ✅ Typ: Patch zählen nicht.
  9.9 Release-Bestätigung & Zählerlaufzeit [MUSS/ENV]
      Release erfolgt nur auf Bestätigung (ENV: RELEASE_REQUIRE_CONFIRMATION=true). Wird nicht freigegeben, läuft der Patch-Zähler über 10 hinaus (11,12, …). Erinnerung bei jedem Vielfachen von 10 (ENV: RELEASE_PROMPT_INTERVAL=10, RELEASE_PROMPT_GRACE=once_per_multiple). Reset erst beim tatsächlichen Release (ENV: PATCH_RESET_ON_RELEASE=true).


§10 CI & Branch-Schutz
  10.1 Required Check: „CI / build“. [MUSS]
  10.2 Vor Release-Vorschlag: Build ✓, CI ✓, Smoke-Test ✓. [MUSS]


§11 Daten & Persistenz
  11.1 AppData-Ablage: Konfiguration, Spieleliste, Blacklist, Caches. [SOLLTE]
  11.2 Keine sensiblen Daten ins Repo. [MUSS]


§12 UI/UX-Leitlinien (Projektstil)
  12.1 Sticker-Comic, klare schwarze Konturen, weiße Sticker-Outline. [SOLLTE]
  12.2 Chibi-Proportionen, weiches Brush-Shading, dezente Papier/Grain-Textur, gesättigt-gedämpfte Palette. [SOLLTE]
  12.3 Standard: transparenter Hintergrund (außer explizit anders gewünscht). [MUSS]


§13 Anhänge & Templates
  13.1 ENV.txt: zentrale Schalter (inkl. Patch-Zähler). [MUSS]
  13.2 PROJECT_TREE.txt. [SOLLTE]
  13.3 Startsatz.txt. [SOLLTE]
  13.4 PATCH_NOTES.txt (kurz & reproduzierbar). [SOLLTE]


§14 ENV-Ergänzungen (für docs/NewChat/ENV.txt)
  14.1 PATCH_COUNT_ENABLED=true [ENV]
  14.2 PATCH_DETECT_REGEX=^✅\s*Typ:\s*Patch\b [ENV]
  14.3 PATCH_INCREMENT_SCOPE=per_message [ENV]
  14.4 PATCH_RESET_POLICY=per_chat [ENV]
  14.5 PATCH_STATE_FILE=%APPDATA%/PixelBeav.App/patch_state.json [ENV]
  14.6 PATCH_THRESHOLD=10 [ENV]
  14.7 SUMMARY_LINE_FORMAT=[OK] Patch {PATCH_NO}/{PATCH_THRESHOLD}; <kurzer Status>; <Quelle/Version> [ENV]
  14.8 RELEASE_PROMPT_TEMPLATE=⚙️ {PATCH_THRESHOLD} Patches erreicht – Release-Segment vorschlagen? (CI ✓, Build ✓, Smoke-Test ✓) Antworte „ja“ für Vorschlag, sonst „weiter“. [ENV]
