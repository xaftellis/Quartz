using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using SkiaSharp;

namespace Quartz.Controls
{
    // Profile tile: a cached avatar and a separate, keyboard-accessible remove
    // button. Caller-owned images are never disposed by this control.
    public class CircularImageButton : ChromiumButton
    {
        private readonly ChromiumButton _action;
        private Image _circularImage, _actionImage, _actionHoverImage;
        private string _buttonText = string.Empty;
        private int _imageSize = 64, _textGap = 5, _actionGap = 5;
        private float _borderSize;
        private Color _borderColor = Color.Black;
        private SKBitmap _avatarPixels;
        private Bitmap _avatar;
        private int _avatarSize, _captionWidth = -1, _captionHeight;

        public CircularImageButton()
        {
            UseToolbarGeometry = false;
            CornerRadius = 4;
            Padding = Padding.Empty;
            _action = new ChromiumButton
            {
                Name = "RemoveProfilePicture", AccessibleName = "Remove profile picture",
                Size = new Size(28, 28), Padding = Padding.Empty, BackColor = Color.Transparent,
                Visible = false, TabStop = true
            };
            _action.Click += (sender, e) => ActionButtonClick?.Invoke(this, e);
            _action.MouseLeave += (sender, e) => UpdateActionVisibility();
            _action.Leave += (sender, e) => UpdateActionVisibility();
            Controls.Add(_action);
        }

        [DefaultValue("")]
        public string ButtonText
        {
            get => _buttonText;
            set
            {
                _buttonText = value ?? string.Empty;
                _captionWidth = -1;
                if (!string.IsNullOrEmpty(_buttonText)) AccessibleName = _buttonText;
                Invalidate();
            }
        }

        public Image CircularImage
        {
            get => _circularImage;
            set
            {
                if (ReferenceEquals(_circularImage, value)) return;
                _circularImage = value;
                ClearAvatar();
                CircularImageChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }

        [DefaultValue(64)]
        public int CircularImageSize
        {
            get => _imageSize;
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(value));
                _imageSize = value;
                ClearAvatar();
                Invalidate();
            }
        }

        [DefaultValue(5)]
        public int CircularImageToTextGapping
        {
            get => _textGap;
            set { _textGap = Math.Max(0, value); Invalidate(); }
        }

        [DefaultValue(0f)]
        public float CircularImageBorderSize
        {
            get => _borderSize;
            set { _borderSize = Math.Max(0, value); Invalidate(); }
        }

        public Color CircularImageBorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }

        public Image ActionButtonImage
        {
            get => _actionImage;
            set
            {
                _actionImage = value;
                _action.Image = value;
                UpdateActionVisibility();
            }
        }

        // Compatibility for existing theme callers. The shared ink drop now
        // supplies the hover feedback instead of a pre-painted hover bitmap.
        public Image ActionButtonHoverImage { get => _actionHoverImage; set => _actionHoverImage = value; }

        [DefaultValue(5)]
        public int ActionButtonGapping
        {
            get => _actionGap;
            set { _actionGap = Math.Max(0, value); LayoutAction(); }
        }

        public event EventHandler ActionButtonClick;
        public event EventHandler CircularImageChanged;

        protected override void PaintButtonContent(Graphics graphics)
        {
            int width = Math.Max(1, Width - (int)Math.Round(8 * DpiScale));
            if (_captionWidth != width)
            {
                _captionWidth = width;
                _captionHeight = string.IsNullOrEmpty(ButtonText) ? 0 : TextRenderer.MeasureText(ButtonText,
                    Font, new Size(width, int.MaxValue), CaptionFlags).Height;
            }
            int gap = _captionHeight == 0 ? 0 : (int)Math.Round(_textGap * DpiScale);
            int size = Math.Min((int)Math.Round(_imageSize * DpiScale), Math.Min(width, Math.Max(1, Height - _captionHeight - gap)));
            int top = (Height - size - gap - _captionHeight) / 2;
            Rectangle bounds = new Rectangle((Width - size) / 2, top, size, size);
            if (_circularImage != null)
            {
                EnsureAvatar(size);
                DrawImage(graphics, _avatar, bounds, Enabled ? 1 : 110 / 255f);
                if (_borderSize > 0)
                {
                    var saved = graphics.Save();
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    Color border = Enabled ? _borderColor : Blend(_borderColor, SurfaceColor, 110 / 255f);
                    using (var pen = new Pen(border, _borderSize * DpiScale)) graphics.DrawEllipse(pen, bounds);
                    graphics.Restore(saved);
                }
            }
            if (_captionHeight > 0)
            {
                int y = _circularImage == null ? (Height - _captionHeight) / 2 : bounds.Bottom + gap;
                DrawButtonText(graphics, ButtonText,
                    new Rectangle((Width - width) / 2, y, width, _captionHeight), CaptionFlags);
            }
        }

        private TextFormatFlags CaptionFlags => TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak |
            TextFormatFlags.TextBoxControl | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis |
            (RightToLeft == RightToLeft.Yes ? TextFormatFlags.RightToLeft : 0);

        private void EnsureAvatar(int size)
        {
            if (_avatar != null && _avatarSize == size) return;
            ClearAvatar();
            _avatarSize = size;
            var info = new SKImageInfo(size, size, SKColorType.Bgra8888, SKAlphaType.Premul);
            using (var source = new SKBitmap(info))
            using (var bitmap = new Bitmap(size, size, source.RowBytes, PixelFormat.Format32bppPArgb, source.GetPixels()))
            {
                // Decode/resample only on image, size or DPI changes.
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.Clear(Color.Transparent);
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    float side = Math.Min(_circularImage.Width, _circularImage.Height);
                    graphics.DrawImage(_circularImage, new Rectangle(0, 0, size, size),
                        (_circularImage.Width - side) / 2, (_circularImage.Height - side) / 2, side, side, GraphicsUnit.Pixel);
                }
                _avatarPixels = new SKBitmap(info);
                _avatar = new Bitmap(size, size, _avatarPixels.RowBytes, PixelFormat.Format32bppPArgb, _avatarPixels.GetPixels());
                using (var canvas = new SKCanvas(_avatarPixels))
                using (var clip = new SKRoundRect(new SKRect(0, 0, size, size), size / 2f, size / 2f))
                {
                    canvas.Clear(SKColors.Transparent);
                    canvas.ClipRoundRect(clip, SKClipOperation.Intersect, true);
                    canvas.DrawBitmap(source, 0, 0, new SKSamplingOptions(SKFilterMode.Nearest));
                    canvas.Flush();
                }
            }
        }

        private void LayoutAction()
        {
            if (_action == null) return;
            int gap = (int)Math.Round(_actionGap * DpiScale);
            int size = (int)Math.Round(28 * DpiScale);
            _action.Bounds = new Rectangle(RightToLeft == RightToLeft.Yes ? gap : Width - size - gap, gap, size, size);
        }

        private void UpdateActionVisibility()
        {
            if (_action == null || IsDisposed || Disposing) return;
            bool pointerInside = IsHandleCreated && ClientRectangle.Contains(PointToClient(MousePosition));
            _action.Visible = _actionImage != null && (pointerInside || ContainsFocus);
            LayoutAction();
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); UpdateActionVisibility(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); UpdateActionVisibility(); }
        protected override void OnEnter(EventArgs e) { base.OnEnter(e); UpdateActionVisibility(); }
        protected override void OnLeave(EventArgs e) { base.OnLeave(e); UpdateActionVisibility(); }
        protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); _captionWidth = -1; LayoutAction(); }
        protected override void OnFontChanged(EventArgs e) { base.OnFontChanged(e); _captionWidth = -1; }
        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            ClearAvatar();
            _captionWidth = -1;
            LayoutAction();
        }

        private void ClearAvatar()
        {
            _avatar?.Dispose(); _avatar = null;
            _avatarPixels?.Dispose(); _avatarPixels = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) ClearAvatar();
            base.Dispose(disposing);
        }
    }
}
