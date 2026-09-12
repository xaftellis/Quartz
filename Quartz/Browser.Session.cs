using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz.Models;
using Quartz.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Quartz
{
    public partial class Browser
    {
        internal Guid SessionProfileId { get; } = ProfileService.Current;
        private string _sessionUrl;
        private string _sessionTitle;
        private double _sessionZoom = 1;
        private bool _sessionMuted;
        private double _sessionScrollX;
        private double _sessionScrollY;
        private bool _sessionPreferencesApplied;
        private bool _applyingSessionPreferences;
        private bool _restoringSessionScroll;
        private ulong? _sessionRestoreNavigation;
        private SessionTabModel _restoredTab;
        private readonly string _scrollMessageKey = Guid.NewGuid().ToString("N");

        internal void PrepareSessionTab(SessionTabModel tab)
        {
            _restoredTab = tab;
            _sessionUrl = _tabAddress;
            _sessionTitle = tab.Title;
            _sessionZoom = tab.ZoomFactor;
            _sessionMuted = tab.IsMuted;
            _sessionScrollX = tab.ScrollX;
            _sessionScrollY = tab.ScrollY;
            _restoringSessionScroll = tab.ScrollX != 0 || tab.ScrollY != 0;
            if (!string.IsNullOrEmpty(tab.Title)) Text = tab.Title;
        }

        internal SessionTabModel CaptureSessionTab(bool pinned)
        {
            if (_sessionPreferencesApplied && !wvWebView1.IsDisposed)
            {
                try
                {
                    _sessionZoom = wvWebView1.ZoomFactor;
                    if (wvWebView1.CoreWebView2 != null)
                        _sessionMuted = wvWebView1.CoreWebView2.IsMuted;
                }
                catch (Exception e) { Debug.WriteLine("Could not read tab preferences: " + e); }
            }
            return new SessionTabModel
            {
                Url = _sessionUrl ?? (_newtab ? _tabAddress : null),
                Title = _sessionTitle ?? Text,
                IsPinned = pinned,
                IsMuted = _sessionMuted,
                ZoomFactor = _sessionZoom,
                ScrollX = _sessionScrollX,
                ScrollY = _sessionScrollY
            };
        }

        private async Task InitializeSessionWebViewAsync()
        {
            wvWebView1.CoreWebView2.IsMutedChanged += (sender, e) => Program.Session?.RequestCheckpoint(this);
            wvWebView1.CoreWebView2.WebMessageReceived += SessionScrollMessageReceived;
            try
            {
                await wvWebView1.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync($@"
(() => {{
    if (window !== window.top) return;
    let pending = false;
    const send = () => {{
        pending = false;
        window.chrome.webview.postMessage({{
            key: '{_scrollMessageKey}', url: location.href, x: scrollX, y: scrollY
        }});
    }};
    addEventListener('scroll', () => {{
        if (!pending) {{ pending = true; setTimeout(send, 150); }}
    }}, {{ passive: true }});
    addEventListener('pagehide', send);
}})();");
            }
            catch (Exception e) { Debug.WriteLine("Could not initialize session scroll capture: " + e); }
        }

        private void ApplySessionPreferences()
        {
            if (_restoredTab != null)
            {
                _applyingSessionPreferences = true;
                try
                {
                    wvWebView1.ZoomFactor = _sessionZoom;
                    wvWebView1.CoreWebView2.IsMuted = _sessionMuted;
                }
                finally { _applyingSessionPreferences = false; }
            }
            _sessionPreferencesApplied = true;
        }

        private void SessionNavigationStarting(CoreWebView2NavigationStartingEventArgs e)
        {
            if (e.Cancel || !SessionStore.IsRestorableUrl(e.Uri) || IsSessionErrorPage(e.Uri)) return;
            if (_restoringSessionScroll)
            {
                if (!_sessionRestoreNavigation.HasValue) _sessionRestoreNavigation = e.NavigationId;
                else if (_sessionRestoreNavigation.Value != e.NavigationId) _restoringSessionScroll = false;
            }
            _sessionUrl = e.Uri;
            if (!_restoringSessionScroll)
            {
                _sessionScrollX = 0;
                _sessionScrollY = 0;
            }
            Program.Session?.RequestCheckpoint(this);
        }

        private void SessionSourceChanged()
        {
            string url = wvWebView1.Source?.AbsoluteUri;
            if (string.IsNullOrEmpty(url) || IsSessionErrorPage(url)) return;
            if (url == "about:blank" && !string.IsNullOrEmpty(_tabAddress) && _tabAddress != "about:blank") return;
            _sessionUrl = url;
            Program.Session?.RequestCheckpoint(this);
        }

        private void RememberSessionTitle()
        {
            if (!IsSessionErrorPage(wvWebView1.Source?.AbsoluteUri))
                _sessionTitle = wvWebView1.CoreWebView2.DocumentTitle;
            Program.Session?.RequestCheckpoint(this);
        }

        private static bool IsSessionErrorPage(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Host == "quartz.com" &&
                (uri.AbsolutePath.EndsWith("/error.html", StringComparison.OrdinalIgnoreCase) ||
                 uri.AbsolutePath.EndsWith("/Safety.html", StringComparison.OrdinalIgnoreCase));
        }

        internal static string ResolveSessionUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Host != "quartz.com") return url;
            string[] path = uri.AbsolutePath.Split('/');
            if (path.Length < 3 || !new[] { "light", "dark", "black", "aqua", "xmas" }.Contains(path[1]))
                return url;
            path[1] = SettingsService.Get("Theme") ?? "light";
            return new UriBuilder(uri) { Path = string.Join("/", path) }.Uri.AbsoluteUri;
        }

        private void SessionScrollMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            if (_restoringSessionScroll) return;
            try
            {
                var message = JObject.Parse(e.WebMessageAsJson);
                if ((string)message["key"] != _scrollMessageKey ||
                    (string)message["url"] != _sessionUrl || e.Source != _sessionUrl) return;
                double x = (double?)message["x"] ?? 0;
                double y = (double?)message["y"] ?? 0;
                if (!SessionStore.IsFinite(x) || !SessionStore.IsFinite(y)) return;
                _sessionScrollX = Math.Max(0, x);
                _sessionScrollY = Math.Max(0, y);
                Program.Session?.RequestCheckpoint(this);
            }
            catch (Exception error) when (error is JsonException || error is FormatException || error is InvalidCastException)
            {
                Debug.WriteLine("Could not read session scroll position: " + error);
            }
        }

        private async void RestoreSessionScroll(CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!_restoringSessionScroll || _sessionRestoreNavigation != e.NavigationId) return;
            string x = JsonConvert.SerializeObject(_sessionScrollX);
            string y = JsonConvert.SerializeObject(_sessionScrollY);
            _restoringSessionScroll = false;
            try { await wvWebView1.CoreWebView2.ExecuteScriptAsync($"window.scrollTo({x}, {y});"); }
            catch (Exception error) { Debug.WriteLine("Could not restore session scroll position: " + error); }
        }
    }
}
