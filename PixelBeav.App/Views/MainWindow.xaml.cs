using System.Windows;
using System.Windows.Controls.Primitives;

namespace PixelBeav.App.Views
{
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (this.DataContext == null)
                this.DataContext = new PixelBeav.App.ViewModels.MainViewModel();
        }

        private void Gear_Click(object sender, RoutedEventArgs e)
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