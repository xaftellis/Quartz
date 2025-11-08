using EasyTabs;
using ExCSS;
using Quartz.Controls;
using Quartz.Libs;
using Quartz.Services;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Quartz
{
    public partial class AppContainer : TitleBarTabs
    {
        public string _windowName = string.Empty;

        private string ToBgr(System.Drawing.Color c) => $"{c.B:X2}{c.G:X2}{c.R:X2}";

        [DllImport("DwmApi")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);

        const int DWWMA_CAPTION_COLOR = 35;
        const int DWWMA_BORDER_COLOR = 34;
        const int DWMWA_TEXT_COLOR = 36;

        public void CustomWindow(System.Drawing.Color captionColor, System.Drawing.Color fontColor, System.Drawing.Color borderColor, IntPtr handle)
        {
            IntPtr hWnd = handle;
            int[] caption = new int[] { int.Parse(ToBgr(captionColor), System.Globalization.NumberStyles.HexNumber) };
            DwmSetWindowAttribute(hWnd, DWWMA_CAPTION_COLOR, caption, 4);

            int[] font = new int[] { int.Parse(ToBgr(fontColor), System.Globalization.NumberStyles.HexNumber) };
            DwmSetWindowAttribute(hWnd, DWMWA_TEXT_COLOR, font, 4);

            int[] border = new int[] { int.Parse(ToBgr(borderColor), System.Globalization.NumberStyles.HexNumber) };
            DwmSetWindowAttribute(hWnd, DWWMA_BORDER_COLOR, border, 4);
        }
        public bool OverlayVisible
        {
            get => _overlay.Visible;
            set => _overlay.Visible = value;
        }

        public AppContainer()
        {
            InitializeComponent();

            ProfileService.LoadCurrentProfile();
            var theme = SettingsService.Get("Theme");
            Icon icon = Quartz.Properties.Resources.favicon;
            System.Drawing.Color barBackColor = System.Drawing.Color.White;
            System.Drawing.Color textForeColor = System.Drawing.Color.Black;
            System.Drawing.Color windowOutline = System.Drawing.Color.White;

            AeroPeekEnabled = false;

            if (theme == "light")
            {
                TabRenderer = new LightTabRenderer(this);

                barBackColor = System.Drawing.Color.FromArgb(222, 225, 230);
                textForeColor = System.Drawing.Color.Black;
                windowOutline = System.Drawing.Color.FromArgb(219, 220, 221);
            }
            else if (theme == "dark")
            {
                TabRenderer = new DarkTabRenderer(this);

                barBackColor = System.Drawing.Color.FromArgb(88, 88, 88);
                textForeColor = System.Drawing.Color.FromArgb(195, 195, 195);
                windowOutline = System.Drawing.Color.FromArgb(88, 88, 88);
            }
            else if (theme == "black")
            {
                TabRenderer = new BlackTabRenderer(this);

                barBackColor = System.Drawing.Color.Black;
                textForeColor = System.Drawing.Color.White;
                windowOutline = System.Drawing.Color.FromArgb(128, 128, 128);
            }
            else if (theme == "aqua")
            {
                TabRenderer = new AquaTabRenderer(this);

                barBackColor = System.Drawing.Color.Blue;
                textForeColor = System.Drawing.Color.Aqua;
                windowOutline = System.Drawing.Color.Blue;
            }
            else if (theme == "xmas")
            {
                TabRenderer = new XmasTabRenderer(this);

                barBackColor = System.Drawing.Color.Lime;
                textForeColor = System.Drawing.Color.Red;
                windowOutline = System.Drawing.Color.Lime;
            }
            else
            {
                TabRenderer = new ChromeTabRenderer(this);
            }

            Icon = FaviconHelper.GetFullResDefaultFavicon();

            CustomWindow(barBackColor, textForeColor, windowOutline, Handle);

            ContextMenuProvider._contextMenuStripNormal = new DefaultContextMenu();
            ContextMenuProvider._contextMenuStripTab = new TabContextMenu();
        }

        public override TitleBarTab CreateTab()
        {
            Browser browser = new Browser(null, false);
            browser.InitializeTab();

            return new TitleBarTab(this)
            {
                Content = browser
            };
        }

        private void AppContainer_Load(object sender, EventArgs e)
        {
            this.TabSelected += AppContainer_TabSelected;
        }

        private void AppContainer_TabSelected(object sender, TitleBarTabEventArgs e)
        {
            Browser browser = (Browser)SelectedTab.Content;
            browser.tabbedApp = (AppContainer)browser.Parent;

            if(_windowName == string.Empty)
                 this.Text = e.Tab.Content.Text;

            foreach (var item in Tabs)
            {
                try
                {
                    Browser form = (Browser)item.Content;

                    if (item.Active)
                    {
                        form.LoadFavourites();
                        form.notifyIcon1.Visible = true;

                        if (form.wvWebView1?.CoreWebView2 != null && form.WasDownloadDialogActive)
                        {
                            form.wvWebView1.CoreWebView2.OpenDefaultDownloadDialog();
                            form.WasDownloadDialogActive = false; // reset after opening
                        }
                    }
                    else
                    {
                        form.notifyIcon1.Visible = false;

                        if (form.wvWebView1?.CoreWebView2 != null && form.wvWebView1.CoreWebView2.IsDefaultDownloadDialogOpen)
                        {
                            form.wvWebView1.CoreWebView2.CloseDefaultDownloadDialog();
                            form.WasDownloadDialogActive = true; // remember that it was open
                        }
                    }
                }
                catch
                {

                }
            }
        }

        private void AppContainer_LocationChanged(object sender, EventArgs e)
        {
            foreach (var item in Tabs.ToList())
            {
                try
                {
                    Browser form = (Browser)item.Content;
                    if (item.Active == true)
                        form.RestoreDownloadDialog();
                }
                catch { }
            }
        }

        private void AppContainer_SizeChanged(object sender, EventArgs e)
        {
            if (MinimumSize != new Size(816, 489))
            {
                Size = new Size(816, 489);
                MinimumSize = new Size(816, 489);
            }
        }
    }
}
