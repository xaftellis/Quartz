// Glyph paths: Copyright 2026 The Chromium Authors. BSD license in
// chromeium/new_tab/LICENSE. Source revision and paths: chromeium/context_menu/README.md.
using Svg;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml;

namespace Quartz.Controls
{
    internal sealed class ChromiumMenuRenderer : ToolStripRenderer
    {
        internal static readonly ChromiumMenuRenderer Instance = new ChromiumMenuRenderer();
        private static readonly Dictionary<string, Bitmap> Glyphs = new Dictionary<string, Bitmap>();

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using (var brush = new SolidBrush(e.ToolStrip.BackColor)) e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) { }
        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e) { }
        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e) { }
        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
        {
            if (!(e.Item is ToolStripMenuItem)) base.OnRenderItemImage(e);
        }
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            if (!(e.Item is ToolStripMenuItem)) base.OnRenderItemText(e);
        }
        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            if (!(e.Item is ToolStripMenuItem)) base.OnRenderArrow(e);
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            var item = e.Item as ToolStripMenuItem;
            var menu = item?.Owner as ToolStripDropDown;
            if (menu == null) { base.OnRenderMenuItemBackground(e); return; }
            var state = ChromiumMenuStyle.GetState(menu);
            var colors = state.Palette ?? ChromiumMenuPalette.Current;
            bool selected = item.Enabled && (item.Selected || item.Pressed);
            if (selected)
                using (var brush = new SolidBrush(colors.Hover)) e.Graphics.FillRectangle(brush, new Rectangle(Point.Empty, item.Size));
            Color foreground = !item.Enabled ? colors.Disabled :
                selected && SystemInformation.HighContrast ? SystemColors.HighlightText : colors.Foreground;
            Color secondary = !item.Enabled ? colors.Disabled :
                selected && SystemInformation.HighContrast ? SystemColors.HighlightText : colors.Secondary;
            bool rtl = menu.RightToLeft == RightToLeft.Yes;
            int edge = ChromiumMenuStyle.Scale(menu, 20), glyph = ChromiumMenuStyle.Scale(menu, 16);
            int gap = ChromiumMenuStyle.Scale(menu, 8), right = item.Width - edge;
            var flags = TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding |
                TextFormatFlags.EndEllipsis | TextFormatFlags.PreserveGraphicsClipping;
            if (!SystemInformation.MenuAccessKeysUnderlined) flags |= TextFormatFlags.HidePrefix;
            if (rtl) flags |= TextFormatFlags.RightToLeft | TextFormatFlags.Right;

            if (item.HasDropDownItems)
            {
                DrawArrow(e.Graphics, Mirror(new Rectangle(right - glyph, (item.Height - glyph) / 2, glyph, glyph), item.Width, rtl), secondary, rtl);
                right -= glyph + gap;
            }
            string shortcut = ChromiumMenuStyle.Shortcut(item);
            if (!string.IsNullOrEmpty(shortcut))
            {
                int width = TextRenderer.MeasureText(e.Graphics, shortcut, item.Font, Size.Empty,
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width;
                TextRenderer.DrawText(e.Graphics, shortcut, item.Font,
                    Mirror(new Rectangle(right - width, 0, width, item.Height), item.Width, rtl), secondary,
                    flags | TextFormatFlags.NoPrefix);
                right -= width + gap;
            }
            TextRenderer.DrawText(e.Graphics, ChromiumMenuStyle.Label(item), item.Font,
                Mirror(new Rectangle(state.LabelStart, 0, System.Math.Max(0, right - state.LabelStart), item.Height), item.Width, rtl), foreground, flags);
            var iconRect = Mirror(new Rectangle(edge, (item.Height - glyph) / 2, glyph, glyph), item.Width, rtl);
            if (item.Checked)
            {
                DrawCheck(e.Graphics, iconRect, secondary, item.CheckState == CheckState.Indeterminate);
            }
            if (item.Image != null)
            {
                if (state.IconColumns == 2) iconRect.Offset(rtl ? -(glyph + gap) : glyph + gap, 0);
                if (item.Enabled) e.Graphics.DrawImage(item.Image, iconRect);
                else ControlPaint.DrawImageDisabled(e.Graphics, item.Image, iconRect.X, iconRect.Y, colors.Background);
            }
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            var menu = e.ToolStrip as ToolStripDropDown;
            if (menu == null) { base.OnRenderSeparator(e); return; }
            var colors = ChromiumMenuStyle.GetState(menu).Palette ?? ChromiumMenuPalette.Current;
            using (var brush = new SolidBrush(colors.Separator))
                e.Graphics.FillRectangle(brush, 0, e.Item.Height / 2, e.Item.Width, System.Math.Max(1, ChromiumMenuStyle.Scale(menu, 1)));
        }

        private static Rectangle Mirror(Rectangle bounds, int width, bool rtl)
        {
            if (rtl) bounds.X = width - bounds.Right;
            return bounds;
        }
        private static void DrawArrow(Graphics graphics, Rectangle box, Color color, bool rtl)
        {
            DrawGlyph(graphics, box, color, false, rtl);
        }
        private static void DrawCheck(Graphics graphics, Rectangle box, Color color, bool indeterminate)
        {
            var save = graphics.Save();
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var pen = new Pen(color, box.Width / 10f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            {
                if (indeterminate) graphics.DrawLine(pen, box.X + box.Width * .2f, box.Y + box.Height * .5f, box.X + box.Width * .8f, box.Y + box.Height * .5f);
                else DrawGlyph(graphics, box, color, true, false);
            }
            graphics.Restore(save);
        }

        private static void DrawGlyph(Graphics graphics, Rectangle box, Color color, bool check, bool rtl)
        {
            string key = check + ":" + rtl + ":" + box.Width + ":" + color.ToArgb();
            if (!Glyphs.TryGetValue(key, out Bitmap image))
            {
                // Bound the cache if Windows changes high-contrast colors/DPI repeatedly.
                if (Glyphs.Count >= 64)
                {
                    foreach (var old in Glyphs.Values) old.Dispose();
                    Glyphs.Clear();
                }
                string path = check
                    ? "m9.55 15.15 8.47-8.47a.97.97 0 0 1 1.4 0c.2.2.3.44.3.71a.98.98 0 0 1-.3.72L10.25 17.3a.96.96 0 0 1-1.4 0L4.55 13a.94.94 0 0 1-.29-.71 1.02 1.02 0 0 1 .31-.72c.2-.2.44-.3.72-.3.27 0 .51.1.71.3Z"
                    : "M8.57 8 5.9 5.33a.68.68 0 0 1 0-.98.67.67 0 0 1 .98 0l3.16 3.16a.67.67 0 0 1 .2.48.67.67 0 0 1-.2.48l-3.16 3.16c-.14.14-.3.21-.48.2a.71.71 0 0 1-.48-.22.67.67 0 0 1 0-.98Z";
                int canvas = check ? 24 : 16;
                var xml = new XmlDocument();
                xml.LoadXml("<svg xmlns='http://www.w3.org/2000/svg' width='" + canvas + "' height='" + canvas +
                    "' viewBox='0 0 " + canvas + " " + canvas + "'><path fill='#" +
                    color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2") + "' d='" + path + "'/></svg>");
                image = SvgDocument.Open(xml).Draw(box.Width, box.Height);
                if (rtl) image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                Glyphs.Add(key, image);
            }
            graphics.DrawImageUnscaled(image, box.Location);
        }
    }
}
