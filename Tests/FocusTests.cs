using EasyTabs;
using Microsoft.Web.WebView2.Core;
using Quartz;
using Quartz.Controls;
using Quartz.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

internal static class FocusTests
{
    private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;
    private static int checks;
    private const string PageUrl = "https://www.example.test/path";
    [DllImport("user32.dll")] private static extern IntPtr SetFocus(IntPtr hwnd);
    private delegate bool EnumWindow(IntPtr hwnd, IntPtr data);
    [DllImport("user32.dll")] private static extern bool EnumChildWindows(IntPtr hwnd, EnumWindow callback, IntPtr data);
    [DllImport("user32.dll", CharSet = CharSet.Auto)] private static extern int GetClassName(IntPtr hwnd, StringBuilder name, int max);

    private sealed class TestBrowser : Browser
    {
        internal TestBrowser() : base(PageUrl, true)
        {
            // Use the real controls without loading profiles or saving history.
            wvWebView1.CoreWebView2InitializationCompleted -= Handler<CoreWebView2InitializationCompletedEventArgs>("wvWebView1_CoreWebView2InitializationCompleted");
            wvWebView1.NavigationStarting -= Handler<CoreWebView2NavigationStartingEventArgs>("wvWebView1_NavigationStarting");
            wvWebView1.NavigationCompleted -= Handler<CoreWebView2NavigationCompletedEventArgs>("wvWebView1_NavigationCompleted");
            foreach (var field in typeof(Browser).GetFields(PrivateInstance | BindingFlags.Public))
            {
                var menu = field.GetValue(this) as ContextMenuStrip;
                if (menu == null) continue;
                RemoveOpening(menu, this, typeof(Browser), field.Name + "_Opening");
                menu.Items.Clear();
                menu.Items.Add("Test action");
                Menus.Add(menu);
            }
        }
        protected override void OnLoad(EventArgs e) { }
        internal readonly List<ContextMenuStrip> Menus = new List<ContextMenuStrip>();
        internal RichTextBox Address => (RichTextBox)Controls.Find("txtWebAddress", true).Single();
        internal IntPtr FocusHook => (IntPtr)typeof(Browser).GetField("_webViewFocusHook", PrivateInstance).GetValue(this);
        private EventHandler<T> Handler<T>(string name) => (EventHandler<T>)Delegate.CreateDelegate(typeof(EventHandler<T>), this, typeof(Browser).GetMethod(name, PrivateInstance));
    }

    private static void RemoveOpening(ContextMenuStrip menu, object target, Type type, string name)
    {
        var method = type.GetMethod(name, PrivateInstance);
        if (method != null) menu.Opening -= (CancelEventHandler)Delegate.CreateDelegate(typeof(CancelEventHandler), target, method);
    }

    private static void Check(bool value, string name)
    {
        if (!value) throw new Exception("FAIL: " + name);
        ++checks;
        Console.WriteLine("PASS: " + name);
    }

    private static async Task Until(Func<bool> predicate, string name)
    {
        var end = DateTime.UtcNow.AddSeconds(5);
        while (!predicate())
        {
            if (DateTime.UtcNow >= end) throw new Exception("Timeout: " + name);
            await Task.Delay(20);
        }
    }

    private static IntPtr RendererWindow(Control view)
    {
        IntPtr renderer = IntPtr.Zero;
        EnumChildWindows(view.Handle, (child, data) =>
        {
            var name = new StringBuilder(256);
            GetClassName(child, name, name.Capacity);
            if (name.ToString() == "Chrome_RenderWidgetHostHWND") renderer = child;
            return true;
        }, IntPtr.Zero);
        return renderer;
    }

    private static async Task Run(TestBrowser browser, string output)
    {
        var view = browser.wvWebView1;
        var address = browser.Address;
        var environment = await CoreWebView2Environment.CreateAsync(null, Path.Combine(output, "profile-" + Guid.NewGuid().ToString("N")));
        await view.EnsureCoreWebView2Async(environment);
        var core = view.CoreWebView2;
        core.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
        core.WebResourceRequested += (s, e) => e.Response = environment.CreateWebResourceResponse(
            new MemoryStream(Encoding.UTF8.GetBytes("<!doctype html><input id='input'><p>Local focus fixture</p>")),
            200, "OK", "Content-Type: text/html");
        var loaded = new TaskCompletionSource<bool>();
        core.NavigationCompleted += (s, e) => loaded.TrySetResult(e.IsSuccess);
        core.Navigate(PageUrl);
        Check(await Task.WhenAny(loaded.Task, Task.Delay(15000)) == loaded.Task && await loaded.Task, "local page loaded in a real WebView2 renderer");
        await Until(() => RendererWindow(view) != IntPtr.Zero, "native renderer window");
        IntPtr native = RendererWindow(view);
        Check(native != view.Handle, "test targets Chromium's child HWND, bypassing managed Focus()");

        for (int round = 0; round < 3; ++round)
        {
            SetFocus(native);
            await Until(() => browser.ActiveControl == view, "initial native page focus");
            address.Text = "example.test/path";
            address.Focus();
            await Until(() => address.SelectionLength == address.TextLength, "address select all");
            Check(address.Text == PageUrl, "address expands on entry " + round);
            address.Select(4, 2);
            SetFocus(native);
            await Until(() => browser.ActiveControl == view, "native focus synchronizes ActiveControl");
            Check(address.Text == "example.test/path", "native focus applies address Leave " + round);
            // Click back into the edit HWND, bypassing ContainerControl.Select.
            SetFocus(address.Handle);
            await Until(() => address.SelectionLength == address.TextLength, "select all after renderer focus");
            Check(address.Text == PageUrl, "returning from WebView2 selects the full URL " + round);
        }

        address.Text = "unsaved search query";
        SetFocus(native);
        await Until(() => browser.ActiveControl == view, "leave edited address");
        Check(address.Text == "unsaved search query", "unsaved address edits survive page focus");
        SetFocus(address.Handle);
        await Until(() => address.SelectionLength == address.TextLength, "select edited address");
        address.Select(0, 0);
        SetFocus(native);
        SetFocus(address.Handle);
        await Task.Delay(100);
        Check(address.Focused, "stale native focus callback cannot steal a newer address focus");

        using (var tabMenu = new TabContextMenu())
        using (var titleMenu = new DefaultContextMenu())
        using (var detached = new FocusAwareContextMenuStrip())
        {
            RemoveOpening(tabMenu, tabMenu, typeof(TabContextMenu), "DefaultContextMenu_Opening");
            RemoveOpening(titleMenu, titleMenu, typeof(DefaultContextMenu), "DefaultContextMenu_Opening");
            tabMenu.Items.Clear(); tabMenu.Items.Add("Tab action");
            titleMenu.Items.Clear(); titleMenu.Items.Add("Window action");
            detached.Items.Add("Detached action");
            var menus = browser.Menus.Concat(new ContextMenuStrip[] { tabMenu, titleMenu, detached }).ToArray();
            Check(browser.Menus.Count == 8, "all eight browser component menus included");
            foreach (var menu in menus)
            {
                Check(menu is FocusAwareContextMenuStrip, "focus-aware menu: " + menu.Name + menu.GetType().Name);
                SetFocus(native);
                await Until(() => browser.ActiveControl == view, "page focus before menu");
                menu.Show(address, new Point(0, address.Height));
                await Task.Delay(100);
                Check(menu.Visible && menu.ContainsFocus && !view.ContainsFocus, "menu remains open for interaction: " + menu.Name);
                SetFocus(native);
                await Until(() => !menu.Visible, "menu dismissal: " + menu.Name);
                Check(view.ContainsFocus, "menu dismissal preserves page focus: " + menu.Name);
            }

            var parent = new ToolStripMenuItem("Submenu");
            parent.DropDownItems.Add("Nested action");
            detached.Items.Add(parent);
            detached.Show(address, new Point(0, address.Height));
            parent.ShowDropDown();
            await Task.Delay(100);
            Check(detached.Visible && parent.DropDown.Visible, "nested menu stays open before page focus");
            SetFocus(native);
            await Until(() => !detached.Visible && !parent.DropDown.Visible, "nested menu dismissal");
            Check(view.ContainsFocus, "nested menu dismissal retains renderer focus");

            SetFocus(address.Handle);
            await Task.Delay(100);
            address.Select(2, 3);
            var editMenu = address.ContextMenuStrip;
            editMenu.Show(address, new Point(0, address.Height));
            await Task.Delay(100);
            editMenu.Close(ToolStripDropDownCloseReason.Keyboard);
            SetFocus(address.Handle);
            await Task.Delay(100);
            Check(address.SelectionStart == 2 && address.SelectionLength == 3, "address context menu preserves partial selection");
        }

        await core.ExecuteScriptAsync("document.getElementById('input').focus()");
        SetFocus(native);
        await Task.Delay(100);
        Check(await core.ExecuteScriptAsync("document.activeElement.id") == "\"input\"", "focus synchronization preserves the page's focused input");
        browser.Hide();
        Check(browser.FocusHook == IntPtr.Zero, "hidden tabs release the native focus hook");
        browser.Show();
        Check(browser.FocusHook != IntPtr.Zero, "shown tabs restore the native focus hook");
        browser.Dispose();
        Check(browser.FocusHook == IntPtr.Zero, "disposed tabs release the native focus hook");
    }

    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        string output = AppDomain.CurrentDomain.BaseDirectory;
        string settings = Path.Combine(output, "settings-" + Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(settings, "[{\"ProfileId\":\"00000000-0000-0000-0000-000000000000\",\"Name\":\"displayFullURLs\",\"Value\":\"false\"}]");
        typeof(SettingsService).GetField("_jsonPath", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, settings);
        using (var host = new Form { ClientSize = new Size(1000, 700), Location = new Point(-2400, -2400), StartPosition = FormStartPosition.Manual, ShowInTaskbar = false })
        using (var browser = new TestBrowser { TopLevel = false, Dock = DockStyle.Fill })
        {
            host.Controls.Add(browser);
            browser.Show();
            host.Shown += async (s, e) =>
            {
                try { await Run(browser, output); Console.WriteLine("PASS: " + checks + " focus regression checks."); }
                catch (Exception error) { Console.Error.WriteLine(error); Environment.ExitCode = 1; }
                finally { host.Close(); }
            };
            Application.Run(host);
        }
    }
}
