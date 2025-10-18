// PixelBeav Patch: PixelPatch-1.1.24  |  2025-10-18  |  Changed: yes
// Summary: Single MainWindow startup + wire-up call + exception handler
using System.Windows.Threading;

namespace PixelBeav.App
{
    public partial class App : global::System.Windows.Application
    {
        protected override void OnStartup(global::System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);
            this.ShutdownMode = global::System.Windows.ShutdownMode.OnMainWindowClose;
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            var wnd = new Views.MainWindow();
            this.MainWindow = wnd;
            PixelBeav.App.Utils.MainWindowWireUp.Apply(wnd);
            wnd.Show();

            PixelBeav.App.Services.Diagnostics.Log("App started; MainWindow shown.");
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try { global::System.Windows.MessageBox.Show(e.Exception?.ToString() ?? "Unbekannter Fehler", "Unerwarteter Fehler"); }
            finally { e.Handled = true; }
        }
    }
}
