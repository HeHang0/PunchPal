using PunchPal.Tools;
using System;
using System.Windows;

namespace PunchPal.WPF.Tools
{
    public class NotificationTools
    {
        static NotificationTools()
        {
#if NETFRAMEWORK
            Microsoft.Toolkit.Uwp.Notifications.ToastNotificationManagerCompat.OnActivated += OnToastActivated;
#endif
        }

#if NETFRAMEWORK
        private static void OnToastActivated(Microsoft.Toolkit.Uwp.Notifications.ToastNotificationActivatedEventArgsCompat _)
        {
            WindowTools.ShowWindow(Application.Current.MainWindow);
        }

        public static void ShowToast(string message, bool longDuration = false)
        {
            try
            {
                Microsoft.Toolkit.Uwp.Notifications.ToastNotificationManagerCompat.History.Clear();
                var builder = new Microsoft.Toolkit.Uwp.Notifications.ToastContentBuilder()
                    .AddArgument(NameTools.AppName)
                    .AddText(message);
                builder.SetToastDuration(longDuration
                    ? Microsoft.Toolkit.Uwp.Notifications.ToastDuration.Long
                    : Microsoft.Toolkit.Uwp.Notifications.ToastDuration.Short);
                builder.Show();
            }
            catch (Exception)
            {
                // ignore
            }
        }

        public static void ExitToast()
        {
            Microsoft.Toolkit.Uwp.Notifications.ToastNotificationManagerCompat.History.Clear();
            Microsoft.Toolkit.Uwp.Notifications.ToastNotificationManagerCompat.Uninstall();
        }

        public static void OnNotification(Core.Events.EventManager.NotificationOption e)
        {
            ShowToast(e.Message, e.LongDuration);
        }
#else
        public static void ShowToast(string message, bool longDuration = false) { }
        public static void ExitToast() { }
#endif
    }
}
