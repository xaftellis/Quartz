using Svg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Management;
using Win32Interop.Structs;

namespace Quartz.Libs
{
    public static class OmniBoxHelper
    {
        // Common TLDs (partial, extend as needed)
        private static readonly string[] CommonTlds = new[]
        {
        "com","org","net","edu","gov","io","co","info","biz","me"
    };

        private static readonly Regex DomainRegex = new Regex(
            @"^([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        private static readonly Regex IpRegex = new Regex(
            @"^(\d{1,3}\.){3}\d{1,3}(:\d{1,5})?$",
            RegexOptions.Compiled
        );

        public static bool IsProbablyUrl(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.Trim();

            // 1. Absolute URL with scheme
            if (Uri.TryCreate(input, UriKind.Absolute, out Uri uriResult))
            {
                if (uriResult.Scheme == Uri.UriSchemeHttp ||
                    uriResult.Scheme == Uri.UriSchemeHttps ||
                    uriResult.Scheme == Uri.UriSchemeFtp ||
                    uriResult.Scheme == Uri.UriSchemeFile)
                {
                    return true;
                }
            }

            // 2. Bare IPv4 address with optional port
            if (IpRegex.IsMatch(input))
            {
                string[] parts = input.Split(':');
                string ipPart = parts[0];
                string[] octets = ipPart.Split('.');
                if (octets.Any(o => !int.TryParse(o, out int n) || n < 0 || n > 255))
                    return false;

                if (parts.Length == 2 && (!int.TryParse(parts[1], out int port) || port < 1 || port > 65535))
                    return false;

                return true;
            }

            // 3. Bare domain name
            string domain = input.Split('/')[0]; // remove path if exists
            if (DomainRegex.IsMatch(domain))
            {
                string tld = domain.Split('.').Last().ToLower();
                if (CommonTlds.Contains(tld))
                    return true;
            }

            return false; // fallback: likely search term
        }
    }
}
