using Newtonsoft.Json;
using PunchPal.Tools;
using System;
using System.Collections.Generic;

namespace PunchPal.Core.ViewModels
{
    public class SettingsData : NotifyPropertyBase
    {
        [JsonIgnore] public static List<int> HoursList => DateTimeTools.HoursList;
        [JsonIgnore] public static List<int> MinutesList => DateTimeTools.MinutesList;

        [JsonIgnore] public static List<int> HoursTotalList => DateTimeTools.HourTotalList;

        private int _refreshDataHour = DateTime.Now.Hour;
        public int RefreshDataHour
        {
            get => _refreshDataHour;
            set
            {
                _refreshDataHour = value;
                OnPropertyChanged();
            }
        }

        private int _refreshDataMinute = DateTime.Now.Minute - 1 >= 0 ? (DateTime.Now.Minute - 1) : 59;
        public int RefreshDataMinute
        {
            get => _refreshDataMinute;
            set
            {
                _refreshDataMinute = value;
                OnPropertyChanged();
            }
        }

        private bool _isRefreshData = true;
        public bool IsRefreshData
        {
            get => _isRefreshData;
            set
            {
                _isRefreshData = value;
                OnPropertyChanged();
            }
        }

        private int _everyDayWorkHour = 8;
        public int EveryDayWorkHour
        {
            get => _everyDayWorkHour;
            set
            {
                _everyDayWorkHour = value;
                OnPropertyChanged();
            }
        }

        private int _dayStartHour = 6;
        public int DayStartHour
        {
            get => _dayStartHour;
            set
            {
                _dayStartHour = value;
                OnPropertyChanged();
            }
        }

        private bool _isIgnoreBeforeWorkTime = false;
        public bool IsIgnoreBeforeWorkTime
        {
            get => _isIgnoreBeforeWorkTime;
            set
            {
                _isIgnoreBeforeWorkTime = value;
                OnPropertyChanged();
            }
        }

        private bool _isAttendanceTime = true;
        public bool IsAttendanceTime
        {
            get => _isAttendanceTime;
            set
            {
                _isAttendanceTime = value;
                OnPropertyChanged();
            }
        }

        private bool _isWeekendTime = false;
        public bool IsWeekendTime
        {
            get => _isWeekendTime;
            set
            {
                _isWeekendTime = value;
                OnPropertyChanged();
            }
        }

        private bool _isIgnoreDinnerAtHoliday = true;
        public bool IsIgnoreDinnerAtHoliday
        {
            get => _isIgnoreDinnerAtHoliday;
            set
            {
                _isIgnoreDinnerAtHoliday = value;
                OnPropertyChanged();
            }
        }

        private bool _isAutoAddRecordAtLock = false;
        public bool IsAutoAddRecordAtLock
        {
            get => _isAutoAddRecordAtLock;
            set
            {
                _isAutoAddRecordAtLock = value;
                OnPropertyChanged();
            }
        }

        private bool _isWeekRecent = true;
        public bool IsWeekRecent
        {
            get => _isWeekRecent;
            set
            {
                _isWeekRecent = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCurrentWeek));
            }
        }
        public bool IsCurrentWeek
        {
            get => !_isWeekRecent;
            set
            {
                _isWeekRecent = !value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsWeekRecent));
            }
        }
    }
}
