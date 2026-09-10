using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace EasyTabs
{
    // WM_TIMER is only delivered after the input queue empties. Post a bounded
    // frame message instead, so a stream of mouse/WebView messages cannot starve
    // animation. The worker only posts; all layout and drawing stays on the UI.
    internal sealed class TabFrameScheduler : IDisposable
    {
        internal const int Message = 0x8000 + 71;
        private readonly object _sync = new object();
        private readonly IntPtr _window;
        private readonly Timer _timer;
        private bool _enabled, _queued, _disposed;

        [DllImport("user32.dll", EntryPoint = "PostMessageW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PostMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);

        internal TabFrameScheduler(IntPtr window)
        {
            _window = window;
            _timer = new Timer(PostFrame, null, Timeout.Infinite, Timeout.Infinite);
        }

        internal bool Enabled
        {
            get { lock (_sync) return _enabled; }
            set
            {
                lock (_sync)
                {
                    if (_disposed || _enabled == value) return;
                    _enabled = value;
                    // Stay below Windows' default 15.625 ms timer quantum. A
                    // 16 ms request can round up to two ticks (~32 fps).
                    _timer.Change(value ? 0 : Timeout.Infinite, value ? 15 : Timeout.Infinite);
                }
            }
        }

        private void PostFrame(object state)
        {
            lock (_sync)
            {
                if (_disposed || !_enabled || _queued) return;
                _queued = PostMessage(_window, Message, IntPtr.Zero, IntPtr.Zero);
            }
        }

        internal void Acknowledge() { lock (_sync) _queued = false; }

        public void Dispose()
        {
            lock (_sync)
            {
                if (_disposed) return;
                _disposed = true;
                _enabled = false;
                _timer.Dispose();
            }
        }
    }
}
