using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects.Models
{
    public class Wallet
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public decimal Balance { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
    }
}
