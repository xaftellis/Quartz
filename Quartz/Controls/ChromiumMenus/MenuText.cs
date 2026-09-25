using HarfBuzzSharp;
using Microsoft.Win32;
using SkiaSharp;
using SkiaSharp.HarfBuzz;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Quartz.Controls.ChromiumMenus
{
    // HarfBuzz shaping, DirectWrite-backed Skia fonts, Uniscribe script/bidi runs.
    // Metrics follow gfx::PlatformFontWin and RenderText's cap-height centring.
    internal sealed class MenuText : IDisposable
    {
        private readonly SKTypeface face;
        internal readonly SKFont Font;
        internal readonly int Height, Ascent, Cap;
        private readonly Dictionary<string, Shaped> cache = new Dictionary<string, Shaped>();
        internal MenuText(bool tooltip = false)
        {
            var nc = new NonClientMetrics { Size = Marshal.SizeOf(typeof(NonClientMetrics)) };
            if (!SystemParametersInfo(0x29, nc.Size, ref nc, 0)) throw new System.ComponentModel.Win32Exception();
            var lf = tooltip ? nc.MessageFont : nc.MenuFont;
            double scale = GetDpiForSystem() / 96.0;
            MenuPlatform.LocaleFont(out double localeScale, out int minimum);
            int adjusted = (int)Math.Round(lf.Height * localeScale / scale / MenuPlatform.AccessibilityScale, MidpointRounding.AwayFromZero);
            lf.Height = (lf.Height < 0 ? -1 : 1) * Math.Max(minimum, Math.Abs(adjusted));
            IntPtr hfont = CreateFontIndirect(ref lf), dc = GetDC(IntPtr.Zero), old = SelectObject(dc, hfont);
            TextMetric tm;
            GetTextMetrics(dc, out tm);
            LogFont mapped;
            GetObject(hfont, Marshal.SizeOf(typeof(LogFont)), out mapped);
            SelectObject(dc, old); DeleteObject(hfont); ReleaseDC(IntPtr.Zero, dc);
            face = SKTypeface.FromFamilyName(mapped.FaceName, new SKFontStyle(lf.Weight == 0 ? 400 : lf.Weight, 5, lf.Italic != 0 ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright));
            Font = MakeFont(face, Math.Max(1, tm.Height - tm.InternalLeading));
            var m = Font.Metrics;
            Height = (int)Math.Ceiling(m.Descent - m.Ascent); Ascent = (int)Math.Ceiling(-m.Ascent); Cap = (int)Math.Ceiling(m.CapHeight);
        }
        private static SKFont MakeFont(SKTypeface f, float size)
        {
            uint smooth = 0, smoothingType = 0;
            SystemParametersInfo(0x4a, 0, ref smooth, 0); SystemParametersInfo(0x200a, 0, ref smoothingType, 0);
            return new SKFont(f, size) { Subpixel = smooth != 0, Edging = smooth == 0 ? SKFontEdging.Alias : smoothingType == 2 ? SKFontEdging.SubpixelAntialias : SKFontEdging.Antialias, Hinting = SKFontHinting.Normal, EmbeddedBitmaps = false, ForceAutoHinting = false };
        }
        internal int Width(string text) => (int)Math.Ceiling(Shape(text ?? "").Width);
        internal int CenterBaseline(int h)
        {
            int leading = Ascent - Cap, space = h - (leading != 0 ? Cap : Height);
            return Ascent + Math.Max(Math.Min(0, h - Height), Math.Min(Math.Abs(h - Height), space / 2 - leading));
        }
        internal static string Label(string raw)
        {
            var result = new StringBuilder();
            for (int i = 0; i < (raw ?? "").Length; ++i)
                if (raw[i] != '&') result.Append(raw[i]);
                else if (i + 1 < raw.Length && raw[i + 1] == '&') { result.Append('&'); ++i; }
            return result.ToString();
        }
        internal static string Percent(int percent) => MenuPlatform.Percent(percent);
        internal string Elide(string text, int width)
        {
            if (Width(text) <= width) return text;
            if (Width("…") > width) return "";
            var indices = StringInfo.ParseCombiningCharacters(text);
            int lo = 0, hi = indices.Length;
            while (lo < hi)
            {
                int mid = (lo + hi + 1) / 2;
                string s = text.Substring(0, mid == indices.Length ? text.Length : indices[mid]) + "…";
                if (Width(s) <= width) lo = mid; else hi = mid - 1;
            }
            return text.Substring(0, lo == indices.Length ? text.Length : indices[lo]) + "…";
        }
        internal void Draw(SKCanvas c, string text, float x, float baseline, SKColor color)
        {
            var shaped = Shape(text ?? "");
            using (var paint = new SKPaint { Color = color, IsAntialias = true })
                if (shaped.Blob != null) c.DrawText(shaped.Blob, x, baseline, paint);
        }
        private Shaped Shape(string text)
        {
            if (cache.TryGetValue(text, out Shaped existing)) return existing;
            // A popup's cache is bounded by its lifetime. Repeated zoom uses the same font.
            var runs = new List<Run>();
            if (text.Length > 0)
            {
                var scripts = new ScriptItem[text.Length + 2]; int count;
                int hr = ScriptItemize(text, text.Length, scripts.Length, IntPtr.Zero, IntPtr.Zero, scripts, out count);
                if (hr < 0) { count = 1; scripts[0].Position = 0; scripts[1].Position = text.Length; }
                for (int s = 0; s < count; ++s)
                {
                    int start = scripts[s].Position, end = scripts[s + 1].Position;
                    string segment = text.Substring(start, end - start);
                    var elements = StringInfo.GetTextElementEnumerator(segment);
                    while (elements.MoveNext())
                    {
                        string element = elements.GetTextElement();
                        SKTypeface use = face;
                        if (!face.ContainsGlyphs(element))
                            use = SKFontManager.Default.MatchCharacter(face.FamilyName, face.FontStyle, new[] { CultureInfo.CurrentUICulture.Name }, char.ConvertToUtf32(element, 0)) ?? face;
                        byte level = (byte)(scripts[s].State & 31);
                        if (runs.Count > 0 && runs[runs.Count - 1].Script == s && runs[runs.Count - 1].Face.FamilyName == use.FamilyName)
                        { runs[runs.Count - 1].Text += element; if (use != face) use.Dispose(); }
                        else runs.Add(new Run { Text = element, Face = use, Level = level, Script = s });
                    }
                }
            }
            int[] order = new int[runs.Count];
            if (runs.Count > 0) ScriptLayout(runs.Count, runs.Select(r => r.Level).ToArray(), order, null);
            float x = 0;
            SKTextBlob blob;
            using (var builder = new SKTextBlobBuilder())
            {
                foreach (int index in order)
                {
                    var run = runs[index];
                    using (var font = MakeFont(run.Face, Font.Size))
                    using (var shaper = new SKShaper(run.Face))
                    using (var buffer = new HarfBuzzSharp.Buffer())
                    {
                        buffer.AddUtf16(run.Text); buffer.GuessSegmentProperties();
                        buffer.Direction = (run.Level & 1) != 0 ? Direction.RightToLeft : Direction.LeftToRight;
                        var shaped = shaper.Shape(buffer, font);
                        var points = shaped.Points.Select(p => new SKPoint(p.X + x, p.Y)).ToArray();
                        builder.AddPositionedRun(shaped.Codepoints.Select(g => (ushort)g).ToArray(), font, points);
                        x += shaped.Width;
                    }
                }
                blob = runs.Count == 0 ? null : builder.Build();
            }
            foreach (var run in runs) if (run.Face != face) run.Face.Dispose();
            var result = new Shaped { Blob = blob, Width = x }; cache.Add(text, result); return result;
        }
        public void Dispose() { foreach (var s in cache.Values) s.Blob?.Dispose(); Font.Dispose(); face.Dispose(); }
        private sealed class Run { internal string Text; internal SKTypeface Face; internal byte Level; internal int Script; }
        private sealed class Shaped { internal SKTextBlob Blob; internal float Width; }
        [StructLayout(LayoutKind.Sequential)] private struct ScriptItem { internal int Position; internal ushort Analysis, State; }
        [StructLayout(LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Unicode)] private struct LogFont
        {
            internal int Height, Width, Escapement, Orientation, Weight;
            internal byte Italic, Underline, StrikeOut, CharSet, OutPrecision, ClipPrecision, Quality, PitchAndFamily;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] internal string FaceName;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Unicode)] private struct NonClientMetrics
        {
            internal int Size, BorderWidth, ScrollWidth, ScrollHeight, CaptionWidth, CaptionHeight;
            internal LogFont CaptionFont; internal int SmallCaptionWidth, SmallCaptionHeight; internal LogFont SmallCaptionFont;
            internal int MenuWidth, MenuHeight; internal LogFont MenuFont, StatusFont, MessageFont; internal int PaddedBorderWidth;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Unicode)] private struct TextMetric
        {
            internal int Height, Ascent, Descent, InternalLeading, ExternalLeading, AveCharWidth, MaxCharWidth, Weight, Overhang, DigitizedAspectX, DigitizedAspectY;
            internal char FirstChar, LastChar, DefaultChar, BreakChar; internal byte Italic, Underlined, StruckOut, PitchAndFamily, CharSet;
        }
        [DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)] private static extern bool SystemParametersInfo(uint a, int b, ref NonClientMetrics c, int d);
        [DllImport("user32.dll")] private static extern bool SystemParametersInfo(uint a, int b, ref uint c, int d);
        [DllImport("user32.dll")] private static extern uint GetDpiForSystem();
        [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);
        [DllImport("gdi32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)] private static extern IntPtr CreateFontIndirect(ref LogFont font);
        [DllImport("gdi32.dll")] private static extern IntPtr SelectObject(IntPtr dc, IntPtr obj);
        [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr obj);
        [DllImport("gdi32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)] private static extern bool GetTextMetrics(IntPtr dc, out TextMetric tm);
        [DllImport("gdi32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)] private static extern int GetObject(IntPtr obj, int size, out LogFont font);
        [DllImport("usp10.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)] private static extern int ScriptItemize(string text, int length, int max, IntPtr control, IntPtr state, [Out] ScriptItem[] items, out int count);
        [DllImport("usp10.dll")] private static extern int ScriptLayout(int count, byte[] levels, [Out] int[] visualToLogical, [Out] int[] logicalToVisual);
    }
}

