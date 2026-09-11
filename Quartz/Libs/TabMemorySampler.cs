// Chromium-derived private-footprint attribution. See EasyTabs/TabHoverCards.md.
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Quartz.Libs
{
    internal sealed class TabMemoryProcess
    {
        internal int ProcessId;
        // One entry per unique frame; values identify the root WebView's frame.
        internal Dictionary<uint, uint> FrameRoots = new Dictionary<uint, uint>();
        internal long? PrivateBytes;
    }

    internal sealed class TabMemorySnapshot
    {
        internal long StartedAt;
        internal Dictionary<uint, long?> Pages = new Dictionary<uint, long?>();
    }

    internal static class TabMemorySampler
    {
        private sealed class Entry
        {
            internal Task<TabMemorySnapshot> Pending;
            internal TabMemorySnapshot Snapshot;
        }

        // UI-thread-only cache. Several WebViews may have distinct Environment
        // wrappers around the same browser process; use its PID to coalesce them.
        private static readonly Dictionary<int, Entry> Entries = new Dictionary<int, Entry>();
        internal static long Timestamp => Stopwatch.GetTimestamp();
        private static double Age(long timestamp) => (Timestamp - timestamp) / (double)Stopwatch.Frequency;

        internal static async Task<TabMemorySnapshot> SampleAsync(CoreWebView2Environment environment,
            int browserProcessId, long notBefore)
        {
            foreach (int key in Entries.Where(pair => pair.Value.Pending == null &&
                (pair.Value.Snapshot == null || Age(pair.Value.Snapshot.StartedAt) > 120)).Select(pair => pair.Key).ToArray())
                Entries.Remove(key);
            Entry entry;
            if (!Entries.TryGetValue(browserProcessId, out entry)) Entries[browserProcessId] = entry = new Entry();
            // Reject a shared query started before this document. Frame IDs can
            // survive navigation, so just comparing IDs is insufficient.
            while (true)
            {
                var cached = entry.Snapshot;
                if (cached != null && cached.StartedAt >= notBefore && Age(cached.StartedAt) < 2) return cached;
                if (entry.Pending == null) entry.Pending = CollectAsync(environment);
                Task<TabMemorySnapshot> pending = entry.Pending;
                TabMemorySnapshot result;
                try { result = await pending; entry.Snapshot = result; }
                finally { if (entry.Pending == pending) entry.Pending = null; }
                if (result.StartedAt >= notBefore) return result;
            }
        }

        private static async Task<TabMemorySnapshot> CollectAsync(CoreWebView2Environment environment)
        {
            long started = Timestamp;
            var infos = await environment.GetProcessExtendedInfosAsync();
            var processes = new List<TabMemoryProcess>();
            foreach (var info in infos)
            {
                if (info.ProcessInfo.Kind != CoreWebView2ProcessKind.Renderer) continue;
                var process = new TabMemoryProcess { ProcessId = info.ProcessInfo.ProcessId };
                foreach (var frame in info.AssociatedFrameInfos)
                {
                    if (frame.FrameId == 0 || process.FrameRoots.ContainsKey(frame.FrameId)) continue;
                    var root = frame;
                    var visited = new HashSet<uint>();
                    while (root.ParentFrameInfo != null && visited.Add(root.FrameId)) root = root.ParentFrameInfo;
                    process.FrameRoots[frame.FrameId] = root.FrameId;
                }
                if (process.FrameRoots.Count > 0) processes.Add(process);
            }
            // Never call WebView2/COM from the worker. Only copied IDs and frame
            // mappings cross threads, and only Quartz's own renderers are read.
            return await Task.Run(() =>
            {
                foreach (var process in processes) process.PrivateBytes = ReadPrivateBytes(process.ProcessId);
                return new TabMemorySnapshot { StartedAt = started, Pages = Attribute(processes) };
            });
        }

        internal static long? ReadPrivateBytes(int id)
        {
            try
            {
                using (var process = Process.GetProcessById(id))
                {
                    process.Refresh();
                    // Chromium's Windows private_footprint is private_bytes,
                    // rounded down to KiB, not WorkingSet64 or JS heap size.
                    return process.PrivateMemorySize64 / 1024 * 1024;
                }
            }
            catch (ArgumentException) { return null; } // exited between enumeration and read
            catch (InvalidOperationException) { return null; }
            catch (Win32Exception) { return null; }
        }

        internal static Dictionary<uint, long?> Attribute(IEnumerable<TabMemoryProcess> processes)
        {
            var pages = new Dictionary<uint, long?>();
            foreach (var process in processes.GroupBy(item => item.ProcessId).Select(group => group.First()))
            {
                if (process.FrameRoots.Count == 0) continue;
                long? share = process.PrivateBytes / process.FrameRoots.Count;
                foreach (uint root in process.FrameRoots.Values)
                {
                    if (root == 0) continue;
                    long? total;
                    if (!pages.TryGetValue(root, out total)) total = 0;
                    // If any contributing renderer cannot be read, don't display
                    // an incomplete lower total as a complete measurement.
                    pages[root] = total + share;
                }
            }
            return pages;
        }
    }
}
