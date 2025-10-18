// PixelBeav Patch: PixelPatch-1.1.32  |  2025-10-18  |  Changed: yes
// PixelBeav Release: 1.1  |  2025-10-18  |  Changed: no
using System;
using System.IO;
using System.Threading.Tasks;
using PixelBeav.App.Models;

namespace PixelBeav.App.Services
{
    public static class ThumbnailService
    {
        private const string AssetsPlaceholder = "Assets/placeholder.png";

        public static void ClearCache()
        {
            // TODO: Implement cache clearing if/when a cache location is defined.
            // Stub exists to satisfy callers without compile errors.
        }

        public static async Task<bool> EnsureAndGenerateAsync(GameEntry game)
        {
            if (game == null) return false;
            try
            {
                // If a valid path already exists, nothing to do.
                var path = game.HeaderImageUri;
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                    return true;

                // Placeholder fallback (regeneration stub).
                await Task.Yield();
                if (File.Exists(AssetsPlaceholder))
                {
                    game.HeaderImageUri = AssetsPlaceholder;
                    return true;
                }

                game.HeaderImageUri = string.Empty;
                return false;
            }
            catch
            {
                game.HeaderImageUri = AssetsPlaceholder;
                return false;
            }
        }
    }
}
