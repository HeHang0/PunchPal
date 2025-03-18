using Newtonsoft.Json;
using PunchPal.Core.Models;
using PunchPal.Core.Services;
using PunchPal.Startup;
using PunchPal.Tools;
using System;
using System.Collections.Generic;
using ModifierKeys = PunchPal.Core.Models.ModifierKeys;

namespace PunchPal.Core.ViewModels
{
    public class SettingsCommon : NotifyPropertyBase
    {
        [JsonIgnore] public static List<int> HoursList => DateTimeTools.HoursList;

        [JsonIgnore] public static List<int> MinutesList => DateTimeTools.MinutesList;

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

        private bool _isShowHoursOnMain = true;

        public bool IsShowHoursOnMain
        {
            get => _isShowHoursOnMain;
            set
            {
                _isShowHoursOnMain = value;
                OnPropertyChanged();
            }
        }

        private bool _isNotifyLockPunch;

        public bool IsNotifyLockPunch
        {
            get => _isNotifyLockPunch;
            set
            {
                _isNotifyLockPunch = value;
                OnPropertyChanged();
            }
        }

        private bool _isNotifyUnLockPunch;

        public bool IsNotifyUnLockPunch
        {
            get => _isNotifyUnLockPunch;
            set
            {
                _isNotifyUnLockPunch = value;
                OnPropertyChanged();
            }
        }

        private bool _isNotifyStartPunch;

        public bool IsNotifyStartPunch
        {
            get => _isNotifyStartPunch;
            set
            {
                _isNotifyStartPunch = value;
                OnPropertyChanged();
            }
        }

        private int _startPunchHour = 8;

        public int StartPunchHour
        {
            get => _startPunchHour;
            set
            {
                if (value >= 0 && value < 24)
                {
                    _startPunchHour = value;
                }
                OnPropertyChanged();
            }
        }

        private int _startPunchMinute = 58;

        public int StartPunchMinute
        {
            get => _startPunchMinute;
            set
            {
                if (value >= 0 && value < 60)
                {
                    _startPunchMinute = value;
                }
                OnPropertyChanged();
            }
        }

        private bool _isNotifyEndPunch;

        public bool IsNotifyEndPunch
        {
            get => _isNotifyEndPunch;
            set
            {
                _isNotifyEndPunch = value;
                OnPropertyChanged();
            }
        }

        private int _endPunchHour = 17;

        public int EndPunchHour
        {
            get => _endPunchHour;
            set
            {
                if (value >= 0 && value < 24)
                {
                    _endPunchHour = value;
                }
                OnPropertyChanged();
            }
        }

        private int _endPunchMinute = 30;

        public int EndPunchMinute
        {
            get => _endPunchMinute;
            set
            {
                if (value >= 0 && value < 60)
                {
                    _endPunchMinute = value;
                }
                OnPropertyChanged();
            }
        }

        public string ShortcutText
        {
            get
            {
                if (ShortcutModifierKeys == ModifierKeys.None || ShortcutKey == Key.None)
                {
                    return string.Empty;
                }

                return ModifierKeysString() + "+" + Enum.GetName(typeof(Key), ShortcutKey);
            }
        }

        public ModifierKeys ShortcutModifierKeys { get; set; } = ModifierKeys.Control | ModifierKeys.Alt;
        public Key ShortcutKey { get; set; } = Key.P;

        public void SetShortcutKey(ModifierKeys modifierKeys, Key key)
        {
            ShortcutModifierKeys = modifierKeys;
            ShortcutKey = key;
            OnPropertyChanged(nameof(ShortcutText));
        }

        private string ModifierKeysString()
        {
            var result = new List<string>();
            if ((uint)(ShortcutModifierKeys & ModifierKeys.Control) > 0)
            {
                result.Add("Ctrl");
            }

            if ((uint)(ShortcutModifierKeys & ModifierKeys.Windows) > 0)
            {
                result.Add("Win");
            }

            if ((uint)(ShortcutModifierKeys & ModifierKeys.Alt) > 0)
            {
                result.Add("Alt");
            }

            if ((uint)(ShortcutModifierKeys & ModifierKeys.Shift) > 0)
            {
                result.Add("Shift");
            }

            return string.Join("+", result);
        }

        public void OnShortcutChanged(ModifierKeys modifierKeys, Key key)
        {
            if (!(modifierKeys == ModifierKeys.None && key == Key.None) && !IsShortcutKeyOk(key))
            {
                return;
            }

            ShortcutModifierKeys = modifierKeys;
            ShortcutKey = key;
            OnPropertyChanged(nameof(ShortcutText));
        }

        private static bool IsShortcutKeyOk(Key key)
        {
            var num = (int)key;
            if (num >= 74 && num <= 83)
            {
                return true;
            }

            if (num >= 34 && num <= 69)
            {
                return true;
            }

            return num >= 90 && num <= 113;
        }
    }
}
