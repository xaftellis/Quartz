using Newtonsoft.Json;
using Quartz.Controls;
using Quartz.Models;
using Quartz.Services;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

internal static partial class FavouritesTests
{
    private sealed class TestBar : FavouritesBar
    {
        protected override bool ShouldAnimate() => AnimationsEnabled;
    }

    private sealed class TestButton : FavouriteButton
    {
        internal void Press(Point point) => OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, point.X, point.Y, 0));
        internal void ClickNow() => OnClick(EventArgs.Empty);
        internal void Key() => OnKeyDown(new KeyEventArgs(Keys.Space));
        internal void LoseCapture() => OnMouseCaptureChanged(EventArgs.Empty);
        internal void MiddleUp() => OnMouseUp(new MouseEventArgs(MouseButtons.Middle, 1, 4, 4, 0));
        internal void Release(Point screen)
        {
            Point point = PointToClient(screen);
            Message message = Message.Create(Handle, 0x0202, IntPtr.Zero,
                (IntPtr)((point.Y << 16) | (point.X & 0xffff)));
            WndProc(ref message);
        }
    }

    private sealed class Row : IDisposable
    {
        internal readonly Form Form = new Form { ShowInTaskbar = false };
        internal readonly TestBar Bar;
        internal readonly TestButton[] Buttons;
        internal int Commits;

        internal Row(params int[] widths)
        {
            Bar = new TestBar { Size = new Size(650, 50), AutoScroll = true };
            Form.Controls.Add(Bar);
            Buttons = widths.Select((width, index) => new TestButton
            {
                Text = ((char)('A' + index)).ToString(), Size = new Size(width, 23),
                AutoSize = false, Margin = new Padding(3)
            }).ToArray();
            Bar.Controls.AddRange(Buttons);
            // WinForms ignores AutoScrollPosition until Created is true. Its
            // internal overload creates hidden controls without showing a window.
            typeof(Control).GetMethod("CreateControl", BindingFlags.Instance | BindingFlags.NonPublic,
                null, new[] { typeof(bool) }, null).Invoke(Form, new object[] { true });
            Bar.PerformLayout();
            Bar.OrderChanged += (s, e) => Commits++;
        }

        internal Point At(int x, int y = 10) => Bar.PointToScreen(new Point(x, y));
        internal string Order => string.Concat(Bar.Controls.Cast<Control>().Select(b => b.Text));
        internal void Start(int index, int grab = 5) => Buttons[index].Press(new Point(grab, 7));
        internal void Move(int x) => Bar.PointerMove(At(x));
        internal void End(int x) => Buttons.First(b => b.SuppressMouseClick).Release(At(x));
        internal void Settle() { for (int i = 0; i < 60; i++) Bar.AdvanceAnimation(16); }
        public void Dispose() => Form.Dispose();
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    [STAThread]
    private static int Main()
    {
        Application.EnableVisualStyles();
        Control.CheckForIllegalCrossThreadCalls = true;
        Action[] tests = { LayoutCompatibility, OrdinaryClick, ReorderAndAnimate, AlphabeticalAnimation,
            ProgrammaticOrderSafety, IconVisibilityAnimation, RapidIconChanges,
            AddRemoveAnimation, MembershipAnimationSafety, SingleLabelFrames, MembershipFrames, UnequalWidths, CancelPaths,
            SuppressedClickAndKeyboard, NativeRelease, ScrollAndResize, CollectionChange,
            Persistence, AddEditAndSort, DuplicateFavourites, LegacyFavouriteIds, DuplicateButtonIdentity, FailedSave, Validation };
        int failures = 0;
        foreach (Action test in tests)
        {
            try { test(); Console.WriteLine("PASS " + test.Method.Name); }
            catch (Exception error) { failures++; Console.WriteLine("FAIL " + test.Method.Name + ": " + error); }
        }
        Console.WriteLine((tests.Length - failures) + "/" + tests.Length + " test groups passed.");
        return failures == 0 ? 0 : 1;
    }

    private static void LayoutCompatibility()
    {
        using (var original = new FlowLayoutPanel { Size = new Size(650, 50), WrapContents = false, AutoScroll = true })
        using (var animated = new TestBar { Size = new Size(650, 50) })
        using (var font = new Font("Segoe UI", 8))
        using (var icon = new Bitmap(16, 16))
        {
            foreach (var panel in new FlowLayoutPanel[] { original, animated })
            {
                foreach (string text in new[] { "Google", "      A longer favourite...", "A" })
                    panel.Controls.Add(new TestButton { Text = text, Font = font, Image = icon,
                        AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        MaximumSize = new Size(150, 23), FlatStyle = FlatStyle.Flat, Margin = new Padding(3) });
                panel.PerformLayout();
            }
            Check(original.Controls.Cast<Control>().Select(b => b.Bounds)
                .SequenceEqual(animated.Controls.Cast<Control>().Select(b => b.Bounds)),
                "Normal button sizes and margins must match the previous flow layout.");
        }
    }

    private static void OrdinaryClick()
    {
        using (var row = new Row(90, 150, 40))
        {
            int clicks = 0;
            row.Buttons[0].Click += (s, e) => clicks++;
            row.Start(0);
            row.Bar.PointerMove(row.Buttons[0].PointToScreen(new Point(6, 7)));
            row.Buttons[0].ClickNow();
            row.Bar.PointerUp(row.At(9));
            Check(clicks == 1 && row.Commits == 0 && row.Order == "ABC" && !row.Bar.IsInteracting,
                "Small pointer motion must remain a click without a save.");
            int middleUps = 0;
            row.Buttons[0].MouseUp += (s, e) => { if (e.Button == MouseButtons.Middle) middleUps++; };
            row.Buttons[0].MiddleUp();
            Check(middleUps == 1, "Middle-click must still reach the browser handler.");
        }
    }

    private static void ReorderAndAnimate()
    {
        using (var row = new Row(90, 150, 40))
        {
            int oldLeft = row.Buttons[1].Left;
            row.Start(0, 20);
            row.Move(295);
            Check(row.Buttons[0].Left == 205, "Dragged item must clamp at the content end.");
            Check(row.Buttons[1].Left == oldLeft, "Neighbours should start at their displayed positions.");
            row.Bar.AdvanceAnimation(16);
            Check(row.Buttons[1].Left > 3 && row.Buttons[1].Left < oldLeft, "Neighbours must interpolate.");
            row.Bar.PointerUp(row.At(295));
            row.Settle();
            Check(row.Order == "BCA" && row.Commits == 1, "Drop must commit exactly once.");
            Check(row.Buttons[1].Left == 3 && row.Buttons[2].Left == 159 && row.Buttons[0].Left == 205,
                "All buttons must settle into exact slots.");
            Check(row.Bar.Controls.Cast<Control>().Select(b => b.TabIndex).SequenceEqual(new[] { 0, 1, 2 }),
                "Keyboard tab order must match visual order.");
        }
    }

    private static void AlphabeticalAnimation()
    {
        using (var row = new Row(90, 150, 40))
        {
            row.Buttons[0].Text = "Zulu";
            row.Buttons[1].Text = "Alpha";
            row.Buttons[2].Text = "Middle";
            var order = row.Buttons.OrderBy(button => button.Text).Cast<Control>().ToList();
            var starts = row.Buttons.Select(button => button.Left).ToArray();
            Check(row.Bar.TryAnimateOrder(order), "Alphabetical sorting must accept the existing buttons.");
            Check(row.Buttons.Select(button => button.Left).SequenceEqual(starts),
                "Sorting must retain the displayed positions for its first frame.");
            row.Bar.AdvanceAnimation(16);
            Check(row.Buttons[1].Left > 3 && row.Buttons[1].Left < starts[1] &&
                row.Buttons[0].Left > starts[0] && row.Buttons[0].Left < 205,
                "Alphabetical sorting must animate buttons moving both left and right.");
            row.Settle();
            Check(row.Order == "AlphaMiddleZulu" && row.Buttons[1].Left == 3 &&
                row.Buttons[2].Left == 159 && row.Buttons[0].Left == 205,
                "Unequal-width favourites must settle into exact alphabetical slots.");
            Check(row.Commits == 0, "A saved alphabetical sort must not trigger a custom-order save.");
            Check(row.Bar.Controls.Cast<Control>().SequenceEqual(order) && row.Buttons.All(button => !button.IsDisposed),
                "Sorting must retain the live buttons and their handlers.");
            Check(row.Bar.Controls.Cast<Control>().Select(button => button.TabIndex).SequenceEqual(new[] { 0, 1, 2 }),
                "Keyboard order must follow alphabetical order.");
            int clicks = 0;
            row.Buttons[1].Click += (s, e) => clicks++;
            row.Buttons[1].ClickNow();
            Check(clicks == 1, "A sorted favourite must remain clickable.");
        }
    }

    private static void ProgrammaticOrderSafety()
    {
        using (var row = new Row(150, 150, 150, 150, 150))
        {
            row.Bar.Width = 240;
            row.Bar.AutoScrollPosition = new Point(100, 0);
            var original = row.Buttons.Cast<Control>().ToList();
            var reversed = original.AsEnumerable().Reverse().ToList();
            Check(row.Bar.TryAnimateOrder(reversed), "A scrolled bar must accept sorting.");
            row.Bar.AdvanceAnimation(16);
            var midway = row.Buttons.Select(button => button.Left).ToArray();
            Check(row.Bar.TryAnimateOrder(original) && row.Buttons.Select(button => button.Left).SequenceEqual(midway),
                "Retargeting a running animation must continue from displayed positions.");
            row.Settle();
            Check(row.Order == "ABCDE" && row.Bar.AutoScrollPosition.X == -100,
                "Programmatic sorting must preserve the scroll offset.");

            row.Bar.TryAnimateOrder(reversed);
            row.Bar.AdvanceAnimation(16);
            row.Start(3); row.Move(30); row.Bar.CancelInteraction(); row.Settle();
            Check(row.Order == "EDCBA" && row.Commits == 0,
                "Cancelling a drag started during sorting must retain the sorted order.");

            row.Start(2);
            Check(!row.Bar.TryAnimateOrder(original) && row.Bar.IsInteracting,
                "Sorting must not silently interrupt an active pointer interaction.");
            row.Bar.CancelInteraction();
            Check(!row.Bar.TryAnimateOrder(new Control[] { row.Buttons[0] }) &&
                !row.Bar.TryAnimateOrder(Enumerable.Repeat<Control>(row.Buttons[0], 5).ToList()),
                "Incomplete or duplicate control lists must be rejected without mutation.");

            row.Bar.AnimationsEnabled = false;
            Check(row.Bar.TryAnimateOrder(original) && row.Buttons[0].Left == -97 && row.Buttons[1].Left == 59,
                "Reduced-motion sorting must snap directly to the saved slots.");
            Check(row.Bar.TryAnimateOrder(original) && row.Commits == 0,
                "Sorting an already sorted bar must be harmless.");
        }
        using (var row = new Row())
            Check(row.Bar.TryAnimateOrder(new Control[0]), "Sorting an empty bar must be harmless.");
    }

    private static TestButton ContentButton(string name) => new TestButton
    {
        Name = name, Text = name, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
        MaximumSize = new Size(150, 23), FlatStyle = FlatStyle.Flat,
        ImageAlign = ContentAlignment.MiddleLeft, TextAlign = ContentAlignment.MiddleCenter,
        TextImageRelation = TextImageRelation.Overlay, UseMnemonic = false,
        BackColor = Color.White, ForeColor = Color.Black, Margin = new Padding(3)
    };

    private static Image TestIcon()
    {
        var image = new Bitmap(16, 16);
        using (Graphics graphics = Graphics.FromImage(image))
        {
            graphics.Clear(Color.OrangeRed);
            graphics.FillRectangle(Brushes.White, 5, 4, 6, 8);
        }
        return image;
    }

    private static void ConfigureContent(FavouriteButton button, bool icons)
    {
        button.SetIconVisibility(icons, TestIcon);
        button.Text = (icons ? "      " : "") + button.Name;
        button.MaximumSize = new Size(150, 23);
    }

    private static bool SamePixels(Bitmap a, Bitmap b)
    {
        if (a.Size != b.Size) return false;
        for (int y = 0; y < a.Height; y++)
            for (int x = 0; x < a.Width; x++)
                if (a.GetPixel(x, y) != b.GetPixel(x, y)) return false;
        return true;
    }

    private static void IconVisibilityAnimation()
    {
        using (var row = new Row())
        {
            var buttons = new FavouriteButton[] { ContentButton("Quartz"), ContentButton("Search"), ContentButton("Videos") };
            row.Bar.UpdateItems(buttons, b => ConfigureContent(b, true), false);
            int oldWidth = buttons[0].Width;
            int oldNext = buttons[1].Left;
            using (Bitmap before = buttons[0].CaptureVisual())
            {
                row.Bar.UpdateItems(buttons, b => ConfigureContent(b, false), true);
                int targetWidth = buttons[0].GetPreferredSize(Size.Empty).Width;
                Check(targetWidth < oldWidth, "Fixture must lose width when the icon gap is removed.");
                Check(buttons[0].Width == oldWidth && buttons[1].Left == oldNext && buttons[0].Image == null,
                    "Icon setting must change immediately while geometry retains its first frame.");
                using (Bitmap first = buttons[0].CaptureVisual())
                    Check(SamePixels(before, first), "The first hide-icon frame must match the displayed button.");
                row.Bar.AdvanceAnimation(50);
                Check(buttons[0].Width > targetWidth && buttons[0].Width < oldWidth &&
                    buttons[1].Left < oldNext && buttons[0].ContentProgress > 0 && buttons[0].ContentProgress < 1,
                    "Icon fade, width, and neighbouring positions must advance together.");
                using (Bitmap during = buttons[0].CaptureVisual())
                {
                    row.Settle();
                    Check(buttons[0].Width == targetWidth && !buttons[0].IsContentAnimating,
                        "Hiding icons must finish exactly and release its temporary rendering.");
                    using (Bitmap after = buttons[0].CaptureVisual())
                    using (var sheet = new Bitmap(360, 110))
                    {
                        using (Graphics graphics = Graphics.FromImage(sheet))
                        {
                            graphics.Clear(Color.FromArgb(235, 235, 235));
                            graphics.DrawImageUnscaled(before, 8, 8);
                            graphics.DrawImageUnscaled(during, 8, 43);
                            graphics.DrawImageUnscaled(after, 8, 78);
                        }
                        sheet.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "favourites-icon-transition.png"));
                    }
                }
            }
            row.Bar.UpdateItems(buttons, b => ConfigureContent(b, true), true);
            Check(buttons[0].IsContentAnimating && buttons[0].Image != null, "Showing icons must also animate.");
            row.Settle();
            Check(buttons[0].Width == oldWidth && buttons[1].Left == oldNext && row.Commits == 0,
                "Showing icons must restore original spacing without changing saved order.");
            int clicks = 0;
            buttons[0].Click += (s, e) => clicks++;
            ((TestButton)buttons[0]).ClickNow();
            Check(clicks == 1, "Icon transitions must retain click handlers.");
        }
    }

    private static void RapidIconChanges()
    {
        using (var row = new Row())
        {
            var buttons = new FavouriteButton[] { ContentButton("Quartz"), ContentButton("Search") };
            row.Bar.UpdateItems(buttons, b => ConfigureContent(b, true), false);
            row.Bar.UpdateItems(buttons, b => ConfigureContent(b, false), true);
            row.Bar.AdvanceAnimation(45);
            using (Bitmap displayed = buttons[0].CaptureVisual())
            {
                row.Bar.UpdateItems(buttons, b => ConfigureContent(b, true), true);
                using (Bitmap reversed = buttons[0].CaptureVisual())
                    Check(SamePixels(displayed, reversed), "A rapid reversal must start from the current rendered frame.");
            }
            row.Settle();
            row.Bar.UpdateItems(buttons, b => ConfigureContent(b, true), true);
            Check(!buttons[0].IsContentAnimating, "An unchanged favicon refresh must not start a new fade.");
            row.Bar.UpdateItems(buttons, b => ConfigureContent(b, false), true);
            row.Bar.AdvanceAnimation(30);
            row.Bar.AnimationsEnabled = false;
            row.Bar.UpdateItems(buttons, b => ConfigureContent(b, false), true);
            Check(!buttons[0].IsContentAnimating && buttons[0].Width == buttons[0].GetPreferredSize(Size.Empty).Width,
                "Reduced motion must finish even an unchanged in-flight icon update.");
        }
    }

    private static void AddRemoveAnimation()
    {
        using (var row = new Row())
        {
            var a = ContentButton("Alpha");
            var b = ContentButton("Bravo");
            var c = ContentButton("Charlie");
            row.Bar.UpdateItems(new FavouriteButton[] { a, c }, button => ConfigureContent(button, true), false);
            int oldLeft = c.Left;
            int oldExtent = row.Bar.AutoScrollMinSize.Width;
            row.Bar.UpdateItems(new FavouriteButton[] { a, b, c }, button => ConfigureContent(button, true), true);
            Check(b.Width == 0 && b.IsEntering && c.Left == oldLeft,
                "An inserted favourite must start collapsed while existing buttons retain position.");
            row.Bar.AdvanceAnimation(45);
            Check(b.Width > 0 && b.Width < b.GetPreferredSize(Size.Empty).Width && b.IsEntering &&
                 c.Left > oldLeft, "New favourites must expand while neighbours slide.");
            using (Bitmap addedFrame = b.CaptureVisual())
                Check(addedFrame.Width == b.Width, "Native painting must support a partially expanded favourite.");
            row.Settle();
            int addedLeft = c.Left;
            row.Bar.UpdateItems(new FavouriteButton[] { a, c }, button => ConfigureContent(button, true), true);
            Check(b.IsDisposed && row.Bar.Controls.Count == 2 && c.Left == addedLeft,
                "Removal must stop interaction with the deleted favourite immediately.");
            row.Bar.AdvanceAnimation(45);
            Check(c.Left > oldLeft && c.Left < addedLeft, "Survivors must animate into a removed favourite's gap.");
            row.Settle();
            Check(c.Left == oldLeft && row.Bar.AutoScrollMinSize.Width == oldExtent && row.Commits == 0,
                "Add/remove must preserve order, final spacing, and scroll extent without drag saves.");

            row.Bar.UpdateItems(new FavouriteButton[0], button => { }, true);
            Check(row.Bar.Controls.Count == 0 && row.Bar.HasVisibleItems,
                "The last removed favourites must remain paintable until their contraction finishes.");
            row.Settle();
            Check(!row.Bar.HasVisibleItems && row.Bar.AutoScrollMinSize.Width == 0,
                "An emptied bar must release removed visuals and become hideable.");
        }
    }

    private static void MembershipAnimationSafety()
    {
        using (var row = new Row())
        {
            var a = ContentButton("Alpha");
            row.Bar.UpdateItems(new FavouriteButton[] { a }, button => ConfigureContent(button, true), true);
            row.Bar.AdvanceAnimation(30);
            row.Bar.UpdateItems(new FavouriteButton[0], button => { }, true);
            var b = ContentButton("Bravo");
            row.Bar.UpdateItems(new FavouriteButton[] { b }, button => ConfigureContent(button, false), true);
            row.Settle();
            Check(a.IsDisposed && !b.IsDisposed && row.Bar.Controls.Count == 1 &&
                b.Width == b.GetPreferredSize(Size.Empty).Width && !b.IsEntering,
                "Removing an entering favourite and adding another must leave exactly the requested items.");

            b.Press(new Point(5, 7));
            Check(!row.Bar.UpdateItems(new FavouriteButton[0], button => { }, true) && !b.IsDisposed,
                "Membership refresh must defer while the user is interacting.");
            row.Bar.CancelInteraction();
            row.Bar.UpdateItems(new FavouriteButton[0], button => { }, true);
            row.Bar.AnimationsEnabled = false;
            row.Bar.UpdateItems(new FavouriteButton[0], button => { }, true);
            Check(!row.Bar.HasVisibleItems, "Disabling animation must clear an existing removal animation.");
        }
    }

    private static void UnequalWidths()
    {
        foreach (int[] widths in new[] { new[] { 30, 150, 45 }, new[] { 150, 30, 150 }, new[] { 150, 150, 30 } })
        {
            foreach (int grab in new[] { 2, widths[0] - 2 })
            using (var row = new Row(widths))
            {
                row.Start(0, grab);
                for (int x = 0; x <= 500; x += 4) row.Move(x);
                row.Bar.PointerUp(row.At(500));
                row.Settle();
                Check(row.Order == "BCA", "Left-to-right drag must work with mixed widths/grab offsets.");
                row.Start(0, grab);
                for (int x = 500; x >= 0; x -= 4) row.Move(x);
                row.Bar.PointerUp(row.At(0));
                row.Settle();
                Check(row.Order == "ABC", "Right-to-left drag must work with mixed widths/grab offsets.");
            }
        }
        using (var row = new Row(150, 30, 150))
        {
            row.Start(0, 75);
            row.Move(115);
            row.Bar.PointerUp(row.At(115));
            string expected = row.Order;
            using (var repeated = new Row(150, 30, 150))
            {
                repeated.Start(0, 75);
                for (int i = 0; i < 100; i++) { repeated.Move(115); repeated.Bar.AdvanceAnimation(16); }
                repeated.Bar.PointerUp(repeated.At(115));
                Check(repeated.Order == expected, "A stationary pointer must not oscillate between slots.");
            }
        }
    }

    private static void CancelPaths()
    {
        using (var row = new Row(90, 150, 40))
        {
            row.Start(0); row.Move(290);
            Message escape = Message.Create(IntPtr.Zero, 0x0100, (IntPtr)Keys.Escape, IntPtr.Zero);
            Check(row.Bar.PreFilterMessage(ref escape), "Escape must be consumed during drag.");
            row.Settle();
            Check(row.Order == "ABC" && row.Commits == 0 && !row.Bar.IsInteracting, "Escape must restore original order.");
            row.Start(0); row.Move(290); row.Bar.PointerUp(row.At(290, 100)); row.Settle();
            Check(row.Order == "ABC" && row.Commits == 0, "An outside drop must cancel.");
            row.Start(0); row.Move(290); row.Buttons[0].Capture = false; row.Buttons[0].LoseCapture(); row.Settle();
            Check(row.Order == "ABC" && row.Commits == 0 && !row.Bar.IsInteracting, "Capture loss must cancel.");
        }
        using (var row = new Row(90))
        {
            row.Start(0); row.Move(350); row.Bar.PointerUp(row.At(350)); row.Settle();
            Check(row.Order == "A" && row.Commits == 0, "A single favourite must never save a spurious reorder.");
        }
    }

    private static void SuppressedClickAndKeyboard()
    {
        using (var row = new Row(90, 90))
        {
            int clicks = 0;
            row.Buttons[0].Click += (s, e) => clicks++;
            row.Start(0); row.Move(180);
            row.Buttons[0].ClickNow();
            row.Bar.PointerUp(row.At(180));
            row.Buttons[0].ClickNow();
            Check(clicks == 0, "Drag release must not navigate, regardless of Click/MouseUp order.");
            row.Buttons[0].Key(); row.Buttons[0].ClickNow();
            Check(clicks == 1, "Keyboard activation must recover after drag.");
            row.Start(0); row.Buttons[0].ClickNow(); row.Bar.PointerUp(row.At(110));
            Check(clicks == 2, "The next ordinary click must work.");
        }
    }

    private static void NativeRelease()
    {
        using (var row = new Row(90, 90))
        {
            int clicks = 0;
            row.Buttons[0].Click += (s, e) => clicks++;
            row.Start(0); row.Move(180);
            row.End(180); row.Settle();
            Check(row.Order == "BA" && row.Commits == 1 && clicks == 0 && !row.Bar.IsInteracting,
                "Native WM_LBUTTONUP must finish once without a click or capture-loss rollback.");
        }
    }

    private static void ScrollAndResize()
    {
        using (var row = new Row(150, 150, 150, 150, 150))
        {
            row.Bar.Width = 240;
            row.Bar.PerformLayout();
            Check(row.Bar.HorizontalScroll.Visible, "Overflow must expose the horizontal scrollbar.");
            row.Bar.AutoScrollPosition = new Point(100, 0);
            row.Bar.PerformLayout();
            Check(row.Buttons[1].Left == 59, "Layout must account for the existing scroll offset: left=" +
                row.Buttons[1].Left + ", scroll=" + row.Bar.AutoScrollPosition + ", display=" + row.Bar.DisplayRectangle);
            row.Start(1);
            row.Move(230);
            for (int i = 0; i < 25; i++) { row.Bar.ScrollAtEdge(new Point(235, 10), 30); row.Move(235); }
            Check(-row.Bar.AutoScrollPosition.X > 100, "Dragging at the right edge must scroll.");
            row.Bar.PointerUp(row.At(235)); row.Settle();
            Check(row.Order == "ACDEB", "A scrolled drop must save logical order.");
            row.Start(2); row.Move(30); row.Bar.Width = 300; row.Settle();
            Check(!row.Bar.IsInteracting && row.Order == "ACDEB", "Resize must cancel pending reorder safely.");
        }
        using (var row = new Row(90, 90))
        {
            row.Bar.AnimationsEnabled = false;
            row.Start(0); row.Move(180); row.Bar.PointerUp(row.At(180));
            Check(row.Buttons[1].Left == 3 && row.Buttons[0].Left == 99, "Reduced-motion layout must snap accurately.");
            row.Start(1); row.Move(18); row.Bar.PointerUp(row.At(18));
            Check(row.Buttons[1].Left == 3, "A reduced-motion same-slot drop must settle without a layout event.");
            row.Start(1); row.Move(18); row.Bar.CancelInteraction();
            Check(row.Buttons[1].Left == 3, "A reduced-motion cancelled drag must settle without a layout event.");
        }
    }

    private static void CollectionChange()
    {
        using (var row = new Row(90, 90, 90))
        {
            row.Start(0); row.Move(250);
            row.Buttons[1].Dispose(); row.Settle();
            Check(!row.Bar.IsInteracting && row.Order == "AC" && row.Commits == 0,
                "Removing a control mid-drag must safely restore surviving items.");
        }
        using (var row = new Row(90, 90))
        {
            row.Start(0); row.Move(150); row.Bar.Dispose();
            Check(!row.Bar.IsInteracting, "Dispose must detach the interaction and timer.");
        }
    }

    private static FavouriteModel Favourite(Guid profile, string name, int index) =>
        new FavouriteModel { Id = Guid.NewGuid(), ProfileId = profile, Name = name, Index = index, WebAddress = "https://example.test/" + name };

    private static void WithFile(Action<string, FavouriteModel[]> test)
    {
        Guid profile = Guid.NewGuid();
        Guid previous = ProfileService.Current;
        ProfileService.Current = profile;
        string path = Path.Combine(Path.GetTempPath(), "quartz-favourites-test-" + Guid.NewGuid().ToString("N") + ".json");
        var items = new[] { Favourite(profile, "A", 0), Favourite(profile, "B", 1), Favourite(Guid.NewGuid(), "Other", 42) };
        try { File.WriteAllText(path, JsonConvert.SerializeObject(items)); test(path, items); }
        finally { ProfileService.Current = previous; File.Delete(path); }
    }

    private static void Persistence()
    {
        WithFile((path, items) =>
        {
            Check(new FavouriteService(path).TryReorder(new[] { items[1], items[0] }), "Valid reorder should save.");
            var saved = JsonConvert.DeserializeObject<FavouriteModel[]>(File.ReadAllText(path));
            Check(saved.Single(f => f.Name == "B").Index == 0 && saved.Single(f => f.Name == "A").Index == 1,
                "Reopening storage must retain reordered indices.");
            Check(saved.Single(f => f.Name == "Other").Index == 42, "Other profiles must remain unchanged.");
            string before = File.ReadAllText(path);
            Check(!new FavouriteService(path).TryReorder(new[] { items[0], items[0] }), "Duplicates must be rejected.");
            Check(!new FavouriteService(path).TryReorder(new[] { items[0] }), "Stale membership must be rejected.");
            var changed = Favourite(items[1].ProfileId, "B", 1); changed.Id = items[1].Id; changed.WebAddress += "/edited";
            Check(!new FavouriteService(path).TryReorder(new[] { changed, items[0] }), "Stale edits must be rejected.");
            Check(!new FavouriteService(path).TryReorder(new[] { items[2], items[0] }), "Cross-profile drops must be rejected.");
            Check(File.ReadAllText(path) == before, "Rejected orders must not write storage.");
        });
    }

    private static void FailedSave()
    {
        WithFile((path, items) =>
        {
            string before = File.ReadAllText(path);
            File.SetAttributes(path, FileAttributes.ReadOnly);
            bool failed = false;
            try { new FavouriteService(path).TryReorder(new[] { items[1], items[0] }); }
            catch (UnauthorizedAccessException) { failed = true; }
            catch (IOException) { failed = true; }
            finally { File.SetAttributes(path, FileAttributes.Normal); }
            Check(failed && File.ReadAllText(path) == before, "A failed replacement must leave the original JSON intact.");
            Check(Directory.GetFiles(Path.GetDirectoryName(path), Path.GetFileName(path) + ".*.tmp").Length == 0,
                "Failed saves must clean up temporary files.");
        });
    }

    private static void AddEditAndSort()
    {
        WithFile((path, items) =>
        {
            var service = new FavouriteService(path);
            service.Get(items[0].Id).Index = 7;
            service.Get(items[1].Id).Index = 12;
            var c = service.Add(Favourite(ProfileService.Current, "C", 0));
            Check(string.Concat(service.All().OrderBy(f => f.Index).Select(f => f.Name)) == "ABC",
                "Adding/pasting after legacy index gaps must append.");
            Check(service.All().Select(f => f.Index).Distinct().Count() == 3, "Append must remove index collisions.");
            service.Remove(items[0].Id);
            service.Add(Favourite(ProfileService.Current, "D", 0));
            service.Edit(c.Id, "C", "https://example.test/C");
            Check(string.Concat(service.All().OrderBy(f => f.Index).Select(f => f.Name)) == "BCD",
                "Deletion and edits must preserve existing custom order.");
            service.Edit(c.Id, "Z", "https://example.test/Z");
            Check(string.Concat(service.All().OrderBy(f => f.Index).Select(f => f.Name)) == "BZD",
                "Renaming in custom mode must retain position.");
            service.SortAlphabetically(); service.SaveChanges();
            Check(string.Concat(new FavouriteService(path).All().OrderBy(f => f.Index).Select(f => f.Name)) == "BDZ",
                "Alphabetical mode must sort renamed or newly added favourites.");
            Check(JsonConvert.DeserializeObject<FavouriteModel[]>(File.ReadAllText(path))
                .Single(f => f.Name == "Other").Index == 42, "Add/edit/sort must preserve other profiles.");
        });
    }

    private static void DuplicateFavourites()
    {
        WithFile((path, items) =>
        {
            var service = new FavouriteService(path);
            var original = service.Get(items[0].Id);
            var sameName = service.Add(new FavouriteModel { Name = original.Name, WebAddress = "https://example.test/other" });
            var sameUrl = service.Add(new FavouriteModel { Name = "Another name", WebAddress = original.WebAddress });
            var identical = service.Add(original);
            var copiedAgain = service.Add(original);
            Check(service.All().Count == 6 && service.All().Select(f => f.Id).Distinct().Count() == 6 &&
                service.All().All(f => f.Id != Guid.Empty), "Same name, URL, and exact copies must each have independent identities.");
            Check(service.Get(items[0].Id) == original && original.Index == 0 && identical != original,
                "Adding a stored model must create a copy without changing the original.");
            service.Edit(sameName.Id, original.Name, original.WebAddress);
            Check(service.Get(sameName.Id).WebAddress == original.WebAddress && sameName.Index == 2,
                "Editing to an existing name and URL must be allowed and retain the selected slot.");
            service.SaveChanges();

            service = new FavouriteService(path);
            var reversed = service.All().OrderByDescending(f => f.Index).ToList();
            Check(service.TryReorder(reversed), "Rearranging identical favourites must save successfully.");
            var reopened = new FavouriteService(path);
            Check(reopened.All().OrderBy(f => f.Index).Select(f => f.Id).SequenceEqual(reversed.Select(f => f.Id)),
                "Every duplicate's exact order must survive reopening.");
            var tiedOrder = reopened.All().Where(f => f.Name == "A").OrderBy(f => f.Index).Select(f => f.Id).ToList();
            reopened.SortAlphabetically(); reopened.SaveChanges();
            Check(reopened.All().Where(f => f.Name == "A").OrderBy(f => f.Index).Select(f => f.Id).SequenceEqual(tiedOrder),
                "Alphabetical sorting must preserve the relative order of identical names.");
            reopened.Edit(identical.Id, "Edited copy", "https://example.test/edited");
            Check(reopened.Get(original.Id).Name == "A" && reopened.Get(copiedAgain.Id).Name == "A" &&
                reopened.Get(identical.Id).Name == "Edited copy", "Editing must affect only the chosen copy.");
            reopened.Remove(sameName.Id); reopened.SaveChanges();
            service = new FavouriteService(path);
            Check(service.Get(sameName.Id) == null && service.Get(original.Id) != null &&
                service.Get(copiedAgain.Id) != null && service.Get(sameUrl.Id) != null && service.All().Count == 5,
                "Delete/cut must remove only the selected duplicate.");
            Check(!service.TryReorder(reversed), "An order from before a duplicate was removed must remain stale.");
            Check(service.Get(items[2].Id) == null && JsonConvert.DeserializeObject<FavouriteModel[]>(File.ReadAllText(path))
                .Single(f => f.Id == items[2].Id).Index == 42, "Duplicate operations must stay within the active profile.");
        });
    }

    private static void LegacyFavouriteIds()
    {
        WithFile((path, items) =>
        {
            var legacy = Newtonsoft.Json.Linq.JArray.FromObject(items);
            foreach (Newtonsoft.Json.Linq.JObject item in legacy) item.Remove("Id");
            legacy.Add(legacy[0].DeepClone());
            File.WriteAllText(path, legacy.ToString());
            string before = File.ReadAllText(path);
            FavouriteService first, second;
            File.SetAttributes(path, FileAttributes.ReadOnly);
            try { first = new FavouriteService(path); second = new FavouriteService(path); }
            finally { File.SetAttributes(path, FileAttributes.Normal); }
            var initialIds = first.All().Select(f => f.Id).ToList();
            Check(initialIds.All(id => id != Guid.Empty) && initialIds.Distinct().Count() == initialIds.Count &&
                initialIds.SequenceEqual(second.All().Select(f => f.Id)) && File.ReadAllText(path) == before,
                "Legacy identities must agree across tabs, including identical records, without writing on read.");
            var reverse = first.All().OrderBy(f => f.Index).Reverse().ToList();
            Check(second.TryReorder(reverse), "Legacy buttons must still reorder against a fresh service.");
            first = new FavouriteService(path);
            first.Edit(initialIds[0], "Migrated", "https://example.test/migrated");
            first.SaveChanges();
            Check(new FavouriteService(path).All().Select(f => f.Id).SequenceEqual(initialIds),
                "First-save IDs must survive edits and subsequent reloads.");

            // Imports containing reused IDs are separated too; keep the first ID.
            var imported = JsonConvert.DeserializeObject<FavouriteModel[]>(File.ReadAllText(path));
            imported[1].Id = imported[0].Id;
            File.WriteAllText(path, JsonConvert.SerializeObject(imported));
            first = new FavouriteService(path); second = new FavouriteService(path);
            Check(first.All().Select(f => f.Id).Distinct().Count() == first.All().Count &&
                first.All().Select(f => f.Id).SequenceEqual(second.All().Select(f => f.Id)) && first.All()[0].Id == initialIds[0],
                "Repeated imported IDs must be repaired consistently without conflating records.");
        });
    }

    private static void DuplicateButtonIdentity()
    {
        using (var row = new Row(90, 90))
        {
            Guid profile = Guid.NewGuid();
            var expected = new[] { Favourite(profile, "Same", 0), Favourite(profile, "Same", 1) };
            row.Buttons[0].Tag = expected[0]; row.Buttons[1].Tag = expected[1];
            Check(FavouriteService.ValidateButtons(row.Bar, expected, false, address => null),
                "Two identical favourites must be valid distinct buttons.");
            row.Bar.Controls.SetChildIndex(row.Buttons[1], 0);
            Check(!FavouriteService.ValidateButtons(row.Bar, expected, false, address => null),
                "Refresh validation must detect swapped identical favourites by identity.");
            Check(row.Bar.TryAnimateOrder(new Control[] { row.Buttons[0], row.Buttons[1] }),
                "Identical labels must still support animated sorting with distinct controls.");
            row.Settle();
            Check(row.Bar.Controls[0] == row.Buttons[0] && row.Bar.Controls[1] == row.Buttons[1],
                "Sorting identical labels must retain both buttons.");
        }
    }

    private static void Validation()
    {
        using (var row = new Row(90, 90))
        {
            Guid profile = Guid.NewGuid();
            var expected = new[] { Favourite(profile, "A", 0), Favourite(profile, "B", 1) };
            row.Buttons[0].Tag = expected[0]; row.Buttons[1].Tag = expected[1];
            Func<string, Image> noIcons = address => { throw new Exception("Hidden icons must not be loaded."); };
            Check(FavouriteService.ValidateButtons(row.Bar, expected, false, noIcons), "Matching order without icons should validate.");
            row.Bar.Controls.SetChildIndex(row.Buttons[1], 0);
            Check(!FavouriteService.ValidateButtons(row.Bar, expected, false, noIcons), "A stale tab with a different order must refresh.");
            row.Bar.Controls.SetChildIndex(row.Buttons[0], 0);
            using (var icon = new Bitmap(16, 16))
            {
                row.Buttons[0].Image = icon;
                Check(!FavouriteService.ValidateButtons(row.Bar, expected, false, noIcons), "Disabling icons must refresh existing icons.");
                row.Buttons[0].Image = null;
            }
            row.Start(0);
            Check(!FavouriteService.ValidatePanelAsync(row.Bar).Result, "Refresh must be deferred during pointer interaction.");
            row.Bar.CancelInteraction();
        }
    }
}
