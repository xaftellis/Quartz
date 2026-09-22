using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Quartz.Controls
{
    // GDI's small-font grayscale mode can drop smoothing altogether. Keep its
    // native layout/hinting, but collapse ClearType coverage to a neutral alpha
    // mask before compositing. The same cached glyphs serve screen, snapshots,
    // and DComp: no font-quality switch when a favourite finishes moving.
    internal sealed class ChromiumButtonText : IDisposable
    {
        private Bitmap _glyphs;
        private string _text;
        private Font _font;
        private Color _color;
        private TextFormatFlags _flags;
        private int _dpi;

        internal void Draw(Graphics graphics, string text, Font font, Rectangle bounds,
            Color color, TextFormatFlags flags, int dpi)
        {
            if (string.IsNullOrEmpty(text) || bounds.Width <= 0 || bounds.Height <= 0) return;
            if (_glyphs == null || _glyphs.Size != bounds.Size || _text != text ||
                !ReferenceEquals(_font, font) || _color != color || _flags != flags || _dpi != dpi)
            {
                var glyphs = Render(text, font, bounds.Size, color, flags);
                _glyphs?.Dispose();
                _glyphs = glyphs;
                _text = text; _font = font; _color = color; _flags = flags; _dpi = dpi;
            }
            graphics.DrawImageUnscaled(_glyphs, bounds.Location);
        }

        private static Bitmap Render(string text, Font font, Size size, Color color, TextFormatFlags flags)
        {
            byte[] pixels = new byte[checked(size.Width * size.Height * 4)];
            using (var buffer = new ChromiumButtonComposition.GdiBuffer(size))
                buffer.Paint(graphics =>
                {
                    graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    TextRenderer.DrawText(graphics, text, font, new Rectangle(Point.Empty, size), Color.Black, flags);
                }, Color.White, pixels);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                // RGB subpixel ordering must not survive into a transparent
                // texture. Average coverage equally so neither edge is tinted.
                int coverage = 255 - (pixels[i] + pixels[i + 1] + pixels[i + 2] + 1) / 3;
                int alpha = (coverage * color.A + 127) / 255;
                pixels[i] = (byte)((color.B * alpha + 127) / 255);
                pixels[i + 1] = (byte)((color.G * alpha + 127) / 255);
                pixels[i + 2] = (byte)((color.R * alpha + 127) / 255);
                pixels[i + 3] = (byte)alpha;
            }
            var bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppPArgb);
            try
            {
                var data = bitmap.LockBits(new Rectangle(Point.Empty, size), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
                try { Marshal.Copy(pixels, 0, data.Scan0, pixels.Length); }
                finally { bitmap.UnlockBits(data); }
                return bitmap;
            }
            catch { bitmap.Dispose(); throw; }
        }

        public void Dispose()
        {
            _glyphs?.Dispose();
            _glyphs = null;
            _font = null;
        }
    }
}
