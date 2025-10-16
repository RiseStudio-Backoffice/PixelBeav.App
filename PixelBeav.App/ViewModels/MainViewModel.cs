using PixelBeav.App.Models;
using PixelBeav.App.Services;
using PixelBeav.App.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace PixelBeav.App.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<GameEntry> AllGames { get; } = new();
        public ICollectionView FilteredGames { get; }
        private GameEntry? _selectedGame;
        public GameEntry? SelectedGame { get => _selectedGame; set { _selectedGame = value; OnPropertyChanged(); } }

        private string _searchText = string.Empty;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); FilteredGames.Refresh(); } }

        public ICommand DeleteGameCommand { get; }
        public ICommand ShowBlacklistCommand { get; }
        public ICommand OpenCollectionsCommand { get; }

        public MainViewModel()
        {
            foreach (var g in StorageService.LoadGames())
                AllGames.Add(g);

            FilteredGames = CollectionViewSource.GetDefaultView(AllGames);
            FilteredGames.Filter = FilterPredicate;

            DeleteGameCommand = new RelayCommand(p => MoveToBlacklist(p as GameEntry ?? SelectedGame), p => (p as GameEntry) != null || SelectedGame != null);
            ShowBlacklistCommand = new RelayCommand(_ => ShowBlacklist());
            OpenCollectionsCommand = new RelayCommand(_ => OpenCollections());

            StorageService.BlacklistChanged += () => System.Windows.Application.Current?.Dispatcher?.Invoke(() => FilteredGames.Refresh());
        }

        private bool FilterPredicate(object? obj)
        {
            if (obj is not GameEntry g) return false;
            if (StorageService.IsBlacklisted(GetKey(g))) return false;
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var s = SearchText.Trim();
                return (g.Title ?? string.Empty).IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            return true;
        }

        private static string GetKey(GameEntry g)
        {
            foreach (var name in new[] { "SteamAppId", "AppId", "Id", "ExecutablePath", "ExePath", "FolderPath" })
            {
                var prop = typeof(GameEntry).GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop != null)
                {
                    var val = prop.GetValue(g) as string;
                    if (!string.IsNullOrWhiteSpace(val)) return val;
                }
            }
            return g.Title ?? string.Empty;
        }

        private void MoveToBlacklist(GameEntry? g)
        {
            if (g == null) return;
            var key = GetKey(g);
            StorageService.AddToBlacklist(key);
            FilteredGames.Refresh();
        }

        private void ShowBlacklist()
        {
            var wnd = new BlacklistWindow();
            wnd.Owner = System.Windows.Application.Current?.MainWindow;
            wnd.Show();
        }

        private void OpenCollections()
        {
            var dlg = new CollectionsWindow();
            dlg.Owner = System.Windows.Application.Current?.MainWindow;
            if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.SelectedCollection))
            {
                var added = SteamScanService.AddCollectionAndReturn(dlg.SelectedCollection!);
                // Neu hinzugekommene Thumbnails ganz oben einfügen (Reihenfolge beibehalten)
                for (int i = added.Count - 1; i >= 0; i--)
                    AllGames.Insert(0, added[i]);
                FilteredGames.Refresh();
            }
        }

        private void ReloadFromStorage()
        {
            var current = StorageService.LoadGames();
            AllGames.Clear();
            foreach (var g in current) AllGames.Add(g);
            FilteredGames.Refresh();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
