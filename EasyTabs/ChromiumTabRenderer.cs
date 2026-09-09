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
        private sealed class Visual : IDisposable
        {
            internal ChromiumTabGeometry Geometry;
            internal Rectangle Bounds, Target;
            internal float Hover;
            internal Point HoverPoint;
            internal bool Closing, HasIcon;
            internal Visual Previous;
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

            internal void Update(Size size, string caption, Icon icon, Rectangle iconBounds,
                Rectangle titleBounds, Color foreground, float scale, Font font, bool force)
            {
                if (Pixels == null || Pixels.Width != size.Width || Pixels.Height != size.Height)
                {
                    Dispose();
                    Pixels = new SKBitmap(new SKImageInfo(size.Width, size.Height, SKColorType.Bgra8888, SKAlphaType.Premul));
                    _bitmap = new Bitmap(size.Width, size.Height, Pixels.RowBytes, PixelFormat.Format32bppPArgb, Pixels.GetPixels());
                    _graphics = Graphics.FromImage(_bitmap);
                    _graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    force = true;
                }
                if (!force && _caption == caption && ReferenceEquals(_icon, icon) &&
                    _iconBounds == iconBounds && _titleBounds == titleBounds && _foreground == foreground && _scale == scale) return;

                _graphics.Clear(Color.Transparent);
                if (icon != null) _graphics.DrawIcon(icon, iconBounds);
                if (titleBounds.Width > 0)
                {
                    using (var brush = new SolidBrush(foreground))
                    using (var format = new StringFormat(StringFormat.GenericTypographic)
                    { FormatFlags = StringFormatFlags.NoWrap, Trimming = StringTrimming.EllipsisCharacter, LineAlignment = StringAlignment.Center })
                        _graphics.DrawString(caption, font, brush, titleBounds, format);
                }
                _graphics.Flush(System.Drawing.Drawing2D.FlushIntention.Sync);
                Pixels.NotifyPixelsChanged();
                _caption = caption; _icon = icon; _iconBounds = iconBounds; _titleBounds = titleBounds;
                _foreground = foreground; _scale = scale;
            }

            public void Dispose()
            {
                _graphics?.Dispose(); _graphics = null;
                _bitmap?.Dispose(); _bitmap = null;
                Pixels?.Dispose(); Pixels = null;
                _icon = null; // Borrowed from the tab's form, never owned here.
            }
        }

        // Chrome 86 uses a 16% hover highlight and 14% ink drop. Close-button
        // fading is a Quartz extension: upstream deliberately disables that fade.
        private sealed class ButtonFeedback : IDisposable
        {
            private float _hoverFrom, _hoverTarget;
            private double _hoverStarted;
            private double? _pressedAt, _releasedAt;
            private bool _held, _inside;
            private SKPath _clip;
            private float _clipRadius;
            internal float HoverOpacity { get; private set; }
            internal float InkOpacity { get; private set; }
            internal float InkProgress { get; private set; }
            internal PointF Origin { get; private set; }
            internal bool IsAnimating { get; private set; }

            private static float Progress(double elapsed, double duration) =>
                (float)Math.Max(0, Math.Min(1, elapsed / duration));
            private float HoverAt(double now)
            {
                float t = Progress(now - _hoverStarted, 200);
                t = t * t * (3 - 2 * t);
                return _hoverFrom + (_hoverTarget - _hoverFrom) * t;
            }

            internal void Press(double now, PointF origin)
            {
                _pressedAt = now;
                _releasedAt = null;
                _held = _inside = true;
                Origin = origin;
                IsAnimating = true;
            }

            internal void Release(double now)
            {
                if (!_held) return; // The native message and global hook may both release.
                _held = false;
                if (_inside) _releasedAt = now;
                else _pressedAt = _releasedAt = null;
            }

            internal void Cancel()
            {
                _held = false;
                _pressedAt = _releasedAt = null;
                InkOpacity = 0;
            }

            internal void Update(bool hovered, double now, bool animate)
            {
                _inside = hovered;
                float target = hovered ? 1 : 0;
                if (target != _hoverTarget)
                {
                    _hoverFrom = HoverAt(now);
                    _hoverTarget = target;
                    _hoverStarted = now;
                }
                if (!animate) _hoverFrom = _hoverTarget;
                HoverOpacity = .16f * HoverAt(now);
                IsAnimating = animate && HoverAt(now) != _hoverTarget;
                InkOpacity = 0;
                if (!_pressedAt.HasValue) return;

                float grow = animate ? Progress(now - _pressedAt.Value, 225) : 1;
                InkProgress = 1 - (float)Math.Pow(1 - grow, 3);
                float fade = _releasedAt.HasValue ? (animate ? Progress(now - _releasedAt.Value, 160) : 1) : 0;
                if (fade == 1)
                {
                    _pressedAt = _releasedAt = null;
                    return;
                }
                InkOpacity = _held && !hovered ? 0 : .14f * (1 - fade);
                IsAnimating |= animate && (grow < 1 || _releasedAt.HasValue);
            }

            internal void Paint(SKCanvas canvas, float cx, float cy, float radius, Color background)
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
                    float fullRadius = radius + (float)Math.Sqrt(x * x + y * y);
                    paint.Color = ink.WithAlpha((byte)(255 * InkOpacity));
                    canvas.DrawCircle(x, y, fullRadius * (.15f + .85f * InkProgress), paint);
                    canvas.Restore();
                }
            }

            public void Dispose() { _clip?.Dispose(); }
        }

        private readonly object _sync = new object();
        private readonly Dictionary<TitleBarTab, Visual> _visuals = new Dictionary<TitleBarTab, Visual>();
        private readonly TabLayoutAnimation _animation = new TabLayoutAnimation();
        private readonly Stopwatch _clock = Stopwatch.StartNew();
        private readonly object _addKey = new object();
        private readonly ButtonFeedback _addFeedback = new ButtonFeedback();
        private ButtonFeedback _pressedFeedback;
        private readonly WindowsSizingBoxes _sizingBoxes;
        private readonly Size _originalMinimum;
        private List<TitleBarTab> _paintOrder = new List<TitleBarTab>();
        private SKBitmap _pixels;
        private Bitmap _buffer;
        private Font _font;
        private float _fontScale;
        private double _lastPaint;
        private bool _hoverAnimating, _buttonAnimating, _addHovered, _disposed;
        private Point _lastCursor = new Point(int.MinValue, int.MinValue);
        private TitleBarTab _hoveredTab;
        private ChromiumTabTheme _theme = ChromiumTabTheme.Light;

        public ChromiumTabRenderer(TitleBarTabs parentWindow) : base(parentWindow)
        {
            _sizingBoxes = new WindowsSizingBoxes(parentWindow);
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
                if (_parentWindow.TabRenderer == this) _parentWindow._overlay?.Render(true);
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
        internal override bool IsLayoutAnimating => _animation.IsAnimating || _hoverAnimating || _buttonAnimating;
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
        }

        internal override bool RequiresHoverRedraw(Point cursor)
        {
            lock (_sync)
                return cursor != _lastCursor && (_hoveredTab != null || FindTab(cursor) != null ||
                    _addHovered || IsOverAddButton(cursor));
        }

        internal override void BeginTabClose(TitleBarTab tab)
        {
            lock (_sync)
            {
                Visual visual;
                int index = _parentWindow.Tabs.IndexOf(tab);
                if (_disposed || index < 0 || !ShouldAnimateLayout() ||
                    (_parentWindow.Tabs.Count == 1 && _parentWindow.ExitOnLastTabClose) ||
                    !_visuals.TryGetValue(tab, out visual) || visual.Geometry == null || visual.Content.Pixels == null) return;
                visual.Closing = true;
                visual.Previous = null;
                if (index > 0) _visuals.TryGetValue(_parentWindow.Tabs[index - 1], out visual.Previous);
                visual.Hover = 0;
                visual.CloseFeedback.Cancel();
                if (_pressedFeedback == visual.CloseFeedback) _pressedFeedback = null;
            }
        }

        internal override void ButtonPointerDown(Point cursor)
        {
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

        protected internal override void Overlay_MouseUp(object sender, MouseEventArgs e)
        {
            base.Overlay_MouseUp(sender, e);
            bool redraw;
            lock (_sync)
            {
                redraw = _pressedFeedback != null;
                _pressedFeedback?.Release(AnimationTimeMilliseconds);
                _pressedFeedback = null;
            }
            if (redraw) _parentWindow.RedrawTabs();
        }

        private void ParentDeactivated(object sender, EventArgs e)
        {
            lock (_sync)
            {
                if (_pressedFeedback == null) return;
                _pressedFeedback.Cancel();
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
                _sizingBoxes.Scale = scale;
                double now = AnimationTimeMilliseconds;
                float step = (float)Math.Max(0, Math.Min(64, now - _lastPaint)) / 200f;
                _lastPaint = now;
                bool animate = ShouldAnimateLayout();
                foreach (TitleBarTab removed in _visuals.Keys.Where(t => !tabs.Contains(t)).ToArray())
                {
                    Visual visual = _visuals[removed];
                    if (visual.Closing && animate && !IsTabRepositioning && !_detachedTabX.HasValue &&
                        visual.Geometry.Scale == scale && visual.Bounds.Y == offset.Y + TopPadding) continue;
                    if (_pressedFeedback == _visuals[removed].CloseFeedback) _pressedFeedback = null;
                    _visuals[removed].Dispose(); _visuals.Remove(removed);
                }
                TitleBarTab[] closingTabs = _visuals.Where(pair => pair.Value.Closing).Select(pair => pair.Key).ToArray();
                _animation.BeginFrame(tabs.Cast<object>().Concat(closingTabs)
                    .Concat(ShowAddButton ? new[] { _addKey } : new object[0]), now, animate);

                Point screenOrigin = _parentWindow.PointToScreen(Point.Empty);
                int startX = SystemInformation.BorderSize.Width + offset.X;
                int y = offset.Y + TopPadding;
                _maxTabArea = new Rectangle(screenOrigin.X + startX, screenOrigin.Y + offset.Y,
                    GetMaxTabAreaWidth(tabs, offset), TabHeight);
                int activeIndex = tabs.FindIndex(t => t.Active);
                int[] widths = ChromiumTabMetrics.LayoutWidths(tabs.Count, activeIndex, _maxTabArea.Width, scale);
                if (_detachedTabWidth.HasValue && tabs.Count == 1) widths[0] = _detachedTabWidth.Value;
                _tabContentWidth = widths.Length == 0 ? 0 : Math.Max(0, widths[0] - Scale(16));

                // Preserve EasyTabs' pointer-anchored drag and list-reorder contract.
                int? draggedX = null;
                if (activeIndex >= 0 && IsTabRepositioning && _tabClickOffset.HasValue)
                {
                    int width = widths[activeIndex];
                    draggedX = Math.Max(startX, Math.Min(startX + _maxTabArea.Width - width, cursor.X - _tabClickOffset.Value));
                    int drop = Math.Max(0, Math.Min(tabs.Count - 1, (int)Math.Round(
                        (draggedX.Value - startX - TabRepositionDragDistance) / (double)Math.Max(1, width - OverlapWidth))));
                    if (drop != activeIndex)
                    {
                        TitleBarTab tab = tabs[activeIndex];
                        _parentWindow.Tabs.SuppressEvents();
                        try { _parentWindow.Tabs.Remove(tab); _parentWindow.Tabs.Insert(drop, tab); }
                        finally { _parentWindow.Tabs.ResumeEvents(); }
                        activeIndex = drop;
                        widths = ChromiumTabMetrics.LayoutWidths(tabs.Count, activeIndex, _maxTabArea.Width, scale);
                    }
                }

                int nextX = startX;
                for (int i = 0; i < tabs.Count; i++)
                {
                    TitleBarTab tab = tabs[i];
                    Visual visual;
                    if (!_visuals.TryGetValue(tab, out visual)) _visuals[tab] = visual = new Visual();
                    Rectangle target = new Rectangle(nextX, y, widths[i], Scale(ChromiumTabMetrics.Height));
                    nextX += widths[i] - OverlapWidth;
                    visual.Target = target;
                    if (tab.Active && draggedX.HasValue) target.X = draggedX.Value;
                    if (_detachedTabX.HasValue && tabs.Count == 1) target.X = _detachedTabX.Value;
                    Rectangle bounds = _animation.GetBounds(tab, target,
                        tab.Active && (IsTabRepositioning || _detachedTabX.HasValue), Scale(ChromiumTabMetrics.MinimumInactiveWidth));
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

                int closingRight = startX;
                foreach (TitleBarTab tab in closingTabs)
                {
                    Visual visual = _visuals[tab];
                    Visual previous = visual.Previous;
                    while (previous != null && previous.Closing) previous = previous.Previous;
                    // Chromium closes to the overlap width, matching the distance
                    // the following tabs travel. Only cached pixels are retained;
                    // the content form has already followed its normal close path.
                    visual.Target = new Rectangle(previous == null ? startX : previous.Target.Right - OverlapWidth,
                        y, OverlapWidth, Scale(ChromiumTabMetrics.Height));
                    visual.Bounds = _animation.GetBounds(tab, visual.Target, false, OverlapWidth);
                    if (visual.Bounds == visual.Target)
                    {
                        visual.Dispose(); _visuals.Remove(tab); _animation.Forget(tab);
                        continue;
                    }
                    ChromiumTabGeometry geometry = visual.Geometry;
                    if (geometry.Width != visual.Bounds.Width)
                    {
                        visual.Geometry = new ChromiumTabGeometry(visual.Bounds.Width, visual.Bounds.Height,
                            scale, geometry.Stroke, geometry.ExtendHit, geometry.First);
                        geometry.Dispose();
                    }
                    closingRight = Math.Max(closingRight, visual.Bounds.Right);
                }

                _paintOrder = tabs.OrderBy(t => t.Active ? float.MaxValue : _visuals[t].Hover + (t == _hoveredTab ? 2 : 0))
                    .ThenByDescending(t => tabs.IndexOf(t)).ToList();
                _hoveredTab = IsTabRepositioning ? null : FindTab(cursor);
                _hoverAnimating = false;
                foreach (TitleBarTab tab in tabs)
                {
                    Visual visual = _visuals[tab];
                    float target = tab == _hoveredTab && !tab.Active ? 1 : 0;
                    visual.Hover = !animate ? target : target > visual.Hover ? Math.Min(target, visual.Hover + step) : Math.Max(target, visual.Hover - step);
                    _hoverAnimating |= visual.Hover != target;
                    if (tab == _hoveredTab) visual.HoverPoint = new Point(cursor.X - tab.Area.X, cursor.Y - tab.Area.Y);
                }
                _paintOrder = tabs.OrderBy(t => t.Active ? float.MaxValue : _visuals[t].Hover + (t == _hoveredTab ? 2 : 0))
                    .ThenByDescending(t => tabs.IndexOf(t)).ToList();

                EnsureBuffer(Math.Max(1, _parentWindow.ClientSize.Width), Math.Max(1, TabHeight + offset.Y));
                _buttonAnimating = false;
                using (var canvas = new SKCanvas(_pixels))
                {
                    canvas.Clear(ToSkia(Theme.Frame));
                    // A continuous one-DIP connection to the toolbar, also covering
                    // the area below the trailing tabs and the new-tab button.
                    using (var paint = new SKPaint { Color = ToSkia(Theme.ActiveTab) })
                        canvas.DrawRect(0, y + Scale(34), _pixels.Width, Scale(1), paint);
                    // Closing visuals are never added to the mouse hit-test order.
                    foreach (TitleBarTab tab in closingTabs)
                        if (_visuals.ContainsKey(tab)) PaintTab(canvas, tab, tabs, cursor, now, animate, false);
                    foreach (TitleBarTab tab in _paintOrder) PaintTab(canvas, tab, tabs, cursor, now, animate, forceRedraw);
                    PaintAddButton(canvas, tabs, startX, y, cursor, now, animate, closingRight);
                    canvas.Flush();
                }
                graphics.DrawImageUnscaled(_buffer, 0, 0);
                if (IsWindows10) _sizingBoxes.Render(graphics, cursor);
                _lastCursor = cursor;
                _previousTabCount = tabs.Count;
            }
        }

        private void EnsureBuffer(int width, int height)
        {
            if (_buffer != null && _buffer.Width == width && _buffer.Height == height) return;
            _buffer?.Dispose(); _pixels?.Dispose();
            _pixels = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul));
            _buffer = new Bitmap(width, height, _pixels.RowBytes, PixelFormat.Format32bppPArgb, _pixels.GetPixels());
        }

        private void PaintTab(SKCanvas canvas, TitleBarTab tab, List<TitleBarTab> tabs, Point cursor, double now, bool animate, bool forceRedraw)
        {
            Visual visual = _visuals[tab];
            ChromiumTabGeometry geometry = visual.Geometry;
            int index = tabs.IndexOf(tab);
            float scale = geometry.Scale;
            float leading = visual.Closing ? 0 : SeparatorOpacity(tab, index > 0 ? tabs[index - 1] : null, true);
            float trailing = visual.Closing ? 0 : SeparatorOpacity(tab, index + 1 < tabs.Count ? tabs[index + 1] : null, false);
            float t = ChromiumTabMetrics.Clamp((geometry.Width / scale - 256) / (32 - 256f), 0, 1);
            float hoverOpacity = (Theme.HoverMinimum + (Theme.HoverMaximum - Theme.HoverMinimum) * t * t) * visual.Hover;
            Color background = tab.Active ? Theme.ActiveTab : ChromiumTabTheme.Blend(Theme.InactiveTab, Theme.ActiveTab, hoverOpacity);
            Color foreground = tab.Active || hoverOpacity > .5f ? Theme.ActiveForeground : Theme.InactiveForeground;
            canvas.Save();
            canvas.Translate(visual.Bounds.X, visual.Bounds.Y);
            using (var paint = new SKPaint { IsAntialias = true, Color = ToSkia(background) })
            {
                canvas.DrawPath(geometry.Fill, paint);
                if (!tab.Active && visual.Hover > 0)
                {
                    canvas.Save(); canvas.ClipPath(geometry.Fill, SKClipOperation.Intersect, true);
                    SKColor center = ToSkia(Theme.ActiveTab).WithAlpha((byte)(255 * Theme.RadialOpacity * visual.Hover));
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
                    paint.Color = ToSkia(Theme.Border); canvas.DrawPath(geometry.Border, paint); paint.Style = SKPaintStyle.Fill;
                }
                float separatorY = (geometry.Height - 20 * scale) / 2;
                paint.Color = ToSkia(Theme.Separator).WithAlpha((byte)(leading * 255));
                canvas.DrawRect(geometry.AlignedBounds.Left + 8 * scale, separatorY, scale, 20 * scale, paint);
                paint.Color = ToSkia(Theme.Separator).WithAlpha((byte)(trailing * 255));
                canvas.DrawRect(geometry.AlignedBounds.Right - 9 * scale, separatorY, scale, 20 * scale, paint);
            }

            // Tab::Layout / UpdateIconVisibility. Mouse close target is the 16-DIP
            // glyph box; Chromium's larger touch-only border isn't a mouse target.
            float contentsWidth = geometry.Width / scale - 32;
            bool roomy = contentsWidth >= 68;
            bool close = tab.ShowCloseButton && (tab.Active || roomy) && (!visual.Closing || contentsWidth >= 16);
            if (!visual.Closing) visual.HasIcon = tab.IsLoading || (tab.Content.ShowIcon && tab.Content.Icon != null);
            bool hasIcon = visual.HasIcon;
            bool icon = hasIcon && (!tab.Active || contentsWidth - (close ? 16 : 0) >= 16);
            bool centerIcon = icon && !tab.Active && contentsWidth < 16;
            int contentStart = Scale(roomy ? 20 : 16);
            int iconX = centerIcon ? (geometry.Width - Scale(16)) / 2 : contentStart;
            int centerY = Scale(geometry.Stroke) + (geometry.Height - Scale(1 + geometry.Stroke * 2) - Scale(16)) / 2;
            int closeX = Math.Max(geometry.Width - Scale(32), (geometry.Width - Scale(16)) / 2);
            tab.CloseButtonArea = close ? new Rectangle(closeX, centerY, Scale(16), Scale(16)) : Rectangle.Empty;
            if (close)
            {
                bool hovered = !visual.Closing && !IsTabRepositioning && IsOverCloseButton(tab, cursor);
                visual.CloseFeedback.Update(hovered, now, animate);
                _buttonAnimating |= visual.CloseFeedback.IsAnimating;
                visual.CloseFeedback.Paint(canvas, closeX + 8 * scale, centerY + 8 * scale, 8 * scale, background);
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
                visual.Content.Update(tab.Area.Size, tab.Caption, icon && !tab.IsLoading ? tab.Content.Icon : null,
                    iconBounds, titleBounds, foreground, scale, _font, forceRedraw);
            RectangleF clip = geometry.ContentClip(leading, trailing);
            if (visual.Closing && close)
                clip.Width = Math.Max(0, Math.Min(clip.Right, titleRight) - clip.Left);
            canvas.Save();
            canvas.ClipRect(new SKRect(clip.Left, clip.Top, clip.Right, clip.Bottom));
            canvas.DrawBitmap(visual.Content.Pixels, 0, 0, new SKSamplingOptions(SKFilterMode.Nearest));
            canvas.Restore();
            canvas.Restore();

            // The loading indicator changes every frame; keep it out of the static
            // content cache, while retaining its existing rendering and timing.
            if (!visual.Closing && icon && tab.IsLoading)
            {
                canvas.Flush();
                using (Graphics content = Graphics.FromImage(_buffer))
                {
                    clip.Offset(tab.Area.Location);
                    content.SetClip(clip);
                    iconBounds.Offset(tab.Area.Location);
                    TabLoadingIndicator.Draw(content, iconBounds, LoadingIndicatorColor, tab.LoadingElapsedMilliseconds);
                }
            }
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

        private void PaintAddButton(SKCanvas canvas, List<TitleBarTab> tabs, int startX, int y, Point cursor, double now, bool animate, int closingRight)
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
            // Stay clear of opening/closing tabs, but never cross the fixed limit,
            // including while the window is shrinking or tab widths are animating.
            _addButtonArea.X = Math.Min(maximumX, Math.Max(_addButtonArea.X, visibleRight));
            float cx = _addButtonArea.X + _addButtonArea.Width / 2f;
            float cy = _addButtonArea.Y + _addButtonArea.Height / 2f;
            _addHovered = !IsTabRepositioning && IsOverAddButton(cursor);
            _addFeedback.Update(_addHovered, now, animate);
            _buttonAnimating |= _addFeedback.IsAnimating;
            _addFeedback.Paint(canvas, cx, cy, Scale(14), Theme.Frame);
            using (var paint = new SKPaint { IsAntialias = true, Color = ToSkia(Theme.InactiveForeground) })
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
                _sizingBoxes.Dispose();
                base.Dispose();
            }
        }
    }
}
