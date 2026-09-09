using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuartzUpdater
{
    public partial class Form1 : Form
    {
        private readonly string _applyJobPath;
        private readonly int _ownerProcessId;
        private readonly IntPtr _ownerWindowHandle;
        private UpdateCheckResult _checkResult;
        private UpdateJob _applyJob;
        private CancellationTokenSource _operationCancellation;
        private bool _isApplying;
        private bool _closingForWorker;
        private int _progressGeneration;
        private string _visibleLogPath;

        public Form1()
            : this(null, 0, IntPtr.Zero)
        {
        }

        internal Form1(string applyJobPath)
            : this(applyJobPath, 0, IntPtr.Zero)
        {
        }

        internal Form1(string applyJobPath, int ownerProcessId)
            : this(applyJobPath, ownerProcessId, IntPtr.Zero)
        {
        }

        internal Form1(
            string applyJobPath,
            int ownerProcessId,
            IntPtr ownerWindowHandle)
        {
            _applyJobPath = applyJobPath;
            _ownerProcessId = ownerProcessId;
            _ownerWindowHandle = ownerWindowHandle;
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_applyJobPath))
            {
                OwnerWindow.TryAttach(
                    this,
                    _ownerProcessId,
                    _ownerWindowHandle);
                await CheckForUpdatesAsync();
            }
            else
                await RunApplyModeAsync();
        }

        private async Task CheckForUpdatesAsync()
        {
            CancelCurrentOperation();
            _operationCancellation = new CancellationTokenSource();

            ShowCheckingState();

            try
            {
                _checkResult = await UpdaterClass.CheckForUpdatesAsync(
                    _operationCancellation.Token);

                if (IsDisposed)
                    return;

                currentVersionValue.Text = _checkResult.InstalledVersion.ToString();
                latestVersionValue.Text = _checkResult.Release.Version.ToString();
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Value = 0;
                retryButton.Visible = false;
                closeButton.Visible = true;

                if (_checkResult.IsUpdateAvailable)
                {
                    statusLabel.Text = "Quartz " + _checkResult.Release.Version + " is available.";
                    SetDetailText("The update is ready to download from GitHub.");
                    updateButton.Text = "Download and install";
                    updateButton.Enabled = true;
                    updateButton.Visible = true;
                    closeButton.Text = "Later";
                }
                else if (_checkResult.IsDevelopmentBuild)
                {
                    statusLabel.Text = "This Quartz build is newer than the latest release.";
                    SetDetailText("QuartzUpdater will never downgrade a newer or development build automatically.");
                    updateButton.Visible = false;
                    closeButton.Text = "Close";
                }
                else
                {
                    statusLabel.Text = "Quartz is up to date.";
                    SetDetailText("You already have the latest available version.");
                    updateButton.Visible = false;
                    closeButton.Text = "Close";
                }
            }
            catch (OperationCanceledException)
            {
                if (!IsDisposed)
                {
                    statusLabel.Text = "Update check cancelled.";
                    SetDetailText(string.Empty);
                    progressBar.Style = ProgressBarStyle.Continuous;
                    retryButton.Visible = true;
                    closeButton.Visible = true;
                }
            }
            catch (Exception exception)
            {
                if (!IsDisposed)
                    ShowRecoverableError("Quartz could not check for updates.", exception);
            }
        }

        private async void updateButton_Click(object sender, EventArgs e)
        {
            if (_checkResult == null || !_checkResult.IsUpdateAvailable)
                return;

            DialogResult confirmation = MessageBox.Show(
                "Quartz " + _checkResult.Release.Version +
                " will be downloaded and installed. Quartz will close and restart when the package is ready. Continue?",
                "Install Quartz update",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);

            if (confirmation != DialogResult.Yes)
                return;

            CancelCurrentOperation();
            _operationCancellation = new CancellationTokenSource();

            updateButton.Enabled = false;
            retryButton.Visible = false;
            closeButton.Visible = false;
            cancelButton.Visible = true;
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 0;

            IProgress<UpdateProgress> progress = CreateProgressReporter();

            try
            {
                UpdateJob job = await UpdaterClass.PrepareUpdateAsync(
                    _checkResult,
                    progress,
                    _operationCancellation.Token);

                StopProgressUpdates();
                cancelButton.Visible = false;
                statusLabel.Text = "Starting the safe installer...";
                SetDetailText("Quartz will close and restart automatically.");

                UpdaterClass.StartApplyWorker(job);
                _closingForWorker = true;
                Application.Exit();
            }
            catch (OperationCanceledException)
            {
                StopProgressUpdates();
                if (IsDisposed)
                    return;

                statusLabel.Text = "Download cancelled.";
                SetDetailText("No installed Quartz files were changed.");
                progressBar.Value = 0;
                cancelButton.Visible = false;
                updateButton.Enabled = true;
                updateButton.Visible = true;
                closeButton.Visible = true;
            }
            catch (Exception exception)
            {
                StopProgressUpdates();
                if (IsDisposed)
                    return;

                cancelButton.Visible = false;
                updateButton.Enabled = true;
                updateButton.Visible = true;
                closeButton.Visible = true;
                ShowRecoverableError("The update could not be prepared.", exception);
            }
        }

        private async Task RunApplyModeAsync()
        {
            UpdateJob job;

            try
            {
                job = UpdaterClass.LoadUpdateJob(_applyJobPath);
                _applyJob = job;
            }
            catch (Exception exception)
            {
                ShowRecoverableError("The prepared update could not be opened.", exception);
                retryButton.Visible = false;
                return;
            }

            currentVersionValue.Text = "Installed";
            latestVersionValue.Text = job.ExpectedVersion;
            updateButton.Visible = false;
            retryButton.Visible = false;
            cancelButton.Visible = false;
            closeButton.Visible = false;
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 0;
            _isApplying = true;
            HideLogLink();

            IProgress<UpdateProgress> progress = CreateProgressReporter();

            try
            {
                UpdateApplyResult result = await UpdateApplier.ApplyAsync(job, progress);
                StopProgressUpdates();
                _isApplying = false;
                statusLabel.Text = "Update complete";
                SetDetailText(result.Message);
                progressBar.Value = 100;

                await Task.Delay(1500);
                Close();
            }
            catch (UpdateApplyException exception)
            {
                StopProgressUpdates();
                _isApplying = false;
                statusLabel.Text = exception.RolledBack
                    ? "Update failed — previous version restored"
                    : "Update stopped";
                SetDetailText(exception.Message);
                ShowLogLink(job.LogFilePath);
                retryButton.Visible = true;
                closeButton.Text = "Close";
                closeButton.Visible = true;

                MessageBox.Show(
                    exception.Message + Environment.NewLine + Environment.NewLine +
                    "Select Open update log for details.",
                    "Quartz update failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception exception)
            {
                StopProgressUpdates();
                _isApplying = false;
                ShowRecoverableError("The update worker failed.", exception);
            }
        }

        private async void retryButton_Click(object sender, EventArgs e)
        {
            retryButton.Visible = false;
            closeButton.Visible = false;

            if (string.IsNullOrWhiteSpace(_applyJobPath))
                await CheckForUpdatesAsync();
            else
                await RunApplyModeAsync();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            cancelButton.Enabled = false;
            StopProgressUpdates();
            statusLabel.Text = "Cancelling...";
            CancelCurrentOperation();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ShowCheckingState()
        {
            statusLabel.Text = "Checking for updates...";
            SetDetailText("Reading the installed version and contacting GitHub.");
            currentVersionValue.Text = "—";
            latestVersionValue.Text = "—";
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.MarqueeAnimationSpeed = 25;
            updateButton.Visible = false;
            retryButton.Visible = false;
            cancelButton.Visible = false;
            cancelButton.Enabled = true;
            closeButton.Visible = false;
        }

        private void ShowProgress(UpdateProgress progress)
        {
            if (progress == null || IsDisposed)
                return;

            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = progress.Percent;
            statusLabel.Text = progress.Stage;
            SetDetailText(progress.Detail);
        }

        private IProgress<UpdateProgress> CreateProgressReporter()
        {
            int generation = ++_progressGeneration;
            return new Progress<UpdateProgress>(progress =>
            {
                if (generation == _progressGeneration)
                    ShowProgress(progress);
            });
        }

        private void StopProgressUpdates()
        {
            _progressGeneration++;
        }

        private void ShowRecoverableError(string heading, Exception exception)
        {
            StopProgressUpdates();
            statusLabel.Text = heading;
            SetDetailText(exception == null
                ? string.Empty
                : UpdateApplier.GetUserFacingError(exception));
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 0;
            updateButton.Visible = false;
            cancelButton.Visible = false;
            cancelButton.Enabled = true;
            retryButton.Visible = true;
            closeButton.Text = "Close";
            closeButton.Visible = true;
        }

        internal void SetDetailText(string text)
        {
            detailLabel.Text = text ?? string.Empty;
            HideLogLink();
        }

        internal void ShowLogLink(string logPath)
        {
            if (string.IsNullOrWhiteSpace(logPath))
            {
                HideLogLink();
                return;
            }

            _visibleLogPath = logPath;
            detailLabel.Height = 36;
            logLinkLabel.Visible = true;
            logLinkLabel.BringToFront();
            logToolTip.SetToolTip(logLinkLabel, logPath);
        }

        internal string VisibleLogPath
        {
            get { return _visibleLogPath; }
        }

        private void HideLogLink()
        {
            _visibleLogPath = null;
            logLinkLabel.Visible = false;
            detailLabel.Height = 54;
            logToolTip.SetToolTip(logLinkLabel, string.Empty);
        }

        private void logLinkLabel_LinkClicked(
            object sender,
            LinkLabelLinkClickedEventArgs e)
        {
            string logPath = _visibleLogPath;

            try
            {
                if (string.IsNullOrWhiteSpace(logPath) || !File.Exists(logPath))
                {
                    MessageBox.Show(
                        "The update log is no longer available.",
                        "Quartz update log",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "The update log could not be opened. " +
                    UpdateApplier.GetUserFacingError(exception),
                    "Quartz update log",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void CancelCurrentOperation()
        {
            if (_operationCancellation == null)
                return;

            try
            {
                _operationCancellation.Cancel();
                _operationCancellation.Dispose();
            }
            catch
            {
            }
            finally
            {
                _operationCancellation = null;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_isApplying && !_closingForWorker)
            {
                e.Cancel = true;
                MessageBox.Show(
                    "Quartz is being replaced or restored. Please wait for the operation to finish.",
                    "Update in progress",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            CancelCurrentOperation();

            if (!_isApplying && _applyJob != null)
                UpdaterClass.CleanupTerminalOperation(_applyJob);

            base.OnFormClosing(e);
        }
    }
}
