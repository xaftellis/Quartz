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

    public sealed class SiteInfoButton : ChromiumButton
    {
        private SiteInfoIcon _iconKind;

        [Category("Appearance"), DefaultValue(SiteInfoIcon.Information)]
        public SiteInfoIcon IconKind
        {
            get => _iconKind;
            set { _iconKind = value; Invalidate(); }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPopupOpen
        {
            get => IsActive;
            set => IsActive = value;
        }

        public SiteInfoButton()
        {
            Size = new Size(26, 26);
            UseToolbarGeometry = false;
            Cursor = Cursors.Hand;
            AccessibleName = "Site information";
            Padding = Padding.Empty;
        }

        // IconLabelBubbleView uses omnibox opacities and its surrounding text
        // colour rather than ToolbarButton's maximum-contrast .08/.06 ink.
        protected override double HighlightOpacity => .10;
        protected override double RippleOpacity => .16;
        protected override Color DefaultInkColor => ForeColor;

        protected override void PaintButtonContent(Graphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            float size = IconSize * DpiScale;
            var saved = graphics.Save();
            graphics.TranslateTransform((Width - size) / 2, (Height - size) / 2);
            graphics.ScaleTransform(size / 16, size / 16);
            // The warning keeps its triangle shape, using the address bar's theme ink.
            Color ink = ContentColor;
            using (var pen = new Pen(ink, 1.5f)
            {
                LineJoin = LineJoin.Round
            }

            )
            using (var brush = new SolidBrush(ink))
            {
                if (IconKind == SiteInfoIcon.Close)
                {
                    graphics.DrawLine(pen, 4, 4, 12, 12);
                    graphics.DrawLine(pen, 12, 4, 4, 12);
                }
                else if (IconKind == SiteInfoIcon.Back)
                {
                    graphics.DrawLines(pen, new[] { new PointF(10, 3), new PointF(5, 8), new PointF(10, 13) });
                }
                else if (IconKind == SiteInfoIcon.Lock)
                {
                    graphics.DrawArc(pen, 4.5f, 1.5f, 7, 8, 180, 180);
                    graphics.FillRectangle(brush, 3, 7, 10, 7);
                    using (var hole = new Pen(BackColor, 1.2f))
                    {
                        graphics.DrawLine(hole, 8, 9, 8, 12);
                    }
                }
                else if (IconKind == SiteInfoIcon.Warning)
                {
                    graphics.DrawPolygon(pen, new[] { new PointF(8, 1.5f), new PointF(15, 14), new PointF(1, 14) });
                    graphics.DrawLine(pen, 8, 6, 8, 9);
                    graphics.FillEllipse(brush, 7.25f, 11, 1.5f, 1.5f);
                }
                else if (IconKind == SiteInfoIcon.Internal)
                {
                    graphics.DrawEllipse(pen, 2, 2, 11, 11);
                    graphics.DrawLine(pen, 9, 10, 14, 15);
                }
                else if (IconKind == SiteInfoIcon.File)
                {
                    graphics.DrawRectangle(pen, 3, 1.5f, 10, 13);
                    graphics.DrawLine(pen, 5, 7, 11, 7);
                    graphics.DrawLine(pen, 5, 10, 11, 10);
                }
                else
                {
                    graphics.DrawEllipse(pen, 1.5f, 1.5f, 13, 13);
                    graphics.FillEllipse(brush, 7.25f, 4, 1.5f, 1.5f);
                    graphics.DrawLine(pen, 8, 7, 8, 12);
                }
            }

            graphics.Restore(saved);
        }
    }
}
