using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Common
{
    public class PayOSWebhook
    {
        public string code { get; set; }
        public string desc { get; set; }

        public PayOSWebhookData data { get; set; }

        public string signature { get; set; }
    }

    public class PayOSWebhookData
    {
        public long orderCode { get; set; }

        public int amount { get; set; }

        public string description { get; set; }

        public string reference { get; set; }

        public string transactionDateTime { get; set; }

        public string code { get; set; }

        public string desc { get; set; }
    }
}
