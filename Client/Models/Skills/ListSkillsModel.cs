using BusinessObjects.Common;
using BusinessObjects.DTOs;

namespace Client.Models.Skills
{
    public class ListSkillsModel
    {
        public FilterSkillDTO Filter { get; set; } = new();
        public PaginateResult<SkillDTO> Skills { get; set; }

        public bool HasFilter()
        {
            return
                !string.IsNullOrWhiteSpace(Filter.Keyword);
        }
    }
}
