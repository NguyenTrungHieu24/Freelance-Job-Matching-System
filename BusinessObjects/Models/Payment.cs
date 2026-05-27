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
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public int ApplicationId { get; set; }

        public decimal Amount { get; set; }

        [StringLength(50)]
        public string PaymentMethod { get; set; } // PayOS

        [StringLength(100)]
        public string TransactionCode { get; set; }

        [StringLength(50)]
        public PaymentStatus Status { get; set; } 

        public long? OrderCode { get; set; } // PayOS orderCode

        public string CheckoutUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? PaidAt { get; set; }

        [ForeignKey("ApplicationId")]
        public Application Application { get; set; }
    }
}
