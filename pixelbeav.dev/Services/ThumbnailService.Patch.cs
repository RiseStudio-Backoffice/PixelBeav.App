// PixelBeav Patch: PixelPatch-1.1.32  |  2025-10-18  |  Changed: yes
// PixelBeav Release: 1.1  |  2025-10-18  |  Changed: no
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using PixelBeav.App.Models;

namespace PixelBeav.App.Services
{
    public static class ThumbnailCache
    {
        private static readonly string AppDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PixelBeav.App");
        private static readonly string ThumbsDir  = Path.Combine(AppDataDir, "thumbs");

        public static async Task<bool> EnsureLocalHeaderAsync(GameEntry game, string? remoteUrl)
        {
            try
            {
                if (game == null) return false;
                Directory.CreateDirectory(ThumbsDir);
                if (string.IsNullOrWhiteSpace(remoteUrl)) { game.HeaderImageUri = string.Empty; return false; }

                var name = SafeNameFromUrl(remoteUrl);
                var target = Path.Combine(ThumbsDir, name + ".jpg");

                if (!File.Exists(target))
                {
                    using var http = new HttpClient();
                    http.Timeout = TimeSpan.FromSeconds(15);
                    var data = await http.GetByteArrayAsync(remoteUrl);
                    await File.WriteAllBytesAsync(target, data);
                }

                game.HeaderImageUri = new Uri(target).AbsoluteUri;
                return true;
            }
            catch
            {
                game.HeaderImageUri = string.Empty;
                return false;
            }
        }

        private static string SafeNameFromUrl(string url)
        {
            foreach (var ch in Path.GetInvalidFileNameChars()) url = url.Replace(ch, '_');
            return (url ?? "thumb").GetHashCode().ToString("x");
        }
    }
}
