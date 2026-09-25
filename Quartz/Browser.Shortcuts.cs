using Quartz.Controls.ChromiumMenus;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Quartz.Services;

namespace Quartz
{
    public partial class Browser
    {
        private void InitializeShortcuts()
        {
            wvWebView1.KeyDown += (sender, e) => (ParentTabs as AppContainer)?.Shortcuts.WebViewKeyDown(this, e);
            wvWebView1.KeyUp += (sender, e) => (ParentTabs as AppContainer)?.Shortcuts.WebViewKeyUp(this, e);

            BindShortcutMenu(newTabToolStripMenuItem1, BrowserCommand.NewTab, newTabToolStripMenuItem_Click);
            BindShortcutMenu(newWindowToolStripMenuItem, BrowserCommand.NewWindow, newWindowToolStripMenuItem_Click);
            BindShortcutMenu(changeProfileToolStripMenuItem, BrowserCommand.Profiles, changeProfileToolStripMenuItem_Click);
            BindShortcutMenu(downloadsToolStripMenuItem, BrowserCommand.Downloads, btnDownload_Click);
            BindShortcutMenu(historyToolStripMenuItem1, BrowserCommand.History, historyToolStripMenuItem_Click);
            BindShortcutMenu(addFavouritesToolStripMenuItem, BrowserCommand.AddFavourite);
            BindShortcutMenu(findToolStripMenuItem, BrowserCommand.Find, findToolStripMenuItem_Click);
            BindShortcutMenu(printToolStripMenuItem, BrowserCommand.Print, printToolStripMenuItem_Click);
            BindShortcutMenu(openFileInBrowserToolStripMenuItem, BrowserCommand.OpenFile, openFileInBrowserToolStripMenuItem_Click);
            BindShortcutMenu(inspectToolStripMenuItem, BrowserCommand.DeveloperTools, inspectToolStripMenuItem_Click);
            BindShortcutMenu(webview2TaskManagerToolStripMenuItem, BrowserCommand.TaskManager, taskManagerToolStripMenuItem_Click);
        }

        private void BindShortcutMenu(ChromiumMenuItem item, BrowserCommand command, EventHandler oldHandler = null)
        {
            ShortcutManager.SetMenuShortcut(item, command);
            if (oldHandler != null) item.Click -= oldHandler;
            item.Click += (sender, e) => ShortcutManager.ExecuteCommand(this, command);
        }

        internal bool OwnsShortcutMenu(ChromiumMenu menu)
        {
            return components != null && components.Components.OfType<ChromiumMenu>()
                .Any(root => ShortcutManager.ContainsMenu(root, menu));
        }

        internal void CloseShortcutMenus()
        {
            if (components == null) return;
            foreach (var menu in components.Components.OfType<ChromiumMenu>().ToArray())
                if (!menu.IsDisposed && menu.Visible) menu.Close();
        }

        internal bool CanExecuteShortcutCommand(BrowserCommand command, bool fromKeyboard = false)
        {
            var window = ParentTabs as AppContainer;
            if (IsDisposed || Disposing || window == null || window.IsDisposed || window.Disposing ||
                !window.Tabs.Any(tab => tab.Content == this)) return false;

            var core = wvWebView1.CoreWebView2;
            // Preserve the existing setting for WebView2's browser accelerators.
            // Menu clicks and Quartz's tab/window commands remain available.
            if (fromKeyboard && core != null && IsWebViewAccelerator(command) && !core.Settings.AreBrowserAcceleratorKeysEnabled)
                return false;
            if (fromKeyboard && core != null && (command == BrowserCommand.ZoomIn || command == BrowserCommand.ZoomOut ||
                command == BrowserCommand.ResetZoom) && !core.Settings.IsZoomControlEnabled) return false;
            switch (command)
            {
                case BrowserCommand.Reload:
                case BrowserCommand.ReloadIgnoringCache:
                case BrowserCommand.Find:
                case BrowserCommand.Print:
                case BrowserCommand.Downloads:
                case BrowserCommand.ClearBrowsingData:
                case BrowserCommand.TaskManager:
                case BrowserCommand.ZoomIn:
                case BrowserCommand.ZoomOut:
                case BrowserCommand.ResetZoom:
                    return core != null;
                case BrowserCommand.DeveloperTools:
                    return core != null && core.Settings.AreDevToolsEnabled;
                case BrowserCommand.AddFavourite:
                    return core != null && wvWebView1.Source != null;
                case BrowserCommand.FavouriteAllTabs:
                    return window.Tabs.Count > 1 && GetTabsToFavourite().Count > 0;
                case BrowserCommand.History:
                    return Program.profileService.Get(window.ProfileId)?.isDisposable == false;
                case BrowserCommand.Back:
                    return core != null && wvWebView1.CanGoBack;
                case BrowserCommand.Forward:
                    return core != null && wvWebView1.CanGoForward;
                case BrowserCommand.NextTab:
                case BrowserCommand.PreviousTab:
                    return window.Tabs.Count > 1;
                default:
                    return true;
            }
        }

        private static bool IsWebViewAccelerator(BrowserCommand command)
        {
            return command == BrowserCommand.Reload || command == BrowserCommand.ReloadIgnoringCache ||
                command == BrowserCommand.Find || command == BrowserCommand.Print ||
                command == BrowserCommand.DeveloperTools || command == BrowserCommand.Back ||
                command == BrowserCommand.Forward || command == BrowserCommand.ZoomIn ||
                command == BrowserCommand.ZoomOut || command == BrowserCommand.ResetZoom;
        }

        internal Task ExecuteShortcutCommandAsync(BrowserCommand command)
        {
            var window = (AppContainer)ParentTabs;
            switch (command)
            {
                case BrowserCommand.NewTab: newTabToolStripMenuItem_Click(this, EventArgs.Empty); break;
                case BrowserCommand.NewWindow: newWindowToolStripMenuItem_Click(this, EventArgs.Empty); break;
                case BrowserCommand.CloseTab: Close(); break;
                case BrowserCommand.CloseWindow: window.Close(); break;
                case BrowserCommand.Reload: wvWebView1.Reload(); break;
                case BrowserCommand.ReloadIgnoringCache:
                    return wvWebView1.CoreWebView2.CallDevToolsProtocolMethodAsync("Page.reload", "{\"ignoreCache\":true}");
                case BrowserCommand.Find: return OpenFindAsync();
                case BrowserCommand.Print: printToolStripMenuItem_Click(this, EventArgs.Empty); break;
                case BrowserCommand.OpenFile: openFileInBrowserToolStripMenuItem_Click(this, EventArgs.Empty); break;
                case BrowserCommand.History: historyToolStripMenuItem_Click(this, EventArgs.Empty); break;
                case BrowserCommand.Downloads: btnDownload_Click(this, EventArgs.Empty); break;
                case BrowserCommand.ClearBrowsingData: ClearHistoryItem_Click(this, EventArgs.Empty); break;
                case BrowserCommand.Profiles: changeProfileToolStripMenuItem_Click(this, EventArgs.Empty); break;
                case BrowserCommand.AddFavourite: btnAddFavourite_Click(this, EventArgs.Empty); break;
                case BrowserCommand.FavouriteAllTabs: AddAllTabsToFavourites_Click(this, EventArgs.Empty); break;
                case BrowserCommand.ToggleFavouritesBar:
                    SettingsService.Set("showFavouritesBar", (SettingsService.Get("showFavouritesBar", window.ProfileId) != "true").ToString().ToLowerInvariant(), window.ProfileId);
                    UpdateFavBar();
                    break;
                case BrowserCommand.DeveloperTools: inspectToolStripMenuItem_Click(this, EventArgs.Empty); break;
                case BrowserCommand.TaskManager: taskManagerToolStripMenuItem_Click(this, EventArgs.Empty); break;
                case BrowserCommand.Fullscreen: window.FullScreen = !window.FullScreen; break;
                case BrowserCommand.FocusAddress:
                    // The address bar is hidden in fullscreen.
                    if (window.FullScreen) window.FullScreen = false;
                    txtWebAddress.Focus();
                    txtWebAddress.SelectAll();
                    break;
                case BrowserCommand.NextTab: SelectShortcutTab((window.SelectedTabIndex + 1) % window.Tabs.Count); break;
                case BrowserCommand.PreviousTab: SelectShortcutTab((window.SelectedTabIndex + window.Tabs.Count - 1) % window.Tabs.Count); break;
                case BrowserCommand.LastTab: SelectShortcutTab(window.Tabs.Count - 1); break;
                case BrowserCommand.Back: wvWebView1.GoBack(); break;
                case BrowserCommand.Forward: wvWebView1.GoForward(); break;
                case BrowserCommand.ZoomIn: StepMenuZoom(1); break;
                case BrowserCommand.ZoomOut: StepMenuZoom(-1); break;
                case BrowserCommand.ResetZoom: SetMenuZoom(1); break;
                default:
                    if (command >= BrowserCommand.Tab1 && command <= BrowserCommand.Tab8)
                        SelectShortcutTab(command - BrowserCommand.Tab1);
                    break;
            }
            return Task.CompletedTask;
        }

        private void SelectShortcutTab(int index)
        {
            if (index < 0 || index >= ParentTabs.Tabs.Count) return;
            ParentTabs.SelectedTabIndex = index;
            if (ParentTabs.SelectedTab?.Content is Browser selected)
                selected.wvWebView1.Focus();
        }

        private Task OpenFindAsync()
        {
            // Empty FindTerm opens WebView2's own Find bar without replaying
            // Ctrl+F through this router (which would recursively trigger Find).
            var core = wvWebView1.CoreWebView2;
            var options = core.Environment.CreateFindOptions();
            options.FindTerm = string.Empty;
            options.SuppressDefaultFindDialog = false;
            wvWebView1.Focus();
            return core.Find.StartAsync(options);
        }

        internal void ApplyFullscreenChrome(bool fullscreen)
        {
            pnlTop.Visible = !fullscreen;
            pnlDivider.Visible = !fullscreen;
        }
    }
}
