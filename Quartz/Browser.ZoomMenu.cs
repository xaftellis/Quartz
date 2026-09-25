using System;
using Quartz.Controls.ChromiumMenus;
using Quartz.Services;

namespace Quartz
{
    public partial class Browser
    {
        private void InitializeZoomMenuRow()
        {
            // AppMenu::ZoomView. The percentage is informational; Ctrl-0 stays independent.
            zoomToolStrip.Text = "Zoom";
            zoomToolStrip.GetZoom = () => wvWebView1.CoreWebView2 == null ? 1 : wvWebView1.ZoomFactor;
            zoomToolStrip.HasContents = () => wvWebView1.CoreWebView2 != null;
            zoomToolStrip.CanZoom = () => CanExecuteShortcutCommand(BrowserCommand.ZoomIn);
            zoomToolStrip.CanFullscreen = () => CanExecuteShortcutCommand(BrowserCommand.Fullscreen);
            zoomToolStrip.IsFullscreen = () => (ParentTabs as AppContainer)?.FullScreen == true;
            zoomToolStrip.Step = direction => ShortcutManager.ExecuteCommand(this, direction < 0 ? BrowserCommand.ZoomOut : BrowserCommand.ZoomIn);
            zoomToolStrip.Fullscreen = () => ShortcutManager.ExecuteCommand(this, BrowserCommand.Fullscreen);
        }
        private void StepMenuZoom(int direction)
        {
            // Chromium compares preset zoom LEVELS with epsilon .001, not factors.
            SetMenuZoom(ChromiumZoomMenuItem.NextFactor(wvWebView1.ZoomFactor, direction));
        }
        private void SetMenuZoom(double zoom)
        {
            wvWebView1.ZoomFactor = Math.Max(.25, Math.Min(5, zoom));
            // ZoomFactorChanged already persists and invalidates the row. Avoid
            // a duplicate synchronous settings-file write and full repaint on
            // every press (including each press in a double-click sequence).
        }
        private void UpdateZoomMenuRow() { zoomToolStrip?.Invalidate(); }
    }
}
