using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Quartz.Controls.ChromiumMenus
{
    // TooltipController/TooltipStateManager: 500 ms show, 10 s visible. Moving
    // within the same target updates a pending anchor without restarting its
    // timer. Changing targets always incurs the full delay, even between tips.
    internal sealed class MenuTooltip : IDisposable
    {
        internal const int ShowDelay = 500, HideDelay = 10000;
        private readonly Stopwatch clock = Stopwatch.StartNew();
        private MenuPopup owner;
        private ChromiumMenuItem item, pressedItem;
        private int part, pressedPart;
        private string text;
        private Point anchor;
        private double deadline;
        private bool pending;
        private TooltipWindow window;
        internal bool Visible => window != null && !window.IsDisposed;
        internal bool Pending => pending;
        internal void Move(MenuPopup popup, MenuRow row, int child, Point position, bool down, double? now = null)
        {
            string value = TextFor(row?.Item, child);
            bool same = owner == popup && item == row?.Item && part == child && text == value;
            if (pressedItem != row?.Item || pressedPart != child) pressedItem = null;
            if (down || row == null || string.IsNullOrWhiteSpace(value)) { Reset(); return; }
            if (same)
            {
                if (pending) anchor = position;
                return;
            }
            Reset(); owner = popup; item = row.Item; part = child; text = value; anchor = position;
            if (pressedItem == item && pressedPart == part) return;
            pending = true; deadline = (now ?? clock.Elapsed.TotalMilliseconds) + ShowDelay;
        }
        internal static string TextFor(ChromiumMenuItem item, int part)
        {
            if (item is ChromiumZoomMenuItem zoom)
                return part == 0 ? "Make Text Smaller" : part == 1 ? "Make Text Larger" : part == 2 ? zoom.IsFullscreen() ? "Exit full screen" : zoom.CanFullscreen() ? "Full screen" : "Full screen disabled" : null;
            return item?.ToolTipText;
        }
        internal void Press()
        {
            pressedItem = item; pressedPart = part; Reset();
        }
        internal void Tick(double? now = null)
        {
            double t = now ?? clock.Elapsed.TotalMilliseconds;
            if (owner == null || owner.IsDisposed || owner.Retired) { Reset(); return; }
            if (pending && t >= deadline)
            {
                pending = false;
                window = new TooltipWindow(text, owner, anchor);
                window.ShowTip(); deadline = t + HideDelay;
            }
            else if (Visible && t >= deadline) Reset();
        }
        internal void Reset()
        {
            pending = false; window?.Dispose(); window = null; owner = null;
            item = null; text = null;
        }
        public void Dispose() { Reset(); clock.Stop(); }

        internal sealed class TooltipWindow : MenuLayeredWindow
        {
            private readonly MenuText metrics = new MenuText(tooltip: true);
            private readonly MenuPopup parent;
            private readonly List<string> lines;
            private readonly float scale;
            private readonly MenuColors colors;
            private readonly int dipWidth, dipHeight;
            internal TooltipWindow(string value, MenuPopup parent, Point point)
            {
                this.parent = parent; scale = parent.Scale; colors = parent.Painter.Appearance.Resolve(); InputTransparent = true;
                Rectangle display = Screen.FromPoint(point).Bounds;
                int maximum = Math.Min(800, ((int)(display.Width / scale) + 1) / 2);
                value = Truncate(value).Replace("\t", "        ");
                lines = Wrap(value, metrics, maximum);
                dipWidth = lines.Max(metrics.Width) + 16; dipHeight = lines.Count * metrics.Height + 9;
                Size = new Size(parent.Pixel(dipWidth), parent.Pixel(dipHeight));
                Location = Place(Size, point, display, scale, parent.Painter.Appearance.RightToLeft);
                AccessibleName = value; AccessibleRole = AccessibleRole.ToolTip;
            }
            internal static Point Place(Size size, Point anchor, Rectangle display, float scale, bool rtl)
            {
                int x = anchor.X + (rtl ? -size.Width : (int)Math.Round(10 * scale, MidpointRounding.AwayFromZero));
                int y = anchor.Y + (int)Math.Round(15 * scale, MidpointRounding.AwayFromZero);
                if (y + size.Height > display.Bottom) y = anchor.Y - size.Height;
                return new Point(Math.Max(display.Left, Math.Min(display.Right - size.Width, x)), Math.Max(display.Top, Math.Min(display.Bottom - size.Height, y)));
            }
            internal void ShowTip()
            {
                Show(parent);
                using (var bitmap = new SKBitmap(Width, Height, SKColorType.Bgra8888, SKAlphaType.Premul))
                using (var props = MenuPlatform.SurfaceProperties())
                using (var surface = SKSurface.Create(bitmap.Info, bitmap.GetPixels(), bitmap.RowBytes, props))
                using (var paint = new SKPaint { IsAntialias = false, Color = colors.TooltipForeground })
                {
                    var canvas = surface.Canvas; canvas.Clear(colors.TooltipBackground); canvas.Scale(scale);
                    // TooltipViewAura: square, 1-DIP solid border; total insets 4,8,5,8.
                    canvas.DrawRect(0, 0, dipWidth, 1, paint); canvas.DrawRect(0, dipHeight - 1, dipWidth, 1, paint);
                    canvas.DrawRect(0, 0, 1, dipHeight, paint); canvas.DrawRect(dipWidth - 1, 0, 1, dipHeight, paint);
                    for (int i = 0; i < lines.Count; ++i)
                    {
                        bool rtl = MenuPlatform.IsTextRtl(lines[i]);
                        metrics.Draw(canvas, lines[i], rtl ? dipWidth - 8 - metrics.Width(lines[i]) : 8, 4 + i * metrics.Height + metrics.CenterBaseline(metrics.Height), colors.TooltipForeground);
                    }
                    SetImage(bitmap);
                }
                AccessibilityNotifyClients(AccessibleEvents.Show, -1);
            }
            internal static string Truncate(string value)
            {
                if (value.Length <= 1024) return value.Trim();
                int end = MenuPlatform.Breaks(value, 1).LastOrDefault(i => i < 1024);
                if (end == 0) end = 1023;
                if (char.IsHighSurrogate(value[end - 1])) --end;
                while (end > 0 && (char.IsWhiteSpace(value[end - 1]) || char.IsControl(value[end - 1]) || char.GetUnicodeCategory(value[end - 1]) == UnicodeCategory.NonSpacingMark)) --end;
                return (value.Substring(0, end) + "…").Trim();
            }
            internal static List<string> Wrap(string value, MenuText metrics, int maximum)
            {
                var result = new List<string>();
                foreach (string paragraph in value.Replace("\r\n", "\n").Split('\n'))
                {
                    if (paragraph.Length == 0) { result.Add(""); continue; }
                    int start = 0;
                    int[] breaks = MenuPlatform.Breaks(paragraph, 2);
                    int[] graphemes = StringInfo.ParseCombiningCharacters(paragraph).Concat(new[] { paragraph.Length }).ToArray();
                    while (start < paragraph.Length)
                    {
                        int end = start;
                        foreach (int next in breaks.Where(b => b > start))
                        {
                            if (metrics.Width(paragraph.Substring(start, next - start).TrimEnd()) > maximum) break;
                            end = next;
                        }
                        if (end == start)
                            foreach (int next in graphemes.Where(b => b > start))
                            {
                                if (end > start && metrics.Width(paragraph.Substring(start, next - start)) > maximum) break;
                                end = next;
                            }
                        result.Add(paragraph.Substring(start, end - start).TrimEnd()); start = end;
                    }
                }
                return result;
            }
            protected override void Dispose(bool disposing) { if (disposing) metrics.Dispose(); base.Dispose(disposing); }
        }
    }
}
