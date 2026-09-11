using System;
using System.Collections.Generic;
using System.Drawing;

namespace EasyTabs
{
    /// <summary>
    /// Remembers displayed bounds independently of layout targets. It owns no timer,
    /// images or windows; the existing overlay supplies timestamps and repaints.
    /// </summary>
    internal sealed class TabLayoutAnimation
    {
        private readonly Dictionary<object, RectangleF> _bounds = new Dictionary<object, RectangleF>();
        private readonly HashSet<object> _live = new HashSet<object>();
        private readonly List<object> _removed = new List<object>();
        private readonly Dictionary<object, RectangleF> _insertionStarts = new Dictionary<object, RectangleF>();
        private double? _insertionStarted;
        private float _insertionProgress;
        private bool _insertionFrame;
        private double _lastFrame;
        private float _amount;
        private bool _enabled, _initialized, _animateEntrance;

        internal bool IsAnimating { get; private set; }
        internal int Count { get { return _bounds.Count; } }

        internal void BeginFrame(IEnumerable<object> liveItems, double milliseconds, bool enabled)
        {
            _live.Clear();
            foreach (object item in liveItems) _live.Add(item);
            _removed.Clear();
            foreach (object key in _bounds.Keys)
                if (!_live.Contains(key)) _removed.Add(key);
            foreach (object removed in _removed)
            {
                _bounds.Remove(removed);
                _insertionStarts.Remove(removed);
            }

            _insertionFrame = enabled && _insertionStarted.HasValue;
            if (_insertionFrame)
            {
                _insertionProgress = (float)Math.Max(0, Math.Min(1, (milliseconds - _insertionStarted.Value) / 200.0));
                if (_insertionProgress == 1) _insertionStarted = null;
            }
            else
            {
                _insertionStarted = null;
                _insertionStarts.Clear();
            }

            // The first frame after idle starts at the displayed position. Repeated
            // redraw requests cannot speed up an animation: only elapsed time counts.
            double elapsed = IsAnimating ? Math.Max(0, milliseconds - _lastFrame) : 0;
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
            else if (_insertionFrame)
            {
                RectangleF start;
                if (!_insertionStarts.TryGetValue(item, out start)) start = current;
                // Chrome 86 BoundsAnimator: 200 ms, quadratic EASE_OUT. Every
                // affected tab and the + button uses the same insertion clock.
                float progress = 1 - (1 - _insertionProgress) * (1 - _insertionProgress);
                current.X = start.X + (target.X - start.X) * progress;
                current.Width = start.Width + (target.Width - start.Width) * progress;
            }
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

        internal void SetInitialBounds(object item, Rectangle bounds) => _bounds[item] = bounds;

        internal void StartInsertion(double milliseconds)
        {
            _insertionStarts.Clear();
            foreach (var pair in _bounds) _insertionStarts.Add(pair.Key, pair.Value);
            _insertionStarted = milliseconds;
            _insertionProgress = 0;
            _insertionFrame = true;
        }

        internal void Reset()
        {
            _bounds.Clear();
            _live.Clear();
            _removed.Clear();
            _insertionStarts.Clear();
            _insertionStarted = null;
            _insertionFrame = false;
            _initialized = false;
            IsAnimating = false;
        }
    }
}
