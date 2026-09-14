using Microsoft.Web.WebView2.Core;
using Quartz.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quartz
{
    public partial class Settings
    {
        private async Task<bool> PrepareUpdateIndicatorAsync()
        {
            try
            {
                if (LoadingProgress.CoreWebView2 == null)
                {
                    var environment = await CoreWebView2Environment.CreateAsync(null,
                        _browser.GetLocalPath() + @"\Xaftellis\Quartz\UserData\WebView2\", null);
                    if (IsDisposed || Disposing) return false;
                    await LoadingProgress.EnsureCoreWebView2Async(environment,
                        environment.CreateCoreWebView2ControllerOptions());
                }
                if (IsDisposed || Disposing) return false;

                using (SettingsService.BeginReadSnapshot())
                {
                    LoadingProgress.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                    LoadingProgress.CoreWebView2.MemoryUsageTargetLevel = SettingsService.Get("MemoryUsage") == "low"
                        ? CoreWebView2MemoryUsageTargetLevel.Low : CoreWebView2MemoryUsageTargetLevel.Normal;
                    NewControlThemeChanger.ChangeControlTheme(LoadingProgress);
                    string theme = SettingsService.Get("Theme");
                    string colour = theme == "xmas" ? "xmas_green" : theme == "black" ? "white" :
                        theme == "aqua" ? "blue" : theme;
                    string path = Path.Combine(Application.StartupPath, "assets", "throbber",
                        "throbber_medium_" + colour + ".svg");
                    LoadingProgress.ZoomFactor = 1;
                    LoadingProgress.Source = new Uri(path);
                }
                return true;
            }
            catch (Exception error)
            {
                // A decorative spinner must not prevent the updater from opening.
                Debug.WriteLine("Update indicator unavailable: " + error.Message);
                return false;
            }
        }

        private async void CheckingForUpdatesAnimation()
        {
            string[] dots = { "", ".", "..", "..." };
            int frame = 0;
            while (updating && !IsDisposed && !Disposing)
            {
                txtUpdate.Text = "Checking For Updates" + dots[frame++ % dots.Length];
                await Task.Delay(500);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (updating || _loadingSettings) return;
            string updaterPath = Path.Combine(Application.StartupPath, "QuartzUpdater.exe");
            if (!File.Exists(updaterPath))
            {
                MessageBox.Show(this, "QuartzUpdater.exe was not found beside Quartz.exe.",
                    "Check for updates", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            updating = true;
            buttonChech.Visible = false;
            txtUpdate.Visible = true;
            CheckingForUpdatesAnimation();
            try
            {
                bool indicatorReady = await PrepareUpdateIndicatorAsync();
                if (IsDisposed || Disposing) return;
                LoadingProgress.Visible = indicatorReady;
                pictureBox1.Visible = !indicatorReady;
                await Task.Delay(1500);
                if (IsDisposed || Disposing) return;

                using (var updater = new Process())
                {
                    updater.StartInfo = new ProcessStartInfo
                    {
                        FileName = updaterPath,
                        Arguments = "--parent-pid " + Process.GetCurrentProcess().Id +
                            " --owner-hwnd " + Handle.ToInt64(),
                        WorkingDirectory = Application.StartupPath,
                        UseShellExecute = true
                    };
                    var exited = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    updater.EnableRaisingEvents = true;
                    updater.Exited += (exitSender, exitArgs) => exited.TrySetResult(true);
                    if (!updater.Start()) throw new InvalidOperationException("The updater did not start.");
                    await exited.Task;
                }
            }
            catch (Exception error)
            {
                if (!IsDisposed && !Disposing)
                    MessageBox.Show(this, "QuartzUpdater could not be opened. " + error.Message,
                        "Check for updates", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                updating = false;
                if (!IsDisposed && !Disposing)
                {
                    buttonChech.Visible = true;
                    txtUpdate.Visible = false;
                    LoadingProgress.Visible = false;
                    pictureBox1.Visible = true;
                }
            }
        }
    }
}
