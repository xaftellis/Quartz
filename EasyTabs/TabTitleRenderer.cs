// Fade behavior adapted from Chromium 85.0.4183.121 ui/gfx/render_text.cc.
// See Chromium-LICENSE.txt and ChromiumTabRendering.md.
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace EasyTabs
{
    // GDI+ retains Quartz's existing shaping/font rendering; Skia applies the
    // opacity gradient to the transparent text pixels, never to the tab fill.
    internal sealed class TabTitleRenderer
    {
        private string _caption;
        private Font _font;
        private float _scale, _contentWidth, _averageWidth;
        private bool _rightToLeft;

        // Windows supplies ICU on every OS supported by Quartz's .NET 4.8.1
        // target. Classify Unicode scalars, including supplementary characters.
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("icu.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern int u_charDirection(int codePoint);

        private static bool IsRightToLeft(string text)
        {
            // Tab::Tab uses ALIGN_TO_HEAD: the first strong character decides,
            // ignoring leading punctuation/numbers. No strong character means LTR.
            for (int i = 0; i < text.Length; i++)
            {
                int codePoint = text[i];
                if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                    codePoint = char.ConvertToUtf32(text[i], text[++i]);
                // base/i18n/rtl.cc also treats embedding and override marks as
                // strong. ICU enum values come from unicode/uchar.h.
                switch (u_charDirection(codePoint))
                {
                    case 0: case 11: case 12: return false; // L, LRE, LRO
                    case 1: case 13: case 14: case 15: return true; // R, AL, RLE, RLO
                }
            }
            return false;
        }

        private static int Round(float value) => (int)Math.Floor(value + .5f);

        private static float AverageWidth(Font font, float scale)
        {
            // PlatformFontSkia::ComputeMetricsIfNecessary uses the average width,
            // then the 'x' advance, max width, or twice the rounded ascent.
            using (var typeface = SKTypeface.FromFamilyName(font.Name))
            using (var skFont = new SKFont(typeface, font.Size / scale))
            {
                var metrics = skFont.Metrics;
                float width = metrics.AverageCharacterWidth;
                if (width == 0) width = skFont.MeasureText("x");
                if (width == 0) width = metrics.MaxCharacterWidth;
                return width == 0 ? (float)Math.Ceiling(-metrics.Ascent) * 2 : width;
            }
        }

        internal void Draw(Graphics graphics, SKBitmap pixels, string caption, Font font,
            Rectangle bounds, Color foreground, float scale)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0 || string.IsNullOrEmpty(caption)) return;

            bool fontChanged = !ReferenceEquals(_font, font) || _scale != scale;
            bool textChanged = _caption != caption;
            if (textChanged) _rightToLeft = IsRightToLeft(caption);
            using (var format = new StringFormat(StringFormat.GenericTypographic)
            {
                FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip | StringFormatFlags.MeasureTrailingSpaces |
                    (_rightToLeft ? StringFormatFlags.DirectionRightToLeft : 0),
                Trimming = StringTrimming.None,
                LineAlignment = StringAlignment.Center
            })
            {
                if (fontChanged) _averageWidth = AverageWidth(font, scale);
                if (fontChanged || textChanged)
                    _contentWidth = (float)Math.Ceiling(graphics.MeasureString(caption, font, PointF.Empty, format).Width / scale);
                _caption = caption; _font = font; _scale = scale;

                GraphicsState state = graphics.Save();
                try
                {
                    // Clip the full shaped line explicitly: GDI+ must not discard
                    // the partial final glyph or substitute an ellipsis.
                    graphics.SetClip(bounds, CombineMode.Intersect);
                    using (var brush = new SolidBrush(foreground))
                        graphics.DrawString(caption, font, brush, bounds, format);
                }
                finally { graphics.Restore(state); }
            }

            float displayWidth = bounds.Width / scale;
            if (_contentWidth <= displayWidth) return;
            float gradientWidth = Math.Min(Round(_averageWidth * 3), Round(displayWidth / 3)) * scale;
            if (gradientWidth <= 0) return;

            // CreateFadeShader ramps its endpoint to at most 20% opacity for very
            // narrow text. Right alignment expands text_rect to the content width.
            float textWidth = _rightToLeft ? _contentWidth : displayWidth;
            int fourCharacters = Math.Max(1, Round(_averageWidth * 4));
            byte endAlpha = (byte)(textWidth < fourCharacters ? Round((1 - textWidth / fourCharacters) * 51) : 0);
            float left = _rightToLeft ? bounds.Left : bounds.Right - gradientWidth;
            float right = left + gradientWidth;
            SKColor solid = SKColors.White, faded = solid.WithAlpha(endAlpha);

            // Both APIs share this premultiplied bitmap. Finish GDI+ before Skia
            // multiplies the glyph coverage (and RGB) by the fade's alpha.
            graphics.Flush(FlushIntention.Sync);
            using (var canvas = new SKCanvas(pixels))
            using (var shader = SKShader.CreateLinearGradient(new SKPoint(left, 0), new SKPoint(right, 0),
                _rightToLeft ? new[] { faded, solid } : new[] { solid, faded }, SKShaderTileMode.Clamp))
            using (var paint = new SKPaint { Shader = shader, BlendMode = SKBlendMode.DstIn })
            {
                canvas.DrawRect(new SKRect(left, bounds.Top, right, bounds.Bottom), paint);
                canvas.Flush();
            }
        }
    }
}
