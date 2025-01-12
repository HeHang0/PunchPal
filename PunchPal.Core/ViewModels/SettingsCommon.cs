using Newtonsoft.Json;
using PunchPal.Core.Models;
using PunchPal.Core.Services;
using PunchPal.Startup;
using ShellLink.Flags;
using System;
using System.ComponentModel;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace PunchPal.Core.ViewModels
{
    public class SettingsCommon
    {
        public event PropertyChangedEventHandler PropertyChanged;

        static SettingsCommon()
        {
            _startupManager = new StartupManager("PunchPal", System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        public bool IsStartupEnabled
        {
            get => _startupManager?.IsStartupEnabled() ?? false;
            set
            {
                SetStartup(value);
                OnPropertyChanged(nameof(IsStartupEnabled));
            }
        }

        private static readonly StartupManager _startupManager;
        public static void SetStartup(bool startup)
        {
            if (startup)
            {
                _startupManager?.EnableStartup();
            }
            else
            {

                _startupManager?.DisableStartup();
            }
        }

        private bool _isCalendarStartSun = true;
        public bool IsCalendarStartSun
        {
            get => _isCalendarStartSun;
            set
            {
                _isCalendarStartSun = value;
                OnPropertyChanged();
            }
        }

        private User _currentUser = null;
        public User CurrentUser
        {
            get
            {
                if (_currentUser == null)
                {
                    _currentUser = UserService.Instance.FirstOrDefault();
                }
                return _currentUser;
            }
            set
            {
                _currentUser = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
