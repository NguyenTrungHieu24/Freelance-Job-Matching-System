using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class EmployerProfile
    {
        [Key]
        public int Id { get; set; }

        public int AccountId { get; set; }

        [Required]
        [StringLength(255)]
        public string CompanyName { get; set; }

        public string Description { get; set; }

        [StringLength(255)]
        public string Logo { get; set; }

        [ForeignKey("AccountId")]
        public User Account { get; set; }
    }
}
