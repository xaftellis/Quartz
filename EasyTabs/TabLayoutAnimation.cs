using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace EasyTabs
{
    /// <summary>
    /// Remembers displayed bounds independently of layout targets. It owns no timer,
    /// images or windows; the existing overlay supplies timestamps and repaints.
    /// </summary>
    internal sealed class TabLayoutAnimation
    {
        private readonly Dictionary<object, RectangleF> _bounds = new Dictionary<object, RectangleF>();
        private double _lastFrame;
        private float _amount;
        private bool _enabled, _initialized, _animateEntrance;

        internal bool IsAnimating { get; private set; }
        internal int Count { get { return _bounds.Count; } }

        internal void BeginFrame(IEnumerable<object> liveItems, double milliseconds, bool enabled)
        {
            var live = new HashSet<object>(liveItems);
            foreach (object removed in _bounds.Keys.Where(key => !live.Contains(key)).ToArray())
                _bounds.Remove(removed);

            // The first frame after idle starts at the displayed position. Repeated
            // redraw requests cannot speed up an animation: only elapsed time counts.
            double elapsed = IsAnimating ? Math.Max(0, Math.Min(64, milliseconds - _lastFrame)) : 0;
            _amount = (float)(1 - Math.Exp(-elapsed / 38.0));
            _lastFrame = milliseconds;
            _enabled = enabled;
            _animateEntrance = _initialized && enabled;
            _initialized = true;
            IsAnimating = false;
        }

        internal Rectangle GetBounds(object item, Rectangle target, bool immediate, int minimumWidth)
        {
            RectangleF current;
            if (!_bounds.TryGetValue(item, out current))
            {
                current = target;
                if (_animateEntrance && !immediate)
                    current.Width = Math.Min(target.Width, Math.Max(1, minimumWidth));
            }

            // Dragged/detached tabs follow the pointer exactly. A maximize/restore
            // height change also snaps so the old frame geometry is never stretched.
            if (!_enabled || immediate || current.Y != target.Y || current.Height != target.Height)
                current = target;
            else
            {
                current.X = Approach(current.X, target.X);
                current.Width = Approach(current.Width, target.Width);
            }

            _bounds[item] = current;
            IsAnimating |= current.X != target.X || current.Width != target.Width;
            return Rectangle.Round(current);
        }

        private float Approach(float current, float target)
        {
            float next = current + (target - current) * _amount;
            return Math.Abs(next - target) < .25f ? target : next;
        }

        internal void Forget(object item) => _bounds.Remove(item);

        internal void Reset()
        {
            _bounds.Clear();
            _initialized = false;
            IsAnimating = false;
        }
    }
}
