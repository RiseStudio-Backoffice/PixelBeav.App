using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace PixelBeav.App.Services
{
    public static class StorageService
    {
        private static string AppDataDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PixelBeav.App");
        private static string ConfigPath => Path.Combine(AppDataDir, "config.json");
        private static string GamesPath => Path.Combine(AppDataDir, "games.json");
        private static string BlacklistPath => Path.Combine(AppDataDir, "blacklist.json");

        public class AppConfig
        {
            public string? RootFolder { get; set; }
        }

        public static void SaveConfig(AppConfig config)
        {
            Directory.CreateDirectory(AppDataDir);
            File.WriteAllText(ConfigPath, JsonSerializer.Serialize(config, new JsonSerializerOptions{WriteIndented=true}));
        }

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
            catch {}
            return new AppConfig();
        }

        public static void SaveGames(List<Models.GameEntry> games)
        {
            Directory.CreateDirectory(AppDataDir);
            File.WriteAllText(GamesPath, JsonSerializer.Serialize(games, new JsonSerializerOptions{WriteIndented=true}));
        }

        public static List<Models.GameEntry> LoadGames()
        {
            try
            {
                if (File.Exists(GamesPath))
                {
                    var txt = File.ReadAllText(GamesPath);
                    var data = JsonSerializer.Deserialize<List<Models.GameEntry>>(txt);
                    if (data != null) return data;
                }
            }
            catch {}
            return new List<Models.GameEntry>();
        }

        public static HashSet<string> LoadBlacklist()
        {
            try
            {
                if (File.Exists(BlacklistPath))
                {
                    var txt = File.ReadAllText(BlacklistPath);
                    var arr = JsonSerializer.Deserialize<string[]>(txt);
                    if (arr != null) return new HashSet<string>(arr, StringComparer.OrdinalIgnoreCase);
                }
            }
            catch {}
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public static void SaveBlacklist(HashSet<string> set)
        {
            Directory.CreateDirectory(AppDataDir);
            File.WriteAllText(BlacklistPath, JsonSerializer.Serialize(set, new JsonSerializerOptions{WriteIndented=true}));
        }

        public static void AddToBlacklist(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            var set = LoadBlacklist();
            set.Add(key);
            SaveBlacklist(set);
        }
    }
}
