using PunchPal.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;

namespace PunchPal.Core.ViewModels
{
    public class CalendarItem: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _weak = false;
        private bool _darkMode = false;
        public DateTime Date { get; private set; }
        public string Day => Date.Day.ToString() ?? string.Empty;
        public float Opacity { get; set; } = 1;
        public int WorkMinutes { get; set; }
        private readonly WorkingHoursModel _workItem;

        public string TimeText
        {
            get
            {
                var startText = _workItem?.StartTime.Unix2DateTime().ToLongTimeString();
                var endText = _workItem?.EndTime.Unix2DateTime().ToLongTimeString();
                if (!string.IsNullOrWhiteSpace(startText) && !string.IsNullOrWhiteSpace(endText))
                {
                    return $"{startText} - {endText}";
                }

                return null;
            }
        }
        public string WorkHoursText => WorkMinutes <= 0 || _weak ? string.Empty : $"{(WorkMinutes * 1.0f / 60).ToString("F3").TrimEnd('0').TrimEnd('.')}(小时)";
        public float WorkHoursTextOpacity => WorkMinutes < (SettingsModel.Load().Data.EveryDayWorkHour * 60) ? 0.9f : 0.5f;
        public bool IsHoliday { get; set; }

        public Brush DayColor
        {
            get
            {
                if (_weak)
                {
                    return _darkMode ? Brushes.White : Brushes.Black;
                }
                if (IsHoliday && !IsWeekends)
                {
                    return Brushes.ForestGreen;
                }

                return !IsHoliday && IsWeekends
                    ? Brushes.Coral
                    : (_darkMode ? Brushes.White : Brushes.Black);
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

        private bool IsWeekends => Date.DayOfWeek == DayOfWeek.Saturday || Date.DayOfWeek == DayOfWeek.Sunday;

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

        public List<string> TextList { get; set; } = new List<string>();

        public CalendarItem(DateTime date, bool weak=false)
        {
            Date = date;
            Opacity = weak ? 0.5f : 1f;
            _weak = weak;
        }

        public void SetRow(int row)
        {
            var firstCol = Date.DayOfWeek == (SettingsModel.Load().Common.IsCalendarStartSun ? DayOfWeek.Sunday : DayOfWeek.Monday);
            BorderThickness = new Rectangle(
                firstCol ? 1 : 0,
                row == 1 ? 1 : 0,
                1,
                1);
        }

        public void SetDark(bool dark)
        {
            _darkMode = true;
            OnPropertyChanged(nameof(DayColor));
        }

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
