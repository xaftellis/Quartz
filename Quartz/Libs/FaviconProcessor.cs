using ImageMagick;
using Quartz.Services;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Quartz.Libs
{
    // At most two fetched images/conversions at once across all tabs. Browser
    // coalesces repeated events before this queue, keeping only its latest request.
    internal static class FaviconProcessor
    {
        private static readonly SemaphoreSlim WorkSlots = new SemaphoreSlim(2);

        internal sealed class Result : IDisposable
        {
            internal Icon Icon;
            internal Bitmap FavouriteImage;
            internal bool CacheAdded;
            internal Icon TakeIcon() { var icon = Icon; Icon = null; return icon; }
            public void Dispose() { Icon?.Dispose(); FavouriteImage?.Dispose(); }
        }

        // The fetch delegate runs on the caller's UI context. ONLY detached bytes
        // cross into Task.Run; neither the WebView nor its COM stream travels there.
        internal static async Task<Result> ProcessAsync(Func<Task<byte[]>> fetch, string address,
            long cacheEpoch, CancellationToken cancellation)
        {
            await WorkSlots.WaitAsync(cancellation);
            try
            {
                byte[] png = await fetch();
                cancellation.ThrowIfCancellationRequested();
                if (png == null || png.Length == 0) return new Result();
                return await Task.Run(() => ConvertAndCache(png, address, cacheEpoch, cancellation), cancellation);
            }
            finally { WorkSlots.Release(); }
        }

        private static Result ConvertAndCache(byte[] png, string address, long cacheEpoch, CancellationToken cancellation)
        {
            byte[] bytes;
            using (var image = new MagickImage(png))
            using (var stream = new MemoryStream())
            {
                image.Format = MagickFormat.Icon;
                image.Write(stream);
                bytes = stream.ToArray();
            }
            cancellation.ThrowIfCancellationRequested();
            var result = new Result();
            try
            {
                // Clone detaches the Icon from its stream; the UI takes ownership
                // only after checking the tab, source and navigation generation.
                using (var stream = new MemoryStream(bytes, false))
                using (var icon = new Icon(stream)) result.Icon = (Icon)icon.Clone();
                try { result.CacheAdded = FaviconService.StoreIcon(address, bytes, cacheEpoch); }
                catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is Newtonsoft.Json.JsonException)
                {
                    // A read-only/full cache must not prevent showing the icon.
                    Debug.WriteLine("Favicon cache write failed: " + error.GetType().Name);
                }
                if (result.CacheAdded) result.FavouriteImage = result.Icon.ToBitmap();
                return result;
            }
            catch { result.Dispose(); throw; }
        }
    }
}
