using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Enums
{
    public enum ReportStatus
    {
        [Description("Pending")]
        PENDING = 1,
        
        [Description("Reviewing")]
        REVIEWING = 2,
        
        [Description("Resolved")]
        RESOLVED = 3,

        [Description("Rejected")]
        REJECTED = 4
    }
}
