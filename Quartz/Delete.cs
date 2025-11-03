using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
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
    public partial class Delete : Form
    {
        WebView2 webView2;
        public Delete(WebView2 WebView2)
        {
            InitializeComponent();
            webView2 = WebView2;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            CoreWebView2BrowsingDataKinds dataKinds = 0;
            DateTime startDate;
            DateTime endDate;

            if (chkBrowsingHistory.Checked)
                dataKinds |= CoreWebView2BrowsingDataKinds.BrowsingHistory;

            if (chkDownloadHistory.Checked)
                dataKinds |= CoreWebView2BrowsingDataKinds.DownloadHistory;

            if (chkCookies.Checked)
                dataKinds |= CoreWebView2BrowsingDataKinds.Cookies;

            startDate = DateTime.MinValue;
            endDate = DateTime.Now;

            webView2.CoreWebView2.Profile.ClearBrowsingDataAsync(dataKinds, startDate, endDate);
        }
    }
}
