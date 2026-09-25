using SkiaSharp;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Quartz.Controls.ChromiumMenus
{
    // Window hosting only. The cached premultiplied Skia image also supplies fade
    // frames, so animation cannot change layout or rerasterize the text.
    internal class MenuLayeredWindow : Form
    {
        private IntPtr dc, dib, previous, bits;
        private Size imageSize;
        internal byte SurfaceAlpha = 255;
        internal bool InputTransparent;
        internal MenuLayeredWindow()
        {
            FormBorderStyle = FormBorderStyle.None; ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual; AutoScaleMode = AutoScaleMode.None;
        }
        protected override bool ShowWithoutActivation => true;
        protected override CreateParams CreateParams
        {
            get { var p = base.CreateParams; p.ExStyle |= 0x80000 | 0x08000000 | 0x80 | (InputTransparent ? 0x20 : 0); return p; }
        }
        internal void SetImage(SKBitmap bitmap)
        {
            if (imageSize != Size || dib == IntPtr.Zero)
            {
                ReleaseImage();
                IntPtr screen = MenuPopup.Native.GetDC(IntPtr.Zero);
                try
                {
                    dc = MenuPopup.Native.CreateCompatibleDC(screen);
                    var info = new MenuPopup.BitmapInfo { Size = 40, Width = Width, Height = -Height, Planes = 1, BitCount = 32 };
                    dib = MenuPopup.Native.CreateDIBSection(dc, ref info, 0, out bits, IntPtr.Zero, 0);
                    if (dib == IntPtr.Zero) throw new System.ComponentModel.Win32Exception();
                    previous = MenuPopup.Native.SelectObject(dc, dib); imageSize = Size;
                }
                finally { MenuPopup.Native.ReleaseDC(IntPtr.Zero, screen); }
            }
            var bytes = new byte[bitmap.ByteCount]; Marshal.Copy(bitmap.GetPixels(), bytes, 0, bytes.Length); Marshal.Copy(bytes, 0, bits, bytes.Length);
            Present();
        }
        internal void Present()
        {
            if (IsDisposed || !IsHandleCreated || dib == IntPtr.Zero) return;
            IntPtr screen = MenuPopup.Native.GetDC(IntPtr.Zero);
            try
            {
                var at = Location; var size = imageSize; var origin = Point.Empty;
                var blend = new MenuPopup.Blend { SourceConstantAlpha = SurfaceAlpha, AlphaFormat = 1 };
                if (!MenuPopup.Native.UpdateLayeredWindow(Handle, screen, ref at, ref size, dc, ref origin, 0, ref blend, 2)) throw new System.ComponentModel.Win32Exception();
            }
            finally { MenuPopup.Native.ReleaseDC(IntPtr.Zero, screen); }
        }
        internal void MakeInputTransparent()
        {
            InputTransparent = true;
            SetWindowLong(Handle, -20, GetWindowLong(Handle, -20) | 0x20);
        }
        private void ReleaseImage()
        {
            if (dc != IntPtr.Zero) { MenuPopup.Native.SelectObject(dc, previous); MenuPopup.Native.DeleteObject(dib); MenuPopup.Native.DeleteDC(dc); }
            dc = dib = previous = bits = IntPtr.Zero;
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x21) { m.Result = new IntPtr(3); return; }
            if (InputTransparent && m.Msg == 0x84) { m.Result = new IntPtr(-1); return; }
            base.WndProc(ref m);
        }
        protected override void Dispose(bool disposing) { if (disposing) ReleaseImage(); base.Dispose(disposing); }
        [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hwnd, int index);
        [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr hwnd, int index, int value);
    }
}
