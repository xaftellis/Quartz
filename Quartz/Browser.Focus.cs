using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Quartz
{
    public partial class Browser
    {
        private const uint EventObjectFocus = 0x8005;
        private IntPtr _webViewFocusHook;
        private WinEventCallback _webViewFocusCallback;
        private int _webViewFocusGeneration;
        private bool _webViewFocusDisposed;

        private void InitializeWebViewFocus()
        {
            VisibleChanged += UpdateWebViewFocusHook;
            HandleCreated += UpdateWebViewFocusHook;
            HandleDestroyed += RemoveWebViewFocusHook;
            wvWebView1.VisibleChanged += UpdateWebViewFocusHook;
            wvWebView1.HandleCreated += UpdateWebViewFocusHook;
            wvWebView1.HandleDestroyed += RemoveWebViewFocusHook;
            wvWebView1.Disposed += RemoveWebViewFocusHook;
            UpdateWebViewFocusHook(this, EventArgs.Empty);
        }

        private bool CanWatchWebViewFocus => !_webViewFocusDisposed &&
            !IsDisposed && !Disposing && IsHandleCreated && Visible &&
            !wvWebView1.IsDisposed && !wvWebView1.Disposing &&
            wvWebView1.IsHandleCreated && wvWebView1.Visible;

        private void UpdateWebViewFocusHook(object sender, EventArgs e)
        {
            if (!CanWatchWebViewFocus)
            {
                RemoveWebViewFocusHook(sender, e);
                return;
            }

            if (_webViewFocusHook != IntPtr.Zero)
                return;

            _webViewFocusCallback = OnNativeWebViewFocus;
            // OUTOFCONTEXT (0) delivers on this UI thread. Do not restrict the
            // process/thread: Chromium can own the focused descendant HWND.
            _webViewFocusHook = SetWinEventHook(EventObjectFocus, EventObjectFocus,
                IntPtr.Zero, _webViewFocusCallback, 0, 0, 0);
            if (_webViewFocusHook == IntPtr.Zero)
            {
                _webViewFocusCallback = null;
                Trace.TraceWarning("Could not install the WebView2 focus hook.");
            }
        }

        private void RemoveWebViewFocusHook(object sender, EventArgs e)
        {
            ++_webViewFocusGeneration; // Invalidate work queued before hide/recreation.
            if (_webViewFocusHook == IntPtr.Zero)
                return;

            if (!UnhookWinEvent(_webViewFocusHook))
            {
                // Retain the delegate while native code may still reference it.
                Trace.TraceWarning("Could not remove the WebView2 focus hook.");
                return;
            }
            _webViewFocusHook = IntPtr.Zero;
            _webViewFocusCallback = null;
        }

        private void DisposeWebViewFocus()
        {
            _webViewFocusDisposed = true;
            RemoveWebViewFocusHook(this, EventArgs.Empty);
        }

        private void OnNativeWebViewFocus(IntPtr hook, uint eventType, IntPtr hwnd,
            int objectId, int childId, uint eventThread, uint eventTime)
        {
            if (hook != _webViewFocusHook || eventType != EventObjectFocus ||
                hwnd == IntPtr.Zero || !CanWatchWebViewFocus)
                return;

            IntPtr webViewHandle = wvWebView1.Handle;
            if (hwnd != webViewHandle && !IsChild(webViewHandle, hwnd))
                return;

            int generation = _webViewFocusGeneration;
            try
            {
                BeginInvoke((Action)(() =>
                {
                    // Focus events are asynchronous; never act on a stale one.
                    if (generation != _webViewFocusGeneration ||
                        _webViewFocusHook == IntPtr.Zero || !CanWatchWebViewFocus ||
                        !wvWebView1.ContainsFocus)
                        return;

                    ActiveControl = wvWebView1;
                    CloseVisibleThreadMenus();
                }));
            }
            catch (InvalidOperationException)
            {
                // The form's handle may have been destroyed during shutdown.
            }
        }

        private static void CloseVisibleThreadMenus()
        {
            // Snapshot first: Closing handlers can destroy other menu windows.
            var menus = new List<ToolStripDropDown>();
            EnumThreadWindows(GetCurrentThreadId(), (hwnd, parameter) =>
            {
                var menu = Control.FromHandle(hwnd) as ToolStripDropDown;
                if (menu != null && !menu.IsDisposed && menu.Visible)
                    menus.Add(menu);
                return true;
            }, IntPtr.Zero);

            foreach (var menu in menus)
            {
                if (!menu.IsDisposed && !menu.Disposing && menu.Visible)
                    menu.Close(ToolStripDropDownCloseReason.AppFocusChange);
            }
        }

        private delegate void WinEventCallback(IntPtr hook, uint eventType,
            IntPtr hwnd, int objectId, int childId, uint eventThread, uint eventTime);
        private delegate bool ThreadWindowCallback(IntPtr hwnd, IntPtr parameter);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax,
            IntPtr module, WinEventCallback callback, uint processId, uint threadId, uint flags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWinEvent(IntPtr hook);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsChild(IntPtr parent, IntPtr child);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumThreadWindows(uint threadId,
            ThreadWindowCallback callback, IntPtr parameter);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();
    }
}
