using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace Quartz.Controls
{
    /// <summary>
    /// A single scrolling row of ordinary buttons. Logical slots are separate from
    /// animated positions, so moving artwork cannot change a drop destination.
    /// </summary>
    public class FavouritesBar : FlowLayoutPanel, IMessageFilter
    {
        private sealed class RowLayout : LayoutEngine
        {
            public override bool Layout(object container, LayoutEventArgs args)
            {
                ((FavouritesBar)container).LayoutButtons();
                return false;
            }
        }

        private static readonly LayoutEngine Row = new RowLayout();
        private sealed class RemovedVisual : IDisposable
        {
            internal Bitmap Image;
            internal Rectangle Bounds;
            internal float Span, Remaining;
            internal float Left, StartLeft, StartRemaining;
            internal int LeadingMargin;
            internal List<Control> Following;
            internal List<Control> Preceding;
            public void Dispose() => Image.Dispose();
        }

        private readonly Dictionary<Control, Rectangle> _targets = new Dictionary<Control, Rectangle>();
        private readonly Dictionary<Control, float> _positions = new Dictionary<Control, float>();
        private readonly Dictionary<Control, float> _widths = new Dictionary<Control, float>();
        private readonly List<RemovedVisual> _removed = new List<RemovedVisual>();
        private readonly Dictionary<Control, RectangleF> _insertionStarts = new Dictionary<Control, RectangleF>();
        private double _insertionElapsed = 200;
        private readonly Timer _timer = new Timer { Interval = 15 };
        private readonly Stopwatch _clock = Stopwatch.StartNew();
        private List<Control> _originalOrder, _previewOrder;
        private FavouriteButton _pressedButton;
        private Point _pressScreen, _grabOffset;
        private float _dragLeft;
        private double _lastFrame;
        private bool _dragging, _animating, _layingOut, _finishing, _filterInstalled;

        public FavouritesBar()
        {
            DoubleBuffered = true;
            AutoScroll = true;
            WrapContents = false;
            _timer.Tick += Tick;
        }

        public override LayoutEngine LayoutEngine => Row;

        [DefaultValue(true)]
        public bool AnimationsEnabled { get; set; } = true;

        [Browsable(false)]
        public bool IsInteracting => _pressedButton != null;

        [Browsable(false)]
        public bool HasVisibleItems => Controls.Count > 0 || _removed.Count > 0;

        public event EventHandler OrderChanged;
        public event EventHandler InteractionEnded;
        public event EventHandler AnimationCompleted;

        // Buffer this small strip, including its child HWNDs, to avoid trails as
        // the existing themed buttons move. Do not apply this to the WebView.
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams parameters = base.CreateParams;
                parameters.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return parameters;
            }
        }

        [DllImport("user32.dll", EntryPoint = "SystemParametersInfoW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetAnimationSetting(uint action, uint parameter,
            [MarshalAs(UnmanagedType.Bool)] out bool enabled, uint flags);

        protected virtual bool ShouldAnimate()
        {
            bool enabled;
            return AnimationsEnabled && !SystemInformation.HighContrast &&
                (!GetAnimationSetting(0x1042, 0, out enabled, 0) || enabled);
        }

        private IList<Control> CurrentOrder => (_previewOrder ?? Controls.Cast<Control>().ToList())
            .Where(button => button.Parent == this).ToList();

        /// <summary>
        /// Displays an order already saved by the caller, retaining each button's
        /// current position. Does not raise the user-drag OrderChanged event.
        /// </summary>
        public bool TryAnimateOrder(IList<Control> order)
        {
            if (IsDisposed || Disposing || IsInteracting || order == null ||
                order.Count != Controls.Count || order.Any(button => button == null || button.Parent != this) ||
                order.Distinct().Count() != order.Count) return false;
            if (order.SequenceEqual(Controls.Cast<Control>())) return true;

            _animating = ShouldAnimate();
            _insertionStarts.Clear();
            _lastFrame = _clock.Elapsed.TotalMilliseconds;
            SuspendLayout();
            try
            {
                for (int i = 0; i < order.Count; i++) Controls.SetChildIndex(order[i], i);
            }
            finally { ResumeLayout(true); }
            LayoutButtons();
            if (_animating) _timer.Start();
            else _timer.Stop();
            return true;
        }

        internal bool UpdateItems(IList<FavouriteButton> items, Action<FavouriteButton> update, bool animate)
        {
            if (IsDisposed || Disposing || IsInteracting) return false;
            if (items == null || items.Any(button => button == null || button.IsDisposed ||
                (button.Parent != null && button.Parent != this)) || items.Distinct().Count() != items.Count)
                throw new ArgumentException("Each favourite button must belong to this bar or have no parent.", nameof(items));
            _animating = animate && ShouldAnimate();
            var addedItems = new HashSet<FavouriteButton>(items.Where(button => button.Parent != this));
            bool inserting = addedItems.Count > 0;
            bool removing = Controls.OfType<FavouriteButton>().Except(items).Any();
            if (!_animating)
            {
                foreach (RemovedVisual visual in _removed) visual.Dispose();
                _removed.Clear();
                _insertionStarts.Clear();
            }
            _lastFrame = _clock.Elapsed.TotalMilliseconds;
            SuspendLayout();
            try
            {
                var previousOrder = Controls.Cast<Control>().ToList();
                foreach (FavouriteButton button in previousOrder.OfType<FavouriteButton>().Except(items).ToList())
                {
                    if (_animating && button.Width > 0 && button.Height > 0)
                    {
                        var bounds = button.Bounds;
                        bounds.Offset(-AutoScrollPosition.X, -AutoScrollPosition.Y);
                        float span = button.Width + button.Margin.Horizontal;
                        _removed.Add(new RemovedVisual
                        {
                            Image = button.CaptureVisual(), Bounds = bounds, Span = span, Remaining = span,
                            Left = bounds.Left, LeadingMargin = button.Margin.Left,
                            Following = previousOrder.Skip(previousOrder.IndexOf(button) + 1).ToList(),
                            Preceding = previousOrder.Take(previousOrder.IndexOf(button)).Reverse().ToList()
                        });
                    }
                    Controls.Remove(button);
                    button.Dispose();
                }
                foreach (FavouriteButton button in items)
                {
                    bool added = button.Parent != this;
                    Size previousSize = button.Size;
                    button.ChangeContent(() => update(button), _animating && !added);
                    if (!button.AutoSize && button.Size != previousSize)
                        _targets[button] = new Rectangle(button.Location, button.Size);
                    if (added)
                    {
                        Controls.Add(button);
                        if (_animating) _widths[button] = 0;
                        button.BeginEntrance(_animating);
                    }
                    else if (!_animating) button.EndEntrance();
                }
                for (int i = 0; i < items.Count; i++) Controls.SetChildIndex(items[i], i);
            }
            finally { ResumeLayout(true); }
            LayoutButtons();
            if (_animating && inserting)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    FavouriteButton button = items[i];
                    if (!addedItems.Contains(button)) continue;
                    Control previous = i == 0 ? null : items[i - 1];
                    float left = previous == null ? Padding.Left + button.Margin.Left :
                        _positions[previous] + _widths[previous] +
                        (addedItems.Contains(previous) ? 0 : previous.Margin.Right + button.Margin.Left);
                    // An insertion during a close starts after the still-visible
                    // closing content. Both then share this insertion clock.
                    foreach (RemovedVisual visual in _removed.OrderBy(visual => visual.Left))
                        if (visual.Left <= left) left = Math.Max(left, visual.Left + visual.Remaining);
                    _positions[button] = left;
                }
                ApplyPositions();
                // EasyTabs insertion: one 200 ms quadratic ease-out clock for
                // the new item and every neighbour affected by the insertion.
                _insertionElapsed = 0;
                _insertionStarts.Clear();
                foreach (Control button in CurrentOrder)
                    _insertionStarts[button] = new RectangleF(_positions[button], 0, _widths[button], 0);
                foreach (RemovedVisual visual in _removed)
                {
                    visual.StartLeft = visual.Left;
                    visual.StartRemaining = visual.Remaining;
                }
            }
            else if (removing) _insertionStarts.Clear();
            if (_animating) _timer.Start();
            else _timer.Stop();
            return true;
        }

        private void LayoutButtons()
        {
            if (_layingOut || _targets == null || IsDisposed) return;
            _layingOut = true;
            try
            {
                int x = Padding.Left;
                int tabIndex = 0;
                foreach (Control button in CurrentOrder)
                {
                    Rectangle previousTarget;
                    Size size = button.AutoSize ? button.GetPreferredSize(Size.Empty) :
                        _animating && _targets.TryGetValue(button, out previousTarget) ? previousTarget.Size : button.Size;
                    size.Width = Math.Max(button.MinimumSize.Width, size.Width);
                    size.Height = Math.Max(button.MinimumSize.Height, size.Height);
                    if (button.MaximumSize.Width > 0) size.Width = Math.Min(size.Width, button.MaximumSize.Width);
                    if (button.MaximumSize.Height > 0) size.Height = Math.Min(size.Height, button.MaximumSize.Height);
                    x += button.Margin.Left;
                    var target = new Rectangle(x, Padding.Top + button.Margin.Top, size.Width, size.Height);
                    _targets[button] = target;
                    if (!_positions.ContainsKey(button) || !_animating) _positions[button] = target.X;
                    if (!_widths.ContainsKey(button) || !_animating) _widths[button] = target.Width;
                    button.TabIndex = tabIndex++;
                    x += size.Width + button.Margin.Right;
                }

                UpdateScrollExtent();
                ApplyPositions();
            }
            finally { _layingOut = false; }
        }

        private void UpdateScrollExtent()
        {
            // Sum displayed widths so hiding icons contracts the scroll range
            // smoothly too. The dragged button's position never changes the extent.
            int width = Padding.Horizontal;
            int height = 0;
            foreach (Control button in CurrentOrder)
            {
                float margin = button is FavouriteButton favourite && favourite.IsEntering && _targets[button].Width > 0
                    ? button.Margin.Horizontal * _widths[button] / _targets[button].Width : button.Margin.Horizontal;
                width += (int)Math.Ceiling(_widths[button] + margin);
                height = Math.Max(height, _targets[button].Bottom + button.Margin.Bottom + Padding.Bottom);
            }
            width += (int)Math.Ceiling(_removed.Sum(visual => visual.Remaining));
            AutoScrollMinSize = new Size(width, height);
        }

        private void ApplyPositions()
        {
            Point scroll = AutoScrollPosition;
            foreach (Control button in CurrentOrder)
            {
                Rectangle bounds;
                if (!_targets.TryGetValue(button, out bounds)) continue;
                bounds.X = (int)Math.Round(button == _pressedButton && _dragging
                    ? _dragLeft : _positions[button]);
                bounds.Width = (int)Math.Round(_widths[button]);
                bounds.Offset(scroll);
                button.Bounds = bounds;
            }
        }

        internal void PointerDown(FavouriteButton button, Point screen)
        {
            CancelInteraction();
            _pressedButton = button;
            _pressScreen = screen;
            _grabOffset = button.PointToClient(screen);
            _originalOrder = Controls.Cast<Control>().ToList();
            _previewOrder = new List<Control>(_originalOrder);
            Application.AddMessageFilter(this);
            _filterInstalled = true;
        }

        internal void PointerMove(Point screen)
        {
            if (_pressedButton == null || _finishing) return;
            if (!_dragging)
            {
                Size threshold = SystemInformation.DragSize;
                var deadZone = new Rectangle(_pressScreen.X - threshold.Width / 2,
                    _pressScreen.Y - threshold.Height / 2, threshold.Width, threshold.Height);
                if (deadZone.Contains(screen)) return;
                _dragging = true;
                _pressedButton.SuppressMouseClick = true;
                _animating = ShouldAnimate();
                _insertionStarts.Clear();
                _lastFrame = _clock.Elapsed.TotalMilliseconds;
                // The preview list retains logical order while the dragged HWND
                // is brought above its neighbours.
                _pressedButton.BringToFront();
                _timer.Start();
            }

            Point point = PointToClient(screen);
            int minimum = Padding.Left + _pressedButton.Margin.Left;
            int maximum = Math.Max(minimum, AutoScrollMinSize.Width - Padding.Right -
                _pressedButton.Margin.Right - _pressedButton.Width);
            _dragLeft = Math.Max(minimum, Math.Min(maximum,
                point.X - AutoScrollPosition.X - _grabOffset.X));

            // Compare the leading/trailing edges with stable slot midpoints.
            // Using animated bounds (or the cursor alone) jitters with unequal widths.
            int index = _previewOrder.IndexOf(_pressedButton);
            while (index > 0 && _dragLeft < Centre(_targets[_previewOrder[index - 1]]))
            {
                _previewOrder.RemoveAt(index);
                _previewOrder.Insert(--index, _pressedButton);
                LayoutButtons();
            }
            while (index < _previewOrder.Count - 1 &&
                _dragLeft + _pressedButton.Width > Centre(_targets[_previewOrder[index + 1]]))
            {
                _previewOrder.RemoveAt(index);
                _previewOrder.Insert(++index, _pressedButton);
                LayoutButtons();
            }
            LayoutButtons();
        }

        private static float Centre(Rectangle bounds) => bounds.Left + bounds.Width / 2f;

        internal void PointerUp(Point screen)
        {
            FinishInteraction(ClientRectangle.Contains(PointToClient(screen)));
        }

        public void CancelInteraction() => FinishInteraction(false);

        private void FinishInteraction(bool commit)
        {
            if (_pressedButton == null || _finishing) return;
            _finishing = true;
            try
            {
                bool changed = commit && _dragging && !_originalOrder.SequenceEqual(_previewOrder);
                List<Control> order = (commit && _dragging ? _previewOrder : _originalOrder)
                    .Where(button => button.Parent == this).ToList();
                FavouriteButton pressed = _pressedButton;
                if (!commit) pressed.SuppressMouseClick = true;
                if (_dragging) _positions[pressed] = _dragLeft;
                _pressedButton = null;
                _dragging = false;
                if (_filterInstalled) Application.RemoveMessageFilter(this);
                _filterInstalled = false;

                SuspendLayout();
                try
                {
                    for (int i = 0; i < order.Count; i++) Controls.SetChildIndex(order[i], i);
                    _previewOrder = null;
                    _originalOrder = null;
                }
                finally { ResumeLayout(true); }
                // SetChildIndex may do nothing for a cancelled or same-slot drag.
                // Still return the button to its slot when animation is disabled.
                LayoutButtons();
                pressed.Capture = false;
                if (_animating) _timer.Start();
                else _timer.Stop();
                if (changed) OrderChanged?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                _finishing = false;
                InteractionEnded?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Tick(object sender, EventArgs args)
        {
            double now = _clock.Elapsed.TotalMilliseconds;
            double elapsed = Math.Max(0, now - _lastFrame);
            _lastFrame = now;
            if (_dragging)
            {
                if (!_pressedButton.Capture || (MouseButtons & MouseButtons.Left) == 0)
                {
                    CancelInteraction();
                    return;
                }
                ScrollAtEdge(PointToClient(MousePosition), elapsed);
                PointerMove(MousePosition);
            }
            AdvanceAnimation(elapsed);
        }

        internal void ScrollAtEdge(Point point, double elapsed)
        {
            if (!HorizontalScroll.Visible || point.Y < 0 || point.Y >= ClientSize.Height) return;
            int edge = Math.Min(28, ClientSize.Width / 4);
            int direction = point.X < edge ? -1 : point.X >= ClientSize.Width - edge ? 1 : 0;
            if (direction == 0) return;
            int offset = -AutoScrollPosition.X;
            int maximum = Math.Max(0, AutoScrollMinSize.Width - ClientSize.Width);
            int step = Math.Max(1, (int)Math.Round(Math.Min(elapsed, 50) * .5));
            AutoScrollPosition = new Point(Math.Max(0, Math.Min(maximum, offset + direction * step)), 0);
        }

        internal void AdvanceAnimation(double elapsed)
        {
            bool moving = false;
            float amount = (float)(1 - Math.Exp(-Math.Max(0, elapsed) / 38.0));
            bool inserting = _animating && _insertionStarts.Count > 0;
            _insertionElapsed = Math.Min(200, _insertionElapsed + Math.Max(0, elapsed));
            float progress = 1 - (float)Math.Pow(1 - _insertionElapsed / 200, 2);
            foreach (Control button in CurrentOrder)
            {
                float current = _positions[button];
                float target = _targets[button].X;
                float next = _animating ? current + (target - current) * amount : target;
                RectangleF start = RectangleF.Empty;
                bool hasStart = inserting && _insertionStarts.TryGetValue(button, out start);
                if (hasStart) next = start.X + (target - start.X) * progress;
                if (Math.Abs(next - target) < .25f) next = target;
                _positions[button] = next;
                moving |= next != target;
                float width = _widths[button];
                float targetWidth = _targets[button].Width;
                float nextWidth = _animating ? width + (targetWidth - width) * amount : targetWidth;
                if (hasStart) nextWidth = start.Width + (targetWidth - start.Width) * progress;
                if (Math.Abs(nextWidth - targetWidth) < .25f) nextWidth = targetWidth;
                _widths[button] = nextWidth;
                moving |= nextWidth != targetWidth;
                if (button is FavouriteButton favourite)
                {
                    if (nextWidth == targetWidth) favourite.EndEntrance();
                    moving |= favourite.AdvanceContentAnimation(elapsed);
                }
            }
            for (int i = _removed.Count - 1; i >= 0; i--)
            {
                RemovedVisual visual = _removed[i];
                Control following = visual.Following.FirstOrDefault(button => _targets.ContainsKey(button));
                Control preceding = visual.Preceding.FirstOrDefault(button => _targets.ContainsKey(button));
                float targetLeft = following != null ? _targets[following].Left :
                    preceding == null ? Padding.Left + visual.LeadingMargin :
                    _targets[preceding].Right + preceding.Margin.Right + visual.LeadingMargin;
                visual.Left = _animating ? visual.Left + (targetLeft - visual.Left) * amount : targetLeft;
                visual.Remaining = _animating ? visual.Remaining * (1 - amount) : 0;
                if (inserting)
                {
                    visual.Left = visual.StartLeft + (targetLeft - visual.StartLeft) * progress;
                    visual.Remaining = visual.StartRemaining * (1 - progress);
                }
                if (visual.Remaining < .25f)
                {
                    visual.Dispose();
                    _removed.RemoveAt(i);
                }
                else moving = true;
                Invalidate();
            }
            if (_insertionElapsed >= 200) _insertionStarts.Clear();
            _layingOut = true;
            try
            {
                UpdateScrollExtent();
                ApplyPositions();
            }
            finally { _layingOut = false; }
            if (!moving && !_dragging)
            {
                bool completed = _animating || _timer.Enabled;
                _animating = false;
                _timer.Stop();
                if (completed) AnimationCompleted?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            foreach (RemovedVisual visual in _removed)
            {
                Rectangle bounds = visual.Bounds;
                bounds.X = (int)Math.Round(visual.Left);
                bounds.Width = (int)Math.Round(visual.Bounds.Width * visual.Remaining / visual.Span);
                bounds.Offset(AutoScrollPosition);
                FavouriteButton.PaintClippedFace(e.Graphics, visual.Image, bounds);
            }
        }

        public bool PreFilterMessage(ref Message message)
        {
            if (IsInteracting && message.Msg == 0x0100 && (Keys)message.WParam.ToInt32() == Keys.Escape)
            {
                CancelInteraction();
                return true;
            }
            return false;
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            LayoutButtons();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            CancelInteraction();
            base.OnSizeChanged(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (!Visible) CancelInteraction();
            base.OnVisibleChanged(e);
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            CancelInteraction();
            base.OnControlAdded(e);
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            CancelInteraction();
            _targets.Remove(e.Control);
            _positions.Remove(e.Control);
            _widths.Remove(e.Control);
            base.OnControlRemoved(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_filterInstalled) Application.RemoveMessageFilter(this);
                _filterInstalled = false;
                _pressedButton = null;
                _previewOrder = null;
                _originalOrder = null;
                foreach (RemovedVisual visual in _removed) visual.Dispose();
                _removed.Clear();
                _timer.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    internal partial class FavouriteButton : Button
    {
        internal bool SuppressMouseClick { get; set; }
        private bool _releasingMouse;
        private FavouritesBar Bar => Parent as FavouritesBar;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            SuppressMouseClick = false;
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left) Bar?.PointerDown(this, PointToScreen(e.Location));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != 0) Bar?.PointerMove(PointToScreen(e.Location));
            base.OnMouseMove(e);
        }

        protected override void OnClick(EventArgs e)
        {
            if (!SuppressMouseClick) base.OnClick(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Bar?.IsInteracting != true) SuppressMouseClick = false;
            base.OnKeyDown(e);
        }

        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            if (!Capture && !_releasingMouse) Bar?.CancelInteraction();
            base.OnMouseCaptureChanged(e);
        }

        protected override void WndProc(ref Message message)
        {
            if (message.Msg != 0x0202) // WM_LBUTTONUP
            {
                base.WndProc(ref message);
                return;
            }
            // Click and capture-loss can occur inside base.WndProc before MouseUp.
            // Keep the entire release suppressed, then commit exactly once.
            FavouritesBar bar = Bar;
            long coordinates = message.LParam.ToInt64();
            Point screen = PointToScreen(new Point((short)coordinates, (short)(coordinates >> 16)));
            _releasingMouse = true;
            try { base.WndProc(ref message); }
            finally
            {
                try { bar?.PointerUp(screen); }
                finally { _releasingMouse = false; }
            }
        }
    }
}
