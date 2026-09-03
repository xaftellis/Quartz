using Microsoft.Web.WebView2;
using Microsoft.Web.WebView2.WinForms;
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

        private void ClearHistory_Load(object sender, EventArgs e)
        {

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
    }
}
