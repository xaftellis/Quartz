using Microsoft.Web.WebView2.Core;
using Quartz.Controls;
using System;
using System.Drawing;

namespace Quartz
{
    public partial class Browser
    {
        private CoreWebView2 _buttonWebView;
        private enum RefreshButtonState { Refresh, Stop }
        private RefreshButtonState _refreshButtonState;
        private Image _refreshButtonImage, _stopButtonImage;

        private void SetRefreshButtonState(RefreshButtonState state)
        {
            _refreshButtonState = state;
            bool stop = state == RefreshButtonState.Stop;
            btnRefresh.Image = stop ? _stopButtonImage : _refreshButtonImage;
            btnRefresh.AccessibleName = stop ? "Stop" : "Refresh";
        }

        private void InitializeChromiumButtons()
        {
            ChromiumButton[] toolbar = { btnBack, btnForward, btnRefresh,
                btnDownload, btnAddFavourite, btnSettings };
            foreach (var button in toolbar)
            {
                // Quartz's theme selects the artwork. Use the shared Chromium
                // image renderer without replacing it with a ForeColor vector.
                button.VectorIcon = ChromiumIcon.None;
                button.Image = button.BackgroundImage;
                button.BackgroundImage = null;
                button.ImageAlign = ContentAlignment.MiddleCenter;
                button.FocusOnPress = false;
            }
            btnSiteInformation.FocusOnPress = false;
            btnBack.AccessibleName = "Back";
            btnForward.AccessibleName = "Forward";
            _refreshButtonImage = btnRefresh.Image;
            _stopButtonImage = Properties.Resources.Stop;
            SetRefreshButtonState(RefreshButtonState.Refresh);
            btnDownload.AccessibleName = "Downloads";
            btnAddFavourite.AccessibleName = "Add favourite";
            btnSettings.AccessibleName = "Settings";
            Disposed += (sender, e) =>
            {
                if (_buttonWebView == null) return;
                //_buttonWebView.IsDefaultDownloadDialogOpenChanged -= DownloadButtonStateChanged;
                _buttonWebView = null;
            };
        }

        private void InitializeDownloadButtonFeedback()
        {
            if (_buttonWebView != null)
                _buttonWebView.IsDefaultDownloadDialogOpenChanged -= DownloadButtonStateChanged;
            _buttonWebView = wvWebView1.CoreWebView2;
            _buttonWebView.IsDefaultDownloadDialogOpenChanged += DownloadButtonStateChanged;
            DownloadButtonStateChanged(this, EventArgs.Empty);
        }

        private void DownloadButtonStateChanged(object sender, object e)
        {
            if (!IsDisposed && !Disposing && _buttonWebView != null)
                btnDownload.IsActive = _buttonWebView.IsDefaultDownloadDialogOpen;
        }
    }
}
