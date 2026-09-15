using System;
using System.Drawing;

namespace EasyTabs
{
    /// <summary>Immutable renderer colours. A future colour picker can replace this palette.</summary>
    public sealed class ChromiumTabTheme
    {
        public Color Frame { get; }
        public Color ActiveTab { get; }
        public Color InactiveTab { get; }
        public Color ActiveForeground { get; }
        public Color InactiveForeground { get; }
        public Color Separator => ActiveWindow.Separator;
        public Color Border => ActiveWindow.Border;
        public float BorderWidth { get; }
        private readonly bool _customColors;

        internal WindowPalette ActiveWindow { get; }
        internal WindowPalette InactiveWindow { get; }

        public static ChromiumTabTheme Light => new ChromiumTabTheme(Color.FromArgb(222, 225, 230), Color.White,
            activeForeground: Color.FromArgb(60, 64, 67), inactiveForeground: Color.FromArgb(60, 64, 67));

        public ChromiumTabTheme(Color frame, Color activeTab, Color? inactiveTab = null,
            Color? activeForeground = null, Color? inactiveForeground = null,
            Color? separator = null, Color? border = null, float borderWidth = 0, bool customColors = false)
        {
            _customColors = customColors;
            Frame = frame; ActiveTab = activeTab; InactiveTab = inactiveTab ?? frame;
            ActiveForeground = activeForeground ?? ReadableForeground(ActiveTab);
            InactiveForeground = inactiveForeground ?? ReadableForeground(InactiveTab);
            BorderWidth = Math.Max(0, Math.Min(2, borderWidth));
            ActiveWindow = new WindowPalette(this, true, separator, border);
            InactiveWindow = new WindowPalette(this, false, separator, border);
        }

        // Chromium 85 keeps window activation independent of tab selection.
        // ThemeProperties::TINT_FRAME_INACTIVE is {-1, -1, 0.642}; its HSLShift
        // lightness operation blends RGB channels toward white by 0.284. Custom
        // colours follow BrowserThemePack::BuildFromColors, which explicitly
        // disables the inactive frame tint. The Windows accent tint is not used.
        internal sealed class WindowPalette
        {
            private readonly ChromiumTabTheme _theme;
            private readonly Color _selectedForeground, _backgroundForeground;
            internal bool IsActive { get; }
            internal Color Frame { get; }
            internal Color ActiveTab => _theme.ActiveTab;
            internal Color InactiveTab { get; }
            internal Color Separator { get; }
            internal Color Border { get; }
            internal float HoverMinimum { get; }
            internal float HoverMaximum { get; }
            internal float RadialOpacity { get; }

            internal WindowPalette(ChromiumTabTheme theme, bool active, Color? separator, Color? border)
            {
                _theme = theme;
                IsActive = active;
                Frame = active || theme._customColors ? theme.Frame : InactiveFrameColor(theme.Frame);
                InactiveTab = active || theme._customColors ? theme.InactiveTab : InactiveFrameColor(theme.InactiveTab);
                _selectedForeground = CalculateForeground(true, ActiveTab);
                _backgroundForeground = CalculateForeground(false, InactiveTab);
                HoverMinimum = OpacityForContrast(InactiveTab, ActiveTab, 1.11);
                HoverMaximum = OpacityForContrast(InactiveTab, ActiveTab, 1.19);
                RadialOpacity = OpacityForContrast(InactiveTab, ActiveTab, 1.13728);
                // TabStrip::UpdateContrastRatioValues recalculates these for both states.
                Separator = separator ?? BlendForContrast(InactiveTab, InactiveTab,
                    Foreground(false, InactiveTab), 2.5f);
                Border = border ?? Separator;
            }

            internal Color Foreground(bool selected, Color background)
            {
                // Most tabs retain one of these fills; only a hovered tab needs
                // to repeat the contrast search during a frame.
                if (selected && background.ToArgb() == ActiveTab.ToArgb()) return _selectedForeground;
                if (!selected && background.ToArgb() == InactiveTab.ToArgb()) return _backgroundForeground;
                return CalculateForeground(selected, background);
            }

            private Color CalculateForeground(bool selected, Color background)
            {
                Color color = selected ? _theme.ActiveForeground : _theme.InactiveForeground;
                // BrowserThemePack propagates a custom selected-tab foreground to
                // both frame states. GetTabForegroundColor returns explicit custom
                // colours unchanged; only the unspecified inactive background-tab
                // foreground falls through to the fade and contrast adjustment.
                if (_theme._customColors && (selected || IsActive)) return color;
                // TabStrip::GetTabForegroundColor: fade first, then retain the
                // reference contrast against the actual (possibly hovered) fill.
                if (!IsActive) color = Blend(background, color, .75f);
                float contrast = selected ? (IsActive ? 10.46f : 5f) : (IsActive ? 7.98f : 4.5f);
                return BlendForContrast(color, background, MaxContrastColor(background), contrast);
            }
        }

        private static Color InactiveFrameColor(Color color)
        {
            float shift = (.642f - .5f) * 2f;
            return Color.FromArgb(255, RoundChannel(color.R + (255 - color.R) * shift),
                RoundChannel(color.G + (255 - color.G) * shift), RoundChannel(color.B + (255 - color.B) * shift));
        }

        internal static Color Blend(Color from, Color to, float amount)
        {
            amount = ChromiumTabMetrics.Clamp(amount, 0, 1);
            // color_utils::AlphaBlend uses round-half-away-from-zero.
            return Color.FromArgb(255, RoundChannel(from.R * (1 - amount) + to.R * amount),
                RoundChannel(from.G * (1 - amount) + to.G * amount),
                RoundChannel(from.B * (1 - amount) + to.B * amount));
        }

        private static int RoundChannel(float value) => (int)Math.Round(value, MidpointRounding.AwayFromZero);

        internal static Color MaxContrastColor(Color background) =>
            Luminance(background) < .211692036f ? Color.White : Color.FromArgb(32, 33, 36);

        private static float Contrast(Color a, Color b)
        {
            float first = (float)Luminance(a) + .05f, second = (float)Luminance(b) + .05f;
            return Math.Max(first, second) / Math.Min(first, second);
        }

        // color_utils::BlendForMinContrast, including its 8-bit alpha search and
        // target fallback when the requested contrast cannot be reached.
        private static Color BlendForContrast(Color foreground, Color background, Color target, float contrast)
        {
            int alpha;
            return BlendForContrast(foreground, background, target, contrast, out alpha);
        }

        private static Color BlendForContrast(Color foreground, Color background, Color target, float contrast, out int alpha)
        {
            alpha = 0;
            if (Contrast(foreground, background) >= contrast) return foreground;
            alpha = 255;
            Color best = target;
            for (int low = 0, high = 256; low < high;)
            {
                int current = (low + high) / 2;
                Color candidate = Blend(foreground, target, current / 255f);
                if (Contrast(candidate, background) >= contrast)
                {
                    alpha = current;
                    best = candidate;
                    high = current;
                }
                else low = current + 1;
            }
            return best;
        }

        internal static double Luminance(Color color)
        {
            return Channel(color.R) * .2126f + Channel(color.G) * .7152f + Channel(color.B) * .0722f;
        }
        private static float Channel(byte value)
        {
            float c = value / 255f;
            return c <= .04045f ? c / 12.92f : (float)Math.Pow((c + .055f) / 1.055f, 2.4f);
        }
        private static Color ReadableForeground(Color background)
        {
            double luminance = Luminance(background);
            return (luminance + .05) / .05 >= 1.05 / (luminance + .05)
                ? Color.FromArgb(32, 33, 36) : Color.White;
        }
        private static float OpacityForContrast(Color background, Color target, double required)
        {
            int alpha;
            BlendForContrast(background, background, target, (float)required, out alpha);
            return alpha / 255f;
        }
    }
}
