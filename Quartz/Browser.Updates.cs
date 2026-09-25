using Quartz.Controls.ChromiumMenus;
using Quartz.Models;
using Quartz.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Quartz
{
    public partial class Browser
    {
        private ChromiumMenuItem updateAvailableToolStripMenuItem;
        private ChromiumMenuSeparator updateAvailableSeparator;

        private void InitializeUpdateAvailableMenuItem()
        {
            updateAvailableToolStripMenuItem = new ChromiumMenuItem
            {
                Name = "updateAvailableToolStripMenuItem",
                Text = "Update available",
                Visible = false
            };
            updateAvailableToolStripMenuItem.Click += UpdateAvailableToolStripMenuItem_Click;

            updateAvailableSeparator = new ChromiumMenuSeparator
            {
                Name = "updateAvailableSeparator",
                Visible = false
            };

            int settingsIndex = SettingsMenuStrip.Items.IndexOf(settingsToolStripMenuItem);
            int insertIndex = settingsIndex >= 0 ? settingsIndex + 1 : SettingsMenuStrip.Items.Count;
            SettingsMenuStrip.Items.Insert(insertIndex, updateAvailableSeparator);
            SettingsMenuStrip.Items.Insert(insertIndex + 1, updateAvailableToolStripMenuItem);
        }

        private void RefreshUpdateAvailableMenuItem()
        {
            UpdateStatusModel status = UpdateStatusService.Get();
            bool visible = status != null &&
                status.IsUpdateAvailable &&
                !string.IsNullOrWhiteSpace(status.LatestVersion);

            updateAvailableToolStripMenuItem.Visible = visible;
            updateAvailableSeparator.Visible = visible;
            updateAvailableToolStripMenuItem.Text = "Update available";
        }

        private void UpdateAvailableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string updaterPath = Path.Combine(Application.StartupPath, "QuartzUpdater.exe");
            if (!File.Exists(updaterPath))
            {
                MessageBox.Show(this,
                    "QuartzUpdater.exe was not found beside Quartz.exe.",
                    "Quartz update",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (Process updater = Process.Start(new ProcessStartInfo
                {
                    FileName = updaterPath,
                    Arguments = "--parent-pid " + Process.GetCurrentProcess().Id +
                        " --owner-hwnd " + Handle.ToInt64() +
                        Settings.GetUpdaterThemeArgument(),
                    WorkingDirectory = Application.StartupPath,
                    UseShellExecute = true
                }))
                {
                    if (updater == null)
                        throw new InvalidOperationException("Windows did not return the updater process.");
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(this,
                    "QuartzUpdater could not be opened. " + error.Message,
                    "Quartz update",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
