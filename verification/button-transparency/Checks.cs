using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Quartz.Controls;

internal static class Checks
{
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private const int TransparentStyles = 0x00080000 | 0x00200000;
    private static int passed;

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static extern int GetWindowLong(IntPtr window, int index);

    private static object Field(ChromiumButton button, string name) =>
        typeof(ChromiumButton).GetField(name, Private).GetValue(button);
    private static void Call(ChromiumButton button, string name, params object[] args) =>
        typeof(ChromiumButton).GetMethod(name, Private).Invoke(button, args);
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        passed++;
    }

    [STAThread]
    private static int Main()
    {
        try
        {
            Application.EnableVisualStyles();
            // A parked root control exercises real HWNDs and DirectComposition
            // without showing a test app or taking over the user's desktop.
            using (var parent = new Form { Size = new Size(300, 120), BackColor = Color.Red })
            using (var button = new ChromiumButton { Size = new Size(100, 32), Text = "Button", BackColor = Color.Blue })
            {
                parent.Controls.Add(button);
                IntPtr parentHandle = parent.Handle;
                // Set managed visibility only; the native top-level window stays
                // hidden. This allows the production button's visibility guards
                // and queued updates to run without displaying any test window.
                typeof(Control).GetMethod("SetState", Private).Invoke(parent, new object[] { 2, true });
                button.CreateControl();
                Application.DoEvents();
                Check(button.TransparentBackground, "Transparency is the shared default");
                Check((GetWindowLong(button.Handle, -20) & TransparentStyles) == TransparentStyles,
                    "A transparent button must have a layered, no-redirection HWND");
                Check((GetWindowLong(button.Handle, -20) & 0x20) == 0, "Button must not request input pass-through");
                Check(Field(button, "_composition") != null,
                    "Composition must initialize without a WM_PAINT callback");

                object composition = Field(button, "_composition");
                button.Text = "Updated";
                button.ForeColor = Color.Green;
                button.Invalidate();
                Application.DoEvents();
                Check(ReferenceEquals(composition, Field(button, "_composition")),
                    "Content changes must retain the animation tree");
                Check(!(bool)Field(button, "_compositionPaintPending"), "Queued paint must drain");

                Call(button, "PrepareBackground", true);
                var buffer = (Bitmap)Field(button, "_buffer");
                Check(buffer.GetPixel(0, 0).A == 0 && buffer.GetPixel(50, 16).A == 0,
                    "Both corners and button face must be fully transparent");
                using (var decoration = new Bitmap(2, 2))
                {
                    decoration.SetPixel(0, 0, Color.Magenta);
                    button.BackgroundImage = decoration;
                    Call(button, "PrepareBackground", true);
                    Check(((Bitmap)Field(button, "_buffer")).GetPixel(0, 0).A == 0,
                        "Background artwork must not make a transparent button opaque");
                    button.BackgroundImage = null;
                }

                button.IsActive = true;
                Application.DoEvents();
                Check(button.IsPressed && Field(button, "_composition") != null,
                    "Active feedback must remain composed");
                IntPtr handle = button.Handle;
                button.TransparentBackground = false;
                Application.DoEvents();
                Check(button.Handle != handle, "Changing transparency recreates the no-redirection HWND");
                Check((GetWindowLong(button.Handle, -20) & TransparentStyles) == 0,
                    "Opaque mode restores ordinary window styles");
                Check(button.IsActive, "Changing transparency must retain activation");
                button.TransparentBackground = true;
                Application.DoEvents();
                Check(Field(button, "_composition") != null, "Re-enabling transparency must restore composition");

                button.Hide();
                Check(Field(button, "_composition") == null, "Hiding disposes the visual tree");
                button.Show();
                Application.DoEvents();
                Check(Field(button, "_composition") != null, "Showing must republish without WM_PAINT");

                // A compositor failure must restore visible software rendering.
                Call(button, "FailComposition", new COMException("Injected verification failure"));
                Application.DoEvents();
                Check((GetWindowLong(button.Handle, -20) & TransparentStyles) == 0,
                    "Failure must remove the styles that suppress GDI painting");
                Check(Field(button, "_composition") == null && (bool)Field(button, "_compositionFailed"),
                    "Failure must select the software fallback");
                Call(button, "PrepareBackground", false);
                Check(((Bitmap)Field(button, "_buffer")).GetPixel(50, 16).A == 255,
                    "Fallback must provide a visible face");
            }
            using (var parent = new Form())
            using (var button = new ChromiumButton())
            {
                PaintEventHandler paint = (sender, e) => { };
                button.Paint += paint;
                parent.Controls.Add(button);
                IntPtr parentHandle = parent.Handle;
                IntPtr buttonHandle = button.Handle;
                Check((GetWindowLong(button.Handle, -20) & TransparentStyles) == 0,
                    "Custom Paint subscribers must retain a GDI-backed window");
                button.Paint -= paint;
            }
            Console.WriteLine("PASS: " + passed + " transparency checks (parked controls; no visual or physical-input validation).");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
