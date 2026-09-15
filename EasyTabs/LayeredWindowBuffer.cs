using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Win32Interop.Methods;

namespace EasyTabs
{
    /// <summary>A reusable top-down, premultiplied bitmap shared by GDI+ and UpdateLayeredWindow.</summary>
    internal sealed class LayeredWindowBuffer : IDisposable
    {
        private IntPtr _dc, _bitmap, _oldBitmap, _bits;
        private byte[] _cornerMask;
        private int _cornerRadius;
        internal Bitmap Bitmap { get; private set; }
        internal Graphics Graphics { get; private set; }
        internal IntPtr DeviceContext => _dc;

        [StructLayout(LayoutKind.Sequential)]
        private struct BitmapInfo
        {
            internal uint Size;
            internal int Width, Height;
            internal ushort Planes, BitCount;
            internal uint Compression, SizeImage;
            internal int XPelsPerMeter, YPelsPerMeter;
            internal uint ClrUsed, ClrImportant, Colour;
        }

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr CreateDIBSection(IntPtr dc, ref BitmapInfo info,
            uint usage, out IntPtr bits, IntPtr section, uint offset);

        internal void EnsureSize(int width, int height)
        {
            if (Bitmap != null && Bitmap.Width == width && Bitmap.Height == height) return;
            Dispose();
            try
            {
                _dc = Gdi32.CreateCompatibleDC(IntPtr.Zero);
                if (_dc == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
                var info = new BitmapInfo { Size = 40, Width = width, Height = -height, Planes = 1, BitCount = 32 };
                _bitmap = CreateDIBSection(_dc, ref info, 0, out _bits, IntPtr.Zero, 0);
                if (_bitmap == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
                _oldBitmap = Gdi32.SelectObject(_dc, _bitmap);
                if (_oldBitmap == IntPtr.Zero || _oldBitmap == new IntPtr(-1))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                Bitmap = new Bitmap(width, height, checked(width * 4), PixelFormat.Format32bppPArgb, _bits);
                Graphics = Graphics.FromImage(Bitmap);
            }
            catch { Dispose(); throw; }
        }

        internal void ClipTopCorners(int radius)
        {
            radius = Math.Min(radius, Math.Min(Bitmap.Width / 2, Bitmap.Height));
            if (radius <= 0) return;
            if (_cornerMask == null || _cornerRadius != radius)
            {
                _cornerRadius = radius;
                _cornerMask = new byte[radius * radius];
                for (int y = 0; y < radius; y++)
                    for (int x = 0; x < radius; x++)
                    {
                        int covered = 0;
                        for (int sy = 0; sy < 8; sy++)
                            for (int sx = 0; sx < 8; sx++)
                            {
                                double dx = radius - x - (sx + .5) / 8;
                                double dy = radius - y - (sy + .5) / 8;
                                if (dx * dx + dy * dy <= radius * radius) covered++;
                            }
                        _cornerMask[y * radius + x] = (byte)((covered * 255 + 32) / 64);
                    }
            }
            // GDI+ SourceCopy with a transparent brush hard-clears edge pixels.
            // Multiply all premultiplied channels by coverage to retain a smooth edge.
            Graphics.Flush(System.Drawing.Drawing2D.FlushIntention.Sync);
            for (int y = 0; y < radius; y++)
                for (int x = 0; x < radius; x++)
                {
                    uint alpha = _cornerMask[y * radius + x];
                    if (alpha == 255) continue;
                    MaskPixel((y * Bitmap.Width + x) * 4, alpha);
                    MaskPixel((y * Bitmap.Width + Bitmap.Width - 1 - x) * 4, alpha);
                }
        }

        private void MaskPixel(int offset, uint alpha)
        {
            uint pixel = unchecked((uint)Marshal.ReadInt32(_bits, offset));
            uint result = 0;
            for (int shift = 0; shift < 32; shift += 8)
                result |= ((((pixel >> shift) & 255) * alpha + 127) / 255) << shift;
            Marshal.WriteInt32(_bits, offset, unchecked((int)result));
        }

        public void Dispose()
        {
            Graphics?.Dispose(); Graphics = null;
            Bitmap?.Dispose(); Bitmap = null;
            if (_oldBitmap != IntPtr.Zero && _oldBitmap != new IntPtr(-1)) Gdi32.SelectObject(_dc, _oldBitmap);
            if (_bitmap != IntPtr.Zero) Gdi32.DeleteObject(_bitmap);
            if (_dc != IntPtr.Zero) Gdi32.DeleteDC(_dc);
            _dc = _bitmap = _oldBitmap = _bits = IntPtr.Zero;
        }
    }
}
