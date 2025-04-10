using Newtonsoft.Json;
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
        private bool _otherFestivalVisible = true;
        public bool OtherFestivalVisible
        {
            get => _otherFestivalVisible;
            set
            {
                _otherFestivalVisible = value;
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

        private bool _isTranslucentBackground = false;
        public bool IsTranslucentBackground
        {
            get => _isTranslucentBackground;
            set
            {
                _isTranslucentBackground = value;
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

        public Dictionary<string, string> GetSchedule(DateTime date, List<CalendarItem> calendars)
        {
            var result = new Dictionary<string, string>();
            var dayInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            foreach (var item in MonthScheduleList)
            {
                if (string.IsNullOrWhiteSpace(item.Remark))
                {
                    continue;
                }
                if (item.Day <= 0 || item.Day > 31)
                {
                    continue;
                }
                var itemDay = item.Day > dayInMonth ? dayInMonth : item.Day;
                var day = new DateTime(date.Year, date.Month, itemDay);
                if (!item.MoveUpWhenWeekend)
                {
                    result[day.ToDateString()] = item.Remark;
                    continue;
                }
                var canNext = false;
                for (var i = calendars.Count - 1; i >= 0; i--)
                {
                    if (calendars[i].Date == day)
                    {
                        if (IsCalendarWorkday(calendars[i].CalendarData))
                        {
                            break;
                        }
                        canNext = true;
                        continue;
                    }
                    if (canNext && IsCalendarWorkday(calendars[i].CalendarData))
                    {
                        day = calendars[i].Date;
                        break;
                    }
                }
                result[day.ToDateString()] = item.Remark;
            }
            return result;
        }

        private bool IsCalendarWorkday(Models.CalendarRecord calendarRecord)
        {
            if (calendarRecord == null)
            {
                return false;
            }
            return calendarRecord.IsWorkday || !(calendarRecord.IsWeekend || calendarRecord.IsHoliday);
        }

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

        [JsonIgnore] public List<KeyValuePair<DayOfWeek, string>> WeekList => _weekList;
        private readonly List<KeyValuePair<DayOfWeek, string>> _weekList = new List<KeyValuePair<DayOfWeek, string>>
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
