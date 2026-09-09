// Portions adapted from Chromium 86.0.4240.75, Copyright The Chromium Authors.
// See Chromium-LICENSE.txt and ChromiumTabRendering.md for sources and scope.
using System;
using System.Drawing;
using SkiaSharp;

namespace EasyTabs
{
    /// <summary>Chrome 86 desktop (non-touch) measurements, in device-independent pixels.</summary>
    internal static class ChromiumTabMetrics
    {
        internal const int CornerRadius = 8;
        internal const int ToolbarOverlap = 1;
        internal const int Height = 34 + ToolbarOverlap;
        internal const int SeparatorWidth = 1;
        internal const int SeparatorHeight = 20;
        internal const int Overlap = CornerRadius * 2 + SeparatorWidth;
        internal const int StandardWidth = 240 + Overlap - SeparatorWidth;
        internal const int ContentsInset = CornerRadius * 2;
        internal const int PinnedWidth = 23 + ContentsInset * 2;
        internal const int MinimumActiveWidth = 16 + ContentsInset * 2;
        internal const int MinimumInactiveWidth = 16 - SeparatorWidth + Overlap;
        internal const int NewTabButtonSize = 28;

        internal static int Pixel(float value) => (int)Math.Round(value, MidpointRounding.AwayFromZero);
        internal static float Clamp(float value, float min, float max) => Math.Max(min, Math.Min(max, value));

        // TabStyle::GetStandardWidth and TabStripLayout's overlapping slots. Reserve
        // the active tab's minimum before distributing remaining space left to right.
        internal static int[] LayoutWidths(int count, int activeIndex, int available, float scale)
        {
            var widths = new int[count];
            if (count == 0) return widths;
            int overlap = Pixel(Overlap * scale);
            int total = Math.Max(0, available) + (count - 1) * overlap;
            int standard = Pixel(StandardWidth * scale);
            int minimumActive = Pixel(MinimumActiveWidth * scale);
            int shared = Math.Min(standard, total / count);
            int reserved = activeIndex >= 0 && shared < minimumActive ? minimumActive : shared;
            int remaining = Math.Max(0, total - (activeIndex >= 0 ? reserved : 0));
            int others = count - (activeIndex >= 0 ? 1 : 0);
            int otherWidth = others == 0 ? 0 : Math.Min(standard, remaining / others);
            int remainder = others == 0 || otherWidth == standard ? 0 : remaining - otherWidth * others;
            for (int i = 0; i < count; i++)
                widths[i] = i == activeIndex ? reserved : otherWidth + (remainder-- > 0 ? 1 : 0);
            // Below the minimum required strip width, retain real minimum tab sizes;
            // the host clips overflow instead of producing inverted paths/hit areas.
            for (int i = 0; i < count; i++)
                widths[i] = Math.Max(Pixel(MinimumInactiveWidth * scale), widths[i]);
            return widths;
        }
    }

    /// <summary>Host-independent Skia paths; inputs and outputs use physical pixels.</summary>
    internal sealed class ChromiumTabGeometry : IDisposable
    {
        internal readonly SKPath Fill, Border, HitTest;
        internal readonly SKRect AlignedBounds;
        internal readonly int Width, Height;
        internal readonly float Scale, Stroke;
        internal readonly bool ExtendHit, First;

        internal ChromiumTabGeometry(int width, int height, float scale, float stroke, bool extendHit, bool first)
        {
            Width = width; Height = height; Scale = scale; Stroke = stroke;
            ExtendHit = extendHit; First = first;
            // GM2TabStyle::ScaleAndAlignBounds. Round both edges independently,
            // keeping shared separator positions aligned at fractional DPI scales.
            float leftInset = ChromiumTabMetrics.CornerRadius * scale;
            float rightInset = (ChromiumTabMetrics.CornerRadius + 1) * scale;
            float verticalInset = stroke * scale;
            AlignedBounds = new SKRect(
                ChromiumTabMetrics.Pixel(leftInset) - leftInset,
                ChromiumTabMetrics.Pixel(verticalInset) - verticalInset,
                ChromiumTabMetrics.Pixel(width - rightInset) + rightInset,
                ChromiumTabMetrics.Pixel(height - verticalInset) + verticalInset);
            Fill = CreatePath(false, false);
            Border = CreatePath(true, false);
            HitTest = CreatePath(false, true);
        }

        private SKPath CreatePath(bool border, bool hit)
        {
            float radius = ChromiumTabMetrics.Clamp((Width / Scale - 16) / 3f, 0, 8) * Scale;
            float extension = ChromiumTabMetrics.CornerRadius * Scale;
            float left = AlignedBounds.Left, right = AlignedBounds.Right;
            float top = AlignedBounds.Top, bottom = AlignedBounds.Bottom - Scale;
            float tabLeft = left + extension, tabRight = right - extension;
            float topRadius = radius, bottomRadius = radius;
            float stroke = Stroke * Scale;
            if (hit)
            {
                bottom -= stroke;
                bottomRadius -= stroke;
                if (ExtendHit && First) tabLeft = left;
            }
            else
            {
                tabLeft += stroke / 2; tabRight -= stroke / 2;
                top += stroke / 2; bottom -= stroke / 2;
                topRadius -= stroke / 2; bottomRadius -= stroke / 2;
            }
            topRadius = Math.Max(0, topRadius); bottomRadius = Math.Max(0, bottomRadius);
            using (var path = new SKPathBuilder())
            {
            path.MoveTo(left, AlignedBounds.Bottom);
            if (tabLeft != left)
            {
                path.LineTo(left, bottom);
                path.LineTo(tabLeft - bottomRadius, bottom);
                Arc(path, bottomRadius, SKPathDirection.CounterClockwise, tabLeft, bottom - bottomRadius);
            }
            if (hit && ExtendHit) path.LineTo(tabLeft, top);
            else
            {
                path.LineTo(tabLeft, top + topRadius);
                Arc(path, topRadius, SKPathDirection.Clockwise, tabLeft + topRadius, top);
            }
            if (hit && ExtendHit) path.LineTo(tabRight, top);
            else
            {
                path.LineTo(tabRight - topRadius, top);
                Arc(path, topRadius, SKPathDirection.Clockwise, tabRight, top + topRadius);
            }
            path.LineTo(tabRight, bottom - bottomRadius);
            Arc(path, bottomRadius, SKPathDirection.CounterClockwise, tabRight + bottomRadius, bottom);
            path.LineTo(right, bottom);
            path.LineTo(right, AlignedBounds.Bottom);
            if (!border) path.Close(); // The toolbar joins the open bottom of the border.
            return path.Detach();
            }
        }

        private static void Arc(SKPathBuilder path, float radius, SKPathDirection direction, float x, float y)
        {
            if (radius <= 0) path.LineTo(x, y);
            else path.ArcTo(radius, radius, 0, SKPathArcSize.Small, direction, x, y);
        }

        internal RectangleF ContentClip(float leadingOpacity, float trailingOpacity)
        {
            // GetPath(kInteriorClip): child padding is applied in aligned pixels.
            float left = AlignedBounds.Left + 2.5f + leadingOpacity + (8 + Stroke) * Scale;
            float right = AlignedBounds.Right - 2.5f - trailingOpacity - (8 + Stroke) * Scale;
            float top = AlignedBounds.Top + Stroke * Scale;
            return RectangleF.FromLTRB(left, top, Math.Max(left, right), AlignedBounds.Bottom - Scale);
        }

        public void Dispose() { Fill.Dispose(); Border.Dispose(); HitTest.Dispose(); }
    }
}
