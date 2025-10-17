using PixelBeav.App.Models;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace PixelBeav.App.Services
{
    public static class SteamScanService
    {
        /// <summary>
        /// Scannt die Steam-Bibliotheken (libraryfolders.vdf + appmanifest_*.acf) und liefert GameEntry-Objekte.
        /// limitAppIds: optional Filter auf bestimmte AppIDs.
        ///</summary>
        public static List<GameEntry> ScanSteamGames(HashSet<string>? limitAppIds = null)
        {
            var result = new List<GameEntry>();

            var steamRoot = GetSteamRoot();
            if (string.IsNullOrWhiteSpace(steamRoot)) return result;

            var libraries = GetLibraryFolders(steamRoot);
            if (libraries.Count == 0) libraries.Add(Path.Combine(steamRoot, "steamapps"));

            var appNameRx = new Regex(@"""name""\s*""(?<n>[^""]+)""", RegexOptions.IgnoreCase);
            var appIdRx   = new Regex(@"appmanifest_(?<id>\d+)\.acf$", RegexOptions.IgnoreCase);

            foreach (var lib in libraries)
            {
                if (!Directory.Exists(lib)) continue;
                foreach (var manifest in Directory.EnumerateFiles(lib, "appmanifest_*.acf", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        var fileName = Path.GetFileName(manifest) ?? string.Empty;
                        var mId = appIdRx.Match(fileName);
                        if (!mId.Success) continue;
                        var appId = mId.Groups["id"].Value;
                        if (limitAppIds != null && !limitAppIds.Contains(appId)) continue;

                        var text = File.ReadAllText(manifest, Encoding.UTF8);
                        var mName = appNameRx.Match(text);
                        if (!mName.Success) continue;

                        var title = mName.Groups["n"].Value;
                        var header = $"https://cdn.akamai.steamstatic.com/steam/apps/{appId}/header.jpg";

                        var ge = new GameEntry();
                        ge.GetType().GetProperty("Title")?.SetValue(ge, title);
                        ge.GetType().GetProperty("HeaderImageUri")?.SetValue(ge, header);
                        ge.GetType().GetProperty("SteamAppId")?.SetValue(ge, appId);

                        result.Add(ge);
                    }
                    catch { /* ignore einzelne defekte Manifeste */ }
                }
            }
            return Dedup(result);
        }

        private static List<GameEntry> Dedup(List<GameEntry> list)
        {
            var dict = new Dictionary<string, GameEntry>(StringComparer.OrdinalIgnoreCase);
            foreach (var g in list ?? new List<GameEntry>())
            {
                var id = g.GetType().GetProperty("SteamAppId")?.GetValue(g) as string;
                var exe = g.GetType().GetProperty("ExecutablePath")?.GetValue(g) as string;
                var title = g.GetType().GetProperty("Title")?.GetValue(g) as string ?? string.Empty;
                string key = !string.IsNullOrWhiteSpace(id) ? $"id:{id}"
                            : !string.IsNullOrWhiteSpace(exe) ? $"exe:{exe}"
                            : $"title:{title}";
                if (!dict.ContainsKey(key)) dict[key] = g;
            }
            return dict.Values.ToList();
        }

        private static string GetSteamRoot()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
                var path = key?.GetValue("SteamPath") as string ?? key?.GetValue("InstallPath") as string;
                if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path)) return path;
            }
            catch { }
            var guess = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam");
            return Directory.Exists(guess) ? guess : string.Empty;
        }

        private static List<string> GetLibraryFolders(string steamRoot)
        {
            var list = new List<string>();
            try
            {
                var vdf = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");
                if (!File.Exists(vdf)) return list;
                var text = File.ReadAllText(vdf, Encoding.UTF8);

                var rx = new Regex(@"""path""\s*""(?<p>[^""]+)""", RegexOptions.IgnoreCase);
                foreach (Match m in rx.Matches(text))
                {
                    var p = m.Groups["p"].Value.Replace(@"\\", @"\").Trim();
                    var steamapps = Path.Combine(p, "steamapps");
                    if (Directory.Exists(steamapps)) list.Add(steamapps);
                }

                var rootApps = Path.Combine(steamRoot, "steamapps");
                if (Directory.Exists(rootApps) && !list.Contains(rootApps, StringComparer.OrdinalIgnoreCase))
                    list.Add(rootApps);
            }
            catch { }
            return list;
        }
    }
}
