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

            // Load runs at the host's actual size, before the first visible
            // frame. Finish native controls before any asynchronous browser work.
            using (SettingsService.BeginReadSnapshot())
            {
                SuspendLayout();
                pnlTop.SuspendLayout();
                try
                {
                    LoadTheme();
                    btnStop.Visible = false;
                    btnRefresh.Visible = true;
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
