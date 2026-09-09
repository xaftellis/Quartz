using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Quartz.Controls
{
    public enum SiteInfoIcon
    {
        Information,
        Lock,
        Warning,
        Internal,
        File,
        Close,
        Back
    }

    public sealed class SiteInfoButton : Button
    {
        // Space on each side of the icon, before Windows display scaling.
        private const int HoverPadding = 4;
        private bool _isMouseOver;
        private bool _isMouseDown;
        private bool _isKeyDown;
        private bool _isClickInProgress;
        private bool _isPopupOpen;
        private bool _showKeyboardFocus;
        private SiteInfoIcon _iconKind;
        private int _iconSize = 18;

        private Bitmap _hoverCircle;
        private Bitmap _pressedCircle;

        [Category("Appearance")]
        [DefaultValue(SiteInfoIcon.Information)]
        public SiteInfoIcon IconKind
        {
            get { return _iconKind; }
            set
            {
                _iconKind = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("The icon size in pixels at 100% display scaling.")]
        [DefaultValue(18)]
        public int IconSize
        {
            get { return _iconSize; }
            set
            {
                if (value < 1)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                _iconSize = value;
                UpdateHoverSize();
                Invalidate();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPopupOpen
        {
            get { return _isPopupOpen; }
            set
            {
                _isPopupOpen = value;
                _isMouseDown = false;
                _isKeyDown = false;
                // Opening the popup can interrupt the button's mouse-leave event.
                _isMouseOver = !value && IsPointerOverButton();
                Invalidate();
            }
        }

        public SiteInfoButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            TabStop = true;
            AccessibleName = "Site information";
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
            UpdateHoverSize();
        }

        private RectangleF GetHoverBounds()
        {
            float diameter = (IconSize + HoverPadding * 2) * DeviceDpi / 96f;
            return new RectangleF((Width - diameter) / 2, (Height - diameter) / 2, diameter, diameter);
        }

        private bool IsPointerOverButton()
        {
            if (!IsHandleCreated || !Visible || !Enabled)
            {
                return false;
            }

            return IsPointOverButton(PointToClient(Cursor.Position));
        }

        private bool IsPointOverButton(Point point)
        {
            using (var shape = new GraphicsPath())
            {
                shape.AddEllipse(GetHoverBounds());
                return ClientRectangle.Contains(point) && shape.IsVisible(point);
            }
        }

        private void UpdateHoverSize()
        {
            DisposeCircleImages();
            int diameter = (int)System.Math.Ceiling((IconSize + HoverPadding * 2) * DeviceDpi / 96f);
            MinimumSize = new Size(diameter, diameter);
            UpdateButtonRegion();
        }

        protected override void OnDpiChangedAfterParent(System.EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            UpdateHoverSize();
        }

        protected override void OnMouseEnter(System.EventArgs e)
        {
            _isMouseOver = IsPointerOverButton();
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnSizeChanged(System.EventArgs e)
        {
            base.OnSizeChanged(e);
            DisposeCircleImages();
            UpdateButtonRegion();
        }

        protected override void OnForeColorChanged(System.EventArgs e)
        {
            base.OnForeColorChanged(e);
            DisposeCircleImages();
            Invalidate();
        }

        private Bitmap CreateCircleImage(Color color)
        {
            Bitmap circle = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            try
            {
                // Same transparent bitmap + TextureBrush technique as CircularImageButton.
                using (Bitmap fill = new Bitmap(Width, Height, PixelFormat.Format32bppArgb))
                using (Graphics fillGraphics = Graphics.FromImage(fill))
                using (Graphics circleGraphics = Graphics.FromImage(circle))
                using (GraphicsPath path = new GraphicsPath())
                {
                    fillGraphics.Clear(color);
                    circleGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                    path.AddEllipse(GetHoverBounds());
                    using (TextureBrush brush = new TextureBrush(fill))
                    {
                        circleGraphics.FillPath(brush, path);
                    }
                }
                return circle;
            }
            catch
            {
                circle.Dispose();
                throw;
            }
        }

        private void DisposeCircleImages()
        {
            _hoverCircle?.Dispose();
            _pressedCircle?.Dispose();
            _hoverCircle = null;
            _pressedCircle = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeCircleImages();
            }
            base.Dispose(disposing);
        }

        private void UpdateButtonRegion()
        {
            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            // A Windows region has hard edges. Leave room for the painted circle's
            // anti-aliased edge instead of clipping at that exact edge.
            using (var shape = new GraphicsPath())
            {
                RectangleF bounds = GetHoverBounds();
                bounds.Inflate(1, 1);
                shape.AddEllipse(bounds);
                Region oldRegion = Region;
                Region = new Region(shape);
                oldRegion?.Dispose();
            }
        }

        protected override void OnMouseLeave(System.EventArgs e)
        {
            _isMouseOver = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _showKeyboardFocus = false;
            Invalidate();
            if (e.Button == MouseButtons.Left)
            {
                _isMouseDown = true;
                Invalidate();
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            // WinForms can repaint during Click, before the popup becomes visible.
            _isClickInProgress = e.Button == MouseButtons.Left && _isMouseDown && IsPointerOverButton();
            try
            {
                base.OnMouseUp(e);
            }
            finally
            {
                _isClickInProgress = false;
                if (e.Button == MouseButtons.Left)
                {
                    _isMouseDown = false;
                }

                Invalidate();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            // The native region includes a small edge outside the painted circle.
            // Crossing from that edge into the circle does not fire MouseEnter again.
            bool isMouseOver = Enabled && IsPointOverButton(e.Location);
            if (_isMouseOver != isMouseOver || _isMouseDown)
            {
                _isMouseOver = isMouseOver;
                Invalidate();
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseCaptureChanged(System.EventArgs e)
        {
            if (!Capture)
            {
                _isMouseDown = false;
                Invalidate();
            }

            base.OnMouseCaptureChanged(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                _showKeyboardFocus = true;
                _isKeyDown = true;
                Invalidate();
            }

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            _isClickInProgress = e.KeyCode == Keys.Space && _isKeyDown;
            try
            {
                base.OnKeyUp(e);
            }
            finally
            {
                _isClickInProgress = false;
                if (e.KeyCode == Keys.Space)
                {
                    _isKeyDown = false;
                }

                Invalidate();
            }
        }

        protected override void OnEnter(System.EventArgs e)
        {
            // Enter tracks tab navigation; popup activation only changes focus.
            _showKeyboardFocus = MouseButtons == MouseButtons.None;
            Invalidate();
            base.OnEnter(e);
        }

        protected override void OnLostFocus(System.EventArgs e)
        {
            _isKeyDown = false;
            Invalidate();
            base.OnLostFocus(e);
        }

        protected override void OnEnabledChanged(System.EventArgs e)
        {
            if (!Enabled)
            {
                _isMouseDown = false;
                _isKeyDown = false;
            }

            Invalidate();
            base.OnEnabledChanged(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Clear the same buffer as the icon in OnPaint, not a separate erase pass.
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF hoverBounds = GetHoverBounds();
            bool pointerOverButton = IsPointerOverButton();
            bool pressed = Enabled && ((_isMouseDown && pointerOverButton) || _isKeyDown || _isClickInProgress);
            if (pressed || (_isMouseOver && pointerOverButton) || _isPopupOpen)
            {
                if (pressed || _isPopupOpen)
                {
                    if (_pressedCircle == null)
                    {
                        _pressedCircle = CreateCircleImage(Color.FromArgb(52, ForeColor));
                    }
                    e.Graphics.DrawImageUnscaled(_pressedCircle, 0, 0);
                }
                else
                {
                    if (_hoverCircle == null)
                    {
                        _hoverCircle = CreateCircleImage(Color.FromArgb(28, ForeColor));
                    }
                    e.Graphics.DrawImageUnscaled(_hoverCircle, 0, 0);
                }
            }

            float size = IconSize * DeviceDpi / 96f;
            var saved = e.Graphics.Save();
            e.Graphics.TranslateTransform((Width - size) / 2, (Height - size) / 2);
            e.Graphics.ScaleTransform(size / 16, size / 16);
            // The warning keeps its triangle shape, using the address bar's theme ink.
            Color ink = ForeColor;
            using (var pen = new Pen(ink, 1.5f)
            {
                LineJoin = LineJoin.Round
            }

            )
            using (var brush = new SolidBrush(ink))
            {
                if (IconKind == SiteInfoIcon.Close)
                {
                    e.Graphics.DrawLine(pen, 4, 4, 12, 12);
                    e.Graphics.DrawLine(pen, 12, 4, 4, 12);
                }
                else if (IconKind == SiteInfoIcon.Back)
                {
                    e.Graphics.DrawLines(pen, new[] { new PointF(10, 3), new PointF(5, 8), new PointF(10, 13) });
                }
                else if (IconKind == SiteInfoIcon.Lock)
                {
                    e.Graphics.DrawArc(pen, 4.5f, 1.5f, 7, 8, 180, 180);
                    e.Graphics.FillRectangle(brush, 3, 7, 10, 7);
                    using (var hole = new Pen(BackColor, 1.2f))
                    {
                        e.Graphics.DrawLine(hole, 8, 9, 8, 12);
                    }
                }
                else if (IconKind == SiteInfoIcon.Warning)
                {
                    e.Graphics.DrawPolygon(pen, new[] { new PointF(8, 1.5f), new PointF(15, 14), new PointF(1, 14) });
                    e.Graphics.DrawLine(pen, 8, 6, 8, 9);
                    e.Graphics.FillEllipse(brush, 7.25f, 11, 1.5f, 1.5f);
                }
                else if (IconKind == SiteInfoIcon.Internal)
                {
                    e.Graphics.DrawEllipse(pen, 2, 2, 11, 11);
                    e.Graphics.DrawLine(pen, 9, 10, 14, 15);
                }
                else if (IconKind == SiteInfoIcon.File)
                {
                    e.Graphics.DrawRectangle(pen, 3, 1.5f, 10, 13);
                    e.Graphics.DrawLine(pen, 5, 7, 11, 7);
                    e.Graphics.DrawLine(pen, 5, 10, 11, 10);
                }
                else
                {
                    e.Graphics.DrawEllipse(pen, 1.5f, 1.5f, 13, 13);
                    e.Graphics.FillEllipse(brush, 7.25f, 4, 1.5f, 1.5f);
                    e.Graphics.DrawLine(pen, 8, 7, 8, 12);
                }
            }

            e.Graphics.Restore(saved);
            if (Focused && ShowFocusCues && _showKeyboardFocus && !_isPopupOpen)
            {
                using (var focusPen = new Pen(ForeColor, 1))
                {
                    focusPen.DashStyle = DashStyle.Dot;
                    float inset = 2f * DeviceDpi / 96f;
                    hoverBounds.Inflate(-inset, -inset);
                    e.Graphics.DrawEllipse(focusPen, hoverBounds);
                }
            }
        }
    }
}
