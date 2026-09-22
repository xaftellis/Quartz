using System.Drawing;
using System.Windows.Forms;

namespace Quartz
{
    public partial class AppContainer
    {
        private bool _fullScreen;
        private FormWindowState _stateBeforeFullscreen;
        private Rectangle _boundsBeforeFullscreen;
        private bool _topMostBeforeFullscreen;

        // Fullscreen belongs to the window, so switching or closing its original
        // tab cannot lose the state needed by F11 to restore the window.
        internal bool FullScreen
        {
            get => _fullScreen;
            set
            {
                if (_fullScreen == value) return;
                if (value) SaveWindowSettings();
                _windowSettingsSaveTimer.Stop();
                _fullScreen = value;
                bool wasRestoring = _restoringWindowSettings;
                _restoringWindowSettings = true;
                try
                {
                    if (value)
                    {
                        _stateBeforeFullscreen = WindowState;
                        _boundsBeforeFullscreen = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
                        _topMostBeforeFullscreen = TopMost;
                        OverlayVisible = false;
                        WindowState = FormWindowState.Normal;
                        FormBorderStyle = FormBorderStyle.None;
                        WindowState = FormWindowState.Maximized;
                        TopMost = true;
                    }
                    else
                    {
                        WindowState = FormWindowState.Normal;
                        FormBorderStyle = FormBorderStyle.Sizable;
                        Bounds = _boundsBeforeFullscreen;
                        WindowState = _stateBeforeFullscreen;
                        OverlayVisible = true;
                        TopMost = _topMostBeforeFullscreen;
                    }
                    foreach (var tab in Tabs)
                        (tab.Content as Browser)?.ApplyFullscreenChrome(value);
                    ResizeTabContents();
                    if (!value) (SelectedTab?.Content as Browser)?.wvWebView1.Focus();
                }
                finally
                {
                    _restoringWindowSettings = wasRestoring;
                }
            }
        }
    }
}
