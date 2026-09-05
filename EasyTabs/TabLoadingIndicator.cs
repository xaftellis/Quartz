using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace EasyTabs
{
    /// <summary>Paints the Chromium-style spinner used by tab headers and the Quartz animation test.</summary>
    public static class TabLoadingIndicator
    {
        /// <summary>Draws a frame without allocating a bitmap or changing the caller's graphics settings.</summary>
        /// <param name="graphics">Destination graphics.</param>
        /// <param name="bounds">Icon rectangle.</param>
        /// <param name="color">Spinner colour.</param>
        /// <param name="elapsedMs">Elapsed animation time in milliseconds.</param>
        public static void Draw(Graphics graphics, Rectangle bounds, Color color, double elapsedMs)
        {
            // Same timing and signed-arc calculation as the successful Quartz test.
            // Reference: https://chromium.googlesource.com/chromium/src/+/main/ui/gfx/paint_throbber.cc
            const double maximumArcSize = 270.0;
            const double minimumArcSize = 5.0;
            double elapsedKeyframes = elapsedMs / (1333.0 / 2.0);
            long sweepFrame = (long)Math.Floor(elapsedKeyframes);
            double sweep = maximumArcSize * Ease(elapsedKeyframes - sweepFrame);
            if (sweepFrame % 2 == 0)
                sweep -= maximumArcSize;

            double startAngle = 270.0 + Math.Round((elapsedMs / 1568.63) * 360.0);
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

        private static double Ease(double progress)
        {
            if (progress <= 0) return 0;
            if (progress >= 1) return 1;
            double lower = 0, upper = 1, parameter = progress;
            // Invert X for CSS cubic-bezier(.4, 0, .2, 1), independently between each keyframe.
            for (int i = 0; i < 16; i++)
            {
                parameter = (lower + upper) / 2.0;
                if (Coordinate(parameter, 0.4, 0.2) < progress)
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
