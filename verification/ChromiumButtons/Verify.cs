using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Quartz.Controls;

internal static class Verify
{
    private const BindingFlags Members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static int _checks;
    private static readonly Type AnimationType = typeof(ChromiumButton).Assembly.GetType("Quartz.Controls.ChromiumButtonAnimation");

    private sealed class Animation
    {
        internal readonly object Value = Activator.CreateInstance(AnimationType, true);
        internal object Call(string name, params object[] args) => AnimationType.GetMethod(name, Members).Invoke(Value, args);
        internal double Sample(string name, double time) => Convert.ToDouble(Call(name, time));
        internal string State => AnimationType.GetProperty("State", Members).GetValue(Value).ToString();
        internal void Reduced() => AnimationType.GetProperty("AnimationsEnabled", Members).SetValue(Value, false);
    }

    private sealed class Probe : ChromiumButton
    {
        internal float TestScale = 1;
        protected override float DpiScale => TestScale;
        internal RectangleF InkBounds => GetInkBounds();
        internal Rectangle ImageBounds { get { GetContentBounds(out Rectangle image, out Rectangle label); return image; } }
        internal Rectangle LabelBounds { get { GetContentBounds(out Rectangle image, out Rectangle label); return label; } }
        internal void Down(Point p) => OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, p.X, p.Y, 0));
        internal void MovePointer(Point p) => OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, p.X, p.Y, 0));
        internal void Release(Point p)
        {
            // Direct logic probe only: manually invoking Click does NOT reproduce
            // Button.OnMouseUp's native dispatch order. It missed the now-fixed
            // release flag bug. Native input verification is still required.
            Field("_releasePoint").SetValue(this, p);
            Field("_releasingMouse").SetValue(this, true);
            try { OnClick(EventArgs.Empty); OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, p.X, p.Y, 0)); }
            finally { Field("_releasingMouse").SetValue(this, false); }
        }
        internal void PressKey(Keys key) => OnKeyDown(new KeyEventArgs(key));
        internal void ReleaseKey(Keys key) => OnKeyUp(new KeyEventArgs(key));
        internal void LoseFocus() => OnLostFocus(EventArgs.Empty);
        internal void LoseCapture() => OnMouseCaptureChanged(EventArgs.Empty);
    }

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            Application.EnableVisualStyles();
            AnimationChecks();
            ControlChecks();
            ImageChecks();
            ProfileChecks();
            FavouriteChecks();
            RenderSheet(args.Length == 0 ? "rendered-buttons.png" : args[0]);
            Console.WriteLine("PASS: " + _checks + " assertions. Offscreen controls; no physical GUI validation.");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
    }

    private static void AnimationChecks()
    {
        var hover = new Animation();
        hover.Call("SetHovered", true, 0d);
        Near(hover.Sample("Highlight", 125), .5, "250 ms hover midpoint");
        Near(hover.Sample("Highlight", 250), 1, "250 ms hover completes");
        hover.Call("SetHovered", false, 250d);
        Near(hover.Sample("Highlight", 375), .5, "250 ms leave midpoint");
        hover.Call("SetHovered", true, 375d);
        Near(hover.Sample("Highlight", 375), 0, "hover re-entry recreates highlight at zero");
        Near(hover.Sample("Highlight", 500), .5, "recreated highlight follows 250 ms curve");

        var click = new Animation();
        click.Call("Press", 0d);
        Near(click.Sample("Opacity", 0), 1, "mouse down opacity is immediate");
        Near(click.Sample("Radius", 120), .775561, "240 ms cubic-bezier expansion", .00001);
        click.Call("Trigger", 30d);
        Near(click.Sample("Opacity", 239), 1, "quick release waits for expansion");
        Near(click.Sample("Opacity", 390), .5, "300 ms queued release fade");
        click.Call("Advance", 540d);
        Check(click.State == "Hidden", "quick click ends at 540 ms");
        Near(click.Sample("Highlight", 600), .5, "unhovered highlight fades over 120 ms after ripple");
        Check(!(bool)click.Call("IsAnimating", 660d), "idle animation stops scheduling");

        var hold = new Animation();
        hold.Call("Press", 0d);
        hold.Call("Trigger", 1000d);
        Near(hold.Sample("Opacity", 1150), .5, "held release starts fading immediately");
        var cancel = new Animation();
        cancel.Call("Press", 0d);
        double radius = cancel.Sample("Radius", 120);
        cancel.Call("Cancel", 120d);
        Near(cancel.Sample("Radius", 120), radius, "cancel does not jump radius");
        Near(cancel.Sample("Opacity", 220), .5, "cancel fades over 200 ms");
        Near(cancel.Sample("Radius", 270), radius / 2, "cancel shrinks over 300 ms");
        cancel.Call("Press", 180d);
        Near(cancel.Sample("Radius", 180), 0, "drag back in starts a new ripple");

        var active = new Animation();
        active.Call("Activate", 0d);
        Near(active.Sample("Opacity", 75), .25, "activation has 150 ms ease-in opacity");
        Near(active.Sample("Radius", 100), .5, "activation has 200 ms ease-in-out expansion");
        active.Call("Cancel", 50d);
        Check(active.State == "Activated", "capture cancellation cannot release persistent state");
        active.Call("Deactivate", 75d);
        Near(active.Sample("Opacity", 75), .25, "early close preserves unfinished activation fade");
        Near(active.Sample("Opacity", 100), 4d / 9, "activation continues until queued fade begins");
        Near(active.Sample("Opacity", 300), .5, "deactivation fade queued after activation");
        active.Call("Activate", 300d);
        Near(active.Sample("Opacity", 300), 0, "reopening discards deactivated ripple");
        Near(active.Sample("Opacity", 450), 1, "reopening returns to active");
        Check(!(bool)active.Call("IsAnimating", 10000d), "held active state has no idle timer");

        var menu = new Animation();
        menu.Call("Press", 0d);
        double pendingRadius = menu.Sample("Radius", 80);
        menu.Call("Activate", 80d);
        Near(menu.Sample("Radius", 80), pendingRadius, "menu activation retains original click expansion");
        Near(menu.Sample("Opacity", 10000), 1, "persistent activation outlives click fade");
        menu.Call("SetHovered", false, 10000d);
        Near(menu.Sample("Highlight", 10250), 1, "active ripple retains highlight after pointer leaves");
        menu.Call("Deactivate", 11000d);
        Near(menu.Sample("Opacity", 11150), .5, "persistent state releases over 300 ms");

        var releasedMenu = new Animation();
        releasedMenu.Call("Press", 0d);
        releasedMenu.Call("Trigger", 80d);
        releasedMenu.Call("Activate", 80d);
        Near(releasedMenu.Sample("Radius", 80), 0, "activation after release recreates triggered ripple");
        Near(releasedMenu.Sample("Opacity", 155), .25, "recreated activation uses its own fade-in");

        var reduced = new Animation();
        reduced.Reduced();
        reduced.Call("Press", 0d);
        Near(reduced.Sample("Radius", 0), 1, "reduced motion pending snaps");
        reduced.Call("Trigger", 0d);
        Check(reduced.State == "Hidden", "reduced motion click completes synchronously");
        reduced.Call("Activate", 1d);
        Near(reduced.Sample("Opacity", 1), 1, "reduced motion active remains visible");
        reduced.Call("Deactivate", 2d);
        Check(reduced.State == "Hidden", "reduced motion release completes synchronously");
    }

    private static void ControlChecks()
    {
        using (var button = new Probe { AnimationEnabled = false, BackColor = Color.White, ForeColor = Color.Black })
        {
            button.CreateControl();
            Check(button.Size == new Size(28, 28) && button.IconSize == 16, "toolbar default target and icon");
            int clicks = 0;
            button.Click += (s, e) => clicks++;
            button.Down(new Point(3, 9));
            Check((PointF)Field("_origin").GetValue(button) == new PointF(3, 9), "mouse ripple originates at actual press");
            button.Release(new Point(3, 9));
            Check(clicks == 1, "one click for one valid release");
            button.Down(new Point(9, 9));
            button.MovePointer(new Point(40, 9));
            button.Release(new Point(40, 9));
            Check(clicks == 1, "drag out does not click");
            button.Down(new Point(9, 9));
            button.MovePointer(new Point(40, 9));
            button.MovePointer(new Point(8, 8));
            button.Release(new Point(8, 8));
            Check(clicks == 2, "drag out and back in clicks once");
            button.Down(new Point(9, 9));
            button.LoseCapture();
            button.Release(new Point(9, 9));
            Check(clicks == 2, "lost capture cancels the click");
            button.IsActive = true;
            button.LoseFocus();
            button.LoseCapture();
            Check(button.IsActive && button.IsPressed, "active survives focus/capture loss");
            button.Enabled = false;
            button.PerformClick();
            Check(clicks == 2 && !button.IsPressed && button.IsActive, "disabled blocks clicks without forgetting explicit active state");
            button.Enabled = true;
            Check(button.IsPressed, "enabling restores explicit active state");
            Check((button.AccessibilityObject.State & AccessibleStates.Pressed) != 0, "accessibility reports pressed state");
            button.IsActive = false;
            button.PerformClick();
            Check(clicks == 3, "PerformClick and IButtonControl behavior retained");
            Check((PointF)Field("_origin").GetValue(button) == new PointF(14, 14), "programmatic activation uses center");
            button.AccessibilityObject.DoDefaultAction();
            Check(clicks == 4 && button.AccessibilityObject.Role == AccessibleRole.PushButton, "accessible default action invokes button");
            button.PressKey(Keys.Space);
            Check(button.IsPressed, "space key-down presses");
            button.ReleaseKey(Keys.Space);
            Check(clicks == 5 && !button.IsPressed, "space key-up clicks once");
            button.PressKey(Keys.Space);
            button.PressKey(Keys.Escape);
            button.ReleaseKey(Keys.Space);
            Check(clicks == 5, "escape cancels keyboard press");
            button.DialogResult = DialogResult.Cancel;
            Check(((IButtonControl)button).DialogResult == DialogResult.Cancel, "dialog result API retained");

            int paints = 0;
            button.Paint += (sender, e) => { paints++; e.Graphics.FillRectangle(Brushes.Magenta, 13, 13, 2, 2); };
            using (var rendered = Render(button))
                Check(paints > 0 && rendered.GetPixel(13, 13).ToArgb() == Color.Magenta.ToArgb(), "Paint events run after shared rendering");
        }
        foreach (float scale in new[] { 1f, 1.25f, 1.5f, 2f })
        using (var button = new Probe { TestScale = scale, Size = new Size((int)(60 * scale), (int)(40 * scale)) })
        {
            Near(button.InkBounds.Height, 28 * scale, "toolbar ink height at " + scale);
            Near(button.InkBounds.Left, 6 * scale, "toolbar symmetric inset at " + scale);
            button.UseToolbarGeometry = false;
            Near(button.InkBounds.Height, button.Height, "surface geometry retains host bounds at " + scale);
        }
        using (var button = new Probe { AnimationEnabled = false, BackColor = Color.White, ForeColor = Color.Black })
        {
            button.CreateControl();
            button.IsActive = true;
            using (var rendered = Render(button))
            {
                int center = rendered.GetPixel(14, 14).R;
                Check(center >= 223 && center <= 227, "8% highlight and 6% ripple composite over white");
                Check(rendered.GetPixel(0, 0).ToArgb() == SystemColors.Control.ToArgb(), "rounded mask leaves target corner clear");
            }
            button.Enabled = false;
            using (var rendered = Render(button)) Check(rendered.GetPixel(14, 14).R == 255, "disabled removes all ink");
        }
    }

    private static void ProfileChecks()
    {
        using (var photo = new Bitmap(96, 64))
        using (var tile = new CircularImageButton { Size = new Size(100, 100), ButtonText = "Profile", CircularImage = photo })
        {
            using (Graphics graphics = Graphics.FromImage(photo)) graphics.Clear(Color.CornflowerBlue);
            using (var bitmap = Render(tile)) { }
            var field = typeof(CircularImageButton).GetField("_avatar", Members);
            object cached = field.GetValue(tile);
            for (int i = 0; i < 20; i++) using (var bitmap = Render(tile)) { }
            Check(ReferenceEquals(cached, field.GetValue(tile)), "profile hover/repaint reuses avatar cache");
            int mainClicks = 0, actionClicks = 0;
            tile.Click += (s, e) => mainClicks++;
            tile.ActionButtonClick += (s, e) => actionClicks++;
            tile.ActionButtonImage = photo;
            var action = (ChromiumButton)typeof(CircularImageButton).GetField("_action", Members).GetValue(tile);
            action.Visible = true;
            action.PerformClick();
            Check(actionClicks == 1 && mainClicks == 0, "remove action does not activate profile");
            Check(action.AccessibleName == "Remove profile picture" && action.TabStop, "remove action has keyboard/accessibility identity");
            tile.CircularImageSize = 48;
            using (var bitmap = Render(tile)) { }
            Check(!ReferenceEquals(cached, field.GetValue(tile)), "profile size change regenerates cache");
            tile.Dispose();
            Check(field.GetValue(tile) == null, "profile disposal releases cached pixels");
            Check(photo.GetPixel(0, 0).B > 0, "profile disposal preserves caller-owned image");
        }
        using (var site = new SiteInfoButton { IsPopupOpen = true })
            Check(site.IsActive && site.IsPressed, "site-info popup uses shared active state");
    }

    private static void ImageChecks()
    {
        foreach (float scale in new[] { 1f, 1.25f, 1.5f, 2f })
        foreach (ChromiumIcon icon in Enum.GetValues(typeof(ChromiumIcon)))
        {
            if (icon == ChromiumIcon.None) continue;
            using (var button = new Probe { TestScale = scale, Size = new Size((int)(28 * scale), (int)(28 * scale)),
                Padding = new Padding((int)Math.Round(6 * scale)), VectorIcon = icon, BackColor = Color.White, ForeColor = Color.Black })
            using (var bitmap = Render(button))
            {
                Check(button.ImageBounds.Size == new Size((int)(16 * scale), (int)(16 * scale)), "device-scale vector slot: " + icon + " at " + scale);
                Check(button.ImageBounds.Top == (button.Height - button.ImageBounds.Height) / 2, "vector stays vertically centered");
                bool painted = false;
                for (int y = button.ImageBounds.Top; y < button.ImageBounds.Bottom; y++)
                    for (int x = button.ImageBounds.Left; x < button.ImageBounds.Right; x++)
                        painted |= bitmap.GetPixel(x, y).R < 128;
                Check(painted, "embedded source vector paints: " + icon + " at " + scale);
            }
        }
        using (var source = new Bitmap(32, 16))
        using (var button = new Probe { Image = source, Size = new Size(150, 28), Text = "Favourite name",
            TextAlign = ContentAlignment.MiddleLeft, ImageTextSpacing = 8, BackColor = Color.White, ForeColor = Color.Black })
        {
            using (Graphics graphics = Graphics.FromImage(source)) graphics.Clear(Color.Red);
            Check(button.ImageBounds.Size == new Size(16, 8), "bitmap scaling preserves aspect ratio");
            Check(button.LabelBounds.Left == button.ImageBounds.Right + 8, "label uses separate 8-DIP gap");
            using (var bitmap = Render(button)) Check(bitmap.GetPixel(10, 14).R == 255, "Lanczos preserves constant colour");
            object renderer = Field("_imageRenderer").GetValue(button);
            FieldInfo pixels = renderer.GetType().GetField("_pixels", Members);
            object cached = pixels.GetValue(renderer);
            using (var bitmap = Render(button)) { }
            Check(ReferenceEquals(cached, pixels.GetValue(renderer)), "image representations are cached across paint");
            button.Enabled = false;
            using (var bitmap = Render(button))
            {
                Color pixel = bitmap.GetPixel(10, 14);
                Check(pixel.R == 255 && pixel.G >= 144 && pixel.G <= 146, "disabled image uses alpha 0x6E, not Windows greyscale");
            }
            button.Dispose();
            Check(pixels.GetValue(renderer) == null && source.GetPixel(0, 0).R == 255, "cache disposed; caller image remains owned by caller");
        }
        using (var button = new Probe { VectorIcon = ChromiumIcon.Back, BackColor = Color.White, ForeColor = Color.Black })
        {
            button.FlatStyle = FlatStyle.System;
            Check(button.FlatStyle == FlatStyle.Flat, "designer cannot restore native paint adapter");
            using (var left = Render(button))
            {
                button.RightToLeft = RightToLeft.Yes;
                using (var right = Render(button))
                {
                    bool matches = true;
                    for (int y = 6; y < 22; y++) for (int x = 6; x < 22; x++)
                        matches &= Math.Abs(left.GetPixel(x, y).R - right.GetPixel(27 - x, y).R) <= 1;
                    Check(matches, "toolbar vector mirrors with RTL canvas");
                }
            }
        }
    }

    private static void FavouriteChecks()
    {
        Type type = typeof(ChromiumButton).Assembly.GetType("Quartz.Controls.FavouriteButton");
        using (var button = (ChromiumButton)Activator.CreateInstance(type, true))
        {
            button.Text = "First favourite";
            button.Size = new Size(135, 28);
            button.BackColor = Color.White;
            button.ForeColor = Color.Black;
            Check(button.GetPreferredSize(Size.Empty).Height == 28, "Chromium favourites are 28 DIPs high");
            Check(button.MaximumSize.Width == 150 && button.Padding.All == 6 && button.ImageTextSpacing == 8,
                "Chromium favourites max width, padding and image gap");
            type.GetMethod("ChangeContent", Members).Invoke(button, new object[] { (Action)(() => button.Text = "Changed title"), true });
            type.GetMethod("AdvanceContentAnimation", Members).Invoke(button, new object[] { 90d });
            using (var bitmap = Render(button)) Check(bitmap.Width == 135, "favourite content transition renders with new button");
            int clicks = 0;
            button.Click += (s, e) => clicks++;
            type.GetProperty("SuppressMouseClick", Members).SetValue(button, true);
            button.PerformClick();
            Check(clicks == 0, "favourite drag suppression preserved");
            type.GetProperty("SuppressMouseClick", Members).SetValue(button, false);
            button.PerformClick();
            Check(clicks == 1, "favourite normal activation preserved");
            type.GetMethod("AdvanceContentAnimation", Members).Invoke(button, new object[] { 100d });
            type.GetMethod("BeginEntrance", Members).Invoke(button, new object[] { true });
            using (var bitmap = Render(button)) Check(bitmap.Height == 28, "favourite entrance foreground renders");
        }
    }

    private static void RenderSheet(string destination)
    {
        using (var sheet = new Bitmap(780, 380))
        using (Graphics graphics = Graphics.FromImage(sheet))
        using (var font = new Font("Segoe UI", 10))
        using (var icon = new Bitmap(16, 16))
        {
            graphics.Clear(Color.White);
            using (var drawing = Graphics.FromImage(icon))
            using (var pen = new Pen(Color.FromArgb(70, 70, 70), 2))
            {
                drawing.DrawLine(pen, 3, 8, 13, 8);
                drawing.DrawLine(pen, 3, 8, 7, 4);
                drawing.DrawLine(pen, 3, 8, 7, 12);
            }
            graphics.DrawString("Actual controls rendered offscreen — 100% DPI", font, Brushes.Black, 18, 12);
            string[] labels = { "Rest", "Hover", "Active", "Disabled" };
            Color[] colors = { Color.White, Color.FromArgb(35, 35, 35), Color.Black, Color.Aqua, Color.Red };
            for (int row = 0; row < colors.Length; row++)
            {
                for (int column = 0; column < 4; column++)
                using (var button = new Probe { AnimationEnabled = false, BackColor = colors[row], ForeColor = Color.Black, Image = icon })
                {
                    button.CreateControl();
                    object animation = Field("_animation").GetValue(button);
                    AnimationType.GetMethod("Reset", Members).Invoke(animation, new object[] { column == 2, column == 1, 0d });
                    Field("_origin").SetValue(button, new PointF(14, 14));
                    if (column == 3) button.Enabled = false;
                    using (var bitmap = Render(button)) graphics.DrawImageUnscaled(bitmap, 30 + column * 130, 70 + row * 48);
                    if (row == 0) graphics.DrawString(labels[column], font, Brushes.Black, 28 + column * 130, 44);
                }
            }
            using (var text = new Probe { Text = "Save changes", Size = new Size(142, 30), BackColor = Color.White, ForeColor = Color.Black, IsActive = true, AnimationEnabled = false })
            using (var bitmap = Render(text)) graphics.DrawImageUnscaled(bitmap, 565, 75);
            using (var small = new Probe { Text = "▼", Padding = Padding.Empty, Size = new Size(20, 20), BackColor = Color.White, ForeColor = Color.Black })
            using (var bitmap = Render(small)) graphics.DrawImageUnscaled(bitmap, 565, 130);
            using (var profile = new CircularImageButton { ButtonText = "Profile name", CircularImage = icon, Size = new Size(110, 110), BackColor = Color.White, ForeColor = Color.Black, IsActive = true, AnimationEnabled = false })
            using (var bitmap = Render(profile)) graphics.DrawImageUnscaled(bitmap, 565, 175);
            graphics.DrawString("Quartz keeps its own icons, palette and dialog layout.", font, Brushes.DimGray, 18, 340);
            sheet.Save(destination, ImageFormat.Png);
        }
        Check(File.Exists(destination), "offscreen render sheet saved");
    }

    private static Bitmap Render(Control control)
    {
        var bitmap = new Bitmap(control.Width, control.Height);
        control.DrawToBitmap(bitmap, control.ClientRectangle);
        return bitmap;
    }
    private static FieldInfo Field(string name) => typeof(ChromiumButton).GetField(name, Members);
    private static void Near(double actual, double expected, string message, double tolerance = .000001)
        => Check(Math.Abs(actual - expected) <= tolerance, message + " (" + actual + " vs " + expected + ")");
    private static void Check(bool passed, string message)
    {
        if (!passed) throw new Exception("FAILED: " + message);
        _checks++;
    }
}
