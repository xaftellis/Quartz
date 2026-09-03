using Microsoft.Web.WebView2;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Quartz.Models;
using Quartz.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quartz
{
    public partial class ClearHistory : Form
    {
        private readonly WebView2 _webView;


        public ClearHistory(WebView2 webView)
        {
            if (webView == null)
                throw new ArgumentNullException(nameof(webView));

            _webView = webView;
            InitializeComponent();


            NewControlThemeChanger.ChangeTheme(this);
            ApplyVisualFinishing();
        }

        private async void ClearHistory_Load(object sender, EventArgs e)
        {
            if (_webView.CoreWebView2 == null)
            {
                SetSelectionEnabled(false);
                btnDelete.Enabled = false;
                return;
            }

            await RefreshDataSummaryAsync();
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            CoreWebView2BrowsingDataKinds dataKinds = GetSelectedDataKinds();
            if (dataKinds == 0 || _webView.CoreWebView2 == null)
                return;

            //SetBusy(true, "Clearing browsing data...");

            try
            {
                DateTime endTime = DateTime.Now;
                DateTime? startTime = GetStartTime(endTime);

                if (startTime.HasValue)
                {
                    await _webView.CoreWebView2.Profile.ClearBrowsingDataAsync(
                        dataKinds,
                        startTime.Value,
                        endTime);
                }
                else
                {
                    await _webView.CoreWebView2.Profile.ClearBrowsingDataAsync(dataKinds);
                }

                if (chkBrowsingHistory.Checked)
                {
                    Guid profileId = ProfileService.Current;
                    var historyService = new HistoryService();
                    historyService.DeleteProfileHistory(profileId, startTime, endTime);
                }

                lblStatus.Text = "Browsing data cleared.";
                //_isBusy = false;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception exception)
            {
                //SetBusy(false, "Quartz couldn't clear the selected data. " + exception.Message);
            }
        }

        #region Methods
        private DateTime? GetStartTime(DateTime endTime)
        {
            switch (cboTimeRange.SelectedIndex)
            {
                case 0:
                    return endTime.AddHours(-1);
                case 1:
                    return endTime.AddDays(-1);
                case 2:
                    return endTime.AddDays(-7);
                case 3:
                    return endTime.AddDays(-28);
                default:
                    return null;
            }
        }

        private CoreWebView2BrowsingDataKinds GetSelectedDataKinds()
        {
            CoreWebView2BrowsingDataKinds dataKinds = 0;

            if (chkBrowsingHistory.Checked)
                dataKinds |= CoreWebView2BrowsingDataKinds.BrowsingHistory;

            if (chkDownloadHistory.Checked)
                dataKinds |= CoreWebView2BrowsingDataKinds.DownloadHistory;

            if (chkCookies.Checked)
                dataKinds |= CoreWebView2BrowsingDataKinds.AllSite;

            if (chkCache.Checked)
                dataKinds |= CoreWebView2BrowsingDataKinds.DiskCache;

            if (chkPasswords.Checked)
                dataKinds |= CoreWebView2BrowsingDataKinds.PasswordAutosave;

            if (chkAutofill.Checked)
                dataKinds |= CoreWebView2BrowsingDataKinds.GeneralAutofill;

            return dataKinds;
        }

        private void SetSelectionEnabled(bool enabled)
        {
            cboTimeRange.Enabled = enabled;
            chkBrowsingHistory.Enabled = enabled;
            chkDownloadHistory.Enabled = enabled;
            chkCookies.Enabled = enabled;
            chkCache.Enabled = enabled;
            chkPasswords.Enabled = enabled;
            chkAutofill.Enabled = enabled;
        }

        private async Task RefreshDataSummaryAsync()
        {
            lblStatus.Text = "Calculating data for this profile...";

            UpdateHistorySummary();
            //await UpdateCookieSummaryAsync();
            //await UpdateCacheSummaryAsync();

            lblStatus.Text = "Only data from this Quartz profile will be cleared.";
        }

        private void UpdateHistorySummary()
        {
            try
            {
                DateTime endTime = DateTime.Now;
                DateTime? startTime = GetStartTime(endTime);
                List<HistoryModel> histories = new HistoryService().GetProfileHistoryFromRange(ProfileService.Current, startTime, endTime);
                //_webView.CoreWebView2.down
                if(histories.Count > 1)
                    lblBrowsingHistoryInfo.Text = $"From {new Uri(histories.FirstOrDefault().WebAddress).Host} + {histories.Count} sites";
                else
                    lblBrowsingHistoryInfo.Text = $"From {new Uri(histories.FirstOrDefault().WebAddress).Host}";

            }
            catch
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
        #endregion

        private void cboTimeRange_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateHistorySummary();
        }
    }
}
