using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EasyTabs
{
    // Chromium's Widget::ShouldPaintAsActive is separate from focus. In WinForms,
    // native ownership identifies the browser's dialogs, menus and caption overlay.
    internal sealed class WindowActivation : IDisposable
    {
        private readonly WinEventProc _callback;
        private IntPtr _hook;

        internal WindowActivation(Action foregroundChanged)
        {
            _callback = (hook, eventId, window, objectId, childId, thread, time) => foregroundChanged();
            // EVENT_SYSTEM_FOREGROUND, WINEVENT_OUTOFCONTEXT. Also observe other
            // processes so an owned dialog cannot keep the browser bright on Alt+Tab.
            _hook = SetWinEventHook(3, 3, IntPtr.Zero, _callback, 0, 0, 0);
        }

        internal static bool BelongsToWindow(IntPtr foreground, TitleBarTabs browser)
        {
            if (foreground == IntPtr.Zero || !browser.IsHandleCreated || browser.IsDisposed) return false;
            if (foreground == browser.Handle) return true;
            var visited = new HashSet<IntPtr>();
            for (IntPtr current = foreground; current != IntPtr.Zero && visited.Add(current);)
            {
                if (current == browser.Handle) return true;
                Control control = Control.FromHandle(current);
                // A separate browser is its own activation unit, even if owned.
                if (control is TitleBarTabs) return false;
                if (control?.TopLevelControl is TitleBarTabs topLevelBrowser)
                    return topLevelBrowser == browser;
                if (control is ToolStripDropDown menu)
                {
                    while (menu.OwnerItem?.Owner is ToolStripDropDown parentMenu) menu = parentMenu;
                    Control source = (menu as ContextMenuStrip)?.SourceControl ?? menu.OwnerItem?.Owner;
                    if (source != null && source.TopLevelControl != null)
                    {
                        current = source.TopLevelControl.Handle;
                        continue;
                    }
                }
                // GA_ROOT includes child WebView HWNDs, but does not skip over a
                // different browser as GA_ROOTOWNER would. GW_OWNER handles native
                // dialogs (including ShowDialog()'s implicit owner) and nested forms.
                IntPtr root = GetAncestor(current, 2);
                current = root != IntPtr.Zero && root != current ? root : GetWindow(current, 4);
            }
            return false;
        }

        public void Dispose()
        {
            if (_hook != IntPtr.Zero) { UnhookWinEvent(_hook); _hook = IntPtr.Zero; }
        }

        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr GetAncestor(IntPtr window, uint flags);
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr window, uint command);
        private delegate void WinEventProc(IntPtr hook, uint eventId, IntPtr window, int objectId, int childId, uint thread, uint time);
        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint min, uint max, IntPtr module, WinEventProc callback, uint process, uint thread, uint flags);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWinEvent(IntPtr hook);
    }
}
