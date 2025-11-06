using AxWMPLib;
using Newtonsoft.Json;
using Quartz.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Omnibox
{
    public static class SearchSuggestions
    {
        private static HttpClient httpClient;

        public enum SearchEngine
        {
            Google
        }

        public static async Task<List<string>> GetAsync(string query, SearchEngine engine = SearchEngine.Google)
        {
            if(httpClient == null)
            {
                httpClient = new HttpClient();
            }

            try
            {
                string url = GetSearchSuggestionsAPI(query);
                string response = await httpClient.GetStringAsync(url);

                var data = JsonConvert.DeserializeObject<object[]>(response);
                var suggestions = JsonConvert.DeserializeObject<List<string>>(data[1].ToString());

                return suggestions;
            }
            catch
            {
                return new List<string>();
            }
        }


        private static string GetSearchSuggestionsAPI(string query)
        {
            string url;
            switch (SettingsService.Get("SearchEngine"))
            {
                case "bing":
                    url = $"https://api.bing.com/osjson.aspx?query={Uri.EscapeDataString(query)}";
                    break;

                case "duckduckgo":
                    url = $"https://duckduckgo.com/ac/?q={Uri.EscapeDataString(query)}&type=list";
                    break;

                case "youtube":
                    url = $"https://suggestqueries.google.com/complete/search?client=youtube&ds=yt&q=QUERY{Uri.EscapeDataString(query)}";
                    break;

                default:
                    url = $"https://suggestqueries.google.com/complete/search?client=firefox&q={Uri.EscapeDataString(query)}";
                    break;

            }

            return url;
        }
    }
}
