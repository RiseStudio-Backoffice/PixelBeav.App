// PixelBeav Patch: PixelPatch-1.1.23  |  2025-10-18  |  Changed: yes
// Summary: Disambiguate MessageBox -> System.Windows.MessageBox to fix CS0104
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using PixelBeav.App.Models;
using PixelBeav.App.Services;

namespace PixelBeav.App.ViewModels
{
    public partial class MainViewModel
    {
        public async Task FullRescanAsync()
        {
            try
            {
                Diagnostics.Log("FullRescanAsync: START");
                var scanned = await Task.Run(() => SteamScanServiceFresh.ScanSteamGamesFresh());
                Diagnostics.Log($"FullRescanAsync: scanned={scanned.Count}");

                var filtered = scanned.Where(g => !StorageService.IsBlacklisted(GetKey(g))).ToList();
                Diagnostics.Log($"FullRescanAsync: filtered={filtered.Count}");

                StorageService.SaveGames(filtered);

                AllGames.Clear();
                foreach (var g in filtered) AllGames.Add(g);
                FilteredGames.Refresh();
                CommandManager.InvalidateRequerySuggested();

                var path = StorageService.GetDataFilePath();
                Diagnostics.Log("FullRescanAsync: DONE -> " + path);
                System.Windows.MessageBox.Show($@"Steam-Scan abgeschlossen.
Gefunden: {filtered.Count} Einträge
Gespeichert nach:
{path}
Log:
{Diagnostics.GetLogPath()}", "PixelBeav – Scan");
            }
            catch (Exception ex)
            {
                Diagnostics.Log("FullRescanAsync EX: " + ex.ToString());
                System.Windows.MessageBox.Show(ex.ToString(), "Steam-Scan fehlgeschlagen");
            }
        }
    }
}
