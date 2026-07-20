using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BusinessObjects.Enums;

namespace BusinessObjects.DTOs
{


    public class JobDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Budget { get; set; }
        public JobStatus Status { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime CreatedAt { get; set; }
        public int EmployerProfileId { get; set; }
        public string EmployerName { get; set; } = null!;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public List<string> Skills { get; set; } = new();
        public string? EmployerCompanyName { get; set; }
        public string? EmployerDescription { get; set; }
        public string? EmployerEmail { get; set; }
        public string? EmployerPhone { get; set; }
        public string? EmployerAddress { get; set; }
        public string? EmployerLogo { get; set; }
    }

    public class CreateJobDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = null!;

        [Range(0.01, double.MaxValue, ErrorMessage = "Budget must be greater than 0")]
        public decimal Budget { get; set; }

        [Required(ErrorMessage = "CategoryId is required")]
        public int CategoryId { get; set; }

        public DateTime? Deadline { get; set; }

        public List<int> Skills { get; set; } = new();
    }

    public class UpdateJobDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = null!;

        [Range(0.01, double.MaxValue, ErrorMessage = "Budget must be greater than 0")]
        public decimal Budget { get; set; }

        [Required(ErrorMessage = "CategoryId is required")]
        public int CategoryId { get; set; }

        public DateTime? Deadline { get; set; }

        public List<int> Skills { get; set; } = new();
    }

    public class FilterJobDto
    {
        public string? Keyword { get; set; }
        public int? CategoryId { get; set; }
        public int? SkillId { get; set; }
        public decimal? MinBudget { get; set; }
        public decimal? MaxBudget { get; set; }
        public string? Status { get; set; } // Allow filtering by status if needed
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
    
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

        public List<string> Skills { get; set; } = [];
        public List<ApplicationHistoryDto> Applications { get; set; } = [];
        public string CompanyName { get; set; } = string.Empty;
        public string EmployerLogo { get; set; } = string.Empty;
    }

    public class FilterJobDTO
    {
        public string? Keyword { get; set; }

        public string? EmployerKeyword { get; set; }

        public JobStatus? Status { get; set; }

        public List<int> SkillIds { get; set; } = [];

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
