// PixelBeav Patch: PixelPatch-1.1.26  |  2025-10-18  |  Changed: yes
// Summary: Log to docs/diagnostics.log under project root during development
using System;
using System.IO;
using PixelBeav.App.Utils;

namespace PixelBeav.App.Services
{
    public static class Diagnostics
    {
        private static readonly string LogFile = Path.Combine(ProjectPaths.DocsDir, "diagnostics.log");

        public static void Log(string message)
        {
            try
            {
                Directory.CreateDirectory(ProjectPaths.DocsDir);
                var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}";
                File.AppendAllText(LogFile, line + Environment.NewLine);
            }
            catch { /* swallow */ }
        }

        public static string GetLogPath() => LogFile;
    }
}
