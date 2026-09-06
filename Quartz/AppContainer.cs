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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quartz
{
    public partial class AppContainer : TitleBarTabs
    {
        private static readonly Size MinimumWindowSize = new Size(816, 489);
        private const int MinimumVisibleWindowEdge = 30;
        private const string WindowSizeSetting = "WindowSize";
        private const string WindowPositionSetting = "WindowPosition";
        private const string WindowStateSetting = "WindowState";

        private bool _restoringWindowSettings = true;
        private FormWindowState _lastNonMinimizedWindowState = FormWindowState.Normal;
        private FormWindowState _lastObservedWindowState = FormWindowState.Normal;
        private Size? _savedWindowSize;
        private System.Drawing.Point? _savedWindowPosition;
        private FormWindowState? _savedWindowState;
        private readonly Timer _windowSettingsSaveTimer = new Timer { Interval = 300 };

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
            MinimumSize = MinimumWindowSize;

            _windowSettingsSaveTimer.Tick += WindowSettingsSaveTimer_Tick;

            ReadWindowSettings();
            ApplyWindowSettings();

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

            // Light uses the blue throbber; the other themes use their existing foreground colour.
            TabRenderer.LoadingIndicatorColor = theme == "light" ||
                !(theme == "dark" || theme == "black" || theme == "aqua" || theme == "xmas")
                ? System.Drawing.Color.FromArgb(66, 133, 244)
                : textForeColor;

            Icon = FaviconHelper.GetFullResDefaultFaviconWithoutCustomFavicon();

            CustomWindow(barBackColor, textForeColor, windowOutline, Handle);

            ContextMenuProvider._contextMenuStripNormal = new DefaultContextMenu();
            ContextMenuProvider._contextMenuStripTab = new TabContextMenu();
        }

        private void ReadWindowSettings()
        {
            string[] savedSize = MainSettingsService.Get(WindowSizeSetting)?.Split(',');
            if (savedSize?.Length == 2
                && int.TryParse(savedSize[0], out int width)
                && int.TryParse(savedSize[1], out int height))
            {
                _savedWindowSize = new Size(
                    Math.Max(MinimumWindowSize.Width, width),
                    Math.Max(MinimumWindowSize.Height, height));
            }

            string[] savedPosition = MainSettingsService.Get(WindowPositionSetting)?.Split(',');
            if (savedPosition?.Length == 2
                && int.TryParse(savedPosition[0], out int x)
                && int.TryParse(savedPosition[1], out int y))
            {
                _savedWindowPosition = new System.Drawing.Point(x, y);
            }

            string savedState = MainSettingsService.Get(WindowStateSetting);
            if (savedState == FormWindowState.Maximized.ToString())
            {
                _savedWindowState = FormWindowState.Maximized;
            }
            else if (savedState == FormWindowState.Normal.ToString())
            {
                _savedWindowState = FormWindowState.Normal;
            }
        }

        private void ApplyWindowSettings()
        {
            if (_savedWindowSize.HasValue)
            {
                WindowState = FormWindowState.Normal;
                StartPosition = FormStartPosition.Manual;
                Bounds = GetSafeRestoredBounds();
            }

            if (_savedWindowState.HasValue)
                WindowState = _savedWindowState.Value;
        }

        private Rectangle GetSafeRestoredBounds()
        {
            Size savedSize = _savedWindowSize.Value;
            bool savedPositionIsVisible = false;
            Screen targetScreen;

            if (_savedWindowPosition.HasValue)
            {
                Rectangle requestedBounds = new Rectangle(_savedWindowPosition.Value, savedSize);
                savedPositionIsVisible = Screen.AllScreens.Any(
                    screen => screen.WorkingArea.IntersectsWith(requestedBounds));

                targetScreen = savedPositionIsVisible
                    ? Screen.FromRectangle(requestedBounds)
                    : Screen.FromPoint(Cursor.Position);
            }
            else
            {
                targetScreen = Screen.FromPoint(Cursor.Position);
            }

            Rectangle workingArea = targetScreen.WorkingArea;
            int width = Math.Min(savedSize.Width, Math.Max(MinimumWindowSize.Width, workingArea.Width));
            int height = Math.Min(savedSize.Height, Math.Max(MinimumWindowSize.Height, workingArea.Height));

            int x = savedPositionIsVisible
                ? Math.Max(workingArea.Left + MinimumVisibleWindowEdge - width,
                    Math.Min(_savedWindowPosition.Value.X, workingArea.Right - MinimumVisibleWindowEdge))
                : workingArea.Left + (workingArea.Width - width) / 2;

            int y = savedPositionIsVisible
                ? Math.Max(workingArea.Top,
                    Math.Min(_savedWindowPosition.Value.Y, workingArea.Bottom - MinimumVisibleWindowEdge))
                : workingArea.Top + (workingArea.Height - height) / 2;

            return new Rectangle(x, y, width, height);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            _lastObservedWindowState = WindowState;
            if (WindowState != FormWindowState.Minimized)
                _lastNonMinimizedWindowState = WindowState;

            _restoringWindowSettings = false;
        }

        private void SaveWindowSettings()
        {
            if (_restoringWindowSettings)
                return;

            Rectangle normalBounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
            if (normalBounds.Width <= 0 || normalBounds.Height <= 0)
                return;

            FormWindowState savedState = WindowState == FormWindowState.Minimized
                ? _lastNonMinimizedWindowState
                : WindowState;

            MainSettingsService.Set(WindowSizeSetting, $"{normalBounds.Width},{normalBounds.Height}");
            MainSettingsService.Set(WindowPositionSetting, $"{normalBounds.X},{normalBounds.Y}");
            MainSettingsService.Set(
                WindowStateSetting,
                savedState == FormWindowState.Maximized
                    ? FormWindowState.Maximized.ToString()
                    : FormWindowState.Normal.ToString());
        }

        private void ScheduleWindowSettingsSave()
        {
            if (_restoringWindowSettings || WindowState != FormWindowState.Normal)
                return;

            _windowSettingsSaveTimer.Stop();
            _windowSettingsSaveTimer.Start();
        }

        private void WindowSettingsSaveTimer_Tick(object sender, EventArgs e)
        {
            _windowSettingsSaveTimer.Stop();
            SaveWindowSettings();
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            SaveWindowSettings();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (!e.Cancel)
                SaveWindowSettings();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _windowSettingsSaveTimer.Dispose();
            base.OnFormClosed(e);
        }

        public override TitleBarTab CreateTab()
        {
            Browser browser = new Browser(null, false);
            browser.InitializeTab();

            return new TitleBarTab(this)
            {
                Content = browser,
                IsLoading = true,
            };
        }

        private void AppContainer_Load(object sender, EventArgs e)
        {
            this.TabSelected += AppContainer_TabSelected;
        }

        private async void AppContainer_TabSelected(object sender, TitleBarTabEventArgs e)
        {
            Browser browser = (Browser)SelectedTab.Content;
            browser.tabbedApp = (AppContainer)browser.Parent;

            if(_windowName == string.Empty)
                 this.Text = e.Tab.Content.Text;

            foreach (var item in Tabs.ToList())
            {
                try
                {
                    Browser form = (Browser)item.Content;

                    if (item.Active)
                    {
                        bool isCorrect = await FavouriteService.ValidatePanelAsync(browser.pnlFavourites);
                        if (!isCorrect)
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

            ScheduleWindowSettingsSave();
        }

        private void AppContainer_SizeChanged(object sender, EventArgs e)
        {
            if (MinimumSize != MinimumWindowSize)
                MinimumSize = MinimumWindowSize;

            FormWindowState currentState = WindowState;
            if (currentState != FormWindowState.Minimized)
                _lastNonMinimizedWindowState = currentState;

            if (!_restoringWindowSettings
                && currentState != FormWindowState.Minimized
                && currentState != _lastObservedWindowState)
            {
                SaveWindowSettings();
            }

            if (currentState == FormWindowState.Normal)
                ScheduleWindowSettingsSave();

            _lastObservedWindowState = currentState;
        }
    }
}
