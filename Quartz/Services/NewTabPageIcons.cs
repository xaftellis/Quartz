using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Quartz.Services
{
    // Ports Chromium's favicon2 host fallback and GenerateMonogramFavicon data.
    // Rendering stays in WebView2 so device scale and the Windows font are respected.
    public static class NewTabPageIcons
    {
        private static readonly Lazy<HashSet<string>> suffixes = new Lazy<HashSet<string>>(() =>
        {
            var rules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var idn = new IdnMapping();
            foreach (var raw in File.ReadLines(Path.Combine(Application.StartupPath, "assets", "quartz.com", "newtab", "effective_tld_names.dat")))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("//")) continue;
                var prefix = line.StartsWith("!") ? "!" : line.StartsWith("*.") ? "*." : "";
                rules.Add(prefix + idn.GetAscii(line.Substring(prefix.Length)));
            }
            return rules;
        });

        public static Dictionary<string, string> Cached(IEnumerable<string> urls)
        {
            var result = new Dictionary<string, string>();
            var cache = new FaviconService().All();
            foreach (var url in urls)
            {
                var uri = new Uri(url);
                // Chromium asks favicon2 for the page and falls back to its host.
                var candidates = cache.Where(f => NewTabPageData.IsWebUrl(f.WebAddress) &&
                    new Uri(f.WebAddress).IdnHost == uri.IdnHost)
                    .OrderByDescending(f => new Uri(f.WebAddress).AbsoluteUri == uri.AbsoluteUri);
                foreach (var icon in candidates)
                {
                    var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Xaftellis", "Quartz", "UserData", "cache", icon.Id + ".ico");
                    if (!File.Exists(path) || new FileInfo(path).Length >= 262144) continue;
                    result[url] = "data:image/x-icon;base64," + Convert.ToBase64String(File.ReadAllBytes(path));
                    break;
                }
            }
            return result;
        }

        public static object Monogram(string url)
        {
            var uri = new Uri(url);
            string host = uri.IdnHost.TrimEnd('.');
            string label;
            if (uri.HostNameType == UriHostNameType.IPv4 || uri.HostNameType == UriHostNameType.IPv6) label = "IP";
            else
            {
                var parts = host.Split('.');
                int suffixLength = 1;
                for (int i = 0; i < parts.Length; i++)
                {
                    string tail = string.Join(".", parts.Skip(i));
                    if (suffixes.Value.Contains("!" + tail)) { suffixLength = parts.Length - i - 1; break; }
                    if (suffixes.Value.Contains(tail)) suffixLength = Math.Max(suffixLength, parts.Length - i);
                    if (i > 0 && suffixes.Value.Contains("*." + tail)) suffixLength = Math.Max(suffixLength, parts.Length - i + 1);
                }
                string domain = parts.Length > suffixLength ? string.Join(".", parts.Skip(parts.Length - suffixLength - 1)) : host;
                string unicode = new IdnMapping().GetUnicode(domain);
                label = StringInfo.GetNextTextElement(unicode).ToUpperInvariant();
            }
            string originHost = uri.HostNameType == UriHostNameType.IPv6 ? "[" + host.Trim('[', ']') + "]" : host;
            string origin = uri.Scheme + "://" + originHost + (uri.IsDefaultPort ? "" : ":" + uri.Port) + "/";
            byte[] hash;
            using (var sha = SHA1.Create()) hash = sha.ComputeHash(Encoding.UTF8.GetBytes(origin));
            var rgb = new[] { (int)hash[0], (int)hash[1], (int)hash[2] };
            // BlendForMinContrast against white, with GoogleGrey900 as its target.
            if (Contrast(rgb) < 4.5)
            {
                var target = new[] { 32, 33, 36 };
                int low = 0, high = 256;
                var best = target;
                while (low < high)
                {
                    int alpha = (low + high) / 2;
                    var color = rgb.Select((channel, i) => (int)Math.Round((target[i] * alpha + channel * (255 - alpha)) / 255.0, MidpointRounding.AwayFromZero)).ToArray();
                    if (Contrast(color) >= 4.5) { best = color; high = alpha; }
                    else low = alpha + 1;
                }
                rgb = best;
            }
            return new { text = label, color = "#" + string.Concat(rgb.Select(c => c.ToString("x2"))) };
        }

        private static double Contrast(int[] rgb)
        {
            var linear = rgb.Select(c => c / 255.0).Select(c => c <= .04045 ? c / 12.92 : Math.Pow((c + .055) / 1.055, 2.4)).ToArray();
            return 1.05 / (.2126 * linear[0] + .7152 * linear[1] + .0722 * linear[2] + .05);
        }
    }
}
