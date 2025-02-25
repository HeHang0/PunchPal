﻿using PunchPal.Core.Models;
using PunchPal.Core.Services;
using PunchPal.Tools;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace PunchPal.Core.ViewModels
{
    public class MainModel : NotifyPropertyBase
    {
        public event EventHandler AddRecord;
        public event EventHandler ShowWindow;

        public enum PageType
        {
            None,
            PunchRecord,
            AttendanceRecord,
            WorkingHours,
            Calendar,
            Overview,
            Settings
        }

        public MainModel()
        {
            InitAutoAddRecord();
            Setting.Calendar.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SettingsCalendar.WeekStart):
                case nameof(SettingsCalendar.LunarSolarTermVisible):
                    InitItems();
                    break;
                case nameof(SettingsCalendar.HolidayCountdownVisible):
                    Calendar.OnPropertyChanged(nameof(Calendar.HolidayCountdownVisible));
                    break;
            }
        }

        public string Title { get; } = $"{NameTools.AppTitle} - {NameTools.AppSubTitle} {NameTools.AppVersion}";

        public CalendarVM Calendar { get; } = new CalendarVM();

        public OverviewVM Overview { get; } = new OverviewVM();

        public PunchRecordVM PunchRecord { get; } = new PunchRecordVM();

        public WorkingHoursVM WorkingHours { get; } = new WorkingHoursVM();

        public AttendanceRecordVM AttendanceRecord { get; } = new AttendanceRecordVM();

        public SettingsModel Setting => SettingsModel.Load();

        private PageType _currentPage = PageType.PunchRecord;
        public PageType CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage == value)
                {
                    return;
                }
                _currentPage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPunchRecord));
                OnPropertyChanged(nameof(IsAttendanceRecord));
                OnPropertyChanged(nameof(IsWorkingHours));
                OnPropertyChanged(nameof(IsCalendar));
                OnPropertyChanged(nameof(IsOverview));
                OnPropertyChanged(nameof(IsSettings));
                OnPropertyChanged(nameof(CanAddRecord));
                OnPropertyChanged(nameof(CanYearMonth));
            }
        }
        public bool IsPunchRecord => CurrentPage == PageType.PunchRecord;
        public bool IsAttendanceRecord => CurrentPage == PageType.AttendanceRecord;
        public bool IsWorkingHours => CurrentPage == PageType.WorkingHours;
        public bool IsCalendar => CurrentPage == PageType.Calendar;
        public bool IsOverview => CurrentPage == PageType.Overview;
        public bool IsSettings => CurrentPage == PageType.Settings;
        public bool CanAddRecord => IsPunchRecord || IsAttendanceRecord;
        public bool CanYearMonth => !IsSettings;

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

        private bool _loading = false;
        public bool Loading
        {
            get => _loading;
            set
            {
                _loading = value;
                OnPropertyChanged();
            }
        }

        public string MonthText => $"{_date.Year}年{_date.Month}月";
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

        private void OnCurrentMonth()
        {
            if (Date.ToString("yyyyMM") == DateTime.Now.ToString("yyyyMM"))
            {
                return;
            }
            Date = DateTime.Now;
            InitItems();
        }

        private void OnAddRecord()
        {
            AddRecord?.Invoke(this, EventArgs.Empty);
        }

        private async void InitAutoAddRecord()
        {
            var settings = SettingsModel.Load();
            if (!settings.Data.IsAutoAddRecordAtLock)
            {
                InitItems();
                return;
            }
            var currentItem = await PunchRecordService.Instance.TodayFirst(settings.Data.DayStartHour);
            if (currentItem != null)
            {
                InitItems();
                return;
            }
            await PunchRecordService.Instance.Add(new PunchRecord()
            {
                PunchTime = DateTime.Now.TimestampUnix(),
                UserId = settings.Common.CurrentUser?.UserId,
                PunchType = Models.PunchRecord.PunchTypeUnLock
            });
            InitItems();
        }

        public async void InitItems()
        {
            Loading = true;
            var start = DateTime.Now.TimestampUnix();
            await PunchRecord.InitItems(Date);
            await AttendanceRecord.InitItems(Date);
            await WorkingHours.InitItems(PunchRecord.Items);
            await Calendar.InitItems(Date, WorkingHours.Items);
            await Overview.InitItems(Date, WorkingHours.Items);
            Loading = false;
        }

        public ICommand CurrentMonthCommand => new ActionCommand(OnCurrentMonth);
        public ICommand LastMonthCommand => new ActionCommand(OnLastMonth);
        public ICommand NextMonthCommand => new ActionCommand(OnNextMonth);
        public ICommand AddRecordCommand => new ActionCommand(OnAddRecord);
        public ICommand RefreshCommand => new ActionCommand(InitItems);
    }
}
