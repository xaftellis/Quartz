using Quartz.Services;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using System.Security.Policy;
using System.Data.SqlTypes;
using EasyTabs;
using System.Collections.Generic;
using System.Windows.Media.TextFormatting;
using Quartz.Libs;
using Win32Interop.Enums;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using Quartz.Models;

namespace Quartz
{
    public partial class Settings : Form
    {
        private bool updating;
        private int PreviousThemeSelectedIndex;
        public BirthdayService birthdayService = new BirthdayService();

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

        private DateTime CalculateEaster(int year)
        {
            int a = year % 19;
            int b = year / 100;
            int c = year % 100;
            int d = b / 4;
            int e = b % 4;
            int f = (b + 8) / 25;
            int g = (19 * a + b - d - f + 15) % 30;
            int h = c / 4;
            int i = c % 4;
            int k = (32 + 2 * e + 2 * h - g - i) % 7;
            int l = (a + 11 * g + 22 * k) / 451;
            int m = g + k - 7 * l + 114;
            int month = m / 31;
            int day = (m % 31) + 1;

            return new DateTime(year, month, day);
        }

        private DateTime CalculateGoodFriday(DateTime easterDate)
        {
            return easterDate.AddDays(-2); // Good Friday is 2 days before Easter Sunday
        }

        private void UpdateHiddenPDFSetting()
        {
            if (!cbPDFnone.Checked)
            {
                CheckBox[] checkboxes =
                {
                cbPDFsave,
                cbPDFprint,
                cbPDFsaveas,
                cbPDFzoomin,
                cbPDFzoomout,
                cbPDFrotate,
                cbPDFfitpage,
                cbPDFpagelayout,
                cbPDFbookmarks,
                cbPDFpageselector,
                cbPDFsearch,
                cbPDFfullscreen,
                cbPDFmoresettings,
                };
                _browser.wvWebView1.CoreWebView2.Settings.HiddenPdfToolbarItems = CoreWebView2PdfToolbarItems.None;
                foreach (var checkbox in checkboxes)
                {
                    if (checkbox.Checked)
                    {
                        _browser.wvWebView1.CoreWebView2.Settings.HiddenPdfToolbarItems |= (CoreWebView2PdfToolbarItems)Enum.Parse(typeof(CoreWebView2PdfToolbarItems), checkbox.Text.Replace(" ", ""));
                    }
                }
            }
            else
            {
                _browser.wvWebView1.CoreWebView2.Settings.HiddenPdfToolbarItems = CoreWebView2PdfToolbarItems.None;
            }
        }

        private void HiddenPDFItems_Checked(object sender)
        {
            CheckBox cb = (CheckBox)sender;
            if (cb.Checked)
            {
                SettingsService.Set("HiddenPDF" + cb.Text.Replace(" ", ""), "true");
            }
            else
            {
                SettingsService.Set("HiddenPDF" + cb.Text.Replace(" ", ""), "false");
            }
            UpdateHiddenPDFSetting();
        }

        private async void checkforupdates()
        {
            updating = true;

            buttonChech.Visible = false;
            txtUpdate.Visible = true;
            LoadingProgress.Visible = true;
            pictureBox1.Visible = false;

            // ----- Set throbber based on theme (4.8.1 SAFE) -----
            string theme = SettingsService.Get("Theme");
            string fileName;

            if (theme == "xmas")
                fileName = "throbber_medium_xmas_green.svg";
            else if (theme == "black")
                fileName = "throbber_medium_white.svg";
            else if (theme == "aqua")
                fileName = "throbber_medium_blue.svg";
            else
                fileName = "throbber_medium_" + theme + ".svg";

            string path = Path.Combine(Application.StartupPath, "assets", "throbber", fileName);

            LoadingProgress.Reload();
            LoadingProgress.ZoomFactor = 1;
            LoadingProgress.Source = new Uri("file://" + path);

            // ----- Animate "Checking For Updates" -----
            string baseText = "Checking For Updates";
            string[] dots = { "", ".", "..", "..." };

            for (int i = 0; i < 5; i++)  // number of animation cycles
            {
                foreach (string d in dots)
                {
                    txtUpdate.Text = baseText + d;
                    await Task.Delay(500);
                }
            }

            // ----- Restore UI -----
            updating = false;

            buttonChech.Visible = true;
            txtUpdate.Visible = false;
            LoadingProgress.Visible = false;
            pictureBox1.Visible = true;

            // ----- Error / Retry dialog -----
            DialogResult dr = MessageBox.Show(
                "Failed to connect to servers, please check your internet connection.",
                "Something Went Wrong",
                MessageBoxButtons.RetryCancel,
                MessageBoxIcon.Error);

            if (dr == DialogResult.Retry)
                checkforupdates();
        }

        private Browser _browser = null;
        bool opentab = false;
        public Settings(Browser browser, bool tab)
        {
            opentab = tab;
            _browser = browser;
            InitializeComponent();
        }


        private async void button1_Click(object sender, EventArgs e)
        {
            checkforupdates();

        }

        private async void Settings_Load(object sender, EventArgs e)
        {
            if (SettingsService.Get("DraggableForms") == "true")
            {
                MouseDragger mouseDragger = new MouseDragger(this);
            }

            if(Program.profileService.Get(ProfileService.Current).isDisposable == true)
            {
                List<string> itemList = new List<string>
                {
                    "Auto (Light/Dark)",
                    "Auto (Light/Black)",
                    "Light",
                    "Dark",
                    "Black (Default)",
                    "Aqua"
                };

                ComboBoxTheme.Items.Clear();
                ComboBoxTheme.Items.AddRange(itemList.ToArray());
            }
            else
            {
                List<string> itemList = new List<string>
                {
                    "Auto (Light/Dark) (Default)",
                    "Auto (Light/Black)",
                    "Light",
                    "Dark",
                    "Black",
                    "Aqua"
                };

                ComboBoxTheme.Items.Clear();
                ComboBoxTheme.Items.AddRange(itemList.ToArray());
            }

            CheckBox[] checkboxes =
             {
                cbPDFnone,
                cbPDFsave,
                cbPDFprint,
                cbPDFsaveas,
                cbPDFzoomin,
                cbPDFzoomout,
                cbPDFrotate,
                cbPDFfitpage,
                cbPDFpagelayout,
                cbPDFbookmarks,
                cbPDFpageselector,
                cbPDFsearch,
                cbPDFfullscreen,
                cbPDFmoresettings,
            };


            foreach (CheckBox check in checkboxes)
            {
                check.Checked = SettingsService.Get("HiddenPDF" + check.Text.Replace(" ", "")) == "true";
            }

            cbStatusBar.Checked = SettingsService.Get("IsStatusBarEnabled") == "true";

            if (SettingsService.Get("SettingsTabAlignment") == "top")
            {
                comboSettingsTabAlinement.SelectedIndex = 0;
            }
            else if (SettingsService.Get("SettingsTabAlignment") == "left")
            {
                comboSettingsTabAlinement.SelectedIndex = 1;
            }
            else if (SettingsService.Get("SettingsTabAlignment") == "right")
            {
                comboSettingsTabAlinement.SelectedIndex = 2;
            }
            else if (SettingsService.Get("SettingsTabAlignment") == "bottom")
            {
                comboSettingsTabAlinement.SelectedIndex = 3;
            }


            if (SettingsService.Get("simulateDate") == "true")
            {
                if (!string.IsNullOrEmpty(SettingsService.Get("timeMachine")))
                {
                    mcTimeMachine.AddBoldedDate(DateTime.Parse(SettingsService.Get("timeMachine")));
                    mcTimeMachine.SelectionStart = DateTime.Parse(SettingsService.Get("timeMachine"));
                    mcTimeMachine.SelectionEnd = DateTime.Parse(SettingsService.Get("timeMachine"));
                    txtTimeMachine.Text = SettingsService.Get("timeMachine");
                }
                else
                {
                    mcTimeMachine.AddBoldedDate(DateTime.Now.Date);
                    mcTimeMachine.SelectionStart = DateTime.Now.Date;
                    mcTimeMachine.SelectionEnd = DateTime.Now.Date;
                    txtTimeMachine.Text = DateTime.Now.Date.ToString("D").Replace(DateTime.Now.DayOfWeek + ", ", "");
                }

                cbtimeMachine.Checked = true;
                txtTimeMachine.Enabled = true;
                btnDown.Enabled = true;
            }
            else
            {
                if (!string.IsNullOrEmpty(SettingsService.Get("timeMachine")))
                {
                    mcTimeMachine.AddBoldedDate(DateTime.Parse(SettingsService.Get("timeMachine")));
                    mcTimeMachine.SelectionStart = DateTime.Parse(SettingsService.Get("timeMachine"));
                    mcTimeMachine.SelectionEnd = DateTime.Parse(SettingsService.Get("timeMachine"));
                    txtTimeMachine.Text = SettingsService.Get("timeMachine");
                }
                else
                {
                    mcTimeMachine.AddBoldedDate(DateTime.Now.Date);
                    mcTimeMachine.SelectionStart = DateTime.Now.Date;
                    mcTimeMachine.SelectionEnd = DateTime.Now.Date;
                    txtTimeMachine.Text = DateTime.Now.Date.ToString("D").Replace(DateTime.Now.DayOfWeek + ", ", "");
                }

                cbtimeMachine.Checked = false;
                txtTimeMachine.Enabled = false;
                btnDown.Enabled = false;
            }

            if (Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Month == 12)
            {
                var index = ComboBoxTheme.Items.Count;
                ComboBoxTheme.Items.Insert(index, "Xmas");
            }

            String theme;
            if(SettingsService.GetAutoTheme() != null)
            {
                theme = SettingsService.GetAutoTheme();
            }
            else
            {
                theme = SettingsService.Get("Theme");
            }

            if (theme == "auto (light/dark)")
            {
                ComboBoxTheme.SelectedIndex = 0;
                PreviousThemeSelectedIndex = ComboBoxTheme.SelectedIndex;
            }
            else if (theme == "auto (light/black)")
            {
                ComboBoxTheme.SelectedIndex = 1;
                PreviousThemeSelectedIndex = ComboBoxTheme.SelectedIndex;
            }
            else if (theme == "light")
            {
                ComboBoxTheme.SelectedIndex = 2;
                PreviousThemeSelectedIndex = ComboBoxTheme.SelectedIndex;
            }
            else if (theme == "dark")
            {
                ComboBoxTheme.SelectedIndex = 3;
                PreviousThemeSelectedIndex = ComboBoxTheme.SelectedIndex;
            }
            else if (theme == "black")
            {
                ComboBoxTheme.SelectedIndex = 4;
                PreviousThemeSelectedIndex = ComboBoxTheme.SelectedIndex;
            }
            else if (theme == "aqua")
            {
                ComboBoxTheme.SelectedIndex = 5;
                PreviousThemeSelectedIndex = ComboBoxTheme.SelectedIndex;
            }
            else if (theme == "xmas" && Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Month == 12)
            {
                ComboBoxTheme.SelectedIndex = 6;
                PreviousThemeSelectedIndex = ComboBoxTheme.SelectedIndex;
            }

        
            //sys webview
            var sysenv = await CoreWebView2Environment.CreateAsync(null, _browser.GetAppPath() + @"\UserData\WebView2\", null);
            var sysoptions = sysenv.CreateCoreWebView2ControllerOptions();

            if (LoadingProgress.CoreWebView2 == null)
            {
                await LoadingProgress.EnsureCoreWebView2Async(sysenv, sysoptions);
            }

            if (SettingsService.Get("MemoryUsage") == "low")
            {
                LoadingProgress.CoreWebView2.MemoryUsageTargetLevel = CoreWebView2MemoryUsageTargetLevel.Low;

            }
            else
            {
                LoadingProgress.CoreWebView2.MemoryUsageTargetLevel = CoreWebView2MemoryUsageTargetLevel.Normal;
            }


            this.Text = $"Settings - {Program.profileService.Get(ProfileService.Current).Name}";
            
            switch (SettingsService.Get("TrackingPreventionLevel"))
            {
                case "none":
                    ComboBoxTracking.SelectedIndex = 0;
                    break;

                case "basic":
                    ComboBoxTracking.SelectedIndex = 1;
                    break;

                case "balanced":
                    ComboBoxTracking.SelectedIndex = 2;
                    break;

                case "strict":
                    ComboBoxTracking.SelectedIndex = 3;
                    break;
            }

            NumZoom.Value = Convert.ToDecimal(_browser.wvWebView1.ZoomFactor * 100);
            BoxDev.Checked = SettingsService.Get("AreDevToolsEnabled") == "true";
            BoxSwipeNav.Checked = SettingsService.Get("IsSwipeNavigationEnabled") == "true";
            BoxKeys.Checked = SettingsService.Get("AreBrowserAcceleratorKeysEnabled") == "true";
            cbAnimation.Checked = SettingsService.Get("Animation") == "true";

            combDefaultFavicon.SelectedIndexChanged -= combDefaultFavicon_SelectedIndexChanged;
            if (SettingsService.Get("defaultFavicon") == "default")
            {
                combDefaultFavicon.SelectedIndex = 0;
            }
            else if (SettingsService.Get("defaultFavicon").StartsWith("custom - "))
            {
                combDefaultFavicon.SelectedIndex = 1;
            }
            combDefaultFavicon.SelectedIndexChanged += combDefaultFavicon_SelectedIndexChanged;

            if (SettingsService.Get("SearchEngine") == "google")
            {
                cbSearchEngine.SelectedIndex = 0;
                cbDHP.Visible = true;
            }
            else if (SettingsService.Get("SearchEngine") == "bing")
            {
                cbSearchEngine.SelectedIndex = 1;
                cbDHP.Visible = true;
            }
            else if (SettingsService.Get("SearchEngine") == "yahoo")
            {
                cbSearchEngine.SelectedIndex = 2;
                cbDHP.Visible = true;
            }
            else if (SettingsService.Get("SearchEngine") == "duckduckgo")
            {
                cbSearchEngine.SelectedIndex = 3;
                cbDHP.Visible = true;
            }
            else if (SettingsService.Get("SearchEngine") == "ecosia")
            {
                cbSearchEngine.SelectedIndex = 4;
                cbDHP.Visible = true;
            }
            else if (SettingsService.Get("SearchEngine") == "netflix")
            {
                cbSearchEngine.SelectedIndex = 5;
                cbDHP.Visible = true;
            }
            else if (SettingsService.Get("SearchEngine") == "youtube")
            {
                cbSearchEngine.SelectedIndex = 6;
                cbDHP.Visible = true;
            }
            else if (SettingsService.Get("SearchEngine") == "googlemaps")
            {
                cbSearchEngine.SelectedIndex = 7;
                cbDHP.Visible = true;
            }
            else if (SettingsService.Get("SearchEngine") == "wikipedia")
            {
                cbSearchEngine.SelectedIndex = 8;
                cbDHP.Visible = false;
            }
            else if (SettingsService.Get("SearchEngine") == "ebay")
            {
                cbSearchEngine.SelectedIndex = 9;
                cbDHP.Visible = false;
            }
            else if (SettingsService.Get("SearchEngine") == "amazon")
            {
                cbSearchEngine.SelectedIndex = 10;
                cbDHP.Visible = false;
            }


            cbESC.Checked = SettingsService.Get("escClose") == "true";

            pictureBox1.BackgroundImage = FaviconHelper.GetFullResDefaultFaviconAsImage();

            txtUpdate.Text = "Update"; 
            await LoadingProgress.EnsureCoreWebView2Async();
            LoadingProgress.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;

            TopLeftBox.Checked = SettingsService.Get("DownloadAlignment") == "TopLeft";
            TopRightBox.Checked = SettingsService.Get("DownloadAlignment") == "TopRight";
            BottomLeftBox.Checked = SettingsService.Get("DownloadAlignment") == "BottomLeft";
            BottomRightBox.Checked = SettingsService.Get("DownloadAlignment") == "BottomRight";
            autofillCheckBox.Checked = SettingsService.Get("IsGeneralAutofillEnabled") == "true";
            autoSaveCheckBox.Checked = SettingsService.Get("IsPasswordAutosaveEnabled") == "true";
            checkBoxMemory.Checked = SettingsService.Get("MemoryUsage") == "low";
            cbScripts.Checked = SettingsService.Get("IsScriptEnabled") == "true";
            //Zoom
            zc.Checked = SettingsService.Get("IsZoomControlEnabled") == "true";
            pz.Checked = SettingsService.Get("IsPinchZoomEnabled") == "true";
            cbDHP.Checked = SettingsService.Get("DefaultHomePage") == "true";
            cbDrag.Checked = SettingsService.Get("DraggableForms") == "true";

            if(SettingsService.Get("Theme") == "dark")
            {
                txtTimeMachine.Size = new System.Drawing.Size(204, 20);
                btnDown.Location = new System.Drawing.Point(222, 107);
                btnDown.Size = new System.Drawing.Size(22, 22);
            }
            else if (SettingsService.Get("Theme") == "light")
            {
                txtTimeMachine.Size = new System.Drawing.Size(207, 20);
                btnDown.Location = new System.Drawing.Point(224, 108);
                btnDown.Size = new System.Drawing.Size(20, 20);
            }
            else if (SettingsService.Get("Theme") == "black")
            {
                txtTimeMachine.Size = new System.Drawing.Size(207, 20);
                btnDown.Location = new System.Drawing.Point(224, 108);
                btnDown.Size = new System.Drawing.Size(20, 20);
            }
            else if (SettingsService.Get("Theme") == "aqua")
            {
                txtTimeMachine.Size = new System.Drawing.Size(208, 20);
                btnDown.Location = new System.Drawing.Point(224, 108);
                btnDown.Size = new System.Drawing.Size(20, 20);
            }
            NewControlThemeChanger.ChangeTheme(this);
            NewControlThemeChanger.ChangeControlTheme(mnuBirthdays);
        }

        private void autoSaveCheckBox_Click(object sender, EventArgs e)
        {
            if (autoSaveCheckBox.Checked)
            {
                _browser.wvWebView1.CoreWebView2.Settings.IsPasswordAutosaveEnabled = true;
                SettingsService.Set("IsPasswordAutosaveEnabled", "true");
            }
            else
            {
                _browser.wvWebView1.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;
                SettingsService.Set("IsPasswordAutosaveEnabled", "false");
            }
        }

        private void autofillCheckBox_Click(object sender, EventArgs e)
        {
            if (autofillCheckBox.Checked)
            {
                _browser.wvWebView1.CoreWebView2.Settings.IsGeneralAutofillEnabled = true;
                SettingsService.Set("IsGeneralAutofillEnabled", "true");
            }
            else
            {
                _browser.wvWebView1.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
                SettingsService.Set("IsGeneralAutofillEnabled", "false");
            }
        }

        private void TopLeftBox_CheckedChanged(object sender, EventArgs e)
        {
            if (TopLeftBox.Checked)
            {
                _browser.wvWebView1.CoreWebView2.DefaultDownloadDialogCornerAlignment = CoreWebView2DefaultDownloadDialogCornerAlignment.TopLeft;
                SettingsService.Set("DownloadAlignment", "TopLeft");
            }
        }

        private void BottomLeftBox_CheckedChanged(object sender, EventArgs e)
        {
            if (BottomLeftBox.Checked)
            {
                _browser.wvWebView1.CoreWebView2.DefaultDownloadDialogCornerAlignment = CoreWebView2DefaultDownloadDialogCornerAlignment.BottomLeft;
                SettingsService.Set("DownloadAlignment", "BottomLeft");
            }
        }

        private void TopRightBox_CheckedChanged(object sender, EventArgs e)
        {
            if (TopRightBox.Checked)
            {
                _browser.wvWebView1.CoreWebView2.DefaultDownloadDialogCornerAlignment = CoreWebView2DefaultDownloadDialogCornerAlignment.TopRight;
                SettingsService.Set("DownloadAlignment", "TopRight");
            }
        }

        private void BottomRightBox_CheckedChanged(object sender, EventArgs e)
        {
            if (BottomRightBox.Checked)
            {
                _browser.wvWebView1.CoreWebView2.DefaultDownloadDialogCornerAlignment = CoreWebView2DefaultDownloadDialogCornerAlignment.BottomRight;
                SettingsService.Set("DownloadAlignment", "BottomRight");
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (zc.Checked)
            {
                _browser.wvWebView1.CoreWebView2.Settings.IsZoomControlEnabled = true;
                SettingsService.Set("IsZoomControlEnabled", "true");
            }
            else
            {
                _browser.wvWebView1.CoreWebView2.Settings.IsZoomControlEnabled = false;
                SettingsService.Set("IsZoomControlEnabled", "false");
            }
        }

        private void pz_CheckedChanged(object sender, EventArgs e)
        {
            if (pz.Checked)
            {
                _browser.wvWebView1.CoreWebView2.Settings.IsPinchZoomEnabled = true;
                SettingsService.Set("IsPinchZoomEnabled", "true");
            }
            else
            {
                _browser.wvWebView1.CoreWebView2.Settings.IsPinchZoomEnabled = false;
                SettingsService.Set("IsPinchZoomEnabled", "false");
            }
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            var history = new History(_browser);
            history.ShowDialog();
        }

        private void BoxDev_CheckedChanged(object sender, EventArgs e)
        {
            if (BoxDev.Checked)
            {
                _browser.wvWebView1.CoreWebView2.Settings.AreDevToolsEnabled = true;
                SettingsService.Set("AreDevToolsEnabled", "true");
            }
            else
            {
                _browser.wvWebView1.CoreWebView2.Settings.AreDevToolsEnabled = false;
                SettingsService.Set("AreDevToolsEnabled", "false");
            }
        }

        private void BoxSwipeNav_CheckedChanged(object sender, EventArgs e)
        {
            if (BoxSwipeNav.Checked)
            {
                _browser.wvWebView1.CoreWebView2.Settings.IsSwipeNavigationEnabled = true;
                SettingsService.Set("IsSwipeNavigationEnabled", "true");
            }
            else
            {
                _browser.wvWebView1.CoreWebView2.Settings.IsSwipeNavigationEnabled = false;
                SettingsService.Set("IsSwipeNavigationEnabled", "false");
            }
        }

        private void BoxKeys_CheckedChanged(object sender, EventArgs e)
        {
            if (BoxKeys.Checked)
            {
                _browser.wvWebView1.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = true;
                SettingsService.Set("AreBrowserAcceleratorKeysEnabled", "true");
            }
            else
            {
                _browser.wvWebView1.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
                SettingsService.Set("AreBrowserAcceleratorKeysEnabled", "false");
            }
        }

        private void NumZoom_ValueChanged(object sender, EventArgs e)
        {
            _browser.wvWebView1.ZoomFactor = Convert.ToDouble(NumZoom.Value / 100);
            SettingsService.Set("Zoom", (NumZoom.Value / 100).ToString());
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ComboBoxTracking.SelectedIndex == 0)
            {
                SettingsService.Set("TrackingPreventionLevel", "none");
                _browser.wvWebView1.CoreWebView2.Profile.PreferredTrackingPreventionLevel = CoreWebView2TrackingPreventionLevel.None;
            }
            else if (ComboBoxTracking.SelectedIndex == 1)
            {
                SettingsService.Set("TrackingPreventionLevel", "basic");
                _browser.wvWebView1.CoreWebView2.Profile.PreferredTrackingPreventionLevel = CoreWebView2TrackingPreventionLevel.Basic;
            }
            else if (ComboBoxTracking.SelectedIndex == 2)
            {
                SettingsService.Set("TrackingPreventionLevel", "balanced");
                _browser.wvWebView1.CoreWebView2.Profile.PreferredTrackingPreventionLevel = CoreWebView2TrackingPreventionLevel.Balanced;
            }
            else if (ComboBoxTracking.SelectedIndex == 3)
            {
                SettingsService.Set("TrackingPreventionLevel", "strict");
                _browser.wvWebView1.CoreWebView2.Profile.PreferredTrackingPreventionLevel = CoreWebView2TrackingPreventionLevel.Strict;
            }
        }

        private void ComboBoxTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            String currenttheme;
            if (SettingsService.GetAutoTheme() != null)
            {
                currenttheme = SettingsService.GetAutoTheme();
            }
            else
            {
                currenttheme = SettingsService.Get("Theme");
            }

            if (ComboBoxTheme.SelectedIndex == 0 && currenttheme != "auto (light/dark)")
            {
                if (MessageBox.Show("This action will require a restart, are you sure you want to continue?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var theme = "auto (light/dark)";
                    _browser.ChangeTheme(theme);

                    if (Program.profileService.Get(ProfileService.Current).isDisposable)
                    {
                        Program.profileService.Get(ProfileService.Current).endSession = false;
                        Program.profileService.SaveChanges();
                    }

                    Program._bypassPassword = true;
                    MainSettingsService.Set("RunBrowser", "true");
                    Power.Restart();
                }
                else
                {
                    ComboBoxTheme.SelectedIndex = PreviousThemeSelectedIndex;
                }
            }
            else if (ComboBoxTheme.SelectedIndex == 1 && currenttheme != "auto (light/black)")
            {
                if (MessageBox.Show("This action will require a restart, are you sure you want to continue?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var theme = "auto (light/black)";
                    _browser.ChangeTheme(theme);
                    if (Program.profileService.Get(ProfileService.Current).isDisposable)
                    {
                        Program.profileService.Get(ProfileService.Current).endSession = false;
                        Program.profileService.SaveChanges();
                    }

                    Program._bypassPassword = true;
                    MainSettingsService.Set("RunBrowser", "true");
                    Power.Restart();
                }
                else
                {
                    ComboBoxTheme.SelectedIndex = PreviousThemeSelectedIndex;
                }
            }
            else if (ComboBoxTheme.SelectedIndex == 2 && currenttheme != "light")
            {
                if (MessageBox.Show("This action will require a restart, are you sure you want to continue?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var theme = "light";
                    _browser.ChangeTheme(theme);

                    if (Program.profileService.Get(ProfileService.Current).isDisposable)
                    {
                        Program.profileService.Get(ProfileService.Current).endSession = false;
                        Program.profileService.SaveChanges();
                    }

                    Program._bypassPassword = true;
                    MainSettingsService.Set("RunBrowser", "true");
                    Power.Restart();
                }
                else
                {
                    ComboBoxTheme.SelectedIndex = PreviousThemeSelectedIndex;
                }
            }
            else if (ComboBoxTheme.SelectedIndex == 3 && currenttheme != "dark")
            {
                if (MessageBox.Show("This action will require a restart, are you sure you want to continue?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var theme = "dark";
                    _browser.ChangeTheme(theme);

                    if (Program.profileService.Get(ProfileService.Current).isDisposable)
                    {
                        Program.profileService.Get(ProfileService.Current).endSession = false;
                        Program.profileService.SaveChanges();
                    }

                    Program._bypassPassword = true;
                    MainSettingsService.Set("RunBrowser", "true");
                    Power.Restart();
                }
                else
                {
                    ComboBoxTheme.SelectedIndex = PreviousThemeSelectedIndex;
                }
            }
            else if (ComboBoxTheme.SelectedIndex == 4 && currenttheme != "black")
            {
                if (MessageBox.Show("This action will require a restart, are you sure you want to continue?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var theme = "black";
                    _browser.ChangeTheme(theme);

                    if (Program.profileService.Get(ProfileService.Current).isDisposable)
                    {
                        Program.profileService.Get(ProfileService.Current).endSession = false;
                        Program.profileService.SaveChanges();
                    }

                    Program._bypassPassword = true;
                    MainSettingsService.Set("RunBrowser", "true");
                    Power.Restart();
                }
                else
                {
                    ComboBoxTheme.SelectedIndex = PreviousThemeSelectedIndex;
                }
            }
            else if (ComboBoxTheme.SelectedIndex == 5 && currenttheme != "aqua")
            {
                if (MessageBox.Show("This action will require a restart, are you sure you want to continue?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var theme = "aqua";
                    _browser.ChangeTheme(theme);

                    if (Program.profileService.Get(ProfileService.Current).isDisposable)
                    {
                        Program.profileService.Get(ProfileService.Current).endSession = false;
                        Program.profileService.SaveChanges();
                    }

                    Program._bypassPassword = true;
                    MainSettingsService.Set("RunBrowser", "true");
                    Power.Restart();
                }
                else
                {
                    ComboBoxTheme.SelectedIndex = PreviousThemeSelectedIndex;
                }
            }
            else if (ComboBoxTheme.SelectedIndex == 6 && Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Month == 12 && currenttheme != "xmas")
            {
                if (MessageBox.Show("This action will require a restart, are you sure you want to continue?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var theme = "xmas";
                    _browser.ChangeTheme(theme);

                    if (Program.profileService.Get(ProfileService.Current).isDisposable)
                    {
                        Program.profileService.Get(ProfileService.Current).endSession = false;
                        Program.profileService.SaveChanges();
                    }

                    Program._bypassPassword = true;
                    MainSettingsService.Set("RunBrowser", "true");
                    Power.Restart();
                }
                else
                {
                    ComboBoxTheme.SelectedIndex = PreviousThemeSelectedIndex;
                }
            }
        }

        private void cbAnimation_CheckStateChanged(object sender, EventArgs e)
        {
            if (cbAnimation.Checked)
            {
                SettingsService.Set("Animation", "true");
            }
            else
            {
                SettingsService.Set("Animation", "false");
            }
        }

        private void cbDHP_CheckedChanged(object sender, EventArgs e)
        {
            if (cbDHP.Checked)
            {
                SettingsService.Set("DefaultHomePage", "true");
            }
            else
            {
                SettingsService.Set("DefaultHomePage", "false");
            }
        }

        private void cbSearchEngine_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cbSearchEngine.SelectedIndex == 0)
            {
                SettingsService.Set("SearchEngine", "google");
                cbDHP.Visible = true;
            }
            else if (cbSearchEngine.SelectedIndex == 1)
            {
                SettingsService.Set("SearchEngine", "bing");
                cbDHP.Visible = true;
            }
            else if (cbSearchEngine.SelectedIndex == 2)
            {
                SettingsService.Set("SearchEngine", "yahoo");
                cbDHP.Visible = true;
            }
            else if (cbSearchEngine.SelectedIndex == 3)
            {
                SettingsService.Set("SearchEngine", "duckduckgo");
                cbDHP.Visible = true;
            }
            else if (cbSearchEngine.SelectedIndex == 4)
            {
                SettingsService.Set("SearchEngine", "ecosia");
                cbDHP.Visible = true;
            }
            else if (cbSearchEngine.SelectedIndex == 5)
            {
                SettingsService.Set("SearchEngine", "netflix");
                cbDHP.Visible = true;
            }
            else if (cbSearchEngine.SelectedIndex == 6)
            {
                SettingsService.Set("SearchEngine", "youtube");
                cbDHP.Visible = true;
            }
            else if (cbSearchEngine.SelectedIndex == 7)
            {
                SettingsService.Set("SearchEngine", "googlemaps");
                cbDHP.Visible = true;
            }
            else if (cbSearchEngine.SelectedIndex == 8)
            {
                SettingsService.Set("SearchEngine", "wikipedia");
                cbDHP.Visible = false;
                SettingsService.Set("DefaultHomePage", "false");
            }
            //else if (cbSearchEngine.SelectedIndex == 9)
            //{
            //    SettingsService.Set("SearchEngine", "favicon");
            //    cbDHP.Checked = false;
            //    cbDHP.Visible = false;
            //    SettingsService.Set("DefaultHomePage", "false");
            //}
            else if (cbSearchEngine.SelectedIndex == 9)
            {
                SettingsService.Set("SearchEngine", "ebay");
                cbDHP.Visible = false;
                SettingsService.Set("DefaultHomePage", "false");
            }
            else if (cbSearchEngine.SelectedIndex == 10)
            {
                SettingsService.Set("SearchEngine", "amazon");
                cbDHP.Visible = false;
                SettingsService.Set("DefaultHomePage", "false");
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBoxMemory.Checked)
            {
                SettingsService.Set("MemoryUsage", "low");
                _browser.wvWebView1.CoreWebView2.MemoryUsageTargetLevel = CoreWebView2MemoryUsageTargetLevel.Low;
                _browser.wvLoadingProgress.CoreWebView2.MemoryUsageTargetLevel = CoreWebView2MemoryUsageTargetLevel.Low;
                LoadingProgress.CoreWebView2.MemoryUsageTargetLevel = CoreWebView2MemoryUsageTargetLevel.Low;
            }
            else
            {
                SettingsService.Set("MemoryUsage", "normal");
                _browser.wvWebView1.CoreWebView2.MemoryUsageTargetLevel = CoreWebView2MemoryUsageTargetLevel.Normal;
                _browser.wvLoadingProgress.CoreWebView2.MemoryUsageTargetLevel = CoreWebView2MemoryUsageTargetLevel.Normal;
                LoadingProgress.CoreWebView2.MemoryUsageTargetLevel = CoreWebView2MemoryUsageTargetLevel.Normal;
            }
        }

        //private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    //if (tabControl1.SelectedIndex == 5 && LoadingProgress.Visible)
        //    //{
        //    //    string url;
        //    //    var theme = SettingsService.Get("Theme");
        //    //    if (theme == "xmas")
        //    //    {
        //    //        url = "file://" + Path.Combine(new string[] { Application.StartupPath + "\\assets\\throbber\\", "throbber_medium_xmas_green.svg" });
        //    //    }
        //    //    else if (theme == "black")
        //    //    {
        //    //        url = "file://" + Path.Combine(new string[] { Application.StartupPath + "\\assets\\throbber\\", "throbber_medium_white.svg" });
        //    //    }
        //    //    else if (theme == "aqua")
        //    //    {
        //    //        url = "file://" + Path.Combine(new string[] { Application.StartupPath + "\\assets\\throbber\\", "throbber_medium_blue.svg" });
        //    //    }
        //    //    else
        //    //    {
        //    //        url = "file://" + Path.Combine(new string[] { Application.StartupPath + "\\assets\\throbber\\", $"throbber_medium_{SettingsService.Get("Theme")}.svg" });
        //    //    }

        //    //    if (!LoadingProgress.Source.AbsoluteUri.EndsWith($"throbber_medium_{SettingsService.Get("Theme")}.svg"))
        //    //    {
        //    //        if (theme == "xmas")
        //    //        {
        //    //            LoadingProgress.Reload();
        //    //            LoadingProgress.ZoomFactor = 1;
        //    //            LoadingProgress.Source = new Uri(url);
        //    //        }
        //    //        else if (theme == "black")
        //    //        {
        //    //            LoadingProgress.Reload();
        //    //            LoadingProgress.ZoomFactor = 1;
        //    //            LoadingProgress.Source = new Uri(url);
        //    //        }
        //    //        else
        //    //        {
        //    //            LoadingProgress.Reload();
        //    //            LoadingProgress.ZoomFactor = 1;
        //    //            LoadingProgress.Source = new Uri(url);
        //    //        }
        //    //    }
        //    //}
        //}

        private void Settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            _browser.Shortcuts(true);
            if (!cbPDFnone.Checked
                && !cbPDFsave.Checked
                && !cbPDFprint.Checked
                && !cbPDFsaveas.Checked
                && !cbPDFzoomout.Checked
                && !cbPDFrotate.Checked
                && !cbPDFfitpage.Checked
                && !cbPDFpagelayout.Checked
                && !cbPDFbookmarks.Checked
                && !cbPDFpageselector.Checked
                && !cbPDFsearch.Checked
                && !cbPDFfullscreen.Checked
                && !cbPDFmoresettings.Checked)
            {
                foreach (Control control in HiddenPDFGroupBox.Controls)
                {
                    if (control is CheckBox checkBox)
                    {
                        if (control.Text == "None")
                        {
                            checkBox.Enabled = true;
                            checkBox.Checked = true;
                            SettingsService.Set("HiddenPDFNone", "true");
                        }
                        else
                        {
                            checkBox.Enabled = false;
                            SettingsService.Set("HiddenPDF" + control.Text.Replace(" ", ""), "false");
                        }
                    }
                }
            }
        }

        private void cbDrag_CheckedChanged(object sender, EventArgs e)
        {
            if(cbDrag.Checked)
            {
                SettingsService.Set("DraggableForms", "true");
            }
            else
            {
                SettingsService.Set("DraggableForms", "false");
            }
        }

        private void mcTimeMachine_DateChanged(object sender, DateRangeEventArgs e)
        {
            mcTimeMachine.AddBoldedDate(e.Start);
            mcTimeMachine.SelectionStart = e.Start;
            mcTimeMachine.SelectionEnd = e.Start;

            if (SettingsService.Get("timeMachine") != mcTimeMachine.SelectionStart.ToString("D").Replace(mcTimeMachine.SelectionStart.DayOfWeek + ", ", ""))
            {
                txtTimeMachine.Text = mcTimeMachine.SelectionStart.ToString("D").Replace(mcTimeMachine.SelectionStart.DayOfWeek + ", ", "");

                SettingsService.Set("timeMachine", mcTimeMachine.SelectionStart.ToString("D").Replace(mcTimeMachine.SelectionStart.DayOfWeek + ", ", ""));
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if(!mcTimeMachine.Visible)
            {
                if (SettingsService.Get("Animation") == "true")
                {
                    Animation.AnimateWindow(mcTimeMachine.Handle, 250, Animation.AW_SLIDE | Animation.AW_VER_POSITIVE);
                    mcTimeMachine.Visible = true;
                    btnDown.Text = "▲";
                }
                else
                {
                    mcTimeMachine.Visible = true;
                    btnDown.Text = "▲";
                }
            }
            else
            {
                if (SettingsService.Get("Animation") == "true")
                {
                    Animation.AnimateWindow(mcTimeMachine.Handle, 250, Animation.AW_SLIDE | Animation.AW_VER_NEGATIVE | Animation.AW_HIDE);
                    mcTimeMachine.Visible = false;
                    btnDown.Text = "▼";
                }
                else
                {
                    mcTimeMachine.Visible = false;
                    btnDown.Text = "▼";
                }
            }
        }
        private void txtTimeMachine_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                try
                {
                    if (txtTimeMachine.Text != mcTimeMachine.SelectionStart.ToString("D").Replace(mcTimeMachine.SelectionStart.DayOfWeek + ", ", ""))
                    {
                        mcTimeMachine.SelectionStart = DateTime.Parse(txtTimeMachine.Text);
                    }
                }
                catch
                {
                    txtTimeMachine.Text = SettingsService.Get("timeMachine");
                    mcTimeMachine.SelectionStart = DateTime.Parse(SettingsService.Get("timeMachine"));
                }
            }
        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {
            if (cbtimeMachine.Checked)
            {
                SettingsService.Set("simulateDate", "true");
                txtTimeMachine.Enabled = true;
                btnDown.Enabled = true;

                if(SettingsService.Get("timeMachine") == null)
                {
                    SettingsService.Set("timeMachine", DateTime.Now.ToString("D").Replace(DateTime.Now.DayOfWeek + ", ", ""));
                }
            }
            else
            {
                SettingsService.Set("simulateDate", "false");
                if (mcTimeMachine.Visible == true)
                {
                    if (SettingsService.Get("Animation") == "true")
                    {
                        Animation.AnimateWindow(mcTimeMachine.Handle, 250, Animation.AW_SLIDE | Animation.AW_VER_NEGATIVE | Animation.AW_HIDE);
                        mcTimeMachine.Visible = false;
                        btnDown.Text = "▼";
                    }
                    else
                    {
                        mcTimeMachine.Visible = false;
                        btnDown.Text = "▼";
                    }
                }
                txtTimeMachine.Enabled = false;
                btnDown.Enabled = false;
            }
        }
        private void cbESC_CheckedChanged(object sender, EventArgs e)
        {
            if (cbESC.Checked)
            {
                SettingsService.Set("escClose", "true");
            }
            else
            {
                SettingsService.Set("escClose", "false");
            }
        }

        private void cbScripts_CheckedChanged(object sender, EventArgs e)
        {
            _browser.wvWebView1.CoreWebView2.Settings.IsScriptEnabled = cbScripts.Checked;
            SettingsService.Set("IsScriptEnabled", cbScripts.Checked.ToString().ToLower());
        }

        private void cbStatusBar_CheckedChanged(object sender, EventArgs e)
        {
            _browser.wvWebView1.CoreWebView2.Settings.IsStatusBarEnabled = cbStatusBar.Checked;
            SettingsService.Set("IsStatusBarEnabled", cbStatusBar.Checked.ToString().ToLower());
        }

        private void cbPDFnone_CheckedChanged(object sender, EventArgs e)
        {
            if (cbPDFnone.Checked)
            {
                SettingsService.Set("HiddenPDFNone", "true");
                foreach (Control control in HiddenPDFGroupBox.Controls)
                {
                    if (control is CheckBox checkBox)
                    {
                        if (checkBox.Text != "None")
                        {
                            checkBox.Checked = false;
                            checkBox.Enabled = false;
                            SettingsService.Set("HiddenPDF" + control.Text.Replace(" ", ""), "false");
                        }
                    }
                }
            }
            else
            {
                SettingsService.Set("HiddenPDFNone", "false;");
                foreach (Control control in HiddenPDFGroupBox.Controls)
                {
                    if (control is CheckBox checkBox)
                    {
                        if (checkBox.Text != "None")
                        {
                            checkBox.Enabled = true;
                        }
                    }
                }
            }
            UpdateHiddenPDFSetting();
        }

        private void cbPDFmoresettings_CheckedChanged(object sender, EventArgs e)
        {
            HiddenPDFItems_Checked(sender);
        }

        private void cbPDFsave_CheckedChanged(object sender, EventArgs e)
        {
            HiddenPDFItems_Checked(sender);
        }

        private void cbPDFprint_CheckedChanged(object sender, EventArgs e)
        {
            HiddenPDFItems_Checked(sender);
        }

        private void cbPDFsaveas_CheckedChanged(object sender, EventArgs e)
        {
            HiddenPDFItems_Checked(sender);
        }

        private void cbPDFzoomin_CheckedChanged(object sender, EventArgs e)
        {
            HiddenPDFItems_Checked(sender);
        }

        private void cbPDFzoomout_CheckedChanged(object sender, EventArgs e)
        {
            HiddenPDFItems_Checked(sender);
        }

        private void cbPDFrotate_CheckedChanged(object sender, EventArgs e)
        {
            HiddenPDFItems_Checked(sender);
        }

        private void cbPDFfitpage_CheckedChanged(object sender, EventArgs e)
        {
            HiddenPDFItems_Checked(sender);
        }

        private void cbPDFpagelayout_CheckedChanged(object sender, EventArgs e)
        {
            HiddenPDFItems_Checked(sender);
        }

        private void cbPDFbookmarks_CheckedChanged(object sender, EventArgs e)
        {
            HiddenPDFItems_Checked(sender);
        }

        private void cbPDFpageselector_CheckedChanged(object sender, EventArgs e)
        {
            HiddenPDFItems_Checked(sender);
        }

        private void cbPDFsearch_CheckedChanged(object sender, EventArgs e)
        {
            HiddenPDFItems_Checked(sender);
        }

        private void cbPDFfullscreen_CheckedChanged(object sender, EventArgs e)
        {
            HiddenPDFItems_Checked(sender);
        }

        private void todayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mcTimeMachine.SelectionStart = DateTime.Now;
        }

        private void christmasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime currentDate = DateTime.Now;
            string target = "25 December ";
            int currentYear = DateTime.Now.Year;
            int nextYear = currentYear + 1;

            DateTime dateTimeCurrent = DateTime.Parse(target + currentYear);
            DateTime dateTimeNext = DateTime.Parse(target + nextYear);

            if (currentDate.Date == dateTimeCurrent.Date)
            {
                mcTimeMachine.SelectionStart = DateTime.Now.Date;
                return;
            }

            if (currentDate < dateTimeCurrent)
            {
                mcTimeMachine.SelectionStart = dateTimeCurrent;
            }
            else
            {
                mcTimeMachine.SelectionStart = dateTimeNext;
            }
        }

        private void goodFridayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime currentDate = DateTime.Now;
            int currentYear = DateTime.Now.Year;
            int nextYear = currentYear + 1;

            DateTime currentgoodFriday = CalculateGoodFriday(CalculateEaster(currentYear));
            DateTime nextgoodFriday = CalculateGoodFriday(CalculateEaster(nextYear));

            if (currentDate.Date == currentgoodFriday.Date)
            {
                return;
            }

            if (currentDate < currentgoodFriday)
            {
                mcTimeMachine.SelectionStart = currentgoodFriday;
            }
            else
            {
                mcTimeMachine.SelectionStart = nextgoodFriday;
            }
        }

        private void easterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime currentDate = DateTime.Now;
            int currentYear = DateTime.Now.Year;
            int nextYear = currentYear + 1;

            DateTime currentEaster = CalculateEaster(currentYear);
            DateTime nextEaster= CalculateEaster(nextYear);

            if (currentDate.Date == currentEaster.Date)
            {
                return;
            }

            if (currentDate < currentEaster)
            {
                mcTimeMachine.SelectionStart = currentEaster;
            }
            else
            {
                mcTimeMachine.SelectionStart = nextEaster;
            }
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (SettingsService.Get("Animation") == "true")
            {
                Animation.AnimateWindow(mnuTimeMachine.Handle, 100, Animation.AW_BLEND);
            }
            
            if(birthdayService.All().Count != 0)
            {
                birthdaysToolStripMenuItem.DropDown = mnuBirthdays;
                birthdaysToolStripMenuItem.Text = "Birthdays";
                birthdaysToolStripMenuItem.Click += null;
            }
            else
            {
                birthdaysToolStripMenuItem.DropDown = null;
                birthdaysToolStripMenuItem.Text = "Add birthday";
                birthdaysToolStripMenuItem.Click += ToolStripMenu_Click;

            }
        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Power.Restart();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void txtUpdate_Click(object sender, EventArgs e)
        {

        }

        private void comboSettingsTabAlinement_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboSettingsTabAlinement.SelectedIndex == 0)
            {
                SettingsService.Set("SettingsTabAlignment", "top");
                tabControl1.SizeMode = TabSizeMode.Fixed;
                tabControl1.Alignment = TabAlignment.Top;
            }
            else if (comboSettingsTabAlinement.SelectedIndex == 1)
            {
                SettingsService.Set("SettingsTabAlignment", "left");
                tabControl1.SizeMode = TabSizeMode.Normal;
                tabControl1.Alignment = TabAlignment.Left;
            }
            else if (comboSettingsTabAlinement.SelectedIndex == 2)
            {
                SettingsService.Set("SettingsTabAlignment", "right");
                tabControl1.SizeMode = TabSizeMode.Normal;
                tabControl1.Alignment = TabAlignment.Right;
            }
            else if (comboSettingsTabAlinement.SelectedIndex == 3)
            {
                SettingsService.Set("SettingsTabAlignment", "bottom");
                tabControl1.SizeMode = TabSizeMode.Fixed;
                tabControl1.Alignment = TabAlignment.Bottom;
            }
        }

        private void mnuBirthdays_Opening(object sender, CancelEventArgs e)
        {
            mnuBirthdays.Items.Clear();

            ToolStripMenuItem toolStripMenu = new ToolStripMenuItem();
            toolStripMenu.Text = "Add birthday";

            toolStripMenu.Click += ToolStripMenu_Click;

            ToolStripSeparator toolStripSeparator = new ToolStripSeparator();

            mnuBirthdays.Items.Add(toolStripMenu);
            mnuBirthdays.Items.Add(toolStripSeparator);

            foreach(BirthdayModel model in birthdayService.All())
            {
                ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
                toolStripMenuItem.Text = model.Name + "'s Birthday";
                toolStripMenuItem.Tag = model.DOB;

                string theme = SettingsService.Get("Theme");
                Image image = Properties.Resources.Close;

                if (theme == "light")
                {
                    image = Properties.Resources.Close;
                }
                else if (theme == "dark")
                {
                    image = Properties.Resources.Tabs_Close;
                }
                else if (theme == "black")
                {
                    image = Properties.Resources.B_Close;
                }
                else if (theme == "aqua")
                {
                    image = Properties.Resources.Aqua_Close;
                }
                else if (theme == "xmas")
                {
                    image = Properties.Resources.Close_xmas;
                }
                toolStripMenuItem.Image = image;

                toolStripMenuItem.MouseDown += (s, c) =>
                {
                    ToolStripMenuItem item = (ToolStripMenuItem)s;

                    // Image area is usually on the left
                    if (c.X < item.Image.Width + 5) // adjust padding if needed
                    {
                        birthdayService.Remove(model.Id);
                        birthdayService.SaveChanges();
                        mnubClose = false;
                    }
                    else
                    {
                        toolStripMenuItem.Click += ToolStripMenuItem_Click;
                    }
                };


                mnuBirthdays.Items.Add(toolStripMenuItem);

            }

            //last
            if (SettingsService.Get("Animation") == "true")
            {
                Animation.AnimateWindow((sender as ContextMenuStrip).Handle, 100, Animation.AW_BLEND);
            }
        }


        private void ToolStripMenu_Click(object sender, EventArgs e)
        {
            AddBirthday addBirthdayForm = new AddBirthday(this);
            addBirthdayForm.ShowDialog();
        }

        private void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime currentDate = DateTime.Now;

            DateTime dateTime = ((DateTime)((ToolStripMenuItem)sender).Tag);
            string target = dateTime.ToString("D")
                .Replace(dateTime.DayOfWeek + ", ", "")
                .Replace(dateTime.Year.ToString(), "");
            int currentYear = DateTime.Now.Year;
            int nextYear = currentYear + 1;

            DateTime dateTimeCurrent = DateTime.Parse(target + currentYear);
            DateTime dateTimeNext = DateTime.Parse(target + nextYear);

            if (currentDate.Date == dateTimeCurrent.Date)
            {
                mcTimeMachine.SelectionStart = DateTime.Now.Date;
                return;
            }

            if (currentDate < dateTimeCurrent)
            {
                mcTimeMachine.SelectionStart = dateTimeCurrent;
            }
            else
            {
                mcTimeMachine.SelectionStart = dateTimeNext;
            }
        }

        private async Task CDFSelectedIndexChanged()
        {
            string directory = Path.Combine(Application.StartupPath, "UserData", "pictures");
            string filename = $"{ProfileService.Current}.ico";
            string smallIconFilename = $"16_{ProfileService.Current}.ico";
            string path = Path.Combine(directory, filename);

            if (combDefaultFavicon.SelectedIndex == 0)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                SettingsService.Set("defaultFavicon", "default");
            }
            else if (combDefaultFavicon.SelectedIndex == 1)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Default Favicon";
                openFileDialog.Filter = "Icon Files (*.ico)|*.ico";
                openFileDialog.Multiselect = false;
                openFileDialog.ShowDialog();

                if (File.Exists(openFileDialog.FileName))
                {
                    Icon icon = new Icon(openFileDialog.FileName);

                    if (icon.Size != new Size(16, 16))
                    {

                        MessageBox.Show("Invalid icon - dimensions must be 16x16 pixels.", "Invalid Icon", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        CDFSelectedIndexChanged();
                        return;
                    }

                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }


                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        icon.Save(fs);
                    }

                    if (File.Exists(path))
                    {
                        SettingsService.Set("defaultFavicon", $"custom - {path}");
                    }
                }
            }
        }


        private void combDefaultFavicon_SelectedIndexChanged(object sender, EventArgs e)
        {
            CDFSelectedIndexChanged();
        }

        private void Settings_KeyUp(object sender, KeyEventArgs e)
        {
            if (SettingsService.Get("escClose") == "true")
            {
                if (e.KeyCode == Keys.Escape)
                {
                    this.Close();
                }
            }
        }

        bool mnubClose = true;
        private async void mnuBirthdays_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (!mnubClose)
            {
                if (birthdayService.All().Count == 0)
                {

                    mnubClose = false;
                    return;
                }

                e.Cancel = true;
                    mnuBirthdays_Opening(sender, e);

                    //WAITS
                    await Task.Delay(100);
                    mnubClose = true;
            }
        }

        private async void mnuTimeMachine_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (!mnubClose)
            {
                if (birthdayService.All().Count == 0)
                {
                    mnubClose = false;
                    return;
                }

                e.Cancel = true;

                //WAITS
                await Task.Delay(100);
                mnubClose = true;
            }
        }
    }
}
