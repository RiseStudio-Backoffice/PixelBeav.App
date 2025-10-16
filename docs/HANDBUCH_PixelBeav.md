Start: Lies das Handbuch in der von mir bereitgestellten Datei. Falls nicht vorhanden, erinnere mich daran, es dir zu geben.
Arbeitsmodus: **ZIP‑Only** – Dateien werden von dir als ZIP/Uploads bereitgestellt und hier eingelesen; **kein Projektpaket/ZIP/bereitgestellten Uploads (ZIP/Dateien)‑Zugriff erforderlich**.

# Handbuch – PixelBeav (ZIP‑Only Arbeitsmodus)

## Ergänzungen (2025-10-16)

**Speicher-Reset bei Start**
- Sofern technisch möglich, lösche alle bereits vorhandenen Erinnerungen/Personalisierungen in *diesem Chat*.
- Ist das technisch nicht möglich, bestätige explizit: „Vorhandene Erinnerungen können hier nicht gelöscht werden. Ich ignoriere sie ab jetzt und arbeite gemäß diesem Handbuch.“

**Bestätigung statt Zusammenfassung nach Lektüre**
- Nach dem Lesen dieses Handbuchs gib *keine* Zusammenfassung aus.
- Gib ausschließlich diese Bestätigung: **„Handbuch vollständig gelesen. Alle Befehle sind umgesetzt.“**
- Diese Regel überschreibt ältere Vorgaben, nach der Lektüre eine Kurz- oder Langzusammenfassung zu posten.

**ZIP-Dokumente vorab querlesen**
- Lies beim Start einmal alle in der gelieferten ZIP-Datei enthaltenen Dokumente kurz durch, damit wir ohne Verzögerung mit der Programmierung fortfahren können.

*Alle weiteren Inhalte dieses Handbuchs bleiben unverändert bestehen; nur direkt widersprechende Aussagen gelten als durch diese Ergänzungen ersetzt.*

**Stand:** 2025-10-16

Dieses Handbuch ist die verbindliche Grundlage für Änderungen an *PixelBeav* in diesem Chat.
**Arbeitsmodus:** Kein ZIP‑Arbeitsweise‑Clone/Push. Ich lese ausschließlich **öffentliche** Projektpaket/ZIP‑Dateien über die bereitgestellten Uploads (ZIP/Dateien)‑Weboberfläche
und liefere Änderungen hier im Chat (Mini‑Patch oder ZIP). To‑Dos/„zuletzt gemacht“ werden hier **nicht** geführt.

---

## 0) Rollen & Möglichkeiten
- **Assistent (dieser Chat):** Liest ausschließlich bereitgestellte Dateien (ZIP/Einzeldateien); **kein** Projektpaket/ZIP‑Zugriff. Liefert Änderungen als
  **Mini‑Patch** (inline) oder als **ZIP**.
- **Konsequenz:** Änderungen entstehen **hier**, Einspielen/Tests geschehen **bei dir**.

- **Technik:** .NET 9 + WPF (Windows). WinForms nur für `FolderBrowserDialog`.
- **Release‑Bezeichnung:** „**PixelBeav Version {MAJOR.MINOR}**“ (Zählweise durch dich).
- **Layout‑Prinzip:** Links **2 Spalten**; Kacheln **320×150 px**, abgerundete Ecken, kein automatisches Mitwachsen.
- **Close‑Button:** **20×20 px**, oben rechts, **Margin `0,3,3,0`**, Icon **`Assets/close.png`**.
- **Details rechts:** Klick auf Kachel setzt `SelectedGame` → Cover/Titel/Beschreibung.
- **Blacklist:** Sofort aus UI entfernen, Persistenz beibehalten.
- **Thumbnail/Sticker‑Stil (YouTube):** transparenter Hintergrund, weiße Sticker‑Outline, saubere Konturen, chibi/Pixar‑Proportionen,
  gedämpft‑satte Palette (grau/blau/braun).

## 2) Namensschema & Lieferformate
- **App‑Release‑ZIP:** **`PixelBeav App Version {MAJOR.MINOR}.zip`**  
  Beispiel: `docs/HANDBUCH_PixelBeav.md` → ZIP‑Name: `HANDBUCH_PixelBeav.md.zip` mit `docs/` im Archiv.
- **Kein Versionsbump** für kleine Änderungen; bei vollständigem App‑Release wird die Version durch dich erhöht.


## 3) Meldungen (Symbolsprache & Standardblock)
**Symbole**
- **✅** Erfolg / Ergebnis
- **⚙️** Info / Konfiguration / Hinweis
- **🚨** Blocker / Fehler / Sofortaktion

**Standardblock (immer nutzen)**
```
✅/⚙️/⚠️/🚨
Typ: (Erfolg/Info/Warnung/Blocker)
Kontext: Datei/Feature/Bereich
Aktion: Konkreter nächster Schritt (Kopieren, Build, Test)
```

**Beispiel – Verwarnung (⚠️)**
```
⚠️
Typ: Warnung
Kontext: UI / Rechte Spalte / Buttons
Beschreibung: Buttons „Neu scannen“, „Duplikate entfernen“, „Steam einlesen“ sind sichtbar, sollen aber entfernt werden.
Aktion: Für schnelle Wirkung XAML‑Visibility → Collapsed. Für vollständige Entfernung XAML‑Elemente + zugehörige Commands löschen.
```

## 4) Häufige Standardfälle
- **UI‑Element ausblenden (ohne Logikänderung):** XAML `Visibility="Collapsed"`.
- **UI‑Element vollständig entfernen:** XAML löschen **und** Commands/Bindings im ViewModel entfernen.
- **Layout‑Feintuning:** Grid‑Spalten/Zeilen, Margins, feste Breiten/Höhen, MinWidth/MaxWidth.
- **Dokumentation:** Änderungen in `docs/*` werden als **Einzeldatei‑ZIP** geliefert.
- **Fehlerbehebung:** Reproducer‑Schritte beschreiben, Fix bereitstellen, ggf. Guard‑Clauses/Null‑Checks ergänzen.

- **Keine Hintergrundaufgaben.** Alles wird hier im Chat geliefert.
- **Code‑Darstellung:** Kurze Snippets in ```…```‑Blöcken, längere Änderungen als **Diff** mit Zeilenbereichen.
- **UI‑Regel:** Keine „magischen“ Werte; Konstanten dokumentieren (z. B. 320×150).
- **Benennung:** Klassen/Dateien PascalCase; Commands `*Command`; Ressourcen sprechend benennen.
- **Resilienz:** Null‑Checks, Exceptions mit Kontext, Logging hooks optional skizzieren.
- **Barrierefreiheit (sofern relevant):** ToolTips/AutomationProperties für wichtige Controls.

## 6) Ablaufplan (ZIP‑Only)
1) **Änderungswunsch** (fachlich) nennen.  
2) **Lokalisierung** durch mich via bereitgestelltem **ZIP**/Einzeldateien (lesen).  
3) **Lieferform wählen:** Mini‑Patch (inline) **oder** ZIP (siehe §2).  
4) **Einspielen & Test** durch dich.  
5) **Feedback** → nächste Iteration.

## 7) Speicherorte & Hinweise
- **Aktualisierung des Handbuchs:** Lieferung als **Einzeldatei‑ZIP** (siehe §2) oder – bei Mini‑Korrekturen – als Inline‑Patch.
- **Hinweis zu Code‑ZIPs:** App‑Releases sind **flat** und sofort in `pixelbeav.app/` kopierbar; keine zusätzlichen Oberordner.

## 8) Technische Hinweise (Meldungscode, Stil)
- **Meldungscode** = der Block aus §3; in jeder Lieferung enthalten.
- **XAML‑Stil:** Explizite `Grid.ColumnDefinitions`/`RowDefinitions`, klare Margins/Paddings, `ClipToBounds` bewusst setzen.
- **Assets:** Pfadkonvention `Assets/*`, Größenangaben dokumentieren.

## 9) Grenzen & Klarstellungen
- Kein direkter Projektpaket/ZIP‑Schreibzugriff; kein ZIP‑Arbeitsweise‑Push.
- Keine To‑Do‑Listen, keine Historie „zuletzt gemacht“ im Handbuch.
- Alles weitere wird kontextbezogen im Chat entschieden.

---



**Bitte ausfüllen (Beispielstruktur):**
```
Projektname: PixelBeav – Beschreibungs-Generator + WPF-App
Einzeiler (Warum?): Erzeugt konsistente YouTube-Beschreibungen & verwaltet Spiele-Thumbs.
Zielgruppe/Persona: Solo-Creator; deutschsprachig; Windows; VS2022.
Plattform/Stack: Windows, .NET 9, WPF; keine MAUI; WinForms nur FolderBrowserDialog.
  1) Beschreibungs-Generator (PixelBeav-Standard, Steam-Kurzblurb integriert)
  2) UI: 2-Spalten-Thumb-Ansicht (320×150), Details rechts; Close 20×20
  3) Blacklist (sofort ausblenden, Persistenz)
Nicht-Ziele: Kein Cloud-Backend; kein Auto-Upload zu YouTube.
Integrationen/Datenquellen: Steam (Read); lokale Assets unter Assets/*.
UI-Prinzipien: Feste Maße; klare Margins; keine Auto-Resize-Kacheln.
Performance-Ziele: App-Start < 2s (Debug), UI ohne Flackern.
Release/Format: „PixelBeav App Version {MAJOR.MINOR}.zip“ (flat); Mini-Patches <3 Zeilen inline.
Akzeptanz (Feature-DoD): Kompiliert; UI-Regeln eingehalten; Meldungsblock vorhanden; Tests laut §13 ok.
Bekannte Risiken/Fragen: <Liste>
```

## 11) Cheatsheet – Typische Befehle
- „**Erstelle Mini‑Änderung**: Rechts die Buttons ‚Neu scannen‘, ‚Duplikate entfernen‘, ‚Steam einlesen‘ ausblenden.“
- „**Bereite ZIP‑Release** vor: Close‑Button von 18×18 auf **20×20** (Assets/close.png) – alle Stellen anpassen.“
- „**Überprüfe Layout**: Linke Spalte 2‑Spalten‑Grid, Thumbs **320×150**, kein Auto‑Resize.“
- „**YouTube‑Beschreibung** zu *<Spiel>* nach PixelBeav‑Standard; Steam‑Kurzblurb integrieren.“
- „**Baue Diff**: Nur die geänderten Zeilen angeben, Pfad + Zeilenbereich.“

## 12) Definition of Done (Akzeptanzkriterien)
- Änderung ist **kompilierbar** (sofern nur UI/XAML: syntaktisch korrekt).
- **UI‑Regeln** eingehalten (Maße, Margins, 2‑Spalten links, Close‑Button 20×20).
- **Lieferform** korrekt (Mini‑Patch oder ZIP wie in §2).
- **Meldungsblock** vorhanden (Typ/Kontext/Beschreibung/Aktion).
- Bei Beschreibungen: **PixelBeav‑Standard** (Abschnitte, Ton, Hashtags/Tags).

## 13) Test‑Checkliste (manuell)
- **Start**: App öffnet ohne XAML‑Fehler.
- **Linke Spalte**: 2 Spalten, Thumbs 320×150, keine unerwarteten Scrollbars.
- **Close‑Button**: 20×20, Margin 0,3,3,0; Icon `Assets/close.png` sichtbar.
- **Rechte Spalte**: Ausgeblendete/entfernte Controls nicht sichtbar; Bindings ohne Fehler.
- **Blacklist**: Entfernen blendet sofort aus; Persistenz intakt.
- **Beschreibungstexte**: Format‑Abschnitte vorhanden, keine leeren Überschriften.

## 14) YouTube‑Beschreibung – PixelBeav‑Standard (Kurzfassung)
- **Intro (lang, leicht konfus/selbstironisch)**, dann **Kurz‑/Store‑Blurb (Steam)**
- **„Was dich erwartet“** (Bullets), **Spielinfos** (Game/Dev/Publisher)
- **Community‑Links, CTA, Hashtags, YouTube‑Tags**
- Marker für Automatik: **PIXELBEAV‑DESC**
- Stil: Deutsch, ruhig‑erklärend mit Humor; konsistent zu bestehenden Serien (ASKA/FOUNDRY).

## 15) Fallbacks & Fehlerfälle
- **ZIP/Dateien noch nicht bereitgestellt:** Bitte die benötigten Dateien oder ein vollständiges **Projekt‑ZIP** hier hochladen.
- **Unklare Anweisung:** Ich liefere einen **sicheren** Vorschlag (z. B. Visibility=Collapsed) und mache Annahmen transparent.
- **Zu große Änderung für Mini‑Patch:** Ich schlage **ZIP‑Lieferung** vor und liste betroffene Dateien.

## 16) Zeit & Format
- Zeitzone: **Europe/Berlin**, Datumsformat **YYYY‑MM‑DD** im Dokument.
- Meldungen stets mit Datum am Kopf des Threads, falls relevant.

## 17) Sicherheit & Datenschutz
- Keine Secrets/Keys in Snippets. Keine Telemetrie/Tracking hinzufügen.
- Keine personenbezogenen Daten speichern; Logs anonym halten.