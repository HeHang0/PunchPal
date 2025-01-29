using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace PunchPal.WPF.Tools
{
    public class HotKeyTools
    {
        #region 系统api
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifiers, uint vk);

        [DllImport("user32.dll")]
        static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        #endregion

        private static Window window;
        private static HotKeyCallBackHandler hotKeyCallBackHandler;
        private static int handlerId = 10;

        public static void SetCallback(Window win, HotKeyCallBackHandler callBackHandler)
        {
            window = win;
            hotKeyCallBackHandler = callBackHandler;
        }
        public static bool Register(ModifierKeys fsModifiers, Key key)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            var _hwndSource = HwndSource.FromHwnd(hwnd);
            _hwndSource.AddHook(WndProc);

            int id = handlerId;
            ++handlerId;
            var vk = KeyInterop.VirtualKeyFromKey(key);
            if (!RegisterHotKey(hwnd, handlerId, fsModifiers, (uint)vk)) return false;
            Unregister(id);
            return true;

        }

        static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                hotKeyCallBackHandler?.Invoke();
            }
            return IntPtr.Zero;
        }

        public static void Unregister(int? id = null)
        {
            try
            {
                var hwnd = new WindowInteropHelper(window).Handle;
                UnregisterHotKey(hwnd, id ?? handlerId);
            }
            catch (Exception)
            {
                // ignore
            }
        }

        const int WM_HOTKEY = 0x312;
        public delegate void HotKeyCallBackHandler();
    }
}
