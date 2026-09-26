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
                LoadThemeSelection();
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
