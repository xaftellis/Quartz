using EasyTabs;
using Quartz.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Animation;

namespace Quartz.Controls
{
    public class TabContextMenu : ContextMenuStrip
    {
        TitleBarTabs _parentForm;
        TitleBarTab _clickedTab;

        // Menu Items
        private ToolStripMenuItem newTabLeftStripMenuItem;
        private ToolStripMenuItem newTabRightStripMenuItem;
        private ToolStripSeparator toolStripSeparator;
        private ToolStripMenuItem reloadTabToolStripMenuItem;
        private ToolStripMenuItem duplicateTabToolStripMenuItem;
        private ToolStripMenuItem pinTabToolStripMenuItem;
        private ToolStripMenuItem muteTabToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem closeTabToolStripMenuItem;
        private ToolStripMenuItem closeOtherToolStripMenuItem;
        private ToolStripMenuItem closeLeftToolStripMenuItem;
        private ToolStripMenuItem closeRightToolStripMenuItem;
        //private ToolStripMenuItem showSiteIconsOnlyToolStripMenuItem;
        public TabContextMenu()
        {
            // Theme
            NewControlThemeChanger.ChangeControlTheme(this);

            // Controls
            newTabLeftStripMenuItem = new ToolStripMenuItem("New tab to the left");
            newTabRightStripMenuItem = new ToolStripMenuItem("New tab to the right");
            toolStripSeparator = new ToolStripSeparator();
            reloadTabToolStripMenuItem = new ToolStripMenuItem("Reload");
            reloadTabToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.R;
            duplicateTabToolStripMenuItem = new ToolStripMenuItem("Duplicate");
            pinTabToolStripMenuItem = new ToolStripMenuItem("Pin");
            muteTabToolStripMenuItem = new ToolStripMenuItem("Mute tab");
            toolStripSeparator1 = new ToolStripSeparator();
            closeTabToolStripMenuItem = new ToolStripMenuItem("Close Tab");
            closeTabToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.W;
            closeOtherToolStripMenuItem = new ToolStripMenuItem("Close other tabs");
            closeLeftToolStripMenuItem = new ToolStripMenuItem("Close tabs to the left");
            closeRightToolStripMenuItem = new ToolStripMenuItem("Close tabs to the right");
            //showSiteIconsOnlyToolStripMenuItem = new ToolStripMenuItem("Show site icons only");

            //showSiteIconsOnlyToolStripMenuItem.CheckOnClick = true;



            this.Items.AddRange(new ToolStripItem[]
            {
                newTabLeftStripMenuItem,
                newTabRightStripMenuItem,
                toolStripSeparator,
                reloadTabToolStripMenuItem,
                duplicateTabToolStripMenuItem,
                pinTabToolStripMenuItem,
                muteTabToolStripMenuItem,
                toolStripSeparator1,
                closeTabToolStripMenuItem,
                closeOtherToolStripMenuItem,
                closeLeftToolStripMenuItem,
                closeRightToolStripMenuItem,
                //showSiteIconsOnlyToolStripMenuItem

            });

            // Events
            this.Opening += DefaultContextMenu_Opening;
            pinTabToolStripMenuItem.Click += (sender, e) =>
            {
                if (_clickedTab != null && _parentForm.Tabs.Contains(_clickedTab))
                    _clickedTab.IsPinned = !_clickedTab.IsPinned;
            };

            newTabLeftStripMenuItem.Click += NewTabLeftStripMenuItem_Click;
            newTabRightStripMenuItem.Click += NewTabRightStripMenuItem_Click;
            reloadTabToolStripMenuItem.Click += ReloadTabToolStripMenuItem_Click;
            duplicateTabToolStripMenuItem.Click += DuplicateTabToolStripMenuItem_Click;
            muteTabToolStripMenuItem.Click += MuteTabToolStripMenuItem_Click;
            closeTabToolStripMenuItem.Click += CloseTabToolStripMenuItem_Click;
            closeOtherToolStripMenuItem.Click += CloseOtherToolStripMenuItem_Click;
            closeLeftToolStripMenuItem.Click += CloseLeftToolStripMenuItem_Click;
            closeRightToolStripMenuItem.Click += CloseRightToolStripMenuItem_Click;
            //showSiteIconsOnlyToolStripMenuItem.CheckStateChanged += ShowSiteIconsOnlyToolStripMenuItem_CheckStateChanged; ;
        }

        //private void ShowSiteIconsOnlyToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
        //{
        //    SettingsService.Set("showSiteIconsOnly", showSiteIconsOnlyToolStripMenuItem.Checked.ToString().ToLower());
        //}

        private void NewTabRightStripMenuItem_Click(object sender, EventArgs e)
        {
            Browser browser = new Browser(null ,false);
            browser.InitializeTab();
            var newtab = new TitleBarTab(_parentForm) { Content = browser };
            int newTabIndex = _parentForm.Tabs.IndexOf(_clickedTab) + 1;

            if (_parentForm.InvokeRequired)
            {
                _parentForm.Invoke(new Action(() =>
                {
                    _parentForm.Tabs.Insert(newTabIndex, newtab);
                    _parentForm.SelectedTab = newtab;
                    _parentForm.RedrawTabs();
                }));
            }
            else
            {
                _parentForm.Tabs.Insert(newTabIndex, newtab);
                _parentForm.SelectedTab = newtab;
                _parentForm.RedrawTabs();
            }
        }

        private void NewTabLeftStripMenuItem_Click(object sender, EventArgs e)
        {
            Browser browser = new Browser(null, false);
            browser.InitializeTab();
            var newtab = new TitleBarTab(_parentForm) { Content = browser };
            int newTabIndex = Math.Max(0, _parentForm.Tabs.IndexOf(_clickedTab));

            if (_parentForm.InvokeRequired)
            {
                _parentForm.Invoke(new Action(() =>
                {
                    _parentForm.Tabs.Insert(newTabIndex, newtab);
                    _parentForm.SelectedTab = newtab;
                    _parentForm.RedrawTabs();
                }));
            }
            else
            {
                _parentForm.Tabs.Insert(newTabIndex, newtab);
                _parentForm.SelectedTab = newtab;
                _parentForm.RedrawTabs();
            }
        }

        private void MuteTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_clickedTab?.Content is Browser browser)
            {
                browser.wvWebView1.CoreWebView2.IsMuted = !browser.wvWebView1.CoreWebView2.IsMuted;
            }
        }

        private void ReloadTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_clickedTab?.Content is Browser browser)
            {
                // Reload using WebView2 API
                browser.wvWebView1.Reload();
            }
        }


        private void CloseLeftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int idx = _parentForm.Tabs.IndexOf(_clickedTab);
            if (idx > 0) CloseTabs(_parentForm.Tabs.Take(idx));
        }

        private void CloseRightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int idx = _parentForm.Tabs.IndexOf(_clickedTab);
            if (idx >= 0) CloseTabs(_parentForm.Tabs.Skip(idx + 1));
        }

        private void DuplicateTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string _url = ((Browser)_clickedTab.Content).wvWebView1.Source?.AbsoluteUri ?? "about:blank";

            Browser browser = new Browser(_url, true);
            browser.InitializeTab();
            var newtab = new TitleBarTab(_parentForm) { Content = browser, IsPinned = _clickedTab.IsPinned };
            int newTabIndex = _parentForm.Tabs.IndexOf(_clickedTab) + 1;

            if (_parentForm.InvokeRequired)
            {
                _parentForm.Invoke(new Action(() =>
                {
                    _parentForm.Tabs.Insert(newTabIndex, newtab);
                    _parentForm.SelectedTab = newtab;
                    _parentForm.RedrawTabs();
                }));
            }
            else
            {
                _parentForm.Tabs.Insert(newTabIndex, newtab);
                _parentForm.SelectedTab = newtab;
                _parentForm.RedrawTabs();
            }
        }

        private void CloseOtherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_clickedTab != null) CloseTabs(_parentForm.Tabs.Where(tab => tab != _clickedTab));
        }

        private void CloseTabs(IEnumerable<TitleBarTab> candidates)
        {
            _parentForm.SelectedTab = _clickedTab;
            // Use the normal close lifecycle (including cancellation, disposal,
            // selection and animation). Chromium protects pins from bulk closes.
            foreach (TitleBarTab tab in candidates.Where(tab => !tab.IsPinned).ToArray())
                if (_parentForm.Tabs.Contains(tab)) tab.Content.Close();
        }

        public void DefineVarables()
        {
            _parentForm = EasyTabs.ContextMenuProvider._parentForm;
            _clickedTab = EasyTabs.ContextMenuProvider._clickedTab;
        }

        private void CloseTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _clickedTab.Content.Close();
        }

        private void DefaultContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DefineVarables();
            if (_parentForm == null || _clickedTab == null || !_parentForm.Tabs.Contains(_clickedTab))
            {
                e.Cancel = true;
                return;
            }
            UpdateMenuItemsEnabledState();
            pinTabToolStripMenuItem.Text = _clickedTab.IsPinned ? "Unpin" : "Pin";

            if (_clickedTab?.Content is Browser browser)
            {
                bool isMuted = browser.wvWebView1.CoreWebView2?.IsMuted ?? false;
                muteTabToolStripMenuItem.Text = !isMuted ? "Mute tab" : "Unmute tab";
                muteTabToolStripMenuItem.Enabled = browser.wvWebView1.CoreWebView2 != null;
            }

            //showSiteIconsOnlyToolStripMenuItem.Checked = SettingsService.Get("showSiteIconsOnly") == "true";

            if (SettingsService.Get("Animation") == "true")
            {
                Animation.AnimateWindow(this.Handle, 100, Animation.AW_BLEND);
            }
        }

        public void UpdateMenuItemsEnabledState()
        {
            int clickedTabIndex = _parentForm.Tabs.IndexOf(_clickedTab);
            closeOtherToolStripMenuItem.Enabled = _parentForm.Tabs.Any(tab => tab != _clickedTab && !tab.IsPinned);
            closeLeftToolStripMenuItem.Enabled = _parentForm.Tabs.Take(clickedTabIndex).Any(tab => !tab.IsPinned);
            closeRightToolStripMenuItem.Enabled = clickedTabIndex >= 0 &&
                _parentForm.Tabs.Skip(clickedTabIndex + 1).Any(tab => !tab.IsPinned);
        }
    }
}
