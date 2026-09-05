using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json.Linq;
using Quartz.Libs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quartz.Controls
{
    internal sealed class SiteInfoPopup : Form
    {
        private readonly SiteInfoController _controller;
        private SiteInfoSnapshot _snapshot;
        private readonly FlowLayoutPanel _content;
        private readonly Color _textColor;
        private readonly Color _secondaryTextColor;
        private readonly Color _borderColor;
        private readonly Color _linkColor;
        private readonly Font _headingFont = new Font("Segoe UI", 12f, FontStyle.Bold);
        private readonly Font _sectionFont = new Font("Segoe UI", 9f, FontStyle.Bold);
        private readonly float _dpiScale;
        private Dictionary<CoreWebView2PermissionKind, CoreWebView2PermissionState> _permissions;
        private List<CoreWebView2Cookie> _cookies;
        private long? _usage;
        private string _page = "home";
        private string _notice;
        private bool _loading = true;
        private bool _busy;
        private bool _dialogOpen;
        private bool _reloadNeeded;
        private Label _securityTitle;
        private Label _securityDescription;
        private Form _anchorOwner;
        private Point _anchor;

        public SiteInfoPopup(SiteInfoController controller, SiteInfoSnapshot snapshot, bool dark, float scale = 1f)
        {
            _controller = controller;
            _snapshot = snapshot;
            _dpiScale = scale;
            AutoScaleMode = AutoScaleMode.None;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            KeyPreview = true;
            Text = "Site information — " + snapshot.DisplayName;
            Font = new Font("Segoe UI", 9f);
            BackColor = dark ? Color.FromArgb(41, 42, 45) : Color.White;
            _textColor = dark ? Color.FromArgb(232, 234, 237) : Color.FromArgb(32, 33, 36);
            _secondaryTextColor = dark ? Color.FromArgb(174, 178, 184) : Color.FromArgb(95, 99, 104);
            _borderColor = dark ? Color.FromArgb(72, 74, 78) : Color.FromArgb(228, 230, 233);
            _linkColor = dark ? Color.FromArgb(138, 180, 248) : Color.FromArgb(26, 115, 232);
            ForeColor = _textColor;
            Padding = new Padding(1);
            ClientSize = new Size(ScalePixels(360), ScalePixels(360));
            _content = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(ScalePixels(16)),
                BackColor = BackColor
            };
            Controls.Add(_content);
            Deactivate += SiteInfoPopup_Deactivate;
            Shown += SiteInfoPopup_Shown;
            BuildPage();
        }

        private void SiteInfoPopup_Deactivate(object sender, EventArgs e)
        {
            // Keep the popup open while its certificate viewer or confirmation box has focus.
            if (!_dialogOpen)
            {
                Close();
            }
        }

        private async void SiteInfoPopup_Shown(object sender, EventArgs e)
        {
            await LoadDetailsAsync();
        }

        private int ScalePixels(int value)
        {
            // Keep the same proportions when Windows display scaling is above 100%.
            return (int)Math.Round(value * _dpiScale);
        }

        private int ContentWidth
        {
            get
            {
                return ScalePixels(328);
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams windowStyle = base.CreateParams;
                windowStyle.ClassStyle |= 0x20000; // CS_DROPSHADOW: add the small shadow around the popup.
                return windowStyle;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(_borderColor))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            if (keyData == (Keys.Alt | Keys.Left) && _page != "home")
            {
                Navigate("home");
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void ShowAnchored(Form owner, Point anchor)
        {
            _anchor = anchor;
            _anchorOwner = owner;
            if (owner != null)
            {
                owner.LocationChanged += OwnerMoved;
                owner.Resize += OwnerMoved;
            }

            FitToScreen();
            if (owner != null)
            {
                Show(owner);
            }
            else
            {
                Show();
            }
        }

        private void OwnerMoved(object sender, EventArgs e)
        {
            Close();
        }

        public void UpdateSecurity(SiteInfoSnapshot snapshot)
        {
            if (IsDisposed || snapshot.Origin != _snapshot.Origin || snapshot.IsInternal != _snapshot.IsInternal)
            {
                return;
            }

            _snapshot = snapshot;
            if (_securityTitle != null && !_securityTitle.IsDisposed)
            {
                _securityTitle.Text = snapshot.Summary;
            }

            if (_securityDescription != null && !_securityDescription.IsDisposed)
            {
                _securityDescription.Text = snapshot.Description;
            }
        }

        private async Task LoadDetailsAsync()
        {
            if (!_snapshot.IsWebsite)
            {
                _loading = false;
                BuildPage();
                return;
            }

            try
            {
                _permissions = await _controller.GetPermissionsAsync(_snapshot.Origin);
            }
            catch (Exception ex)
            {
                SiteInfoController.Report("read permissions", ex);
            }

            if (IsDisposed)
            {
                return;
            }

            try
            {
                _cookies = await _controller.GetCookiesAsync(_snapshot.Uri);
            }
            catch (Exception ex)
            {
                SiteInfoController.Report("read cookies", ex);
            }

            if (IsDisposed)
            {
                return;
            }

            try
            {
                _usage = await _controller.GetStorageUsageAsync(_snapshot.Origin);
            }
            catch (Exception ex)
            {
                SiteInfoController.Report("read storage", ex);
            }

            if (IsDisposed)
            {
                return;
            }

            _loading = false;
            BuildPage();
        }

        private void Navigate(string page)
        {
            _page = page;
            BuildPage();
        }

        private void BuildPage()
        {
            if (IsDisposed)
            {
                return;
            }

            _content.SuspendLayout();
            foreach (Control control in _content.Controls.Cast<Control>().ToArray())
            {
                control.Dispose();
            }

            _content.Controls.Clear();
            _securityTitle = _securityDescription = null;
            var header = new Panel
            {
                Size = new Size(ContentWidth, ScalePixels(38)),
                Margin = new Padding(0, 0, 0, ScalePixels(6))
            };
            bool subpage = _page != "home";
            var title = new Label
            {
                Text = subpage ? PageTitle() : _snapshot.DisplayName,
                Font = _headingFont,
                ForeColor = _textColor,
                AutoEllipsis = true,
                Location = new Point(subpage ? ScalePixels(30) : 0, ScalePixels(5)),
                Size = new Size(ContentWidth - ScalePixels(subpage ? 62 : 32), ScalePixels(28))
            };
            header.Controls.Add(title);
            var close = SmallButton("×", "Close site information");
            close.Location = new Point(ContentWidth - ScalePixels(28), 0);
            close.Click += (s, e) => Close();
            header.Controls.Add(close);
            if (subpage)
            {
                var back = SmallButton("‹", "Back to site information");
                back.Click += (s, e) => Navigate("home");
                header.Controls.Add(back);
            }

            _content.Controls.Add(header);
            if (_snapshot.IsWebsite)
            {
                AddText(_snapshot.Origin, false, _secondaryTextColor);
            }

            if (!string.IsNullOrEmpty(_notice))
            {
                AddText(_notice, false, _linkColor);
            }

            if (_page == "connection")
            {
                BuildConnection();
            }
            else if (_page == "permissions")
            {
                BuildPermissions();
            }
            else if (_page == "cookies")
            {
                BuildCookies();
            }
            else
            {
                BuildHome();
            }

            if (_reloadNeeded)
            {
                Separator();
                AddText("Reload this page to apply your changes.");
                AddAction("Reload", () =>
                {
                    _controller.Reload(_snapshot);
                    return Task.CompletedTask;
                });
            }

            _content.ResumeLayout(true);
            FitToScreen();
        }

        private string PageTitle()
        {
            switch (_page)
            {
                case "connection":
                    return "Connection details";

                case "permissions":
                    return "Site settings";

                default:
                    return "Cookies and site data";
            }
        }

        private void BuildHome()
        {
            _securityTitle = AddText(_snapshot.Summary, true, _snapshot.IsSecure ? _linkColor : _textColor);
            _securityDescription = AddText(_snapshot.Description, false, _secondaryTextColor);
            if (!_snapshot.IsWebsite)
            {
                return;
            }

            AddNavigation("Connection details", "connection");
            Separator();
            string cookieText = "Cookies and site data";
            if (_loading)
            {
                cookieText += " — Loading…";
            }
            else if (_cookies != null)
            {
                cookieText += " (" + _cookies.Count + ")";
            }

            AddNavigation(cookieText, "cookies");
            Separator();
            AddText("Permissions", true);
            if (_loading)
            {
                AddText("Loading site permissions…", false, _secondaryTextColor);
            }
            else if (_permissions == null)
            {
                AddText("Site permissions are unavailable in this WebView2 runtime.", false, _secondaryTextColor);
            }
            else
            {
                var kinds = _permissions.Keys.Concat(_snapshot.RequestedPermissions).Distinct().Where(k => k != CoreWebView2PermissionKind.UnknownPermission).ToArray();
                foreach (var kind in kinds.Take(4))
                {
                    AddPermission(kind);
                }

                if (kinds.Length == 0)
                {
                    AddText("No custom permissions. The browser's defaults apply.", false, _secondaryTextColor);
                }

                if (kinds.Length > 4)
                {
                    AddText("More permissions are available in Site settings.", false, _secondaryTextColor);
                }
            }

            AddNavigation("Site settings", "permissions");
        }

        private void BuildConnection()
        {
            _securityTitle = AddText(_snapshot.Summary, true);
            _securityDescription = AddText(_snapshot.Description, false, _secondaryTextColor);
            var cert = _snapshot.Certificate;
            if (cert == null)
            {
                AddText("No certificate details are available for this page.", false, _secondaryTextColor);
                return;
            }

            Separator();
            AddText("Certificate", true);
            AddText("Issued to: " + ((string)cert["subjectName"] ?? "Unavailable"));
            AddText("Issued by: " + ((string)cert["issuer"] ?? "Unavailable"));
            AddText("Valid from: " + FormatTime(cert["validFrom"]));
            AddText("Valid until: " + FormatTime(cert["validTo"]));
            var error = (string)cert["certificateNetworkError"];
            if (!string.IsNullOrEmpty(error))
            {
                AddText("Certificate problem: " + error);
            }

            var certificates = cert["certificate"] as JArray;
            if (certificates?.Count > 0)
            {
                AddAction("View certificate…", () =>
                {
                    _dialogOpen = true;
                    try
                    {
                        using (var certificate = new X509Certificate2(Convert.FromBase64String((string)certificates[0])))
                        {
                            X509Certificate2UI.DisplayCertificate(certificate, Handle);
                        }
                    }
                    finally
                    {
                        _dialogOpen = false;
                    }

                    return Task.CompletedTask;
                });
            }

            Separator();
            AddText("Encryption", true);
            AddText("Protocol: " + ((string)cert["protocol"] ?? "Unavailable"));
            AddText("Cipher: " + ((string)cert["cipher"] ?? "Unavailable"));
            string exchange = (string)cert["keyExchangeGroup"] ?? (string)cert["keyExchange"];
            if (!string.IsNullOrEmpty(exchange))
            {
                AddText("Key exchange: " + exchange);
            }

            if (_snapshot.Security["securityStateIssueIds"] is JArray issues && issues.Count > 0)
            {
                AddText("Reported issues: " + string.Join(", ", issues.Values<string>()), false, _secondaryTextColor);
            }
        }

        private void BuildPermissions()
        {
            AddText("These settings apply to this site's origin in your current Quartz profile. Default uses WebView2's normal behaviour.", false, _secondaryTextColor);
            if (_loading)
            {
                AddText("Loading…");
                return;
            }

            if (_permissions == null)
            {
                AddText("This runtime could not provide permission settings.");
                return;
            }

            foreach (CoreWebView2PermissionKind kind in Enum.GetValues(typeof(CoreWebView2PermissionKind)))
            {
                if (kind != CoreWebView2PermissionKind.UnknownPermission)
                {
                    AddPermission(kind);
                }
            }

            Separator();
            AddAction("Reset permissions", async () =>
            {
                if (!Confirm("Reset this site's permissions to the browser defaults?"))
                {
                    return;
                }

                await _controller.ResetPermissionsAsync(_snapshot.Origin);
                _reloadNeeded = true;
                _permissions = await _controller.GetPermissionsAsync(_snapshot.Origin);
                _notice = "Site permissions reset.";
                BuildPage();
            });
        }

        private void AddPermission(CoreWebView2PermissionKind kind)
        {
            var row = new Panel
            {
                Size = new Size(ContentWidth, ScalePixels(39)),
                Margin = new Padding(0, 0, 0, ScalePixels(4))
            };
            var name = new Label
            {
                Text = PermissionName(kind),
                Location = new Point(0, ScalePixels(7)),
                Size = new Size(ScalePixels(198), ScalePixels(29)),
                ForeColor = _textColor,
                AutoEllipsis = true
            };
            var choice = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(ScalePixels(202), ScalePixels(4)),
                Width = ScalePixels(124),
                BackColor = BackColor,
                ForeColor = _textColor,
                AccessibleName = PermissionName(kind) + " permission"
            };
            choice.Items.AddRange(new object[] { "Default", "Allow", "Block" });
            _permissions.TryGetValue(kind, out CoreWebView2PermissionState state);
            int savedIndex = 0;
            if (state == CoreWebView2PermissionState.Allow)
            {
                savedIndex = 1;
            }
            else if (state == CoreWebView2PermissionState.Deny)
            {
                savedIndex = 2;
            }
            choice.SelectedIndex = savedIndex;
            choice.SelectionChangeCommitted += async (s, e) =>
            {
                int selected = choice.SelectedIndex;
                await RunActionAsync(async () =>
                {
                    try
                    {
                        CoreWebView2PermissionState next = CoreWebView2PermissionState.Default;
                        if (selected == 1)
                        {
                            next = CoreWebView2PermissionState.Allow;
                        }
                        else if (selected == 2)
                        {
                            next = CoreWebView2PermissionState.Deny;
                        }

                        await _controller.SetPermissionAsync(_snapshot.Origin, kind, next);
                        _permissions[kind] = next;
                        savedIndex = selected;
                        _reloadNeeded = true;
                        _notice = PermissionName(kind) + " updated.";
                        BuildPage();
                    }
                    catch
                    {
                        if (!choice.IsDisposed)
                        {
                            choice.SelectedIndex = savedIndex;
                        }

                        throw;
                    }
                });
            };
            row.Controls.Add(name);
            row.Controls.Add(choice);
            _content.Controls.Add(row);
        }

        private void BuildCookies()
        {
            AddText("Cookies available to this host, including all paths and shared parent-domain cookies. Embedded third-party sites are not included.", false, _secondaryTextColor);
            if (_loading)
            {
                AddText("Loading…");
                return;
            }

            AddText("Site storage: " + (_usage.HasValue ? FormatBytes(_usage.Value) : "Unavailable"), true);
            AddText("Storage includes databases, service workers and cached site data. Some storage types may not be included in the reported size.", false, _secondaryTextColor);
            if (_cookies == null)
            {
                AddText("Cookies could not be loaded.");
                return;
            }

            var list = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                MultiSelect = false,
                HideSelection = false,
                Size = new Size(ContentWidth, ScalePixels(190)),
                BackColor = BackColor,
                ForeColor = _textColor,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, ScalePixels(8)),
                AccessibleName = "Site cookies"
            };
            list.Columns.Add("Name", ScalePixels(152));
            list.Columns.Add("Domain", ScalePixels(148));
            foreach (var cookie in _cookies)
            {
                list.Items.Add(new ListViewItem(new[] { cookie.Name, cookie.Domain }) { Tag = cookie });
            }

            _content.Controls.Add(list);
            var details = AddText("Select a cookie to inspect its details.", false, _secondaryTextColor);
            var remove = AddAction("Remove selected cookie", async () =>
            {
                if (list.SelectedItems.Count == 0)
                {
                    return;
                }

                var cookie = (CoreWebView2Cookie)list.SelectedItems[0].Tag;
                if (!Confirm("Remove this cookie? This may sign you out or reset a site preference."))
                {
                    return;
                }

                await _controller.DeleteCookieAsync(cookie);
                _cookies.Remove(cookie);
                _reloadNeeded = true;
                BuildPage();
            });
            remove.Enabled = false;
            list.SelectedIndexChanged += (s, e) =>
            {
                remove.Enabled = list.SelectedItems.Count > 0;
                if (list.SelectedItems.Count == 0)
                {
                    return;
                }

                var c = (CoreWebView2Cookie)list.SelectedItems[0].Tag;
                details.Text = "Path: " + c.Path + "\nSecure: " + c.IsSecure + "   HTTP-only: " + c.IsHttpOnly + "\nSameSite: " + c.SameSite + "\nExpires: " + (c.IsSession ? "End of session" : c.Expires.ToLocalTime().ToString("g"));
                FitToScreen();
            };
            AddAction("Clear cookies and site data…", async () =>
            {
                if (!Confirm("Clear this site's cookies and stored data? You may be signed out. Shared domain cookies can also affect related subdomains."))
                {
                    return;
                }

                await _controller.ClearSiteDataAsync(_snapshot.Uri);
                _reloadNeeded = true;
                _notice = "Site data cleared.";
                await LoadDetailsAsync();
            });
        }

        private async Task RunActionAsync(Func<Task> action)
        {
            if (_busy || IsDisposed)
            {
                return;
            }

            _busy = true;
            _content.Enabled = false;
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                SiteInfoController.Report("site action", ex);
                if (!IsDisposed)
                {
                    _notice = "Couldn't finish that action. Some changes may have applied. Reopen this panel to refresh.";
                    BuildPage();
                }
            }
            finally
            {
                _busy = false;
                if (!IsDisposed)
                {
                    _content.Enabled = true;
                }
            }
        }

        private bool Confirm(string message)
        {
            _dialogOpen = true;
            try
            {
                return MessageBox.Show(this, message, "Site information", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.OK;
            }
            finally
            {
                _dialogOpen = false;
            }
        }

        private Label AddText(string text, bool bold = false, Color? color = null)
        {
            var label = new Label
            {
                Text = text,
                AutoSize = true,
                MaximumSize = new Size(ContentWidth, 0),
                MinimumSize = new Size(ContentWidth, 0),
                Margin = new Padding(0, 0, 0, ScalePixels(10)),
                ForeColor = color ?? _textColor,
                Font = bold ? _sectionFont : Font,
                UseMnemonic = false
            };
            _content.Controls.Add(label);
            return label;
        }

        private Button SmallButton(string text, string accessible)
        {
            Button button = new Button();
            button.Text = text;
            button.AccessibleName = accessible;
            button.FlatStyle = FlatStyle.Flat;
            button.Size = new Size(ScalePixels(28), ScalePixels(28));
            button.ForeColor = _secondaryTextColor;
            button.BackColor = BackColor;
            button.FlatAppearance.BorderSize = 0;
            button.TabStop = true;
            return button;
        }

        private Button AddAction(string text, Func<Task> action)
        {
            var button = new Button
            {
                Text = text,
                AccessibleName = text,
                TextAlign = ContentAlignment.MiddleLeft,
                FlatStyle = FlatStyle.Flat,
                ForeColor = _linkColor,
                BackColor = BackColor,
                Size = new Size(ContentWidth, ScalePixels(38)),
                Margin = new Padding(0, 0, 0, ScalePixels(4)),
                Padding = new Padding(ScalePixels(4), 0, 0, 0),
                FlatAppearance =
                {
                    BorderSize = 0,
                    MouseOverBackColor = _borderColor
                },
                Cursor = Cursors.Hand
            };
            button.Click += async (s, e) => await RunActionAsync(action);
            _content.Controls.Add(button);
            return button;
        }

        private void AddNavigation(string label, string page)
        {
            AddAction(label + "   ›", () =>
            {
                Navigate(page);
                return Task.CompletedTask;
            });
        }

        private void Separator()
        {
            Panel divider = new Panel();
            divider.Height = 1;
            divider.Width = ContentWidth;
            divider.BackColor = _borderColor;
            divider.Margin = new Padding(0, ScalePixels(4), 0, ScalePixels(12));
            _content.Controls.Add(divider);
        }

        private void FitToScreen()
        {
            var area = Screen.FromPoint(_anchor).WorkingArea;
            int preferred = _content.Controls.Cast<Control>().Sum(c => c.Height + c.Margin.Vertical) + ScalePixels(32) + 2;
            int height = Math.Min(Math.Max(ScalePixels(180), preferred), Math.Min(ScalePixels(660), area.Height - ScalePixels(24)));
            ClientSize = new Size(ScalePixels(360) + (preferred > height ? SystemInformation.VerticalScrollBarWidth : 0), height);
            Location = new Point(Math.Max(area.Left, Math.Min(_anchor.X, area.Right - Width)), Math.Max(area.Top, Math.Min(_anchor.Y, area.Bottom - Height)));
        }

        private static string FormatTime(JToken token)
        {
            if (token == null)
            {
                return "Unavailable";
            }

            long seconds = (long)(double)token;
            DateTimeOffset time = DateTimeOffset.FromUnixTimeSeconds(seconds);
            return time.LocalDateTime.ToString("g");
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes >= 1024 * 1024)
            {
                return (bytes / (1024d * 1024)).ToString("0.0") + " MB";
            }

            if (bytes >= 1024)
            {
                return (bytes / 1024d).ToString("0.0") + " KB";
            }

            return bytes + " bytes";
        }

        private static string PermissionName(CoreWebView2PermissionKind kind)
        {
            switch (kind)
            {
                case CoreWebView2PermissionKind.Geolocation:
                    return "Location";
                case CoreWebView2PermissionKind.ClipboardRead:
                    return "Clipboard";
                case CoreWebView2PermissionKind.OtherSensors:
                    return "Motion / light sensors";
                case CoreWebView2PermissionKind.MultipleAutomaticDownloads:
                    return "Automatic downloads";
                case CoreWebView2PermissionKind.FileReadWrite:
                    return "File editing";
                case CoreWebView2PermissionKind.LocalFonts:
                    return "Local fonts";
                case CoreWebView2PermissionKind.MidiSystemExclusiveMessages:
                    return "MIDI devices";
                case CoreWebView2PermissionKind.WindowManagement:
                    return "Window management";
                case CoreWebView2PermissionKind.PersistentStorage:
                    return "Persistent storage";
                default:
                    return kind.ToString();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_anchorOwner != null)
                {
                    _anchorOwner.LocationChanged -= OwnerMoved;
                    _anchorOwner.Resize -= OwnerMoved;
                }

                _headingFont.Dispose();
                _sectionFont.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
