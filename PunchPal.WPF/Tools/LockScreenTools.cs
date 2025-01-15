using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace PunchPal.WPF.Tools
{
    public class LockScreenTools
    {
        private const int WTS_SESSION_LOCK = 0x7;  // 锁屏事件
        private const int WTS_SESSION_UNLOCK = 0x8;  // 解锁事件

        private const int NOTIFY_FOR_THIS_SESSION = 0;
        private const int WM_WTS_SESSION_CHANGE = 0x2B1;

        [DllImport("wtsapi32.dll")]
        private static extern bool WTSRegisterSessionNotification(IntPtr hWnd, int dwFlags);

        [DllImport("wtsapi32.dll")]
        private static extern bool WTSUnRegisterSessionNotification(IntPtr hWnd);

        private static readonly Dictionary<IntPtr, Action<bool>> _hooks = new Dictionary<IntPtr, Action<bool>>();

        public static void Register(IntPtr windowIntPtr, Action<bool> hook)
        {
            var hwndSource = HwndSource.FromHwnd(windowIntPtr);
            if (hwndSource == null)
            {
                return;
            }
            hwndSource.AddHook(WndProc);
            _hooks.Add(hwndSource.Handle, hook);
            WTSRegisterSessionNotification(hwndSource.Handle, NOTIFY_FOR_THIS_SESSION);
        }

        private static IntPtr WndProc(IntPtr windowIntPtr, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != WM_WTS_SESSION_CHANGE || !_hooks.TryGetValue(windowIntPtr, out var action)) return IntPtr.Zero;
            switch (wParam.ToInt32())
            {
                case WTS_SESSION_LOCK:
                    action?.Invoke(true);
                    break;
                case WTS_SESSION_UNLOCK:
                    action?.Invoke(false);
                    break;
            }

            return IntPtr.Zero;
        }

        public static void UnRegister(IntPtr windowIntPtr)
        {
            var source = HwndSource.FromHwnd(windowIntPtr);
            if (source == null)
            {
                return;
            }
            WTSUnRegisterSessionNotification(source.Handle);
        }
    }
}
