// Painting/layout adapted from Chromium 86.0.4240.75. See ChromiumTabRendering.md.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using SkiaSharp;
using Win32Interop.Enums;

namespace EasyTabs
{
    /// <summary>Chrome 86 vector tabs presented through EasyTabs' existing overlay.</summary>
    public class ChromiumTabRenderer : BaseTabRenderer
    {
        /// <summary>Optional loading colour for the selected tab's different background.</summary>
        public Color? ActiveLoadingIndicatorColor { get; set; }

        private sealed class Visual : IDisposable
        {
            internal ChromiumTabGeometry Geometry;
            internal Rectangle Bounds, Target;
            internal readonly ChromiumHoverAnimation HoverAnimation = new ChromiumHoverAnimation();
            internal float Hover => (float)HoverAnimation.Value;
            internal bool MouseHovered, Visible;
            internal Point HoverPoint;
            internal bool Closing, ClosingTargetInitialized, HasIcon;
            internal Visual Previous;
            internal int Index;
            internal bool ContentInitialized, ShowingIcon, WasLoading;
            internal int LoadingCompletionVersion;
            internal Rectangle TitleBounds, TitleStart, TitleTarget;
            internal double TitleStarted = double.NaN, FaviconStarted = double.NaN;
            internal double WaitingElapsed, SpinningStarted = double.NaN, WaitingArcOffset = double.NaN;
            internal float FaviconProgress = 1;
            internal readonly ButtonFeedback CloseFeedback = new ButtonFeedback();
            internal readonly ContentCache Content = new ContentCache();
            public void Dispose() { Geometry?.Dispose(); CloseFeedback.Dispose(); Content.Dispose(); }
        }

        // Text shaping and favicon conversion are expensive. Keep their transparent
        // pixels until their inputs change; hover, position and neighbour clipping
        // are applied when composing the frame and do not invalidate this cache.
        private sealed class ContentCache : IDisposable
        {
            internal SKBitmap Pixels;
            private Bitmap _bitmap;
            private Graphics _graphics;
            private string _caption;
            private Icon _icon;
            private Rectangle _iconBounds, _titleBounds;
            private Color _foreground;
            private float _scale;
            private readonly TabTitleRenderer _title = new TabTitleRenderer();
            internal Rectangle FaviconSource { get; private set; }
            internal Rectangle IconBounds => _iconBounds;
            internal bool HasFavicon => _icon != null;

            internal void Update(Size size, string caption, Icon icon, Rectangle iconBounds,
                Rectangle titleBounds, Color foreground, float scale, Font font, bool force)
            {
                // Width animates every frame. Retain capacity for a full tab so a
                // one-pixel layout change does not allocate three native objects.
                int iconSize = ChromiumTabMetrics.Pixel(16 * scale);
                if (Pixels == null || Pixels.Width < size.Width + iconSize || Pixels.Height != size.Height)
                {
                    Dispose();
                    int capacity = Math.Max(size.Width, ChromiumTabMetrics.Pixel(ChromiumTabMetrics.StandardWidth * scale)) + iconSize;
                    Pixels = new SKBitmap(new SKImageInfo(capacity, size.Height, SKColorType.Bgra8888, SKAlphaType.Premul));
                    _bitmap = new Bitmap(capacity, size.Height, Pixels.RowBytes, PixelFormat.Format32bppPArgb, Pixels.GetPixels());
                    _graphics = Graphics.FromImage(_bitmap);
                    _graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    // A small extra column keeps icon pixels separate from the
                    // title, in this same buffer. Reveals only transform this slot.
                    FaviconSource = new Rectangle(capacity - iconSize, 0, iconSize, iconSize);
                    force = true;
                }
                if (!force && _caption == caption && ReferenceEquals(_icon, icon) &&
                    _iconBounds == iconBounds && _titleBounds == titleBounds && _foreground == foreground && _scale == scale) return;

                _graphics.Clear(Color.Transparent);
                if (icon != null) _graphics.DrawIcon(icon, FaviconSource);
                _title.Draw(_graphics, Pixels, caption, font, titleBounds, foreground, scale);
                _graphics.Flush(System.Drawing.Drawing2D.FlushIntention.Sync);
                Pixels.NotifyPixelsChanged();
                _caption = caption; _icon = icon; _iconBounds = iconBounds; _titleBounds = titleBounds;
                _foreground = foreground; _scale = scale;
            }

            internal void UpdateClosingTitle(Rectangle titleBounds, Color foreground, float scale, Font font)
            {
                if (Pixels == null || (_titleBounds == titleBounds && _foreground == foreground && _scale == scale)) return;
                // The page form is already disposed. Keep the cached favicon
                // pixels; only clear/redraw the title side of the shared bitmap.
                var state = _graphics.Save();
                try
                {
                    _graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    using (var clear = new SolidBrush(Color.Transparent))
                        _graphics.FillRectangle(clear, 0, 0, FaviconSource.Left, Pixels.Height);
                }
                finally { _graphics.Restore(state); }
                _title.Draw(_graphics, Pixels, _caption, font, titleBounds, foreground, scale);
                _graphics.Flush(System.Drawing.Drawing2D.FlushIntention.Sync);
                Pixels.NotifyPixelsChanged();
                _titleBounds = titleBounds; _foreground = foreground; _scale = scale;
            }

            public void Dispose()
            {
                _graphics?.Dispose(); _graphics = null;
                _bitmap?.Dispose(); _bitmap = null;
                Pixels?.Dispose(); Pixels = null;
                _icon = null; // Borrowed from the tab's form, never owned here.
            }
        }

        // Keep the raster composition, with Chromium's separate hover/ripple clocks.
        private sealed class ButtonFeedback : IDisposable
        {
            private readonly ChromiumButtonAnimation _animation = new ChromiumButtonAnimation();
            private SKPath _clip;
            private float _clipRadius;
            internal float HoverOpacity => _animation.HoverOpacity;
            internal float InkOpacity => _animation.InkOpacity;
            internal float InkProgress => _animation.InkProgress;
            internal PointF Origin { get; private set; }
            internal bool IsAnimating => _animation.IsAnimating;

            internal void Press(double now, PointF origin)
            {
                Origin = origin;
                _animation.Press(now);
            }

            internal void Release(double now, bool inside) => _animation.Release(now, inside);

            internal void Cancel() => _animation.Reset();
            internal void Cancel(double now) => _animation.Cancel(now);

            internal void Update(bool hovered, double now, bool animate) => _animation.Update(hovered, now, animate);

            internal void Paint(SKCanvas canvas, float cx, float cy, float radius, Color background, float scale)
            {
                if (HoverOpacity == 0 && InkOpacity == 0) return;
                double luminance = ChromiumTabTheme.Luminance(background);
                SKColor ink = (luminance + .05) / .05 >= 1.05 / (luminance + .05) ? SKColors.Black : SKColors.White;
                using (var paint = new SKPaint { IsAntialias = true, Color = ink.WithAlpha((byte)(255 * HoverOpacity)) })
                {
                    canvas.DrawCircle(cx, cy, radius, paint);
                    if (InkOpacity == 0) return;
                    if (_clip == null || _clipRadius != radius)
                    {
                        _clip?.Dispose();
                        using (var path = new SKPathBuilder())
                        {
                            path.AddCircle(0, 0, radius, SKPathDirection.Clockwise);
                            _clip = path.Detach();
                        }
                        _clipRadius = radius;
                    }
                    canvas.Save();
                    canvas.Translate(cx, cy);
                    canvas.ClipPath(_clip, SKClipOperation.Intersect, true);
                    float x = Origin.X * radius, y = Origin.Y * radius;
                    // FloodFillInkDropRipple expands from one DIP to the farthest
                    // corner of its host bounds, then clips to the button shape.
                    float dx = radius + Math.Abs(x), dy = radius + Math.Abs(y);
                    float fullRadius = (float)Math.Sqrt(dx * dx + dy * dy);
                    float minimumRadius = scale;
                    paint.Color = ink.WithAlpha((byte)(255 * InkOpacity));
                    canvas.DrawCircle(x, y, minimumRadius + (fullRadius - minimumRadius) * InkProgress, paint);
                    canvas.Restore();
                }
            }

            public void Dispose() { _clip?.Dispose(); }
        }

        private readonly object _sync = new object();
        private readonly Dictionary<TitleBarTab, Visual> _visuals = new Dictionary<TitleBarTab, Visual>();
        private readonly ChromiumBoundsAnimation _animation = new ChromiumBoundsAnimation();
        private readonly Stopwatch _clock = Stopwatch.StartNew();
        private readonly object _addKey = new object();
        private readonly ButtonFeedback _addFeedback = new ButtonFeedback();
        private ButtonFeedback _pressedFeedback;
        private readonly WindowsSizingBoxes _sizingBoxes;
        private readonly Size _originalMinimum;
        private readonly List<TitleBarTab> _paintOrder = new List<TitleBarTab>();
        private readonly List<TitleBarTab> _closingTabs = new List<TitleBarTab>();
        private readonly List<TitleBarTab> _removedTabs = new List<TitleBarTab>();
        private readonly HashSet<TitleBarTab> _liveTabs = new HashSet<TitleBarTab>();
        private readonly List<object> _animationItems = new List<object>();
        private readonly Comparison<TitleBarTab> _comparePaintOrder;
        private readonly SKPaint _spinnerPaint = new SKPaint
        { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round };
        private readonly SKRoundRect _faviconClip = new SKRoundRect();
        private SKBitmap _pixels;
        private Bitmap _buffer;
        private Font _font;
        private float _fontScale;
        private bool _hoverAnimating, _buttonAnimating, _contentAnimating, _addHovered, _disposed;
        private Point _lastCursor = new Point(int.MinValue, int.MinValue);
        private Rectangle _tabPaintBounds;
        private TitleBarTab _hoveredTab;
        private ChromiumTabTheme _theme = ChromiumTabTheme.Light;
        private int? _availableWidthDuringMouseClose;
        private Size _mouseCloseWindowSize;
        private float _mouseCloseScale;
        private Rectangle _mouseCloseWatchBounds;
        private double? _mouseCloseExitStarted;

        public ChromiumTabRenderer(TitleBarTabs parentWindow) : base(parentWindow)
        {
            _sizingBoxes = new WindowsSizingBoxes(parentWindow);
            _comparePaintOrder = ComparePaintOrder;
            _originalMinimum = parentWindow.MinimumSize;
            AddButtonMarginRight = 45; // Preserve Quartz's draggable caption space.
            parentWindow.Disposed += ParentDisposed;
            parentWindow.Deactivate += ParentDeactivated;
        }

        public ChromiumTabTheme Theme
        {
            get { return _theme; }
            set
            {
                _theme = value ?? throw new ArgumentNullException(nameof(value));
                if (_parentWindow.TabRenderer == this) _parentWindow._overlay?.RequestRender();
            }
        }

        // DeviceDpi follows the host's coordinate system (96 when Windows virtualizes
        // a DPI-unaware application). Do not mix physical hook coordinates into it.
        protected virtual float RenderScale => Math.Max(1, _parentWindow.DeviceDpi / 96f);
        private int Scale(float value) => ChromiumTabMetrics.Pixel(value * RenderScale);
        public override int TopPadding => _parentWindow.WindowState == FormWindowState.Maximized ? 0 : Scale(8);
        public override int TabHeight => Scale(ChromiumTabMetrics.Height) + TopPadding;
        public override int OverlapWidth => Scale(ChromiumTabMetrics.Overlap);
        public override bool RendersEntireTitleBar => IsWindows10;
        internal override bool IsLayoutAnimating => _animation.IsAnimating || _hoverAnimating || _buttonAnimating || _contentAnimating || _mouseCloseExitStarted.HasValue;
        internal override bool IsTabClosingMode => _availableWidthDuringMouseClose.HasValue;
        protected virtual double AnimationTimeMilliseconds => _clock.Elapsed.TotalMilliseconds;

        public override bool IsOverSizingBox(Point cursor) => _sizingBoxes.Contains(cursor);
        public override HT NonClientHitTest(Message message, Point cursor)
        {
            HT result = _sizingBoxes.NonClientHitTest(cursor);
            return result == HT.HTNOWHERE ? HT.HTCAPTION : result;
        }

        protected override void Tabs_CollectionModified(object sender, ListModificationEventArgs e)
        {
            // Chrome allows background tabs to lose their close buttons. The old
            // image renderer enlarged MinimumSize as if every close button remained.
            _parentWindow.MinimumSize = _originalMinimum;
            if (e.Modification == ListModification.ItemAdded || e.Modification == ListModification.RangeAdded ||
                e.Modification == ListModification.Cleared)
                lock (_sync) ExitTabClosingMode();
        }

        internal override void BeginPinnedTabAnimation()
        {
            lock (_sync)
            {
                if (_disposed) return;
                ExitTabClosingMode();
                // The next layout animates changed targets from displayed bounds.
                _pressedFeedback?.Cancel();
                _pressedFeedback = null;
            }
        }

        internal override bool RequiresHoverRedraw(Point cursor)
        {
            lock (_sync)
            {
                if (UpdateTabClosingPointer(cursor)) return true;
                return cursor != _lastCursor && (_hoveredTab != null || FindTab(cursor) != null ||
                    _addHovered || IsOverAddButton(cursor));
            }
        }

        private bool ExitTabClosingMode()
        {
            bool wasClosing = IsTabClosingMode;
            _availableWidthDuringMouseClose = null;
            _mouseCloseExitStarted = null;
            return wasClosing;
        }

        internal override bool UpdateTabClosingPointer(Point cursor, bool pressed = false)
        {
            lock (_sync)
            {
                if (!IsTabClosingMode) return false;
                if (_mouseCloseWatchBounds.Contains(cursor))
                {
                    bool wasWaiting = _mouseCloseExitStarted.HasValue;
                    _mouseCloseExitStarted = null;
                    return wasWaiting;
                }
                double now = AnimationTimeMilliseconds;
                if (!pressed && !_mouseCloseExitStarted.HasValue)
                {
                    // Chromium MouseWatcher waits 300 ms after leaving its zone.
                    // Keep the frame scheduler alive even when all tabs are idle.
                    _mouseCloseExitStarted = now;
                    return true;
                }
                if (!pressed && now - _mouseCloseExitStarted.Value < 300) return false;
                ExitTabClosingMode();
                return true;
            }
        }

        private void KeepMouseCloseWidth(TitleBarTab tab, int index)
        {
            var tabs = _parentWindow.Tabs;
            Visual closing, first, last;
            if (tabs.Count < 2 || !_visuals.TryGetValue(tab, out closing) ||
                !_visuals.TryGetValue(tabs[0], out first) || !_visuals.TryGetValue(tabs[tabs.Count - 1], out last))
            {
                ExitTabClosingMode();
                return;
            }

            int available = _availableWidthDuringMouseClose ?? last.Target.Right - first.Target.Left;
            if (index < tabs.Count - 1)
            {
                // Preserve the following close target. At minimum sizes the next
                // tab takes the active width, so subtract its former width instead.
                int removedWidth = closing.Target.Width;
                Visual next;
                if (tab.Active && !tab.IsPinned && _visuals.TryGetValue(tabs[index + 1], out next))
                    removedWidth = next.Target.Width;
                available -= removedWidth - OverlapWidth;
            }
            // Closing the rightmost tab keeps the trailing edge, as in Chromium;
            // the survivors may grow to fill that existing width budget.
            _availableWidthDuringMouseClose = Math.Max(1, available);
            _mouseCloseExitStarted = null;
            _mouseCloseWindowSize = _parentWindow.ClientSize;
            _mouseCloseScale = RenderScale;
            _mouseCloseWatchBounds = new Rectangle(0, 0,
                _parentWindow._overlay?.Width ?? _parentWindow.ClientSize.Width,
                (_parentWindow._overlay?.Height ?? TabHeight) + Scale(40));
        }

        /// <summary>Show Chromium's former two-stage waiting/loading spinner. Disable for the modern single spinner.</summary>
        public bool ShowWaitingAnimation { get; set; } = true;

        /// <summary>Borrowed fallback for pinned pages with no visible favicon. Never changes the page's icon.</summary>
        public Icon DefaultFavicon { get; set; } = SystemIcons.Application;

        internal override void BeginTabClose(TitleBarTab tab, bool fromMouse = false)
        {
            lock (_sync)
            {
                Visual visual;
                int index = _parentWindow.Tabs.IndexOf(tab);
                if (_disposed || index < 0) return;
                if (fromMouse) KeepMouseCloseWidth(tab, index);
                else ExitTabClosingMode();
                if (!ShouldAnimateLayout() ||
                    (_parentWindow.Tabs.Count == 1 && _parentWindow.ExitOnLastTabClose) ||
                    !_visuals.TryGetValue(tab, out visual) || !visual.Visible || visual.Geometry == null || visual.Content.Pixels == null) return;
                visual.Closing = true;
                visual.ClosingTargetInitialized = false;
                visual.Previous = null;
                if (index > 0) _visuals.TryGetValue(_parentWindow.Tabs[index - 1], out visual.Previous);
                // Tab::SetClosing does not reset hover. Active tabs track hover
                // too, so their existing highlight appears when they lose selection.
                if (_pressedFeedback == visual.CloseFeedback) _pressedFeedback = null;
            }
        }

        internal override bool BeginCaptionButtonPress(Point cursor)
        {
            lock (_sync)
                return !_disposed && RendersEntireTitleBar && !IsTabRepositioning && _sizingBoxes.PointerDown(cursor);
        }

        internal override void EndCaptionButtonPress()
        {
            lock (_sync) _sizingBoxes.CancelPress();
        }

        internal override void ButtonPointerDown(Point cursor)
        {
            base.ButtonPointerDown(cursor);
            lock (_sync)
            {
                if (_disposed || IsTabRepositioning) return;
                _pressedFeedback?.Cancel();
                _pressedFeedback = null;
                Rectangle bounds = Rectangle.Empty;
                TitleBarTab tab = FindTab(cursor);
                if (tab != null && IsOverCloseButton(tab, cursor))
                {
                    _pressedFeedback = _visuals[tab].CloseFeedback;
                    bounds = tab.CloseButtonArea;
                    bounds.Offset(tab.Area.Location);
                }
                else if (IsOverAddButton(cursor))
                {
                    _pressedFeedback = _addFeedback;
                    bounds = _addButtonArea;
                }
                if (_pressedFeedback == null) return;
                var origin = new PointF(
                    ChromiumTabMetrics.Clamp((cursor.X - bounds.Left) * 2f / bounds.Width - 1, -1, 1),
                    ChromiumTabMetrics.Clamp((cursor.Y - bounds.Top) * 2f / bounds.Height - 1, -1, 1));
                _pressedFeedback.Press(AnimationTimeMilliseconds, origin);
                _buttonAnimating = true;
            }
            _parentWindow.RedrawTabs();
        }

        protected internal override void Overlay_MouseDown(object sender, MouseEventArgs e)
        {
            // A close-button press must not arm the tab's drag operation.
            if (_pressedFeedback != null) return;
            base.Overlay_MouseDown(sender, e);
        }

        internal override void ButtonPointerUp(Point cursor)
        {
            lock (_sync)
            {
                if (_pressedFeedback == null) return;
                // Hit-test the release before adding a tab can move the button.
                TitleBarTab tab = FindTab(cursor);
                bool inside = _pressedFeedback == _addFeedback ? IsOverAddButton(cursor) :
                    tab != null && _visuals[tab].CloseFeedback == _pressedFeedback && IsOverCloseButton(tab, cursor);
                ButtonFeedback released = _pressedFeedback;
                _pressedFeedback = null;
                released.Release(AnimationTimeMilliseconds, inside);
                _buttonAnimating |= released.IsAnimating;
            }
            // Consume the press before the click callback changes focus/capture.
            // A later cancellation must not abort an already committed ripple.
            _parentWindow.RedrawTabs();
        }

        internal override void CancelButtonPress()
        {
            lock (_sync)
            {
                if (_pressedFeedback == null) return;
                _pressedFeedback.Cancel(AnimationTimeMilliseconds);
                _pressedFeedback = null;
                _buttonAnimating = true;
            }
            _parentWindow.RedrawTabs();
        }

        protected internal override void Overlay_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                ButtonPointerUp(_parentWindow._overlay.GetRelativeCursorPosition(e.Location));
            // Once the drag ends, changed targets animate back from their
            // displayed bounds, including drags across the pinned boundary.
            base.Overlay_MouseUp(sender, e);
            bool redraw;
            lock (_sync)
            {
                redraw = _sizingBoxes.CancelPress();
            }
            if (redraw) _parentWindow.RedrawTabs();
        }

        private void ParentDeactivated(object sender, EventArgs e)
        {
            lock (_sync)
            {
                bool captionPressed = _sizingBoxes.CancelPress();
                if (_pressedFeedback == null && !captionPressed) return;
                _pressedFeedback?.Cancel(AnimationTimeMilliseconds);
                _pressedFeedback = null;
            }
            _parentWindow.RedrawTabs();
        }

        public override TitleBarTab OverTab(IEnumerable<TitleBarTab> tabs, Point cursor)
        {
            lock (_sync) return FindTab(cursor, tabs);
        }

        private TitleBarTab FindTab(Point cursor, IEnumerable<TitleBarTab> candidates = null)
        {
            if (!_tabPaintBounds.Contains(cursor)) return null;
            // Exactly the reverse of painting, including hovered and dragged tabs.
            for (int i = _paintOrder.Count - 1; i >= 0; --i)
            {
                TitleBarTab tab = _paintOrder[i];
                Visual visual;
                if ((candidates == null || candidates.Contains(tab)) && _visuals.TryGetValue(tab, out visual) &&
                    visual.Bounds.Contains(cursor) && visual.Geometry != null &&
                    visual.Geometry.HitTest.Contains(cursor.X - visual.Bounds.X, cursor.Y - visual.Bounds.Y))
                    return tab;
            }
            return null;
        }

        private bool ShouldShowTab(TitleBarTab tab, Visual visual, int activeIndex, int[] widths)
        {
            // TabStrip::ShouldTabBeVisible hides tabs that cross the trailing
            // edge, rather than leaving a partial tab beside the + button.
            if (visual.Bounds.Right > _tabPaintBounds.Right || visual.Bounds.Right <= _tabPaintBounds.Left)
                return false;
            if (visual.Closing || tab.IsPinned || activeIndex <= visual.Index) return true;

            // A background tab before the active one must still fit if selected.
            int extraWidth = Math.Max(0, widths[activeIndex] - widths[visual.Index]);
            return visual.Bounds.Right + extraWidth <= _tabPaintBounds.Right;
        }

        public override bool IsOverAddButton(Point cursor)
        {
            if (!ShowAddButton || _wasTabRepositioning || !_addButtonArea.Contains(cursor)) return false;
            if (_parentWindow.WindowState == FormWindowState.Maximized && cursor.Y < _addButtonArea.Top + _addButtonArea.Height / 2)
                return true;
            float dx = cursor.X - (_addButtonArea.Left + _addButtonArea.Width / 2f);
            float dy = cursor.Y - (_addButtonArea.Top + _addButtonArea.Height / 2f);
            return dx * dx + dy * dy <= _addButtonArea.Width * _addButtonArea.Width / 4f;
        }

        public override bool IsOverCloseButton(TitleBarTab tab, Point cursor)
        {
            lock (_sync)
                return base.IsOverCloseButton(tab, cursor) && FindTab(cursor) == tab;
        }

        protected override int GetMaxTabAreaWidth(List<TitleBarTab> tabs, Point offset)
        {
            return Math.Max(1, _parentWindow.ClientSize.Width - offset.X - _sizingBoxes.Width -
                (ShowAddButton ? Scale(ChromiumTabMetrics.NewTabButtonSize + 8 + AddButtonMarginRight) : 0));
        }

        public override void Render(List<TitleBarTab> tabs, Graphics graphics, Point offset, Point cursor, bool forceRedraw = false)
        {
            if (_disposed || _suspendRendering || tabs == null) return;
            lock (_sync)
            {
                float scale = RenderScale;
                bool windowActive = _parentWindow._overlay?.IsWindowActive ?? (Form.ActiveForm == _parentWindow);
                ChromiumTabTheme.WindowPalette palette = windowActive ? Theme.ActiveWindow : Theme.InactiveWindow;
                _sizingBoxes.Scale = scale;
                double now = AnimationTimeMilliseconds;
                bool animate = ShouldAnimateLayout();
                if (IsTabClosingMode && (_parentWindow.ClientSize != _mouseCloseWindowSize ||
                    scale != _mouseCloseScale || IsTabRepositioning || _detachedTabX.HasValue || tabs.Count == 0 ||
                    tabs.Any(tab => !_visuals.ContainsKey(tab))))
                    ExitTabClosingMode();
                UpdateTabClosingPointer(cursor);
                _liveTabs.Clear();
                foreach (TitleBarTab tab in tabs) _liveTabs.Add(tab);
                _removedTabs.Clear();
                _closingTabs.Clear();
                foreach (var pair in _visuals)
                {
                    if (_liveTabs.Contains(pair.Key)) continue;
                    Visual visual = pair.Value;
                    if (visual.Closing && animate && !IsTabRepositioning && !_detachedTabX.HasValue &&
                        visual.Geometry.Scale == scale && visual.Bounds.Y == offset.Y + TopPadding)
                        _closingTabs.Add(pair.Key);
                    else _removedTabs.Add(pair.Key);
                }
                foreach (TitleBarTab removed in _removedTabs)
                {
                    if (_pressedFeedback == _visuals[removed].CloseFeedback) _pressedFeedback = null;
                    _visuals[removed].Dispose(); _visuals.Remove(removed);
                }
                _animationItems.Clear();
                foreach (TitleBarTab tab in tabs) _animationItems.Add(tab);
                foreach (TitleBarTab tab in _closingTabs) _animationItems.Add(tab);
                if (ShowAddButton) _animationItems.Add(_addKey);
                _animation.BeginFrame(_animationItems, now, animate);

                Point screenOrigin = _parentWindow.PointToScreen(Point.Empty);
                int startX = SystemInformation.BorderSize.Width + offset.X;
                int y = offset.Y + TopPadding;
                _maxTabArea = new Rectangle(screenOrigin.X + startX, screenOrigin.Y + offset.Y,
                    GetMaxTabAreaWidth(tabs, offset), TabHeight);
                _tabPaintBounds = new Rectangle(startX, y, _maxTabArea.Width, Scale(ChromiumTabMetrics.Height));
                int activeIndex = tabs.FindIndex(t => t.Active);
                bool inserting = animate && !IsTabRepositioning && !_detachedTabX.HasValue &&
                    tabs.Any(tab => !_visuals.ContainsKey(tab)) && tabs.Any(tab => _visuals.ContainsKey(tab));
                if (inserting)
                {
                    // Start against the neighbour's displayed edge, before any
                    // existing tab moves to make room. In a crowded strip this
                    // keeps adjacent tabs joined throughout the opening motion.
                    int edge = _visuals[tabs.First(tab => _visuals.ContainsKey(tab))].Bounds.Left;
                    foreach (TitleBarTab tab in tabs)
                    {
                        Visual existing;
                        if (_visuals.TryGetValue(tab, out existing)) edge = existing.Bounds.Right - OverlapWidth;
                        else _animation.SetInitialBounds(tab, new Rectangle(edge, y, OverlapWidth, Scale(ChromiumTabMetrics.Height)));
                    }
                }
                int pinnedCount = _parentWindow.PinnedTabCount;
                int[] widths = ChromiumTabMetrics.LayoutWidths(tabs.Count, pinnedCount, activeIndex,
                    _availableWidthDuringMouseClose ?? _maxTabArea.Width, scale);
                if (_detachedTabWidth.HasValue && tabs.Count == 1) widths[0] = _detachedTabWidth.Value;
                _tabContentWidth = widths.Length == 0 ? 0 : Math.Max(0, widths[0] - Scale(16));

                // Preserve EasyTabs' pointer-anchored drag and list-reorder contract.
                int? draggedX = null;
                if (activeIndex >= 0 && IsTabRepositioning && _tabClickOffset.HasValue)
                {
                    int width = widths[activeIndex];
                    UpdateTabDragOffset(new Size(width, Scale(ChromiumTabMetrics.Height)));
                    bool pinned = tabs[activeIndex].IsPinned;
                    int first = pinned ? 0 : pinnedCount;
                    int last = pinned ? pinnedCount - 1 : tabs.Count - 1;
                    int sectionStart = startX;
                    for (int i = 0; i < first; i++) sectionStart += widths[i] - OverlapWidth;
                    // GetAttachedDragPoint clamps the visual only to the full
                    // strip. The legal model slots below remain section-specific.
                    draggedX = Math.Max(startX, Math.Min(startX + _maxTabArea.Width - width, cursor.X - _tabClickOffset.Value));
                    // Compare against model slot centers, never animating neighbour
                    // positions: a stationary pointer must not repeatedly reorder.
                    // In a crowded strip the active slot is wider. Calculate each
                    // candidate's leading edge from the preceding inactive widths,
                    // independent of where the active tab currently sits.
                    int[] slotWidths = pinned ? widths : ChromiumTabMetrics.LayoutWidths(
                        tabs.Count, pinnedCount, last, _maxTabArea.Width, scale);
                    int drop = first;
                    int slotX = sectionStart;
                    for (int i = first; i < last; i++)
                    {
                        int nextSlot = slotX + slotWidths[i] - OverlapWidth;
                        if (draggedX.Value - TabRepositionDragDistance > (slotX + nextSlot) / 2) drop = i + 1;
                        slotX = nextSlot;
                    }
                    if (drop != activeIndex)
                    {
                        TitleBarTab tab = tabs[activeIndex];
                        _parentWindow.Tabs.SuppressEvents();
                        try { _parentWindow.Tabs.Remove(tab); _parentWindow.Tabs.Insert(drop, tab); }
                        finally { _parentWindow.Tabs.ResumeEvents(); }
                        activeIndex = drop;
                        widths = ChromiumTabMetrics.LayoutWidths(tabs.Count, pinnedCount, activeIndex, _maxTabArea.Width, scale);
                    }
                }

                int nextX = startX;
                for (int i = 0; i < tabs.Count; i++)
                {
                    TitleBarTab tab = tabs[i];
                    Visual visual;
                    bool added = !_visuals.TryGetValue(tab, out visual);
                    if (added) _visuals[tab] = visual = new Visual();
                    visual.Index = i;
                    Rectangle target = new Rectangle(nextX, y, widths[i], Scale(ChromiumTabMetrics.Height));
                    nextX += widths[i] - OverlapWidth;
                    visual.Target = target;
                    if (tab.Active && draggedX.HasValue) target.X = draggedX.Value;
                    if (_detachedTabX.HasValue && tabs.Count == 1) target.X = _detachedTabX.Value;
                    Rectangle bounds = _animation.GetBounds(tab, target,
                        (added && !inserting) || (tab.Active && (IsTabRepositioning || _detachedTabX.HasValue)), OverlapWidth);
                    visual.Bounds = tab.Area = bounds;
                    bool extend = _parentWindow.WindowState == FormWindowState.Maximized;
                    float stroke = tab.Active ? Theme.BorderWidth : 0;
                    ChromiumTabGeometry geometry = visual.Geometry;
                    if (geometry == null || geometry.Width != bounds.Width || geometry.Height != bounds.Height ||
                        geometry.Scale != scale || geometry.Stroke != stroke || geometry.ExtendHit != extend || geometry.First != (i == 0))
                    {
                        geometry?.Dispose();
                        visual.Geometry = new ChromiumTabGeometry(bounds.Width, bounds.Height, scale, stroke, extend, i == 0);
                    }
                }

                _hoverAnimating = false;
                int closingRight = startX;
                foreach (TitleBarTab tab in _closingTabs)
                {
                    Visual visual = _visuals[tab];
                    Visual previous = visual.Previous;
                    while (previous != null && previous.Closing) previous = previous.Previous;
                    // Chromium closes to the overlap width, matching the distance
                    // the following tabs travel. Only cached pixels are retained;
                    // the content form has already followed its normal close path.
                    // StartRemoveTabAnimation computes this target once. Later
                    // closes must not restart an already closing tab's 200 ms clock.
                    if (!visual.ClosingTargetInitialized)
                    {
                        visual.Target = new Rectangle(previous == null ? startX : previous.Target.Right - OverlapWidth,
                            y, OverlapWidth, Scale(ChromiumTabMetrics.Height));
                        visual.ClosingTargetInitialized = true;
                    }
                    visual.Bounds = _animation.GetBounds(tab, visual.Target, false, OverlapWidth);
                    if (visual.Bounds == visual.Target && !_animation.IsItemAnimating(tab))
                    {
                        visual.Dispose(); _visuals.Remove(tab); _animation.Forget(tab);
                        continue;
                    }
                    ChromiumTabGeometry geometry = visual.Geometry;
                    if (geometry.Width != visual.Bounds.Width || geometry.Stroke != 0)
                    {
                        visual.Geometry = new ChromiumTabGeometry(visual.Bounds.Width, visual.Bounds.Height,
                            scale, 0, geometry.ExtendHit, geometry.First);
                        geometry.Dispose();
                    }
                    closingRight = Math.Max(closingRight, visual.Bounds.Right);
                }

                LayoutAddButton(tabs, startX, y, closingRight, now);
                if (ShowAddButton)
                    _tabPaintBounds.Width = Math.Max(0, Math.Min(_tabPaintBounds.Right, _addButtonArea.Left) - startX);

                foreach (TitleBarTab tab in _closingTabs)
                {
                    Visual visual;
                    if (!_visuals.TryGetValue(tab, out visual)) continue;
                    visual.Visible = ShouldShowTab(tab, visual, activeIndex, widths);
                    // Keep the highlight while the pointer is inside a visible
                    // shrinking tab; hidden overflow cannot hover under buttons.
                    bool hovered = visual.Visible && _tabPaintBounds.Contains(cursor) && visual.Bounds.Contains(cursor) &&
                        visual.Geometry.HitTest.Contains(cursor.X - visual.Bounds.X, cursor.Y - visual.Bounds.Y);
                    UpdateHover(visual, hovered, cursor, animate, now);
                }

                _paintOrder.Clear();
                foreach (TitleBarTab tab in tabs)
                {
                    Visual visual = _visuals[tab];
                    visual.Visible = ShouldShowTab(tab, visual, activeIndex, widths);
                    if (visual.Visible) _paintOrder.Add(tab);
                    else
                    {
                        tab.CloseButtonArea = Rectangle.Empty;
                        visual.CloseFeedback.Cancel();
                        if (_pressedFeedback == visual.CloseFeedback) _pressedFeedback = null;
                    }
                }
                _paintOrder.Sort(_comparePaintOrder);
                _hoveredTab = IsTabRepositioning ? null : FindTab(cursor);
                foreach (TitleBarTab tab in tabs)
                {
                    Visual visual = _visuals[tab];
                    UpdateHover(visual, tab == _hoveredTab, cursor, animate, now);
                }
                _paintOrder.Sort(_comparePaintOrder);

                EnsureBuffer(Math.Max(1, _parentWindow.ClientSize.Width), Math.Max(1, TabHeight + offset.Y));
                _buttonAnimating = false;
                _contentAnimating = false;
                using (var canvas = new SKCanvas(_pixels))
                {
                    canvas.Clear(ToSkia(palette.Frame));
                    canvas.Save();
                    // Also contain antialiasing and aligned paths at fractional DPI.
                    canvas.ClipRect(new SKRect(_tabPaintBounds.Left, _tabPaintBounds.Top,
                        _tabPaintBounds.Right, _tabPaintBounds.Bottom));
                    // Closing visuals are never added to the mouse hit-test order.
                    foreach (TitleBarTab tab in _closingTabs)
                        if (_visuals.ContainsKey(tab)) PaintTab(canvas, tab, tabs, cursor, now, animate, false, palette);
                    foreach (TitleBarTab tab in _paintOrder) PaintTab(canvas, tab, tabs, cursor, now, animate, forceRedraw, palette);
                    canvas.Restore();
                    PaintAddButton(canvas, cursor, now, animate, palette);
                    // The toolbar covers the tabs' bottom overlap in Chromium.
                    // Paint this join last so inactive fills and hover effects
                    // cannot extend into it. Round its two edges independently.
                    int toolbarTop = y + Scale(ChromiumTabMetrics.Height - ChromiumTabMetrics.ToolbarOverlap);
                    using (var paint = new SKPaint { Color = ToSkia(palette.ActiveTab) })
                        canvas.DrawRect(0, toolbarTop, _pixels.Width, y + Scale(ChromiumTabMetrics.Height) - toolbarTop, paint);
                    canvas.Flush();
                }
                graphics.DrawImageUnscaled(_buffer, 0, 0);
                if (IsWindows10)
                {
                    _sizingBoxes.Render(graphics, cursor, palette.Frame, now, animate, palette.IsActive);
                    _buttonAnimating |= _sizingBoxes.IsAnimating;
                }
                _lastCursor = cursor;
                _previousTabCount = tabs.Count;
            }
        }

        private void UpdateHover(Visual visual, bool hovered, Point cursor, bool animate, double now)
        {
            visual.MouseHovered = hovered;
            visual.HoverAnimation.Update(hovered, now, animate);
            _hoverAnimating |= visual.HoverAnimation.IsAnimating;
            if (hovered) visual.HoverPoint = new Point(cursor.X - visual.Bounds.X, cursor.Y - visual.Bounds.Y);
        }

        private void EnsureBuffer(int width, int height)
        {
            if (_buffer != null && _buffer.Width == width && _buffer.Height == height) return;
            _buffer?.Dispose(); _pixels?.Dispose();
            _pixels = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul));
            _buffer = new Bitmap(width, height, _pixels.RowBytes, PixelFormat.Format32bppPArgb, _pixels.GetPixels());
        }

        private void PaintTab(SKCanvas canvas, TitleBarTab tab, List<TitleBarTab> tabs, Point cursor, double now, bool animate, bool forceRedraw,
            ChromiumTabTheme.WindowPalette palette)
        {
            Visual visual = _visuals[tab];
            if (!visual.Visible) return;
            ChromiumTabGeometry geometry = visual.Geometry;
            // Chromium's IsActiveTab returns false once a tab leaves the model.
            // The retained visual must not inherit the removed tab's Active flag.
            bool active = tab.Active && !visual.Closing;
            int index = visual.Index;
            float scale = geometry.Scale;
            float leading = visual.Closing ? 0 : SeparatorOpacity(tab, index > 0 ? tabs[index - 1] : null, true);
            float trailing = visual.Closing ? 0 : SeparatorOpacity(tab, index + 1 < tabs.Count ? tabs[index + 1] : null, false);
            float t = ChromiumTabMetrics.Clamp((geometry.Width / scale - 256) / (32 - 256f), 0, 1);
            float targetHoverOpacity = palette.HoverMinimum + (palette.HoverMaximum - palette.HoverMinimum) * t * t;
            float hoverOpacity = targetHoverOpacity * visual.Hover;
            Color background = active ? palette.ActiveTab : ChromiumTabTheme.Blend(palette.InactiveTab, palette.ActiveTab, hoverOpacity);
            // GM2TabStyle::CalculateColors uses the intended hover state for text,
            // independently of the background's fade. Update immediately on enter
            // and exit so narrow tabs do not switch text shades halfway through.
            float expectedOpacity = active ? 1 : visual.MouseHovered ? targetHoverOpacity : 0;
            Color expectedBackground = ChromiumTabTheme.Blend(palette.InactiveTab, palette.ActiveTab, expectedOpacity);
            Color foreground = palette.Foreground(expectedOpacity > .5f, expectedBackground);
            canvas.Save();
            canvas.Translate(visual.Bounds.X, visual.Bounds.Y);
            using (var paint = new SKPaint { IsAntialias = true, Color = ToSkia(background) })
            {
                canvas.DrawPath(geometry.Fill, paint);
                if (!active && visual.Hover > 0)
                {
                    canvas.Save(); canvas.ClipPath(geometry.Fill, SKClipOperation.Intersect, true);
                    SKColor center = ToSkia(palette.ActiveTab).WithAlpha((byte)(255 * palette.RadialOpacity * visual.Hover));
                    using (var shader = SKShader.CreateRadialGradient(new SKPoint(visual.HoverPoint.X, visual.HoverPoint.Y),
                        Math.Max(geometry.Width / 4f, 16 * scale), new[] { center, center.WithAlpha(0) }, SKShaderTileMode.Clamp))
                    {
                        paint.Shader = shader; canvas.DrawRect(geometry.AlignedBounds, paint); paint.Shader = null;
                    }
                    canvas.Restore();
                }
                if (geometry.Stroke > 0)
                {
                    paint.Style = SKPaintStyle.Stroke; paint.StrokeWidth = geometry.Stroke * scale;
                    paint.Color = ToSkia(palette.Border); canvas.DrawPath(geometry.Border, paint); paint.Style = SKPaintStyle.Fill;
                }
                float separatorY = (geometry.Height - 20 * scale) / 2;
                paint.Color = ToSkia(palette.Separator).WithAlpha((byte)(leading * 255));
                canvas.DrawRect(geometry.AlignedBounds.Left + 8 * scale, separatorY, scale, 20 * scale, paint);
                paint.Color = ToSkia(palette.Separator).WithAlpha((byte)(trailing * 255));
                canvas.DrawRect(geometry.AlignedBounds.Right - 9 * scale, separatorY, scale, 20 * scale, paint);
            }

            // Tab::Layout / UpdateIconVisibility. Mouse close target is the 16-DIP
            // glyph box; Chromium's larger touch-only border isn't a mouse target.
            float contentsWidth = geometry.Width / scale - 32;
            bool roomy = contentsWidth >= 68;
            bool close = !tab.IsPinned && tab.ShowCloseButton && (active || roomy) && (!visual.Closing || contentsWidth >= 16);
            if (!visual.Closing) visual.HasIcon = tab.IsPinned || tab.IsLoading || (tab.Content.ShowIcon && tab.Content.Icon != null);
            bool hasIcon = visual.HasIcon;
            bool icon = hasIcon && (tab.IsPinned || !active || contentsWidth - (close ? 16 : 0) >= 16);
            bool centerIcon = icon && !active && contentsWidth < 16;
            int contentStart = Scale(!tab.IsPinned && roomy ? 20 : 16);
            int iconX = centerIcon ? (geometry.Width - Scale(16)) / 2 : contentStart;
            bool normalContents = !tab.IsPinned || geometry.Width >= Scale(ChromiumTabMetrics.PinnedWidth + ChromiumTabMetrics.PinnedTitleThreshold);
            if (tab.IsPinned && !normalContents)
            {
                // Tab::MaybeAdjustLeftForPinnedTab: interpolate only in the last
                // 30 DIP of contraction; keep the favicon at native size.
                float progress = ChromiumTabMetrics.Clamp(1 - (geometry.Width / scale - ChromiumTabMetrics.PinnedWidth) /
                    ChromiumTabMetrics.PinnedTitleThreshold, 0, 1);
                int centered = (Scale(ChromiumTabMetrics.PinnedWidth) - Scale(16)) / 2;
                iconX += ChromiumTabMetrics.Pixel(progress * (centered - iconX));
            }
            int centerY = Scale(geometry.Stroke) + (geometry.Height - Scale(1 + geometry.Stroke * 2) - Scale(16)) / 2;
            int closeX = Math.Max(geometry.Width - Scale(32), (geometry.Width - Scale(16)) / 2);
            tab.CloseButtonArea = close ? new Rectangle(closeX, centerY, Scale(16), Scale(16)) : Rectangle.Empty;
            if (close)
            {
                bool hovered = !visual.Closing && !IsTabRepositioning && tab == _hoveredTab &&
                    base.IsOverCloseButton(tab, cursor);
                visual.CloseFeedback.Update(hovered, now, animate);
                _buttonAnimating |= visual.CloseFeedback.IsAnimating;
                visual.CloseFeedback.Paint(canvas, closeX + 8 * scale, centerY + 8 * scale, 8 * scale, background, scale);
                PaintClose(canvas, closeX, centerY, scale, foreground);
            }
            else { visual.CloseFeedback.Cancel(); visual.CloseFeedback.Update(false, now, false); }
            int titleLeft = icon ? Math.Max(contentStart, iconX + Scale(24)) : contentStart;
            int titleRight = close ? closeX - Scale(4) : geometry.Width - Scale(16);
            if (_font == null || _fontScale != scale)
            {
                _font?.Dispose(); _font = new Font("Segoe UI", 12 * scale, FontStyle.Regular, GraphicsUnit.Pixel); _fontScale = scale;
            }
            var iconBounds = new Rectangle(iconX, centerY, Scale(16), Scale(16));
            var titleBounds = new Rectangle(titleLeft, Scale(geometry.Stroke), Math.Max(0, titleRight - titleLeft),
                geometry.Height - Scale(1 + geometry.Stroke * 2));
            if (!visual.Closing)
            {
                titleBounds = AnimateTitle(visual, titleBounds, icon, now, animate);
                if (!normalContents) titleBounds = Rectangle.Empty;
                bool useDefault = tab.IsPinned && (!tab.Content.ShowIcon || tab.Content.Icon == null);
                Icon favicon = !icon ? null : useDefault ? (DefaultFavicon ?? SystemIcons.Application)
                    : tab.Content.ShowIcon ? tab.Content.Icon : null;
                bool realFavicon = favicon != null && !useDefault && !((tab.Content as ITabFaviconState)?.IsDefaultFavicon ?? false);
                bool waiting = (tab.Content as ITabLoadingPhase)?.IsWaiting ?? false;
                AnimateFavicon(visual, tab, now, animate);
                visual.ContentInitialized = true;
                visual.Content.Update(tab.Area.Size, tab.Caption, tab.IsLoading && (waiting || !realFavicon) ? null : favicon,
                    iconBounds, titleBounds, foreground, scale, _font, forceRedraw);
            }
            else
                visual.Content.UpdateClosingTitle(normalContents ? titleBounds : Rectangle.Empty,
                    foreground, scale, _font);
            RectangleF clip = geometry.ContentClip(leading, trailing);
            if (visual.Closing && close)
                clip.Width = Math.Max(0, Math.Min(clip.Right, titleRight) - clip.Left);
            canvas.Save();
            canvas.ClipRect(new SKRect(clip.Left, clip.Top, clip.Right, clip.Bottom));
            canvas.DrawBitmap(visual.Content.Pixels, 0, 0, new SKSamplingOptions(SKFilterMode.Nearest));
            if (visual.Content.HasFavicon) PaintFavicon(canvas, visual, scale);
            // Paint the spinner on the same raster canvas, with the same clip and
            // z-order as its tab. No per-tab GDI+ context or cross-renderer flush.
            if (!visual.Closing && icon && tab.IsLoading)
            {
                float start, sweep;
                var phase = tab.Content as ITabLoadingPhase;
                Color loadingColor = tab.Active ? ActiveLoadingIndicatorColor ?? LoadingIndicatorColor : LoadingIndicatorColor;
                Color waitingColor = ChromiumTabTheme.Blend(background, loadingColor, 0x47 / 255f);
                Color spinnerColor = loadingColor;
                if (ShowWaitingAnimation && phase != null && phase.IsWaiting)
                {
                    visual.WaitingElapsed = tab.LoadingElapsedMilliseconds;
                    visual.SpinningStarted = visual.WaitingArcOffset = double.NaN;
                    TabLoadingIndicator.GetWaitingAngles(visual.WaitingElapsed, out start, out sweep);
                    spinnerColor = waitingColor;
                }
                else if (ShowWaitingAnimation && phase != null)
                {
                    if (double.IsNaN(visual.SpinningStarted)) visual.SpinningStarted = now;
                    double elapsed = Math.Max(0, now - visual.SpinningStarted);
                    TabLoadingIndicator.GetAnglesAfterWaiting(elapsed, visual.WaitingElapsed,
                        ref visual.WaitingArcOffset, out start, out sweep);
                    spinnerColor = ChromiumTabTheme.Blend(waitingColor, loadingColor,
                        (float)TabLoadingIndicator.LinearOutSlowIn(elapsed / 900));
                }
                else TabLoadingIndicator.GetAngles(tab.LoadingElapsedMilliseconds, out start, out sweep);
                _spinnerPaint.Color = ToSkia(spinnerColor);
                _spinnerPaint.StrokeWidth = 2 * scale; // TabIcon's 2020 stroke width.
                float inset = scale;
                canvas.Save();
                canvas.ClipRect(new SKRect(iconBounds.Left, iconBounds.Top, iconBounds.Right, iconBounds.Bottom));
                canvas.DrawArc(new SKRect(iconBounds.Left + inset, iconBounds.Top + inset,
                    iconBounds.Right - inset, iconBounds.Bottom - inset), start, sweep, false, _spinnerPaint);
                canvas.Restore();
            }
            canvas.Restore();
            canvas.Restore();
        }

        private void AnimateFavicon(Visual visual, TitleBarTab tab, double now, bool animate)
        {
            // Chromium 85 TabIcon: loading immediately uses the small crop;
            // completion reveals a real favicon over a fresh 250 ms EASE_OUT.
            bool completed = visual.LoadingCompletionVersion != tab.LoadingCompletionVersion;
            if (tab.IsLoading && (!visual.WasLoading || completed))
            {
                visual.WaitingElapsed = 0;
                visual.SpinningStarted = visual.WaitingArcOffset = double.NaN;
            }
            if (tab.IsLoading || !animate)
                visual.FaviconStarted = double.NaN;
            else if (visual.ContentInitialized && (visual.WasLoading || completed))
                visual.FaviconStarted = tab.RevealFaviconOnLoadCompletion ? now : double.NaN;

            visual.FaviconProgress = tab.IsLoading ? 0 : 1;
            if (!double.IsNaN(visual.FaviconStarted))
            {
                float t = (float)Math.Max(0, Math.Min(1, (now - visual.FaviconStarted) / 250));
                visual.FaviconProgress = 1 - (1 - t) * (1 - t);
                if (t == 1) visual.FaviconStarted = double.NaN;
                else _contentAnimating = true;
            }
            visual.WasLoading = tab.IsLoading;
            visual.LoadingCompletionVersion = tab.LoadingCompletionVersion;
        }

        private Rectangle AnimateTitle(Visual visual, Rectangle target, bool showingIcon, double now, bool animate)
        {
            if (!visual.ContentInitialized || !animate || target.Width == 0 || visual.TitleBounds.Width == 0 ||
                target.Y != visual.TitleBounds.Y || target.Height != visual.TitleBounds.Height)
            {
                visual.TitleBounds = visual.TitleTarget = target;
                visual.TitleStarted = double.NaN;
            }
            else if (target != visual.TitleTarget)
            {
                // Tab::Layout animates the title only when the icon's visibility
                // changes. Ordinary resizing must not trail another animation.
                visual.TitleTarget = target;
                if (showingIcon != visual.ShowingIcon)
                {
                    // Tab::Layout retargets a running slide without restarting it.
                    if (double.IsNaN(visual.TitleStarted))
                    {
                        visual.TitleStart = visual.TitleBounds;
                        visual.TitleStarted = now;
                    }
                }
                else
                {
                    visual.TitleBounds = target;
                    visual.TitleStarted = double.NaN;
                }
            }
            if (!double.IsNaN(visual.TitleStarted))
            {
                double progress = Math.Max(0, Math.Min(1, (now - visual.TitleStarted) / 100));
                double eased = TabLoadingIndicator.FastOutSlowIn(progress);
                visual.TitleBounds = ChromiumBoundsAnimation.Interpolate(visual.TitleStart, target, eased);
                if (progress == 1) visual.TitleStarted = double.NaN;
                else _contentAnimating = true;
            }
            visual.ShowingIcon = showingIcon;
            return visual.TitleBounds;
        }

        private void PaintFavicon(SKCanvas canvas, Visual visual, float scale)
        {
            Rectangle source = visual.Content.FaviconSource;
            Rectangle bounds = visual.Content.IconBounds;
            var destination = new SKRect(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
            bool revealing = visual.FaviconProgress < 1;
            if (revealing)
            {
                // TabIcon::MaybePaintFavicon: scale/crop a 10-DIP circle to the
                // favicon's full diagonal. The reveal easing is applied above.
                float progress = visual.FaviconProgress;
                float diameter = (10 + ((float)Math.Sqrt(2) * 16 - 10) * progress) * scale;
                float cx = (bounds.Left + bounds.Right) / 2f, cy = (bounds.Top + bounds.Bottom) / 2f;
                float radius = diameter / 2;
                _faviconClip.SetOval(new SKRect(cx - radius, cy - radius, cx + radius, cy + radius));
                canvas.Save();
                canvas.ClipRoundRect(_faviconClip, SKClipOperation.Intersect, true);
                float halfSize = Math.Min(diameter, bounds.Width) / 2;
                destination = new SKRect(cx - halfSize, cy - halfSize, cx + halfSize, cy + halfSize);
            }
            canvas.DrawBitmap(visual.Content.Pixels, new SKRect(source.Left, source.Top, source.Right, source.Bottom),
                destination, new SKSamplingOptions(SKFilterMode.Linear));
            if (revealing) canvas.Restore();
        }

        private int ComparePaintOrder(TitleBarTab left, TitleBarTab right)
        {
            Visual a = _visuals[left], b = _visuals[right];
            float rankA = left.Active ? float.MaxValue : a.Hover + (left == _hoveredTab ? 2 : 0);
            float rankB = right.Active ? float.MaxValue : b.Hover + (right == _hoveredTab ? 2 : 0);
            int rank = rankA.CompareTo(rankB);
            return rank != 0 ? rank : b.Index.CompareTo(a.Index);
        }

        private float SeparatorOpacity(TitleBarTab tab, TitleBarTab adjacent, bool leading)
        {
            if (tab.Active) return 0;
            Visual visual = _visuals[tab];
            if (adjacent == null && leading)
                return Math.Min(1, Math.Abs(visual.Bounds.X - visual.Target.X) / (float)Math.Max(visual.Bounds.Width, visual.Target.Width));
            float otherHover = adjacent != null && !adjacent.Active ? _visuals[adjacent].Hover : 0;
            return 1 - Math.Max(visual.Hover, otherHover);
        }

        private static void PaintClose(SKCanvas canvas, float x, float y, float scale, Color foreground)
        {
            using (var paint = new SKPaint { IsAntialias = true, Color = ToSkia(foreground) })
            {
                // TabCloseButton::OnPaint: 16-DIP glyph box, 1.5-DIP rounded stroke.
                paint.Style = SKPaintStyle.Stroke; paint.StrokeWidth = 1.5f * scale; paint.StrokeCap = SKStrokeCap.Round;
                float inset = 4.75f * scale, end = 11.25f * scale;
                canvas.DrawLine(x + inset, y + inset, x + end, y + end, paint);
                canvas.DrawLine(x + end, y + inset, x + inset, y + end, paint);
            }
        }

        private void LayoutAddButton(List<TitleBarTab> tabs, int startX, int y, int closingRight, double now)
        {
            if (!ShowAddButton)
            {
                _addButtonArea = Rectangle.Empty;
                _addHovered = false;
                _addFeedback.Cancel();
                _addFeedback.Update(false, now, false);
                if (_pressedFeedback == _addFeedback) _pressedFeedback = null;
                return;
            }
            // Anchor to the normal layout, so reordering cannot pull the button
            // inward. A drag only pushes it out when tabs enter unused strip space.
            int right = tabs.Count == 0 ? startX : _visuals[tabs[tabs.Count - 1]].Target.Right;
            int visibleRight = tabs.Count == 0 ? startX : tabs.Max(t => t.Area.Right);
            visibleRight = Math.Max(visibleRight, closingRight);
            bool dragging = IsTabRepositioning || _detachedTabX.HasValue;
            if (dragging) right = Math.Max(right, visibleRight);
            int maximumX = startX + _maxTabArea.Width;
            int x = Math.Min(maximumX, right);
            var target = new Rectangle(x, y + Scale(3), Scale(28), Scale(28));
            _addButtonArea = _animation.GetBounds(_addKey, target, dragging, target.Width);
            // Chromium gives the button its own bounds animation. Only a manual
            // drag pushes it outside the tabs; normal frames keep that curve intact.
            if (dragging)
                _addButtonArea.X = Math.Min(maximumX, Math.Max(_addButtonArea.X, visibleRight));
            // A window resize can leave the old animated bounds outside the new
            // tab area. Keep the button clear of the caption controls immediately.
            if (_addButtonArea.X > maximumX)
            {
                _addButtonArea.X = maximumX;
                _animation.RecordDisplayedBounds(_addKey, _addButtonArea);
            }
        }

        private void PaintAddButton(SKCanvas canvas, Point cursor, double now, bool animate,
            ChromiumTabTheme.WindowPalette palette)
        {
            if (!ShowAddButton) return;
            float cx = _addButtonArea.X + _addButtonArea.Width / 2f;
            float cy = _addButtonArea.Y + _addButtonArea.Height / 2f;
            _addHovered = !IsTabRepositioning && IsOverAddButton(cursor);
            _addFeedback.Update(_addHovered, now, animate);
            _buttonAnimating |= _addFeedback.IsAnimating;
            _addFeedback.Paint(canvas, cx, cy, Scale(14), palette.Frame, RenderScale);
            using (var paint = new SKPaint { IsAntialias = true, Color = ToSkia(palette.Foreground(false, palette.InactiveTab)) })
            {
                paint.Style = SKPaintStyle.Stroke; paint.StrokeWidth = 2 * RenderScale; paint.StrokeCap = SKStrokeCap.Round;
                canvas.DrawLine(cx - Scale(5), cy, cx + Scale(5), cy, paint);
                canvas.DrawLine(cx, cy - Scale(5), cx, cy + Scale(5), paint);
            }
        }

        private static SKColor ToSkia(Color color) => new SKColor(color.R, color.G, color.B, color.A);
        private void ParentDisposed(object sender, EventArgs e) => Dispose();
        public override void Dispose()
        {
            lock (_sync)
            {
                if (_disposed) return;
                _disposed = true;
                _parentWindow.Disposed -= ParentDisposed;
                _parentWindow.Deactivate -= ParentDeactivated;
                foreach (Visual visual in _visuals.Values) visual.Dispose();
                _visuals.Clear(); _paintOrder.Clear();
                _animation.Reset();
                _buffer?.Dispose(); _pixels?.Dispose(); _font?.Dispose();
                _addFeedback.Dispose(); _pressedFeedback = null;
                _spinnerPaint.Dispose();
                _faviconClip.Dispose();
                _sizingBoxes.Dispose();
                base.Dispose();
            }
        }
    }
}
