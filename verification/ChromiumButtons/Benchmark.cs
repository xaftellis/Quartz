using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Quartz.Controls;

// Compile this unchanged against the original and updated Quartz assemblies.
// Measures CPU painting only, not screen presentation or live browser FPS.
internal static class Benchmark
{
    private const BindingFlags Members = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    private sealed class Probe : ChromiumButton
    {
        internal void PaintFrame(PaintEventArgs e) => OnPaint(e);
        internal void MovePointer() => OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, 8, 12, 0));
    }

    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Type frames = typeof(ChromiumButton).GetNestedType("ButtonFrames", Members);
        var now = (Func<double>)Delegate.CreateDelegate(typeof(Func<double>), frames.GetProperty("Now", Members).GetGetMethod(true));
        var buttons = new Probe[24];
        var press = new Action<double>[buttons.Length];
        using (var icon = new Bitmap(16, 16))
        using (var iconGraphics = Graphics.FromImage(icon))
        using (var surface = new Bitmap(150, 28))
        using (var graphics = Graphics.FromImage(surface))
        using (var args = new PaintEventArgs(graphics, new Rectangle(0, 0, 150, 28)))
        {
            iconGraphics.FillEllipse(Brushes.DimGray, 2, 2, 12, 12);
            for (int i = 0; i < buttons.Length; i++)
            {
                var button = new Probe { Size = surface.Size, BackColor = Color.White, ForeColor = Color.Black,
                    Text = "Favourite " + i, Image = icon, ImageTextSpacing = 8, UseMnemonic = false };
                button.CreateControl();
                object animation = typeof(ChromiumButton).GetField("_animation", Members).GetValue(button);
                animation.GetType().GetProperty("AnimationsEnabled", Members).SetValue(animation, true);
                press[i] = (Action<double>)Delegate.CreateDelegate(typeof(Action<double>), animation,
                    animation.GetType().GetMethod("Press", Members));
                buttons[i] = button;
            }
            try
            {
                Action<int> paint = rounds =>
                {
                    for (int round = 0; round < rounds; round++)
                        for (int i = 0; i < buttons.Length; i++)
                        {
                            press[i](now() - (1 + (round + i) % 12) * 18);
                            buttons[i].PaintFrame(args);
                        }
                };
                paint(50);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                var samples = new double[5];
                for (int sample = 0; sample < samples.Length; sample++)
                {
                    var clock = Stopwatch.StartNew();
                    paint(200);
                    samples[sample] = clock.Elapsed.TotalMilliseconds;
                }
                Console.WriteLine("Paint samples (4800 frames each): " + string.Join(", ", Array.ConvertAll(samples, x => x.ToString("F1"))));
                Array.Sort(samples);
                Console.WriteLine("Median microseconds per paint: " + (samples[2] * 1000 / 4800).ToString("F2"));
                Probe hover = buttons[0];
                hover.AnimationEnabled = false;
                hover.MovePointer();
                int invalidations = 0;
                hover.Invalidated += (sender, e) => invalidations++;
                for (int i = 0; i < 1000; i++) hover.MovePointer();
                Console.WriteLine("Repaint requests for 1000 unchanged hover moves: " + invalidations);
                Console.WriteLine("CPU paint benchmark; 24 controls, 150x28 pixels, cached icons and labels, varying ripple radius. Not a live frame-rate measurement.");
            }
            finally { foreach (Probe button in buttons) button?.Dispose(); }
        }
    }
}
