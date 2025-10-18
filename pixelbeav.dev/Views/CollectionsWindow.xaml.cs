// PixelBeav Patch: PixelPatch-1.1.32  |  2025-10-18  |  Changed: yes
// PixelBeav Release: 1.1  |  2025-10-18  |  Changed: no
using System.Windows;

namespace PixelBeav.App.Views
{
    public partial class CollectionsWindow : Window
    {
        public string? SelectedCollection { get; private set; }
        public CollectionsWindow()
        {
            InitializeComponent();
            // Absichtlich keine Steam-Abfragen in diesem Rollback-Build.
        }
    }
}