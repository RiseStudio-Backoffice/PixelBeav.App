using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PixelBeav.App
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Global exception handlers
            this.DispatcherUnhandledException += (s, exArgs) =>
            {
                LogException("DispatcherUnhandledException", exArgs.Exception);
                System.Windows.MessageBox.Show($"Fehler beim Starten:\n{exArgs.Exception.Message}\n\nDetails: %APPDATA%/PixelBeav.App/log.txt", "PixelBeav", MessageBoxButton.OK, MessageBoxImage.Error);
                exArgs.Handled = true;
                Shutdown(-1);
            };
            AppDomain.CurrentDomain.UnhandledException += (s, exArgs) =>
            {
                if (exArgs.ExceptionObject is Exception ex) LogException("UnhandledException", ex);
            };
            TaskScheduler.UnobservedTaskException += (s, exArgs) =>
            {
                LogException("UnobservedTaskException", exArgs.Exception);
                exArgs.SetObserved();
            };

            try
            {
                var win = new Views.MainWindow();
                win.Show();
            }
            catch (Exception ex)
            {
                LogException("OnStartup", ex);
                System.Windows.MessageBox.Show($"Fehler beim Starten:\n{ex.Message}\n\nDetails: %APPDATA%/PixelBeav.App/log.txt", "PixelBeav", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(-1);
            }
        }

        private static void LogException(string source, Exception ex)
        {
            try
            {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PixelBeav.App");
                Directory.CreateDirectory(dir);
                var path = Path.Combine(dir, "log.txt");
                var sb = new StringBuilder();
                sb.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                sb.AppendLine(source);
                sb.AppendLine(ex.ToString());
                sb.AppendLine(new string('-', 60));
                File.AppendAllText(path, sb.ToString());
            }
            catch { }
        }
    }
}
