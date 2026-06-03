using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    // DTO returned by API for employer dashboard summary
    public class DashBoardDto
    {
        public int TotalPosts { get; set; }
        public int ActivePosts { get; set; }
        public int ClosedPosts { get; set; }
        public int PendingApplications { get; set; }

        public List<RecentApplicantDto> RecentApplicants { get; set; } = new List<RecentApplicantDto>();

        
        public Dictionary<string,int> ApplicationsPerDay { get; set; } = new Dictionary<string,int>();
    }

    public class RecentApplicantDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Position { get; set; }
        public DateTime AppliedAt { get; set; }
    }
}
