using System;

namespace PixelBeav.App.Models
{
    public class GameEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
        public string? HeaderImageUri { get; set; }
        public string ShortDescription { get; set; } = string.Empty;
        public bool IsGame { get; set; } = true;
        public int ThumbHeight { get; set; } = 160;
    }
}
