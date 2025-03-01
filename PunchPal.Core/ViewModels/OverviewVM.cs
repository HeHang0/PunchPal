using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Painting;
using PunchPal.Core.Models;
using PunchPal.Tools;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace PunchPal.Core.ViewModels
{
    public class OverviewVM : NotifyPropertyBase
    {
        private bool _isWeeklySelected = false;
        public bool IsWeeklySelected
        {
            get => _isWeeklySelected;
            set
            {
                _isWeeklySelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsMonthSelected));
                _ = InitData();
            }
        }
        public bool IsMonthSelected
        {
            get => !_isWeeklySelected || !IsCurrentMonth;
            set
            {
                if (!IsCurrentMonth)
                {
                    return;
                }
                _isWeeklySelected = !value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsWeeklySelected));
                _ = InitData();
            }
        }
        public int StandardHours => SettingsModel.Load().Data.EveryDayWorkHour;

        public bool AdequateDaysVisible => _adequateDays > 0;
        public int _adequateDays = 0;
        public int AdequateDays
        {
            get => _adequateDays;
            set
            {
                _adequateDays = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StandardHours));
                OnPropertyChanged(nameof(AdequateDaysVisible));
            }
        }

        private int _dayAverage = 0;
        private int _dayAverageRate = 0;
        private int _standardAverage = 0;
        private int _overtimeAverage = 0;
        private int _holidayAverage = 0;
        private int _monthMinute = 0;
        public Brush DayAverageColor => _dayAverage >= SettingsModel.Load().Data.EveryDayWorkHour * 60 ? Brushes.Green : Brushes.Red;
        public Brush MonthMinuteColor => _monthMinute >= 0 ? Brushes.Green : Brushes.Red;

        public string DayAverageText => (_dayAverage / 60f).ToString("F3").TrimEnd('0').TrimEnd('.');//$"日均{}小时，当月{(_monthMinute >= 0 ? "盈余" : "欠")}{(Math.Abs(_monthMinute) / 60f).ToString("F3").TrimEnd('0').TrimEnd('.')}小时";
        public string MonthHourText => (Math.Abs(_monthMinute) / 60f).ToString("F3").TrimEnd('0').TrimEnd('.');
        public string MonthHourUnit => "当月" + (_monthMinute >= 0 ? "盈余" : "欠");
        public string DayAverageRateText => _dayAverageRate != 0 ? $"较上周{(_dayAverageRate > 0 ? "增加" : "降低")}{Math.Abs(_dayAverageRate)}%" : string.Empty;
        public string StandardAverageText => $"{_standardAverage / 60f:F3}";
        public string OvertimeAverageText => $"{_overtimeAverage / 60f:F3}";
        public string HolidayAverageText => $"{_holidayAverage / 60f:F3}";
        private ObservableCollection<WorkingHours> workingHours;
        private DateTime currentDate;
        public int MonthSpan => IsCurrentMonth ? 1 : 3;
        public int MonthCol => IsCurrentMonth ? 3 : 1;
        public bool IsCurrentMonth => currentDate != null && currentDate.Year == DateTime.Now.Year && currentDate.Month == DateTime.Now.Month;
        public ObservableCollection<string> TipsTextList { get; } = new ObservableCollection<string>();
        public async Task InitItems(DateTime date, ObservableCollection<WorkingHours> items)
        {
            currentDate = date;
            workingHours = items;
            await InitData();
            OnPropertyChanged(nameof(MonthSpan));
            OnPropertyChanged(nameof(MonthCol));
            OnPropertyChanged(nameof(IsCurrentMonth));
            OnPropertyChanged(nameof(IsMonthSelected));
            OnPropertyChanged(nameof(IsWeeklySelected));
        }

        private async Task InitData()
        {
            if (currentDate == null || workingHours == null)
            {
                return;
            }
            var (start, end) = GetTimeRange(currentDate, IsMonthSelected);
            var startUnix = start.TimestampUnix();
            var endUnix = end.TimestampUnix();
            var dayHours = SettingsModel.Load().Data.EveryDayWorkHour;
            _monthMinute = workingHours.Where(m => !m.IsToday && !m.IsHoliday).Select(m => m.TotalMinutes - dayHours * 60).Sum();
            var currentWorkHours = workingHours.Where(m => !m.IsToday && m.WorkingDate >= startUnix && m.WorkingDate <= endUnix).ToList();
            var workDayHours = currentWorkHours.Where(m => !m.IsHoliday).ToList();
            AdequateDays = workDayHours.Count(m => m.TotalMinutes < dayHours * 60);
            var standardMinute = workDayHours.Sum(m => m.StandardMinutes);
            var overtimeMinute = workDayHours.Sum(m => m.WorkOvertimeMinutes);
            _standardAverage = workDayHours.Count == 0 ? 0 : standardMinute / workDayHours.Count;
            _overtimeAverage = workDayHours.Count == 0 ? 0 : overtimeMinute / workDayHours.Count;
            var holidayList = currentWorkHours.Where(m => m.IsHoliday).ToList();
            var holidayMinute = holidayList.Sum(m => m.TotalMinutes);
            _holidayAverage = holidayList.Count == 0 ? 0 : holidayMinute / holidayList.Count;
            _dayAverage = currentWorkHours.Count == 0 ? 0 : currentWorkHours.Sum(m => m.TotalMinutes) / currentWorkHours.Count;
            _index = 0;
            ChartSeries.Clear();
            if (standardMinute != 0 || overtimeMinute != 0 || holidayMinute != 0)
            {
                ChartSeries = new[] { standardMinute, overtimeMinute, holidayMinute }.AsPieSeries((value, series) =>
                {
                    series.InnerRadius = 0;
                    series.Fill = _colors[_index++];
                    series.Name = string.Empty;
                    series.DataLabelsSize = 15;
                    series.DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer;
                    series.DataLabelsPaint = _isDark ? WhitePaint : BlackPaint;
                    series.DataLabelsFormatter = point => point.Coordinate.PrimaryValue <= 0 ? string.Empty : $"{(point.Coordinate.PrimaryValue / 60).ToString("F3").TrimEnd('0').TrimEnd('.')}";
                    series.ToolTipLabelFormatter = point => $"{point.StackedValue.Share:P2}";
                });
            }
            if (!IsMonthSelected)
            {
                var (startLW, endLW) = GetTimeRange(currentDate.AddDays(-7), false);
                var startUnixLW = startLW.TimestampUnix();
                var endUnixLW = endLW.TimestampUnix();
                var currentWorkHoursLW = workingHours.Where(m => !m.IsToday && m.WorkingDate >= startUnixLW && m.WorkingDate <= endUnixLW).ToList();
                var dayAverageLW = currentWorkHoursLW.Count == 0 ? 0 : currentWorkHoursLW.Sum(m => m.TotalMinutes) / currentWorkHoursLW.Count;
                _dayAverageRate = dayAverageLW == 0 ? 0 : (_dayAverage - dayAverageLW) * 100 / dayAverageLW;
            }
            else
            {
                _dayAverageRate = 0;
            }
            TipsTextList.Clear();
            var settings = SettingsModel.Load();
            var extraMinute = _dayAverage - settings.Data.EveryDayWorkHour * 60;
            if (extraMinute > 60)
            {
                TipsTextList.Add($"日均工作时长超过标准工作时间{(extraMinute / 60f).ToString("F1").TrimEnd('0').TrimEnd('.')}小时，请合理安排工作时间");
            }
            if (_overtimeAverage > 90)
            {
                TipsTextList.Add($"日均加班时长超过1.5小时，建议保证充足休息时间");
            }
            if (holidayList.Count > 1)
            {
                TipsTextList.Add($"周末加班过多，建议关注工作与生活的平衡");
            }
            if (_dayAverage >= 10 * 60)
            {
                TipsTextList.Add($"当前工作强度过高，建议本周减少加班并安排至少 1 天完整休息时间");
            }
            if (TipsTextList.Count == 0 && _dayAverage < ((settings.Data.EveryDayWorkHour + 1) * 60))
            {
                TipsTextList.Add($"当前工作强度正常，建议保持");
            }
            OnPropertyChanged(nameof(DayAverageText));
            OnPropertyChanged(nameof(DayAverageRateText));
            OnPropertyChanged(nameof(StandardAverageText));
            OnPropertyChanged(nameof(OvertimeAverageText));
            OnPropertyChanged(nameof(HolidayAverageText));
            OnPropertyChanged(nameof(DayAverageColor));
            OnPropertyChanged(nameof(MonthMinuteColor));
            OnPropertyChanged(nameof(MonthHourUnit));
            OnPropertyChanged(nameof(MonthHourText));
            await Task.CompletedTask;
        }

        private static (DateTime start, DateTime end) GetTimeRange(DateTime dateTime, bool isMonth)
        {
            if (isMonth)
            {
                // 获取当前月的时间范围
                DateTime start = new DateTime(dateTime.Year, dateTime.Month, 1); // 当月第一天
                DateTime end = start.AddMonths(1).AddTicks(-1);                 // 当月最后一天的最后时刻
                return (start, end);
            }
            else
            {
                // 获取当前周的时间范围（假设一周从周一开始）
                int diffToMonday = dateTime.DayOfWeek == 0 ? 6 : (int)dateTime.DayOfWeek - 1; // 计算距周一的天数
                DateTime start = dateTime.Date.AddDays(-diffToMonday);                             // 本周周一
                DateTime end = start.AddDays(7).AddTicks(-1);                                     // 本周周日的最后时刻
                return (start, end);
            }
        }

        //public SolidColorPaint TooltipBackgroundPaint => new SolidColorPaint(new SKColor(0, 0, 0, 0));
        //public SolidColorPaint TooltipTextPaint => new SolidColorPaint(new SKColor(0, 0, 0, 0));

        public static readonly SolidColorPaint BlackPaint = new SolidColorPaint(new SKColor(0, 0, 0));
        public static readonly SolidColorPaint WhitePaint = new SolidColorPaint(new SKColor(0xFF, 0xFF, 0xFF));
        private int _index = 0;
        private readonly SolidColorPaint[] _colors = new SolidColorPaint[] { new SolidColorPaint(new SKColor(0x42, 0x99, 0xE1)), new SolidColorPaint(new SKColor(0x48, 0xBB, 0x78)), new SolidColorPaint(new SKColor(0xED, 0x89, 0x36)) };
        private ObservableCollection<PieSeries<int>> _chartSeries = new ObservableCollection<PieSeries<int>>();
        public ObservableCollection<PieSeries<int>> ChartSeries
        {
            get => _chartSeries;
            set
            {
                _chartSeries = value;
                OnPropertyChanged();
            }
        }

        private bool _isDark = false;
        public bool IsDarkMode
        {
            get => _isDark;
            set
            {
                _isDark = value;
                foreach (var item in ChartSeries)
                {
                    item.DataLabelsPaint = _isDark ? WhitePaint : BlackPaint;
                }
                OnPropertyChanged();
            }
        }
    }
}
