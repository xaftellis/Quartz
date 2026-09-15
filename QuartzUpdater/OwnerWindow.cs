using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QuartzUpdater
{
    internal static class OwnerWindow
    {
        private sealed class WindowOwner : IWin32Window
        {
            internal WindowOwner(IntPtr handle) { Handle = handle; }
            public IntPtr Handle { get; private set; }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr windowHandle);

        internal static bool TryShowDialog(
            Form window,
            int quartzProcessId,
            IntPtr requestedOwnerHandle)
        {
            if (window == null || quartzProcessId <= 0)
                return false;

            IntPtr ownerHandle = FindQuartzWindow(
                quartzProcessId,
                requestedOwnerHandle);
            if (ownerHandle == IntPtr.Zero)
                return false;

            // Supply the owner to WinForms before the window is shown. A modal
            // dialog preserves ownership through its normal visibility lifecycle.
            window.ShowInTaskbar = false;
            window.StartPosition = FormStartPosition.CenterParent;
            window.ShowDialog(new WindowOwner(ownerHandle));
            return true;
        }

        internal static IntPtr FindQuartzWindow(int quartzProcessId)
        {
            return FindQuartzWindow(quartzProcessId, IntPtr.Zero);
        }

        internal static IntPtr FindQuartzWindow(
            int quartzProcessId,
            IntPtr requestedOwnerHandle)
        {
            if (quartzProcessId <= 0)
                return IntPtr.Zero;

            try
            {
                using (Process process = Process.GetProcessById(quartzProcessId))
                {
                    if (process.HasExited || process.MainModule == null)
                        return IntPtr.Zero;

                    string expectedQuartzPath = Path.GetFullPath(
                        Path.Combine(AppContext.BaseDirectory, "Quartz.exe"));
                    string processPath = Path.GetFullPath(process.MainModule.FileName);

                    if (!string.Equals(
                        processPath,
                        expectedQuartzPath,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return IntPtr.Zero;
                    }

                    if (requestedOwnerHandle != IntPtr.Zero &&
                        IsWindow(requestedOwnerHandle))
                    {
                        uint requestedProcessId;
                        GetWindowThreadProcessId(
                            requestedOwnerHandle,
                            out requestedProcessId);

                        if (requestedProcessId == (uint)quartzProcessId)
                            return requestedOwnerHandle;
                    }

                    process.Refresh();
                    IntPtr handle = process.MainWindowHandle;
                    return handle != IntPtr.Zero && IsWindow(handle)
                        ? handle
                        : IntPtr.Zero;
                }
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(
            IntPtr windowHandle,
            out uint processId);

    }
}
