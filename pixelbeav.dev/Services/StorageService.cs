// PixelBeav Patch: PixelPatch-1.1.26  |  2025-10-18  |  Changed: yes
// Summary: Store games.json/blacklist in project-level Data (not AppData) for dev
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using PixelBeav.App.Models;
using PixelBeav.App.Utils;

namespace PixelBeav.App.Services
{
    public static class StorageService
    {
        private static readonly string DataDir = ProjectPaths.DataDir;
        private static readonly string DataFilePath = Path.Combine(DataDir, "games.json");
        private static readonly string BlacklistPath = Path.Combine(DataDir, "blacklist.txt");

        public static event Action? BlacklistChanged;

        private static JsonSerializerOptions JsonOpts => new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static string GetDataFilePath() => DataFilePath;

        public static bool IsGamesJsonEffectivelyEmpty()
        {
            try
            {
                if (!File.Exists(DataFilePath)) return true;
                var raw = File.ReadAllText(DataFilePath);
                if (string.IsNullOrWhiteSpace(raw)) return true;
                var t = raw.Trim();
                if (t == "[]" || t == "[ ]" || t.Replace(" ", "") == "[]") return true;
                var list = System.Text.Json.JsonSerializer.Deserialize<List<GameEntry>>(t, JsonOpts) ?? new List<GameEntry>();
                return list.Count == 0;
            }
            catch
            {
                return true;
            }
        }

        public static List<GameEntry> LoadGames()
        {
            try
            {
                if (!File.Exists(DataFilePath)) return new List<GameEntry>();
                var raw = File.ReadAllText(DataFilePath);
                if (string.IsNullOrWhiteSpace(raw)) return new List<GameEntry>();
                var t = raw.Trim();
                if (t == "[]" || t == "[ ]" || t.Replace(" ", "") == "[]") return new List<GameEntry>();
                var list = System.Text.Json.JsonSerializer.Deserialize<List<GameEntry>>(t, JsonOpts) ?? new List<GameEntry>();
                Diagnostics.Log($"LoadGames: loaded {list.Count} from {DataFilePath}");
                return list;
            }
            catch (Exception ex)
            {
                Diagnostics.Log("LoadGames EX: " + ex.ToString());
                return new List<GameEntry>();
            }
        }

        public static void SaveGames(List<GameEntry> games)
        {
            try
            {
                Directory.CreateDirectory(DataDir);
                var json = System.Text.Json.JsonSerializer.Serialize(games ?? new List<GameEntry>(), JsonOpts);
                using (var fs = new FileStream(DataFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
                using (var sw = new StreamWriter(fs))
                {
                    sw.Write(json);
                    sw.Flush();
                    fs.Flush(true);
                }
                Diagnostics.Log($"SaveGames: wrote {games?.Count ?? 0} entries to {DataFilePath} (len={new FileInfo(DataFilePath).Length})");
            }
            catch (Exception ex)
            {
                Diagnostics.Log("SaveGames EX: " + ex.ToString());
            }
        }

        public static List<string> GetBlacklist()
        {
            try
            {
                if (!File.Exists(BlacklistPath)) return new List<string>();
                return File.ReadAllLines(BlacklistPath)
                           .Select(s => s?.Trim())
                           .Where(s => !string.IsNullOrWhiteSpace(s))
                           .Distinct(StringComparer.OrdinalIgnoreCase)
                           .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public static bool IsBlacklisted(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return false;
            try
            {
                return GetBlacklist().Any(x => string.Equals(x, key.Trim(), StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }

        public static void AddToBlacklist(string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key)) return;
                Directory.CreateDirectory(DataDir);
                var all = GetBlacklist();
                if (!all.Any(x => string.Equals(x, key.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    using var sw = new StreamWriter(BlacklistPath, append: true);
                    sw.WriteLine(key.Trim());
                }
                Diagnostics.Log("AddToBlacklist: " + key);
                BlacklistChanged?.Invoke();
            }
            catch
            {
                // ignore
            }
        }

        public static void RemoveFromBlacklist(string key)
        {
            try
            {
                if (!File.Exists(BlacklistPath)) return;
                var target = key?.Trim();
                var keep = GetBlacklist().Where(x => !string.Equals(x, target, StringComparison.OrdinalIgnoreCase)).ToList();
                File.WriteAllLines(BlacklistPath, keep);
                Diagnostics.Log("RemoveFromBlacklist: " + key);
                BlacklistChanged?.Invoke();
            }
            catch
            {
                // ignore
            }
        }
    }
}
