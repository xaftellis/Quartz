using EasyTabs;
using Microsoft.SqlServer.Server;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Quartz.Libs;
using Quartz.Models;
using Quartz.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Remoting.Channels;
using System.Security.AccessControl;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.ModelBinding;
using System.Web.UI.WebControls.WebParts;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Markup;
using System.Windows.Markup.Localizer;
using Win32Interop.Structs;

namespace Quartz
{
    public partial class Browser : Form
    {
        #region Declarations
        public AppContainer tabbedApp;

        //private Omnibox.OmniBoxUI suggestionsClass;
        private Omnibox.OmniBoxTheme themeClass;

        public bool _newtab = false;
        public string _tabAddress = string.Empty;
        private Settings _settings;
        int loadnum = 0;
        FormWindowState _windowState;
        Size size;
        Point point;

        public HttpClient httpClient = new HttpClient();
        //ListView lstSuggestions = new ListView();

        public bool WasDownloadDialogActive { get; set; } = false;

        private string ToBgr(Color c) => $"{c.B:X2}{c.G:X2}{c.R:X2}";
        [DllImport("DwmApi")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);

        const int DWWMA_CAPTION_COLOR = 35;
        const int DWWMA_BORDER_COLOR = 34;
        const int DWMWA_TEXT_COLOR = 36;
        public void CustomWindow(Color captionColor, Color fontColor, Color borderColor, IntPtr handle)
        {
            IntPtr hWnd = handle;
            //Change caption color
            int[] caption = new int[] { int.Parse(ToBgr(captionColor), System.Globalization.NumberStyles.HexNumber) };
            DwmSetWindowAttribute(hWnd, DWWMA_CAPTION_COLOR, caption, 4);
            //Change font color
            int[] font = new int[] { int.Parse(ToBgr(fontColor), System.Globalization.NumberStyles.HexNumber) };
            DwmSetWindowAttribute(hWnd, DWMWA_TEXT_COLOR, font, 4);
            //Change border color
            int[] border = new int[] { int.Parse(ToBgr(borderColor), System.Globalization.NumberStyles.HexNumber) };
            DwmSetWindowAttribute(hWnd, DWWMA_BORDER_COLOR, border, 4);
        }

        private bool fullScreen = false;
        [DefaultValue(false)]
        public bool FullScreen
        {
            get { return fullScreen; }
            set
            {
                fullScreen = value;
                if (value)
                {

                    _windowState = tabbedApp.WindowState;
                    size = tabbedApp.Size;
                    point = tabbedApp.Location;
                    tabbedApp.OverlayVisible = false;
                    tabbedApp.WindowState = FormWindowState.Normal;
                    tabbedApp.FormBorderStyle = FormBorderStyle.None;
                    tabbedApp.WindowState = FormWindowState.Maximized;
                    pnlTop.Visible = false;
                    pnlDivider.Visible = false;
                    tabbedApp.TopMost = true;
                }
                else
                {
                    if (_windowState != FormWindowState.Maximized)
                    {
                        tabbedApp.WindowState = _windowState;
                        tabbedApp.Size = size;
                        Location = point;
                    }
                    else
                    {
                        tabbedApp.WindowState = FormWindowState.Maximized;
                    }
                    tabbedApp.OverlayVisible = true;
                    tabbedApp.FormBorderStyle = FormBorderStyle.Sizable;
                    pnlTop.Visible = true;
                    pnlDivider.Visible = true;
                    tabbedApp.TopMost = false;
                    wvWebView1.Focus();
                }
            }
        }
        #endregion

        #region Properties
        public TitleBarTabs ParentTabs => (ParentForm as TitleBarTabs);
        #endregion

        #region Constructors
        public Browser(string address, bool newtabrequest)
        {
            InitializeComponent();
            _newtab = newtabrequest;
            _tabAddress = address;
            //lstSuggestions.View = View.Details;
            //lstSuggestions.HeaderStyle = ColumnHeaderStyle.None;
            //lstSuggestions.FullRowSelect = true;
            //lstSuggestions.MultiSelect = false;
            //lstSuggestions.HideSelection = false;
            //lstSuggestions.HotTracking = true;
            //lstSuggestions.OwnerDraw = false;

            //// Add one column to simulate a ListBox
            //lstSuggestions.Columns.Add(String.Empty, lstSuggestions.Width);
            //lstSuggestions.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular);
            //lstSuggestions.Visible = false;
            //this.Controls.Add(lstSuggestions);

           //suggestionsClass = new Omnibox.OmniBoxUI(this, txtWebAddress);
            themeClass = new Omnibox.OmniBoxTheme(txtWebAddress);
        }

        #endregion

        #region Private Methods
        private void UpdateTitleWithEvent(string message)
        {
            string currentDocumentTitle = wvWebView1?.CoreWebView2?.DocumentTitle ?? "Uninitialized";
            this.Text = currentDocumentTitle + " (" + message + ")";
        }
        #endregion

        #region Public Methods

        public void InitializeTab()
        {
            var theme = SettingsService.Get("Theme");

            Icon = FaviconHelper.GetDefaultFavicon16();

            Text = "New Tab";
        }


        public void LoadTheme()
        {
            //IMAGES
            Image backImage = Quartz.Properties.Resources.Left;
            Image forwardImage = Quartz.Properties.Resources.Right;
            Image houseImage = Quartz.Properties.Resources.icon_House;
            Image refreshImage = Quartz.Properties.Resources.Refresh;
            Image stopImage = Quartz.Properties.Resources.Stop;
            Image downloadImage = Quartz.Properties.Resources.download;
            Image favImage = Quartz.Properties.Resources.icon_Fav;
            Image settingsImage = Quartz.Properties.Resources.icon_Setting;
            Image leftImage = Quartz.Properties.Resources.UrlBoxLeftWhite;
            Image rightImage = Quartz.Properties.Resources.UrlBoxRightWhite;

            //COLORS
            Color mouseOver = Color.FromArgb(242, 242, 242);
            Color PanelbackColor = Color.FromArgb(200, 200, 200);
            Color PanelforeColor = Color.Black;
            Color dividerColor = Color.FromArgb(219, 220, 221);

            string theme = SettingsService.Get("Theme");

            if (theme == "dark")
            {
                //IMAGES
                backImage = Quartz.Properties.Resources.DLeft;
                forwardImage = Quartz.Properties.Resources.DRight;
                houseImage = Quartz.Properties.Resources.Dicon_House;
                refreshImage = Quartz.Properties.Resources.DRefresh;
                stopImage = Quartz.Properties.Resources.DStop;
                downloadImage = Quartz.Properties.Resources.Ddownload;
                favImage = Quartz.Properties.Resources.Dicon_Fav;
                settingsImage = Quartz.Properties.Resources.Dicon_Setting;
                leftImage = Quartz.Properties.Resources.DUrlBoxLeftWhite;
                rightImage = Quartz.Properties.Resources.DUrlBoxRightWhite;

                //COLORS
                mouseOver = Color.FromArgb(64, 64, 64);
                PanelbackColor = Color.FromArgb(88, 88, 88);
                PanelforeColor = Color.FromArgb(195, 195, 195);
                dividerColor = Color.FromArgb(88, 88, 88);
            }
            if (theme == "black")
            {
                //IMAGES
                backImage = Quartz.Properties.Resources.Black_Left;
                forwardImage = Quartz.Properties.Resources.Black_Right;
                houseImage = Quartz.Properties.Resources.Black_House;
                refreshImage = Quartz.Properties.Resources.Black_Refresh;
                stopImage = Quartz.Properties.Resources.Black_Stop;
                downloadImage = Quartz.Properties.Resources.Black_Download;
                favImage = Quartz.Properties.Resources.Black_Fav;
                settingsImage = Quartz.Properties.Resources.Black_Settings;
                leftImage = Quartz.Properties.Resources.Black_URL_Left;
                rightImage = Quartz.Properties.Resources.Black_URL_Right;

                //COLORS
                mouseOver = Color.FromArgb(18, 18, 18);
                PanelbackColor = Color.White;
                PanelforeColor = Color.Black;
                dividerColor = Color.FromArgb(128, 128, 128);
            }
            if (theme == "aqua")
            {
                //IMAGES
                backImage = Quartz.Properties.Resources.Aqua_Icon_Left;
                forwardImage = Quartz.Properties.Resources.Aqua_Icon_Right;
                houseImage = Quartz.Properties.Resources.Aqua_Home;
                refreshImage = Quartz.Properties.Resources.Aqua_Refresh;
                stopImage = Quartz.Properties.Resources.Aqua_Stop;
                downloadImage = Quartz.Properties.Resources.Aqua_Download;
                favImage = Quartz.Properties.Resources.Aqua_Fav;
                settingsImage = Quartz.Properties.Resources.Aqua_Settings;
                leftImage = Quartz.Properties.Resources.UrlBoxLeftWhiteAqua;
                rightImage = Quartz.Properties.Resources.UrlBoxRightAqua;

                //COLORS
                mouseOver = Color.FromArgb(0, 238, 238);
                PanelbackColor = Color.Blue;
                PanelforeColor = Color.Aqua;
                dividerColor = Color.Blue;
            }
            if (theme == "xmas")
            {
                //IMAGES
                backImage = Quartz.Properties.Resources.XLeft;
                forwardImage = Quartz.Properties.Resources.XRight;
                houseImage = Quartz.Properties.Resources.Xicon_House;
                refreshImage = Quartz.Properties.Resources.XRefresh;
                stopImage = Quartz.Properties.Resources.XStop;
                downloadImage = Quartz.Properties.Resources.Xdownload;
                favImage = Quartz.Properties.Resources.Xicon_Fav;
                settingsImage = Quartz.Properties.Resources.Xicon_Setting;
                leftImage = Quartz.Properties.Resources.XUrlBoxLeft;
                rightImage = Quartz.Properties.Resources.XUrlBoxRight;

                //COLORS
                mouseOver = Color.FromArgb(255, 50, 50);
                PanelbackColor = Color.Lime;
                PanelforeColor = Color.Red;
                dividerColor = Color.Lime;
            }


            //BUTTONS
            btnBack.BackgroundImage = backImage;
            btnBack.FlatAppearance.MouseOverBackColor = mouseOver;
            btnBack.FlatAppearance.MouseDownBackColor = mouseOver;
            btnBack.FlatAppearance.BorderColor = PanelforeColor;

            btnForward.BackgroundImage = forwardImage;
            btnForward.FlatAppearance.MouseOverBackColor = mouseOver;
            btnForward.FlatAppearance.MouseDownBackColor = mouseOver;
            btnForward.FlatAppearance.BorderColor = PanelforeColor;

            //btnHome.BackgroundImage = houseImage;
            //btnHome.FlatAppearance.MouseOverBackColor = mouseOver;
            //btnHome.FlatAppearance.MouseDownBackColor = mouseOver;
            //btnHome.FlatAppearance.BorderColor = PanelforeColor;

            btnRefresh.BackgroundImage = refreshImage;
            btnRefresh.FlatAppearance.MouseOverBackColor = mouseOver;
            btnRefresh.FlatAppearance.MouseDownBackColor = mouseOver;
            btnRefresh.FlatAppearance.BorderColor = PanelforeColor;

            btnStop.BackgroundImage = stopImage;
            btnStop.FlatAppearance.MouseOverBackColor = mouseOver;
            btnStop.FlatAppearance.MouseDownBackColor = mouseOver;
            btnStop.FlatAppearance.BorderColor = PanelforeColor;

            btnDownload.BackgroundImage = downloadImage;
            btnDownload.FlatAppearance.MouseOverBackColor = mouseOver;
            btnDownload.FlatAppearance.MouseDownBackColor = mouseOver;
            btnDownload.FlatAppearance.BorderColor = PanelforeColor;

            btnAddFavourite.BackgroundImage = favImage;
            btnAddFavourite.FlatAppearance.MouseOverBackColor = mouseOver;
            btnAddFavourite.FlatAppearance.MouseDownBackColor = mouseOver;
            btnAddFavourite.FlatAppearance.BorderColor = PanelforeColor;

            btnSettings.BackgroundImage = settingsImage;
            btnSettings.FlatAppearance.MouseOverBackColor = mouseOver;
            btnSettings.FlatAppearance.MouseDownBackColor = mouseOver;
            btnSettings.FlatAppearance.BorderColor = PanelforeColor;

            //PICTUREBOXS
            UrlLeft.BackgroundImage = leftImage;

            UrlBox.BackColor = PanelbackColor;

            UrlRight.BackgroundImage = rightImage;

            //PANNELS
            pnlDivider.BackColor = PanelbackColor;

            picFavicon.BackColor = PanelbackColor;

            pnlDivider.BackColor = dividerColor;

            //TEXTBOXS
            txtWebAddress.BackColor = PanelbackColor;
            txtWebAddress.ForeColor = PanelforeColor;

            //MODERN THEME SYSTEM
            NewControlThemeChanger.ChangeControlTheme(this);
            NewControlThemeChanger.ChangeControlTheme(SettingsMenuStrip);
            NewControlThemeChanger.ChangeControlTheme(mnuExperts);
            NewControlThemeChanger.ChangeControlTheme(mnuHistory);
            NewControlThemeChanger.ChangeControlTheme(mnuMenu);
            NewControlThemeChanger.ChangeControlTheme(mnuTabs);
            NewControlThemeChanger.ChangeControlTheme(mnuUserData);
            NewControlThemeChanger.ChangeControlTheme(mnuDownloadsDropDown);
            NewControlThemeChanger.ChangeControlTheme(mnuHistory);
            NewControlThemeChanger.ChangeControlTheme(mnuUserData);
            NewControlThemeChanger.ChangeControlTheme(mnuSearch);
            NewControlThemeChanger.ChangeControlTheme(zoomToolStrip);

        }

        public void ChangeTheme(string theme)
        {
            SettingsService.Set("Theme", theme);
            LoadTheme();
        }

        public void LoadFavourites()
        {
            if (SettingsService.Get("showFavouriteIcon") == "true")
            {
                pnlFavourites.Controls.Clear();

                var service = new FavouriteService();

                foreach (var favourite in service.All().OrderBy(f => f.Index).ToList())
                {
                    var button = new Button
                    {
                        Text = "      " + favourite.Name,
                        Tag = favourite.WebAddress,
                        ContextMenuStrip = mnuMenu,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        ImageAlign = ContentAlignment.MiddleLeft,
                        TextAlign = ContentAlignment.MiddleCenter,
                        MaximumSize = new Size(0, 23),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 8)
                    };

                    Uri address = new Uri(favourite.WebAddress);

                    NewControlThemeChanger.ChangeControlTheme(button);

                    button.Image = FaviconHelper.GetFaviconFileExternalAsImage(address.AbsoluteUri);

                    var toolTip = new ToolTip();
                    toolTip.SetToolTip(button, favourite.Name + Environment.NewLine + favourite.WebAddress);

                    pnlFavourites.Controls.Add(button);
                    button.MouseDown += Button_MouseDown;
                    button.MouseMove += Button_MouseMove;
                    button.MouseUp += Button_MouseUp;
                    button.Click += btnGotoFavourite_Click;
                }
            }
            else
            {
                pnlFavourites.Controls.Clear();

                var service = new FavouriteService();

                foreach (var favourite in service.All().OrderBy(f => f.Index).ToList())
                {
                    var button = new Button
                    {
                        Text = favourite.Name,
                        Tag = favourite.WebAddress,
                        ContextMenuStrip = mnuMenu,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        ImageAlign = ContentAlignment.MiddleLeft,
                        TextAlign = ContentAlignment.MiddleCenter,
                        MaximumSize = new Size(0, 23),
                        AutoSize = true,
                    };

                    Uri address = new Uri(favourite.WebAddress);

                    NewControlThemeChanger.ChangeControlTheme(button);

                    var toolTip = new ToolTip();
                    toolTip.SetToolTip(button, favourite.Name + Environment.NewLine + favourite.WebAddress);

                    pnlFavourites.Controls.Add(button);
                    button.MouseDown += Button_MouseDown;
                    button.MouseMove += Button_MouseMove;
                    button.MouseUp += Button_MouseUp;
                    button.Click += btnGotoFavourite_Click;
                }
            }
        }

        bool mouseReleased = false;
        Point mouseDownLocation;
        bool isDragging = false;
        Button draggedButton = null;
        int originalButtonIndex = 0;

        private void Button_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseReleased = false;
                isDragging = false;
                draggedButton = sender as Button;
                mouseDownLocation = e.Location;
                originalButtonIndex = pnlFavourites.Controls.GetChildIndex(sender as Button);
            }
        }

        private void Button_MouseMove(object sender, MouseEventArgs e)
        {
            if (draggedButton == null)
                return;

            if (!mouseReleased && (Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left)
            {
                if (!isDragging)
                {
                    // Get the current mouse location in screen coordinates
                    Point currentScreenPos = draggedButton.PointToScreen(e.Location);

                    // Get the button's rectangle in screen coordinates
                    Rectangle buttonBounds = draggedButton.RectangleToScreen(draggedButton.ClientRectangle);

                    // Start dragging only if mouse has moved outside the button
                    if (!buttonBounds.Contains(currentScreenPos))
                    {
                        isDragging = true;
                    }
                }

                if (isDragging)
                {
                    int currentIndex = pnlFavourites.Controls.GetChildIndex(draggedButton);
                    int newIndex = GetNewButtonIndex(draggedButton, Cursor.Position);

                    if (newIndex != currentIndex)
                    {
                        pnlFavourites.Controls.SetChildIndex(draggedButton, newIndex);
                    }
                }

            }
        }

        private async void Button_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                var button = (System.Windows.Forms.Button)sender;

                var favourite = new FavouriteService()
                    .Get(button.Text.Trim());

                var browser = new Browser(favourite.WebAddress, true);
                browser.InitializeTab();

                var newTab = new TitleBarTab(ParentTabs) { Content = browser };

                void AddTab()
                {
                    int index = ParentTabs.SelectedTabIndex + 1;
                    ParentTabs.Tabs.Insert(index, newTab);
                    ParentTabs.SelectedTabIndex = index;
                    ParentTabs.RedrawTabs();
                }

                if (ParentTabs.InvokeRequired)
                    ParentTabs.Invoke(new Action(AddTab));
                else
                    AddTab();

                // Instant UI activation (0–1ms)
                await Task.Yield();
            }
            else if (e.Button == MouseButtons.Left && isDragging)
            {
                mouseReleased = true;
                isDragging = false;
                draggedButton = null;

                FavouriteService favouriteService = new FavouriteService();
                foreach (Button button in pnlFavourites.Controls)
                {
                    favouriteService.Get(button.Text.Replace("      ", "")).Index = pnlFavourites.Controls.GetChildIndex(button);
                    favouriteService.SaveChanges();
                }

                int newButtonIndex = pnlFavourites.Controls.GetChildIndex(sender as Button);
                if (originalButtonIndex != newButtonIndex)
                {
                    SettingsService.Set("sortFavouritesBy", "custom");
                }
            }
        }
        private void btnGotoFavourite_Click(object sender, EventArgs e)
        {
            if (sender is Button && !isDragging)
            {
                var button = (Button)sender;

                var service = new FavouriteService();
                var favourite = service.Get(button.Text.Replace("      ", ""));
                if (favourite != null)
                {
                    SetSource(favourite.WebAddress);
                }
            }
        }

        private int GetNewButtonIndex(Button draggedButton, Point screenMousePosition)
        {
            Point panelMousePoint = pnlFavourites.PointToClient(screenMousePosition);

            var buttons = pnlFavourites.Controls.Cast<Control>().OfType<Button>()
                .Where(b => b != draggedButton)
                .OrderBy(b => b.Left)
                .ToList();

            for (int i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];
                int centerX = button.Left + button.Width / 2;

                if (panelMousePoint.X < centerX)
                {
                    // Mouse is to the left of this button’s center → insert before
                    return i;
                }
            }

            // If we're past all buttons → insert at end
            return buttons.Count;
        }

        public void SetSource(string url)
        {
            SetSource(new Uri(url));
        }

        public void SetSource(Uri uri)
        {
            if (wvWebView1.Source != uri)
            {
                wvWebView1.Source = uri;
            }
            else
            {
                wvWebView1.CoreWebView2.Reload();
            }
        }

        public string GetAppPath()
        {
            return Application.StartupPath;
        }

        public string GetCachePath()
        {
            var path = GetAppPath() + @"\UserData\cache\";

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        public string GetHomeUrl()
        {
            string engine = SettingsService.Get("SearchEngine");
            string theme = SettingsService.Get("Theme");
            bool useDefaultHome = SettingsService.Get("DefaultHomePage") == "true";

            // Helper for themed pages
            string ThemePage(string name) => $"https://quartz.com/{theme}/{name}.html";

            switch (engine)
            {
                case "bing":
                    return useDefaultHome ? ThemePage("Bing") : "https://www.bing.com/";

                case "yahoo":
                    return useDefaultHome ? ThemePage("Yahoo") : "https://search.yahoo.com/";

                case "duckduckgo":
                    return useDefaultHome ? ThemePage("DuckDuckGo") : "https://duckduckgo.com/";

                case "wikipedia":
                    return "https://www.wikipedia.org/";

                case "netflix":
                    return useDefaultHome ? ThemePage("Netflix") : "https://www.netflix.com/";

                case "youtube":
                    return useDefaultHome ? ThemePage("YouTube") : "https://www.youtube.com/";

                case "googlemaps":
                    return useDefaultHome ? ThemePage("Google Maps") : "https://www.google.com/maps";

                case "ebay":
                    return "https://www.ebay.com/";

                case "amazom": // your spelling
                    return "https://www.amazon.com/";

                case "ecosia":
                    return useDefaultHome ? ThemePage("Ecosia") : "https://www.ecosia.org/";

                case "google":
                    return useDefaultHome ? ThemePage("Google") : "https://www.google.com/";

                default:
                    // Fallback to Google
                    return useDefaultHome ? ThemePage("Google") : "https://www.google.com/";
            }
        }
        public void UpdateFavBar()
        {
            bool showFavSetting = SettingsService.Get("showFavouritesBar") == "true";

            // --- Safely check Source ---
            string currentUrl = wvWebView1?.Source?.ToString() ?? "";
            bool isHome = currentUrl == GetHomeUrl();

            bool shouldShow = showFavSetting || isHome;

            // --- No favourites? Force hidden ---
            if (pnlFavourites.Controls.Count == 0)
            {
                pnlFavourites.Visible = false;
                pnlTop.Height = 49;
                return;
            }

            // --- If user disabled bar AND not home, hide it ---
            if (!shouldShow)
            {
                pnlFavourites.Visible = false;
                pnlTop.Height = 49;
                return;
            }

            // --- Show favourites bar ---
            pnlFavourites.Visible = true;

            if (pnlFavourites.HorizontalScroll.Visible)
            {
                pnlFavourites.Height = 47;
                pnlTop.Height = 93;
            }
            else
            {
                pnlFavourites.Height = 31;
                pnlTop.Height = 80;
            }
        }



        public async void RestoreDownloadDialog()
        {
            if (wvWebView1.CoreWebView2 != null)
            {
                if (wvWebView1.CoreWebView2.IsDefaultDownloadDialogOpen)
                {
                    wvWebView1.CoreWebView2.CloseDefaultDownloadDialog();
                    await Task.Delay(100);
                    wvWebView1.CoreWebView2.OpenDefaultDownloadDialog();
                }
            }
        }

        public void Shortcuts(bool enable)
        {
            if (enable)
            {
                SettingsMenuStrip.Enabled = true;
                mnuExperts.Enabled = true;
            }
            else
            {
                SettingsMenuStrip.Enabled = false;
                mnuExperts.Enabled = false;
            }
        }
        #endregion

        #region Events
        private async void Browser_Load(object sender, EventArgs e)
        {
            //bugfix to the 0,0 location of the settings context menu when first opened.
            SettingsMenuStrip.Opening -= SettingsMenuStrip_Opening;
            SettingsMenuStrip.Show(this, new Point(-10000, -10000));
            SettingsMenuStrip.Close();              // closes it immediately
            SettingsMenuStrip.Opening += SettingsMenuStrip_Opening;

            // Force the underlying window handle to be created early
            var h = SettingsMenuStrip.Handle;

            tabbedApp = (AppContainer)Parent;
            var profile = Program.profileService.GetDefault();

            if (profile == null)
            {
                MainSettingsService.Set("RunBrowser", "false");
            }

            if (!Directory.Exists(GetAppPath() + @"\UserData\WebView2\"))
            {
                Directory.CreateDirectory(GetAppPath() + @"\UserData\WebView2\");
            }

            //user webview
            CoreWebView2Environment env;
            CoreWebView2ControllerOptions options;

            try
            {
                env = await CoreWebView2Environment.CreateAsync(null, GetAppPath() + @"\UserData\WebView2\", null);
                options = env.CreateCoreWebView2ControllerOptions();
                options.ProfileName = ProfileService.Current.ToString();
                options.IsInPrivateModeEnabled = Program.profileService.Get(ProfileService.Current).isDisposable;

                //sys webview
                var sysenv = await CoreWebView2Environment.CreateAsync(null, GetAppPath() + @"\UserData\WebView2\", null);
                var sysoptions = sysenv.CreateCoreWebView2ControllerOptions();

                if (wvWebView1.CoreWebView2 == null)
                {
                    await wvWebView1.EnsureCoreWebView2Async(env, options);
                }

                if (wvLoadingProgress.CoreWebView2 == null)
                {
                    await wvLoadingProgress.EnsureCoreWebView2Async(sysenv, sysoptions);
                }

                if (Program.profileService.Get(ProfileService.Current).isDisposable)
                {
                    Program.profileService.Get(ProfileService.Current).endSession = true;
                    Program.profileService.SaveChanges();
                }
            }
            catch (Exception f)
            {
                if (f.HResult == -2146233088)
                {
                    var msg = MessageBox.Show("Couldn't find a compatible Webview2 Runtime installation to run quartz.", "Something Went Wrong", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    if (msg == DialogResult.OK)
                    {
                        System.Diagnostics.Process.Start("https://developer.microsoft.com/en-us/microsoft-edge/webview2/");
                        Power.Shutdown();
                    }
                }

            }

            // Assuming 'webView' is your WebView2 control
            //wvWebView1.CoreWebView2.Settings.UserAgent = "Mozilla/1.0 (compatible; Mosaic/1.0; Windows 3.1)";


            wvWebView1.CoreWebView2.SetVirtualHostNameToFolderMapping("quartz.com", Application.StartupPath + @"\assets\quartz.com\", CoreWebView2HostResourceAccessKind.Allow);

            if (_newtab)
            {
                SetSource(_tabAddress);
            }
            else
            {
                SetSource(GetHomeUrl());
            }

            wvWebView1.CoreWebView2.ContainsFullScreenElementChanged += (obj, args) =>
            {
                this.FullScreen = wvWebView1.CoreWebView2.ContainsFullScreenElement;
            };

            UpdateFavBar();
            LoadTheme();
            LoadFavourites();
            btnStop.Visible = false;
            wvLoadingProgress.Visible = false;

            if (SettingsService.Get("HiddenPDFNone") != "true")
            {
                Quartz.Models.SettingModel[] items =
                {
                    SettingsService.GetModel("HiddenPDFSave"),
                    SettingsService.GetModel("HiddenPDFPrint"),
                    SettingsService.GetModel("HiddenPDFSaveAs"),
                    SettingsService.GetModel("HiddenPDFZoomIn"),
                    SettingsService.GetModel("HiddenPDFZoomOut"),
                    SettingsService.GetModel("HiddenPDFRotate"),
                    SettingsService.GetModel("HiddenPDFFitPage"),
                    SettingsService.GetModel("HiddenPDFPageLayout"),
                    SettingsService.GetModel("HiddenPDFBookmarks"),
                    SettingsService.GetModel("HiddenPDFPageSelector"),
                    SettingsService.GetModel("HiddenPDFSearch"),
                    SettingsService.GetModel("HiddenPDFFullScreen"),
                    SettingsService.GetModel("HiddenPDFMoreSettings"),
                    };

                wvWebView1.CoreWebView2.Settings.HiddenPdfToolbarItems = CoreWebView2PdfToolbarItems.None;
                foreach (SettingModel setting in items)
                {
                    if (setting.Value != null)
                    {
                        if (setting.Value == "true")
                        {
                            wvWebView1.CoreWebView2.Settings.HiddenPdfToolbarItems |= (CoreWebView2PdfToolbarItems)Enum.Parse(typeof(CoreWebView2PdfToolbarItems), setting.Name.Replace("HiddenPDF", ""));
                        }
                    }
                }
            }
            else
            {
                wvWebView1.CoreWebView2.Settings.HiddenPdfToolbarItems = CoreWebView2PdfToolbarItems.None;
            }

            if (SettingsService.Get("MemoryUsage") == "low")
            {
                wvWebView1.CoreWebView2.MemoryUsageTargetLevel = CoreWebView2MemoryUsageTargetLevel.Low;
                wvLoadingProgress.CoreWebView2.MemoryUsageTargetLevel = CoreWebView2MemoryUsageTargetLevel.Low;

            }
            else
            {
                wvWebView1.CoreWebView2.MemoryUsageTargetLevel = CoreWebView2MemoryUsageTargetLevel.Normal;
                wvLoadingProgress.CoreWebView2.MemoryUsageTargetLevel = CoreWebView2MemoryUsageTargetLevel.Low;
            }

            if (SettingsService.Get("Zoom") != null)
            {
                wvWebView1.ZoomFactor = Convert.ToDouble(SettingsService.Get("Zoom"));
            }

            switch (SettingsService.Get("DownloadAlignment"))
            {
                case "TopLeft":
                    wvWebView1.CoreWebView2.DefaultDownloadDialogCornerAlignment = CoreWebView2DefaultDownloadDialogCornerAlignment.TopLeft;
                    break;

                case "TopRight":
                    wvWebView1.CoreWebView2.DefaultDownloadDialogCornerAlignment = CoreWebView2DefaultDownloadDialogCornerAlignment.TopRight;
                    break;

                case "BottomLeft":
                    wvWebView1.CoreWebView2.DefaultDownloadDialogCornerAlignment = CoreWebView2DefaultDownloadDialogCornerAlignment.BottomLeft;
                    break;

                case "BottomRight":
                    wvWebView1.CoreWebView2.DefaultDownloadDialogCornerAlignment = CoreWebView2DefaultDownloadDialogCornerAlignment.BottomRight;
                    break;

                default:
                    wvWebView1.CoreWebView2.DefaultDownloadDialogCornerAlignment = CoreWebView2DefaultDownloadDialogCornerAlignment.TopRight;
                    break;
            }

            switch (SettingsService.Get("TrackingPreventionLevel"))
            {
                case "none":
                    wvWebView1.CoreWebView2.Profile.PreferredTrackingPreventionLevel = CoreWebView2TrackingPreventionLevel.None;
                    break;

                case "basic":
                    wvWebView1.CoreWebView2.Profile.PreferredTrackingPreventionLevel = CoreWebView2TrackingPreventionLevel.Basic;
                    break;

                case "balanced":
                    wvWebView1.CoreWebView2.Profile.PreferredTrackingPreventionLevel = CoreWebView2TrackingPreventionLevel.Balanced;
                    break;

                case "strict":
                    wvWebView1.CoreWebView2.Profile.PreferredTrackingPreventionLevel = CoreWebView2TrackingPreventionLevel.Strict;
                    break;
            }

            wvWebView1.CoreWebView2.Settings.IsBuiltInErrorPageEnabled = false;
            //wvLoadingProgress.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            wvWebView1.CoreWebView2.Settings.IsGeneralAutofillEnabled = SettingsService.Get("IsGeneralAutofillEnabled") == "true";
            wvWebView1.CoreWebView2.Settings.IsPasswordAutosaveEnabled = SettingsService.Get("IsPasswordAutosaveEnabled") == "true";
            wvWebView1.CoreWebView2.Settings.IsZoomControlEnabled = SettingsService.Get("IsZoomControlEnabled") == "true";
            wvWebView1.CoreWebView2.Settings.IsPinchZoomEnabled = SettingsService.Get("IsPinchZoomEnabled") == "true";
            wvWebView1.CoreWebView2.Settings.AreDevToolsEnabled = SettingsService.Get("AreDevToolsEnabled") == "true";
            wvWebView1.CoreWebView2.Settings.IsSwipeNavigationEnabled = SettingsService.Get("IsSwipeNavigationEnabled") == "true";
            wvWebView1.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = SettingsService.Get("AreBrowserAcceleratorKeysEnabled") == "true";
            wvWebView1.CoreWebView2.Settings.IsScriptEnabled = SettingsService.Get("IsScriptEnabled") == "true";
            wvWebView1.CoreWebView2.Settings.IsStatusBarEnabled = SettingsService.Get("IsStatusBarEnabled") == "true";

            notifyIcon1.Text = "Quartz v2.2.0 RC1";
            notifyIcon1.Icon = FaviconHelper.GetFullResDefaultFaviconWithoutCustomFavicon();
            notifyIcon1.ContextMenuStrip = SettingsMenuStrip;
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Shortcuts(false);
            Settings setting = new Settings(this, false);
            setting.Owner = this;
            setting.ShowDialog();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (wvWebView1.CanGoBack)
            {
                wvWebView1.GoBack();
            }
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            if (wvWebView1.CanGoForward)
            {
                wvWebView1.GoForward();
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            wvWebView1.Reload();
        }

        private void btnAddFavourite_Click(object sender, EventArgs e)
        {
            Shortcuts(false);
            var form = new Favourite(this, wvWebView1.CoreWebView2.DocumentTitle, wvWebView1.Source.ToString(), false, null);
            form.Owner = this;
            form.ShowDialog();
        }

        HistoryService historyService = new HistoryService();
        private void txtWebAddress_KeyUp(object sender, KeyEventArgs e)
        {
            Keys key = e.KeyCode;
            if (key == Keys.Enter)

            {
                #region Search
                try
                {
                    var rawUrl = txtWebAddress.Text;
                    Uri uri;
                    if (Uri.IsWellFormedUriString(rawUrl, UriKind.Absolute))
                    {
                        uri = new Uri(rawUrl);
                    }
                    else if (!rawUrl.Contains(" ") && rawUrl.Contains("."))
                    {
                        uri = new Uri("http://" + rawUrl); // An invalid URI contains a dot and no spaces, try tacking http:// on the front.

                    }
                    else
                    {
                        switch (SettingsService.Get("SearchEngine"))
                        {
                            case "bing":
                                uri = new Uri("https://www.bing.com/search?q=" + String.Join("+", Uri.EscapeDataString(rawUrl).Split(new string[] { "%20" }, StringSplitOptions.RemoveEmptyEntries)));
                                break;

                            case "yahoo":
                                uri = new Uri("https://search.yahoo.com/search?p=" + String.Join("+", Uri.EscapeDataString(rawUrl).Split(new string[] { "%20" }, StringSplitOptions.RemoveEmptyEntries)));
                                break;

                            case "duckduckgo":
                                uri = new Uri("https://duckduckgo.com/?q=" + String.Join("+", Uri.EscapeDataString(rawUrl).Split(new string[] { "%20" }, StringSplitOptions.RemoveEmptyEntries)));
                                break;

                            case "wikipedia":
                                uri = new Uri("https://wikipedia.org/w/index.php?search=" + String.Join(" ", Uri.EscapeDataString(rawUrl).Split(new string[] { "%20" }, StringSplitOptions.RemoveEmptyEntries)));
                                break;

                            case "netflix":
                                uri = new Uri("https://www.netflix.com/search?q=" + String.Join("+", Uri.EscapeDataString(rawUrl).Split(new string[] { "%20" }, StringSplitOptions.RemoveEmptyEntries)));
                                break;

                            case "youtube":
                                uri = new Uri("https://www.youtube.com/results?search_query=" + String.Join("+", Uri.EscapeDataString(rawUrl).Split(new string[] { "%20" }, StringSplitOptions.RemoveEmptyEntries)));
                                break;

                            case "googlemaps":
                                uri = new Uri("https://www.google.com/maps/search/" + String.Join("+", Uri.EscapeDataString(rawUrl).Split(new string[] { "%20" }, StringSplitOptions.RemoveEmptyEntries)));
                                break;

                            //case "favicon":
                            //    uri = new Uri("https://www.google.com/s2/favicons?domain=" + String.Join("", Uri.EscapeDataString(rawUrl).Split(new string[] { "" }, StringSplitOptions.RemoveEmptyEntries)));
                            //    break;

                            case "ebay":
                                uri = new Uri("https://www.ebay.com/sch/?_nkw=" + String.Join("+", Uri.EscapeDataString(rawUrl).Split(new string[] { "%20" }, StringSplitOptions.RemoveEmptyEntries)));
                                break;

                            case "amazom":
                                uri = new Uri("https://www.amazon.com/s?k=" + String.Join("+", Uri.EscapeDataString(rawUrl).Split(new string[] { "%20" }, StringSplitOptions.RemoveEmptyEntries)));
                                break;

                            case "ecosia":
                                uri = new Uri("https://www.ecosia.org/search?q=" + String.Join("+", Uri.EscapeDataString(rawUrl).Split(new string[] { "%20" }, StringSplitOptions.RemoveEmptyEntries)));
                                break;

                            case "amazon":
                                uri = new Uri("https://www.amazon.com/s?k=" + String.Join("+", Uri.EscapeDataString(rawUrl).Split(new string[] { "%20" }, StringSplitOptions.RemoveEmptyEntries)));
                                break;

                            default:
                                uri = new Uri("https://www.google.com/search?q=" + String.Join("+", Uri.EscapeDataString(rawUrl).Split(new string[] { "%20" }, StringSplitOptions.RemoveEmptyEntries)));
                                break;
                        }
                    }
                    SetSource(uri);
                }
                catch
                {
                    var rawUrl = txtWebAddress.Text;
                    Uri uri;
                    switch (SettingsService.Get("SearchEngine"))
                    {
                        case "bing":
                            uri = new Uri("https://www.bing.com/search?q=" + String.Join("+", Uri.EscapeDataString(rawUrl).Split(new string[] { "%20" }, StringSplitOptions.RemoveEmptyEntries)));
                            break;

                        case "yahoo":
                            uri = new Uri("https://search.yahoo.com/search?p=" + String.Join("+", Uri.EscapeDataString(rawUrl).Split(new string[] { "%20" }, StringSplitOptions.RemoveEmptyEntries)));
                            break;

                        case "duckduckgo":
                            uri = new Uri("https://duckduckgo.com/?q=" + String.Join("+", Uri.EscapeDataString(rawUrl).Split(new string[] { "%20" }, StringSplitOptions.RemoveEmptyEntries)));
                            break;

                        case "wikipedia":
                            uri = new Uri("https://wikipedia.org/w/index.php?search=" + String.Join(" ", Uri.EscapeDataString(rawUrl).Split(new string[] { "%20" }, StringSplitOptions.RemoveEmptyEntries)));
                            break;

                        case "netflix":
                            uri = new Uri("https://www.netflix.com/search?q=" + String.Join("+", Uri.EscapeDataString(rawUrl).Split(new string[] { "%20" }, StringSplitOptions.RemoveEmptyEntries)));
                            break;

                        case "youtube":
                            uri = new Uri("https://www.youtube.com/results?search_query=" + String.Join("+", Uri.EscapeDataString(rawUrl).Split(new string[] { "%20" }, StringSplitOptions.RemoveEmptyEntries)));
                            break;

                        case "googlemaps":
                            uri = new Uri("https://www.google.com/maps/search/" + String.Join("+", Uri.EscapeDataString(rawUrl).Split(new string[] { "%20" }, StringSplitOptions.RemoveEmptyEntries)));
                            break;

                        //case "favicon":
                        //    uri = new Uri("https://www.google.com/s2/favicons?domain=" + String.Join("", Uri.EscapeDataString(rawUrl).Split(new string[] { "" }, StringSplitOptions.RemoveEmptyEntries)));
                        //    break;

                        case "ebay":
                            uri = new Uri("https://www.ebay.com/sch/?_nkw=" + String.Join("+", Uri.EscapeDataString(rawUrl).Split(new string[] { "%20" }, StringSplitOptions.RemoveEmptyEntries)));
                            break;

                        case "amazom":
                            uri = new Uri("https://www.amazon.com/s?k=" + String.Join("+", Uri.EscapeDataString(rawUrl).Split(new string[] { "%20" }, StringSplitOptions.RemoveEmptyEntries)));
                            break;

                        case "ecosia":
                            uri = new Uri("https://www.ecosia.org/search?q=" + String.Join("+", Uri.EscapeDataString(rawUrl).Split(new string[] { "%20" }, StringSplitOptions.RemoveEmptyEntries)));
                            break;

                        case "amazon":
                            uri = new Uri("https://www.amazon.com/s?k=" + String.Join("+", Uri.EscapeDataString(rawUrl).Split(new string[] { "%20" }, StringSplitOptions.RemoveEmptyEntries)));
                            break;

                        default:
                            uri = new Uri("https://www.google.com/search?q=" + String.Join("+", Uri.EscapeDataString(rawUrl).Split(new string[] { "%20" }, StringSplitOptions.RemoveEmptyEntries)));
                            break;
                    }
                    SetSource(uri);
                }
                #endregion
            }
        }

        private void wvWebView1_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                MessageBox.Show($"WebView2 creation failed with exception = {e.InitializationException}");
                //UpdateTitleWithEvent("CoreWebView2InitializationCompleted failed");
                return;
            }
            wvWebView1.CoreWebView2.SourceChanged += CoreWebView2_SourceChanged;
            wvWebView1.CoreWebView2.HistoryChanged += CoreWebView2_HistoryChanged;
            wvWebView1.CoreWebView2.DocumentTitleChanged += CoreWebView2_DocumentTitleChanged;
            wvWebView1.CoreWebView2.DownloadStarting += CoreViewView2__DownloadStarting;
            wvWebView1.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            wvWebView1.CoreWebView2.FaviconChanged += CoreWebView2_FaviconChanged;
        }

        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;
            Browser browser = new Browser(e.Uri, true);
            browser.InitializeTab();
            var newtab = new TitleBarTab(ParentTabs) { Content = browser };

            if (ParentTabs.InvokeRequired)
            {
                ParentTabs.Invoke(new Action(() =>
                {
                    ParentTabs.Tabs.Insert(ParentTabs.SelectedTabIndex + 1, newtab);
                    ParentTabs.SelectedTabIndex++;
                    ParentTabs.RedrawTabs();
                    ParentTabs.Refresh();
                }));
            }
            else
            {
                ParentTabs.Tabs.Insert(ParentTabs.SelectedTabIndex + 1, newtab);
                ParentTabs.SelectedTabIndex++;
                ParentTabs.RedrawTabs();
                ParentTabs.Refresh();
            }
        }

        private void wvWebView1_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            Cursor = Cursors.AppStarting;
            picFavicon.Visible = false;
            wvLoadingProgress.Visible = true;
            var theme = SettingsService.Get("Theme");
            if (theme == "xmas")
            {
                wvLoadingProgress.Source = new Uri("file://" + Path.Combine(new string[] { Environment.CurrentDirectory + "\\assets\\throbber\\", $"throbber_small_xmas_red.svg" }));
            }
            else
            {
                wvLoadingProgress.Source = new Uri("file://" + Path.Combine(new string[] { Environment.CurrentDirectory + "\\assets\\throbber\\", $"throbber_small_{theme}.svg" }));
            }
            btnRefresh.Visible = false;
            btnStop.Visible = true;
            //UpdateTitleWithEvent("NavigationStarting");
        }

        private void wvWebView1_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (loadnum == 0)
            {
                loadnum++;
            }

            Cursor = Cursors.Default;


            btnRefresh.Visible = true;
            btnStop.Visible = false;
            //UpdateTitleWithEvent("NavigationCompleted");
            wvLoadingProgress.Visible = false;
            picFavicon.Visible = true;
            if (!e.IsSuccess)
            {
                switch (e.WebErrorStatus)
                {
                    // Real network failures only
                    case CoreWebView2WebErrorStatus.CannotConnect:
                    case CoreWebView2WebErrorStatus.ConnectionReset:
                    case CoreWebView2WebErrorStatus.Disconnected:
                    case CoreWebView2WebErrorStatus.HostNameNotResolved:
                    case CoreWebView2WebErrorStatus.RedirectFailed:
                    case CoreWebView2WebErrorStatus.ServerUnreachable:
                    case CoreWebView2WebErrorStatus.Timeout:
                    case CoreWebView2WebErrorStatus.UnexpectedError:
                        wvWebView1.Source = new Uri($"https://quartz.com/{SettingsService.Get("Theme")}/error.html");
                        break;

                    // Certificate issues
                    case CoreWebView2WebErrorStatus.CertificateCommonNameIsIncorrect:
                    case CoreWebView2WebErrorStatus.CertificateExpired:
                    case CoreWebView2WebErrorStatus.CertificateIsInvalid:
                    case CoreWebView2WebErrorStatus.CertificateRevoked:
                    case CoreWebView2WebErrorStatus.ClientCertificateContainsErrors:
                        wvWebView1.Source = new Uri($"https://quartz.com/{SettingsService.Get("Theme")}/Safety.html");
                        break;

                    default:
                        // Unknown, HTTP errors, Google robot pages → do nothing
                        break;
                }
            }
        }

        private Color GetBackColor()
        {
            string theme = SettingsService.Get("Theme");

            if (theme == "light")
            {
                return Color.FromArgb(200, 200, 200);
            }
            else if (theme == "dark")
            {
                return Color.FromArgb(88, 88, 88);
            }
            else if (theme == "black")
            {
                return Color.White;
            }
            else if (theme == "aqua")
            {
                return Color.Blue;
            }
            else if (theme == "xmas")
            {
                return Color.Lime;
            }
            else
            {
                return Color.FromArgb(200, 200, 200);
            }
        }

        private void CoreWebView2_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            if (loadnum == 0 && _newtab == false)
            {
                if (txtWebAddress.SelectionLength != txtWebAddress.TextLength)
                {
                    txtWebAddress.Focus();
                    txtWebAddress.SelectAll();
                }
            }
            else
            {
                if (SettingsService.Get("displayFullURLs") == "true")
                {
                    var url = wvWebView1.Source.AbsoluteUri;

                    if (url.EndsWith("/"))
                    {
                        url = url.Substring(0, url.Length - 1); // remove the trailing slash
                    }

                    this.txtWebAddress.Text = url;
                }
                else
                {
                    var uri = wvWebView1.Source;

                    // Get host
                    string host = uri.Host;

                    // Get path and query
                    string pathAndQuery = uri.PathAndQuery;

                    // Remove trailing slash if it exists
                    if (pathAndQuery.EndsWith("/") && pathAndQuery.Length > 1)
                    {
                        pathAndQuery = pathAndQuery.TrimEnd('/');
                    }
                    else if (pathAndQuery == "/")
                    {
                        pathAndQuery = ""; // remove lone slash
                    }

                    txtWebAddress.Text = host.Replace("www.", "") + pathAndQuery;
                }
            }

            UpdateFavBar();
        }

        private async void CoreWebView2_FaviconChanged(object sender, object e)
        {
            // could add code that refreshes tab favicon to show changes.
            if (Uri.IsWellFormedUriString(wvWebView1.Source.AbsoluteUri, UriKind.Absolute))
            {
                //bool showSiteIconsOnly = bool.Parse(SettingsService.Get("showSiteIconsOnly"));
                //if (showSiteIconsOnly)
                //{
                //    ShowIcon = true;
                //}

                if (!FaviconHelper.DoesFaviconFileExist(wvWebView1.Source.AbsoluteUri))
                {
                    Stream originalStream = await wvWebView1.CoreWebView2.GetFaviconAsync(Microsoft.Web.WebView2.Core.CoreWebView2FaviconImageFormat.Png);

                    if (originalStream != null && originalStream.Length != 0)
                    {
                        // Copy stream into memory so it can be reused
                        MemoryStream memoryStream = new MemoryStream();
                        await originalStream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;

                        bool isDefaultFavicon = await FaviconHelper.IsDefaultFaviconAsync(new MemoryStream(memoryStream.ToArray()));
                        if (!isDefaultFavicon)
                        {
                            memoryStream.Position = 0;
                            Icon icon = FaviconHelper.ConvertToIcon(memoryStream);
                            Icon = icon;
                            FaviconHelper.UpdateCurrentTab(ParentTabs, this);

                            picFavicon.Image = icon.ToBitmap();

                            FaviconHelper.SaveToFile(icon, wvWebView1.Source.AbsoluteUri);
                        }
                        else
                        {
                            //if (showSiteIconsOnly)
                            //{
                            //    ShowIcon = false;
                            //    return;
                            //}

                            Icon icon = FaviconHelper.GetDefaultFavicon16();
                            Icon = icon;
                            FaviconHelper.UpdateCurrentTab(ParentTabs, this);
                            picFavicon.Image = icon.ToBitmap();
                        }
                    }
                }
                else
                {
                    Icon icon = FaviconHelper.GetFaviconFile(wvWebView1.Source.AbsoluteUri);
                    Icon = icon;
                    FaviconHelper.UpdateCurrentTab(ParentTabs, this);

                    picFavicon.Image = icon.ToBitmap();
                }
            }
        } 

        private void CoreWebView2_HistoryChanged(object sender, object e)
        {
            // No explicit check for webView2Control initialization because the events can only start
            // firing after the CoreWebView2 and its events exist for us to subscribe.
            btnBack.Enabled = wvWebView1.CoreWebView2.CanGoBack;
            btnForward.Enabled = wvWebView1.CoreWebView2.CanGoForward;
            //UpdateTitleWithEvent("HistoryChanged");
        }

        private void CoreWebView2_DocumentTitleChanged(object sender, object e)
        {
            this.Text = wvWebView1.CoreWebView2.DocumentTitle;

            if (tabbedApp.SelectedTab.Content == this)
            {
                if (tabbedApp._windowName == string.Empty)
                    tabbedApp.Text = this.Text;
            }

            //UpdateTitleWithEvent("DocumentTitleChanged");

            var service = new HistoryService();

            if (wvWebView1.Source.Scheme.ToLower().StartsWith("http"))
            {
                service.Add(new Models.HistoryModel()
                {
                    Title = wvWebView1.CoreWebView2.DocumentTitle,
                    WebAddress = wvWebView1.Source.AbsoluteUri,
                });
                service.SaveChanges();
            }
        }

        private void openFileInBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.CheckFileExists)
            {
                if (openFileDialog1.CheckPathExists)
                {
                    openFileDialog1.ShowDialog(this);
                    wvWebView1.Source = new Uri("file://" + openFileDialog1.FileName);
                }
                else
                {
                    MessageBox.Show("The Selected Path Does Not Exist", "Something Not Right", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("The Selected File Does Not Exist", "Something Not Right", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void CoreViewView2__DownloadStarting(object sender, CoreWebView2DownloadStartingEventArgs e)
        {
            switch (SettingsService.Get("DownloadAlignment"))
            {
                case "TopLeft":
                    wvWebView1.CoreWebView2.DefaultDownloadDialogCornerAlignment = CoreWebView2DefaultDownloadDialogCornerAlignment.TopLeft;
                    break;

                case "TopRight":
                    wvWebView1.CoreWebView2.DefaultDownloadDialogCornerAlignment = CoreWebView2DefaultDownloadDialogCornerAlignment.TopRight;
                    break;

                case "BottomLeft":
                    wvWebView1.CoreWebView2.DefaultDownloadDialogCornerAlignment = CoreWebView2DefaultDownloadDialogCornerAlignment.BottomLeft;
                    break;

                case "BottomRight":
                    wvWebView1.CoreWebView2.DefaultDownloadDialogCornerAlignment = CoreWebView2DefaultDownloadDialogCornerAlignment.BottomRight;
                    break;

                default:
                    wvWebView1.CoreWebView2.DefaultDownloadDialogCornerAlignment = CoreWebView2DefaultDownloadDialogCornerAlignment.TopRight;
                    break;
            }
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Try to cast the sender to a ToolStripItem
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem != null)
            {
                // Retrieve the ContextMenuStrip that owns this ToolStripItem
                ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                if (owner != null)
                {
                    // Get the control that is displaying this context menu
                    Button button = (Button)owner.SourceControl;

                    var FavouriteService = new FavouriteService();
                    FavouriteService.Remove(button.Text.Replace("      ", ""));
                    FavouriteService.SaveChanges();
                    LoadFavourites();
                }
            }
        }

        private void taskManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                this.wvWebView1.CoreWebView2.OpenTaskManagerWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), "Open Task Manager Window failed");
            }
        }

        private void btnBack_EnabledChanged(object sender, EventArgs e)
        {
            var theme = SettingsService.Get("Theme");
            if (theme == "light")
            {
                if (btnBack.Enabled)
                {
                    btnBack.BackgroundImage = Quartz.Properties.Resources.Left;
                }
                else
                {
                    btnBack.BackgroundImage = Quartz.Properties.Resources.DLeft;
                }
            }
            else if (theme == "dark")
            {
                if (btnBack.Enabled)
                {
                    btnBack.BackgroundImage = Quartz.Properties.Resources.DLeft;
                }
                else
                {
                    btnBack.BackgroundImage = Quartz.Properties.Resources.Left;
                }
            }
            else if (theme == "black")
            {
                if (btnBack.Enabled)
                {
                    btnBack.BackgroundImage = Quartz.Properties.Resources.Black_Left;
                }
                else
                {
                    btnBack.BackgroundImage = Quartz.Properties.Resources.Left;
                }
            }
            else if (theme == "aqua")
            {
                if (btnBack.Enabled)
                {
                    btnBack.BackgroundImage = Quartz.Properties.Resources.Aqua_Icon_Left;
                }
                else
                {
                    btnBack.BackgroundImage = Quartz.Properties.Resources.Aqua_Left_Inactive;
                }
            }
            else if (theme == "xmas")
            {
                if (btnBack.Enabled)
                {
                    btnBack.BackgroundImage = Quartz.Properties.Resources.XLeft;
                }
                else
                {
                    btnBack.BackgroundImage = Quartz.Properties.Resources.XLeft_disabled;
                }
            }
        }

        private void btnForward_EnabledChanged(object sender, EventArgs e)
        {
            var theme = SettingsService.Get("Theme");
            if (theme == "light")
            {
                if (btnForward.Enabled)
                {
                    btnForward.BackgroundImage = Quartz.Properties.Resources.Right;
                }
                else
                {
                    btnForward.BackgroundImage = Quartz.Properties.Resources.DRight;
                }
            }
            else if (theme == "dark")
            {
                if (btnForward.Enabled)
                {
                    btnForward.BackgroundImage = Quartz.Properties.Resources.DRight;
                }
                else
                {
                    btnForward.BackgroundImage = Quartz.Properties.Resources.Right;
                }
            }
            else if (theme == "black")
            {
                if (btnForward.Enabled)
                {
                    btnForward.BackgroundImage = Quartz.Properties.Resources.Black_Right;
                }
                else
                {
                    btnForward.BackgroundImage = Quartz.Properties.Resources.Right;
                }
            }
            else if (theme == "aqua")
            {
                if (btnForward.Enabled)
                {
                    btnForward.BackgroundImage = Quartz.Properties.Resources.Aqua_Icon_Right;
                }
                else
                {
                    btnForward.BackgroundImage = Quartz.Properties.Resources.Aqua_Right_Inactive;
                }
            }
            else if (theme == "xmas")
            {
                if (btnForward.Enabled)
                {
                    btnForward.BackgroundImage = Quartz.Properties.Resources.XRight;
                }
                else
                {
                    btnForward.BackgroundImage = Quartz.Properties.Resources.XRight_disabled;
                }
            }
        }

        private void wvWebView1_ZoomFactorChanged(object sender, EventArgs e)
        {
            SettingsService.Set("Zoom", wvWebView1.ZoomFactor.ToString());
        }

        private void removeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show($"You Are About To Delete {pnlFavourites.Controls.Count} Favourites?", "Favourite", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
            {
                var FavouriteService = new FavouriteService();
                FavouriteService.DeleteProfileFav(ProfileService.Current);
                FavouriteService.SaveChanges();
                LoadFavourites();
            }
        }

        private void pnlFavourites_ControlRemoved(object sender, ControlEventArgs e)
        {
            UpdateFavBar();
        }

        private void pnlFavourites_ControlAdded(object sender, ControlEventArgs e)
        {
            UpdateFavBar();
        }

        private void Browser_MouseMove(object sender, MouseEventArgs e)
        {
            if (!wvWebView1.Bounds.Contains(e.Location))
            {
                wvWebView1.Visible = false; // Menu panel 
            }
        }

        private void Browser_LocationChanged(object sender, EventArgs e)
        {
            //Pos();
        }

        private void Browser_SizeChanged(object sender, EventArgs e)
        {
            UpdateFavBar();
            RestoreDownloadDialog();
        }

        private void mnuMenu_Opening(object sender, CancelEventArgs e)
        {
            FavouriteService favouriteService = new FavouriteService();

            removeAllToolStripMenuItem.Enabled = pnlFavourites.Controls.Count > 1;

            showFavouritesBarToolStripMenuItem.Checked = SettingsService.Get("showFavouritesBar") == "true";
            toolStripMenuItem5.Checked = SettingsService.Get("showFavouriteIcon") == "true";
            sortByAlphabeticallyToolStripMenuItem.Checked = SettingsService.Get("sortFavouritesBy") == "alphabetically";

            openToolStripMenuItem.Enabled = !(mnuMenu.SourceControl is Panel);
            modifyToolStripMenuItem.Enabled = !(mnuMenu.SourceControl is Panel);
            removeToolStripMenuItem.Enabled = !(mnuMenu.SourceControl is Panel);
            copyToolStripMenuItem.Enabled = !(mnuMenu.SourceControl is Panel);
            cutToolStripMenuItem1.Enabled = !(mnuMenu.SourceControl is Panel);

            pasteToolStripMenuItem1.Enabled = !string.IsNullOrEmpty(Clipboard.GetText());

            if (!(mnuMenu.SourceControl is Panel))
            {
                openInNewWindowToolStripMenuItem.Text = "Open in new window";
                openInNewTabToolStripMenuItem.Text = "Open in new tab";
            }
            else
            {
                openInNewWindowToolStripMenuItem.Text = "Open in new window" + $" ({favouriteService.All().Count})";
                openInNewTabToolStripMenuItem.Text = "Open in new tab" + $" ({favouriteService.All().Count})";
            }


            if (SettingsService.Get("Animation") == "true")
            {
                Animation.AnimateWindow(mnuMenu.Handle, 100, Animation.AW_BLEND);
            }
        }

        private void changeProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Shortcuts(false);
            var profile = new Profiles(this, null);
            profile.Owner = this;
            profile.ShowDialog();
        }

        private void SettingsMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            if (SettingsService.Get("Animation") == "true")
            {
                Animation.AnimateWindow(SettingsMenuStrip.Handle, 100, Animation.AW_BLEND);
            }

            if (SettingsService.Get("AreDevToolsEnabled") == "true")
            {
                inspectToolStripMenuItem.Enabled = true;
            }
            else
            {
                inspectToolStripMenuItem.Enabled = false;
            }

            historyToolStripMenuItem.Enabled = Program.profileService.Get(ProfileService.Current).isDisposable != true;

            zoomToolStrip.Items.Clear();

            for (double f = 25; f <= 500; f = f + 25)
            {
                zoomToolStrip.Items.Add(f.ToString() + "%");
            }

            zoomToolStrip.Text = (wvWebView1.ZoomFactor * 100).ToString() + "%";
        }
        private void Browser_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Get all open AppContainer forms
            var forms = Application.OpenForms.Cast<Form>()
                .Where(f => f.Name == "AppContainer")
                .ToList();
            AppContainer form = (AppContainer)Application.OpenForms["AppContainer"];

            // Only act if this is the last AppContainer with a single tab
            if (forms.Count != 1 || form.Tabs.Count != 1)
                return;

            // Get the profile associated with the WebView2 instance
            var webViewProfileId = Guid.Parse(wvWebView1.CoreWebView2.Profile.ProfileName);
            var webViewProfile = Program.profileService.Get(webViewProfileId);

            // Update last active timestamp
            webViewProfile.lastActive = DateTime.Now;
            Program.profileService.SaveChanges();

            // Only delete if the profile is disposable and marked for end session
            if (webViewProfile.isDisposable && webViewProfile.endSession)
            {
                // Store the profile that should remain active after deletion
                var currentActive = Program.profileService.Get(ProfileService.Current);

                // Delete associated data
                SettingsService.DeleteProfileSettings(webViewProfileId);
                new HistoryService().DeleteProfileHistory(webViewProfileId);
                new FavouriteService().DeleteProfileFav(webViewProfileId);

                // Remove profile from service and delete WebView2 profile
                Program.profileService.Remove(webViewProfileId);
                Program.profileService.SaveChanges();
                wvWebView1.CoreWebView2.Profile.Delete();

                // Set the active profile: prioritize override if applicable
                bool overrideActive = currentActive != null && currentActive.Id != webViewProfileId && currentActive.isDisposable;
                if (overrideActive)
                    Program.profileService.SetActive(currentActive.Id);
                else
                    Program.profileService.SetActive(Program.profileService.LastActiveProfile());

                Program.profileService.SaveChanges();
            }
        }

        private void modifyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Try to cast the sender to a ToolStripItem
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem != null)
            {
                // Retrieve the ContextMenuStrip that owns this ToolStripItem
                ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                if (owner != null)
                {
                    // Get the control that is displaying this context menu
                    Button button = (Button)owner.SourceControl;

                    var FavouriteService = new FavouriteService();
                    var form = new Favourite(this, button.Text.Replace("      ", ""), FavouriteService.Get(button.Text.Replace("      ", "")).WebAddress, true, button);
                    form.Owner = this;
                    form.ShowDialog();
                }
            }
        }
        #endregion

        #region Helper Functions
        public void ConvertToIco(Image img, string file, int width, int height)
        {
            Icon icon;
            using (var msImg = new MemoryStream())
            using (var msIco = new MemoryStream())
            {
                img.Save(msImg, ImageFormat.Png);
                using (var bw = new BinaryWriter(msIco))
                {
                    bw.Write((short)0);           //0-1 reserved
                    bw.Write((short)1);           //2-3 image type, 1 = icon, 2 = cursor
                    bw.Write((short)1);           //4-5 number of images
                    bw.Write((byte)width);        //6 image width
                    bw.Write((byte)height);       //7 image height
                    bw.Write((byte)0);            //8 number of colors
                    bw.Write((byte)0);            //9 reserved
                    bw.Write((short)0);           //10-11 color planes
                    bw.Write((short)32);          //12-13 bits per pixel
                    bw.Write((int)msImg.Length);  //14-17 size of image data
                    bw.Write(22);                 //18-21 offset of image data
                    bw.Write(msImg.ToArray());    // write image data
                    bw.Flush();
                    bw.Seek(0, SeekOrigin.Begin);
                    icon = new Icon(msIco);
                }
            }
            using (var fs = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                icon.Save(fs);
            }
        }
        #endregion

        private void historyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Shortcuts(false);
            var history = new History(this);
            history.Owner = this;
            history.ShowDialog();
        }

        private void inspectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            wvWebView1.CoreWebView2.OpenDevToolsWindow();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Power.Shutdown();
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (wvWebView1.CoreWebView2.IsDefaultDownloadDialogOpen)
            {
                wvWebView1.CoreWebView2.CloseDefaultDownloadDialog();
            }
            else
            {
                wvWebView1.CoreWebView2.OpenDefaultDownloadDialog();
            }
        }

        private void mnuDownloadsDropDown_Opening(object sender, CancelEventArgs e)
        {
            if (SettingsService.Get("Animation") == "true")
            {
                Animation.AnimateWindow(mnuDownloadsDropDown.Handle, 100, Animation.AW_BLEND);
            }
            locationToolStripMenuItem1.Text = $"Location: {wvWebView1.CoreWebView2.Profile.DefaultDownloadFolderPath}";
        }

        private void changeLocationToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.SelectedPath = wvWebView1.CoreWebView2.Profile.DefaultDownloadFolderPath;
            folderBrowserDialog1.ShowDialog();
            try
            {
                wvWebView1.CoreWebView2.Profile.DefaultDownloadFolderPath = folderBrowserDialog1.SelectedPath;
            }
            catch
            {
            }
        }

        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // Get the control that is displaying this context menu
            Button button = (Button)mnuMenu.SourceControl;

            Clipboard.SetText(button.Text.Replace("      ", ""));
        }

        private void copyAddressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the contol that is displaying this context menu
            Button button = (Button)mnuMenu.SourceControl;

            var FavouriteService = new FavouriteService();
            var fav = FavouriteService.Get(button.Text.Replace("      ", ""));
            Clipboard.SetText(fav.WebAddress);
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadFavourites();
        }
        private void newTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Browser browser = new Browser(null, false);
            browser.InitializeTab();
            var newtab = new TitleBarTab(ParentTabs) { Content = browser };
            if (ParentTabs.InvokeRequired)
            {
                ParentTabs.Invoke(new Action(() =>
                {
                    ParentTabs.Tabs.Insert(ParentTabs.SelectedTabIndex + 1, newtab);
                    ParentTabs.SelectedTabIndex++;
                    ParentTabs.RedrawTabs();
                    ParentTabs.Refresh();
                }));
            }
            else
            {
                ParentTabs.Tabs.Insert(ParentTabs.SelectedTabIndex + 1, newtab);
                ParentTabs.SelectedTabIndex++;
                ParentTabs.RedrawTabs();
                ParentTabs.Refresh();
            }
        }

        private void closeTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabbedApp.SelectedTab.Content.Close();
        }

        private void mnuExperts_Opening(object sender, CancelEventArgs e)
        {
            if (SettingsService.Get("Animation") == "true")
            {
                Animation.AnimateWindow(mnuExperts.Handle, 100, Animation.AW_BLEND);
            }
        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Program._bypassPassword = true;
            Power.Restart();
        }

        private void copyTitleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(tabbedApp.SelectedTab.Caption);
        }

        private void copyAddressToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(wvWebView1.Source.AbsoluteUri);
        }

        private void copyFaviconToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetImage(tabbedApp.SelectedTab.Icon.ToBitmap());
        }
        private void closeAllTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedtab = tabbedApp.SelectedTab;
            if (selectedtab != null)
            {
                foreach (var tab in tabbedApp.Tabs.ToList())
                {
                    if (tab != selectedtab)
                    {
                        tabbedApp.Tabs.Remove(tab);
                    }
                }
            }
        }

        private void mnuHistory_Opening(object sender, CancelEventArgs e)
        {
            HistoryService historyService = new HistoryService();
            int count = 0;
            int limit = 10;

            mnuHistory.Items.Clear();

            ToolStripMenuItem historyItem = new ToolStripMenuItem();
            historyItem.Text = "History";
            historyItem.Click += historyToolStripMenuItem_Click;
            mnuHistory.Items.Add(historyItem);

            ToolStripSeparator separatorItem = new ToolStripSeparator();
            mnuHistory.Items.Add(separatorItem);

            if (historyService.All().Count >= 1)
            {
                var orderedHistory = historyService.All()
                                   .OrderByDescending(i => i.When)
                                   .ToList(); // materialize

                foreach (HistoryModel history in orderedHistory)
                {
                    if (count <= limit)
                    {
                        ToolStripMenuItem menuItem = new ToolStripMenuItem();

                        menuItem.Text = history.Title.Length > 64 ? history.Title.Substring(0, 64) + "..." : history.Title;
                        menuItem.Tag = history.WebAddress;
                        menuItem.Image = FaviconHelper.GetFaviconFileExternalAsImage(history.WebAddress);
                        menuItem.ToolTipText = history.WebAddress;
                        menuItem.Click += MenuItem_Click;
                        menuItem.MouseUp += MenuItem_MouseUp;

                        mnuHistory.Items.Add(menuItem);

                        count++;
                    }
                }
            }
            else
            {
                ToolStripMenuItem lohItem = new ToolStripMenuItem();
                lohItem.Text = "History will appear here once you start browsing.";
                mnuHistory.Items.Add(lohItem);
            }

            if (SettingsService.Get("Animation") == "true")
            {
                Animation.AnimateWindow(mnuHistory.Handle, 100, Animation.AW_BLEND);
            }
        }

        private void MenuItem_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                SettingsMenuStrip.Close();

                ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;

                Browser browser = new Browser(menuItem.Tag.ToString(), true);
                browser.InitializeTab();
                var newtab = new TitleBarTab(ParentTabs) { Content = browser };
                if (ParentTabs.InvokeRequired)
                {
                    ParentTabs.Invoke(new Action(() =>
                    {
                        ParentTabs.Tabs.Insert(ParentTabs.SelectedTabIndex + 1, newtab);
                        ParentTabs.SelectedTabIndex++;
                        ParentTabs.RedrawTabs();
                        ParentTabs.Refresh();
                    }));
                }
                else
                {
                    ParentTabs.Tabs.Insert(ParentTabs.SelectedTabIndex + 1, newtab);
                    ParentTabs.SelectedTabIndex++;
                    ParentTabs.RedrawTabs();
                    ParentTabs.Refresh();
                }
            }
        }

        private void MenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;

            SetSource(new Uri(menuItem.Tag.ToString()));
        }

        private void resetToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                "This action will delete ALL user data, including profiles, settings, browsing history, favorites, saved passwords, cookies, cached data, and any other personalized information you have stored.\n\n" +
                "This is essentially a complete factory reset of the browser, returning it to its original state.\n\n" +
                "Once initiated, this process CANNOT be undone. All your custom settings and personal data will be permanently lost and cannot be recovered.\n\n" +
                "Make sure you have backed up any important information, as this is a destructive operation.\n\n" +
                "Are you absolutely sure you want to proceed?",
                "Critical Warning: UserData Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
                ) == DialogResult.No)
                return;

            if (MessageBox.Show(
                "Seriously, are you sure you want to permanently delete ALL your browser’s userdata?\n\n" +
                "This action WILL erase everything, including your profiles, settings, history, and preferences, and it cannot be undone. Once deleted, this data will be lost forever.\n\n" +
                "If you are absolutely sure you want to proceed, click 'Yes'. To cancel and keep your data, click 'No'.",
                "Confirm Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
                ) == DialogResult.No)
                return;

            if (MessageBox.Show(
                "This is your last warning: This will permanently delete all your browser’s userdata. Only continue if you know what you're doing.\n\n" +
                "Once confirmed, this action cannot be undone.",
                "Confirm Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
                ) == DialogResult.No)
                return;

            MainSettingsService.Set("Reset", "true");
            Power.Restart();
        }

        private void mnuUserData_Opening(object sender, CancelEventArgs e)
        {
            if (SettingsService.Get("Animation") == "true")
            {
                Animation.AnimateWindow(mnuUserData.Handle, 100, Animation.AW_BLEND);
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainSettingsService.Set("Export", "true");
            Power.Restart();
        }

        /// <summary>
        /// Opens File Explorer to a specified directory and optionally selects a file or folder.
        /// </summary>
        /// <param name="directoryPath">The path to the directory to open in File Explorer.</param>
        /// <param name="fileOrFolderToSelect">The path of the file or folder to select (optional). Pass null to skip selecting a file.</param>
        public static void OpenDirectoryAndSelect(string directoryPath, string fileOrFolderToSelect = null)
        {
            // Validate that the directory exists
            if (Directory.Exists(directoryPath))
            {
                // If no specific file/folder to select is given, just open the directory
                string argument = directoryPath;

                // If a file or folder is specified to be selected, ensure it exists and modify the argument
                if (!string.IsNullOrEmpty(fileOrFolderToSelect) && (File.Exists(fileOrFolderToSelect) || Directory.Exists(fileOrFolderToSelect)))
                {
                    // Append the /select argument to highlight the file/folder
                    argument = $"/select,\"{fileOrFolderToSelect}\"";
                }
                else if (!string.IsNullOrEmpty(fileOrFolderToSelect))
                {
                    Console.WriteLine("The specified file or folder to select does not exist.");
                    return;
                }

                // Open File Explorer with the argument (either directory or with file/folder selection)
                Process.Start("explorer.exe", argument);
            }
            else
            {
                Console.WriteLine("The specified directory does not exist.");
            }
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainSettingsService.Set("Import", "true");
            Power.Restart();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            Shortcuts(false);
            Settings setting = new Settings(this, false);
            setting.Owner = this;
            setting.ShowDialog();
        }

        private void Item_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;

            // Example string: "formIndex: 2, tabIndex: 5"
            string tagValue = menuItem.Tag.ToString();

            // Split the string into parts by the comma
            string[] parts = tagValue.Split(',');

            // Extract the formIndex and tabIndex values
            int formIndex = 0;
            int tabIndex = 0;

            foreach (var part in parts)
            {
                if (part.Contains("formIndex"))
                {
                    // Extract the number after "formIndex: "
                    formIndex = int.Parse(part.Split(':')[1].Trim());
                }
                else if (part.Contains("tabIndex"))
                {
                    // Extract the number after "tabIndex: "
                    tabIndex = int.Parse(part.Split(':')[1].Trim());
                }
            }

            // Now you have the formIndex and tabIndex as integers

            AppContainer form = Application.OpenForms.Cast<Form>().Where(f => f.Name == "AppContainer").ToList()[formIndex] as AppContainer;
            TitleBarTab tab = form.Tabs[tabIndex];

            form.Activate();
            form.SelectedTabIndex = tabIndex;


        }

        private void zoomToolStrip_SelectedIndexChanged(object sender, EventArgs e)
        {
            wvWebView1.ZoomFactor = Convert.ToDouble(zoomToolStrip.Text.Replace("%", "")) / 100;
            SettingsService.Set("Zoom", (Convert.ToDouble(zoomToolStrip.Text.Replace("%", "")) / 100).ToString());
        }

        private void zoomToolStrip_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (!zoomToolStrip.Items.Contains(zoomToolStrip.Text))
                {
                    zoomToolStrip.Text = (wvWebView1.ZoomFactor * 100).ToString() + "%";
                }
            }
        }

        private void toolStripMenuItem5_CheckedChanged(object sender, EventArgs e)
        {
            SettingsService.Set("showFavouriteIcon", toolStripMenuItem5.Checked.ToString().ToLower());
            LoadFavourites();
        }

        private void fullscreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FullScreen)
            {
                FullScreen = false;
            }
            else
            {
                FullScreen = true;
            }
        }


        string _originalURL;
        private void txtWebAddress_Leave(object sender, EventArgs e)
        {
            if (SettingsService.Get("displayFullURLs") == "false"
                && _originalURL == txtWebAddress.Text)
            {
                var uri = wvWebView1.Source;

                // Get host
                string host = uri.Host;

                // Get path and query
                string pathAndQuery = uri.PathAndQuery;

                // Remove trailing slash if it exists
                if (pathAndQuery.EndsWith("/") && pathAndQuery.Length > 1)
                {
                    pathAndQuery = pathAndQuery.TrimEnd('/');
                }
                else if (pathAndQuery == "/")
                {
                    pathAndQuery = ""; // remove lone slash
                }

                txtWebAddress.Text = host.Replace("www.", "") + pathAndQuery;
            }
        }

        private void txtWebAddress_Enter(object sender, EventArgs e)
        {
            var uri = wvWebView1.Source;

            if (uri == null)
                return;

            // Get host
            string host = uri.Host;

            // Get path and query
            string pathAndQuery = uri.PathAndQuery;

            // Remove trailing slash if it exists
            if (pathAndQuery.EndsWith("/") && pathAndQuery.Length > 1)
            {
                pathAndQuery = pathAndQuery.TrimEnd('/');
            }
            else if (pathAndQuery == "/")
            {
                pathAndQuery = ""; // remove lone slash
            }

            var url = host.Replace("www.", "") + pathAndQuery;

            if (txtWebAddress.Text == url)
            {
                if (SettingsService.Get("displayFullURLs") == "false")
                {
                    var _url = wvWebView1.Source.AbsoluteUri;

                    if (_url.EndsWith("/"))
                    {
                        _url = _url.Substring(0, _url.Length - 1); // remove the trailing slash
                    }

                    this.txtWebAddress.Text = _url;
                }

                _originalURL = txtWebAddress.Text;
            }

            BeginInvoke((Action)(() => txtWebAddress.SelectAll()));
        }

        public void SortByAlphabetially()
        {
            FavouriteService favouriteService = new FavouriteService();
            List<FavouriteModel> allItems = favouriteService.All();

            var sortedItems = allItems.OrderBy(item => item.Name).ToList();

            for (int i = 0; i < sortedItems.Count; i++)
            {
                sortedItems[i].Index = i; // Update Index to match new position
            }

            favouriteService.SaveChanges();
        }


        private void sortByAlphabeticallyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sortByAlphabeticallyToolStripMenuItem.Checked)
            {
                SettingsService.Set("sortFavouritesBy", "alphabetically");
                SortByAlphabetially();
                LoadFavourites();
            }
            else
            {
                SettingsService.Set("sortFavouritesBy", "custom");
            }
        }

        private void openInNewTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Try to cast the sender to a ToolStripItem
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem != null)
            {
                // Retrieve the ContextMenuStrip that owns this ToolStripItem
                ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                if (owner != null)
                {
                    if (owner.SourceControl is Button)
                    {
                        // Get the control that is displaying this context menu
                        Button button = (Button)owner.SourceControl;

                        // Fake a middle mouse button click at position (0,0) with one click and delta 0
                        MouseEventArgs middleClick = new MouseEventArgs(MouseButtons.Middle, 1, 0, 0, 0);

                        // Pass the fake mouse event to your existing handler
                        Button_MouseUp(button, middleClick);
                    }
                    else
                    {
                        foreach (Button button in pnlFavourites.Controls)
                        {
                            // Fake a middle mouse button click at position (0,0) with one click and delta 0
                            MouseEventArgs middleClick = new MouseEventArgs(MouseButtons.Middle, 1, 0, 0, 0);

                            // Pass the fake mouse event to your existing handler
                            Button_MouseUp(button, middleClick);
                        }
                    }
                }
            }
        }

        private async void openInNewWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Try to cast the sender to a ToolStripItem
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem != null)
            {
                // Retrieve the ContextMenuStrip that owns this ToolStripItem
                ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                if (owner != null)
                {

                    if (owner.SourceControl is Button)
                    {
                        // Get the control that is displaying this context menu
                        Button button = (Button)owner.SourceControl;

                        Program.OpenNewAppContainer(button.Tag.ToString());
                    }
                    else
                    {

                        List<string> addreses = new List<string>();
                        foreach (Button _button in pnlFavourites.Controls)
                        {
                            addreses.Add(_button.Tag.ToString());
                        }

                        await Program.OpenNewWindowWithTabsFast(addreses);
                    }
                }
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Try to cast the sender to a ToolStripItem
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem != null)
            {
                // Retrieve the ContextMenuStrip that owns this ToolStripItem
                ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                if (owner != null)
                {
                    // Get the control that is displaying this context menu
                    Button button = (Button)owner.SourceControl;

                    Clipboard.SetText(button.Tag.ToString());
                }
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Try to cast the sender to a ToolStripItem
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem != null)
            {
                // Retrieve the ContextMenuStrip that owns this ToolStripItem
                ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                if (owner != null)
                {
                    // Get the control that is displaying this context menu
                    Button button = (Button)owner.SourceControl;

                    btnGotoFavourite_Click(button, e);
                }
            }
        }

        private void newWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Program.OpenNewAppContainer(null);
        }

        
        private async void txtWebAddress_TextChanged(object sender, EventArgs e)
        {
            themeClass.RenderText();
            //await suggestionsClass.ShowSuggestionsAsync();
        }

        private void emojiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EmojiHelper.ShowEmojiPanel();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtWebAddress.Undo();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtWebAddress.Cut();
        }

        private void copyToolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            txtWebAddress.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtWebAddress.Paste();
            NewControlThemeChanger.ChangeControlTheme(txtWebAddress);
        }

        private void pasteAndGoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtWebAddress.Clear();
            txtWebAddress.Paste();
            SendKeys.Send("{ENTER}");
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SendKeys.Send("{DELETE}");
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtWebAddress.SelectAll();
        }

        private void alwaysShowFullURLsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            SettingsService.Set("displayFullURLs", alwaysShowFullURLsToolStripMenuItem.Checked.ToString().ToString().ToLower());

            if (SettingsService.Get("displayFullURLs") == "true")
            {
                var url = wvWebView1.Source.AbsoluteUri;

                if (url.EndsWith("/"))
                {
                    url = url.Substring(0, url.Length - 1); // remove the trailing slash
                }

                this.txtWebAddress.Text = url;
            }
            else
            {
                var uri = wvWebView1.Source;

                // Get host
                string host = uri.Host;

                // Get path and query
                string pathAndQuery = uri.PathAndQuery;

                // Remove trailing slash if it exists
                if (pathAndQuery.EndsWith("/") && pathAndQuery.Length > 1)
                {
                    pathAndQuery = pathAndQuery.TrimEnd('/');
                }
                else if (pathAndQuery == "/")
                {
                    pathAndQuery = ""; // remove lone slash
                }

                txtWebAddress.Text = host.Replace("www.", "") + pathAndQuery;
            }
        }
        public static string LimitStringLength(string input, int maxLength)
        {
            if (input.Length > maxLength)
            {
                return input.Substring(0, maxLength - 3) + "...";  // Substring removes extra characters and adds "..."
            }
            return input;
        }  

        private void mnuSearch_Opening(object sender, CancelEventArgs e)
        {
            txtWebAddress.Focus();

            alwaysShowFullURLsToolStripMenuItem.Checked = SettingsService.Get("displayFullURLs") == "true";
          
            undoToolStripMenuItem.Enabled = txtWebAddress.CanUndo;
            redoToolStripMenuItem.Enabled = txtWebAddress.CanRedo;
            cutToolStripMenuItem.Enabled = !String.IsNullOrEmpty(txtWebAddress.SelectedText);
            copyToolStripMenuItem1.Enabled = !String.IsNullOrEmpty(txtWebAddress.SelectedText);
            deleteToolStripMenuItem.Enabled = !String.IsNullOrEmpty(txtWebAddress.SelectedText);


            if (SettingsService.Get("Animation") == "true")
            {
                Animation.AnimateWindow(mnuSearch.Handle, 100, Animation.AW_BLEND);
            }


            if (!string.IsNullOrEmpty(Clipboard.GetText()))
            {
                pasteAndGoToolStripMenuItem.Text = LimitStringLength(
                    ($@"Paste and search for ""{Clipboard.GetText()}""")
                    .Replace("\r\n", " ")
                    .Replace("\n", " ")
                    .Replace("\r", " "),
                    64);

                pasteToolStripMenuItem.Enabled = true;
                pasteAndGoToolStripMenuItem.Enabled = true;
            }
            else
            {
                pasteAndGoToolStripMenuItem.Text = "Paste and go";
                pasteAndGoToolStripMenuItem.Enabled = false;
                pasteToolStripMenuItem.Enabled = false;
            }
        }

        private void openFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenDirectoryAndSelect(Application.StartupPath, Application.StartupPath + @"\UserData\");
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtWebAddress.Redo();
        }

        private void mnuSearch_Opened(object sender, EventArgs e)
        {
            txtWebAddress.Focus();
        }

        private void txtWebAddress_BackColorChanged(object sender, EventArgs e)
        {
            txtWebAddress.BackColor = GetBackColor();
            txtWebAddress.SelectionBackColor = GetBackColor();
        }

        private void showFavouritesBarToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            SettingsService.Set("showFavouritesBar", showFavouritesBarToolStripMenuItem.Checked.ToString().ToLower());
            UpdateFavBar();
        }

        private void cutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // Try to cast the sender to a ToolStripItem
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem != null)
            {
                // Retrieve the ContextMenuStrip that owns this ToolStripItem
                ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                if (owner != null)
                {
                    // Get the control that is displaying this context menu
                    Button button = (Button)owner.SourceControl;

                    Clipboard.SetText(button.Tag.ToString());

                    FavouriteService favouriteService = new FavouriteService();
                    favouriteService.Remove(button.Text.Replace("      ", ""));
                    favouriteService.SaveChanges();

                    LoadFavourites();
                }
            }
        }
    }
}