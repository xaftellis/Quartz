// Portions adapted from Chromium, Copyright The Chromium Authors.
// See Chromium-LICENSE.txt and TabHoverCards.md for the pinned source revision.
using System;
using System.Drawing;

namespace EasyTabs
{
    // Chromium 85 TabHoverCardBubbleView's fade and slide delegates.
    // No timers or HWNDs here: interrupted animations can be checked deterministically.
    internal sealed class TabHoverCardAnimation
    {
        internal const int SlideDuration = 75, FadeInDuration = 200, FadeOutDuration = 150;
        private RectangleF _from, _target;
        private double _slideStart, _fadeStart;
        private int _fade; // 1 = in, -1 = out
        private bool _sliding;
        internal RectangleF Bounds { get; private set; }
        internal double Opacity { get; private set; }
        internal double TextProgress { get; private set; } = 1;
        internal bool IsAnimating => _sliding || _fade != 0;
        internal bool IsFadingOut => _fade == -1;

        internal void UpdateTarget(Rectangle bounds)
        {
            // Content/metrics refreshes must not restart a tab-switch animation.
            // Chromium's UpdateTargetBounds preserves the current animation clock.
            _target = bounds;
            if (!_sliding) Bounds = _from = bounds;
        }

        internal static double ShowDelay(double tabWidthDip)
        {
            // Chromium 85's default delay group: 300..800 ms, based on the
            // hovered tab (55-DIP pinned / 256-DIP standard), not the widest tab.
            if (tabWidthDip <= ChromiumTabMetrics.PinnedWidth) return 300;
            return 300 + 500 * Math.Log(tabWidthDip - ChromiumTabMetrics.PinnedWidth + 1) /
                Math.Log(ChromiumTabMetrics.StandardWidth - ChromiumTabMetrics.PinnedWidth + 1);
        }

        internal static double Clamp(double value) => Math.Max(0, Math.Min(1, value));

        internal static double Ease(double progress)
        {
            // gfx::Tween::FAST_OUT_SLOW_IN = cubic-bezier(.4, 0, .2, 1).
            progress = Clamp(progress);
            if (progress == 0 || progress == 1) return progress;
            double low = 0, high = 1, t = progress;
            for (int i = 0; i < 24; ++i)
            {
                double x = 3 * (1 - t) * (1 - t) * t * .4 + 3 * (1 - t) * t * t * .2 + t * t * t;
                if (x < progress) low = t; else high = t;
                t = (low + high) / 2;
            }
            return 3 * (1 - t) * t * t + t * t * t;
        }

        internal void Show(Rectangle bounds, double now, bool animate)
        {
            Bounds = _target = _from = bounds;
            _sliding = false;
            TextProgress = 1;
            _fadeStart = now;
            _fade = animate ? 1 : 0;
            Opacity = animate ? 0 : 1;
        }

        internal void Move(Rectangle bounds, double now, bool animate)
        {
            // Bounds is the last presented frame. Retarget from that exact
            // position, even if this input arrived between animation frames.
            if (_fade == -1) { _fade = 0; Opacity = 1; }
            _from = Bounds;
            _target = bounds;
            if (!animate)
            {
                Bounds = bounds;
                _sliding = false;
                TextProgress = 1;
                return;
            }
            // The 85 delegate restarts a full 75 ms slide on each new anchor.
            _slideStart = now;
            TextProgress = 0;
            _sliding = true;
        }

        internal void Hide(double now, bool animate)
        {
            Sample(now);
            _sliding = false;
            if (_fade == -1 && animate) return;
            _fade = animate ? -1 : 0;
            _fadeStart = now;
            Opacity = animate ? 1 : 0;
        }

        internal void Sample(double now)
        {
            if (_fade != 0)
            {
                double t = Clamp((now - _fadeStart) / (_fade == 1 ? FadeInDuration : FadeOutDuration));
                Opacity = _fade == 1 ? Ease(t) : 1 - Ease(t);
                if (t >= 1) _fade = 0;
            }
            if (!_sliding) return;
            double value = Ease((now - _slideStart) / SlideDuration);
            TextProgress = value;
            Bounds = ChromiumBoundsAnimation.Interpolate(Rectangle.Round(_from), Rectangle.Round(_target), value);
            if (value >= 1) { Bounds = _target; TextProgress = 1; _sliding = false; }
        }
    }
}
