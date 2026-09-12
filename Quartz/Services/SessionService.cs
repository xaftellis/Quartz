using Newtonsoft.Json;
using Quartz.Models;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Quartz.Services
{
    internal sealed class SessionService : IDisposable
    {
        internal const string ContinueSetting = "ContinueWhereYouLeftOff";
        private readonly SessionStore _store;
        private readonly Func<BrowserSessionModel> _capture;
        private readonly Timer _timer = new Timer { Interval = 1000 };
        private bool _started;
        private int _capturePauses;
        private bool _checkpointPosted;
        private string _lastSavedState;

        internal Guid ProfileId { get; }
        internal bool Enabled { get; private set; }
        internal bool IsShuttingDown { get; private set; }

        internal SessionService(Guid profileId, bool enabled, Func<BrowserSessionModel> capture, SessionStore store = null)
        {
            ProfileId = profileId;
            Enabled = enabled;
            _capture = capture;
            _store = store ?? new SessionStore();
            _timer.Tick += (sender, e) =>
            {
                _checkpointPosted = false;
                Checkpoint();
            };
        }

        internal BrowserSessionModel ReadSavedSession() => Enabled ? _store.Read(ProfileId) : null;

        internal void Start()
        {
            // Wait until all restored windows and tabs exist before replacing the saved session.
            _started = true;
            _timer.Start();
            Checkpoint();
        }

        internal void SetEnabled(bool enabled)
        {
            Enabled = enabled;
            _lastSavedState = null;
            if (enabled) Checkpoint();
            else _store.Delete(ProfileId);
        }

        internal void PauseCapture() => _capturePauses++;

        internal void ResumeCapture()
        {
            if (_capturePauses > 0) _capturePauses--;
            Checkpoint();
        }

        internal void RequestCheckpoint(Control dispatcher)
        {
            if (!_started || !Enabled || IsShuttingDown || _checkpointPosted ||
                dispatcher == null || dispatcher.IsDisposed || !dispatcher.IsHandleCreated)
                return;
            _checkpointPosted = true;
            try
            {
                // Finish tab moves/reorders before observing the collection.
                dispatcher.BeginInvoke(new Action(() =>
                {
                    _checkpointPosted = false;
                    Checkpoint();
                }));
            }
            catch (InvalidOperationException) { _checkpointPosted = false; }
        }

        internal void Checkpoint()
        {
            if (!_started || !Enabled || IsShuttingDown || _capturePauses != 0) return;
            try
            {
                var snapshot = _capture();
                if (snapshot == null) return;
                snapshot.ProfileId = ProfileId;
                snapshot.SavedAtUtc = default(DateTime);
                string state = JsonConvert.SerializeObject(snapshot);
                if (state == _lastSavedState) return;
                snapshot.SavedAtUtc = DateTime.UtcNow;
                if (_store.Write(snapshot)) _lastSavedState = state;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Could not capture browser session: " + e);
            }
        }

        internal void PrepareForShutdown()
        {
            Checkpoint();
            // Sequential FormClosing events must not overwrite the complete pre-exit snapshot.
            IsShuttingDown = true;
        }

        internal void CancelShutdown()
        {
            IsShuttingDown = false;
            Checkpoint();
        }

        public void Dispose() => _timer.Dispose();
    }
}
