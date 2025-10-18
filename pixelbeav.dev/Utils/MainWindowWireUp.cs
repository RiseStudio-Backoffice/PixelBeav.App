// PixelBeav Patch: PixelPatch-1.1.25  |  2025-10-18  |  Changed: yes
// Summary: Disambiguate Button type and null checks; fix CS0019/CS0104
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using PixelBeav.App.ViewModels;
using WpfButton = System.Windows.Controls.Button; // alias to avoid WinForms ambiguity
using DependencyObject = System.Windows.DependencyObject;

namespace PixelBeav.App.Utils
{
    public static class MainWindowWireUp
    {
        public static void Apply(Window wnd)
        {
            try
            {
                if (wnd is null) return;
                wnd.Loaded += (_, __) =>
                {
                    try
                    {
                        // Ensure DataContext
                        if (wnd.DataContext is not MainViewModel)
                            wnd.DataContext = new MainViewModel();

                        // Find Steam scan button by Name or Content
                        var byName = wnd.FindName("SteamScanButton") as WpfButton;
                        var btn = byName ?? FindButtonByContent(wnd, "Steam scannen") ?? FindButtonByContent(wnd, "Steam Scan");
                        if (btn is not null)
                        {
                            btn.Click -= SteamScanClickForwarder;
                            btn.Click += SteamScanClickForwarder;
                        }

                        // F9 shortcut as safety net
                        wnd.PreviewKeyDown += (s, e) =>
                        {
                            if (e.Key == Key.F9)
                            {
                                (wnd.DataContext as MainViewModel)?.RunFullScanFromUI();
                                e.Handled = true;
                                PixelBeav.App.Services.Diagnostics.Log("WireUp: F9 triggered FullRescanAsync");
                            }
                        };

                        PixelBeav.App.Services.Diagnostics.Log("WireUp: Loaded & bound.");
                    }
                    catch (Exception ex) { PixelBeav.App.Services.Diagnostics.Log("WireUp inner EX: " + ex); }
                };
            }
            catch (Exception ex) { PixelBeav.App.Services.Diagnostics.Log("WireUp EX: " + ex); }
        }

        private static void SteamScanClickForwarder(object? sender, RoutedEventArgs e)
        {
            try
            {
                var wnd = Window.GetWindow(sender as DependencyObject);
                (wnd?.DataContext as MainViewModel)?.RunFullScanFromUI();
                PixelBeav.App.Services.Diagnostics.Log("WireUp: Button click forwarded to RunFullScanFromUI()");
            }
            catch (Exception ex) { PixelBeav.App.Services.Diagnostics.Log("WireUp Click EX: " + ex); }
        }

        private static WpfButton? FindButtonByContent(DependencyObject root, string content)
        {
            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is WpfButton b)
                {
                    var txt = b.Content?.ToString()?.Trim();
                    if (string.Equals(txt, content, StringComparison.OrdinalIgnoreCase))
                        return b;
                }
                var deeper = FindButtonByContent(child, content);
                if (deeper is not null) return deeper;
            }
            return null;
        }
    }
}
