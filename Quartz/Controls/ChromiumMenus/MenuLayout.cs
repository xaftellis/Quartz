using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quartz.Controls.ChromiumMenus
{
    internal sealed class MenuRow
    {
        internal ChromiumMenuItem Item;
        internal int Y, Height, TitleX, TitleWidth, MinorWidth;
        internal SKRect[] Buttons;
        internal SKRect Percentage;
    }
    internal sealed class MenuLayout
    {
        internal readonly List<MenuRow> Rows = new List<MenuRow>();
        internal readonly int Width, Height, Column, LabelX, PercentageWidth, SharedMinor;
        internal MenuLayout(ChromiumMenu menu, MenuText text, int maximumWidth = 752)
        {
            var all = menu.Items;
            Column = all.Select(i => i.IsCheck || i.CheckOnClick || i.Radio ? 16 : IconWidth(i)).DefaultIfEmpty().Max();
            int k = all.Any(i => i.IsCheck || i.CheckOnClick || i.Radio || IconWidth(i) > 0) ? 16 : 0;
            LabelX = 20 + Column + (Column > 0 ? 12 : 0);
            PercentageWidth = 4 + (all.OfType<ChromiumZoomMenuItem>().Any(z => !z.HasContents()) ? text.Width(MenuText.Percent(100)) :
                ChromiumZoomMenuItem.Factors.Select(f => text.Width(MenuText.Percent((int)(f * 100 + .5)))).Max());
            int simple = 0, complex = 0, minor = 0, y = 12;
            foreach (var item in all.Where(i => i.Visible))
            {
                if (item is ChromiumMenuSeparator sep)
                {
                    int h = SeparatorHeight(sep.Kind); Rows.Add(new MenuRow { Item = item, Y = y, Height = h }); y += h; continue;
                }
                int iconHeight = IconWidth(item) > 0 ? item.Image != null ? item.ImageSize.Height : 16 : 0;
                int hRow = Math.Max(Math.Max(text.Height * (string.IsNullOrEmpty(item.SecondaryText) ? 1 : 2), iconHeight), k) + 12;
                int x = LabelX + ((item.IsCheck || item.CheckOnClick || item.Radio) && IconWidth(item) > 0 ? IconWidth(item) + 12 : 0);
                int w = text.Width(item.Text);
                int m = string.IsNullOrEmpty(item.ShortcutKeyDisplayString) ? 0 : text.Width(item.ShortcutKeyDisplayString);
                if (item.HasSubmenu) m += 24 + (m > 0 ? 8 : 0);
                minor = Math.Max(minor, m);
                int standard = x + w + 20 + (item is ChromiumZoomMenuItem && w > 0 ? 12 : 0);
                simple = Math.Max(simple, standard);
                complex = Math.Max(complex, standard + (item is ChromiumZoomMenuItem ? 146 + PercentageWidth : 0));
                Rows.Add(new MenuRow { Item = item, Y = y, Height = hRow, TitleX = x, MinorWidth = m }); y += hRow;
            }
            SharedMinor = minor > 0 ? minor + 8 : 0;
            Width = Math.Min(maximumWidth, Math.Max(complex, simple + SharedMinor)); Height = y + 12;
            foreach (var row in Rows)
            {
                row.TitleWidth = Math.Max(0, Width - row.TitleX - 20 - SharedMinor);
                if (row.Item is ChromiumZoomMenuItem)
                {
                    int x = Width - (146 + PercentageWidth);
                    row.TitleWidth = Math.Max(0, x - row.TitleX - 12);
                    row.Buttons = new[] { SKRect.Create(x, row.Y, 46, row.Height), SKRect.Create(x + 46 + PercentageWidth, row.Y, 46, row.Height), SKRect.Create(Width - 54, row.Y, 54, row.Height) };
                    row.Percentage = SKRect.Create(x + 46, row.Y, PercentageWidth, row.Height);
                }
            }
        }
        internal static int IconWidth(ChromiumMenuItem i) => i is ChromiumZoomMenuItem || i.VectorIcon != null ? 16 : i.Image != null ? i.ImageSize.Width : 0;
        internal static int SeparatorHeight(MenuSeparatorKind k) => new[] { 17, 5, 7, 18, 4, 1 }[(int)k];
        internal static float SeparatorY(MenuSeparatorKind k) => new[] { 8.5f, .5f, 6.5f, 9, 0, .5f }[(int)k];
        internal MenuRow Hit(float x, float y) => x < 0 || x >= Width ? null : Rows.FirstOrDefault(r => y >= r.Y && y < r.Y + r.Height && !(r.Item is ChromiumMenuSeparator));
    }
}
