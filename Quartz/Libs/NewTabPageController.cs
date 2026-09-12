using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz.Models;
using Quartz.Omnibox;
using Quartz.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Quartz.Libs
{
    public sealed class NewTabPageController : IDisposable
    {
        private readonly CoreWebView2 core;
        private readonly Guid profile;
        private readonly bool isPrivate;
        private readonly Func<string, string> setting;
        private readonly Func<IEnumerable<HistoryModel>> history;
        private readonly Action<string> removeHistory;
        private readonly Func<string, string, CancellationToken, Task<List<string>>> fetchSuggestions;
        private readonly Stopwatch requestClock = Stopwatch.StartNew();
        private long lastSuggestRequest = -100;
        private List<HistoryModel> historySnapshot;
        private DateTime historyStamp;
        private readonly HashSet<string> servedHistory = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> loadedIcons = new HashSet<string>(StringComparer.Ordinal);
        private CancellationTokenSource suggestions;
        private int documentVersion;
        private bool disposed;
        private static readonly string HistoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Xaftellis", "Quartz", "UserData", "jsons", "history.json");

        public NewTabPageController(CoreWebView2 core, Guid profile, bool isPrivate,
            Func<string, string> setting, Func<IEnumerable<HistoryModel>> history, Action<string> removeHistory = null,
            Func<string, string, CancellationToken, Task<List<string>>> fetchSuggestions = null)
        {
            this.core = core;
            this.profile = profile;
            this.isPrivate = isPrivate;
            this.setting = setting;
            this.history = history;
            this.removeHistory = removeHistory ?? (url => new HistoryService().DeleteProfileUrl(profile, url));
            this.fetchSuggestions = fetchSuggestions ?? SearchSuggestions.GetForEngineAsync;
            core.WebMessageReceived += OnMessage;
            core.NavigationStarting += OnNavigation;
        }

        private void CancelSuggestions()
        {
            if (suggestions == null) return;
            suggestions.Cancel();
            suggestions.Dispose();
            suggestions = null;
        }

        private void OnNavigation(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            documentVersion++;
            CancelSuggestions();
            historySnapshot = null;
            servedHistory.Clear();
            loadedIcons.Clear();
        }

        public void UpdateTheme()
        {
            if (disposed || !NewTabPageData.IsPage(core.Source)) return;
            Post(new { type = "state", theme = setting("Theme"), engine = setting("SearchEngine") });
        }

        private void Post(object message)
        {
            if (!disposed && NewTabPageData.IsPage(core.Source))
                core.PostWebMessageAsJson(JsonConvert.SerializeObject(message));
        }

        private IEnumerable<HistoryModel> Snapshot()
        {
            if (isPrivate) return Enumerable.Empty<HistoryModel>();
            var stamp = File.GetLastWriteTimeUtc(HistoryPath);
            if (historySnapshot == null || stamp != historyStamp)
            {
                historySnapshot = (history() ?? Enumerable.Empty<HistoryModel>()).ToList();
                historyStamp = stamp;
            }
            return historySnapshot;
        }

        private void SendIcons(IEnumerable<string> addresses, bool withMonograms)
        {
            var urls = addresses.Where(NewTabPageData.IsWebUrl).Distinct().Take(10).ToList();
            if (!withMonograms) urls = urls.Where(url => !loadedIcons.Contains(url)).ToList();
            if (urls.Count == 0) return;
            var icons = NewTabPageIcons.Cached(urls);
            var fallbacks = new Dictionary<string, object>();
            if (withMonograms)
                foreach (var url in urls.Where(url => !icons.ContainsKey(url))) fallbacks[url] = NewTabPageIcons.Monogram(url);
            foreach (var url in urls) loadedIcons.Add(url);
            Post(new { type = "icons", icons, fallbacks });
        }

        private async void OnMessage(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            if (disposed || !NewTabPageData.IsPage(e.Source) || !NewTabPageData.IsPage(core.Source)) return;
            int version = documentVersion;
            int id = 0;
            try
            {
                if (e.WebMessageAsJson.Length > 32768) return;
                var message = JObject.Parse(e.WebMessageAsJson);
                if ((string)message["channel"] != "quartz-newtab") return;
                id = (int?)message["id"] ?? 0;
                switch ((string)message["type"])
                {
                    case "state": UpdateTheme(); break;
                    case "stop-suggest": CancelSuggestions(); break;
                    case "suggest":
                        CancelSuggestions();
                        string query = ((string)message["query"] ?? "").Trim();
                        if (query.Length > 2048) return;
                        suggestions = new CancellationTokenSource();
                        var token = suggestions.Token;
                        var recent = NewTabPageData.RecentHistory(Snapshot(), profile, query, isPrivate)
                            .Select(h => new { text = string.IsNullOrWhiteSpace(h.Title) ? new Uri(h.WebAddress).Host : h.Title,
                                url = h.WebAddress }).ToList();
                        foreach (var item in recent) servedHistory.Add(item.url);
                        Post(new { type = "suggestions", id, query, history = recent, searches = new string[0], complete = query.Length == 0 });
                        // Favicon failures must never delay or suppress autocomplete.
                        try { SendIcons(recent.Select(h => h.url), false); }
                        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException || ex is ArgumentException) { }
                        if (query.Length == 0) break;
                        // Chromium's default polling strategy measures 100ms from the
                        // last request sent, not from each keystroke (SearchProvider).
                        int delay = (int)Math.Max(0, 100 - (requestClock.ElapsedMilliseconds - lastSuggestRequest));
                        if (delay > 0) await Task.Delay(delay, token);
                        token.ThrowIfCancellationRequested();
                        lastSuggestRequest = requestClock.ElapsedMilliseconds;
                        var remote = await fetchSuggestions(query, setting("SearchEngine"), token);
                        if (!disposed && version == documentVersion && !token.IsCancellationRequested)
                            Post(new { type = "suggestions", id, query, history = recent, searches = remote.Take(6).ToArray(), complete = true });
                        break;
                    case "delete-history":
                        string address = (string)message["url"];
                        bool removed = false;
                        if (!isPrivate && address != null && servedHistory.Contains(address) && NewTabPageData.IsWebUrl(address))
                        {
                            CancelSuggestions();
                            try
                            {
                                removeHistory(address);
                                historySnapshot = null;
                                servedHistory.Remove(address);
                                removed = true;
                            }
                            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException) { }
                        }
                        Post(new { type = "history-deleted", id, url = address, success = removed });
                        break;
                    case "navigate":
                        string destination = NewTabPageData.SearchUrl((string)message["text"], setting("SearchEngine"),
                            (bool?)message["forceSearch"] == true);
                        if (destination != null) core.Navigate(destination);
                        break;
                    case "icons":
                        var urls = message["urls"] as JArray;
                        if (urls == null || urls.Count > 10) return;
                        SendIcons(urls.Values<string>(), true);
                        break;
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) when (ex is JsonException || ex is IOException || ex is UnauthorizedAccessException ||
                ex is ArgumentException || ex is InvalidOperationException || ex is FormatException || ex is OverflowException)
            {
                if (!disposed && version == documentVersion) Post(new { type = "error", id });
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            CancelSuggestions();
            core.WebMessageReceived -= OnMessage;
            core.NavigationStarting -= OnNavigation;
        }
    }
}

