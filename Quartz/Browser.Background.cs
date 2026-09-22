using Quartz.Models;
using Quartz.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quartz
{
    public partial class Browser
    {
        private int _favouritesReadVersion, _historyMenuVersion, _favouritesMenuVersion;
        private bool _favouritesReadPending;

        private sealed class BrowserItems : IDisposable
        {
            internal List<FavouriteModel> Favourites;
            internal List<HistoryModel> History;
            internal readonly Dictionary<string, Bitmap> Icons = new Dictionary<string, Bitmap>();
            public void Dispose() { foreach (var image in Icons.Values) image.Dispose(); }
        }

        private byte[] DefaultIconBytes()
        {
            // This helper also owns a hidden WebView, so call it only on the UI.
            using (var stream = new MemoryStream())
            {
                Libs.FaviconHelper.GetDefaultFavicon16().Save(stream);
                return stream.ToArray();
            }
        }

        private static void ReadIcons(BrowserItems data, IEnumerable<string> addresses, byte[] fallback)
        {
            if (fallback == null) return;
            lock (FaviconService.CacheSync)
            {
                var index = new FaviconService().All().GroupBy(item => item.WebAddress)
                    .ToDictionary(group => group.Key, group => group.First().Id);
                string cache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"Xaftellis\Quartz\UserData\cache");
                foreach (string address in addresses.Where(value => !string.IsNullOrEmpty(value)).Distinct())
                {
                    byte[] bytes = fallback;
                    Guid id;
                    if (index.TryGetValue(address, out id))
                    {
                        string path = Path.Combine(cache, id + ".ico");
                        if (File.Exists(path)) bytes = File.ReadAllBytes(path);
                    }
                    try
                    {
                        using (var stream = new MemoryStream(bytes, false))
                        using (var icon = new Icon(stream)) data.Icons[address] = icon.ToBitmap();
                    }
                    catch (ArgumentException)
                    {
                        using (var stream = new MemoryStream(fallback, false))
                        using (var icon = new Icon(stream)) data.Icons[address] = icon.ToBitmap();
                    }
                }
            }
        }

        private static BrowserItems ReadFavouriteItems(Guid profile, byte[] fallback)
        {
            var data = new BrowserItems();
            try
            {
                data.Favourites = new FavouriteService().All(profile).OrderBy(item => item.Index).ToList();
                ReadIcons(data, data.Favourites.Select(item => item.WebAddress), fallback);
                return data;
            }
            catch { data.Dispose(); throw; }
        }

        private async Task RefreshFavouriteControlsAsync()
        {
            ++_favouritesReadVersion;
            if (_favouritesReadPending) return;
            _favouritesReadPending = true;
            try
            {
                while (!IsDisposed && !Disposing)
                {
                    int version = _favouritesReadVersion;
                    Guid profile = ProfileService.Current;
                    bool icons = SettingsService.Get("showFavouriteIcon") == "true";
                    string theme = SettingsService.Get("Theme");
                    byte[] fallback = icons ? DefaultIconBytes() : null;
                    using (var data = await BrowserWork.Read(() => ReadFavouriteItems(profile, fallback)))
                    {
                        if (IsDisposed || Disposing) return;
                        if (version != _favouritesReadVersion || profile != ProfileService.Current) continue;
                        if (pnlFavourites.IsInteracting) { _reloadFavourites = true; return; }
                        LoadFavouriteControls(data.Favourites, data.Icons, icons, theme);
                        return;
                    }
                }
            }
            catch (Exception error) { ReportBackgroundError("load favourites", error); }
            finally { _favouritesReadPending = false; }
        }

        private void ReportBackgroundError(string operation, Exception error)
        {
            Debug.WriteLine("Could not " + operation + ": " + error);
            if (!IsDisposed && !Disposing)
                MessageBox.Show(this, "Couldn't " + operation + ". " + error.Message,
                    "Quartz", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void ClearMenuItems(ContextMenuStrip menu)
        {
            foreach (ToolStripItem item in menu.Items.Cast<ToolStripItem>().ToArray())
            {
                Image image = item.Image;
                item.Image = null;
                menu.Items.Remove(item);
                item.Dispose();
                image?.Dispose();
            }
        }

        private async Task PopulateHistoryMenuAsync(int version, ToolStripItem loading)
        {
            Guid profile = ProfileService.Current;
            byte[] fallback = DefaultIconBytes();
            try
            {
                using (var data = await BrowserWork.Read(() =>
                {
                    var result = new BrowserItems();
                    try
                    {
                        result.History = new HistoryService().All(profile).OrderByDescending(item => item.When).Take(11).ToList();
                        ReadIcons(result, result.History.Select(item => item.WebAddress), fallback);
                        return result;
                    }
                    catch { result.Dispose(); throw; }
                }))
                {
                    if (IsDisposed || Disposing || version != _historyMenuVersion ||
                        profile != ProfileService.Current || loading.IsDisposed || !mnuHistory.Visible) return;
                    int position = mnuHistory.Items.IndexOf(loading);
                    if (position < 0) return;
                    mnuHistory.SuspendLayout();
                    try
                    {
                        mnuHistory.Items.Remove(loading);
                        loading.Dispose();
                        foreach (var entry in data.History)
                        {
                            Bitmap icon;
                            var item = new ToolStripMenuItem
                            {
                                Text = entry.Title != null && entry.Title.Length > 64 ? entry.Title.Substring(0, 64) + "..." : entry.Title,
                                Tag = entry.WebAddress, ToolTipText = entry.WebAddress,
                                Image = data.Icons.TryGetValue(entry.WebAddress, out icon) ? (Image)icon.Clone() : null
                            };
                            item.Click += MenuItem_Click;
                            item.MouseUp += MenuItem_MouseUp;
                            mnuHistory.Items.Insert(position++, item);
                        }
                        if (data.History.Count == 0)
                            mnuHistory.Items.Insert(position, new ToolStripMenuItem("No recent pages") { Enabled = false });
                    }
                    finally { mnuHistory.ResumeLayout(true); }
                }
            }
            catch (Exception error)
            {
                Debug.WriteLine(error);
                if (!loading.IsDisposed) loading.Text = "Couldn't load history";
            }
        }

        private async Task PopulateFavouriteMenuAsync(int version, ToolStripItem loading)
        {
            Guid profile = ProfileService.Current;
            byte[] fallback = SettingsService.Get("showFavouriteIcon") == "true" ? DefaultIconBytes() : null;
            try
            {
                using (var data = await BrowserWork.Read(() => ReadFavouriteItems(profile, fallback)))
                {
                    if (IsDisposed || Disposing || version != _favouritesMenuVersion ||
                        profile != ProfileService.Current || loading.IsDisposed || !mnuFavourites.Visible) return;
                    mnuFavourites.SuspendLayout();
                    try
                    {
                        mnuFavourites.Items.Remove(loading);
                        loading.Dispose();
                        if (data.Favourites.Count > 0) mnuFavourites.Items.Add(new ToolStripSeparator());
                        foreach (var favourite in data.Favourites)
                        {
                            Bitmap icon;
                            var item = new ToolStripMenuItem
                            {
                                Name = "smi" + favourite.Id.ToString("N"), Text = favourite.Name,
                                Tag = favourite.WebAddress,
                                ToolTipText = favourite.Name + Environment.NewLine + favourite.WebAddress,
                                Image = data.Icons.TryGetValue(favourite.WebAddress, out icon) ? (Image)icon.Clone() : null
                            };
                            item.Click += MenuItem_Click;
                            item.MouseUp += MenuItem_MouseUp;
                            mnuFavourites.Items.Add(item);
                        }
                    }
                    finally { mnuFavourites.ResumeLayout(true); }
                }
            }
            catch (Exception error)
            {
                Debug.WriteLine(error);
                if (!loading.IsDisposed) loading.Text = "Couldn't load favourites";
            }
        }
    }
}
