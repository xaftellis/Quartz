using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using EasyTabs;

internal static class TabAnimationTests
{
    private static int _checks;
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            TestTiming();
            TestRenderer(args.Length == 0 ? null : args[0]);
            Console.WriteLine("PASS: " + _checks + " animation and renderer checks.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    private static void Check(bool pass, string name)
    {
        if (!pass) throw new Exception("FAIL: " + name);
        _checks++;
    }

    private static void TestTiming()
    {
        var animation = new TabLayoutAnimation();
        object tab = new object(), added = new object();
        Rectangle original = new Rectangle(10, 8, 220, 30);
        Rectangle target = new Rectangle(180, 8, 150, 30);
        animation.BeginFrame(new[] { tab }, 0, true);
        Check(animation.GetBounds(tab, original, false, 40) == original, "initial layout does not animate");
        Check(!animation.IsAnimating, "initial layout is idle");
        animation.BeginFrame(new[] { tab }, 5000, true);
        Check(animation.GetBounds(tab, target, false, 40) == original, "retarget after idle starts at displayed bounds");
        animation.BeginFrame(new[] { tab }, 5016, true);
        Rectangle moving = animation.GetBounds(tab, target, false, 40);
        Check(moving.X > original.X && moving.X < target.X && moving.Width < original.Width, "position and width ease together");
        animation.BeginFrame(new[] { tab }, 5016, true);
        Check(animation.GetBounds(tab, target, false, 40) == moving, "extra paints at same timestamp do not accelerate motion");
        for (int i = 2; i < 35; i++)
        {
            animation.BeginFrame(new[] { tab }, 5000 + i * 16, true);
            Rectangle next = animation.GetBounds(tab, target, false, 40);
            Check(next.X >= moving.X && next.X <= target.X, "easing does not overshoot");
            moving = next;
        }
        Check(moving == target && !animation.IsAnimating, "animation reaches exact layout and stops");
        animation.BeginFrame(new[] { tab, added }, 6000, true);
        animation.GetBounds(tab, target, false, 40);
        Check(animation.GetBounds(added, original, false, 40).Width == 40, "new tab grows from its edge width");
        animation.BeginFrame(new[] { added }, 6016, true);
        Check(animation.Count == 1, "removed tabs are released from animation state");
        Check(animation.GetBounds(added, target, true, 40) == target, "dragged tab follows pointer immediately");
        animation.BeginFrame(new[] { added }, 6032, false);
        Check(animation.GetBounds(added, original, false, 40) == original && !animation.IsAnimating, "disabled motion snaps to layout");
        animation.BeginFrame(new[] { added }, 6048, true);
        Rectangle maximized = new Rectangle(0, 0, 300, 38);
        Check(animation.GetBounds(added, maximized, false, 40) == maximized, "frame height changes snap");
        animation.Reset();
        Check(animation.Count == 0 && !animation.IsAnimating, "reset releases state and stops animation");
    }

    private static void TestRenderer(string output)
    {
        // Never Show the host: no overlay, mouse hook, browser profile, or desktop input.
        using (var host = new TestHost())
        using (var bitmap = new Bitmap(1200, 60))
        using (var graphics = Graphics.FromImage(bitmap))
        {
            var renderer = new RecordingRenderer(host);
            host.TabRenderer = renderer;
            for (int i = 0; i < 3; i++) host.Tabs.Add(host.CreateTab());
            host.SelectedTabIndex = 0;
            Action paint = () => renderer.Render(host.Tabs, graphics, Point.Empty, new Point(-100, -100), false);
            paint();
            Check(renderer.Areas.Count == 3, "original renderer draws every tab");
            if (output != null)
            {
                Directory.CreateDirectory(output);
                bitmap.Save(Path.Combine(output, "original-tabs.png"), ImageFormat.Png);
            }
            host.Tabs.Add(host.CreateTab());
            paint();
            Rectangle newStart = renderer.Areas[host.Tabs.Last()];
            Settle(renderer, paint);
            Rectangle newEnd = renderer.Areas[host.Tabs.Last()];
            Check(newStart.Width <= newEnd.Width, "opening tab grows without altering its artwork");
            var settled = new Dictionary<TitleBarTab, Rectangle>(renderer.Areas);
            renderer.AnimationsEnabled = false;
            paint();
            Check(settled.All(pair => renderer.Areas[pair.Key] == pair.Value), "settled geometry equals original instantaneous layout");
            renderer.AnimationsEnabled = true;
            TitleBarTab dragged = host.Tabs[0];
            Point cursor = new Point(renderer.Areas[host.Tabs[2]].X + 50, renderer.Areas[dragged].Y + 15);
            renderer.StartDrag(30);
            renderer.Render(host.Tabs, graphics, Point.Empty, cursor, true);
            Check(host.Tabs.IndexOf(dragged) > 0, "existing reorder logic still runs");
            Check(renderer.Areas[dragged].X == cursor.X - 30, "held tab stays exactly under pointer");
            Check(renderer.Drawn.Count == host.Tabs.Count && renderer.Drawn.Distinct().Count() == host.Tabs.Count,
                "reorder frame draws each tab once");
            Check(renderer.Drawn.Last() == dragged, "selected tab is drawn on top after reindexing");
            Rectangle draggedArea = renderer.Areas[dragged];
            Check(renderer.OverTab(host.Tabs, new Point(draggedArea.X + draggedArea.Width / 2, draggedArea.Y + 15)) == dragged,
                "hit testing uses animated rectangle");
            renderer.EndDrag();
            Settle(renderer, paint);
            TitleBarTab removed = host.Tabs[1];
            host.Tabs.Remove(removed);
            removed.Content.Dispose();
            Settle(renderer, paint);
            Check(renderer.Areas.Count == host.Tabs.Count && !renderer.Areas.ContainsKey(removed), "closing tab relayout excludes removed content");
            if (output != null) bitmap.Save(Path.Combine(output, "settled-tabs.png"), ImageFormat.Png);
            var bitmapProperty = typeof(TitleBarTab).GetProperty("TabImage", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (TitleBarTab tab in host.Tabs)
            {
                Rectangle area = renderer.Areas[tab];
                using (Bitmap image = (Bitmap)bitmapProperty.GetValue(tab, null))
                    Check(image.Width == area.Width && image.Height == area.Height, "cached artwork matches displayed hit area");
            }
        }
    }

    private static void Settle(RecordingRenderer renderer, Action paint)
    {
        paint();
        for (int i = 0; i < 60 && renderer.Moving; i++) { Thread.Sleep(16); paint(); }
        Check(!renderer.Moving, "renderer finishes its transition");
    }

    private sealed class TestHost : TitleBarTabs
    {
        public TestHost()
        {
            AeroPeekEnabled = false;
            ExitOnLastTabClose = false;
            ClientSize = new Size(1100, 500);
        }
        public override TitleBarTab CreateTab() => new TitleBarTab(this)
        {
            Content = new Form { Text = "Tab " + (Tabs.Count + 1), ShowIcon = false }
        };
    }

    private sealed class RecordingRenderer : ChromeTabRenderer
    {
        internal readonly Dictionary<TitleBarTab, Rectangle> Areas = new Dictionary<TitleBarTab, Rectangle>();
        internal readonly List<TitleBarTab> Drawn = new List<TitleBarTab>();
        internal bool Moving => (bool)typeof(BaseTabRenderer).GetProperty("IsLayoutAnimating",
            BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this, null);
        internal RecordingRenderer(TitleBarTabs host) : base(host) { }
        internal void StartDrag(int grabOffset) { _isTabRepositioning = true; _tabClickOffset = grabOffset; }
        internal void EndDrag() { _isTabRepositioning = false; _tabClickOffset = null; }
        public override void Render(List<TitleBarTab> tabs, Graphics graphics, Point offset, Point cursor, bool forceRedraw = false)
        {
            Areas.Clear(); Drawn.Clear();
            base.Render(tabs, graphics, offset, cursor, forceRedraw);
        }
        protected override void Render(Graphics graphics, TitleBarTab tab, int index, Rectangle area, Point cursor,
            Image left, Image centre, Image right)
        {
            base.Render(graphics, tab, index, area, cursor, left, centre, right);
            Areas[tab] = area; Drawn.Add(tab);
        }
    }
}
