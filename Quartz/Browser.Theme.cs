using Quartz.Libs;
using Quartz.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quartz
{
    public partial class Browser
    {
        internal void ApplyLiveTheme()
        {
            SuspendLayout();
            pnlTop.SuspendLayout();
            try
            {
                // Browser owns its toolbar/URL palette. A recursive dialog theme
                // pass would overwrite transparent buttons and rounded URL caps.
                LoadTheme();
                btnBack_EnabledChanged(btnBack, EventArgs.Empty);
                btnForward_EnabledChanged(btnForward, EventArgs.Empty);
                themeClass?.RefreshColors();
                bool showFavouriteIcon = SettingsService.Get("showFavouriteIcon") == "true";
                foreach (var button in pnlFavourites.Controls.OfType<Controls.FavouriteButton>())
                {
                    button.ChangeContent(() =>
                    {
                        NewControlThemeChanger.ChangeControlTheme(button);
                        var favourite = GetFavourite(button);
                        if (favourite != null)
                            button.SetIconVisibility(showFavouriteIcon,
                                () => FaviconHelper.GetFaviconFileExternalAsImage(favourite.WebAddress));
                        button.ThemeKey = SettingsService.Get("Theme");
                    }, false);
                    button.EndEntrance();
                }
                if (IsDefaultFavicon) Icon = FaviconHelper.GetDefaultFavicon16();
            }
            finally
            {
                pnlTop.ResumeLayout(true);
                ResumeLayout(true);
            }
            Invalidate(true);
            _ = ApplyInternalPageThemeAsync();
        }

        private async Task ApplyInternalPageThemeAsync()
        {
            try
            {
                if (IsDisposed || Disposing || wvWebView1.IsDisposed || wvWebView1.CoreWebView2 == null)
                    return;
                string script = InternalPageTheme.CreateScript(wvWebView1.Source, SettingsService.Get("Theme"));
                if (script != null) await wvWebView1.ExecuteScriptAsync(script);
            }
            catch (Exception error) when (error is InvalidOperationException || error is System.Runtime.InteropServices.COMException ||
                error is IOException || error is UnauthorizedAccessException)
            {
                // Navigation/disposal may interrupt this cosmetic update. A
                // completed navigation applies the latest theme again.
                Debug.WriteLine("Quartz page theme unavailable: " + error.Message);
            }
        }
    }
}
