// PixelBeav Patch: PixelPatch-1.1.32  |  2025-10-18  |  Changed: yes
// PixelBeav Release: 1.1  |  2025-10-18  |  Changed: no
using System.Windows;

namespace PixelBeav.App.Views
{
    public partial class PromptWindow : Window
    {
        public string? ResultText { get; private set; }

        public PromptWindow(string title, string prompt)
        {
            InitializeComponent();
            TitleText.Text = title;
            PromptText.Text = prompt;
            InputBox.Focus();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            ResultText = InputBox.Text;
            DialogResult = true;
            Close();
        }
    }
}