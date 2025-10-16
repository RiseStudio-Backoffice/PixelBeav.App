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
        public static List<string> ListCollections()
        {
            var map = ReadCollectionsFromLocalConfig();
            if (map.Count == 0)
                map = ReadCollectionsFromLevelDbCache(); // Fallback (neue Steam-UI speichert dort)

            if (map.Count == 0)
                return ListLegacyTagCollections(); // letzter Fallback (alte Tags)

            return map.Keys.OrderBy(s => s, StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        public static HashSet<string> GetCollectionAppIds(string collectionName)
        {
            var map = ReadCollectionsFromLocalConfig();
            if (map.Count == 0)
                map = ReadCollectionsFromLevelDbCache();

            if (map.TryGetValue(collectionName, out var ids) && ids.Count > 0)
                return ids;

            return GetLegacyTagAppIds(collectionName);
        }

        /// <summary>
        /// Fügt die Collection zu den gespeicherten Spielen hinzu (ohne Duplikate) und
        /// gibt nur die NEU hinzugekommenen Einträge zurück (für UI-Einfügen oben).
        /// </summary>
        public static List<GameEntry> AddCollectionAndReturn(string collectionName)
        {
            var ids = GetCollectionAppIds(collectionName);
            var scanned = ScanSteamGames(ids);      // deduped Scan-Ergebnis

            var existing = StorageService.LoadGames();
            var index = BuildIndex(existing);

            var added = new List<GameEntry>();
            foreach (var g in scanned)
            {
                var key = KeyOf(g);
                if (!index.ContainsKey(key))
                {
                    existing.Add(g);
                    added.Add(g);
                    index[key] = g;
                }
            }
            StorageService.SaveGames(existing); // SaveGames deduped zusätzlich
            return added;
        }

        public static void ReplaceWithCollection(string collectionName)
        {
            var ids = GetCollectionAppIds(collectionName);
            var scanned = ScanSteamGames(ids);
            StorageService.SaveGames(scanned);
        }

        // ===== localconfig.vdf =====
        private static Dictionary<string, HashSet<string>> ReadCollectionsFromLocalConfig()
        {
            var dict = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var steamRoot = GetSteamRoot();
                if (string.IsNullOrWhiteSpace(steamRoot)) return dict;
                var userdata = Path.Combine(steamRoot, "userdata");
                if (!Directory.Exists(userdata)) return dict;

                foreach (var userDir in Directory.EnumerateDirectories(userdata))
                {
                    var localcfg = Path.Combine(userDir, "config", "localconfig.vdf");
                    if (!File.Exists(localcfg)) continue;
                    var text = File.ReadAllText(localcfg, Encoding.UTF8);

                    var m = Regex.Match(text, @"""user-collections""\s*""(?<json>.*?)""", RegexOptions.Singleline);
                    if (!m.Success) continue;

                    var escaped = m.Groups["json"].Value;
                    var json = UnescapeVdfString(escaped);

                    ParseCollectionsJsonInto(json, dict);
                }
            }
            catch { }
            return dict;
        }

        // ===== Steam-UI LevelDB Cache (HTML-Client) =====
        private static Dictionary<string, HashSet<string>> ReadCollectionsFromLevelDbCache()
        {
            var dict = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var levelDb = Path.Combine(localApp, "Steam", "htmlcache", "Local Storage", "leveldb");
                if (!Directory.Exists(levelDb)) return dict;

                foreach (var file in Directory.EnumerateFiles(levelDb, "*.*", SearchOption.TopDirectoryOnly))
                {
                    var ext = Path.GetExtension(file).ToLowerInvariant();
                    if (ext != ".log" && ext != ".ldb" && ext != ".sst") continue;

                    byte[] bytes;
                    try { bytes = File.ReadAllBytes(file); }
                    catch { continue; }

                    var text = Encoding.UTF8.GetString(bytes);
                    if (!text.Contains("collections", StringComparison.OrdinalIgnoreCase) &&
                        !text.Contains("user-collections", StringComparison.OrdinalIgnoreCase))
                    {
                        text = Encoding.GetEncoding(1252).GetString(bytes);
                    }

                    foreach (var json in ExtractJsonBlobs(text))
                        ParseCollectionsJsonInto(json, dict);
                }
            }
            catch { }
            return dict;
        }

        // Heuristisch JSON-Blöcke finden, die "collections" enthalten
        private static IEnumerable<string> ExtractJsonBlobs(string text)
        {
            var hits = new List<string>();
            int idx = 0;
            while (idx < text.Length)
            {
                int pos = text.IndexOf("{", idx);
                if (pos < 0) break;

                int windowStart = Math.Max(0, pos - 100);
                int windowEnd = Math.Min(text.Length, pos + 80000);
                var window = text.Substring(windowStart, windowEnd - windowStart);
                if (window.IndexOf("\"collections\"", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    int depth = 0;
                    for (int i = pos; i < text.Length; i++)
                    {
                        char c = text[i];
                        if (c == '{') depth++;
                        else if (c == '}')
                        {
                            depth--;
                            if (depth == 0)
                            {
                                string candidate = text.Substring(pos, i - pos + 1);
                                if (candidate.IndexOf("\"collections\"", StringComparison.OrdinalIgnoreCase) >= 0)
                                    hits.Add(candidate);
                                idx = i + 1;
                                goto next;
                            }
                        }
                    }
                    idx = pos + 1;
                }
                else
                {
                    idx = pos + 1;
                }
                next: ;
            }
            return hits;
        }

        private static void ParseCollectionsJsonInto(string json, Dictionary<string, HashSet<string>> dict)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object) return;

                if (root.TryGetProperty("collections", out var collections) && collections.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in collections.EnumerateObject())
                    {
                        var obj = prop.Value;
                        string? name = null;
                        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                        if (obj.TryGetProperty("name", out var nameEl) && nameEl.ValueKind == JsonValueKind.String)
                            name = nameEl.GetString();

                        ExtractAppIdsFromCollectionObject(obj, ids);

                        if (!string.IsNullOrWhiteSpace(name) && ids.Count > 0)
                        {
                            if (!dict.ContainsKey(name)) dict[name] = ids;
                            else foreach (var id in ids) dict[name].Add(id);
                        }
                    }
                }
            }
            catch { /* ignore malformed snippets */ }
        }

        private static void ExtractAppIdsFromCollectionObject(JsonElement obj, HashSet<string> ids)
        {
            foreach (var key in new[] { "added", "app_ids", "apps", "explicitly_added" })
            {
                if (obj.TryGetProperty(key, out var arr) && arr.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in arr.EnumerateArray())
                    {
                        var s = el.ValueKind switch
                        {
                            JsonValueKind.String => el.GetString(),
                            JsonValueKind.Number => el.TryGetInt32(out var n) ? n.ToString() : null,
                            _ => null
                        };
                        if (!string.IsNullOrWhiteSpace(s)) ids.Add(s!);
                    }
                }
            }
            if (obj.TryGetProperty("additions", out var adds) && adds.ValueKind == JsonValueKind.Object)
            {
                foreach (var sub in new[] { "apps", "app_ids" })
                {
                    if (adds.TryGetProperty(sub, out var arr) && arr.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var el in arr.EnumerateArray())
                        {
                            var s = el.ValueKind switch
                            {
                                JsonValueKind.String => el.GetString(),
                                JsonValueKind.Number => el.TryGetInt32(out var n) ? n.ToString() : null,
                                _ => null
                            };
                            if (!string.IsNullOrWhiteSpace(s)) ids.Add(s!);
                        }
                    }
                }
            }
        }

        private static string UnescapeVdfString(string s)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '\\' && i + 1 < s.Length)
                {
                    var c = s[i + 1];
                    i++;
                    switch (c)
                    {
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        default: sb.Append(c); break;
                    }
                }
                else sb.Append(s[i]);
            }
            return sb.ToString();
        }

        // ===== Legacy (alte Tags) =====
        private static List<string> ListLegacyTagCollections()
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var steamRoot = GetSteamRoot();
                if (string.IsNullOrWhiteSpace(steamRoot)) return set.ToList();
                var userData = Path.Combine(steamRoot, "userdata");
                if (!Directory.Exists(userData)) return set.ToList();

                foreach (var userDir in Directory.EnumerateDirectories(userData))
                {
                    var cfg = Path.Combine(userDir, "7", "remote", "sharedconfig.vdf");
                    if (!File.Exists(cfg)) continue;
                    var text = File.ReadAllText(cfg, Encoding.UTF8);
                    var tagRx = new Regex(@"""\d+""\s*""(?<t>[^""]+)""", RegexOptions.Singleline);
                    foreach (Match m in tagRx.Matches(text))
                    {
                        var t = m.Groups["t"].Value.Trim();
                        if (!string.IsNullOrWhiteSpace(t)) set.Add(t);
                    }
                }
            }
            catch { }
            return set.OrderBy(s => s, StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        private static HashSet<string> GetLegacyTagAppIds(string collectionName)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var steamRoot = GetSteamRoot();
                if (string.IsNullOrWhiteSpace(steamRoot)) return set;
                var userData = Path.Combine(steamRoot, "userdata");
                if (!Directory.Exists(userData)) return set;

                foreach (var userDir in Directory.EnumerateDirectories(userData))
                {
                    var cfg = Path.Combine(userDir, "7", "remote", "sharedconfig.vdf");
                    if (!File.Exists(cfg)) continue;
                    var text = File.ReadAllText(cfg, Encoding.UTF8);

                    var appBlockRx = new Regex(@"""(\d+)""\s*\{[^}]*?""tags""\s*\{(?<tags>[^}]*)\}[^}]*\}", RegexOptions.Singleline);
                    var tagRx      = new Regex(@"""\d+""\s*""(.*?)""", RegexOptions.Singleline);

                    foreach (Match m in appBlockRx.Matches(text))
                    {
                        var appId = m.Groups[1].Value;
                        var tagsRaw = m.Groups["tags"].Value;
                        foreach (Match tm in tagRx.Matches(tagsRaw))
                        {
                            var tag = tm.Groups[1].Value;
                            if (string.Equals(tag, collectionName, StringComparison.OrdinalIgnoreCase))
                            {
                                set.Add(appId);
                                break;
                            }
                        }
                    }
                }
            }
            catch { }
            return set;
        }

        // ===== Scanning & Utils =====
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
                    catch { /* ignore */ }
                }
            }
            return Dedup(result);
        }

        private static Dictionary<string, object> BuildIndex(List<GameEntry> list)
        {
            var index = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var g in list ?? new List<GameEntry>()) index[KeyOf(g)] = g;
            return index;
        }

        private static string KeyOf(GameEntry g)
        {
            var id = g.GetType().GetProperty("SteamAppId")?.GetValue(g) as string;
            var exe = g.GetType().GetProperty("ExecutablePath")?.GetValue(g) as string;
            var title = g.GetType().GetProperty("Title")?.GetValue(g) as string ?? string.Empty;
            return !string.IsNullOrWhiteSpace(id) ? $"id:{id}"
                 : !string.IsNullOrWhiteSpace(exe) ? $"exe:{exe}"
                 : $"title:{title}";
        }

        private static List<GameEntry> Dedup(List<GameEntry> list)
        {
            var dict = new Dictionary<string, GameEntry>(StringComparer.OrdinalIgnoreCase);
            foreach (var g in list ?? new List<GameEntry>()) if (!dict.ContainsKey(KeyOf(g))) dict[KeyOf(g)] = g;
            return new List<GameEntry>(dict.Values);
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