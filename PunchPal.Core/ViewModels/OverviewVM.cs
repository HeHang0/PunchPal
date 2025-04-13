using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Painting;
using PunchPal.Core.Models;
using PunchPal.Tools;
using SkiaSharp;
using System;
using System.Collections.Generic;
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
        private int _standardAverage = 0;
        private int _overtimeAverage = 0;
        private int _holidayAverage = 0;
        private int _monthMinute = 0;
        public Brush DayAverageColor => _dayAverage >= SettingsModel.Load().Data.EveryDayWorkHour * 60 ? Brushes.Green : Brushes.Red;
        public Brush MonthMinuteColor => _monthMinute >= 0 ? Brushes.Green : Brushes.Red;

        public string DayAverageText => (_dayAverage / 60f).ToString("F3").TrimEnd('0').TrimEnd('.');
        public string MonthHourText => (Math.Abs(_monthMinute) / 60f).ToString("F3").TrimEnd('0').TrimEnd('.');
        public string MonthHourUnit => "当月" + (_monthMinute >= 0 ? "贡献" : "欠");
        public string StandardAverageText => $"{_standardAverage / 60f:F3}";
        public string OvertimeAverageText => $"{_overtimeAverage / 60f:F3}";
        public string HolidayAverageText => $"{_holidayAverage / 60f:F3}";
        private ObservableCollection<WorkingHours> workingHours;
        private IEnumerable<WorkingHours> currentWeekWorkingHours;
        private DateTime currentDate;
        public int MonthSpan => IsCurrentMonth ? 1 : 3;
        public int MonthCol => IsCurrentMonth ? 3 : 1;
        public bool IsCurrentMonth => currentDate != null && currentDate.Year == DateTime.Now.Year && currentDate.Month == DateTime.Now.Month;
        public ObservableCollection<string> TipsTextList { get; } = new ObservableCollection<string>();
        public async Task InitItems(DateTime date, ObservableCollection<WorkingHours> items, IEnumerable<WorkingHours> weekWorkingHours)
        {
            currentDate = date;
            workingHours = items;
            currentWeekWorkingHours = weekWorkingHours;
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
            var (start, end) = GetTimeRange(currentDate, true);
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
                    series.Name = _pieNames[_index];
                    series.Fill = _chartsColors[_index++];
                    series.DataLabelsSize = 15;
                    series.DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer;
                    series.DataLabelsPaint = TextPaint;
                    series.DataLabelsFormatter = point => point.Coordinate.PrimaryValue <= 0 ? string.Empty : $"{(point.Coordinate.PrimaryValue / 60).ToString("F3").TrimEnd('0').TrimEnd('.')}小时";
                    series.ToolTipLabelFormatter = point =>
                    $"{(point.Coordinate.PrimaryValue <= 0 ? string.Empty : (point.Coordinate.PrimaryValue / 60).ToString("F3").TrimEnd('0').TrimEnd('.'))}小时" +
                    $"({point.StackedValue.Share:P2})";
                });
            }
            InitWeekCharts();
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
            OnPropertyChanged(nameof(StandardAverageText));
            OnPropertyChanged(nameof(OvertimeAverageText));
            OnPropertyChanged(nameof(HolidayAverageText));
            OnPropertyChanged(nameof(DayAverageColor));
            OnPropertyChanged(nameof(MonthMinuteColor));
            OnPropertyChanged(nameof(MonthHourUnit));
            OnPropertyChanged(nameof(MonthHourText));
            await Task.CompletedTask;
        }

        public SolidColorPaint TooltipTextPaint => new SolidColorPaint
        {
            Color = IsDarkMode ? SKColor.Parse("#000000") : SKColor.Parse("#FFFFFF"),
            FontFamily = SystemFonts.DefaultFont.Name
        };

        private SolidColorPaint TextPaint => new SolidColorPaint
        {
            Color = IsDarkMode ? SKColor.Parse("#FFFFFF") : SKColor.Parse("#000000"),
            FontFamily = SystemFonts.DefaultFont.Name
        };

        private readonly string[] _weekNames = { "日", "一", "二", "三", "四", "五", "六" };
        private void InitWeekCharts()
        {
            ChartWeekStackSeries.Clear();
            ChartWeekStackXAxis.Clear();
            ChartWeekStackYAxis.Clear();
            if (currentDate == null || currentWeekWorkingHours == null)
            {
                return;
            }
            var _chartWeekStackSeries = new ObservableCollection<StackedColumnSeries<int>>();
            var standardHours = new List<double>();
            var overtimeHours = new List<double>();
            List<string> weekNames = new List<string>();
            List<string> weekDates = new List<string>();
            double max = 0;
            for (int i = -7; i < 0; i++)
            {
                var day = DateTime.Now.AddDays(i).Date;
                weekNames.Add(_weekNames[(int)day.DayOfWeek]);
                weekDates.Add(day.ToDateString());
                var work = currentWeekWorkingHours.FirstOrDefault(m => m.WorkingDateTime.Date == day);
                standardHours.Add(Math.Round(((work?.IsHoliday ?? false) ? 0 : work?.StandardMinutes ?? 0) / 60f, 3));
                overtimeHours.Add(Math.Round(((work?.IsHoliday ?? false) ? work?.TotalMinutes ?? 0 : work?.WorkOvertimeMinutes ?? 0) / 60f, 3));
                var total = Math.Ceiling((work?.TotalMinutes ?? 0) / 60f);
                if (total > max)
                {
                    max = total;
                }
            }
            ChartWeekStackXAxis = new ObservableCollection<ICartesianAxis>()
            {
                new Axis
                {
                    Labels = weekNames,
                    LabelsPaint = TextPaint
                }
            };
            ChartWeekStackYAxis = new ObservableCollection<ICartesianAxis>()
            {
                new Axis
                {
                    MinLimit = 0,
                    MaxLimit = max + 2,
                    ShowSeparatorLines = false,
                    LabelsPaint = TextPaint,
                }
            };
            ChartWeekStackSeries = new ObservableCollection<StackedColumnSeries<double>>()
            {
                new StackedColumnSeries<double>
                {
                    Name = "标准工时",
                    Values = standardHours,
                    Stroke = null,
                    DataLabelsFormatter = point => string.Empty,
                    Fill = _chartsColors[0],
                    XToolTipLabelFormatter = point =>
                    {
                        return $"星期{weekNames[point.Index]}({weekDates[point.Index]})";
                    }
                },
                new StackedColumnSeries<double>
                {
                    Name = "加班工时",
                    Values = overtimeHours,
                    Stroke = null,
                    DataLabelsPaint = new SolidColorPaint
                    {
                        Color = IsDarkMode ? SKColor.Parse("#FFFFFF") : SKColor.Parse("#000000"),
                        FontFamily = SystemFonts.DefaultFont.Name
                    },
                    DataLabelsFormatter = point => (standardHours[point.Index] + overtimeHours[point.Index]).ToString(),
                    Fill = _chartsColors[1]
                }
            };
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

        private int _index = 0;
        private readonly SolidColorPaint[] _chartsColors = new SolidColorPaint[] { new SolidColorPaint(new SKColor(0x42, 0x99, 0xE1)), new SolidColorPaint(new SKColor(0x48, 0xBB, 0x78)), new SolidColorPaint(new SKColor(0xED, 0x89, 0x36)) };
        private readonly string[] _pieNames = new string[] { "标准工时", "工作日加班", "节假日加班" };
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
        private ObservableCollection<StackedColumnSeries<double>> _chartWeekStackSeries = new ObservableCollection<StackedColumnSeries<double>>();
        public ObservableCollection<StackedColumnSeries<double>> ChartWeekStackSeries
        {
            get => _chartWeekStackSeries;
            set
            {
                _chartWeekStackSeries = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<ICartesianAxis> _chartWeekStackXAxis = new ObservableCollection<ICartesianAxis>();
        public ObservableCollection<ICartesianAxis> ChartWeekStackXAxis
        {
            get => _chartWeekStackXAxis;
            set
            {
                _chartWeekStackXAxis = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<ICartesianAxis> _chartWeekStackYAxis = new ObservableCollection<ICartesianAxis>();
        public ObservableCollection<ICartesianAxis> ChartWeekStackYAxis
        {
            get => _chartWeekStackYAxis;
            set
            {
                _chartWeekStackYAxis = value;
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
                    item.DataLabelsPaint = TextPaint;
                }
                foreach (var item in ChartWeekStackSeries)
                {
                    item.DataLabelsPaint = TextPaint;
                }
                foreach (var item in ChartWeekStackXAxis)
                {
                    item.NamePaint = TextPaint;
                    item.LabelsPaint = TextPaint;
                }
                foreach (var item in ChartWeekStackYAxis)
                {
                    item.NamePaint = TextPaint;
                    item.LabelsPaint = TextPaint;
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(TooltipTextPaint));
            }
        }
    }
}
