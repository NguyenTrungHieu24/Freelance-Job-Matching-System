using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Client.Models.Employer
{
    public class CreateJobViewModel
    {
        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public decimal Budget { get; set; }

        public string? CategoryId { get; set; }

        public DateTime? Deadline { get; set; }

        public List<string> SkillIds { get; set; } = [];

        public List<CategoryDTO> Categories { get; set; } = [];

        public List<SkillDTO> Skills { get; set; } = [];

        public List<IFormFile>? AttachedImages { get; set; }
    }
}
