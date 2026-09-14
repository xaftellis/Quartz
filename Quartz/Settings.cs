using EasyTabs;
using ImageMagick;
using Microsoft.SqlServer.Server;
using Microsoft.Web.WebView2.Core;
using Quartz.Libs;
using Quartz.Models;
using Quartz.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.TextFormatting;

namespace Quartz
{
    public partial class Settings : Form
    {
        private bool updating;
        private BirthdayService _birthdayService;
        public BirthdayService birthdayService => _birthdayService ?? (_birthdayService = new BirthdayService());

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
            if (_loadingSettings) return;
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

        
        private Browser _browser = null;
        public Settings(Browser browser)
        {
            _browser = browser;
            InitializeComponent();
            DoubleBuffered = true;
            _browser.wvWebView1.CoreWebView2InitializationCompleted += BrowserSettingsReady;
            Disposed += (sender, args) =>
            {
                updating = false;
                _browser.wvWebView1.CoreWebView2InitializationCompleted -= BrowserSettingsReady;
            };
        }

        public event EventHandler QuartzUpdaterClosed;
        private void Settings_Load(object sender, EventArgs e)
        {
            PrepareSettingsForm();
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

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadingSettings) return;
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
            if (_loadingSettings) return;
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
            if (_loadingSettings) return;
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
            if (_loadingSettings) return;
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
            if (_loadingSettings) return;
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
            if (_loadingSettings) return;
            _browser.wvWebView1.ZoomFactor = Convert.ToDouble(NumZoom.Value / 100);
            SettingsService.Set("Zoom", (NumZoom.Value / 100).ToString());
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loadingSettings) return;
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
            if (_loadingSettings) return;
            string[] themes = { "auto (light/dark)", "auto (light/black)", "light", "dark", "black", "aqua", "xmas" };
            int index = ComboBoxTheme.SelectedIndex;
            if (index < 0 || index >= themes.Length) return;
            string preference = themes[index];
            if (preference == (SettingsService.GetAutoTheme() ?? SettingsService.Get("Theme"))) return;
            _browser.ChangeTheme(preference);
        }

        internal void ApplyLiveTheme()
        {
            if (!_settingsLoaded) return;
            _loadingSettings = true;
            SuspendLayout();
            try
            {
                NewControlThemeChanger.ChangeTheme(this);
                NewControlThemeChanger.ChangeControlTheme(mnuBirthdays);
                NewControlThemeChanger.ChangeControlTheme(contextMenuStrip1);
                LoadThemeSelection();
                ApplyTimeMachineLayout();
                var previous = pictureBox1.BackgroundImage;
                pictureBox1.BackgroundImage = FaviconHelper.GetFullResDefaultFaviconAsImage();
                previous?.Dispose();
                if (LoadingProgress.CoreWebView2 != null) ApplyUpdateIndicatorTheme();
            }
            finally
            {
                ResumeLayout(true);
                _loadingSettings = false;
            }
        }

        private void cbDHP_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadingSettings) return;
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
            if (_loadingSettings) return;
            if (cbSearchEngine.SelectedIndex == 0)
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
            if (_loadingSettings) return;
            SettingsService.Set("MemoryUsage", checkBoxMemory.Checked ? "low" : "normal");
            var target = checkBoxMemory.Checked ? CoreWebView2MemoryUsageTargetLevel.Low : CoreWebView2MemoryUsageTargetLevel.Normal;
            foreach (var view in new[] { _browser.wvWebView1, _browser.wvLoadingProgress, LoadingProgress })
            {
                if (!view.IsDisposed && view.CoreWebView2 != null)
                    view.CoreWebView2.MemoryUsageTargetLevel = target;
            }
        }
        private void Settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            updating = false;
            _browser.Shortcuts(true);
            if (_loadingSettings) return;
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
            if (_loadingSettings) return;
            if (cbDrag.Checked)
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
            if (_loadingSettings) return;
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
            if (!mcTimeMachine.Visible)
            {
                Animation.AnimateWindow(mcTimeMachine.Handle, 250, Animation.AW_SLIDE | Animation.AW_VER_POSITIVE);
                mcTimeMachine.Visible = true;
                btnDown.Text = "▲";
            }
            else
            {
                Animation.AnimateWindow(mcTimeMachine.Handle, 250, Animation.AW_SLIDE | Animation.AW_VER_NEGATIVE | Animation.AW_HIDE);
                mcTimeMachine.Visible = false;
                btnDown.Text = "▼";
            }
        }
        private void txtTimeMachine_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
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
            if (_loadingSettings) return;
            if (cbtimeMachine.Checked)
            {
                SettingsService.Set("simulateDate", "true");
                txtTimeMachine.Enabled = true;
                btnDown.Enabled = true;

                if (SettingsService.Get("timeMachine") == null)
                {
                    SettingsService.Set("timeMachine", DateTime.Now.ToString("D").Replace(DateTime.Now.DayOfWeek + ", ", ""));
                }
            }
            else
            {
                if (mcTimeMachine.Visible == true)
                {
                    Animation.AnimateWindow(mcTimeMachine.Handle, 250, Animation.AW_SLIDE | Animation.AW_VER_NEGATIVE | Animation.AW_HIDE);
                    mcTimeMachine.Visible = false;
                    btnDown.Text = "▼";
                }
                txtTimeMachine.Enabled = false;
                btnDown.Enabled = false;

                //reset UI to today
                SettingsService.Set("simulateDate", "false");
                txtTimeMachine.Text = DateTime.Now.ToString("D").Replace(DateTime.Now.DayOfWeek + ", ", "");
                mcTimeMachine.SelectionStart = DateTime.Now;
            }
        }
        private void cbESC_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadingSettings) return;
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
            if (_loadingSettings) return;
            _browser.wvWebView1.CoreWebView2.Settings.IsScriptEnabled = cbScripts.Checked;
            SettingsService.Set("IsScriptEnabled", cbScripts.Checked.ToString().ToLower());
        }

        private void cbStatusBar_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadingSettings) return;
            _browser.wvWebView1.CoreWebView2.Settings.IsStatusBarEnabled = cbStatusBar.Checked;
            SettingsService.Set("IsStatusBarEnabled", cbStatusBar.Checked.ToString().ToLower());
        }

        private void cbPDFnone_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadingSettings) return;
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
                SettingsService.Set("HiddenPDFNone", "false");
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
            DateTime nextEaster = CalculateEaster(nextYear);

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
            Animation.AnimateWindow(mnuTimeMachine.Handle, 100, Animation.AW_BLEND);

            if (birthdayService.All().Count != 0)
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
            if (_loadingSettings) return;
            int index = comboSettingsTabAlinement.SelectedIndex;
            string[] values = { "top", "left", "right", "bottom" };
            if (index < 0 || index >= values.Length) return;
            SettingsService.Set("SettingsTabAlignment", values[index]);
            ApplySettingsTabAlignment();
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

            // Populate the main menu
            foreach (BirthdayModel model in birthdayService.All())
            {
                ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem
                {
                    Text = $"{model.Name}'s Birthday",
                    Tag = model,            // Store the model for click handling
                    DropDown = contextMenuStrip1         // Assign the same menu
                };

                // When opening the dropdown, set the menu's Tag to the current model
                toolStripMenuItem.DropDownOpening += (s, ee) =>
                {
                    contextMenuStrip1.Tag = toolStripMenuItem.Tag;
                };

                mnuBirthdays.Items.Add(toolStripMenuItem);
                toolStripMenuItem.Click += ToolStripMenuItem_Click;
            }


            //last
            Animation.AnimateWindow((sender as ContextMenuStrip).Handle, 100, Animation.AW_BLEND);
        }


        private void ToolStripMenu_Click(object sender, EventArgs e)
        {
            AddBirthday addBirthdayForm = new AddBirthday(this, false, null, DateTime.MinValue, Guid.Empty);
            addBirthdayForm.ShowDialog();
        }

        private void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var model = ((ToolStripMenuItem)sender).Tag as BirthdayModel;

            DateTime currentDate = DateTime.Now;
            DateTime dateTime = model.DOB;
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
            mnuTimeMachine.Close();
        }

        private async void CDFSelectedIndexChanged()
        {
            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                            "Xaftellis", "Quartz", "UserData", "pictures");
            string filename = string.Format("{0}.ico", ProfileService.Current);
            string path = Path.Combine(directory, filename);

            if (combDefaultFavicon.SelectedIndex == 0)
            {
                DeleteFileIfExists(path);
                SettingsService.Set("defaultFavicon", "default");
            }
            else if (combDefaultFavicon.SelectedIndex == 1)
            {
                DeleteFileIfExists(path);
                SettingsService.Set("defaultFavicon", "chrome");
            }
            else if (combDefaultFavicon.SelectedIndex == 2)
            {
                string file = OpenImageFileDialog();
                if (string.IsNullOrEmpty(file) || !File.Exists(file))
                {
                    combDefaultFavicon.SelectedIndexChanged -= combDefaultFavicon_SelectedIndexChanged;
                    if (SettingsService.Get("defaultFavicon") == "default")
                    {
                        combDefaultFavicon.SelectedIndex = 0;
                    }
                    else if (SettingsService.Get("defaultFavicon") == "chrome")
                    {
                        combDefaultFavicon.SelectedIndex = 1;
                    }
                    else if (SettingsService.Get("defaultFavicon").StartsWith("custom - "))
                    {
                        combDefaultFavicon.SelectedIndex = 2;
                    }
                    combDefaultFavicon.SelectedIndexChanged += combDefaultFavicon_SelectedIndexChanged;
                    return;
                }

                Icon icon;

                // Load image first to check if conversion/resizing is needed
                using (MagickImage magickImage = new MagickImage(file))
                {
                    bool isIcon = magickImage.Format == MagickFormat.Icon || magickImage.Format == MagickFormat.Ico;
                    bool is16x16 = magickImage.Width == 16 && magickImage.Height == 16;
                    bool isMultiSized = IsMultiSizedIcon(file);

                    bool needsConversion = !(isIcon && is16x16) || isMultiSized;

                    if (needsConversion)
                    {
                        // Ask user for confirmation
                        DialogResult result = MessageBox.Show(
                            "The selected image will be resized to 16x16 and/or converted to .ico format.\nDo you want to continue?",
                            "Favicon Conversion",
                            MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Information
                        );

                        if (result != DialogResult.OK)
                        {
                            combDefaultFavicon.SelectedIndexChanged -= combDefaultFavicon_SelectedIndexChanged;
                            if (SettingsService.Get("defaultFavicon") == "default")
                            {
                                combDefaultFavicon.SelectedIndex = 0;
                            }
                            else if (SettingsService.Get("defaultFavicon") == "chrome")
                            {
                                combDefaultFavicon.SelectedIndex = 1;
                            }
                            else if (SettingsService.Get("defaultFavicon").StartsWith("custom - "))
                            {
                                combDefaultFavicon.SelectedIndex = 2;
                            }
                            combDefaultFavicon.SelectedIndexChanged += combDefaultFavicon_SelectedIndexChanged;
                            return;
                        }
                       
                        //resizes image if needed
                        if (!is16x16)
                        {
                            magickImage.FilterType = FilterType.Lanczos;
                            magickImage.Resize(16, 16);
                        }

                        magickImage.Format = MagickFormat.Ico;
                        using (MemoryStream outStream = new MemoryStream())
                        {
                            magickImage.Write(outStream);
                            outStream.Position = 0;
                            icon = new Icon(outStream);
                        }
                    }
                    else
                    {
                        icon = new Icon(file);
                    }
                }

                SaveIconToFile(icon, path);
                SettingsService.Set("defaultFavicon", string.Format("custom - {0}", path));
            }

            bool isCorrect = await FavouriteService.ValidatePanelAsync(_browser.pnlFavourites);
            if (!isCorrect)
                _browser.LoadFavourites();

            _browser.CoreWebView2_FaviconChanged(null, null);
        }

        private void DeleteFileIfExists(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        private string OpenImageFileDialog()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select Default Favicon";
                dialog.Filter =
                    "Image Files (*.ico;*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tif;*.tiff;*.svg;*.webp;*.heic;*.avif)|" +
                    "*.ico;*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tif;*.tiff;*.svg;*.webp;*.heic;*.avif|" +
                    "All Files (*.*)|*.*";
                dialog.Multiselect = false;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    return dialog.FileName;
                }
                else
                {
                    return null;
                }
            }
        }

        public static bool IsMultiSizedIcon(string path)
        {
            using (var icoImages = new MagickImageCollection(path))
            {
                return icoImages.Count > 1; // more than 1 frame → multi-sized
            }
        }

        private void SaveIconToFile(Icon icon, string path)
        {
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                icon.Save(fs);
            }
        }

        private void combDefaultFavicon_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loadingSettings) return;
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

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (contextMenuStrip1.Tag is BirthdayModel model)
            {
                birthdayService.Remove(model.Id);
                birthdayService.SaveChanges();
                // Refresh your menu or UI if needed
            }
        }

        private void contextMenuStrip1_Opening_1(object sender, CancelEventArgs e)
        {
            Animation.AnimateWindow((sender as ContextMenuStrip).Handle, 100, Animation.AW_BLEND);
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (contextMenuStrip1.Tag is BirthdayModel model)
            {
                AddBirthday addBirthdayForm = new AddBirthday(this, true, model.Name, model.DOB, model.Id);
                addBirthdayForm.ShowDialog();
            }
        }

        private void cbDownloadAlighment_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loadingSettings) return;
            if (cbDownloadAlighment.SelectedIndex == 0)
            {
                _browser.wvWebView1.CoreWebView2.DefaultDownloadDialogCornerAlignment = CoreWebView2DefaultDownloadDialogCornerAlignment.TopRight;
                SettingsService.Set("DownloadAlignment", "TopRight");
            }
            else if (cbDownloadAlighment.SelectedIndex == 1)
            {
                _browser.wvWebView1.CoreWebView2.DefaultDownloadDialogCornerAlignment = CoreWebView2DefaultDownloadDialogCornerAlignment.TopLeft;
                SettingsService.Set("DownloadAlignment", "TopLeft");
            }
            else if (cbDownloadAlighment.SelectedIndex == 2)
            {
                _browser.wvWebView1.CoreWebView2.DefaultDownloadDialogCornerAlignment = CoreWebView2DefaultDownloadDialogCornerAlignment.BottomRight;
                SettingsService.Set("DownloadAlignment", "BottomRight");
            }
            else if (cbDownloadAlighment.SelectedIndex == 3)
            {
                _browser.wvWebView1.CoreWebView2.DefaultDownloadDialogCornerAlignment = CoreWebView2DefaultDownloadDialogCornerAlignment.BottomLeft;
                SettingsService.Set("DownloadAlignment", "BottomLeft");
            }
        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (_loadingSettings) return;
            string frequency;

            switch (cbUpdateCheckFrequency.SelectedIndex)
            {
                case 0:
                    frequency = "startup";
                    break;

                case 1:
                    frequency = "daily";
                    break;

                case 2:
                    frequency = "weekly";
                    break;

                case 3:
                    frequency = "monthly";
                    break;

                case 4:
                    frequency = "never";
                    break;

                default:
                    return;
            }

            MainSettingsService.Set("UpdateCheckFrequency", frequency);
        }
    }
}
