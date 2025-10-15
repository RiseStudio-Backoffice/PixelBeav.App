using PixelBeav.App.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PixelBeav.App.ViewModels
{
    public class BlacklistViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<string> Items { get; } = new ObservableCollection<string>();
        private string? _selected;
        public string? Selected { get => _selected; set { _selected = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasSelection)); } }
        public bool HasSelection => !string.IsNullOrWhiteSpace(Selected);

        public ICommand RestoreSelectedCommand { get; }

        public BlacklistViewModel()
        {
            foreach (var s in BlacklistService.GetAll().OrderBy(s => s))
                Items.Add(s);
            RestoreSelectedCommand = new RelayCommand(_ => Restore());
        }

        private void Restore()
        {
            if (Selected == null) return;
            BlacklistService.Remove(Selected);
            Items.Remove(Selected);
            OnPropertyChanged(nameof(HasSelection));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
