// Compiles the production router against small host doubles. Native HWNDs are
// real but all forms stay hidden; no Quartz profiles or WebView2 sessions load.
// This checks routing/dispatch policy, not actual WebView2 keyboard delivery.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Quartz;

namespace EasyTabs
{
    internal static class ContextMenuProvider
    {
        internal static AppContainer _parentForm;
        internal static ContextMenuStrip _contextMenuStripNormal = null;
        internal static ContextMenuStrip _contextMenuStripTab;
    }
}

namespace Quartz
{
    internal sealed class TestTab { internal Browser Content; }

    public sealed partial class AppContainer : Form
    {
        internal TestTab SelectedTab;
        internal IntPtr TabStrip;
        internal bool _restoringWindowSettings;
        internal readonly Timer _windowSettingsSaveTimer = new Timer();
        internal readonly List<TestTab> Tabs = new List<TestTab>();
        internal bool OverlayVisible = true;
        internal int SettingsSaves;
        internal readonly List<Delegate> Pending = new List<Delegate>();
        private void SaveWindowSettings() { SettingsSaves++; }
        internal void ResizeTabContents() { }
        internal bool IsTabStripHandle(IntPtr handle) => handle == TabStrip;
        public new IAsyncResult BeginInvoke(Delegate method)
        {
            Pending.Add(method);
            return null;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) _windowSettingsSaveTimer.Dispose();
            base.Dispose(disposing);
        }
    }

    internal sealed class FocusTarget
    {
        internal int FocusCalls;
        internal void Focus() { FocusCalls++; }
    }

    internal sealed class Browser : Form
    {
        internal bool CommandEnabled = true;
        internal bool WasKeyboard;
        internal bool ChromeVisible = true;
        internal readonly FocusTarget wvWebView1 = new FocusTarget();
        internal void ApplyFullscreenChrome(bool fullscreen) { ChromeVisible = !fullscreen; }
        internal readonly List<BrowserCommand> Executed = new List<BrowserCommand>();
        internal readonly HashSet<ToolStripDropDown> Menus = new HashSet<ToolStripDropDown>();
        internal bool CanExecuteShortcutCommand(BrowserCommand command, bool fromKeyboard)
        {
            WasKeyboard = fromKeyboard;
            return CommandEnabled;
        }
        internal Task ExecuteShortcutCommandAsync(BrowserCommand command)
        {
            Executed.Add(command);
            return Task.CompletedTask;
        }
        internal bool OwnsShortcutMenu(ToolStripDropDown menu)
        {
            foreach (var root in Menus)
                if (ShortcutManager.ContainsMenu(root, menu)) return true;
            return false;
        }
        internal void CloseShortcutMenus() { }
    }
}

internal static class ShortcutManagerChecks
{
    private static int _checks;
    private static readonly Type Router = typeof(ShortcutManager);

    private static object Call(ShortcutManager router, string method, params object[] args) =>
        Router.GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(router, args);

    private static void Check(bool condition, string description)
    {
        if (!condition) throw new Exception(description);
        _checks++;
    }

    [STAThread]
    private static int Main()
    {
        try
        {
            CheckDefinitions();
            CheckWindowBoundaries();
            CheckDeferredInputAndRepeat();
            CheckCommandTargets();
            CheckFullscreenState();
            Console.WriteLine("PASS: " + _checks + " shortcut routing checks. No visible windows or WebView2 sessions were opened.");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
    }

    private static void CheckDefinitions()
    {
        var map = (Dictionary<Keys, BrowserCommand>)Router.GetField("CommandsByKey", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        foreach (BrowserCommand command in Enum.GetValues(typeof(BrowserCommand)))
            Check(map.ContainsValue(command), "Missing key for " + command);
        Check(map[Keys.Control | Keys.W] == BrowserCommand.CloseTab, "Ctrl+W closes a tab");
        Check(map[Keys.Control | Keys.Tab] == BrowserCommand.NextTab, "Ctrl+Tab changes tabs");
        Check(map[Keys.Control | Keys.Shift | Keys.Tab] == BrowserCommand.PreviousTab, "Ctrl+Shift+Tab changes tabs backwards");
        Check(map[Keys.F11] == BrowserCommand.Fullscreen, "F11 belongs to the shared definitions");
        Check(map[Keys.Control | Keys.D9] == BrowserCommand.LastTab, "Ctrl+9 selects the last tab");
        Check(map[Keys.Control | Keys.Shift | Keys.D] == BrowserCommand.FavouriteAllTabs, "Favourite all tabs exists before menus open");
        Check(map[Keys.Control | Keys.Shift | Keys.Delete] == BrowserCommand.ClearBrowsingData, "Clear data exists before menus open");
        Check(map[Keys.Control | Keys.Oemplus] == map[Keys.Control | Keys.Add], "Zoom accepts main keyboard and numpad");
        Check(map[Keys.Control | Keys.L] == map[Keys.Alt | Keys.D], "Address aliases agree");
        foreach (Keys key in new[] { Keys.A, Keys.Enter, Keys.Escape, Keys.Tab, Keys.Shift | Keys.Tab,
            Keys.Control | Keys.C, Keys.Control | Keys.X, Keys.Control | Keys.V, Keys.Control | Keys.A,
            Keys.Control | Keys.Z, Keys.Control | Keys.Alt | Keys.D, Keys.Alt | Keys.Space })
            Check(!map.ContainsKey(key), "Normal control/page input must pass through: " + key);
        using (var menu = new ToolStripMenuItem())
        {
            foreach (BrowserCommand command in Enum.GetValues(typeof(BrowserCommand)))
            {
                menu.ShortcutKeys = Keys.Control | Keys.T;
                ShortcutManager.SetMenuShortcut(menu, command);
                Check(menu.ShortcutKeys == Keys.None, "No competing WinForms shortcut for " + command);
                Check(!string.IsNullOrWhiteSpace(menu.ShortcutKeyDisplayString), "Menu label for " + command);
            }
            ShortcutManager.SetMenuShortcut(menu, BrowserCommand.TaskManager);
            Check(menu.ShortcutKeyDisplayString == "Shift+Esc", "Shift+Esc is displayable without invalid ShortcutKeys assignment");
        }
    }

    private static void CheckWindowBoundaries()
    {
        using (var first = new AppContainer())
        using (var second = new AppContainer())
        using (var browser = new Browser { TopLevel = false, Parent = first })
        using (var dialog = new Form { Owner = first })
        using (var dialogChild = new TextBox { Parent = dialog })
        using (var overlay = new Form { Owner = first })
        using (var router = new ShortcutManager(first))
        using (var otherRouter = new ShortcutManager(second))
        using (var menu = new ContextMenuStrip())
        using (var nested = new ToolStripDropDown())
        using (var tabMenu = new ContextMenuStrip())
        {
            first.SelectedTab = new TestTab { Content = browser };
            IntPtr main = first.Handle;
            IntPtr other = second.Handle;
            first.TabStrip = overlay.Handle;
            Func<IntPtr, bool> accepts = handle => (bool)Call(router, "IsInputSurface", handle);
            Check(accepts(main), "Main window is in scope");
            Check(accepts(browser.Handle), "Embedded Browser child is in scope");
            Check(accepts(overlay.Handle), "EasyTabs overlay is in scope");
            Check(!accepts(other), "Another main window is outside this router");
            Check(!(bool)Call(otherRouter, "IsInputSurface", main), "Window isolation works both ways");
            Check(!accepts(dialog.Handle), "Owned dialog is excluded");
            Check(!accepts(dialogChild.Handle), "Textbox inside owned dialog is excluded");
            Check(!accepts(IntPtr.Zero), "Missing input target is excluded");
            browser.Menus.Add(menu);
            var parent = new ToolStripMenuItem("Parent") { DropDown = nested };
            nested.Items.Add("Child");
            menu.Items.Add(parent);
            Check(accepts(menu.Handle), "Browser menu is in scope");
            Check(accepts(nested.Handle), "Nested browser menu is in scope");
            Check(!(bool)Call(otherRouter, "IsInputSurface", nested.Handle), "Another browser's menu is excluded");
            EasyTabs.ContextMenuProvider._contextMenuStripTab = tabMenu;
            EasyTabs.ContextMenuProvider._parentForm = first;
            Check(accepts(tabMenu.Handle), "Shared tab menu belongs to its current window");
            EasyTabs.ContextMenuProvider._parentForm = second;
            Check(!accepts(tabMenu.Handle), "Stale shared menu ownership is excluded");
            Check(!(bool)Call(router, "CanRouteInput"), "Hidden/background window cannot execute shortcuts");
            router.Dispose();
            Check(!(bool)Call(router, "CanRouteInput"), "Disposed router cannot execute shortcuts");
            EasyTabs.ContextMenuProvider._contextMenuStripTab = null;
            EasyTabs.ContextMenuProvider._parentForm = null;
        }
    }

    private static void CheckDeferredInputAndRepeat()
    {
        using (var window = new AppContainer())
        using (var browser = new Browser())
        using (var router = new ShortcutManager(window))
        {
            window.SelectedTab = new TestTab { Content = browser };
            Check((bool)Call(router, "HandleKeyDown", Keys.Control | Keys.W, false), "Ctrl+W is handled");
            Check(window.Pending.Count == 1 && browser.Executed.Count == 0, "Command is queued, never executed in an input callback");
            Call(router, "HandleKeyDown", Keys.Control | Keys.W, false);
            Check(window.Pending.Count == 1, "Repeated WebView2 keydown cannot close another tab");
            var release = new KeyEventArgs(Keys.W); // Ctrl may have been released first.
            router.WebViewKeyUp(browser, release);
            Check(release.Handled, "Release is matched by key code without requiring old modifiers");
            Call(router, "HandleKeyDown", Keys.Control | Keys.W, false);
            Check(window.Pending.Count == 2, "A fresh key press queues another command");
            Call(router, "HandleKeyDown", Keys.F11, true);
            Check(window.Pending.Count == 2, "Native repeat bit prevents fullscreen retoggling");
            router.WebViewKeyUp(browser, new KeyEventArgs(Keys.F11));
            Call(router, "HandleKeyDown", Keys.F11, false);
            Check(window.Pending.Count == 3, "Initial F11 is queued");
            Call(router, "OnDeactivate", window, EventArgs.Empty);
            Call(router, "HandleKeyDown", Keys.F11, false);
            Check(window.Pending.Count == 3, "Activation transitions do not forget held F11");
            Call(router, "HandleKeyDown", Keys.Control | Keys.Oemplus, false);
            Call(router, "HandleKeyDown", Keys.Control | Keys.Oemplus, true);
            Check(window.Pending.Count == 5, "Zoom may repeat while held");
            Check(!(bool)Call(router, "HandleKeyDown", Keys.Control | Keys.C, false), "Copy is not consumed");
            var backgroundKey = new KeyEventArgs(Keys.Control | Keys.T);
            router.WebViewKeyDown(browser, backgroundKey);
            Check(!backgroundKey.Handled, "Background WebView2 event is ignored");
            // Pending callbacks also recheck foreground state and disposal.
            foreach (var pending in window.Pending) pending.DynamicInvoke();
            Check(browser.Executed.Count == 0, "Queued input cannot act in a background window");
        }
    }

    private static void CheckCommandTargets()
    {
        using (var selected = new Browser())
        using (var clicked = new Browser())
        {
            ShortcutManager.ExecuteCommand(clicked, BrowserCommand.CloseTab);
            Check(clicked.Executed.Count == 1 && selected.Executed.Count == 0, "Menu action keeps its explicitly clicked tab target");
            ShortcutManager.ExecuteCommand(selected, BrowserCommand.Reload, fromKeyboard: true);
            Check(selected.Executed.Count == 1 && selected.WasKeyboard, "Keyboard origin reaches availability policy");
            selected.CommandEnabled = false;
            ShortcutManager.ExecuteCommand(selected, BrowserCommand.AddFavourite);
            Check(selected.Executed.Count == 1, "Unavailable commands do not execute");
            selected.Dispose();
            ShortcutManager.ExecuteCommand(selected, BrowserCommand.CloseTab);
            Check(selected.Executed.Count == 1, "Disposed tab is ignored");
            ShortcutManager.ExecuteCommand(null, BrowserCommand.CloseTab);
            Check(true, "Missing tab is ignored");
        }
    }

    private static void CheckFullscreenState()
    {
        using (var window = new AppContainer())
        using (var first = new Browser())
        using (var second = new Browser())
        {
            window.Bounds = new Rectangle(120, 140, 950, 680);
            Rectangle originalBounds = window.Bounds;
            window.TopMost = true;
            var original = new TestTab { Content = first };
            var replacement = new TestTab { Content = second };
            window.Tabs.Add(original);
            window.Tabs.Add(replacement);
            window.SelectedTab = original;
            Check(!window.IsHandleCreated, "Fullscreen state check starts without native windows");
            window.FullScreen = true;
            Check(window.FullScreen && !window.OverlayVisible, "Fullscreen hides the overlay");
            Check(!first.ChromeVisible && !second.ChromeVisible, "Fullscreen chrome state reaches every tab");
            Check(window.SettingsSaves == 1, "Normal window state is saved before fullscreen");
            window.SelectedTab = replacement;
            window.Tabs.Remove(original);
            Check(window.FullScreen, "Switching and closing the originating tab preserves fullscreen state");
            window.FullScreen = false;
            Check(window.Bounds == originalBounds, "F11 restores original bounds after the original tab closes");
            Check(window.WindowState == FormWindowState.Normal, "F11 restores window state");
            Check(window.TopMost, "F11 preserves the original TopMost value");
            Check(window.OverlayVisible && second.ChromeVisible, "Exit restores the selected tab's chrome");
            Check(second.wvWebView1.FocusCalls == 1, "Exit focuses the newly selected tab");
            Check(!window._restoringWindowSettings, "Fullscreen restores the persistence guard");
            Check(!window.IsHandleCreated, "Fullscreen state check never creates a native window");
        }
    }
}
