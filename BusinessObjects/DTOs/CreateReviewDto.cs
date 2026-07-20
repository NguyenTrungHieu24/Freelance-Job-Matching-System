using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs
{
    public class CreateReviewDto
    {
        [Required]
        public int RevieweeId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(1000)]
        public string Comment { get; set; } = string.Empty;
    }
}
