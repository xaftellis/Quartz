using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Quartz.Controls
{
    public partial class ChromiumButton
    {
        private ChromiumButtonComposition _composition;
        private bool _compositionFailed, _compositionAnimationDirty = true, _printing;
        private bool _transparentBackground = true;
        private bool _transparentWindow, _updatingTransparentWindow, _compositionPaintPending;
        private const int LayeredWindowStyle = 0x00080000;
        private const int NoRedirectionBitmapStyle = 0x00200000;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetLayeredWindowAttributes(IntPtr window, uint colorKey, byte alpha, uint flags);

        [Category("Appearance"), DefaultValue(true)]
        [Description("Show controls underneath the button while retaining its icon, ink effects and full click area.")]
        public bool TransparentBackground
        {
            get => _transparentBackground;
            set
            {
                if (_transparentBackground == value) return;
                _transparentBackground = value;
                UpdateTransparentWindow();
                Invalidate();
            }
        }

        private bool WantsTransparentWindow => _transparentBackground && CompositionAllowed &&
            Environment.OSVersion.Version >= new Version(6, 2);

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams parameters = base.CreateParams;
                // The layered child participates in sibling composition. Without
                // a GDI redirection bitmap, only our alpha-bearing visuals are
                // displayed. Do not add WS_EX_TRANSPARENT: input belongs here.
                if (WantsTransparentWindow)
                    parameters.ExStyle |= LayeredWindowStyle | NoRedirectionBitmapStyle;
                return parameters;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (!_transparentWindow || _printing) base.OnPaintBackground(e);
        }

        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            base.OnInvalidated(e);
            QueueCompositionPaint();
        }

        private void InitializeTransparentWindow()
        {
            _transparentWindow = WantsTransparentWindow;
            // Set a uniform input opacity. The DirectComposition surfaces
            // supply visual alpha; the button retains its rectangular hit area.
            if (_transparentWindow && !SetLayeredWindowAttributes(Handle, 0, 255, 2)) // LWA_ALPHA
            {
                Trace.WriteLine("Transparent button window unavailable: " +
                    new Win32Exception(Marshal.GetLastWin32Error()).Message);
                _compositionFailed = true;
                // QueueCompositionPaint restores an ordinary HWND after this
                // handle has finished being created, rather than throwing from
                // Form.InitializeComponent or recursing into CreateHandle.
            }
        }

        private void UpdateTransparentWindow()
        {
            if (!IsHandleCreated || IsDisposed || Disposing || _updatingTransparentWindow ||
                _transparentWindow == WantsTransparentWindow) return;
            _updatingTransparentWindow = true;
            try
            {
                // WS_EX_NOREDIRECTIONBITMAP cannot be removed from a live HWND
                // with UpdateStyles. Recreate it so the software fallback really
                // has a GDI surface, preserving the managed button and events.
                RecreateHandle();
                // Repaint siblings that were previously clipped by our opaque HWND.
                Parent?.Invalidate(Bounds, true);
                Invalidate();
            }
            finally { _updatingTransparentWindow = false; }
        }

        private void QueueCompositionPaint()
        {
            if (_printing || _compositionPaintPending || !IsHandleCreated || !Visible ||
                IsDisposed || Disposing || !(_transparentWindow || WantsTransparentWindow)) return;
            _compositionPaintPending = true;
            // A no-redirection layered HWND receives no ordinary WM_PAINT.
            // Coalesce changes on its owning UI thread and publish the existing
            // composition tree directly. DWM still owns the ripple timeline.
            BeginInvoke((Action)(() =>
            {
                _compositionPaintPending = false;
                if (!IsHandleCreated || IsDisposed || Disposing || !Visible || _printing) return;
                UpdateTransparentWindow();
                if (_transparentWindow) TryPaintComposition(null);
                else Invalidate();
            }));
        }

        // Custom Paint subscribers retain their normal one-call GDI contract.
        // EventPaint is the .NET Framework Control event-list key.
        private static readonly object PaintEventKey = typeof(Control)
            .GetField("EventPaint", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null);

        private bool CompositionAllowed => !_compositionFailed && Parent != null && !(Parent is ToolStrip) &&
            !IsDesignPreview &&
            PaintEventKey != null && Events[PaintEventKey] == null;

        private bool CanCompose => CompositionAllowed && !_printing && IsHandleCreated && Visible &&
            Width > 0 && Height > 0;

        private bool TryPaintComposition(PaintEventArgs e)
        {
            // DrawToBitmap must see the complete software face, without reading
            // or changing the screen's cached foreground (favourites use this).
            if (_printing) return false;
            UpdateTransparentWindow();
            if (!CanCompose)
            {
                DisposeComposition();
                return false;
            }
            try
            {
                if (_composition == null)
                {
                    _composition = new ChromiumButtonComposition(Handle);
                    _compositionAnimationDirty = true;
                }
                double now = ButtonFrames.Now;
                _animation.Advance(now);
                ConfigureComposition();
                PrepareBackground(_transparentWindow);
                // No text or ink is drawn to the HWND underneath the tree.
                if (!_transparentWindow) e.Graphics.DrawImageUnscaled(_buffer, 0, 0);
                _composition.PaintBackground(_pixels);
                _composition.PaintForeground(graphics =>
                {
                    PaintButtonContent(graphics);
                    PaintFocusRing(graphics);
                });
                if (_compositionAnimationDirty)
                {
                    // Start from the current time after preparing surfaces.
                    _composition.Animate(_animation, ButtonFrames.Now, Enabled);
                    _compositionAnimationDirty = false;
                }
                _composition.Commit();
                ButtonFrames.Remove(this);
                return true;
            }
            catch (Exception error) when (IsCompositionFailure(error))
            {
                FailComposition(error);
                return false;
            }
        }

        private void ConfigureComposition()
        {
            RectangleF bounds = GetInkBounds();
            Color ink = _inkColor.IsEmpty ? DefaultInkColor : _inkColor;
            bool testColor = TestModernLightClickColor && IsLightThemeForClickColorTest;
            _compositionAnimationDirty |= _composition.Configure(Size, bounds, InkCornerRadius(bounds), _origin,
                DpiScale, ink, HighlightOpacity, testColor ? Color.FromArgb(0x7C, 0xAC, 0xF8) : ink,
                testColor ? 0x52 / 255d : RippleOpacity);
        }

        // Called before Click invokes the browser's work. Committing the full
        // timeline lets DWM finish the ripple even if that work blocks this STA.
        private bool UpdateCompositionAnimation()
        {
            if (_composition == null || _printing) return false;
            try
            {
                ConfigureComposition();
                _composition.Animate(_animation, ButtonFrames.Now, Enabled);
                _composition.Commit();
                _compositionAnimationDirty = false;
                return true;
            }
            catch (Exception error) when (IsCompositionFailure(error))
            {
                FailComposition(error);
                return false;
            }
        }

        private static bool IsCompositionFailure(Exception error) => error is COMException ||
            DirectComposition.IsInterfaceFailure(error) || error is Win32Exception || error is DllNotFoundException ||
            error is EntryPointNotFoundException || error is PlatformNotSupportedException;

        private void FailComposition(Exception error)
        {
            Trace.WriteLine("Button composition unavailable: " + error.Message);
            _compositionFailed = true;
            DisposeComposition();
            // Restore a paintable HWND before using the software renderer. A
            // failed compositor must never leave an invisible, clickable button.
            UpdateTransparentWindow();
            if (IsHandleCreated && Visible && _animation.IsAnimating(ButtonFrames.Now)) ButtonFrames.Add(this);
        }

        private void DisposeComposition()
        {
            var composition = _composition;
            _composition = null;
            _compositionAnimationDirty = true;
            composition?.Dispose();
        }
    }
}
