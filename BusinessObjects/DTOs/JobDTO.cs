using BusinessObjects.Enums;

namespace BusinessObjects.DTOs
{
    public class JobDTO
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
        public int ApplicationsCount { get; set; } = 0;
    }

    public class FilterJobDTO
    {
        public string? Keyword { get; set; }

        public string? EmployerKeyword { get; set; }

        public JobStatus? Status { get; set; }

        public int? CategoryId { get; set; }

        public int? EmployerProfileId { get; set; }

        public decimal? MinBudget { get; set; }
        public decimal? MaxBudget { get; set; }

        public DateTime? DeadlineFrom { get; set; }
        public DateTime? DeadlineTo { get; set; }

        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public JobTemperature? Temperature { get; set; }

        public string? SortBy { get; set; } = "CreatedAt";
        public bool IsDescending { get; set; } = true;

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
