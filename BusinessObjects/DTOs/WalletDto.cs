using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs
{
    // DTO hiển thị trong bảng danh sách ví
    public class WalletUserDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
    }

    // DTO cho request nạp/rút tiền
    public class AdjustBalanceDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(1000, 1000000000, ErrorMessage = "Amount must be at least 1,000 VNĐ")]
        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    // DTO trả về số dư cho navbar
    public class WalletBalanceDto
    {
        public decimal Balance { get; set; }
    }
}
