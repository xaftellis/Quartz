using System;
using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using EasyTabs;

// Deterministic checks against the built EasyTabs assembly. Hidden HWND fixtures
// send native messages without moving the user's pointer or opening browser pages.
static class TabAnimationChecks
{
    const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    static int checks, failures;
    static readonly Assembly Assembly = typeof(ChromiumTabRenderer).Assembly;

    internal static object Field(object obj, string name)
    {
        for (Type t = obj.GetType(); t != null; t = t.BaseType)
        {
            FieldInfo field = t.GetField(name, Flags);
            if (field != null) return field.GetValue(obj);
        }
        throw new MissingFieldException(name);
    }
    internal static void Set(object obj, string name, object value)
    {
        for (Type t = obj.GetType(); t != null; t = t.BaseType)
        {
            FieldInfo field = t.GetField(name, Flags);
            if (field != null) { field.SetValue(obj, value); return; }
        }
        throw new MissingFieldException(name);
    }
    internal static object Call(object obj, string name, params object[] args)
    {
        Type type = obj as Type ?? obj.GetType();
        for (Type t = type; t != null; t = t.BaseType)
            foreach (MethodInfo method in t.GetMethods(Flags | BindingFlags.DeclaredOnly))
                if (method.Name == name && method.GetParameters().Length == args.Length)
                    return method.Invoke(obj is Type ? null : obj, args);
        throw new MissingMethodException(name);
    }
    static object New(string name, params object[] args) =>
        Activator.CreateInstance(Assembly.GetType("EasyTabs." + name, true), Flags, null, args, null);
    internal static object Property(object obj, string name) => obj.GetType().GetProperty(name, Flags).GetValue(obj, null);
    static double Number(object obj, string name) => Convert.ToDouble(Property(obj, name));
    static bool Running(object obj) => (bool)Property(obj, "IsAnimating");
    static void Require(bool value, string message)
    {
        ++checks;
        if (!value) throw new Exception(message);
    }
    static void Near(double actual, double expected, string message, double tolerance = .00001) =>
        Require(Math.Abs(actual - expected) <= tolerance, message + ": expected " + expected + ", got " + actual);
    static void Test(string name, Action action)
    {
        try { action(); Console.WriteLine("PASS " + name); }
        catch (Exception error)
        {
            ++failures;
            while (error is TargetInvocationException && error.InnerException != null) error = error.InnerException;
            Console.WriteLine("FAIL " + name + ": " + error);
        }
    }
    static void Hover(object animation, bool hovered, double now, bool animate = true) =>
        Call(animation, "Update", hovered, now, animate);

    static void HoverChecks()
    {
        object a = New("ChromiumHoverAnimation", 200d, true);
        Hover(a, false, 10000);
        Hover(a, true, 10000);
        Near(Number(a, "Value"), 0, "No advance from preceding idle time");
        Hover(a, true, 10050); Near(Number(a, "Value"), .4375, "Entry quarter time");
        Hover(a, true, 10100); Near(Number(a, "Value"), .75, "Entry midpoint ease-out");
        Hover(a, true, 10199); Require(Running(a), "Entry keeps its full lifetime");
        Hover(a, true, 10200); Near(Number(a, "Value"), 1, "Entry completes at 200 ms");
        Require(!Running(a), "Entry idles");
        Hover(a, false, 10250); Hover(a, false, 10350);
        Near(Number(a, "Value"), .75, "Exit midpoint ease-in");
        Hover(a, false, 10450); Near(Number(a, "Value"), 0, "Exit completes at 200 ms");
        Require(!Running(a), "Exit idles");

        a = New("ChromiumHoverAnimation", 200d, true);
        Hover(a, true, 0); Hover(a, true, 50); Hover(a, false, 50);
        Hover(a, false, 93.75); Near(Number(a, "Value"), .328125, "Partial exit duration scales to 87.5 ms");
        Hover(a, true, 93.75); Hover(a, true, 160.9375);
        Near(Number(a, "Value"), .83203125, "Re-entry duration scales from displayed opacity");
        Hover(a, true, 228.125); Near(Number(a, "Value"), 1, "Re-entry endpoint");
        Hover(a, false, 300); Hover(a, false, 5000);
        Near(Number(a, "Value"), 0, "Long frame uses elapsed time without 64 ms clamp");
        Require(!Running(a), "Long frame finishes");
        Hover(a, true, 6000, false); Near(Number(a, "Value"), 1, "Reduced motion entry");
        Hover(a, false, 6001, false); Near(Number(a, "Value"), 0, "Reduced motion exit");

        object caption = New("ChromiumHoverAnimation", 150d, false);
        Hover(caption, true, 0); Hover(caption, true, 75);
        Near(Number(caption, "Value"), .75, "Caption entry at 75 ms");
        Hover(caption, true, 150); Hover(caption, false, 150); Hover(caption, false, 225);
        Near(Number(caption, "Value"), .25, "Caption exit also eases out");
    }

    static void ButtonChecks()
    {
        object a = New("ChromiumButtonAnimation", false);
        Hover(a, true, 0); Hover(a, true, 62.5);
        Near(Number(a, "HoverOpacity"), .02, "Button quarter-time quadratic ease-in-out");
        Hover(a, true, 125); Near(Number(a, "HoverOpacity"), .08, "Button hover midpoint");
        Hover(a, true, 250); Near(Number(a, "HoverOpacity"), .16, "Button full opacity at 250 ms");
        Require(!Running(a), "Settled hover idles");
        Hover(a, false, 300); Hover(a, false, 425);
        Near(Number(a, "HoverOpacity"), .08, "Button exit midpoint");
        Hover(a, true, 425); Near(Number(a, "HoverOpacity"), 0, "Chromium recreates highlight on re-entry");
        Hover(a, true, 675); Near(Number(a, "HoverOpacity"), .16, "Re-entry full duration");

        a = New("ChromiumButtonAnimation", false);
        Call(a, "Press", 0d); Hover(a, true, 0);
        Near(Number(a, "InkOpacity"), .14, "Pending ripple opacity is immediate");
        Near(Number(a, "InkProgress"), 0, "Ripple starts at minimum radius");
        Call(a, "Release", 20d, true);
        Hover(a, true, 120);
        Near(Number(a, "InkProgress"), .7755613, "240 ms FAST_OUT_SLOW_IN expansion", .0001);
        Near(Number(a, "InkOpacity"), .14, "Quick release waits for expansion");
        Hover(a, false, 240);
        Near(Number(a, "InkOpacity"), .14, "Release fade starts at 240 ms");
        Hover(a, false, 390); Near(Number(a, "InkOpacity"), .07, "300 ms release fade midpoint");
        Near(Number(a, "HoverOpacity"), .16, "Highlight follows committed ripple after mouse exit");
        Hover(a, false, 540); Near(Number(a, "InkOpacity"), 0, "Release finishes at 540 ms");
        Hover(a, false, 600); Near(Number(a, "HoverOpacity"), .08, "Post-ripple highlight fades over 120 ms");
        Hover(a, false, 660); Require(!Running(a), "All button feedback idles");

        a = New("ChromiumButtonAnimation", false);
        Call(a, "Press", 0d); Hover(a, true, 500); Require(!Running(a), "Held full-size ripple idles");
        Call(a, "Release", 500d, true); Call(a, "Release", 520d, true);
        Hover(a, true, 650); Near(Number(a, "InkOpacity"), .07, "Held release starts immediately; duplicate release ignored");
        Hover(a, true, 800); Near(Number(a, "InkOpacity"), 0, "Held click fade endpoint");
        Near(Number(a, "HoverOpacity"), .16, "Hovered button stays highlighted after ripple");

        a = New("ChromiumButtonAnimation", false);
        Call(a, "Press", 0d); Hover(a, true, 120); Hover(a, false, 120);
        Hover(a, false, 220); Near(Number(a, "InkOpacity"), .07, "Cancelled ripple has 200 ms fade");
        Require(Number(a, "InkProgress") < .77556, "Cancelled ripple shrinks");
        Hover(a, true, 220); Near(Number(a, "InkProgress"), 0, "Drag back in starts a new ripple");
        Call(a, "Release", 230d, false); Hover(a, false, 530);
        Require(!Running(a), "Release outside leaves no stuck feedback");
        Call(a, "Press", 600d); Call(a, "Cancel", 650d); Hover(a, false, 1000);
        Require(!Running(a), "Capture cancellation completes");
        Call(a, "Press", 1100d); Hover(a, true, 1100, false);
        Near(Number(a, "InkProgress"), 1, "Reduced motion held state");
        Call(a, "Release", 1110d, true); Hover(a, true, 1110, false);
        Near(Number(a, "InkOpacity"), 0, "Reduced motion release");
        Require(!Running(a), "Reduced motion schedules no frames");
    }

    static void BoundsChecks()
    {
        object a = New("ChromiumBoundsAnimation"), first = new object(), second = new object();
        object[] live = { first, second };
        Rectangle initial = new Rectangle(0, 8, 200, 35), target = new Rectangle(100, 8, 100, 35);
        Call(a, "BeginFrame", live, 0d, true);
        Call(a, "GetBounds", first, initial, true, 17);
        Call(a, "GetBounds", second, initial, true, 17);
        Call(a, "BeginFrame", live, 10d, true);
        Call(a, "GetBounds", first, target, false, 17);
        Call(a, "BeginFrame", live, 110d, true);
        Rectangle mid = (Rectangle)Call(a, "GetBounds", first, target, false, 17);
        Require(mid == new Rectangle(75, 8, 125, 35), "Bounds ease-out midpoint interpolates edges");
        Call(a, "GetBounds", second, target, false, 17);
        Call(a, "BeginFrame", live, 210d, true);
        Require((Rectangle)Call(a, "GetBounds", first, target, false, 17) == target, "Unchanged target keeps its clock");
        Require((Rectangle)Call(a, "GetBounds", second, target, false, 17) == mid, "Second tab uses its independent clock");
        Call(a, "BeginFrame", live, 220d, true);
        Require((Rectangle)Call(a, "GetBounds", second, initial, false, 17) == mid, "Retarget starts at displayed bounds");
        Call(a, "BeginFrame", live, 420d, true);
        Require((Rectangle)Call(a, "GetBounds", second, initial, false, 17) == initial, "Retarget completes in 200 ms");
        var type = Assembly.GetType("EasyTabs.ChromiumBoundsAnimation");
        Rectangle rounded = (Rectangle)Call(type, "Interpolate", new Rectangle(-1, 0, 2, 1), new Rectangle(0, 0, 1, 1), .5d);
        Require(rounded == new Rectangle(0, 0, 1, 1), "Negative half-pixel edges round as Chromium");
    }

    static void CardChecks()
    {
        object a = New("TabHoverCardAnimation"); Type type = a.GetType();
        Near((double)Call(type, "ShowDelay", 55d), 300, "Pinned hover card delay");
        Near((double)Call(type, "ShowDelay", 256d), 800, "Standard hover card delay");
        Require((double)Call(type, "ShowDelay", 100d) < 800, "Narrow hovered card uses its own width");
        Rectangle initial = new Rectangle(100, 100, 256, 200), target = new Rectangle(400, 100, 256, 200);
        Call(a, "Show", initial, 0d, true); Call(a, "Sample", 100d);
        Near(Number(a, "Opacity"), .7755613, "200 ms card fade-in", .0001);
        Call(a, "Sample", 200d); Call(a, "Move", target, 200d, true); Call(a, "Sample", 237.5d);
        Near(Number(a, "TextProgress"), .7755613, "75 ms card movement and text fade", .0001);
        RectangleF displayed = (RectangleF)Property(a, "Bounds");
        Call(a, "Move", initial, 237.5d, true);
        Require((RectangleF)Property(a, "Bounds") == displayed, "Card reversal preserves displayed bounds");
        Call(a, "Sample", 275d); Require(Running(a), "Card reversal restarts its full duration");
        Call(a, "Sample", 312.5d); Require(!Running(a), "Card reversal completes at 75 ms");
        Call(a, "Hide", 400d, true); Call(a, "Sample", 475d);
        Near(Number(a, "Opacity"), 1 - .7755613, "150 ms card fade-out", .0001);
        Call(a, "Sample", 550d); Near(Number(a, "Opacity"), 0, "Card hidden endpoint");
    }

    static object Visual(HiddenWindow window, TitleBarTab tab) => ((IDictionary)Field(window.TabRenderer, "_visuals"))[tab];
    static Rectangle Area(TitleBarTab tab) => (Rectangle)Property(tab, "Area");
    static Point Body(TitleBarTab tab) { Rectangle a = Area(tab); return new Point(a.Left + 30, a.Top + 17); }
    static double TabHover(HiddenWindow w, TitleBarTab tab) => Number(Visual(w, tab), "Hover");

    static void RendererChecks(float scale)
    {
        using (var w = new HiddenWindow(3, scale))
        {
            TitleBarTab tab = w.Tabs[1]; Point point = Body(tab);
            w.Paint(1000, point); w.Paint(1100, point);
            Near(TabHover(w, tab), .75, "Renderer integrates ease-out hover at scale " + scale);
            w.Paint(1200, point); w.Paint(1200, new Point(point.X, w.Renderer.TabHeight + 100));
            w.Paint(1300, new Point(point.X, w.Renderer.TabHeight + 100));
            Near(TabHover(w, tab), .75, "Renderer integrates ease-in exit at scale " + scale);
            w.Paint(1400, new Point(point.X, w.Renderer.TabHeight + 100));
            Near(TabHover(w, tab), 0, "Renderer clears hover into content at scale " + scale);
            w.Paint(1500, point); w.Paint(1700, point);
            // Leave an idle strip. The native non-client leave must itself queue
            // the repaint; no later mouse move in the tab bar is supplied.
            Set(w.Overlay, "_renderPending", false);
            Set(w.Overlay, "_mouseMovePending", false);
            w.Overlay.Native(0x2A2, new Point(point.X, w.Renderer.TabHeight + 100));
            Require((bool)Field(w.Overlay, "_renderPending"), "Native leave wakes idle repaint");
            Require((bool)Field(w.Overlay, "_mouseMovePending"), "Native leave queues fresh pointer sample");
            Call(w.Overlay, "LoadingAnimation_Tick", w.Overlay, EventArgs.Empty);
            w.Paint(1700, w.Overlay.GetRelativeCursorPosition(Cursor.Position));
            w.Paint(1900, w.Overlay.GetRelativeCursorPosition(Cursor.Position));
            Near(TabHover(w, tab), 0, "Native leave completes without tab-bar re-entry");

            // A frame following a low-level hook must discard that hook's stale
            // coordinate and use the cursor after Windows dispatches the move.
            Set(w.Overlay, "_latestMousePosition", new Point(w.Overlay.Left + point.X, w.Overlay.Top + point.Y));
            Set(w.Overlay, "_mouseMovePending", true);
            Call(w.Overlay, "LoadingAnimation_Tick", w.Overlay, EventArgs.Empty);
            Require((Point)Field(w.Overlay, "_latestMousePosition") == Cursor.Position, "Frame resamples stale hook coordinate");

            Rectangle before = Area(w.Tabs[2]);
            w.Renderer.Now = 2000; TitleBarTab added = w.CreateTab(); w.Tabs.Add(added);
            w.Paint(2000, new Point(-1000, -1000));
            Near(Area(added).Width, w.Renderer.OverlapWidth, "New tab starts at overlap width");
            Near(Area(added).Left, before.Right - w.Renderer.OverlapWidth, "New tab starts on displayed neighbour edge");
            w.Paint(2199, new Point(-1000, -1000));
            Require((bool)Call(Field(w.Renderer, "_animation"), "IsItemAnimating", added), "Open lasts full 200 ms");
            w.Paint(2200, new Point(-1000, -1000));
            Require(!(bool)Call(Field(w.Renderer, "_animation"), "IsItemAnimating", added), "Open finishes at 200 ms");

            w.Renderer.Now = 2300; Call(w.Renderer, "BeginTabClose", added, false); w.Tabs.Remove(added);
            w.Paint(2300, new Point(-1000, -1000)); w.Paint(2499, new Point(-1000, -1000));
            Require(Visual(w, added) != null, "Close visual survives until 200 ms");
            w.Paint(2500, new Point(-1000, -1000));
            Require(Visual(w, added) == null, "Close visual released at 200 ms");
            added.Content.Dispose();

            Rectangle close = (Rectangle)Property(tab, "CloseButtonArea");
            Point closePoint = new Point(Area(tab).X + close.X + close.Width / 2, Area(tab).Y + close.Y + close.Height / 2);
            w.Paint(3000, closePoint); w.Paint(3125, closePoint);
            object closeFeedback = Field(Visual(w, tab), "CloseFeedback");
            Near(Number(closeFeedback, "HoverOpacity"), .08, "Close hover uses add-button 250 ms fade");
            Rectangle add = (Rectangle)Field(w.Renderer, "_addButtonArea");
            Point addPoint = new Point(add.Left + add.Width / 2, add.Top + add.Height / 2);
            w.Paint(3500, addPoint); w.Paint(3625, addPoint);
            Near(Number(Field(w.Renderer, "_addFeedback"), "HoverOpacity"), .08, "Add and close hover timing agrees");
        }
    }

    static void RapidCloseChecks()
    {
        using (var w = new HiddenWindow(4, 1))
        {
            TitleBarTab closing = w.Tabs[2], nextClosing = w.Tabs[0];
            w.Renderer.Now = 1000; Call(w.Renderer, "BeginTabClose", closing, false); w.Tabs.Remove(closing);
            w.Paint(1000, new Point(-1000, -1000)); w.Paint(1075, new Point(-1000, -1000));
            Rectangle target = (Rectangle)Field(Visual(w, closing), "Target");
            w.Renderer.Now = 1075; Call(w.Renderer, "BeginTabClose", nextClosing, false); w.Tabs.Remove(nextClosing);
            w.Paint(1075, new Point(-1000, -1000));
            Require((Rectangle)Field(Visual(w, closing), "Target") == target, "Later close leaves earlier close target unchanged");
            w.Paint(1200, new Point(-1000, -1000));
            Require(Visual(w, closing) == null, "Later close does not extend earlier 200 ms lifetime");
            Require(Visual(w, nextClosing) != null, "Later close retains its own lifetime");
            w.Paint(1275, new Point(-1000, -1000));
            Require(Visual(w, nextClosing) == null, "Later close completes independently");
            closing.Content.Dispose(); nextClosing.Content.Dispose();
        }
    }

    static void NativeLeaveOnly()
    {
        using (var w = new HiddenWindow(3, 1))
        {
            Point point = Body(w.Tabs[1]); w.Paint(1000, point); w.Paint(1200, point);
            Set(w.Overlay, "_renderPending", false); Set(w.Overlay, "_mouseMovePending", false);
            w.Overlay.Native(0x2A2, new Point(point.X, w.Renderer.TabHeight + 100));
            Require((bool)Field(w.Overlay, "_renderPending"), "Leaving an idle non-client strip must request repaint");
            Require((bool)Field(w.Overlay, "_mouseMovePending"), "Leaving must queue the final pointer position");
        }
    }

    [STAThread]
    static int Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        if (Array.IndexOf(args, "--native-leave-only") >= 0)
        {
            Test("idle strip native leave regression", NativeLeaveOnly);
            return failures == 0 ? 0 : 1;
        }
        Test("tab hover and caption curves", HoverChecks);
        Test("button hover, click, interruption, reduced motion", ButtonChecks);
        Test("independent bounds clocks and edge rounding", BoundsChecks);
        Test("Chromium 85 hover-card timing", CardChecks);
        Test("rapid close preserves each animation lifetime", RapidCloseChecks);
        foreach (float scale in new[] { 1f, 1.25f, 1.5f, 2f })
        {
            float current = scale;
            Test("hidden renderer and native leave, scale " + scale, () => RendererChecks(current));
        }
        Console.WriteLine(checks + " assertions; " + failures + " failing groups.");
        return failures == 0 ? 0 : 1;
    }
}

sealed class ClockRenderer : ChromiumTabRenderer
{
    public double Now;
    readonly float scale;
    public ClockRenderer(TitleBarTabs window, float scale) : base(window) { this.scale = scale; }
    protected override double AnimationTimeMilliseconds => Now;
    protected override float RenderScale => scale;
    protected override bool ShouldAnimateLayout() => AnimationsEnabled;
}

sealed class HiddenWindow : TitleBarTabs
{
    public ClockRenderer Renderer => (ClockRenderer)TabRenderer;
    public HiddenOverlay Overlay => (HiddenOverlay)_overlay;
    public HiddenWindow(int count, float scale)
    {
        AeroPeekEnabled = false; ExitOnLastTabClose = false; ShowTooltips = false;
        Bounds = new Rectangle(120, 150, (int)(1100 * scale), 600);
        TabRenderer = new ClockRenderer(this, scale);
        for (int i = 0; i < count; i++)
        {
            var tab = CreateTab(); Tabs.Add(tab); IntPtr handle = tab.Content.Handle;
        }
        SelectedTabIndex = 0;
        _overlay = new HiddenOverlay(this);
        Overlay.LayoutNow();
        Paint(0, new Point(-1000, -1000));
    }
    protected override void SetVisibleCore(bool value) { base.SetVisibleCore(false); }
    public override TitleBarTab CreateTab() => new TitleBarTab(this) { Content = new Form { Text = "Animation fixture " + Tabs.Count } };
    public new void Paint(double now, Point local)
    {
        Renderer.Now = now;
        Overlay.Render(new Point(Overlay.Left + local.X, Overlay.Top + local.Y), true);
    }
}

sealed class HiddenOverlay : TitleBarTabsOverlay
{
    public HiddenOverlay(TitleBarTabs parent) : base(parent) { }
    protected override void SetVisibleCore(bool value) { base.SetVisibleCore(false); }
    public void LayoutNow() { OnPosition(); }
    public void Native(int message, Point local)
    {
        // Move only this hidden fixture; leave the physical pointer untouched.
        Point cursor = Cursor.Position;
        Location = new Point(cursor.X - local.X, cursor.Y - local.Y);
        Message input = Message.Create(Handle, message, IntPtr.Zero, IntPtr.Zero);
        WndProc(ref input);
    }
}
