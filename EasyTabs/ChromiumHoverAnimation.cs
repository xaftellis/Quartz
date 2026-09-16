// Portions adapted from Chromium. See Chromium-LICENSE.txt.
using System;

namespace EasyTabs
{
    // Chromium 85 GlowHoverController + gfx::SlideAnimation. Reversals start
    // from the displayed value and shorten the duration by the remaining distance.
    internal sealed class ChromiumHoverAnimation
    {
        private readonly double _slideDuration;
        private readonly bool _easeInOnHide;
        private double _from, _target, _started, _duration;
        internal double Value { get; private set; }
        internal bool IsAnimating { get; private set; }

        internal ChromiumHoverAnimation(double duration = 200, bool easeInOnHide = true)
        {
            _slideDuration = duration;
            _easeInOnHide = easeInOnHide;
        }

        internal void Reset(double value)
        {
            Value = _from = _target = value;
            IsAnimating = false;
        }

        internal void Update(bool hovered, double now, bool animate)
        {
            double target = hovered ? 1 : 0;
            if (target != _target)
            {
                _from = Value;
                _target = target;
                _started = now;
                // LinearAnimation clamps short slides to one 60 Hz interval.
                _duration = Math.Max(16.666, _slideDuration * Math.Abs(_target - _from));
                IsAnimating = _from != _target;
            }
            if (!animate)
            {
                Value = _target;
                IsAnimating = false;
            }
            else if (IsAnimating)
            {
                double t = Math.Max(0, Math.Min(1, (now - _started) / _duration));
                double eased = _target == 1 || !_easeInOnHide ? 1 - (1 - t) * (1 - t) : t * t;
                Value = _from + (_target - _from) * eased;
                IsAnimating = t < 1;
            }
        }
    }
}
