using SkiaSharp;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Quartz.Controls.ChromiumMenus
{
    internal static class MenuPlatform
    {
        internal static int[] Breaks(string text, int kind)
        {
            var pin = GCHandle.Alloc(text, GCHandleType.Pinned); IntPtr iterator = IntPtr.Zero;
            try
            {
                int status = 0;
                iterator = ubrk_open(kind, CultureInfo.CurrentUICulture.Name.Replace('-', '_'), pin.AddrOfPinnedObject(), text.Length, ref status);
                if (status > 0 || iterator == IntPtr.Zero) throw new InvalidOperationException();
                var breaks = new List<int>();
                for (int i = ubrk_next(iterator); i != -1; i = ubrk_next(iterator)) breaks.Add(i);
                return breaks.ToArray();
            }
            catch (Exception e) when (e is DllNotFoundException || e is EntryPointNotFoundException || e is InvalidOperationException)
            { return Enumerable.Range(1, text.Length).Where(i => i == text.Length || char.IsWhiteSpace(text[i - 1])).ToArray(); }
            finally { if (iterator != IntPtr.Zero) ubrk_close(iterator); pin.Free(); }
        }
        internal static bool IsTextRtl(string text)
        {
            try { return ubidi_getBaseDirection(text, text.Length) == 1; }
            catch (Exception e) when (e is DllNotFoundException || e is EntryPointNotFoundException)
            { return CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft; }
        }
        [DllImport("icu.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)] private static extern IntPtr ubrk_open(int type, string locale, IntPtr text, int length, ref int status);
        [DllImport("icu.dll", CallingConvention = CallingConvention.Cdecl)] private static extern int ubrk_next(IntPtr iterator);
        [DllImport("icu.dll", CallingConvention = CallingConvention.Cdecl)] private static extern void ubrk_close(IntPtr iterator);
        [DllImport("icu.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, ExactSpelling = true)] private static extern int ubidi_getBaseDirection(string text, int length);
        // Mirrors UwpTextScaleFactor's UISettings activation/fallback. Read per popup;
        // Quartz's process-wide awareness and WebView scaling remain unchanged.
        internal static double AccessibilityScale
        {
            get
            {
                object settings = null;
                try
                {
                    var type = Type.GetType("Windows.UI.ViewManagement.UISettings, Windows, ContentType=WindowsRuntime");
                    if (type == null) return 1;
                    settings = Activator.CreateInstance(type);
                    return Math.Max(1, Convert.ToDouble(type.GetProperty("TextScaleFactor").GetValue(settings)));
                }
                catch (Exception e) when (e is TypeLoadException || e is System.Reflection.TargetInvocationException || e is COMException || e is MissingMemberException || e is NotSupportedException)
                { return 1; }
                finally { if (settings != null && Marshal.IsComObject(settings)) Marshal.ReleaseComObject(settings); }
            }
        }
        // Windows-specific entries in ui/strings/translations/app_locale_settings_*.xtb,
        // chromium154 a6548414. All remaining locales inherit 100%, minimum 5, default family.
        internal static void LocaleFont(out double scale, out int minimum)
        {
            string language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            scale = language == "hi" || language == "mr" ? 1.25 : language == "gu" ? 1.15 : 1;
            minimum = Array.IndexOf(new[] { "ar", "fa", "gu", "hi", "ja", "kn", "ko", "ml", "mr", "te", "th", "zh" }, language) >= 0 ? 10 : 5;
        }
        internal static string Percent(int value)
        {
            // Same ICU percent formatter as base::FormatPercent; Windows supplies ICU's C ABI.
            // The OS ICU data version may differ from Chromium's bundled ICU (documented).
            IntPtr formatter = IntPtr.Zero;
            try
            {
                int status = 0;
                formatter = unum_open(3, null, 0, CultureInfo.CurrentUICulture.Name.Replace('-', '_'), IntPtr.Zero, ref status);
                if (status > 0 || formatter == IntPtr.Zero) throw new InvalidOperationException("ICU percent formatter unavailable");
                var output = new char[128];
                int length = unum_formatDouble(formatter, value / 100.0, output, output.Length, IntPtr.Zero, ref status);
                if (status > 0 || length > output.Length) throw new InvalidOperationException("ICU percent formatting failed");
                return new string(output, 0, length);
            }
            catch (Exception e) when (e is DllNotFoundException || e is EntryPointNotFoundException || e is InvalidOperationException)
            { return (value / 100.0).ToString("P0", CultureInfo.CurrentUICulture); }
            finally { if (formatter != IntPtr.Zero) unum_close(formatter); }
        }
        internal static SKSurfaceProperties SurfaceProperties()
        {
            uint smooth = 0, type = 0, orientation = 0;
            SystemParametersInfo(0x4a, 0, ref smooth, 0); SystemParametersInfo(0x200a, 0, ref type, 0);
            if (smooth == 0 || type != 2) return new SKSurfaceProperties(SKPixelGeometry.Unknown);
            var display = new DisplayDevice { Size = Marshal.SizeOf(typeof(DisplayDevice)) };
            for (uint i = 0; EnumDisplayDevices(null, i, ref display, 0); ++i)
            {
                if ((display.StateFlags & 4) == 0) continue;
                string name = display.Name.Substring(display.Name.LastIndexOf('\\') + 1);
                object value = Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Avalon.Graphics\" + name, "PixelStructure", null);
                if (value != null)
                {
                    int v = Convert.ToInt32(value);
                    return new SKSurfaceProperties(v == 1 ? SKPixelGeometry.RgbHorizontal : v == 2 ? SKPixelGeometry.BgrHorizontal : SKPixelGeometry.Unknown);
                }
                break;
            }
            SystemParametersInfo(0x2012, 0, ref orientation, 0);
            return new SKSurfaceProperties(orientation == 1 ? SKPixelGeometry.RgbHorizontal : orientation == 0 ? SKPixelGeometry.BgrHorizontal : SKPixelGeometry.Unknown);
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)] private struct DisplayDevice
        {
            internal int Size;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] internal string Name;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] internal string Description;
            internal int StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] internal string Id;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] internal string Key;
        }
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern bool EnumDisplayDevices(string name, uint index, ref DisplayDevice device, uint flags);
        [DllImport("user32.dll")] private static extern bool SystemParametersInfo(uint action, int param, ref uint value, int flags);
        [DllImport("icu.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)] private static extern IntPtr unum_open(int style, char[] pattern, int patternLength, string locale, IntPtr parseError, ref int status);
        [DllImport("icu.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, ExactSpelling = true)] private static extern int unum_formatDouble(IntPtr format, double value, [Out] char[] output, int length, IntPtr position, ref int status);
        [DllImport("icu.dll", CallingConvention = CallingConvention.Cdecl)] private static extern void unum_close(IntPtr format);
    }
}
