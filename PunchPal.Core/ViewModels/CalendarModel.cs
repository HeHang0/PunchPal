using Microsoft.Xaml.Behaviors.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PunchPal.Core.ViewModels
{
    public class CalendarModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public CalendarModel()
        {
            InitItems();
        }

        private DateTime _date = DateTime.Now;
        public DateTime Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MonthText));
            }
        }

        public string MonthText => $"{_date.Year}年{_date.Month}月";

        public ObservableCollection<CalendarItem> Items { get; } = new ObservableCollection<CalendarItem>();

        private readonly string[] _calendarHeaders = { "一", "二", "三", "四", "五", "六", "日" };
        private readonly string[] _calendarSunHeaders = { "日", "一", "二", "三", "四", "五", "六" };
        public string[] CalendarHeaders => SettingsModel.Load().Common.IsCalendarStartSun ? _calendarSunHeaders : _calendarHeaders;

        public ICommand LastMonthCommand => new ActionCommand(OnLastMonth);
        public ICommand NextMonthCommand => new ActionCommand(OnNextMonth);

        private void OnNextMonth()
        {
            Date = Date.AddMonths(1);
            InitItems();
        }

        private void OnLastMonth()
        {
            Date = Date.AddMonths(-1);
            InitItems();
        }

        private void InitItems()
        {
            Items.Clear();
            var result = new List<CalendarItem>();
            var sunDayFirst = SettingsModel.Load().Common.IsCalendarStartSun;
            var firstDay = new DateTime(Date.Year, Date.Month, 1);
            var firstDayWeek = firstDay.DayOfWeek == DayOfWeek.Sunday && !sunDayFirst ? 7 : (int)firstDay.DayOfWeek;
            for (var i = sunDayFirst ? 0 : 1; i < firstDayWeek; i++)
            {
                result.Add(new CalendarItem(firstDay.AddDays(i - firstDayWeek), true));
            }
            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            var days = (lastDay - firstDay).Days + 1;
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

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
