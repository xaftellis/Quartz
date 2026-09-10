using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Quartz.Controls
{
    internal partial class FavouriteButton
    {
        private const double ContentDuration = 180;
        private Image _ownedIcon;
        private Font _iconFont, _noIconFont;
        private Button _previousFace;
        private Button _currentFace;
        private Bitmap _previousFrame;
        private Bitmap _entranceFrame;
        private Bitmap _oldTextLayer, _newTextLayer;
        private Bitmap _iconLayer;
        private Rectangle _iconBounds;
        private Point _oldTextOrigin, _newTextOrigin;
        private bool _captureWithoutText;
        private double _contentElapsed = ContentDuration;

        internal string ThemeKey { get; set; }
        internal bool IsContentAnimating => _previousFace != null || _previousFrame != null;
        internal float ContentProgress => Ease(_contentElapsed / ContentDuration);
        internal bool IsEntering => _entranceFrame != null;

        internal void SetIconVisibility(bool visible, Func<Image> loadIcon)
        {
            if (_noIconFont == null) _noIconFont = Font;
            if (visible)
            {
                Image icon = loadIcon();
                if (SameImage(_ownedIcon, icon)) icon?.Dispose();
                else
                {
                    Image old = _ownedIcon;
                    _ownedIcon = icon;
                    Image = null;
                    old?.Dispose();
                }
                if (_iconFont == null) _iconFont = new Font("Segoe UI", 8);
                Image = _ownedIcon;
                Font = _iconFont;
            }
            else
            {
                Image = null;
                Font = _noIconFont;
            }
        }

        // The old face uses normal Button painting at the current animated width,
        // keeping text crisp instead of stretching a screenshot during resizing.
        internal void ChangeContent(Action update, bool animate)
        {
            string text = Text;
            Font font = Font;
            Image icon = Image;
            Button face = null;
            Bitmap frame = null;
            Bitmap displayedText = null;
            Bitmap displayedIcon = IsContentAnimating ? (Bitmap)_iconLayer?.Clone() : null;
            Rectangle displayedIconBounds = _iconBounds;
            Point displayedTextOrigin = Point.Empty;
            if (animate && Width > 0 && Height > 0)
            {
                if (IsContentAnimating)
                {
                    displayedText = CaptureDisplayedText(out displayedTextOrigin);
                    _captureWithoutText = true;
                    try { frame = CaptureVisual(); }
                    finally { _captureWithoutText = false; }
                }
                else face = CopyFace();
            }
            try
            {
                update();
                if (text == Text && font == Font && icon == Image)
                {
                    if (!animate)
                    {
                        ClearPreviousFace();
                        _contentElapsed = ContentDuration;
                    }
                    return;
                }
                ClearPreviousFace();
                EndEntrance();
                _previousFace = face;
                _previousFrame = frame;
                face = null;
                frame = null;
                if (_previousFace != null || _previousFrame != null)
                {
                    if (_previousFace != null)
                    {
                        _oldTextLayer = CaptureText(_previousFace, out _oldTextOrigin);
                        CaptureIcon(_previousFace);
                        _previousFace.Text = string.Empty;
                        RemoveFaceIcon(_previousFace);
                    }
                    else
                    {
                        _oldTextLayer = displayedText;
                        _oldTextOrigin = displayedTextOrigin;
                        displayedText = null;
                        _iconLayer = displayedIcon;
                        displayedIcon = null;
                        _iconBounds = displayedIconBounds;
                    }
                    _currentFace = CopyFace();
                    Button targetFace = CopyFace();
                    try
                    {
                        targetFace.Size = AutoSize ? GetPreferredSize(Size.Empty) : Size;
                        _newTextLayer = CaptureText(targetFace, out _newTextOrigin);
                    }
                    finally { DisposeFace(targetFace); }
                    if (_currentFace.Image != null) CaptureIcon(_currentFace);
                    _currentFace.Text = string.Empty;
                    RemoveFaceIcon(_currentFace);
                }
                _contentElapsed = animate ? 0 : ContentDuration;
                Invalidate();
            }
            finally
            {
                DisposeFace(face);
                frame?.Dispose();
                displayedText?.Dispose();
                displayedIcon?.Dispose();
            }
        }

        private Button CopyFace()
        {
            var face = new Button
            {
                Text = Text, Font = (Font)Font.Clone(), Image = (Image)Image?.Clone(),
                BackColor = BackColor, ForeColor = ForeColor, FlatStyle = FlatStyle,
                Padding = Padding, TextAlign = TextAlign, ImageAlign = ImageAlign,
                TextImageRelation = TextImageRelation, UseMnemonic = UseMnemonic,
                UseCompatibleTextRendering = UseCompatibleTextRendering,
                UseVisualStyleBackColor = UseVisualStyleBackColor, Enabled = Enabled,
                Size = Size, AutoSize = false
            };
            face.FlatAppearance.BorderColor = FlatAppearance.BorderColor;
            face.FlatAppearance.BorderSize = FlatAppearance.BorderSize;
            face.FlatAppearance.MouseDownBackColor = FlatAppearance.MouseDownBackColor;
            face.FlatAppearance.MouseOverBackColor = FlatAppearance.MouseOverBackColor;
            return face;
        }

        internal Bitmap CaptureVisual()
        {
            var bitmap = new Bitmap(Math.Max(1, Width), Math.Max(1, Height));
            DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
            return bitmap;
        }

        internal void BeginEntrance(bool animate)
        {
            EndEntrance();
            if (!animate) return;
            Button face = CopyFace();
            try
            {
                face.Size = AutoSize ? GetPreferredSize(Size.Empty) : Size;
                _entranceFrame = new Bitmap(Math.Max(1, face.Width), Math.Max(1, face.Height));
                face.DrawToBitmap(_entranceFrame, new Rectangle(Point.Empty, _entranceFrame.Size));
            }
            finally { DisposeFace(face); }
            Invalidate();
        }

        internal void EndEntrance()
        {
            if (_entranceFrame == null) return;
            _entranceFrame.Dispose();
            _entranceFrame = null;
            Invalidate();
        }

        // Reveal/crop at native resolution. Only the right edge follows the
        // changing width; letters and icons are never scaled or faded.
        internal static void PaintClippedFace(Graphics graphics, Bitmap face, Rectangle bounds)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0) return;
            var state = graphics.Save();
            graphics.SetClip(bounds, System.Drawing.Drawing2D.CombineMode.Intersect);
            graphics.DrawImageUnscaled(face, bounds.Location);
            int edge = Math.Min(2, Math.Min(bounds.Width, face.Width));
            graphics.DrawImage(face, new Rectangle(bounds.Right - edge, bounds.Top, edge, bounds.Height),
                new Rectangle(face.Width - edge, 0, edge, face.Height), GraphicsUnit.Pixel);
            graphics.Restore(state);
        }

        internal bool AdvanceContentAnimation(double elapsed)
        {
            bool changing = IsContentAnimating;
            _contentElapsed = Math.Min(ContentDuration, _contentElapsed + Math.Max(0, elapsed));
            if (_contentElapsed >= ContentDuration) ClearPreviousFace();
            if (changing) Invalidate();
            return IsContentAnimating;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (IsEntering && !IsContentAnimating)
            {
                PaintClippedFace(e.Graphics, _entranceFrame, ClientRectangle);
                return;
            }
            if (!IsContentAnimating || Width <= 0 || Height <= 0)
            {
                base.OnPaint(e);
                return;
            }
            using (var current = new Bitmap(Width, Height))
            {
                using (Graphics graphics = Graphics.FromImage(current))
                {
                    // Never paint this live button underneath the moving label.
                    // Button's native adapter can retain its caption layout, so
                    // temporarily overriding Text during OnPaint is insufficient.
                    // These independent faces have genuinely empty captions.
                    _currentFace.Size = Size;
                    _currentFace.DrawToBitmap(current, ClientRectangle);
                    if (_previousFace != null)
                    {
                        _previousFace.Size = Size;
                        using (var previous = new Bitmap(Width, Height))
                        {
                            _previousFace.DrawToBitmap(previous, ClientRectangle);
                            DrawFaded(graphics, previous, ClientRectangle, 1 - ContentProgress);
                        }
                    }
                    else if (_previousFrame != null)
                        DrawFaded(graphics, _previousFrame, ClientRectangle, 1 - ContentProgress);
                    if (!_captureWithoutText)
                    {
                        DrawMovingIcon(graphics);
                        DrawMovingText(graphics);
                    }
                }
                e.Graphics.DrawImageUnscaled(current, Point.Empty);
            }
        }

        private Point MovingTextOrigin
        {
            get
            {
                // Keep the label's horizontal movement in step with the width;
                // otherwise a shrinking button can clip the still-moving title.
                float progress = ContentProgress;
                if (_oldTextLayer.Width != _newTextLayer.Width)
                    progress = Math.Max(0, Math.Min(1, (Width - _oldTextLayer.Width) /
                        (float)(_newTextLayer.Width - _oldTextLayer.Width)));
                return new Point(
                    (int)Math.Round(_oldTextOrigin.X + (_newTextOrigin.X - _oldTextOrigin.X) * progress),
                    (int)Math.Round(_oldTextOrigin.Y + (_newTextOrigin.Y - _oldTextOrigin.Y) * ContentProgress));
            }
        }

        private void DrawMovingText(Graphics graphics)
        {
            // Exactly one native glyph layer per frame. Crossfading two labels
            // produces doubled outlines even when their starting points align.
            bool firstFrame = _contentElapsed == 0;
            Bitmap text = firstFrame ? _oldTextLayer : _newTextLayer;
            Point source = firstFrame ? _oldTextOrigin : _newTextOrigin;
            Point origin = MovingTextOrigin;
            graphics.DrawImageUnscaled(text, origin.X - source.X, origin.Y - source.Y);
        }

        private void DrawMovingIcon(Graphics graphics)
        {
            if (_iconLayer == null || _iconBounds.Width == 0) return;
            // Reserve a gap before the one moving label. Shrink both dimensions
            // together so the outgoing icon cannot overlap the incoming text.
            int width = Math.Min(_iconBounds.Width, Math.Max(0, MovingTextOrigin.X - _iconBounds.Left - 3));
            if (width == 0) return;
            float visibility = width / (float)_iconBounds.Width;
            int height = Math.Max(1, (int)Math.Round(_iconBounds.Height * visibility));
            var bounds = new Rectangle(_iconBounds.Left,
                _iconBounds.Top + (_iconBounds.Height - height) / 2, width, height);
            using (var attributes = new ImageAttributes())
            {
                attributes.SetColorMatrix(new ColorMatrix { Matrix33 = visibility });
                graphics.DrawImage(_iconLayer, bounds, _iconBounds.X, _iconBounds.Y,
                    _iconBounds.Width, _iconBounds.Height, GraphicsUnit.Pixel, attributes);
            }
        }

        private void CaptureIcon(Button face)
        {
            if (face.Image == null) return;
            _iconLayer?.Dispose();
            Point origin;
            _iconLayer = CaptureForeground(face, false, out origin);
            _iconBounds = new Rectangle(origin, face.Image.Size);
        }

        private static void RemoveFaceIcon(Button face)
        {
            Image icon = face.Image;
            face.Image = null;
            icon?.Dispose();
        }

        private Bitmap CaptureDisplayedText(out Point origin)
        {
            var text = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(text)) DrawMovingText(graphics);
            origin = MovingTextOrigin;
            return text;
        }

        private static Bitmap CaptureText(Button face, out Point origin)
            => CaptureForeground(face, true, out origin);

        private static Bitmap CaptureForeground(Button face, bool label, out Point origin)
        {
            // Isolate native-rendered glyphs from an otherwise identical blank
            // face. The label can then move independently of the fading icon,
            // with its glyphs retained at their native resolution.
            Image icon = face.Image;
            string text = face.Text;
            if (label) face.Image = null;
            else face.Text = string.Empty;
            var bounds = new Rectangle(Point.Empty, face.Size);
            var glyphs = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
            using (var blank = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb))
            {
                face.DrawToBitmap(glyphs, bounds);
                face.Text = string.Empty;
                face.Image = null;
                face.DrawToBitmap(blank, bounds);
                face.Text = text;
                face.Image = icon;
                BitmapData glyphData = glyphs.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                BitmapData blankData = blank.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                int left = bounds.Width, top = bounds.Height;
                try
                {
                    int length = glyphData.Stride * bounds.Height;
                    var pixels = new byte[length];
                    var background = new byte[length];
                    Marshal.Copy(glyphData.Scan0, pixels, 0, length);
                    Marshal.Copy(blankData.Scan0, background, 0, length);
                    for (int y = 0; y < bounds.Height; y++)
                        for (int x = 0; x < bounds.Width; x++)
                        {
                            int offset = y * glyphData.Stride + x * 4;
                            if (pixels[offset] == background[offset] && pixels[offset + 1] == background[offset + 1] &&
                                pixels[offset + 2] == background[offset + 2]) pixels[offset + 3] = 0;
                            else
                            {
                                left = Math.Min(left, x);
                                top = Math.Min(top, y);
                            }
                        }
                    Marshal.Copy(pixels, 0, glyphData.Scan0, length);
                }
                finally
                {
                    glyphs.UnlockBits(glyphData);
                    blank.UnlockBits(blankData);
                }
                origin = left == bounds.Width ? Point.Empty : new Point(left, top);
            }
            return glyphs;
        }

        internal static void DrawFaded(Graphics graphics, Image image, Rectangle bounds, float opacity)
        {
            if (opacity <= 0 || bounds.Width <= 0 || bounds.Height <= 0) return;
            using (var attributes = new ImageAttributes())
            {
                attributes.SetColorMatrix(new ColorMatrix { Matrix33 = Math.Min(1, opacity) });
                graphics.DrawImage(image, bounds, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }
        }

        private static float Ease(double progress)
        {
            float value = (float)Math.Max(0, Math.Min(1, progress));
            return 1 - (1 - value) * (1 - value);
        }

        private static bool SameImage(Image a, Image b)
        {
            if (a == null || b == null) return a == b;
            if (a.Size != b.Size) return false;
            using (var left = new Bitmap(a))
            using (var right = new Bitmap(b))
                for (int y = 0; y < left.Height; y++)
                    for (int x = 0; x < left.Width; x++)
                        if (left.GetPixel(x, y) != right.GetPixel(x, y)) return false;
            return true;
        }

        private static void DisposeFace(Button face)
        {
            if (face == null) return;
            Image image = face.Image;
            Font font = face.Font;
            face.Dispose();
            image?.Dispose();
            font.Dispose();
        }

        private void ClearPreviousFace()
        {
            DisposeFace(_previousFace);
            _previousFace = null;
            DisposeFace(_currentFace);
            _currentFace = null;
            _previousFrame?.Dispose();
            _previousFrame = null;
            _oldTextLayer?.Dispose();
            _oldTextLayer = null;
            _newTextLayer?.Dispose();
            _newTextLayer = null;
            _iconLayer?.Dispose();
            _iconLayer = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClearPreviousFace();
                EndEntrance();
                Image = null;
                _ownedIcon?.Dispose();
                _ownedIcon = null;
                _iconFont?.Dispose();
                _iconFont = null;
            }
            base.Dispose(disposing);
        }
    }
}
