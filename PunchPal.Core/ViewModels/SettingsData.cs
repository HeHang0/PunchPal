using PunchPal.Tools;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PunchPal.Core.ViewModels
{
    public class SettingsData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static List<int> HoursList => DateTimeTools.HoursList;

        public static List<int> HoursTotalList => DateTimeTools.HourTotalList;

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

        private bool _isIgnoreAttendanceTime = false;
        public bool IsIgnoreAttendanceTime
        {
            get => _isIgnoreAttendanceTime;
            set
            {
                _isIgnoreAttendanceTime = value;
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


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
