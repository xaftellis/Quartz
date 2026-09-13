using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Quartz.Controls
{
    // A non-activating, click-through surface behind the real ToolStrip HWND.
    // It supplies antialiased 12-DIP corners and Chromium's two-layer shadow;
    // the actual menu continues to handle input and expose its accessible items.
    internal sealed class ChromiumMenuWindow : NativeWindow, IDisposable
    {
        private readonly ToolStripDropDown menu;
        private Size lastSize;
        private int lastDpi;
        private Color lastColor;
        private IntPtr memoryDc, nativeBitmap, previousBitmap;
        private Size surfaceSize;
        private byte opacity = 255;
        private bool visible;
        private int margin;

        internal ChromiumMenuWindow(ToolStripDropDown menu) { this.menu = menu; }

        internal void Update(Color color)
        {
            if (!menu.Visible || menu.IsDisposed || menu.Width < 1 || menu.Height < 1) return;
            if (Handle == IntPtr.Zero)
                CreateHandle(new CreateParams { Caption = "Quartz menu shadow", Style = unchecked((int)0x80000000),
                    ExStyle = 0x00080000 | 0x00000020 | 0x08000000 | 0x00000080 | 0x00000008 });
            bool changed = memoryDc == IntPtr.Zero || lastSize != menu.Size || lastDpi != menu.DeviceDpi || lastColor != color;
            if (changed)
            {
                lastSize = menu.Size; lastDpi = menu.DeviceDpi; lastColor = color;
                int elevation = ChromiumMenuStyle.Scale(menu, menu.OwnerItem == null ? 12 : 16);
                margin = Math.Max(1, elevation * 3);
                int radius = ChromiumMenuStyle.Scale(menu, 12);
                ReleaseSurface();
                using (var bitmap = RenderSurface(menu.Size, radius, elevation, margin, color))
                {
                    surfaceSize = bitmap.Size;
                    memoryDc = CreateCompatibleDC(IntPtr.Zero);
                    nativeBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
                    previousBitmap = SelectObject(memoryDc, nativeBitmap);
                }
                var oldRegion = menu.Region;
                menu.Region = CreateMenuRegion(menu.Size, radius);
                oldRegion?.Dispose();
            }
            Point location = new Point(menu.Left - margin, menu.Top - margin);
            byte nextOpacity = (byte)Math.Round(menu.Opacity * 255);
            if (changed || !visible || opacity != nextOpacity)
            {
                opacity = nextOpacity;
                Upload(location);
            }
            SetWindowPos(Handle, menu.Handle, location.X, location.Y, surfaceSize.Width, surfaceSize.Height,
                0x0010 | 0x0040); // SWP_NOACTIVATE | SWP_SHOWWINDOW, immediately behind the menu.
            visible = true;
        }

        internal void SetOpacity(byte value)
        {
            if (opacity == value) return;
            opacity = value;
            if (visible && memoryDc != IntPtr.Zero)
                Upload(new Point(menu.Left - margin, menu.Top - margin));
        }

        internal void Hide()
        {
            visible = false;
            if (Handle != IntPtr.Zero) ShowWindow(Handle, 0);
        }

        public void Dispose()
        {
            Hide();
            ReleaseSurface();
            if (Handle != IntPtr.Zero) DestroyHandle();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0084) { m.Result = new IntPtr(-1); return; } // HTTRANSPARENT
            if (m.Msg == 0x0021) { m.Result = new IntPtr(3); return; } // MA_NOACTIVATE
            base.WndProc(ref m);
        }

        internal static GraphicsPath RoundedRectangle(RectangleF bounds, float radius)
        {
            float diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
            var path = new GraphicsPath();
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static Region CreateMenuRegion(Size size, int radius)
        {
            // Only the rounded corners need an antialiased pixel supplied by
            // the backdrop. Insetting the entire HWND also clips every hover
            // row, exposing a white strip down both straight edges.
            using (var corners = RoundedRectangle(new RectangleF(1, 1, size.Width - 2, size.Height - 2),
                Math.Max(1, radius - 1)))
            {
                var region = new Region(corners);
                region.Union(new Rectangle(0, radius, size.Width, Math.Max(0, size.Height - radius * 2)));
                return region;
            }
        }

        internal static Bitmap RenderSurface(Size size, int radius, int elevation, int margin, Color color)
        {
            int width = size.Width + margin * 2, height = size.Height + margin * 2;
            var result = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
            var bounds = new Rectangle(margin, margin, size.Width, size.Height);
            // Rasterize the rounded mask once; three sliding box passes closely
            // approximate the Gaussian blur used by Chromium/Skia without a dependency.
            using (var graphics = Graphics.FromImage(result))
            using (var path = RoundedRectangle(bounds, radius))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.FillPath(Brushes.Black, path);
            }
            var rect = new Rectangle(0, 0, width, height);
            var data = result.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
            int[] pixels = new int[width * height];
            Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
            result.UnlockBits(data);
            float[] mask = new float[pixels.Length];
            for (int i = 0; i < mask.Length; i++) mask[i] = (uint)pixels[i] >> 24;
            // gfx::CreateShadowDrawLooper applies RadiusToSigma(blur / 2):
            // sigma = 0.288675 * radius + 0.5. ShadowValue's key blur is
            // 4 * elevation and its ambient blur is 2 * elevation.
            float scale = radius / 12f;
            float[] key = Blur(mask, width, height, .57735f * elevation + .5f * scale);
            float[] ambient = Blur(mask, width, height, .288675f * elevation + .5f * scale);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    int i = y * width + x;
                    float a = ambient[i] * (31f / 255);
                    float k = y >= elevation ? key[(y - elevation) * width + x] * (61f / 255) : 0;
                    int alpha = Math.Min(255, (int)Math.Round(a + k * (1 - a / 255)));
                    pixels[i] = alpha << 24;
                }
            data = result.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
            Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
            result.UnlockBits(data);
            using (var graphics = Graphics.FromImage(result))
            using (var path = RoundedRectangle(bounds, radius))
            using (var brush = new SolidBrush(color))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.FillPath(brush, path);
                // The menu HWND owns this interior. Leaving another opaque body
                // behind it would blend the two alphas and spoil the fade.
                // Match its native Region exactly; retain just the outer edge
                // and shadow on this click-through surface.
                using (var interior = CreateMenuRegion(size, radius))
                {
                    interior.Translate(margin, margin);
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.FillRegion(Brushes.Transparent, interior);
                }
            }
            return result;
        }

        private static float[] Blur(float[] input, int width, int height, float sigma)
        {
            float[] source = (float[])input.Clone(), target = new float[input.Length];
            int lowerWidth = (int)Math.Floor(Math.Sqrt(4 * sigma * sigma + 1));
            if (lowerWidth % 2 == 0) lowerWidth--;
            lowerWidth = Math.Max(1, lowerWidth);
            int lowerPasses = (int)Math.Round((12 * sigma * sigma - 3 * lowerWidth * lowerWidth -
                12 * lowerWidth - 9) / (-4.0 * lowerWidth - 4));
            for (int pass = 0; pass < 3; pass++)
            {
                int diameter = pass < lowerPasses ? lowerWidth : lowerWidth + 2;
                int radius = (diameter - 1) / 2;
                for (int y = 0; y < height; y++)
                {
                    int row = y * width;
                    float sum = 0;
                    for (int x = 0; x <= radius && x < width; x++) sum += source[row + x];
                    for (int x = 0; x < width; x++)
                    {
                        target[row + x] = sum / diameter;
                        if (x - radius >= 0) sum -= source[row + x - radius];
                        if (x + radius + 1 < width) sum += source[row + x + radius + 1];
                    }
                }
                for (int x = 0; x < width; x++)
                {
                    float sum = 0;
                    for (int y = 0; y <= radius && y < height; y++) sum += target[y * width + x];
                    for (int y = 0; y < height; y++)
                    {
                        source[y * width + x] = sum / diameter;
                        if (y - radius >= 0) sum -= target[(y - radius) * width + x];
                        if (y + radius + 1 < height) sum += target[(y + radius + 1) * width + x];
                    }
                }
            }
            return source;
        }

        private void Upload(Point location)
        {
            var position = new NativePoint(location.X, location.Y);
            var size = new NativeSize(surfaceSize.Width, surfaceSize.Height);
            var origin = new NativePoint(0, 0);
            var blend = new BlendFunction { SourceConstantAlpha = opacity, AlphaFormat = 1 };
            // Reuse the selected native bitmap/DC: each frame only changes alpha.
            UpdateLayeredWindow(Handle, IntPtr.Zero, ref position, ref size, memoryDc, ref origin, 0, ref blend, 2);
        }

        private void ReleaseSurface()
        {
            if (memoryDc == IntPtr.Zero) return;
            SelectObject(memoryDc, previousBitmap);
            DeleteObject(nativeBitmap);
            DeleteDC(memoryDc);
            memoryDc = nativeBitmap = previousBitmap = IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential)] private struct NativePoint { internal int X, Y; internal NativePoint(int x, int y) { X = x; Y = y; } }
        [StructLayout(LayoutKind.Sequential)] private struct NativeSize { internal int Width, Height; internal NativeSize(int w, int h) { Width = w; Height = h; } }
        [StructLayout(LayoutKind.Sequential, Pack = 1)] private struct BlendFunction { internal byte BlendOp, BlendFlags, SourceConstantAlpha, AlphaFormat; }
        [DllImport("user32.dll")] private static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdc, ref NativePoint position, ref NativeSize size, IntPtr source, ref NativePoint origin, int key, ref BlendFunction blend, int flags);
        [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int width, int height, int flags);
        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hwnd, int command);
        [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleDC(IntPtr dc);
        [DllImport("gdi32.dll")] private static extern bool DeleteDC(IntPtr dc);
        [DllImport("gdi32.dll")] private static extern IntPtr SelectObject(IntPtr dc, IntPtr obj);
        [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr obj);
    }
}
