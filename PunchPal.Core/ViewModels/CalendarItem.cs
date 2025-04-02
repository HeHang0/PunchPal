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
        public bool IsNextOrLast => IsNext || IsLast;
        public int WorkMinutes => _workItem?.TotalMinutes ?? 0;
        public string DaySchedule { get; set; } = string.Empty;
        public bool IsDaySchedule => !string.IsNullOrWhiteSpace(DaySchedule);
        private WorkingHours _workItem;
        public WorkingHours WorkItem
        {
            get => _workItem;
            set
            {
                _workItem = value;
                _standardMinutes = SettingsModel.Load().Data.EveryDayWorkHour * 60;
                _overtimeCoefficient = (int)Math.Round((WorkMinutes - _standardMinutes) / 30f);
            }
        }

        private int _standardMinutes;
        private int _overtimeCoefficient;

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
                    return $"{startText} - {endText}{extText}\n{WorkHoursToolTips}";
                }

                return null;
            }
        }
        public string WorkHoursText => WorkMinutes <= 0 || _weak ? string.Empty : $"{WorkMinutes * 1.0f / 60:F1}";
        public string WorkHoursToolTips => WorkMinutes <= 0 || _weak ? string.Empty : $"{(WorkMinutes * 1.0f / 60).ToString("F3").TrimEnd('0').TrimEnd('.')}(小时)";
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
                switch (_overtimeCoefficient)
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
                    default: return _overtimeCoefficient > 10 ? 1 : (WorkMinutes >= _standardMinutes ? 0.1f : 0);
                }
            }
        }

        public Brush WorkHoursColor
        {
            get
            {
                if (_overtimeCoefficient <= 0)
                {
                    return new SolidBrush(Color.FromArgb(0x67, 0xC2, 0x3A)); //(IsDarkMode ? Brushes.White : Brushes.Black);
                }
                switch (_overtimeCoefficient)
                {
                    case 1:
                    case 2: return new SolidBrush(Color.FromArgb(0xEE, 0xBE, 0x77));
                    case 3:
                    case 4: return new SolidBrush(Color.FromArgb(0xE6, 0xA2, 0x3C));
                    case 5:
                    case 6: return new SolidBrush(Color.FromArgb(0xB8, 0x82, 0x30));
                    case 7:
                    case 8: return new SolidBrush(Color.FromArgb(0xF8, 0x98, 0x98));
                    case 9:
                    case 10: return new SolidBrush(Color.FromArgb(0xF5, 0x6C, 0x6C));
                    default: return new SolidBrush(Color.FromArgb(0xC4, 0x56, 0x56));
                }
            }
        }

        public string WorkHoursEmoji
        {
            get
            {
                if (WorkMinutes <= 0)
                {
                    return string.Empty;
                }
                if (_overtimeCoefficient <= 0)
                {
                    return "😊";
                }
                switch (_overtimeCoefficient)
                {
                    case 1:
                    case 2: return "😅";
                    case 3:
                    case 4: return "🥲";
                    case 5:
                    case 6: return "😐";
                    case 7:
                    case 8: return "😔";
                    case 9:
                    case 10: return "😠";
                    default: return "👿";
                }
            }
        }

        public bool IsWorkOvertime { get; set; }

        private List<string> _textList = null;
        private readonly List<string> _remarks = new List<string>();
        private string _lunarDate = string.Empty;
        public string TextListText
        {
            get
            {
                List<string> result = new List<string>();
                if (IsDaySchedule)
                {
                    result.Add(DaySchedule);
                }
                var textList = TextList;
                if (_remarks.Count > 0)
                {
                    result.AddRange(_remarks);
                }
                var settings = SettingsModel.Load();
                var lunarSolarTermVisible = settings.Calendar.LunarSolarTermVisible;
                var otherFestivalVisible = settings.Calendar.OtherFestivalVisible;
                if (lunarSolarTermVisible && otherFestivalVisible)
                {
                    result.AddRange(textList);
                }
                else if (lunarSolarTermVisible)
                {
                    result.AddRange(textList.Where(m => CalendarService.ChineseHolidays.Contains(m) || CalendarService.SolarTerms.Contains(m)));
                }
                else if (otherFestivalVisible)
                {
                    result.AddRange(textList.Where(m => !CalendarService.ChineseHolidays.Contains(m) && !CalendarService.SolarTerms.Contains(m)));
                }
                if (lunarSolarTermVisible && result.Count == 0 && !string.IsNullOrWhiteSpace(_lunarDate))
                {
                    result.Add(_lunarDate);
                }
                return string.Join(" ", result.Distinct());
            }
        }
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
                        _remarks.Add(item.Trim());
                    }
                }
                var festivalEmpty = string.IsNullOrWhiteSpace(record.Festival);
                var solarTermEmpty = string.IsNullOrWhiteSpace(record.SolarTerm);
                if (!solarTermEmpty && record.SolarTerm != record.Festival && !_textList.Contains(record.SolarTerm))
                {
                    foreach (var item in (record.SolarTerm.Split(' ')))
                    {
                        _textList.Add(item.Trim());
                    }
                }
                if (!festivalEmpty && !_textList.Contains(record.Festival))
                {
                    foreach (var item in (record.Festival?.Split(' ')))
                    {
                        _textList.Add(item.Trim());
                    }
                }
                if (CalendarService.ChineseHolidays.Contains(record.SolarTerm))
                {
                    _textList.Add(record.SolarTerm);
                }
                if (record.LunarDate == "初一" && !string.IsNullOrWhiteSpace(record.LunarMonth))
                {
                    _lunarDate = record.LunarMonth + "月";
                }
                else
                {
                    _lunarDate = record.LunarDate;
                }
                return _textList.Distinct().ToList();
            }
        }

        public string TextListToolTip
        {
            get
            {
                var result = new List<string>();
                var textList = TextList;
                if (_remarks != null && _remarks.Count > 0)
                {
                    result.AddRange(_remarks);
                }
                result.AddRange(textList);
                var text = string.Join(Environment.NewLine, result.Distinct());
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
                return text + Environment.NewLine + lunarDate;
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
