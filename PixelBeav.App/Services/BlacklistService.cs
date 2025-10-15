using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PixelBeav.App.Services
{
    public static class BlacklistService
    {
        private static string AppDataDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PixelBeav.App");
        private static string FilePath => Path.Combine(AppDataDir, "blacklist.json");
        private static HashSet<string>? _cache;

        private static void EnsureLoaded()
        {
            if (_cache != null) return;
            try
            {
                if (File.Exists(FilePath))
                {
                    var txt = File.ReadAllText(FilePath);
                    var list = JsonSerializer.Deserialize<List<string>>(txt) ?? new List<string>();
                    _cache = new HashSet<string>(list, StringComparer.OrdinalIgnoreCase);
                }
                else _cache = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                _cache = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static void Save()
        {
            Directory.CreateDirectory(AppDataDir);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(_cache?.ToList() ?? new List<string>(), new JsonSerializerOptions{WriteIndented=true}));
        }

        public static IEnumerable<string> GetAll()
        {
            EnsureLoaded();
            return _cache!.ToList();
        }

        public static bool IsBlacklisted(string key)
        {
            EnsureLoaded();
            return _cache!.Contains(key);
        }

        public static void Add(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            EnsureLoaded();
            if (_cache!.Add(key)) Save();
        }

        public static void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            EnsureLoaded();
            if (_cache!.Remove(key)) Save();
        }
    }
}
