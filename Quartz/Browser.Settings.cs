using EasyTabs;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Quartz
{
    public partial class Browser
    {
        public bool IsSettingsTab { get; private set; }
        private Settings _settingsPage;
        private Label _settingsLoadingLabel;
        private bool _closingSettingsTab;

        private void OpenSettingsTab()
        {
            // Reuse this window's settings tab instead of opening another copy.
            foreach (TitleBarTab tab in ParentTabs.Tabs)
            {
                Browser browser = tab.Content as Browser;
                if (browser != null && browser.IsSettingsTab)
                {
                    ParentTabs.SelectedTab = tab;
                    return;
                }
            }

            Browser settingsBrowser = new Browser("about:blank", true);
            settingsBrowser.InitializeTab();
            settingsBrowser.PrepareSettingsTab();

            TitleBarTab settingsTab = new TitleBarTab(ParentTabs);
            settingsTab.Content = settingsBrowser;
            ParentTabs.Tabs.Insert(ParentTabs.SelectedTabIndex + 1, settingsTab);
            ParentTabs.SelectedTab = settingsTab;
            ParentTabs.RedrawTabs();
        }

        internal void PrepareSettingsTab()
        {
            IsSettingsTab = true;
            Text = "Settings";

            // Keep a normal Browser as the tab's owner. Existing drag/drop and tab menus
            // can keep working, while the visible page is an ordinary WinForms form.
            _settingsLoadingLabel = new Label();
            _settingsLoadingLabel.Text = "Opening settings…";
            _settingsLoadingLabel.TextAlign = ContentAlignment.MiddleCenter;
            _settingsLoadingLabel.Dock = DockStyle.Fill;
            _settingsLoadingLabel.BackColor = BackColor;
            _settingsLoadingLabel.ForeColor = ForeColor;
            Controls.Add(_settingsLoadingLabel);
            _settingsLoadingLabel.BringToFront();
        }

        private void ShowSettingsPage()
        {
            if (!IsSettingsTab || IsDisposed || Disposing || _settingsPage != null)
            {
                return;
            }

            // The WebView is ready before Settings reads or changes its preferences.
            // It belongs to this tab, so closing the original website won't break Settings.
            _settingsPage = new Settings(this, true);
            _settingsPage.TopLevel = false;
            _settingsPage.FormBorderStyle = FormBorderStyle.None;
            _settingsPage.Dock = DockStyle.Fill;
            _settingsPage.FormClosed += SettingsPage_FormClosed;
            Controls.Add(_settingsPage);
            _settingsPage.BringToFront();
            _settingsPage.Show();

            _settingsLoadingLabel.Dispose();
            pnlTop.Visible = false;
            pnlFavourites.Visible = false;
            wvWebView1.Visible = false;
            Text = "Settings";
            SetTabLoading(false);
        }

        private void SettingsPage_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!_closingSettingsTab && !IsDisposed && !Disposing)
            {
                Close();
            }
        }

        private void CloseSettingsPage()
        {
            _closingSettingsTab = true;
            if (_settingsPage != null && !_settingsPage.IsDisposed)
            {
                _settingsPage.Close();
            }
        }
    }
}
