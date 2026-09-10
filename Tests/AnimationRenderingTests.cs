using Quartz.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

internal static partial class FavouritesTests
{
    private static Bitmap CaptureBar(FavouritesBar bar)
    {
        typeof(Control).GetMethod("CreateControl", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            null, new[] { typeof(bool) }, null).Invoke(bar, new object[] { true });
        var image = new Bitmap(bar.Width, bar.Height);
        bar.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
        return image;
    }

    private static string BlueGlyphs(Bitmap image)
    {
        var points = new List<Point>();
        for (int y = 0; y < image.Height; y++)
            for (int x = 0; x < image.Width; x++)
            {
                Color pixel = image.GetPixel(x, y);
                if (pixel.B > pixel.R + 20 && pixel.B > pixel.G + 20) points.Add(new Point(x, y));
            }
        Check(points.Count > 0, "The frame must contain a legible label.");
        int left = points.Min(p => p.X), top = points.Min(p => p.Y);
        return string.Join(";", points.Select(p => (p.X - left) + "," + (p.Y - top)));
    }

    private static void SingleLabelFrames()
    {
        foreach (FlatStyle style in new[] { FlatStyle.Flat, FlatStyle.Popup, FlatStyle.Standard })
        foreach (Color background in new[] { Color.White, Color.FromArgb(35, 35, 35) })
        using (var row = new Row())
        {
            var button = ContentButton("Quartz");
            button.FlatStyle = style;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = background;
            button.ForeColor = Color.Blue;
            var items = new FavouriteButton[] { button };
            var expected = new Dictionary<bool, string>();
            foreach (bool icons in new[] { false, true })
            {
                row.Bar.UpdateItems(items, b => ConfigureContent(b, icons), false);
                using (Bitmap frame = button.CaptureVisual()) expected[icons] = BlueGlyphs(frame);
            }
            foreach (bool icons in new[] { false, true, false, true })
            {
                row.Bar.UpdateItems(items, b => ConfigureContent(b, icons), true);
                for (int i = 0; i < 13; i++)
                {
                    row.Bar.AdvanceAnimation(16);
                    using (Bitmap frame = button.CaptureVisual())
                        Check(BlueGlyphs(frame) == expected[icons],
                            "Every icon frame must have exactly one intact native label: " + style + ", icons=" + icons + ", frame=" + i);
                }
            }
        }
    }

    private static void MembershipFrames()
    {
        foreach (bool icons in new[] { false, true })
        foreach (bool dark in new[] { false, true })
        using (var row = new Row())
        using (var sheet = new Bitmap(440, 570))
        using (Graphics graphics = Graphics.FromImage(sheet))
        {
            row.Bar.Width = 330;
            row.Bar.BackColor = dark ? Color.FromArgb(35, 35, 35) : Color.White;
            graphics.Clear(row.Bar.BackColor);
            TestButton a = ContentButton("Alpha"), b = ContentButton("Bravo"), c = ContentButton("Charlie");
            Action<FavouriteButton> configure = button =>
            {
                ConfigureContent(button, icons);
                button.FlatStyle = dark ? FlatStyle.Popup : FlatStyle.Flat;
                button.BackColor = row.Bar.BackColor;
                button.ForeColor = dark ? Color.FromArgb(195, 195, 195) : Color.Black;
                button.FlatAppearance.BorderSize = dark ? 1 : 0;
            };
            row.Bar.UpdateItems(new FavouriteButton[] { a, c }, configure, false);
            int insertionLeft = c.Left;
            row.Bar.UpdateItems(new FavouriteButton[] { a, b, c }, configure, true);
            int fullWidth = b.GetPreferredSize(Size.Empty).Width;
            using (var reference = ContentButton("Bravo"))
            {
                configure(reference);
                reference.AutoSize = false;
                reference.Size = new Size(fullWidth, b.Height);
                using (Bitmap full = reference.CaptureVisual())
                {
                    for (int frame = 0; frame <= 5; frame++)
                    {
                        if (frame > 0) row.Bar.AdvanceAnimation(40);
                        float t = frame / 5f, eased = 1 - (1 - t) * (1 - t);
                        Check(b.Width == (int)Math.Round(fullWidth * eased), "Add must use the tab's 200 ms quadratic width curve.");
                        Check(c.Left == insertionLeft + (int)Math.Round((fullWidth + b.Margin.Horizontal) * eased),
                            "Insertion and neighbour movement must use the same clock.");
                        if (b.Width > 2)
                        using (Bitmap actual = b.CaptureVisual())
                            for (int y = 2; y < actual.Height - 2; y++)
                                for (int x = 2; x < actual.Width - 2; x++)
                                    Check(actual.GetPixel(x, y) == full.GetPixel(x, y),
                                        "An entering title/icon must be an opaque, unscaled crop of its final face.");
                        using (Bitmap image = CaptureBar(row.Bar)) graphics.DrawImageUnscaled(image, 80, frame * 45);
                        using (var brush = new SolidBrush(dark ? Color.White : Color.Black))
                            graphics.DrawString("Add " + frame * 40, SystemFonts.DefaultFont, brush, 2, frame * 45 + 6);
                    }
                    int closingLeft = b.Left;
                    row.Bar.UpdateItems(new FavouriteButton[] { a, c }, configure, true);
                    for (int frame = 0; frame <= 5; frame++)
                    {
                        if (frame > 0) row.Bar.AdvanceAnimation(24);
                        int width = (int)Math.Round(fullWidth * Math.Exp(-frame * 24 / 38.0));
                        using (Bitmap actual = CaptureBar(row.Bar))
                        {
                            for (int y = 2; y < full.Height - 2; y++)
                                for (int x = 2; x < width - 2; x++)
                                    Check(actual.GetPixel(closingLeft + x, a.Top + y) == full.GetPixel(x, y),
                                        "A closing favourite must contract at full opacity, with one unscaled label.");
                            // Space between the contracting favourite and Charlie
                            // must be clean: no full-width fading copy underneath.
                            for (int x = closingLeft + width; x < c.Left; x++)
                                Check(actual.GetPixel(x, a.Top + a.Height / 2).ToArgb() == row.Bar.BackColor.ToArgb(),
                                    "A closing title must not paint outside its contracting bounds.");
                            graphics.DrawImageUnscaled(actual, 80, 285 + frame * 45);
                        }
                        using (var brush = new SolidBrush(dark ? Color.White : Color.Black))
                            graphics.DrawString("Close " + frame * 24, SystemFonts.DefaultFont, brush, 2, 291 + frame * 45);
                    }
                }
            }
            sheet.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "favourites-membership-" + (dark ? "dark" : "light") + (icons ? "-icons" : "-text") + ".png"));
            row.Settle();
            Check(c.Left == insertionLeft, "Closing must restore the exact original row.");
        }
    }
}
