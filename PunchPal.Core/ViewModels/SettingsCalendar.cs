using PunchPal.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace PunchPal.Core.ViewModels
{
    public class SettingsCalendar : NotifyPropertyBase
    {
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

        private ObservableCollection<MonthSchedule> _monthScheduleList = new ObservableCollection<MonthSchedule>();
        public ObservableCollection<MonthSchedule> MonthScheduleList
        {
            get => _monthScheduleList;
            set
            {
                _monthScheduleList = value;
                OnPropertyChanged();
            }
        }

        public Dictionary<string, string> GetSchedule(DateTime date)
        {
            var result = new Dictionary<string, string>();
            foreach (var item in MonthScheduleList)
            {
                if (string.IsNullOrWhiteSpace(item.Remark))
                {
                    continue;
                }
                var day = new DateTime(date.Year, date.Month, item.Day);
                if (item.MoveUpWhenWeekend && (day.DayOfWeek == DayOfWeek.Sunday || day.DayOfWeek == DayOfWeek.Saturday))
                {
                    day = day.AddDays(day.DayOfWeek == DayOfWeek.Sunday ? -2 : -1);
                }
                result[day.ToDateString()] = item.Remark;
            }
            return result;
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

        public ICommand AddMonthSchedule => new ActionCommand(OnAddMonthSchedule);
        public ICommand RemoveMonthSchedule => new RelayCommand<MonthSchedule>(OnRemoveMonthSchedule);

        private void OnRemoveMonthSchedule(MonthSchedule schedule)
        {
            MonthScheduleList.Remove(schedule);
        }

        private void OnAddMonthSchedule()
        {
            for (int i = 0; i < DateTimeTools.DayList.Count; i++)
            {
                var index = MonthScheduleList.FirstOrDefault(x => x.Day == DateTimeTools.DayList[i]);
                if (index == null)
                {
                    MonthScheduleList.Add(new MonthSchedule { Day = DateTimeTools.DayList[i] });
                    return;
                }
            }
            MonthScheduleList.Add(new MonthSchedule());
        }
    }
}
