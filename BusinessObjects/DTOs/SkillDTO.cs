using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class SkillDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

    }

    public class FilterSkillDTO
    {
        public string? Keyword { get; set; }


        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
