using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs
{

    //Tao lop dto cho job
    public class JobDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Budget { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? Deadline { get; set; }
        public DateTime CreatedAt { get; set; }
        public int EmployerProfileId { get; set; }
        public string EmployerName { get; set; } = null!;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public List<SkillDTO> Skills { get; set; } = new();
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

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = null!;

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
}
