using Svg;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Win32Interop.Enums;

namespace EasyTabs
{
    public class WindowsSizingBoxes : IDisposable
    {
        protected TitleBarTabs _parentWindow;
        protected Image _minimizeImage = null;
        protected Image _restoreImage = null;
        protected Image _maximizeImage = null;
        protected Image _closeImage = null;
        protected SolidBrush _minimizeMaximizeButtonHighlight = new SolidBrush(Color.FromArgb(27, Color.Black));
        protected SolidBrush _closeButtonHighlight = new SolidBrush(Color.FromArgb(232, 17, 35));
        protected Rectangle _minimizeButtonArea = new Rectangle(0, 0, 45, 29);
        protected Rectangle _maximizeRestoreButtonArea = new Rectangle(45, 0, 45, 29);
        protected Rectangle _closeButtonArea = new Rectangle(90, 0, 45, 29);

        private readonly HoverFade _minimizeHover = new HoverFade();
        private readonly HoverFade _maximizeHover = new HoverFade();
        private readonly HoverFade _closeHover = new HoverFade();
        private readonly ImageAttributes _glyphAttributes = new ImageAttributes();
        private readonly ColorMatrix _glyphTint = new ColorMatrix { Matrix00 = 0, Matrix11 = 0, Matrix22 = 0 };
        private readonly FontFamily _captionFontFamily;
        private int _glyphSize;
        private HT _pressedButton = HT.HTNOWHERE;

        internal bool IsAnimating => _minimizeHover.IsAnimating || _maximizeHover.IsAnimating || _closeHover.IsAnimating;

        internal bool PointerDown(Point cursor)
        {
            _pressedButton = NonClientHitTest(cursor);
            return _pressedButton != HT.HTNOWHERE;
        }

        internal bool CancelPress()
        {
            bool pressed = _pressedButton != HT.HTNOWHERE;
            _pressedButton = HT.HTNOWHERE;
            return pressed;
        }

        private sealed class HoverFade
        {
            // Windows10CaptionButton inherits Button's 150 ms EASE_OUT slide
            // in both directions; pressed -> hovered snaps to the hovered state.
            private readonly ChromiumHoverAnimation _animation = new ChromiumHoverAnimation(150, false);
            private bool _pressed;
            internal float Value => (float)_animation.Value;
            internal bool IsAnimating => _animation.IsAnimating;

            internal void Update(bool hovered, bool pressed, double now, bool animate)
            {
                if (pressed) _animation.Reset(0);
                else if (_pressed) _animation.Reset(hovered ? 1 : 0);
                else _animation.Update(hovered, now, animate);
                _pressed = pressed;
            }
        }

        public float Scale { get; set; } = 1;
        private int Pixel(float value) => (int)Math.Round(value * Scale);

        public WindowsSizingBoxes(TitleBarTabs parentWindow)
        {
            _parentWindow = parentWindow;
            try { _captionFontFamily = new FontFamily("Segoe Fluent Icons"); }
            catch (ArgumentException) { } // Older Windows versions keep the SVG fallback.
            EnsureGlyphs();
        }

        private void EnsureGlyphs()
        {
            int size = Math.Max(1, Pixel(10));
            if (_glyphSize == size) return;
            _minimizeImage?.Dispose(); _restoreImage?.Dispose();
            _maximizeImage?.Dispose(); _closeImage?.Dispose();
            // Windows 11's own caption glyphs, rasterized at the current display scale.
            // https://learn.microsoft.com/windows/apps/design/iconography/segoe-fluent-icons-font
            _minimizeImage = LoadCaptionGlyph("\uE921", Resources.Minimize, size);
            _maximizeImage = LoadCaptionGlyph("\uE922", Resources.Maximize, size);
            _restoreImage = LoadCaptionGlyph("\uE923", Resources.Restore, size);
            _closeImage = LoadCaptionGlyph("\uE8BB", Resources.Close, size);
            _glyphSize = size;
        }

        private Image LoadCaptionGlyph(string glyph, byte[] fallback, int size)
        {
            if (_captionFontFamily == null)
                return LoadSvg(Encoding.UTF8.GetString(fallback), size, size);
            // Padding retains the antialiased edge pixels. DrawString applies the
            // font's pixel hinting; converting it to a path loses that hinting.
            const int padding = 2;
            var bitmap = new Bitmap(size + padding * 2, size + padding * 2, PixelFormat.Format32bppPArgb);
            using (var graphics = Graphics.FromImage(bitmap))
            using (var font = new Font(_captionFontFamily, size, FontStyle.Regular, GraphicsUnit.Pixel))
            using (var format = StringFormat.GenericTypographic)
            {
                graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                graphics.DrawString(glyph, font, Brushes.Black, new PointF(padding, padding), format);
            }
            return bitmap;
        }

        protected Image LoadSvg(string svgXml, int width, int height)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(svgXml);

            return SvgDocument.Open(xmlDocument).Draw(width, height);
        }

        public int Width
        {
            get
            {
                return Pixel(45) * 3;
            }
        }

        public bool Contains(Point cursor)
        {
            return _minimizeButtonArea.Contains(cursor) || _maximizeRestoreButtonArea.Contains(cursor) || _closeButtonArea.Contains(cursor);
        }

        public void Render(Graphics graphicsContext, Point cursor)
        {
            // Keep the original renderer's immediate hover behaviour.
            Render(graphicsContext, cursor, Color.FromArgb(222, 225, 230), 0, false,
                _parentWindow._overlay?.IsWindowActive ?? (Form.ActiveForm == _parentWindow));
        }

        internal void Render(Graphics graphicsContext, Point cursor, Color frame, double now, bool animate, bool windowActive)
        {
            EnsureGlyphs();
            int right = _parentWindow.ClientRectangle.Width;
            
            int buttonWidth = Pixel(45);
            _minimizeButtonArea = new Rectangle(right - buttonWidth * 3, 0, buttonWidth, Pixel(29));
            _maximizeRestoreButtonArea = new Rectangle(right - buttonWidth * 2, 0, buttonWidth, Pixel(29));
            _closeButtonArea = new Rectangle(right - buttonWidth, 0, buttonWidth, Pixel(29));

            // The overlay owns capture and explicitly ends the press on release/cancel.
            // Do not sample global button state: it may already describe a later event.
            HT hoveredButton = NonClientHitTest(cursor);
            bool minimizePressed = _pressedButton == HT.HTMINBUTTON && hoveredButton == _pressedButton;
            bool maximizePressed = _pressedButton == HT.HTMAXBUTTON && hoveredButton == _pressedButton;
            bool closePressed = _pressedButton == HT.HTCLOSE && hoveredButton == _pressedButton;
            _minimizeHover.Update(_minimizeButtonArea.Contains(cursor), minimizePressed, now, animate);
            _maximizeHover.Update(_maximizeRestoreButtonArea.Contains(cursor), maximizePressed, now, animate);
            _closeHover.Update(_closeButtonArea.Contains(cursor), closePressed, now, animate);

            // Caption buttons sit on the frame, which can differ from the active tab.
            Color foreground = CaptionForeground(frame);
            PaintHover(graphicsContext, _minimizeButtonArea, _minimizeMaximizeButtonHighlight,
                foreground, minimizePressed ? 48 : 27, minimizePressed ? 1 : _minimizeHover.Value);
            PaintHover(graphicsContext, _maximizeRestoreButtonArea, _minimizeMaximizeButtonHighlight,
                foreground, maximizePressed ? 48 : 27, maximizePressed ? 1 : _maximizeHover.Value);
            PaintHover(graphicsContext, _closeButtonArea, _closeButtonHighlight,
                Color.FromArgb(232, 17, 35), closePressed ? 153 : 255, closePressed ? 1 : _closeHover.Value);

            Color closeForeground = !windowActive && hoveredButton != HT.HTCLOSE && !closePressed
                ? foreground : ChromiumTabTheme.Blend(foreground, Color.White, closePressed ? 1 : _closeHover.Value);
            DrawGlyph(graphicsContext, _closeImage, _closeButtonArea,
                CaptionSymbolColor(closeForeground, windowActive, hoveredButton == HT.HTCLOSE, closePressed));
            DrawGlyph(graphicsContext, _parentWindow.WindowState == FormWindowState.Maximized ? _restoreImage : _maximizeImage,
                _maximizeRestoreButtonArea, CaptionSymbolColor(foreground, windowActive, hoveredButton == HT.HTMAXBUTTON, maximizePressed));
            DrawGlyph(graphicsContext, _minimizeImage, _minimizeButtonArea,
                CaptionSymbolColor(foreground, windowActive, hoveredButton == HT.HTMINBUTTON, minimizePressed));
        }

        // Windows10CaptionButton::PaintSymbol / kInactiveTitlebarFeatureAlpha.
        // Hovered and pressed symbols remain opaque even on an inactive window.
        internal static Color CaptionSymbolColor(Color color, bool windowActive, bool hovered, bool pressed) =>
            Color.FromArgb(windowActive || hovered || pressed ? 255 : 0x66, color);

        // GlassBrowserFrameView::GetReadableFeatureColor follows Windows' luma
        // threshold, which differs from the contrast endpoint used for tab text.
        internal static Color CaptionForeground(Color frame) =>
            .25f * frame.R + .625f * frame.G + .125f * frame.B <= 128f ? Color.White : Color.Black;

        private static void PaintHover(Graphics graphics, Rectangle area, SolidBrush brush, Color color, int alpha, float amount)
        {
            if (amount <= 0) return;
            brush.Color = Color.FromArgb((int)Math.Round(alpha * amount), color);
            graphics.FillRectangle(brush, area);
        }

        private void DrawGlyph(Graphics graphics, Image image, Rectangle button, Color color)
        {
            // Tint the glyph silhouette while preserving its alpha and shape.
            _glyphTint.Matrix40 = color.R / 255f;
            _glyphTint.Matrix41 = color.G / 255f;
            _glyphTint.Matrix42 = color.B / 255f;
            _glyphTint.Matrix33 = color.A / 255f;
            _glyphAttributes.SetColorMatrix(_glyphTint);
            int padding = _captionFontFamily == null ? 0 : 2;
            GraphicsState state = graphics.Save();
            try
            {
                // Copy the hinted pixels one-for-one, without a second smoothing pass.
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                graphics.PixelOffsetMode = PixelOffsetMode.Half;
                graphics.DrawImage(image, new Rectangle(button.X + Pixel(17) - padding, Pixel(9) - padding, image.Width, image.Height),
                    0, 0, image.Width, image.Height, GraphicsUnit.Pixel, _glyphAttributes);
            }
            finally { graphics.Restore(state); }
        }

        public void Dispose()
        {
            _minimizeImage?.Dispose(); _restoreImage?.Dispose(); _maximizeImage?.Dispose();
            _closeImage?.Dispose(); _glyphAttributes.Dispose();
            _captionFontFamily?.Dispose();
            _minimizeMaximizeButtonHighlight?.Dispose(); _closeButtonHighlight?.Dispose();
        }

        public HT NonClientHitTest(Point cursor)
        {
            if (_minimizeButtonArea.Contains(cursor))
            {
                return HT.HTMINBUTTON;
            }

            else if (_maximizeRestoreButtonArea.Contains(cursor))
            {
                return HT.HTMAXBUTTON;
            }

            else if (_closeButtonArea.Contains(cursor))
            {
                return HT.HTCLOSE;
            }

            return HT.HTNOWHERE;
        }
    }
}
