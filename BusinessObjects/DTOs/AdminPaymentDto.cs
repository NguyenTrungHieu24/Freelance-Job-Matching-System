using System;

namespace BusinessObjects.DTOs
{
    public class AdminPaymentDto
    {
        public int Id { get; set; }
        public string TransactionCode { get; set; }
        public string JobTitle { get; set; }
        public string EmployerName { get; set; }
        public string FreelancerName { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
