using System.ComponentModel.DataAnnotations;

namespace PunchPal.Core.Models
{
    public class User
    {
        [Key] public string UserId { get; set; }
        public string Name { get; set; }
        public string Avator { get; set; }
        public string Remark { get; set; }
    }
}
