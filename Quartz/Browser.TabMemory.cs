using EasyTabs;
using Quartz.Libs;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Quartz
{
    public partial class Browser
    {
        private Timer _memoryTimer;
        private bool _memoryQueryPending, _memoryQueryAgain;
        private long _memoryDocumentStarted;
        private int _memoryVersion;
        public long? MemoryUsageBytes { get; private set; }
        public event EventHandler MemoryUsageChanged;

        private void InitializeTabMemory()
        {
            _memoryTimer = new Timer(components) { Interval = 120000 };
            _memoryTimer.Tick += (s, e) => RequestMemoryUsage();
        }

        private void InvalidateTabMemory()
        {
            ++_memoryVersion;
            _memoryDocumentStarted = TabMemorySampler.Timestamp;
            _memoryTimer.Stop();
            MemoryUsageBytes = null;
            MemoryUsageChanged?.Invoke(this, EventArgs.Empty);
        }

        public async void RequestMemoryUsage()
        {
            if (_previewDisposed || !_previewReady || _previewCrashed || IsDisposed || Disposing ||
                wvWebView1.IsDisposed || wvWebView1.CoreWebView2 == null) return;
            if (_memoryQueryPending) { _memoryQueryAgain = true; return; }
            _memoryQueryPending = true;
            int version = _memoryVersion;
            try
            {
                var core = wvWebView1.CoreWebView2;
                uint frame = core.FrameId;
                if (frame == 0) return;
                var snapshot = await TabMemorySampler.SampleAsync(core.Environment, (int)core.BrowserProcessId, _memoryDocumentStarted);
                if (_previewDisposed || IsDisposed || Disposing || version != _memoryVersion) return;
                long? bytes;
                snapshot.Pages.TryGetValue(frame, out bytes);
                MemoryUsageBytes = bytes > 0 ? bytes : null;
                MemoryUsageChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception error) when (error is COMException || error is InvalidOperationException ||
                error is NotImplementedException || error is NotSupportedException || error is ArgumentException)
            {
                if (!_previewDisposed && version == _memoryVersion)
                {
                    MemoryUsageBytes = null;
                    MemoryUsageChanged?.Invoke(this, EventArgs.Empty);
                }
                Debug.WriteLine("Tab memory sample unavailable: " + error.GetType().Name);
            }
            finally
            {
                _memoryQueryPending = false;
                bool again = _memoryQueryAgain;
                _memoryQueryAgain = false;
                if (again && !_previewDisposed && IsHandleCreated && !IsDisposed && !Disposing)
                    BeginInvoke(new Action(RequestMemoryUsage));
            }
        }
    }
}
