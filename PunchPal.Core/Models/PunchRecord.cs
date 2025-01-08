using PunchPal.Tools;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PunchPal.Core.Models
{
    public class PunchRecord
    {
        [Key] public long PunchTime { get; set; }
        [Index] public string UserId { get; set; }
        public string PunchType { get; set; }
        public string Remark { get; set; }
        [NotMapped] public DateTime PunchDateTime => PunchTime.Unix2DateTime();
        [NotMapped] public string PunchDateTimeText => PunchDateTime.ToDateTimeString();
    }
}
