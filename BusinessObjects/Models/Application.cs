using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class Application
    {
        [Key]
        public int Id { get; set; }

        public int JobId { get; set; }

        public int FreelancerProfileId { get; set; }

        public string CoverLetter { get; set; }

        public string? CvUrl { get; set; }

        public ApplicationStatus Status { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.Now;

        [ForeignKey(nameof(JobId))]
        public Job Job { get; set; }

        [ForeignKey(nameof(FreelancerProfileId))]
        public FreelancerProfile FreelancerProfile { get; set; }
    }
}
