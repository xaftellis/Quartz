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
        private Bitmap bitmap;
        private int margin;

        internal ChromiumMenuWindow(ToolStripDropDown menu) { this.menu = menu; }

        internal void Update(Color color)
        {
            if (!menu.Visible || menu.IsDisposed || menu.Width < 1 || menu.Height < 1) return;
            if (Handle == IntPtr.Zero)
                CreateHandle(new CreateParams { Caption = "Quartz menu shadow", Style = unchecked((int)0x80000000),
                    ExStyle = 0x00080000 | 0x00000020 | 0x08000000 | 0x00000080 | 0x00000008 });
            bool changed = bitmap == null || lastSize != menu.Size || lastDpi != menu.DeviceDpi || lastColor != color;
            if (changed)
            {
                lastSize = menu.Size; lastDpi = menu.DeviceDpi; lastColor = color;
                int elevation = ChromiumMenuStyle.Scale(menu, menu.OwnerItem == null ? 12 : 16);
                margin = Math.Max(1, elevation * 3);
                int radius = ChromiumMenuStyle.Scale(menu, 12);
                bitmap?.Dispose();
                bitmap = RenderSurface(menu.Size, radius, elevation, margin, color);
                // The backdrop paints the outer antialiased pixel. Native GDI
                // paints only the opaque interior, so there are no jagged corners.
                using (var path = RoundedRectangle(new RectangleF(1, 1, menu.Width - 2, menu.Height - 2), Math.Max(1, radius - 1)))
                {
                    var oldRegion = menu.Region;
                    menu.Region = new Region(path);
                    oldRegion?.Dispose();
                }
            }
            Point location = new Point(menu.Left - margin, menu.Top - margin);
            if (changed) Upload(bitmap, location);
            SetWindowPos(Handle, menu.Handle, location.X, location.Y, bitmap.Width, bitmap.Height,
                0x0010 | 0x0040); // SWP_NOACTIVATE | SWP_SHOWWINDOW, immediately behind the menu.
        }

        internal void Hide()
        {
            if (Handle != IntPtr.Zero) ShowWindow(Handle, 0);
        }

        public void Dispose()
        {
            Hide();
            bitmap?.Dispose(); bitmap = null;
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

        private void Upload(Bitmap image, Point location)
        {
            IntPtr screen = GetDC(IntPtr.Zero), memory = CreateCompatibleDC(screen);
            IntPtr hBitmap = image.GetHbitmap(Color.FromArgb(0)), previous = SelectObject(memory, hBitmap);
            try
            {
                var position = new NativePoint(location.X, location.Y);
                var size = new NativeSize(image.Width, image.Height);
                var origin = new NativePoint(0, 0);
                var blend = new BlendFunction { SourceConstantAlpha = 255, AlphaFormat = 1 };
                UpdateLayeredWindow(Handle, screen, ref position, ref size, memory, ref origin, 0, ref blend, 2);
            }
            finally
            {
                SelectObject(memory, previous); DeleteObject(hBitmap);
                DeleteDC(memory); ReleaseDC(IntPtr.Zero, screen);
            }
        }

        [StructLayout(LayoutKind.Sequential)] private struct NativePoint { internal int X, Y; internal NativePoint(int x, int y) { X = x; Y = y; } }
        [StructLayout(LayoutKind.Sequential)] private struct NativeSize { internal int Width, Height; internal NativeSize(int w, int h) { Width = w; Height = h; } }
        [StructLayout(LayoutKind.Sequential, Pack = 1)] private struct BlendFunction { internal byte BlendOp, BlendFlags, SourceConstantAlpha, AlphaFormat; }
        [DllImport("user32.dll")] private static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdc, ref NativePoint position, ref NativeSize size, IntPtr source, ref NativePoint origin, int key, ref BlendFunction blend, int flags);
        [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int width, int height, int flags);
        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hwnd, int command);
        [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);
        [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleDC(IntPtr dc);
        [DllImport("gdi32.dll")] private static extern bool DeleteDC(IntPtr dc);
        [DllImport("gdi32.dll")] private static extern IntPtr SelectObject(IntPtr dc, IntPtr obj);
        [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr obj);
    }
}
