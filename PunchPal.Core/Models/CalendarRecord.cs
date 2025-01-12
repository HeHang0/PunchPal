using PunchPal.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PunchPal.Core.Models
{
    public class CalendarRecord
    {
        [Key] public long Date { get; set; }
        [NotMapped] public DateTime DateTime => Date.Unix2DateTime();
        public string Festival { get; set; }
        public string LunarMonth { get; set; }
        public string LunarDate { get; set; }
        public string LunarYear { get; set; }
        public string SolarTerm { get; set; }
        public bool IsHoliday { get; set; }
        public bool IsWorkday { get; set; }
        public string Remark { get; set; }
        public CalendarType Type { get; set; } = CalendarType.Baidu;
    }
}
