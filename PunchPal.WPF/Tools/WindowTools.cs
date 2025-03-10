using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace PunchPal.WPF.Tools
{
    public class WindowTools
    {
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int dwFlags);

        const int SWP_NOSIZE = 0x0001;
        const int SWP_CENTER = 0x0002;
        const int SWP_SHOWWINDOW = 0x0040;

        public static void SetForegroundWindow(Core.Events.EventManager.ForegroundWindowOption option)
        {
            if (option.Handle == IntPtr.Zero)
            {
                return;
            }
            try
            {
                ShowWindow(option.Handle, 1);
                SetForegroundWindow(option.Handle);
                if (option.Center && option.Half)
                {
                    CenterAndHalfWindow(option.Handle);
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }

        public static void CenterAndHalfWindow(IntPtr handle)
        {
            try
            {
                var height = (int)SystemParameters.PrimaryScreenHeight / 2;
                var width = (int)SystemParameters.PrimaryScreenWidth / 2;
                SetWindowPos(handle, IntPtr.Zero,
                    (width - (width % 2)) / 2, (height - (height % 2)) / 2,
                    width, height, SWP_CENTER | SWP_SHOWWINDOW);
            }
            catch (Exception)
            {
                // ignore
            }
        }

        public static void ShowWindow(Window win)
        {
            win.Show();
            win.WindowState = WindowState.Normal;
            win.Activate();
            var handle = new WindowInteropHelper(win).Handle;
            if (handle != IntPtr.Zero)
            {
                SetForegroundWindow(handle);
            }
        }
    }
}
