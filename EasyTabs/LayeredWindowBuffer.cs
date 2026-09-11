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
        private IntPtr _dc, _bitmap, _oldBitmap;
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
                IntPtr bits;
                _bitmap = CreateDIBSection(_dc, ref info, 0, out bits, IntPtr.Zero, 0);
                if (_bitmap == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
                _oldBitmap = Gdi32.SelectObject(_dc, _bitmap);
                if (_oldBitmap == IntPtr.Zero || _oldBitmap == new IntPtr(-1))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                Bitmap = new Bitmap(width, height, checked(width * 4), PixelFormat.Format32bppPArgb, bits);
                Graphics = Graphics.FromImage(Bitmap);
            }
            catch { Dispose(); throw; }
        }

        public void Dispose()
        {
            Graphics?.Dispose(); Graphics = null;
            Bitmap?.Dispose(); Bitmap = null;
            if (_oldBitmap != IntPtr.Zero && _oldBitmap != new IntPtr(-1)) Gdi32.SelectObject(_dc, _oldBitmap);
            if (_bitmap != IntPtr.Zero) Gdi32.DeleteObject(_bitmap);
            if (_dc != IntPtr.Zero) Gdi32.DeleteDC(_dc);
            _dc = _bitmap = _oldBitmap = IntPtr.Zero;
        }
    }
}
