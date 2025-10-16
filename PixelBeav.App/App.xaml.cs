namespace PixelBeav.App
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);
            var wnd = new Views.MainWindow();
            wnd.Show();
        }
    }
}