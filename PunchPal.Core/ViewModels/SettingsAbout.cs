using PunchPal.Tools;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PunchPal.Core.ViewModels
{
    public class SettingsAbout : NotifyPropertyBase
    {
        private readonly bool _canUpdate = false;
        private readonly string _newVersion = string.Empty;
        private bool _isUpdateChecking = false;
        public bool CanUpdate => _canUpdate;
        public string CheckUpdateText => _isUpdateChecking ? "" : (_canUpdate ? $"更新({_newVersion})" : "检查更新");
        public string NewVersionText => (_canUpdate ? $"有新版本({_newVersion})" : "");
        public string AppVersion => $"{NameTools.AppTitle} {NameTools.AppVersion}";

        public bool IsUpdateChecking
        {
            get => _isUpdateChecking;
            private set
            {
                _isUpdateChecking = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CheckUpdateText));
                OnPropertyChanged(nameof(NewVersionText));
                OnPropertyChanged(nameof(CanUpdate));
            }
        }

        public ICommand CheckUpdate => new ActionCommand(OnCheckUpdate);

        public ICommand EmailWithMe => new ActionCommand(OnEmailWithMe);

        private async void OnCheckUpdate()
        {
            IsUpdateChecking = true;
            await Task.Delay(TimeSpan.FromSeconds(1));
            IsUpdateChecking = false;
        }

        private void OnEmailWithMe()
        {
            try
            {
                Process.Start("mailto:hehang724590957@qq.com?subject=" + WebUtility.UrlEncode("关于工时助手"));
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}
