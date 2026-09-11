using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Quartz.Omnibox
{
    public static class SearchSuggestions
    {
        private static readonly HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(4) };
        public enum SearchEngine { Google }

        public static Task<List<string>> GetAsync(string query, SearchEngine engine = SearchEngine.Google)
        {
            return GetForEngineAsync(query, SettingsService.Get("SearchEngine"), CancellationToken.None);
        }

        public static async Task<List<string>> GetForEngineAsync(string query, string engine, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length > 2048) return new List<string>();
            string encoded = Uri.EscapeDataString(query);
            string url;
            switch (engine)
            {
                case "bing": url = "https://api.bing.com/osjson.aspx?query=" + encoded; break;
                case "duckduckgo": url = "https://duckduckgo.com/ac/?type=list&q=" + encoded; break;
                case "youtube": url = "https://suggestqueries.google.com/complete/search?client=firefox&ds=yt&q=" + encoded; break;
                case null: case "": case "google":
                    url = "https://suggestqueries.google.com/complete/search?client=firefox&q=" + encoded; break;
                default: return new List<string>();
            }
            try
            {
                using (var response = await httpClient.GetAsync(url, token).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    var data = JArray.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    var list = data.Count > 1 ? data[1] as JArray : null;
                    return list == null ? new List<string>() : list.Where(t => t.Type == JTokenType.String)
                        .Values<string>().Where(s => !string.IsNullOrWhiteSpace(s) && s.Length <= 2048)
                        .Distinct(StringComparer.OrdinalIgnoreCase).Take(8).ToList();
                }
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is OperationCanceledException || ex is JsonException)
            {
                return new List<string>();
            }
        }
    }
}
