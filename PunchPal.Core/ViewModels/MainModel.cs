using PunchPal.Core.Models;
using PunchPal.Tools;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PunchPal.Core.ViewModels
{
    public class MainModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<TipsOption> Tips;
        public event EventHandler<ConfirmDialogEventArgs> ConfirmDialog;
        public event EventHandler AddRecord;

        public enum PageType
        {
            None,
            PunchRecord,
            WorkingHours,
            Calendar,
            Overview,
            Settings
        }

        public MainModel()
        {
            PunchRecord.ConfirmDialog += (sender, e) => ConfirmDialog?.Invoke(sender, e);
            InitItems();
        }

        public string Title { get; } = "工时助手 - 高效的管理工作";

        public CalendarVM Calendar { get; } = new CalendarVM();

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
                if(_currentPage == value)
                {
                    return;
                }
                _currentPage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPunchRecord));
                OnPropertyChanged(nameof(IsWorkingHours));
                OnPropertyChanged(nameof(IsCalendar));
                OnPropertyChanged(nameof(IsOverview));
                OnPropertyChanged(nameof(IsSettings));
                OnPropertyChanged(nameof(CanAddRecord));
                OnPropertyChanged(nameof(CanYearMonth));
            }
        }
        public bool IsPunchRecord => CurrentPage == PageType.PunchRecord;
        public bool IsWorkingHours => CurrentPage == PageType.WorkingHours;
        public bool IsCalendar => CurrentPage == PageType.Calendar;
        public bool IsOverview => CurrentPage == PageType.Overview;
        public bool IsSettings => CurrentPage == PageType.Settings;
        public bool CanAddRecord => IsPunchRecord;
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
            if(Date.ToString("yyyyMM") == DateTime.Now.ToString("yyyyMM"))
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

        public async void InitItems()
        {
            Loading = true;
            var start = DateTime.Now.TimestampUnix();
            Trace.WriteLine($"Start InitItems: {start}");
            await PunchRecord.InitItems(Date);
            Trace.WriteLine($"PunchRecord InitItems: {DateTime.Now.TimestampUnix() - start}");
            await AttendanceRecord.InitItems(Date);
            Trace.WriteLine($"AttendanceRecord InitItems: {DateTime.Now.TimestampUnix() - start}");
            await WorkingHours.InitItems(PunchRecord.Items);
            Trace.WriteLine($"WorkingHours InitItems: {DateTime.Now.TimestampUnix() - start}");
            await Calendar.InitItems(Date, WorkingHours.Items);
            Trace.WriteLine($"Calendar InitItems: {DateTime.Now.TimestampUnix() - start}");
            Loading = false;
        }

        public ICommand CurrentMonthCommand => new ActionCommand(OnCurrentMonth);
        public ICommand LastMonthCommand => new ActionCommand(OnLastMonth);
        public ICommand NextMonthCommand => new ActionCommand(OnNextMonth);
        public ICommand AddRecordCommand => new ActionCommand(OnAddRecord);
        public ICommand RefreshCommand => new ActionCommand(InitItems);

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
