using System;

namespace EasyTabs
{
    /// <summary>Optional loading state supplied by a tab's content on the UI thread.</summary>
    public interface ITabLoadingState
    {
        /// <summary>Whether the content is currently loading.</summary>
        bool IsLoading { get; }

        /// <summary>Raised on the UI thread when <see cref="IsLoading"/> changes.</summary>
        event EventHandler LoadingStateChanged;
    }
}
