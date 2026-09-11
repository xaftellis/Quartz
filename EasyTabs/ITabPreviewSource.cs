using System;
using System.Drawing;

namespace EasyTabs
{
    /// <summary>Optional page information for a tab hover card. All access is on the UI thread.</summary>
    public interface ITabPreviewSource
    {
        string PreviewAddress { get; }
        /// <summary>A borrowed, memory-only thumbnail. Consumers must clone it before retaining it.</summary>
        Image PreviewImage { get; }
        bool IsPreviewReady { get; }
        bool IsPreviewCrashed { get; }
        event EventHandler PreviewChanged;
        /// <summary>Requests a capture without activating the tab or blocking the UI.</summary>
        void RequestPreview();
    }
}
