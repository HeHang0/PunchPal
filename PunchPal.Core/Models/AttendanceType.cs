using System.ComponentModel.DataAnnotations;

namespace PunchPal.Core.Models
{
    public class AttendanceType
    {
        [Key] public string TypeId { get; set; }
        public string TypeName { get; set; }
        public string Remark { get; set; }
    }
}
