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
        private ToolStripMenuItem muteTabToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem closeTabToolStripMenuItem;
        private ToolStripMenuItem closeOtherToolStripMenuItem;
        private ToolStripMenuItem closeLeftToolStripMenuItem;
        private ToolStripMenuItem closeRightToolStripMenuItem;
        public TabContextMenu()
        {
            // Theme
            NewControlThemeChanger.ChangeControlTheme(this);

            // Controls
            newTabLeftStripMenuItem = new ToolStripMenuItem("New tab to the left");
            newTabRightStripMenuItem = new ToolStripMenuItem("New tab to the right");
            toolStripSeparator = new ToolStripSeparator();
            reloadTabToolStripMenuItem = new ToolStripMenuItem("Reload");
            duplicateTabToolStripMenuItem = new ToolStripMenuItem("Duplicate");
            muteTabToolStripMenuItem = new ToolStripMenuItem("Mute tab");
            toolStripSeparator1 = new ToolStripSeparator();
            closeTabToolStripMenuItem = new ToolStripMenuItem("Close Tab");
            closeOtherToolStripMenuItem = new ToolStripMenuItem("Close other tabs");
            closeLeftToolStripMenuItem = new ToolStripMenuItem("Close tabs to the left");
            closeRightToolStripMenuItem = new ToolStripMenuItem("Close tabs to the right");


            this.Items.AddRange(new ToolStripItem[]
            {
                newTabLeftStripMenuItem,
                newTabRightStripMenuItem,
                toolStripSeparator,
                reloadTabToolStripMenuItem,
                duplicateTabToolStripMenuItem,
                muteTabToolStripMenuItem,
                toolStripSeparator1,
                closeTabToolStripMenuItem,
                closeOtherToolStripMenuItem,
                closeLeftToolStripMenuItem,
                closeRightToolStripMenuItem
            });

            // Events
            this.Opening += DefaultContextMenu_Opening;

            newTabLeftStripMenuItem.Click += NewTabLeftStripMenuItem_Click;
            newTabRightStripMenuItem.Click += NewTabRightStripMenuItem_Click;
            reloadTabToolStripMenuItem.Click += ReloadTabToolStripMenuItem_Click;
            duplicateTabToolStripMenuItem.Click += DuplicateTabToolStripMenuItem_Click;
            muteTabToolStripMenuItem.Click += MuteTabToolStripMenuItem_Click;
            closeTabToolStripMenuItem.Click += CloseTabToolStripMenuItem_Click;
            closeOtherToolStripMenuItem.Click += CloseOtherToolStripMenuItem_Click;
            closeLeftToolStripMenuItem.Click += CloseLeftToolStripMenuItem_Click;
            closeRightToolStripMenuItem.Click += CloseRightToolStripMenuItem_Click;
        }

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
                    _parentForm.SelectedTabIndex = newTabIndex;
                    _parentForm.RedrawTabs();
                    _parentForm.Refresh();
                }));
            }
            else
            {
                _parentForm.Tabs.Insert(newTabIndex, newtab);
                _parentForm.SelectedTabIndex = newTabIndex;
                _parentForm.RedrawTabs();
                _parentForm.Refresh();
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
                    _parentForm.SelectedTabIndex = newTabIndex;
                    _parentForm.RedrawTabs();
                    _parentForm.Refresh();
                }));
            }
            else
            {
                _parentForm.Tabs.Insert(newTabIndex, newtab);
                _parentForm.SelectedTabIndex = newTabIndex;
                _parentForm.RedrawTabs();
                _parentForm.Refresh();
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
            if (idx > 0)
            {
                // Ensures tab is selected
                _parentForm.SelectedTab = _clickedTab;

                // Keep only the clicked tab and all tabs to its right
                var remaining = _parentForm.Tabs.Skip(idx).ToList();

                _parentForm.Tabs.Clear();
                _parentForm.Tabs.AddRange(remaining);
            }
        }

        private void CloseRightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int idx = _parentForm.Tabs.IndexOf(_clickedTab);
            if (idx >= 0 && idx < _parentForm.Tabs.Count - 1)
            {
                // Ensures tab is selected
                _parentForm.SelectedTab = _clickedTab;

                // Keep only the clicked tab and all tabs to its left
                var remaining = _parentForm.Tabs.Take(idx + 1).ToList();

                _parentForm.Tabs.Clear();
                _parentForm.Tabs.AddRange(remaining);
            }
        }

        private void DuplicateTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string _url = ((Browser)_clickedTab.Content).wvWebView1.Source.AbsoluteUri.ToString();

            Browser browser = new Browser(_url, true);
            browser.InitializeTab();
            var newtab = new TitleBarTab(_parentForm) { Content = browser };
            int newTabIndex = _parentForm.Tabs.IndexOf(_clickedTab) + 1;

            if (_parentForm.InvokeRequired)
            {
                _parentForm.Invoke(new Action(() =>
                {
                    _parentForm.Tabs.Insert(newTabIndex, newtab);
                    _parentForm.SelectedTabIndex = newTabIndex;
                    _parentForm.RedrawTabs();
                    _parentForm.Refresh();
                }));
            }
            else
            {
                _parentForm.Tabs.Insert(newTabIndex, newtab);
                _parentForm.SelectedTabIndex = newTabIndex;
                _parentForm.RedrawTabs();
                _parentForm.Refresh();
            }
        }

        private void CloseOtherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_clickedTab != null)
            {
                // Ensures tab is selected
                _parentForm.SelectedTab = _clickedTab;

                _parentForm.Tabs.Clear();
                _parentForm.Tabs.Add(_clickedTab);
            }
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
            UpdateMenuItemsEnabledState();

            if (_clickedTab?.Content is Browser browser)
            {
                bool isMuted = browser.wvWebView1.CoreWebView2.IsMuted;
                muteTabToolStripMenuItem.Text = !isMuted ? "Mute tab" : "Unmute tab";
            }

            if (SettingsService.Get("Animation") == "true")
            {
                Animation.AnimateWindow(this.Handle, 100, Animation.AW_BLEND);
            }
        }

        public void UpdateMenuItemsEnabledState()
        {
            int clickedTabIndex = _parentForm.Tabs.IndexOf(_clickedTab);
            int count = _parentForm.Tabs.Count;

            // Basic logic:
            // - Close other: enabled when more than 1 tab exists
            // - Close left: enabled when one or more tabs to the left (idx > 0)
            // - Close right: enabled when one or more tabs to the right (idx < count-1)
            closeOtherToolStripMenuItem.Enabled = count > 1;
            closeLeftToolStripMenuItem.Enabled = clickedTabIndex > 0;
            closeRightToolStripMenuItem.Enabled = (clickedTabIndex >= 0 && clickedTabIndex < count - 1);
        }
    }
}
