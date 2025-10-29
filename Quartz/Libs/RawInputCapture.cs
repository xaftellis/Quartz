using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quartz.Libs
{
    public class RawInputCapture : IDisposable
    {
        // ---- Win32 constants & P/Invoke ----
        private const int WM_INPUT = 0x00FF;
        private const uint RIDEV_INPUTSINK = 0x00000100;
        private const uint RIDEV_REMOVE = 0x00000001;
        private const ushort HID_USAGE_PAGE_GENERIC = 0x01;
        private const ushort HID_USAGE_GENERIC_MOUSE = 0x02;
        private const ushort HID_USAGE_GENERIC_KEYBOARD = 0x06;
        private const uint RID_INPUT = 0x10000003;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterRawInputDevices(RAWINPUTDEVICE[] pRawInputDevices, uint uiNumDevices, uint cbSize);

        [DllImport("user32.dll")]
        private static extern uint GetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);

        [StructLayout(LayoutKind.Sequential)]
        private struct RAWINPUTDEVICE
        {
            public ushort usUsagePage;
            public ushort usUsage;
            public uint dwFlags;
            public IntPtr hwndTarget;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RAWINPUTHEADER
        {
            public uint dwType;
            public uint dwSize;
            public IntPtr hDevice;
            public IntPtr wParam;
        }

        // We define mouse/keyboard sequentially and marshal them at the correct buffer offset
        [StructLayout(LayoutKind.Sequential)]
        private struct RAWMOUSE
        {
            public ushort usFlags;
            public ushort usReserved;        // alignment padding to match native union layout
            public uint ulButtons;           // union: ulButtons / (usButtonFlags, usButtonData)
            public uint ulRawButtons;
            public int lLastX;
            public int lLastY;
            public uint ulExtraInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RAWKEYBOARD
        {
            public ushort MakeCode;
            public ushort Flags;
            public ushort Reserved;
            public ushort VKey;
            public uint Message;
            public uint ExtraInformation;
        }

        // ---- events ----
        public event EventHandler<KeyCapturedEventArgs> KeyCaptured;
        public event EventHandler<MouseCapturedEventArgs> MouseCaptured;

        // ---- state ----
        private readonly Control _owner;
        private bool _registered;

        public RawInputCapture(Control owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            // Defer registration until handle exists
            if (_owner.IsHandleCreated)
                TryRegister();
            else
                _owner.HandleCreated += Owner_HandleCreated;

            _owner.Disposed += Owner_Disposed;
        }

        private void Owner_Disposed(object sender, EventArgs e)
        {
            Dispose();
        }

        private void Owner_HandleCreated(object sender, EventArgs e)
        {
            _owner.HandleCreated -= Owner_HandleCreated;
            TryRegister();
        }

        private void TryRegister()
        {
            if (_registered) return;

            try
            {
                var rid = new RAWINPUTDEVICE[2];

                rid[0].usUsagePage = HID_USAGE_PAGE_GENERIC;
                rid[0].usUsage = HID_USAGE_GENERIC_KEYBOARD;
                rid[0].dwFlags = RIDEV_INPUTSINK;
                rid[0].hwndTarget = _owner.Handle;

                rid[1].usUsagePage = HID_USAGE_PAGE_GENERIC;
                rid[1].usUsage = HID_USAGE_GENERIC_MOUSE;
                rid[1].dwFlags = RIDEV_INPUTSINK;
                rid[1].hwndTarget = _owner.Handle;

                if (!RegisterRawInputDevices(rid, (uint)rid.Length, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICE))))
                {
                    int err = Marshal.GetLastWin32Error();
                    Debug.WriteLine($"RegisterRawInputDevices failed: {err}");
                    // do not throw from here; keep _registered false and let app continue
                    return;
                }

                _registered = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception registering raw input: {ex}");
                // swallow—don't crash app on startup
            }
        }

        // Call this from your Form.WndProc
        public void ProcessMessage(ref Message m)
        {
            if (m.Msg != WM_INPUT) return;

            try
            {
                uint headerSize = (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER));
                uint dwSize = 0;

                // ask for buffer size
                uint res = GetRawInputData(m.LParam, RID_INPUT, IntPtr.Zero, ref dwSize, headerSize);
                if (dwSize == 0) return;

                IntPtr buffer = Marshal.AllocHGlobal((int)dwSize);
                try
                {
                    uint read = GetRawInputData(m.LParam, RID_INPUT, buffer, ref dwSize, headerSize);
                    if (read == 0 || read != dwSize) return;

                    // read header
                    var header = Marshal.PtrToStructure<RAWINPUTHEADER>(buffer);

                    // pointer to the payload (right after header)
                    IntPtr payloadPtr = IntPtr.Add(buffer, (int)headerSize);

                    if (header.dwType == 0) // RIM_TYPEMOUSE
                    {
                        var mouse = Marshal.PtrToStructure<RAWMOUSE>(payloadPtr);
                        OnMouseCaptured(mouse);
                    }
                    else if (header.dwType == 1) // RIM_TYPEKEYBOARD
                    {
                        var kb = Marshal.PtrToStructure<RAWKEYBOARD>(payloadPtr);
                        OnKeyCaptured(kb);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
            catch (Exception ex)
            {
                // Very important: don't let marshalling issues bubble up and crash WinForms internals
                Debug.WriteLine($"ProcessMessage WM_INPUT exception: {ex}");
            }
        }

        private void OnKeyCaptured(RAWKEYBOARD kb)
        {
            var args = new KeyCapturedEventArgs((Keys)kb.VKey, kb.Message == 0x0100 || kb.Message == 0x0104, kb.MakeCode, kb.Flags);
            KeyCaptured?.Invoke(this, args);
        }

        private void OnMouseCaptured(RAWMOUSE m)
        {
            var args = new MouseCapturedEventArgs(m.lLastX, m.lLastY, m.ulButtons, m.ulRawButtons, m.usFlags);
            MouseCaptured?.Invoke(this, args);
        }

        public void Dispose()
        {
            if (_registered)
            {
                try
                {
                    var rid = new RAWINPUTDEVICE[2];

                    rid[0].usUsagePage = HID_USAGE_PAGE_GENERIC;
                    rid[0].usUsage = HID_USAGE_GENERIC_KEYBOARD;
                    rid[0].dwFlags = RIDEV_REMOVE;
                    rid[0].hwndTarget = IntPtr.Zero;

                    rid[1].usUsagePage = HID_USAGE_PAGE_GENERIC;
                    rid[1].usUsage = HID_USAGE_GENERIC_MOUSE;
                    rid[1].dwFlags = RIDEV_REMOVE;
                    rid[1].hwndTarget = IntPtr.Zero;

                    RegisterRawInputDevices(rid, (uint)rid.Length, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICE)));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception removing raw input: {ex}");
                }
                _registered = false;
            }

            _owner.HandleCreated -= Owner_HandleCreated;
            _owner.Disposed -= Owner_Disposed;
        }
    }

    // ---- Event arg classes ----
    public sealed class KeyCapturedEventArgs : EventArgs
    {
        public Keys VKey { get; }
        public bool IsKeyDown { get; }
        public ushort MakeCode { get; }
        public ushort Flags { get; }

        internal KeyCapturedEventArgs(Keys vkey, bool isDown, ushort makeCode, ushort flags)
        {
            VKey = vkey;
            IsKeyDown = isDown;
            MakeCode = makeCode;
            Flags = flags;
        }
    }

    public sealed class MouseCapturedEventArgs : EventArgs
    {
        public int DeltaX { get; }
        public int DeltaY { get; }
        public uint Buttons { get; }       // raw button union (ulButtons)
        public uint RawButtons { get; }    // extra raw button data
        public ushort Flags { get; }       // usFlags

        internal MouseCapturedEventArgs(int dx, int dy, uint buttons, uint rawButtons, ushort flags)
        {
            DeltaX = dx;
            DeltaY = dy;
            Buttons = buttons;
            RawButtons = rawButtons;
            Flags = flags;
        }
    }
}
