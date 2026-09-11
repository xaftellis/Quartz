// Adapted from Chromium ui/base/text/bytes_formatting.cc and TabResourceUsage.
// See TabHoverCards.md and Chromium-LICENSE.txt.
using System;
using System.Globalization;

namespace EasyTabs
{
    internal static class TabMemoryUsageFormatter
    {
        internal const long HighMemoryThreshold = 800L * 1024 * 1024;

        internal static string Footer(long? bytes)
        {
            if (!bytes.HasValue || bytes.Value <= 0) return "Memory usage: unavailable";
            return (bytes > HighMemoryThreshold ? "High memory usage: " : "Memory usage: ") + Format(bytes.Value);
        }

        internal static string Format(long bytes)
        {
            long[] thresholds = { 0, 3 * 1024, 2 * 1024 * 1024, 1L << 30, 1L << 40, 1L << 50 };
            string[] units = { "B", "KB", "MB", "GB", "TB", "PB" };
            int unit = 0;
            for (int i = 1; i < thresholds.Length; ++i) if (bytes >= thresholds[i]) unit = i;
            double amount = Math.Max(0, bytes) / Math.Pow(1024, unit);
            return amount.ToString(unit > 0 && amount < 100 && bytes != 0 ? "N1" : "N0", CultureInfo.CurrentCulture)
                + " " + units[unit];
        }
    }
}
