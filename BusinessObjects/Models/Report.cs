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
    public class Report
    {
        [Key]
        public int Id { get; set; }

        public int ReporterId { get; set; }

        [ForeignKey(nameof(ReporterId))]
        public User Reporter { get; set; }

        public int ReportedUserId { get; set; }

        [ForeignKey(nameof(ReportedUserId))]
        public User ReportedUser { get; set; }

        [Required]
        [MaxLength(255)]
        public string Reason { get; set; } = string.Empty;

        public string? Description { get; set; }

        [MaxLength(500)]
        public string? EvidenceUrl { get; set; }

        public ReportStatus Status { get; set; } = ReportStatus.PENDING;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }

        public int? ResolvedBy { get; set; }

        [ForeignKey(nameof(ResolvedBy))]
        public User? Resolver { get; set; }
    }
}
