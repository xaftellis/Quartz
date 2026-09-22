using Microsoft.Web.WebView2.Core;
using Quartz.Controls;
using Quartz.Libs;
using Quartz.Models;
using Quartz.Services;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Quartz
{
    public partial class Browser
    {
        private int _faviconRequestVersion;
        private bool _faviconRefreshPending, _faviconRefreshAgain;
        private CancellationTokenSource _faviconCancellation;
        private Icon _ownedFavicon;

        public async void CoreWebView2_FaviconChanged(object sender, object e)
        {
            if (IsDisposed || Disposing || wvWebView1.IsDisposed || wvWebView1.CoreWebView2 == null) return;
            ++_faviconRequestVersion;
            _faviconRefreshAgain = true;
            if (_faviconRefreshPending) return;
            if (_faviconCancellation == null)
            {
                _faviconCancellation = new CancellationTokenSource();
                Disposed += (s, args) =>
                {
                    _faviconCancellation.Cancel();
                    _faviconCancellation.Dispose();
                    _ownedFavicon?.Dispose(); _ownedFavicon = null;
                };
            }
            CancellationToken cancellation = _faviconCancellation.Token;
            _faviconRefreshPending = true;
            try
            {
                do
                {
                    _faviconRefreshAgain = false;
                    int version = _faviconRequestVersion;
                    ulong navigation = _activeNavigationId;
                    Uri source = wvWebView1.Source;
                    if (source == null || !source.IsAbsoluteUri) continue;
                    if (isQuartzDotCom(source))
                    {
                        ShowIcon = false;
                        IsDefaultFavicon = true;
                        FaviconHelper.UpdateCurrentTab(ParentTabs, this);
                        continue;
                    }
                    long epoch = FaviconService.CacheEpoch;
                    try
                    {
                        using (var result = await FaviconProcessor.ProcessAsync(
                            () => ReadFaviconBytesAsync(version, navigation, source), source.AbsoluteUri, epoch, cancellation))
                        {
                            if (!IsCurrentFaviconRequest(version, navigation, source)) continue;
                            Icon previous = _ownedFavicon;
                            _ownedFavicon = result.TakeIcon();
                            IsDefaultFavicon = _ownedFavicon == null;
                            Icon = _ownedFavicon ?? FaviconHelper.GetDefaultFavicon16();
                            ShowIcon = true;
                            FaviconHelper.UpdateCurrentTab(ParentTabs, this);
                            previous?.Dispose();
                            if (result.CacheAdded && epoch == FaviconService.CacheEpoch)
                                RefreshFavouriteIcon(source.AbsoluteUri, result.FavouriteImage);
                        }
                    }
                    catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { return; }
                    catch (Exception error)
                    {
                        // A bad image/cache or a disappearing WebView is a local
                        // favicon failure, not an unhandled async-void exception.
                        Debug.WriteLine("Favicon update failed: " + error.GetType().Name);
                    }
                } while (_faviconRefreshAgain && !cancellation.IsCancellationRequested && !IsDisposed && !Disposing);
            }
            finally { _faviconRefreshPending = false; }
        }

        private bool IsCurrentFaviconRequest(int version, ulong navigation, Uri source) =>
            !IsDisposed && !Disposing && !wvWebView1.IsDisposed && version == _faviconRequestVersion &&
            navigation == _activeNavigationId && source == wvWebView1.Source;

        private async Task<byte[]> ReadFaviconBytesAsync(int version, ulong navigation, Uri source)
        {
            if (!IsCurrentFaviconRequest(version, navigation, source)) return null;
            var core = wvWebView1.CoreWebView2;
            if (core == null || string.IsNullOrEmpty(core.FaviconUri)) return null;
            using (Stream stream = await core.GetFaviconAsync(CoreWebView2FaviconImageFormat.Png))
            using (var memory = new MemoryStream())
            {
                if (stream != null) await stream.CopyToAsync(memory);
                return IsCurrentFaviconRequest(version, navigation, source) ? memory.ToArray() : null;
            }
        }

        private void RefreshFavouriteIcon(string address, Bitmap image)
        {
            if (image == null || IsDisposed || Disposing) return;
            var affected = pnlFavourites.Controls.OfType<FavouriteButton>().Where(button =>
                button.Image != null && (button.Tag as FavouriteModel)?.WebAddress == address).ToList();
            if (affected.Count == 0) return;
            if (pnlFavourites.IsInteracting) { _reloadFavourites = true; return; }
            foreach (var button in affected)
                button.SetIconVisibility(true, () => (Image)image.Clone());
        }
    }
}
