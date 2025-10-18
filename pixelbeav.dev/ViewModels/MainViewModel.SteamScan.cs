// PixelBeav Patch: PixelPatch-1.1.32  |  2025-10-18  |  Changed: yes
// PixelBeav Release: 1.1  |  2025-10-18  |  Changed: no
using System;
using System.Linq;
using System.Threading.Tasks;
using PixelBeav.App.Models;
using PixelBeav.App.Services;

namespace PixelBeav.App.ViewModels
{
    public partial class MainViewModel
    {
        private async Task ScanSteamAndPersistAsync()
        {
            // 1) Scan im Hintergrund
            var scanned = await Task.Run(() => SteamScanService.ScanSteamGames());

            // 2) Deduplizieren + Blacklist
            var distinct = scanned
                .GroupBy(g => {
                    var appId = g.GetType().GetProperty("SteamAppId")?.GetValue(g) as string;
                    var exe   = g.GetType().GetProperty("ExecutablePath")?.GetValue(g) as string;
                    var title = g.Title ?? string.Empty;
                    return !string.IsNullOrWhiteSpace(appId) ? $"id:{appId}"
                         : !string.IsNullOrWhiteSpace(exe)   ? $"exe:{exe}"
                         : $"title:{title}";
                }, StringComparer.OrdinalIgnoreCase)
                .Select(grp => grp.First())
                .Where(g => !StorageService.IsBlacklisted(GetKey(g)))
                .ToList();

            // 3) Metadaten (de) + Thumbnails erzeugen
            foreach (var g in distinct)
            {
                try
                {
                    var appId = g.GetType().GetProperty("SteamAppId")?.GetValue(g) as string;
                    if (!string.IsNullOrWhiteSpace(appId))
                    {
                        var meta = await SteamService.FetchStoreMetaAsync(appId!, "de");
                        if (!string.IsNullOrWhiteSpace(meta.Item2)) g.ShortDescription = meta.Item2!;
                        if (!string.IsNullOrWhiteSpace(meta.Item1)) g.HeaderImageUri = meta.Item1!;
                        if (string.IsNullOrWhiteSpace(g.HeaderImageUri))
                            g.HeaderImageUri = $"https://cdn.akamai.steamstatic.com/steam/apps/{appId}/header.jpg";
                    }
                    await ThumbnailService.EnsureAndGenerateAsync(g);
                }
                catch { /* robust weiter */ }
            }

            // 4) Persistenz & UI
            AllGames.Clear();
            foreach (var g in distinct) AllGames.Add(g);
            StorageService.SaveGames(AllGames.ToList());
            AllGames.Clear();
            foreach (var g in distinct) AllGames.Add(g);
            FilteredGames.Refresh();
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
    }
}
