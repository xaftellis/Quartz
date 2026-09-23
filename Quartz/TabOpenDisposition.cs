using System.Windows.Forms;

namespace Quartz
{
    // Chromium 130: ui/base/window_open_disposition_utils.cc and
    // third_party/blink/renderer/core/loader/navigation_policy.cc.
    internal enum TabOpenDisposition
    {
        CurrentTab,
        NewForegroundTab,
        NewBackgroundTab,
        NewWindow,
        SaveToDisk
    }

    internal static class TabOpenPolicy
    {
        internal static TabOpenDisposition FromClick(MouseButtons button, Keys modifiers)
        {
            if (button == MouseButtons.Middle || (modifiers & Keys.Control) != 0)
                return (modifiers & Keys.Shift) != 0
                    ? TabOpenDisposition.NewForegroundTab : TabOpenDisposition.NewBackgroundTab;
            if ((modifiers & Keys.Shift) != 0) return TabOpenDisposition.NewWindow;
            if ((modifiers & Keys.Alt) != 0) return TabOpenDisposition.SaveToDisk;
            return TabOpenDisposition.CurrentTab;
        }

        internal static bool ShouldActivate(TabOpenDisposition disposition, bool empty)
        {
            // TabStripModel::InsertWebContentsAtImpl: an empty strip always
            // needs a selection, even when the request was for a background tab.
            return empty || disposition != TabOpenDisposition.NewBackgroundTab;
        }

        internal static TabOpenDisposition FromAddressBar(Keys modifiers)
        {
            // OmniboxViewViews::HandleKeyEvent(VKEY_RETURN), Windows branch.
            if ((modifiers & Keys.Alt) != 0)
                return (modifiers & Keys.Shift) != 0
                    ? TabOpenDisposition.NewBackgroundTab : TabOpenDisposition.NewForegroundTab;
            return (modifiers & Keys.Shift) != 0
                ? TabOpenDisposition.NewWindow : TabOpenDisposition.CurrentTab;
        }
    }
}
