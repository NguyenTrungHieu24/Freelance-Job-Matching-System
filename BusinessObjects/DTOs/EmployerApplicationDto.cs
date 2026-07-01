namespace BusinessObjects.DTOs
{
    public class EmployerApplicationDto
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public string JobTitle { get; set; } = null!;
        public int FreelancerProfileId { get; set; }
        public string FreelancerName { get; set; } = null!;
        public string CoverLetter { get; set; } = null!;
        public string Status { get; set; } = null!; // Pending, Accepted, Rejected
        public DateTime AppliedAt { get; set; }
    }
}
