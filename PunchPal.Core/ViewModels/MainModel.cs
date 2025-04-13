using PunchPal.Core.Events;
using PunchPal.Core.Models;
using PunchPal.Core.Services;
using PunchPal.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Timer = System.Threading.Timer;

namespace PunchPal.Core.ViewModels
{
    public class MainModel : NotifyPropertyBase
    {
        public event EventHandler AddRecord;
        private readonly SynchronizationContext uiContext = SynchronizationContext.Current;
        private Timer _timer;
        private bool _isTimerRunning;

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
            Setting.About.SetUIContext(uiContext);
            InitAutoAddRecord();
            Setting.Calendar.PropertyChanged += OnPropertyChanged;
            InitTimer();
            Calendar.NextOrLastClick += OnNextOrLastClick;
        }

        private void OnNextOrLastClick(object sender, bool isNext)
        {
            if (isNext)
            {
                OnNextMonth();
            }
            else
            {
                OnLastMonth();
            }
        }

        private void InitTimer()
        {
            _timer = new Timer(OnTimer, null, 0, 1000);
        }

        public string CountdownOffWork
        {
            get
            {
                var now = DateTime.Now;
                var work = Setting.WorkingTimeRange.CurrentItems;
                if (work == null)
                {
                    return string.Empty;
                }
                var offWork = new DateTime(now.Year, now.Month, now.Day, work.Work.EndHour, work.Work.EndMinute, 0);
                var diff = (offWork - now);
                if (diff.TotalSeconds <= 0)
                {
                    return "下班啦";
                }
                return "下班倒计时：" + diff.ToString("hh\\:mm\\:ss");
            }
        }

        private void OnTimer(object state)
        {
            uiContext.Post(_ =>
            {
                OnPropertyChanged(nameof(CountdownOffWork));
            }, null);
            if (DateTime.Now.Second != 0)
            {
                return;
            }
            var now = DateTime.Now;
            if (Setting.Common.IsNotifyEndPunch)
            {
                if (Setting.Common.EndPunchHour == now.Hour && Setting.Common.EndPunchMinute == now.Minute)
                {
                    NotifyEndPunch();
                }
            }

            if (Setting.Common.IsNotifyStartPunch)
            {
                if (now.Hour == Setting.Common.StartPunchHour && now.Minute == Setting.Common.StartPunchMinute)
                {
                    NotifyStartPunch();
                }
            }

            if (Setting.Data.DayStartHour == now.Hour && now.Minute == 0)
            {
                uiContext.Post(_ =>
                {
                    TodayNotifyStartPunch = false;
                }, null);
            }

            if (!_isTimerRunning && Setting.Data.IsRefreshData &&
                Setting.Data.RefreshDataHour == now.Hour &&
                Setting.Data.RefreshDataMinute == now.Minute)
            {
                RunSyncData();
            }
        }

        public async void RunSyncData()
        {
            _isTimerRunning = true;
            uiContext.Post(_ =>
            {
                DataSourceLoading = true;
            }, null);
            var ok = await Setting.DataSource.SyncData(Date);
            _isTimerRunning = false;
            uiContext.Post(_ =>
            {
                DataSourceLoading = false;
                if (ok) InitItems();
            }, null);
        }

        public bool TodayNotifyStartPunch { get; set; } = false;

        public async void NotifyEndPunch()
        {
            var record = await PunchRecordService.Instance.TodayFirst(SettingsModel.Load().Data.DayStartHour);
            if (record != null)
            {
                EventManager.ShowNotification(new EventManager.NotificationOption("记得下班打卡哦！！！", true));
            }
        }

        public async void NotifyStartPunch()
        {
            var record = await PunchRecordService.Instance.TodayFirst(SettingsModel.Load().Data.DayStartHour);
            if (record == null)
            {
                EventManager.ShowNotification(new EventManager.NotificationOption("记得上班打卡哦！！！", true));
            }
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

        private bool _dataSourceLoading = false;
        public bool DataSourceLoading
        {
            get => _dataSourceLoading;
            set
            {
                _dataSourceLoading = value;
                OnPropertyChanged();
            }
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
            var (start, end) = Calendar.GetDateTimeRange(Date);
            await PunchRecord.InitItems(Date, start, end);
            await AttendanceRecord.InitItems(Date, start, end);
            await WorkingHours.InitItems(Date, start, end, PunchRecord.ItemsAll, AttendanceRecord.ItemsAll);
            await Calendar.InitItems(Date, WorkingHours.ItemsAll);
            var recentWeekWorkingHours = await GetRecentWeekWorkingHours();
            await Overview.InitItems(Date, WorkingHours.Items, recentWeekWorkingHours);
            Loading = false;
        }

        private async Task<IEnumerable<WorkingHours>> GetRecentWeekWorkingHours()
        {
            var now = DateTime.Now;
            if (now.Year != Date.Year || now.Month != Date.Month)
            {
                return null;
            }
            var start = now.AddDays(-6).Date;
            var end = now.Date;
            var punchRecords = await PunchRecord.GetRecords(start, end);
            var attendanceRecords = await AttendanceRecord.GetRecords(start, end);
            return await WorkingHours.GetRecords(start, end, punchRecords, attendanceRecords);
        }

        public ICommand CurrentMonthCommand => new ActionCommand(OnCurrentMonth);
        public ICommand LastMonthCommand => new ActionCommand(OnLastMonth);
        public ICommand NextMonthCommand => new ActionCommand(OnNextMonth);
        public ICommand AddRecordCommand => new ActionCommand(OnAddRecord);
        public ICommand RefreshCommand => new ActionCommand(InitItems);
    }
}
