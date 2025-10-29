using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Libs
{
    class EmojiHelper
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        private const int KEYEVENTF_KEYUP = 0x0002;
        private const byte VK_LWIN = 0x5B;
        private const byte VK_OEM_PERIOD = 0xBE; // '.' key

        public static void ShowEmojiPanel()
        {
            // Press Win key down
            keybd_event(VK_LWIN, 0, 0, 0);
            // Press '.' key down
            keybd_event(VK_OEM_PERIOD, 0, 0, 0);

            // Release '.' key
            keybd_event(VK_OEM_PERIOD, 0, KEYEVENTF_KEYUP, 0);
            // Release Win key
            keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, 0);
        }
    }
}
