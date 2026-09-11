using EasyTabs;
using Microsoft.Web.WebView2.Core;
using Quartz.Libs;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quartz
{
    public partial class Browser
    {
        private TabPreviewCapture _tabPreview;
        private Timer _previewTimer;
        private bool _previewReady, _previewCrashed, _previewDisposed;
        private string _previewAddress = string.Empty;

        public string PreviewAddress => _previewAddress;
        public Image PreviewImage => _tabPreview?.Image;
        public bool IsPreviewReady => _previewReady;
        public bool IsPreviewCrashed => _previewCrashed;
        public event EventHandler PreviewChanged;

        private void InitializeTabPreview()
        {
            _tabPreview = new TabPreviewCapture(CaptureTabPreviewAsync);
            _tabPreview.Changed += (s, e) => PreviewChanged?.Invoke(this, EventArgs.Empty);
            _previewTimer = new Timer(components) { Interval = 2000 };
            _previewTimer.Tick += (s, e) => RequestPreview();
            VisibleChanged += (s, e) =>
            {
                if (_previewDisposed) return;
                _previewTimer.Enabled = Visible && _previewReady;
                if (Visible) RequestPreview();
            };
        }

        public async void RequestPreview()
        {
            if (_previewDisposed || !_previewReady || _previewCrashed || IsDisposed || Disposing) return;
            try { await _tabPreview.Request(); }
            catch (Exception error)
            {
                // Preview failure must not escape an async-void event and close
                // the browser. The normal page and navigation remain usable.
                Debug.WriteLine("Tab preview capture failed: " + error.GetType().Name);
            }
        }

        private async Task<Bitmap> CaptureTabPreviewAsync()
        {
            if (_previewDisposed || !_previewReady || _previewCrashed || IsDisposed || Disposing ||
                wvWebView1.IsDisposed || wvWebView1.CoreWebView2 == null ||
                wvWebView1.ClientSize.Width <= 0 || wvWebView1.ClientSize.Height <= 0) return null;
            var core = wvWebView1.CoreWebView2;
            try
            {
                using (var stream = new MemoryStream())
                {
                    // Capture the WebView surface, never the desktop or another
                    // tab's pixels. This does not show or activate a hidden tab.
                    await core.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, stream);
                    if (_previewDisposed || IsDisposed || Disposing || stream.Length == 0) return null;
                    stream.Position = 0;
                    using (var image = Image.FromStream(stream))
                    {
                        // Retain at most 512 x 288 pixels (2x the hover card),
                        // preserving aspect ratio. Full-resolution PNGs die here.
                        double fit = Math.Min(512.0 / image.Width, 288.0 / image.Height);
                        var thumbnail = new Bitmap(Math.Max(1, (int)Math.Round(image.Width * fit)),
                            Math.Max(1, (int)Math.Round(image.Height * fit)), PixelFormat.Format32bppPArgb);
                        using (var graphics = Graphics.FromImage(thumbnail))
                        {
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.DrawImage(image, new Rectangle(Point.Empty, thumbnail.Size));
                        }
                        return thumbnail;
                    }
                }
            }
            catch (COMException) { return null; }
            catch (InvalidOperationException) { return null; }
            catch (ArgumentException) { return null; }
        }

        private void PreviewNavigationStarting(CoreWebView2NavigationStartingEventArgs args)
        {
            if (args.Cancel || _previewDisposed) return;
            _previewReady = false; _previewCrashed = false;
            _previewAddress = args.Uri ?? string.Empty;
            _previewTimer.Stop();
            InvalidateTabMemory();
            _tabPreview.Invalidate();
        }

        private void PreviewContentLoading()
        {
            if (_previewDisposed) return;
            _previewReady = true;
            _previewAddress = wvWebView1.Source?.AbsoluteUri ?? _previewAddress;
            _previewTimer.Enabled = Visible;
            _memoryTimer.Start();
            PreviewChanged?.Invoke(this, EventArgs.Empty);
        }

        private void PreviewNavigationCompleted()
        {
            if (_previewDisposed) return;
            _previewAddress = wvWebView1.Source?.AbsoluteUri ?? _previewAddress;
            PreviewChanged?.Invoke(this, EventArgs.Empty);
            RequestPreview();
            RequestMemoryUsage();
        }

        private void PreviewProcessFailed()
        {
            if (_previewDisposed) return;
            _previewCrashed = true; _previewReady = false;
            _previewTimer.Stop();
            InvalidateTabMemory();
            _tabPreview.Invalidate();
        }

        private void DisposeTabPreview()
        {
            _previewDisposed = true;
            _previewTimer?.Dispose();
            _memoryTimer?.Dispose();
            ++_memoryVersion;
            MemoryUsageChanged = null;
            _tabPreview?.Dispose();
            PreviewChanged = null;
        }
    }
}
