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

    /// <summary>Optional distinction between waiting for a response and loading its content.</summary>
    public interface ITabLoadingPhase : ITabLoadingState
    {
        bool IsWaiting { get; }
    }

    /// <summary>Optional favicon metadata; placeholders stay out of the loading ring.</summary>
    public interface ITabFaviconState
    {
        bool IsDefaultFavicon { get; }
    }
}
