using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Enums
{
    public enum RoleEnum
    {
        [Description("Administrator")]
        ADMIN = 1,

        [Description("Employer")]
        EMPLOYER = 2,

        [Description("Freelancer")]
        FREELANCER = 3,

        [Description("Guest")]
        GUEST = 4,
    }
}
