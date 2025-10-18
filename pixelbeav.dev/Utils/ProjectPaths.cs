// PixelBeav Patch: PixelPatch-1.1.26  |  2025-10-18  |  Changed: yes
// Summary: Resolve project-root paths for dev-time storage
using System;
using System.IO;
using System.Reflection;

namespace PixelBeav.App.Utils
{
    public static class ProjectPaths
    {
        public static string Root
        {
            get
            {
                try
                {
                    // Typical VS layout: bin/Debug/netX → go 3 up to project folder
                    var baseDir = AppContext.BaseDirectory;
                    var guess = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
                    if (Directory.Exists(guess) && File.Exists(Path.Combine(guess, "pixelbeav.dev.csproj")))
                        return guess;

                    // Walk up to find folder named PixelBeav.App
                    var dir = new DirectoryInfo(baseDir);
                    for (int i = 0; i < 8 && dir != null; i++)
                    {
                        if (string.Equals(dir.Name, "PixelBeav.App", StringComparison.OrdinalIgnoreCase))
                            return dir.FullName;
                        var probe = Path.Combine(dir.FullName, "pixelbeav.dev.csproj");
                        if (File.Exists(probe)) return dir.FullName;
                        dir = dir.Parent;
                    }
                }
                catch { }
                // Fallback: use base directory
                return AppContext.BaseDirectory;
            }
        }

        public static string DataDir => Path.Combine(Root, "Data");
        public static string DocsDir => Path.Combine(Root, "docs");
    }
}
