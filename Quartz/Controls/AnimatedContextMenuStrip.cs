using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Quartz.Controls
{
    // Shared version of Quartz's original stopwatch/timer fade. Chromium154's
    // menu visibility animation is 150 ms, linear (window_animations.cc:203–240,
    // 294–340; layer_animator.h:435), gated by Windows client-area animations.
    internal sealed class MenuFadeAnimation : IDisposable
    {
        internal const int DurationMilliseconds = 150;
        private readonly Timer timer = new Timer { Interval = 15 };
        private readonly Stopwatch elapsed = new Stopwatch();
        private Action<double> paint;
        private Action completed;
        private double from, to;
        internal static bool SystemEnabled
        {
            get { bool enabled = true; return SystemParametersInfo(0x1042, 0, ref enabled, 0) ? enabled : !SystemInformation.TerminalServerSession; }
        }
        internal static double Value(double from, double to, double milliseconds) => from + (to - from) * Math.Max(0, Math.Min(1, milliseconds / DurationMilliseconds));
        internal MenuFadeAnimation() { timer.Tick += Tick; }
        internal void Start(double start, double end, Action<double> frame, Action done = null)
        {
            Stop(); from = start; to = end; paint = frame; completed = done;
            elapsed.Restart(); paint(from); timer.Start();
        }
        private void Tick(object sender, EventArgs e)
        {
            double ms = elapsed.Elapsed.TotalMilliseconds;
            paint?.Invoke(Value(from, to, ms));
            if (ms >= DurationMilliseconds) { var done = completed; Stop(); done?.Invoke(); }
        }
        internal void Stop() { timer.Stop(); elapsed.Reset(); paint = null; completed = null; }
        public void Dispose() { Stop(); timer.Dispose(); }
        [DllImport("user32.dll")] private static extern bool SystemParametersInfo(uint action, int param, ref bool value, int flags);
    }

    // Retained for legacy callers; the Skia menus use only MenuFadeAnimation,
    // never this ToolStrip host or its renderer.
    public class AnimatedContextMenuStrip : ContextMenuStrip
    {
        private readonly MenuFadeAnimation fade = new MenuFadeAnimation();
        public AnimatedContextMenuStrip() { }
        public AnimatedContextMenuStrip(IContainer container) : this() { container.Add(this); }
        protected override void OnOpening(CancelEventArgs e)
        {
            fade.Stop(); base.OnOpening(e);
            if (!IsDisposed && !Disposing) Opacity = !e.Cancel && !DesignMode && MenuFadeAnimation.SystemEnabled ? 0 : 1;
        }
        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            if (!IsDisposed && !Disposing && Visible && Opacity < 1)
                fade.Start(0, 1, value => { if (!IsDisposed) Opacity = value; });
        }
        protected override void OnClosed(ToolStripDropDownClosedEventArgs e)
        { fade.Stop(); Opacity = 1; base.OnClosed(e); }
        protected override void Dispose(bool disposing) { if (disposing) fade.Dispose(); base.Dispose(disposing); }
    }
}
