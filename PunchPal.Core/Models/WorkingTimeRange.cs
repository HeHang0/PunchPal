using PunchPal.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PunchPal.Core.Models
{
    public class WorkingTimeRange
    {
        [Key, Column(Order = 0)] public long Date { get; set; }
        [Key, Column(Order = 1)] public WorkingTimeRangeType Type { get; set; }
        public int StartHour { get; set; }
        public int StartMinute { get; set; }
        public int EndHour { get; set; }
        public int EndMinute { get; set; }
        public string UserId { get; set; }
        [NotMapped]
        public DateTime DateTime
        {
            get => Date.Unix2DateTime();
            set
            {
                Date = value.Date.TimestampUnix();
            }
        }
        [NotMapped] public string DateText => Date <= 0 ? "全部" : DateTime.ToDateString();
        [NotMapped] public string StartText => $"{StartHour:D2}:{StartMinute:D2}";
        [NotMapped] public string EndText => $"{EndHour:D2}:{EndMinute:D2}";
        [NotMapped] public string RangeText => $"{StartText} - {EndText}";
        [NotMapped]
        public string TypeText => TypeMappinng.ContainsKey(Type) ? TypeMappinng[Type] : string.Empty;

        [NotMapped]
        public static Dictionary<WorkingTimeRangeType, string> TypeMappinng = new Dictionary<WorkingTimeRangeType, string>()
        {
            { WorkingTimeRangeType.Work, "工作时间" },
            { WorkingTimeRangeType.Lunch, "午餐时间" },
            { WorkingTimeRangeType.Dinner, "晚餐时间" }
        };
    }
}
