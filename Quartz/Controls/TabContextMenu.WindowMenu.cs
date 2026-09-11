using EasyTabs;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Quartz.Controls
{
    public partial class TabContextMenu
    {
        private ToolStripMenuItem moveTabToolStripMenuItem;

        private bool CanMoveClickedTab => _parentForm != null && !_parentForm.IsDisposed &&
            !_parentForm.Disposing && !_parentForm.IsClosing && _clickedTab != null &&
            _parentForm.Tabs.Contains(_clickedTab) && _parentForm.ApplicationContext != null;

        private void UpdateMoveWindowMenu()
        {
            // Rebuild on opening: titles, window membership and activation order can change.
            foreach (ToolStripItem item in moveTabToolStripMenuItem.DropDownItems.Cast<ToolStripItem>().ToArray())
                item.Dispose();
            moveTabToolStripMenuItem.DropDownItems.Clear();

            var windows = _parentForm.ApplicationContext?.OpenWindowsByActivation
                .Where(window => window.CanReceiveTabsFrom(_parentForm)).ToArray()
                ?? new TitleBarTabs[0];
            bool canCreate = CanMoveClickedTab && _parentForm.Tabs.Count > 1;
            moveTabToolStripMenuItem.Text = windows.Length == 0
                ? "Move tab to new window" : "Move tab to another window";
            moveTabToolStripMenuItem.Enabled = windows.Length > 0 || canCreate;
            if (windows.Length == 0) return;

            var newWindow = new ToolStripMenuItem("New window") { Enabled = canCreate };
            newWindow.Click += (sender, e) => MoveTabToNewWindow();
            moveTabToolStripMenuItem.DropDownItems.Add(newWindow);
            moveTabToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            foreach (TitleBarTabs window in windows)
            {
                var item = new ToolStripMenuItem(GetMoveWindowTitle(window).Replace("&", "&&"));
                // Capture the window itself; a stale menu must never move to a different window.
                item.Click += (sender, e) => MoveTabToExistingWindow(window);
                moveTabToolStripMenuItem.DropDownItems.Add(item);
            }
            moveTabToolStripMenuItem.DropDown.Renderer = Renderer;
            moveTabToolStripMenuItem.DropDown.BackColor = BackColor;
            moveTabToolStripMenuItem.DropDown.ForeColor = ForeColor;
        }

        private string GetMoveWindowTitle(TitleBarTabs window)
        {
            string name = (window as AppContainer)?._windowName;
            string title = string.IsNullOrEmpty(name) ? window.SelectedTab?.Caption : name;
            title = string.IsNullOrWhiteSpace(title) ? "New tab" : title.Replace("\r", "").Replace("\n", "");
            int otherTabs = string.IsNullOrEmpty(name) ? Math.Max(0, window.Tabs.Count - 1) : 0;
            string suffix = otherTabs == 0 ? "" : " and " + otherTabs + (otherTabs == 1 ? " other tab" : " other tabs");
            int maxWidth = (int)Math.Round(400 * DeviceDpi / 96.0);
            return ElideWindowTitle(title, Math.Max(1, maxWidth - MeasureWindowTitle(suffix))) + suffix;
        }

        private int MeasureWindowTitle(string title) => TextRenderer.MeasureText(title, Font,
            Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine).Width;

        private string ElideWindowTitle(string title, int maxWidth)
        {
            if (MeasureWindowTitle(title) <= maxWidth) return title;
            // Keep Unicode text elements intact, including emoji and combining characters.
            int[] elements = StringInfo.ParseCombiningCharacters(title);
            int low = Math.Min(4, elements.Length), high = elements.Length;
            while (low < high)
            {
                int middle = (low + high + 1) / 2;
                int end = middle == elements.Length ? title.Length : elements[middle];
                if (MeasureWindowTitle(title.Substring(0, end) + "…") <= maxWidth) low = middle;
                else high = middle - 1;
            }
            return title.Substring(0, low == elements.Length ? title.Length : elements[low]) + "…";
        }

        private void MoveTabToExistingWindow(TitleBarTabs destination)
        {
            if (CanMoveClickedTab && _parentForm.MoveTabToWindow(_clickedTab, destination))
                ActivateMoveWindow(destination);
        }

        private void MoveTabToNewWindow()
        {
            // Chromium disables this when it would move every tab out of the source.
            if (!CanMoveClickedTab || _parentForm.Tabs.Count <= 1) return;
            TitleBarTabs source = _parentForm;
            TitleBarTab tab = _clickedTab;
            TitleBarTabs destination = CreateMoveWindow(source);
            if (destination == null) return;
            if (source.MoveTabToWindow(tab, destination)) ActivateMoveWindow(destination);
            else destination.Close();
        }

        protected virtual TitleBarTabs CreateMoveWindow(TitleBarTabs source)
        {
            var destination = new AppContainer();
            Program.PositionNewAppContainer(destination, source as AppContainer);
            // Run Load before selecting the transferred tab so all browser handlers are attached.
            source.ApplicationContext.Start(destination);
            return destination;
        }

        protected virtual void ActivateMoveWindow(TitleBarTabs destination)
        {
            if (destination is AppContainer browserWindow) browserWindow.ActivateForTabMove();
            else
            {
                if (destination.WindowState == FormWindowState.Minimized)
                    destination.WindowState = FormWindowState.Normal;
                destination.Show();
                destination.Activate();
            }
        }
    }
}
