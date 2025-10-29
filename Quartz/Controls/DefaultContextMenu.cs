using EasyTabs;
using Quartz.Models;
using Quartz.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Quartz.Controls
{
    public class DefaultContextMenu : ContextMenuStrip
    {
        TitleBarTabs _parentForm;
        TitleBarTab _clickedTab;

        // Win32 constants
        private const int WM_SYSCOMMAND = 0x0112;
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int HTCAPTION = 0x02;

        private const int SC_SIZE = 0xF000;
        private const int SC_MOVE = 0xF010;
        private const int SC_MINIMIZE = 0xF020;
        private const int SC_MAXIMIZE = 0xF030;
        private const int SC_CLOSE = 0xF060;
        private const int SC_RESTORE = 0xF120;

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        private ToolStripMenuItem restoreToolStripMenuItem;
        private ToolStripMenuItem moveToolStripMenuItem;
        private ToolStripMenuItem sizeToolStripMenuItem;
        private ToolStripMenuItem minimizeToolStripMenuItem;
        private ToolStripMenuItem maximizeToolStripMenuItem;
        private ToolStripMenuItem reloadAllToolStripMenuItem;
        private ToolStripMenuItem favouriteAllToolStripMenuItem;
        private ToolStripMenuItem nameWindowAllToolStripMenuItem;
        private ToolStripMenuItem taskManagerToolStripMenuItem;
        private ToolStripMenuItem closeToolStripMenuItem;

        public DefaultContextMenu()
        {
            // Theme
            NewControlThemeChanger.ChangeControlTheme(this);

            // Menu items
            restoreToolStripMenuItem = new ToolStripMenuItem("Restore");
            moveToolStripMenuItem = new ToolStripMenuItem("Move");
            sizeToolStripMenuItem = new ToolStripMenuItem("Size");
            minimizeToolStripMenuItem = new ToolStripMenuItem("Minimize");
            maximizeToolStripMenuItem = new ToolStripMenuItem("Maximize");
            var sep = new ToolStripSeparator();
            reloadAllToolStripMenuItem = new ToolStripMenuItem("Reload all tabs...");
            favouriteAllToolStripMenuItem = new ToolStripMenuItem("Favourite all tabs...");
            nameWindowAllToolStripMenuItem = new ToolStripMenuItem("Name window...");

            var sep2 = new ToolStripSeparator();
            taskManagerToolStripMenuItem = new ToolStripMenuItem("Task manager");
            var sep3 = new ToolStripSeparator();
            closeToolStripMenuItem = new ToolStripMenuItem("Close");

            this.Items.AddRange(new ToolStripItem[]
            {
                restoreToolStripMenuItem,
                moveToolStripMenuItem,
                sizeToolStripMenuItem,
                minimizeToolStripMenuItem,
                maximizeToolStripMenuItem,
                sep,
                reloadAllToolStripMenuItem,
                favouriteAllToolStripMenuItem,
                nameWindowAllToolStripMenuItem,
                sep2,
                taskManagerToolStripMenuItem,
                sep3,
                closeToolStripMenuItem
            });

            // Hook clicks, always call DefineVarables() first
            restoreToolStripMenuItem.Click += (s, e) => { DoRestore(); };
            moveToolStripMenuItem.Click += (s, e) => { DoMove(); };
            sizeToolStripMenuItem.Click += (s, e) => { DoSize(); };
            minimizeToolStripMenuItem.Click += (s, e) => { DoMinimize(); };
            maximizeToolStripMenuItem.Click += (s, e) => { DoMaximize(); };
            closeToolStripMenuItem.Click += (s, e) => { DoClose(); };
            taskManagerToolStripMenuItem.Click += TaskManagerToolStripMenuItem_Click;
            reloadAllToolStripMenuItem.Click += ReloadAllToolStripMenuItem_Click;
            favouriteAllToolStripMenuItem.Click += FavouriteAllToolStripMenuItem_Click;
            nameWindowAllToolStripMenuItem.Click += NameWindowAllToolStripMenuItem_Click;

            this.Opening += DefaultContextMenu_Opening;
        }

        private void NameWindowAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NameWindow form = new NameWindow((AppContainer)_parentForm);
            form.ShowDialog();
        }

        private void FavouriteAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FavouriteService favouriteService = new FavouriteService();

            foreach (TitleBarTab tab in _parentForm.Tabs)
            {
                if (tab.Content is Browser browser)
                {
                    var favourite = new FavouriteModel
                    {
                        Name = browser.Text,
                        WebAddress = browser.wvWebView1.Source.AbsoluteUri
                    };

                    favouriteService.Modify(favourite);

                    // Only handle index here when not sorting alphabetically
                    if (SettingsService.Get("sortFavouritesBy") != "alphabetically")
                    {
                        // Get the highest existing index and assign the next one
                        int nextIndex = favouriteService.All().Count - 1;
                        favouriteService.Get(browser.Text).Index = nextIndex;
                    }
                }
            }

            // Handle alphabetical sorting ONLY once if needed
            if (SettingsService.Get("sortFavouritesBy") == "alphabetically")
            {
                List<FavouriteModel> allItems = favouriteService.All();
                var sortedItems = allItems.OrderBy(item => item.Name).ToList();

                for (int i = 0; i < sortedItems.Count; i++)
                {
                    sortedItems[i].Index = i;
                }
            }

            favouriteService.SaveChanges();
            ((Browser)_parentForm.SelectedTab.Content).LoadFavourites();
        }


        private void ReloadAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach(TitleBarTab tab in _parentForm.Tabs)
            {
                if(tab.Content is Browser)
                {
                    Browser browser = tab.Content as Browser;
                    browser.wvWebView1.Reload();
                }
            }
        }

        private void TaskManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((Browser)_parentForm.SelectedTab.Content).wvWebView1.CoreWebView2.OpenTaskManagerWindow();
        }

        public void DefineVarables()
        {
            _parentForm = EasyTabs.ContextMenuProvider._parentForm;
            _clickedTab = EasyTabs.ContextMenuProvider._clickedTab;
        }

        private void DefaultContextMenu_Opening(object sender, CancelEventArgs e)
        {
            DefineVarables();

            if (SettingsService.Get("Animation") == "true")
            {
                Animation.AnimateWindow(this.Handle, 100, Animation.AW_BLEND);
            }

            // Enable or disable items based on window state and capabilities
            restoreToolStripMenuItem.Enabled = (_parentForm.WindowState != FormWindowState.Normal);
            moveToolStripMenuItem.Enabled = (_parentForm.FormBorderStyle != FormBorderStyle.FixedDialog &&
                                             _parentForm.FormBorderStyle != FormBorderStyle.FixedSingle &&
                                             _parentForm.FormBorderStyle != FormBorderStyle.Fixed3D &&
                                             _parentForm.WindowState == FormWindowState.Normal);
            sizeToolStripMenuItem.Enabled = (_parentForm.FormBorderStyle != FormBorderStyle.FixedDialog &&
                                             _parentForm.FormBorderStyle != FormBorderStyle.FixedSingle &&
                                             _parentForm.FormBorderStyle != FormBorderStyle.Fixed3D &&
                                             _parentForm.WindowState == FormWindowState.Normal);
            minimizeToolStripMenuItem.Enabled = _parentForm.MinimizeBox;
            maximizeToolStripMenuItem.Enabled = (_parentForm.MaximizeBox &&
                                                _parentForm.WindowState != FormWindowState.Maximized);
            restoreToolStripMenuItem.Enabled = (_parentForm.WindowState != FormWindowState.Normal);
        }

        private void DoRestore()
        {
            if (_parentForm.WindowState != FormWindowState.Normal)
                _parentForm.WindowState = FormWindowState.Normal;
            else
                SendMessage(_parentForm.Handle, WM_SYSCOMMAND, new IntPtr(SC_RESTORE), IntPtr.Zero);
        }

        private void DoMinimize()
        {
            if (_parentForm.MinimizeBox)
                _parentForm.WindowState = FormWindowState.Minimized;
            else
                SendMessage(_parentForm.Handle, WM_SYSCOMMAND, new IntPtr(SC_MINIMIZE), IntPtr.Zero);
        }

        private void DoMaximize()
        {
            if (_parentForm.MaximizeBox)
                _parentForm.WindowState = FormWindowState.Maximized;
            else
                SendMessage(_parentForm.Handle, WM_SYSCOMMAND, new IntPtr(SC_MAXIMIZE), IntPtr.Zero);
        }

        private void DoClose()
        {
            _parentForm.Close();
        }

        private void DoMove()
        {
            ReleaseCapture();
            SendMessage(_parentForm.Handle, WM_SYSCOMMAND, new IntPtr(SC_MOVE), IntPtr.Zero);
        }

        private void DoSize()
        {
            ReleaseCapture();
            SendMessage(_parentForm.Handle, WM_SYSCOMMAND, new IntPtr(SC_SIZE), IntPtr.Zero);
        }
    }
}
