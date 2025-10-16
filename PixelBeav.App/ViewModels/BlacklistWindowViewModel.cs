using PixelBeav.App; // for RelayCommand
using PixelBeav.App.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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

        public ICommand RemoveSelectedCommand { get; }

        public BlacklistWindowViewModel()
        {
            foreach (var s in BlacklistService.GetAll())
                Items.Add(s);

            FilteredItems = CollectionViewSource.GetDefaultView(Items);
            FilteredItems.Filter = o =>
            {
                if (string.IsNullOrWhiteSpace(Query)) return true;
                if (o is string s) return s.IndexOf(Query, StringComparison.OrdinalIgnoreCase) >= 0;
                return false;
            };

            RemoveSelectedCommand = new RelayCommand(_ => RemoveSelected(), _ => HasSelection);
        }

        private void RemoveSelected()
        {
            if (Selected == null) return;
            var set = StorageService.LoadBlacklist(); set.Remove(Selected); StorageService.SaveBlacklist(set);
            Items.Remove(Selected);
            Selected = null;
            OnPropertyChanged(nameof(HasSelection));
            FilteredItems.Refresh();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}