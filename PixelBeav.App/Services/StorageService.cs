using PixelBeav.App.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PixelBeav.App.Services
{
    public static class StorageService
    {
        private static readonly string AppDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PixelBeav.App");
        private static readonly string ConfigPath = Path.Combine(AppDataDir, "config.json");
        private static readonly string GamesPath  = Path.Combine(AppDataDir, "games.json");
        private static readonly string BlacklistPath = Path.Combine(AppDataDir, "blacklist.json");

        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions { WriteIndented = true };

        public class AppConfig
        {
            public string? RootFolder { get; set; }
        }

        public static event Action? BlacklistChanged;

        // ---------- Config ----------
        public static AppConfig LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var txt = File.ReadAllText(ConfigPath);
                    return JsonSerializer.Deserialize<AppConfig>(txt) ?? new AppConfig();
                }
            }
            catch { }
            return new AppConfig();
        }

        public static void SaveConfig(AppConfig config)
        {
            Directory.CreateDirectory(AppDataDir);
            var json = JsonSerializer.Serialize(config ?? new AppConfig(), JsonOpts);
            File.WriteAllText(ConfigPath, json);
        }

        // ---------- Games ----------
        public static List<GameEntry> LoadGames()
        {
            try
            {
                if (File.Exists(GamesPath))
                {
                    var txt = File.ReadAllText(GamesPath);
                    return JsonSerializer.Deserialize<List<GameEntry>>(txt) ?? new List<GameEntry>();
                }
            }
            catch { }
            return new List<GameEntry>();
        }

        public static void SaveGames(List<GameEntry> games)
        {
            Directory.CreateDirectory(AppDataDir);
            var list = games ?? new List<GameEntry>();
            var json = JsonSerializer.Serialize(list, JsonOpts);
            File.WriteAllText(GamesPath, json);
        }

        // ---------- Blacklist ----------
        private static HashSet<string>? _blacklistCache;

        private static void EnsureBlacklistLoaded()
        {
            if (_blacklistCache != null) return;
            try
            {
                if (File.Exists(BlacklistPath))
                {
                    var txt = File.ReadAllText(BlacklistPath);
                    var list = JsonSerializer.Deserialize<List<string>>(txt) ?? new List<string>();
                    _blacklistCache = new HashSet<string>(list, StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    _blacklistCache = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }
            }
            catch
            {
                _blacklistCache = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static void SaveBlacklistInternal()
        {
            Directory.CreateDirectory(AppDataDir);
            var list = _blacklistCache?.ToList() ?? new List<string>();
            var json = JsonSerializer.Serialize(list, JsonOpts);
            File.WriteAllText(BlacklistPath, json);
        }

        public static IEnumerable<string> GetBlacklist()
        {
            EnsureBlacklistLoaded();
            return _blacklistCache!.ToList();
        }

        public static bool IsBlacklisted(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return false;
            EnsureBlacklistLoaded();
            return _blacklistCache!.Contains(key);
        }

        public static void AddToBlacklist(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            EnsureBlacklistLoaded();
            if (_blacklistCache!.Add(key))
            {
                SaveBlacklistInternal();
                BlacklistChanged?.Invoke();
            }
        }

        public static void RemoveFromBlacklist(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            EnsureBlacklistLoaded();
            if (_blacklistCache!.Remove(key))
            {
                SaveBlacklistInternal();
                BlacklistChanged?.Invoke();
            }
        }
    }
}