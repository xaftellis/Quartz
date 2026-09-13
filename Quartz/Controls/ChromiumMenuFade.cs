using Quartz.Services;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Quartz.Controls
{
    // AnimateWindow runs synchronously on the calling thread. Updating layered
    // window opacity instead leaves ToolStrip's input/message loop available.
    internal sealed class ChromiumMenuFade : IDisposable
    {
        private const double DurationMilliseconds = 120;
        private readonly ToolStripDropDown menu;
        private readonly Action<byte> updateSurfaceOpacity;
        private readonly Stopwatch elapsed = new Stopwatch();
        private Timer timer;
        private bool pending;

        internal ChromiumMenuFade(ToolStripDropDown menu, Action<byte> updateSurfaceOpacity)
        {
            this.menu = menu;
            this.updateSurfaceOpacity = updateSurfaceOpacity;
        }

        internal void PrepareOpening()
        {
            Stop();
            pending = SystemInformation.IsMenuAnimationEnabled && SystemInformation.IsMenuFadeEnabled &&
                SystemInformation.UIEffectsEnabled &&
                !SystemInformation.TerminalServerSession && SettingsService.Get("Animation") == "true";
            // A nonzero alpha retains native hit testing from the first frame.
            // Set it before ShowWindow so there is no fully opaque first-frame flash.
            SetOpacity(pending ? (byte)1 : (byte)255);
        }

        internal void Start()
        {
            if (!pending || !menu.Visible || menu.IsDisposed) return;
            pending = false;
            if (timer == null)
            {
                timer = new Timer { Interval = 15 };
                timer.Tick += Tick;
            }
            elapsed.Restart();
            timer.Start();
        }

        private void Tick(object sender, EventArgs e)
        {
            if (!menu.Visible || menu.IsDisposed) { Reset(); return; }
            // Elapsed time, rather than frame counts, avoids stretching the fade
            // if a busy UI thread misses a tick. No sleeping or message pumping.
            double progress = Math.Min(1, elapsed.Elapsed.TotalMilliseconds / DurationMilliseconds);
            SetOpacity((byte)Math.Max(1, Math.Round(255 * progress)));
            if (progress >= 1) Stop();
        }

        internal void Reset()
        {
            Stop();
            if (!menu.IsDisposed) SetOpacity(255);
        }

        private void SetOpacity(byte opacity)
        {
            double value = opacity / 255.0;
            if (menu.Opacity != value) menu.Opacity = value;
            updateSurfaceOpacity(opacity);
        }

        private void Stop()
        {
            pending = false;
            timer?.Stop();
            elapsed.Reset();
        }

        public void Dispose()
        {
            Stop();
            timer?.Dispose();
            timer = null;
        }
    }
}
