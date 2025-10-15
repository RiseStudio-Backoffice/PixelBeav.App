using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace PixelBeav.App.Services
{
    public static class ThumbnailService
    {
        private static string CacheDir
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PixelBeav.App", "cache", "thumbs");

        private static string Hash(string input)
        {
            using var sha1 = SHA1.Create();
            var bytes = sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }

        public static async Task<string?> GetOrCreateThumb320x160Async(string? source)
        {
            if (string.IsNullOrWhiteSpace(source)) return null;
            Directory.CreateDirectory(CacheDir);
            var hash = Hash(source);
            var dest = Path.Combine(CacheDir, $"{hash}_320x150.png");
            if (File.Exists(dest)) return dest;

            try
            {
                Stream? srcStream = null;
                if (Uri.TryCreate(source, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                {
                    using var http = new HttpClient();
                    http.Timeout = TimeSpan.FromSeconds(15);
                    var bytes = await http.GetByteArrayAsync(uri);
                    srcStream = new MemoryStream(bytes);
                }
                else
                {
                    if (!File.Exists(source)) return null;
                    srcStream = File.OpenRead(source);
                }

                using (srcStream)
                using (var img = Image.FromStream(srcStream, useEmbeddedColorManagement:true, validateImageData:true))
                using (var bmp = new Bitmap(320, 150))
                using (var g = Graphics.FromImage(bmp))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.Clear(System.Drawing.Color.Transparent);
                    // Stretch to fill exactly 320x160 (aspect ratio may change intentionally)
                    g.DrawImage(img, new Rectangle(0, 0, 320, 150));
                    bmp.Save(dest, ImageFormat.Png);
                }
                return dest;
            }
            catch
            {
                return null;
            }
        }
    }
}
