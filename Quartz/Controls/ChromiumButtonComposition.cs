using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace Quartz.Controls
{
    // One tree owns the complete face. WinForms supplies content changes and
    // keeps input/layout; DWM animates the ink without per-frame UI callbacks.
    internal sealed class ChromiumButtonComposition : IDisposable
    {
        private DirectComposition.DeviceContext _context;
        private DirectComposition.ITarget _target;
        private DirectComposition.IVisual _root, _background, _ink, _highlight, _ripple, _foreground;
        private DirectComposition.IClip _clip;
        private DirectComposition.IScale _scale;
        private DirectComposition.IEffect _highlightOpacity, _rippleOpacity;
        private DirectComposition.ISurface _foregroundSurface, _backgroundSurface;
        private SKBitmap _foregroundPixels;
        private GdiBuffer _blackCapture, _whiteCapture;
        private byte[] _black, _white;
        private Size _size;
        private Size _backgroundSize;
        private RectangleF _bounds;
        private PointF _origin;
        private float _cornerRadius, _minimumRadius, _maximumRadius;
        private double _highlightMaximum, _rippleMaximum;
        private Color _highlightColor, _rippleColor;
        private bool _configured;

        internal ChromiumButtonComposition(IntPtr hwnd)
        {
            try
            {
                _context = DirectComposition.Acquire();
                var device = _context.Device;
                // Profile action child controls must remain above this face.
                device.CreateTargetForHwnd(hwnd, false, out _target);
                device.CreateVisual(out _root);
                device.CreateVisual(out _background);
                device.CreateVisual(out _ink);
                device.CreateVisual(out _highlight);
                device.CreateVisual(out _ripple);
                device.CreateVisual(out _foreground);
                device.CreateRectangleClip(out _clip);
                device.CreateScaleTransform(out _scale);
                device.CreateEffectGroup(out _highlightOpacity);
                device.CreateEffectGroup(out _rippleOpacity);
                _highlightOpacity.SetOpacity(0); _rippleOpacity.SetOpacity(0);
                _highlight.SetEffect(_highlightOpacity);
                _ripple.SetEffect(_rippleOpacity);
                _ripple.SetTransform(_scale);
                _scale.SetCenterX(128); _scale.SetCenterY(128);
                _ripple.SetBitmapInterpolationMode(1); // Linear filtering.
                // Texture filtering does not antialias rounded CLIP edges.
                _root.SetBorderMode(0); // SOFT, inherited by every child.
                _root.SetBitmapInterpolationMode(1);
                _foreground.SetBitmapInterpolationMode(0); // Glyphs stay at native size.
                _ink.SetClip(_clip);
                _ink.AddVisual(_highlight, true, null);
                _ink.AddVisual(_ripple, true, _highlight);
                _root.AddVisual(_background, true, null);
                _root.AddVisual(_ink, true, _background);
                _root.AddVisual(_foreground, true, _ink);
                _target.SetRoot(_root);
            }
            catch { Dispose(); throw; }
        }

        internal bool Configure(Size size, RectangleF bounds, float cornerRadius, PointF origin,
            float minimumRadius, Color highlightColor, double highlightMaximum, Color rippleColor, double rippleMaximum)
        {
            bool changed = !_configured || _size != size || _bounds != bounds || _origin != origin ||
                _cornerRadius != cornerRadius || _minimumRadius != minimumRadius ||
                _highlightColor != highlightColor || _rippleColor != rippleColor ||
                _highlightMaximum != highlightMaximum || _rippleMaximum != rippleMaximum;
            if (!changed) return false;
            _context.Device.CheckDeviceState(out bool valid);
            if (!valid) throw new COMException("The button composition device was lost.");
            _size = size; _bounds = bounds; _cornerRadius = cornerRadius; _origin = origin;
            _minimumRadius = minimumRadius; _highlightMaximum = highlightMaximum; _rippleMaximum = rippleMaximum;
            _highlightColor = highlightColor; _rippleColor = rippleColor; _configured = true;
            double dx = Math.Max(Math.Abs(origin.X), Math.Abs(size.Width - origin.X));
            double dy = Math.Max(Math.Abs(origin.Y), Math.Abs(size.Height - origin.Y));
            _maximumRadius = (float)Math.Sqrt(dx * dx + dy * dy);
            _clip.SetLeft(bounds.Left); _clip.SetTop(bounds.Top); _clip.SetRight(bounds.Right); _clip.SetBottom(bounds.Bottom);
            _clip.SetTopLeftRadiusX(cornerRadius); _clip.SetTopLeftRadiusY(cornerRadius);
            _clip.SetTopRightRadiusX(cornerRadius); _clip.SetTopRightRadiusY(cornerRadius);
            _clip.SetBottomLeftRadiusX(cornerRadius); _clip.SetBottomLeftRadiusY(cornerRadius);
            _clip.SetBottomRightRadiusX(cornerRadius); _clip.SetBottomRightRadiusY(cornerRadius);
            _highlight.SetContent(_context.Texture(highlightColor, false));
            var matrix = new DirectComposition.Matrix { M11 = bounds.Width, M22 = bounds.Height, X = bounds.X, Y = bounds.Y };
            _highlight.SetMatrix(ref matrix);
            _ripple.SetContent(_context.Texture(rippleColor, true));
            _ripple.SetOffsetX(origin.X - 128); _ripple.SetOffsetY(origin.Y - 128);
            return true;
        }

        internal void Animate(ChromiumButtonAnimation state, double now, bool enabled)
        {
            long begin = Stopwatch.GetTimestamp();
            var frames = state.CaptureTimeline(now);
            SetCurve(frames, frame => (float)(enabled ? frame.Highlight * _highlightMaximum : 0),
                _highlightOpacity.SetOpacity, _highlightOpacity.AnimateOpacity, begin);
            SetCurve(frames, frame => (float)(enabled ? frame.Opacity * _rippleMaximum : 0),
                _rippleOpacity.SetOpacity, _rippleOpacity.AnimateOpacity, begin);
            SetCurve(frames, frame => (_minimumRadius + (_maximumRadius - _minimumRadius) * (float)frame.Radius) / 127,
                value => { _scale.SetScaleX(value); _scale.SetScaleY(value); },
                curve => { _scale.AnimateScaleX(curve); _scale.AnimateScaleY(curve); }, begin);
        }

        private void SetCurve(List<ChromiumButtonAnimation.Frame> frames, Func<ChromiumButtonAnimation.Frame, float> value,
            Action<float> set, Action<DirectComposition.IAnimation> animate, long begin)
        {
            float first = value(frames[0]);
            bool constant = true;
            foreach (var frame in frames) if (value(frame) != first) { constant = false; break; }
            if (constant) { set(first); return; }
            DirectComposition.IAnimation curve;
            _context.Device.CreateAnimation(out curve);
            try
            {
                curve.SetAbsoluteBeginTime(begin);
                for (int i = 0; i + 1 < frames.Count; i++)
                {
                    double duration = frames[i + 1].Time - frames[i].Time;
                    float a = value(frames[i]), b = value(frames[i + 1]);
                    curve.AddCubic(frames[i].Time, a, (float)((b - a) / duration), 0, 0);
                }
                var last = frames[frames.Count - 1];
                curve.End(last.Time, value(last));
                animate(curve);
            }
            finally { DirectComposition.Release(curve); }
        }

        internal void PaintForeground(Action<Graphics> paint)
        {
            if (_foregroundPixels == null || _foregroundPixels.Width != _size.Width || _foregroundPixels.Height != _size.Height)
            {
                DisposePixels();
                _foregroundPixels = new SKBitmap(_size.Width, _size.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
                _blackCapture = new GdiBuffer(_size);
                _whiteCapture = new GdiBuffer(_size);
                _black = new byte[_foregroundPixels.ByteCount]; _white = new byte[_foregroundPixels.ByteCount];
            }
            // GDI TextRenderer does not preserve alpha. Recover coverage from
            // two cached backgrounds so the original text/layout and derived
            // button glyphs can stay ABOVE the ink, without per-frame uploads.
            _blackCapture.Paint(paint, Color.Black, _black);
            _whiteCapture.Paint(paint, Color.White, _white);
            for (int i = 0; i < _black.Length; i += 4)
            {
                int difference = Math.Min(_white[i] - _black[i], Math.Min(_white[i + 1] - _black[i + 1], _white[i + 2] - _black[i + 2]));
                _black[i + 3] = (byte)Math.Max(0, Math.Min(255, 255 - difference));
            }
            Marshal.Copy(_black, 0, _foregroundPixels.GetPixels(), _black.Length);
            PaintSurface(ref _foregroundSurface, _foreground, _foregroundPixels, false);
        }

        internal void PaintBackground(SKBitmap pixels)
        {
            var size = new Size(pixels.Width, pixels.Height);
            PaintSurface(ref _backgroundSurface, _background, pixels, _backgroundSize != size);
            _backgroundSize = size;
        }

        private void PaintSurface(ref DirectComposition.ISurface surface, DirectComposition.IVisual visual,
            SKBitmap pixels, bool replace)
        {
            if (surface != null && !replace)
            {
                try
                {
                    _context.Upload(surface, pixels);
                    return;
                }
                catch (Exception error) when (DirectComposition.IsInterfaceFailure(error))
                {
                    // Rebuild only the failed cached surface. The visual tree
                    // and its committed ripple timeline remain active.
                    Trace.WriteLine("Replacing unavailable button surface: " + error.Message);
                }
            }
            // Publish the replacement only after creation, upload and binding
            // succeed. A second failure reaches the control's raster fallback.
            var replacement = _context.CreateSurface(pixels);
            try { visual.SetContent(replacement); }
            catch { DirectComposition.Release(replacement); throw; }
            var previous = surface;
            surface = replacement;
            DirectComposition.Release(previous);
        }

        internal void Commit() => _context.Device.Commit();

        private void DisposePixels()
        {
            DirectComposition.Release(_foregroundSurface); _foregroundSurface = null;
            _blackCapture?.Dispose(); _blackCapture = null;
            _whiteCapture?.Dispose(); _whiteCapture = null;
            _foregroundPixels?.Dispose(); _foregroundPixels = null;
            _black = _white = null;
        }

        // Graphics.FromImage gives GDI a write-only, sentinel-filled temporary
        // bitmap. TextRenderer would antialias against that sentinel instead
        // of our black/white mattes, destroying the recovered edge coverage.
        // A real DIB-backed HDC lets both GDI text and GDI+ images blend against
        // the same initialized pixels. These buffers are reused between paints.
        private sealed class GdiBuffer : IDisposable
        {
            private IntPtr _dc, _bitmap, _previous, _bits;

            internal GdiBuffer(Size size)
            {
                try
                {
                    _dc = CreateCompatibleDC(IntPtr.Zero);
                    if (_dc == IntPtr.Zero) throw new Win32Exception();
                    var info = new BitmapInfo { HeaderSize = 40, Width = size.Width, Height = -size.Height,
                        Planes = 1, BitCount = 32 };
                    _bitmap = CreateDIBSection(_dc, ref info, 0, out _bits, IntPtr.Zero, 0);
                    if (_bitmap == IntPtr.Zero) throw new Win32Exception();
                    _previous = SelectObject(_dc, _bitmap);
                    if (_previous == IntPtr.Zero || _previous == new IntPtr(-1)) throw new Win32Exception();
                }
                catch { Dispose(); throw; }
            }

            internal void Paint(Action<Graphics> paint, Color background, byte[] pixels)
            {
                using (var graphics = Graphics.FromHdc(_dc))
                {
                    graphics.Clear(background);
                    // A transparent texture has one coverage value per pixel;
                    // ClearType's three subpixel coverages cannot be stored in it.
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    paint(graphics);
                    graphics.Flush(System.Drawing.Drawing2D.FlushIntention.Sync);
                }
                GdiFlush(); // Finish GDI writes before reading the DIB memory.
                Marshal.Copy(_bits, pixels, 0, pixels.Length);
            }

            public void Dispose()
            {
                if (_previous != IntPtr.Zero && _previous != new IntPtr(-1)) SelectObject(_dc, _previous);
                if (_bitmap != IntPtr.Zero) DeleteObject(_bitmap);
                if (_dc != IntPtr.Zero) DeleteDC(_dc);
                _dc = _bitmap = _previous = _bits = IntPtr.Zero;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct BitmapInfo
            {
                internal uint HeaderSize;
                internal int Width, Height;
                internal ushort Planes, BitCount;
                internal uint Compression, ImageSize;
                internal int XPelsPerMeter, YPelsPerMeter;
                internal uint ColorsUsed, ColorsImportant, ColorTable;
            }

            [DllImport("gdi32.dll", SetLastError = true)]
            private static extern IntPtr CreateCompatibleDC(IntPtr dc);
            [DllImport("gdi32.dll", SetLastError = true)]
            private static extern IntPtr CreateDIBSection(IntPtr dc, ref BitmapInfo info,
                uint usage, out IntPtr bits, IntPtr section, uint offset);
            [DllImport("gdi32.dll")]
            private static extern IntPtr SelectObject(IntPtr dc, IntPtr value);
            [DllImport("gdi32.dll")]
            private static extern bool DeleteObject(IntPtr value);
            [DllImport("gdi32.dll")]
            private static extern bool DeleteDC(IntPtr dc);
            [DllImport("gdi32.dll")]
            private static extern bool GdiFlush();
        }

        public void Dispose()
        {
            if (_target != null)
            {
                try { _target.SetRoot(null); _context.Device.Commit(); }
                catch (Exception error) when (error is COMException || DirectComposition.IsInterfaceFailure(error)) { }
            }
            DisposePixels();
            DirectComposition.Release(_backgroundSurface); _backgroundSurface = null;
            foreach (object item in new object[] { _foreground, _ripple, _highlight, _ink, _background, _root, _clip, _scale,
                _highlightOpacity, _rippleOpacity, _target }) DirectComposition.Release(item);
            _foreground = _ripple = _highlight = _ink = _background = _root = null;
            _clip = null; _scale = null; _highlightOpacity = _rippleOpacity = null; _target = null;
            if (_context != null) DirectComposition.Release(_context);
            _context = null;
        }
    }
}
