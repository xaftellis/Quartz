using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace Quartz.Controls
{
    public class AnimatedContextMenuStrip : ContextMenuStrip
    {
        private const int FadeDurationMilliseconds = 100;
        private readonly Timer fadeTimer = new Timer { Interval = 15 };
        private readonly Stopwatch fadeElapsed = new Stopwatch();

        public AnimatedContextMenuStrip()
        {
            fadeTimer.Tick += FadeTimer_Tick;
        }

        public AnimatedContextMenuStrip(IContainer container) : this()
        {
            container.Add(this);
        }

        protected override void OnOpening(CancelEventArgs e)
        {
            fadeTimer.Stop();
            fadeElapsed.Reset();
            base.OnOpening(e);
            if (IsDisposed || Disposing) return;

            // Opening handlers prepare the items. Keep the first frame transparent
            // while WinForms finishes sizing and positioning the complete menu.
            bool animate = !e.Cancel && !DesignMode && SystemInformation.UIEffectsEnabled &&
                SystemInformation.IsMenuAnimationEnabled && SystemInformation.IsMenuFadeEnabled;
            Opacity = animate ? 0 : 1;
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            if (!IsDisposed && !Disposing && Visible && Opacity < 1)
            {
                fadeElapsed.Restart();
                fadeTimer.Start();
            }
        }

        private void FadeTimer_Tick(object sender, EventArgs e)
        {
            if (IsDisposed || Disposing || !Visible)
            {
                fadeTimer.Stop();
                return;
            }

            Opacity = Math.Min(1, fadeElapsed.Elapsed.TotalMilliseconds / FadeDurationMilliseconds);
            if (Opacity >= 1)
            {
                fadeTimer.Stop();
                fadeElapsed.Reset();
            }
        }

        protected override void OnClosed(ToolStripDropDownClosedEventArgs e)
        {
            fadeTimer.Stop();
            fadeElapsed.Reset();
            Opacity = 1;
            base.OnClosed(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                fadeTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
