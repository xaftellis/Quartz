// Chromium-derived hover-card behavior. See TabHoverCards.md and Chromium-LICENSE.txt.
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EasyTabs
{
    internal sealed class TabHoverCardController : IDisposable, IMessageFilter
    {
        private readonly TitleBarTabs _parent;
        private readonly Form _overlay;
        private readonly Func<TitleBarTab> _hitTest;
        private readonly Stopwatch _clock = Stopwatch.StartNew();
        private readonly Timer _delay = new Timer();
        private readonly TabHoverCardAnimation _motion = new TabHoverCardAnimation();
        private TabHoverCard _card;
        private TitleBarTab _target;
        private ITabPreviewSource _source;
        private ITabMemorySource _memorySource;
        private string _footer;
        private static readonly ChromiumTabTheme DefaultTheme = ChromiumTabTheme.Light;
        private string _title, _domain;
        private Image _image;
        private bool _crashed, _showPreview, _capturePending, _disposed;
        private double _lastExit = double.NegativeInfinity;
        private ChromiumTabTheme _theme;
        private float _scale;

        internal TitleBarTab Target => _target;
        internal bool IsVisible => _card != null && _card.Visible;
        internal bool IsAnimating => _motion.IsAnimating;
        internal RectangleF CurrentBounds => _motion.Bounds;
        internal bool OwnsWindow(IntPtr handle) => _card != null && _card.IsHandleCreated && _card.Handle == handle;

        internal TabHoverCardController(TitleBarTabs parent, Form overlay, Func<TitleBarTab> hitTest)
        {
            _parent = parent; _overlay = overlay; _hitTest = hitTest;
            _delay.Tick += DelayElapsed;
            _parent.TabSelected += SelectionChanged;
            _parent.Tabs.CollectionModified += CollectionChanged;
            Application.AddMessageFilter(this);
        }

        private double Now => _clock.Elapsed.TotalMilliseconds;
        private bool Animate => _parent.TabRenderer?.CanAnimateHoverCards ?? false;
        private bool Valid => !_disposed && !_parent.IsDisposed && !_parent.Disposing && _parent.Visible &&
            _parent.WindowState != FormWindowState.Minimized && _parent.ShowTooltips &&
            _parent.TabRenderer != null && !_parent.TabRenderer.IsTabRepositioning &&
            Control.MouseButtons == MouseButtons.None && _target != null && _target.Parent == _parent &&
            _parent.Tabs.Contains(_target) && _target.Content != null && !_target.Content.IsDisposed &&
            !_target.Content.Disposing;

        internal void Hover(TitleBarTab target)
        {
            if (_disposed) return;
            if (target == _target) { if (target != null && !Valid) Dismiss(true); return; }
            if (target == null) { Dismiss(); return; }
            _delay.Stop(); _capturePending = false;
            Unsubscribe();
            _target = target;
            if (!Valid) { Dismiss(true); return; }
            _source = target.Content as ITabPreviewSource;
            if (_source != null) _source.PreviewChanged += PreviewChanged;
            _memorySource = target.Content as ITabMemorySource;
            if (_memorySource != null) _memorySource.MemoryUsageChanged += PreviewChanged;
            target.Content.TextChanged += PreviewChanged;
            target.Content.Disposed += ContentDisposed;

            if (_card != null && _card.Visible)
            {
                UpdateContent(true);
                _motion.Move(TargetBounds(), Now, Animate);
                Present();
                RequestImage(false);
                _memorySource?.RequestMemoryUsage();
            }
            else if (Now - _lastExit <= 300)
                Show(false);
            else
            {
                float scale = Math.Max(1, _parent.DeviceDpi / 96f);
                double width = _parent.Tabs.Max(tab => tab.Area.Width) / scale;
                _delay.Interval = Math.Max(1, (int)Math.Round(TabHoverCardAnimation.ShowDelay(width)));
                _delay.Start();
            }
        }

        private void DelayElapsed(object sender, EventArgs e)
        {
            _delay.Stop();
            if (!SynchronizePointer()) return;
            if (!Valid) { Dismiss(true); return; }
            if (_capturePending) { _capturePending = false; _source?.RequestPreview(); }
            else Show(true);
        }

        private void Show(bool initial)
        {
            if (!Valid) return;
            if (_card == null)
            {
                _card = new TabHoverCard();
                _card.Frame += CardFrame;
            }
            UpdateContent(false);
            _motion.Show(TargetBounds(), Now, initial && Animate);
            Present();
            RequestImage(initial);
            _memorySource?.RequestMemoryUsage();
        }

        private void RequestImage(bool initial)
        {
            if (_source == null || _target == null || _target.Active) return;
            if (initial || _source.PreviewImage != null) _source.RequestPreview();
            else
            {
                _capturePending = true;
                _delay.Interval = _source.IsPreviewReady ? 300 : 800;
                _delay.Start();
            }
        }

        private void UpdateContent(bool transition)
        {
            _title = GetTitle();
            _domain = FormatDomain(_source?.PreviewAddress);
            _image = _source?.PreviewImage;
            _crashed = _source?.IsPreviewCrashed ?? false;
            if (_crashed) _title = "Tab crashed";
            _showPreview = !_target.Active && _source != null;
            _theme = (_parent.TabRenderer as ChromiumTabRenderer)?.Theme ?? DefaultTheme;
            _scale = Math.Max(1, _parent.DeviceDpi / 96f);
            _footer = GetFooter();
            _card.SetContent(_title, _domain, _image, _showPreview, _crashed, _theme, _scale, transition && Animate, Now,
                _motion.TextProgress, _footer);
        }

        private string GetTitle() => (_source?.IsPreviewCrashed ?? false) ? "Tab crashed" :
            !string.IsNullOrEmpty(_target.Caption) ? _target.Caption : _target.IsLoading ? "Loading..." : "Untitled";

        private string GetFooter() => _memorySource == null ? null : TabMemoryUsageFormatter.Footer(_memorySource.MemoryUsageBytes);

        private bool SynchronizePointer()
        {
            if (_target == null) return false;
            if (!Valid) { Dismiss(true); return false; }
            var hovered = _hitTest();
            if (hovered == _target) return true;
            // The card's frame message, a preview completion, or a memory
            // callback can precede the strip's queued mouse frame. Consume the
            // latest target instead of destroying the card on this valid move.
            Hover(hovered);
            return false;
        }

        internal static string FormatDomain(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) return string.Empty;
            if (address.StartsWith("blob:", StringComparison.OrdinalIgnoreCase)) return "Blob";
            if (address.StartsWith("view-source:", StringComparison.OrdinalIgnoreCase)) return "View source";
            Uri uri;
            if (!Uri.TryCreate(address, UriKind.Absolute, out uri)) return string.Empty;
            if (uri.IsFile) return "Local or shared file";
            if (uri.Scheme == "about") return "about:" + uri.AbsolutePath;
            // Don't surface data URLs, embedded credentials, queries, fragments,
            // or file paths. Keep IDNs in ASCII rather than guessing IDN safety.
            string host = uri.IdnHost;
            if (host.StartsWith("www.", StringComparison.OrdinalIgnoreCase)) host = host.Substring(4);
            if (host.Length == 0) return uri.Scheme == "data" ? "Data" : uri.Scheme;
            if (!uri.IsDefaultPort) host += ":" + uri.Port;
            return host;
        }

        internal static Rectangle PlaceCard(Rectangle anchor, Size card, Rectangle workArea, float scale)
        {
            // Align to the tab's leading edge. Mirror at the right screen edge;
            // flip above only if there isn't room below, then constrain to screen.
            int x = anchor.Left;
            int y = anchor.Bottom - ChromiumTabMetrics.Pixel(2 * scale);
            if (x + card.Width > workArea.Right) x = anchor.Right - card.Width;
            if (y + card.Height > workArea.Bottom) y = anchor.Top - card.Height;
            x = Math.Max(workArea.Left, Math.Min(x, workArea.Right - card.Width));
            y = Math.Max(workArea.Top, Math.Min(y, workArea.Bottom - card.Height));
            return new Rectangle(x, y, Math.Min(card.Width, workArea.Width), Math.Min(card.Height, workArea.Height));
        }

        private Rectangle TargetBounds()
        {
            Rectangle anchor = _target.Area;
            anchor.Offset(_overlay.Location);
            return PlaceCard(anchor, _card.CardSize, Screen.FromRectangle(anchor).WorkingArea, _scale);
        }

        private void PreviewChanged(object sender, EventArgs e) => Refresh();
        private void ContentDisposed(object sender, EventArgs e) => Dismiss(true);
        private void SelectionChanged(object sender, TitleBarTabEventArgs e) => Dismiss(true);
        private void CollectionChanged(object sender, ListModificationEventArgs e) => Refresh();

        internal void Refresh()
        {
            if (_disposed || _target == null) return;
            if (!SynchronizePointer()) return;
            if (_card == null || !_card.Visible) return;
            string title = GetTitle();
            if (_title != title || _domain != FormatDomain(_source?.PreviewAddress) ||
                _showPreview != (!_target.Active && _source != null) || _crashed != (_source?.IsPreviewCrashed ?? false) ||
                _scale != Math.Max(1, _parent.DeviceDpi / 96f) ||
                _theme != ((_parent.TabRenderer as ChromiumTabRenderer)?.Theme ?? DefaultTheme))
            {
                UpdateContent(false);
                _motion.UpdateTarget(TargetBounds());
            }
            else if (_image != _source?.PreviewImage)
            {
                _image = _source?.PreviewImage;
                _card.UpdateThumbnail(_image, _crashed, Animate, Now);
            }
            string footer = GetFooter();
            if (_footer != footer)
            {
                _footer = footer;
                _card.UpdateFooter(footer);
            }
            _motion.UpdateTarget(TargetBounds());
            Present();
        }

        private void CardFrame(object sender, EventArgs e)
        {
            if (_target != null && !SynchronizePointer()) return;
            Present();
        }

        private void Present()
        {
            if (_card == null) return;
            _motion.Sample(Now);
            _card.Present(_overlay, _motion, Now);
        }

        internal void Dismiss(bool immediate = false)
        {
            if (_disposed) return;
            _delay.Stop(); _capturePending = false;
            Unsubscribe();
            _target = null;
            if (_card == null || !_card.Visible) return;
            if (!_motion.IsFadingOut) _lastExit = Now;
            if (immediate) { _motion.Hide(Now, false); _card.Clear(); }
            else { _motion.Hide(Now, Animate); Present(); }
        }

        private void Unsubscribe()
        {
            if (_source != null) _source.PreviewChanged -= PreviewChanged;
            if (_memorySource != null) _memorySource.MemoryUsageChanged -= PreviewChanged;
            if (_target?.Content != null)
            {
                _target.Content.TextChanged -= PreviewChanged;
                _target.Content.Disposed -= ContentDisposed;
            }
            _source = null; _memorySource = null; _image = null;
        }

        public bool PreFilterMessage(ref Message message)
        {
            int id = message.Msg;
            if ((id >= 0x100 && id <= 0x109) || id == 0x201 || id == 0x204 || id == 0x207 ||
                id == 0x20B || id == 0x20A || id == 0xA1 || id == 0xA4 || id == 0xA7 || id == 0x119)
            {
                // Each controller sees only interaction with its own window.
                Control control = Control.FromChildHandle(message.HWnd);
                if (control != null && (control == _overlay || control.FindForm() == _parent ||
                    _parent.Contains(control))) Dismiss(true);
            }
            return false;
        }

        public void Dispose()
        {
            if (_disposed) return;
            Dismiss(true); _disposed = true;
            Application.RemoveMessageFilter(this);
            _parent.TabSelected -= SelectionChanged;
            _parent.Tabs.CollectionModified -= CollectionChanged;
            _delay.Dispose(); _card?.Dispose();
        }
    }
}
