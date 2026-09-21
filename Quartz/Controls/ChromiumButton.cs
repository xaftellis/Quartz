using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SkiaSharp;

namespace Quartz.Controls
{
    /// <summary>A WinForms Button with Chromium 85 toolbar ink-drop feedback.</summary>
    public class ChromiumButton : Button
    {
        // TEMPORARY click-colour test: set this to false to restore all original colours.
        internal static readonly bool TestModernLightClickColor = true;
        internal static bool IsLightThemeForClickColorTest { get; set; }
        private readonly ChromiumButtonAnimation _animation = new ChromiumButtonAnimation();
        private readonly ChromiumButtonImage _imageRenderer = new ChromiumButtonImage();
        private bool _mouseDown, _keyDown, _hovered, _active, _menuActive, _releasingMouse, _keyboardFocus;
        private bool _animationEnabled = true, _systemAnimations = true, _toolbarGeometry = true;
        private int _iconSize = 16;
        private int _imageTextSpacing = 12;
        private ChromiumIcon _vectorIcon;
        private bool _mirrorImageInRtl = true;
        private float _cornerRadius = -1;
        private PointF _origin;
        private Point _releasePoint;
        private bool _releasingKey, _touchMouseMessage;
        private ContextMenuStrip _observedMenu;
        private Color _inkColor = Color.Empty;
        private Color _focusRingColor = Color.Empty;
        private SKBitmap _pixels;
        private SKCanvas _canvas;
        private SKPaint _paint;
        private SKRoundRect _clip;
        private Bitmap _buffer;
        internal bool SuppressSnapshotImage { get; set; }
        internal bool SuppressSnapshotText { get; set; }

        public ChromiumButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false;
            Padding = new Padding(6);
            TextImageRelation = TextImageRelation.ImageBeforeText;
            Size = new Size(28, 28);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);
        }

        // Keep Button's input/dialog/accessibility contracts, but exclude its
        // themed paint adapter completely. ButtonBase.OnPaint only delegates to
        // Control.OnPaint when UserPaint is off, preserving ordinary Paint events.
        protected override void OnPaint(PaintEventArgs e)
        {
            RenderButton(this, e);
            SetStyle(ControlStyles.UserPaint, false);
            try { base.OnPaint(e); }
            finally { SetStyle(ControlStyles.UserPaint, true); }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new FlatStyle FlatStyle
        {
            get => base.FlatStyle;
            set => base.FlatStyle = FlatStyle.Flat;
        }

        [Category("Behavior"), DefaultValue(false)]
        [Description("Hold the activated ink drop until explicitly set to false.")]
        public bool IsActive
        {
            get => _active;
            set
            {
                if (_active == value) return;
                _active = value;
                UpdateActivation();
                if (IsHandleCreated) AccessibilityNotifyClients(AccessibleEvents.StateChange, -1);
            }
        }

        [Browsable(false)]
        public bool IsPressed => Enabled && (_active || _menuActive || _keyDown || (_mouseDown && _hovered));

        [Category("Behavior"), DefaultValue(true)]
        public bool FocusOnPress { get; set; } = true;

        [Category("Behavior"), DefaultValue(true)]
        public bool AnimationEnabled
        {
            get => _animationEnabled;
            set { _animationEnabled = value; ResetFeedback(); }
        }

        [Category("Appearance"), DefaultValue(true)]
        [Description("Inset larger targets to Chromium's 28-DIP toolbar ink height.")]
        public bool UseToolbarGeometry
        {
            get => _toolbarGeometry;
            set { _toolbarGeometry = value; Invalidate(); }
        }

        [Category("Appearance"), DefaultValue(-1f)]
        [Description("Corner radius in DIPs. -1 uses Chromium's maximum-emphasis capsule.")]
        public float CornerRadius
        {
            get => _cornerRadius;
            set
            {
                if (value < -1 || float.IsNaN(value) || float.IsInfinity(value)) throw new ArgumentOutOfRangeException(nameof(value));
                _cornerRadius = value;
                Invalidate();
            }
        }

        [Category("Appearance"), DefaultValue(16)]
        [Description("Icon slot in DIPs. Zero preserves the supplied image's pixel size.")]
        public int IconSize
        {
            get => _iconSize;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                _iconSize = value;
                ClearImageCache();
                Invalidate();
            }
        }

        [Category("Appearance"), DefaultValue(ChromiumIcon.None)]
        [Description("A Chromium 85 vector icon rasterized for the current DPI. None uses Image.")]
        public ChromiumIcon VectorIcon
        {
            get => _vectorIcon;
            set
            {
                if (!Enum.IsDefined(typeof(ChromiumIcon), value)) throw new ArgumentOutOfRangeException(nameof(value));
                _vectorIcon = value;
                ClearImageCache();
                Invalidate();
                if (AutoSize) Parent?.PerformLayout(this, nameof(VectorIcon));
            }
        }

        [Category("Appearance"), DefaultValue(12)]
        public int ImageTextSpacing
        {
            get => _imageTextSpacing;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                _imageTextSpacing = value;
                Invalidate();
                if (AutoSize) Parent?.PerformLayout(this, nameof(ImageTextSpacing));
            }
        }

        [Category("Appearance"), DefaultValue(true)]
        public bool MirrorImageInRtl
        {
            get => _mirrorImageInRtl;
            set { _mirrorImageInRtl = value; Invalidate(); }
        }

        [Category("Appearance"), DefaultValue(typeof(Color), "Empty")]
        [Description("Empty selects white or Google Grey 900 from the background luminance.")]
        public Color InkColor
        {
            get => _inkColor;
            set { _inkColor = value; Invalidate(); }
        }

        [Category("Appearance"), DefaultValue(typeof(Color), "Empty")]
        public Color FocusRingColor
        {
            get => _focusRingColor;
            set { _focusRingColor = value; Invalidate(); }
        }

        protected virtual float DpiScale => DeviceDpi / 96f;
        protected bool IsHovered => _hovered;
        private bool Activated => _active || _menuActive;
        private bool CanAnimate => _animationEnabled && _systemAnimations && !DesignMode;
        protected Color SurfaceColor => OpaqueBackground(this);
        protected Color ContentColor => Enabled ? ForeColor : Blend(ForeColor, SurfaceColor, 110 / 255f);
        protected virtual double HighlightOpacity => .08;
        protected virtual double RippleOpacity => .06;
        protected virtual Color DefaultInkColor => GetContrastInk(SurfaceColor);

        protected virtual RectangleF GetInkBounds()
        {
            float inset = _toolbarGeometry ? Math.Max(0, (Height - 28 * DpiScale) / 2) : 0;
            return new RectangleF(inset, inset, Math.Max(0, Width - inset * 2), Math.Max(0, Height - inset * 2));
        }

        protected virtual bool HitTest(Point point) => ClientRectangle.Contains(point);

        private void RenderButton(object sender, PaintEventArgs e)
        {
            if (Width <= 0 || Height <= 0) return;
            double now = ButtonFrames.Now;
            _animation.Advance(now);
            EnsureBuffer();
            RectangleF bounds = GetInkBounds();
            float radius = _cornerRadius < 0 ? Math.Min(bounds.Width, bounds.Height) / 2
                : Math.Min(_cornerRadius * DpiScale, Math.Min(bounds.Width, bounds.Height) / 2);
            _clip.SetRect(ToSkia(bounds), radius, radius);
            _canvas.Clear(ToSkia(OpaqueBackground(Parent)));
            _paint.Style = SKPaintStyle.Fill;
            _paint.Color = ToSkia(SurfaceColor);
            _canvas.DrawRoundRect(_clip, _paint);
            _canvas.Flush();
            e.Graphics.DrawImageUnscaled(_buffer, 0, 0);

            // Retain Quartz's decorative backgrounds (including seasonal snow).
            if (BackgroundImage != null)
                DrawBackgroundImage(e.Graphics);

            _canvas.Clear(SKColors.Transparent);
            _canvas.Save();
            _canvas.ClipRoundRect(_clip, SKClipOperation.Intersect, true);
            // Chromium 85's platform high-contrast ink-drop feature is disabled
            // by default. Toolbar ink still uses maximum contrast in that mode.
            Color ink = _inkColor.IsEmpty ? DefaultInkColor : _inkColor;
            double highlight = Enabled ? _animation.Highlight(now) : 0;
            double opacity = Enabled ? _animation.Opacity(now) : 0;
            _paint.Color = ToSkia(ink, highlight * HighlightOpacity);
            _canvas.DrawRect(ToSkia(bounds), _paint);
            if (opacity > 0)
            {
                // FloodFillInkDropRipple expands to the farthest HOST corner;
                // the separate highlight mask supplies the rounded paint clip.
                double dx = Math.Max(Math.Abs(_origin.X), Math.Abs(Width - _origin.X));
                double dy = Math.Max(Math.Abs(_origin.Y), Math.Abs(Height - _origin.Y));
                float maximum = (float)Math.Sqrt(dx * dx + dy * dy);
                float rippleRadius = DpiScale + (maximum - DpiScale) * (float)_animation.Radius(now);
                // ORIGINAL (kept for easy rollback):
                // _paint.Color = ToSkia(ink, opacity * RippleOpacity);
                // Chrome's baseline light kColorSysStateRipplePrimary:
                // kColorRefPrimary70 (#7CACF8), alpha 0x52. Used by profile/menu
                // buttons; applied to every shared button here only for this test.
                // References: ui/color/{ref,sys}_color_mixer.cc in modern Chromium.
                bool testClickColor = TestModernLightClickColor && IsLightThemeForClickColorTest;
                _paint.Color = testClickColor
                    ? ToSkia(Color.FromArgb(0x7C, 0xAC, 0xF8), opacity * (0x52 / 255d))
                    : ToSkia(ink, opacity * RippleOpacity);
                _canvas.DrawCircle(_origin.X, _origin.Y, rippleRadius, _paint);
            }
            _canvas.Restore();
            _canvas.Flush();
            e.Graphics.DrawImageUnscaled(_buffer, 0, 0);
            PaintButtonContent(e.Graphics);

            if (Enabled && Focused && (!FocusOnPress || (ShowFocusCues && _keyboardFocus)))
            {
                _canvas.Clear(SKColors.Transparent);
                // A child HWND cannot paint Chromium's outward halo. Keep the
                // same 2-DIP stroke just inside the control's clipping boundary.
                float inset = DpiScale;
                bounds.Inflate(-inset, -inset);
                if (bounds.Width > 0 && bounds.Height > 0)
                {
                    _paint.Style = SKPaintStyle.Stroke;
                    _paint.StrokeWidth = 2 * DpiScale;
                    Color focus = FocusRingColor.IsEmpty
                        ? (SystemInformation.HighContrast ? SystemColors.ControlText
                            : Color.FromArgb(0x4D, GetContrastInk(SurfaceColor) == Color.White
                                ? Color.FromArgb(138, 180, 248) : Color.FromArgb(26, 115, 232)))
                        : FocusRingColor;
                    _paint.Color = ToSkia(focus);
                    _canvas.DrawRoundRect(ToSkia(bounds), Math.Max(0, radius - inset), Math.Max(0, radius - inset), _paint);
                    _canvas.Flush();
                    e.Graphics.DrawImageUnscaled(_buffer, 0, 0);
                }
            }
        }

        protected virtual void PaintButtonContent(Graphics graphics)
        {
            GetContentBounds(out Rectangle imageBounds, out Rectangle textBounds);
            if (!SuppressSnapshotImage && HasImage && imageBounds.Width > 0 && imageBounds.Height > 0)
            {
                EnsureBuffer();
                _canvas.Clear(SKColors.Transparent);
                _canvas.Save();
                if (_mirrorImageInRtl && RightToLeft == RightToLeft.Yes)
                {
                    _canvas.Translate(imageBounds.Left + imageBounds.Right, 0);
                    _canvas.Scale(-1, 1);
                }
                _paint.Style = SKPaintStyle.Fill;
                _paint.Color = new SKColor(255, 255, 255, Enabled ? (byte)255 : (byte)110);
                // Rasterize once at device scale and then copy pixels 1:1. Hover
                // and press change ink, never the glyph or its position.
                SKBitmap pixels = _imageRenderer.Get(Image, VectorIcon, imageBounds.Size, ForeColor);
                _canvas.DrawBitmap(pixels, imageBounds.Left, imageBounds.Top,
                    new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None), _paint);
                _canvas.Restore();
                _canvas.Flush();
                graphics.DrawImageUnscaled(_buffer, 0, 0);
            }
            if (!SuppressSnapshotText && textBounds.Width > 0 && textBounds.Height > 0 && !string.IsNullOrEmpty(Text))
                TextRenderer.DrawText(graphics, Text, Font, textBounds, ContentColor, GetTextFlags());
        }

        // LabelButton::Layout / ImageButton::ComputeImagePaintPosition. Icons
        // and labels have separate bounds, including when the text is elided.
        protected void GetContentBounds(out Rectangle imageBounds, out Rectangle textBounds)
        {
            Rectangle content = new Rectangle(Padding.Left, 0, Math.Max(0, Width - Padding.Horizontal), Height);
            Size imageSize = GetImageSize();
            imageBounds = Align(content, imageSize, MirrorAlignment(ImageAlign));
            textBounds = content;
            TextImageRelation relation = TextImageRelation;
            if (RightToLeft == RightToLeft.Yes)
            {
                if (relation == TextImageRelation.ImageBeforeText) relation = TextImageRelation.TextBeforeImage;
                else if (relation == TextImageRelation.TextBeforeImage) relation = TextImageRelation.ImageBeforeText;
            }
            int gap = HasImage && !string.IsNullOrEmpty(Text) ? (int)Math.Round(_imageTextSpacing * DpiScale) : 0;
            if (HasImage && !string.IsNullOrEmpty(Text) && relation != TextImageRelation.Overlay)
            {
                int labelWidth = Math.Min(MeasureLabel().Width, Math.Max(0, content.Width - imageSize.Width - gap));
                int groupWidth = imageSize.Width + gap + labelWidth;
                int groupX = Align(content, new Size(groupWidth, content.Height), MirrorAlignment(TextAlign)).X;
                if (relation == TextImageRelation.ImageBeforeText)
                {
                    imageBounds.X = groupX;
                    textBounds.X = groupX + imageSize.Width + gap;
                    textBounds.Width = labelWidth;
                }
                else if (relation == TextImageRelation.TextBeforeImage)
                {
                    imageBounds.X = groupX + labelWidth + gap;
                    textBounds.X = groupX;
                    textBounds.Width = labelWidth;
                }
                else if (relation == TextImageRelation.ImageAboveText)
                {
                    imageBounds.Y = content.Top;
                    textBounds.Y += imageSize.Height + gap;
                    textBounds.Height = Math.Max(0, textBounds.Height - imageSize.Height - gap);
                }
                else if (relation == TextImageRelation.TextAboveImage)
                {
                    imageBounds.Y = content.Bottom - imageSize.Height;
                    textBounds.Height = Math.Max(0, textBounds.Height - imageSize.Height - gap);
                }
            }
        }

        protected TextFormatFlags GetTextFlags()
        {
            TextFormatFlags flags = TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.PreserveGraphicsClipping;
            ContentAlignment alignment = MirrorAlignment(TextAlign);
            if ((alignment & (ContentAlignment.TopCenter | ContentAlignment.MiddleCenter | ContentAlignment.BottomCenter)) != 0)
                flags |= TextFormatFlags.HorizontalCenter;
            else if ((alignment & (ContentAlignment.TopRight | ContentAlignment.MiddleRight | ContentAlignment.BottomRight)) != 0)
                flags |= TextFormatFlags.Right;
            if ((alignment & (ContentAlignment.MiddleLeft | ContentAlignment.MiddleCenter | ContentAlignment.MiddleRight)) != 0)
                flags |= TextFormatFlags.VerticalCenter;
            else if ((alignment & (ContentAlignment.BottomLeft | ContentAlignment.BottomCenter | ContentAlignment.BottomRight)) != 0)
                flags |= TextFormatFlags.Bottom;
            if (!UseMnemonic) flags |= TextFormatFlags.NoPrefix;
            else if (!ShowKeyboardCues) flags |= TextFormatFlags.HidePrefix;
            if (RightToLeft == RightToLeft.Yes) flags |= TextFormatFlags.RightToLeft;
            flags |= Text.IndexOf('\n') >= 0 ? TextFormatFlags.WordBreak : TextFormatFlags.SingleLine;
            return flags;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size text = MeasureLabel();
            Size icon = GetImageSize();
            int gap = icon.Width > 0 && text.Width > 0 ? (int)Math.Round(_imageTextSpacing * DpiScale) : 0;
            bool vertical = TextImageRelation == TextImageRelation.ImageAboveText || TextImageRelation == TextImageRelation.TextAboveImage;
            int width = vertical || TextImageRelation == TextImageRelation.Overlay ? Math.Max(text.Width, icon.Width) : text.Width + icon.Width + gap;
            int height = vertical ? text.Height + icon.Height + gap : Math.Max(text.Height, icon.Height);
            width = Math.Max(MinimumSize.Width, width + Padding.Horizontal);
            height = Math.Max(MinimumSize.Height, Math.Max((int)Math.Round(28 * DpiScale), height + Padding.Vertical));
            if (MaximumSize.Width > 0) width = Math.Min(width, MaximumSize.Width);
            if (MaximumSize.Height > 0) height = Math.Min(height, MaximumSize.Height);
            return new Size(width, height);
        }

        private Size MeasureLabel() => string.IsNullOrEmpty(Text) ? Size.Empty : TextRenderer.MeasureText(Text, Font,
            new Size(int.MaxValue, int.MaxValue), GetTextFlags());

        protected override void OnMouseEnter(EventArgs e)
        {
            SetHovered(IsPointerOver());
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            SetHovered(false);
            if (_mouseDown && !Activated) _animation.Cancel(ButtonFrames.Now);
            WakeAnimation();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (FocusOnPress) _keyboardFocus = false;
            if (Enabled && e.Button == MouseButtons.Left && HitTest(e.Location))
            {
                _mouseDown = true;
                SetHovered(!_touchMouseMessage);
                if (!Activated && CanStartRippleFromInput())
                {
                    _origin = e.Location;
                    _animation.Press(ButtonFrames.Now);
                }
                WakeAnimation();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            bool inside = Enabled && HitTest(e.Location);
            SetHovered(inside && !_touchMouseMessage);
            if (_mouseDown && !Activated)
            {
                if (!inside) _animation.Cancel(ButtonFrames.Now);
                else if (CanStartRippleFromInput() && (_animation.State == ChromiumButtonAnimation.InkState.Hiding || _animation.State == ChromiumButtonAnimation.InkState.Hidden))
                {
                    _origin = e.Location;
                    _animation.Press(ButtonFrames.Now);
                }
                WakeAnimation();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            // Button.OnMouseUp raises Click BEFORE the public MouseUp event.
            // Keep our press alive until that call has accepted or rejected it.
            try { base.OnMouseUp(e); }
            finally
            {
                if (e.Button == MouseButtons.Left && !IsDisposed)
                {
                    _mouseDown = false;
                    if (_animation.State == ChromiumButtonAnimation.InkState.Pending)
                        _animation.Cancel(ButtonFrames.Now);
                    WakeAnimation();
                }
            }
        }

        protected override void OnClick(EventArgs e)
        {
            if ((_releasingMouse && (!_mouseDown || !HitTest(_releasePoint))) || (_releasingKey && !_keyDown)) return;
            if (Enabled && !Activated && CanStartRippleFromInput())
            {
                if (!_mouseDown && !_keyDown) _origin = CenterPoint();
                _animation.Trigger(ButtonFrames.Now);
                WakeAnimation();
            }
            _mouseDown = _keyDown = false;
            base.OnClick(e); // preserves validation, DialogResult and all existing handlers
        }

        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            if (!Capture && !_releasingMouse && !_releasingKey)
            {
                _mouseDown = false;
                _animation.Cancel(ButtonFrames.Now);
                SetHovered(IsPointerOver());
                WakeAnimation();
            }
            base.OnMouseCaptureChanged(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            _keyboardFocus = true;
            if (Enabled && e.KeyCode == Keys.Space && e.Modifiers == Keys.None && !_keyDown)
            {
                _keyDown = true;
                if (!Activated)
                {
                    _origin = CenterPoint();
                    _animation.Press(ButtonFrames.Now);
                }
                WakeAnimation();
            }
            if (e.KeyCode == Keys.Escape && (_mouseDown || _keyDown))
            {
                _mouseDown = _keyDown = false;
                _animation.Cancel(ButtonFrames.Now);
                Capture = false;
                WakeAnimation();
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            _releasingKey = e.KeyCode == Keys.Space;
            try { base.OnKeyUp(e); }
            finally { _releasingKey = false; }
            if (e.KeyCode == Keys.Space)
            {
                _keyDown = false;
                if (_animation.State == ChromiumButtonAnimation.InkState.Pending) _animation.Cancel(ButtonFrames.Now);
                WakeAnimation();
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            _keyboardFocus = MouseButtons == MouseButtons.None;
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            bool pendingPress = _keyDown || _mouseDown;
            _keyDown = false;
            if (pendingPress && !Activated) _animation.Cancel(ButtonFrames.Now);
            WakeAnimation();
            base.OnLostFocus(e);
        }

        protected override void WndProc(ref Message m)
        {
            bool release = m.Msg == 0x0202; // capture may be released before Click/MouseUp
            bool suppressFocus = !FocusOnPress && (m.Msg == 0x0201 || m.Msg == 0x0203);
            bool selectable = GetStyle(ControlStyles.Selectable);
            bool previousTouchMessage = _touchMouseMessage;
            // ui/events/win/events_win_utils.cc: Windows-promoted touch mouse
            // messages carry this signature; pen messages do not have bit 0x80.
            if (m.Msg >= 0x0200 && m.Msg <= 0x020E)
                _touchMouseMessage = (GetMessageExtraInfo().ToInt64() & 0xFF515780L) == 0xFF515780L;
            if (suppressFocus) SetStyle(ControlStyles.Selectable, false);
            if (release)
            {
                _releasingMouse = true;
                long coordinates = m.LParam.ToInt64();
                _releasePoint = new Point((short)coordinates, (short)(coordinates >> 16));
            }
            try { base.WndProc(ref m); }
            finally
            {
                if (suppressFocus) SetStyle(ControlStyles.Selectable, selectable);
                if (release) _releasingMouse = false;
                _touchMouseMessage = previousTouchMessage;
            }
            if (m.Msg == 0x001A) ReadAnimationPreference(); // WM_SETTINGCHANGE
        }

        protected override void OnContextMenuStripChanged(EventArgs e)
        {
            ObserveMenu(null);
            base.OnContextMenuStripChanged(e);
            ObserveMenu(ContextMenuStrip);
        }

        private void ObserveMenu(ContextMenuStrip menu)
        {
            if (_observedMenu != null)
            {
                _observedMenu.Opened -= MenuOpened;
                _observedMenu.Closed -= MenuClosed;
                _observedMenu.Disposed -= MenuDisposed;
            }
            _observedMenu = menu;
            _menuActive = false;
            if (menu != null)
            {
                menu.Opened += MenuOpened;
                menu.Closed += MenuClosed;
                menu.Disposed += MenuDisposed;
                _menuActive = menu.Visible && menu.SourceControl == this;
            }
            UpdateActivation();
        }

        private void MenuOpened(object sender, EventArgs e)
        {
            if (_observedMenu?.SourceControl != this) return;
            _menuActive = true;
            UpdateActivation();
        }

        private void MenuClosed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            if (!_menuActive) return;
            _menuActive = false;
            UpdateActivation();
        }

        private void MenuDisposed(object sender, EventArgs e) => ObserveMenu(null);

        private void UpdateActivation()
        {
            if (_animation == null || IsDisposed || Disposing) return;
            double now = ButtonFrames.Now;
            if (!Enabled || !Visible) { ResetFeedback(); return; }
            if (Activated)
            {
                if (_animation.State != ChromiumButtonAnimation.InkState.Pending &&
                    _animation.State != ChromiumButtonAnimation.InkState.Activated) _origin = CenterPoint();
                _animation.Activate(now);
            }
            else
            {
                SetHovered(IsPointerOver());
                _animation.Deactivate(now);
            }
            WakeAnimation();
            if (IsHandleCreated) AccessibilityNotifyClients(AccessibleEvents.StateChange, -1);
        }

        private void SetHovered(bool hovered)
        {
            _hovered = hovered;
            _animation.SetHovered(hovered, ButtonFrames.Now);
            WakeAnimation();
        }

        private bool IsPointerOver() => IsHandleCreated && Enabled && Visible && HitTest(PointToClient(MousePosition));
        private bool CanStartRippleFromInput() => !_touchMouseMessage ||
            (_animation.State != ChromiumButtonAnimation.InkState.Hidden && _animation.State != ChromiumButtonAnimation.InkState.Hiding);
        private PointF CenterPoint() => new PointF(Width / 2f, Height / 2f);

        private void WakeAnimation()
        {
            if (IsDisposed || Disposing) return;
            Invalidate();
            if (IsHandleCreated && Visible && _animation.IsAnimating(ButtonFrames.Now)) ButtonFrames.Add(this);
        }

        internal bool AdvanceFrame(double now)
        {
            if (IsDisposed || Disposing || !IsHandleCreated || !Visible) return false;
            bool animating = _animation.IsAnimating(now);
            Invalidate();
            return animating;
        }

        private void ResetFeedback()
        {
            if (_animation == null) return;
            _mouseDown = _keyDown = false;
            _hovered = IsPointerOver();
            _animation.AnimationsEnabled = CanAnimate;
            _origin = CenterPoint();
            _animation.Reset(Enabled && Visible && Activated, Enabled && Visible && _hovered, ButtonFrames.Now);
            ButtonFrames.Remove(this);
            Invalidate();
        }

        protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); ResetFeedback(); }
        protected override void OnVisibleChanged(EventArgs e) { base.OnVisibleChanged(e); ResetFeedback(); }
        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); ReadAnimationPreference(); }
        protected override void OnHandleDestroyed(EventArgs e) { ButtonFrames.Remove(this); base.OnHandleDestroyed(e); }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (_animation == null) return;
            // Views rebuilds ink layers when host bounds change. Drop transient
            // feedback, but preserve Quartz's explicit active-state contract.
            _origin = CenterPoint();
            _animation.Reset(Enabled && Visible && Activated, Enabled && Visible && _hovered, ButtonFrames.Now);
            ButtonFrames.Remove(this);
        }
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            ResetFeedback();
        }
        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            ClearImageCache();
            ResetFeedback();
        }

        private void ReadAnimationPreference()
        {
            int enabled = 1;
            _systemAnimations = SystemParametersInfo(0x1042, 0, ref enabled, 0)
                ? enabled != 0 : !SystemInformation.TerminalServerSession;
            ResetFeedback();
        }

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(int action, uint parameter, ref int value, uint flags);
        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();

        protected override AccessibleObject CreateAccessibilityInstance() => new ChromiumButtonAccessibleObject(this);

        private sealed class ChromiumButtonAccessibleObject : ButtonBaseAccessibleObject
        {
            private readonly ChromiumButton _owner;
            internal ChromiumButtonAccessibleObject(ChromiumButton owner) : base(owner) { _owner = owner; }
            public override AccessibleRole Role => AccessibleRole.PushButton;
            public override string DefaultAction => "Press";
            public override void DoDefaultAction() => _owner.PerformClick();
            public override AccessibleStates State => base.State | (_owner.IsPressed ? AccessibleStates.Pressed : 0);
        }

        private void EnsureBuffer()
        {
            if (_pixels != null && _pixels.Width == Width && _pixels.Height == Height) return;
            DisposeBuffer();
            _pixels = new SKBitmap(new SKImageInfo(Width, Height, SKColorType.Bgra8888, SKAlphaType.Premul));
            _buffer = new Bitmap(Width, Height, _pixels.RowBytes, PixelFormat.Format32bppPArgb, _pixels.GetPixels());
            _canvas = new SKCanvas(_pixels);
            _paint = new SKPaint { IsAntialias = true };
            _clip = new SKRoundRect();
        }

        private void DisposeBuffer()
        {
            _canvas?.Dispose(); _canvas = null;
            _buffer?.Dispose(); _buffer = null;
            _pixels?.Dispose(); _pixels = null;
            _paint?.Dispose(); _paint = null;
            _clip?.Dispose(); _clip = null;
        }

        private bool HasImage => VectorIcon != ChromiumIcon.None || Image != null;
        private Size GetImageSize()
        {
            if (!HasImage) return Size.Empty;
            int slot = _iconSize == 0 ? 16 : _iconSize;
            Size logical = VectorIcon != ChromiumIcon.None ? new Size(slot, slot) : Image.Size;
            if (VectorIcon == ChromiumIcon.None && _iconSize > 0)
            {
                float fit = _iconSize / (float)Math.Max(logical.Width, logical.Height);
                logical = new Size(Math.Max(1, (int)Math.Round(logical.Width * fit)), Math.Max(1, (int)Math.Round(logical.Height * fit)));
            }
            return new Size(Math.Max(1, (int)Math.Ceiling(logical.Width * DpiScale)), Math.Max(1, (int)Math.Ceiling(logical.Height * DpiScale)));
        }

        private void ClearImageCache() => _imageRenderer?.Dispose();

        protected static void DrawImage(Graphics graphics, Image image, Rectangle bounds, float opacity)
        {
            if (image == null || bounds.Width <= 0 || bounds.Height <= 0) return;
            if (opacity >= 1) { graphics.DrawImage(image, bounds); return; }
            using (var attributes = new ImageAttributes())
            {
                attributes.SetColorMatrix(new ColorMatrix { Matrix33 = opacity });
                graphics.DrawImage(image, bounds, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }
        }

        private void DrawBackgroundImage(Graphics graphics)
        {
            Rectangle bounds = ClientRectangle;
            if (BackgroundImageLayout == ImageLayout.None || BackgroundImageLayout == ImageLayout.Center)
                bounds = Align(bounds, BackgroundImage.Size, BackgroundImageLayout == ImageLayout.Center ? ContentAlignment.MiddleCenter : ContentAlignment.TopLeft);
            else if (BackgroundImageLayout == ImageLayout.Zoom)
            {
                float factor = Math.Min(Width / (float)BackgroundImage.Width, Height / (float)BackgroundImage.Height);
                bounds = Align(bounds, new Size((int)(BackgroundImage.Width * factor), (int)(BackgroundImage.Height * factor)), ContentAlignment.MiddleCenter);
            }
            if (BackgroundImageLayout == ImageLayout.Tile)
            {
                using (var brush = new TextureBrush(BackgroundImage)) graphics.FillRectangle(brush, ClientRectangle);
            }
            else graphics.DrawImage(BackgroundImage, bounds);
        }

        private ContentAlignment MirrorAlignment(ContentAlignment alignment)
        {
            if (RightToLeft != RightToLeft.Yes) return alignment;
            switch (alignment)
            {
                case ContentAlignment.TopLeft: return ContentAlignment.TopRight;
                case ContentAlignment.TopRight: return ContentAlignment.TopLeft;
                case ContentAlignment.MiddleLeft: return ContentAlignment.MiddleRight;
                case ContentAlignment.MiddleRight: return ContentAlignment.MiddleLeft;
                case ContentAlignment.BottomLeft: return ContentAlignment.BottomRight;
                case ContentAlignment.BottomRight: return ContentAlignment.BottomLeft;
                default: return alignment;
            }
        }

        private static Rectangle Align(Rectangle area, Size size, ContentAlignment alignment)
        {
            int x = area.Left, y = area.Top;
            if ((alignment & (ContentAlignment.TopCenter | ContentAlignment.MiddleCenter | ContentAlignment.BottomCenter)) != 0) x += (area.Width - size.Width) / 2;
            else if ((alignment & (ContentAlignment.TopRight | ContentAlignment.MiddleRight | ContentAlignment.BottomRight)) != 0) x = area.Right - size.Width;
            if ((alignment & (ContentAlignment.MiddleLeft | ContentAlignment.MiddleCenter | ContentAlignment.MiddleRight)) != 0) y += (area.Height - size.Height) / 2;
            else if ((alignment & (ContentAlignment.BottomLeft | ContentAlignment.BottomCenter | ContentAlignment.BottomRight)) != 0) y = area.Bottom - size.Height;
            return new Rectangle(x, y, size.Width, size.Height);
        }

        private static Color OpaqueBackground(Control control)
        {
            for (Control current = control; current != null; current = current.Parent)
                if (current.BackColor.A == 255) return current.BackColor;
            return SystemColors.Control;
        }

        internal static Color GetContrastInk(Color background)
        {
            double luminance = .2126 * Linear(background.R) + .7152 * Linear(background.G) + .0722 * Linear(background.B);
            return luminance < .211692036 ? Color.White : Color.FromArgb(32, 33, 36);
        }

        private static double Linear(byte component)
        {
            double value = component / 255d;
            return value <= .04045 ? value / 12.92 : Math.Pow((value + .055) / 1.055, 2.4);
        }

        protected static Color Blend(Color foreground, Color background, float alpha) => Color.FromArgb(
            (int)Math.Round(background.R + (foreground.R - background.R) * alpha),
            (int)Math.Round(background.G + (foreground.G - background.G) * alpha),
            (int)Math.Round(background.B + (foreground.B - background.B) * alpha));

        private static SKColor ToSkia(Color color, double opacity = 1) => new SKColor(color.R, color.G, color.B, (byte)Math.Max(0, Math.Min(255, Math.Round(color.A * opacity))));
        private static SKRect ToSkia(RectangleF rectangle) => new SKRect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ButtonFrames.Remove(this);
                ObserveMenu(null);
                DisposeBuffer();
                ClearImageCache();
            }
            base.Dispose(disposing);
        }

        // One UI timer per UI thread; idle buttons have no timers or subscriptions.
        private sealed class ButtonFrames
        {
            [ThreadStatic] private static ButtonFrames _current;
            private static readonly Stopwatch Clock = Stopwatch.StartNew();
            private readonly HashSet<ChromiumButton> _buttons = new HashSet<ChromiumButton>();
            private readonly Timer _timer = new Timer { Interval = 15 };
            internal static double Now => Clock.Elapsed.TotalMilliseconds;

            private ButtonFrames()
            {
                _timer.Tick += (sender, args) =>
                {
                    double now = Now;
                    var buttons = new ChromiumButton[_buttons.Count];
                    _buttons.CopyTo(buttons);
                    foreach (var button in buttons)
                        if (!button.AdvanceFrame(now)) _buttons.Remove(button);
                    if (_buttons.Count == 0) _timer.Stop();
                };
            }

            internal static void Add(ChromiumButton button)
            {
                if (_current == null) _current = new ButtonFrames();
                _current._buttons.Add(button);
                _current._timer.Start();
            }

            internal static void Remove(ChromiumButton button)
            {
                if (_current == null) return;
                _current._buttons.Remove(button);
                if (_current._buttons.Count == 0) _current._timer.Stop();
            }
        }
    }
}
