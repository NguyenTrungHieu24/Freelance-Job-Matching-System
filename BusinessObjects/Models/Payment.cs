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

        public string PaymentMethod { get; set; }

        public string Status { get; set; }

        public DateTime PaidAt { get; set; } = DateTime.Now;

        [ForeignKey("ApplicationId")]
        public Application Application { get; set; }
    }
}
