using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quartz.Libs
{
    // Owns only the address-bar site controls; tab icons/loading stay in Browser/EasyTabs.
    internal sealed class SiteInfoController : IDisposable
    {
        private readonly WebView2 _webView;
        private readonly SiteInfoButton _siteInfoButton;
        private readonly ToolTip _tooltip = new ToolTip();
        private CoreWebView2 _coreWebView;
        private CoreWebView2DevToolsProtocolEventReceiver _securityReceiver;
        private CoreWebView2DevToolsProtocolEventReceiver _frameReceiver;
        private SiteInfoPopup _siteInfoPopup;
        private Uri _loadedPageUri;
        private JObject _securityDetails;
        private ulong _navigationId;
        private int _navigationVersion;
        private bool _isNavigating;
        private bool _isDisposed;
        private int _lastButtonDismissal;
        private Task _securityRefresh = Task.CompletedTask;
        private readonly HashSet<CoreWebView2PermissionKind> _requestedPermissions = new HashSet<CoreWebView2PermissionKind>();
        public SiteInfoController(WebView2 view, SiteInfoButton button)
        {
            _webView = view;
            _siteInfoButton = button;
            _tooltip.OwnerDraw = true;
            _tooltip.Draw += Tooltip_Draw;
            button.Click += Button_Click;
            view.VisibleChanged += View_VisibleChanged;
        }

        public async Task InitializeAsync()
        {
            if (_isDisposed || _coreWebView != null || _webView.CoreWebView2 == null)
            {
                return;
            }

            _coreWebView = _webView.CoreWebView2;
            _coreWebView.NavigationStarting += NavigationStarting;
            _coreWebView.NavigationCompleted += NavigationCompleted;
            _coreWebView.SourceChanged += SourceChanged;
            _coreWebView.PermissionRequested += PermissionRequested;
            _coreWebView.ProcessFailed += ProcessFailed;
            _coreWebView.ServerCertificateErrorDetected += CertificateError;
            try
            {
                _frameReceiver = _coreWebView.GetDevToolsProtocolEventReceiver("Page.frameNavigated");
                _frameReceiver.DevToolsProtocolEventReceived += FrameNavigated;
                _securityReceiver = _coreWebView.GetDevToolsProtocolEventReceiver("Security.visibleSecurityStateChanged");
                _securityReceiver.DevToolsProtocolEventReceived += SecurityChanged;
                await _coreWebView.CallDevToolsProtocolMethodAsync("Page.enable", "{}");
                if (_isDisposed)
                {
                    return;
                }

                // Ask WebView2 which page is actually loaded, rather than trusting the address text.
                string frameJson = await _coreWebView.CallDevToolsProtocolMethodAsync("Page.getFrameTree", "{}");
                JObject frameDetails = JObject.Parse(frameJson);
                JToken frame = frameDetails["frameTree"]?["frame"];
                if (_isDisposed)
                {
                    return;
                }

                _loadedPageUri = ParseUri((string)frame?["url"]);
                await RefreshSecurityAsync();
            }
            catch (Exception ex)
            {
                Report("initialize", ex);
            }

            UpdateButton();
        }

        public void ApplyTheme(Color addressBackground, Color foreground)
        {
            _siteInfoButton.BackColor = addressBackground;
            _siteInfoButton.ForeColor = foreground;
            _siteInfoButton.Invalidate();
            Form browser = _siteInfoButton.FindForm();
            _tooltip.BackColor = browser?.BackColor ?? SystemColors.Window;
            _tooltip.ForeColor = browser?.ForeColor ?? SystemColors.WindowText;
            ClosePopup();
        }

        private void Tooltip_Draw(object sender, DrawToolTipEventArgs e)
        {
            e.DrawBackground();
            using (var pen = new Pen(_tooltip.ForeColor))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, e.Bounds.Width - 1, e.Bounds.Height - 1);
            }

            e.DrawText();
        }

        private void NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (e.Cancel)
            {
                return;
            }

            _navigationId = e.NavigationId;
            _navigationVersion++;
            _isNavigating = true;
            _securityDetails = null;
            _requestedPermissions.Clear();
            ClosePopup();
            UpdateButton();
        }

        private async void NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (_isDisposed || e.NavigationId != _navigationId)
            {
                return;
            }

            _isNavigating = false;
            if (!e.IsSuccess)
            {
                _securityDetails = null;
            }

            await RefreshSecurityAsync();
            UpdateButton();
        }

        private void SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            // History API / fragment navigation keeps the same document and certificate.
            var uri = CurrentUri();
            if (!e.IsNewDocument && Origin(uri) != null && Origin(uri) == Origin(_loadedPageUri))
            {
                _loadedPageUri = uri;
            }

            ClosePopup();
            UpdateButton();
        }

        private void FrameNavigated(object sender, CoreWebView2DevToolsProtocolEventReceivedEventArgs e)
        {
            if (_isDisposed)
            {
                return;
            }

            try
            {
                var frame = JObject.Parse(e.ParameterObjectAsJson)["frame"];
                if (frame == null || frame["parentId"] != null)
                {
                    return;
                }

                _navigationVersion++;
                _loadedPageUri = ParseUri((string)frame["url"]);
                _securityDetails = null;
                _isNavigating = false;
                ClosePopup();
                UpdateButton();
            }
            catch (Exception ex)
            {
                Report("frame", ex);
            }
        }

        private void SecurityChanged(object sender, CoreWebView2DevToolsProtocolEventReceivedEventArgs e)
        {
            if (_isDisposed || _isNavigating)
            {
                return;
            }

            try
            {
                _securityDetails = JObject.Parse(e.ParameterObjectAsJson)["visibleSecurityState"] as JObject;
                UpdateButton();
                _siteInfoPopup?.UpdateSecurity(GetSnapshot());
            }
            catch (Exception ex)
            {
                Report("security", ex);
            }
        }

        private async Task RefreshSecurityAsync()
        {
            // Wait for the previous refresh before starting another one.
            // Otherwise an older page could turn off the newer page's security updates.
            var previous = _securityRefresh;
            _securityRefresh = RefreshSecurityCoreAsync(previous, _navigationVersion);
            await _securityRefresh;
        }

        private async Task RefreshSecurityCoreAsync(Task previous, int generation)
        {
            await previous;
            if (_isDisposed || generation != _navigationVersion)
            {
                return;
            }

            try
            {
                await _coreWebView.CallDevToolsProtocolMethodAsync("Security.disable", "{}");
                if (_isDisposed)
                {
                    return;
                }

                await _coreWebView.CallDevToolsProtocolMethodAsync("Security.enable", "{}");
            }
            catch (Exception ex)
            {
                Report("security refresh", ex);
            }
        }

        private void CertificateError(object sender, CoreWebView2ServerCertificateErrorDetectedEventArgs e)
        {
            // WebView2 retains its normal certificate error handling.
            _securityDetails = new JObject
            {
                ["securityState"] = "insecure-broken"
            };
            UpdateButton();
        }

        private void ProcessFailed(object sender, CoreWebView2ProcessFailedEventArgs e)
        {
            if (e.ProcessFailedKind != CoreWebView2ProcessFailedKind.BrowserProcessExited && e.ProcessFailedKind != CoreWebView2ProcessFailedKind.RenderProcessExited)
            {
                return;
            }

            _securityDetails = null;
            ClosePopup();
            UpdateButton();
        }

        private void PermissionRequested(object sender, CoreWebView2PermissionRequestedEventArgs e)
        {
            if (Origin(ParseUri(e.Uri)) == Origin(CurrentUri()))
            {
                _requestedPermissions.Add(e.PermissionKind);
            }
        }

        private void View_VisibleChanged(object sender, EventArgs e)
        {
            if (!_webView.Visible)
            {
                ClosePopup();
            }
        }

        public SiteInfoSnapshot GetSnapshot()
        {
            Uri uri = CurrentUri();
            var snapshot = new SiteInfoSnapshot
            {
                Uri = uri,
                RequestedPermissions = _requestedPermissions.ToArray()
            };
            // Don't show a previous page's padlock while a different page is loading.
            bool matches = !_isNavigating && SameDocument(uri, _loadedPageUri);
            if (matches && _securityDetails != null)
            {
                snapshot.Security = (JObject)_securityDetails.DeepClone();
            }

            return snapshot;
        }

        private void UpdateButton()
        {
            if (_isDisposed)
            {
                return;
            }

            var state = GetSnapshot();
            _siteInfoButton.IconKind = state.Icon;
            _siteInfoButton.AccessibleName = "Site information: " + state.Summary;
            _tooltip.SetToolTip(_siteInfoButton, state.Summary + " — Site information");
            _siteInfoButton.Invalidate();
        }

        private void Button_Click(object sender, EventArgs e)
        {
            if (_isDisposed)
            {
                return;
            }

            // Clicking the anchor deactivates the popup before this Click event arrives.
            if (_lastButtonDismissal != 0 && unchecked(Environment.TickCount - _lastButtonDismissal) < 250)
            {
                _lastButtonDismissal = 0;
                return;
            }

            if (_siteInfoPopup != null && !_siteInfoPopup.IsDisposed)
            {
                ClosePopup();
                return;
            }

            Form browser = _siteInfoButton.FindForm();
            Color popupBackground = browser?.BackColor ?? SystemColors.Window;
            Color popupForeground = browser?.ForeColor ?? SystemColors.WindowText;
            _siteInfoPopup = new SiteInfoPopup(this, GetSnapshot(), popupBackground, popupForeground, _siteInfoButton.DeviceDpi / 96f);
            _siteInfoPopup.VisibleChanged += (s, args) =>
            {
                if (ReferenceEquals(_siteInfoPopup, s) && !_siteInfoButton.IsDisposed)
                {
                    _siteInfoButton.IsPopupOpen = _siteInfoPopup.Visible;
                }
            };
            _siteInfoPopup.FormClosed += (s, args) =>
            {
                if (_siteInfoButton.ClientRectangle.Contains(_siteInfoButton.PointToClient(Cursor.Position)))
                {
                    _lastButtonDismissal = Environment.TickCount;
                }

                if (ReferenceEquals(_siteInfoPopup, s))
                {
                    _siteInfoPopup = null;
                    if (!_siteInfoButton.IsDisposed)
                    {
                        _siteInfoButton.IsPopupOpen = false;
                    }
                }
            };
            Point location = _siteInfoButton.PointToScreen(new Point(-10, _siteInfoButton.Height + 8));
            _siteInfoPopup.ShowAnchored(_siteInfoButton.FindForm()?.TopLevelControl as Form, location);
        }

        public bool IsCurrent(SiteInfoSnapshot snapshot)
        {
            if (_isDisposed || _coreWebView == null)
            {
                return false;
            }

            return SameDocument(snapshot.Uri, CurrentUri());
        }

        public async Task<Dictionary<CoreWebView2PermissionKind, CoreWebView2PermissionState>> GetPermissionsAsync(string origin)
        {
            var settings = await _coreWebView.Profile.GetNonDefaultPermissionSettingsAsync();
            var permissions = new Dictionary<CoreWebView2PermissionKind, CoreWebView2PermissionState>();

            // A profile contains many sites. Only return this site's saved permissions.
            foreach (var setting in settings)
            {
                Uri permissionUri = ParseUri(setting.PermissionOrigin);
                if (Origin(permissionUri) == origin)
                {
                    permissions[setting.PermissionKind] = setting.PermissionState;
                }
            }

            return permissions;
        }

        public Task SetPermissionAsync(string origin, CoreWebView2PermissionKind kind, CoreWebView2PermissionState state)
        {
            return _coreWebView.Profile.SetPermissionStateAsync(kind, origin, state);
        }

        public async Task ResetPermissionsAsync(string origin)
        {
            var settings = await GetPermissionsAsync(origin);
            foreach (var kind in settings.Keys)
            {
                await SetPermissionAsync(origin, kind, CoreWebView2PermissionState.Default);
            }
        }

        public async Task<List<CoreWebView2Cookie>> GetCookiesAsync(Uri uri)
        {
            var cookies = await _coreWebView.CookieManager.GetCookiesAsync(null);
            return cookies.Where(c => CookieMatchesHost(uri.IdnHost, c.Domain)).OrderBy(c => c.Domain).ThenBy(c => c.Name).ToList();
        }

        internal static bool CookieMatchesHost(string host, string domain)
        {
            // Host-only cookies do not belong to subdomains; domain cookies have a leading dot.
            string cookieDomain = domain.TrimStart('.');
            if (string.Equals(host, cookieDomain, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // ".example.com" can belong to "shop.example.com", but not "notexample.com".
            if (domain.StartsWith(".", StringComparison.Ordinal))
            {
                return host.EndsWith("." + cookieDomain, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public async Task<long> GetStorageUsageAsync(string origin)
        {
            string parameters = JsonConvert.SerializeObject(new { origin });
            string response = await _coreWebView.CallDevToolsProtocolMethodAsync("Storage.getUsageAndQuota", parameters);
            JObject result = JObject.Parse(response);
            return (long?)result["usage"] ?? 0;
        }

        public Task DeleteCookieAsync(CoreWebView2Cookie cookie)
        {
            string parameters = JsonConvert.SerializeObject(new
            {
                name = cookie.Name,
                domain = cookie.Domain,
                path = cookie.Path
            });

            // The popup waits for this task before saying the cookie was deleted.
            return _coreWebView.CallDevToolsProtocolMethodAsync("Network.deleteCookies", parameters);
        }

        public async Task ClearSiteDataAsync(Uri uri)
        {
            // Never use Profile.ClearBrowsingData here: that would clear every website.
            await _coreWebView.CallDevToolsProtocolMethodAsync("Storage.clearDataForOrigin", JsonConvert.SerializeObject(new { origin = Origin(uri), storageTypes = "local_storage,indexeddb,websql,service_workers,cache_storage,file_systems" }));
            var cookies = await GetCookiesAsync(uri);
            foreach (var cookie in cookies)
            {
                await DeleteCookieAsync(cookie);
            }
        }

        public void Reload(SiteInfoSnapshot snapshot)
        {
            if (IsCurrent(snapshot))
            {
                ClosePopup();
                _coreWebView.Reload();
            }
        }

        private void ClosePopup()
        {
            if (_siteInfoPopup != null && !_siteInfoPopup.IsDisposed)
            {
                _siteInfoPopup.Close();
            }
        }

        private Uri CurrentUri()
        {
            if (_isDisposed || _coreWebView == null)
            {
                return null;
            }

            try
            {
                return ParseUri(_coreWebView.Source);
            }
            catch (Exception ex)
            {
                Report("current page", ex);
                return null;
            }
        }

        internal static Uri ParseUri(string value)
        {
            Uri uri;
            if (Uri.TryCreate(value, UriKind.Absolute, out uri))
            {
                return uri;
            }

            return null;
        }

        internal static string Origin(Uri uri)
        {
            // An origin is the scheme, host and port, without the page path.
            // For example, https://example.com/news belongs to https://example.com.
            if (uri == null || (uri.Scheme != "https" && uri.Scheme != "http"))
            {
                return null;
            }

            int port = -1;
            if (!uri.IsDefaultPort)
            {
                port = uri.Port;
            }

            var siteAddress = new UriBuilder(uri.Scheme, uri.IdnHost, port);
            return siteAddress.Uri.GetLeftPart(UriPartial.Authority);
        }

        private static bool SameDocument(Uri firstUri, Uri secondUri)
        {
            if (firstUri == null || secondUri == null)
            {
                return false;
            }

            // Ignore the #fragment, but compare the host, path and query string.
            UriComponents parts = UriComponents.SchemeAndServer | UriComponents.PathAndQuery;
            return Uri.Compare(firstUri, secondUri, parts, UriFormat.UriEscaped, StringComparison.Ordinal) == 0;
        }

        internal static void Report(string action, Exception ex)
        {
            Debug.WriteLine("Site information " + action + ": " + ex.GetType().Name);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            ClosePopup();
            _siteInfoButton.Click -= Button_Click;
            _webView.VisibleChanged -= View_VisibleChanged;
            _tooltip.Dispose();
            try
            {
                if (_coreWebView == null)
                {
                    return;
                }

                _coreWebView.NavigationStarting -= NavigationStarting;
                _coreWebView.NavigationCompleted -= NavigationCompleted;
                _coreWebView.SourceChanged -= SourceChanged;
                _coreWebView.PermissionRequested -= PermissionRequested;
                _coreWebView.ProcessFailed -= ProcessFailed;
                _coreWebView.ServerCertificateErrorDetected -= CertificateError;
                if (_securityReceiver != null)
                {
                    _securityReceiver.DevToolsProtocolEventReceived -= SecurityChanged;
                }

                if (_frameReceiver != null)
                {
                    _frameReceiver.DevToolsProtocolEventReceived -= FrameNavigated;
                }
            }
            catch (Exception ex)
            {
                Report("dispose", ex);
            }
        }
    }

    // A copy of the current page's details. The icon and popup share these display rules.
    internal sealed class SiteInfoSnapshot
    {
        public Uri Uri;
        public JObject Security;
        public CoreWebView2PermissionKind[] RequestedPermissions = new CoreWebView2PermissionKind[0];

        public string Origin
        {
            get
            {
                return SiteInfoController.Origin(Uri);
            }
        }

        public bool IsInternal
        {
            get
            {
                if (Uri == null)
                {
                    return false;
                }

                if (Uri.Scheme == "about")
                {
                    return true;
                }

                bool isWebAddress = Uri.Scheme == "http" || Uri.Scheme == "https";
                return isWebAddress && Uri.IdnHost.Equals("quartz.com", StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool IsWebsite
        {
            get
            {
                return Origin != null && !IsInternal;
            }
        }

        public JObject Certificate
        {
            get
            {
                if (Security == null)
                {
                    return null;
                }

                return Security["certificateSecurityState"] as JObject;
            }
        }

        public bool IsSecure
        {
            get
            {
                // An https:// address alone is not enough to show a padlock.
                if (!IsWebsite || Uri.Scheme != "https")
                {
                    return false;
                }

                if ((string)Security?["securityState"] != "secure" || Certificate == null)
                {
                    return false;
                }

                return string.IsNullOrEmpty((string)Certificate["certificateNetworkError"]);
            }
        }

        public string DisplayName
        {
            get
            {
                if (IsInternal)
                {
                    return "Quartz";
                }

                if (Uri == null)
                {
                    return "New tab";
                }

                if (Uri.IsFile)
                {
                    return "Local file";
                }

                if (string.IsNullOrEmpty(Uri.IdnHost))
                {
                    return Uri.Scheme + ": page";
                }

                string siteName = Uri.IdnHost;
                if (!Uri.IsDefaultPort)
                {
                    siteName += ":" + Uri.Port;
                }

                return siteName;
            }
        }

        public SiteInfoIcon Icon
        {
            get
            {
                if (IsInternal)
                {
                    return SiteInfoIcon.Internal;
                }

                if (Uri != null && Uri.IsFile)
                {
                    return SiteInfoIcon.File;
                }

                if (IsSecure)
                {
                    return SiteInfoIcon.Lock;
                }

                if (IsWebsite && (Uri.Scheme == "http" || (string)Security?["securityState"] == "insecure-broken"))
                {
                    return SiteInfoIcon.Warning;
                }

                return SiteInfoIcon.Information;
            }
        }

        public string Summary
        {
            get
            {
                if (IsInternal)
                {
                    return "You're viewing a Quartz page";
                }

                if (Uri != null && Uri.IsFile)
                {
                    return "You're viewing a local file";
                }

                if (!IsWebsite)
                {
                    return "Page information";
                }

                if (IsSecure)
                {
                    return "Connection is secure";
                }

                if (Uri.Scheme == "http")
                {
                    return "Connection is not secure";
                }

                if (Security == null)
                {
                    return "Connection information unavailable";
                }

                return "Connection is not fully secure";
            }
        }

        public string Description
        {
            get
            {
                if (IsInternal)
                {
                    return "This page is provided by Quartz. Website connection and permission settings do not apply.";
                }

                if (Uri != null && Uri.IsFile)
                {
                    return "This file is on your computer. It is not an encrypted connection to a website.";
                }

                if (!IsWebsite)
                {
                    return "Connection and website settings are available when you open an HTTP or HTTPS website.";
                }

                if (IsSecure)
                {
                    return "Your connection to this site is encrypted. This does not guarantee that the site's content is trustworthy.";
                }

                if (Uri.Scheme == "http")
                {
                    return "Information you send or receive on this site could be seen or changed by others.";
                }

                if (Security == null)
                {
                    return "Quartz has not received verified connection details for this page yet.";
                }

                return "This page has a connection, certificate, or insecure-content issue. Check the connection details before entering sensitive information.";
            }
        }
    }
}
