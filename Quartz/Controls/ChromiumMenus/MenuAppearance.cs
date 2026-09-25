using Microsoft.Win32;
using SkiaSharp;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Quartz.Controls.ChromiumMenus
{
    public sealed class MenuAppearance
    {
        // Eligible unbranded developer field-trial configuration, not C++ defaults.
        public bool RoundedIcons { get; set; } = true;
        public bool DarkNeutrals26 { get; set; } = true;
        public bool Dark { get; set; }
        public bool Animations { get; set; } = true;
        public bool Grayscale { get; set; }
        public bool HighContrast { get; set; } = SystemInformation.HighContrast;
        public bool RightToLeft { get; set; } = System.Globalization.CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;
        public static MenuAppearance ForApplication()
        {
            string theme = Services.SettingsService.Get("Theme");
            if (string.IsNullOrEmpty(theme))
                theme = Convert.ToInt32(Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1)) == 0 ? "dark" : "light";
            return new MenuAppearance { Dark = theme == "dark" || theme == "black" };
        }
        internal MenuColors Resolve() { return new MenuColors(this); }
    }

    internal sealed class MenuColors
    {
        internal SKColor Body, Text, Minor, SelectedText, Selected, Separator, Disabled, DisabledIcon, Icon, RadioOn, RadioOff, Shadow, Disc, HotDisc, ZoomIcon;
        internal SKColor TooltipBackground, TooltipForeground;
        internal static SKColor Hex(uint rgb) => new SKColor(0xff000000 | rgb);
        internal static SKColor Win(Color c) => new SKColor(c.R, c.G, c.B, c.A);
        internal static SKColor Blend(SKColor fg, SKColor bg, byte alpha)
        {
            return new SKColor((byte)((fg.Red * alpha + bg.Red * (255 - alpha) + 127) / 255),
                (byte)((fg.Green * alpha + bg.Green * (255 - alpha) + 127) / 255),
                (byte)((fg.Blue * alpha + bg.Blue * (255 - alpha) + 127) / 255));
        }
        private static double Luminance(SKColor c)
        {
            Func<byte, double> linear = v => v / 255.0 <= .04045 ? v / 3294.6 : Math.Pow((v / 255.0 + .055) / 1.055, 2.4);
            return .2126 * linear(c.Red) + .7152 * linear(c.Green) + .0722 * linear(c.Blue);
        }
        internal static SKColor DerivedIcon(SKColor text)
        {
            double l = Luminance(text), low = Luminance(Hex(0x202124));
            return Blend((1.05 / (l + .05)) > ((l + .05) / (low + .05)) ? SKColors.White : Hex(0x202124), text, 0x4c);
        }
        internal MenuColors(MenuAppearance a)
        {
            bool d = a.Dark, n = d && a.DarkNeutrals26;
            Body = Hex(!d ? 0xffffffu : n ? 0x191b1fu : 0x1f1f1fu);
            Text = Hex(!d ? 0x1f1f1fu : n ? 0xdadbe5u : 0xe3e3e3u);
            Minor = Hex(!d ? 0x474747u : n ? 0xc2c3ccu : 0xc7c7c7u);
            SelectedText = Text;
            Selected = Blend(Hex(!d ? 0x1f1f1fu : n ? 0xf9fafcu : 0xfdfcfbu), Body, d ? (byte)0x1a : (byte)0x0f);
            Separator = Hex(!d ? a.Grayscale ? 0xe3e3e3u : 0xd3e3fdu : n ? 0x4e5059u : 0x5e5e5eu);
            Disabled = Hex(!d ? 0x5f6368u : n ? 0x80868bu : 0x9aa0a6u);
            DisabledIcon = Text.WithAlpha(0x60);
            RadioOn = Hex(d ? 0xa8c7fau : 0x0b57d0u);
            RadioOff = Hex(d ? (n ? 0x8e9099u : 0x8e918fu) : 0x747775u);
            Shadow = Hex(d ? 0u : 0x3c4043u);
            Disc = Hex(!d ? 0xf2f2f2u : n ? 0x22242au : 0x282828u);
            HotDisc = Hex(!d ? 0xe6e6e6u : n ? 0x383a3fu : 0x3e3e3eu);
            ZoomIcon = Text;
            TooltipBackground = Body;
            TooltipForeground = Blend(Text, TooltipBackground, 0xde);
            if (a.HighContrast)
            {
                Body = Disc = HotDisc = Win(SystemColors.Control);
                Text = Minor = Separator = ZoomIcon = Win(SystemColors.ControlText);
                Selected = Win(SystemColors.Highlight); SelectedText = Win(SystemColors.HighlightText);
                Disabled = DisabledIcon = Win(SystemColors.GrayText);
                RadioOn = RadioOff = Win(SystemColors.ControlText);
                TooltipBackground = Win(SystemColors.Window); TooltipForeground = Win(SystemColors.WindowText);
            }
            Icon = DerivedIcon(Text);
        }
    }
}
