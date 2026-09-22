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

        // Custom Paint subscribers retain their normal one-call GDI contract.
        // EventPaint is the .NET Framework Control event-list key.
        private static readonly object PaintEventKey = typeof(Control)
            .GetField("EventPaint", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null);

        private bool CanCompose => !_compositionFailed && !_printing && IsHandleCreated && Visible &&
            Width > 0 && Height > 0 && Parent != null && !(Parent is ToolStrip) &&
            !IsDesignPreview &&
            PaintEventKey != null && Events[PaintEventKey] == null;

        private bool TryPaintComposition(PaintEventArgs e)
        {
            // DrawToBitmap must see the complete software face, without reading
            // or changing the screen's cached foreground (favourites use this).
            if (_printing) return false;
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
                PrepareBackground();
                // No text or ink is drawn to the HWND underneath the tree.
                e.Graphics.DrawImageUnscaled(_buffer, 0, 0);
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
