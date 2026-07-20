using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects.Models
{
    public enum TransactionType
    {
        JOB_POSTING_FEE = 1,    // Phí đăng tin
        JOB_PAYMENT = 2,        // Employer trả tiền cho Job
        JOB_EARNING = 3,        // Freelancer nhận tiền
        COMMISSION_FEE = 4,     // Hoa hồng hệ thống
        DEPOSIT = 5,            // Nạp tiền (FM)
        WITHDRAW = 6,           // Rút tiền (FM)
    }

    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        public int WalletId { get; set; }

        public int? JobId { get; set; }

        public TransactionType Type { get; set; }

        public decimal Amount { get; set; }

        public decimal BalanceAfter { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey(nameof(WalletId))]
        public Wallet Wallet { get; set; }

        [ForeignKey(nameof(JobId))]
        public Job? Job { get; set; }
    }
}
