# PixelBeav Release: 1.1  |  2025-10-18  |  Changed: no
<#
.SYNOPSIS
  Packt eine Full-Release-ZIP aus einem angegebenen Windows-Ordner.

.DESCRIPTION
  - Default-Quelle ist **zwei Ebenen über diesem Skript** (…\Docs\NewChat\.. \..), also typischer Repo-Root.
  - Alternativ -Source übergeben (Strings mit Sonderzeichen in **einfachen Anführungszeichen** angeben).
  - Artefaktfrei (ohne bin/ obj/ .vs/ .git/ .github/ packages/ PackageCache/ *.user *.suo).
  - Ergebnis: <Quelle>\Releases\PixelBeav App Version <VERSION>.zip

.PARAMETER Source
  Quellordner. Beispiel (mit Backtick im Pfad!):
    -Source 'E:\LET`S PLAYS\005-BESCHREIBUNG&TITEL\PROGRAMM\PixelBeav.App'

.PARAMETER Version
  Optional. Wenn leer, wird aus <Source>\Docs\NewChat\VERSION.txt gelesen.
#>

param(
  [string]$Source,
  [string]$Version
)

$ErrorActionPreference = 'Stop'

# Default: zwei Ebenen über diesem Skript (…\Docs\NewChat\..\..)
if ([string]::IsNullOrWhiteSpace($Source)) {
  $Source = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
}

function Read-Version([string]$root) {
  if ($Version) { return $Version }
  $vf = Join-Path $root 'Docs\NewChat\VERSION.txt'
  if (-not (Test-Path -LiteralPath $vf)) { throw "VERSION.txt fehlt: $vf" }
  $raw = (Get-Content -LiteralPath $vf -Raw)
  $release = ($raw -split "`r?`n" | Where-Object { $_ -match '^\s*release\s*=\s*(.+)\s*$' } | ForEach-Object { $Matches[1].Trim() }) | Select-Object -First 1
  if ([string]::IsNullOrWhiteSpace($release)) { return $raw.Trim() }  # Fallback für alte Files
  return $release
}


function Stage-And-Zip([string]$root, [string]$version) {
  $releases = Join-Path $root 'Releases'
  if (-not (Test-Path -LiteralPath $releases)) { New-Item -ItemType Directory -Path $releases | Out-Null }

  $stage = Join-Path $env:TEMP ("pixelbeav_stage_" + (Get-Date -Format 'yyyyMMddHHmmss'))
  New-Item -ItemType Directory -Path $stage | Out-Null

  $excludeDirs = @('bin','obj','.vs','.git','.github','packages','PackageCache')
  $excludeFiles = @('*.suo','*.user')
  $xd = @(); foreach ($d in $excludeDirs) { $xd += @('/XD', (Join-Path $root $d)) }
  $xf = @(); foreach ($f in $excludeFiles) { $xf += @('/XF', $f) }

  $args = @($root, $stage, '/MIR','/NFL','/NDL','/NJH','/NJS','/NP','/R:1','/W:1') + $xd + $xf
  robocopy @args | Out-Null

  $zipName = "PixelBeav App Version $version.zip"
  $zipPath = Join-Path $releases $zipName
  if (Test-Path -LiteralPath $zipPath) { Remove-Item -LiteralPath $zipPath -Force }
  Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zipPath

  Remove-Item -LiteralPath $stage -Recurse -Force
  Write-Host "Release erstellt: $zipPath"
}

# Validate source exists (supports Sonderzeichen via -LiteralPath)
if (-not (Test-Path -LiteralPath $Source)) {
  throw "Quellordner nicht gefunden (achte auf einfache Anführungszeichen bei Sonderzeichen): `n$Source"
}

$root = (Resolve-Path -LiteralPath $Source).Path
$ver = Read-Version $root
Stage-And-Zip $root $ver
