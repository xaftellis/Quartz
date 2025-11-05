using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Quartz.Libs
{
    public static class QueryAnalyzer
    {
        // Common TLDs — extended for realism
        private static readonly string[] CommonTlds = new[]
        {
            "com","org","net","edu","gov","io","co","info","biz","me",
            "au","uk","us","ca","de","fr","jp","cn","ru","in","tv","xyz","dev","app"
        };

        // Domain (includes subdomains, supports unicode)
        private static readonly Regex DomainRegex = new Regex(
            @"^(?:[a-zA-Z0-9-]+\.)+[a-zA-Z]{2,63}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        // IPv4 pattern (strict 0–255 validation done later)
        private static readonly Regex IpRegex = new Regex(
            @"^(?:\d{1,3}\.){3}\d{1,3}(?::\d{1,5})?$",
            RegexOptions.Compiled
        );

        public static bool IsProbablyUrl(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;
            input = input.Trim();

            // 1. Try parsing as an absolute URL (with scheme)
            Uri uri;
            if (Uri.TryCreate(input, UriKind.Absolute, out uri))
            {
                string scheme = uri.Scheme.ToLowerInvariant();
                if (scheme == "http" || scheme == "https" || scheme == "ftp" || scheme == "file" || scheme == "mailto")
                    return true;
            }

            // 2. Handle missing scheme: prepend http:// and retry
            if (!input.Contains("://") && Uri.TryCreate("http://" + input, UriKind.Absolute, out uri))
            {
                string host = uri.Host;

                // Check if host is a valid IP or domain
                if (IsValidIp(host) || IsValidDomain(host))
                    return true;
            }

            return false;
        }

        private static bool IsValidDomain(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
                return false;

            // Strip any trailing dot
            domain = domain.TrimEnd('.');

            if (!DomainRegex.IsMatch(domain))
                return false;

            string tld = domain.Split('.').Last().ToLowerInvariant();
            return CommonTlds.Contains(tld);
        }

        private static bool IsValidIp(string ip)
        {
            if (!IpRegex.IsMatch(ip))
                return false;

            string[] parts = ip.Split(':');
            string[] octets = parts[0].Split('.');

            // Each octet must be 0–255
            foreach (string o in octets)
            {
                int n;
                if (!int.TryParse(o, out n) || n < 0 || n > 255)
                    return false;
            }

            // Validate optional port
            if (parts.Length == 2)
            {
                int port;
                if (!int.TryParse(parts[1], out port) || port < 1 || port > 65535)
                    return false;
            }

            return true;
        }
    }
}
