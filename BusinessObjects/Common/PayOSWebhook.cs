using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Common
{
    public class PayOSWebhook
    {
        public long OrderCode { get; set; }
        public string Status { get; set; }
        public int Amount { get; set; }
        public string TransactionId { get; set; }
    }
}
