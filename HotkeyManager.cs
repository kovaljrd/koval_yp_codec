using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace koval_yp_codec
{
    /// <summary>
    /// Менеджер для регистрации и обработки глобальных горячих клавиш через WinAPI
    /// </summary>
    public static class HotkeyManager
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        /// <summary>Идентификатор сообщения о нажатии горячей клавиши</summary>
        public const int WM_HOTKEY = 0x0312;

        /// <summary>Уникальный идентификатор горячей клавиши</summary>
        public const int HOTKEY_ID = 9000;

        // Константы модификаторов
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;

        /// <summary>
        /// Регистрация глобальной горячей клавиши
        /// </summary>
        /// <param name="hWnd">Дескриптор окна, получающего сообщения</param>
        /// <param name="key">Основная клавиша</param>
        /// <param name="ctrl">Требуется ли Ctrl</param>
        /// <param name="shift">Требуется ли Shift</param>
        /// <param name="alt">Требуется ли Alt</param>
        /// <returns>True если регистрация успешна, иначе False</returns>
        public static bool Register(IntPtr hWnd, Keys key, bool ctrl = false, bool shift = false, bool alt = false)
        {
            uint modifiers = 0;
            if (ctrl) modifiers |= MOD_CONTROL;
            if (shift) modifiers |= MOD_SHIFT;
            if (alt) modifiers |= MOD_ALT;

            return RegisterHotKey(hWnd, HOTKEY_ID, modifiers, (uint)key);
        }

        /// <summary>
        /// Отмена регистрации горячей клавиши
        /// </summary>
        /// <param name="hWnd">Дескриптор окна</param>
        /// <returns>True если отмена успешна, иначе False</returns>
        public static bool Unregister(IntPtr hWnd)
        {
            return UnregisterHotKey(hWnd, HOTKEY_ID);
        }
    }
}