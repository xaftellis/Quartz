using Quartz.Controls.ChromiumMenus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EasyTabs;

namespace Quartz
{
    internal enum BrowserCommand
    {
        NewTab, NewWindow, CloseTab, CloseWindow, Reload, ReloadIgnoringCache,
        Find, Print, OpenFile, History, Downloads, ClearBrowsingData, Profiles,
        AddFavourite, FavouriteAllTabs, ToggleFavouritesBar, DeveloperTools,
        TaskManager, Fullscreen, FocusAddress, NextTab, PreviousTab,
        Tab1, Tab2, Tab3, Tab4, Tab5, Tab6, Tab7, Tab8, LastTab,
        Back, Forward, ZoomIn, ZoomOut, ResetZoom
    }

    // One definition feeds menu labels and both WinForms and WebView2 input.
    // Each main window owns a router; dialogs never register one.
    internal sealed class ShortcutManager : IMessageFilter, IDisposable
    {
        private sealed class Binding
        {
            internal readonly BrowserCommand Command;
            internal readonly Keys[] Keys;

            internal Binding(BrowserCommand command, params Keys[] keys)
            {
                Command = command;
                Keys = keys;
            }
        }

        private static readonly Binding[] Bindings =
        {
            new Binding(BrowserCommand.NewTab, Keys.Control | Keys.T),
            new Binding(BrowserCommand.NewWindow, Keys.Control | Keys.N),
            new Binding(BrowserCommand.CloseTab, Keys.Control | Keys.W, Keys.Control | Keys.F4),
            new Binding(BrowserCommand.CloseWindow, Keys.Alt | Keys.F4, Keys.Control | Keys.Shift | Keys.W),
            new Binding(BrowserCommand.Reload, Keys.Control | Keys.R, Keys.F5),
            new Binding(BrowserCommand.ReloadIgnoringCache, Keys.Control | Keys.Shift | Keys.R, Keys.Control | Keys.F5, Keys.Shift | Keys.F5),
            new Binding(BrowserCommand.Find, Keys.Control | Keys.F),
            new Binding(BrowserCommand.Print, Keys.Control | Keys.P),
            new Binding(BrowserCommand.OpenFile, Keys.Control | Keys.O),
            new Binding(BrowserCommand.History, Keys.Control | Keys.H),
            new Binding(BrowserCommand.Downloads, Keys.Control | Keys.J),
            new Binding(BrowserCommand.ClearBrowsingData, Keys.Control | Keys.Shift | Keys.Delete),
            new Binding(BrowserCommand.Profiles, Keys.Control | Keys.Shift | Keys.M),
            new Binding(BrowserCommand.AddFavourite, Keys.Control | Keys.D),
            new Binding(BrowserCommand.FavouriteAllTabs, Keys.Control | Keys.Shift | Keys.D),
            new Binding(BrowserCommand.ToggleFavouritesBar, Keys.Control | Keys.Shift | Keys.B),
            new Binding(BrowserCommand.DeveloperTools, Keys.F12, Keys.Control | Keys.Shift | Keys.I),
            new Binding(BrowserCommand.TaskManager, Keys.Shift | Keys.Escape),
            new Binding(BrowserCommand.Fullscreen, Keys.F11),
            new Binding(BrowserCommand.FocusAddress, Keys.Control | Keys.L, Keys.Alt | Keys.D, Keys.F6),
            new Binding(BrowserCommand.NextTab, Keys.Control | Keys.Tab, Keys.Control | Keys.PageDown),
            new Binding(BrowserCommand.PreviousTab, Keys.Control | Keys.Shift | Keys.Tab, Keys.Control | Keys.PageUp),
            new Binding(BrowserCommand.Tab1, Keys.Control | Keys.D1, Keys.Control | Keys.NumPad1),
            new Binding(BrowserCommand.Tab2, Keys.Control | Keys.D2, Keys.Control | Keys.NumPad2),
            new Binding(BrowserCommand.Tab3, Keys.Control | Keys.D3, Keys.Control | Keys.NumPad3),
            new Binding(BrowserCommand.Tab4, Keys.Control | Keys.D4, Keys.Control | Keys.NumPad4),
            new Binding(BrowserCommand.Tab5, Keys.Control | Keys.D5, Keys.Control | Keys.NumPad5),
            new Binding(BrowserCommand.Tab6, Keys.Control | Keys.D6, Keys.Control | Keys.NumPad6),
            new Binding(BrowserCommand.Tab7, Keys.Control | Keys.D7, Keys.Control | Keys.NumPad7),
            new Binding(BrowserCommand.Tab8, Keys.Control | Keys.D8, Keys.Control | Keys.NumPad8),
            new Binding(BrowserCommand.LastTab, Keys.Control | Keys.D9, Keys.Control | Keys.NumPad9),
            new Binding(BrowserCommand.Back, Keys.Alt | Keys.Left),
            new Binding(BrowserCommand.Forward, Keys.Alt | Keys.Right),
            new Binding(BrowserCommand.ZoomIn, Keys.Control | Keys.Oemplus, Keys.Control | Keys.Shift | Keys.Oemplus, Keys.Control | Keys.Add),
            new Binding(BrowserCommand.ZoomOut, Keys.Control | Keys.OemMinus, Keys.Control | Keys.Subtract),
            new Binding(BrowserCommand.ResetZoom, Keys.Control | Keys.D0, Keys.Control | Keys.NumPad0)
        };

        private static readonly Dictionary<Keys, BrowserCommand> CommandsByKey = Bindings
            .SelectMany(binding => binding.Keys.Select(key => new { key, binding.Command }))
            .ToDictionary(binding => binding.key, binding => binding.Command);

        private readonly AppContainer _window;
        private readonly HashSet<Keys> _pressedKeys = new HashSet<Keys>();
        private bool _disposed;
        private int _activation;

        internal ShortcutManager(AppContainer window)
        {
            _window = window;
            _window.Deactivate += OnDeactivate;
            _window.Activated += OnActivated;
            _window.Disposed += OnDisposed;
            Application.AddMessageFilter(this);
        }

        internal static string GetDisplayShortcut(BrowserCommand command)
        {
            Keys key = Bindings.First(binding => binding.Command == command).Keys[0];
            return new KeysConverter().ConvertToInvariantString(key)
                .Replace("Escape", "Esc").Replace("Oemplus", "+").Replace("OemMinus", "-");
        }

        internal static void SetMenuShortcut(ChromiumMenuItem item, BrowserCommand command)
        {
            // WinForms must not independently execute the same key, or use a
            // context menu's remembered right-click target for keyboard input.
            item.ShortcutKeys = Keys.None;
            item.ShortcutKeyDisplayString = GetDisplayShortcut(command);
        }

        internal static async void ExecuteCommand(Browser browser, BrowserCommand command, bool fromKeyboard = false)
        {
            if (browser == null || browser.IsDisposed || browser.Disposing) return;
            try
            {
                if (browser.CanExecuteShortcutCommand(command, fromKeyboard))
                    await browser.ExecuteShortcutCommandAsync(command);
            }
            catch (Exception error)
            {
                if (!browser.IsDisposed && !browser.Disposing)
                    MessageBox.Show(browser, error.Message, "Couldn't complete the command",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public bool PreFilterMessage(ref Message message)
        {
            if (MenuSession.FilterKeys(ref message)) return true;
            bool keyDown = message.Msg == 0x0100 || message.Msg == 0x0104;
            bool keyUp = message.Msg == 0x0101 || message.Msg == 0x0105;
            if ((!keyDown && !keyUp) || !CanRouteInput() || !IsInputSurface(message.HWnd)) return false;

            Keys key = (Keys)message.WParam.ToInt32() & Keys.KeyCode;
            if (keyUp) return _pressedKeys.Remove(key);
            bool repeated = (message.LParam.ToInt64() & (1L << 30)) != 0;
            return HandleKeyDown(key | Control.ModifierKeys, repeated);
        }

        internal void WebViewKeyDown(Browser browser, KeyEventArgs e)
        {
            if (browser != _window.SelectedTab?.Content || !CanRouteInput()) return;
            // Handled suppresses the WebView2 accelerator. Do not suppress its
            // KeyUp event: that event ends the shared key-repeat tracking.
            if (HandleKeyDown(e.KeyData, _pressedKeys.Contains(e.KeyCode))) e.Handled = true;
        }

        internal void WebViewKeyUp(Browser browser, KeyEventArgs e)
        {
            if (browser == _window.SelectedTab?.Content && _pressedKeys.Remove(e.KeyCode))
                e.Handled = true;
        }

        private bool HandleKeyDown(Keys keyData, bool repeated)
        {
            if (!CommandsByKey.TryGetValue(keyData, out BrowserCommand command)) return false;
            Keys key = keyData & Keys.KeyCode;
            bool alreadyPressed = !_pressedKeys.Add(key);
            bool allowRepeat = command == BrowserCommand.NextTab || command == BrowserCommand.PreviousTab ||
                command == BrowserCommand.ZoomIn || command == BrowserCommand.ZoomOut;
            if ((repeated || alreadyPressed) && !allowRepeat) return true;

            var browser = _window.SelectedTab?.Content as Browser;
            int activation = _activation;
            // Both input paths leave the input callback before invoking browser
            // APIs, opening modal dialogs, closing a tab, or resizing WebView2.
            _window.BeginInvoke((Action)(() =>
            {
                if (activation != _activation || !CanRouteInput() || browser != _window.SelectedTab?.Content) return;
                CloseBrowserMenus();
                ExecuteCommand(browser, command, fromKeyboard: true);
            }));
            return true;
        }

        private bool CanRouteInput()
        {
            return !_disposed && !_window.IsDisposed && !_window.Disposing && _window.IsHandleCreated &&
                _window.Visible && _window.Enabled && IsWindowEnabled(_window.Handle) &&
                IsInputSurface(GetForegroundWindow());
        }

        private bool IsInputSurface(IntPtr handle)
        {
            if (handle == IntPtr.Zero || !_window.IsHandleCreated) return false;
            if (handle == _window.Handle || GetAncestor(handle, 2) == _window.Handle) return true;
            if (_window.IsTabStripHandle(handle)) return true;

            return MenuSession.OwnsHandle(handle);
        }

        internal static bool ContainsMenu(ChromiumMenu root, ChromiumMenu target)
        {
            return root != null && !root.IsDisposed && (root == target ||
                root.Items.Any(item => item.HasSubmenu && ContainsMenu(item.DropDown, target)));
        }

        private void CloseBrowserMenus()
        {
            (_window.SelectedTab?.Content as Browser)?.CloseShortcutMenus();
            if (ContextMenuProvider._parentForm != _window) return;
            ContextMenuProvider._contextMenuStripNormal?.Close();
            ContextMenuProvider._contextMenuStripTab?.Close();
        }

        private void OnDeactivate(object sender, EventArgs e)
        {
            ++_activation;
        }

        private void OnActivated(object sender, EventArgs e)
        {
            // A release can happen in a dialog/another app. Keep keys that are
            // still physically held, including F11 during a fullscreen resize.
            _pressedKeys.RemoveWhere(key => (GetAsyncKeyState((int)key) & 0x8000) == 0);
        }

        private void OnDisposed(object sender, EventArgs e) => Dispose();

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            ++_activation;
            _pressedKeys.Clear();
            Application.RemoveMessageFilter(this);
            _window.Deactivate -= OnDeactivate;
            _window.Activated -= OnActivated;
            _window.Disposed -= OnDisposed;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr GetAncestor(IntPtr window, uint flags);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowEnabled(IntPtr window);
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int key);
    }
}
