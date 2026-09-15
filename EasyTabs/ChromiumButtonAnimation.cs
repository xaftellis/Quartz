using System;

namespace EasyTabs
{
    // Chromium 85 InkDropImpl (SHOW_ON_RIPPLE), InkDropHighlight and
    // FloodFillInkDropRipple. Rendering and the shared frame scheduler stay outside.
    internal sealed class ChromiumButtonAnimation
    {
        private enum Ripple { Hidden, Pending, Triggered, Hiding }
        private readonly double _hoverDuration;
        private Ripple _ripple;
        private bool _held, _inside;
        private double _hoverFrom, _hoverTarget, _hoverStarted, _hoverTime;
        private double _pressedAt, _fadeAt, _hideAt, _hideOpacity, _hideProgress;
        private bool _hoverAnimating, _rippleAnimating;

        internal ChromiumButtonAnimation(bool immediateHover = false)
        {
            _hoverDuration = immediateHover ? 0 : 250;
        }

        internal float HoverOpacity { get; private set; }
        internal float InkOpacity { get; private set; }
        internal float InkProgress { get; private set; }
        internal bool IsAnimating => _hoverAnimating || _rippleAnimating;

        private static double Progress(double elapsed, double duration) =>
            duration == 0 ? 1 : Math.Max(0, Math.Min(1, elapsed / duration));

        private static double EaseInOut(double t) =>
            t < .5 ? 2 * t * t : 1 - 2 * (1 - t) * (1 - t);

        private void Highlight(bool visible, double now, double duration)
        {
            double target = visible ? 1 : 0;
            if (_hoverTarget == target) return;
            // InkDropHighlight::FadeIn recreates the highlight at zero opacity.
            // FadeOut preempts from the current opacity, with its full duration.
            _hoverFrom = visible ? 0 : HoverOpacity / .16;
            _hoverTarget = target;
            _hoverStarted = now;
            _hoverTime = duration;
        }

        private void StartRipple(double now)
        {
            _ripple = Ripple.Pending;
            _pressedAt = now;
            InkOpacity = .14f;
            InkProgress = 0;
            _rippleAnimating = true;
            Highlight(true, now, 250);
        }

        internal void Press(double now)
        {
            Sample(now, true);
            _held = _inside = true;
            StartRipple(now);
        }

        internal void Release(double now, bool inside)
        {
            if (!_held) return;
            Update(inside, now, true);
            _held = false;
            if (!inside) return;
            // ACTION_TRIGGERED queues its fade behind ACTION_PENDING's opacity
            // pause. A fast click still finishes the 240 ms expansion first.
            _ripple = Ripple.Triggered;
            _fadeAt = Math.Max(now, _pressedAt + 240);
            _rippleAnimating = true;
        }

        private void HideRipple(double now)
        {
            if (_ripple == Ripple.Hidden || _ripple == Ripple.Hiding) return;
            _hideAt = now;
            _hideOpacity = InkOpacity;
            _hideProgress = InkProgress;
            _ripple = Ripple.Hiding;
            _rippleAnimating = true;
            if (!_inside) Highlight(false, now, 120);
        }

        internal void Cancel(double now)
        {
            Sample(now, true);
            _held = _inside = false;
            HideRipple(now);
            Highlight(false, now, _hoverDuration);
        }

        internal void Reset()
        {
            _held = _inside = false;
            _ripple = Ripple.Hidden;
            _hoverFrom = _hoverTarget = 0;
            HoverOpacity = InkOpacity = InkProgress = 0;
            _hoverAnimating = _rippleAnimating = false;
        }

        internal void Update(bool hovered, double now, bool animate)
        {
            Sample(now, animate);
            bool changed = _inside != hovered;
            _inside = hovered;
            if (_held && changed)
            {
                if (hovered) StartRipple(now);
                else HideRipple(now);
            }
            // Pointer exit does not remove the highlight while a committed click
            // is playing. It fades over 120 ms when that ripple becomes hidden.
            if (_ripple == Ripple.Hidden || _ripple == Ripple.Hiding)
                Highlight(hovered, now, _hoverDuration);
            Sample(now, animate);
        }

        private void Sample(double now, bool animate)
        {
            _rippleAnimating = false;
            if (_ripple == Ripple.Pending || _ripple == Ripple.Triggered)
            {
                double grow = animate ? Progress(now - _pressedAt, 240) : 1;
                InkProgress = (float)TabLoadingIndicator.FastOutSlowIn(grow);
                double fade = _ripple == Ripple.Triggered
                    ? (animate ? Progress(now - _fadeAt, 300) : 1) : 0;
                InkOpacity = (float)(.14 * (1 - EaseInOut(fade)));
                _rippleAnimating = animate && (grow < 1 || _ripple == Ripple.Triggered);
                if (fade == 1)
                {
                    _ripple = Ripple.Hidden;
                    InkProgress = 0;
                    _rippleAnimating = false;
                    if (!_inside) Highlight(false, animate ? _fadeAt + 300 : now, 120);
                }
            }
            else if (_ripple == Ripple.Hiding)
            {
                double fade = animate ? Progress(now - _hideAt, 200) : 1;
                double shrink = animate ? Progress(now - _hideAt, 300) : 1;
                InkOpacity = (float)(_hideOpacity * (1 - EaseInOut(fade)));
                InkProgress = (float)(_hideProgress * (1 - EaseInOut(shrink)));
                _rippleAnimating = animate && shrink < 1;
                if (shrink == 1) _ripple = Ripple.Hidden;
            }

            double t = animate ? Progress(now - _hoverStarted, _hoverTime) : 1;
            HoverOpacity = (float)(.16 * (_hoverFrom + (_hoverTarget - _hoverFrom) * EaseInOut(t)));
            _hoverAnimating = animate && t < 1 && _hoverFrom != _hoverTarget;
        }
    }
}
