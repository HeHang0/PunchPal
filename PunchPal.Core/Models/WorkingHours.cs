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
        public int LateMinutes { get; set; }
        public int LeaveEarlyMinutes { get; set; }
        public int TotalRealMinutes { get; set; }
        public int StandardMinutes { get; set; }
        public int WorkOvertimeMinutes { get; set; }
        public string AttendanceText { get; set; }
        public long StartTime { get; set; }
        public long EndTime { get; set; }
        public string StartTimeText => StartTime.Unix2DateTime().ToTimeString();
        public string EndTimeText
        {
            get
            {
                var dateTime = EndTime.Unix2DateTime();
                var startDateTime = StartTime.Unix2DateTime();
                var text = dateTime.ToTimeString();
                if (dateTime.Day != startDateTime.Day)
                {
                    text += " (次日)";
                }
                return text;
            }
        }
        public string TotalHoursText => IsToday ? string.Empty : $"{((double)TotalMinutes / 60):F3}\t({TotalMinutes})";
        public string LateMinutesText => LateMinutes > 0 ? $"{LateMinutes}" : string.Empty;
        public string LeaveEarlyMinutesText => !IsToday && LeaveEarlyMinutes > 0 ? $"{LeaveEarlyMinutes}" : string.Empty;
        public string Remark { get; set; } = string.Empty;
        public bool IsHoliday { get; set; } = false;
        public bool IsToday { get; set; } = false;

        public string ExportText => $"{WorkingDateTimeText}\t{StartTimeText}\t{EndTimeText}\t\"{TotalHoursText}\"\t\"{Remark}\"";
    }
}
