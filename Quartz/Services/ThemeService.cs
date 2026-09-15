using EasyTabs;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Quartz.Services
{
    // All entry points run on the WinForms thread, including the Windows-theme
    // monitor. A theme change never recreates a window, tab or WebView.
    internal static class ThemeService
    {
        private static Timer monitor;
        private static bool applying;

        internal static void Start()
        {
            if (monitor != null) return;
            SettingsService.RefreshWindowsTheme();
            monitor = new Timer { Interval = 1000 };
            monitor.Tick += CheckWindowsTheme;
            monitor.Start();
            Application.ApplicationExit += Stop;
        }

        private static void Stop(object sender, EventArgs e)
        {
            monitor?.Dispose();
            monitor = null;
            Application.ApplicationExit -= Stop;
        }

        private static void CheckWindowsTheme(object sender, EventArgs e)
        {
            if (SettingsService.RefreshWindowsTheme() && SettingsService.GetAutoTheme() != null)
                ApplyCurrentTheme();
        }

        internal static void ChangeTheme(string preference)
        {
            SettingsService.RefreshWindowsTheme();
            SettingsService.Set("Theme", preference);
            ApplyCurrentTheme();
        }

        internal static void ApplyCurrentTheme()
        {
            if (applying) return;
            applying = true;
            try
            {
                using (SettingsService.BeginReadSnapshot())
                {
                    var forms = Application.OpenForms.Cast<Form>().ToArray();
                    var windows = (Program.EasyTabsContext?.OpenWindows.OfType<AppContainer>()
                        ?? Enumerable.Empty<AppContainer>()).Concat(forms.OfType<AppContainer>())
                        .Where(w => !w.IsDisposed && !w.Disposing && w.ProfileId == ProfileService.Current)
                        .Distinct().ToArray();
                    // Inactive tabs need not be present in Application.OpenForms.
                    var browsers = windows.SelectMany(w => w.Tabs).Select(t => t.Content).OfType<Browser>()
                        .Concat(forms.OfType<Browser>().Where(b => !(b.ParentTabs is AppContainer host) ||
                            host.ProfileId == ProfileService.Current)).Distinct().ToArray();

                    foreach (AppContainer window in windows) window.ApplyWindowTheme();
                    foreach (Browser browser in browsers)
                        if (!browser.IsDisposed && !browser.Disposing) browser.ApplyLiveTheme();

                    foreach (Form form in forms)
                    {
                        if (form.IsDisposed || form.Disposing || form is Browser || form is AppContainer)
                            continue;
                        if (form is Settings settings) settings.ApplyLiveTheme();
                        else if (form is History history) history.ApplyLiveTheme();
                        else if (form is ClearHistory clearHistory) clearHistory.ApplyLiveTheme();
                        else if (form.GetType().Namespace == typeof(Browser).Namespace)
                            NewControlThemeChanger.ChangeTheme(form);
                        form.Invalidate(true);
                    }

                    NewControlThemeChanger.ChangeControlTheme(ContextMenuProvider._contextMenuStripNormal);
                    NewControlThemeChanger.ChangeControlTheme(ContextMenuProvider._contextMenuStripTab);
                    foreach (AppContainer window in windows) window.RedrawTabs();
                }
            }
            finally { applying = false; }
        }
    }
}
