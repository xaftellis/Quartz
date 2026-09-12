using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Quartz
{
    public partial class Browser
    {
        private const uint EventObjectFocus = 0x8005;
        private IntPtr _webViewFocusHook;
        private WinEventCallback _webViewFocusCallback;
        private bool _webViewFocusPending;

        private delegate void WinEventCallback(IntPtr hook, uint eventType, IntPtr window,
            int objectId, int childId, uint threadId, uint time);
        private delegate bool EnumThreadWindowCallback(IntPtr window, IntPtr parameter);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr module,
            WinEventCallback callback, uint processId, uint threadId, uint flags);
        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hook);
        [DllImport("user32.dll")]
        private static extern bool IsChild(IntPtr parent, IntPtr child);
        [DllImport("user32.dll")]
        private static extern bool EnumThreadWindows(uint threadId, EnumThreadWindowCallback callback, IntPtr parameter);
        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        private void InitializeWebViewFocus()
        {
            // Keep the delegate alive until the native hook has been removed.
            _webViewFocusCallback = WebViewNativeFocusChanged;
            wvWebView1.HandleCreated += UpdateWebViewFocusHook;
            wvWebView1.VisibleChanged += UpdateWebViewFocusHook;
            wvWebView1.HandleDestroyed += RemoveWebViewFocusHook;
            wvWebView1.GotFocus += WebViewGotFocus;
            VisibleChanged += UpdateWebViewFocusHook;
            HandleDestroyed += RemoveWebViewFocusHook;
            Disposed += RemoveWebViewFocusHook;
        }

        private void UpdateWebViewFocusHook(object sender, EventArgs e)
        {
            if (IsDisposed || Disposing || !Visible || !wvWebView1.IsHandleCreated || !wvWebView1.Visible)
            {
                RemoveWebViewFocusHook(sender, e);
                return;
            }

            // OUTOFCONTEXT delivers on this UI thread. Do not filter by process:
            // Chromium owns the native page HWND in a different process.
            if (_webViewFocusHook == IntPtr.Zero)
                _webViewFocusHook = SetWinEventHook(EventObjectFocus, EventObjectFocus,
                    IntPtr.Zero, _webViewFocusCallback, 0, 0, 0);
        }

        private void RemoveWebViewFocusHook(object sender, EventArgs e)
        {
            _webViewFocusPending = false;
            if (_webViewFocusHook == IntPtr.Zero) return;
            UnhookWinEvent(_webViewFocusHook);
            _webViewFocusHook = IntPtr.Zero;
        }

        private void WebViewNativeFocusChanged(IntPtr hook, uint eventType, IntPtr window,
            int objectId, int childId, uint threadId, uint time)
        {
            if (IsDisposed || Disposing || !Visible || !wvWebView1.IsHandleCreated || !wvWebView1.Visible) return;
            if (window == wvWebView1.Handle || IsChild(wvWebView1.Handle, window))
                WebViewGotFocus(this, EventArgs.Empty);
        }

        private void WebViewGotFocus(object sender, EventArgs e)
        {
            if (_webViewFocusPending || IsDisposed || Disposing || !IsHandleCreated) return;
            _webViewFocusPending = true;
            BeginInvoke((Action)(() =>
            {
                _webViewFocusPending = false;
                // A newer click or tab switch may have superseded this event.
                if (IsDisposed || Disposing || !Visible || !wvWebView1.Visible || !wvWebView1.ContainsFocus) return;

                // Native focus bypasses ContainerControl's ActiveControl update.
                // Synchronizing it restores the URL bar's Leave/Enter cycle.
                if (ActiveControl != wvWebView1) ActiveControl = wvWebView1;

                // Include component menus, detached submenus, tab/title-bar menus,
                // and menus owned by other Quartz windows on this UI thread.
                var menus = new List<ToolStripDropDown>();
                EnumThreadWindows(GetCurrentThreadId(), (window, parameter) =>
                {
                    var menu = Control.FromHandle(window) as ToolStripDropDown;
                    if (menu != null && menu.Visible) menus.Add(menu);
                    return true;
                }, IntPtr.Zero);
                foreach (var menu in menus)
                    if (!menu.IsDisposed && menu.Visible) menu.Close(ToolStripDropDownCloseReason.AppFocusChange);
            }));
        }
    }
}
