using Quartz.Controls;
using Quartz.Libs;
using System;
using System.Drawing;

namespace Quartz
{
    public partial class Browser
    {
        private SiteInfoButton _siteInfoButton;
        private SiteInfoController _siteInfoController;
        private void InitializeSiteInfo()
        {
            // The tab now handles the favicon and loading animation.
            // Put the site-information button in their old address-bar position.
            picFavicon.Visible = false;
            wvLoadingProgress.Visible = false;
            Rectangle bounds = picFavicon.Bounds;
            int buttonPadding = (int)Math.Round(6f * DeviceDpi / 96f);
            bounds.Inflate(buttonPadding, buttonPadding);
            _siteInfoButton = new SiteInfoButton
            {
                Name = "btnSiteInformation",
                Bounds = bounds,
                Anchor = picFavicon.Anchor,
                BackColor = txtWebAddress.BackColor,
                ForeColor = txtWebAddress.ForeColor,
                TabIndex = picFavicon.TabIndex
            };
            pnlTop.Controls.Add(_siteInfoButton);
            _siteInfoButton.BringToFront();
            _siteInfoController = new SiteInfoController(wvWebView1, _siteInfoButton);
            Disposed += Browser_SiteInfoDisposed;
        }

        private void Browser_SiteInfoDisposed(object sender, EventArgs e)
        {
            _siteInfoController.Dispose();
        }
    }
}
