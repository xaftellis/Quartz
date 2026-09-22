// Algorithms and icon data derived from Chromium 85. See docs/Chromium-LICENSE.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SkiaSharp;

namespace Quartz.Controls
{
    public enum ChromiumIcon { None, Back, Forward, Reload, Stop, Favourite, Download, Settings }

    // ImageSkia/ImageView-style device-scale cache. All visible button icons go
    // through this renderer; ButtonBase's image adapter is not used for output.
    internal sealed class ChromiumButtonImage : IDisposable
    {
        private Image _source;
        private ChromiumIcon _icon;
        private Size _size;
        private Color _color;
        private SKBitmap _pixels;
        private SKBitmap _drawingPixels;
        private Bitmap _drawingImage;
        private bool _drawingEnabled, _drawingMirrored;

        internal SKBitmap Get(Image source, ChromiumIcon icon, Size size, Color color)
        {
            if (_pixels != null && ReferenceEquals(source, _source) && _icon == icon &&
                size == _size && color == _color)
                return _pixels;
            Dispose();
            _source = source; _icon = icon; _size = size; _color = color;
            _pixels = NewBitmap(size.Width, size.Height);
            if (icon != ChromiumIcon.None)
            {
                VectorRep rep = GetRepresentation(icon, Math.Max(size.Width, size.Height));
                using (var canvas = new SKCanvas(_pixels))
                using (var paint = new SKPaint { IsAntialias = true, Color = new SKColor(color.R, color.G, color.B, color.A) })
                {
                    canvas.Clear(SKColors.Transparent);
                    canvas.Scale(size.Width / (float)rep.Dimension, size.Height / (float)rep.Dimension);
                    canvas.DrawPath(rep.Path, paint);
                }
            }
            else if (source != null)
            {
                // No GDI+ interpolation, grey disabled bitmap or pressed offset.
                // Chromium ImageView requests RESIZE_BEST (Lanczos3) when a
                // representation for the current device scale is unavailable.
                byte[] output;
                if (!ToolbarPreparation.TryGet(source, size, out output))
                {
                    byte[] input = ReadPremultipliedPixels(source);
                    output = source.Size == size ? input : Resize(input, source.Width, source.Height, size.Width, size.Height);
                }
                Marshal.Copy(output, 0, _pixels.GetPixels(), output.Length);
            }
            return _pixels;
        }

        internal Bitmap GetDrawingImage(Image source, ChromiumIcon icon, Size size, Color color, bool enabled, bool mirrored)
        {
            SKBitmap pixels = Get(source, icon, size, color);
            if (_drawingImage != null && _drawingEnabled == enabled && _drawingMirrored == mirrored)
                return _drawingImage;

            DisposeDrawingImage();
            _drawingEnabled = enabled;
            _drawingMirrored = mirrored;
            if (!enabled || mirrored)
            {
                _drawingPixels = NewBitmap(size.Width, size.Height);
                using (var canvas = new SKCanvas(_drawingPixels))
                using (var paint = new SKPaint { Color = new SKColor(255, 255, 255, enabled ? (byte)255 : (byte)110) })
                {
                    canvas.Clear(SKColors.Transparent);
                    if (mirrored)
                    {
                        canvas.Translate(size.Width, 0);
                        canvas.Scale(-1, 1);
                    }
                    canvas.DrawBitmap(pixels, 0, 0, new SKSamplingOptions(SKFilterMode.Nearest), paint);
                    canvas.Flush();
                }
                pixels = _drawingPixels;
            }
            _drawingImage = new Bitmap(size.Width, size.Height, pixels.RowBytes,
                PixelFormat.Format32bppPArgb, pixels.GetPixels());
            return _drawingImage;
        }

        // Browser registers only its immutable, bundled toolbar artwork. Reading
        // the Image stays on its owner thread; the expensive Lanczos pass receives
        // detached bytes and runs in the background. Every Browser form can reuse
        // the completed result without sharing mutable GDI or Skia objects.
        internal static void PrepareToolbarImage(string assetKey, Image source, Size size)
        {
            ToolbarPreparation.Start(assetKey, source, size);
        }

        private static class ToolbarPreparation
        {
            private sealed class SourceKey
            {
                internal readonly string Asset;
                internal SourceKey(string asset) { Asset = asset; }
            }

            private static readonly object Gate = new object();
            private static readonly ConditionalWeakTable<Image, SourceKey> Sources =
                new ConditionalWeakTable<Image, SourceKey>();
            private static readonly Dictionary<string, Task<byte[]>> Results = new Dictionary<string, Task<byte[]>>();

            private static string Key(string asset, Size size) => asset + ":" + size.Width + "x" + size.Height;

            internal static void Start(string asset, Image source, Size size)
            {
                if (source == null || size.Width <= 0 || size.Height <= 0 || source.Size == size) return;
                lock (Gate)
                {
                    if (!Sources.TryGetValue(source, out SourceKey existing))
                        Sources.Add(source, new SourceKey(asset));
                    string key = Key(asset, size);
                    if (Results.ContainsKey(key)) return;
                    byte[] input = ReadPremultipliedPixels(source);
                    int width = source.Width, height = source.Height;
                    Results.Add(key, Task.Run(() => Resize(input, width, height, size.Width, size.Height)));
                }
            }

            internal static bool TryGet(Image source, Size size, out byte[] pixels)
            {
                pixels = null;
                Task<byte[]> task;
                string key;
                lock (Gate)
                {
                    if (!Sources.TryGetValue(source, out SourceKey entry)) return false;
                    key = Key(entry.Asset, size);
                    if (!Results.TryGetValue(key, out task))
                    {
                        Start(entry.Asset, source, size);
                        Results.TryGetValue(key, out task);
                    }
                }
                if (task == null || !task.IsCompleted) return false;
                if (task.IsFaulted || task.IsCanceled)
                {
                    var ignored = task.Exception; // Observe a failed background task.
                    lock (Gate)
                        if (Results.TryGetValue(key, out Task<byte[]> current) && ReferenceEquals(current, task))
                            Results.Remove(key);
                    return false;
                }
                pixels = task.Result;
                return true;
            }
        }

        private void DisposeDrawingImage()
        {
            // GDI's bitmap borrows the Skia pixels; release it first.
            _drawingImage?.Dispose();
            _drawingImage = null;
            _drawingPixels?.Dispose();
            _drawingPixels = null;
        }

        internal static SKBitmap NewBitmap(int width, int height) =>
            new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul));

        private static byte[] ReadPremultipliedPixels(Image source)
        {
            using (var copy = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppPArgb))
            {
                using (Graphics graphics = Graphics.FromImage(copy))
                {
                    graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    graphics.DrawImageUnscaled(source, 0, 0);
                }
                var bounds = new Rectangle(Point.Empty, copy.Size);
                BitmapData data = copy.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
                try
                {
                    var bytes = new byte[copy.Width * copy.Height * 4];
                    for (int y = 0; y < copy.Height; y++)
                        Marshal.Copy(IntPtr.Add(data.Scan0, y * data.Stride), bytes, y * copy.Width * 4, copy.Width * 4);
                    return bytes;
                }
                finally { copy.UnlockBits(data); }
            }
        }

        private struct Filter
        {
            internal int Start;
            internal int[] Weights;
        }

        // skia/ext/image_operations.cc ResizeFilter::ComputeFilters, and
        // skia/ext/convolver.cc: separable, normalized 14-bit fixed-point filter.
        private static Filter[] Filters(int source, int destination)
        {
            float scale = destination / (float)source, clamped = Math.Min(1, scale);
            float support = 3 / clamped, inverse = 1 / scale;
            var result = new Filter[destination];
            for (int i = 0; i < destination; i++)
            {
                float center = (i + .5f) * inverse;
                int first = Math.Max(0, (int)Math.Floor(center - support));
                int last = Math.Min(source - 1, (int)Math.Ceiling(center + support));
                var values = new float[last - first + 1];
                float total = 0;
                for (int j = 0; j < values.Length; j++)
                {
                    float distance = (first + j + .5f - center) * clamped;
                    float angle = distance * (float)Math.PI;
                    float weight = Math.Abs(distance) >= 3 ? 0 : Math.Abs(distance) < 1.1920929e-7f ? 1
                        : (float)((Math.Sin(angle) / angle) * Math.Sin(angle / 3) / (angle / 3));
                    values[j] = weight;
                    total += weight;
                }
                var weights = new int[values.Length];
                int fixedTotal = 0;
                for (int j = 0; j < weights.Length; j++)
                {
                    weights[j] = (int)(values[j] / total * (1 << 14));
                    fixedTotal += weights[j];
                }
                weights[weights.Length / 2] += (1 << 14) - fixedTotal;
                result[i] = new Filter { Start = first, Weights = weights };
            }
            return result;
        }

        private static byte[] Resize(byte[] input, int width, int height, int targetWidth, int targetHeight)
        {
            Filter[] horizontal = Filters(width, targetWidth), vertical = Filters(height, targetHeight);
            var rows = new byte[targetWidth * height * 4];
            var output = new byte[targetWidth * targetHeight * 4];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < targetWidth; x++)
                    for (int channel = 0; channel < 4; channel++)
                    {
                        Filter filter = horizontal[x];
                        int sum = 0;
                        for (int n = 0; n < filter.Weights.Length; n++)
                            sum += input[(y * width + filter.Start + n) * 4 + channel] * filter.Weights[n];
                        rows[(y * targetWidth + x) * 4 + channel] = Clamp(sum >> 14);
                    }
            for (int y = 0; y < targetHeight; y++)
                for (int x = 0; x < targetWidth; x++)
                {
                    int offset = (y * targetWidth + x) * 4;
                    for (int channel = 0; channel < 4; channel++)
                    {
                        Filter filter = vertical[y];
                        int sum = 0;
                        for (int n = 0; n < filter.Weights.Length; n++)
                            sum += rows[((filter.Start + n) * targetWidth + x) * 4 + channel] * filter.Weights[n];
                        output[offset + channel] = Clamp(sum >> 14);
                    }
                    // Keep premultiplied RGB <= alpha after ringing/rounding.
                    output[offset + 3] = Math.Max(output[offset + 3], Math.Max(output[offset], Math.Max(output[offset + 1], output[offset + 2])));
                }
            return output;
        }

        private static byte Clamp(int value) => (byte)Math.Max(0, Math.Min(255, value));

        private sealed class VectorRep
        {
            internal int Dimension = 48; // paint_vector_icon.cc kReferenceSizeDip
            internal SKPath Path;
            internal SKPathBuilder Builder = new SKPathBuilder { FillType = SKPathFillType.EvenOdd };
            internal SKPoint Last, Start;
        }

        private static readonly Dictionary<ChromiumIcon, VectorRep[]> Vectors = new Dictionary<ChromiumIcon, VectorRep[]>();
        private static readonly string[] Files = { null, "back_arrow", "forward_arrow", "reload", "navigate_stop", "star", "file_download", "settings" };

        private static VectorRep GetRepresentation(ChromiumIcon icon, int pixels)
        {
            VectorRep[] reps;
            lock (Vectors)
            {
                if (!Vectors.TryGetValue(icon, out reps))
                    Vectors.Add(icon, reps = ReadVector(Files[(int)icon]));
            }
            // paint_vector_icon.cc GetRepForPxSize. Prefer exact size, then an
            // exact divisor, then the next larger representation, then largest.
            VectorRep best = null;
            foreach (VectorRep rep in reps)
            {
                if (rep.Dimension == pixels) return rep;
                if (pixels % rep.Dimension == 0) best = rep;
                if (rep.Dimension > pixels) return best ?? rep;
            }
            return best ?? reps[reps.Length - 1];
        }

        private static VectorRep[] ReadVector(string name)
        {
            var reps = new List<VectorRep>();
            VectorRep rep = null;
            using (Stream stream = typeof(ChromiumButtonImage).Assembly.GetManifestResourceStream("Quartz.ChromiumIcons." + name + ".icon"))
            using (var reader = new StreamReader(stream ?? throw new InvalidOperationException("Missing Chromium icon: " + name)))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Split(new[] { "//" }, StringSplitOptions.None)[0].Trim();
                    if (line.Length == 0) continue;
                    string[] tokens = line.TrimEnd(',').Split(',').Select(t => t.Trim()).ToArray();
                    string command = tokens[0];
                    float[] a = tokens.Skip(1).Select(t => float.Parse(t.TrimEnd('f'), CultureInfo.InvariantCulture)).ToArray();
                    if (command == "CANVAS_DIMENSIONS")
                    {
                        rep = new VectorRep { Dimension = (int)a[0] };
                        reps.Add(rep);
                        continue;
                    }
                    if (rep == null) { rep = new VectorRep(); reps.Add(rep); }
                    SKPathBuilder path = rep.Builder;
                    SKPoint last = rep.Last;
                    switch (command)
                    {
                        case "MOVE_TO": path.MoveTo(a[0], a[1]); rep.Last = rep.Start = new SKPoint(a[0], a[1]); break;
                        case "LINE_TO": path.LineTo(a[0], a[1]); rep.Last = new SKPoint(a[0], a[1]); break;
                        case "R_LINE_TO": path.RLineTo(a[0], a[1]); rep.Last = new SKPoint(last.X + a[0], last.Y + a[1]); break;
                        case "H_LINE_TO": path.LineTo(a[0], last.Y); rep.Last = new SKPoint(a[0], last.Y); break;
                        case "V_LINE_TO": path.LineTo(last.X, a[0]); rep.Last = new SKPoint(last.X, a[0]); break;
                        case "R_H_LINE_TO": path.RLineTo(a[0], 0); rep.Last = new SKPoint(last.X + a[0], last.Y); break;
                        case "R_V_LINE_TO": path.RLineTo(0, a[0]); rep.Last = new SKPoint(last.X, last.Y + a[0]); break;
                        case "CUBIC_TO":
                            path.CubicTo(a[0], a[1], a[2], a[3], a[4], a[5]);
                            rep.Last = new SKPoint(a[4], a[5]); break;
                        case "R_CUBIC_TO":
                            path.RCubicTo(a[0], a[1], a[2], a[3], a[4], a[5]);
                            rep.Last = new SKPoint(last.X + a[4], last.Y + a[5]); break;
                        case "CLOSE": path.Close(); rep.Last = rep.Start; break;
                        default: throw new InvalidOperationException("Unsupported Chromium icon command: " + command);
                    }
                }
            }
            foreach (VectorRep item in reps)
            {
                item.Path = item.Builder.Detach();
                item.Builder.Dispose();
                item.Builder = null;
            }
            return reps.OrderBy(r => r.Dimension).ToArray();
        }

        public void Dispose()
        {
            DisposeDrawingImage();
            _pixels?.Dispose();
            _pixels = null;
            _source = null;
        }
    }
}
