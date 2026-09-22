using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quartz.Services
{
    // Only detached data belongs here. Never pass controls, WebViews, or COM streams.
    internal static class BrowserWork
    {
        private static readonly SemaphoreSlim ReadSlots = new SemaphoreSlim(2);
        private static readonly object Sync = new object();
        private static Task _writes = Task.CompletedTask;
        private static bool _initialized;

        internal static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            // Writes do not depend on UI continuations. Finish accepted writes at exit.
            Application.ApplicationExit += (sender, args) =>
            {
                Task pending;
                lock (Sync) pending = _writes;
                try { pending.GetAwaiter().GetResult(); }
                catch (Exception error) { System.Diagnostics.Debug.WriteLine(error); }
            };
        }

        internal static async Task<T> Read<T>(Func<T> work)
        {
            await ReadSlots.WaitAsync().ConfigureAwait(false);
            try { return await Task.Run(work).ConfigureAwait(false); }
            finally { ReadSlots.Release(); }
        }

        internal static Task<T> Write<T>(Func<T> work)
        {
            lock (Sync)
            {
                var next = _writes.ContinueWith(previous =>
                {
                    // A failed write must not stop later independent writes.
                    if (previous.IsFaulted) System.Diagnostics.Debug.WriteLine(previous.Exception);
                    return work();
                }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
                _writes = next;
                return next;
            }
        }
    }
}
