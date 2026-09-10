using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace EasyTabs
{
    /// <summary>Paints the Chromium-style spinner used by tab headers and the Quartz animation test.</summary>
    public static class TabLoadingIndicator
    {
        private const double ArcTime = 2000.0 / 3.0;
        private const double RotationTime = 1568.0;
        /// <summary>Draws a frame without allocating a bitmap or changing the caller's graphics settings.</summary>
        /// <param name="graphics">Destination graphics.</param>
        /// <param name="bounds">Icon rectangle.</param>
        /// <param name="color">Spinner colour.</param>
        /// <param name="elapsedMs">Elapsed animation time in milliseconds.</param>
        public static void Draw(Graphics graphics, Rectangle bounds, Color color, double elapsedMs)
        {
            float startAngle, sweep;
            GetAngles(elapsedMs, out startAngle, out sweep);
            DrawArc(graphics, bounds, color, startAngle, sweep);
        }

        internal static void GetAngles(double elapsedMs, out float angle, out float arc, int sweepKeyframeOffset = 0)
        {
            // Chromium CalculateThrobberSpinningState, including its pixel-angle rounding.
            const double maximumArcSize = 270.0;
            const double minimumArcSize = 5.0;
            // Modern TabIcon uses offset 1 to enter from the short-arc keyframe.
            double elapsedKeyframes = elapsedMs / ArcTime + sweepKeyframeOffset;
            long sweepFrame = (long)Math.Floor(elapsedKeyframes);
            double sweep = maximumArcSize * FastOutSlowIn(elapsedKeyframes - sweepFrame);
            if (sweepFrame % 2 == 0)
                sweep -= maximumArcSize;

            double startAngle = 270.0 + Math.Round((elapsedMs / RotationTime) * 360.0, MidpointRounding.AwayFromZero);
            if (sweep >= 0.0 && sweep < minimumArcSize)
            {
                startAngle -= minimumArcSize - sweep;
                sweep = minimumArcSize;
            }
            else if (sweep <= 0.0 && sweep > -minimumArcSize)
            {
                startAngle += -minimumArcSize - sweep;
                sweep = -minimumArcSize;
            }
            startAngle += ((sweepFrame / 2) % 4) * maximumArcSize;

            angle = (float)(startAngle % 360.0);
            arc = (float)sweep;
        }

        internal static void GetWaitingAngles(double elapsedMs, out float angle, out float arc)
        {
            // Last Chromium waiting implementation, before its October 2024 removal:
            // https://chromium.googlesource.com/chromium/src/+/e8b04254875e840f401e669680ae05005895d71c/ui/gfx/paint_throbber.cc
            double finish = 90 + Math.Round(Math.Max(0, elapsedMs) / 1320 * 360, MidpointRounding.AwayFromZero);
            angle = (float)(-finish % 360);
            arc = (float)Math.Min(180, finish - 90);
        }

        internal static void GetAnglesAfterWaiting(double elapsedMs, double waitingMs, ref double arcOffset,
            out float angle, out float arc)
        {
            float waitingAngle, waitingSweep;
            GetWaitingAngles(waitingMs, out waitingAngle, out waitingSweep);
            if (double.IsNaN(arcOffset))
            {
                // Find the same first millisecond as Chromium's linear search,
                // once per navigation, so the arc length is continuous on reversal.
                int lower = 0, upper = (int)Math.Ceiling(ArcTime);
                while (lower < upper)
                {
                    int middle = (lower + upper) / 2;
                    if (270 * FastOutSlowIn(Math.Min(middle / ArcTime, 1)) >= waitingSweep) upper = middle;
                    else lower = middle + 1;
                }
                arcOffset = ArcTime + Math.Min(lower, ArcTime);
            }
            double effectiveTime = Math.Max(0, elapsedMs) + arcOffset;
            GetAngles(effectiveTime, out angle, out arc);
            angle = (float)((angle + waitingAngle - 270 +
                Math.Round(elapsedMs / RotationTime * 360, MidpointRounding.AwayFromZero) -
                Math.Round(effectiveTime / RotationTime * 360, MidpointRounding.AwayFromZero)) % 360);
        }

        private static void DrawArc(Graphics graphics, Rectangle bounds, Color color, float startAngle, float sweep)
        {
            float size = Math.Min(bounds.Width, bounds.Height);
            if (size <= 0) return;
            // All themes use the small SVG's proportions: 16px, radius 6.875px,
            // stroke 2.25px. Rounding its 1.125px inset up to 2px shrinks the ring.
            float strokeWidth = 2.25F * (size / 16F);
            // Leave a fractional pixel for GDI+ antialiasing at the viewport edge.
            float inset = strokeWidth / 2F + 0.125F;
            RectangleF arcBounds = new RectangleF(bounds.X + inset, bounds.Y + inset,
                size - 2F * inset, size - 2F * inset);
            GraphicsState state = graphics.Save();
            try
            {
                // Like the SVG viewport, keep antialiasing inside the favicon slot.
                graphics.SetClip(bounds, CombineMode.Intersect);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                // SVG coordinates describe pixel edges; GDI+'s default samples at integer
                // centres, shifting a full-size ring into the bottom/right clipping edge.
                graphics.PixelOffsetMode = PixelOffsetMode.Half;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                using (Pen pen = new Pen(color, strokeWidth))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    graphics.DrawArc(pen, arcBounds, (float)(startAngle % 360.0), (float)sweep);
                }
            }
            finally
            {
                graphics.Restore(state);
            }
        }

        internal static double FastOutSlowIn(double progress)
        {
            return CubicBezier(progress, .4, .2);
        }

        internal static double LinearOutSlowIn(double progress)
        {
            return CubicBezier(progress, 0, .2);
        }

        private static double CubicBezier(double progress, double control1, double control2)
        {
            if (progress <= 0) return 0;
            if (progress >= 1) return 1;
            double lower = 0, upper = 1, parameter = progress;
            // Invert X for CSS cubic-bezier(.4, 0, .2, 1), independently between each keyframe.
            for (int i = 0; i < 16; i++)
            {
                parameter = (lower + upper) / 2.0;
                if (Coordinate(parameter, control1, control2) < progress)
                    lower = parameter;
                else
                    upper = parameter;
            }
            return Coordinate(parameter, 0, 1);
        }

        private static double Coordinate(double parameter, double control1, double control2)
        {
            double inverse = 1.0 - parameter;
            return 3.0 * inverse * inverse * parameter * control1 +
                   3.0 * inverse * parameter * parameter * control2 +
                   parameter * parameter * parameter;
        }
    }
}
