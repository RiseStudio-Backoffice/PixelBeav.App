# PixelBeav – WPF App (.NET 9.0) • Version: v1.1.0


Erstellt anhand des **Projekt‑Briefs v1.0 (STRIKT)**.

## Kerndesign
- Links: **nur Spiel‑Thumbnails** (WrapPanel), **keine** Beschriftungen.
- Rechts: Details mit **Cover (~200 px Höhe)**, **Titel**, **Beschreibung**, Buttons **„Ordner öffnen“** und **„Thumbnails…“**.
- Aktionen: **Steam einlesen**, **Duplikate entfernen**, **Ordner wählen**, **Neu scannen**.
- Tech: **.NET 9.0 + WPF (Windows)**; WinForms **nur** für `FolderBrowserDialog`.
- Fallback‑Bild: `Assets/placeholder.png`.

## Start (Visual Studio)
1. ZIP entpacken
2. `PixelBeavLibrary.sln` öffnen
3. Startprojekt: `PixelBeav.App`
4. F5

> Hinweis: `Steam einlesen` verwendet Registry + `libraryfolders.vdf` + optionale Store‑API (Header/Short‑Desc). Ohne Internet/Steam läuft der **lokale Scan** trotzdem.

[![CI](https://github.com/RiseStudio-Backoffice/PixelBeav.App/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/RiseStudio-Backoffice/PixelBeav.App/actions/workflows/ci.yml)
[![Release](https://img.shields.io/github/v/release/RiseStudio-Backoffice/PixelBeav.App?display_name=tag)](https://github.com/RiseStudio-Backoffice/PixelBeav.App/releases)
