// PixelBeav Patch: PixelPatch-1.1.32  |  2025-10-18  |  Changed: yes
// PixelBeav Release: 1.1  |  2025-10-18  |  Changed: no
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using PixelBeav.App.Models;
using PixelBeav.App.Services;

namespace PixelBeav.App.ViewModels
{
    public partial class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<GameEntry> AllGames { get; } = new();
        public ICollectionView FilteredGames { get; private set; }

        private string _filter = string.Empty;
        public string Filter
        {
            get => _filter;
            set { if (_filter != value) { _filter = value; OnPropertyChanged(); FilteredGames.Refresh(); } }
        }

        private GameEntry? _selectedGame;
        public GameEntry? SelectedGame
        {
            get => _selectedGame;
            set { if (_selectedGame != value) { _selectedGame = value; OnPropertyChanged(); CommandManager.InvalidateRequerySuggested(); } }
        }

        public ICommand ShowBlacklistCommand { get; }
        public ICommand OpenFolderCommand { get; }
        public ICommand RescanCommand { get; }
        public ICommand DeleteGameCommand { get; }
        public ICommand ScanSteamCommand { get; }
        public ICommand ClearAllThumbnailsCommand { get; }
        public ICommand RemoveThumbnailCommand { get; }

        public MainViewModel()
        {
            var saved = StorageService.LoadGames() ?? new System.Collections.Generic.List<GameEntry>();
            foreach (var g in saved) AllGames.Add(g);

            FilteredGames = CollectionViewSource.GetDefaultView(AllGames);
            FilteredGames.Filter = FilterPredicate;

            ShowBlacklistCommand      = new RelayCommand(_ => ShowBlacklist());
            OpenFolderCommand         = new RelayCommand(_ => OpenSelectedFolder(), _ => SelectedGame != null);
            RescanCommand             = new RelayCommand(_ => ReloadFromStorage());
            DeleteGameCommand         = new RelayCommand(p => MoveToBlacklist((p as GameEntry) ?? SelectedGame!), p => (p as GameEntry) != null || SelectedGame != null);
            ScanSteamCommand          = new RelayCommand(_ => ScanSteamAndPersist());
            ClearAllThumbnailsCommand = new RelayCommand(_ => ClearAllThumbnails(), _ => AllGames.Count > 0);
            RemoveThumbnailCommand    = new RelayCommand(p => RemoveThumbnail((p as GameEntry) ?? SelectedGame!), p => (p as GameEntry) != null || SelectedGame != null);
        }

        private bool FilterPredicate(object? obj)
        {
            if (obj is not GameEntry g) return false;
            if (string.IsNullOrWhiteSpace(Filter)) return true;
            return (g.Title ?? string.Empty).IndexOf(Filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void ReloadFromStorage()
        {
            var current = StorageService.LoadGames() ?? new System.Collections.Generic.List<GameEntry>();
            AllGames.Clear();
            foreach (var g in current) AllGames.Add(g);
            FilteredGames.Refresh();
        }

        private void ScanSteamAndPersist()
        {
            try
            {
                var scanned = SteamScanService.ScanSteamGames() ?? new System.Collections.Generic.List<GameEntry>();
                var filtered = scanned.Where(g => !StorageService.IsBlacklisted(GetKey(g))).ToList();
                StorageService.SaveGames(filtered);
                AllGames.Clear();
                foreach (var g in filtered) AllGames.Add(g);
                FilteredGames.Refresh();
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString(), "Scan fehlgeschlagen");
            }
        }

        private void ClearAllThumbnails()
        {
            foreach (var g in AllGames) { g.ResetThumbnail(); }
            AllGames.Clear();
            StorageService.SaveGames(new System.Collections.Generic.List<GameEntry>());
            ThumbnailService.ClearCache();
            FilteredGames.Refresh();
            CommandManager.InvalidateRequerySuggested();
        }

        private void RemoveThumbnail(GameEntry game)
        {
            if (game == null) return;
            AllGames.Remove(game);
            StorageService.SaveGames(AllGames.ToList());
            FilteredGames.Refresh();
            CommandManager.InvalidateRequerySuggested();
        }

        private string GetKey(GameEntry g)
            => (g?.Title ?? string.Empty).Trim();

        private void MoveToBlacklist(GameEntry? game)
        {
            if (game == null) return;
            var key = GetKey(game);
            StorageService.AddToBlacklist(key);
            AllGames.Remove(game);
            StorageService.SaveGames(AllGames.ToList());
            FilteredGames.Refresh();
        }

        private void ShowBlacklist()
        {
            var win = new PixelBeav.App.Views.BlacklistWindow();
            win.Owner = System.Windows.Application.Current?.MainWindow;
            win.ShowDialog();
        }

        private void OpenSelectedFolder()
        {
            var path = SelectedGame?.FolderPath;
            if (string.IsNullOrWhiteSpace(path) || !System.IO.Directory.Exists(path)) return;
            try { System.Diagnostics.Process.Start("explorer.exe", path); } catch { }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
