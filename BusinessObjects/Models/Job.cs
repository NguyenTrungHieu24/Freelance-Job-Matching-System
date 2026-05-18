using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class Job
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }

        public decimal Budget { get; set; }

        public string Status { get; set; }

        public int EmployerProfileId { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey("EmployerProfileId")]
        public EmployerProfile EmployerProfile { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
    }
}
