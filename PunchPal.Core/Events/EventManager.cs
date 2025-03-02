using PunchPal.Core.Models;
using System;
using System.Threading.Tasks;

namespace PunchPal.Core.Events
{
    public class EventManager
    {
        #region ConfirmDialog
        public class ConfirmDialogOption
        {
            public string Title { get; set; }
            public string Message { get; set; }
            public ControlAppearance Appearance { get; set; } = ControlAppearance.Primary;
        }
        private static Func<ConfirmDialogOption, Task<bool>> _confirmDialog = null;
        public static void RegisterConfirmDialog(Func<ConfirmDialogOption, Task<bool>> confirmDialog)
        {
            _confirmDialog = confirmDialog;
        }

        public static Task<bool> ShowConfirmDialog(ConfirmDialogOption option = null)
        {
            return _confirmDialog?.Invoke(option) ?? Task.Run(() => false);
        }
        #endregion

        #region FileDialog
        public class FileDialogOption
        {
            public string Filter { get; set; } = string.Empty;
            public bool Multiselect { get; set; }
        }
        private static Func<FileDialogOption, string[]> _fileDialog = null;
        public static void RegisterFileDialog(Func<FileDialogOption, string[]> fileDialog)
        {
            _fileDialog = fileDialog;
        }

        public static string[] ShowFileDialog(FileDialogOption option = null)
        {
            return _fileDialog?.Invoke(option) ?? new string[] { };
        }
        #endregion

        #region SaveDialog
        public class SaveDialogOption
        {
            public string Title { get; set; }
            public string Filter { get; set; }
            public string DefaultExt { get; set; }
            public string FileName { get; set; }
            public bool AddExtension { get; set; }
        }
        private static Func<SaveDialogOption, string> _saveDialog = null;
        public static void RegisterSaveDialog(Func<SaveDialogOption, string> saveDialog)
        {
            _saveDialog = saveDialog;
        }

        public static string ShowSaveDialog(SaveDialogOption option = null)
        {
            return _saveDialog?.Invoke(option);
        }
        #endregion

        #region Tips
        private static Action<TipsOption> _tips = null;
        public static void RegisterTips(Action<TipsOption> tips)
        {
            _tips = tips;
        }

        public static void ShowTips(TipsOption option = null)
        {
            _tips?.Invoke(option);
        }
        #endregion

        #region Notification
        public class NotificationOption
        {
            public string Message { get; set; }
            public bool LongDuration { get; set; }
            public NotificationOption(string message, bool longDuration = false)
            {
                Message = message;
                LongDuration = longDuration;
            }
        }
        private static Action<NotificationOption> _notification = null;
        public static void RegisterNotification(Action<NotificationOption> notification)
        {
            _notification = notification;
        }

        public static void ShowNotification(NotificationOption option = null)
        {
            _notification?.Invoke(option);
        }
        #endregion
    }
}
