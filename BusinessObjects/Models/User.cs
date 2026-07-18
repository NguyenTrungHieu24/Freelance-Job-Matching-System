using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public Role Role { get; set; }

        public FreelancerProfile FreelancerProfile { get; set; }

        public EmployerProfile EmployerProfile { get; set; }

        public ICollection<Notification> Notifications { get; set; }

        public string? PasswordResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }

        public Wallet Wallet { get; set; }
    }
}
