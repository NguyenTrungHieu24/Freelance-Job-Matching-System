using BusinessObjects.DTOs;
using BusinessObjects.Enums;

namespace Client.Models.Jobs
{
    public class JobDetailViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Budget { get; set; }

        public JobStatus Status { get; set; }

        public DateTime? Deadline { get; set; }

        public DateTime CreatedAt { get; set; }

        public int EmployerProfileId { get; set; }

        public string EmployerName { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public int ApplicationsCount { get; set; }

        public List<string> Skills { get; set; } = [];

        public List<ApplicationHistoryDto> Applications { get; set; } = [];
    }
}
