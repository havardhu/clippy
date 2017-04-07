using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Clippy
{
    public class HotkeyManager
    {
        private static HwndSource _source;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public static void Initialize(Visual visual)
        {
            _source = PresentationSource.FromVisual(visual) as HwndSource;
            _source.AddHook(WndProc);
        }

        private static int id = 0;

        private static Dictionary<int, Action> _hotkeys = new Dictionary<int, Action>();

        public static int RegisterHotKey(Key key, Action callback, bool controlMod = false, bool shiftMod = false, bool altMod = false)
        {
            var interopKey = KeyInterop.VirtualKeyFromKey(key);

            int mods = 0;

            if (controlMod)
                mods |= MOD_CONTROL;

            if (shiftMod)
                mods |= MOD_SHIFT;

            if (altMod)
                mods |= MOD_ALT;

            RegisterHotKey(_source.Handle, mods, ++id, interopKey);

            _hotkeys.Add(id, callback);

            return id;
        }

        public const int MOD_ALT = 0x0001;
        public const int MOD_CONTROL = 0x0002;
        public const int MOD_SHIFT = 0x0004;

        const int WM_HOTKEY = 0x0312;

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                if (_hotkeys.ContainsKey((int)wParam))
                {
                    _hotkeys[(int) wParam]();
                    handled = true;
                }
                
            }

            handled = false;

            return IntPtr.Zero;
        }
    }
}
