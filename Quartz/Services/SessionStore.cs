using Newtonsoft.Json;
using Quartz.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Quartz.Services
{
    internal sealed class SessionStore
    {
        private readonly string _directory;

        internal SessionStore(string directory = null)
        {
            _directory = directory ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Xaftellis\Quartz\UserData\jsons\sessions");
        }

        internal string PathFor(Guid profileId) => Path.Combine(_directory, profileId.ToString("D") + ".json");

        internal BrowserSessionModel Read(Guid profileId) =>
            ReadFile(PathFor(profileId), profileId) ?? ReadFile(PathFor(profileId) + ".bak", profileId);

        private BrowserSessionModel ReadFile(string path, Guid profileId)
        {
            try
            {
                if (!File.Exists(path) || new FileInfo(path).Length > 32 * 1024 * 1024)
                    return null;

                var session = JsonConvert.DeserializeObject<BrowserSessionModel>(File.ReadAllText(path));
                if (session == null || session.Version != 1 || session.ProfileId != profileId || session.Windows == null)
                    return null;

                session.Windows.RemoveAll(window => window == null || window.Tabs == null);
                foreach (var window in session.Windows)
                {
                    var selected = window.Tabs.ElementAtOrDefault(window.SelectedTabIndex);
                    window.Tabs.RemoveAll(tab => tab == null || !IsRestorableUrl(tab.Url));
                    window.SelectedTabIndex = Math.Max(0, window.Tabs.IndexOf(selected));
                    foreach (var tab in window.Tabs)
                    {
                        if (!IsFinite(tab.ZoomFactor) || tab.ZoomFactor < 0.25 || tab.ZoomFactor > 5)
                            tab.ZoomFactor = 1;
                        if (!IsFinite(tab.ScrollX) || tab.ScrollX < 0) tab.ScrollX = 0;
                        if (!IsFinite(tab.ScrollY) || tab.ScrollY < 0) tab.ScrollY = 0;
                    }
                }
                session.Windows.RemoveAll(window => window.Tabs.Count == 0);
                return session;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is JsonException)
            {
                Debug.WriteLine("Could not read browser session: " + e);
                return null;
            }
        }

        internal static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);

        internal static bool IsRestorableUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return true; // An unloaded home tab.
            if (string.Equals(url, "about:blank", StringComparison.OrdinalIgnoreCase)) return true;
            return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeFile);
        }

        internal bool Write(BrowserSessionModel session)
        {
            try
            {
                Directory.CreateDirectory(_directory);
                string path = PathFor(session.ProfileId);
                byte[] bytes = new UTF8Encoding(false).GetBytes(JsonConvert.SerializeObject(session, Formatting.Indented));
                using (var stream = new FileStream(path + ".tmp", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }
                if (File.Exists(path))
                    File.Replace(path + ".tmp", path, path + ".bak");
                else
                    File.Move(path + ".tmp", path);
                return true;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is JsonException)
            {
                Debug.WriteLine("Could not save browser session: " + e);
                return false;
            }
        }

        internal void Delete(Guid profileId)
        {
            foreach (string suffix in new[] { "", ".bak", ".tmp" })
            {
                try { File.Delete(PathFor(profileId) + suffix); }
                catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
                {
                    Debug.WriteLine("Could not remove browser session: " + e);
                }
            }
        }
    }
}
