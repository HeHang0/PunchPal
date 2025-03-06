using PunchPal.Core.Models;
using PunchPal.Core.Services;
using PunchPal.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PunchPal.Core.ViewModels
{
    public class CalendarItem : NotifyPropertyBase
    {
        public static bool IsDarkMode { get; set; }

        private readonly bool _weak = false;
        public DateTime Date { get; private set; }
        public string DateText => Date.ToString("yyyy年MM月dd日");
        public string Day => Date.Day.ToString() ?? string.Empty;
        public float Opacity { get; set; } = 1;
        public bool IsLast { get; set; }
        public bool IsNext { get; set; }
        public int WorkMinutes => _workItem?.TotalMinutes ?? 0;
        private WorkingHours _workItem;
        public WorkingHours WorkItem
        {
            get => _workItem;
            set
            {
                _workItem = value;
            }
        }

        public string TimeText
        {
            get
            {
                var startDateTime = _workItem?.StartTime.Unix2DateTime();
                var endDateTime = _workItem?.EndTime.Unix2DateTime();
                var startText = startDateTime?.ToString("HH:mm:ss");
                var endText = endDateTime?.ToString("HH:mm:ss");
                var extText = startDateTime?.ToDateString() == endDateTime?.ToDateString() ? string.Empty : "(次日)";
                if (!string.IsNullOrWhiteSpace(startText) && !string.IsNullOrWhiteSpace(endText))
                {
                    return $"{startText} - {endText}{extText}";
                }

                return null;
            }
        }
        public bool Is996 => BackgroundOpacity == 1;
        public string WorkHoursText => WorkMinutes <= 0 || _weak ? string.Empty : $"{(WorkMinutes * 1.0f / 60).ToString("F3").TrimEnd('0').TrimEnd('.')}(小时)";
        public float WorkHoursTextOpacity => WorkMinutes < (SettingsModel.Load().Data.EveryDayWorkHour * 60) ? 0.9f : 0.5f;
        public bool IsHoliday { get; set; }
        public bool IsWorkday { get; set; }
        public Brush DayColor
        {
            get
            {
                //if (_weak)
                //{
                //    return IsDarkMode ? Brushes.White : Brushes.Black;
                //}
                if (IsHoliday)
                {
                    return Brushes.ForestGreen;
                }
                return !IsHoliday && !IsWorkday && IsWeekends
                    ? Brushes.Coral
                    : (IsDarkMode ? Brushes.White : Brushes.Black);
            }
        }

        public bool IsDayDecorations
        {
            get
            {
                var now = DateTime.Now;
                if (Date.Year == now.Year && Date.Month == now.Month && Date.Day == now.Day)
                {
                    return true;
                }

                return false;
            }
        }

        public bool IsWeekends => Date.DayOfWeek == DayOfWeek.Saturday || Date.DayOfWeek == DayOfWeek.Sunday;

        public string HolidayText { get; set; }
        public Rectangle BorderThickness { get; private set; }

        public float BackgroundOpacity
        {
            get
            {
                var standardMinutes = SettingsModel.Load().Data.EveryDayWorkHour * 60;
                var overtimeCoefficient = Math.Round((WorkMinutes - standardMinutes) / 30f);
                switch (overtimeCoefficient)
                {
                    case 1: return 0.2f;
                    case 2: return 0.2f;
                    case 3: return 0.3f;
                    case 4: return 0.3f;
                    case 5: return 0.4f;
                    case 6: return 0.4f;
                    case 7: return 0.5f;
                    case 8: return 0.6f;
                    case 9: return 0.7f;
                    case 10: return 0.8f;
                    default: return overtimeCoefficient > 10 ? 1 : (WorkMinutes >= standardMinutes ? 0.1f : 0);
                }
            }
        }

        public bool IsWorkOvertime { get; set; }

        private List<string> _textList = null;
        public string TextListText => string.Join(" ", TextList);
        public List<string> TextList
        {
            get
            {
                if (_textList != null)
                {
                    return _textList;
                }
                _textList = new List<string>();
                if (CalendarData == null)
                {
                    return _textList;
                }
                var record = CalendarData;
                if (!string.IsNullOrWhiteSpace(record.Remark))
                {
                    foreach (var item in (record.Remark.Split(' ')))
                    {
                        _textList.Add(item.Trim());
                    }
                }
                var lunarSolarTermVisible = SettingsModel.Load().Calendar.LunarSolarTermVisible;
                if (!lunarSolarTermVisible && !IsHoliday)
                {
                    return _textList.Distinct().ToList();
                }
                var festivalEmpty = string.IsNullOrWhiteSpace(record.Festival);
                var solarTermEmpty = string.IsNullOrWhiteSpace(record.SolarTerm);
                if (lunarSolarTermVisible && !solarTermEmpty && record.SolarTerm != record.Festival && !_textList.Contains(record.SolarTerm))
                {
                    foreach (var item in (record.SolarTerm.Split(' ')))
                    {
                        _textList.Add(item.Trim());
                    }
                }
                if (lunarSolarTermVisible && !festivalEmpty && !_textList.Contains(record.Festival))
                {
                    foreach (var item in (record.Festival?.Split(' ')))
                    {
                        _textList.Add(item.Trim());
                    }
                }
                if (!lunarSolarTermVisible && CalendarService.ChineseHolidays.Contains(record.SolarTerm))
                {
                    _textList.Add(record.SolarTerm);
                }
                if (lunarSolarTermVisible && _textList.Count == 0 && record.LunarDate.Length > 0 && record.LunarMonth.Length > 0)
                {
                    if (record.LunarDate == "初一" && !string.IsNullOrWhiteSpace(record.LunarMonth))
                    {
                        _textList.Add(record.LunarMonth + "月");
                    }
                    else
                    {
                        _textList.Add(record.LunarDate);
                    }
                }
                return _textList.Distinct().ToList();
            }
        }

        public string TextListToolTip
        {
            get
            {
                var text = string.Join(Environment.NewLine, TextList);
                if (!SettingsModel.Load().Calendar.LunarSolarTermVisible ||
                    _calendarData == null ||
                    string.IsNullOrWhiteSpace(_calendarData.LunarYear) ||
                    string.IsNullOrWhiteSpace(_calendarData.LunarMonth) ||
                    string.IsNullOrWhiteSpace(_calendarData.LunarDate))
                {
                    return text;
                }
                var lunarDate = _calendarData == null ? string.Empty : $"{_calendarData.LunarYear}年{_calendarData.LunarMonth}月{_calendarData.LunarDate}";
                if (lunarDate.EndsWith(text))
                {
                    return lunarDate;
                }
                return string.Join(Environment.NewLine, TextList) + Environment.NewLine + lunarDate;
            }
        }

        private CalendarRecord _calendarData;
        public CalendarRecord CalendarData
        {
            get => _calendarData;
            set
            {
                _calendarData = value;
                if (_calendarData != null)
                {
                    IsHoliday = _calendarData.IsHoliday;
                    IsWorkday = _calendarData.IsWorkday;
                    IsWorkOvertime = !_calendarData.IsWorkday && (_calendarData.IsHoliday || IsWeekends) && WorkMinutes > 0;
                }
            }
        }

        public CalendarItem(DateTime date, bool weak = false)
        {
            Date = date;
            Opacity = weak ? 0.5f : 1f;
            _weak = weak;
        }

        public void SetRow(int row)
        {
            var firstCol = Date.DayOfWeek == SettingsModel.Load().Calendar.WeekStart;
            BorderThickness = new Rectangle(
                firstCol ? 1 : 0,
                row == 1 ? 1 : 0,
                1,
                1);
        }

        public void UpdateDayColor()
        {
            OnPropertyChanged(nameof(DayColor));
        }
    }
}
