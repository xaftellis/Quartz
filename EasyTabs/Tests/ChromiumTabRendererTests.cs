using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using EasyTabs;

internal static class ChromiumTabRendererTests
{
    private static int checks;
    private static readonly PropertyInfo AreaProperty = typeof(TitleBarTab).GetProperty("Area", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly PropertyInfo CloseProperty = typeof(TitleBarTab).GetProperty("CloseButtonArea", BindingFlags.Instance | BindingFlags.NonPublic);
    private static Rectangle Area(TitleBarTab tab) => (Rectangle)AreaProperty.GetValue(tab);
    private static Rectangle Close(TitleBarTab tab) => (Rectangle)CloseProperty.GetValue(tab);
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception("FAIL: " + message);
        checks++;
    }

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            Control.CheckForIllegalCrossThreadCalls = true;
            string output = args.Length > 0 ? args[0] : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "chromium-test-artifacts");
            Directory.CreateDirectory(output);
            foreach (float scale in new[] { 1f, 1.25f, 1.5f, 2f, 3f })
            {
                TestGeometry(scale);
                TestRenderer(scale, output);
            }
            Console.WriteLine("PASS: " + checks + " Chromium geometry, layout, painting and interaction checks.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    private static void TestGeometry(float scale)
    {
        int width = ChromiumTabMetrics.Pixel(256 * scale), height = ChromiumTabMetrics.Pixel(35 * scale);
        using (var normal = new ChromiumTabGeometry(width, height, scale, 0, false, true))
        using (var maximized = new ChromiumTabGeometry(width, height, scale, 0, true, true))
        {
            Check(normal.HitTest.Contains(128 * scale, 16 * scale), "tab body is clickable at " + scale);
            Check(!normal.HitTest.Contains(1 * scale, 1 * scale), "rounded top-left leaves caption space");
            Check(maximized.HitTest.Contains(1 * scale, 1 * scale), "maximized first tab reaches the leading screen edge");
            Check(normal.Fill.Contains(1 * scale, height - .1f), "bottom toe overlaps toolbar");
            Check(!normal.Fill.Contains(-1, 16), "fill stays outside negative x");
        }
        for (int logicalWidth = 32; logicalWidth < 256; logicalWidth += 11)
        using (var narrow = new ChromiumTabGeometry(ChromiumTabMetrics.Pixel(logicalWidth * scale), height, scale, 0, false, false))
            Check(narrow.Fill.Contains(logicalWidth * scale / 2, 10 * scale), "narrow tab retains flat clickable centre");
        int[] widths = ChromiumTabMetrics.LayoutWidths(3, 1, ChromiumTabMetrics.Pixel(735 * scale), scale);
        Check(widths.Sum() - 2 * ChromiumTabMetrics.Pixel(17 * scale) == ChromiumTabMetrics.Pixel(735 * scale), "fractional scale width allocation fits available space");
    }

    private static void TestRenderer(float scale, string output)
    {
        int pixelWidth = ChromiumTabMetrics.Pixel(1100 * scale);
        using (var host = new TestHost(pixelWidth))
        using (var renderer = new ProbeRenderer(host, scale))
        using (var bitmap = new Bitmap(pixelWidth, ChromiumTabMetrics.Pixel(90 * scale)))
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            host.TabRenderer = renderer;
            renderer.AnimationsEnabled = false;
            for (int i = 0; i < 3; i++) host.Tabs.Add(host.CreateTab());
            host.SelectedTabIndex = 1;
            Point cursor = new Point(-100, -100);
            Action paint = () => renderer.Render(host.Tabs, graphics, Point.Empty, cursor, true);
            paint();
            for (int i = 0; i < host.Tabs.Count; i++)
            {
                TitleBarTab tab = host.Tabs[i]; Rectangle bounds = Area(tab);
                Check(bounds.Width == ChromiumTabMetrics.Pixel(256 * scale), "full drawing bounds are 256 DIP");
                Check(bounds.Height == ChromiumTabMetrics.Pixel(35 * scale), "tab bounds are 35 DIP");
                if (i > 0) Check(Area(host.Tabs[i - 1]).Right - bounds.Left == renderer.OverlapWidth, "overlap is 17 DIP");
                Point centre = new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
                Check(renderer.OverTab(host.Tabs, centre) == tab, "visible tab maps to its model");
                Rectangle close = Close(tab);
                Check(renderer.IsOverCloseButton(tab, new Point(bounds.X + close.X + close.Width / 2, bounds.Y + close.Y + close.Height / 2)), "close glyph has correct mouse target");
                var image = typeof(TitleBarTab).GetProperty("TabImage", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tab);
                Check(image == null, "vector renderer does not allocate tab artwork images");
            }
            object buffer = renderer.PixelBuffer;
            paint();
            Check(ReferenceEquals(buffer, renderer.PixelBuffer), "repaints reuse native backing storage");
            Check(renderer.IsOverAddButton(renderer.AddCentre), "new-tab button is clickable");
            bitmap.Save(Path.Combine(output, "tabs-" + scale.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + ".png"), ImageFormat.Png);

            cursor = new Point(Area(host.Tabs[0]).X + 70, Area(host.Tabs[0]).Y + 18);
            paint();
            Check(bitmap.GetPixel(cursor.X, Area(host.Tabs[0]).Y + 3).ToArgb() != renderer.Theme.Frame.ToArgb(), "hover changes background without moving the tab");
            renderer.AnimationsEnabled = true;
            TitleBarTab dragged = host.SelectedTab;
            renderer.BeginDrag(25);
            cursor = new Point(Area(host.Tabs[2]).X + 70, Area(dragged).Y + 18);
            paint();
            Check(Area(dragged).X == cursor.X - 25, "dragged tab remains attached to pointer");
            Check(host.Tabs.IndexOf(dragged) == 2, "drag reorders tab models");
            Check(renderer.OverTab(host.Tabs, cursor) == dragged, "dragged tab wins overlap hit testing");
            renderer.EndDrag();
            cursor = new Point(-100, -100);
            Settle(renderer, paint);
            host.Tabs.Add(host.CreateTab());
            paint();
            Check(renderer.Moving, "opening tab schedules animation frames");
            Settle(renderer, paint);
            TitleBarTab removed = host.Tabs[0];
            host.Tabs.Remove(removed); removed.Content.Dispose();
            Settle(renderer, paint);

            renderer.AnimationsEnabled = false;
            for (int i = host.Tabs.Count; i < 18; i++) host.Tabs.Add(host.CreateTab());
            paint();
            Check(!Close(host.SelectedTab).IsEmpty, "crowded active tab retains its close button");
            Check(host.Tabs.Where(t => !t.Active).All(t => Close(t).IsEmpty), "crowded inactive tabs hide close buttons");
            bitmap.Save(Path.Combine(output, "crowded-" + scale.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + ".png"), ImageFormat.Png);
            renderer.Theme = new ChromiumTabTheme(Color.FromArgb(30, 36, 46), Color.FromArgb(62, 71, 87));
            paint();
            Check(bitmap.GetPixel(2, 2).ToArgb() == renderer.Theme.Frame.ToArgb(), "colour theme updates the frame without assets");
            renderer.Theme = ChromiumTabTheme.Light;
            var clock = Stopwatch.StartNew();
            for (int frame = 0; frame < 60; frame++) paint();
            Console.WriteLine("Scale " + scale + ": 18-tab paint average " + (clock.Elapsed.TotalMilliseconds / 60).ToString("F2") + " ms");
            renderer.ShowAddButton = false; paint();
            Check(!renderer.IsOverAddButton(renderer.AddCentre), "hidden add button is not clickable");
        }
    }

    private static void Settle(ProbeRenderer renderer, Action paint)
    {
        paint();
        for (int i = 0; i < 90 && renderer.Moving; i++) { Thread.Sleep(16); paint(); }
        Check(!renderer.Moving, "animations settle and stop requesting frames");
    }

    private sealed class TestHost : TitleBarTabs
    {
        public TestHost(int width) { AeroPeekEnabled = false; ExitOnLastTabClose = false; ClientSize = new Size(width, 500); }
        public override TitleBarTab CreateTab() => new TitleBarTab(this)
        {
            Content = new Form { Text = "Tab " + (Tabs.Count + 1) + " — Quartz", ShowIcon = true, Icon = SystemIcons.Application }
        };
    }
    private sealed class ProbeRenderer : ChromiumTabRenderer
    {
        private readonly float scale;
        internal ProbeRenderer(TitleBarTabs parent, float scale) : base(parent) { this.scale = scale; }
        protected override float RenderScale => scale;
        internal Point AddCentre => new Point(_addButtonArea.X + _addButtonArea.Width / 2, _addButtonArea.Y + _addButtonArea.Height / 2);
        internal bool Moving => (bool)typeof(ChromiumTabRenderer).GetProperty("IsLayoutAnimating", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
        internal object PixelBuffer => typeof(ChromiumTabRenderer).GetField("_pixels", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
        internal void BeginDrag(int offset) { _isTabRepositioning = true; _tabClickOffset = offset; }
        internal void EndDrag() { _isTabRepositioning = false; _tabClickOffset = null; }
    }
}
