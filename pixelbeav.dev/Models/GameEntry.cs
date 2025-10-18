// PixelBeav Patch: PixelPatch-1.1.32  |  2025-10-18  |  Changed: yes
// PixelBeav Release: 1.1  |  2025-10-18  |  Changed: no
using System;
using System.ComponentModel;

namespace PixelBeav.App.Models
{
    public class GameEntry : INotifyPropertyChanged
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
        public string? HeaderImageUri { get; set; }
        public string ShortDescription { get; set; } = string.Empty;
        public bool IsGame { get; set; } = true;
        public int ThumbHeight { get; set; } = 160;

        public void ResetThumbnail()
        {
            HeaderImageUri = null;
            OnPropertyChanged(nameof(HeaderImageUri));
            ThumbHeight = 160;
            OnPropertyChanged(nameof(ThumbHeight));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
