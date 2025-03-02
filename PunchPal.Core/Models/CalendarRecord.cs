using PunchPal.Tools;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PunchPal.Core.Models
{
    public class CalendarRecord
    {
        [Key, Column(Order = 0)] public long Date { get; set; }
        [NotMapped] public DateTime DateTime => Date.Unix2DateTime();
        public string Festival { get; set; }
        public string LunarMonth { get; set; }
        public string LunarDate { get; set; }
        public string LunarYear { get; set; }
        public string SolarTerm { get; set; }
        public bool IsHoliday { get; set; }
        public bool IsWorkday { get; set; }
        [NotMapped]
        public bool IsCustomWeekend { get; set; }
        public string Remark { get; set; }
        [Key, Column(Order = 1)] public CalendarType Type { get; set; } = CalendarType.Baidu;
        [NotMapped]
        public bool IsWeekend
        {
            get
            {
                var date = DateTime;
                return date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday;
            }
        }
    }
}
