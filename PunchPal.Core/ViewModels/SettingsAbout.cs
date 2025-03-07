using AutoUpdate.Core;
using PunchPal.Core.Apis;
using PunchPal.Core.Events;
using PunchPal.Tools;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PunchPal.Core.ViewModels
{
    public class SettingsAbout : NotifyPropertyBase
    {
        private SynchronizationContext _uiContext = null;
        private bool _canUpdate = false;
        private string _newVersion = string.Empty;
        private bool _isUpdateChecking = false;
        public bool CanUpdate => _canUpdate;
        public string CheckUpdateText => _isUpdateChecking ? "" : (_canUpdate ? $"更新({_newVersion})" : "检查更新");
        public string NewVersionText => (_canUpdate ? $"有新版本({_newVersion})" : "");
        public string AppVersion => $"{NameTools.AppTitle} {NameTools.AppVersion}";

        public void SetUIContext(SynchronizationContext uiContext)
        {
            _uiContext = uiContext;
        }

        private bool _isUpdating = false;
        public bool IsUpdating
        {
            get => _isUpdating;
            private set
            {
                _isUpdating = value;
                OnPropertyChanged();
            }
        }

        private int updateProgress = 0;
        public int UpdateProgress
        {
            get => updateProgress;
            private set
            {
                updateProgress = value;
                OnPropertyChanged();
            }
        }

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

        private void OnCheckUpdate()
        {
            if (IsUpdateChecking)
            {
                return;
            }
            if (CanUpdate)
            {
                UpdateNewVersion();
                return;
            }
            RunCheckUpdate();
        }

        private async void RunCheckUpdate()
        {
            IsUpdateChecking = true;
            var (ok, version) = (false, string.Empty);
            await Task.Run(async () =>
            {
                (ok, version) = await Update.GithubChecker.CheckUpdate();
            });
            _uiContext?.Post(_ =>
            {
                _canUpdate = ok;
                if (ok)
                {
                    _newVersion = version;
                }
                else
                {
                    EventManager.ShowTips(new Models.TipsOption("提示", "当前已是最新版本！"));
                }
                IsUpdateChecking = false;
            }, null);
        }

        private void UpdateNewVersion()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            IsUpdateChecking = true;
            IsUpdating = true;
            Update.AutoUpdate?.Update(new SingleInstaller(), cts.Token, new Progress<int>(p =>
            {
                if (p == 100 || p == -1)
                {
                    _uiContext?.Post(_ =>
                    {
                        UpdateProgress = 0;
                        IsUpdateChecking = false;
                        IsUpdating = false;
                    }, null);
                }
                else
                {
                    _uiContext?.Post(_ =>
                    {
                        UpdateProgress = p;
                    }, null);
                }
            }));
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
