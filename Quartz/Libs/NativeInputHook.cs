using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace Quartz.Libs
{
    public class NativeInputHook
    {
        // ------------------- Constants -------------------
        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        // ------------------- Delegates & Hooks -------------------
        private static LowLevelMouseProc _mouseProc;
        private static LowLevelKeyboardProc _keyboardProc;
        private static IntPtr _mouseHookID = IntPtr.Zero;
        private static IntPtr _keyboardHookID = IntPtr.Zero;

        // ------------------- Events -------------------
        public static event Action MouseClickedOutside;
        public static event Action<Keys> KeyPressed;

        // ------------------- Start / Stop -------------------
        public static void Start()
        {
            if (_mouseHookID != IntPtr.Zero || _keyboardHookID != IntPtr.Zero)
                return;

            _mouseProc = MouseHookCallback;
            _keyboardProc = KeyboardHookCallback;

            _mouseHookID = SetHook(_mouseProc, WH_MOUSE_LL);
            _keyboardHookID = SetHook(_keyboardProc, WH_KEYBOARD_LL);
        }

        public static void Stop()
        {
            if (_mouseHookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_mouseHookID);
                _mouseHookID = IntPtr.Zero;
            }

            if (_keyboardHookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_keyboardHookID);
                _keyboardHookID = IntPtr.Zero;
            }
        }

        // ------------------- Hook Setup -------------------
        private static IntPtr SetHook(Delegate proc, int hookType)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(
                    hookType,
                    proc,
                    GetModuleHandle(curModule.ModuleName),
                    0 // 0 = global (all threads)
                );
            }
        }

        // ------------------- Callbacks -------------------
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_LBUTTONDOWN || wParam == (IntPtr)WM_RBUTTONDOWN))
            {
                if (IsAppOrWebViewFocused())
                    MouseClickedOutside?.Invoke();
            }
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                if (IsAppOrWebViewFocused())
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    KeyPressed?.Invoke((Keys)vkCode);
                }
            }
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        // ------------------- Focus Handling -------------------
        private static bool IsAppOrWebViewFocused()
        {
            IntPtr fgWindow = GetForegroundWindow();
            if (fgWindow == IntPtr.Zero) return false;

            GetWindowThreadProcessId(fgWindow, out uint pid);

            // App window focused
            if (pid == (uint)Process.GetCurrentProcess().Id)
                return true;

            return false;
        }

        public static bool IsWebViewFocused(WebView2 webView)
        {
            if (webView == null) return false;

            IntPtr fgWindow = GetForegroundWindow();
            if (fgWindow == IntPtr.Zero) return false;

            // Walk child windows to see if fgWindow belongs to WebView2
            IntPtr child = FindWindowEx(webView.Handle, IntPtr.Zero, null, null);
            while (child != IntPtr.Zero)
            {
                if (fgWindow == child)
                    return true;

                child = FindWindowEx(webView.Handle, child, null, null);
            }
            return fgWindow == webView.Handle;
        }

        // ------------------- P/Invoke -------------------
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, Delegate lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);
    }
}
