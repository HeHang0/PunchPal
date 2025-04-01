using PunchPal.Core.Models;
using PunchPal.Core.Services;
using PunchPal.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PunchPal.Core.ViewModels
{
    public class CalendarVM : NotifyPropertyBase
    {
        public event EventHandler<bool> NextOrLastClick;
        public ObservableCollection<CalendarItem> Items { get; } = new ObservableCollection<CalendarItem>();

        private readonly string[] _calendarHeaders = { "日", "一", "二", "三", "四", "五", "六" };
        public string[] CalendarHeaders
        {
            get
            {
                var start = (int)SettingsModel.Load().Calendar.WeekStart;
                return _calendarHeaders.Skip(start).Concat(_calendarHeaders.Take(start)).ToArray();
            }
        }

        private string _holidayCountdownText = string.Empty;
        public string HolidayCountdownText
        {
            get => _holidayCountdownText;
            set
            {
                _holidayCountdownText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HolidayCountdownVisible));
            }
        }

        private string _scheduleCountdownText = string.Empty;
        public string ScheduleCountdownText
        {
            get => _scheduleCountdownText;
            set
            {
                _scheduleCountdownText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HolidayCountdownVisible));
            }
        }

        public bool HolidayCountdownVisible => SettingsModel.Load().Calendar.HolidayCountdownVisible && !string.IsNullOrWhiteSpace(_holidayCountdownText);

        public ICommand ToLastOrNextCommand => new RelayCommand<CalendarItem>(ToLastOrNext);

        private void ToLastOrNext(CalendarItem item)
        {
            if (item.IsNextOrLast)
            {
                NextOrLastClick?.Invoke(this, item.IsNext);
            }
        }

        public async Task InitItems(DateTime dateTime, IList<WorkingHours> hours)
        {
            var result = new List<CalendarItem>();
            var settings = SettingsModel.Load();
            var weekStart = settings.Calendar.WeekStart;
            var weekStartIndex = (int)weekStart;
            var firstDay = new DateTime(dateTime.Year, dateTime.Month, 1);
            var firstDayWeek = (int)firstDay.DayOfWeek;
            for (var i = weekStartIndex; i < firstDayWeek; i++)
            {
                result.Add(new CalendarItem(firstDay.AddDays(i - firstDayWeek), true) { IsLast = true });
            }
            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            var days = (lastDay - firstDay).Days + 1;
            var recordMap = GetWorkingHoursMap(hours);
            for (int i = 0; i < days; i++)
            {
                result.Add(new CalendarItem(firstDay.AddDays(i)));
            }

            var day = 0;
            var endNextDays = 7 - result.Count % 7;
            for (int i = 0; endNextDays < 7 && i < endNextDays; i++)
            {
                result.Add(new CalendarItem(lastDay.AddDays(++day), true) { IsNext = true });
            }
            var startValue = result.FirstOrDefault()?.Date.TimestampUnix() ?? 0;
            var endValue = result.LastOrDefault()?.Date.AddDays(1).TimestampUnix() ?? 0;
            List<CalendarRecord> calendars = null;
            await Task.Run(async () =>
            {
                await CalendarService.Instance.Sync(firstDay);
                calendars = await CalendarService.Instance.ListAll(m => m.Date >= startValue && m.Date < endValue);
            });
            var calendarMap = CalendarService.GetCalendarMap(calendars);
            foreach (var item in result)
            {
                var date = item.Date.ToDateString();
                item.WorkItem = recordMap.ContainsKey(date) ? recordMap[date] : null;
                item.CalendarData = calendarMap.ContainsKey(date) ? calendarMap[date] : null;
            }
            var daySchedule = settings.Calendar.GetSchedule(firstDay, result);
            string scheduleDateText = null;
            foreach (var item in result)
            {
                var date = item.Date.ToDateString();
                if (item.Date.Month == dateTime.Month && daySchedule.ContainsKey(date))
                {
                    if (scheduleDateText == null && item.Date > DateTime.Now)
                    {
                        scheduleDateText = date;
                    }
                    item.DaySchedule = daySchedule[date];
                }
            }
            if (scheduleDateText == null)
            {
                ScheduleCountdownText = string.Empty;
            }
            else if (DateTime.TryParse(scheduleDateText, out DateTime scheduleDate))
            {

                var distance = (int)(scheduleDate - DateTime.Now.Date).TotalDays;
                ScheduleCountdownText = distance > 0 ? $"距离 {daySchedule[scheduleDateText]} 还有{distance}天" : string.Empty;
            }
            await UpdateHolidayCountdown();

            var row = 0;
            Items.Clear();
            foreach (var item in result)
            {
                if (item.Date.DayOfWeek == weekStart)
                {
                    row++;
                }
                item.SetRow(row);
                Items.Add(item);
            }
            OnPropertyChanged(nameof(CalendarHeaders));
        }

        public async Task UpdateHolidayCountdown()
        {
            var result = await CalendarService.Instance.GetRecentHolidays();
            if (result.Record == null)
            {
                HolidayCountdownText = string.Empty;
                return;
            }
            var year = result.Record.SolarTerm == "国庆节" ? DateTime.Now.Year.ToString() : result.Record.LunarYear;
            if (result.Distance > 0)
            {
                HolidayCountdownText = $"距离 {year}年{result.Record.SolarTerm}假期 还有{result.Distance}天";
            }
            else
            {
                HolidayCountdownText = $"今天是 {year}年{result.Record.SolarTerm}假期";
            }
        }

        private Dictionary<string, WorkingHours> GetWorkingHoursMap(IList<WorkingHours> records)
        {
            Dictionary<string, WorkingHours> recordMap = new Dictionary<string, WorkingHours>();
            foreach (var item in records)
            {
                recordMap[item.WorkingDateTimeText] = item;
            }
            return recordMap;
        }
    }
}
