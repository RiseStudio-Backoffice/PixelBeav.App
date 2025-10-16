using System.Windows;
using System.Windows.Input;

namespace PixelBeav.App.Views
{
    public partial class CollectionsWindow : Window
    {
        public string? SelectedCollection { get; private set; }

        public CollectionsWindow()
        {
            InitializeComponent();
            var list = PixelBeav.App.Services.SteamScanService.ListCollections();
            List.ItemsSource = list;
            // Kein Auto-Select -> verhindert sofortiges Schließen
        }

        private void Commit()
        {
            SelectedCollection = List.SelectedItem as string;
            if (!string.IsNullOrWhiteSpace(SelectedCollection))
            {
                DialogResult = true;
                Close();
            }
        }

        private void List_MouseDoubleClick(object sender, MouseButtonEventArgs e) => Commit();
        private void List_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) { if (e.Key == System.Windows.Input.Key.Enter) Commit(); }
    }
}
