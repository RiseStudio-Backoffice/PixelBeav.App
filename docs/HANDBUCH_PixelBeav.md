# Handbuch – PixelBeav (Stand: 2025-10-15)

Dieses Handbuch fasst die Ergebnisse der heutigen Session zusammen und beschreibt, wie du dir **jederzeit die neuesten Codedaten aus dem öffentlichen Repo** holst.

---

## 1) Projekt-Überblick

- **Technik:** .NET 9 + WPF (Windows), WinForms nur für `FolderBrowserDialog`.
- **Release-Namen:** „**PixelBeav Version {X.Y}**“ (aktueller Chat-Stand: **1.16**).
- **Layout (links):** genau **2 Spalten**, feste Breite (für **320×150**-Kacheln), **kein** automatisches Mitwachsen.
- **Thumbnails:** **320×150 px**, **abgerundete Ecken** (CornerRadius=10). Das Bild wird **als Background derselben Border** mit `ImageBrush` und `Stretch="Fill"` gerendert → **kein Zuschneiden**, bewusst „zusammengeschoben“.
- **Schließen-Button:** **20×20 px**, oben rechts, **3 px** Abstand (Margin `0,3,3,0`), Icon `Assets/close.png`.
- **Details (rechts):** Klick auf Kachel setzt `SelectedGame` → Cover/Titel/Beschreibung.
- **Blacklist:** Sofort-Löschen (Eintrag verschwindet direkt), Eintrag wird **persistiert** und kann über **„Blacklist anzeigen“** eingesehen werden.

---

## 2) Neueste Codedaten holen (Repo)

**Repo (öffentlich):** https://github.com/RiseStudio-Backoffice/PixelBeav.App

### Visual Studio (ohne Terminal)
1. **Git → Repository-Einstellungen**: Prüfen, dass `origin` auf  
   `https://github.com/RiseStudio-Backoffice/PixelBeav.App.git` zeigt.
2. **Git → Pull** (Branch `master`) holt den neuesten Stand.
3. **Build/Start:** Startprojekt `PixelBeav.App` wählen → **F5**.

### Alternativ per CLI
```bash
git clone https://github.com/RiseStudio-Backoffice/PixelBeav.App.git
cd PixelBeav.App
git pull origin master
dotnet restore PixelBeavLibrary.sln
dotnet build PixelBeavLibrary.sln -c Release
```

---

## 3) CI & Branch-Schutz (Kurzüberblick)

- **CI-Workflow:** `.github/workflows/ci.yml`  
  - Trigger: `push`/`pull_request` auf `main, master`, optional `workflow_dispatch`.
  - In Branch-Regeln wird der **Job-Name** (z. B. `CI / build`) hinterlegt – **nicht** nur „CI“.
- **CI-Status ansehen:**
  1) **Actions-Tab** → Workflow „CI“ → letzter Run auf `master`.
  2) **Commit-Seite** → Abschnitt **Checks**.
  3) **README-Badge** (dauerhaft):
     ```md
     [![CI](https://github.com/RiseStudio-Backoffice/PixelBeav.App/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/RiseStudio-Backoffice/PixelBeav.App/actions/workflows/ci.yml)
     ```
- **Branch Protection (Empfehlung):**
  - **Prevent deletions**, **Force-Push verbieten**.
  - Optional: **Require a pull request before merging** + **Require status checks to pass** → `CI / build` (oder Gate-Job wie `CI / required`).

---

## 4) Troubleshooting (heute besprochene Punkte)

- **Rechte Rundungen fehlen:** Meist Viewport-Clipping. Links Breite/Padding konsistent halten; Bild **als Background exakt der Border** mit CornerRadius (nicht als Kind-`<Image>`).
- **Bilder wirken „abgeschnitten“:** Prüfen, dass **`ImageBrush.Stretch="Fill"`** aktiv ist (kein `UniformToFill`). Kein Zuschneiden, sondern *einpassen/zusammenschieben*.
- **Close-Button unsichtbar:** Resource-Pfad (`Assets/close.png`, Build Action=Resource), **ZIndex** > Select-Button, **Margin `0,3,3,0`**, Größe **20×20**.
- **Required Checks in Branch-Regeln fehlen:** GitHub erwartet **Job-Namen** (z. B. `CI / build`). Ein Lauf muss auf **`master`** vorhanden sein. Namen notfalls **manuell eintippen** und mit Enter bestätigen.

---

## 5) Nächste sinnvolle Schritte

- **README**: Badges (CI/Release/License) einfügen.
- **Rules/Classic Branch Protection** finalisieren (Required Checks hinterlegen).
- **Optionale Features**:
  - Letterboxing-Modus (ohne Verzerrung, mit Balken),
  - Blacklist-Fenster (Suchen, Re-Enable),
  - Hover/Pressed-Stil für den Close-Button.

---

### Hinweis zu Code-ZIPs
Künftige **Code-ZIPs** liefere ich **ohne Unterordner (flat)**, damit du sie direkt in **`pixelbeav.app/`** kopieren kannst.
