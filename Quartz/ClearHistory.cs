using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using Quartz.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quartz
{
    public partial class ClearHistory : Form
    {
        private readonly WebView2 _webView;
        private CoreWebView2Profile _profile;
        private Guid _profileId;
        private bool _isReady;
        private bool _isBusy;
        private static readonly string[] TimeRangeIds =
        {
            "last15Minutes",
            "lastHour",
            "lastDay",
            "lastWeek",
            "last4Weeks",
            "allTime"
        };

        // Refresh the caller even after a partially successful deletion.
        internal bool DataChanged { get; private set; }

        public ClearHistory(WebView2 webView)
        {
            _webView = webView ?? throw new ArgumentNullException(nameof(webView));
            InitializeComponent();
            SetDefaultSelection();
            foreach (CheckBox option in SelectionOptions)
                option.CheckedChanged += Selection_CheckedChanged;
            NewControlThemeChanger.ChangeTheme(this);
            ApplyVisualFinishing();
            UpdateSelectionState();
        }

        private IEnumerable<CheckBox> SelectionOptions => new[]
        {
            chkBrowsingHistory, chkDownloadHistory, chkCookies,
            chkCache, chkPasswords, chkAutofill
        };

        private async void ClearHistory_Load(object sender, EventArgs e)
        {
            try
            {
                if (_webView.IsDisposed || _webView.CoreWebView2 == null)
                    throw new InvalidOperationException("Reopen this dialog when the browser tab is ready.");
                _profile = _webView.CoreWebView2.Profile;
                if (!Guid.TryParse(_profile.ProfileName, out _profileId))
                    throw new InvalidOperationException("Quartz couldn't identify this tab's profile.");
                RestoreSelection();
                _isReady = true;
                UpdateSelectionState();
                await RefreshDataSummaryAsync();
            }
            catch (Exception exception)
            {
                if (IsDisposed || Disposing) return;
                _isReady = false;
                UpdateSelectionState();
                lblBrowsingHistoryInfo.Text = "History information is unavailable.";
                lblCookiesInfo.Text = "Cookie information is unavailable.";
                lblStatus.Text = exception.Message;
            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (!_isReady || _isBusy) return;
            CoreWebView2BrowsingDataKinds dataKinds = GetSelectedDataKinds();
            if (dataKinds == 0)
            {
                UpdateSelectionState();
                return;
            }

            try
            {
                SaveSelection();
            }
            catch (Exception exception)
            {
                lblStatus.Text = "Couldn't save your options. Please try again.";
                MessageBox.Show(this, exception.Message, "Save clearing options",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Capture the selection before yielding to WebView2.
            bool clearHistory = chkBrowsingHistory.Checked;
            bool clearCache = chkCache.Checked;
            DateTime endTime = DateTime.Now;
            DateTime? startTime = GetStartTime(endTime);
            var errors = new List<string>();
            SetBusy(true);
            DataChanged = true;
            try
            {
                try
                {
                    if (startTime.HasValue)
                        await _profile.ClearBrowsingDataAsync(dataKinds, startTime.Value, endTime);
                    else
                        await _profile.ClearBrowsingDataAsync(dataKinds);
                }
                catch (Exception exception)
                {
                    errors.Add("Browser data: " + exception.Message);
                }

                if (clearHistory)
                {
                    try
                    {
                        // Reload after the await to retain newer visits and other profiles.
                        new HistoryService().DeleteProfileHistory(
                            _profileId, startTime, startTime.HasValue ? endTime : (DateTime?)null);
                    }
                    catch (Exception exception)
                    {
                        errors.Add("Quartz history: " + exception.Message);
                    }
                }
                // Shared icons have no visit timestamps; only clear them for All time.
                if (clearCache && !startTime.HasValue)
                {
                    try
                    {
                        string cachePath = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            @"Xaftellis\Quartz\UserData\cache");
                        new FaviconService().ClearCache(cachePath);
                    }
                    catch (Exception exception)
                    {
                        errors.Add("Cached website icons: " + exception.Message);
                    }
                }
            }
            finally
            {
                if (!IsDisposed && !Disposing) SetBusy(false);
            }

            if (IsDisposed || Disposing) return;
            if (errors.Count == 0)
            {
                DialogResult = DialogResult.OK;
                Close();
                return;
            }
            UpdateHistorySummary();
            lblCookiesInfo.Text = "Cookies and other site data; some data may have been cleared.";
            lblStatus.Text = "Some data couldn't be cleared. You can try again.";
            MessageBox.Show(this,
                "Quartz couldn't finish clearing all the selected data. Some data may already have been removed."
                + Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, errors),
                "Clear browsing data", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            // WebView2 cannot cancel a clear operation once it has started.
            if (_isBusy && e.CloseReason == CloseReason.UserClosing) e.Cancel = true;
        }

        private void RestoreSelection()
        {
            string saved = SettingsService.Get("ClearBrowsingDataOptions", _profileId);
            if (string.IsNullOrEmpty(saved)) return;

            Dictionary<string, string> options;
            try
            {
                options = JsonConvert.DeserializeObject<Dictionary<string, string>>(saved);
            }
            catch (JsonException)
            {
                return; // Keep the defaults if the saved options cannot be read.
            }
            if (options == null) return;

            cboTimeRange.SelectedIndex = GetSavedTimeRange(options);

            string value;
            foreach (CheckBox option in SelectionOptions)
            {
                bool selected;
                if (options.TryGetValue(option.Name, out value) && bool.TryParse(value, out selected))
                    option.Checked = selected;
            }
        }

        private void SaveSelection()
        {
            var options = new Dictionary<string, string>();
            foreach (CheckBox option in SelectionOptions)
            {
                options[option.Name] = option.Checked.ToString();
            }
            options["TimeRangeId"] = TimeRangeIds[cboTimeRange.SelectedIndex];
            SettingsService.Set("ClearBrowsingDataOptions", JsonConvert.SerializeObject(options), _profileId);
        }

        private void SetDefaultSelection()
        {
            cboTimeRange.SelectedIndex = 0;
            chkBrowsingHistory.Checked = true;
            chkDownloadHistory.Checked = true;
            chkCookies.Checked = true;
            chkCache.Checked = true;
            chkPasswords.Checked = false;
            chkAutofill.Checked = false;
        }

        private DateTime? GetStartTime(DateTime endTime)
        {
            switch (cboTimeRange.SelectedIndex)
            {
                case 0:
                    return endTime.AddMinutes(-15);
                case 1:
                    return endTime.AddHours(-1);
                case 2:
                    return endTime.AddDays(-1);
                case 3:
                    return endTime.AddDays(-7);
                case 4:
                    return endTime.AddDays(-28);
                default:
                    return null;
            }
        }

        internal static int GetSavedTimeRange(Dictionary<string, string> options)
        {
            string value;
            if (options.TryGetValue("TimeRangeId", out value))
            {
                int index = Array.IndexOf(TimeRangeIds, value);
                if (index >= 0)
                {
                    return index;
                }

                return 0;
            }
            // The old list started with Last hour. Preserve its saved meaning.
            int legacyIndex;
            if (options.TryGetValue("TimeRange", out value) && int.TryParse(value, out legacyIndex))
            {
                if (legacyIndex >= 0 && legacyIndex <= 4)
                {
                    return legacyIndex + 1;
                }
            }

            return 0;
        }

        private CoreWebView2BrowsingDataKinds GetSelectedDataKinds()
        { 
            CoreWebView2BrowsingDataKinds dataKinds = 0;
            if (chkBrowsingHistory.Checked) dataKinds |= CoreWebView2BrowsingDataKinds.BrowsingHistory;
            if (chkDownloadHistory.Checked) dataKinds |= CoreWebView2BrowsingDataKinds.DownloadHistory;
            if (chkCookies.Checked) dataKinds |= CoreWebView2BrowsingDataKinds.AllSite;
            if (chkCache.Checked) dataKinds |= CoreWebView2BrowsingDataKinds.DiskCache;
            if (chkPasswords.Checked) dataKinds |= CoreWebView2BrowsingDataKinds.PasswordAutosave;
            if (chkAutofill.Checked) dataKinds |= CoreWebView2BrowsingDataKinds.GeneralAutofill;
            return dataKinds;
        }

        private void SetBusy(bool busy)
        {
            _isBusy = busy;
            UseWaitCursor = busy;
            btnCancel.Enabled = !busy;
            btnDelete.Text = busy ? "Deleting..." : "Delete data";
            UpdateSelectionState();
        }

        private void UpdateSelectionState()
        {
            bool enabled = _isReady && !_isBusy;
            bool hasSelection = GetSelectedDataKinds() != 0;

            cboTimeRange.Enabled = enabled;
            foreach (CheckBox option in SelectionOptions)
            {
                option.Enabled = enabled;
            }
            btnDelete.Enabled = enabled && hasSelection;

            if (_isBusy)
            {
                lblStatus.Text = "Clearing browsing data...";
            }
            else if (!_isReady)
            {
                lblStatus.Text = "Waiting for the browser profile...";
            }
            else if (!hasSelection)
            {
                lblStatus.Text = "Select at least one type of data to clear.";
            }
    
            else
            {
                lblStatus.Text = "Only data from this Quartz profile will be cleared.";
            }
        }

        private async Task RefreshDataSummaryAsync()
        {
            UpdateHistorySummary();
            try
            {
                var cookies = await _webView.CoreWebView2.CookieManager.GetCookiesAsync(null);
                if (IsDisposed || Disposing || _isBusy || DataChanged) return;
                int domains = cookies.Select(cookie => cookie.Domain.TrimStart('.'))
                    .Distinct(StringComparer.OrdinalIgnoreCase).Count();
                lblCookiesInfo.Text = $"Cookies from {domains:N0} domain{(domains == 1 ? "" : "s")} (all time), plus other site data.";
            }
            catch (Exception)
            {
                if (!IsDisposed && !Disposing && !_isBusy && !DataChanged)
                    lblCookiesInfo.Text = "Cookies and other site data. Cookie count is unavailable.";
            }
        }

        private void UpdateHistorySummary()
        {
            if (!_isReady) return;
            try
            {
                DateTime endTime = DateTime.Now;
                DateTime? startTime = GetStartTime(endTime);
                var histories = new HistoryService().GetProfileHistoryFromRange(
                    _profileId, startTime, startTime.HasValue ? endTime : DateTime.MaxValue);
                if (histories.Count > 1)
                    lblBrowsingHistoryInfo.Text = $"From {new Uri(histories.FirstOrDefault().WebAddress).Host} + {histories.Count} sites";
                else
                    lblBrowsingHistoryInfo.Text = $"From {new Uri(histories.FirstOrDefault().WebAddress).Host}";
            }
            catch (Exception)
            {
                lblBrowsingHistoryInfo.Text = "None";
            }
        }

        private void ApplyVisualFinishing()
        {
            bool darkBackground = BackColor.GetBrightness() < 0.45f;
            Color secondaryText = darkBackground
                ? Color.FromArgb(175, 175, 175)
                : Color.FromArgb(95, 99, 104);

            Label[] secondaryLabels =
            {
                lblSubtitle,
                lblBrowsingHistoryInfo,
                lblDownloadHistoryInfo,
                lblCookiesInfo,
                lblCacheInfo,
                lblPasswordsInfo,
                lblAutofillInfo,
                //lblNotice,
                lblStatus
            };

            foreach (Label label in secondaryLabels)
                label.ForeColor = secondaryText;

            pnlNotice.BackColor = darkBackground
                ? Color.FromArgb(48, 48, 48)
                : Color.FromArgb(241, 243, 244);
            lblNotice.BackColor = darkBackground
                ? Color.FromArgb(48, 48, 48)
                : Color.FromArgb(241, 243, 244);

            Color dividerColor = darkBackground
                ? Color.FromArgb(70, 70, 70)
                : Color.FromArgb(218, 220, 224);
            pnlTopDivider.BackColor = dividerColor;
            pnlBottomDivider.BackColor = dividerColor;

            btnDelete.BackColor = Color.FromArgb(26, 115, 232);
            btnDelete.ForeColor = Color.White;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.FlatAppearance.BorderSize = 0;
        }

        private void Selection_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSelectionState();
        }

        private void cboTimeRange_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateHistorySummary();
            UpdateSelectionState();
        }
    }
}
