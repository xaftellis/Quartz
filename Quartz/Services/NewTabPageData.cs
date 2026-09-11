using Quartz.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quartz.Services
{
    public static class NewTabPageData
    {
        public const string PageUrl = "https://quartz.com/newtab/index.html";

        public static bool IsPage(string address)
        {
            Uri uri;
            return Uri.TryCreate(address, UriKind.Absolute, out uri) &&
                uri.Scheme == Uri.UriSchemeHttps && uri.Host == "quartz.com" &&
                uri.IsDefaultPort && uri.UserInfo.Length == 0 &&
                uri.AbsolutePath == "/newtab/index.html";
        }

        public static bool IsWebUrl(string address)
        {
            Uri uri;
            return Uri.TryCreate(address, UriKind.Absolute, out uri) &&
                (uri.Scheme == "https" || uri.Scheme == "http") && uri.UserInfo.Length == 0;
        }

        public static List<HistoryModel> RecentHistory(IEnumerable<HistoryModel> history,
            Guid profile, string query, bool isPrivate)
        {
            if (isPrivate) return new List<HistoryModel>();
            query = (query ?? "").Trim();
            return (history ?? Enumerable.Empty<HistoryModel>())
                .Where(h => h != null && h.ProfileId == profile && IsWebUrl(h.WebAddress) &&
                    new Uri(h.WebAddress).Host != "quartz.com" &&
                    (query.Length == 0 || h.WebAddress.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (h.Title ?? "").IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0))
                .OrderByDescending(h => h.When)
                .GroupBy(h => new Uri(h.WebAddress).AbsoluteUri, StringComparer.Ordinal)
                .Select(g => g.First()).Take(query.Length == 0 ? 8 : 3).ToList();
        }

        public static string SearchUrl(string query, string engine, bool forceSearch = false)
        {
            query = (query ?? "").Trim();
            if (query.Length == 0 || query.Length > 2048) return null;
            if (!forceSearch)
            {
                if (IsWebUrl(query)) return new Uri(query).AbsoluteUri;
                Uri candidate;
                if (!query.Any(char.IsWhiteSpace) && !query.Contains("://") &&
                    Uri.TryCreate("https://" + query, UriKind.Absolute, out candidate) &&
                    candidate.UserInfo.Length == 0 &&
                    (candidate.Host.Contains(".") || candidate.Host == "localhost" ||
                     candidate.HostNameType == UriHostNameType.IPv6)) return candidate.AbsoluteUri;
            }
            string prefix;
            switch (engine)
            {
                case "bing": prefix = "https://www.bing.com/search?q="; break;
                case "yahoo": prefix = "https://search.yahoo.com/search?p="; break;
                case "duckduckgo": prefix = "https://duckduckgo.com/?q="; break;
                case "wikipedia": prefix = "https://wikipedia.org/w/index.php?search="; break;
                case "netflix": prefix = "https://www.netflix.com/search?q="; break;
                case "youtube": prefix = "https://www.youtube.com/results?search_query="; break;
                case "googlemaps": prefix = "https://www.google.com/maps/search/"; break;
                case "ebay": prefix = "https://www.ebay.com/sch/?_nkw="; break;
                case "amazon": case "amazom": prefix = "https://www.amazon.com/s?k="; break;
                case "ecosia": prefix = "https://www.ecosia.org/search?q="; break;
                default: prefix = "https://www.google.com/search?q="; break;
            }
            return prefix + Uri.EscapeDataString(query);
        }
    }
}
