using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Quartz.Services
{
    internal static class InternalPageTheme
    {
        private static readonly string[] Themes = { "light", "dark" };
        private static readonly string[] Pages = { "Google.html", "Bing.html", "Yahoo.html", "DuckDuckGo.html",
            "Ecosia.html", "Netflix.html", "YouTube.html", "Google Maps.html", "Custom.html", "Error.html", "Safety.html" };
        private static readonly Lazy<string> Script = new Lazy<string>(() =>
        {
            using (var reader = new StreamReader(typeof(InternalPageTheme).Assembly.GetManifestResourceStream(
                "Quartz.Services.InternalPageTheme.js"))) return reader.ReadToEnd();
        });

        internal static string CreateScript(Uri source, string theme)
        {
            if (source == null || !source.IsAbsoluteUri || source.Scheme != Uri.UriSchemeHttps ||
                !source.IsDefaultPort || source.Host != "quartz.com" || !Themes.Contains(theme)) return null;
            string[] parts = source.AbsolutePath.Split('/');
            if (parts.Length != 3 || !Themes.Contains(parts[1])) return null;
            string page = Pages.FirstOrDefault(p => p.Equals(Uri.UnescapeDataString(parts[2]), StringComparison.OrdinalIgnoreCase));
            if (page == null) return null;
            string path = Path.Combine(Application.StartupPath, "assets", "quartz.com", theme, page);
            if (!File.Exists(path)) return null;
            return "(" + Script.Value + ")(" + JsonConvert.SerializeObject(new
            {
                source = source.AbsoluteUri,
                theme,
                url = "https://quartz.com/" + theme + "/" + Uri.EscapeDataString(page),
                html = File.ReadAllText(path)
            }) + ");";
        }
    }
}
