// PixelBeav Patch: {STAMP}
// Summary: Correct regex/string escaping; compile-safe
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using PixelBeav.App.Models;

namespace PixelBeav.App.Services
{
    public static class SteamScanServiceFresh
    {
        public static List<GameEntry> ScanSteamGamesFresh()
        {
            var result = new List<GameEntry>();

            try
            {
                var steamRoot = ResolveSteamRoot();
                if (string.IsNullOrWhiteSpace(steamRoot) || !Directory.Exists(steamRoot))
                    return result;

                var libraries = GetLibraryPaths(steamRoot);
                foreach (var lib in libraries.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    var steamapps = Path.Combine(lib, "steamapps");
                    if (!Directory.Exists(steamapps)) continue;

                    foreach (var manifest in Directory.EnumerateFiles(steamapps, "appmanifest_*.acf", SearchOption.TopDirectoryOnly))
                    {
                        try
                        {
                            var appId = ExtractAppIdFromFileName(manifest);
                            var dict = ParseKeyValues(manifest);
                            dict.TryGetValue("name", out var name);
                            dict.TryGetValue("installdir", out var installDir);

                            var installPath = !string.IsNullOrWhiteSpace(installDir)
                                ? Path.Combine(steamapps, "common", installDir)
                                : Path.GetDirectoryName(manifest) ?? steamapps;

                            var entry = new GameEntry
                            {
                                Title = name ?? $"App {appId}",
                                FolderPath = installPath,
                                IsGame = true,
                                HeaderImageUri = appId > 0 ? $"https://steamcdn-a.akamaihd.net/steam/apps/{appId}/header.jpg" : null,
                                ShortDescription = string.Empty
                            };
                            result.Add(entry);
                        }
                        catch { /* ignore one manifest */ }
                    }
                }
            }
            catch
            {
                // ignore top-level
            }

            return result;
        }

        private static string? ResolveSteamRoot()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
                var path = key?.GetValue("SteamPath") as string;
                if (!string.IsNullOrWhiteSpace(path)) return path;

                var env = Environment.GetEnvironmentVariable("STEAM_PATH");
                if (!string.IsNullOrWhiteSpace(env)) return env;

                var progx86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                var guess = Path.Combine(progx86, "Steam");
                return guess;
            }
            catch { return null; }
        }

        private static IEnumerable<string> GetLibraryPaths(string steamRoot)
        {
            var libs = new List<string>();
            try
            {
                libs.Add(steamRoot);

                var vdf = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");
                if (!File.Exists(vdf)) return libs;

                var text = File.ReadAllText(vdf);
                var regex = new Regex(@"""path""\s*""([^""]+)""", RegexOptions.IgnoreCase);
                foreach (Match m in regex.Matches(text))
                {
                    var path = m.Groups[1].Value.Replace("\\\\", "\\").Trim();
                    if (Directory.Exists(path))
                        libs.Add(path);
                }
            }
            catch { }
            return libs;
        }

        private static int ExtractAppIdFromFileName(string manifestPath)
        {
            var name = Path.GetFileNameWithoutExtension(manifestPath);
            var m = Regex.Match(name ?? string.Empty, @"appmanifest_(\d+)", RegexOptions.IgnoreCase);
            return m.Success ? int.Parse(m.Groups[1].Value) : 0;
        }

        private static Dictionary<string,string> ParseKeyValues(string acfPath)
        {
            var dict = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var lines = File.ReadAllLines(acfPath);
                foreach (var raw in lines)
                {
                    var line = raw.Trim();
                    var m = Regex.Match(line, "^\\\"(?<k>[^\\\"]+)\\\"\\s+\\\"(?<v>[^\\\"]+)\\\"$");
                    if (m.Success)
                    {
                        var k = m.Groups["k"].Value;
                        var v = m.Groups["v"].Value;
                        dict[k] = v;
                    }
                }
            }
            catch { }
            return dict;
        }
    }
}
