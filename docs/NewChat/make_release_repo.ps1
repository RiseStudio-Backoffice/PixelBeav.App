<#
.SYNOPSIS
  Baut eine Full-Release-ZIP aus dem aktuellen Repo-Root.

.DESCRIPTION
  - Erwartet, dass dieses Skript aus dem REPO-ROOT gestartet wird.
  - Liest Version aus .\Docs\NewChat\VERSION.txt
  - Packt artefaktfrei (ohne bin/ obj/ .vs/ .git/ .github/ packages/ PackageCache/ *.user *.suo)
  - Ergebnis: .\Releases\PixelBeav App Version <VERSION>.zip
#>

$ErrorActionPreference = 'Stop'

function Assert-RepoRoot {
  if (-not (Test-Path ".\Docs\NewChat\VERSION.txt")) {
    throw "Nicht im Repo-Root: Docs\NewChat\VERSION.txt wurde nicht gefunden."
  }
}
function Read-Version {
  (Get-Content ".\Docs\NewChat\VERSION.txt" -Raw).Trim()
}
function Stage-And-Zip([string]$version) {
  $releases = Join-Path "." "Releases"
  if (-not (Test-Path $releases)) { New-Item -ItemType Directory -Path $releases | Out-Null }

  $stage = Join-Path $env:TEMP ("pixelbeav_stage_" + (Get-Date -Format 'yyyyMMddHHmmss'))
  New-Item -ItemType Directory -Path $stage | Out-Null

  $excludeDirs = @('bin','obj','.vs','.git','.github','packages','PackageCache')
  $excludeFiles = @('*.suo','*.user')
  $xd = @(); foreach ($d in $excludeDirs) { $xd += @('/XD', (Join-Path "." $d)) }
  $xf = @(); foreach ($f in $excludeFiles) { $xf += @('/XF', $f) }
  $args = @(".", $stage, '/MIR','/NFL','/NDL','/NJH','/NJS','/NP','/R:1','/W:1') + $xd + $xf
  robocopy @args | Out-Null

  $zipName = "PixelBeav App Version $version.zip"
  $zipPath = Join-Path $releases $zipName
  if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
  Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zipPath
  Remove-Item $stage -Recurse -Force
  Write-Host "Release erstellt: $zipPath"
}

Assert-RepoRoot
$version = Read-Version
Stage-And-Zip $version
