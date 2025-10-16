using PixelBeav.App.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace PixelBeav.App.Services
{
    public static class StorageService
    {
        private static readonly string Roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string AppDataDir = Path.Combine(Roaming, "PixelBeav.App");
        private static readonly string ConfigPath = Path.Combine(AppDataDir, "config.json");
        private static readonly string GamesPath  = Path.Combine(AppDataDir, "games.json");
        private static readonly string BlacklistPath = Path.Combine(AppDataDir, "blacklist.json");

        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions { WriteIndented = true };
        public static event Action? BlacklistChanged;

        private static string ReadTextTolerant(string path)
        {
            if (!File.Exists(path)) return string.Empty;
            var bytes = File.ReadAllBytes(path);
            var utf8 = Encoding.UTF8.GetString(bytes);
            if (!utf8.Contains("\uFFFD")) return utf8;
            return Encoding.GetEncoding(1252).GetString(bytes);
        }

        private static T? LoadJson<T>(string path) where T : class
        {
            try
            {
                var txt = ReadTextTolerant(path);
                if (string.IsNullOrWhiteSpace(txt)) return null;
                return JsonSerializer.Deserialize<T>(txt);
            }
            catch { return null; }
        }
        private static void SaveJson<T>(string path, T data)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var json = JsonSerializer.Serialize(data, JsonOpts);
            File.WriteAllText(path, json, new UTF8Encoding(false));
        }

        public class AppConfig { public string? RootFolder { get; set; } }
        public static AppConfig LoadConfig() => LoadJson<AppConfig>(ConfigPath) ?? new AppConfig();
        public static void SaveConfig(AppConfig config) => SaveJson(ConfigPath, config ?? new AppConfig());
        public static List<GameEntry> LoadGames() => LoadJson<List<GameEntry>>(GamesPath) ?? new List<GameEntry>();

        public static void SaveGames(List<GameEntry> games)
        {
            // De-dup by SteamAppId -> ExecutablePath -> Title
            var dict = new Dictionary<string, GameEntry>(StringComparer.OrdinalIgnoreCase);
            foreach (var g in games ?? new List<GameEntry>())
            {
                var id = g.GetType().GetProperty("SteamAppId")?.GetValue(g) as string;
                var exe = g.GetType().GetProperty("ExecutablePath")?.GetValue(g) as string;
                var title = g.GetType().GetProperty("Title")?.GetValue(g) as string ?? string.Empty;
                string key = !string.IsNullOrWhiteSpace(id) ? $"id:{id}"
                            : !string.IsNullOrWhiteSpace(exe) ? $"exe:{exe}"
                            : $"title:{title}";
                if (!dict.ContainsKey(key)) dict[key] = g;
            }
            SaveJson(GamesPath, dict.Values.ToList());
        }

        private static HashSet<string>? _blacklistCache;
        private static readonly StringComparer KeyCmp = StringComparer.OrdinalIgnoreCase;

        private static void EnsureBlacklistLoaded()
        {
            if (_blacklistCache != null) return;
            _blacklistCache = LoadJson<HashSet<string>>(BlacklistPath) ?? new HashSet<string>(KeyCmp);
        }
        private static void SaveBlacklist() => SaveJson(BlacklistPath, _blacklistCache?.ToList() ?? new List<string>());

        public static IEnumerable<string> GetBlacklist() { EnsureBlacklistLoaded(); return _blacklistCache!.ToList(); }
        public static bool IsBlacklisted(string key) { if (string.IsNullOrWhiteSpace(key)) return false; EnsureBlacklistLoaded(); return _blacklistCache!.Contains(key); }
        public static void AddToBlacklist(string key) { if (string.IsNullOrWhiteSpace(key)) return; EnsureBlacklistLoaded(); if (_blacklistCache!.Add(key)) { SaveBlacklist(); BlacklistChanged?.Invoke(); } }
        public static void RemoveFromBlacklist(string key) { if (string.IsNullOrWhiteSpace(key)) return; EnsureBlacklistLoaded(); if (_blacklistCache!.Remove(key)) { SaveBlacklist(); BlacklistChanged?.Invoke(); } }
    }
}