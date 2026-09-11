using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using Quartz.Libs;
using Quartz.Models;
using Quartz.Omnibox;
using Quartz.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

internal static class NewTabTests
{
    private static int checks;
    private static WebView2 view;
    private static string theme = "light", engine = "google";
    private static string destination;
    private static string output;
    private static NewTabPageController controller;
    private static readonly Guid profile = Guid.NewGuid();
    private static readonly Guid otherProfile = Guid.NewGuid();
    private static readonly List<HistoryModel> history = new List<HistoryModel>
    {
        Entry(profile, "https://example.org/older", "Older Quartz visit", -5),
        Entry(profile, "https://example.org/recent", "Latest Quartz visit", -1),
        Entry(profile, "https://example.org/recent", "Old duplicate", -9),
        Entry(otherProfile, "https://example.net/private", "OTHER PROFILE", 1),
        Entry(profile, "https://quartz.com/newtab/index.html", "Internal", 2),
        Entry(profile, "javascript:alert(1)", "Unsafe", 3),
        Entry(profile, "https://example.org/null", null, -10)
    };

    private static HistoryModel Entry(Guid id, string url, string title, int minutes)
    {
        return new HistoryModel { ProfileId = id, Id = Guid.NewGuid(), WebAddress = url, Title = title, When = DateTime.Now.AddMinutes(minutes) };
    }
    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception("FAIL: " + name);
        checks++; Console.WriteLine("PASS: " + name);
    }
    private static Task<string> JS(string script) { return view.CoreWebView2.ExecuteScriptAsync(script); }
    private static async Task AssertJS(string expression, string name)
    {
        Check(await JS(expression) == "true", name);
    }
    private static async Task Until(string expression, int milliseconds = 5000)
    {
        var end = DateTime.UtcNow.AddMilliseconds(milliseconds);
        do { if (await JS(expression) == "true") return; await Task.Delay(50); } while (DateTime.UtcNow < end);
        throw new Exception("Timed out: " + expression);
    }
    private static async Task Navigate(string url)
    {
        var ready = new TaskCompletionSource<bool>();
        EventHandler<CoreWebView2NavigationCompletedEventArgs> handler = (s, e) => ready.TrySetResult(e.IsSuccess);
        view.CoreWebView2.NavigationCompleted += handler;
        view.CoreWebView2.Navigate(url);
        await Task.WhenAny(ready.Task, Task.Delay(10000));
        view.CoreWebView2.NavigationCompleted -= handler;
        Check(ready.Task.IsCompleted && ready.Task.Result, "local new-tab document loads");
        await Until("!!document.getElementById('add-shortcut') || document.querySelectorAll('#shortcuts .tile').length > 0");
    }
    private static async Task Snapshot(string name)
    {
        await Task.Delay(100);
        using (var stream = File.Create(Path.Combine(output, name + ".png")))
            await view.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, stream);
    }

    [STAThread]
    private static void Main(string[] args)
    {
        output = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "newtab-render");
        Directory.CreateDirectory(output);
        var recent = NewTabPageData.RecentHistory(history, profile, "", false);
        Check(recent.Count == 3 && recent[0].Title == "Latest Quartz visit", "recent history deduplicates and sorts newest first");
        Check(recent.All(h => h.ProfileId == profile && !h.WebAddress.Contains("quartz.com")), "history is profile-scoped and excludes internal/unsafe URLs");
        Check(NewTabPageData.RecentHistory(history, profile, "visit", true).Count == 0, "private pages never expose saved history");
        Check(NewTabPageData.RecentHistory(history, profile, "older", false).Single().Title == "Older Quartz visit", "typed history matches title and URL");
        Check(NewTabPageData.IsPage(NewTabPageData.PageUrl + "?theme=dark"), "trusted local page recognized");
        foreach (var address in new[] { "http://quartz.com/newtab/index.html", "https://quartz.com:444/newtab/index.html", "https://quartz.com.evil/newtab/index.html", "https://quartz.com/newtab/other.html", "https://user@quartz.com/newtab/index.html" })
            Check(!NewTabPageData.IsPage(address), "bridge rejects " + address);
        Check(NewTabPageData.SearchUrl("example.com/a?x=1", "google") == "https://example.com/a?x=1", "bare URL navigation");
        Check(NewTabPageData.SearchUrl("localhost:8080", "google") == "https://localhost:8080/", "localhost navigation");
        Check(NewTabPageData.SearchUrl("a & b", "google") == "https://www.google.com/search?q=a%20%26%20b", "Google query encoding");
        Check(NewTabPageData.SearchUrl("example.com", "google", true).Contains("/search?q=example.com"), "remote suggestions always search");
        Check(NewTabPageData.SearchUrl("javascript:alert(1)", "bing").StartsWith("https://www.bing.com/search?"), "executable schemes become search text");
        Check(NewTabPageData.SearchUrl(" ", "google") == null, "empty submission is ignored");
        Application.EnableVisualStyles();
        var form = new Form { ClientSize = new Size(1200, 800), StartPosition = FormStartPosition.Manual, Location = new Point(-2000, -2000), ShowInTaskbar = false };
        view = new WebView2 { Dock = DockStyle.Fill };
        form.Controls.Add(view);
        form.Shown += async (s, e) =>
        {
            try
            {
                string data = Path.Combine(output, "profile-" + Guid.NewGuid().ToString("N"));
                var environment = await CoreWebView2Environment.CreateAsync(null, data);
                await view.EnsureCoreWebView2Async(environment);
                var core = view.CoreWebView2;
                core.SetVirtualHostNameToFolderMapping("quartz.com", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "quartz.com"), CoreWebView2HostResourceAccessKind.DenyCors);
                controller = new NewTabPageController(core, profile, false, key => key == "Theme" ? theme : engine, () => history);
                core.NavigationStarting += (sender, nav) => { if (NewTabPageData.IsPage(core.Source) && !NewTabPageData.IsPage(nav.Uri)) { destination = nav.Uri; nav.Cancel = true; } };
                await Navigate(NewTabPageData.PageUrl);
                await AssertJS("document.getElementById('logo').naturalWidth > 0", "supplied Quartz logo loads from build output");
                await AssertJS("Math.round(document.getElementById('search-form').getBoundingClientRect().height) === 48 && Math.round(document.getElementById('searchbox').getBoundingClientRect().width) === 561", "Chromium desktop searchbox dimensions");
                await Snapshot("light");
                await JS("document.getElementById('search-input').focus()");
                await Until("document.querySelectorAll('.suggestion').length === 3");
                await AssertJS("document.querySelector('.suggestion').textContent.includes('Latest Quartz visit') && !document.getElementById('suggestions').textContent.includes('OTHER PROFILE')", "empty focus displays actual recent Quartz history only");
                await Snapshot("history");
                await JS("document.getElementById('search-input').dispatchEvent(new KeyboardEvent('keydown',{key:'ArrowDown',bubbles:true})); document.getElementById('search-form').requestSubmit()");
                await Task.Delay(100);
                Check(destination == "https://example.org/recent", "keyboard selection navigates to the selected history URL");
                await JS("document.getElementById('search-input').value='quartz browser'; document.getElementById('search-input').dispatchEvent(new Event('input',{bubbles:true}))");
                await Until("document.querySelector('.suggestion')?.textContent === 'quartz browser'");
                await JS("document.getElementById('search-input').dispatchEvent(new KeyboardEvent('keydown',{key:'Escape',bubbles:true}))");
                await Task.Delay(300);
                await AssertJS("document.getElementById('suggestions').hidden", "Escape prevents in-flight responses reopening suggestions");

                await AddShortcut("Example", "example.org");
                await AddShortcut("Chromium", "https://www.chromium.org/");
                await AssertJS("document.querySelectorAll('#shortcuts a').length === 2", "add shortcuts with normalized URL");
                await JS("document.getElementById('add-shortcut').click(); document.getElementById('shortcut-url').value='https://example.org/'; document.getElementById('shortcut-url').dispatchEvent(new Event('input'))");
                await AssertJS("document.getElementById('save-shortcut').disabled && document.getElementById('url-error').textContent.includes('already')", "duplicate shortcut rejected");
                await JS("document.getElementById('shortcut-url').value='javascript:alert(1)'; document.getElementById('shortcut-url').dispatchEvent(new Event('input'))");
                await AssertJS("document.getElementById('save-shortcut').disabled", "shortcut executable URL rejected");
                await JS("document.getElementById('cancel-shortcut').click(); document.querySelector('.more').click(); document.getElementById('edit-shortcut').click()");
                await Snapshot("edit-shortcut");
                await JS("document.getElementById('shortcut-name').value='<b>Edited</b>'; document.getElementById('shortcut-form').requestSubmit()");
                await AssertJS("document.querySelector('.tile-title').textContent === '<b>Edited</b>' && !document.querySelector('#shortcuts b')", "edit preserves literal text without HTML injection");
                await JS("document.querySelector('#shortcuts a').dispatchEvent(new KeyboardEvent('keydown',{key:'ArrowRight',altKey:true,bubbles:true}))");
                await AssertJS("document.querySelector('#shortcuts a').href === 'https://www.chromium.org/'", "keyboard shortcut reorder");
                await JS("document.querySelector('.more').click(); document.getElementById('remove-shortcut').click()");
                await AssertJS("document.querySelectorAll('#shortcuts a').length === 1", "remove shortcut");
                await JS("document.getElementById('undo').click()");
                await AssertJS("document.querySelectorAll('#shortcuts a').length === 2", "undo restores removed shortcut and order");
                await Navigate(NewTabPageData.PageUrl);
                await AssertJS("document.querySelectorAll('#shortcuts a').length === 2 && document.querySelector('#shortcuts a').href === 'https://www.chromium.org/'", "shortcuts and order persist after page reload");

                foreach (string palette in new[] { "dark", "black", "aqua", "xmas" })
                {
                    theme = palette; controller.UpdateTheme();
                    await Until("document.documentElement.dataset.theme === '" + palette + "'");
                    await Snapshot(palette);
                }
                theme = "light"; controller.UpdateTheme();
                form.ClientSize = new Size(360, 640);
                await Task.Delay(100);
                await AssertJS("document.documentElement.scrollWidth <= innerWidth", "narrow window has no horizontal overflow");
                await Snapshot("narrow");
                form.ClientSize = new Size(1200, 800);

                if (args.Contains("--online"))
                {
                    var live = await SearchSuggestions.GetForEngineAsync("weather in perth", "google", CancellationToken.None);
                    Check(live.Count > 0, "live Google autocomplete endpoint returns real suggestions");
                    await JS("document.getElementById('search-input').value='weather in perth'; document.getElementById('search-input').focus(); document.getElementById('search-input').dispatchEvent(new Event('input',{bubbles:true}))");
                    await Until("document.querySelectorAll('.suggestion').length > 1", 8000);
                    await Snapshot("google-suggestions");
                    await JS("document.querySelectorAll('.suggestion')[1].click()");
                    await Task.Delay(150);
                    Check(destination.StartsWith("https://www.google.com/search?q="), "live Google suggestion selection submits to Google");
                }
                controller.Dispose();
                controller = new NewTabPageController(core, profile, true, key => key == "Theme" ? theme : engine, () => { throw new Exception("Private page read history"); });
                await JS("document.getElementById('search-input').value=''; document.getElementById('search-input').focus(); document.getElementById('search-input').dispatchEvent(new Event('input',{bubbles:true}))");
                await Task.Delay(150);
                await AssertJS("document.getElementById('suggestions').hidden", "private bridge does not even read history");
                Console.WriteLine("All " + checks + " new-tab checks passed. Renders: " + output);
            }
            catch (Exception ex) { Console.Error.WriteLine(ex); Environment.ExitCode = 1; }
            finally { controller?.Dispose(); form.Close(); }
        };
        Application.Run(form);
    }
    private static async Task AddShortcut(string name, string url)
    {
        await JS("document.getElementById('add-shortcut').click(); document.getElementById('shortcut-name').value=" + JsonConvert.SerializeObject(name) + "; document.getElementById('shortcut-url').value=" + JsonConvert.SerializeObject(url) + "; document.getElementById('shortcut-url').dispatchEvent(new Event('input')); document.getElementById('shortcut-form').requestSubmit()");
    }
}
