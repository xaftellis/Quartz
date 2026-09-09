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
        private readonly Color _hoverColor;
        private readonly Color _pressedColor;
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
        private Label _noticeLabel;
        private Button _reloadButton;
        private Form _anchorOwner;
        private Point _anchor;

        public SiteInfoPopup(SiteInfoController controller, SiteInfoSnapshot snapshot, Color background, Color foreground, float scale = 1f)
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
            // Browser.LoadTheme supplies Quartz's actual form colours for every theme.
            BackColor = background;
            _textColor = foreground;
            _secondaryTextColor = foreground;
            _linkColor = foreground;
            _borderColor = BlendThemeColor(30);
            _hoverColor = BlendThemeColor(10);
            _pressedColor = BlendThemeColor(20);
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

        private Color BlendThemeColor(int foregroundPercent)
        {
            int backgroundPercent = 100 - foregroundPercent;
            return Color.FromArgb(
                (BackColor.R * backgroundPercent + _textColor.R * foregroundPercent) / 100,
                (BackColor.G * backgroundPercent + _textColor.G * foregroundPercent) / 100,
                (BackColor.B * backgroundPercent + _textColor.B * foregroundPercent) / 100);
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
            _noticeLabel = null;
            _reloadButton = null;
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
            var close = SmallButton(SiteInfoIcon.Close, "Close site information");
            close.Location = new Point(ContentWidth - ScalePixels(28), 0);
            close.Click += (s, e) => Close();
            header.Controls.Add(close);
            if (subpage)
            {
                var back = SmallButton(SiteInfoIcon.Back, "Back to site information");
                back.Click += (s, e) => Navigate("home");
                header.Controls.Add(back);
            }

            _content.Controls.Add(header);
            if (_snapshot.IsWebsite)
            {
                AddText(_snapshot.Origin, false, _secondaryTextColor);
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

            // Set the final bounds before AutoScroll lays out the rebuilt controls.
            UpdateStatus();
            _content.ResumeLayout(true);
        }

        private void UpdateStatus()
        {
            if (IsDisposed || Disposing)
            {
                return;
            }

            Point scroll = _content.AutoScrollPosition;
            _content.SuspendLayout();
            try
            {
                if (!string.IsNullOrEmpty(_notice))
                {
                    if (_noticeLabel == null)
                    {
                        _noticeLabel = AddText(_notice, false, _linkColor);
                        _content.Controls.SetChildIndex(_noticeLabel, _snapshot.IsWebsite ? 2 : 1);
                    }
                    else
                    {
                        _noticeLabel.Text = _notice;
                    }
                }

                if (_reloadNeeded && _reloadButton == null)
                {
                    Separator();
                    AddText("Reload this page to apply your changes.");
                    _reloadButton = AddAction("Reload", () =>
                    {
                        _controller.Reload(_snapshot);
                        return Task.CompletedTask;
                    });
                }

                FitToScreen();
            }
            finally
            {
                _content.ResumeLayout(true);
            }
            _content.AutoScrollPosition = new Point(-scroll.X, -scroll.Y);
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
                if (IsDisposed)
                {
                    return;
                }

                foreach (Control row in _content.Controls)
                {
                    foreach (ComboBox choice in row.Controls.OfType<ComboBox>())
                    {
                        choice.SelectedIndex = PermissionIndex((CoreWebView2PermissionKind)choice.Tag);
                    }
                }
                _notice = "Site permissions reset.";
                UpdateStatus();
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
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = Font.Height + ScalePixels(4),
                Location = new Point(ScalePixels(202), ScalePixels(4)),
                Width = ScalePixels(124),
                BackColor = BackColor,
                ForeColor = _textColor,
                Tag = kind,
                AccessibleName = PermissionName(kind) + " permission"
            };
            choice.DrawItem += PermissionChoice_DrawItem;
            choice.Items.AddRange(new object[] { "Default", "Allow", "Block" });
            choice.SelectedIndex = PermissionIndex(kind);
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
                        _reloadNeeded = true;
                        _notice = PermissionName(kind) + " updated.";
                        UpdateStatus();
                    }
                    catch
                    {
                        if (!choice.IsDisposed)
                        {
                            choice.SelectedIndex = PermissionIndex(kind);
                        }

                        throw;
                    }
                });
            };
            row.Controls.Add(name);
            row.Controls.Add(choice);
            _content.Controls.Add(row);
        }

        private int PermissionIndex(CoreWebView2PermissionKind kind)
        {
            _permissions.TryGetValue(kind, out CoreWebView2PermissionState state);
            if (state == CoreWebView2PermissionState.Allow)
            {
                return 1;
            }
            if (state == CoreWebView2PermissionState.Deny)
            {
                return 2;
            }
            return 0;
        }

        private void PermissionChoice_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            ComboBox choice = (ComboBox)sender;
            bool selected = (e.State & DrawItemState.Selected) != 0;
            using (var brush = new SolidBrush(selected ? _pressedColor : BackColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            Rectangle textBounds = Rectangle.Inflate(e.Bounds, -ScalePixels(4), 0);
            TextRenderer.DrawText(e.Graphics, choice.Items[e.Index].ToString(), choice.Font,
                textBounds, _textColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            e.DrawFocusRectangle();
        }

        private void BuildCookies()
        {
            AddText("Cookies available to this host, including all paths and shared parent-domain cookies. Embedded third-party sites are not included.", false, _secondaryTextColor);
            if (_loading)
            {
                AddText("Loading…");
                return;
            }

            var storage = AddText("Site storage: " + (_usage.HasValue ? FormatBytes(_usage.Value) : "Unavailable"), true);
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
                OwnerDraw = true,
                Size = new Size(ContentWidth, ScalePixels(190)),
                BackColor = BackColor,
                ForeColor = _textColor,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, ScalePixels(8)),
                AccessibleName = "Site cookies"
            };
            list.DrawColumnHeader += CookieList_DrawColumnHeader;
            list.DrawSubItem += CookieList_DrawSubItem;
            list.Columns.Add("Name", ScalePixels(152));
            list.Columns.Add("Domain", ScalePixels(148));
            list.ClientSizeChanged += (s, e) =>
            {
                // Resizing can also fire while the page's controls are being disposed.
                if (list.Disposing || list.IsDisposed || list.Columns.Count < 2)
                {
                    return;
                }

                list.Columns[1].Width = Math.Max(1, list.ClientSize.Width - list.Columns[0].Width);
            };
            list.Columns[1].Width = Math.Max(1, list.ClientSize.Width - list.Columns[0].Width);
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

                var item = list.SelectedItems[0];
                var cookie = (CoreWebView2Cookie)item.Tag;
                if (!Confirm("Remove this cookie? This may sign you out or reset a site preference."))
                {
                    return;
                }

                await _controller.DeleteCookieAsync(cookie);
                _cookies.Remove(cookie);
                _reloadNeeded = true;
                if (list.IsDisposed)
                {
                    return;
                }
                item.Remove();
                UpdateStatus();
            });
            remove.Enabled = false;
            list.SelectedIndexChanged += (s, e) =>
            {
                if (list.Disposing || list.IsDisposed)
                {
                    return;
                }
                remove.Enabled = list.SelectedItems.Count > 0;
                if (list.SelectedItems.Count == 0)
                {
                    details.Text = "Select a cookie to inspect its details.";
                    FitToScreen();
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
                // Read back the actual data: a live page can create cookies again.
                _cookies = await _controller.GetCookiesAsync(_snapshot.Uri);
                try
                {
                    _usage = await _controller.GetStorageUsageAsync(_snapshot.Origin);
                }
                catch (Exception ex)
                {
                    _usage = null;
                    SiteInfoController.Report("read storage", ex);
                }
                if (list.IsDisposed)
                {
                    return;
                }
                list.BeginUpdate();
                try
                {
                    list.Items.Clear();
                    foreach (var cookie in _cookies)
                    {
                        list.Items.Add(new ListViewItem(new[] { cookie.Name, cookie.Domain }) { Tag = cookie });
                    }
                }
                finally
                {
                    list.EndUpdate();
                }
                storage.Text = "Site storage: " + (_usage.HasValue ? FormatBytes(_usage.Value) : "Unavailable");
                UpdateStatus();
            });
        }

        private void CookieList_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            DrawCookieCell(e.Graphics, e.Bounds, e.Header.Text, _hoverColor);
            using (var pen = new Pen(_borderColor))
            {
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            }
        }

        private void CookieList_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            DrawCookieCell(e.Graphics, e.Bounds, e.SubItem.Text, e.Item.Selected ? _pressedColor : BackColor);
            if (e.ColumnIndex == 0 && e.Item.Focused && ((ListView)sender).Focused)
            {
                e.DrawFocusRectangle(e.Bounds);
            }
        }

        private void DrawCookieCell(Graphics graphics, Rectangle bounds, string text, Color background)
        {
            using (var brush = new SolidBrush(background))
            {
                graphics.FillRectangle(brush, bounds);
            }

            Rectangle textBounds = Rectangle.Inflate(bounds, -ScalePixels(4), 0);
            TextRenderer.DrawText(graphics, text, Font, textBounds, _textColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        private async Task RunActionAsync(Func<Task> action)
        {
            if (_busy || IsDisposed)
            {
                return;
            }

            _busy = true;
            Control focusedControl = ActiveControl;
            Point scroll = _content.AutoScrollPosition;
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
                    UpdateStatus();
                }
            }
            finally
            {
                _busy = false;
                if (!IsDisposed)
                {
                    _content.Enabled = true;
                    if (focusedControl != null && !focusedControl.IsDisposed && focusedControl.CanFocus)
                    {
                        focusedControl.Focus();
                        _content.AutoScrollPosition = new Point(-scroll.X, -scroll.Y);
                    }
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

        private Button SmallButton(SiteInfoIcon icon, string accessible)
        {
            SiteInfoButton button = new SiteInfoButton();
            button.IconKind = icon;
            button.AccessibleName = accessible;
            button.FlatStyle = FlatStyle.Flat;
            button.Size = new Size(ScalePixels(28), ScalePixels(28));
            button.ForeColor = _secondaryTextColor;
            button.BackColor = BackColor;
            button.UseVisualStyleBackColor = false;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = _hoverColor;
            button.FlatAppearance.MouseDownBackColor = _pressedColor;
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
                UseVisualStyleBackColor = false,
                Size = new Size(ContentWidth, ScalePixels(38)),
                Margin = new Padding(0, 0, 0, ScalePixels(4)),
                Padding = new Padding(ScalePixels(4), 0, 0, 0),
                FlatAppearance =
                {
                    BorderSize = 0,
                    MouseOverBackColor = _hoverColor,
                    MouseDownBackColor = _pressedColor
                },
                Cursor = Cursors.Hand
            };
            button.Paint += PopupButton_Paint;
            button.Click += async (s, e) => await RunActionAsync(action);
            _content.Controls.Add(button);
            return button;
        }

        private void PopupButton_Paint(object sender, PaintEventArgs e)
        {
            Button button = (Button)sender;
            if (button.Enabled)
            {
                return;
            }

            // WinForms' default disabled text can disappear on the black theme.
            using (var brush = new SolidBrush(BackColor))
            {
                e.Graphics.FillRectangle(brush, button.ClientRectangle);
            }

            TextFormatFlags alignment = TextFormatFlags.Left;
            if (button.TextAlign == ContentAlignment.MiddleCenter)
            {
                alignment = TextFormatFlags.HorizontalCenter;
            }

            Rectangle textBounds = Rectangle.Inflate(button.ClientRectangle, -ScalePixels(4), 0);
            TextRenderer.DrawText(e.Graphics, button.Text, button.Font, textBounds, BlendThemeColor(65),
                alignment | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
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
            // Auto-sized labels have not received their final height during a rebuild.
            int preferred = _content.Controls.Cast<Control>().Sum(c =>
                (c.AutoSize ? c.GetPreferredSize(new Size(ContentWidth, 0)).Height : c.Height)
                + c.Margin.Vertical) + ScalePixels(32) + 2;
            int height = Math.Min(Math.Max(ScalePixels(180), preferred), Math.Min(ScalePixels(660), area.Height - ScalePixels(24)));
            if (preferred > height)
            {
                _content.AutoScrollMinSize = new Size(0, preferred);
            }
            else
            {
                _content.AutoScrollMinSize = Size.Empty;
            }
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
