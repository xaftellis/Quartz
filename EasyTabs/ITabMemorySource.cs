using System;

namespace EasyTabs
{
    /// <summary>Optional, asynchronous tab private-footprint estimate, in bytes.</summary>
    public interface ITabMemorySource
    {
        /// <summary>Null means no measurement is available; it never means zero usage.</summary>
        long? MemoryUsageBytes { get; }
        event EventHandler MemoryUsageChanged;
        void RequestMemoryUsage();
    }
}
