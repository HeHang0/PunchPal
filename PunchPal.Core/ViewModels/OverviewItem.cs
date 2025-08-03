using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace PunchPal.Core.ViewModels
{
    public class OverviewItem
    {
        public int AllOvertimeMinute { get; private set; }
        public int AdequateDays { get; private set; }
        public int StandardMinute { get; private set; }
        public int OvertimeMinute { get; private set; }
        public int StandardAverage { get; private set; }
        public int OvertimeAverage { get; private set; }
        public int DayAverage { get; private set; }
        public int HolidayMinute { get; private set; }
        public int HolidayAverage { get; private set; }
        public List<string> TipsTextList { get; private set; }

        public OverviewItem(int allOvertimeMinute, int adequateDays,
            int standardMinute, int overtimeMinute,
            int standardAverage, int overtimeAverage,
            int dayAverage, int holidayMinute,
            int holidayAverage, List<string> tipsTextList)
        {
            AllOvertimeMinute = allOvertimeMinute;
            AdequateDays = adequateDays;
            StandardMinute = standardMinute;
            OvertimeMinute = overtimeMinute;
            StandardAverage = standardAverage;
            OvertimeAverage = overtimeAverage;
            DayAverage = dayAverage;
            HolidayMinute = holidayMinute;
            HolidayAverage = holidayAverage;
            TipsTextList = tipsTextList;
        }

        public OverviewItem()
        {
            TipsTextList = new List<string>();
        }

        public Brush DayAverageColor => DayAverage >= SettingsModel.Load().Data.EveryDayWorkHour * 60 ? Brushes.Green : Brushes.Red;
        public Brush MinuteColor => AllOvertimeMinute >= 0 ? Brushes.Green : Brushes.Red;
        public string DayAverageText => (DayAverage / 60f).ToString("F3").TrimEnd('0').TrimEnd('.');
        public string HourText => (Math.Abs(AllOvertimeMinute) / 60f).ToString("F3").TrimEnd('0').TrimEnd('.');
        public string HourUnit => AllOvertimeMinute >= 0 ? "贡献" : "欠";
        public string StandardAverageText => $"{StandardAverage / 60f:F3}";
        public string OvertimeAverageText => $"{OvertimeAverage / 60f:F3}";
        public string HolidayAverageText => $"{HolidayAverage / 60f:F3}";
        public bool AdequateDaysVisible => AdequateDays > 0;
    }
}
