using BusinessObjects.DTOs;

namespace Client.Models.Employer
{
    public class CreateJobViewModel
    {
        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public decimal Budget { get; set; }

        public int CategoryId { get; set; }

        public DateTime? Deadline { get; set; }

        public List<int> SkillIds { get; set; } = [];

        public List<CategoryDTO> Categories { get; set; } = [];

        public List<SkillDTO> Skills { get; set; } = [];
    }
}
