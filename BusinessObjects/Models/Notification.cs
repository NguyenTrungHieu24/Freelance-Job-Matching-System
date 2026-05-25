using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public int AccountId { get; set; }

        public string Content { get; set; }

        public bool IsRead { get; set; } = false;
        public bool IsMail { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("AccountId")]
        public User Account { get; set; }
    }
}
