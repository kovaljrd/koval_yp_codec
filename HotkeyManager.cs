using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace koval_yp_codec
{
    public static class HotkeyManager
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public const int WM_HOTKEY = 0x0312;
        public const int HOTKEY_ID = 9000;

        public static bool Register(IntPtr hWnd, Keys key, bool ctrl = false, bool shift = false, bool alt = false)
        {
            uint modifiers = 0;
            if (ctrl) modifiers |= 0x0002; // MOD_CONTROL
            if (shift) modifiers |= 0x0004; // MOD_SHIFT
            if (alt) modifiers |= 0x0001;   // MOD_ALT

            return RegisterHotKey(hWnd, HOTKEY_ID, modifiers, (uint)key);
        }

        public static bool Unregister(IntPtr hWnd)
        {
            return UnregisterHotKey(hWnd, HOTKEY_ID);
        }
    }
}