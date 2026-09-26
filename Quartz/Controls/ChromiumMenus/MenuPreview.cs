using SkiaSharp;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace Quartz.Controls.ChromiumMenus
{
    // Separate executable entry avoids Program's profile initialization and browser/WebView creation.
    public static class MenuPreview
    {
        public static int Run(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (new MenuSession.DpiScope())
            {
                if (args.Length > 0 && args[0] == "--verify")
                {
                    string directory = Path.GetFullPath(args.Length > 1 ? args[1] : "menu-verification");
                    Directory.CreateDirectory(directory);
                    try { Verify(directory); return 0; }
                    catch (Exception e) { File.WriteAllText(Path.Combine(directory, "failure.txt"), e.ToString()); return 1; }
                }
                Application.Run(new PreviewForm()); return 0;
            }
        }
        private sealed class Fixture : IDisposable
        {
            internal ChromiumMenu Menu;
            internal ChromiumZoomMenuItem Zoom;
            internal ChromiumMenuItem Plain, Nested, Disabled;
            internal double Factor = 1;
            internal int Commands, ZoomCommands, FullscreenCommands;
            internal bool Fullscreen, ClosedBeforeFullscreen;
            internal Fixture(MenuAppearance appearance)
            {
                Menu = new ChromiumMenu { Appearance = appearance, Name = "preview" };
                Plain = new ChromiumMenuItem("&Open link in new tab") { ShortcutKeyDisplayString = "Ctrl+T" };
                Plain.Click += (s, e) => ++Commands;
                Disabled = new ChromiumMenuItem("Copy image address") { Enabled = false };
                Nested = new ChromiumMenuItem("&Spelling") { ShortcutKeyDisplayString = "Ctrl+Shift+S" };
                Nested.DropDownItems.Add(new ChromiumMenuItem("English (United States)") { Radio = true, Checked = true });
                Nested.DropDownItems.Add(new ChromiumMenuItem("English (United Kingdom)") { Radio = true });
                foreach (var language in Nested.DropDownItems.ToArray())
                    language.Click += (sender, args) => { foreach (var option in Nested.DropDownItems.Where(i => i.Radio)) option.Checked = option == language; };
                Nested.DropDownItems.Add(new ChromiumMenuSeparator());
                Nested.DropDownItems.Add(new ChromiumMenuItem("Check spelling") { CheckOnClick = true, Checked = true });
                Zoom = new ChromiumZoomMenuItem
                {
                    GetZoom = () => Factor, Step = direction => { Factor = ChromiumZoomMenuItem.NextFactor(Factor, direction); ++ZoomCommands; },
                    IsFullscreen = () => Fullscreen,
                    Fullscreen = () => { ClosedBeforeFullscreen = MenuSession.Current == null; Fullscreen = !Fullscreen; ++FullscreenCommands; }
                };
                Menu.Items.AddRange(new ChromiumMenuItem[] { Plain, Disabled, new ChromiumMenuSeparator(), new ChromiumMenuItem("Show controls") { Checked = true, CheckOnClick = true }, Nested,
                    new ChromiumMenuSeparator(), Zoom, new ChromiumMenuSeparator(), new ChromiumMenuItem("Inspect") { ShortcutKeyDisplayString = "Ctrl+Shift+I" },
                    new ChromiumMenuItem("Literal && ampersand"), new ChromiumMenuItem("日本語 • العربية • हिन्दी • 😀"), new ChromiumMenuItem("A secondary title") { SecondaryText = "Additional information" } });
            }
            public void Dispose() { Menu.Dispose(); }
        }
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        private static void Verify(string directory)
        {
            var report = new StringBuilder("Chromium 154.0.8037.58 / a654841425914cbb703a2931e07b70a83aedbafd\r\n");
            report.AppendLine($"Windows={Environment.OSVersion}; locale={CultureInfo.CurrentUICulture.Name}; UWP text scale={MenuPlatform.AccessibilityScale}; 64-bit={Environment.Is64BitProcess}");
            using (var text = new MenuText())
            {
                report.AppendLine($"Menu font: {text.Font.Typeface.FamilyName}, {text.Font.Size} DIP; height={text.Height}, ascent={text.Ascent}, cap={text.Cap}");
                foreach (bool rounded in new[] { false, true })
                foreach (bool dark in new[] { false, true })
                foreach (float scale in new[] { 1, 1.25f, 1.5f, 1.75f, 2 })
                {
                    var appearance = new MenuAppearance { RoundedIcons = rounded, Dark = dark, DarkNeutrals26 = rounded, HighContrast = false };
                    using (var fixture = new Fixture(appearance))
                    {
                        var layout = new MenuLayout(fixture.Menu, text); var painter = new MenuPainter(text, appearance);
                        var row = layout.Rows.Single(r => r.Item == fixture.Zoom);
                        Require(row.Buttons[0].Right == row.Percentage.Left && row.Percentage.Right == row.Buttons[1].Left && row.Buttons[1].Right == row.Buttons[2].Left && row.Buttons[2].Right == layout.Width, "Zoom hit areas must abut and end at body edge");
                        Require(row.Percentage.Width >= ChromiumZoomMenuItem.Factors.Max(f => text.Width(MenuText.Percent((int)(f * 100 + .5)))) + 4, "Readout clips a preset");
                        foreach (int state in new[] { -1, 0, 1, 2, 3 })
                        {
                            fixture.Factor = state == 3 ? .25 : 1;
                            string name = $"{(rounded ? "developer" : "cpp-defaults")}-{(dark ? "dark" : "light")}-{scale * 100:0}-{(state == -1 ? "normal" : state == 3 ? "minimum" : "hot-" + state)}";
                            Save(directory, name, painter, layout, scale, state == -1 ? fixture.Plain : fixture.Zoom, state == 3 ? -1 : state);
                        }
                    }
                }
                using (var fixture = new Fixture(new MenuAppearance { RightToLeft = true, Dark = true }))
                { Save(directory, "rtl-150", new MenuPainter(text, fixture.Menu.Appearance), new MenuLayout(fixture.Menu, text), 1.5f, fixture.Zoom, 2); }
                using (var fixture = new Fixture(new MenuAppearance { Grayscale = true, HighContrast = true }))
                { Save(directory, "system-contrast", new MenuPainter(text, fixture.Menu.Appearance), new MenuLayout(fixture.Menu, text), 1, fixture.Zoom, 1); }
                // Hidden icons contribute to the shared column; hidden labels do not widen the menu.
                using (var menu = new ChromiumMenu())
                {
                    menu.Items.Add(new ChromiumMenuItem("One")); var plain = new MenuLayout(menu, text);
                    menu.Items.Add(new ChromiumMenuItem(new string('w', 100)) { Visible = false, VectorIcon = "zoom_in" });
                    var hidden = new MenuLayout(menu, text);
                    Require(hidden.LabelX - plain.LabelX == 28 && hidden.Width - plain.Width == 28, "Hidden icon layout contribution");
                }
                Require(Math.Abs(ChromiumZoomMenuItem.NextFactor(1.0001, 1) - 1.1) < 1e-10, "Zoom level epsilon up");
                Require(Math.Abs(ChromiumZoomMenuItem.NextFactor(.9999, -1) - .9) < 1e-10, "Zoom level epsilon down");
                Require(ChromiumZoomMenuItem.NextFactor(.25, -1) == .25 && ChromiumZoomMenuItem.NextFactor(5, 1) == 5, "Zoom endpoints");
                Require(ChromiumZoomMenuItem.NextFactor(1.1, 1, 1.2) == 1.2 && ChromiumZoomMenuItem.NextFactor(1.25, -1, 1.2) == 1.2, "Profile default inserted in level-space presets");
                Require(text.Width("office العربية हिन्दी") > 0, "Shaping result");
                using (var bitmap = new Bitmap(32, 40))
                using (var menu = new ChromiumMenu())
                {
                    var big = new ChromiumMenuItem("Tall image") { Image = bitmap, ImageSize = new Size(32, 40) };
                    var check = new ChromiumMenuItem("Unchecked") { Checked = false };
                    var checkedIcon = new ChromiumMenuItem("Extra icon") { Checked = true, Image = bitmap, ImageSize = new Size(32, 40) };
                    menu.Items.AddRange(new[] { big, check, checkedIcon });
                    var layout = new MenuLayout(menu, text);
                    Require(layout.Column == 32 && layout.Rows[0].Height == 52 && layout.Rows[1].Height == Math.Max(16, text.Height) + 12, "Tall icon only increases its own row height");
                    Require(layout.Rows[2].TitleX == layout.LabelX + 44, "Additional check icon must not enlarge shared column");
                    Save(directory, "wide-tall-icons", new MenuPainter(text, new MenuAppearance()), layout, 1.5f, check, -1);
                }
                using (var menu = new ChromiumMenu())
                {
                    foreach (MenuSeparatorKind kind in Enum.GetValues(typeof(MenuSeparatorKind)))
                    { menu.Items.Add(new ChromiumMenuItem(kind.ToString())); menu.Items.Add(new ChromiumMenuSeparator { Kind = kind }); }
                    Save(directory, "separator-types", new MenuPainter(text, new MenuAppearance()), new MenuLayout(menu, text), 2, null, -1);
                }
            }
            var previousCulture = CultureInfo.CurrentUICulture;
            try
            {
                foreach (string locale in new[] { "en-US", "ar", "hi", "ja" })
                {
                    CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(locale);
                    using (var text = new MenuText())
                    using (var fixture = new Fixture(new MenuAppearance { HighContrast = false, Animations = false }))
                    {
                        report.AppendLine($"Locale {locale}: percent='{MenuText.Percent(125)}', font={text.Font.Size}, height={text.Height}");
                        Save(directory, "locale-" + locale, new MenuPainter(text, fixture.Menu.Appearance), new MenuLayout(fixture.Menu, text), 1.5f, fixture.Zoom, 1);
                    }
                }
            }
            finally { CultureInfo.CurrentUICulture = previousCulture; }
            using (var owner = new Form { Text = "Quartz menu verification", Size = new Size(760, 700), StartPosition = FormStartPosition.CenterScreen })
            using (var fixture = new Fixture(new MenuAppearance { HighContrast = false, Animations = false }))
            {
                owner.Show(); owner.Hide(); owner.Show(); owner.Activate(); Application.DoEvents();
                fixture.Menu.Show(owner, new Point(70, 70));
                Require(fixture.Menu.Visible, "Native popup failed to open");
                var popup = Application.OpenForms.OfType<MenuPopup>().Single();
                Require(popup.AccessibilityObject.GetChildCount() == popup.LayoutData.Rows.Count, "Accessibility row count");
                var zoomRow = popup.LayoutData.Rows.Single(r => r.Item == fixture.Zoom);
                var zoomAccessible = popup.AccessibilityObject.GetChild(popup.LayoutData.Rows.IndexOf(zoomRow));
                Require(ReferenceEquals(zoomAccessible, popup.AccessibilityObject.GetChild(popup.LayoutData.Rows.IndexOf(zoomRow))) && ReferenceEquals(zoomAccessible.GetChild(2), zoomAccessible.GetChild(2)), "Accessibility node identity must remain stable");
                Require(zoomAccessible.GetChild(1).Role == AccessibleRole.Alert && zoomAccessible.GetChild(1).DefaultAction == "", "Percentage accessibility must be informational alert");
                popup.Selected = fixture.Zoom; popup.Part = 1;
                SendKey(Keys.Enter);
                Require(fixture.ZoomCommands == 1 && fixture.Factor == 1.1 && fixture.Menu.Visible, "Enter zoom must invoke once and keep menu open");
                popup.Part = 0; SendKey(Keys.Space);
                Require(fixture.ZoomCommands == 2 && fixture.Factor == 1 && fixture.Menu.Visible, "Space zoom must keep menu open");
                fixture.Factor = .25; popup.Part = 0; SendKey(Keys.Enter);
                Require(fixture.ZoomCommands == 2, "Disabled zoom must not invoke");
                SendKey(Keys.Down); Require(popup.Selected == fixture.Zoom && popup.Part == 1, "Endpoint navigation skips the now-disabled minus");
                popup.Part = 1; SendKey(Keys.Down); Require(popup.Part == 2, "Down traverses plus to fullscreen");
                SendKey(Keys.Up); Require(popup.Part == 1, "Up traverses fullscreen to plus");
                SendKey(Keys.Left); Require(popup.Part == 1 && fixture.ZoomCommands == 2, "Left must not change zoom or child selection");
                SendKey(Keys.F11); Require(fixture.FullscreenCommands == 0 && fixture.Menu.Visible, "F11 is consumed while menu is open");
                popup.Selected = fixture.Zoom; popup.Part = -1; SendKey(Keys.Space);
                Require(fixture.ZoomCommands == 2, "Percentage has no reset command");
                popup.Selected = fixture.Nested; popup.Part = -1; SendKey(Keys.Right);
                Require(Application.OpenForms.OfType<MenuPopup>().Count() == 2, "Submenu must open in a separate host");
                SendKey(Keys.Escape); Require(fixture.Menu.Visible && Application.OpenForms.OfType<MenuPopup>().Count() == 1, "Escape closes just the submenu");
                popup.Selected = fixture.Zoom; popup.Part = 2; SendKey(Keys.Enter); Application.DoEvents();
                Require(fixture.FullscreenCommands == 1 && fixture.ClosedBeforeFullscreen && !fixture.Menu.Visible, "Fullscreen must close before command");
                fixture.Menu.Show(owner, new Point(owner.ClientSize.Width - 2, owner.ClientSize.Height - 2));
                popup = Application.OpenForms.OfType<MenuPopup>().Single();
                Require(Screen.FromHandle(popup.Handle).WorkingArea.Contains(popup.BodyBounds), "Visible menu body must fit work area");
                popup.Selected = fixture.Plain; popup.Part = -1; SendKey(Keys.Enter); Application.DoEvents();
                Require(fixture.Commands == 1 && !fixture.Menu.Visible, "Ordinary action must execute once and dismiss");
                fixture.Zoom.CanFullscreen = () => false;
                Require(fixture.Zoom.ButtonEnabled(2), "Already-fullscreen must permit exit even when entry is forbidden");
                fixture.Fullscreen = false;
                Require(!fixture.Zoom.ButtonEnabled(2), "Forbidden fullscreen entry");
                fixture.Menu.Show(owner, new Point(70, 70));
                popup = Application.OpenForms.OfType<MenuPopup>().Single();
                popup.Selected = fixture.Disabled; SendKey(Keys.Enter);
                Require(fixture.Menu.Visible && fixture.Commands == 1, "Disabled ordinary item must not dismiss or execute");
                SendKey(Keys.Home); Require(popup.Selected == fixture.Plain, "Home selects first enabled item");
                SendKey(Keys.End); Require(popup.Selected == fixture.Menu.Items.Last(), "End selects last enabled item");
                SendKey(Keys.Escape);
                using (var longMenu = new ChromiumMenu { Appearance = new MenuAppearance { Animations = false } })
                {
                    for (int i = 0; i < 90; ++i) longMenu.Items.Add(new ChromiumMenuItem("Scrollable item " + i));
                    longMenu.Show(owner, new Point(100, 100)); popup = Application.OpenForms.OfType<MenuPopup>().Single();
                    Require(popup.ScrollArrowHeight > 0, "Oversized menu must scroll");
                    SendKey(Keys.End); Require(popup.Scroll > 0 && popup.Selected == longMenu.Items.Last(), "Keyboard selection scrolls the last row into view");
                    Require(popup.BodyBounds.Contains(popup.RowBounds(popup.LayoutData.Rows.Last())), "Last row lies in visible menu body");
                    SendKey(Keys.Home); Require(popup.Scroll <= 12, "Home restores the first visible row");
                    SendKey(Keys.Escape);
                }
                // Exercise a command that enters ShowDialog through the real filter
                // pipeline. A second menu must receive its own messages inside it.
                VerifyModalMenus(owner, report);
                VerifyTooltipAndFade(owner, report);
                VerifyInputLifetime(owner, report);
                VerifySharedSubmenu(owner, report);
                owner.Close();
            }
            report.AppendLine("PASS: render matrix; shaped metrics; hidden/icon/check columns; tall icons; separator variants; localized preset width and font overrides; zoom endpoints/epsilon; native layered popup; stable accessibility tree and alert role; zoom keyboard invocation/disabled state; child traversal; F11 consumption; informational percentage; submenu open/Escape; fullscreen close-before-command and capability/exit state; work-area placement; ordinary command execution; Home/End; oversized-menu scrolling.");
            report.AppendLine("Images are renderer outputs, not a Chromium pixel comparison. Physical mouse, Narrator and mixed-monitor checks remain separate.");
            File.WriteAllText(Path.Combine(directory, "verification.txt"), report.ToString());
        }
        private static void PumpFor(int milliseconds)
        {
            var time = Stopwatch.StartNew();
            while (time.ElapsedMilliseconds < milliseconds) { Application.DoEvents(); System.Threading.Thread.Sleep(5); }
        }
        private static void VerifyModalMenus(Form owner, StringBuilder report)
        {
            bool entered = false, invoked = false, checkedState = false; Exception failure = null;
            using (var root = new ChromiumMenu { Appearance = new MenuAppearance { Animations = false } })
            {
                var open = root.Items.Add("Open modal dialog");
                open.Click += (s, e) =>
                {
                    entered = true;
                    using (var modal = new Form { Text = "Modal menu regression fixture", Size = new Size(420, 380) })
                    using (var menu = new ChromiumMenu { Appearance = new MenuAppearance { Animations = false } })
                    using (var timeout = new Timer { Interval = 2000 })
                    {
                        var button = new Button { Text = "Profile fixture", Location = new Point(40, 50), Size = new Size(130, 50) };
                        modal.Controls.Add(button); menu.Attach(button);
                        var item = menu.Items.Add("Invoke owned menu command"); item.Click += (ss, ee) => { invoked = true; modal.Close(); };
                        var nested = menu.Items.Add("Nested fixture"); nested.DropDownItems.Add(new ChromiumMenuItem("Edit fixture"));
                        modal.Shown += (ss, ee) => modal.BeginInvoke((Action)(() =>
                        {
                            // Native controls send WM_CONTEXTMENU during their own
                            // right-up handling. Its deferred open must survive.
                            SendMessage(button.Handle, 0x7b, button.Handle, new IntPtr(-1));
                            button.BeginInvoke((Action)(() =>
                            {
                                try
                                {
                                    var popup = Application.OpenForms.OfType<MenuPopup>().Single(p => !p.Retired);
                                    checkedState = popup.Owner == modal && popup.Enabled && GetCapture() == popup.Handle;
                                    // Simulate zero-extended DWORD accessibility IDs on x64.
                                    if (IntPtr.Size == 8) SendMessage(popup.Handle, 0x3d, IntPtr.Zero, new IntPtr(0xfffffffcL));
                                    PostMessage(popup.Handle, 0x100, new IntPtr((int)Keys.Down), IntPtr.Zero);
                                    PostMessage(popup.Handle, 0x100, new IntPtr((int)Keys.Enter), IntPtr.Zero);
                                }
                                catch (Exception ex) { failure = ex; modal.Close(); }
                            }));
                        }));
                        timeout.Tick += (ss, ee) => modal.Close(); timeout.Start(); modal.ShowDialog(owner);
                    }
                };
                root.Show(owner, new Point(60, 60));
                var host = Application.OpenForms.OfType<MenuPopup>().Single(p => !p.Retired);
                PostMessage(host.Handle, 0x100, new IntPtr((int)Keys.Down), IntPtr.Zero);
                PostMessage(host.Handle, 0x100, new IntPtr((int)Keys.Enter), IntPtr.Zero);
                PumpFor(200);
                if (failure != null) throw failure;
                Require(entered && invoked && checkedState, "Modal menu command routing/capture failed");
            }
            report.AppendLine("PASS: queued ordinary command opens a modal dialog; deferred WM_CONTEXTMENU from a native button; enabled owned popup/capture; nested modal message filter receives keyboard input; command invokes once; x64 UIA object ID does not overflow.");
        }
        private static void VerifyTooltipAndFade(Form owner, StringBuilder report)
        {
            using (var fixture = new Fixture(new MenuAppearance { Animations = false }))
            using (var tip = new MenuTooltip())
            {
                fixture.Menu.Show(owner, new Point(80, 80));
                var popup = Application.OpenForms.OfType<MenuPopup>().Single(p => !p.Retired);
                var row = popup.LayoutData.Rows.Single(r => r.Item == fixture.Zoom);
                var point = new Point(popup.BodyBounds.X + 100, popup.BodyBounds.Y + 30);
                tip.Move(popup, row, 0, point, false, 0); tip.Tick(499);
                Require(!tip.Visible && tip.Pending, "Tooltip must wait 500 ms");
                point.Offset(7, 5); tip.Move(popup, row, 0, point, false, 300); tip.Tick(500);
                Require(tip.Visible, "Moving inside a target must not restart the tooltip timer");
                var window = Application.OpenForms.OfType<MenuTooltip.TooltipWindow>().Single();
                Require(window.Left == point.X + popup.Pixel(10) && window.Top == point.Y + popup.Pixel(15), "Tooltip tracks pending pointer with 10/15 DIP offset");
                tip.Move(popup, row, 1, point, false, 600); tip.Tick(1099); Require(!tip.Visible, "Adjacent tooltip must also wait full delay");
                tip.Tick(1100); Require(tip.Visible, "Next tooltip appears"); tip.Tick(11099); Require(tip.Visible, "Tooltip lifetime is 10 s");
                tip.Tick(11100); Require(!tip.Visible, "Tooltip times out after 10 s");
                tip.Move(popup, row, 2, point, false, 12000); tip.Press(); tip.Move(popup, row, 2, point, false, 13000); tip.Tick(15000);
                Require(!tip.Visible && !tip.Pending, "Pressed target suppresses tooltip until another target");
                tip.Move(popup, row, 0, point, false, 16000); tip.Tick(16500); Require(tip.Visible, "Leaving pressed target rearms tooltip");
                tip.Reset(); Require(!tip.Visible && !tip.Pending, "Keyboard/scroll/dismiss cancels tooltip");
                var display = new Rectangle(0, 0, 800, 600);
                Require(MenuTooltip.TooltipWindow.Place(new Size(100, 30), new Point(790, 595), display, 1, false) == new Point(700, 565), "Tooltip screen-edge fit/flip");
                Require(MenuTooltip.TooltipWindow.Place(new Size(100, 30), new Point(300, 100), display, 1.5f, true) == new Point(200, 123), "RTL tooltip position");
                fixture.Menu.Close();
            }
            using (var text = new MenuText(tooltip: true))
            {
                foreach (var value in new[] { "Long title with words wrapping across lines", new string('x', 200), "日本語のツールチップ", "العربية العربية العربية", "a\tb\nc" })
                    Require(MenuTooltip.TooltipWindow.Wrap(value.Replace("\t", "        "), text, 90).All(line => text.Width(line) <= 90), "Tooltip wrapping overflow");
                Require(MenuTooltip.TooltipWindow.Truncate(new string('x', 2000)).Length == 1024, "Windows tooltip truncation");
            }
            Require(MenuFadeAnimation.Value(0, 1, 75) == .5 && MenuFadeAnimation.Value(1, 0, 150) == 0, "150 ms linear fade curve");
            using (var fixture = new Fixture(new MenuAppearance { Animations = true }))
            {
                fixture.Menu.Show(owner, new Point(80, 80)); PumpFor(180);
                var popup = Application.OpenForms.OfType<MenuPopup>().Single(p => !p.Retired);
                Require(popup.SurfaceAlpha == 255, "Fade-in completes"); fixture.Menu.Close();
                Require(!fixture.Menu.Visible && MenuSession.Current == null && popup.IsDisposed, "Dismissal destroys the popup immediately, with no closing fade");
            }
            report.AppendLine("PASS: tooltip delay/anchor update/target change/10-second timeout/click suppression/cancellation/edge fit/RTL/wrapping/truncation; 150 ms linear opening fade; immediate close/input/capture release. OS animation permission=" + MenuFadeAnimation.SystemEnabled);
        }
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        [DllImport("user32.dll")] private static extern IntPtr GetCapture();
        [DllImport("user32.dll")] private static extern bool IsWindowEnabled(IntPtr hwnd);
        private sealed class PointerProbe : Control
        {
            internal int LastMessage, Count, Hit = 1;
            internal IntPtr LastFlags, LastPoint;
            protected override void WndProc(ref Message m)
            {
                if (m.Msg == 0x84) { m.Result = new IntPtr(Hit); return; }
                if (m.Msg == 0x201 || m.Msg == 0xa1)
                { LastMessage = m.Msg; LastFlags = m.WParam; LastPoint = m.LParam; ++Count; return; }
                base.WndProc(ref m);
            }
        }
        private static void VerifyInputLifetime(Form owner, StringBuilder report)
        {
            using (var field = new TextBox { Text = "Caret test", Bounds = new Rectangle(15, 15, 200, 24) })
            using (var probe = new PointerProbe { Bounds = new Rectangle(15, 80, 160, 40) })
            using (var fixture = new Fixture(new MenuAppearance { Animations = false }))
            {
                owner.Controls.Add(field); owner.Controls.Add(probe); field.CreateControl(); probe.CreateControl(); owner.Activate(); Application.DoEvents(); field.Focus(); field.SelectionStart = 3;
                Require(field.Focused, "Caret fixture must have focus before opening: owner enabled=" + owner.Enabled + ", field enabled=" + field.Enabled + ", owner visible=" + owner.Visible + ", field visible=" + field.Visible + ", can focus=" + field.CanFocus + ", handle=" + field.IsHandleCreated + ", native owner=" + IsWindowEnabled(owner.Handle) + ", native field=" + IsWindowEnabled(field.Handle));
                Require(CreateCaret(field.Handle, IntPtr.Zero, 1, 16) && ShowCaret(field.Handle), "Test native caret creation");
                var gui = new GuiInfo { Size = Marshal.SizeOf(typeof(GuiInfo)) };
                GetGUIThreadInfo(0, ref gui); Require((gui.Flags & 1) != 0, "Test caret is initially visible");
                fixture.Menu.Show(field, new Point(230, 60));
                var popup = Application.OpenForms.OfType<MenuPopup>().Single(p => !p.Retired);
                GetGUIThreadInfo(0, ref gui); Require((gui.Flags & 1) == 0 && field.Focused && field.SelectionStart == 3, "Menu hides caret without changing focus/selection: flags=" + gui.Flags + ", focused=" + field.Focused + ", selection=" + field.SelectionStart + ", caret=" + gui.Caret + ", hidden=" + typeof(MenuSession).GetField("hiddenCaret", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(MenuSession.Current));
                Require(EasyTabs.ContextMenuProvider.IsOwnedMenuOpen(), "Tab hooks must know menu owns input");
                // Every click, including WM_LBUTTONDBLCLK, must activate immediately.
                var row = popup.LayoutData.Rows.Single(r => r.Item == fixture.Zoom);
                var plus = row.Buttons[1];
                var local = new Point(popup.Pixel(popup.LeftInset + plus.Left + 3), popup.Pixel(popup.TopInset + plus.MidY));
                fixture.Zoom.GetZoom = () => 1; fixture.Zoom.Step = d => ++fixture.ZoomCommands;
                var elapsed = Stopwatch.StartNew();
                for (int i = 0; i < 20; ++i)
                {
                    var down = Message.Create(popup.Handle, i % 2 == 0 ? 0x201 : 0x203, new IntPtr(1), MenuSession.PointParam(local));
                    Require(MenuSession.Current.PreFilterMessage(ref down), "Menu consumes each press");
                    var up = Message.Create(popup.Handle, 0x202, IntPtr.Zero, MenuSession.PointParam(local));
                    Require(MenuSession.Current.PreFilterMessage(ref up) && fixture.ZoomCommands == i + 1, "Rapid click must immediately invoke exactly once");
                }
                report.AppendLine("20 rapid press/release pairs, alternating ordinary and double-click presses: " + elapsed.ElapsedMilliseconds + " ms; all 20 invoked synchronously.");
                // An outside press over nonclient caption must preserve its native
                // hit code and screen coordinate; then a client press uses local coordinates.
                probe.Hit = 2; var screen = probe.PointToScreen(new Point(7, 9));
                var captured = popup.PointToClient(screen);
                var outside = Message.Create(popup.Handle, 0x201, new IntPtr(1), MenuSession.PointParam(captured));
                MenuSession.Current.PreFilterMessage(ref outside); Application.DoEvents();
                Require(probe.Count == 1 && probe.LastMessage == 0xa1 && (int)probe.LastFlags == 2 && probe.LastPoint == MenuSession.PointParam(screen), "Dismissal repost preserves caption drag press");
                GetGUIThreadInfo(0, ref gui); Require((gui.Flags & 1) != 0 && field.Focused && !EasyTabs.ContextMenuProvider.IsOwnedMenuOpen(), "Dismiss restores caret and relinquishes input");
                probe.Hit = 1; fixture.Menu.Show(field, new Point(230, 60)); popup = Application.OpenForms.OfType<MenuPopup>().Single(p => !p.Retired);
                captured = popup.PointToClient(screen); outside = Message.Create(popup.Handle, 0x201, new IntPtr(1), MenuSession.PointParam(captured));
                MenuSession.Current.PreFilterMessage(ref outside); Application.DoEvents();
                Require(probe.Count == 2 && probe.LastMessage == 0x201 && probe.LastPoint == MenuSession.PointParam(new Point(7, 9)), "Dismissal repost preserves client click");
                fixture.Menu.Show(field, new Point(230, 60)); probe.Capture = true;
                Require(MenuSession.Current == null && probe.Capture, "Capture loss must not release the new owner's capture"); probe.Capture = false;
                DestroyCaret(); owner.Controls.Remove(field); owner.Controls.Remove(probe);
            }
            report.AppendLine("PASS: native caret hidden/restored with focus and selection preserved; tab-hook input ownership; immediate rapid zoom including double-clicks/full rectangular padding; same-thread client/nonclient dismissal replay; capture handoff preserves new owner.");
        }
        private static void VerifySharedSubmenu(Form owner, StringBuilder report)
        {
            using (var menu = new ChromiumMenu { Appearance = new MenuAppearance { Animations = false } })
            using (var shared = new ChromiumMenu())
            {
                int chosen = 0; shared.Items.Add("Edit item").Click += (s, e) => chosen = (int)shared.Tag;
                var first = menu.Items.Add("First item"); var second = menu.Items.Add("Second item");
                first.DropDown = second.DropDown = shared;
                first.DropDownOpening += (s, e) => shared.Tag = 1; second.DropDownOpening += (s, e) => shared.Tag = 2;
                menu.Show(owner, new Point(70, 70)); var popup = Application.OpenForms.OfType<MenuPopup>().Single(p => !p.Retired);
                popup.Selected = first; SendKey(Keys.Right); Require((int)shared.Tag == 1, "First item captures its context");
                popup.Selected = second;
                // Open the other item's shared submenu without first dismissing it.
                var open = typeof(MenuSession).GetMethod("OpenSubmenu", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                open.Invoke(MenuSession.Current, new object[] { popup, popup.LayoutData.Rows.Single(r => r.Item == second), true });
                Require((int)shared.Tag == 2 && shared.OwnerItem == second, "Shared submenu must refresh context when parent item changes");
                SendKey(Keys.Enter); Application.DoEvents(); Require(chosen == 2, "Shared submenu action uses the selected item");
            }
            using (var fixture = new Fixture(new MenuAppearance { Animations = false }))
            {
                fixture.Menu.Show(owner, new Point(70, 70)); var popup = Application.OpenForms.OfType<MenuPopup>().Single(p => !p.Retired);
                popup.Selected = fixture.Nested; SendKey(Keys.Right); SendKey(Keys.Down); SendKey(Keys.Enter); Application.DoEvents();
                Require(!fixture.Nested.DropDownItems[0].Checked && fixture.Nested.DropDownItems[1].Checked, "Language radio selection remains exclusive");
                fixture.Menu.Show(owner, new Point(70, 70)); popup = Application.OpenForms.OfType<MenuPopup>().Single(p => !p.Retired);
                popup.Selected = fixture.Nested; SendKey(Keys.Right); SendKey(Keys.End); SendKey(Keys.Enter); Application.DoEvents();
                Require(!fixture.Nested.DropDownItems.Last().Checked, "Checkbox toggles once and retains its value");
            }
            report.AppendLine("PASS: shared item dropdown switches invocation context; language radio items select exclusively; check items toggle once; nested keyboard navigation preserves state.");
        }
        [StructLayout(LayoutKind.Sequential)] private struct GuiInfo { internal int Size, Flags; internal IntPtr Active, Focus, Capture, Menu, Move, Caret; internal Rectangle Rect; }
        [DllImport("user32.dll")] private static extern bool GetGUIThreadInfo(uint thread, ref GuiInfo info);
        [DllImport("user32.dll")] private static extern bool CreateCaret(IntPtr hwnd, IntPtr bitmap, int width, int height);
        [DllImport("user32.dll")] private static extern bool ShowCaret(IntPtr hwnd);
        [DllImport("user32.dll")] private static extern bool DestroyCaret();
        private static void Save(string directory, string name, MenuPainter painter, MenuLayout layout, float scale, ChromiumMenuItem selected, int part)
        {
            using (var bitmap = new SKBitmap((int)Math.Ceiling((layout.Width + 48) * scale), (int)Math.Ceiling((layout.Height + 48) * scale), SKColorType.Bgra8888, SKAlphaType.Premul))
            using (var props = MenuPlatform.SurfaceProperties())
            using (var surface = SKSurface.Create(bitmap.Info, bitmap.GetPixels(), bitmap.RowBytes, props))
            {
                painter.Paint(surface.Canvas, layout, false, scale, layout.Height, 0, selected, part, true);
                using (var image = SKImage.FromBitmap(bitmap)) using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (var file = File.Create(Path.Combine(directory, name + ".png"))) data.SaveTo(file);
            }
        }
        private static void SendKey(Keys key)
        {
            var message = Message.Create(IntPtr.Zero, 0x100, new IntPtr((int)key), IntPtr.Zero);
            Require(MenuSession.FilterKeys(ref message), "Menu must consume key " + key);
        }
        private sealed class PreviewForm : Form
        {
            private Fixture fixture;
            private readonly CheckBox dark = new CheckBox { Text = "Dark", AutoSize = true };
            private readonly CheckBox defaults = new CheckBox { Text = "C++ feature defaults", AutoSize = true };
            private readonly CheckBox rtl = new CheckBox { Text = "Right to left", AutoSize = true };
            private readonly CheckBox grayscale = new CheckBox { Text = "Grayscale", AutoSize = true };
            private readonly CheckBox noFullscreen = new CheckBox { Text = "Forbid fullscreen entry", AutoSize = true };
            private readonly CheckBox noZoom = new CheckBox { Text = "Disable zoom", AutoSize = true };
            private readonly NumericUpDown zoom = new NumericUpDown { Minimum = 25, Maximum = 500, Value = 100, Width = 65 };
            private readonly Label status = new Label { Dock = DockStyle.Bottom, Height = 42, Padding = new Padding(12), Text = "No commands invoked." };
            internal PreviewForm()
            {
                Text = "Chromium 154 menu preview"; Size = new Size(850, 750);
                var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 64 };
                panel.Controls.AddRange(new Control[] { dark, defaults, rtl, grayscale, noFullscreen, noZoom, new Label { Text = "Zoom %", AutoSize = true }, zoom }); Controls.Add(panel);
                var text = new Label { Dock = DockStyle.Fill, Text = "Right-click here to open the menu.\r\n\r\nZoom and fullscreen only change this fixture.\r\nThe percentage is informational. Use Up/Down and Enter/Space.\r\n\r\nReference: Chromium 154.0.8037.58\r\nAppMenuGlowUp off; baseline colours; 150 ms fade.", Padding = new Padding(24, 90, 24, 24) };
                Controls.Add(text); Controls.Add(status); text.BringToFront(); panel.BringToFront(); status.BringToFront();
                text.MouseUp += (s, e) =>
                {
                    if (e.Button != MouseButtons.Right) return;
                    bool fullscreen = fixture?.Fullscreen ?? false;
                    fixture?.Dispose(); fixture = new Fixture(new MenuAppearance { Dark = dark.Checked, RoundedIcons = !defaults.Checked, DarkNeutrals26 = !defaults.Checked, RightToLeft = rtl.Checked, Grayscale = grayscale.Checked });
                    fixture.Factor = (double)zoom.Value / 100; fixture.Fullscreen = fullscreen;
                    fixture.Zoom.CanZoom = () => !noZoom.Checked;
                    fixture.Zoom.CanFullscreen = () => !noFullscreen.Checked;
                    var step = fixture.Zoom.Step; var full = fixture.Zoom.Fullscreen;
                    fixture.Zoom.Step = direction => { step(direction); zoom.Value = fixture.Zoom.Percent; status.Text = $"Zoom: {fixture.Zoom.Percent}% — {fixture.ZoomCommands} zoom actions; menu remains open."; };
                    fixture.Zoom.Fullscreen = () => { full(); status.Text = $"Fullscreen fixture: {fixture.Fullscreen}; menu closed before action: {fixture.ClosedBeforeFullscreen}."; };
                    fixture.Menu.Show(text, e.Location);
                };
            }
            protected override void Dispose(bool disposing) { if (disposing) fixture?.Dispose(); base.Dispose(disposing); }
        }
    }
}
