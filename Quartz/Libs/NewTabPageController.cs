using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz.Models;
using Quartz.Omnibox;
using Quartz.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Quartz.Libs
{
    public sealed class NewTabPageController : IDisposable
    {
        private readonly CoreWebView2 core;
        private readonly Guid profile;
        private readonly bool isPrivate;
        private readonly Func<string, string> setting;
        private readonly Func<IEnumerable<HistoryModel>> history;
        private CancellationTokenSource suggestions;
        private int documentVersion;
        private bool disposed;

        public NewTabPageController(CoreWebView2 core, Guid profile, bool isPrivate,
            Func<string, string> setting, Func<IEnumerable<HistoryModel>> history)
        {
            this.core = core;
            this.profile = profile;
            this.isPrivate = isPrivate;
            this.setting = setting;
            this.history = history;
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

        private async void OnMessage(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            // Expose profile data only to this exact local top-level document.
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
                    case "suggest":
                        CancelSuggestions();
                        suggestions = new CancellationTokenSource();
                        var token = suggestions.Token;
                        string query = ((string)message["query"] ?? "").Trim();
                        if (query.Length > 2048) return;
                        var recent = NewTabPageData.RecentHistory(isPrivate ? null : history(), profile, query, isPrivate)
                            .Select(h => new { kind = "history", text = string.IsNullOrWhiteSpace(h.Title) ? new Uri(h.WebAddress).Host : h.Title,
                                url = h.WebAddress }).ToList();
                        Post(new { type = "suggestions", id, query, history = recent, searches = new string[0] });
                        if (query.Length == 0) break;
                        var remote = await SearchSuggestions.GetForEngineAsync(query, setting("SearchEngine"), token);
                        if (!disposed && version == documentVersion && !token.IsCancellationRequested)
                            Post(new { type = "suggestions", id, query, history = recent, searches = remote.Take(6).ToArray() });
                        break;
                    case "navigate":
                        string destination = NewTabPageData.SearchUrl((string)message["text"], setting("SearchEngine"),
                            (bool?)message["forceSearch"] == true);
                        if (destination != null) core.Navigate(destination);
                        break;
                    case "icons":
                        var urls = message["urls"] as JArray;
                        if (urls == null || urls.Count > 10) return;
                        var icons = new Dictionary<string, string>();
                        var cache = new FaviconService();
                        foreach (string url in urls.Values<string>().Where(NewTabPageData.IsWebUrl))
                        {
                            var icon = cache.Get(url);
                            if (icon == null) continue;
                            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                "Xaftellis", "Quartz", "UserData", "cache", icon.Id + ".ico");
                            if (File.Exists(path) && new FileInfo(path).Length < 262144)
                                icons[url] = "data:image/x-icon;base64," + Convert.ToBase64String(File.ReadAllBytes(path));
                        }
                        Post(new { type = "icons", icons });
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
