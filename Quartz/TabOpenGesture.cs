using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Quartz
{
    // WebView2 does not expose WindowOpenDisposition. Preserve native input at
    // the source rather than sampling modifier keys after its asynchronous IPC.
    // This is necessarily a bounded correlation, not Blink's CurrentInputEvent.
    internal sealed class TabOpenGesture : IDisposable
    {
        internal const int MaximumAgeMilliseconds = 1000;
        // Keep the delegate rooted until Windows confirms removal of its hook.
        private static readonly HashSet<TabOpenGesture> LiveHooks = new HashSet<TabOpenGesture>();
        private readonly Control _webView;
        private readonly Func<bool> _isActive;
        private readonly MouseHook _callback;
        private IntPtr _hook;
        private TabOpenDisposition? _pending;
        private int _inputTime;

        internal TabOpenGesture(Control webView, Func<bool> isActive)
        {
            _webView = webView;
            _isActive = isActive;
            _callback = OnMouse;
            webView.VisibleChanged += UpdateHook;
            webView.HandleCreated += UpdateHook;
            webView.HandleDestroyed += StopHook;
            webView.KeyDown += OnKeyDown;
            UpdateHook(this, EventArgs.Empty);
        }

        internal void Record(MouseButtons button, Keys modifiers, int time)
        {
            _pending = TabOpenPolicy.FromClick(button, modifiers);
            _inputTime = time;
        }

        internal void Clear() => _pending = null;

        internal TabOpenDisposition Take(bool userInitiated, bool popup, int time)
        {
            var input = _pending;
            Clear();
            if (userInitiated && input.HasValue &&
                unchecked((uint)(time - _inputTime)) <= MaximumAgeMilliseconds &&
                input != TabOpenDisposition.CurrentTab && input != TabOpenDisposition.SaveToDisk)
                return input.Value;

            // NavigationPolicyForCreateWindow: ordinary _blank/window.open is
            // foreground, including allowed script requests without a gesture.
            return popup ? TabOpenDisposition.NewWindow : TabOpenDisposition.NewForegroundTab;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && _isActive())
                Record(MouseButtons.Left, e.Modifiers, Environment.TickCount);
            else if (e.KeyCode != Keys.ControlKey && e.KeyCode != Keys.ShiftKey && e.KeyCode != Keys.Menu)
                Clear();
        }

        private void UpdateHook(object sender, EventArgs e)
        {
            if (_webView.IsDisposed || !_webView.IsHandleCreated || !_webView.Visible)
            {
                StopHook(sender, e);
                return;
            }
            if (_hook != IntPtr.Zero) return;
            // A thread message filter misses Chromium's child HWNDs. The low
            // level hook sees input across that process boundary; only button
            // events over this visible, active WebView are retained.
            _hook = SetWindowsHookEx(14, _callback, GetModuleHandle(null), 0);
            if (_hook == IntPtr.Zero)
                Trace.TraceWarning("Could not observe WebView tab-opening mouse gestures: {0}", Marshal.GetLastWin32Error());
            else LiveHooks.Add(this);
        }

        private IntPtr OnMouse(int code, IntPtr message, IntPtr data)
        {
            try
            {
                if (code < 0) return CallNextHookEx(_hook, code, message, data);
                int kind = message.ToInt32();
                if (kind == 0x201 || kind == 0x204 || kind == 0x207) Clear();
                if (kind == 0x202 || kind == 0x208)
                {
                    var input = Marshal.PtrToStructure<MouseInput>(data);
                    IntPtr target = WindowFromPoint(input.Point);
                    if (!_webView.IsDisposed && _webView.IsHandleCreated && _webView.Visible &&
                        _isActive() && (target == _webView.Handle || IsChild(_webView.Handle, target)))
                    {
                        Keys modifiers = Keys.None;
                        if (GetAsyncKeyState(0x11) < 0) modifiers |= Keys.Control;
                        if (GetAsyncKeyState(0x10) < 0) modifiers |= Keys.Shift;
                        if (GetAsyncKeyState(0x12) < 0) modifiers |= Keys.Alt;
                        Record(kind == 0x208 ? MouseButtons.Middle : MouseButtons.Left,
                            modifiers, unchecked((int)input.Time));
                    }
                    else Clear();
                }
            }
            catch (Exception error)
            {
                Clear();
                Trace.TraceWarning("Could not observe tab-opening input: {0}", error.Message);
            }
            return CallNextHookEx(_hook, code, message, data);
        }

        private void StopHook(object sender, EventArgs e)
        {
            Clear();
            if (_hook == IntPtr.Zero) return;
            if (UnhookWindowsHookEx(_hook))
            {
                _hook = IntPtr.Zero;
                LiveHooks.Remove(this);
            }
            else Trace.TraceWarning("Could not remove tab-opening mouse hook.");
        }

        public void Dispose()
        {
            _webView.VisibleChanged -= UpdateHook;
            _webView.HandleCreated -= UpdateHook;
            _webView.HandleDestroyed -= StopHook;
            _webView.KeyDown -= OnKeyDown;
            StopHook(this, EventArgs.Empty);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativePoint { internal int X, Y; }
        [StructLayout(LayoutKind.Sequential)]
        private struct MouseInput
        {
            internal NativePoint Point;
            internal uint MouseData, Flags, Time;
            internal UIntPtr ExtraInfo;
        }
        private delegate IntPtr MouseHook(int code, IntPtr message, IntPtr data);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int id, MouseHook callback, IntPtr module, uint thread);
        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hook);
        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr message, IntPtr data);
        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(NativePoint point);
        [DllImport("user32.dll")]
        private static extern bool IsChild(IntPtr parent, IntPtr child);
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int key);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetModuleHandle(string name);
    }
}
