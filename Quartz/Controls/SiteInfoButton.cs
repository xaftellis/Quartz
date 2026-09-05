using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Quartz.Controls
{
    internal enum SiteInfoIcon
    {
        Information,
        Lock,
        Warning,
        Internal,
        File
    }

    internal sealed class SiteInfoButton : Button
    {
        private bool _isMouseOver;
        public SiteInfoIcon IconKind { get; set; }

        public SiteInfoButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            TabStop = true;
            AccessibleName = "Site information";
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnMouseEnter(System.EventArgs e)
        {
            _isMouseOver = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(System.EventArgs e)
        {
            _isMouseOver = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            if (_isMouseOver || Focused)
            {
                using (var brush = new SolidBrush(Color.FromArgb(28, ForeColor)))
                {
                    e.Graphics.FillEllipse(brush, 1, 1, Width - 2, Height - 2);
                }
            }

            float size = 16f * DeviceDpi / 96f;
            var saved = e.Graphics.Save();
            e.Graphics.TranslateTransform((Width - size) / 2, (Height - size) / 2);
            e.Graphics.ScaleTransform(size / 16, size / 16);
            Color ink = IconKind == SiteInfoIcon.Warning ? Color.FromArgb(200, 72, 54) : ForeColor;
            using (var pen = new Pen(ink, 1.5f)
            {
                LineJoin = LineJoin.Round
            }

            )
            using (var brush = new SolidBrush(ink))
            {
                if (IconKind == SiteInfoIcon.Lock)
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
            if (Focused && ShowFocusCues)
            {
                ControlPaint.DrawFocusRectangle(e.Graphics, ClientRectangle);
            }
        }
    }
}
