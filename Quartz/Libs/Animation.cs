using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Services
{
    internal class Animation
    {
        /// <summary>
        /// Animates the window from left to right. This flag can be used with roll or slide animation. It is ignored when used with AW_CENTER or AW_BLEND.
        /// </summary>
        public const int AW_HOR_POSITIVE = 0x00000001;
        /// <summary>
        /// Animates the window from right to left. This flag can be used with roll or slide animation. It is ignored when used with AW_CENTER or AW_BLEND.
        /// </summary>
        public const int AW_HOR_NEGATIVE = 0x00000002;
        /// <summary>
        /// Animates the window from top to bottom. This flag can be used with roll or slide animation. It is ignored when used with AW_CENTER or AW_BLEND.
        /// </summary>
        public const int AW_VER_POSITIVE = 0x00000004;
        /// <summary>
        /// Animates the window from bottom to top.This flag can be used with roll or slide animation.It is ignored when used with AW_CENTER or AW_BLEND.
        /// </summary>
        public const int AW_VER_NEGATIVE = 0x00000008;
        /// <summary>
        /// Makes the window appear to collapse inward if AW_HIDE is used or expand outward if the AW_HIDE is not used. The various direction flags have no effect.
        /// </summary>
        public const int AW_CENTER = 0x00000010;
        /// <summary>
        /// Hides the window. By default, the window is shown.
        /// </summary>
        public const int AW_HIDE = 0x00010000;
        /// <summary>
        /// Activates the window. Do not use this value with AW_HIDE.
        /// </summary>
        public const int AW_ACTIVATE = 0x00020000;
        /// <summary>
        /// Uses slide animation. By default, roll animation is used. This flag is ignored when used with AW_CENTER.
        /// </summary>
        public const int AW_SLIDE = 0x00040000;
        /// <summary>
        /// Uses a fade effect. This flag can be used only if hwnd is a top-level window.
        /// </summary>
        public const int AW_BLEND = 0x00080000;
        /// <summary>
        /// Animates
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int AnimateWindow(IntPtr hwand, int dwTime, int dwFlags);
    }
}
