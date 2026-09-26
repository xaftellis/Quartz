using Microsoft.Web.WebView2.Core;
using Quartz.Libs;
using Quartz.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Quartz
{
    public partial class Settings
    {
        // Also covers events raised by InitializeComponent.
        private bool _loadingSettings = true;
        private bool _settingsLoaded;

        private void PrepareSettingsForm()
        {
            if (_settingsLoaded) return;

            using (SettingsService.BeginReadSnapshot())
            {
                var controls = NewControlThemeChanger.GetAllControls(this)
                    .OfType<Control>()
                    .Distinct().ToArray();
                var containers = controls.Where(control => control.HasChildren).ToArray();
                foreach (Control control in containers) control.SuspendLayout();
                _loadingSettings = true;
                try
                {
                    LoadSettingsSelections();
                    LoadThemeSelection();

                    NewControlThemeChanger.ChangeWindowTheme(Handle);
                    foreach (Control control in controls)
                        NewControlThemeChanger.ChangeControlTheme(control);
                    UpdateBrowserSettingAvailability();

                    pictureBox1.BackgroundImage = FaviconHelper.GetFullResDefaultFaviconAsImage();
                    txtUpdate.Text = "Update";
                    txtUpdate.Visible = false;
                    LoadingProgress.Visible = false;
                    buttonChech.Visible = true;

                    var profile = Program.profileService.Get(ProfileService.Current);
                    Text = profile == null ? "Settings" : "Settings - " + profile.Name;
                    if (SettingsService.Get("DraggableForms") == "true")
                        new MouseDragger(this);
                    _settingsLoaded = true;
                }
                finally
                {
                    for (int i = containers.Length - 1; i >= 0; i--)
                        containers[i].ResumeLayout(true);
                    _loadingSettings = false;
                }
            }
        }

        private void LoadSettingsSelections()
        {
            var checkboxes = new Dictionary<CheckBox, string>
            {
                { cbStatusBar, "IsStatusBarEnabled" },
                { BoxDev, "AreDevToolsEnabled" },
                { BoxSwipeNav, "IsSwipeNavigationEnabled" },
                { BoxKeys, "AreBrowserAcceleratorKeysEnabled" },
                { cbESC, "escClose" },
                { autofillCheckBox, "IsGeneralAutofillEnabled" },
                { autoSaveCheckBox, "IsPasswordAutosaveEnabled" },
                { cbScripts, "IsScriptEnabled" },
                { zc, "IsZoomControlEnabled" },
                { pz, "IsPinchZoomEnabled" },
                { cbDHP, "DefaultHomePage" },
                { cbDrag, "DraggableForms" }
            };
            foreach (var checkbox in checkboxes)
                checkbox.Key.Checked = SettingsService.Get(checkbox.Value) == "true";
            checkBoxMemory.Checked = SettingsService.Get("MemoryUsage") == "low";
            decimal zoom = Convert.ToDecimal(_browser.wvWebView1.ZoomFactor * 100);
            NumZoom.Value = Math.Max(NumZoom.Minimum, Math.Min(NumZoom.Maximum, zoom));

            cbPDFnone.Checked = SettingsService.Get("HiddenPDFNone") == "true";
            foreach (CheckBox checkbox in HiddenPDFGroupBox.Controls.OfType<CheckBox>())
            {
                if (checkbox == cbPDFnone) continue;
                checkbox.Checked = !cbPDFnone.Checked &&
                    SettingsService.Get("HiddenPDF" + checkbox.Text.Replace(" ", "")) == "true";
                checkbox.Enabled = !cbPDFnone.Checked;
            }

            SelectSettingsItem(cbUpdateCheckFrequency,
                MainSettingsService.Get("UpdateCheckFrequency"), 1,
                "startup", "daily", "weekly", "monthly", "never");
            SelectSettingsItem(ComboBoxTracking, SettingsService.Get("TrackingPreventionLevel"), 2,
                "none", "basic", "balanced", "strict");
            SelectSettingsItem(comboSettingsTabAlinement, SettingsService.Get("SettingsTabAlignment"), 0,
                "top", "left", "right", "bottom");
            ApplySettingsTabAlignment();
            SelectSettingsItem(cbDownloadAlighment, SettingsService.Get("DownloadAlignment"), 0,
                "TopRight", "TopLeft", "BottomRight", "BottomLeft");
            SelectSettingsItem(cbSearchEngine, SettingsService.Get("SearchEngine"), 0,
                "google", "bing", "yahoo", "duckduckgo", "ecosia", "netflix", "youtube",
                "googlemaps", "wikipedia", "ebay", "amazon");
            cbDHP.Visible = cbSearchEngine.SelectedIndex < 8;

            string favicon = SettingsService.Get("defaultFavicon") ?? "default";
            combDefaultFavicon.SelectedIndex = favicon.StartsWith("custom - ", StringComparison.Ordinal) ? 2 :
                favicon == "chrome" ? 1 : 0;
        }

        private static void SelectSettingsItem(ComboBox control, string value, int fallback, params string[] values)
        {
            int index = Array.IndexOf(values, value);
            control.SelectedIndex = index < 0 ? fallback : index;
        }

        private void LoadThemeSelection()
        {
            bool disposable = Program.profileService.Get(ProfileService.Current)?.isDisposable == true;
            string theme = SettingsService.GetAutoTheme() ?? SettingsService.Get("Theme");
            ComboBoxTheme.Items.Clear();
            ComboBoxTheme.Items.AddRange(new object[]
            {
                disposable ? "Auto (Light/Dark)" : "Auto (Light/Dark) (Default)",
                "Auto (Light/Black)", "Light", "Dark",
                disposable ? "Black (Default)" : "Black", "Aqua"
            });
            if (DateTime.Now.Month == 12 || theme == "xmas")
                ComboBoxTheme.Items.Add("Xmas");
            SelectSettingsItem(ComboBoxTheme, theme, disposable ? 4 : 0,
                "auto (light/dark)", "auto (light/black)", "light", "dark", "black", "aqua", "xmas");
        }

        private void ApplySettingsTabAlignment()
        {
            var alignments = new[] { TabAlignment.Top, TabAlignment.Left, TabAlignment.Right, TabAlignment.Bottom };
            int index = comboSettingsTabAlinement.SelectedIndex;
            if (index < 0 || index >= alignments.Length) return;
            tabControl1.SizeMode = index == 1 || index == 2 ? TabSizeMode.Normal : TabSizeMode.Fixed;
            tabControl1.Alignment = alignments[index];
        }

        private void BrowserSettingsReady(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!IsDisposed && !Disposing) UpdateBrowserSettingAvailability();
        }

        private void UpdateBrowserSettingAvailability()
        {
            bool ready = !_browser.wvWebView1.IsDisposed && _browser.wvWebView1.CoreWebView2 != null;
            foreach (Control control in new Control[]
            {
                autofillCheckBox, autoSaveCheckBox, zc, pz, BoxDev, BoxSwipeNav, BoxKeys,
                NumZoom, ComboBoxTracking, cbScripts, cbStatusBar, cbDownloadAlighment, HiddenPDFGroupBox
            }) control.Enabled = ready;
        }
    }
}
