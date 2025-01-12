using PunchPal.Core.Models;
using PunchPal.Core.Services;
using PunchPal.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PunchPal.Core.ViewModels
{
    public class CalendarVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<CalendarItem> Items { get; } = new ObservableCollection<CalendarItem>();

        private readonly string[] _calendarHeaders = { "一", "二", "三", "四", "五", "六", "日" };
        private readonly string[] _calendarSunHeaders = { "日", "一", "二", "三", "四", "五", "六" };
        public string[] CalendarHeaders => SettingsModel.Load().Common.IsCalendarStartSun ? _calendarSunHeaders : _calendarHeaders;

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

        public bool HolidayCountdownVisible => !string.IsNullOrWhiteSpace(_holidayCountdownText);

        public async Task InitItems(DateTime dateTime, IList<WorkingHours> hours)
        {
            Items.Clear();
            var result = new List<CalendarItem>();
            var sunDayFirst = SettingsModel.Load().Common.IsCalendarStartSun;
            var firstDay = new DateTime(dateTime.Year, dateTime.Month, 1);
            var firstDayWeek = firstDay.DayOfWeek == DayOfWeek.Sunday && !sunDayFirst ? 7 : (int)firstDay.DayOfWeek;
            for (var i = sunDayFirst ? 0 : 1; i < firstDayWeek; i++)
            {
                result.Add(new CalendarItem(firstDay.AddDays(i - firstDayWeek), true));
            }
            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            var days = (lastDay - firstDay).Days + 1;
            var recordMap = GetWorkingHoursMap(hours);
            for (int i = 0; i < days; i++)
            {
                result.Add(new CalendarItem(firstDay.AddDays(i)));
            }

            var day = 0;
            var lastDayWeek = lastDay.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)lastDay.DayOfWeek;
            for (int i = lastDayWeek + 1; i <= (sunDayFirst ? 6 : 7); i++)
            {
                result.Add(new CalendarItem(lastDay.AddDays(++day), true));
            }
            var startValue = result.FirstOrDefault()?.Date.TimestampUnix() ?? 0;
            var endValue = result.LastOrDefault()?.Date.AddDays(1).TimestampUnix() ?? 0;
            var calendars = await CalendarService.Instance.ListOrSync(m => m.Date >= startValue && m.Date < endValue, firstDay);
            var calendarMap = GetCalendarMap(calendars);
            foreach (var item in result)
            {
                var date = item.Date.ToDateString();
                item.WorkItem = recordMap.ContainsKey(date) ? recordMap[date] : null;
                item.CalendarData = calendarMap.ContainsKey(date) ? calendarMap[date] : null;
            }
            await UpdateHolidayCountdown();

            var row = 0;
            foreach (var item in result)
            {
                if (item.Date.DayOfWeek == (sunDayFirst ? DayOfWeek.Sunday : DayOfWeek.Monday))
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
            if(result.Distance > 0)
            {
                var year = result.Record.SolarTerm == "国庆节" ? DateTime.Now.Year.ToString() : result.Record.LunarYear;
                HolidayCountdownText = $"距离 {year}年{result.Record.SolarTerm}假期 还有{result.Distance}天";
            }
            else
            {
                HolidayCountdownText = string.Empty;
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

        private Dictionary<string, CalendarRecord> GetCalendarMap(IList<CalendarRecord> records)
        {
            Dictionary<string, CalendarRecord> recordMap = new Dictionary<string, CalendarRecord>();
            foreach (var item in records)
            {
                recordMap[item.Date.Unix2DateTime().ToDateString()] = item;
            }
            return recordMap;
        }

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
