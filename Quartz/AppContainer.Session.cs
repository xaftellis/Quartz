using EasyTabs;
using Quartz.Models;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Quartz
{
    public partial class AppContainer
    {
        internal Guid SessionWindowId { get; private set; } = Guid.NewGuid();
        internal bool SessionClosing { get; private set; }

        internal SessionWindowModel CaptureSessionWindow()
        {
            Rectangle normalBounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
            var state = WindowState == FormWindowState.Minimized ? _lastNonMinimizedWindowState : WindowState;
            var window = new SessionWindowModel
            {
                Id = SessionWindowId,
                X = normalBounds.X,
                Y = normalBounds.Y,
                Width = normalBounds.Width,
                Height = normalBounds.Height,
                Maximized = state == FormWindowState.Maximized,
                Name = _windowName
            };
            foreach (var tab in Tabs.ToArray())
            {
                if (!(tab.Content is Browser browser) || browser.IsDisposed) continue;
                if (tab == SelectedTab) window.SelectedTabIndex = window.Tabs.Count;
                window.Tabs.Add(browser.CaptureSessionTab(tab.IsPinned));
            }
            return window;
        }

        internal void RestoreSessionWindow(SessionWindowModel window)
        {
            SessionWindowId = window.Id == Guid.Empty ? Guid.NewGuid() : window.Id;
            _windowName = window.Name ?? string.Empty;
            if (window.Width > 0 && window.Height > 0)
            {
                _savedWindowSize = new Size(Math.Max(MinimumWindowSize.Width, window.Width),
                    Math.Max(MinimumWindowSize.Height, window.Height));
                _savedWindowPosition = new Point(window.X, window.Y);
            }
            _savedWindowState = window.Maximized ? FormWindowState.Maximized : FormWindowState.Normal;
            ApplyWindowSettings();
        }

        protected override void CloseTab(TitleBarTab closingTab)
        {
            base.CloseTab(closingTab);
            Program.Session?.Checkpoint();
        }
    }
}
