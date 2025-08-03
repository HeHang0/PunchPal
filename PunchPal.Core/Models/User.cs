using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PunchPal.Core.Models
{
    public class User
    {
        [Key] public string UserId { get; set; }
        public string Name { get; set; }
        [Column("avator")]public string Avatar { get; set; }
        public string Password { get; set; }
        public string Remark { get; set; }
    }
}
