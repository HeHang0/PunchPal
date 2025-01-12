using PunchPal.Tools;
using System;

namespace PunchPal.Core.Models
{
    public class WorkingHours
    {
        public long WorkingDate { get; set; }
        public DateTime WorkingDateTime => WorkingDate.Unix2DateTime();
        public string WorkingDateTimeText => WorkingDateTime.ToDateString();
        public int TotalMinutes { get; set; }
        public int TotalRealMinutes { get; set; }
        public string AttendanceText { get; set; }
        public long StartTime { get; set; }
        public long EndTime { get; set; }
        public string StartTimeText => StartTime.Unix2DateTime().ToDateTimeString();
        public string EndTimeText => EndTime.Unix2DateTime().ToDateTimeString();
        public string TotalHoursText => $"{((double)TotalMinutes / 60):F3}\t({TotalMinutes})";
        public string Remark { get; set; } = string.Empty;
        public bool IsHoliday { get; set; } = false;
        public bool IsToday { get; set; } = false;

        public string ExportText => $"{WorkingDateTimeText}\t{StartTimeText}\t{EndTimeText}\t\"{TotalHoursText}\"\t\"{Remark}\"";
    }
}
