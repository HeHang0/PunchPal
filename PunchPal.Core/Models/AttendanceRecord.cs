using PunchPal.Tools;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PunchPal.Core.Models
{
    public class AttendanceRecord
    {
        [Key] public string AttendanceId { get; set; }
        public string AttendanceTypeId { get; set; } = string.Empty;
        [NotMapped] public string AttendanceType { get; set; } = string.Empty;
        public string UserId { get; set; }
        public long StartTime { get; set; }
        public long EndTime { get; set; }
        public long AttendanceTime { get; set; }
        public string Remark { get; set; }
        [NotMapped]
        public DateTime? AttendanceDateTime
        {
            get
            {
                if (AttendanceTime == 0) return null;
                return AttendanceTime.Unix2DateTime();
            }
        }
        [NotMapped]
        public DateTime? StartDateTime
        {
            get
            {
                if (StartTime == 0) return null;
                return StartTime.Unix2DateTime();
            }
        }
        [NotMapped]
        public DateTime? EndDateTime
        {
            get
            {
                if (EndTime == 0) return null;
                return EndTime.Unix2DateTime();
            }
        }
        [NotMapped] public string AttendanceDateTimeText => AttendanceDateTime?.ToDateTimeString() ?? string.Empty;
        [NotMapped] public string AttendanceDateText => AttendanceDateTime?.ToDateString() ?? string.Empty;
        [NotMapped] public string StartDateTimeText => StartDateTime?.ToDateTimeString() ?? string.Empty;
        [NotMapped] public string EndDateTimeText => EndDateTime?.ToDateTimeString() ?? string.Empty;
    }
}
