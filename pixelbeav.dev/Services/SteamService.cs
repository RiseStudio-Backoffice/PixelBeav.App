// PixelBeav Patch: PixelPatch-1.1.32  |  2025-10-18  |  Changed: yes
// PixelBeav Release: 1.1  |  2025-10-18  |  Changed: no
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PixelBeav.App.Services
{
    public static class SteamService
    {
        public static string? FindSteamPath()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
                var path = key?.GetValue("SteamPath") as string;
                if (!string.IsNullOrWhiteSpace(path)) return path;
            }
            catch {}
            return null;
        }

        public static IEnumerable<string> GetLibraryFolders(string steamPath)
        {
            var list = new List<string>();
            try
            {
                var vdf = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
                if (File.Exists(vdf))
                {
                    var txt = File.ReadAllText(vdf);
                    var rx = new Regex(@"""path""\s+""([^""]+)""", RegexOptions.IgnoreCase);
                    foreach (Match m in rx.Matches(txt))
                    {
                        var p = m.Groups[1].Value;
                        p = p.Replace(@"\\", @"\");
                        list.Add(p);
                    }
                }
                var def = Path.Combine(steamPath, "steamapps");
                if (Directory.Exists(def)) list.Add(def);
            }
            catch {}
            return list;
        }

        public static IEnumerable<string> GetAppManifestFiles(IEnumerable<string> steamAppsFolders)
        {
            foreach (var folder in steamAppsFolders)
            {
                string path = folder;
                if (!path.EndsWith("steamapps", StringComparison.OrdinalIgnoreCase))
                    path = Path.Combine(path, "steamapps");
                if (Directory.Exists(path))
                {
                    foreach (var f in Directory.EnumerateFiles(path, "appmanifest_*.acf", SearchOption.TopDirectoryOnly))
                        yield return f;
                }
            }
        }

        public static Dictionary<string, string> ParseAppManifest(string file)
        {
            var dict = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var line in File.ReadAllLines(file))
                {
                    var m = Regex.Match(line, @"""(?<k>[^""]+)""\s+""(?<v>[^""]*)""");
                    if (m.Success) dict[m.Groups["k"].Value] = m.Groups["v"].Value;
                }
            }
            catch {}
            return dict;
        }

        public static async Task<(string? headerImage, string? shortDesc)> FetchStoreMetaAsync(string appId, string language = "de")
        {
            try
            {
                using var http = new HttpClient();
                http.Timeout = TimeSpan.FromSeconds(10);
                var url = $"https://store.steampowered.com/api/appdetails?appids={appId}&l={language}";
                var json = await http.GetStringAsync(url);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement.GetProperty(appId);
                if (root.TryGetProperty("success", out var ok) && ok.GetBoolean())
                {
                    var data = root.GetProperty("data");
                    var header = data.TryGetProperty("header_image", out var h) ? h.GetString() : null;
                    var desc = data.TryGetProperty("short_description", out var s) ? s.GetString() : null;
                    return (header, desc);
                }
            }
            catch {}
            return (null, null);
        }
    }
}
