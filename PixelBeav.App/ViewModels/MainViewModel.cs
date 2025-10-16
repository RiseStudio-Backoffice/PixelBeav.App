using PixelBeav.App.Models;
using PixelBeav.App.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using PixelBeav.App.Views;

namespace PixelBeav.App.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<GameEntry> Games { get; } = new();
        public ObservableCollection<string> BlacklistItems { get; } = new();
        public ICollection<GameEntry> FilteredGames => Games.Where(g => !OnlyGames || g.IsGame).ToList();

        private GameEntry? _selectedGame;
        public GameEntry? SelectedGame { get => _selectedGame; set { _selectedGame = value; OnPropertyChanged(); } }

        private bool _onlyGames = true;
        public bool OnlyGames { get => _onlyGames; set { _onlyGames = value; OnPropertyChanged(); OnPropertyChanged(nameof(FilteredGames)); } }

        public ICommand SelectGameCommand { get; }
        public ICommand ImportSteamCommand { get; }
        public ICommand DeduplicateCommand { get; }
        public ICommand ChooseFolderCommand { get; }
        public ICommand RescanCommand { get; }
        public ICommand OpenFolderCommand { get; }
        public ICommand OpenThumbnailsCommand { get; }
        public ICommand DeleteGameCommand { get; }
        public ICommand ShowBlacklistCommand { get; }

        private StorageService.AppConfig _config;
        private HashSet<string> _blacklist = new(StringComparer.OrdinalIgnoreCase);

        public MainViewModel()
        {
            SelectGameCommand = new RelayCommand(p => SelectedGame = p as GameEntry);
            ImportSteamCommand = new RelayCommand(async _ => await ImportSteamAsync());
            DeduplicateCommand = new RelayCommand(_ => Deduplicate());
            ChooseFolderCommand = new RelayCommand(_ => ChooseFolder());
            RescanCommand = new RelayCommand(_ => Rescan());
            OpenFolderCommand = new RelayCommand(_ => OpenFolder());
            OpenThumbnailsCommand = new RelayCommand(_ => OpenThumbnails());
            DeleteGameCommand = new RelayCommand(p => DeleteGame(p as GameEntry));
            ShowBlacklistCommand = new RelayCommand(_ => ShowBlacklist());

            _config = StorageService.LoadConfig();
            _blacklist = StorageService.LoadBlacklist();
            BlacklistItems.Clear();
            foreach (var b in _blacklist) BlacklistItems.Add(b);

            var loaded = StorageService.LoadGames();
            foreach (var g in loaded)
            {
                var key = string.IsNullOrWhiteSpace(g.FolderPath) ? g.Title : g.FolderPath;
                if (!_blacklist.Contains(key))
                    Games.Add(g);
            }
            if (Games.Count == 0)
            {
                Games.Add(new GameEntry{ Title="Beispiel-Spiel", FolderPath=Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), HeaderImageUri="Assets/placeholder.png", ShortDescription="Lokaler Platzhalter-Eintrag.", IsGame=true });
            }

            _ = EnsureThumbsAsync();
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public event PropertyChangedEventHandler? PropertyChanged;

        private async Task ImportSteamAsync()
        {
            try
            {
                var steam = SteamService.FindSteamPath();
                if (steam == null)
                {
                    System.Windows.MessageBox.Show("Steam-Installation wurde nicht gefunden.", "Steam einlesen", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                var libs = SteamService.GetLibraryFolders(steam).ToList();
                var manifests = SteamService.GetAppManifestFiles(libs).ToList();
                var added = 0;
                foreach (var man in manifests)
                {
                    var dict = SteamService.ParseAppManifest(man);
                    if (!dict.TryGetValue("appid", out var appid)) continue;
                    dict.TryGetValue("installdir", out var dir);
                    var folder = dir ?? appid;

                    var keyCheck = string.IsNullOrWhiteSpace(folder) ? (dict.TryGetValue("name", out var n) ? n : folder) : folder;
                    if (_blacklist.Contains(keyCheck)) continue;

                    // Avoid duplicates by appid folder name in existing list
                    if (Games.Any(g => g.FolderPath.EndsWith(folder, StringComparison.OrdinalIgnoreCase))) continue;

                    var entry = new GameEntry
                    {
                        Title = dict.TryGetValue("name", out var name) ? name : folder,
                        FolderPath = FindInstallFolder(libs, folder) ?? folder,
                        IsGame = true
                    };
                    try
                    {
                        var (header, desc) = await SteamService.FetchStoreMetaAsync(appid);
                        entry.HeaderImageUri = header ?? "Assets/placeholder.png";
                        entry.ShortDescription = desc ?? "";
                        var cached = await ThumbnailService.GetOrCreateThumb320x160Async(entry.HeaderImageUri);
                        if (!string.IsNullOrWhiteSpace(cached)) entry.HeaderImageUri = cached;
                    }
                    catch { entry.HeaderImageUri = "Assets/placeholder.png"; }
                    Games.Add(entry);
                    added++;
                }
                if (added > 0) StorageService.SaveGames(Games.ToList());
                OnPropertyChanged(nameof(FilteredGames));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Fehler beim Steam-Import: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string? FindInstallFolder(List<string> libs, string folder)
        {
            foreach (var lib in libs)
            {
                var baseDir = lib.EndsWith("steamapps", StringComparison.OrdinalIgnoreCase) ? Path.Combine(lib, "common") : Path.Combine(lib, "steamapps", "common");
                var full = Path.Combine(baseDir, folder);
                if (Directory.Exists(full)) return full;
            }
            return null;
        }

        private void Deduplicate()
        {
            var unique = new Dictionary<string, GameEntry>(StringComparer.OrdinalIgnoreCase);
            foreach (var g in Games.ToList())
            {
                var key = string.IsNullOrWhiteSpace(g.FolderPath) ? g.Title : g.FolderPath;
                if (unique.ContainsKey(key))
                {
                    Games.Remove(g);
                }
                else unique[key] = g;
            }
            StorageService.SaveGames(Games.ToList());
            OnPropertyChanged(nameof(FilteredGames));
        }

        private void ChooseFolder()
        {
            using var dlg = new FolderBrowserDialog();

            dlg.Description = "Root-Ordner mit lokalen Spielen wählen";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _config.RootFolder = dlg.SelectedPath;
                StorageService.SaveConfig(_config);
            }
        }

        private void Rescan()
        {
            if (string.IsNullOrWhiteSpace(_config.RootFolder) || !Directory.Exists(_config.RootFolder))
            {
                System.Windows.MessageBox.Show("Bitte zuerst ‚Ordner wählen‘.", "Neu scannen", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var list = new List<GameEntry>();
            foreach (var dir in Directory.EnumerateDirectories(_config.RootFolder))
            {
                var title = Path.GetFileName(dir);
                var lower = title.ToLowerInvariant();
                var isGame = !(lower.Contains("dlc") || lower.Contains("tool") || lower.Contains("soundtrack"));

                var key = !string.IsNullOrWhiteSpace(dir) ? dir : title;
                if (_blacklist.Contains(key)) continue;

                list.Add(new GameEntry
                {
                    Title = title,
                    FolderPath = dir,
                    IsGame = isGame,
                    HeaderImageUri = "Assets/placeholder.png",
                    ShortDescription = isGame ? "Lokal erkannt – keine Steam-Metadaten (noch)." : "Ausgefiltert (kein Game)."
                });
            }
            Games.Clear();
            foreach (var e in list)
                Games.Add(e);
            StorageService.SaveGames(Games.ToList());
            OnPropertyChanged(nameof(FilteredGames));
        }

        private void OpenFolder()
        {
            if (SelectedGame == null) return;
            try
            {
                if (Directory.Exists(SelectedGame.FolderPath))
                    Process.Start(new ProcessStartInfo("explorer.exe", $"\"{SelectedGame.FolderPath}\"") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ordner konnte nicht geöffnet werden: {ex.Message}");
            }
        }

        private void OpenThumbnails()
        {
            System.Windows.MessageBox.Show("Thumbnails-Dialog (Stub) – wird in einem späteren Schritt implementiert.", "Thumbnails");
        }

        private async Task EnsureThumbsAsync()
        {
            foreach (var g in Games)
            {
                if (string.IsNullOrWhiteSpace(g.HeaderImageUri)) continue;
                if (g.HeaderImageUri.StartsWith("http", StringComparison.OrdinalIgnoreCase) || !File.Exists(g.HeaderImageUri))
                {
                    var cached = await ThumbnailService.GetOrCreateThumb320x160Async(g.HeaderImageUri);
                    if (!string.IsNullOrWhiteSpace(cached)) g.HeaderImageUri = cached;
                }
            }
            OnPropertyChanged(nameof(FilteredGames));
        }

        private void DeleteGame(GameEntry? g)
        {
            if (g == null) return;
            var key = string.IsNullOrWhiteSpace(g.FolderPath) ? g.Title : g.FolderPath;
            try

            {
                StorageService.AddToBlacklist(key);
                _blacklist.Add(key);
                Games.Remove(g);
                StorageService.SaveGames(Games.ToList());
                OnPropertyChanged(nameof(FilteredGames));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Eintrag konnte nicht entfernt werden: {ex.Message}", "Löschen", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ShowBlacklist()
        {
            var wnd = new BlacklistWindow();
            wnd.Owner = System.Windows.Application.Current?.MainWindow;
            wnd.ShowDialog();

            // Nach dem Schließen: Blacklist neu laden und Liste auffrischen
            _blacklist = StorageService.LoadBlacklist();
            BlacklistItems.Clear();
            if (!string.IsNullOrWhiteSpace(_config.RootFolder) && Directory.Exists(_config.RootFolder)) Rescan();
        }

    }
}