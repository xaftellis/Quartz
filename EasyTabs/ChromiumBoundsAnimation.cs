using System;
using System.Collections.Generic;
using System.Drawing;

namespace EasyTabs
{
    // Chromium 85: ui/views/animation/bounds_animator.{h,cc} and
    // ui/gfx/animation/tween.cc. Each view keeps its own start, target and clock;
    // repainting an unchanged target must not restart its animation.
    internal sealed class ChromiumBoundsAnimation
    {
        private const double DurationMilliseconds = 200;

        private sealed class Item
        {
            internal Rectangle Bounds, Start, Target;
            internal double Started;
            internal bool Animating;
        }

        private readonly Dictionary<object, Item> _items = new Dictionary<object, Item>();
        private readonly HashSet<object> _live = new HashSet<object>();
        private readonly List<object> _removed = new List<object>();
        private double _now;
        private bool _enabled, _initialized, _animateEntrance;

        internal bool IsAnimating { get; private set; }
        internal int Count => _items.Count;

        internal void BeginFrame(IEnumerable<object> liveItems, double milliseconds, bool enabled)
        {
            _live.Clear();
            foreach (object item in liveItems) _live.Add(item);
            _removed.Clear();
            foreach (object item in _items.Keys)
                if (!_live.Contains(item)) _removed.Add(item);
            foreach (object item in _removed) _items.Remove(item);

            _now = milliseconds;
            _enabled = enabled;
            _animateEntrance = _initialized && enabled;
            _initialized = true;
            IsAnimating = false;
        }

        internal Rectangle GetBounds(object key, Rectangle target, bool immediate, int minimumWidth)
        {
            Item item;
            if (!_items.TryGetValue(key, out item))
            {
                Rectangle start = target;
                if (_animateEntrance && !immediate)
                    start.Width = Math.Min(target.Width, Math.Max(1, minimumWidth));
                _items.Add(key, item = new Item { Bounds = start, Start = start, Target = start });
            }

            // A dragged tab must follow the pointer directly. Size/position
            // changes between window states must not stretch old tab geometry.
            if (!_enabled || immediate || item.Bounds.Y != target.Y || item.Bounds.Height != target.Height)
            {
                item.Bounds = item.Start = item.Target = target;
                item.Animating = false;
                return target;
            }

            if (item.Target != target)
            {
                // BoundsAnimator::AnimateViewTo starts from the displayed bounds.
                // Only a changed target gets a new clock; other tabs continue.
                item.Start = item.Bounds;
                item.Target = target;
                item.Started = _now;
                item.Animating = true;
            }

            if (item.Animating)
            {
                double t = Math.Max(0, Math.Min(1, (_now - item.Started) / DurationMilliseconds));
                double eased = 1 - (1 - t) * (1 - t);
                item.Bounds = Interpolate(item.Start, item.Target, eased);
                // Keep the lifetime until 200 ms even if rounding reaches the
                // final pixel earlier, including a tab that is being closed.
                item.Animating = t < 1;
            }
            else
            {
                // The + button may have been held outside a closing tab after
                // its own animation finished. Release that temporary clamp as
                // the tab shrinks, rather than leaving the button stranded.
                item.Bounds = item.Target;
            }
            IsAnimating |= item.Animating;
            return item.Bounds;
        }

        internal static Rectangle Interpolate(Rectangle start, Rectangle target, double value)
        {
            // Tween::RectValueBetween rounds edges, not position and width
            // separately. Shared edges therefore round to the same pixel.
            int left = Between(start.Left, target.Left, value);
            int top = Between(start.Top, target.Top, value);
            int right = Between(start.Right, target.Right, value);
            int bottom = Between(start.Bottom, target.Bottom, value);
            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        private static int Between(int start, int target, double value) =>
            (int)Math.Floor(0.5 + start + (target - start) * value);

        internal bool IsItemAnimating(object key) =>
            _items.TryGetValue(key, out Item item) && item.Animating;

        internal void SetInitialBounds(object key, Rectangle bounds) =>
            _items[key] = new Item { Bounds = bounds, Start = bounds, Target = bounds };

        internal void RecordDisplayedBounds(object key, Rectangle bounds)
        {
            if (_items.TryGetValue(key, out Item item)) item.Bounds = bounds;
        }

        internal void Forget(object key) => _items.Remove(key);

        internal void Reset()
        {
            _items.Clear();
            _live.Clear();
            _removed.Clear();
            _initialized = false;
            IsAnimating = false;
        }
    }
}
