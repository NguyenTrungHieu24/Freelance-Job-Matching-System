using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Common
{
    public class CreatePaymentResponse
    {
        public string CheckoutUrl { get; set; } = string.Empty;

        public string QrCode { get; set; } = string.Empty;
    }
}
