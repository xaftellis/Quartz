using EasyTabs;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quartz
{
    public partial class Browser
    {
        private Task _initializationTask;
        private CoreWebView2Environment _openingEnvironment;
        private bool _requestedWebWindow;
        private TabOpenGesture _tabOpenGesture;

        internal Task EnsureBrowserInitializedAsync() =>
            _initializationTask ?? (_initializationTask = InitializeBrowserAsync());

        internal async void StartTabInitialization()
        {
            try { await EnsureBrowserInitializedAsync(); }
            catch (Exception error) { Trace.TraceError("Could not initialize tab: {0}", error); }
        }

        private void InitializeTabOpening()
        {
            _tabOpenGesture = new TabOpenGesture(wvWebView1,
                () => ParentTabs?.SelectedTab?.Content == this && ParentTabs.ContainsFocus);
            Disposed += (sender, e) => _tabOpenGesture.Dispose();
            wvWebView1.CoreWebView2.ContextMenuRequested += TabOpeningContextMenuRequested;
            wvWebView1.CoreWebView2.NavigationStarting += (sender, e) => _tabOpenGesture.Clear();
        }

        internal Browser OpenTab(string address, TabOpenDisposition disposition,
            CoreWebView2Environment environment = null, bool requestedWebWindow = false,
            int? insertionIndex = null, bool pinned = false)
        {
            var window = ParentTabs as AppContainer;
            if (window == null || window.IsDisposed || window.Disposing) return null;
            if (window.InvokeRequired)
                throw new InvalidOperationException("Tabs must be opened on the UI thread.");
            if (disposition == TabOpenDisposition.CurrentTab || disposition == TabOpenDisposition.SaveToDisk)
                throw new ArgumentException("The disposition must create a tab or window.", nameof(disposition));

            bool newWindow = disposition == TabOpenDisposition.NewWindow;
            if (newWindow)
            {
                window = new AppContainer(((AppContainer)ParentTabs).ProfileId);
                Program.PositionNewAppContainer(window, (AppContainer)ParentTabs);
            }

            var browser = new Browser(address, address != null)
            {
                _openingEnvironment = environment,
                _requestedWebWindow = requestedWebWindow
            };
            browser.InitializeTab();
            var tab = new TitleBarTab(window)
            {
                Content = browser,
                Caption = address == null ? "New Tab" : "Loading...",
                IsPinned = pinned
            };
            bool activate = TabOpenPolicy.ShouldActivate(disposition, window.Tabs.Count == 0);
            // Keep existing link placement; select by identity because the
            // pinned-prefix collection may adjust the actual insertion index.
            int index = insertionIndex ?? (newWindow || address == null
                ? window.Tabs.Count : window.Tabs.IndexOf(window.Tabs.Find(t => t.Content == this)) + 1);
            window.Tabs.Insert(Math.Max(0, Math.Min(index, window.Tabs.Count)), tab);
            window.ResizeTabContents(tab);
            if (activate) window.SelectedTab = tab;
            // Form.Load only fires when shown; background tabs must load too.
            browser.StartTabInitialization();
            window.RedrawTabs();
            if (newWindow) Program.EasyTabsContext.Start(window);
            return browser;
        }

        internal void OpenFavouriteOrHistory(string address, MouseButtons button, Keys modifiers)
        {
            var disposition = TabOpenPolicy.FromClick(button, modifiers);
            if (disposition == TabOpenDisposition.CurrentTab) SetSource(address);
            else if (disposition == TabOpenDisposition.SaveToDisk)
            {
                // WebView2 has no native download-URL command. Keep Quartz's
                // existing navigation for this non-tab-opening action.
                SetSource(address);
            }
            else OpenTab(address, disposition);
        }

        private void NavigateFromAddressBar(Uri address, Keys modifiers)
        {
            var disposition = TabOpenPolicy.FromAddressBar(modifiers);
            if (disposition == TabOpenDisposition.CurrentTab) SetSource(address);
            else OpenTab(address.AbsoluteUri, disposition, insertionIndex: ParentTabs.Tabs.Count);
            if (ParentTabs.SelectedTab?.Content == this) wvWebView1.Focus();
        }

        private void TabOpeningContextMenuRequested(object sender, CoreWebView2ContextMenuRequestedEventArgs e)
        {
            _tabOpenGesture.Clear();
            // Default WebView commands do not report their disposition to the
            // host. Replace only the explicit open commands, keeping the native
            // menu and all its other commands. These are ordinary URL opens;
            // renderer-created windows still use NewWindowRequested.NewWindow.
            if (e.ContextMenuTarget.HasLinkUri &&
                !e.MenuItems.Any(item => item.Name == "openLinkInNewTab"))
            {
                // WebView2's default menu can expose only "new window".
                var nativeOpen = e.MenuItems.FirstOrDefault(item => item.Name == "openLinkInNewWindow");
                e.MenuItems.Insert(0, CreateTabOpeningMenuItem("Open link in new tab",
                    e.ContextMenuTarget.LinkUri, TabOpenDisposition.NewBackgroundTab, nativeOpen?.IsEnabled ?? true));
            }
            ReplaceTabOpeningMenuItems(e.MenuItems, e.ContextMenuTarget);
        }

        private void ReplaceTabOpeningMenuItems(IList<CoreWebView2ContextMenuItem> items,
            CoreWebView2ContextMenuTarget target)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var original = items[i];
                if (original.Kind == CoreWebView2ContextMenuItemKind.Submenu)
                {
                    ReplaceTabOpeningMenuItems(original.Children, target);
                    continue;
                }
                string address;
                var disposition = TabOpenDisposition.NewBackgroundTab;
                switch (original.Name)
                {
                    case "openLinkInNewTab": address = target.LinkUri; break;
                    case "openImageInNewTab": address = target.SourceUri; break;
                    case "openVideoInNewTab": address = target.SourceUri; break;
                    case "openAudioInNewTab": address = target.SourceUri; break;
                    case "openFrameInNewTab": address = target.FrameUri; break;
                    case "openLinkInNewWindow":
                        address = target.LinkUri;
                        disposition = TabOpenDisposition.NewWindow;
                        break;
                    default: continue;
                }
                if (string.IsNullOrEmpty(address)) continue;
                var replacement = CreateTabOpeningMenuItem(original.Label, address, disposition, original.IsEnabled);
                items.RemoveAt(i);
                items.Insert(i, replacement);
            }
        }

        private CoreWebView2ContextMenuItem CreateTabOpeningMenuItem(string label,
            string address, TabOpenDisposition disposition, bool enabled)
        {
            var item = wvWebView1.CoreWebView2.Environment.CreateContextMenuItem(
                label, null, CoreWebView2ContextMenuItemKind.Command);
            item.IsEnabled = enabled;
            item.CustomItemSelected += (sender, e) =>
            {
                // Leave WebView's callback before creating another control.
                if (!IsDisposed && !Disposing)
                    BeginInvoke((Action)(() =>
                    {
                        if (!IsDisposed && !Disposing) OpenTab(address, disposition);
                    }));
            };
            return item;
        }
    }
}
