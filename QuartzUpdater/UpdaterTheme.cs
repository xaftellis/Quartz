using Microsoft.Win32;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QuartzUpdater
{
    // Keep the worker independent of Quartz.exe, its profile store and WebView2.
    internal static class UpdaterTheme
    {
        internal static string Resolve(string name)
        {
            switch ((name ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "light": return "light";
                case "dark": return "dark";
                default:
                    try
                    {
                        object value = Registry.GetValue(
                            @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                            "AppsUseLightTheme", null);
                        return value is int && (int)value == 0 ? "dark" : "light";
                    }
                    catch
                    {
                        return "light";
                    }
            }
        }

        private sealed class Palette
        {
            internal Color Background = Color.White;
            internal Color Foreground = Color.Black;
            internal Color Secondary = Color.FromArgb(96, 96, 96);
            internal Color ButtonBackground = Color.White;
            internal Color ButtonForeground = Color.Black;
            internal Color Hover = Color.FromArgb(242, 242, 242);
            internal Color Border = Color.FromArgb(219, 220, 221);
            internal Color Accent = Color.Blue;
            internal Color ProgressBackground = Color.FromArgb(242, 242, 242);
            internal FlatStyle ButtonStyle = FlatStyle.Flat;
            internal int ButtonBorder;
        }

        private static Palette GetPalette(string name)
        {
            var palette = new Palette();
            // Match Quartz.Services.NewControlThemeChanger's existing palettes.
            switch (name)
            {
                case "dark":
                    palette.Background = Color.FromArgb(35, 35, 35);
                    palette.Foreground = palette.Secondary = palette.ButtonForeground =
                        palette.Accent = Color.FromArgb(195, 195, 195);
                    palette.ButtonBackground = palette.Border = Color.FromArgb(88, 88, 88);
                    palette.Hover = Color.FromArgb(64, 64, 64);
                    palette.ProgressBackground = Color.FromArgb(64, 64, 64);
                    palette.ButtonStyle = FlatStyle.Popup;
                    palette.ButtonBorder = 1;
                    break;
            }

            if (SystemInformation.HighContrast)
            {
                palette.Background = SystemColors.Window;
                palette.Foreground = palette.Secondary = SystemColors.WindowText;
                palette.ButtonBackground = SystemColors.Control;
                palette.ButtonForeground = SystemColors.ControlText;
                palette.Accent = SystemColors.Highlight;
                palette.ProgressBackground = SystemColors.Window;
                palette.Border = SystemColors.WindowFrame;
                palette.ButtonStyle = FlatStyle.Standard;
            }
            return palette;
        }

        internal static void Apply(Form form, string name, params Control[] secondaryLabels)
        {
            Quartz.Controls.ChromiumButton.IsLightThemeForClickColorTest = name == "light";
            Palette palette = GetPalette(name);
            ApplyControl(form, palette, name == "light");
            foreach (Control label in secondaryLabels)
                label.ForeColor = palette.Secondary;
            if (form.IsHandleCreated)
                ApplyTitleBar(form.Handle, name);
        }

        private static void ApplyControl(Control control, Palette palette, bool useWindowsProgress)
        {
            control.BackColor = palette.Background;
            control.ForeColor = palette.Foreground;

            var button = control as Button;
            if (button != null)
            {
                button.UseVisualStyleBackColor = SystemInformation.HighContrast;
                button.FlatStyle = button is Quartz.Controls.ChromiumButton ? FlatStyle.Flat : palette.ButtonStyle;
                button.BackColor = palette.ButtonBackground;
                button.ForeColor = palette.ButtonForeground;
                button.FlatAppearance.BorderSize = palette.ButtonBorder;
                button.FlatAppearance.BorderColor = palette.Foreground;
                button.FlatAppearance.MouseOverBackColor = palette.Hover;
                button.FlatAppearance.MouseDownBackColor = palette.Hover;
            }

            var link = control as LinkLabel;
            if (link != null)
            {
                link.LinkColor = link.ActiveLinkColor = link.VisitedLinkColor = palette.Accent;
                link.DisabledLinkColor = palette.Secondary;
            }

            var progress = control as ProgressBar;
            if (progress != null)
            {
                progress.BackColor = useWindowsProgress ? SystemColors.Control : palette.ProgressBackground;
                progress.ForeColor = useWindowsProgress ? SystemColors.Highlight : palette.Accent;
                ApplyProgressBar(progress, useWindowsProgress);
            }

            foreach (Control child in control.Controls)
                ApplyControl(child, palette, useWindowsProgress);
        }

        internal static void ApplyProgressBar(ProgressBar progress, bool useWindowsStyle)
        {
            if (!progress.IsHandleCreated) return;
            try
            {
                if (useWindowsStyle)
                {
                    // Restore Windows styling and clear any previous palette overrides.
                    SetWindowTheme(progress.Handle, null, null);
                    var defaultColour = new IntPtr(unchecked((int)0xFF000000)); // CLR_DEFAULT
                    SendMessage(progress.Handle, 0x0409, IntPtr.Zero, defaultColour);
                    SendMessage(progress.Handle, 0x2001, IntPtr.Zero, defaultColour);
                    return;
                }

                // Native themed progress bars ignore custom colours. Retain the
                // native progress/marquee behaviour while allowing Quartz's palette.
                SetWindowTheme(progress.Handle, string.Empty, string.Empty);
                SendMessage(progress.Handle, 0x0409, IntPtr.Zero,
                    new IntPtr(ColorTranslator.ToWin32(progress.ForeColor))); // PBM_SETBARCOLOR
                SendMessage(progress.Handle, 0x2001, IntPtr.Zero,
                    new IntPtr(ColorTranslator.ToWin32(progress.BackColor))); // PBM_SETBKCOLOR
            }
            catch (DllNotFoundException) { }
            catch (EntryPointNotFoundException) { }
        }

        internal static void ApplyTitleBar(IntPtr handle, string name)
        {
            if (SystemInformation.HighContrast) return;
            Palette palette = GetPalette(name);
            try
            {
                int dark = name == "dark" ? 1 : 0;
                DwmSetWindowAttribute(handle, 20, ref dark, sizeof(int));
                int background = ColorTranslator.ToWin32(palette.Background);
                int foreground = ColorTranslator.ToWin32(palette.Foreground);
                int border = ColorTranslator.ToWin32(palette.Border);
                // Unsupported attributes return an HRESULT; Windows keeps its default.
                DwmSetWindowAttribute(handle, 35, ref background, sizeof(int));
                DwmSetWindowAttribute(handle, 36, ref foreground, sizeof(int));
                DwmSetWindowAttribute(handle, 34, ref border, sizeof(int));
            }
            catch (DllNotFoundException) { }
            catch (EntryPointNotFoundException) { }
        }

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr handle, string application, string classes);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr handle, int message, IntPtr wParam, IntPtr lParam);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr handle, int attribute, ref int value, int size);
    }
}
