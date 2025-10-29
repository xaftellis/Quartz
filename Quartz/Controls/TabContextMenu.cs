using EasyTabs;
using Quartz.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quartz.Controls
{
    public class TabContextMenu : ContextMenuStrip
    {
        TitleBarTabs _parentForm;
        TitleBarTab _clickedTab;

        // Menu Items
        private ToolStripMenuItem reloadTabToolStripMenuItem;
        private ToolStripMenuItem duplicateTabToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator;
        private ToolStripMenuItem closeTabToolStripMenuItem;
        private ToolStripMenuItem closeOtherToolStripMenuItem;
        private ToolStripMenuItem closeLeftToolStripMenuItem;
        private ToolStripMenuItem closeRightToolStripMenuItem;
        public TabContextMenu()
        {
            // Theme
            NewControlThemeChanger.ChangeControlTheme(this);

            // Controls
            reloadTabToolStripMenuItem = new ToolStripMenuItem("Reload");
            duplicateTabToolStripMenuItem = new ToolStripMenuItem("Duplicate");
            toolStripSeparator = new ToolStripSeparator();
            closeTabToolStripMenuItem = new ToolStripMenuItem("Close Tab");
            closeOtherToolStripMenuItem = new ToolStripMenuItem("Close other tabs");
            closeLeftToolStripMenuItem = new ToolStripMenuItem("Close tabs to the left");
            closeRightToolStripMenuItem = new ToolStripMenuItem("Close tabs to the right");


            this.Items.AddRange(new ToolStripItem[]
            {

                reloadTabToolStripMenuItem,
                duplicateTabToolStripMenuItem,
                toolStripSeparator,
                closeTabToolStripMenuItem,
                closeOtherToolStripMenuItem,
                closeLeftToolStripMenuItem,
                closeRightToolStripMenuItem
            });

            // Events
            this.Opening += DefaultContextMenu_Opening;

            reloadTabToolStripMenuItem.Click += ReloadTabToolStripMenuItem_Click;
            duplicateTabToolStripMenuItem.Click += DuplicateTabToolStripMenuItem_Click;
            closeTabToolStripMenuItem.Click += CloseTabToolStripMenuItem_Click;
            closeOtherToolStripMenuItem.Click += CloseOtherToolStripMenuItem_Click;
            closeLeftToolStripMenuItem.Click += CloseLeftToolStripMenuItem_Click;
            closeRightToolStripMenuItem.Click += CloseRightToolStripMenuItem_Click;

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

                var toClose = _parentForm.Tabs.Take(idx).ToList();
                foreach (var tab in toClose)
                {
                    var f = tab.Content as Form;
                    f?.Close(); // respects FormClosing
                    if (f == null || f.IsDisposed || !f.Visible)
                        _parentForm.Tabs.Remove(tab);
                }
                _parentForm.RedrawTabs();
            }
        }

        private void CloseRightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int idx = _parentForm.Tabs.IndexOf(_clickedTab);
            if (idx >= 0 && idx < _parentForm.Tabs.Count - 1)
            {
                // Ensures tab is selected
                _parentForm.SelectedTab = _clickedTab;

                var toClose = _parentForm.Tabs.Skip(idx + 1).ToList();
                foreach (var tab in toClose)
                {
                    var f = tab.Content as Form;
                    f?.Close(); // respects FormClosing
                    if (f == null || f.IsDisposed || !f.Visible)
                        _parentForm.Tabs.Remove(tab);
                }
                _parentForm.RedrawTabs();
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
