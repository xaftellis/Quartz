using Quartz.Services;
using System.Threading.Tasks;

namespace Quartz
{
    public partial class Browser
    {
        private bool _formUiPrepared;
        private bool _updatingFavouriteLayout;
        private bool _favouritesPreparedForActivation;

        private void PrepareBrowserForm()
        {
            if (_formUiPrepared) return;

            // Finish the native UI in the constructor, while the entire tab is
            // hidden. Load is too late to establish its initial colors and bounds.
            using (SettingsService.BeginReadSnapshot())
            {
                SuspendLayout();
                pnlTop.SuspendLayout();
                try
                {
                    LoadTheme();
                    SetRefreshButtonState(RefreshButtonState.Refresh);
                    wvLoadingProgress.Visible = false;
                    btnBack.Enabled = false;
                    btnForward.Enabled = false;
                    LoadFavourites();
                    _formUiPrepared = true;
                }
                finally
                {
                    pnlTop.ResumeLayout(true);
                    ResumeLayout(true);
                }

                pnlFavourites.PerformLayout();
                UpdateFavBar();
                _favouritesPreparedForActivation = true;
            }
        }

        protected override void SetVisibleCore(bool value)
        {
            if (value && _formUiPrepared && !IsDisposed && !Disposing)
            {
                // EasyTabs has assigned the host size before selecting this tab.
                // Resolve anchoring and scrollbar overflow while it is still hidden.
                PerformLayout();
                pnlTop.PerformLayout();
                pnlFavourites.PerformLayout();
                UpdateFavBar();
                pnlBottom.PerformLayout();
            }
            base.SetVisibleCore(value);
        }

        internal async Task RefreshFavouritesOnActivationAsync()
        {
            // The first activation follows Load on the same UI call stack.
            // Its favourites were just prepared; do not read every icon again.
            if (_favouritesPreparedForActivation)
            {
                _favouritesPreparedForActivation = false;
                return;
            }

            bool correct = await FavouriteService.ValidatePanelAsync(pnlFavourites);
            if (!IsDisposed && !Disposing && !correct) LoadFavourites();
        }
    }
}
