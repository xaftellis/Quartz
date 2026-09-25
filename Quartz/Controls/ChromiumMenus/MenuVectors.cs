using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Quartz.Controls.ChromiumMenus
{
    internal static class MenuVectors
    {
        private sealed class Rep { internal int Size; internal SKPath Path; internal bool Flip; }
        private static readonly Dictionary<string, List<Rep>> cache = new Dictionary<string, List<Rep>>();
        private static readonly Dictionary<string, string> svg = new Dictionary<string, string>
        {
            { "MOVE_TO", "M" }, { "R_MOVE_TO", "m" }, { "LINE_TO", "L" }, { "R_LINE_TO", "l" },
            { "H_LINE_TO", "H" }, { "R_H_LINE_TO", "h" }, { "V_LINE_TO", "V" }, { "R_V_LINE_TO", "v" },
            { "CUBIC_TO", "C" }, { "R_CUBIC_TO", "c" }, { "ARC_TO", "A" }, { "R_ARC_TO", "a" }, { "CLOSE", "Z" }
        };
        private static List<Rep> Read(string name)
        {
            if (cache.TryGetValue(name, out var cached)) return cached;
            var reps = new List<Rep>();
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ChromiumIcons." + name + ".icon"))
            using (var reader = new StreamReader(stream ?? throw new InvalidOperationException("Missing Chromium vector: " + name)))
            {
                string source = Regex.Replace(reader.ReadToEnd(), @"//[^\r\n]*", "");
                string[] tokens = source.Split(new[] { ',', ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                Rep rep = null; var path = new StringBuilder();
                Action flush = () =>
                {
                    if (rep == null) return;
                    var parsed = SKPath.ParseSvgPathData(path.ToString());
                    if (parsed != null) { rep.Path.AddPath(parsed); parsed.Dispose(); }
                    path.Clear();
                };
                for (int i = 0; i < tokens.Length; ++i)
                {
                    string t = tokens[i];
                    if (t == "CANVAS_DIMENSIONS")
                    {
                        flush(); rep = new Rep { Size = int.Parse(tokens[++i]), Path = new SKPath { FillType = SKPathFillType.EvenOdd } }; reps.Add(rep);
                    }
                    else if (t == "FILL_RULE_NONZERO") rep.Path.FillType = SKPathFillType.Winding;
                    else if (t == "FLIPS_IN_RTL") rep.Flip = true;
                    else if (t == "CIRCLE")
                    {
                        flush(); float x = Number(tokens[++i]), y = Number(tokens[++i]), r = Number(tokens[++i]); rep.Path.AddCircle(x, y, r);
                    }
                    else if (svg.TryGetValue(t, out string command)) path.Append(command).Append(' ');
                    else path.Append(Number(t).ToString("R", CultureInfo.InvariantCulture)).Append(' ');
                }
                flush();
            }
            cache.Add(name, reps); return reps;
        }
        private static float Number(string s) => float.Parse(s.TrimEnd('f'), CultureInfo.InvariantCulture);
        internal static void Draw(SKCanvas c, string name, float x, float y, int size, float scale, SKColor color, bool rtl)
        {
            var reps = Read(name); int px = (int)Math.Ceiling(size * scale);
            var rep = reps.FirstOrDefault(r => r.Size == px)
                ?? reps.Where(r => px % r.Size == 0).OrderByDescending(r => r.Size).FirstOrDefault()
                ?? reps.OrderBy(r => r.Size).FirstOrDefault(r => r.Size > px)
                ?? reps.OrderByDescending(r => r.Size).First();
            c.Save(); c.Translate(x, y);
            if (rtl && rep.Flip) { c.Translate(size, 0); c.Scale(-1, 1); }
            c.Scale((float)size / rep.Size);
            using (var p = new SKPaint { Color = color, IsAntialias = true }) c.DrawPath(rep.Path, p);
            c.Restore();
        }
    }
}
