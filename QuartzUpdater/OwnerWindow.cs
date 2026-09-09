using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QuartzUpdater
{
    internal static class OwnerWindow
    {
        private const int GwlpHwndParent = -8;
        private const uint GwOwner = 4;

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr64(
            IntPtr windowHandle,
            int index,
            IntPtr newValue);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
        private static extern int SetWindowLong32(
            IntPtr windowHandle,
            int index,
            int newValue);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr windowHandle, uint command);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(
            IntPtr windowHandle,
            out NativeRect rectangle);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr windowHandle);

        internal static bool TryAttach(Form window, int quartzProcessId)
        {
            return TryAttach(window, quartzProcessId, IntPtr.Zero);
        }

        internal static bool TryAttach(
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

            try
            {
                window.ShowInTaskbar = false;
                IntPtr windowHandle = window.Handle;
                SetOwner(windowHandle, ownerHandle);

                if (GetWindow(windowHandle, GwOwner) != ownerHandle)
                {
                    window.ShowInTaskbar = true;
                    return false;
                }

                CenterOverOwner(window, ownerHandle);
                return true;
            }
            catch
            {
                window.ShowInTaskbar = true;
                return false;
            }
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

        private static void SetOwner(IntPtr windowHandle, IntPtr ownerHandle)
        {
            if (IntPtr.Size == 8)
                SetWindowLongPtr64(windowHandle, GwlpHwndParent, ownerHandle);
            else
                SetWindowLong32(windowHandle, GwlpHwndParent, ownerHandle.ToInt32());
        }

        private static void CenterOverOwner(Form window, IntPtr ownerHandle)
        {
            NativeRect ownerRectangle;
            if (!GetWindowRect(ownerHandle, out ownerRectangle))
                return;

            Rectangle workingArea = Screen.FromHandle(ownerHandle).WorkingArea;
            int ownerWidth = ownerRectangle.Right - ownerRectangle.Left;
            int ownerHeight = ownerRectangle.Bottom - ownerRectangle.Top;
            int left = ownerRectangle.Left + (ownerWidth - window.Width) / 2;
            int top = ownerRectangle.Top + (ownerHeight - window.Height) / 2;

            left = Math.Max(
                workingArea.Left,
                Math.Min(left, workingArea.Right - window.Width));
            top = Math.Max(
                workingArea.Top,
                Math.Min(top, workingArea.Bottom - window.Height));

            window.StartPosition = FormStartPosition.Manual;
            window.Location = new Point(left, top);
        }
    }
}
