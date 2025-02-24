using PunchPal.Tools;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PunchPal.Core.Models
{
    public class PunchRecord
    {
        [Key, Column(Order = 0)] public long PunchTime { get; set; }
        [Key, Column(Order = 1)] public string UserId { get; set; }
        public string PunchType { get; set; }
        public string Remark { get; set; }
        [NotMapped] public DateTime PunchDateTime => PunchTime.Unix2DateTime();
        [NotMapped] public string PunchDateTimeText => PunchDateTime.ToDateTimeString();

        [NotMapped] public static readonly string PunchTypeManual = "手动添加";
        [NotMapped] public static readonly string PunchTypeFile = "文件导入";
        [NotMapped] public static readonly string PunchTypeLock = "锁屏";
        [NotMapped] public static readonly string PunchTypeUnLock = "解锁";
        [NotMapped] public static readonly string PunchTypeImport = "导入";
    }
}
