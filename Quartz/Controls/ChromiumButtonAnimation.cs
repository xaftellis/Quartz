using System;

namespace Quartz.Controls
{
    // Chromium 85: InkDropImpl (SHOW_ON_RIPPLE), InkDropHighlight and
    // FloodFillInkDropRipple. Times are monotonic milliseconds, not frame counts.
    internal sealed class ChromiumButtonAnimation
    {
        internal enum InkState { Hidden, Pending, Triggered, Activated, Deactivated, Hiding }
        internal enum Curve { EaseIn, EaseInOut, FastOutSlowIn }

        private sealed class Transition
        {
            internal double From, To, Start, Duration;
            internal Curve Curve;
            internal double End => Start + Duration;

            internal double Value(double now)
            {
                if (now < Start) return From;
                if (Duration <= 0 || now >= End) return To;
                return From + (To - From) * Ease((now - Start) / Duration, Curve);
            }

            internal void Set(double from, double to, double start, double duration, Curve curve)
            {
                From = from;
                To = to;
                Start = start;
                Duration = duration;
                Curve = curve;
            }

            internal bool Running(double now) => From != To && now < End;
        }

        private readonly Transition _highlight = new Transition();
        private readonly Transition _radius = new Transition();
        private readonly Transition _opacity = new Transition();
        private bool _hovered;
        private double _finishedAt;
        private double? _fadeAt;
        private double _fadeDuration;

        internal bool AnimationsEnabled { get; set; } = true;
        internal InkState State { get; private set; }
        internal double Highlight(double now) => _highlight.Value(now);
        internal double Radius(double now) => _radius.Value(now);
        internal double Opacity(double now)
        {
            if (!_fadeAt.HasValue || now < _fadeAt.Value) return _opacity.Value(now);
            double progress = _fadeDuration == 0 ? 1 : Math.Min(1, (now - _fadeAt.Value) / _fadeDuration);
            return _opacity.Value(_fadeAt.Value) * (1 - Ease(progress, Curve.EaseInOut));
        }
        private double Duration(double value) => AnimationsEnabled ? value : 0;

        internal void SetHovered(bool hovered, double now)
        {
            Advance(now);
            if (_hovered == hovered) return;
            _hovered = hovered;
            if (State == InkState.Hidden || State == InkState.Hiding)
                SetHighlight(hovered, now, 250);
        }

        internal void Press(double now)
        {
            Advance(now);
            if (State == InkState.Activated) return;
            State = InkState.Pending;
            _fadeAt = null;
            _radius.Set(0, 1, now, Duration(240), Curve.FastOutSlowIn);
            // ACTION_PENDING opacity is immediate, with a pause until expansion ends.
            _opacity.Set(1, 1, now, Duration(240), Curve.EaseIn);
            SetHighlight(true, now, 250);
        }

        internal void Trigger(double now)
        {
            Advance(now);
            if (State == InkState.Activated) return;
            if (State != InkState.Pending) Press(now);
            State = InkState.Triggered;
            double start = Math.Max(now, _opacity.End);
            QueueFade(start);
            Advance(now);
        }

        internal void Activate(double now)
        {
            Advance(now);
            if (State == InkState.Activated) return;
            _fadeAt = null;
            if (State == InkState.Pending)
            {
                // A menu opened by this press finishes the pending expansion.
                // Chromium only queues zero-duration observer notifications here.
            }
            else
            {
                // InkDropImpl::DestroyHiddenTargetedAnimations discards a ripple
                // whose target is HIDDEN, ACTION_TRIGGERED or DEACTIVATED.
                _opacity.Set(0, 1, now, Duration(150), Curve.EaseIn);
                _radius.Set(0, 1, now, Duration(200), Curve.EaseInOut);
            }
            State = InkState.Activated;
            SetHighlight(true, now, 250);
        }

        internal void Deactivate(double now)
        {
            Advance(now);
            if (State != InkState.Activated) return;
            State = InkState.Deactivated;
            double start = Math.Max(now, _opacity.End);
            QueueFade(start);
            _finishedAt = Math.Max(_radius.End, _finishedAt);
            Advance(now);
        }

        internal void Cancel(double now)
        {
            Advance(now);
            if (State == InkState.Activated || State == InkState.Hidden || State == InkState.Hiding) return;
            bool discarded = State == InkState.Triggered || State == InkState.Deactivated;
            double opacity = discarded ? 0 : Opacity(now);
            double radius = discarded ? 0 : _radius.Value(now);
            State = InkState.Hiding;
            _fadeAt = null;
            _opacity.Set(opacity, 0, now, Duration(200), Curve.EaseInOut);
            _radius.Set(radius, 0, now, Duration(300), Curve.EaseInOut);
            _finishedAt = Math.Max(_opacity.End, _radius.End);
            SetHighlight(_hovered, now, 120);
            Advance(now);
        }

        internal void Advance(double now)
        {
            if ((State == InkState.Triggered || State == InkState.Deactivated || State == InkState.Hiding)
                && now >= _finishedAt)
            {
                State = InkState.Hidden;
                _fadeAt = null;
                _opacity.Set(0, 0, now, 0, Curve.EaseInOut);
                _radius.Set(0, 0, now, 0, Curve.EaseInOut);
                SetHighlight(_hovered, _finishedAt, 120);
            }
        }

        internal bool IsAnimating(double now)
        {
            Advance(now);
            return _highlight.Running(now) || _radius.Running(now) || _opacity.Running(now)
                || State == InkState.Triggered || State == InkState.Deactivated || State == InkState.Hiding;
        }

        internal void Reset(bool active, bool hovered, double now)
        {
            _hovered = hovered;
            _fadeAt = null;
            State = active ? InkState.Activated : InkState.Hidden;
            _highlight.Set(active || hovered ? 1 : 0, active || hovered ? 1 : 0, now, 0, Curve.EaseInOut);
            _opacity.Set(active ? 1 : 0, active ? 1 : 0, now, 0, Curve.EaseInOut);
            _radius.Set(active ? 1 : 0, active ? 1 : 0, now, 0, Curve.EaseInOut);
        }

        private void SetHighlight(bool visible, double now, double duration)
        {
            double target = visible ? 1 : 0;
            if (_highlight.To == target) return;
            // SetHighlight(true) recreates InkDropHighlight, whose FadeIn always
            // begins at zero. FadeOut preempts from the current opacity.
            _highlight.Set(visible ? 0 : _highlight.Value(now), target, now, Duration(duration), Curve.EaseInOut);
        }

        private void QueueFade(double start)
        {
            // Preserve the earlier fade-in until it finishes. Replacing it with
            // a delayed transition would jump straight to full opacity.
            _fadeAt = start;
            _fadeDuration = Duration(300);
            _finishedAt = start + _fadeDuration;
        }

        // ui/gfx/animation/tween.cc. EASE_IN_OUT is piecewise quadratic,
        // not smoothstep. FAST_OUT_SLOW_IN solves cubic-bezier(.4, 0, .2, 1).
        internal static double Ease(double value, Curve curve)
        {
            value = Math.Max(0, Math.Min(1, value));
            if (curve == Curve.EaseIn) return value * value;
            if (curve == Curve.EaseInOut)
                return value < .5 ? 2 * value * value : 1 - 2 * (1 - value) * (1 - value);
            double low = 0, high = 1;
            for (int i = 0; i < 24; i++)
            {
                double t = (low + high) / 2;
                double x = 3 * (1 - t) * (1 - t) * t * .4 + 3 * (1 - t) * t * t * .2 + t * t * t;
                if (x < value) low = t;
                else high = t;
            }
            double result = (low + high) / 2;
            return 3 * (1 - result) * result * result + result * result * result;
        }
    }
}
