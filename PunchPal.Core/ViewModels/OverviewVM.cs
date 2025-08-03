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
                UpdateData();
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
                UpdateData();
            }
        }

        private void UpdateData()
        {
            OnPropertyChanged(nameof(IsMonthSelected));
            OnPropertyChanged(nameof(IsWeeklySelected));
            OnPropertyChanged(nameof(MonthFlag));
            OnPropertyChanged(nameof(MonthWeekFlag));
            OnPropertyChanged(nameof(MonthOverviewItem));
            OnPropertyChanged(nameof(CurrentOverviewItem));
        }

        public int StandardHours => SettingsModel.Load().Data.EveryDayWorkHour;

        public OverviewItem MonthOverviewItem { get; private set; } = new OverviewItem();
        private OverviewItem _weekOverviewItem = new OverviewItem();
        public OverviewItem CurrentOverviewItem => IsMonthSelected ? MonthOverviewItem : _weekOverviewItem;
        public string MonthFlag => IsCurrentMonth ? "本月" : "当月";
        public string MonthWeekFlag => IsMonthSelected ? MonthFlag : (SettingsModel.Load().Data.IsWeekRecent ? "近一周" : "本周");
        private List<WorkingHours> workingHours;
        private DateTime currentDate;
        public int MonthSpan => IsCurrentMonth ? 1 : 3;
        public int MonthCol => IsCurrentMonth ? 3 : 1;
        public bool IsCurrentMonth => currentDate != null && currentDate.Year == DateTime.Now.Year && currentDate.Month == DateTime.Now.Month;
        public async Task InitItems(DateTime date, List<WorkingHours> items)
        {
            currentDate = date;
            workingHours = items;
            await InitData();
            OnPropertyChanged(nameof(MonthSpan));
            OnPropertyChanged(nameof(MonthCol));
            OnPropertyChanged(nameof(IsCurrentMonth));
        }

        private OverviewItem GetData(DateTime start, DateTime end)
        {
            if (currentDate == null || workingHours == null)
            {
                return null;
            }
            var settings = SettingsModel.Load();
            var startUnix = start.TimestampUnix();
            var endUnix = end.TimestampUnix();
            var dayHours = settings.Data.EveryDayWorkHour;
            //var cmHours = settings.Data.IsWeekendTime ? workingHours.Where(m => !m.IsToday) : workingHours.Where(m => !m.IsToday && !m.IsHoliday);
            var allCurrentWorkHours = workingHours.Where(m => !m.IsToday && m.WorkingDate >= startUnix && m.WorkingDate <= endUnix);
            var currentWorkHours = (settings.Data.IsWeekendTime ? allCurrentWorkHours : allCurrentWorkHours.Where(m => !m.IsHoliday)).ToList();
            // 总加班时长
            var monthMinute = currentWorkHours.Select(m => m.TotalMinutes - dayHours * 60).Sum();
            // 小于标准工作时长的天数
            var adequateDays = currentWorkHours.Count(m => m.TotalMinutes < dayHours * 60);
            // 当前工作日的标准工作时长
            var standardMinute = currentWorkHours.Sum(m => m.StandardMinutes);
            // 当前工作日的加班时长
            var overtimeMinute = currentWorkHours.Sum(m => m.WorkOvertimeMinutes);
            // 当前工作日的平均标准工作时长
            var standardAverage = currentWorkHours.Count == 0 ? 0 : standardMinute / currentWorkHours.Count;
            // 当前工作日的平均加班时长
            var overtimeAverage = currentWorkHours.Count == 0 ? 0 : overtimeMinute / currentWorkHours.Count;
            // 当前工作日的平均工作时长
            var dayAverage = currentWorkHours.Count == 0 ? 0 : currentWorkHours.Sum(m => m.TotalMinutes) / currentWorkHours.Count;
            var holidayList = allCurrentWorkHours.Where(m => m.IsHoliday).ToList();
            var holidayMinute = holidayList.Sum(m => m.TotalMinutes);
            var holidayAverage = holidayList.Count == 0 ? 0 : holidayMinute / holidayList.Count;

            var extraMinute = dayAverage - dayHours * 60;
            var tipsTextList = new List<string>();
            if (extraMinute > 60)
            {
                tipsTextList.Add($"日均工作时长超过标准工作时间{(extraMinute / 60f).ToString("F1").TrimEnd('0').TrimEnd('.')}小时，请合理安排工作时间");
            }
            if (overtimeAverage > 90)
            {
                tipsTextList.Add($"日均加班时长超过1.5小时，建议保证充足休息时间");
            }
            if (holidayList.Count > 1)
            {
                tipsTextList.Add($"周末加班过多，建议关注工作与生活的平衡");
            }
            if (dayAverage >= 10 * 60)
            {
                tipsTextList.Add($"当前工作强度过高，建议本周减少加班并安排至少 1 天完整休息时间");
            }
            if (tipsTextList.Count == 0)
            {
                if (dayAverage < (dayHours * 60))
                {
                    tipsTextList.Add($"当前工作强度轻松，非常不错");
                }
                else
                {
                    tipsTextList.Add($"当前工作强度正常，建议保持");
                }
            }

            return new OverviewItem(monthMinute, adequateDays, standardMinute, overtimeMinute, standardAverage, overtimeAverage, dayAverage, holidayMinute, holidayAverage, tipsTextList);
        }

        private void InitMonthData()
        {
            var settings = SettingsModel.Load();
            var monthRange = DateTimeTools.GetTimeRange(currentDate, true, false);
            var data = GetData(monthRange[0], monthRange[1]);
            _index = 0;
            ChartSeries.Clear();
            MonthOverviewItem = data ?? new OverviewItem();
            if (data == null)
            {
                return;
            }

            if (data.StandardMinute != 0 || data.OvertimeMinute != 0 || data.HolidayMinute != 0)
            {
                ChartSeries = new[] { data.StandardMinute, data.OvertimeMinute, data.HolidayMinute }.AsPieSeries((value, series) =>
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
        }

        private void InitWeekData()
        {
            var settings = SettingsModel.Load();
            var timeRange = DateTimeTools.GetTimeRange(currentDate, false, settings.Data.IsWeekRecent);
            var data = GetData(timeRange[0], timeRange[1]);
            ChartWeekStackSeries.Clear();
            ChartWeekStackXAxis.Clear();
            ChartWeekStackYAxis.Clear();
            var _chartWeekStackSeries = new ObservableCollection<StackedColumnSeries<int>>();
            var standardHours = new List<double>();
            var overtimeHours = new List<double>();
            List<string> weekNames = new List<string>();
            List<string> weekDates = new List<string>();
            double max = 0;
            var start = timeRange[0];
            var end = timeRange[1];
            for (; start <= end; start = start.AddDays(1))
            {
                var day = start.Date;
                weekNames.Add(_weekNames[(int)day.DayOfWeek]);
                weekDates.Add(day.ToDateString());
                var work = workingHours.FirstOrDefault(m => m.WorkingDateTime.Date == day);
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
            _weekOverviewItem = data ?? new OverviewItem();
        }

        private async Task InitData()
        {
            if (currentDate == null || workingHours == null)
            {
                return;
            }
            var settings = SettingsModel.Load();
            var timeRange = DateTimeTools.GetTimeRange(currentDate, !_isWeeklySelected, settings.Data.IsWeekRecent);
            var start = timeRange[0];
            var end = timeRange[1];
            var startUnix = start.TimestampUnix();
            var endUnix = end.TimestampUnix();
            var cmHours = settings.Data.IsWeekendTime ? workingHours.Where(m => !m.IsToday) : workingHours.Where(m => !m.IsToday && !m.IsHoliday);
            
            InitMonthData();
            InitWeekData();
            UpdateData();
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
