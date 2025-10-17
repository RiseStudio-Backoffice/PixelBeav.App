using System.Windows;
using System.Windows.Controls.Primitives;

namespace PixelBeav.App.Views
{
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Set DataContext at runtime to avoid XAML resolver issues
            this.DataContext = new PixelBeav.App.ViewModels.MainViewModel();
        }

        private void Hamburger_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
            e.Handled = true;
        }
    }
}