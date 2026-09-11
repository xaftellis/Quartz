using EasyTabs;
using Quartz.Controls;
using Quartz.Services;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

internal static partial class PinnedTabsTests
{
    private static int _checks;
    private static void Check(bool condition, string message)
    {
        _checks++;
        if (!condition) throw new Exception(message);
    }

    private sealed class TestRenderer : ChromiumTabRenderer
    {
        internal double Time;
        internal float DpiScale = 1;
        internal TestRenderer(TitleBarTabs window) : base(window) { }
        protected override double AnimationTimeMilliseconds => Time;
        protected override float RenderScale => DpiScale;
        protected override bool ShouldAnimateLayout() => AnimationsEnabled;
        internal void Drag(TitleBarTab tab)
        {
            _parentWindow.SelectedTab = tab;
            _tabClickOffset = tab.Area.Width / 2;
            _tabClickOffsetY = tab.Area.Height / 2;
            IsTabRepositioning = true;
        }
        internal void Release()
        {
            Overlay_MouseUp(null, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
        }
    }

    private sealed class Window : TitleBarTabs
    {
        internal readonly TestRenderer Renderer;
        internal Window()
        {
            AeroPeekEnabled = false;
            ExitOnLastTabClose = false;
            ShowInTaskbar = false;
            ClientSize = new Size(1200, 500);
            Renderer = new TestRenderer(this);
            TabRenderer = Renderer;
        }
        public override TitleBarTab CreateTab() => New("New");
        internal TitleBarTab New(string title, bool pinned = false, bool cancelClose = false)
        {
            var content = new Form { Text = title, Icon = SystemIcons.Information };
            if (cancelClose) content.FormClosing += (s, e) => e.Cancel = true;
            var tab = new TitleBarTab(this) { Content = content, IsPinned = pinned };
            IntPtr handle = content.Handle;
            return tab;
        }
        internal TitleBarTab Add(string title, bool pinned = false, bool cancelClose = false)
        {
            var tab = New(title, pinned, cancelClose);
            Tabs.Add(tab);
            if (SelectedTab == null) SelectedTab = tab;
            return tab;
        }
        internal Bitmap Frame(double time, Point? cursor = null)
        {
            Renderer.Time = time;
            var image = new Bitmap(ClientSize.Width, Renderer.TabHeight);
            using (Graphics graphics = Graphics.FromImage(image))
                Renderer.Render(Tabs, graphics, Point.Empty, cursor ?? new Point(-100, -100));
            return image;
        }
        internal new void Paint(double time, Point? cursor = null) { using (Frame(time, cursor)) { } }
        internal string Order => string.Join(",", Tabs.Select(tab => tab.Caption));
    }

    [STAThread]
    private static int Main()
    {
        // Read no real profile settings and launch no WebView2 instances.
        typeof(SettingsService).GetField("_jsonPath", BindingFlags.Static | BindingFlags.NonPublic)
            .SetValue(null, Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json"));
        try
        {
            OrderingAndSelection();
            LayoutAndFrames();
            ReversalsAndReducedMotion();
            DragAndTransfer();
            PinnedFallbackIcon();
            MenusAndClosing();
            WindowTransfers();
            WindowMoveMenus();
            Console.WriteLine("PASS: " + _checks + " tab checks.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    private static void OrderingAndSelection()
    {
        using (var window = new Window())
        {
            var a = window.Add("A"); var b = window.Add("B"); var c = window.Add("C");
            window.SelectedTab = b;
            c.IsPinned = true;
            Check(window.Order == "C,A,B" && window.SelectedTab == b, "Pin background tab without changing selection.");
            b.IsPinned = true;
            Check(window.Order == "C,B,A" && window.SelectedTab == b, "Pin selected tab at end of pinned prefix.");
            c.IsPinned = false;
            Check(window.Order == "B,C,A" && window.SelectedTab == b, "Unpin to start of normal tabs.");
            b.IsPinned = true;
            Check(window.Order == "B,C,A", "Repeated pin is a no-op.");
            var normal = window.New("Normal");
            window.Tabs.Insert(0, normal); window.SelectedTab = normal;
            Check(window.Order == "B,Normal,C,A" && window.SelectedTab == normal, "Normal insertion respects pinned prefix and selection.");
            var pin = window.New("Pin", true);
            window.Tabs.Add(pin);
            Check(window.Order == "B,Pin,Normal,C,A", "Adding pre-pinned tab clamps its insertion.");
            window.Tabs.AddRange(new[] { window.New("D"), window.New("E", true), window.New("F") });
            Check(window.PinnedTabCount == 3 && window.Tabs.Take(3).All(tab => tab.IsPinned) &&
                window.Tabs.Skip(3).All(tab => !tab.IsPinned), "Mixed range maintains pinned prefix.");
            Check(window.Tabs.All(tab => !tab.Content.IsDisposed), "Pinning keeps original content alive.");
        }
        using (var fresh = new Window()) Check(!fresh.Add("Fresh").IsPinned, "Pin state is session-only.");
    }

    private static void LayoutAndFrames()
    {
        foreach (float scale in new[] { 1f, 1.25f, 1.5f, 2f })
        foreach (bool dark in new[] { false, true })
        using (var window = new Window())
        {
            window.ClientSize = new Size(1800, 500);
            window.Renderer.DpiScale = scale;
            if (dark) window.Renderer.Theme = new ChromiumTabTheme(Color.FromArgb(88, 88, 88), Color.FromArgb(35, 35, 35),
                activeForeground: Color.LightGray, inactiveForeground: Color.LightGray);
            var a = window.Add("Alpha"); var b = window.Add("Bravo"); var c = window.Add("Charlie");
            window.SelectedTab = c;
            window.Paint(0);
            Rectangle from = c.Area, neighbour = a.Area;
            Point formerClose = new Point(c.Area.X + c.CloseButtonArea.X + 4, c.Area.Y + c.CloseButtonArea.Y + 4);
            c.IsPinned = true;
            Check(!window.Renderer.IsOverCloseButton(c, formerClose), "Pin disables stale close hit area before the next frame.");
            window.Paint(0);
            Check(c.Area == from && a.Area == neighbour, "Pin first frame must retain displayed bounds.");
            Check(c.CloseButtonArea.IsEmpty, "Pin immediately removes close hit target.");
            using (var sheet = new Bitmap(window.ClientSize.Width, window.Renderer.TabHeight * 6))
            using (Graphics graphics = Graphics.FromImage(sheet))
            {
                for (int i = 0; i <= 5; i++)
                using (Bitmap frame = window.Frame(i * 40))
                {
                    float eased = 1 - (1 - i / 5f) * (1 - i / 5f);
                    int pinnedWidth = ChromiumTabMetrics.Pixel(55 * scale);
                    Check(c.Area.Width == (int)Math.Round(from.Width + (pinnedWidth - from.Width) * eased), "Pin width follows 200 ms ease-out.");
                    Check(c.Area.X == (int)Math.Round(from.X + (neighbour.X - from.X) * eased), "Pin position follows same clock.");
                    Check(c.CloseButtonArea.IsEmpty, "No close target during contraction.");
                    graphics.DrawImageUnscaled(frame, 0, i * window.Renderer.TabHeight);
                }
                if (scale == 1) sheet.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pinned-tabs-" + (dark ? "dark" : "light") + ".png"));
            }
            Check(c.Area.Width == ChromiumTabMetrics.Pixel(55 * scale), "Fixed pinned width at each display scale.");
            Point center = new Point(c.Area.X + c.Area.Width / 2, c.Area.Y + c.Area.Height / 2);
            Check(window.Renderer.OverTab(window.Tabs, center) == c && !window.Renderer.IsOverCloseButton(c, center), "Pin hit test selects its tab, not close.");
            window.SelectedTab = a;
            window.Paint(220);
            Check(c.Area.Width == ChromiumTabMetrics.Pixel(55 * scale), "Inactive pinned width stays fixed.");
            window.ClientSize = new Size(620, 500);
            window.Paint(240); window.Paint(440);
            Check(c.Area.Width == ChromiumTabMetrics.Pixel(55 * scale), "Crowded strip keeps pinned width.");
            c.IsLoading = true; window.Paint(460);
            Check(c.IsLoading && c.CloseButtonArea.IsEmpty, "Loading pinned tab renders without a close button.");
            c.IsLoading = false; window.Paint(740);
        }
        int[] widths = ChromiumTabMetrics.LayoutWidths(3, 3, 1, 30, 1);
        Check(widths.All(width => width == 55), "All-pinned overflow retains fixed widths.");
    }

    private static void ReversalsAndReducedMotion()
    {
        using (var window = new Window())
        {
            var a = window.Add("A"); var b = window.Add("B"); window.Paint(0);
            b.IsPinned = true; window.Paint(80);
            Rectangle intermediate = b.Area;
            b.IsPinned = false; window.Paint(80);
            Check(b.Area == intermediate, "Rapid unpin starts from displayed bounds without jumping.");
            window.Paint(280);
            Check(b.Area.Width == a.Area.Width && !b.CloseButtonArea.IsEmpty, "Unpin expands and restores close button.");
            b.IsPinned = true; window.Paint(330);
            window.Renderer.AnimationsEnabled = false; window.Paint(330);
            Check(b.Area.Width == 55 && b.Area.Left < a.Area.Left, "Reduced motion finishes pending pin immediately.");
            b.IsPinned = false; window.Paint(331);
            Check(b.Area.Width == 256, "Reduced-motion unpin snaps to normal width.");
        }
    }

    private static void DragAndTransfer()
    {
        using (var window = new Window())
        using (var target = new Window())
        {
            var a = window.Add("A", true); var b = window.Add("B", true);
            var c = window.Add("C"); var d = window.Add("D"); window.Paint(0);
            window.Renderer.Drag(a); window.Paint(20, new Point(700, 20));
            Check(window.Order == "B,A,C,D", "Pinned drag reorders only within pinned tabs.");
            Check(a.Area.X == 700 - 55 / 2 && a.Area.Right > d.Area.Right,
                "Pinned visual follows the pointer across normal tabs.");
            for (int i = 1; i <= 15; i++) window.Paint(20 + i * 16, new Point(700, 20));
            Check(window.Order == "B,A,C,D", "Stationary drag is stable during neighbour animation.");
            int releasedX = a.Area.X;
            double releasedAt = window.Renderer.Time;
            window.Renderer.Release(); window.Paint(releasedAt);
            Check(a.Area.X == releasedX, "Release does not snap the pinned visual.");
            window.Paint(releasedAt + 100);
            int legalX = b.Area.Left + 55 - 17;
            Check(a.Area.X == (int)Math.Round(releasedX + (legalX - releasedX) * .75), "Pin returns with the 200 ms ease-out curve.");
            window.Paint(releasedAt + 200);
            Check(a.Area.X == legalX && a.IsPinned, "Pin settles into its legal slot without unpinning.");
            window.Paint(500);
            window.Renderer.Drag(d); window.Paint(520, new Point(-100, 20));
            Check(window.Order == "B,A,D,C", "Normal drag cannot enter pinned section.");
            Check(d.Area.Left == b.Area.Left, "Normal visual follows pointer over the pinned section.");
            releasedX = d.Area.X;
            window.Renderer.Release(); window.Paint(520);
            Check(d.Area.X == releasedX, "Normal tab release does not snap at the pinned boundary.");
            window.Paint(620);
            Check(d.Area.X > releasedX && d.Area.X < b.Area.Left + 2 * (55 - 17), "Normal visual animates back out of the pinned section.");
            window.Paint(720);
            Check(d.Area.X == b.Area.Left + 2 * (55 - 17) && !d.IsPinned, "Normal tab settles after pins without changing state.");
            target.Add("TargetPin", true); target.Add("TargetNormal"); target.Paint(0);
            a.ClearSubscriptions(); window.Tabs.Remove(a);
            target.Renderer.CombineTab(a, new Point(1100, 20), new PointF(.5f, .5f));
            target.Paint(20, new Point(1100, 20));
            Check(a.IsPinned && a.Parent == target && target.SelectedTab == a && target.Tabs.IndexOf(a) < target.PinnedTabCount,
                "Pinned transfer preserves pin state and selects actual inserted tab.");
            target.Renderer.Release();
            d.ClearSubscriptions(); window.Tabs.Remove(d);
            target.Renderer.CombineTab(d, new Point(10, 20), new PointF(.5f, .5f));
            target.Paint(40, new Point(10, 20));
            Check(!d.IsPinned && target.SelectedTab == d && target.Tabs.IndexOf(d) >= target.PinnedTabCount,
                "Normal transfer cannot displace pins and selects actual inserted tab.");
        }
        using (var window = new Window())
        {
            window.ClientSize = new Size(620, 500);
            window.Add("PinA", true); window.Add("PinB", true);
            var active = window.Add("Active");
            for (int i = 0; i < 20; i++) window.Add("Tab" + i);
            window.SelectedTab = active; window.Paint(0);
            window.Renderer.Drag(active);
            var pointer = new Point(138, 20);
            window.Paint(20, pointer);
            string order = window.Order;
            for (int i = 1; i <= 20; i++)
            {
                window.Paint(20 + i * 16, pointer);
                Check(window.Order == order, "Crowded strip's wider active tab must not keep reordering under a stationary pointer.");
            }
        }
    }

    private static bool SamePixels(Bitmap a, Bitmap b)
    {
        if (a.Size != b.Size) return false;
        for (int y = 0; y < a.Height; y++)
            for (int x = 0; x < a.Width; x++)
                if (a.GetPixel(x, y) != b.GetPixel(x, y)) return false;
        return true;
    }

    private static void PinnedFallbackIcon()
    {
        foreach (bool dark in new[] { false, true })
        using (var window = new Window())
        using (var reference = new Window())
        {
            Icon fallback = dark ? Quartz.Properties.Resources.default_favicon_dark : Quartz.Properties.Resources.default_favicon;
            window.Renderer.DefaultFavicon = reference.Renderer.DefaultFavicon = fallback;
            if (dark) window.Renderer.Theme = reference.Renderer.Theme = new ChromiumTabTheme(Color.FromArgb(88, 88, 88),
                Color.FromArgb(35, 35, 35), activeForeground: Color.LightGray, inactiveForeground: Color.LightGray);
            var hidden = window.Add("Hidden icon");
            Icon original = hidden.Icon;
            hidden.Content.ShowIcon = false;
            var expected = reference.Add("Hidden icon", true);
            expected.Icon = fallback;
            using (Bitmap originalFrame = window.Frame(0))
            {
                hidden.IsPinned = true;
                using (Bitmap pinned = window.Frame(200))
                using (Bitmap expectedFrame = reference.Frame(200))
                {
                    Check(SamePixels(pinned, expectedFrame), "Hidden-icon pin paints the real default favicon in " + (dark ? "dark" : "light") + " theme.");
                    Check(!hidden.Content.ShowIcon && ReferenceEquals(hidden.Icon, original), "Fallback never overwrites the underlying icon or visibility.");
                    using (var sheet = new Bitmap(pinned.Width, pinned.Height * 3))
                    using (Graphics graphics = Graphics.FromImage(sheet))
                    {
                        graphics.DrawImageUnscaled(originalFrame, 0, 0);
                        graphics.DrawImageUnscaled(pinned, 0, pinned.Height);
                        hidden.IsPinned = false;
                        window.Paint(400);
                        using (Bitmap unpinned = window.Frame(600))
                        {
                            Check(SamePixels(originalFrame, unpinned), "Unpin restores the original iconless appearance exactly.");
                            graphics.DrawImageUnscaled(unpinned, 0, pinned.Height * 2);
                        }
                        sheet.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pinned-fallback-" + (dark ? "dark" : "light") + ".png"));
                    }
                }
            }
            hidden.IsPinned = true; window.Paint(800);
            hidden.Content.ShowIcon = true;
            hidden.Icon = expected.Icon = SystemIcons.Warning;
            using (Bitmap actual = window.Frame(820))
            using (Bitmap expectedFrame = reference.Frame(820))
                Check(SamePixels(actual, expectedFrame), "A real favicon arriving while pinned replaces the fallback.");
            hidden.IsLoading = expected.IsLoading = true;
            window.Paint(840); reference.Paint(840);
            hidden.Content.ShowIcon = false;
            hidden.IsLoading = expected.IsLoading = false;
            expected.Icon = fallback;
            window.Paint(900); reference.Paint(900);
            using (Bitmap actual = window.Frame(1300))
            using (Bitmap expectedFrame = reference.Frame(1300))
                Check(SamePixels(actual, expectedFrame), "Default favicon remains visible after pinned navigation completes.");
        }
        using (var window = new Window())
        {
            var pin = window.Add("Pin", true); var normal = window.Add("Normal");
            window.Paint(0); window.Renderer.Drag(normal); window.Paint(20, new Point(0, 20));
            window.Renderer.AnimationsEnabled = false;
            window.Renderer.Release(); window.Paint(20);
            Check(normal.Area.Left == pin.Area.Left + 55 - 17, "Reduced-motion release returns immediately to the legal slot.");
        }
    }

    private static void MenuAction(TabContextMenu menu, string method) =>
        typeof(TabContextMenu).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(menu, new object[] { null, EventArgs.Empty });

    private static void MenusAndClosing()
    {
        foreach (string command in new[] { "CloseOther", "CloseLeft", "CloseRight" })
        using (var window = new Window())
        using (var menu = new TabContextMenu())
        {
            var pin = window.Add("Pin", true); var left = window.Add("Left"); var clicked = window.Add("Clicked");
            var right = window.Add("Right"); var cancelled = window.Add("Cancelled", false, true);
            ContextMenuProvider._parentForm = window; ContextMenuProvider._clickedTab = clicked;
            menu.DefineVarables(); menu.UpdateMenuItemsEnabledState();
            MenuAction(menu, command + "ToolStripMenuItem_Click");
            Check(window.Tabs.Contains(pin) && !pin.Content.IsDisposed, command + " preserves pinned tab.");
            Check(window.Tabs.Contains(cancelled) && !cancelled.Content.IsDisposed, command + " honours close cancellation.");
            Check(window.SelectedTab == clicked, command + " preserves clicked selection.");
            Check(command == "CloseRight" || left.Content.IsDisposed, command + " disposes closed left content.");
            Check(command == "CloseLeft" || right.Content.IsDisposed, command + " disposes closed right content.");
            pin.Content.Close();
            Check(!window.Tabs.Contains(pin) && pin.Content.IsDisposed, "Explicitly closing a pinned tab still works.");
        }
        using (var window = new Window())
        using (var menu = new TabContextMenu())
        {
            var a = window.Add("A", true); var b = window.Add("B", true);
            ContextMenuProvider._parentForm = window; ContextMenuProvider._clickedTab = b;
            menu.DefineVarables(); menu.UpdateMenuItemsEnabledState();
            Check(menu.Items.OfType<ToolStripMenuItem>().Where(item => item.Text.StartsWith("Close ") && item.Text != "Close Tab")
                .All(item => !item.Enabled), "Bulk close actions disabled when only pins remain.");
            var opening = typeof(TabContextMenu).GetMethod("DefaultContextMenu_Opening", BindingFlags.Instance | BindingFlags.NonPublic);
            opening.Invoke(menu, new object[] { null, new System.ComponentModel.CancelEventArgs() });
            var pinItem = menu.Items.OfType<ToolStripMenuItem>().Single(item => item.Text == "Unpin");
            pinItem.PerformClick();
            Check(!b.IsPinned, "Menu toggles pin state.");
            opening.Invoke(menu, new object[] { null, new System.ComponentModel.CancelEventArgs() });
            Check(pinItem.Text == "Pin", "Menu label follows current pin state.");
        }
        ContextMenuProvider._parentForm = null; ContextMenuProvider._clickedTab = null;
    }
}
