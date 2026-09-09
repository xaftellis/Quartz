using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuartzUpdater
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            string applyJobPath = GetArgumentValue(args, "--apply-job");
            if (string.IsNullOrWhiteSpace(applyJobPath) &&
                HasArgument(args, "--check-only"))
            {
                Environment.ExitCode = RunCheckOnlyAsync()
                    .GetAwaiter()
                    .GetResult();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            int ownerProcessId = GetOwnerProcessId(args, applyJobPath);
            IntPtr ownerWindowHandle = GetOwnerWindowHandle(args, applyJobPath);

            Application.Run(new Form1(
                applyJobPath,
                ownerProcessId,
                ownerWindowHandle));
        }

        internal static async Task<int> RunCheckOnlyAsync()
        {
            string logPath = Path.Combine(
                UpdaterClass.GetUpdatesRoot(),
                "Logs",
                "automatic-check.log");
            UpdateLog.Initialize(logPath, "Automatic update check started.");

            try
            {
                UpdateCheckResult result = await UpdaterClass.CheckForUpdatesAsync(
                    CancellationToken.None);

                UpdateLog.Write(result.IsUpdateAvailable
                    ? "Automatic check found Quartz " + result.Release.Version + "."
                    : "Automatic check completed; Quartz is up to date.");
                return 0;
            }
            catch (Exception exception)
            {
                UpdateLog.Write(exception);
                return 1;
            }
        }

        internal static bool HasArgument(string[] args, string name)
        {
            if (args == null || string.IsNullOrWhiteSpace(name))
                return false;

            foreach (string argument in args)
            {
                if (string.Equals(argument, name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        internal static int GetOwnerProcessId(string[] args, string applyJobPath)
        {
            return string.IsNullOrWhiteSpace(applyJobPath)
                ? GetPositiveIntegerArgument(args, "--parent-pid")
                : 0;
        }

        internal static int GetPositiveIntegerArgument(string[] args, string name)
        {
            string value = GetArgumentValue(args, name);
            int parsed;
            return int.TryParse(value, out parsed) && parsed > 0 ? parsed : 0;
        }

        internal static IntPtr GetOwnerWindowHandle(string[] args, string applyJobPath)
        {
            if (!string.IsNullOrWhiteSpace(applyJobPath))
                return IntPtr.Zero;

            string value = GetArgumentValue(args, "--owner-hwnd");
            long parsed;
            if (!long.TryParse(value, out parsed) || parsed <= 0)
                return IntPtr.Zero;

            try
            {
                return new IntPtr(parsed);
            }
            catch (OverflowException)
            {
                return IntPtr.Zero;
            }
        }

        internal static string GetArgumentValue(string[] args, string name)
        {
            if (args == null)
                return null;

            for (int index = 0; index < args.Length; index++)
            {
                string argument = args[index];

                if (string.Equals(argument, name, StringComparison.OrdinalIgnoreCase))
                {
                    if (index + 1 < args.Length)
                        return args[index + 1];

                    return null;
                }

                string prefix = name + "=";
                if (argument.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return argument.Substring(prefix.Length);
            }

            return null;
        }
    }
}
