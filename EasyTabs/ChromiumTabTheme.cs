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
        public Color Separator { get; }
        public Color Border { get; }
        public float BorderWidth { get; }
        internal float HoverMinimum { get; }
        internal float HoverMaximum { get; }
        internal float RadialOpacity { get; }

        public static ChromiumTabTheme Light => new ChromiumTabTheme(Color.FromArgb(222, 225, 230), Color.White);

        public ChromiumTabTheme(Color frame, Color activeTab, Color? inactiveTab = null,
            Color? activeForeground = null, Color? inactiveForeground = null,
            Color? separator = null, Color? border = null, float borderWidth = 0)
        {
            Frame = frame; ActiveTab = activeTab; InactiveTab = inactiveTab ?? frame;
            ActiveForeground = activeForeground ?? ReadableForeground(ActiveTab);
            InactiveForeground = inactiveForeground ?? ReadableForeground(InactiveTab);
            Separator = separator ?? Blend(InactiveTab, InactiveForeground, .3f);
            Border = border ?? Separator;
            BorderWidth = Math.Max(0, Math.Min(2, borderWidth));
            // TabStrip::UpdateContrastRatioValues, Chromium 86.
            HoverMinimum = OpacityForContrast(InactiveTab, ActiveTab, 1.11);
            HoverMaximum = OpacityForContrast(InactiveTab, ActiveTab, 1.19);
            RadialOpacity = OpacityForContrast(InactiveTab, ActiveTab, 1.13728);
        }

        internal static Color Blend(Color from, Color to, float amount)
        {
            amount = ChromiumTabMetrics.Clamp(amount, 0, 1);
            return Color.FromArgb(255, (int)Math.Round(from.R + (to.R - from.R) * amount),
                (int)Math.Round(from.G + (to.G - from.G) * amount),
                (int)Math.Round(from.B + (to.B - from.B) * amount));
        }

        internal static double Luminance(Color color)
        {
            return Channel(color.R) * .2126 + Channel(color.G) * .7152 + Channel(color.B) * .0722;
        }
        private static double Channel(byte value)
        {
            double c = value / 255.0;
            return c <= .04045 ? c / 12.92 : Math.Pow((c + .055) / 1.055, 2.4);
        }
        private static Color ReadableForeground(Color background)
        {
            double luminance = Luminance(background);
            return (luminance + .05) / .05 >= 1.05 / (luminance + .05)
                ? Color.FromArgb(32, 33, 36) : Color.White;
        }
        private static float OpacityForContrast(Color background, Color target, double required)
        {
            double back = Luminance(background);
            for (int alpha = 0; alpha < 256; alpha++)
            {
                double blended = Luminance(Blend(background, target, alpha / 255f));
                if ((Math.Max(back, blended) + .05) / (Math.Min(back, blended) + .05) >= required)
                    return alpha / 255f;
            }
            return 1;
        }
    }
}
