using Quartz.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Quartz
{
    internal static class QuartzAutomaticUpdateChecker
    {
        private const string SettingName = "AutomaticallyCheckForUpdates";
        private static readonly TimeSpan CheckInterval = TimeSpan.FromDays(7);

        public static bool IsEnabled
        {
            get
            {
                return !string.Equals(
                    MainSettingsService.Get(SettingName),
                    "false",
                    StringComparison.OrdinalIgnoreCase);
            }
        }

        public static void SetEnabled(bool enabled)
        {
            MainSettingsService.Set(
                SettingName,
                enabled ? "true" : "false");
        }

        public static bool IsCheckDue()
        {
            QuartzUpdateStatus status;
            return !QuartzUpdateStatusStore.TryRead(out status) ||
                status.IsOlderThan(CheckInterval);
        }

        public static bool TryStartIfDue()
        {
            try
            {
                if (!IsEnabled || !IsCheckDue())
                    return false;

                string updaterPath = Path.Combine(
                    Application.StartupPath,
                    "QuartzUpdater.exe");
                if (!File.Exists(updaterPath))
                    return false;

                Process process = Process.Start(new ProcessStartInfo
                {
                    FileName = updaterPath,
                    Arguments = "--check-only",
                    WorkingDirectory = Application.StartupPath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });

                if (process == null)
                    return false;

                process.Dispose();
                return true;
            }
            catch
            {
                // An automatic check must never stop Quartz from opening.
                return false;
            }
        }
    }
}
