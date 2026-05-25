using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class FreelancerSkill
    {
        public int FreelancerProfileId { get; set; }

        public int SkillId { get; set; }

        [ForeignKey("FreelancerProfileId")]
        public FreelancerProfile FreelancerProfile { get; set; }

        [ForeignKey("SkillId")]
        public Skill Skill { get; set; }
    }
}
