using BusinessObjects.DTOs;

namespace Client.Models.Employer
{
    public class CreateJobViewModel : CreateJobDto
    {

        public List<CategoryDTO> Categories { get; set; } = [];

        public List<SkillDTO> Skills { get; set; } = [];
    }
}
