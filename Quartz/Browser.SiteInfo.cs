using Quartz.Libs;
using System;

namespace Quartz
{
    public partial class Browser
    {
        private SiteInfoController _siteInfoController;
        private void InitializeSiteInfo()
        {
            // The button's layout is set in the Browser designer.
            btnSiteInformation.BackColor = txtWebAddress.BackColor;
            btnSiteInformation.ForeColor = txtWebAddress.ForeColor;
            _siteInfoController = new SiteInfoController(wvWebView1, btnSiteInformation);
            Disposed += Browser_SiteInfoDisposed;
        }

        private void Browser_SiteInfoDisposed(object sender, EventArgs e)
        {
            _siteInfoController.Dispose();
        }
    }
}
