using PixelBeav.App.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;

namespace PixelBeav.App.ViewModels
{
    public class BlacklistWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<string> Items { get; } = new();
        public ICollectionView FilteredItems { get; }
        private string? _selected;
        public string? Selected { get => _selected; set { _selected = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasSelection)); } }
        private string _query = string.Empty;
        public string Query { get => _query; set { _query = value; OnPropertyChanged(); FilteredItems.Refresh(); } }
        public bool HasSelection => !string.IsNullOrWhiteSpace(Selected);

        public ICommand RestoreCommand { get; }
        public ICommand CloseCommand { get; }

        public BlacklistWindowViewModel()
        {
            foreach (var key in StorageService.GetBlacklist())
                Items.Add(key);

            FilteredItems = CollectionViewSource.GetDefaultView(Items);
            FilteredItems.Filter = o =>
            {
                if (string.IsNullOrWhiteSpace(Query)) return true;
                if (o is string s) return s.IndexOf(Query, StringComparison.OrdinalIgnoreCase) >= 0;
                return false;
            };

            RestoreCommand = new RelayCommand(_ => Restore(), _ => HasSelection);
            CloseCommand = new RelayCommand(w => (w as System.Windows.Window)?.Close());
            StorageService.BlacklistChanged += SyncFromService;
        }

        private void SyncFromService()
        {
            var set = new System.Collections.Generic.HashSet<string>(StorageService.GetBlacklist(), StringComparer.OrdinalIgnoreCase);
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                if (!set.Contains(Items[i])) Items.RemoveAt(i);
            }
            foreach (var k in set)
            {
                if (!Items.Contains(k)) Items.Add(k);
            }
            FilteredItems.Refresh();
        }

        private void Restore()
        {
            if (Selected == null) return;
            StorageService.RemoveFromBlacklist(Selected);
            Selected = null;
            OnPropertyChanged(nameof(HasSelection));
            FilteredItems.Refresh();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}