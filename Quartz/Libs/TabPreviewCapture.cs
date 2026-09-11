using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Quartz.Libs
{
    // UI-thread cache with one capture in flight. Epochs separate documents even
    // when a WebView callback arrives after navigation, transfer, or disposal.
    internal sealed class TabPreviewCapture : IDisposable
    {
        private readonly Func<Task<Bitmap>> _capture;
        private Task _pending;
        private int _epoch;
        private bool _again, _disposed;
        internal Bitmap Image { get; private set; }
        internal event EventHandler Changed;

        internal TabPreviewCapture(Func<Task<Bitmap>> capture) { _capture = capture; }

        internal Task Request()
        {
            if (_disposed) return Task.CompletedTask;
            if (_pending != null) return _pending;
            // A separate completion task also handles synchronously completing
            // capture delegates, without leaving a completed task in _pending.
            var completion = new TaskCompletionSource<bool>();
            _pending = completion.Task;
            Run(completion);
            return completion.Task;
        }

        private async void Run(TaskCompletionSource<bool> completion)
        {
            try
            {
                do
                {
                    _again = false;
                    int epoch = _epoch;
                    Bitmap image = await _capture();
                    if (_disposed || epoch != _epoch) image?.Dispose();
                    else if (image != null)
                    {
                        var old = Image;
                        Image = image;
                        old?.Dispose();
                        Changed?.Invoke(this, EventArgs.Empty);
                    }
                } while (_again && !_disposed);
                completion.TrySetResult(true);
            }
            catch (Exception error) { completion.TrySetException(error); }
            finally { _pending = null; }
        }

        internal void Invalidate()
        {
            ++_epoch;
            Image?.Dispose(); Image = null;
            // If navigation finishes while an older capture is completing,
            // retry using the latest document once that capture releases COM.
            _again = _pending != null;
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true; ++_epoch; _again = false;
            Image?.Dispose(); Image = null;
            Changed = null;
        }
    }
}
