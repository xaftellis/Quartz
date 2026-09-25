using SkiaSharp;
using System;
using System.IO;

namespace Quartz.Controls.ChromiumMenus
{
    internal sealed class MenuPainter
    {
        internal readonly MenuText Text;
        internal readonly MenuAppearance Appearance;
        internal readonly MenuColors Colors;
        internal MenuPainter(MenuText text, MenuAppearance appearance) { Text = text; Appearance = appearance; Colors = appearance.Resolve(); }
        internal void Paint(SKCanvas c, MenuLayout layout, bool submenu, float scale, int visibleHeight, int scroll, ChromiumMenuItem selected, int part, bool cues)
        {
            c.Clear(SKColors.Transparent); c.Scale(scale);
            int left = submenu ? 16 : 24, top = submenu ? 16 : 12;
            var body = SKRect.Create(left, top, layout.Width, visibleHeight);
            using (var round = new SKRoundRect(body, 12))
            using (var p = new SKPaint { IsAntialias = true })
            {
                // gfx::ShadowValue: blur is a diameter; Skia's blur mask uses sigma.
                c.Save(); c.ClipRoundRect(round, SKClipOperation.Difference, true);
                PaintShadow(c, body, submenu ? 32 : 48, 12, 0x3d);
                PaintShadow(c, body, submenu ? 32 : 24, 0, submenu ? (byte)0x1a : (byte)0x1f);
                c.Restore(); p.Color = Colors.Body; c.DrawRoundRect(round, p);
                c.Save(); c.ClipRoundRect(round, SKClipOperation.Intersect, true); c.Translate(left, top);
                bool scrolling = layout.Height > visibleHeight; int arrowH = Text.Height + 12;
                c.Save();
                c.ClipRect(SKRect.Create(0, scrolling ? arrowH : 0, layout.Width, visibleHeight - (scrolling ? arrowH * 2 : 0)));
                c.Translate(0, -scroll + (scrolling ? arrowH : 0));
                foreach (var row in layout.Rows)
                {
                    var item = row.Item;
                    bool hot = item == selected && item.Enabled;
                    if (item is ChromiumMenuSeparator sep)
                    {
                        if (sep.Kind != MenuSeparatorKind.Spacing)
                        {
                            p.IsAntialias = false; p.Color = Colors.Separator; p.StrokeWidth = 0;
                            c.DrawLine(sep.Kind == MenuSeparatorKind.Padded ? 64 : 0, row.Y + MenuLayout.SeparatorY(sep.Kind), layout.Width, row.Y + MenuLayout.SeparatorY(sep.Kind), p);
                        }
                        continue;
                    }
                    if (hot && !(item is ChromiumZoomMenuItem))
                    { p.Color = Colors.Selected; p.IsAntialias = false; c.DrawRect(0, row.Y, layout.Width, row.Height, p); }
                    SKColor color = !item.Enabled ? Colors.Disabled : hot ? Colors.SelectedText : Colors.Text;
                    int lines = string.IsNullOrEmpty(item.SecondaryText) ? 1 : 2;
                    int baseline = row.Y + 6 + (row.Height - 12 - lines * Text.Height) / 2 + Text.Ascent;
                    string label = Text.Elide(MenuText.Label(item.Text), row.TitleWidth);
                    Text.Draw(c, label, X(row.TitleX, Text.Width(label), layout.Width), baseline, color);
                    if (cues) DrawMnemonic(c, item.Text, label, row.TitleX, baseline, layout.Width, color);
                    if (lines == 2) Text.Draw(c, Text.Elide(item.SecondaryText, row.TitleWidth), X(row.TitleX, Text.Width(Text.Elide(item.SecondaryText, row.TitleWidth)), layout.Width), baseline + Text.Height, item.Enabled ? Colors.Minor : Colors.Disabled);
                    int iy = row.Y + (row.Height - 16) / 2;
                    if (!item.Radio && (item.IsCheck || item.CheckOnClick) && item.Checked)
                        Glyph(c, Appearance.RoundedIcons ? "check" : "menu_check_old", 20, iy, layout.Width, scale, MenuColors.DerivedIcon(color));
                    else if (item.Radio)
                        Glyph(c, item.Checked ? Appearance.RoundedIcons ? "radio_button_checked" : "menu_radio_selected_old" : Appearance.RoundedIcons ? "circle" : "menu_radio_empty_old", 20, iy, layout.Width, scale, item.Checked ? Colors.RadioOn : Colors.RadioOff);
                    int ix = item.IsCheck || item.CheckOnClick || item.Radio ? layout.LabelX : 20 + (layout.Column - MenuLayout.IconWidth(item)) / 2;
                    if (item is ChromiumZoomMenuItem zoom)
                    {
                        Glyph(c, Appearance.RoundedIcons ? "zoom_in" : "zoom_in_old", ix, iy, layout.Width, scale, Appearance.HighContrast ? MenuColors.Win(System.Drawing.SystemColors.WindowText) : Colors.Minor);
                        PaintZoom(c, layout, row, zoom, scale, hot ? part : -1); continue;
                    }
                    if (item.VectorIcon != null) Glyph(c, item.VectorIcon, ix, iy, layout.Width, scale, item.Enabled ? hot ? MenuColors.DerivedIcon(color) : Colors.Minor : Colors.DisabledIcon);
                    if (item.Image != null)
                    {
                        using (var ms = new MemoryStream())
                        {
                            item.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            using (var bitmap = SKBitmap.Decode(ms.ToArray())) c.DrawBitmap(bitmap, SKRect.Create(X(ix, item.ImageSize.Width, layout.Width), row.Y + (row.Height - item.ImageSize.Height) / 2, item.ImageSize.Width, item.ImageSize.Height));
                        }
                    }
                    if (item.HasSubmenu) Glyph(c, Appearance.RoundedIcons ? "keyboard_arrow_right_flippable" : "submenu_arrow_chrome_refresh_old", layout.Width - 36, iy, layout.Width, scale, MenuColors.DerivedIcon(color));
                    if (!string.IsNullOrEmpty(item.ShortcutKeyDisplayString))
                    {
                        string minor = item.ShortcutKeyDisplayString;
                        int w = Text.Width(minor) + 1, right = layout.Width - 20 - (item.HasSubmenu ? 24 : 0);
                        Text.Draw(c, minor, X(right - w, w, layout.Width), row.Y + 6 + Text.CenterBaseline(row.Height - 12), !item.Enabled ? Colors.Disabled : hot ? Colors.SelectedText : Colors.Minor);
                    }
                }
                c.Restore();
                if (scrolling) { ScrollArrow(c, layout.Width, arrowH / 2, true, scroll > 0); ScrollArrow(c, layout.Width, visibleHeight - arrowH / 2, false, scroll < layout.Height - visibleHeight + 2 * arrowH); }
                c.Restore();
            }
        }
        private void PaintShadow(SKCanvas c, SKRect r, int blur, int dy, byte alpha)
        {
            using (var p = new SKPaint { Color = Colors.Shadow.WithAlpha(alpha), IsAntialias = true, MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, .288675f * (blur / 2f) + .5f) })
            { r.Offset(0, dy); c.DrawRoundRect(r, 12, 12, p); }
        }
        private void PaintZoom(SKCanvas c, MenuLayout layout, MenuRow row, ChromiumZoomMenuItem zoom, float scale, int part)
        {
            string percentage = MenuText.Percent(zoom.Percent);
            int w = Text.Width(percentage); float px = row.Percentage.Right - 2 - w;
            Text.Draw(c, percentage, X(px, w, layout.Width), row.Y + Text.CenterBaseline(row.Height), Colors.Text);
            for (int i = 0; i < 3; ++i)
            {
                var rect = row.Buttons[i];
                using (var p = new SKPaint { IsAntialias = false })
                {
                    if (i == 2) { p.Color = Colors.Separator; p.StrokeWidth = 0; float dx = X(rect.Left + .5f, 0, layout.Width); c.DrawLine(dx, rect.Top, dx, rect.Bottom, p); }
                    if (zoom.ButtonEnabled(i))
                    {
                        p.Color = i == part ? Appearance.HighContrast ? Colors.Selected : Colors.HotDisc : Colors.Disc;
                        float dx = rect.Left + (i == 2 ? 13 : 9);
                        c.DrawRoundRect(SKRect.Create(X(dx, 28, layout.Width), row.Y + (row.Height - 28) / 2, 28, 28), 14, 14, p);
                    }
                }
                string glyph = i == 0 ? Appearance.RoundedIcons ? "remove" : "zoom_minus_menu_refresh_old" : i == 1 ? Appearance.RoundedIcons ? "add" : "zoom_plus_menu_refresh_old" : Appearance.RoundedIcons ? "fullscreen" : "fullscreen_refresh_old";
                Glyph(c, glyph, rect.Left + (i == 2 ? 19 : 15), row.Y + (row.Height - 16) / 2, layout.Width, scale, i == part && zoom.ButtonEnabled(i) ? Colors.SelectedText : Colors.ZoomIcon);
            }
        }
        private void Glyph(SKCanvas c, string icon, float x, float y, int width, float scale, SKColor color) => MenuVectors.Draw(c, icon, X(x, 16, width), y, 16, scale, color, Appearance.RightToLeft);
        private float X(float x, float w, int width) => Appearance.RightToLeft ? width - x - w : x;
        private void ScrollArrow(SKCanvas c, int w, int y, bool up, bool enabled)
        {
            using (var path = new SKPath()) using (var p = new SKPaint { Color = enabled ? Colors.Icon : Colors.Disabled, IsAntialias = true })
            { path.MoveTo(w / 2 - 3, y + (up ? 1.5f : -1.5f)); path.LineTo(w / 2 + 3, y + (up ? 1.5f : -1.5f)); path.LineTo(w / 2, y + (up ? -1.5f : 1.5f)); path.Close(); c.DrawPath(path, p); }
        }
        private void DrawMnemonic(SKCanvas c, string raw, string label, int x, int y, int width, SKColor color)
        {
            int index = -1, visible = 0;
            for (int i = 0; i < raw.Length; ++i)
            {
                if (raw[i] == '&') { if (i + 1 < raw.Length && raw[i + 1] == '&') ++i; else { index = visible; break; } }
                ++visible;
            }
            if (index < 0 || index >= label.Length) return;
            float start = Text.Width(label.Substring(0, index)), end = Text.Width(label.Substring(0, index + 1));
            using (var p = new SKPaint { Color = color, StrokeWidth = 1 }) c.DrawLine(X(x + start, 0, width), y + 1, X(x + end, 0, width), y + 1, p);
        }
    }
}
