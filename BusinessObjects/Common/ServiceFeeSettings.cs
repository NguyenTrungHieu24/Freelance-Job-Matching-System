using System;

namespace BusinessObjects.Common
{
    public class ServiceFeeSettings
    {
        public decimal JobPostingFee { get; set; } = 50000;
        public int CommissionPercent { get; set; } = 10;
    }
}
