using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace PunchPal.Core.ViewModels
{
    public class SettingsCalendar : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _lunarSolarTermVisible = true;
        public bool LunarSolarTermVisible
        {
            get => _lunarSolarTermVisible;
            set
            {
                _lunarSolarTermVisible = value;
                OnPropertyChanged();
            }
        }

        private bool _holidayCountdownVisible = true;
        public bool HolidayCountdownVisible
        {
            get => _holidayCountdownVisible;
            set
            {
                _holidayCountdownVisible = value;
                OnPropertyChanged();
            }
        }

        //private bool _isCalendarStartSun = true;
        //public bool IsCalendarStartSun
        //{
        //    get => _isCalendarStartSun;
        //    set
        //    {
        //        _isCalendarStartSun = value;
        //        OnPropertyChanged();
        //    }
        //}

        private DayOfWeek _weekStart = DayOfWeek.Sunday;
        public DayOfWeek WeekStart
        {
            get => _weekStart;
            set
            {
                _weekStart = value;
                OnPropertyChanged();
            }
        }

        public List<KeyValuePair<DayOfWeek, string>> WeekList { get; } = new List<KeyValuePair<DayOfWeek, string>>
        {
            new KeyValuePair<DayOfWeek, string>(DayOfWeek.Sunday, "星期日"),
            new KeyValuePair<DayOfWeek, string>(DayOfWeek.Monday, "星期一"),
            new KeyValuePair<DayOfWeek, string>(DayOfWeek.Tuesday, "星期二"),
            new KeyValuePair<DayOfWeek, string>(DayOfWeek.Wednesday, "星期三"),
            new KeyValuePair<DayOfWeek, string>(DayOfWeek.Thursday, "星期四"),
            new KeyValuePair<DayOfWeek, string>(DayOfWeek.Friday, "星期五"),
            new KeyValuePair<DayOfWeek, string>(DayOfWeek.Saturday, "星期六")
        };

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
