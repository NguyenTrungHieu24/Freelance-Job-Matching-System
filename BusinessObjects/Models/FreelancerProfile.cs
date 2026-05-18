using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class FreelancerProfile
    {
        [Key]
        public int Id { get; set; }

        public int AccountId { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        public string Bio { get; set; }

        [StringLength(255)]
        public string ProfilePhoto { get; set; }

        public string CVUrl { get; set; }

        public string PortfolioUrl { get; set; }

        public string PortfolioDescription { get; set; }

        [ForeignKey("AccountId")]
        public Account Account { get; set; }
    }
}
